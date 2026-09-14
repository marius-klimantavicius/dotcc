#!/usr/bin/env python3
"""Freeze reviewed C# consumer changes while proving every C emission input unchanged."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import tempfile

from core_inputs import emission_identity, profile_sources

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--profile', type=Path, required=True)
parser.add_argument('--add-managed-source', type=Path, action='append', default=[])
parser.add_argument('--refresh-host-project', action='store_true')
args = parser.parse_args()
previous = args.profile.resolve()
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
inputs = json.loads((previous / 'inputs.json').read_text())
for name, digest in inputs['staged_headers'].items():
    if sha(previous / name) != digest:
        raise SystemExit('previous snapshot changed: ' + name)
old_sources = profile_sources(previous, ROOT, inputs)
expected = {row['path']:emission_identity(previous, ROOT, inputs, row) for row in old_sources}
stage = Path(tempfile.mkdtemp(prefix='attempt-', dir=ROOT / 'generated/core-profile'))
shutil.copytree(previous, stage, dirs_exist_ok=True)
bindings = json.loads((stage / 'binding-sources.json').read_text())
manifest = json.loads((ROOT / 'config/host-bindings.json').read_text())
available = {Path(name).name:ROOT / name for name in manifest['managedSources']}
replacements = {}
for relative in bindings['authored_managed']:
    original = available.get(Path(relative).name)
    if original is None:
        raise SystemExit('no reviewed source mapping: ' + relative)
    replacements[relative] = original
for path in args.add_managed_source:
    original = path.resolve()
    original.relative_to(ROOT)  # Additions must be explicit campaign-owned files.
    relative = 'managed/' + original.name
    if relative in replacements and replacements[relative] != original:
        raise SystemExit('managed source basename collision: ' + relative)
    replacements[relative] = original
    if relative not in bindings['authored_managed']:
        bindings['authored_managed'].append(relative)
changes = {}
for relative, original in replacements.items():
    before = sha(stage / relative) if (stage / relative).exists() else None
    shutil.copyfile(original, stage / relative)
    digest = sha(stage / relative)
    if digest != sha(original):
        raise SystemExit('source changed during snapshot: ' + str(original))
    changes[relative] = dict(source=str(original.relative_to(ROOT)), previous_sha256=before, sha256=digest)
if args.refresh_host_project:
    shutil.rmtree(stage / 'host-project')
    shutil.copytree(ROOT / 'src/Managed.Emulation.Host', stage / 'host-project',
                    ignore=shutil.ignore_patterns('bin', 'obj'))
    for path in (stage / 'host-project').rglob('*'):
        if path.is_file() and sha(path) != sha(ROOT / 'src/Managed.Emulation.Host' / path.relative_to(stage / 'host-project')):
            raise SystemExit('Host source changed during snapshot: ' + str(path))
(stage / 'binding-sources.json').write_text(json.dumps(bindings, indent=2) + '\n')
receipt = dict(kind='consumer-only frozen overlay; no C/header changes',
               previous_profile=str(previous), previous_inputs_sha256=sha(previous / 'inputs.json'),
               managed_sources=changes, refreshed_host_project=args.refresh_host_project,
               script_sha256=sha(Path(__file__)))
(stage / 'consumer-overlay.json').write_text(json.dumps(receipt, indent=2) + '\n')
for override in inputs.get('source_overrides', {}).values():
    old_path = ROOT / override['staged_path']
    override['staged_path'] = str((stage / old_path.relative_to(previous)).relative_to(ROOT))
inputs['staged_headers'] = {str(path.relative_to(stage)):sha(path) for path in stage.rglob('*')
                            if path.is_file() and path.name != 'inputs.json'}
(stage / 'inputs.json').write_text(json.dumps(inputs, indent=2) + '\n')
actual = {row['path']:emission_identity(stage, ROOT, inputs, row)
          for row in profile_sources(stage, ROOT, inputs)}
if actual != expected:
    raise SystemExit('consumer overlay changed C inputs; rejected snapshot: ' + str(stage))
print(json.dumps(dict(profile=str(stage), c_objects_unchanged=len(actual),
                      inputs_sha256=sha(stage / 'inputs.json'))))
