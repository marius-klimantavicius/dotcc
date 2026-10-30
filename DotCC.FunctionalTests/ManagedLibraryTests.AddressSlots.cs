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
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Cached_pointer_and_scalar_slots_preserve_values_GC_and_context_owners(bool link, bool contextual)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "scoped-global-address-slots", "main.c");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-address-slots-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            if (link) { var obj = Path.Combine(directory, "slots.o"); File.WriteAllText(obj, Compiler.EmitObject(path)); path = obj; }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, StateContext: contextual);
            var generated = link
                ? Compiler.LinkObjects([path], emit: EmitMode.ManagedLib, className: "Api", outputOptions: options)
                : Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            generated.ShouldNotContain("__DotccGlobalAddressCache");
            if (contextual) generated.ShouldNotContain("__dotcc_globalAddress_");
            else
            {
                generated.ShouldContain("static unsafe nint* __dotcc_globalAddress_");
                generated.ShouldContain("static unsafe int* __dotcc_globalAddress_");
                generated.ShouldContain("static unsafe CBool* __dotcc_globalAddress_");
            }
            var host = contextual ? """
                public static unsafe class SlotProbe {
                    private static Api.__DotCcContext target;
                    private static System.IDisposable switchOwner;
                    private static int Collect() {
                        _ = new byte[100000];
                        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                        switchOwner = target.Enter();
                        return 0;
                    }
                    public static int Check() {
                        using var first = Api.__DotCcCreateContext();
                        using var second = Api.__DotCcCreateContext();
                        target = second;
                        using (second.Enter()) { Api.Globals.second.value = 37; }
                        using (first.Enter()) {
                            int result = Api.run(&Collect);
                            switchOwner.Dispose();
                            if (Api.current_value() != 7 || Api.tally_run() != 11 || Api.bool_run() != 2) return -1;
                            return result;
                        }
                    }
                }
                """ : """
                public static unsafe class SlotProbe {
                    private static int Collect() {
                        _ = new byte[100000];
                        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                        return 0;
                    }
                    public static int Check() {
                        int result = Api.run(&Collect);
                        return Api.current_value() == 19 && Api.tally_run() == 11 && Api.bool_run() == 2 ? result : -1;
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("AddressSlots" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated), ParseSource(host) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            stream.Position = 0;
            var assembly = new AssemblyLoadContext("slots-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(stream);
            assembly.GetType("SlotProbe")!.GetMethod("Check")!.Invoke(null, null).ShouldBe(contextual ? 44 : 26);
        }
        finally { Directory.Delete(directory, true); }
    }
}
