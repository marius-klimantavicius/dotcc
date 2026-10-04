"""Direct managed API spellings for public, static and synthesized EDG wrappers."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-dir', required=True, type=Path)
parser.add_argument('--compiler', type=Path)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
tests = Path(__file__).resolve().parent
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
compiler = args.compiler.resolve() if args.compiler else root / 'build/bin/cpfe-dot'
commands, checks = [], []


def run(command):
    result = subprocess.run(list(map(str, command)), capture_output=True, text=True)
    index = len(commands)
    (out / f'{index:03d}.stdout').write_text(result.stdout)
    (out / f'{index:03d}.stderr').write_text(result.stderr)
    commands.append({'command': list(map(str, command)), 'exit': result.returncode})
    (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
    if result.returncode:
        raise RuntimeError(result.stdout + result.stderr)
    return result


for instance, split in [(False, 'none'), (True, 'source')]:
    name = 'instance-source' if instance else 'static-unsplit'
    directory = out / name
    run([sys.executable, root / 'dotnet.py', tests / 'export_names_internal.c', tests / 'export_names_public.c',
         '--compiler', compiler, '--language', 'c', '--emit', 'managedlib', '--namespace', 'Original.Api',
         '--class-name', 'Core', '--split', split, '--output-dir', directory,
         *(['--instance-methods'] if instance else [])])
    manifest = json.loads((directory / 'dot-gen.json').read_text())
    methods = {row['method'] for row in manifest['exports']}
    for expected in ['shared_api', 'public_api', 'public_api__body', 'call_internal',
                     'local_helper', 'local_helper__body', '@event']:
        assert expected in methods, (name, expected, sorted(methods))
    assert not any(method.startswith('__edg_export_') for method in methods), methods
    source = '\n'.join(path.read_text() for path in directory.glob('Translated*.cs'))
    assert 'local_helper__body_2(' in source and 'public_api__body_2(' in source
    locations = json.loads((directory / 'dot-gen.sources.json').read_text())['members']
    assert any(row['managedName'] == 'local_helper' for row in locations)
    host = directory / 'host'
    host.mkdir()
    (host / 'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
    receiver, setup = ('core', 'using var core=new Core();') if instance else ('Core', '')
    (host / 'Program.cs').write_text('''using Original.Api;
class Program {
    static int Main() {
        SETUP
        return RECEIVER.shared_api(1)==201 && RECEIVER.public_api(1)==12 &&
            RECEIVER.public_api__body(1)==14 && RECEIVER.call_internal(1)==119 &&
            RECEIVER.local_helper(1)==4 && RECEIVER.local_helper__body(1)==6 &&
            RECEIVER.@event(1)==8 ? 0 : 61;
    }
}'''.replace('SETUP', setup).replace('RECEIVER', receiver))
    run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
    run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
    checks.append(name + ': JIT direct original APIs, external over earlier static TU, keyword escaping, body-name collisions and source navigation')
    if args.nativeaot:
        run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
        run([host / 'aot/Host'])
        checks.append(name + ': NativeAOT direct original APIs')

cpp = out / 'cpp'
run([sys.executable, root / 'dotnet.py', tests / 'export_names.cpp', '--compiler', compiler,
     '--emit', 'managedlib', '--namespace', 'Cpp.Api', '--class-name', 'Core', '--output-dir', cpp])
manifest = json.loads((cpp / 'dot-gen.json').read_text())
assert any(row['method'] == 'cpp_public_api' for row in manifest['exports'])
assert not any(row['method'].startswith('__edg_export_') for row in manifest['exports']), manifest['exports']
assert any('NamedOwner__ctor' in row['method'] for row in manifest['exports']), manifest['exports']
assert any('NamedOwner__destroy' in row['method'] for row in manifest['exports']), manifest['exports']
host = cpp / 'host'
host.mkdir()
(host / 'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
(host / 'Program.cs').write_text('using Cpp.Api; class Program { static int Main() => Core.cpp_public_api(5)==32 ? 0 : 62; }')
run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
checks.append('C++: JIT original extern-C API, namespace overload allocation and named synthesized owner constructors/destructors')
if args.nativeaot:
    run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
         '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
    run([host / 'aot/Host'])
    checks.append('C++: NativeAOT extern-C API and synthesized members')

# Ordinary program output has no public wrapper reservation and retains bodies.
program = out / 'program.c'
program.write_text('static int original_helper(int value) { return value + 3; }\nint main(void) { return original_helper(4)==7 ? 0 : 63; }\n')
run([sys.executable, root / 'dotnet.py', program, '--compiler', compiler, '--language', 'c', '--output-dir', out / 'program'])
program_source = (out / 'program/Translated.cs').read_text()
assert 'int original_helper(int value)' in program_source and 'original_helper__body' not in program_source
run(['dotnet', 'build', out / 'program/Generated.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
run(['dotnet', out / 'program/bin/Release/net10.0/Generated.dll'])
checks.append('program mode: original implementation name remains unchanged; JIT behavior')
inputs = [root / 'backend/dot_gen_source_names.h', tests / 'export_names_verify.py',
          tests / 'export_names_internal.c', tests / 'export_names_public.c', tests / 'export_names.cpp', compiler]
(out / 'results.json').write_text(json.dumps({'status': 'passed', 'checks': checks,
    'inputs': {str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in inputs}}, indent=2) + '\n')
print(str(len(checks)) + ' export naming checks passed')
