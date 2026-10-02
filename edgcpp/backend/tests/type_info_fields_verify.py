"""Compiler type_info names and real user-created type_info objects coexist."""
import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import sys


def verify_type_info_fields(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    source = tests / 'type_info_fields.cpp'
    lowered = out / 'lowered.c'
    run([root / 'build/bin/cpfe-dot-oracle', '--c++17', '--gnu_version', '130300', '-DNATIVE_ORACLE',
         '--gen_c_file_name', lowered, '-o', out / 'lowered.il', source],
        env=dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc')))
    run(['gcc', '-std=gnu11', '-w', lowered, root / 'build/lib/libC.a', '-o', out / 'oracle'])
    run([out / 'oracle'])
    checks = ['EDG C: compiler RTTI and constructed type_info-derived object fields/destruction']
    for no_rtti in (False, True):
      for instance in (False, True):
        mode = ('no-rtti-' if no_rtti else '') + ('instance' if instance else 'static')
        directory = out / mode
        if not no_rtti:
            run([sys.executable, root / 'dotnet.py', source, '--emit', 'managedlib', '--namespace', '',
                 '--class-name', 'Core', '--split', 'function', '--output-dir', directory,
                 *(['--instance-methods'] if instance else [])])
            manifest = json.loads((directory / 'dot-gen.json').read_text())
            method = next(e['method'] for e in manifest['exports'] if e['symbol'] == 'type_info_fields')
        else:
            # Driver deliberately has no arbitrary frontend-option passthrough.
            # Exercise EDG's documented no-RTTI mode directly to ensure the
            # helper has no dependency on a nonexistent compiler token table.
            directory.mkdir()
            env = dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc'),
                       EDG_DOT_NAMESPACE='', EDG_DOT_CLASS='@Core', EDG_DOT_INSTANCE='1' if instance else '0',
                       EDG_DOT_EMIT='managedlib', EDG_DOT_NEST='1', EDG_DOT_RUNTIME='none',
                       EDG_DOT_LIBC='none', EDG_DOT_TARGET='portable-lp64-binary80-v2')
            result = run([root / 'build/bin/cpfe-dot', '--c++17', '--gnu_version', '130300', '--no_rtti', '--no_exceptions',
                          '--clear_flag', 'use_predefined_macro_file', '-DNO_COMPILER_RTTI', source], env=env)
            (directory / 'Translated.cs').write_text(result.stdout)
            assert '__edg_type_token_base => null;' in result.stdout, 'zero-token path was not exercised'
            method = next(m[2] for m in re.finditer(r'^// dot-gen:export ([0-9a-f]+) (\w+)$', result.stdout, re.M)
                          if bytes.fromhex(m[1]).decode() == 'type_info_fields')
            (directory / 'Generated.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
<ItemGroup><Compile Include="Translated.cs" /></ItemGroup></Project>''')
        host = directory / 'host'; host.mkdir()
        has_shutdown = any('void Shutdown()' in p.read_text() for p in directory.glob('Translated*.cs'))
        (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host / 'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  static void Collect({('Core owner' if instance else '')}) {{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
  static int Main() {{
    {((('using ' if has_shutdown else '')+'var first=new Core();'+('using ' if has_shutdown else '')+'var second=new Core();') if instance else '')}
    for(int i=0;i<3;++i) {{
      int result={('first' if instance else 'Core')}.{method}(&Collect);if(result!=0)return result;
      {('result=second.'+method+'(&Collect);if(result!=0)return result;' if instance else '')}
    }}
    {('Core.Shutdown();' if has_shutdown and not instance else '')}
    return 0;
  }}
}}''')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo'])
        run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
        checks.append(mode + '/JIT: inline type_info fields, constructed object, cleanup and GC')
        if nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run([host / 'aot/Host'])
            checks.append(mode + '/NativeAOT: inline type_info fields, constructed object, cleanup and GC')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    output = args.output_dir.resolve(); output.mkdir(parents=True, exist_ok=True)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command)); result = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(commands)
        (output / f'command-{index:03d}.stdout').write_text(result.stdout)
        (output / f'command-{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (output / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode: raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    checks = verify_type_info_fields(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent,
                                     output, args.nativeaot, run)
    (output / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} type_info field checks passed')
