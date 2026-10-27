#!/usr/bin/env python3
"""Native ordinary guest workload and staged private-mapping PTE invariants.

This does not emit managed code. Run the resulting workload-static ELF through
the coordinator's fresh product consumer for actual translated qualification.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import signal
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[2]
sys.path[:0] = [str(ROOT / 'scripts'), str(ROOT.parent / 'Scripts')]
import memory_bindings
from production_support import locked_command

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-root', type=Path, help='Parent directory for native scratch and receipts')
args = parser.parse_args()
out = args.output_root.resolve() if args.output_root else ROOT / 'artifacts/guest-file-mapping'
out.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='native-', dir=out))
source = ROOT / 'build/native/source'
archive = source / 'o/blink/blink.a'
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
receipt = {'scope': 'ordinary mappings and staged guest PTE metadata; no invalid access or injected host failure',
           'passed': False, 'managed_passed': False, 'commands': [], 'inputs': {}}


def save():
    (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')


def run(command, label, timeout=60):
    requested = list(map(str, command))
    command = locked_command(requested)
    code = None
    try:
        with (attempt / (label + '.log')).open('wb') as log:
            process = subprocess.Popen(command, cwd=attempt, stdout=log, stderr=log,
                                       start_new_session=True, env=dict(os.environ, LC_ALL='C'))
            try:
                code = process.wait(timeout=timeout)
            except BaseException:
                try: os.killpg(process.pid, signal.SIGKILL)
                except ProcessLookupError: pass
                process.wait()
                raise
    finally:
        receipt['commands'].append(dict(label=label, requested=requested, effective=command, exit_code=code))
        save()
    if code:
        raise RuntimeError(label + ' failed: ' + str(attempt / (label + '.log')))


try:
    receipt['inputs']['native_archive'] = sha(archive)
    receipt['inputs']['native_config'] = sha(source / 'config.h')
    for relative in memory_bindings.INPUTS:
        receipt['inputs'][relative] = sha(ROOT / relative)
    for name in ('workload.c', 'invariants.c'):
        original = Path(__file__).parent / name
        receipt['inputs'][name] = sha(original)
        (attempt / name).write_bytes(original.read_bytes())
    for name in ('memorymalloc.c', 'memory.c', 'throw.c'):
        original = source / 'blink' / name
        receipt['inputs']['native/' + name] = sha(original)
        (attempt / name).write_bytes(b'#include "HostMemory.h"\n#include "HostFileMapping.h"\n' +
                                    memory_bindings.adapt('blink/' + name, original.read_bytes()))
    # Durable source assertions cover access ordering and signal translation
    # without executing an excluded invalid-access/fault workload.
    memory = (attempt / 'memory.c').read_text()
    lookup = memory[memory.index('u8 *LookupAddress2('):memory.index('u8 *LookupAddress(', memory.index('u8 *LookupAddress2('))]
    assert lookup.index('if ((entry & mask) != need)') < lookup.index('BlinkHostMemoryFilePageAvailable') < lookup.index('GetPageAddress')
    walk = memory[memory.index('u64 FindPageTableEntry('):memory.index('u8 *LookupAddress2(')]
    assert walk.index('BlinkHostMemoryFilePageAvailable') < walk.index('HandlePageFault') < walk.index('m->tlb[tlbkey].entry = entry')
    assert 'return entry;  /* permission checks precede EOF faults */' in walk
    throw = (attempt / 'throw.c').read_text()
    assert 'SIGBUS_LINUX : SIGSEGV_LINUX' in throw and '-BUS_ADRERR_LINUX' in throw
    reservation = (attempt / 'memorymalloc.c').read_text()
    reservation = reservation[reservation.index('i64 ReserveVirtual('):reservation.index('i64 FindVirtual(')]
    assert 'PanicDueToMmap' not in reservation and 'exit(' not in reservation
    assert reservation.index('backing = blink_host_mmap') < reservation.index('TrackHostPage') < reservation.index('CasPte(slot, old')
    receipt['source_invariants_passed'] = True
    run([sys.executable, ROOT / 'src/Host/scripts/stage-map.py', '--output', attempt / 'map.c',
         '--receipt', attempt / 'map-adaptation.json'], 'stage-map')
    profile = attempt / 'native-profile/sys'
    profile.mkdir(parents=True)
    (profile / 'mman.h').write_bytes((ROOT / 'config/managed-host/sys/mman.h').read_bytes())
    # Use the same explicit no-fork/private-Bus configuration as HostMemory's
    # native adapter oracle; native configure enables process-shared Bus memory.
    for name in ('core-config.h', 'target-storage.h'):
        original = ROOT / 'config' / name
        receipt['inputs'][name] = sha(original)
        (attempt / ('config.h' if name == 'core-config.h' else name)).write_bytes(original.read_bytes())
    args = ['-std=gnu17', '-D_GNU_SOURCE', '-D_DEFAULT_SOURCE', '-DNOLINEAR',
            '-I', attempt, '-I', attempt / 'native-profile', '-I', source, '-I', ROOT / 'src/Host/include']
    run(['cc', '-std=c17', '-Wall', '-Wextra', '-Werror', '-static', attempt / 'workload.c',
         '-o', attempt / 'workload-static'], 'workload-build')
    run([attempt / 'workload-static'], 'workload-native', 30)
    run(['cc', *args, attempt / 'invariants.c', attempt / 'memorymalloc.c', attempt / 'memory.c',
         attempt / 'throw.c', attempt / 'map.c', ROOT / 'src/Host/HostMemory.c',
         source / 'blink/bus.c', source / 'blink/flag.c', source / 'blink/pte32.c', archive,
         '-lz', '-lrt', '-lm', '-pthread', '-o', attempt / 'invariants'], 'invariants-build')
    run([attempt / 'invariants'], 'invariants-native', 30)
    for name in ('workload-static', 'invariants'):
        receipt.setdefault('binaries', {})[name] = sha(attempt / name)
    if (attempt / 'mapping-data').exists() or (attempt / 'invariant-data').exists():
        raise RuntimeError('ordinary workload did not clean up its private files')
    receipt['passed'] = True
finally:
    save()
print(attempt / 'receipt.json')
