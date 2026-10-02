"""Generated BCL atomic-wait imports with static/instance storage and compacting GC."""
import argparse
import json
from pathlib import Path
import subprocess
import sys


def verify_libcxx_atomic_wait(root, tests, out, nativeaot, run):
    checks = []
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        generated = out / mode
        run([sys.executable, root / 'dotnet.py', tests / 'libcxx_atomic_wait.cpp',
             '--libc', 'dotcc', '--emit', 'managedlib', '--namespace', '', '--class-name', 'Core',
             '--output-dir', generated, '--split', 'function',
             *(['--instance-methods'] if instance else [])])
        manifest = json.loads((generated / 'dot-gen.json').read_text())
        exports = {e['symbol']: e['method'] for e in manifest['exports']}
        has_shutdown = any('void Shutdown()' in p.read_text() for p in generated.glob('Translated*.cs'))
        def call(symbol, receiver='first'):
            return (receiver if instance else 'Core') + '.' + exports[symbol]
        host = generated / 'host'; host.mkdir()
        (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host / 'Program.cs').write_text(f'''using System;
using System.Threading;
using System.Runtime.InteropServices;
unsafe class EntryPoint {{
 static int Main() {{
  {((('using ' if has_shutdown else '')+'var first=new Core(); '+('using ' if has_shutdown else '')+'var second=new Core();') if instance else '')}
  long* native=(long*)NativeMemory.AllocZeroed(8);
  try {{
   for(int selection=0; selection<2; ++selection) {{
    long* p=selection==0?native:{call('wait_word')}();
    int completed=0;
    var worker=new Thread(()=>{{for(long value=1;value<=128;++value) {{
     {call('wait_value')}(p,value-1);
     if(Interlocked.Read(ref *p)!=value)Environment.FailFast("wait value");
     Interlocked.Increment(ref completed);
    }}}});
    worker.IsBackground=true; worker.Start();
    for(long value=1;value<=128;++value) {{
     if((value&31)==0)GC.Collect(2,GCCollectionMode.Forced,true,true);
     {call('publish_value','second')}(p,value);
     if(!SpinWait.SpinUntil(()=>Volatile.Read(ref completed)==value,10000))return 1;
    }}
    if(!worker.Join(10000))return 2;
   }}
   {('if(*'+call('wait_word','second')+'()!=0)return 3;' if instance else '')}
  }} finally {{ NativeMemory.Free(native); }}
  {('Core.Shutdown();' if has_shutdown and not instance else '')}
  return 0;
 }}
}}''')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo'])
        run(['dotnet', host / 'bin/Release/net10.0/Host.dll'], timeout=90)
        checks.append(mode + '/JIT: native and pinned global storage, GC, 256 wait handshakes')
        if nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run([host / 'aot/Host'], timeout=90)
            checks.append(mode + '/NativeAOT: native and pinned global storage, GC, 256 wait handshakes')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args(); out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command)); p = subprocess.run(command, capture_output=True, text=True, **kwargs)
        i = len(commands); (out / f'{i:03d}.stdout').write_text(p.stdout); (out / f'{i:03d}.stderr').write_text(p.stderr)
        commands.append({'command': command, 'exit': p.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if p.returncode: raise RuntimeError(f'{command}: {p.returncode}\n{p.stdout}\n{p.stderr}')
        return p
    checks = verify_libcxx_atomic_wait(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent, out, args.nativeaot, run)
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(len(checks), 'generated atomic wait checks passed')
