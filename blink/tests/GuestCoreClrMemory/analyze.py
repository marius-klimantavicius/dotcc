#!/usr/bin/env python3
"""Summarize retained syscall evidence; all numbers remain measured, not inferred caps."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import struct


NAMES = {0: 'read', 1: 'write', 2: 'open', 3: 'close', 9: 'mmap', 10: 'mprotect',
    11: 'munmap', 12: 'brk', 16: 'ioctl', 20: 'writev', 28: 'madvise', 56: 'clone',
    60: 'exit', 74: 'fsync', 83: 'mkdir', 202: 'futex', 231: 'exit_group',
    234: 'tgkill', 257: 'openat', 262: 'newfstatat', 319: 'memfd_create', 435: 'clone3'}


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('attempt', type=Path)
    parser.add_argument('--closure', type=Path, default=Path(__file__).resolve().parents[2] /
        'artifacts/coreclr-runtime/native/attempt-h40bq9ta/closure.json')
    args = parser.parse_args()
    attempt = args.attempt.resolve()
    report = attempt / 'public-workload.json'
    trace = Path(str(report) + '.syscalls.jsonl')
    closure = json.loads(args.closure.read_text())
    image_files = {r['GuestPath']: Path(closure['Images']) / r['File'] for r in closure['Files']}
    rows, partial = [], 0
    for line in trace.read_text().splitlines():
        try: rows.append(json.loads(line))
        except json.JSONDecodeError: partial += 1
    errors, memory_errors, writes = [], [], []
    descriptors, mappings = {}, []
    elf_headers = {}

    def resolve(address):
        result = []
        for start, end, offset, path in mappings:
            if not start <= address < end: continue
            if path == '[anonymous]': continue
            entry = dict(guest_address=hex(address), path=path,
                         file_offset=hex(offset + address - start))
            native = image_files.get(path)
            if native is not None:
                if path not in elf_headers:
                    data = native.read_bytes()
                    segments = []
                    if data[:6] == b'\x7fELF\x02\x01':
                        phoff = struct.unpack_from('<Q', data, 32)[0]
                        phentsize, phnum = struct.unpack_from('<HH', data, 54)
                        for index in range(phnum):
                            kind, _, off, va, _, filesz, memsz, _ = struct.unpack_from(
                                '<IIQQQQQQ', data, phoff + index * phentsize)
                            if kind == 1: segments.append((off, va, filesz, memsz))
                    elf_headers[path] = segments
                at = offset + address - start
                for off, va, filesz, _ in elf_headers[path]:
                    if off <= at < off + filesz:
                        entry['elf_address'] = hex(va + at - off)
                        break
            result.append(entry)
        return result

    def remove_mapping(start, end):
        remaining = []
        for lo, hi, off, path in mappings:
            if hi <= start or lo >= end: remaining.append((lo, hi, off, path))
            else:
                if lo < start: remaining.append((lo, start, off, path))
                if hi > end: remaining.append((end, hi, off + end - lo, path))
        mappings[:] = remaining

    for row in rows:
        call = row['Call']; number, value = call['Number'], call['ReturnValue']
        failed = value is not None and value >= (1 << 64) - 4095
        if failed or number in (1, 20, 234):
            event = dict(sequence=row['Sequence'], syscall=NAMES.get(number, str(number)),
                errno=(1 << 64) - value if failed else None, call=call,
                instruction=resolve(call['InstructionPointer']),
                stack_modules=[x for word in call.get('GuestStack') or [] for x in resolve(word)])
            if number == 9:
                size = (call['Argument2'] + 4095) & -4096
                start = call['Argument1']
                overlap = sum(max(0, min(hi, start + size) - max(lo, start))
                              for lo, hi, _, _ in mappings) if start else 0
                entry = call['EntryMemory']
                event['virtual_reservation'] = dict(requested_pages=size // 4096,
                    recorded_overlap_pages=overlap // 4096,
                    fixed=bool(call['Argument4'] & 16))
                if 'GuestVssPages' in entry:
                    event['virtual_reservation'].update(current_pages=entry['GuestVssPages'],
                        limit_pages=entry['GuestMaxVssPages'],
                        whole_request_exceeds_limit=entry['GuestVssPages'] + size // 4096 > entry['GuestMaxVssPages'],
                        delta_exceeds_limit=entry['GuestVssPages'] + (size - overlap) // 4096 > entry['GuestMaxVssPages'])
            if failed: errors.append(event)
            if failed and event['errno'] == 12: memory_errors.append(event)
            if number in (1, 20, 234): writes.append(event)
        if number in (2, 257) and value is not None and not failed:
            descriptors[value] = call['DiagnosticPath']
        elif number == 319 and value is not None and not failed:
            descriptors[value] = '/memfd:' + (call['DiagnosticPath'] or '')
        elif number == 3 and value == 0: descriptors.pop(call['Argument1'], None)
        elif number == 9 and value is not None and not failed:
            end = value + ((call['Argument2'] + 4095) & -4096)
            remove_mapping(value, end)
            path = descriptors.get(call['Argument5'])
            mappings.append((value, end, call['Argument6'], path or '[anonymous]'))
        elif number == 11 and value == 0:
            remove_mapping(call['Argument1'], call['Argument1'] + ((call['Argument2'] + 4095) & -4096))
    snapshots = [(r['Sequence'], r['Call'][key]) for r in rows for key in ('EntryMemory', 'ReturnMemory')
                 if r['Call'].get(key)]
    peak_combined = max(snapshots, key=lambda r: r[1]['Combined']['CombinedBytes']) if snapshots else None
    summary = dict(diagnostic_only=True, fresh_qualification=False,
        inputs={str(p): digest(p) for p in (trace, args.closure.resolve())},
        calls=len(rows), incomplete_lines=partial,
        max_instructions_at_syscall=max((r['Call']['Instructions'] for r in rows), default=0),
        syscall_errors={f'{name}:errno={errno}': count for (name, errno), count in
            sorted(Counter((e['syscall'], e['errno']) for e in errors).items())},
        peak_combined_at_syscall=peak_combined,
        memory_errors=memory_errors, last_errors=errors[-64:], writes_and_signals=writes,
        last_calls=rows[-16:])
    if report.exists():
        summary['guest_report'] = json.loads(report.read_text())
        summary['inputs'][str(report)] = digest(report)
    target = attempt / 'analysis.json'
    target.write_text(json.dumps(summary, indent=2) + '\n')
    print(target)
    print(json.dumps({k: summary[k] for k in ('calls', 'max_instructions_at_syscall',
                                            'syscall_errors', 'peak_combined_at_syscall')}, indent=2))


if __name__ == '__main__':
    main()
