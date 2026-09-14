#!/usr/bin/env python3
"""Compile staged upstream core TUs separately to isolate the first failure."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile
import time

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--profile', type=Path, required=True, help='immutable generated/core-profile/attempt-* directory')
parser.add_argument('--timeout', type=float, default=20)
parser.add_argument('--start', default='', help='first source filename, e.g. machine.c')
parser.add_argument('--only', action='store_true', help='stop after the first selected source, even if it emits')
args = parser.parse_args()
profile = args.profile.resolve()
profile_inputs = json.loads((profile / 'inputs.json').read_text())
for name, expected in profile_inputs['staged_headers'].items():
    if hashlib.sha256((profile / name).read_bytes()).hexdigest() != expected:
        raise SystemExit('staged profile checksum mismatch: ' + name)
out = Path(tempfile.mkdtemp(prefix='isolate-', dir=root / 'artifacts/core'))
closure = json.loads((root / 'artifacts/core/closure.json').read_text())
if (profile / 'managed-additions.json').exists():
    for entry in json.loads((profile / 'managed-additions.json').read_text())['sources']:
        closure['sources'].append(dict(path=entry['path'], sha256=entry['sha256'],
                                      staged_path=str(profile / 'additional' / Path(entry['path']).name)))
command = ['dotnet', str(root.parent / 'DotCC/bin/Release/net10.0/dotcc.dll'),
           '-std=c17', '-D_GNU_SOURCE', '-DNDEBUG', '-DNOLINEAR',
           '-I', str(profile), '-I', str(root / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580')]
if (profile / 'host').is_dir():
    command += ['-I', str(profile / 'host')]
command += ['--overrides-file', str(profile / 'overrides.json'), '--emit=obj']
compiler = root.parent / 'DotCC/bin/Release/net10.0'
compiler_files = list(compiler.glob('DotCC*.dll')) + [compiler / 'dotcc.dll']
receipt = {'profile': str(profile), 'profile_inputs_sha256': hashlib.sha256((profile / 'inputs.json').read_bytes()).hexdigest(),
           'compiler_sha256': {path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in compiler_files},
           'timeout_seconds': args.timeout, 'rows': []}
active = not args.start
for entry in closure['sources']:
    source = root / entry['staged_path']
    if source.name == args.start:
        active = True
    if not active:
        continue
    if hashlib.sha256(source.read_bytes()).hexdigest() != entry['sha256']:
        raise SystemExit('staged source checksum mismatch: ' + str(source))
    log = out / (source.stem + '.log')
    invocation = command + [str(source), '-o', str(out / (source.stem + '.cs')),
                            '--override-report', str(out / (source.stem + '.overrides.jsonl'))]
    started = time.monotonic()
    with log.open('wb') as stream:
        try:
            result = subprocess.run(invocation, stdout=stream, stderr=subprocess.STDOUT, timeout=args.timeout)
            code = result.returncode
            kind = 'emitted object' if code == 0 else 'compiler diagnostic'
        except subprocess.TimeoutExpired:
            code = 124
            kind = 'per-TU timeout; not an architectural blocker'
    row = {'source': entry['path'], 'source_sha256': entry['sha256'], 'exit_code': code,
           'classification': kind, 'seconds': time.monotonic() - started, 'log': str(log),
           'command': invocation}
    if receipt['compiler_sha256'] != {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
                                      for path in compiler_files}:
        code = 125
        row['exit_code'] = code
        row['classification'] = 'compiler changed during invocation; retry required'
    receipt['rows'].append(row)
    (out / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps({key: row[key] for key in ('source', 'exit_code', 'classification', 'seconds', 'log')}), flush=True)
    if code:
        raise SystemExit(code)
    if args.only:
        break
if not receipt['rows']:
    raise SystemExit('no source matched --start')
