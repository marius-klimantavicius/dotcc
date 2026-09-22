#!/usr/bin/env python3
"""Emit a resumable frozen core object set, then link the planned managed library."""
import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import time
from core_inputs import compiler_identity, profile_sources, emission_identity, semantic_selection

ROOT = Path(__file__).resolve().parents[1]
LINK_OPTIONS = ['--emit=managedlib', '--literal-pool', '--deduplicate-inline', '--nest-types', '--class-name', 'BlinkCore',
                '--namespace', 'Managed.Emulation', '--runtime=c']
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--profile', type=Path, required=True)
parser.add_argument('--jobs', type=int, choices=range(1, 5), default=2)
parser.add_argument('--timeout', type=float, default=180)
parser.add_argument('--emit-only', action='store_true')
parser.add_argument('--sources', nargs='+', help='selected filenames for bounded diagnosis; implies --emit-only')
args = parser.parse_args()
profile = args.profile.resolve()
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
inputs_path = profile / 'inputs.json'
inputs = json.loads(inputs_path.read_text())
if not (profile / 'closure.json').is_file():
    raise SystemExit('requires a profile with its own frozen closure.json; stage a new profile')
for name, digest in inputs['staged_headers'].items():
    if sha(profile / name) != digest:
        raise SystemExit('profile checksum mismatch: ' + name)
compiler_dir = ROOT.parent / 'DotCC/bin/Release/net10.0'
compiler = compiler_identity(compiler_dir)
if sha(profile / 'compiler-identity.py') != sha(ROOT / 'scripts/core_inputs.py'):
    raise SystemExit('compiler identity helper differs from frozen profile; stage a new profile')
if compiler != inputs['compiler']:
    raise SystemExit('compiler differs from frozen profile; stage a new profile before assembly')
# Every upstream include remains pinned; unchanged source files are never edited.
inventory = json.loads((ROOT / 'config/source-inventory.json').read_text())
source_manifest = json.loads((ROOT / 'config/source-manifest.json').read_text())
upstream = ROOT / 'ref' / source_manifest['upstream']['directory']
for row in inventory['files']:
    if sha(upstream / row['path']) != row['sha256']:
        raise SystemExit('upstream checksum mismatch: ' + row['path'])
isolator = ROOT / 'scripts/isolate-core.py'
identity = dict(profile_inputs_sha256=sha(inputs_path), compiler_sha256=compiler,
                isolation_script_sha256=sha(isolator), closure_sha256=sha(profile / 'closure.json'),
                compiler_identity_script_sha256=sha(ROOT / 'scripts/core_inputs.py'),
                source_inventory_sha256=sha(ROOT / 'config/source-inventory.json'),
                link_options=LINK_OPTIONS)
key = hashlib.sha256(json.dumps(identity, sort_keys=True).encode()).hexdigest()
cache = ROOT / 'artifacts/core/objects' / key
cache.mkdir(parents=True, exist_ok=True)
entries = profile_sources(profile, ROOT, inputs)
for entry in entries:
    if sha(ROOT / entry['staged_path']) != entry['sha256']:
        raise SystemExit('source checksum mismatch: ' + entry['path'])
if args.sources:
    unknown = set(args.sources) - {Path(entry['path']).name for entry in entries}
    if unknown:
        raise SystemExit('unknown source filename: ' + ', '.join(sorted(unknown)))
    entries = [entry for entry in entries if Path(entry['path']).name in args.sources]
selected = {entry['path']:entry for entry in entries}
expected_emission = {entry['path']:emission_identity(profile, ROOT, inputs, entry) for entry in entries}
objects = {}
for receipt_path in sorted((ROOT / 'artifacts/core').glob('isolate-*/result.json')):
    try:
        receipt = json.loads(receipt_path.read_text())
        for row in receipt['rows']:
            expected = selected.get(row['source'])
            path = Path(row.get('object_path', '/missing'))
            if expected and row.get('emission_identity') == expected_emission[row['source']] and row['exit_code'] == 0 and row['source_sha256'] == expected['sha256'] and path.is_file() and sha(path) == row.get('object_sha256'):
                objects[row['source']] = dict(row, receipt=str(receipt_path), producing_profile=receipt['profile'],
                                               producing_profile_inputs_sha256=receipt['profile_inputs_sha256'])
    except (OSError, ValueError, KeyError):
        continue  # incomplete active diagnostic receipts are not reusable
report = dict(kind='frozen-core-object-set-not-managed-execution', identity=identity,
              profile=str(profile), key=key, selected=list(selected), reused_objects=len(objects),
              assembly_script_sha256=sha(Path(__file__)), objects=objects, failures={}, linked=False)
def save():
    temporary = cache / 'receipt.tmp'
    temporary.write_text(json.dumps(report, indent=2) + '\n')
    temporary.replace(cache / 'receipt.json')
def emit(entry):
    name = Path(entry['path']).name
    command = [sys.executable, str(isolator), '--profile', str(profile), '--start', name,
               '--only', '--timeout', str(args.timeout)]
    started = time.monotonic()
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                            text=True, timeout=args.timeout + 30)
    (cache / (name + '.dispatch.log')).write_text(result.stdout)
    row = dict(source=entry['path'], command=command, exit_code=result.returncode, seconds=time.monotonic()-started)
    if result.returncode == 0:
        summary = json.loads(result.stdout.splitlines()[-1])
        receipt_path = Path(summary['log']).parent / 'result.json'
        receipt = json.loads(receipt_path.read_text())
        if any(receipt.get(k) != v for k, v in identity.items()
               if k not in ('source_inventory_sha256', 'link_options')):
            raise RuntimeError('emission identity changed: ' + name)
        emitted = receipt['rows'][0]
        if emitted.get('emission_identity') != expected_emission[entry['path']] or emitted['source_sha256'] != entry['sha256'] or sha(Path(emitted['object_path'])) != emitted['object_sha256']:
            raise RuntimeError('object checksum mismatch: ' + name)
        row = dict(emitted, receipt=str(receipt_path), producing_profile=receipt['profile'],
                   producing_profile_inputs_sha256=receipt['profile_inputs_sha256'])
    return row
save()
with ThreadPoolExecutor(max_workers=args.jobs) as pool:
    pending = {pool.submit(emit, entry):entry for entry in entries if entry['path'] not in objects}
    for future in as_completed(pending):
        entry = pending[future]
        try:
            row = future.result()
        except Exception as error:
            row = dict(source=entry['path'], exit_code=125, error=str(error))
        target = report['objects'] if row['exit_code'] == 0 else report['failures']
        target[entry['path']] = row
        save()
        print(json.dumps({k:row[k] for k in ('source', 'exit_code')}), flush=True)
if report['failures']:
    raise SystemExit('object emission incomplete; see ' + str(cache / 'receipt.json'))
if (profile / 'semantic-intrinsics.json').exists():
    coverage = {}
    for name, row in report['objects'].items():
        observed = row.get('semantic_intrinsics')
        if not observed or semantic_selection(profile, Path(observed['report']), name) != observed:
            raise SystemExit('Semantic selection evidence changed or missing: ' + name)
        coverage[name] = observed
    spec = json.loads((profile / 'semantic-intrinsics.json').read_text())
    if not (args.emit_only or args.sources) and not set(spec['required_units']).issubset(coverage):
        raise SystemExit('Required semantic core producers missing')
    report['semantic_intrinsics'] = dict(specification_sha256=sha(profile / 'semantic-intrinsics.json'),
        selected_units=sum(not row['absent'] for row in coverage.values()),
        absent_units=sum(row['absent'] for row in coverage.values()), coverage=coverage)
    save()
if compiler_identity(compiler_dir) != compiler:
    raise SystemExit('compiler changed before link; cached objects retain their old identity')
if args.emit_only or args.sources:
    print('verified object set: ' + str(cache / 'receipt.json'))
    raise SystemExit(0)
output = ROOT / 'generated/core-objects' / key / 'ManagedCore'
output.parent.mkdir(parents=True, exist_ok=True)
command = ['dotnet', str(compiler_dir / 'dotcc.dll'),
           *[report['objects'][entry['path']]['object_path'] for entry in entries],
           *LINK_OPTIONS, '-o', str(output)]
with (cache / 'link.log').open('wb') as stream:
    result = subprocess.run(command, stdout=stream, stderr=subprocess.STDOUT, timeout=180)
report['link'] = dict(command=command, exit_code=result.returncode, output=str(output))
report['linked'] = result.returncode == 0
if compiler_identity(compiler_dir) != compiler:
    report['linked'] = False
    report['link']['invalidated'] = 'compiler changed during link'
    save()
    raise SystemExit('compiler changed during link; retry with a frozen profile')
save()
if result.returncode:
    raise SystemExit('object link failed; see ' + str(cache / 'link.log'))
report['generated'] = {str(path.relative_to(output)):sha(path) for path in output.rglob('*') if path.is_file()}
save()
print('linked managed source; build/execution still require qualification: ' + str(cache / 'receipt.json'))
