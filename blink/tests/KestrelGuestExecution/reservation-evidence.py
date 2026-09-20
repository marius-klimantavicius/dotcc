#!/usr/bin/env python3
"""Read-only reconstruction of the preserved 64 MiB Kestrel admission failure.

Prints JSON to stdout. Does not run the guest, edit an attempt, or claim a direct
snapshot of vss/rss or peak backing. All evidence inputs are exact content pins.
"""
import hashlib
import json
from pathlib import Path
import struct

ROOT = Path(__file__).resolve().parents[2]
ATTEMPT = 'artifacts/kestrel-guest-execution/attempt-7lswgkg3/'
UPSTREAM = 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/'
ELF = 'artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService'
NATIVE_TRACE = 'artifacts/kestrel-native-profile/attempt-zphi57zq/native.strace'
PINS = {
    ATTEMPT + 'receipt.json': 'e7e2075e1b6b6088724093069e592dd7d8a7fbb23bd35c720094b2995c4e4af7',
    ATTEMPT + 'result/result.json': '5fe079f63162191852f93819771e74bc82605eb02b8e074151b8a7bf418ce544',
    ATTEMPT + 'private/src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.cs': 'be5a6a6a4806bbb9d408c87a82abdf36a270b1410cae2635fdb94458114435ed',
    ATTEMPT + 'private/generated/TranslatedBlink/Sources/BlinkCore.00005.cs': '891ff71b4d29d13498abfe2878fc2df496f4c7414f046aa40ace8f48ae0d56ca',
    ATTEMPT + 'private/generated/TranslatedBlink/Sources/BlinkCore.00010.cs': 'b89d3e87366c5fae3042f7c4da543472c663f6e4d6f932e4d4a46088c2a41ff0',
    'src/GuestResources/GuestResources.c': '88f3e01f7f50c98b55e06e0eea5e79840fb894c3678ea9e1161e564bf418961f',
    UPSTREAM + 'loader.c': '8175b61a1de47f9decf91e71f682e73f5260b66ca1eba6686c2d4d302bcd751c',
    UPSTREAM + 'memorymalloc.c': '589becd0e214d5f422e75a9b63b1bf5d5280b3f8ca4e00dc212ede120e945b12',
    UPSTREAM + 'syscall.c': '4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3',
    UPSTREAM + 'tunables.h': 'f9126c9f4f52f17afe9fee2ad65ab3797391e32868dd3823f9ef8989e9268f90',
    ELF: 'ef6f1433794a42fe32b0fed4851bf88dd0631cd6a836550c6effca79d9e9a3ac',
    NATIVE_TRACE: '5114818b518b04fb4e1902f24f3faa2dfda9edcac6c7b06d21c91ffe32122132',
}


def pages(address, size):
    return set(range(address // 4096, (address + size + 4095) // 4096))


def intervals(page_set):
    result = []
    for page in sorted(page_set):
        if result and result[-1][1] == page * 4096:
            result[-1][1] += 4096
        else:
            result.append([page * 4096, (page + 1) * 4096])
    return [{'start': hex(a), 'end_exclusive': hex(b), 'bytes': b - a} for a, b in result]


def main():
    inputs = {}
    for name, expected in PINS.items():
        data = (ROOT / name).read_bytes()
        if hashlib.sha256(data).hexdigest() != expected:
            raise RuntimeError('Evidence identity changed: ' + name)
        inputs[name] = data
    result = json.loads(inputs[ATTEMPT + 'result/result.json'])
    receipt = json.loads(inputs[ATTEMPT + 'receipt.json'])
    assert receipt['limits']['memory_bytes'] == 64 * 1024 * 1024
    main_thread = next(t for t in result['syscall_threads'] if t['guest_thread_id'] == 1)
    failed = [(i, r) for i, r in enumerate(main_thread['trace'])
              if r['number'] == 9 and r['return_value'] == (1 << 64) - 12]
    assert len(failed) == 1
    fail_index, fail = failed[0]
    assert (fail['argument1'], fail['argument2'], fail['argument3'], fail['argument4']) == (0, 274432, 0, 34)
    mappings = set()
    heap_base = heap_end = None
    events = []
    truncation = []
    # Per-thread order is retained. The only observed child mapping is a separate
    # 16 KiB interval, disjoint from all main intervals, before the GC reservation.
    for thread in result['syscall_threads']:
        tid = thread['guest_thread_id']
        trace = thread['trace'][:fail_index] if tid == 1 else thread['trace']
        if thread['truncated']:
            truncation.append({'tid': tid, 'recorded': len(thread['trace']), 'total': thread['syscall_count']})
        for index, row in enumerate(trace):
            number, ret = row['number'], row['return_value']
            if ret is None:
                continue
            address, size = row['argument1'], row['argument2']
            if number == 9 and ret < (1 << 63):
                mapped = pages(ret, size)
                if tid != 1:
                    assert tid == 262144 and ret == 0x20000018c000 and size == 16384
                    assert not mapped & mappings
                mappings |= mapped
            elif number == 11 and ret == 0:
                assert tid == 1
                mappings -= pages(address, size)
            elif number == 12:
                assert tid == 1
                if heap_base is None:
                    assert address == 0
                    heap_base = heap_end = ret
                else:
                    assert ret == address and ret >= heap_end
                    heap_end = ret
            elif number == 25 and ret < (1 << 63):
                raise RuntimeError('Unexpected successful mremap requires replay support')
            else:
                continue
            events.append({'tid': tid, 'thread_trace_index': index, 'syscall': number,
                           'address': hex(address), 'size': size, 'return': hex(ret)})
    assert heap_base == 0x110001000000 and heap_end == heap_base + 5 * 4096
    heap = pages(heap_base, heap_end - heap_base)
    heap_extra = heap - mappings
    elf = inputs[ELF]
    assert elf[:6] == b'\x7fELF\x02\x01'
    phoff = struct.unpack_from('<Q', elf, 32)[0]
    phsize, phcount = struct.unpack_from('<HH', elf, 54)
    elf_pages, segments = set(), []
    for i in range(phcount):
        kind, flags, offset, address, physical, filesz, memsz, align = struct.unpack_from('<IIQQQQQQ', elf, phoff + i * phsize)
        if kind == 1:
            elf_pages |= pages(address, memsz)
            segments.append({'address': hex(address), 'file_bytes': filesz, 'memory_bytes': memsz})
    stack_pages = 8 * 1024 * 1024 // 4096  # pinned tunables.h kStackSize
    assert not elf_pages & (mappings | heap)
    total = len(mappings | heap | elf_pages) + stack_pages
    assert (len(mappings), len(heap_extra), len(elf_pages), total) == (11436, 4, 2887, 16375)
    native_lines = inputs[NATIVE_TRACE].decode().splitlines()
    native_witness = [{'line': i + 1, 'text': line} for i, line in enumerate(native_lines)
                      if 'RLIMIT_AS' in line or '274432' in line or '.NET TP Gate' in line]
    # The resumed return and ensuing clone are included with exact trace lines.
    native_gate_window = [{'line': i + 1, 'text': native_lines[i]} for i in range(561, 605)]
    output = dict(
        kind='read-only-recorded-reservation-reconstruction',
        producer_sha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(), inputs=PINS,
        failed_mmap=dict(thread_trace_index=fail_index, observation=fail),
        counts=dict(active_mmap_pages=len(mappings), extra_heap_pages=len(heap_extra),
                    elf_load_pages=len(elf_pages), main_stack_pages=stack_pages,
                    reconstructed_vss_pages=total, reconstructed_vss_bytes=total * 4096,
                    limit_pages=16384, remaining_pages=16384 - total,
                    requested_pages=fail['argument2'] // 4096,
                    excess_bytes=(total + fail['argument2'] // 4096 - 16384) * 4096),
        elf_load_segments=segments, active_mmap_intervals=intervals(mappings),
        heap_intervals=intervals(heap), elf_load_intervals=intervals(elf_pages), mapping_events=events,
        native_witness=native_witness, native_gate_window=native_gate_window,
        truncated_threads=truncation,
        limitations=[
            'Per-thread traces lack global order; one child omits its final 3664 syscalls.',
            'Recorded child mapping is disjoint; no unrecorded mapping activity is assumed.',
            'This reconstructs reservations, not a direct vss/rss or backing snapshot.',
            'VSS admission is insufficient; the earlier RSS check may also fail.',
            'Retained backing after FreeMachine is not peak physical usage.',
            'The preserved guest attempt failed; arithmetic is not guest qualification.',
        ])
    print(json.dumps(output, indent=2))


if __name__ == '__main__':
    main()
