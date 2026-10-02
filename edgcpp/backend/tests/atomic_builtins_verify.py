"""GNU atomics lowered by EDG: C oracle, translated calls and concurrent owners."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys


def verify_atomic_builtins(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    checks = []
    clang = shutil.which('clang') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang'))), None)
    if clang is None:
        raise RuntimeError('Clang is required for the atomic C oracle')
    env = dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc'))
    for language in ('c', 'c++'):
        label = 'cxx' if language == 'c++' else 'c'
        source = out / ('atomic-source.cpp' if language == 'c++' else 'atomic-source.c')
        source.write_bytes((tests / 'atomic_builtins.c').read_bytes())
        lowered = out / f'atomic-{label}-oracle.c'
        run([root / 'build/bin/cpfe', '--c++17' if language == 'c++' else '--c11',
             '--gnu_version', '130300', '-DATOMIC_ORACLE_MAIN', '--gen_c_file_name', lowered,
             '-o', out / f'atomic-{label}-oracle.il', source], env=env)
        for compiler_label, compiler in (('gcc', 'gcc'), ('clang', clang)):
            executable = out / f'atomic-{label}-{compiler_label}'
            if compiler_label == 'gcc':
                run([compiler, '-std=gnu11', '-w', lowered, root / 'build/lib/libC.a',
                     '-lm', '-latomic', '-o', executable])
            else:
                # EDG C emits GCC's size-suffixed builtins, which Clang does
                # not recognize. Use the original synthetic input for this
                # independent oracle; do not rewrite EDG's generated C.
                run([compiler, '-std=c++17' if language == 'c++' else '-std=gnu11',
                     '-w', '-DATOMIC_ORACLE_MAIN', source, '-latomic', '-o', executable])
            run([executable])
            checks.append(f'{language}: ' + ('original EDG C + GCC' if compiler_label == 'gcc' else 'Clang source') + ' atomic oracle')
        for instance in (False, True):
            mode = 'instance' if instance else 'static'
            directory = out / f'{label}-{mode}'
            run([sys.executable, root / 'dotnet.py', source, '--language', language,
                 '--emit', 'managedlib', '--namespace', '', '--class-name', 'Core',
                 '--split', 'function', '--output-dir', directory,
                 *(['--instance-methods'] if instance else [])])
            manifest = json.loads((directory / 'dot-gen.json').read_text())
            exports = {entry['symbol']: entry['method'] for entry in manifest['exports']}
            def call(symbol, owner='owner'):
                return (owner if instance else 'Core') + '.' + exports[symbol]
            host = directory / 'host'
            host.mkdir()
            (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
            (host / 'Program.cs').write_text(f'''using System;
using System.Threading.Tasks;
unsafe class EntryPoint {{
  static void Require(bool condition,string message) {{if(!condition)throw new Exception(message);}}
  static int Main() {{
    {('using var owner = new Core();' if instance else '')}
    Require({call('atomic_cases')}()==0,"translated atomic cases");
    Parallel.For(0,4,worker=>{{for(int i=0;i<6000;++i){call('atomic_increment')}();}});
    Require({call('atomic_counter')}()==24000,"translated concurrent counter");
    {('using var other = new Core();' if instance else '')}
    {(f'Require({call("atomic_counter","other")}()==0,"owner counter isolation");' if instance else '')}
    {(f'{call("atomic_increment","other")}(); Require({call("atomic_counter","other")}()==1 && {call("atomic_counter")}()==24000,"independent owners");' if instance else '')}
    Console.WriteLine("translated atomic cases and concurrent storage:ok");
    return 0;
  }}
}}''')
            run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
            run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
            checks.append(f'{language}/{mode}: translated JIT scalar/generic atomics and concurrent storage')
            if nativeaot:
                run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                     '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
                run([host / 'aot/Host'])
                checks.append(f'{language}/{mode}: translated NativeAOT scalar/generic atomics and concurrent storage')
    return checks


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--nativeaot", action="store_true")
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=True)
    commands = []

    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, text=True, capture_output=True, **kwargs)
        index = len(commands)
        (output / f"command-{index:03d}.stdout").write_text(result.stdout)
        (output / f"command-{index:03d}.stderr").write_text(result.stderr)
        commands.append({"command": command, "exit": result.returncode})
        (output / "commands.json").write_text(json.dumps(commands, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(f"{command}: {result.returncode}\n{result.stdout}\n{result.stderr}")
        return result

    root = Path(__file__).resolve().parents[2]
    checks = verify_atomic_builtins(root, Path(__file__).resolve().parent,
                                     output, args.nativeaot, run)
    (output / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} atomic builtin checks passed")
