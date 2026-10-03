"""Synthetic two-owner libc++ source-provider acceptance, explicitly single TU."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys


def verify_libcxx_core_owners(root, tests, out, acquisition, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    adapter = root / 'backend/runtime/libcxx'
    profile = json.loads((adapter / 'core-source-profile.json').read_text())
    acquired = json.loads((acquisition / 'acquisition.json').read_text())
    upstream = acquisition / acquired['source']['archiveRoot']
    sources = []
    for entry in profile['orderedSources']:
        path = (adapter if entry['base'] == 'adapter' else upstream) / entry['path']
        assert hashlib.sha256(path.read_bytes()).hexdigest() == entry['sha256'], path
        sources.append(path)
    fixture = tests / 'libcxx/core_owners.cpp'
    run(['g++', '-std=c++20', '-DNATIVE_LIBRARY_CORE', fixture, '-o', out / 'native'])
    run([out / 'native'])
    checks = ['Native C++ synthetic source oracle: container, allocation, handler and TLS behavior']
    combined = out / 'combined.cpp'
    combined.write_text('// Explicit single-TU source-provider component acceptance.\n' + ''.join(
        '#include ' + json.dumps(str(path.resolve())) + '\n' for path in [*sources, fixture]))
    generated = out / 'generated'
    run([sys.executable, root / 'dotnet.py', combined, '--libc', 'musl', '--std', 'c++20',
         '-I', acquisition / 'config', '-I', upstream / 'include', '--instance-methods',
         '--emit', 'managedlib', '--namespace', '', '--class-name', 'Core', '--split', 'size',
         '--output-dir', generated])
    manifest = json.loads((generated / 'dot-gen.json').read_text())
    exports = {entry['symbol']: entry['method'] for entry in manifest['exports']}
    owner = exports['core_owner_step']; tls = exports['core_thread_step']
    host = generated / 'host'; host.mkdir()
    (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
    (host / 'Program.cs').write_text(f'''using System;
using System.Threading;
class EntryPoint {{
 static int Main() {{
  using var first = new Core(); using var second = new Core();
  for(int i=1;i<=3;++i) {{
   int result=first.{owner}(1,i); if(result!=0)return result;
   result=second.{owner}(2,i); if(result!=0)return result+20;
   result=first.{tls}(1,i); if(result!=0)return result+40;
   result=second.{tls}(2,i); if(result!=0)return result+60;
   GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
  }}
  int threadResult=0;
  var thread=new Thread(()=>{{try {{ for(int i=1;i<=2;++i) {{
   int result=first.{tls}(1,i);if(result!=0){{threadResult=result+80;return;}}
   result=second.{tls}(2,i);if(result!=0){{threadResult=result+100;return;}}
  }} }} finally {{ first.ReleaseThread();second.ReleaseThread(); }} }});
  thread.Start();thread.Join();if(threadResult!=0)return threadResult;
  first.Dispose();
  for(int i=4;i<=5;++i) {{int result=second.{owner}(2,i);if(result!=0)return result+120;}}
  return second.{tls}(2,4);
 }}
}}''')
    run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo'])
    run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
    checks.append('Instance JIT: independent libc++ globals/new_handler/TLS, forced GC and surviving owner after disposal')
    if nativeaot:
        run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
        run([host / 'aot/Host'])
        checks.append('Instance NativeAOT: independent libc++ globals/new_handler/TLS, forced GC and surviving owner after disposal')
    (out / 'profile.json').write_text(json.dumps(profile, indent=2) + '\n')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args(); out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command)); result = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(commands); (out / f'command-{index:03d}.stdout').write_text(result.stdout)
        (out / f'command-{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode: raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    checks = verify_libcxx_core_owners(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent,
                                      out, args.acquisition.resolve(), args.nativeaot, run)
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} libc++ source-provider owner checks passed')
