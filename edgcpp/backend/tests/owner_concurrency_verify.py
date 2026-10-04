#!/usr/bin/env python3
"""Concurrent generated heap/lifecycle/libc TLS regression for managed callers."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    tests = Path(__file__).resolve().parent
    root = tests.parents[1]
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    commands = []

    def run(label, command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, timeout=600)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        commands.append({'label': label, 'command': command, 'exitCode': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        assert result.returncode == 0, (label, result.stdout[-6000:], result.stderr[-6000:])
        return result

    generated = out / 'generated'
    run('translate', [sys.executable, root / 'dotnet.py', tests / 'owner_concurrency.c',
        '--compiler', args.compiler.resolve(), '--language', 'c', '--std', 'c17', '--libc', 'dotcc',
        '--namespace', '', '--class-name', 'ConcurrencyCore', '--instance-methods',
        '--emit', 'managedlib', '--output-dir', generated])
    host = out / 'host'
    host.mkdir()
    shutil.copyfile(tests / 'owner_concurrency.cs', host / 'Program.cs')
    (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../generated/Generated.csproj" /></ItemGroup></Project>''')
    run('build', ['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
    run('jit', ['dotnet', host / 'bin/Release/net10.0/Host.dll'])
    checks = ['strict JIT: eight callers, 64,000 allocation/reallocation/calloc cycles across same and independent owners',
              'thread/owner errno separation and fresh errno after ReleaseThread; pinned errno survives compacting GC',
              'Shutdown waits for an active translated callback and rejects subsequent calls']
    if args.nativeaot:
        run('publish', ['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
            '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true', '-o', out / 'aot'])
        run('aot', [out / 'aot/Host'])
        checks.append('same checks on Linux x64 NativeAOT')
    (out / 'results.json').write_text(json.dumps({'status': 'passed', 'checks': checks,
        'compiler': {'path': str(args.compiler.resolve()), 'sha256': hashlib.sha256(args.compiler.read_bytes()).hexdigest()},
        'inputs': {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in [tests / 'owner_concurrency.c', tests / 'owner_concurrency.cs', Path(__file__)]},
        'limitations': ['Bounded runtime concurrency check; SQLite mutex/VFS behavior tested separately.',
                        'No benchmark or exhaustive race proof; execution host is Linux x64.']}, indent=2) + '\n')
    print('PASS: generated owner concurrency (JIT' + (' and NativeAOT)' if args.nativeaot else ')'))


if __name__ == '__main__':
    main()
