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
    [InlineData(false, SourceSplit.None, EmitMode.ManagedLib, "Managed.Database")]
    [InlineData(false, SourceSplit.Function, EmitMode.ManagedLib, "Managed.Database")]
    [InlineData(true, SourceSplit.Function, EmitMode.ManagedLib, "Managed.Database")]
    [InlineData(true, SourceSplit.Size, EmitMode.ManagedLib, "sample.class")]
    [InlineData(false, SourceSplit.None, EmitMode.Csproj, "Managed.Database")]
    [InlineData(true, SourceSplit.Function, EmitMode.Csproj, "Managed.Database")]
    [InlineData(false, SourceSplit.Size, EmitMode.SharedLib, "Managed.Database")]
    public void Namespaced_output_preserves_runtime_types_enums_callbacks_and_entry_points(bool link, SourceSplit split, EmitMode mode, string namespaceName)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-namespace-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = Path.ChangeExtension(path, ".cs");
        try
        {
            File.WriteAllText(path, """
                #include <stdlib.h>
                enum Kind { First = 3 }; int Kind = 4;
                typedef int (*Callback)(int);
                int add(int x) { return x + 1; }
                Callback current = add;
                Callback get(void) { return add; }
                Callback runtime(void) { return abs; }
                _Bool truth(int x) { return x != 0; }
                int run(void) { return current(33) + First + Kind + truth(1); }
                int main(void) { return run() == 42 ? 0 : 1; }
                const char *text(void) { return "global::DotCcFunctionPointers namespace Managed.Database"; }
                """);
            if (link) File.WriteAllText(obj, Compiler.EmitObject(path));
            var api = mode == EmitMode.Csproj ? null : "Api";
            var files = link
                ? Compiler.LinkObjectFiles(new[] { obj }, emit: mode, className: api, split: split, namespaceName: namespaceName)
                : Compiler.EmitCSharpFiles(new[] { path }, emit: mode, className: api, split: split, namespaceName: namespaceName);
            var escaped = namespaceName.Replace(".class", ".@class");
            files[(api ?? "DotCcProgram") + ".cs"].ShouldContain("namespace " + escaped + ";");
            var owner = "global::" + escaped + "." + (api ?? "DotCcProgram");
            var consumer = $$"""
                public static unsafe class Consumer {
                    public static int Run() {
                        if ({{owner}}.get() != global::{{escaped}}.{{(api ?? "DotCcProgram")}}FunctionPointers.add) return -1;
                        if ({{owner}}.runtime()(-42) != 42) return -2;
                        if (System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint){{owner}}.text()) != "global::DotCcFunctionPointers namespace Managed.Database") return -3;
                        return {{owner}}.run();
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("Namespaced_" + Guid.NewGuid().ToString("N"),
                files.Select(f => CSharpSyntaxTree.ParseText(f.Value, path: f.Key)).Append(CSharpSyntaxTree.ParseText(consumer, cancellationToken: TestContext.Current.CancellationToken)),
                RuntimeReferences(), new CSharpCompilationOptions(mode == EmitMode.Csproj ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var context = new AssemblyLoadContext("ns-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var assembly = context.LoadFromStream(image);
            assembly.GetType("CBool").ShouldBeNull();
            assembly.GetType(namespaceName + ".CBool").ShouldNotBeNull();
            assembly.GetType("Consumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(42);
            if (mode == EmitMode.Csproj) assembly.EntryPoint!.Invoke(null, new object[] { Array.Empty<string>() }).ShouldBe(0);
        }
        finally { File.Delete(path); File.Delete(obj); }
    }

    [Theory]
    [InlineData("")]
    [InlineData("A..B")]
    [InlineData("global::A")]
    [InlineData("A.9B")]
    [InlineData("A; class Inject {}")]
    [InlineData("@@A")]
    public void Invalid_namespace_is_rejected_before_reading_inputs(string name)
    {
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(Array.Empty<string>(), namespaceName: name)).Message.ShouldContain("--namespace");
        Should.Throw<CompileException>(() => Compiler.LinkObjects(Array.Empty<string>(), namespaceName: name)).Message.ShouldContain("--namespace");
    }
}
