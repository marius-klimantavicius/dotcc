#!/usr/bin/env python3
"""Apply reviewed CPU exclusions and bounded advertisement policy to pinned CPUID."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
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
# These advertisements lack complete-core instruction-family qualification.
# Clearing an advertisement does not remove or reject its instruction handler.
for line, location in [
    ('      cx |= 1 << 0;    // sse3\n', '00000001:ECX[0]'),
    ('      cx |= 1 << 1;    // pclmulqdq\n', '00000001:ECX[1]'),
    ('      cx |= 1 << 9;    // ssse3\n', '00000001:ECX[9]'),
    ('      cx |= 1 << 23;   // popcnt\n', '00000001:ECX[23]'),
    ('      cx |= 1 << 30;   // rdrnd\n', '00000001:ECX[30]'),
    ('      cx |= 1 << 13;   // cmpxchg16b\n', '00000001:ECX[13]'),
    ('          bx |= 1 << 0;   // fsgsbase\n', '00000007:EBX[0]'),
    ('          bx |= 1 << 9;   // erms\n', '00000007:EBX[9]'),
    ('          bx |= 1 << 18;  // rdseed\n', '00000007:EBX[18]'),
    ('          cx |= 1 << 22;  // rdpid\n', '00000007:ECX[22]'),
    ('      cx |= 1 << 0;     // lahf\n', '80000001:ECX[0]'),
    ('      dx |= 1 << 27;    // rdtscp\n', '80000001:EDX[27]'),
    ('      dx |= 1 << 8;  // invtsc\n', '80000007:EDX[8]'),
    ('      ax = 0x00000077;\n      bx = 0x00000002;\n      cx = 0x0000000b;\n      dx = 0x00000000;\n', '00000006:all')]:
    if source.count(line) != 1:
        raise SystemExit('CPUID policy boundary changed: ' + location)
    after = '      /* Private profile: unqualified advertisement omitted (' + location + '). */\n'
    source = source.replace(line, after)
    changes.append(dict(location=location, before=line, after=after,
                        policy='advertisement-only; handler behavior unchanged'))
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(source)
args.receipt.parent.mkdir(parents=True, exist_ok=True)
args.receipt.write_text(json.dumps(dict(kind='cpuid-exclusion-and-qualified-profile-advertisement-policy',
    originalSha256=PIN, stagedSha256=hashlib.sha256(source.encode()).hexdigest(),
    changes=changes), indent=2)+'\n')
