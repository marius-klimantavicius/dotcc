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
    public void Cached_global_addresses_track_GC_and_rebind_after_owner_changing_calls(bool link, bool contextual)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-addresses-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "addresses.c");
            File.WriteAllText(path, """
                struct State { int cells[3]; } state = {{7,11,13}};
                int run(int (*callback)(void)) {
                    int before = state.cells[0] + state.cells[1];
                    if (before < 0) goto done;
                    callback();
                done:
                    return before + state.cells[0] + state.cells[1];
                }
                int pure(void) {
                    int before = state.cells[0];
                    state.cells[1] += 2;
                    return before + state.cells[1] + state.cells[0];
                }
                int conditional(int yes) {
                    if (yes) { return state.cells[0] + state.cells[1]; }
                    return 42;
                }
                int short_circuit(int yes) { return yes && (state.cells[0] + state.cells[1]); }
                int choose(int yes) { return yes ? state.cells[0] + state.cells[1] : 23; }
                int repeat(int count) {
                    int total = 0;
                    while (count-- > 0) total += state.cells[0] + state.cells[1];
                    return total;
                }
                int labeled(int skip) {
                    int total = 0;
                    if (skip) goto done;
                    total = state.cells[0] + state.cells[1];
                done:
                    return total;
                }
                int backward(int count) {
                    int total = 0;
                again:
                    if (count-- <= 0) return total;
                    total += state.cells[0] + state.cells[1];
                    goto again;
                }
                int single_loop(int count) {
                    int total = 0;
                    while (count-- > 0) total += state.cells[0];
                    return total;
                }
                """);
            if (link) { var obj = path + ".o"; File.WriteAllText(obj, Compiler.EmitObject(path)); path = obj; }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, StateContext: contextual);
            var generated = link
                ? Compiler.LinkObjects([path], emit: EmitMode.ManagedLib, className: "Api", outputOptions: options)
                : Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            generated.ShouldContain(contextual ? "ref State __dotcc_address" : "__DotccGlobalAddressCache __dotcc_address");
            var host = contextual ? """
                public static unsafe class Probe {
                    private static Api.__DotCcContext target;
                    private static System.IDisposable switchOwner;
                    private static int Collect() {
                        _ = new byte[100000];
                        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                        switchOwner = target.Enter();
                        return 0;
                    }
                    public static int Check() {
                        // There is no current owner here. Hoisting a pure
                        // address onto any unexecuted branch would throw.
                        if (Api.conditional(0) != 42 || Api.short_circuit(0) != 0 ||
                            Api.choose(0) != 23 || Api.repeat(0) != 0 ||
                            Api.labeled(1) != 0 || Api.backward(0) != 0 || Api.single_loop(0) != 0) return -2;
                        using var first = Api.__DotCcCreateContext();
                        using var second = Api.__DotCcCreateContext();
                        target = second;
                        using (second.Enter()) { Api.Globals.state.cells[0] = 19; }
                        int value;
                        using (first.Enter()) {
                            if (Api.conditional(1) != 18 || Api.choose(1) != 18 || Api.repeat(3) != 54 ||
                                Api.labeled(0) != 18 || Api.backward(3) != 54 || Api.single_loop(3) != 21) return -3;
                            value = Api.run(&Collect);
                            switchOwner.Dispose();
                            if (Api.pure() != 27) return -1;
                        }
                        return value;
                    }
                }
                """ : """
                public static unsafe class Probe {
                    private static int Collect() {
                        _ = new byte[100000];
                        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                        return 0;
                    }
                    public static int Check() {
                        if (Api.conditional(0) != 42 || Api.short_circuit(0) != 0 ||
                            Api.choose(0) != 23 || Api.repeat(0) != 0 || Api.repeat(3) != 54 ||
                            Api.labeled(1) != 0 || Api.labeled(0) != 18 || Api.backward(3) != 54 || Api.single_loop(3) != 21) return -2;
                        int value = Api.run(&Collect);
                        return Api.pure() == 27 ? value : -1;
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("AddressReuse" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated), ParseSource(host) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            stream.Position = 0;
            var assembly = new AssemblyLoadContext("addresses-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(stream);
            assembly.GetType("Probe")!.GetMethod("Check")!.Invoke(null, null).ShouldBe(contextual ? 48 : 36);
        }
        finally { Directory.Delete(directory, true); }
    }
}
