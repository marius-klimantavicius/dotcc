"""Public macro constant/property/method APIs under C/C++, JIT and NativeAOT."""
import argparse
import json
import os
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
commands, checks = [], []


def run(command, expected=0):
    result = subprocess.run(list(map(str, command)), capture_output=True, text=True)
    index = len(commands)
    (out / f'{index:03d}.stdout').write_text(result.stdout)
    (out / f'{index:03d}.stderr').write_text(result.stderr)
    commands.append({'command': list(map(str, command)), 'exit': result.returncode})
    (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
    if (result.returncode == 0) != (expected == 0):
        raise RuntimeError(result.stdout + result.stderr)
    return result


base = [sys.executable, root / 'dotnet.py', tests / 'macro_exports.c', '--emit', 'managedlib',
        '--namespace', 'Macro.Api', '--class-name', 'Core', '--libc', 'dotcc']
if args.compiler:
    base += ['--compiler', args.compiler]
selection = 'API_(INT|U64|NEGATIVE|TEXT|ALIAS|REDEFINED|TWICE|AT|POINT|LIVE)'
for language, instance, split in [('c', False, 'none'), ('c++', True, 'source')]:
    name = 'cpp-instance' if instance else 'c-static'
    directory = out / name
    run([*base, '--language', language, '--output-dir', directory,
         '--export-macros', selection, '--macro-signatures', tests / 'macro_exports.signatures.json',
         '--split', split, *(['--instance-methods'] if instance else [])])
    manifest = json.loads((directory / 'dot-gen.json').read_text())
    macros = {row['name']: row for row in manifest['macroExports']['macros']}
    assert len(macros) == 10 and macros['API_INT']['managedKind'] == 'const'
    assert macros['API_U64']['managedKind'] == 'const' and macros['API_REDEFINED']['constantValue'] == '9'
    assert macros['API_TEXT']['managedKind'] == 'property' and macros['API_TWICE']['managedKind'] == 'method'
    assert not any(row['symbol'].startswith('dotgen_macro_') for row in manifest['exports'])
    generated = '\n'.join(path.read_text() for path in directory.glob('Translated*.cs'))
    if language == 'c++':
        assert 'catch (global::System.Exception __edg_boundary_error)' in generated
    assert 'API_TEXT(__edg_boundary_error)' not in generated
    source_index = json.loads((directory / 'dot-gen.sources.json').read_text())
    assert len([row for row in source_index['members'] if row.get('kind') == 'macro']) == 10
    host = directory / 'host'
    host.mkdir()
    (host / 'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
    receiver = 'core' if instance else 'Core'
    setup = 'using var core=new Core();' if instance else ''
    (host / 'Program.cs').write_text('''using Macro.Api;
unsafe class Program {
    const int ConstantUse = Core.API_INT;
    static int Switch(int value) { switch(value) { case Core.API_ALIAS: return 1; default: return 0; } }
    static int Main() {
        SETUP
        if (RECEIVER.macro_register_cleanup() != 0) return 53;
        RECEIVER.macro_set_live(31);
        if (RECEIVER.API_LIVE != 31) return 52;
        int effect=7;
        int twice=RECEIVER.API_TWICE(effect++);
        int* data=stackalloc int[]{11,22,33};
        var point=RECEIVER.API_POINT(4,8);
        sbyte* text=RECEIVER.API_TEXT;
        return ConstantUse==4096 && Switch(4096)==1 && Core.API_U64==ulong.MaxValue &&
            Core.API_REDEFINED==9 && Core.API_NEGATIVE==-17 && effect==8 && twice==14 &&
            RECEIVER.API_AT(data,1)==22 && point.x==4 && point.y==8 &&
            text[0]=='m' && (byte)text[6]==0xc3 && (byte)text[7]==0xa9 && text[8]==0 && text[9]=='t'
            ? 0 : 51;
    }
}'''.replace('RECEIVER', receiver).replace('SETUP', setup))
    run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
    run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
    checks.append(name + ': constants in switch/const contexts, u64, final redefinition, UTF-8 embedded NUL pointer, typed argument-once/aggregate/pointer macros, lifecycle exception boundaries and source navigation')
    if args.nativeaot:
        run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
        run([host / 'aot/Host'])
        checks.append(name + ': NativeAOT direct macro APIs')

for selector, message in [('API_EMPTY', 'empty/configuration'), ('API_TWICE', 'requires a --macro-signatures'),
                           ('API_REMOVED', 'matched no active macros'), ('[', 'invalid --export-macros')]:
    result = run([*base, '--language', 'c', '--output-dir', out / ('reject-' + str(len(checks))),
                  '--export-macros', selector], expected=1)
    assert message in result.stderr, result.stderr
    checks.append('reject: ' + message)

contextual = out / 'contextual.c'
contextual.write_text('#define API_X 7\nconst char *base=__BASE_FILE__;\n')
result = run([sys.executable, root / 'dotnet.py', contextual, '--language', 'c', '--emit', 'managedlib',
              '--output-dir', out / 'reject-include-context', '--export-macros', 'API_X',
              *(['--compiler', args.compiler] if args.compiler else [])], expected=1)
assert 'changes preprocessed source' in result.stderr, result.stderr
checks.append('reject: include-sensitive original source')
(out / 'results.json').write_text(json.dumps({'status': 'passed', 'checks': checks}, indent=2) + '\n')
print(str(len(checks)) + ' macro export checks passed')
