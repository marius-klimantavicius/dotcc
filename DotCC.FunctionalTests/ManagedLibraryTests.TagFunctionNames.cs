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
    [InlineData(false, false, SourceSplit.Size)]
    [InlineData(true, false, SourceSplit.Function)]
    [InlineData(true, true, SourceSplit.None)]
    public void Nested_C_tags_and_functions_keep_separate_namespaces(bool objects, bool reverse, SourceSplit split)
    {
        var fixture = FixtureRunner.Discover().Single(row => row.name == "tag-function-names");
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-tag-functions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var first = Path.Combine(dir, "first.c");
            var second = Path.Combine(dir, "second.c");
            // The Remote tag and function definition meet only at object link.
            File.WriteAllText(first, "struct Remote { int value; }; int Remote(void); int relay(void) { return Remote(); }");
            File.WriteAllText(second, "int Remote(void) { return 3; }");
            var sources = fixture.sources.Concat(new[] { first, second }).ToArray();
            var paths = sources;
            if (objects)
                paths = sources.Select((source, index) => {
                    var path = Path.Combine(dir, "unit" + index + ".obj.cs");
                    File.WriteAllText(path, Compiler.EmitObject(source));
                    return path;
                }).ToArray();
            if (reverse) Array.Reverse(paths);
            var options = new CSharpOutputOptions(NestTypes:true, Runtime:RuntimeProfile.C);
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit:EmitMode.ManagedLib, className:"Api", namespaceName:"Managed.Tags", split:split, outputOptions:options)
                : Compiler.EmitCSharpFiles(paths, emit:EmitMode.ManagedLib, className:"Api", namespaceName:"Managed.Tags", split:split, outputOptions:options);
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("TagLibrary_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path:file.Key)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe:true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken:TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("TagConsumer_" + Guid.NewGuid().ToString("N"), """
                using Api = Managed.Tags.Api;
                public static unsafe class Consumer {
                    public static int Run() {
                        Api.__DotCcTags original = new Api.__DotCcTags { marker = 7 };
                        Api.Peer peer = default;
                        peer.entry.value = 20;
                        Api.__DotCcTags_.Entry entry = default;
                        entry.value = 21; entry.peer = &peer;
                        Api.__DotCcTags_.Number number = default; number.value = 1;
                        if (original.marker != 7 || sizeof(Api.__DotCcTags_.Remote) != 4) return 1;
                        if (Api.Entry(&entry) != 41 || Api.Number(&number) != 1) return 2;
                        if (Api.Mode(Api.__DotCcTags_.Mode.ModeOn) != 1 || Api.relay() != 3) return 3;
                        if (Api.@event(Api.__DotCcTags_.@event.EventReady) != 2) return 5;
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                        return Api.check() == 42 ? 0 : 4;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("tag-functions-" + Guid.NewGuid(), isCollectible:false);
            image.Position = 0;
            context.LoadFromStream(image);
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(dir, true); }
    }
}
