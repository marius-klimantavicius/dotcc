using System.Collections.Immutable;
using System.Runtime.Loader;
using DotCC.PostProcess.Analyzers;
using DotCC.PostProcess.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace DotCC.PostProcess.Analyzers.Tests;

public sealed partial class AnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private const string CondId = PostProcessAnalyzer.InlineConditionId;
    private const string BlockId = PostProcessAnalyzer.EmptyBlockId;
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
    private static readonly PostProcessCodeFixProvider Fixer = new();

    private static Project AddProject(AdhocWorkspace workspace, string name = "Sample", bool helper = true)
    {
        var project = workspace.AddProject(ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), name, name,
            LanguageNames.CSharp, compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true, optimizationLevel: OptimizationLevel.Release),
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview), metadataReferences: References));
        if (helper)
        {
            workspace.AddDocument(project.Id, "Cond.cs", SourceText.From(Helper));
            using var stream = typeof(AnalyzerTests).Assembly.GetManifestResourceStream("CBool.cs")!;
            using var reader = new StreamReader(stream);
            workspace.AddDocument(project.Id, "CBool.cs", SourceText.From(reader.ReadToEnd()));
        }
        return workspace.CurrentSolution.GetProject(project.Id)!;
    }

    private static async Task<ImmutableArray<Diagnostic>> Diagnostics(Project project, CancellationToken token = default)
    {
        var compilation = (await project.GetCompilationAsync(token))!;
        var diagnostics = await compilation.WithAnalyzers([new PostProcessAnalyzer()]).GetAnalyzerDiagnosticsAsync(token);
        diagnostics.ShouldNotContain(d => d.Id == "AD0001");
        return diagnostics;
    }

    private static async Task<ImmutableArray<Diagnostic>> Diagnostics(Document document)
    {
        var tree = await document.GetSyntaxTreeAsync(Token);
        return (await Diagnostics(document.Project)).Where(d => d.Location.SourceTree == tree).ToImmutableArray();
    }

    private static async Task<Document> Apply(Document document, Diagnostic diagnostic)
    {
        var actions = new List<CodeAction>();
        await Fixer.RegisterCodeFixesAsync(new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), default));
        actions.Count.ShouldBe(1);
        var operations = await actions[0].GetOperationsAsync(default);
        return operations.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id)!;
    }

    private sealed class DiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        private readonly Dictionary<ProjectId, Task<ImmutableArray<Diagnostic>>> cache = [];
        private Task<ImmutableArray<Diagnostic>> Get(Project project, CancellationToken token)
        {
            if (!cache.TryGetValue(project.Id, out var task)) cache.Add(project.Id, task = Diagnostics(project, token));
            return task;
        }
        public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken token)
        {
            var tree = await document.GetSyntaxTreeAsync(token);
            return (await Get(document.Project, token)).Where(d => d.Location.SourceTree == tree);
        }
        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken token)
            => Task.FromResult(Enumerable.Empty<Diagnostic>());
        public override async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken token)
            => await Get(project, token);
    }

    private static async Task<Solution> FixAll(Document document, string id, FixAllScope scope)
    {
        var context = new FixAllContext(document, Fixer, scope, id, [id], new DiagnosticProvider(), default);
        var action = await Fixer.GetFixAllProvider().GetFixAsync(context);
        action.ShouldNotBeNull();
        return (await action.GetOperationsAsync(default)).OfType<ApplyChangesOperation>().Single().ChangedSolution;
    }

    private static async Task<string> Run(Project project)
    {
        var compilation = (await project.GetCompilationAsync(Token))!;
        using var output = new MemoryStream();
        var emitted = compilation.Emit(output);
        Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics));
        output.Position = 0;
        var context = new AssemblyLoadContext("analyzer-test", isCollectible: true);
        try { return (string)context.LoadFromStream(output).GetType("Case")!.GetMethod("Run")!.Invoke(null, null)!; }
        finally { context.Unload(); }
    }

    [Fact]
    public void Code_fix_is_discoverable_through_MEF()
    {
        using var container = new System.Composition.Hosting.ContainerConfiguration()
            .WithAssembly(typeof(PostProcessCodeFixProvider).Assembly).CreateContainer();
        container.GetExports<CodeFixProvider>().Single().ShouldBeOfType<PostProcessCodeFixProvider>();
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
    [InlineData("double", "-0.0")]
    [InlineData("double", "double.PositiveInfinity")]
    [InlineData("DotCC.Libc.CBool", "42")]
    [InlineData("int*", "(int*)1")]
    [InlineData("delegate*<void>", "&Empty")]
    public async Task Code_fix_preserves_all_overloads_and_one_evaluation(string type, string value)
    {
        using var workspace = new AdhocWorkspace();
        var project = AddProject(workspace);
        var document = workspace.AddDocument(project.Id, "Case.cs", SourceText.From($$"""
            public static unsafe class Case {
                static int calls;
                static void Empty() {}
                static {{type}} Next() { ++calls; return {{value}}; }
                public static string Run() => Cond.B(Next()) + ":" + calls;
            }
            """));
        var diagnostic = (await Diagnostics(document)).Single(d => d.Id == CondId);
        var changed = await Apply(document, diagnostic);
        (await Run(changed.Project)).ShouldBe(await Run(document.Project));
        (await changed.GetTextAsync(Token)).ToString().ShouldNotContain("Cond.B(");
        (await Diagnostics(changed)).ShouldBeEmpty();
        // Registering and previewing a fix must not change the workspace itself.
        (await workspace.CurrentSolution.GetDocument(document.Id)!.GetTextAsync(Token)).ToString().ShouldContain("Cond.B(");
    }

    [Fact]
    public async Task Single_fix_handles_nested_calls_CBool_and_preserves_siblings()
    {
        using var workspace = new AdhocWorkspace();
        var document = workspace.AddDocument(AddProject(workspace).Id, "Case.cs", SourceText.From("""
            public static class Case {
                public static string Run() => Cond.B((DotCC.Libc.CBool)Cond.B(1)) + ":" + Cond.B(0);
            }
            """));
        var diagnostics = await Diagnostics(document);
        diagnostics.Length.ShouldBe(3);
        var outer = diagnostics.OrderByDescending(d => d.Location.SourceSpan.Length).First();
        var changed = await Apply(document, outer);
        (await Run(changed.Project)).ShouldBe(await Run(document.Project));
        (await Diagnostics(changed)).Count(d => d.Id == CondId).ShouldBe(1);
        (await changed.GetTextAsync(Token)).ToString().ShouldContain("Cond.B(0)");
    }

    [Fact]
    public async Task Empty_block_fix_uses_original_spans_and_keeps_required_bodies_and_comments()
    {
        using var workspace = new AdhocWorkspace();
        var document = workspace.AddDocument(AddProject(workspace, helper: false).Id, "Case.cs", SourceText.From("""
            public static class Case {
                public static string Run() {
                    { { /* inner */ } } { /* sibling */ }
                    if (true) {}
                    goto label;
                    label: {}
                    return "ok";
                }
            }
            """));
        var diagnostics = await Diagnostics(document);
        diagnostics.Length.ShouldBe(3);
        var outer = diagnostics.OrderByDescending(d => d.Location.SourceSpan.Length).First();
        var changed = await Apply(document, outer);
        (await Run(changed.Project)).ShouldBe("ok");
        var text = (await changed.GetTextAsync(Token)).ToString();
        text.ShouldContain("/* inner */"); text.ShouldContain("{ /* sibling */ }");
        text.ShouldContain("if (true) {}"); text.ShouldContain("label: {}");
        (await Diagnostics(changed)).Length.ShouldBe(1);
    }

    [Fact]
    public async Task Reports_generated_sources_and_honors_diagnostic_suppression()
    {
        using var workspace = new AdhocWorkspace();
        var document = workspace.AddDocument(AddProject(workspace).Id, "Case.g.cs", SourceText.From("""
            // <auto-generated/>
            public static class Case {
                static bool First() => Cond.B(1);
            #pragma warning disable DCCPP001
                static bool Second() => Cond.B(2);
            #pragma warning restore DCCPP001
                static void Empty() { {} }
            }
            """));
        var diagnostics = await Diagnostics(document);
        diagnostics.Count(d => d.Id == CondId).ShouldBe(1);
        diagnostics.Count(d => d.Id == BlockId).ShouldBe(1);
        var solution = await FixAll(document, CondId, FixAllScope.Document);
        (await solution.GetDocument(document.Id)!.GetTextAsync(Token)).ToString().ShouldContain("Cond.B(2)");
    }

    [Fact]
    public async Task Does_not_offer_fixes_for_observable_or_unknown_helpers_and_invalid_code()
    {
        using var workspace = new AdhocWorkspace();
        var project = AddProject(workspace);
        var document = workspace.AddDocument(project.Id, "Case.cs", SourceText.From("""
            public static class Case {
                static string Capture(System.Action action,
                    [System.Runtime.CompilerServices.CallerArgumentExpression("action")] string text = "") => text;
                static string Captured() => Capture(() => { {} if (Cond.B(1)) {} });
                static System.Linq.Expressions.Expression<System.Func<bool>> Tree() => () => Cond.B(2);
                static void Discarded() { Cond.B(3); }
            }
            """));
        (await Diagnostics(document)).ShouldBeEmpty();
        var broken = document.WithText(SourceText.From("class Broken { bool X => Cond.B(missing); }"));
        (await Diagnostics(broken)).ShouldBeEmpty();
        var unknown = workspace.AddDocument(AddProject(workspace, "Unknown", helper: false).Id, "Case.cs", SourceText.From("""
            static class Cond { public static bool B(int x) => x > 1; }
            class Case { bool X => Cond.B(3); }
            """));
        (await Diagnostics(unknown)).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(FixAllScope.Document, CondId)]
    [InlineData(FixAllScope.Project, CondId)]
    [InlineData(FixAllScope.Solution, CondId)]
    [InlineData(FixAllScope.Document, BlockId)]
    [InlineData(FixAllScope.Project, BlockId)]
    [InlineData(FixAllScope.Solution, BlockId)]
    public async Task Fix_all_respects_scope_and_selected_rule(FixAllScope scope, string id)
    {
        using var workspace = new AdhocWorkspace();
        var first = AddProject(workspace, "First");
        var a = workspace.AddDocument(first.Id, "A.cs", SourceText.From("class A { bool X() { {} return Cond.B(1); } }"));
        var b = workspace.AddDocument(first.Id, "B.cs", SourceText.From("class B { bool X() { {} return Cond.B(2); } }"));
        var other = AddProject(workspace, "Other");
        var c = workspace.AddDocument(other.Id, "C.cs", SourceText.From("class C { bool X() { {} return Cond.B(3); } }"));
        var solution = await FixAll(workspace.CurrentSolution.GetDocument(a.Id)!, id, scope);
        foreach (var document in new[] { a, b, c })
        {
            bool affected = document.Id == a.Id || scope == FixAllScope.Solution || scope == FixAllScope.Project && document.Project.Id == first.Id;
            var remaining = await Diagnostics(solution.GetDocument(document.Id)!);
            remaining.Count(d => d.Id == id).ShouldBe(affected ? 0 : 1);
            remaining.Count(d => d.Id != id).ShouldBe(1);
        }
    }

    [Fact]
    public async Task Fix_all_preserves_caller_lines_and_directives_and_is_idempotent()
    {
        using var workspace = new AdhocWorkspace();
        var document = workspace.AddDocument(AddProject(workspace).Id, "Case.cs", SourceText.From("""
            public static class Case {
                static int Line([System.Runtime.CompilerServices.CallerLineNumber] int line = 0) => line;
                public static string Run() {
                    {} { {} }
                    {
            #if NEVER
                        DisabledSource();
            #endif
                    }
                    return Cond.B(Cond.B(0)) + ":" + Line();
                }
            }
            """));
        var expected = await Run(document.Project);
        var solution = await FixAll(document, CondId, FixAllScope.Document);
        solution = await FixAll(solution.GetDocument(document.Id)!, BlockId, FixAllScope.Document);
        var changed = solution.GetDocument(document.Id)!;
        (await Run(changed.Project)).ShouldBe(expected);
        (await Diagnostics(changed)).ShouldBeEmpty();
        (await changed.GetTextAsync(Token)).ToString().ShouldContain("DisabledSource();");
    }

    [Fact]
    public async Task Fix_all_preserves_an_empty_top_level_entry_point()
    {
        using var workspace = new AdhocWorkspace();
        var project = AddProject(workspace, helper: false);
        var document = workspace.AddDocument(project.Id, "Program.cs", SourceText.From("{} { {} }"));
        document = document.Project.WithCompilationOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication)).GetDocument(document.Id)!;
        var solution = await FixAll(document, BlockId, FixAllScope.Document);
        var changed = solution.GetDocument(document.Id)!;
        (await changed.Project.GetCompilationAsync(Token))!.GetEntryPoint(Token).ShouldNotBeNull();
        (await Diagnostics(changed)).ShouldBeEmpty();
        (await changed.GetSyntaxRootAsync(Token))!.DescendantNodes().OfType<BlockSyntax>().Count().ShouldBe(1);
    }
}
