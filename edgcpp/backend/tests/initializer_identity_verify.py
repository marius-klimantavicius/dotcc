#!/usr/bin/env python3
"""Verify lowered initializer identity across translation-unit relocations."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shlex
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'passed': False, 'commands': [], 'checks': [], 'contracts': [
        {'file': 'upstream/src/il_def.h', 'line': 3283,
         'contract': 'trans_unit_corresp points to a front end data structure and is for front end use only'},
        {'file': 'upstream/src/trans_corresp.h', 'line': 164,
         'contract': 'EXPENSIVE_CHECKING asserts in_front_end before correspondence access'},
        {'file': 'upstream/src/fe_wrapup.c', 'line': 854,
         'contract': 'No-IL-file configuration frees FRONT_END_REGION_NUMBER before backend execution'},
        {'file': 'upstream/src/c_gen_be.c', 'contract': 'Initializer emission uses lowered constants and explicit ck_init_repeat; no compare_constants call'},
    ]}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, timeout=900)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        report['commands'].append({'stage': label, 'command': command, 'exitCode': result.returncode})
        save()
        if result.returncode:
            raise RuntimeError(f'{label} failed; see {out}')
        return result.stdout

    fixtures = [ROOT / 'backend/tests/initializer_identity.cpp', ROOT / 'backend/tests/initializer_identity_data.cpp']
    report['sources'] = [{'path': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}
                         for path in [*fixtures, ROOT / 'backend/tests/initializer_identity.h',
                                      ROOT / 'backend/tests/initializer_identity_unit.cpp']]
    run('native-build', ['g++', '-std=c++17', '-O2', '-DIDENTITY_NATIVE_MAIN', *fixtures, '-o', out / 'native'])
    expected = run('native', [out / 'native'])
    report['checks'].append('Native two-TU relocation/representation oracle')
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        generated = out / mode
        run(mode + '-translate', [sys.executable, ROOT / 'dotnet.py', *fixtures,
            '--compiler', args.compiler.resolve(), '--std', 'c++17', '--libc', 'dotcc',
            '--emit', 'managedlib', '--namespace', 'Identity', '--class-name', 'Core',
            '--split', 'source', '--output-dir', generated, *(['--instance-methods'] if instance else [])])
        text = '\n'.join(path.read_text() for path in generated.glob('Translated*.cs'))
        repeats = list(re.finditer(r'for\s*\(\s*int\s+(__edg_repeat\w*)\s*=\s*0;\s*\1\s*<\s*4;', text))
        if not any(re.search(r'identity_repeated\s*\[\s*0\s*\+\s*' + re.escape(match[1]), text)
                   for match in repeats):
            raise AssertionError('Equal same-entity rows lost repeated-initializer compression')
        manifest = json.loads((generated / 'dot-gen.json').read_text())
        assert not manifest['unboundExternals']['symbols']
        host = out / ('host-' + mode)
        host.mkdir()
        (host / 'Host.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup><ItemGroup><ProjectReference Include="../{mode}/Generated.csproj" /></ItemGroup></Project>''')
        receiver = 'owner' if instance else 'Core'
        setup = 'using var owner = new Core();' if instance else ''
        extra = 'using var other = new Core(); if (other.initializer_identity_verify() != 0) return 100;' if instance else ''
        (host / 'Program.cs').write_text(f'''using System; using Identity;
unsafe class Entry {{ static int Main() {{
{setup}
int result = {receiver}.initializer_identity_verify();
GC.Collect(2, GCCollectionMode.Forced, true, true);
if ({receiver}.initializer_identity_verify() != result) return 101;
{extra}
Console.WriteLine($"initializer identity {{result}}");
return result;
}} }}''')
        run(mode + '-build', ['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo'])
        assert run(mode + '-jit', ['dotnet', host / 'bin/Release/net10.0/Host.dll']) == expected
        report['checks'].append(mode + '/JIT: distinct globals, subobject offsets, repeated aliases, function addresses, signed zero, nested unions and compacting GC')
        if args.nativeaot:
            run(mode + '-publish', ['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            assert run(mode + '-aot', [host / 'aot/Host']) == expected
            report['checks'].append(mode + '/NativeAOT: same identity/representation checks')
    unit = ROOT / 'backend/tests/initializer_identity_unit.cpp'
    compile_db = json.loads((ROOT / 'build/compile_commands.json').read_text())
    backend = next(entry for entry in compile_db if entry['file'].endswith('/dot_gen_be.c'))
    original = shlex.split(backend['command'])
    # Match actual backend target declarations and layout; do not link the
    # front end. Unused numeric hooks abort in this address-only unit.
    flags = []
    for argument in original[1:]:
        if argument.startswith(('-D', '-I')):
            flags.append(argument)
    run('poison-build', ['g++', '-std=c++14', '-O2', *flags, unit, '-o', out / 'poison-unit'])
    run('poison-unit', [out / 'poison-unit'])
    report['checks'].append('Actual EDG relocation constants compare without reading poisoned frontend-only correspondence pointers')
    report['passed'] = True
    save()
    print(f"{len(report['checks'])} initializer identity checks passed")


if __name__ == '__main__':
    main()
