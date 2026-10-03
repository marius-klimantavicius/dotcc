#!/usr/bin/env python3
"""Compare EDG-translated SQLite with the pinned native differential corpora.

Uses unchanged amalgamation, test harness and deterministic test-only memory
VFS. This profile does not establish production filesystem or threading support.
Outputs must be fresh; generated files and all failing commands are retained.
"""
import argparse
import difflib
import hashlib
import json
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parent
SQLITE = ROOT.parent / 'sqlite'
CORPORA = {
    'core': ('probe.c', 'native-corpus.expected'),
    'api': ('api_native.c', 'native-api.expected'),
    'vfs': ('vfs_native.c', 'native-vfs.expected'),
    'vtable': ('vtable_native.c', 'native-vtable.expected'),
    'allocation': ('allocation_native.c', 'native-allocation.expected'),
    'fts5': ('fts5_native.c', 'native-fts5.expected'),
}


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--suite', choices=CORPORA, action='append')
    parser.add_argument('--native-only', action='store_true')
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--instance-methods', action='store_true')
    parser.add_argument('--without-math', action='store_true',
                        help='diagnostic reduced profile; final acceptance requires math enabled')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    config = json.loads((SQLITE / 'config/sources.json').read_text())['product']
    amalgamation = SQLITE / 'ref' / config['directory']
    engine = amalgamation / 'sqlite3.c'
    archive = SQLITE / 'ref' / config['archive']
    if archive.exists() and digest(archive) != config['sha256']:
        raise ValueError('Pinned SQLite archive hash mismatch')
    definitions = [line.strip() for line in (SQLITE / 'config/defines.txt').read_text().splitlines()
                   if line.strip() and not line.lstrip().startswith('#')]
    if args.without_math:
        definitions = [value for value in definitions if value != 'SQLITE_ENABLE_MATH_FUNCTIONS']
    suites = list(dict.fromkeys(args.suite or ['core']))
    inputs = [engine, amalgamation / 'sqlite3.h', SQLITE / 'tests/memory_vfs.c',
              SQLITE / 'tests/memory_vfs.h', SQLITE / 'tests/optional_features.h',
              SQLITE / 'config/sources.json', SQLITE / 'config/defines.txt']
    inputs += [SQLITE / 'tests' / name for suite in suites for name in CORPORA[suite]]
    report = {
        'schema': 1, 'status': 'running', 'project': 'SQLite',
        'profile': 'pinned feature definitions; deterministic test-only memory VFS; THREADSAFE=0',
        'target': 'portable-lp64-binary80-v2', 'libc': 'musl',
        'compiler': str(args.compiler.resolve()),
        'compilerSha256': digest(args.compiler.resolve()) if not args.native_only else None,
        'instanceMethods': args.instance_methods, 'definitions': definitions,
        'reducedDiagnosticProfile': args.without_math,
        'archiveSha256': config['sha256'],
        'inputs': [{'path': str(path.relative_to(ROOT.parent)), 'sha256': digest(path)}
                   for path in dict.fromkeys(inputs)],
        'commands': [], 'checks': [],
    }

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command, timeout=1200):
        command = list(map(str, command))
        start = time.monotonic()
        entry = {'label': label, 'command': command}
        report['commands'].append(entry)
        save()
        stdout = stderr = b''
        try:
            result = subprocess.run(command, cwd=out, capture_output=True, timeout=timeout)
            stdout, stderr = result.stdout, result.stderr
            entry['exitCode'] = result.returncode
        except subprocess.TimeoutExpired as error:
            stdout, stderr = error.stdout or b'', error.stderr or b''
            entry['timedOut'] = True
            raise
        finally:
            entry['seconds'] = round(time.monotonic() - start, 3)
            (out / (label + '.stdout')).write_bytes(stdout)
            (out / (label + '.stderr')).write_bytes(stderr)
            save()
        if result.returncode:
            raise RuntimeError(f'{label} failed with exit {result.returncode}; see retained logs')
        return result.stdout

    def compare(label, actual, expected):
        if actual != expected:
            difference = ''.join(difflib.unified_diff(
                expected.decode(errors='replace').splitlines(True),
                actual.decode(errors='replace').splitlines(True),
                fromfile='native-expected', tofile=label))
            (out / (label + '.diff')).write_text(difference)
            raise RuntimeError(f'{label} transcript differs; see retained diff')
        report['checks'].append({'label': label, 'stdoutSha256': hashlib.sha256(actual).hexdigest(),
                                 'bytes': len(actual)})
        save()

    try:
        for suite in suites:
            harness, expected_file = CORPORA[suite]
            sources = [engine, SQLITE / 'tests' / harness, SQLITE / 'tests/memory_vfs.c']
            options = ['-D' + definition for definition in definitions]
            includes = ['-I', amalgamation, '-I', SQLITE / 'tests']
            native = out / (suite + '-native')
            run(suite + '-native-build', ['gcc', '-std=c17', '-O2', *options, *includes,
                                         *sources, '-lm', '-o', native])
            native_output = run(suite + '-native-run', [native], timeout=180)
            if not args.without_math:
                compare(suite + '-native-reference', native_output,
                        (SQLITE / 'tests' / expected_file).read_bytes())
            if args.native_only:
                continue
            generated = out / (suite + '-managed')
            run(suite + '-translate', [sys.executable, ROOT / 'dotnet.py', *sources,
                                      '--compiler', args.compiler.resolve(),
                                      '--language', 'c', '--std', 'c17', '--libc', 'musl',
                                      *options, *includes, '--emit', 'exe', '--namespace', 'Edg.SQLite',
                                      '--class-name', 'SqliteCore', '--split', 'size',
                                      '--output-dir', generated,
                                      *(['--instance-methods'] if args.instance_methods else [])])
            project = generated / 'Generated.csproj'
            run(suite + '-managed-build', ['dotnet', 'build', project, '-c', 'Release',
                                          '--nologo', '-p:TreatWarningsAsErrors=true'])
            output = run(suite + '-jit-run', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'],
                         timeout=180)
            compare(suite + '-jit-native-parity', output, native_output)
            if args.nativeaot:
                publish = generated / 'aot'
                run(suite + '-aot-build', ['dotnet', 'publish', project, '-c', 'Release',
                                          '-r', 'linux-x64', '-p:PublishAot=true',
                                          '-p:IlcTreatWarningsAsErrors=true', '--nologo', '-o', publish])
                output = run(suite + '-aot-run', [publish / 'Generated'], timeout=180)
                compare(suite + '-aot-native-parity', output, native_output)
        report['status'] = 'passed'
    except Exception as error:
        report['status'] = 'failed'
        report['failure'] = str(error)
        raise
    finally:
        save()
    print(json.dumps({'status': report['status'], 'checks': report['checks']}, indent=2))


if __name__ == '__main__':
    main()
