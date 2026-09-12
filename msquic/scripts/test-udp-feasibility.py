#!/usr/bin/env python3
"""Build/run the standalone BCL UDP feasibility cases under JIT and Linux AOT."""
import hashlib
import json
from pathlib import Path
import subprocess

root = Path(__file__).resolve().parents[1]
project = root / 'tests/UdpFeasibility/UdpFeasibility.csproj'
artifacts = root / 'artifacts'
artifacts.mkdir(parents=True, exist_ok=True)
output = root / 'build/udp-feasibility-aot'
records = []
manifest = artifacts / 'udp-feasibility-run.json'
manifest.unlink(missing_ok=True)
for runtime in ['jit', 'aot']:
    (artifacts / f'udp-feasibility-{runtime}.json').unlink(missing_ok=True)
commands = [
    ('build', ['dotnet', 'build', str(project), '-c', 'Release', '--nologo']),
    ('jit', ['dotnet', str(project.parent / 'bin/Release/net10.0/UdpFeasibility.dll'),
             str(artifacts / 'udp-feasibility-jit.json')]),
    ('aot-build', ['dotnet', 'publish', str(project), '-c', 'Release', '-r', 'linux-x64',
                   '-p:PublishAot=true', '-o', str(output), '--nologo']),
    ('aot', [str(output / 'UdpFeasibility'), str(artifacts / 'udp-feasibility-aot.json')]),
]
for label, command in commands:
    with (artifacts / f'udp-feasibility-{label}.log').open('w') as log:
        result = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=240)
    records.append({'stage': label, 'command': command, 'exit_code': result.returncode})
    print(label, result.returncode, flush=True)
    if result.returncode:
        raise SystemExit(f'{label} failed; see artifacts/udp-feasibility-{label}.log')
receipts = {name: json.loads((artifacts / f'udp-feasibility-{name}.json').read_text()) for name in ['jit', 'aot']}
if any(not receipt['Passed'] for receipt in receipts.values()):
    raise SystemExit('At least one UDP feasibility case failed')
if receipts['aot']['Runtime'] != 'nativeaot':
    raise SystemExit('Published consumer did not identify NativeAOT runtime')
source_hashes = {str(path.relative_to(root)): hashlib.sha256(path.read_bytes()).hexdigest()
                 for path in [project, project.parent / 'Program.cs']}
manifest.write_text(json.dumps({'commands': records, 'source_sha256': source_hashes,
    'case_counts': {name: len(receipt['Cases']) for name, receipt in receipts.items()},
    'passed': True, 'limits': receipts['aot']['Limits']}, indent=2) + '\n')
