#!/usr/bin/env python3
"""Make pinned CPUID feature advertisements honor existing CPU exclusions."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
UPSTREAM = ROOT / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/cpuid.c'
PIN = '0675b9d86847b17398936d052f863762b3ef367ad6d964f7bb64fade3d74abca'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', required=True, type=Path)
parser.add_argument('--receipt', required=True, type=Path)
args = parser.parse_args()
original = UPSTREAM.read_bytes()
if hashlib.sha256(original).hexdigest() != PIN:
    raise SystemExit('immutable upstream cpuid.c checksum mismatch')
source = original.decode()
changes = []
for line, guard, location in [
    ('      dx |= 1 << 23;   // mmx\n', 'DISABLE_MMX', '00000001:EDX[23]'),
    ('      dx |= 1 << 0;     // fpu\n', 'DISABLE_X87', '80000001:EDX[0]'),
    ('      dx |= 1 << 23;    // mmx\n', 'DISABLE_MMX', '80000001:EDX[23]')]:
    if source.count(line) != 1:
        raise SystemExit('CPUID advertisement boundary changed: ' + location)
    after = '#ifndef ' + guard + '\n' + line + '#endif\n'
    source = source.replace(line, after)
    changes.append(dict(location=location, before=line, after=after))
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(source)
args.receipt.parent.mkdir(parents=True, exist_ok=True)
args.receipt.write_text(json.dumps(dict(kind='cpuid-exclusion-advertisement-only',
    originalSha256=PIN, stagedSha256=hashlib.sha256(source.encode()).hexdigest(),
    changes=changes), indent=2)+'\n')
