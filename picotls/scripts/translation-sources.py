#!/usr/bin/env python3
"""Select verified upstream units, optionally included by authored C wrappers."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
inputs = json.loads((root / 'config/inputs.json').read_text())
reference = root / 'ref' / inputs['picotls']['directory']


def lines(name):
    return [value.strip() for value in (root / 'config' / name).read_text().splitlines()
            if value.strip() and not value.lstrip().startswith('#')]


core = lines('core-sources.txt')
host = lines('host-sources.txt')
wrappers = json.loads((root / 'config/core-wrappers.json').read_text())
if not set(wrappers).issubset(core) or not set(wrappers.values()).issubset(host):
    raise SystemExit('Core wrappers must name selected upstream units and recorded host sources')
if len(set(wrappers.values())) != len(wrappers):
    raise SystemExit('Each wrapper must include exactly one selected upstream unit')
selected = [reference / name for name in core if name not in wrappers] + [root / name for name in host]
for path in selected:
    path.resolve().relative_to(root)
    if not path.is_file():
        raise SystemExit(f'Missing translation source: {path}')
    print(path)
