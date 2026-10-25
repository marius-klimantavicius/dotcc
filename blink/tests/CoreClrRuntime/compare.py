#!/usr/bin/env python3
"""Verify two retained native runtime/app images have identical guest paths/bytes."""
import argparse
import hashlib
import json
from pathlib import Path
from evidence import Evidence, manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('receipts', nargs=2, type=Path)
    args = parser.parse_args()
    ev = Evidence('reproducibility')
    try:
        ev.authored()
        images = []
        for path in args.receipts:
            receipt = json.loads(ev.pin(path).read_text())
            if not receipt.get('passed') or not receipt.get('completed'): raise RuntimeError('Native producer did not pass')
            closure = json.loads(ev.pin(receipt['closure']).read_text())
            root = Path(closure['Images'])
            actual = manifest(root)
            if actual != receipt['image_files']: raise RuntimeError('Retained image changed')
            for name, digest in actual.items(): ev.pin(root / name, digest)
            images.append(actual)
        changed = [name for name in sorted(set(images[0]) | set(images[1])) if images[0].get(name) != images[1].get(name)]
        ev.value.update(differences=changed, files=len(images[0]),
            manifest_sha256=hashlib.sha256(json.dumps(images[0], sort_keys=True, separators=(',', ':')).encode()).hexdigest(),
            passed=not changed)
    except BaseException as error: ev.value['error'] = repr(error)
    return ev.finish()


if __name__ == '__main__': raise SystemExit(main())
