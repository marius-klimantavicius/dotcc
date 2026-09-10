using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotCC.PostProcess;
using Shouldly;
using Xunit;

namespace DotCC.PostProcess.Tests;

public sealed partial class CondInlinerTests
{
    private static (RewriteResult Result, string Source) CleanupCheck(string source, int removed, string helper = "")
    {
        var original = Compile(source, helper, helper.Length == 0 ? "" : null);
        var before = Run(original);
        var result = SourcePostProcessor.Rewrite(original);
        result.RemovedEmptyBlocks.ShouldBe(removed);
        Run(result.Compilation).ShouldBe(before);
        var roundtrip = result.Compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(result.Compilation.SyntaxTrees.Select(t =>
            CSharpSyntaxTree.ParseText(t.GetRoot().ToFullString(), (CSharpParseOptions)t.Options, t.FilePath)));
        Run(roundtrip).ShouldBe(before);
        var second = SourcePostProcessor.Rewrite(roundtrip);
        second.RemovedEmptyBlocks.ShouldBe(0);
        second.Rewritten.ShouldBe(0);
        return (result, result.Compilation.SyntaxTrees.First().GetRoot().ToFullString());
    }

    [Fact]
    public void Removes_standalone_and_nested_empty_blocks_without_a_Cond_helper()
    {
        var (_, source) = CleanupCheck("""
            public static class Case {
                public static string Run() {
                    {} { { } {} }
                    string result;
                    { result = "nonempty"; }
                    return result;
                    {}
                }
            }
            """, 5);
        CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<BlockSyntax>().Count().ShouldBe(2);
    }

    [Fact]
    public void Retains_required_statement_method_accessor_and_lambda_bodies()
    {
        var (result, _) = CleanupCheck("""
            public static class Case {
                static int Value { get { return 0; } set {} }
                static void EmptyMethod() {}
                public static string Run() {
                    if (Value == 0) {} else {}
                    while (Value != 0) {}
                    do {} while (Value != 0);
                    for (int i = 0; i < 1; i++) {}
                    foreach (var item in new[] { 1 }) {}
                    using (var stream = new System.IO.MemoryStream()) {}
                    lock (new object()) {}
                    try {} catch (System.Exception) {} finally {}
                    checked {} unchecked {} unsafe {}
                    System.Action action = () => {};
                    action(); EmptyMethod(); Value = 1;
                    return "ok";
                }
            }
            """, 0);
        result.Compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<BlockSyntax>()
            .Count(b => b.Statements.Count == 0).ShouldBe(17);
    }

    [Fact]
    public void Cleans_inside_required_bodies_but_keeps_labeled_block()
    {
        var (_, source) = CleanupCheck("""
            public static class Case {
                public static string Run() {
                    if (true) { {} }
                    goto label;
                    label: { {} }
                    return "ok";
                }
            }
            """, 2);
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        root.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement.ShouldBeOfType<BlockSyntax>().Statements.ShouldBeEmpty();
        root.DescendantNodes().OfType<LabeledStatementSyntax>().Single().Statement.ShouldBeOfType<BlockSyntax>().Statements.ShouldBeEmpty();
    }

    [Fact]
    public void Removes_switch_section_blocks_and_preserves_trailing_comments()
    {
        var (_, source) = CleanupCheck("""
            public static class Case {
                public static string Run() {
                    switch (System.Environment.ProcessId) {
                        case -1: {} break; { /* after-break */ }
                        default: { {} } break;
                    }
                    return "ok";
                }
            }
            """, 4);
        source.ShouldContain("/* after-break */");
        CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<SwitchSectionSyntax>()
            .ShouldAllBe(s => s.Statements.Count == 1 && s.Statements[0] is BreakStatementSyntax);
    }

    [Fact]
    public void Preserves_comments_and_caller_line_numbers_when_braces_are_removed()
    {
        const string input = """
            public static class Case {
                static int Line([System.Runtime.CompilerServices.CallerLineNumber] int line = 0) => line;
                public static string Run() {
                    { // opening
                        /* interior */
                    } // closing
                    var result = Line();
                    { /* last */ }
                    return result.ToString();
                }
            }
            """;
        var (_, source) = CleanupCheck(input, 2);
        foreach (var comment in new[] { "// opening", "/* interior */", "// closing", "/* last */" })
            source.Split(comment).Length.ShouldBe(2);
        source.Count(c => c == '\n').ShouldBe(input.Count(c => c == '\n'));
    }

    [Fact]
    public void Retains_directive_bearing_blocks_and_disabled_source()
    {
        var (_, source) = CleanupCheck("""
            public static class Case {
                public static string Run() {
                    {
            #if NEVER
                        System.Console.WriteLine("disabled");
            #endif
                    }
                    {
            #region Empty
            #endregion
                    }
                    return "ok";
                }
            }
            """, 0);
        source.ShouldContain("System.Console.WriteLine(\"disabled\")");
    }

    [Fact]
    public void Preserves_empty_blocks_in_captured_caller_argument_text()
    {
        CleanupCheck("""
            public static class Case {
                static string Capture(System.Action action,
                    [System.Runtime.CompilerServices.CallerArgumentExpression("action")] string text = "") => text;
                public static string Run() {
                    {}
                    return Capture(() => { {} });
                }
            }
            """, 1);
    }

    [Theory]
    [InlineData("{} { { } }", 2, 1)]
    [InlineData("{} System.Console.WriteLine(42); {}", 2, 1)]
    [InlineData("{} class Other {}", 0, 1)]
    public void Handles_top_level_blocks_and_preserves_implicit_entry_point(string source, int removed, int statements)
    {
        var original = Compile(source, "", "").WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        var result = SourcePostProcessor.Rewrite(original);
        result.RemovedEmptyBlocks.ShouldBe(removed);
        result.Compilation.GetEntryPoint(default).ShouldNotBeNull();
        var tree = result.Compilation.SyntaxTrees.First();
        tree.GetRoot().DescendantNodes().OfType<GlobalStatementSyntax>().Count().ShouldBe(statements);
        var reparsed = result.Compilation.ReplaceSyntaxTree(tree,
            CSharpSyntaxTree.ParseText(tree.GetRoot().ToFullString(), (CSharpParseOptions)tree.Options));
        using var output = new MemoryStream();
        reparsed.Emit(output).Success.ShouldBeTrue();
        SourcePostProcessor.Rewrite(reparsed).RemovedEmptyBlocks.ShouldBe(0);
    }

    [Fact]
    public void Runs_cleanup_after_semantic_condition_inlining()
    {
        var (result, source) = CleanupCheck("""
            public static class Case {
                public static string Run() {
                    {}
                    if (Cond.B(1)) { {} }
                    return Cond.B(0).ToString();
                }
            }
            """, 2, Helper);
        result.Rewritten.ShouldBe(2);
        source.ShouldNotContain("Cond.B(");
    }
}
