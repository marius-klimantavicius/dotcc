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
    [InlineData(false)]
    [InlineData(true)]
    public void Instance_cleanup_callbacks_cancel_owned_workers(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-cancel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var fixture = FixtureRunner.Discover().Single(f => f.name == "pthread-cancel");
            string path = Path.Combine(fixture.dir, "main.c");
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true);
            string generated;
            if (objectLink)
            {
                string obj = Path.Combine(directory, "cancel.o");
                File.WriteAllText(obj, Compiler.EmitObject(path, outputOptions: new(InstanceMethods: true)));
                generated = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            const string host = """
                public static class CancelProbe {
                    public static int Run() {
                        using var a = new Api(); using var b = new Api();
                        using (b.__DotCcEnter()) {
                            using (a.__DotCcEnter()) if (a.test() != 0) return 1;
                            if (b.test() != 0) return 1;
                            if (a.__DotCcRuntime.Termination != null || b.__DotCcRuntime.Termination != null) return 2;
                            return 0;
                        }
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("Cancellation_" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated), ParseSource(host) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("cancellation-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            ((int)assembly.GetType("CancelProbe")!.GetMethod("Run")!.Invoke(null, null)!).ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
