"""Storage acceptance tests, called by verify.py with its subprocess checker."""


def verify_storage(root, tests, out, nativeaot, run):
    import sys
    checks = []
    run(["g++", "-std=c++17", "-O0", tests / "pointers.cpp", "-o", out / "native-pointers"])
    run([out / "native-pointers"])
    run(["g++", "-std=c++17", "-O0", tests / "pointer_owners.cpp", "-o", out / "native-pointer-owners"])
    run([out / "native-pointer-owners"], expected=11)
    for fixture, expected in (("aggregates", 0), ("aggregate_owners", 11)):
        run(["g++", "-std=c++17", "-O0", tests / f"{fixture}.cpp", "-o", out / f"native-{fixture}"])
        run([out / f"native-{fixture}"], expected=expected)
    for mode in ("static", "instance"):
        options = ["--instance-methods"] if mode == "instance" else []
        for fixture in ("pointers", "pointer_owners", "aggregates", "aggregate_owners"):
            directory = out / f"{fixture}-{mode}"
            run([sys.executable, root / "dotnet.py", tests / f"{fixture}.cpp", "--output-dir", directory,
                 "--namespace", "Storage.Tests", "--class-name", "Core", "--split", "size",
                 "--split-size", "1200", *options])
            if fixture.endswith("owners"):
                owners = '''
        var a = new Storage.Tests.Core();
        var b = new Storage.Tests.Core();
        if (a.Run() != 11 || a.Run() != 12 || b.Run() != 11) return 201;
        Collect();
        if (a.Run() != 13 || b.Run() != 12) return 202;
        for (int i = 0; i < 8; i++) {
            if (a.Run() != 14 + i || b.Run() != 13 + i) return 203;
            // No caller-held owner variable during this entire translated call.
            if (new Storage.Tests.Core().Run() != 11) return 204;
        }
        global::System.GC.KeepAlive(a);
        global::System.GC.KeepAlive(b);
''' if mode == "instance" else '''
        if (Storage.Tests.Core.Run() != 11) return 201;
        Collect();
        if (Storage.Tests.Core.Run() != 12) return 202;
        for (int i = 0; i < 8; i++) if (Storage.Tests.Core.Run() != 13 + i) return 203;
'''
                (directory / "EntryPoint.cs").write_text('''internal static class EntryPoint {
    private static void Collect() {
        for (int i = 0; i < 4; ++i) {
            var pressure = new byte[131072];
            pressure[0] = 1;
            global::System.GC.Collect(2, global::System.GCCollectionMode.Forced, true, true);
            global::System.GC.WaitForPendingFinalizers();
            global::System.GC.KeepAlive(pressure);
        }
    }
    public static int Main() {
        using var stop = new global::System.Threading.ManualResetEventSlim(false);
        using var started = new global::System.Threading.ManualResetEventSlim(false);
        var collector = new global::System.Threading.Thread(() => {
            Collect();
            started.Set();
            while (!stop.IsSet) {
                Collect();
                global::System.Threading.Thread.Sleep(1);
            }
        });
        collector.Start();
        try {
            if (!started.Wait(10000)) return 200;
''' + owners + '''
            return 0;
        } finally {
            stop.Set();
            collector.Join();
        }
    }
}
''')
            project = directory / "Generated.csproj"
            run(["dotnet", "build", project, "-c", "Release", "--nologo"])
            run(["dotnet", directory / "bin/Release/net10.0/Generated.dll"])
            checks.append(f"{fixture}/{mode}: managed aliasing, alignment, sequencing or GC/owner stress")
            if nativeaot:
                run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                     "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-o", directory / "aot"])
                run([directory / "aot/Generated"])
                checks.append(f"{fixture}/{mode}: linux-x64 NativeAOT execution")
    return checks
