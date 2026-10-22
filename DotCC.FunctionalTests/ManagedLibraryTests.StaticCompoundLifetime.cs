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
    public void Static_compound_objects_keep_independent_owner_lifetimes(bool objects)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-static-compound-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "literal.c");
            File.WriteAllText(path, """
                struct Item { int value; };
                static struct Item *items[] = { &(struct Item){19}, &(struct Item){23} };
                int read(void) { return items[0]->value + items[1]->value; }
                void change(void) { items[0]->value=41; }
                unsigned long address(void) { return (unsigned long)items[0]; }
                """);
            if (objects)
            {
                string obj = path + ".cs";
                File.WriteAllText(obj, Compiler.EmitObject(path));
                path = obj;
            }
            var options = new CSharpOutputOptions(NestTypes: true, Runtime: RuntimeProfile.C, StateContext: true);
            var files = objects
                ? Compiler.LinkObjectFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options)
                : Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            const string host = """
                public static class StaticCompoundProbe {
                    public static string Run() {
                        using var a=Api.__DotCcCreateContext();
                        using var b=Api.__DotCcCreateContext();
                        ulong address;
                        using(a.Enter()) {
                            if(Api.read()!=42) return "initial a";
                            address=Api.address(); Api.change();
                        }
                        System.GC.Collect(); System.GC.WaitForPendingFinalizers(); System.GC.Collect();
                        using(b.Enter()) {
                            if(Api.read()!=42 || Api.address()==address) return "independent b";
                        }
                        using(a.Enter()) {
                            if(Api.read()!=64 || Api.address()!=address) return "retained a";
                        }
                        a.Dispose();
                        using var fresh=Api.__DotCcCreateContext();
                        using(fresh.Enter()) { if(Api.read()!=42) return "fresh"; }
                        using(b.Enter()) { if(Api.read()!=42) return "retained b"; }
                        return "ok";
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("StaticCompounds_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key)).Append(ParseSource(host)), RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("static-compounds-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            ((string)assembly.GetType("StaticCompoundProbe")!.GetMethod("Run")!.Invoke(null, null)!).ShouldBe("ok");
        }
        finally { Directory.Delete(directory, true); }
    }
}
