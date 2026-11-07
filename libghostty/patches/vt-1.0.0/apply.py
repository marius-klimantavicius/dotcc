"""Apply or verify the explicit compatibility chain for libghostty-vt 1.0.0."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("source", type=Path)
parser.add_argument("--manifest", type=Path,
                    default=Path(__file__).resolve().with_name("manifest.json"))
parser.add_argument("--check", action="store_true",
                    help="require the complete recorded result without writing")
args = parser.parse_args()
manifest_path = args.manifest.resolve()
manifest = json.loads(manifest_path.read_text())
chains = {}
for entry in manifest["patches"]:
    chain = chains.setdefault(entry["source"], [])
    if chain and chain[-1]["patched_sha256"] != entry["original_sha256"]:
        raise SystemExit("Discontinuous patch chain: " + entry["source"])
    chain.append(entry)
    if digest(manifest_path.parent / entry["patch"]) != entry["patch_sha256"]:
        raise SystemExit("Patch differs from the recorded compatibility patch")

# Validate every source before any mutation, including ordered adaptations to
# the same file. Unknown source bytes are always preserved.
already_applied = set()
for name, chain in chains.items():
    source = args.source / name
    value = digest(source)
    if args.check and value != chain[-1]["patched_sha256"]:
        raise SystemExit("Source is not the complete recorded result: " + str(source))
    if value == chain[0]["original_sha256"]:
        continue
    through = next((i for i, entry in enumerate(chain)
                    if value == entry["patched_sha256"]), None)
    if through is None:
        raise SystemExit("Source differs from this patch baseline; preserving it: " + str(source))
    already_applied.update(entry["patch"] for entry in chain[:through + 1])

for entry in manifest["patches"]:
    source = args.source / entry["source"]
    if entry["patch"] in already_applied:
        print("already applied:", entry["patch"])
        continue
    if digest(source) != entry["original_sha256"]:
        raise SystemExit("Source differs from this patch baseline; preserving it: " + str(source))
    subprocess.run(["patch", "--batch", "--forward", "--fuzz=0", "-p1", "-i",
                    str(manifest_path.parent / entry["patch"])],
                   cwd=args.source, check=True)
    if digest(source) != entry["patched_sha256"]:
        raise SystemExit("Patched source differs from the recorded result")
