using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, false, SourceSplit.None)]
    [InlineData(false, true, SourceSplit.Function)]
    [InlineData(true, false, SourceSplit.Size)]
    [InlineData(true, true, SourceSplit.None)]
    public void Constant_profile_branches_preserve_effects_and_external_label_entry(bool objects, bool nested, SourceSplit split)
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == "constant-profile-branches");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-constant-branches-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var sources = fixture.sources;
            if (objects)
                sources = sources.Select((source, index) => {
                    var path = Path.Combine(directory, "unit" + index + ".obj.cs");
                    File.WriteAllText(path, Compiler.EmitObject(source));
                    return path;
                }).ToArray();
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C);
            var files = objects
                ? Compiler.LinkObjectFiles(sources, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Managed.Branches", split: split, outputOptions: options)
                : Compiler.EmitCSharpFiles(sources, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Managed.Branches", split: split, outputOptions: options);
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("BranchLibrary_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key, cancellationToken: TestContext.Current.CancellationToken)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("BranchConsumer_" + Guid.NewGuid().ToString("N"), """
                public static class Consumer {
                    public static int Run() {
                        if (Managed.Branches.Api.check() != 42) return 1;
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                        return Managed.Branches.Api.check() == 42 ? 0 : 2;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("branches-" + Guid.NewGuid(), isCollectible: false);
            image.Position = 0;
            context.LoadFromStream(image);
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
