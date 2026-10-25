#!/usr/bin/env python3
"""Extract Console initialization and failed memory/thread calls from a retained trace."""
import argparse
import json
from pathlib import Path

NAMES = {0: 'read', 1: 'write', 3: 'close', 9: 'mmap', 10: 'mprotect',
         11: 'munmap', 12: 'brk', 13: 'rt_sigaction', 14: 'rt_sigprocmask',
         16: 'ioctl', 22: 'pipe', 56: 'clone', 60: 'exit', 72: 'fcntl',
         231: 'exit_group', 257: 'openat', 293: 'pipe2', 435: 'clone3'}


def signed(value):
    return None if value is None else value if value < (1 << 63) else value - (1 << 64)


def describe(index, row):
    arguments = [row['Argument' + str(i)] for i in range(1, 7)]
    return dict(index=index, tid=row['GuestThreadId'], ip=hex(row['InstructionPointer']),
        syscall=NAMES.get(row['Number'], str(row['Number'])),
        args=[hex(a) for a in arguments], result=signed(row['ReturnValue']),
        path=row.get('DiagnosticPath'),
        metrics={name: row[name] for name in ('Instructions', 'VssPages', 'RssPages',
            'MaxVssPages', 'MaxRssPages', 'RlimitAsCurrent', 'OwnerBytes', 'MemoryFileBytes') if name in row})


def analyze(path):
    source = Path(path).read_text()
    calls = [json.loads(line) for line in source.splitlines()] if str(path).endswith('.jsonl') else json.loads(source)['Calls']
    descriptors, library_bases, native_pipes = {}, set(), []
    for i, row in enumerate(calls):
        result = signed(row['ReturnValue'])
        if row['Number'] == 257 and result is not None and result >= 0:
            descriptors[result] = row.get('DiagnosticPath')
        if row['Number'] == 9 and result is not None and result >= 0 and row['Argument6'] == 0:
            name = descriptors.get(row['Argument5'])
            if name and name.endswith('/libSystem.Native.so'):
                library_bases.add(result)
        if row['Number'] == 3 and result == 0:
            descriptors.pop(row['Argument1'], None)
        if row['Number'] == 293 and row['Argument1'] - 0x1a9b8 in library_bases:
            native_pipes.append(i)
    selected = set()
    for i, row in enumerate(calls):
        result = signed(row['ReturnValue'])
        if row['Number'] in (16, 22, 56, 72, 293, 435) or (
                row['Number'] in (9, 10, 11, 12) and result is not None and -4095 <= result < 0):
            selected.update(range(max(0, i - 2), min(len(calls), i + 3)))
        if row.get('DiagnosticPath') and 'libSystem.Native' in row['DiagnosticPath']:
            selected.update(range(max(0, i - 1), min(len(calls), i + 10)))
    return dict(trace=str(Path(path).resolve()), call_count=len(calls),
        diagnostic_only=True, exact_native_bases=[hex(base) for base in sorted(library_bases)],
        exact_native_initialization=[dict(pipe_index=i,
            context=[describe(j, calls[j]) for j in range(max(0, i - 5), min(len(calls), i + 14))]) for i in native_pipes],
        calls=[describe(i, calls[i]) for i in sorted(selected)])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('trace', type=Path)
    args = parser.parse_args()
    print(json.dumps(analyze(args.trace), indent=2))
