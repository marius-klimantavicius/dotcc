#!/usr/bin/env python3
"""Atomic loads must never write read-only shared mappings (SQLite WAL reader)."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--runtime-source', type=Path)
    args = parser.parse_args()
    tests = Path(__file__).resolve().parent
    runtime = args.runtime_source or tests.parent / 'runtime/AtomicOps.cs'
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    shutil.copyfile(runtime, out / 'AtomicOps.cs')
    shutil.copyfile(tests / 'atomic_readonly.cs', out / 'Program.cs')
    project = out / 'ReadOnlyAtomic.csproj'
    project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>''')
    commands = []
    def run(label, command):
        command = list(map(str, command))
        # Each run is an isolated process: the pre-fix hardware access violation
        # must not take down the verifier or other acceptance tests.
        result = subprocess.run(command, capture_output=True, text=True, timeout=600)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        commands.append({'label': label, 'command': command, 'exitCode': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        assert result.returncode == 0, (label, result.returncode, result.stdout[-3000:], result.stderr[-3000:])
    run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
    run('jit', ['dotnet', out / 'bin/Release/net10.0/ReadOnlyAtomic.dll', out / 'jit-data.bin'])
    if args.nativeaot:
        run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
            '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true', '-o', out / 'aot'])
        run('aot', [out / 'aot/ReadOnlyAtomic', out / 'aot-data.bin'])
    (out / 'results.json').write_text(json.dumps({'status': 'passed', 'checks': [
        'all legal atomic load orders on OS-enforced read-only mappings',
        'typed and raw loads: 1/2/4/8/16 byte integer and float/double values; arbitrary 3/7/24 byte objects',
        'zero and nonzero contents at aligned and unaligned addresses; backing file unchanged'],
        'nativeaot': args.nativeaot,
        'inputs': {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in [runtime, tests / 'atomic_readonly.cs', Path(__file__)]}}, indent=2) + '\n')
    print('PASS: read-only mapped atomic loads')


if __name__ == '__main__':
    main()
