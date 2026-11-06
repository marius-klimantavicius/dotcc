"""Apply the explicit direct-route compatibility patches to a pristine pinned tree."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('source', type=Path)
args = parser.parse_args()
manifest_dir = Path(__file__).resolve().parent
manifest = json.loads((manifest_dir / 'manifest.json').read_text())
# Multiple small adaptations may touch the same upstream file. Recognize a
# complete or partially applied chain before mutating anything; comparing its
# final bytes only with the first patch's result would reject a valid tree.
chains = {}
for entry in manifest['patches']:
    chain = chains.setdefault(entry['source'], [])
    if chain and chain[-1]['patched_sha256'] != entry['original_sha256']:
        raise SystemExit('Discontinuous patch chain: ' + entry['source'])
    chain.append(entry)
    patch = manifest_dir / entry['patch']
    if hashlib.sha256(patch.read_bytes()).hexdigest() != entry['patch_sha256']:
        raise SystemExit('Patch differs from the recorded compatibility patch')
already_applied = set()
for name, chain in chains.items():
    source = args.source / name
    digest = hashlib.sha256(source.read_bytes()).hexdigest()
    if digest == chain[0]['original_sha256']:
        continue
    through = next((index for index, entry in enumerate(chain)
                    if digest == entry['patched_sha256']), None)
    if through is None:
        raise SystemExit('Source differs from this patch baseline; preserving it: ' + str(source))
    already_applied.update(entry['patch'] for entry in chain[:through + 1])
for entry in manifest['patches']:
    source = args.source / entry['source']
    digest = hashlib.sha256(source.read_bytes()).hexdigest()
    if entry['patch'] in already_applied:
        print('already applied:', entry['patch'])
        continue
    if digest != entry['original_sha256']:
        raise SystemExit('Source differs from this patch baseline; preserving it: ' + str(source))
    patch = manifest_dir / entry['patch']
    subprocess.run(['patch', '--batch', '--forward', '-p1', '-i', str(patch)], cwd=args.source, check=True)
    if hashlib.sha256(source.read_bytes()).hexdigest() != entry['patched_sha256']:
        raise SystemExit('Patched source differs from the recorded result')
