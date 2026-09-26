#!/usr/bin/env python3
"""Run direct translated-picotls QUIC message probes, raw/optimized x JIT/AOT."""
import hashlib
import json
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parents[1]
picotls = root.parent / 'picotls'
project = root / 'tests/QuicTlsFeasibility/QuicTlsFeasibility.csproj'
artifacts = root / 'artifacts'
artifacts.mkdir(parents=True, exist_ok=True)
manifest = artifacts / 'tls-feasibility-run.json'
manifest.unlink(missing_ok=True)
records, receipts = [], {}
for variant in ['raw', 'optimized']:
    product = picotls / 'generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls') / 'TranslatedPicotls.csproj'
    if not product.exists():
        raise SystemExit(f'Generate picotls first: missing {product}')
    build = root / 'build/tls-feasibility' / variant
    properties = [f'-p:PicotlsProject={product}', '--artifacts-path', str(build)]
    for runtime in ['jit', 'aot']:
        (artifacts / f'tls-feasibility-{variant}-{runtime}.json').unlink(missing_ok=True)
    commands = [
        ('build', ['dotnet', 'build', str(project), '-c', 'Release', '--nologo', *properties]),
        ('jit', ['dotnet', str(build / 'bin/QuicTlsFeasibility/release/QuicTlsFeasibility.dll'),
                 str(artifacts / f'tls-feasibility-{variant}-jit.json')]),
        ('aot-build', ['dotnet', 'publish', str(project), '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                      '-o', str(build / 'publish'), '--nologo', *properties]),
        ('aot', [str(build / 'publish/QuicTlsFeasibility'), str(artifacts / f'tls-feasibility-{variant}-aot.json')]),
    ]
    for label, command in commands:
        logfile = artifacts / f'tls-feasibility-{variant}-{label}.log'
        with logfile.open('w') as log:
            result = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=300)
        records.append({'variant': variant, 'stage': label, 'command': command, 'exit_code': result.returncode})
        print(variant, label, result.returncode, flush=True)
        if result.returncode:
            raise SystemExit(f'{variant} {label} failed; see {logfile}')
        if re.search(r'\bwarning IL\d+', logfile.read_text()):
            raise SystemExit(f'AOT/trim warning in {logfile}')
    for runtime in ['jit', 'aot']:
        receipt = json.loads((artifacts / f'tls-feasibility-{variant}-{runtime}.json').read_text())
        if not receipt['Passed'] or (runtime == 'aot' and receipt['Runtime'] != 'nativeaot'):
            raise SystemExit('Invalid TLS feasibility receipt')
        receipts[variant + '-' + runtime] = receipt
sources = list(project.parent.glob('*.cs')) + [project]
sources += list((picotls / 'src/BclProvider').glob('*.cs'))
for directory in ['TranslatedPicotls', 'TranslatedPicotls.Raw']:
    sources += list((picotls / 'generated' / directory).glob('*.cs'))
source_hashes = {str(path.relative_to(root.parent)): hashlib.sha256(path.read_bytes()).hexdigest() for path in sources}
manifest.write_text(json.dumps({'commands': records, 'source_sha256': source_hashes,
    'case_counts': {name: len(receipt['Cases']) for name, receipt in receipts.items()},
    'passed': True, 'limits': next(iter(receipts.values()))['Limits']}, indent=2) + '\n')
