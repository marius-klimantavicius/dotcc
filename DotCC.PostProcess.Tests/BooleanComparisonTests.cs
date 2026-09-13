using DotCC.PostProcess;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.PostProcess.Tests;

public sealed partial class CondInlinerTests
{
    private static (RewriteResult Result, string Source) BooleanCheck(string source, int simplified,
        string helper = "", string runtime = "")
    {
        var original = Compile(source, helper, runtime);
        string expected = Run(original);
        var result = SourcePostProcessor.Rewrite(original);
        result.SimplifiedBooleanComparisons.ShouldBe(simplified, result.Compilation.SyntaxTrees.First().GetRoot().ToFullString());
        Run(result.Compilation).ShouldBe(expected);
        var roundtrip = result.Compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(result.Compilation.SyntaxTrees.Select(t =>
            CSharpSyntaxTree.ParseText(t.GetRoot().ToFullString(), (CSharpParseOptions)t.Options, t.FilePath)));
        Run(roundtrip).ShouldBe(expected);
        var second = SourcePostProcessor.Rewrite(roundtrip);
        second.SimplifiedBooleanComparisons.ShouldBe(0);
        second.Rewritten.ShouldBe(0);
        return (result, result.Compilation.SyntaxTrees.First().GetRoot().ToFullString());
    }

    [Theory]
    [InlineData("(Next(value) ? 0 : 1) != 0", true)]
    [InlineData("(Next(value) ? 0 : 1) == 0", false)]
    [InlineData("(Next(value) ? 1 : 0) != 0", false)]
    [InlineData("(Next(value) ? 1 : 0) == 0", true)]
    [InlineData("(Next(value) ? 0 : 1) != 1", false)]
    [InlineData("(Next(value) ? 0 : 1) == 1", true)]
    [InlineData("(Next(value) ? 1 : 0) != 1", true)]
    [InlineData("(Next(value) ? 1 : 0) == 1", false)]
    [InlineData("0 == (Next(value) ? 0 : 1)", false)]
    [InlineData("1 != (Next(value) ? 0 : 1)", false)]
    [InlineData("(Next(value) ? 0 : 1) < 1", false)]
    [InlineData("(Next(value) ? 0 : 1) <= 0", false)]
    [InlineData("(Next(value) ? 0 : 1) > 0", true)]
    [InlineData("(Next(value) ? 0 : 1) >= 1", true)]
    [InlineData("0 < (Next(value) ? 0 : 1)", true)]
    [InlineData("1 <= (Next(value) ? 0 : 1)", true)]
    [InlineData("1 > (Next(value) ? 0 : 1)", false)]
    [InlineData("0 >= (Next(value) ? 0 : 1)", false)]
    [InlineData("checked((byte)(Next(value) ? 0u : 1u)) != 0", true)]
    [InlineData("((nint)(Next(value) ? 0 : 1)) != 0", true)]
    [InlineData("(Next(value) ? 0m : 1m) != 0m", true)]
    [InlineData("(Next(value) ? -0.0 : 1.0) != 0.0", true)]
    [InlineData("(Next(value) ? '\\0' : '\\u0001') != 0", true)]
    public void Simplifies_comparison_polarity_and_preserves_single_evaluation(string expression, bool negated)
    {
        var (_, source) = BooleanCheck($$"""
            public static class Case {
                static int calls;
                static bool Next(bool value) { calls++; return value; }
                static bool Test(bool value) => {{expression}};
                public static string Run() => Test(true) + ":" + Test(false) + ":" + calls;
            }
            """, 1);
        var body = CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(m => m.Identifier.ValueText == "Test").ExpressionBody!.Expression;
        if (negated) body.ShouldBeOfType<PrefixUnaryExpressionSyntax>().IsKind(SyntaxKind.LogicalNotExpression).ShouldBeTrue();
        else body.ShouldBeOfType<ParenthesizedExpressionSyntax>().Expression.ShouldBeOfType<InvocationExpressionSyntax>();
        source.ShouldNotContain(" ? ");
    }

    [Fact]
    public void Runs_after_Cbool_and_Cond_inlining_in_one_invocation()
    {
        var (result, source) = BooleanCheck("""
            using DotCC.Libc;
            public static class Case {
                public static string Run() {
                    int n = 0;
                    if (Cond.B((CBool)((++n > 0) ? 0 : 1))) n += 10;
                    return n.ToString();
                }
            }
            """, 1, Helper, Runtime);
        result.Rewritten.ShouldBe(1);
        source.ShouldNotContain("Cond.B(");
        source.ShouldNotContain(" ? ");
        source.ShouldContain("!(");
    }

    [Fact]
    public void Nested_conditions_short_circuit_loops_and_throwing_conditions_keep_their_effects()
    {
        BooleanCheck("""
            public static class Case {
                static int calls;
                static bool Next() { calls++; return calls < 3; }
                static bool Fail() => throw new System.InvalidOperationException();
                public static string Run() {
                    int n = 0;
                    while ((Next() ? 1 : 0) != 0) n++;
                    bool result = false && ((Next() ? 0 : 1) == 0);
                    result |= (((Next() ? 0 : 1) != 0) ? 0 : 1) != 0;
                    try { result |= (Fail() ? 1 : 0) == 1; }
                    catch (System.InvalidOperationException) { n += 10; }
                    return result + ":" + n + ":" + calls;
                }
            }
            """, 5);
    }

    [Theory]
    [InlineData("(Next() ? 0 : 1) >= 0")]
    [InlineData("(Next() ? 1 : 1) == 1")]
    [InlineData("(Next() ? 0 : 2) != 0")]
    [InlineData("(Next() ? 0 : calls++) != 0")]
    [InlineData("(Next() ? (int?)0 : 1) != 0")]
    [InlineData("(Next() ? 0m : 1.0000000000000000000000000001m) == 1m")]
    [InlineData("(Next() ? 0.0 : 1.0) != double.NaN")]
    public void Leaves_unsupported_and_constant_result_comparisons_intact(string expression)
    {
        var (_, source) = BooleanCheck($$"""
            public static class Case {
                static int calls;
                static bool Next() { calls++; return false; }
                public static string Run() => ({{expression}}) + ":" + calls;
            }
            """, 0);
        source.ShouldContain(expression);
    }

    [Fact]
    public void Keeps_user_defined_comparisons_conversions_and_truth_operators()
    {
        BooleanCheck("""
            public struct Truth {
                public static int Calls;
                public static bool operator true(Truth x) { Calls++; return true; }
                public static bool operator false(Truth x) => false;
                public static bool operator !(Truth x) { Calls += 100; return true; }
            }
            public struct Number {
                public static int Calls;
                public static implicit operator Number(int x) { Calls++; return default; }
                public static bool operator ==(Number a, Number b) { Calls++; return true; }
                public static bool operator !=(Number a, Number b) { Calls++; return false; }
                public override bool Equals(object obj) => false;
                public override int GetHashCode() => 0;
            }
            public static class Case {
                public static string Run() {
                    bool value = true;
                    bool a = (new Truth() ? 0 : 1) != 0;
                    bool b = (Number)(value ? 0 : 1) != (Number)0;
                    return a + ":" + b + ":" + Truth.Calls + ":" + Number.Calls;
                }
            }
            """, 0);
    }

    [Fact]
    public void Keeps_target_typed_conditions_boolean_after_removing_the_conditional()
    {
        BooleanCheck("""
            public static class Case {
                static string Pick(bool b) => "bool:" + b;
                static string Pick(int b) => "int:" + b;
                public static string Run() {
                    var a = (default ? 1 : 0) != 0;
                    var b = (new() ? 0 : 1) != 0;
                    return a + ":" + b + ":" + Pick((default ? 1 : 0) == 1);
                }
            }
            """, 3);
    }

    [Fact]
    public void Preserves_comments_newlines_and_caller_line_numbers()
    {
        const string input = """
            public static class Case {
                static int Line([System.Runtime.CompilerServices.CallerLineNumber] int line = 0) => line;
                public static string Run() {
                    bool b = ( /* before */ Line() > 0 /* condition */ ? // yes
                        0 : /* no */ 1) != 0; // after
                    return b + ":" + Line();
                }
            }
            """;
        var (_, source) = BooleanCheck(input, 1);
        source.Count(c => c == '\n').ShouldBe(input.Count(c => c == '\n'));
        foreach (string comment in new[] { "/* before */", "/* condition */", "// yes", "/* no */", "// after" })
            source.Split(comment).Length.ShouldBe(2);
    }

    [Fact]
    public void Preserves_expression_trees_query_shapes_caller_argument_text_and_directives()
    {
        BooleanCheck("""
            using System;
            using System.Linq;
            using System.Linq.Expressions;
            using System.Runtime.CompilerServices;
            public static class Case {
                static string Capture(bool b, [CallerArgumentExpression("b")] string text = "") => text;
                public static string Run() {
                    Expression<Func<bool, bool>> tree = b => (b ? 0 : 1) != 0;
                    var query = from n in new[] { 1 }.AsQueryable() where (n > 0 ? 0 : 1) != 0 select n;
                    bool result = (true ?
            #if DEBUG
                        0
            #else
                        0
            #endif
                        : 1) != 0;
                    return tree.Body.NodeType + ":" + Capture((true ? 0 : 1) != 0) + ":" + query.Expression + ":" + result;
                }
            }
            """, 0);
    }
}
