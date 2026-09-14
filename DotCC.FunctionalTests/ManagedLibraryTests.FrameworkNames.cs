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
    [InlineData(false, SourceSplit.None, RuntimeProfile.C)]
    [InlineData(true, SourceSplit.Function, RuntimeProfile.C)]
    [InlineData(false, SourceSplit.Size, RuntimeProfile.All)]
    [InlineData(true, SourceSplit.None, RuntimeProfile.All)]
    public void Nested_System_record_preserves_public_name_and_framework_bindings(bool objects, SourceSplit split, RuntimeProfile runtime)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-framework-names-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var source = Path.Combine(dir, "source.c");
            File.WriteAllText(source, """
                #include <stddef.h>
                struct System { int x; union { long value; unsigned char bytes[8]; }; };
                typedef long Pair[2];
                _Thread_local Pair scratch;
                int inspect(struct System *state) {
                    unsigned __int128 wide = ((unsigned __int128)1 << 65) + 5;
                    scratch[0] = (long)(wide >> 65);
                    return state->x + (int)state->value + (int)scratch[0];
                }
                unsigned swap(unsigned value) { return __builtin_bswap32(value); }
                unsigned long field_offset(void) { return offsetof(struct System, value); }
                const char *text(void) { return "System.Runtime is literal data"; }
                """);
            var input = source;
            if (objects) {
                input = Path.Combine(dir, "source.o");
                File.WriteAllText(input, Compiler.EmitObject(source));
            }
            var options = new CSharpOutputOptions(NestTypes: true, Runtime: runtime);
            var files = objects
                ? Compiler.LinkObjectFiles(new[] { input }, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Managed.Emulation", split: split, outputOptions: options)
                : Compiler.EmitCSharpFiles(new[] { input }, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Managed.Emulation", split: split, outputOptions: options);
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("FrameworkLibrary_" + Guid.NewGuid().ToString("N"),
                files.Select(f => ParseSource(f.Value, path: f.Key, cancellationToken: TestContext.Current.CancellationToken)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("FrameworkConsumer_" + Guid.NewGuid().ToString("N"), """
                public static unsafe class Consumer {
                    public static int Run() {
                        Managed.Emulation.Api.System state = default;
                        state.x = 30;
                        state.value = 11;
                        if (Managed.Emulation.Api.inspect(&state) != 42) return 1;
                        if (Managed.Emulation.Api.swap(0x01020304) != 0x04030201) return 2;
                        if (sizeof(Managed.Emulation.Api.System) != 16 || Managed.Emulation.Api.field_offset() != 8) return 3;
                        global::System.GC.Collect(2, global::System.GCCollectionMode.Forced, true, true);
                        if (Managed.Emulation.Api.inspect(&state) != 42) return 4;
                        if (global::System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)Managed.Emulation.Api.text()) != "System.Runtime is literal data") return 5;
                        return 0;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("framework-names-" + Guid.NewGuid(), isCollectible: false);
            image.Position = 0;
            var library = context.LoadFromStream(image);
            library.GetType("Managed.Emulation.Api+System").ShouldNotBeNull();
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(dir, true); }
    }
}
