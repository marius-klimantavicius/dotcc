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
    public void Authored_allocator_overrides_preserve_calls_ownership_and_function_pointers(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-allocator-override-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                #include <stdlib.h>
                struct value { int n; };
                int check(void) {
                    struct value* p = (struct value*)malloc(sizeof(struct value));
                    p->n = 37;
                    int result = p->n;
                    free(p);
                    char* buffer = (char*)malloc(16);
                    buffer[0] = 5;
                    result += buffer[0];
                    free(buffer);
                    void* (*allocate)(size_t) = malloc;
                    void (*release)(void*) = free;
                    void* q = allocate(9);
                    release(q);
                    return result;
                }
                """);
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
            {
                new FunctionOverride("malloc", new("void*", new[] { "size_t" }),
                    new("managedMethod", "global::NativeAllocator.Allocate"), RequireMatch: true),
                new FunctionOverride("free", new("void", new[] { "void*" }),
                    new("managedMethod", "global::NativeAllocator.Release"), RequireMatch: true),
            });
            if (objectLink)
            {
                var obj = Path.ChangeExtension(path, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: options));
                path = obj;
            }
            var generated = objectLink
                ? Compiler.LinkObjects(new[] { path }, emit: EmitMode.ManagedLib, className: "Api")
                : Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", preprocessing: options);
            const string host = """
                public static unsafe class NativeAllocator {
                    public static int Allocations, Releases;
                    public static ulong TotalBytes;
                    public static void* Allocate(ulong size) {
                        Allocations++; TotalBytes += size;
                        return System.Runtime.InteropServices.NativeMemory.Alloc((nuint)size);
                    }
                    public static void Release(void* p) {
                        Releases++;
                        System.Runtime.InteropServices.NativeMemory.Free(p);
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("AllocatorOverride_" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated), ParseSource(host) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("allocator-override-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            ((int)assembly.GetType("Api")!.GetMethod("check")!.Invoke(null, null)!).ShouldBe(42);
            var allocator = assembly.GetType("NativeAllocator")!;
            ((int)allocator.GetField("Allocations")!.GetValue(null)!).ShouldBe(3);
            ((int)allocator.GetField("Releases")!.GetValue(null)!).ShouldBe(3);
            ((ulong)allocator.GetField("TotalBytes")!.GetValue(null)!).ShouldBe(29UL);
        }
        finally { Directory.Delete(directory, true); }
    }
}
