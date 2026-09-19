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
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void External_globals_resolve_definition_storage(bool objects, bool nested, bool reverse)
    {
        RunGlobalStorageCase(objects, nested, reverse, "Api", """
            _Thread_local int values[2];
            _Alignas(32) int aligned = 7;
            """, """
            extern _Thread_local int values[2];
            extern int aligned;
            int *initial_address = &aligned;
            int *array_address(void) { return values; }
            int *aligned_address(void) { return &aligned; }
            int check_initializer(void) { return initial_address == &aligned; }
            """, """
            int* main = Review.Api.array_address();
            if (main == null || main[0] != 0 || main[1] != 0) return 1;
            main[0] = 42;
            int error = 0;
            var worker = new System.Threading.Thread(() => {
                int* other = Review.Api.array_address();
                if (other == null || other == main || other[0] != 0) { error = 2; return; }
                other[0] = 9;
                System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                if (Review.Api.array_address() != other || other[0] != 9) error = 3;
            });
            worker.Start();
            if (!worker.Join(10000)) return 4;
            if (error != 0) return error;
            if (Review.Api.array_address() != main || main[0] != 42) return 5;
            int* aligned = Review.Api.aligned_address();
            if ((nuint)aligned % 32 != 0 || *aligned != 7) return 6;
            return Review.Api.check_initializer() == 1 ? 0 : 7;
            """);
    }

    [Theory]
    [InlineData(false, false, "Api")]
    [InlineData(false, true, "Api")]
    [InlineData(true, false, "Api")]
    [InlineData(true, true, "Api")]
    [InlineData(false, true, "Globals")]
    [InlineData(true, true, "Globals")]
    public void Global_storage_names_do_not_shadow_user_symbols(bool objects, bool nested, string owner)
    {
        var functions = owner == "Globals" ? "" : "int Globals(void) { return value; }";
        RunGlobalStorageCase(objects, nested, false, owner, """
            int value = 7;
            _Thread_local int per_thread;
            struct ThreadGlobals { int x; };
            """, """
            extern int value;
            extern _Thread_local int per_thread;
            int ThreadGlobals(void) { return value; }
            int __threadGlobals(void) { return value; }
            int inspect(int Globals, int ThreadGlobals, int DotCcFunctions) {
                per_thread = Globals + ThreadGlobals + DotCcFunctions;
                return value + per_thread;
            }
            const char *literal(void) { return "/*__dotcc_global_ref__*/value"; }
            """ + functions, $$"""
            if (Review.{{owner}}.inspect(1, 2, 3) != 13) return 1;
            if (Review.{{owner}}.ThreadGlobals() != 7 || Review.{{owner}}.__threadGlobals() != 7) return 2;
            return System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)Review.{{owner}}.literal())
                == "/*__dotcc_global_ref__*/value" ? 0 : 3;
            """);
    }

    private static void RunGlobalStorageCase(bool objects, bool nested, bool reverse, string owner,
        string definitions, string uses, string consumerBody)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-global-storage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var paths = new[] { Path.Combine(directory, "definitions.c"), Path.Combine(directory, "uses.c") };
            File.WriteAllText(paths[0], definitions);
            File.WriteAllText(paths[1], uses);
            if (objects)
                paths = paths.Select(path => {
                    var obj = Path.ChangeExtension(path, ".cs");
                    File.WriteAllText(obj, Compiler.EmitObject(path));
                    return obj;
                }).ToArray();
            if (reverse) Array.Reverse(paths);
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C);
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: owner, namespaceName: "Review", split: SourceSplit.Function, outputOptions: options)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: owner, namespaceName: "Review", split: SourceSplit.Function, outputOptions: options);
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("GlobalStorageLibrary_" + Guid.NewGuid().ToString("N"),
                files.Select(f => ParseSource(f.Value, path: f.Key, cancellationToken: TestContext.Current.CancellationToken)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("GlobalStorageConsumer_" + Guid.NewGuid().ToString("N"),
                "public static unsafe class Consumer { public static int Run() { " + consumerBody + " } }", references);
            var context = new AssemblyLoadContext("global-storage-" + Guid.NewGuid(), isCollectible: false);
            image.Position = 0;
            context.LoadFromStream(image);
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Zig_debug_print_binds_runtime_stderr(bool objects)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-debug-stderr-" + Guid.NewGuid().ToString("N") + ".zig");
        var obj = Path.ChangeExtension(path, ".cs");
        try
        {
            File.WriteAllText(path, "const std = @import(\"std\"); pub fn main() void { std.debug.print(\"hello\\n\", .{}); }");
            string emitted;
            if (objects)
            {
                File.WriteAllText(obj, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib);
            Compile("DebugStderr_" + Guid.NewGuid().ToString("N"), emitted, RuntimeReferences());
        }
        finally { File.Delete(path); File.Delete(obj); }
    }
}
