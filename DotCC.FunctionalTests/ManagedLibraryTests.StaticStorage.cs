using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using DotCC;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Self_linked_array_addresses_survive_collection_and_object_linking(bool link)
    {
        var fixture = FixtureRunner.Discover().Single(f => f.name == "self-linked-static-storage");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-static-graph-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var paths = fixture.sources;
            if (link)
                paths = paths.Select((source, index) =>
                {
                    var path = Path.Combine(directory, index + ".o");
                    File.WriteAllText(path, Compiler.EmitObject(source, includeDirs: new[] { fixture.dir }));
                    return path;
                }).ToArray();
            var options = new CSharpOutputOptions(NestTypes: true, Runtime: RuntimeProfile.C);
            var files = link
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "StaticGraph", outputOptions: options)
                : Compiler.EmitCSharpFiles(paths, includeDirs: new[] { fixture.dir }, emit: EmitMode.ManagedLib,
                    className: "Api", namespaceName: "StaticGraph", outputOptions: options);
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("StaticGraphLibrary" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("StaticGraphConsumer" + Guid.NewGuid().ToString("N"), """
                using StaticGraph;
                public static class Consumer {
                    public static bool Check() {
                        ulong address = Api.storage_address();
                        if (address == 0 || Api.storage_identity_check() != 1) return false;
                        for (int i = 0; i < 3; i++) {
                            _ = new byte[100000];
                            System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                            System.GC.WaitForPendingFinalizers();
                            if (address != Api.storage_address() || Api.storage_identity_check() != 1) return false;
                        }
                        return true;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("static-graph-" + Guid.NewGuid(), isCollectible: false);
            image.Position = 0;
            context.LoadFromStream(image);
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Check")!.Invoke(null, null).ShouldBe(true);
        }
        finally { Directory.Delete(directory, true); }
    }
}
