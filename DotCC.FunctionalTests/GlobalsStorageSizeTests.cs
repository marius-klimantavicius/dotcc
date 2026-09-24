using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class GlobalsStorageSizeTests
{
    [Theory]
    [InlineData(false, "static")]
    [InlineData(true, "static")]
    [InlineData(false, "context")]
    [InlineData(true, "context")]
    [InlineData(false, "instance")]
    [InlineData(true, "instance")]
    public void Large_globals_and_initialized_tls_keep_addresses_and_owner_isolation(bool objects, string mode)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-large-globals-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "large.c");
            File.WriteAllText(path, """
                struct Big { unsigned char bytes[70000]; int marker; };
                struct Big global = {.marker = 7};
                _Thread_local struct Big local = {.marker = 11};
                struct Small { int value; };
                _Thread_local struct Small positional = {17};
                struct Small shared = {19};
                long global_address(void) { return (long)&global; }
                long local_address(void) { return (long)&local; }
                int initial_global(void) { return global.marker == 7 && global.bytes[0] == 0 && global.bytes[69999] == 0; }
                int initial_local(void) { return positional.value == 17 && local.marker == 11 && local.bytes[0] == 0 && local.bytes[69999] == 0; }
                void set_global(int v) { global.marker = v; global.bytes[0] = v; global.bytes[69999] = v + 1; }
                void set_local(int v) { positional.value = v; shared.value = v; local.marker = v; local.bytes[0] = v; local.bytes[69999] = v + 1; }
                int global_ok(int v) { return global.marker == v && global.bytes[0] == (unsigned char)v && global.bytes[69999] == (unsigned char)(v + 1) && global.bytes[35000] == 0; }
                int shared_value(void) { return shared.value; }
                int local_ok(int v) { return positional.value == v && local.marker == v && local.bytes[0] == (unsigned char)v && local.bytes[69999] == (unsigned char)(v + 1) && local.bytes[35000] == 0; }
                """);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C,
                InstanceMethods: mode == "instance", StateContext: mode == "context");
            string emitted;
            if (objects)
            {
                string fragment = path + ".o.cs";
                File.WriteAllText(fragment, Compiler.EmitObject(path,
                    outputOptions: mode == "instance" ? new(InstanceMethods: true) : null));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.ManagedLib,
                    className: "LargeApi", outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                className: "LargeApi", outputOptions: options);
            string owner = mode == "instance" ? "api" : "LargeApi";
            string create = mode == "instance" ? "new LargeApi()" : "LargeApi.__DotCcCreateContext()";
            string enter = mode == "instance" ? "__DotCcEnter" : "Enter";
            string setup = mode == "static" ? "" : $"using var api = {create}; using var binding = api.{enter}();";
            string otherOwner = mode == "instance" ? "other" : "LargeApi";
            string isolation = mode == "static" ? "" : $$"""
                using (var other = {{create}})
                using (other.{{enter}}()) {
                    if ({{otherOwner}}.initial_global() != 1 || {{otherOwner}}.initial_local() != 1) return 10;
                    if ({{otherOwner}}.global_address() == globalAddress || {{otherOwner}}.local_address() == localAddress) return 11;
                    {{otherOwner}}.set_global(70); {{otherOwner}}.set_local(80);
                    Collect();
                    if ({{otherOwner}}.global_ok(70) != 1 || {{otherOwner}}.local_ok(80) != 1) return 12;
                }
                bool rejected = false;
                try { api.Dispose(); } catch (System.InvalidOperationException) { rejected = true; }
                if (!rejected) return 13;
                """;
            string workerBinding = mode == "static" ? "" : $"using var workerBinding = api.{enter}();";
            emitted += $$"""

                public static class LargeStorageProbe {
                    private static void Collect() {
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                        System.GC.WaitForPendingFinalizers();
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                    }
                    public static int Main() {
                        {{setup}}
                        if ({{owner}}.initial_global() != 1 || {{owner}}.initial_local() != 1) return 1;
                        long globalAddress = {{owner}}.global_address(), localAddress = {{owner}}.local_address();
                        {{owner}}.set_global(30); {{owner}}.set_local(40);
                        {{isolation}}
                        Collect();
                        if ({{owner}}.global_ok(30) != 1 || {{owner}}.local_ok(40) != 1 ||
                            {{owner}}.global_address() != globalAddress || {{owner}}.local_address() != localAddress) return 2;
                        int workerError = 0;
                        var worker = new System.Threading.Thread(() => {
                            try {
                                {{workerBinding}}
                                if ({{owner}}.initial_local() != 1) { workerError = 31; return; }
                                if ({{owner}}.shared_value() != 40) { workerError = 35; return; }
                                if ({{owner}}.local_address() == localAddress) { workerError = 32; return; }
                                if ({{owner}}.global_address() != globalAddress) { workerError = 33; return; }
                                if ({{owner}}.global_ok(30) != 1) { workerError = 34; return; }
                                long address = {{owner}}.local_address();
                                {{owner}}.set_local(50);
                                Collect();
                                if ({{owner}}.local_ok(50) != 1 || {{owner}}.local_address() != address) workerError = 4;
                            } catch { workerError = 5; }
                        }) { IsBackground = true };
                        worker.Start();
                        if (!worker.Join(10000)) return 6;
                        if (workerError != 0) return workerError;
                        return {{owner}}.global_ok(30) == 1 && {{owner}}.local_ok(40) == 1 &&
                            {{owner}}.global_address() == globalAddress && {{owner}}.local_address() == localAddress ? 0 : 7;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(emitted, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
