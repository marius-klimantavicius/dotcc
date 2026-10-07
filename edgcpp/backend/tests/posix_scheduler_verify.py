#!/usr/bin/env python3
"""Native/translated scheduling plus ioctl without stdio-import registration."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    if not sys.platform.startswith('linux'):
        parser.error('This native sched_getcpu comparison currently requires Linux; runtime dispatch also supports Windows')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    allowed = sorted(os.sched_getaffinity(0))
    report = {'status': 'running', 'allowedNativeCpuIds': allowed, 'cpuUpperBound': max(allowed) + 1,
              'commands': [], 'checks': [], 'inputs': [],
              'scope': 'Linux CPU ID range and yield; standard-descriptor ioctl with stdout redirected to file. No equality assumption across separate CPU samples.'}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command):
        command = list(map(str, command))
        entry = {'label': label, 'command': command}
        report['commands'].append(entry)
        save()
        # A real file is intentional: the ioctl fixture must see ENOTTY, not
        # EBADF, even when it is the only import that uses descriptor 1.
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, stdout=stdout, stderr=stderr, timeout=900)
        entry['exitCode'] = result.returncode
        save()
        if result.returncode:
            raise RuntimeError(label + ' failed; see retained stdout/stderr')
        return (out / (label + '.stdout')).read_text()

    try:
        for path in [Path(__file__), TESTS / 'posix_scheduler.c', TESTS / 'posix_ioctl_no_stdio.c',
                     ROOT / 'backend/runtime/PosixScheduler.cs', ROOT / 'backend/runtime/posix/include/sched.h',
                     ROOT / 'backend/dot_gen_posix.h', args.compiler]:
            with path.open('rb') as stream:
                report['inputs'].append({'path': str(path.resolve()), 'sha256': hashlib.file_digest(stream, 'sha256').hexdigest()})
        for fixture, arguments, expected in [
                ('posix_scheduler', [str(report['cpuUpperBound'])], 'scheduler CPU range and yield: 2002 checks passed\n'),
                ('posix_ioctl_no_stdio', [], '')]:
            source = TESTS / (fixture + '.c')
            native = out / (fixture + '-native')
            run(fixture + '-native-build', ['cc', '-std=c11', '-O2', '-Wall', '-Wextra', '-Werror', source, '-o', native])
            actual = run(fixture + '-native-run', [native, *arguments])
            if actual != expected:
                raise RuntimeError('Unexpected native observations for ' + fixture)
            report['checks'].append(fixture + ': native oracle')
            for mode in ('static', 'instance'):
                label = fixture + '-' + mode
                generated = out / label
                run(label + '-translate', [sys.executable, ROOT / 'dotnet.py', source,
                    '--compiler', args.compiler.resolve(), '--language', 'c', '--std', 'c11',
                    '--target', 'portable-lp64-binary80-v2', '--libc', 'musl', '--posix',
                    '--unbound-externals', 'reject', '--namespace', 'Scheduler.Tests', '--class-name', 'Core',
                    *(['--instance-methods'] if mode == 'instance' else []), '--output-dir', generated])
                metadata = json.loads((generated / 'dot-gen.json').read_text())
                if metadata['unboundExternals']['symbols']:
                    raise RuntimeError('Unbound runtime import: ' + label)
                project = generated / 'Generated.csproj'
                run(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
                actual = run(label + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll', *arguments])
                if actual != expected:
                    raise RuntimeError('Translated/native observations differ: ' + label)
                report['checks'].append(label + ': JIT')
                if args.nativeaot:
                    rid = 'linux-arm64' if os.uname().machine == 'aarch64' else 'linux-x64'
                    run(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', rid,
                        '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true',
                        '-o', generated / 'aot'])
                    actual = run(label + '-aot', [generated / 'aot/Generated', *arguments])
                    if actual != expected:
                        raise RuntimeError('Translated AOT/native observations differ: ' + label)
                    report['checks'].append(label + ': NativeAOT')
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
