using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace DotCC.PostProcess.Analyzers.Tests;

public sealed partial class AnalyzerTests
{
    public static bool HasSqliteSnapshot => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTCC_POSTPROCESS_SQLITE_SNAPSHOT"));

    [Fact(Skip = "Set DOTCC_POSTPROCESS_SQLITE_SNAPSHOT to an original/optimized standalone SQLite snapshot.", SkipUnless = nameof(HasSqliteSnapshot))]
    public async Task Sqlite_fix_all_matches_standalone_output()
    {
        var snapshot = Path.GetFullPath(Environment.GetEnvironmentVariable("DOTCC_POSTPROCESS_SQLITE_SNAPSHOT")!);
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(snapshot, "manifest.json"), Token));
        int calls = manifest.RootElement.GetProperty("Rewritten").GetInt32();
        int blocks = manifest.RootElement.GetProperty("RemovedEmptyBlocks").GetInt32();
        calls.ShouldBeGreaterThan(0);
        blocks.ShouldBeGreaterThan(0);
        var originalDirectory = Path.Combine(snapshot, "Original");
        var projectXml = XDocument.Load(Path.Combine(originalDirectory, "Original.csproj"));
        string Property(string name) => projectXml.Root!.Elements("PropertyGroup").Elements(name).First().Value;
        var references = projectXml.Descendants("ReferencePathWithRefAssemblies").Attributes("Include")
            .Select(a => MetadataReference.CreateFromFile(Path.GetFullPath(a.Value, originalDirectory))).ToArray();
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject(ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Sqlite", "Sqlite",
            LanguageNames.CSharp, compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true, optimizationLevel: OptimizationLevel.Release,
                checkOverflow: bool.Parse(Property("CheckForOverflowUnderflow"))),
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview,
                preprocessorSymbols: Property("DefineConstants").Split(';')), metadataReferences: references));
        var paths = new Dictionary<DocumentId, string>();
        foreach (var source in projectXml.Descendants("Compile").Attributes("Include"))
        {
            var path = Path.GetFullPath(source.Value, originalDirectory);
            var document = workspace.AddDocument(project.Id, source.Value, SourceText.From(await File.ReadAllTextAsync(path, Token)));
            paths.Add(document.Id, source.Value);
        }
        project = workspace.CurrentSolution.GetProject(project.Id)!;
        var timer = Stopwatch.StartNew();
        var diagnostics = await Diagnostics(project, Token);
        diagnostics.Count(d => d.Id == CondId).ShouldBe(calls);
        diagnostics.Count(d => d.Id == BlockId).ShouldBe(blocks);
        var solution = await FixAll(project.Documents.First(), CondId, FixAllScope.Project);
        solution = await FixAll(solution.GetProject(project.Id)!.Documents.First(), BlockId, FixAllScope.Project);
        foreach (var document in solution.GetProject(project.Id)!.Documents)
        {
            var expected = await File.ReadAllTextAsync(Path.Combine(snapshot, "Optimized", paths[document.Id]), Token);
            (await document.GetTextAsync(Token)).ToString().ShouldBe(expected, paths[document.Id]);
        }
        (await Diagnostics(solution.GetProject(project.Id)!, Token)).ShouldBeEmpty();
        using var output = new MemoryStream();
        var emit = (await solution.GetProject(project.Id)!.GetCompilationAsync(Token))!.Emit(output, cancellationToken: Token);
        Assert.True(emit.Success, string.Join("\n", emit.Diagnostics));
        TestContext.Current.TestOutputHelper!.WriteLine($"SQLite IDE Fix All: {calls} calls, {blocks} blocks; exact standalone source match; {timer.Elapsed.TotalSeconds:F2}s including diagnostics, revalidation and emit.");
    }
}
