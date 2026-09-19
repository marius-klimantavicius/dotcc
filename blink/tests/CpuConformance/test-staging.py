#!/usr/bin/env python3
"""Fail-closed tests use private copies; immutable upstream files are never edited."""
import hashlib, json, shutil, subprocess, tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'src/UpstreamScalarFp'
UPSTREAM = ROOT / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink'
base = ROOT / 'artifacts/scalar-fp-staging'
base.mkdir(parents=True, exist_ok=True)
a = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
module = a / 'src/UpstreamScalarFp'
shutil.copytree(SOURCE, module)
reference = a / 'ref' / UPSTREAM.parent.name / 'blink'
reference.mkdir(parents=True)
manifest = json.loads((SOURCE / 'patch-inputs.json').read_text())
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
before = {name: sha(UPSTREAM / name) for name in manifest}
for name in manifest:
    shutil.copyfile(UPSTREAM / name, reference / name)
results = []
def stage(label, success):
    cmd = ['python3', str(module / 'stage.py'), '--output', str(a / label),
           '--receipt', str(a / (label + '.json'))]
    p = subprocess.run(cmd, capture_output=True, text=True, timeout=20)
    (a / (label + '.stdout')).write_text(p.stdout)
    (a / (label + '.stderr')).write_text(p.stderr)
    assert (p.returncode == 0) == success, (label, p.stderr)
    results.append({'name': label, 'exit': p.returncode})

stage('valid', True)
assert (a / 'valid/scalar-fp.patch').read_bytes() == (SOURCE / 'scalar-fp.patch').read_bytes()
for name in manifest:
    p = reference / name
    original = p.read_bytes()
    p.write_bytes(original + b'\n')
    stage('reject-source-' + name, False)
    p.write_bytes(original)
changed = json.loads((module / 'patch-inputs.json').read_text())
changed['cvt.c']['block_sha256'] = '0' * 64
(module / 'patch-inputs.json').write_text(json.dumps(changed))
stage('reject-block', False)

# Compile real staged source with the native profile's JIT exclusion removed.
# This verifies an actual error, rather than relying on documentation alone.
headers = a / 'headers'
headers.mkdir()
native = ROOT / 'build/native/source'
shutil.copytree(native / 'blink', headers / 'blink',
                ignore=lambda directory, names: [n for n in names if not n.endswith(('.h', '.inc'))])
config = (native / 'config.h').read_text()
assert '#define DISABLE_JIT\n' in config
(headers / 'config.h').write_text(config.replace('#define DISABLE_JIT\n', ''))
for name in manifest:
    cmd = ['cc', '-std=c17', '-D_GNU_SOURCE', '-DNOLINEAR', '-I', str(headers),
           '-fsyntax-only', str(a / 'valid' / name)]
    p = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
    (a / ('jit-' + name + '.stderr')).write_text(p.stderr)
    assert p.returncode and 'Reviewed scalar FP staging requires DISABLE_JIT' in p.stderr
    results.append({'name': 'reject-jit-' + name, 'exit': p.returncode})
assert before == {name: sha(UPSTREAM / name) for name in manifest}
receipt = {'passed': True, 'scope': 'staging identity and explicit interpreter-only guard',
           'original_sha256': before, 'tests': results,
           'script_sha256': sha(Path(__file__)),
           'module_sha256': {p.name: sha(p) for p in SOURCE.iterdir() if p.is_file()}}
(a / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(a / 'receipt.json')
