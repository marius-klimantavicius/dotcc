"""Generated thread ABI calls, TLS destruction and retained-owner shutdown."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys


def verify_libcxx_threads(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    checks = []
    # This nonthrowing fixture needs no EH tables in its C oracle. libC.a is
    # built as EDG's generic reference runtime; its default thread-exit hook is
    # replaced only in the native harness with the system's supported hook.
    oracle = out / 'oracle'
    oracle.mkdir()
    lowered = oracle / 'lowered.c'
    include = root / 'backend/runtime/libcxx/config'
    run([root / 'build/bin/cpfe', '--c++17', '--no_exceptions', '--gnu_version', '130300',
         '--instantiate', 'used', '-I', include, '-I', root / 'backend/runtime/libc/include',
         '--gen_c_file_name', lowered, '-o', oracle / 'lowered.il', tests / 'libcxx_threads.cpp'],
        env=dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc')))
    bridge = oracle / 'bridge.o'
    run(['gcc', '-std=gnu11', '-pthread', '-I', include, '-c', tests / 'libcxx_threads_oracle.c', '-o', bridge])
    run(['gcc', '-std=gnu11', '-w', '-pthread', lowered, bridge, root / 'build/lib/libC.a', '-lm', '-o', oracle / 'edg'])
    run([oracle / 'edg'], timeout=60)
    checks.append('original EDG C: nonthrowing thread fixture, native thread-exit adapter')
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if clang is None:
        raise RuntimeError('Clang C++ is required for the independent thread oracle')
    run([clang, '-std=c++17', '-pthread', '-I', include, '-I', root / 'backend/runtime/libc/include',
         tests / 'libcxx_threads.cpp', bridge, '-o', oracle / 'clang'])
    run([oracle / 'clang'], timeout=60)
    checks.append('Clang C++: synthetic thread/TLS oracle')
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        run([sys.executable, root / 'dotnet.py', tests / 'libcxx_threads.cpp', '--libc', 'dotcc',
             '-I', root / 'backend/runtime/libcxx/config', '--emit', 'managedlib', '--namespace', '',
             '--class-name', 'Core', '--split', 'function', '--output-dir', directory,
             *(['--instance-methods'] if instance else [])])
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        exports = {entry['symbol']: entry['method'] for entry in manifest['exports']}
        def call(symbol, receiver='owner'):
            return (receiver if instance else 'Core') + '.' + exports[symbol]
        host = directory / 'host'
        host.mkdir()
        (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host / 'Program.cs').write_text(f'''using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
unsafe class EntryPoint {{
  static void Check(bool condition,string message) {{if(!condition)throw new Exception(message);}}
  static int Main() {{
    {('using var owner = new Core();' if instance else '')}
    int result={call('thread_cases')}();
    Check(result==0,"translated thread cases: "+result);
    {('using (var other = new Core()) {' + f'Check({call("thread_count", "other")}()==0,"owner isolation"); Check({call("thread_cases", "other")}()==0,"second owner threads");' + '}' if instance else '')}
    Check({call('thread_count')}()==4000,"original owner counter");
    int* report=(int*)NativeMemory.Alloc(4);*report=0;
    try {{
    int* allow={call('start_retained_thread')}(report);
    Check(allow!=null,"detached retained worker started");
    using var starting=new ManualResetEventSlim();
    var shutdown=Task.Run(()=>{{starting.Set(); {('owner' if instance else 'Core')}.Shutdown();}});
    starting.Wait();
    // The worker cannot finish until the host releases this native/pinned
    // global, so shutdown must retain its storage regardless of task scheduling.
    Check(!shutdown.Wait(30),"shutdown waits detached worker and its TLS cleanup");
    GC.Collect(2,GCCollectionMode.Forced,true,true);
    Volatile.Write(ref *allow,1);
    Check(shutdown.Wait(10000),"retained cleanup completes during shutdown");
    shutdown.GetAwaiter().GetResult();
    Check(*report==3,"retained TLS/errno cleanup and thread created by global destructor");
    }} finally {{NativeMemory.Free(report);}}
    bool rejected=false;try{{{call('thread_count')}();}}catch(ObjectDisposedException){{rejected=true;}}
    Check(rejected,"owner shutdown complete");
    Console.WriteLine("translated thread ABI, TLS and retained shutdown:ok");
    return 0;
  }}
}}''')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', host / 'bin/Release/net10.0/Host.dll'], timeout=90)
        checks.append(f'{mode}/JIT: generated threads, TLS order, owner isolation and retained shutdown')
        if nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run([host / 'aot/Host'], timeout=90)
            checks.append(f'{mode}/NativeAOT: generated threads, TLS order, owner isolation and retained shutdown')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=True)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command))
        completed = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(commands)
        (output / f'command-{index:03d}.stdout').write_text(completed.stdout)
        (output / f'command-{index:03d}.stderr').write_text(completed.stderr)
        commands.append({'command': command, 'exit': completed.returncode})
        (output / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if completed.returncode:
            raise RuntimeError(f'{command}: {completed.returncode}\n{completed.stdout}\n{completed.stderr}')
        return completed
    root = Path(__file__).resolve().parents[2]
    checks = verify_libcxx_threads(root, Path(__file__).resolve().parent, output, args.nativeaot, run)
    (output / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} generated thread checks passed')
