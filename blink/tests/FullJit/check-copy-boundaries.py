#!/usr/bin/env python3
"""Source/model checks only. Never invokes a compiler, emitter or guest.

Usage: python3 check-copy-boundaries.py --bound-source PINNED_NATIVE4_BOUND
       --selected-stage PINNED_NATIVE4_STAGE_JSON --output SCRATCH_JSON
"""
import argparse
import ast
import hashlib
import importlib.util
import json
from pathlib import Path
import sys

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[2]


def digest(raw):
    return hashlib.sha256(raw).hexdigest()


def rejected(action):
    try:
        action()
    except ValueError:
        return True
    return False


def main():
    args = argparse.ArgumentParser()
    args.add_argument('--bound-source', type=Path, required=True)
    args.add_argument('--selected-stage', type=Path, required=True)
    args.add_argument('--output', type=Path, required=True)
    options = args.parse_args()
    script = ROOT / 'scripts/full_jit_copy_bindings.py'
    ast.parse(script.read_text())
    spec = importlib.util.spec_from_file_location('copy_adapter', script)
    adapter = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(adapter)
    stage_bytes = options.selected_stage.read_bytes()
    stage = json.loads(stage_bytes)
    inputs = {}; outputs = {}; pins = []
    for path in sorted(options.bound_source.glob('*.c')):
        name = 'blink/' + path.name
        raw = path.read_bytes()
        # Native4's selected stage authenticates every bound file by relative
        # profile path. This check is path-specific, not hash membership alone.
        expected = stage['profile_files'].get('bound/' + path.name)
        assert expected == digest(raw), (path, expected, digest(raw))
        # Copy adaptation precedes semantic cloning. Strip only the separately
        # marked producer suffix from this authenticated historical bound file;
        # do not run that producer or pass duplicate syscall dispatcher state.
        clone_marker = b'/* Full-JIT operation-specific semantic clones. */'
        precopy = raw.split(clone_marker, 1)[0] if clone_marker in raw else raw
        inputs[name] = precopy
        outputs[name] = adapter.adapt(name, precopy)
        pins.append(dict(path=str(path), sha256=digest(raw), bytes=len(raw),
                         precloneSha256=digest(precopy), strippedCloneSuffix=clone_marker in raw))
    changed = [name for name in inputs if inputs[name] != outputs[name]]
    assert changed == sorted(['blink/memory.c', 'blink/memorymalloc.c', 'blink/reset.c',
        'blink/signal.c', 'blink/throw.c', 'blink/iovs.c', 'blink/syscall.c', 'blink/ioctl.c'])
    negative = {}
    for name in changed:
        negative['double-adaptation:' + name] = rejected(lambda n=name: adapter.adapt(n, outputs[n]))
    negative['unknown-owning-root'] = rejected(lambda: adapter.adapt('blink/loader.c',
        inputs['blink/loader.c'] + b'\nvoid Bad(struct Machine *m) { AddToFreeList(m, (char *)malloc(8) + 1); }\n'))
    negative['unknown-freelist-consumer'] = rejected(lambda: adapter.adapt('blink/address.c',
        inputs['blink/address.c'] + b'\nvoid Bad(struct Machine *m) { free(m->freelist.p[0]); }\n'))
    negative['changed-variable-root'] = rejected(lambda: adapter.adapt('blink/memory.c',
        inputs['blink/memory.c'].replace(b'res = AddToFreeList(m, copy);', b'res = AddToFreeList(m, copy + 1);')))
    negative['already-cloned-input'] = rejected(lambda: adapter.adapt('blink/syscall.c',
        (options.bound_source / 'syscall.c').read_bytes()))
    assert all(negative.values()), negative
    memory = outputs['blink/memory.c'].decode()
    syscall = outputs['blink/syscall.c'].decode()
    allocator = outputs['blink/memorymalloc.c'].decode()
    checks = {
        'virtual-copy-read': 'BlinkGuestReadCopy(m, r, p, k);' in memory,
        'virtual-copy-write': 'BlinkGuestWriteCopy(m, p, r, k);' in memory,
        'schlepr-owned': 'return BlinkGuestSnapshot(m, addr, size, PAGE_U, PAGE_U);' in memory,
        'string-owned': 'BlinkGuestSnapshotResize(m, copy, capacity, slot)' in memory,
        'copystr-no-duplicate-strdup': 'AddToFreeList(m, strdup(s))' not in memory,
        'iov-passive-pointer': b'BlinkGuestLookupAddressCopy(m, addr, mask, need)' in outputs['blink/iovs.c'],
        'futex-live-compare': syscall.count('if (Load32(mem) != expect)') == 1,
        'futex-managed-finally-handoff': 'blink_host_guest_futex_enqueue(m, mem, expect' in syscall,
        'childtid-stores': syscall.count('BlinkGuestTidStore(m, ') == 4,
        'garbage-tag-dispatch': 'blink_host_guest_snapshot_free(m,' in allocator,
        'garbage-slot-cleared-before-free': allocator.index('m->freelist.p[i] = 0;') < allocator.index('blink_host_guest_snapshot_free(m,'),
        'vector-capacity-actual-free': 'blink_host_guest_freelist_free(m)' in allocator,
        'reset-preserves-ownership': b'memset(&m->freelist' not in outputs['blink/reset.c'],
        'consume-signal-before-lock': b'blink_host_guest_consume_signal(m, delivered, restart)' in outputs['blink/signal.c'],
        'fault-signal-before-lock': b'blink_host_guest_deliver_signal_user(m, sig, code)' in outputs['blink/throw.c'],
        'sigaction-before-lock': b'return blink_host_guest_sigaction(m, sig, act, old, sigsetsize)' in outputs['blink/syscall.c'] and
            b'int BlinkGuestSigaction(struct Machine *m, int sig, i64 act, i64 old,' in outputs['blink/syscall.c'],
        'sigpipe-before-lock': 'blink_host_guest_handle_sigpipe(m, rc, flags)' in syscall,
        'tkill-before-lock': 'blink_host_guest_tkill_self(m, sig)' in syscall,
        'robust-target-is-real-alias': '(_Atomic(u32) *)SchlepRW(m, futex_addr, 4)' in syscall,
    }
    assert all(checks.values()), checks
    # Independent partition model: every actual-use interval stays in one host
    # page, covers exactly the requested bytes, and caps a 64KiB I/O envelope.
    partitions = 0
    for start in range(4096):
        for length in (0, 1, 4095, 4096, 4097, 65536):
            offset = 0
            while offset < length:
                size = min(4096 - ((start + offset) & 4095), length - offset)
                assert 0 < size <= 4096
                assert (start + offset) // 4096 == (start + offset + size - 1) // 4096
                offset += size
            assert offset == length
            partitions += 1
    # Independent ownership/quota model. Replacement must reserve overlapping
    # old+new storage; a failed replacement leaves the old object charged/live.
    quota_cases = 0
    for limit in (8192, 65536, 1048576):
        live = {}; charged = 0
        for count in (1, 4096, 8192, limit, 128, 65536):
            charge = count + 32
            if charge <= limit - charged:
                live[count] = live.get(count, 0) + 1; charged += charge
            assert charged == sum((size + 32) * n for size, n in live.items()) <= limit
            quota_cases += 1
        for old in list(live):
            replacement = old + 4096; amount = replacement + 32
            before = charged
            if amount <= limit - charged:
                charged += amount
                charged -= old + 32
                live[old] -= 1; live[replacement] = live.get(replacement, 0) + 1
            else:
                assert charged == before and live[old] > 0
            assert charged == sum((size + 32) * n for size, n in live.items()) <= limit
            quota_cases += 1
        for count, n in live.items(): charged -= (count + 32) * n
        assert charged == 0
    result = dict(status='PASS_SOURCE_MODEL_ONLY', adapterSha256=digest(script.read_bytes()),
        stage=dict(path=str(options.selected_stage), sha256=digest(stage_bytes)),
        selectedSources=pins, transformed=changed, guardRejections=negative, sourceChecks=checks,
        pagePartitionCases=partitions, independentQuotaCases=quota_cases,
        runtime='UNRUN: no build, emission, C#, native, guest, concurrency or performance execution')
    options.output.write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], auditedUnits=len(inputs), transformed=len(changed),
        guardRejections=len(negative), sourceChecks=len(checks), pagePartitionCases=partitions, quotaCases=quota_cases)))


if __name__ == '__main__':
    main()
