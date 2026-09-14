#!/usr/bin/env python3
"""Diagnostic link of verified same-C-input objects from explicit compiler generations."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess

from core_inputs import compiler_identity, emission_identity, profile_sources

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--profile', type=Path, required=True)
parser.add_argument('--baseline-assembly', type=Path, required=True)
parser.add_argument('--replacement-assembly', type=Path, action='append', default=[])
args = parser.parse_args()
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
profile = args.profile.resolve()
inputs = json.loads((profile / 'inputs.json').read_text())
for name, digest in inputs['staged_headers'].items():
    if sha(profile / name) != digest:
        raise SystemExit('frozen profile changed: ' + name)
entries = profile_sources(profile, ROOT, inputs)
selected = {entry['path']:entry for entry in entries}
objects = {}
receipts = {}
for receipt_path in [args.baseline_assembly, *args.replacement_assembly]:
    receipt_path = receipt_path.resolve()
    report = json.loads(receipt_path.read_text())
    receipts[str(receipt_path)] = sha(receipt_path)
    for name, row in report['objects'].items():
        if name not in selected:
            raise SystemExit('replacement contains an unselected source: ' + name)
        entry = selected[name]
        if sha(ROOT / entry['staged_path']) != entry['sha256'] or row['source_sha256'] != entry['sha256']:
            raise SystemExit('C source differs: ' + name)
        expected = emission_identity(profile, ROOT, inputs, entry)
        actual = dict(row['emission_identity'])
        # This deliberately allows only the recorded compiler generation to
        # differ. Headers, source, options, canonical path contract and helpers
        # must still match; this is never a canonical emission-cache receipt.
        expected.pop('compiler_sha256')
        producer = actual.pop('compiler_sha256')
        if actual != expected:
            raise SystemExit('noncompiler emission input differs: ' + name)
        if row['exit_code'] or sha(Path(row['object_path'])) != row['object_sha256']:
            raise SystemExit('invalid object: ' + name)
        objects[name] = dict(row, replay_assembly_receipt=str(receipt_path), producer_compiler=producer)
if set(objects) != set(selected):
    raise SystemExit('incomplete selected object set')
compiler_dir = ROOT.parent / 'DotCC/bin/Release/net10.0'
compiler = compiler_identity(compiler_dir)
options = ['--emit=managedlib', '--nest-types', '--class-name', 'BlinkCore',
           '--namespace', 'Managed.Emulation', '--runtime=c']
identity = dict(profile_inputs_sha256=sha(profile / 'inputs.json'), linker_compiler=compiler,
                object_sha256={name:row['object_sha256'] for name,row in objects.items()},
                source_receipts=receipts, link_options=options, replay_script_sha256=sha(Path(__file__)))
key = hashlib.sha256(json.dumps(identity, sort_keys=True).encode()).hexdigest()
out = ROOT / 'artifacts/core/objects' / key
out.mkdir(parents=True, exist_ok=True)
generated = ROOT / 'generated/core-objects' / key / 'ManagedCore'
generated.parent.mkdir(parents=True, exist_ok=True)
command = ['dotnet', str(compiler_dir / 'dotcc.dll'),
           *[objects[entry['path']]['object_path'] for entry in entries], *options, '-o', str(generated)]
with (out / 'link.log').open('wb') as log:
    code = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=180).returncode
report = dict(kind='diagnostic-mixed-compiler-object-replay-not-canonical-validation',
              diagnostic_replay=True, identity=identity, key=key, profile=str(profile),
              selected=list(selected), objects=objects, failures={}, linked=code == 0,
              link=dict(command=command, exit_code=code),
              generated={str(path.relative_to(generated)):sha(path) for path in generated.rglob('*') if path.is_file()})
if compiler_identity(compiler_dir) != compiler:
    report['linked'] = False
    report['link']['invalidated'] = 'linker compiler changed during invocation'
(out / 'receipt.json').write_text(json.dumps(report, indent=2) + '\n')
print(out / 'receipt.json')
raise SystemExit(0 if report['linked'] else 1)
