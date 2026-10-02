"""Pinned libc++ contention algorithm with cross-owner native atomics."""
import argparse
import json
from pathlib import Path
import subprocess
import sys


def verify_libcxx_atomic_protocol(root, tests, out, nativeaot, run, acquisition):
    acquisition = acquisition.resolve()
    info = json.loads((acquisition / 'acquisition.json').read_text())
    headers = acquisition / info['source']['archiveRoot'] / 'include'
    checks = []
    for instance in (False, True):
        mode = 'instance' if instance else 'static'; generated = out / mode
        # Explicit one-TU component acceptance; independent-TU identity/linking
        # remains a separate gate for the libc++ provider.
        combined = out / (mode + '-component.cpp')
        combined.write_text(''.join('#include ' + json.dumps(str(p)) + '\n' for p in
                            [root / 'backend/runtime/libcxx/abi_atomic.cpp', tests / 'libcxx/atomic_protocol.cpp']))
        run([sys.executable, root / 'dotnet.py', combined, '--std', 'c++20', '--libc', 'dotcc', '--emit', 'managedlib',
             '--namespace', '', '--class-name', 'Core', '--split', 'function', '--output-dir', generated,
             '-I', acquisition / 'config', '-I', headers, *(['--instance-methods'] if instance else [])])
        exports = {e['symbol']: e['method'] for e in json.loads((generated / 'dot-gen.json').read_text())['exports']}
        def call(symbol, receiver='first'):
            return (receiver if instance else 'Core') + '.' + exports[symbol]
        host = generated / 'host'; host.mkdir()
        (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        has_shutdown = any('void Shutdown()' in p.read_text() for p in generated.glob('Translated*.cs'))
        (host / 'Program.cs').write_text(f'''using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
unsafe class EntryPoint {{
 const int Workers=12, Rounds=128;
 static int Main() {{
  {((('using ' if has_shutdown else '')+'var first=new Core(); '+('using ' if has_shutdown else '')+'var second=new Core();') if instance else '')}
  long* native=(long*)NativeMemory.AllocZeroed(65536);
  try {{
   var buckets=new List<nint>[256];
   var collisions=new List<nint>();
   for(int i=0;i<8192;++i) {{
    int bucket=(int){call('atomic_bucket')}(native+i);
    buckets[bucket]??=new(); buckets[bucket].Add((nint)(native+i));
    if(buckets[bucket].Count==Workers) {{collisions=buckets[bucket];break;}}
   }}
   if(collisions.Count!=Workers)return 1;
   // Generic waits launder through a shared sequence; direct waits monitor the
   // actual 64-bit word. Both must synchronize across generated owner objects.
   for(int variant=0;variant<3;++variant) {{
    int direct=variant==0?0:1;
    var locations=new nint[Workers];
    for(int i=0;i<Workers;++i) {{locations[i]=variant==2?(nint)native:collisions[i];*(long*)locations[i]=0;}}
    int completed=0;
    var threads=new Thread[Workers];
    using var ready=new CountdownEvent(Workers);
    for(int worker=0;worker<Workers;++worker) {{
     nint location=locations[worker];
     threads[worker]=new Thread(()=>{{
      ready.Signal();
      for(long value=1;value<=Rounds;++value) {{
       while(Interlocked.Read(ref *(long*)location)==value-1) {{
        long monitor={call('atomic_monitor')}((void*)location,direct);
        if(Interlocked.Read(ref *(long*)location)!=value-1)break;
        {call('atomic_wait')}((void*)location,monitor,direct);
       }}
       if(Interlocked.Read(ref *(long*)location)!=value)Environment.FailFast("atomic protocol value");
       Interlocked.Increment(ref completed);
      }}
     }});
     threads[worker].IsBackground=true;threads[worker].Start();
    }}
    ready.Wait();
    for(long value=1;value<=Rounds;++value) {{
     if((value&31)==0)GC.Collect(2,GCCollectionMode.Forced,true,true);
     for(int i=0;i<(variant==2?1:Workers);++i) {{
      Interlocked.Exchange(ref *(long*)locations[i],value);
      {call('atomic_notify','second')}((void*)locations[i],direct,variant==2?1:(int)(value&1));
     }}
     if(!SpinWait.SpinUntil(()=>Volatile.Read(ref completed)==value*Workers,10000))return 2;
    }}
    foreach(Thread thread in threads)if(!thread.Join(10000))return 3;
   }}
  }} finally {{NativeMemory.Free(native);}}
  {('Core.Shutdown();' if has_shutdown and not instance else '')}
  return 0;
 }}
}}''')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo'])
        run(['dotnet', host / 'bin/Release/net10.0/Host.dll'], timeout=90)
        checks.append(mode + '/JIT: single-TU libc++ direct/generic protocol, 4608 handshakes, collisions and GC')
        if nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run([host / 'aot/Host'], timeout=90)
            checks.append(mode + '/NativeAOT: single-TU libc++ direct/generic protocol, 4608 handshakes, collisions and GC')
    return checks


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--acquisition', type=Path, required=True)
    p.add_argument('--output-dir', type=Path, required=True)
    p.add_argument('--nativeaot', action='store_true')
    args = p.parse_args(); out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command)); result = subprocess.run(command, text=True, capture_output=True, **kwargs)
        i = len(commands); (out / f'{i:03d}.stdout').write_text(result.stdout); (out / f'{i:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode: raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    checks = verify_libcxx_atomic_protocol(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent,
                                          out, args.nativeaot, run, args.acquisition)
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(len(checks), 'libc++ atomic protocol checks passed')
