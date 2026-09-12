#!/usr/bin/env python3
"""Compile-only scope probe. Never changes ref/ or compiler sources."""
import argparse
import collections
import difflib
import hashlib
import json
from pathlib import Path
import re
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
SNAPSHOT = json.loads((ROOT / 'ref/snapshot.json').read_text())
SOURCE = ROOT / 'ref' / SNAPSHOT['directory']
DEFINES = ['CX_PLATFORM_LINUX=1', '__linux__=1', '_GNU_SOURCE=1', 'NDEBUG=1',
           'QUIC_BUILD_STATIC=1', 'QUIC_EVENTS_STUB=1', 'QUIC_LOGS_STUB=1']
COMPILER = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'


def snapshot_compiler():
    """Keep a multi-unit probe independent of concurrent development rebuilds."""
    files = sorted(path for path in COMPILER.parent.iterdir() if path.suffix in ('.dll', '.json'))
    contents = {path.name: path.read_bytes() for path in files}
    hashes = {name: hashlib.sha256(value).hexdigest() for name, value in contents.items()}
    identity = hashlib.sha256(json.dumps(hashes, sort_keys=True).encode()).hexdigest()
    destination = ROOT / 'artifacts/toolchains' / identity
    destination.mkdir(parents=True, exist_ok=True)
    for name, value in contents.items():
        (destination / name).write_bytes(value)
    if any(hashlib.sha256(path.read_bytes()).hexdigest() != hashes[path.name] for path in files):
        raise RuntimeError('Compiler changed while snapshotting; retry after the build completes')
    (destination / 'manifest.json').write_text(json.dumps(hashes, indent=2) + '\n')
    return destination / COMPILER.name, hashes


def prepare_headers():
    """Diagnostic declarations only; this is not the managed product ABI."""
    target = ROOT / 'artifacts/diagnostic-headers'
    target.mkdir(parents=True, exist_ok=True)
    for header in (ROOT / 'config/probe-headers').rglob('*'):
        if header.is_file():
            output = target / header.relative_to(ROOT / 'config/probe-headers')
            output.parent.mkdir(parents=True, exist_ok=True)
            output.write_bytes(header.read_bytes())
    (target / 'probe-pthread-base.h').write_bytes((REPO / 'DotCC.Lib/include/pthread.h').read_bytes())
    return target


def prepare():
    """Mechanical diagnostic copies; every upstream difference is saved."""
    mirror = ROOT / 'artifacts/probe-source'
    changes = []
    callback_pattern = r'(?m)^\((?:(QUIC_API)\s+)?([A-Z][A-Z0-9_]+)\)\('
    callback_names = set()
    callback_prototypes = {}
    for path in (SOURCE / 'src').rglob('*.h'):
        content = path.read_text()
        for match in re.finditer(callback_pattern, content):
            callback_names.add(match[2])
            begin = content.rfind('typedef', 0, match.start())
            end = content.find(';', match.end())
            callback_prototypes[match[2]] = (content[begin + len('typedef'):match.start()]
                + (match[1] + ' ' if match[1] else '') + '{function_name}' + content[match.end()-1:end+1])
    platform_header = (SOURCE / 'src/platform/platform_internal.h').read_text()
    common_bodies = {name: re.search(r'typedef struct ' + name + r' \{(.*?)\} ' + name + ';',
                                   platform_header, re.S)[1]
                     for name in ['CXPLAT_SOCKET_COMMON', 'CXPLAT_DATAPATH_COMMON']}
    recv_body = re.search(r'typedef struct CXPLAT_RECV_DATA \{(.*?)\} CXPLAT_RECV_DATA;',
                         (SOURCE / 'src/inc/quic_datapath.h').read_text(), re.S)[1]
    handle_body = re.search(r'typedef struct QUIC_HANDLE \{(.*?)\} QUIC_HANDLE;',
                           (SOURCE / 'src/core/library.h').read_text(), re.S)[1]
    for section in ['inc', 'core', 'platform']:
        for source in sorted((SOURCE / 'src' / section).iterdir()):
            if source.suffix not in ('.h', '.c', '.ver'):
                continue
            original = source.read_text()
            # dotcc rejects function-type typedefs even without parentheses.
            # Use pointer typedefs and remove one explicit pointer at uses.
            modified = original
            # A bare function-typedef declaration declares a FUNCTION, not a
            # pointer variable. Preserve that distinction before changing aliases.
            for name, prototype in callback_prototypes.items():
                modified = re.sub(r'(?m)^\s*' + name + r'\s+(\w+)\s*;',
                    lambda m: prototype.replace('{function_name}', m[1]), modified)
            for name in sorted(callback_names):
                modified = re.sub(r'\b' + name + r'\s*\*', name + ' ', modified)
            modified = re.sub(callback_pattern,
                              lambda m: '(' + (m[1] + ' ' if m[1] else '') + '*' + m[2] + ')(', modified)
            modified = modified.replace('"msquic.ver"', '"msquic-version.h"')
            # Scope-only bypass: dropping this alignment invalidates ABI claims.
            modified = re.sub(r'(?<=struct )__attribute__\(\(aligned\([A-Za-z0-9_]+\)\)\) ', '', modified)
            modified = modified.replace('__attribute__((noinline, noreturn))', '_Noreturn')
            modified = modified.replace('__attribute__((no_instrument_function))', '')
            modified = modified.replace('__attribute__((always_inline))', '')
            # Bypass the separately reproduced nested variadic expansion bug.
            # Logging arguments are not evaluated in this diagnostic copy.
            if source.name == 'quic_trace.h':
                modified = re.sub(r'(?m)^#define (QuicTrace\w*|clog)\((?:[^\n]*\\\n)*[^\n]*',
                    lambda m: '#define ' + m[1] + '(...) ' + ('0' if m[1].endswith('Enabled') else '((void)0)'), modified)
            # The upstream C no-op macro leaves an unsupported file-scope ';'.
            modified = re.sub(r'(?m)^DEFINE_ENUM_FLAG_OPERATORS\((\w+)\);', r'DEFINE_ENUM_FLAG_OPERATORS(\1)', modified)
            modified = modified.replace('#define CXPLAT_STATIC_ASSERT(X,Y) static_assert(X, #Y);',
                                        '#define CXPLAT_STATIC_ASSERT(X,Y) static_assert(X, #Y)')
            # GCC's integer value for the upstream ASCII pool tags.
            modified = re.sub(r"(?m)^(#define QUIC_POOL_\w+\s+)'([A-Za-z0-9_]{4})'",
                              lambda m: m[1] + hex(int.from_bytes(m[2].encode(), 'big')), modified)
            # Expand -fms-extensions typedef-based anonymous members to C11 form.
            for name, body in common_bodies.items():
                modified = re.sub(r'(?m)^(\s+)' + name + ';', lambda m: m[1] + 'struct {' + body + '};', modified)
            modified = modified.replace('    struct CXPLAT_RECV_DATA;', '    struct {' + recv_body + '};')
            modified = modified.replace('    struct QUIC_HANDLE;', '    struct {' + handle_body + '};')
            # Avoid initializing a runtime-owned aggregate unknown to the binder.
            modified = modified.replace('struct timespec Ts = {0, 0};',
                                        'struct timespec Ts; Ts.tv_sec = 0; Ts.tv_nsec = 0;')
            target = mirror / 'src' / section / (source.name if source.suffix != '.ver' else 'msquic-version.h')
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(modified)
            if original != modified:
                changes.extend(difflib.unified_diff(original.splitlines(True), modified.splitlines(True),
                    fromfile=str(source.relative_to(SOURCE)), tofile=str(target.relative_to(mirror))))
    (ROOT / 'artifacts/probe-source.patch').write_text(''.join(changes))
    (mirror / 'src/inc/probe-pthread-base.h').write_text((REPO / 'DotCC.Lib/include/pthread.h').read_text())
    return mirror


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--stage', choices=['baseline', 'headers', 'syntax'], default='baseline')
    parser.add_argument('--label', help='Separate result directory label, e.g. compiler-iteration-3')
    parser.add_argument('--units', nargs='*', help='Basenames; default: all core + five portable platform candidates')
    parser.add_argument('--timeout', type=int, default=90)
    parser.add_argument('--preprocess', action='store_true')
    args = parser.parse_args()
    compiler, compiler_hashes = snapshot_compiler()
    source = prepare() if args.stage == 'syntax' else SOURCE
    header_overlay = prepare_headers() if args.stage == 'headers' else None
    cmake = (SOURCE / 'src/core/CMakeLists.txt').read_text().split('set(SOURCES', 1)[1].split(')', 1)[0]
    units = ['src/core/' + name for name in re.findall(r'\b[\w]+\.c\b', cmake)]
    units += ['src/platform/' + name + '.c' for name in ['crypt', 'hashtable', 'pcp', 'platform_worker', 'toeplitz']]
    if args.units:
        units = [unit for unit in units if Path(unit).stem in args.units]
    if args.label and (Path(args.label).name != args.label or args.label in ('.', '..')):
        parser.error('--label must be a single directory name')
    label = (args.label or args.stage) + ('-preprocess' if args.preprocess else '')
    logs = ROOT / 'artifacts' / label
    output = ROOT / 'generated' / label
    logs.mkdir(parents=True, exist_ok=True)
    output.mkdir(parents=True, exist_ok=True)
    (logs / 'results.json').unlink(missing_ok=True)
    results = []
    for unit in units:
        key = Path(unit).parent.name + '-' + Path(unit).stem
        command = ['dotnet', str(compiler), '-std=c17'] + ['-D' + d for d in DEFINES]
        command += ['-I', str(source / 'src/inc')]
        if args.stage == 'syntax':
            command += ['-I', str(ROOT / 'config/probe-headers')]
        if header_overlay:
            command += ['-I', str(header_overlay)]
        command += [str(source / unit)]
        command += ['-E'] if args.preprocess else ['--emit=obj', '-o', str(output / (key + '.cs'))]
        (logs / (key + '.command.json')).write_text(json.dumps(command, indent=2) + '\n')
        start = time.monotonic()
        try:
            result = subprocess.run(command, capture_output=True, text=True, timeout=args.timeout)
            code = result.returncode
            stdout, stderr = result.stdout, result.stderr
        except subprocess.TimeoutExpired as error:
            code, stdout, stderr = 124, '', f'Timed out after {args.timeout}s\n'
        (logs / (key + '.log')).write_text(stderr + ('' if args.preprocess else stdout))
        if args.preprocess:
            (logs / (key + '.i')).write_text(stdout)
        lines = (stderr + ('' if args.preprocess else stdout)).splitlines()
        error = next((line for line in lines if any(word in line.lower() for word in
            ['parse failed', 'unsupported', 'not supported', 'error:', 'timed out'])), lines[0] if lines else '')
        record = {'unit': unit, 'exit_code': code, 'seconds': round(time.monotonic()-start, 3),
                  'first_error': error, 'log': str((logs / (key + '.log')).relative_to(ROOT))}
        results.append(record)
        print(f'{unit:43} {code:3} {error[:200]}', flush=True)
    (logs / 'results.json').write_text(json.dumps({'snapshot': SNAPSHOT, 'defines': DEFINES,
        'stage': label, 'source_stage': args.stage, 'compiler': str(compiler), 'compiler_hashes': compiler_hashes,
        'results': results}, indent=2) + '\n')
    print('Exit codes:', dict(collections.Counter(r['exit_code'] for r in results)))


if __name__ == '__main__':
    main()
