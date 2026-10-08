#!/usr/bin/env python3
"""Verify explicit startup, reentry, concurrency and cached failure with JIT/AOT."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
HOST = r'''using System;
using System.Threading;
using StartupSuite;
internal static unsafe class StartupHost
{
    internal static int Calls, Enters, Leaves, Exits;
    private static readonly ManualResetEventSlim Entered = new(), Release = new();
    private static Exception? Error;
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    internal static void Observe(int phase)
    {
        Interlocked.Increment(ref Calls);
        if (phase != 1) return;
        Check(Core.read_startup_trace() == 1, "first constructor storage");
        Edg.Runtime.ObjectProgram.InitializeLibrary(); // Same-thread recursion returns.
        int seen = -1;
        var reader = new Thread(() => seen = Core.read_startup_trace()) { IsBackground = true };
        reader.Start();
        Check(reader.Join(5000), "constructor worker blocked on a CLR initializer lock");
        Check(seen == 1, "constructor worker export");
        Entered.Set();
        Check(Release.Wait(10000), "host never released constructor");
    }
    private static void Initialize()
    {
        try { Edg.Runtime.ObjectProgram.InitializeLibrary(); }
        catch (Exception error) { Interlocked.CompareExchange(ref Error, error, null); }
    }
    private static int Entry() => Core.main();
    public static int Main()
    {
        try
        {
            Check(Core.read_startup_trace() == 0, "export implicitly ran source constructors");
            var first = new Thread(Initialize) { IsBackground = true };
            first.Start(); Check(Entered.Wait(5000), "constructor never entered");
            bool complete = false;
            var waiter = new Thread(() => { Initialize(); Volatile.Write(ref complete, true); }) { IsBackground = true };
            waiter.Start();
            Check(SpinWait.SpinUntil(() => (waiter.ThreadState & ThreadState.WaitSleepJoin) != 0, 5000), "concurrent initialization did not wait");
            Check(!Volatile.Read(ref complete), "concurrent host observed partial startup");
            Release.Set();
            Check(first.Join(5000) && waiter.Join(5000), "initialization did not complete");
            if (Error is not null) throw Error;
            Check(Calls == 3 && Core.read_startup_trace() == 123, "constructor order/count");
            Check(Enters == 0 && Leaves == 0 && Exits == 0, "library startup impersonated process entry");
            Edg.Runtime.ObjectProgram.InitializeLibrary();
            Check(Edg.Runtime.ObjectProgram.Run(&Entry) == 0, "executable entry result");
            Check(Calls == 3 && Enters == 1 && Leaves == 1 && Exits == 1, "executable lifecycle/order/count");
            Console.WriteLine("startup concurrency, private actions, reentry, worker export and explicit lifecycle OK");
            return 0;
        }
        catch (Exception error) { Release.Set(); Console.Error.WriteLine(error); return 1; }
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void host_startup_step(int argument0) => StartupHost.Observe(argument0);
    }
    public static unsafe partial class ObjectProgram
    {
        static partial void Enter() { ++StartupHost.Enters; }
        static partial void Leave() { ++StartupHost.Leaves; }
        static partial void NormalExit(int status) { if (status != 0) throw new Exception("exit status"); ++StartupHost.Exits; }
    }
}
'''
FAILURE = r'''using System;
using System.Threading;
using StartupSuite;
internal static unsafe class StartupHost
{
    internal static readonly Exception Marker = new InvalidOperationException("startup marker");
    internal static int Calls, Enters, Leaves;
    private static int MainCalls, WrongErrors;
    internal static void Fail() { Interlocked.Increment(ref Calls); throw Marker; }
    private static void Initialize()
    {
        try { Edg.Runtime.ObjectProgram.InitializeLibrary(); Interlocked.Increment(ref WrongErrors); }
        catch (Exception error) { if (!ReferenceEquals(error, Marker)) Interlocked.Increment(ref WrongErrors); }
    }
    private static int Entry() { ++MainCalls; return Core.main(); }
    public static int Main()
    {
        if (Core.read_startup_attempts() != 0) return 1;
        Initialize(); Initialize();
        var workers = new Thread[8];
        for (int i = 0; i < workers.Length; ++i) { workers[i] = new Thread(Initialize) { IsBackground = true }; workers[i].Start(); }
        foreach (var worker in workers) if (!worker.Join(5000)) return 2;
        if (WrongErrors != 0 || Calls != 1 || Core.read_startup_attempts() != 1 || Enters != 0 || Leaves != 0) return 3;
        try { Edg.Runtime.ObjectProgram.Run(&Entry); return 4; }
        catch (Exception error) { if (!ReferenceEquals(error, Marker)) return 5; }
        if (MainCalls != 0 || Calls != 1 || Enters != 1 || Leaves != 1) return 6;
        Console.WriteLine("startup failure cached once, same exception rethrown, executable main skipped and leave executed OK");
        return 0;
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void host_startup_failure() => StartupHost.Fail();
    }
    public static unsafe partial class ObjectProgram
    {
        static partial void Enter() { ++StartupHost.Enters; }
        static partial void Leave() { ++StartupHost.Leaves; }
    }
}
'''


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'checks': [], 'commands': [], 'nativeAotVerified': False}
    for name, file in [('compiler', args.compiler), ('linker', args.linker)]:
        report[name + 'Sha256'] = hashlib.sha256(file.read_bytes()).hexdigest()

    def check(label, condition):
        if not condition:
            raise AssertionError(label)
        report['checks'].append(label)

    def run(label, command, timeout=300):
        command = list(map(str, command))
        with (out / (label + '.log')).open('wb') as log:
            result = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=timeout)
        report['commands'].append({'label': label, 'command': command, 'exitCode': result.returncode})
        check(label, result.returncode == 0)

    try:
        for name, harness in [('startup_host_hooks', HOST), ('startup_host_failure', FAILURE)]:
            source = ROOT / 'tests' / (name + '.c')
            obj = out / (name + '.dotobj')
            generated = out / name
            run(name + '-compile', [sys.executable, '-B', ROOT / 'compile.py', source, '--compiler', args.compiler.resolve(),
                                   '--std', 'c11', '--output', obj, '--work-dir', out / (name + '-work')])
            run(name + '-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--allow-externs',
                                '--namespace', 'StartupSuite', '--class', 'Core', obj])
            source_map = json.loads((generated / 'source-map.json').read_text())
            count = 3 if name == 'startup_host_hooks' else 1
            check(name + '-source-action-provenance', len(source_map['objects'][0]['initializationActions']) == count)
            (generated / 'StartupHost.cs').write_text(harness)
            project = generated / 'Translated.csproj'
            project.write_text(project.read_text().replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType>\n    <StartupObject>StartupHost</StartupObject>'))
            run(name + '-build', ['dotnet', 'build', generated, '-v:q'])
            run(name + '-jit', ['dotnet', generated / 'bin/Debug/net10.0/Translated.dll'], timeout=40)
            if args.nativeaot:
                run(name + '-aot-build', ['dotnet', 'publish', generated, '-r', args.rid, '-c', 'Release',
                                        '-p:PublishAot=true', '-o', generated / 'aot', '-v:q'])
                run(name + '-aot', [generated / 'aot' / ('Translated.exe' if args.rid.startswith('win-') else 'Translated')], timeout=40)
        report['nativeAotVerified'] = args.nativeaot
        report['status'] = 'passed'
    except Exception as error:
        report['status'] = 'failed'
        report['error'] = str(error)
        raise
    finally:
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps({'status': report['status'], 'checks': len(report['checks']), 'output': str(out)}))


if __name__ == '__main__':
    main()
