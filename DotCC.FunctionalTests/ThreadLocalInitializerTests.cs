using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ThreadLocalInitializerTests
{
    [Theory]
    [InlineData(false, "static")]
    [InlineData(true, "static")]
    [InlineData(false, "context")]
    [InlineData(true, "context")]
    [InlineData(false, "instance")]
    [InlineData(true, "instance")]
    public void Constant_tls_initializers_run_per_thread_and_owner_with_stable_addresses(bool objects, string mode)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-tls-init-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string[] sources = [
                "_Thread_local int index = -1; _Thread_local unsigned long mask = 1UL << 32; _Thread_local int *empty = 0;",
                """
                extern _Thread_local int index;
                extern _Thread_local unsigned long mask;
                extern _Thread_local int *empty;
                int get_index(void) { return index; }
                void set_index(int value) { index = value; }
                long address(void) { return (long)&index; }
                long pointer_address(void) { return (long)&empty; }
                int constants(void) { return mask == (1UL << 32) && empty == 0; }
                int pointer_roundtrip(void) { empty = &index; int value = *empty; empty = 0; return value; }
                int step(void) { static _Thread_local int counter = 7; return ++counter; }
                """];
            var paths = sources.Select((source, i) =>
            {
                string path = Path.Combine(directory, "unit" + i + ".c");
                File.WriteAllText(path, source);
                return path;
            }).ToArray();
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C,
                InstanceMethods: mode == "instance", StateContext: mode == "context");
            string emitted;
            if (objects)
            {
                var fragments = paths.Select(path =>
                {
                    string fragment = path + ".o.cs";
                    File.WriteAllText(fragment, Compiler.EmitObject(path,
                        outputOptions: mode == "instance" ? new(InstanceMethods: true) : null));
                    return fragment;
                }).ToArray();
                emitted = Compiler.LinkObjects(fragments, emit: EmitMode.ManagedLib,
                    className: "TlsApi", outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib,
                className: "TlsApi", outputOptions: options);
            string owner = mode == "instance" ? "api" : "TlsApi";
            string setup = mode == "instance" ? "using var api = new TlsApi();"
                : mode == "context" ? "using var api = TlsApi.__DotCcCreateContext();" : "";
            string binding = mode == "context" ? "using var binding = api.Enter();" : "";
            string secondOwner = mode == "static" ? "" : mode == "instance" ? """
                using (var other = new TlsApi()) {
                    if (other.get_index() != -1 || other.step() != 8 || other.address() == mainAddress) return 11;
                    other.set_index(200);
                    if (api.get_index() != 100) return 12;
                }
                """ : """
                using (var other = TlsApi.__DotCcCreateContext())
                using (other.Enter()) {
                    if (TlsApi.get_index() != -1 || TlsApi.step() != 8 || TlsApi.address() == mainAddress) return 11;
                    TlsApi.set_index(200);
                }
                if (TlsApi.get_index() != 100) return 12;
                """;
            emitted += $$"""

                public static class TlsProbe {
                    public static int Main() {
                        {{setup}}
                        {{binding}}
                        if ({{owner}}.get_index() != -1 || {{owner}}.constants() != 1 || {{owner}}.pointer_roundtrip() != -1 || {{owner}}.step() != 8) return 1;
                        long mainAddress = {{owner}}.address();
                        {{owner}}.set_index(100);
                        {{secondOwner}}
                        var addresses = new long[2];
                        var errors = new int[2];
                        using var ready = new System.Threading.CountdownEvent(2);
                        using var release = new System.Threading.ManualResetEventSlim(false);
                        var threads = new System.Threading.Thread[2];
                        for (int i = 0; i < 2; ++i) {
                            int slot = i;
                            threads[i] = new System.Threading.Thread(() => {
                                try {
                                    {{binding.Replace("var binding", "var workerBinding")}}
                                    if ({{owner}}.get_index() != -1 || {{owner}}.constants() != 1 || {{owner}}.pointer_roundtrip() != -1 || {{owner}}.step() != 8) errors[slot] = 2;
                                    addresses[slot] = {{owner}}.address();
                                    long pointerAddress = {{owner}}.pointer_address();
                                    {{owner}}.set_index(20 + slot);
                                    ready.Signal();
                                    if (!release.Wait(10000)) { errors[slot] = 3; return; }
                                    if ({{owner}}.get_index() != 20 + slot || {{owner}}.step() != 9 ||
                                        {{owner}}.address() != addresses[slot] || {{owner}}.pointer_address() != pointerAddress) errors[slot] = 4;
                                } catch { errors[slot] = 5; }
                            });
                            threads[i].Start();
                        }
                        bool allReady = ready.Wait(10000);
                        System.GC.Collect(); System.GC.WaitForPendingFinalizers(); System.GC.Collect();
                        release.Set();
                        foreach (var thread in threads) if (!thread.Join(10000)) return 6;
                        if (!allReady || errors[0] != 0 || errors[1] != 0) return 7;
                        if (addresses[0] == addresses[1] || addresses[0] == mainAddress || addresses[1] == mainAddress) return 8;
                        return {{owner}}.get_index() == 100 && {{owner}}.step() == 9 && {{owner}}.address() == mainAddress ? 0 : 9;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(emitted, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
