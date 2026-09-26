#!/usr/bin/env python3
"""Copy the core/test closure and apply unambiguous local-context patches."""
import json
from pathlib import Path
import shutil
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import policy, references
from campaigns.identity import apply_unified_patch


def stage(source, destination, *, pristine=False, hashes=None):
    hashes = hashes or policy()
    shutil.copytree(source / "Marius.Pinta", destination / "Marius.Pinta")
    if pristine:
        return destination
    pending = []
    for item in json.loads((ROOT / "config/patches.json").read_text()):
        target = destination / item["path"]
        patch = ROOT / "config" / item["patch"]
        if not target.resolve().is_relative_to(destination.resolve()) or not patch.resolve().is_relative_to((ROOT / "config").resolve()):
            raise RuntimeError("Unsafe patch path")
        if hashes.mode == "strict":
            hashes.check(target, item["before_sha256"])
            hashes.check(patch, item["patch_sha256"])
        adapted = apply_unified_patch(target.read_text(), patch.read_text(), label=item["path"])
        pending.append((target, adapted, item))
    for target, adapted, item in pending:
        target.write_text(adapted)
        if hashes.mode == "strict":
            hashes.check(target, item["after_sha256"])
    return destination


def main():
    source = references(ROOT, no_fetch=True)["product"]
    destination = ROOT / "generated/native-input"
    destination.parent.mkdir(exist_ok=True)
    with tempfile.TemporaryDirectory(dir=destination.parent) as temp:
        staged = stage(source, Path(temp) / "source")
        backup = Path(temp) / "previous"
        if destination.exists():
            destination.rename(backup)
        try:
            staged.rename(destination)
        except BaseException:
            if backup.exists():
                backup.rename(destination)
            raise
    print(destination)


if __name__ == "__main__":
    main()
