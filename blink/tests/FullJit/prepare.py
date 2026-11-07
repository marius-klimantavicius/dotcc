#!/usr/bin/env python3
"""Prepare normal retained fixtures and the unchanged normal CPU payload."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

R = Path(__file__).resolve().parents[3]
D = R/'blink/artifacts/jit-implementation/full-jit-20261027-r1'
S = Path('/mnt/storage/blink-full-jit-20261027')
p = argparse.ArgumentParser()
p.add_argument('--output', type=Path, default=S/'fixtures')
a = p.parse_args()
out = a.output.resolve()
if not out.is_relative_to(S): raise SystemExit('Outputs must stay in task storage')
out.mkdir(parents=True, exist_ok=True)

def run(label, command):
    subprocess.run([sys.executable, str(D/'command.py'), '--label', label, '--cwd', str(R), '--', *map(str, command)], check=True)

run('fixtures-prepare', [sys.executable, R/'blink/tests/GuestJit/run.py', '--output', out/'ordinary', '--prepare-only'])
c = R/'blink/tests/CpuConformance'
run('cpu-guest-build', ['cc', '-static', '-O2', '-fno-pie', '-no-pie', '-Wl,--build-id=none',
    c/'hardware.c', c/'hardware-capture.S', '-o', out/'cpu.elf'])
run('cpu-describe-build', ['cc', '-O2', c/'describe.c', '-o', out/'describe'])
env = dict(os.environ, TMPDIR=str(S/'tmp'))
with (out/'corpus.json').open('wb') as target: subprocess.run([out/'describe'], stdout=target, check=True, env=env)
run('cpu-fixtures', [sys.executable, R/'blink/tests/FullJit/cpu-fixtures.py', out])
run('host-read-pin-build', ['cc', '-O2', '-D_GNU_SOURCE', '-DBLINK_MANAGED_GUEST_THREADS',
    '-I'+str(R/'blink/src/Host/include'), R/'blink/tests/FullJit/host-read-pin.c', R/'blink/src/Host/HostMemory.c',
    '-pthread', '-o', out/'host-read-pin'])
run('host-read-pin', [out/'host-read-pin'])
ordinary = out/'ordinary/fixtures'
for name, sources in [('native-chain', ['native-chain.S']), ('memory-recovery', ['memory-recovery.c', 'memory-recovery.S'])]:
    run(name+'-build', ['cc', '-O2', '-std=gnu11', '-fno-pie', '-no-pie', '-fno-builtin',
        '-fno-stack-protector', '-mgeneral-regs-only', '-nostdlib', '-static', '-Wl,--build-id=none', '-Wl,-e,_start',
        *[R/'blink/tests/FullJit'/s for s in sources], '-o', ordinary/(name+'.elf')])
    run(name+'-native', [ordinary/(name+'.elf')])
run('elf-baseline-build', ['cc','-nostdlib','-static','-no-pie','-Wl,--build-id=none',
    R/'blink/tests/ElfRuntime/baseline.S','-o',ordinary/'elf-baseline.elf'])
run('elf-baseline-native', [ordinary/'elf-baseline.elf'])
(D/'fixture-pins.json').write_text(json.dumps({str(f.relative_to(S)): hashlib.sha256(f.read_bytes()).hexdigest()
    for f in out.rglob('*') if f.is_file() and f.suffix in ('.elf', '.json')}, indent=2)+'\n')
