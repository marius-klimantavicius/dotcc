#!/usr/bin/env python3
"""Valid nonfixed mmap hint selection across guest page-table boundaries.

Native metadata and Linux guest ELF checks; no shared translation/build.
"""
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

out = ROOT / 'artifacts/guest-mapping-hints'
out.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='native-', dir=out))
source = ROOT / 'build/native/source'
archive = source / 'o/blink/blink.a'
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
receipt = {'scope': 'ordinary nonfixed mmap hint selection, retained neighboring mappings, boundary scans and cleanup',
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
    for name in ('fixture.c', 'invariants.c'):
        original = Path(__file__).parent / name
        receipt['inputs'][name] = sha(original)
        (attempt / name).write_bytes(original.read_bytes())
    for name in ('memorymalloc.c', 'memory.c', 'throw.c'):
        original = source / 'blink' / name
        receipt['inputs']['native/' + name] = sha(original)
        (attempt / name).write_bytes(b'#include "HostMemory.h"\n' +
                                    memory_bindings.adapt('blink/' + name, original.read_bytes()))
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
    run(['cc', '-std=c17', '-Wall', '-Wextra', '-Werror', '-static', attempt / 'fixture.c',
         '-o', attempt / 'fixture-static'], 'workload-build')
    run([attempt / 'fixture-static'], 'workload-native', 30)
    run(['cc', *args, attempt / 'invariants.c', attempt / 'memorymalloc.c', attempt / 'memory.c',
         attempt / 'throw.c', attempt / 'map.c', ROOT / 'src/Host/HostMemory.c',
         source / 'blink/bus.c', source / 'blink/flag.c', source / 'blink/pte32.c', archive,
         '-lz', '-lrt', '-lm', '-pthread', '-o', attempt / 'invariants'], 'invariants-build')
    run([attempt / 'invariants'], 'invariants-native', 30)
    for name in ('fixture-static', 'invariants'):
        receipt.setdefault('binaries', {})[name] = sha(attempt / name)
    receipt['passed'] = True
finally:
    save()
print(attempt / 'receipt.json')
