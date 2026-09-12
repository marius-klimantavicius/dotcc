#!/usr/bin/env python3
"""Save GCC and dotcc results for the independent reduced blockers."""
import argparse
import json
from pathlib import Path
import subprocess

root = Path(__file__).resolve().parents[1]
repo = root.parent
out = root / 'artifacts/reproducers'
out.mkdir(parents=True, exist_ok=True)
results = []
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--build', action='store_true', help='Also compile emitted C# and compare two native runs')
args = parser.parse_args()
for source in sorted((root / 'probes').glob('*.c')):
    record = {'source': str(source.relative_to(root))}
    commands = {
        'gcc': ['gcc', '-std=gnu17', '-fms-extensions', '-fsyntax-only', str(source)],
        'dotcc': ['dotnet', str(repo / 'DotCC/bin/Release/net10.0/dotcc.dll'), str(source),
                  '--emit=obj', '-o', str(out / (source.stem + '.cs'))],
    }
    for name, command in commands.items():
        result = subprocess.run(command, capture_output=True, text=True, timeout=30)
        record[name] = {'command': command, 'exit_code': result.returncode,
                        'diagnostics': result.stdout + result.stderr}
    results.append(record)
    print(source.name, 'gcc:', record['gcc']['exit_code'], 'dotcc:', record['dotcc']['exit_code'], flush=True)
    if source.stem in ['nested-variadic', 'include-nonstandard-extension']:
        for name, command in [('gcc', ['gcc', '-E', '-P', str(source)]),
                              ('dotcc', ['dotnet', str(repo / 'DotCC/bin/Release/net10.0/dotcc.dll'), '-E', str(source)])]:
            result = subprocess.run(command, capture_output=True, text=True, timeout=30)
            (out / (source.stem + '.' + name + '.i')).write_text(result.stdout)
            (out / (source.stem + '.' + name + '.log')).write_text(result.stderr)
(out / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
if args.build:
    records = []
    for name in ['member-alignment', 'nested-anonymous-bitfields', 'gnu-intrinsics',
                 'include-nonstandard-extension', 'high-bit-conversions', 'callback-aggregate']:
        source = root / 'probes' / (name + '.c')
        destination = root / 'generated/reproducers' / name
        executable = name in ['member-alignment', 'nested-anonymous-bitfields']
        record = {'probe': name, 'commands': []}
        commands = []
        if executable:
            commands.append(['gcc', '-std=gnu17', '-fms-extensions', str(source), '-o', str(out / name)])
        commands.append(['dotnet', str(repo / 'DotCC/bin/Release/net10.0/dotcc.dll'), str(source),
                         '--emit=csproj' if executable else '--emit=managedlib', '-o', str(destination)])
        for command in commands:
            result = subprocess.run(command, capture_output=True, text=True, timeout=30)
            record['commands'].append({'command': command, 'exit_code': result.returncode,
                                       'output': result.stdout + result.stderr})
            if result.returncode:
                raise SystemExit(f'Unexpected probe setup failure: {command}')
        if executable:
            result = subprocess.run([str(out / name)], capture_output=True, text=True, timeout=30)
            record['native_run'] = {'exit_code': result.returncode, 'stdout': result.stdout}
        project = next(destination.glob('*.csproj'))
        result = subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo'],
                                capture_output=True, text=True, timeout=90)
        (out / (name + '.build.log')).write_text(result.stdout + result.stderr)
        record['build_exit_code'] = result.returncode
        if result.returncode == 0 and executable:
            result = subprocess.run(['dotnet', 'run', '--no-build', '--project', str(project), '-c', 'Release'],
                                    capture_output=True, text=True, timeout=30)
            record['managed_run'] = {'exit_code': result.returncode, 'stdout': result.stdout, 'stderr': result.stderr}
        records.append(record)
        print(json.dumps(record), flush=True)
    (out / 'build-results.json').write_text(json.dumps(records, indent=2) + '\n')
