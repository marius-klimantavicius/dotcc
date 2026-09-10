using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using DotCC.PostProcess;
using Shouldly;
using Xunit;

namespace DotCC.PostProcess.Tests;

public sealed partial class CondInlinerTests
{
    private const string Helper = """
        static unsafe class Cond {
            public static bool B(bool x) => x;
            public static bool B(int x) => x != 0;
            public static bool B(uint x) => x != 0;
            public static bool B(long x) => x != 0;
            public static bool B(ulong x) => x != 0;
            public static bool B(nint x) => x != 0;
            public static bool B(nuint x) => x != 0;
            public static bool B(byte x) => x != 0;
            public static bool B(sbyte x) => x != 0;
            public static bool B(short x) => x != 0;
            public static bool B(ushort x) => x != 0;
            public static bool B(float x) => x != 0;
            public static bool B(double x) => x != 0;
            public static bool B(void* x) => x != null;
            public static bool B(DotCC.Libc.CBool x) => (int)x != 0;
        }
        """;
    private static readonly MetadataReference[] References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p)).ToArray();
    private static readonly string Runtime = ReadRuntime();
    private static string ReadRuntime()
    {
        using var stream = typeof(CondInlinerTests).Assembly.GetManifestResourceStream("CBool.cs")!;
        using var reader = new StreamReader(stream); return reader.ReadToEnd();
    }
    private static CSharpCompilation Compile(string source, string helper = Helper, string? runtime = null)
        => CSharpCompilation.Create("Case" + Guid.NewGuid().ToString("N"), new[] { source, helper, runtime ?? Runtime }
            .Select((s, i) => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.Preview), "case" + i + ".cs")), References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
    private static string Run(CSharpCompilation compilation)
    {
        using var output = new MemoryStream();
        var emitted = compilation.Emit(output);
        Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics));
        output.Position = 0;
        var context = new AssemblyLoadContext("postprocess-test", isCollectible: true);
        try { return (string)context.LoadFromStream(output).GetType("Case")!.GetMethod("Run")!.Invoke(null, null)!; }
        finally { context.Unload(); }
    }
    private static (RewriteResult Result, string Source) Check(string source, string? expected = null, string helper = Helper, string? runtime = null)
    {
        var original = Compile(source, helper, runtime);
        var result = CondInliner.Rewrite(original);
        var before = Run(original);
        Run(result.Compilation).ShouldBe(before);
        if (expected != null) before.ShouldBe(expected);
        // Parse serialized trees again, to test actual output rather than only
        // the already-bound in-memory syntax tree.
        var roundtrip = result.Compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(result.Compilation.SyntaxTrees.Select(t =>
            CSharpSyntaxTree.ParseText(t.GetRoot().ToFullString(), (CSharpParseOptions)t.Options, t.FilePath)));
        Run(roundtrip).ShouldBe(before);
        var second = CondInliner.Rewrite(roundtrip);
        second.Rewritten.ShouldBe(0);
        return (result, result.Compilation.SyntaxTrees.First().GetRoot().ToFullString());
    }

    [Theory]
    [InlineData("bool", "true")]
    [InlineData("bool", "false")]
    [InlineData("sbyte", "-1")]
    [InlineData("byte", "255")]
    [InlineData("short", "-32768")]
    [InlineData("ushort", "65535")]
    [InlineData("int", "int.MinValue")]
    [InlineData("uint", "uint.MaxValue")]
    [InlineData("long", "long.MinValue")]
    [InlineData("ulong", "ulong.MaxValue")]
    [InlineData("nint", "-1")]
    [InlineData("nuint", "nuint.MaxValue")]
    [InlineData("float", "float.NaN")]
    [InlineData("float", "-0.0f")]
    [InlineData("double", "double.NaN")]
    [InlineData("double", "-0.0")]
    [InlineData("double", "double.PositiveInfinity")]
    [InlineData("DotCC.Libc.CBool", "42")]
    public void Existing_overloads_preserve_value_and_single_evaluation(string type, string value)
    {
        var (result, text) = Check($$"""
            public static class Case {
                static int calls;
                static {{type}} Next() { ++calls; return {{value}}; }
                public static string Run() => Cond.B(Next()) + ":" + calls;
            }
            """);
        result.Rewritten.ShouldBe(1); text.ShouldNotContain("Cond.B(");
    }

    [Fact]
    public void Cbool_casts_collapse_only_inside_conditions_and_keep_normalized_stores()
    {
        var (result, text) = Check("""
            using DotCC.Libc;
            public static class Case {
                public static string Run() {
                    int x = 2;
                    CBool stored = (CBool)(x++ > 0);
                    bool a = Cond.B((CBool)(x++ > 0));
                    bool b = Cond.B((CBool)(Cond.B((CBool)(x > 0)) && Cond.B(--x)));
                    return a + ":" + b + ":" + (int)stored + ":" + x;
                }
            }
            """, "True:True:1:3");
        result.Rewritten.ShouldBe(4);
        text.ShouldContain("CBool stored = (CBool)(x++ > 0)");
        text.ShouldNotContain("bool a = ((int)");
        text.ShouldNotContain("Cond.B(");
        // Only the store retains a CBool cast.
        text.Split("(CBool)").Length.ShouldBe(2);
    }

    [Fact]
    public void Scalar_Cbool_normalization_and_pointer_function_pointer_null_conditions()
    {
        var (result, text) = Check("""
            using DotCC.Libc;
            public static unsafe class Case {
                static int F() => 1;
                public static string Run() {
                    int x = 1; int* p = &x; delegate*<int> f = &F;
                    return Cond.B(p) + ":" + Cond.B(f) + ":" + Cond.B((void*)null) + ":"
                        + Cond.B((CBool)p) + ":" + Cond.B((CBool)(-0.0)) + ":" + Cond.B((CBool)double.NaN);
                }
            }
            """, "True:True:False:True:False:True");
        result.Rewritten.ShouldBe(6, string.Join("\n", result.Diagnostics) + "\n" + text); text.ShouldNotContain("CBool)");
    }

    [Fact]
    public void Parentheses_assignments_conditional_coalescing_and_short_circuit_are_preserved()
    {
        Check("""
            public static class Case {
                public static string Run() {
                    int x = 0; int? n = null;
                    bool a = !Cond.B(x = 2) || Cond.B(++x);
                    bool b = Cond.B(n ?? (x == 3 ? 0 : ++x)) && Cond.B(++x);
                    return a + ":" + b + ":" + x;
                }
            }
            """, "True:False:3").Result.Rewritten.ShouldBe(4);
    }

    [Fact]
    public void Conversion_selected_by_overload_resolution_runs_once()
    {
        Check("""
            public struct Number {
                public static int Calls;
                public static implicit operator int(Number n) { ++Calls; return -2; }
            }
            public static class Case {
                public static string Run() => Cond.B(new Number()) + ":" + Number.Calls;
            }
            """, "True:1", "static class Cond { public static bool B(int x) => x != 0; }").Result.Rewritten.ShouldBe(1);
    }

    [Fact]
    public void Target_typed_default_new_and_checked_overflow_remain_valid()
    {
        Check("""
            public static class Case {
                public static string Run() {
                    int x = int.MaxValue;
                    try { checked { return Cond.B(x + 1).ToString(); } }
                    catch (System.OverflowException) { return Cond.B(default) + ":" + Cond.B(new()); }
                }
            }
            """, "False:False", "static class Cond { public static bool B(int x) => x != 0; }").Result.Rewritten.ShouldBe(3);
    }

    [Fact]
    public void Names_aliases_and_using_static_bind_to_symbols()
    {
        var (result, text) = Check("""
            using Alias = Cond;
            using static Cond;
            namespace Shadow { static class Cond { public static bool B(int x) => false; } }
            public static class Case {
                public static string Run() => Alias.B(1) + ":" + B(2) + ":" + global::Cond.B(3) + ":" + Shadow.Cond.B(4);
            }
            """, "True:True:True:False");
        result.Rewritten.ShouldBe(3); text.ShouldContain("Shadow.Cond.B(4)");
    }

    [Fact]
    public void Helper_type_initializer_and_modified_implementation_are_not_erased()
    {
        var (result, _) = Check("""
            public static class Case { public static string Run() => Cond.B(0) + ":" + Cond.Calls; }
            """, "True:1", "static class Cond { public static int Calls; static Cond() { ++Calls; } public static bool B(int x) => x == 0; }");
        result.Rewritten.ShouldBe(0); result.Skipped.ShouldBe(1);
    }

    [Fact]
    public void Custom_Cbool_normalizer_side_effects_are_preserved()
    {
        var runtime = Runtime.Replace("public static implicit operator CBool(bool b) => new((byte)(b ? 1 : 0));",
            "public static int Calls; public static implicit operator CBool(bool b) { ++Calls; return new((byte)(b ? 1 : 0)); }");
        var (result, text) = Check("""
            using DotCC.Libc;
            public static class Case { public static string Run() => Cond.B((CBool)true) + ":" + CBool.Calls; }
            """, "True:1", runtime: runtime);
        result.Rewritten.ShouldBe(1); text.ShouldContain("(CBool)true");
    }

    [Fact]
    public void Expression_trees_and_caller_argument_text_remain_observable_calls()
    {
        var (result, _) = Check("""
            using System;
            using System.Linq.Expressions;
            using System.Runtime.CompilerServices;
            public static class Case {
                static string Capture(bool b, [CallerArgumentExpression("b")] string text = "") => text;
                public static string Run() {
                    Expression<Func<bool>> e = () => Cond.B(1);
                    return e.Body.NodeType + ":" + Capture(Cond.B(2));
                }
            }
            """, "Call:Cond.B(2)");
        result.Rewritten.ShouldBe(0); result.Skipped.ShouldBe(2);
    }

    [Fact]
    public void Comments_multiline_and_statement_calls_are_retained()
    {
        var (result, text) = Check("""
            public static class Case {
                public static string Run() {
                    int n = 0;
                    Cond.B(++n);
                    bool b = Cond /* first */ .B(
                        /* second */ ++n /* third */
                    );
                    return b + ":" + n;
                }
            }
            """, "True:2");
        result.Rewritten.ShouldBe(1); result.Skipped.ShouldBe(1);
        foreach (var word in new[] { "first", "second", "third" }) text.Split(word).Length.ShouldBe(2);
    }

    [Fact]
    public void Cbool_field_initializer_effect_is_preserved()
    {
        var runtime = Runtime.Replace("private readonly byte _v;", "private readonly byte _v = global::Case.Mark();");
        var (result, text) = Check("""
            using DotCC.Libc;
            public static class Case {
                static int calls;
                public static byte Mark() { ++calls; return 0; }
                public static string Run() => Cond.B((CBool)true) + ":" + calls;
            }
            """, "True:1", runtime: runtime);
        result.Rewritten.ShouldBe(1); text.ShouldContain("(CBool)true");
    }

    [Fact]
    public void Constructor_caller_argument_capture_is_preserved()
    {
        var (result, _) = Check("""
            using System.Runtime.CompilerServices;
            public class Captured {
                public string Text;
                public Captured(bool b, [CallerArgumentExpression("b")] string text = "") { Text = text; }
            }
            public static class Case { public static string Run() => new Captured(Cond.B(1)).Text; }
            """, "Cond.B(1)");
        result.Rewritten.ShouldBe(0); result.Skipped.ShouldBe(1);
    }

    [Fact]
    public void Volatile_atomic_and_conditional_target_types_preserve_effects()
    {
        Check("""
            using System.Threading;
            public static class Case {
                static volatile int x = 1;
                public static string Run() {
                    int y = 0;
                    return Cond.B(x) + ":" + Cond.B(Interlocked.Increment(ref y)) + ":"
                        + Cond.B(x == 1 ? new() : 5) + ":" + y;
                }
            }
            """, "True:True:False:1", "static class Cond { public static bool B(int x) => x != 0; }");
    }

    [Fact]
    public void Explicit_conversion_shadowing_does_not_replace_selected_implicit_conversion()
    {
        var (result, _) = Check("""
            public class Base { public static implicit operator int(Base b) => 1; }
            public class Derived : Base { public static explicit operator int(Derived d) => 0; }
            public static class Case { public static string Run() => Cond.B(new Derived()).ToString(); }
            """, "True", "static class Cond { public static bool B(int x) => x != 0; }");
        result.Rewritten.ShouldBe(0); result.Skipped.ShouldBe(1);
    }

    [Fact]
    public void Entire_conditional_new_expression_keeps_its_target_type()
    {
        Check("""
            public static class Case {
                public static string Run() { bool choose = true; return Cond.B(choose ? new() : new()).ToString(); }
            }
            """, "False", "static class Cond { public static bool B(int x) => x != 0; }");
    }

    [Fact]
    public void Constructor_initializer_and_conditional_indexer_captures_are_preserved()
    {
        var (result, _) = Check("""
            using System.Runtime.CompilerServices;
            public class Base {
                public string Text;
                public Base(bool b, [CallerArgumentExpression("b")] string text = "") { Text = text; }
            }
            public class Derived : Base { public Derived() : base(Cond.B(1)) {} }
            public class Index {
                public string this[bool b, [CallerArgumentExpression("b")] string text = ""] => text;
            }
            public static class Case { public static string Run() => new Derived().Text + ":" + new Index()?[Cond.B(2)]; }
            """, "Cond.B(1):Cond.B(2)");
        result.Rewritten.ShouldBe(0); result.Skipped.ShouldBe(2);
    }

    [Fact]
    public void Query_provider_expression_shape_is_preserved()
    {
        var (result, _) = Check("""
            using System.Linq;
            public static class Case {
                public static string Run() {
                    var query = from n in new[] { 1, 2 }.AsQueryable() where Cond.B(n) select n;
                    return query.Expression.ToString();
                }
            }
            """);
        result.Rewritten.ShouldBe(0); result.Skipped.ShouldBe(1);
    }

    [Fact]
    public void Optional_helper_argument_is_rejected_without_throwing()
    {
        var (result, _) = Check("public static class Case { public static string Run() => Cond.B().ToString(); }",
            "False", "static class Cond { public static bool B(int x = 0) => x != 0; }");
        result.Rewritten.ShouldBe(0); result.Skipped.ShouldBe(1);
    }

    [Fact]
    public void Embedded_runtime_global_Cbool_has_the_same_proven_normalization()
    {
        var (result, text) = Check("""
            public static class Case { public static string Run() => Cond.B((CBool)(2 > 1)).ToString(); }
            """, "True", Helper.Replace("DotCC.Libc.CBool", "CBool"), Runtime.Replace("namespace DotCC.Libc;", ""));
        result.Rewritten.ShouldBe(1); text.ShouldNotContain("CBool");
    }

    [Fact]
    public void Namespaced_helpers_and_Cbool_are_inlined_and_reparsed()
    {
        var (result, text) = Check("""
            using Managed.Database;
            public static class Case { public static string Run() => Cond.B((CBool)(2 > 1)).ToString(); }
            """, "True", "namespace Managed.Database;\n" + Helper.Replace("DotCC.Libc.CBool", "CBool"),
            Runtime.Replace("namespace DotCC.Libc;", "namespace Managed.Database;"));
        result.Rewritten.ShouldBe(1); text.ShouldNotContain("Cond.B"); text.ShouldNotContain("CBool");
    }

    [Fact]
    public void Each_namespaced_helper_must_prove_its_own_body()
    {
        var (result, _) = Check("""
            public static class Case { public static string Run() => First.Cond.B(-1) + ":" + Second.Cond.B(-1); }
            """, "True:False", """
            namespace First { static class Cond { public static bool B(int x) => x != 0; } }
            namespace Second { static class Cond { public static bool B(int x) => x > 0; } }
            """);
        result.Rewritten.ShouldBe(1); result.Skipped.ShouldBe(1);
    }

    [Fact]
    public void Invalid_input_fails_before_rewriting()
        => Should.Throw<InvalidOperationException>(() => CondInliner.Rewrite(Compile("public class Broken { bool x = Cond.B(missing); }")));
}
