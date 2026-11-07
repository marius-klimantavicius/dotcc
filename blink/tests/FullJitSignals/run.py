#!/usr/bin/env python3
"""Build small guest fixtures in task storage and optionally run a built consumer.

Managed source production/build orchestration belongs to the caller. This runner
never builds DotCC or .NET projects, changes generated C#, or starts timing runs.
"""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import platform
import resource
import signal
import subprocess
import sys
import time

HERE = Path(__file__).resolve().parent
STORAGE = Path('/mnt/storage/blink-full-jit-20261027').resolve()
CAUGHT = ('full-jit-signals: caught=2 code=2 trap=6 ip=edited registers=restored '
          'flags=restored mxcsr=restored mask=restored\n').encode()
RECURSIVE = b'full-jit-signals: recursive=1 caught=2 code=2 trap=6 context=restored\n'
SIMD = CAUGHT.replace(b'code=2 trap=6', b'code=3 trap=19')
SIMD_RECURSIVE = RECURSIVE.replace(b'code=2 trap=6', b'code=3 trap=19')
GP = CAUGHT.replace(b'code=2 trap=6', b'code=128 trap=13')
GP_RECURSIVE = RECURSIVE.replace(b'code=2 trap=6', b'code=128 trap=13')
UNCAUGHT = b'full-jit-signals: uncaught-before-ud2\n'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--scratch', type=Path, default=STORAGE/'cpu-signals/native-r1')
    parser.add_argument('--consumer', type=Path, help='Already-built FullJitSignals .dll or executable, with matching deployed worker')
    parser.add_argument('--native-fp', action='store_true', help='Run Full-mode SSE fault recovery against native Linux controls, independently of legacy #UD parity')
    parser.add_argument('--native-gp', action='store_true', help='Run Full-mode #GP saved-context recovery against native Linux controls')
    args = parser.parse_args()
    if args.native_fp and args.native_gp:
        parser.error('Select one focused native-fp or native-gp mode')
    scratch = args.scratch.resolve()
    if not scratch.is_relative_to(STORAGE) or scratch == STORAGE:
        parser.error('--scratch must be a child of ' + str(STORAGE))
    if platform.system() != 'Linux' or platform.machine().lower() not in ('x86_64', 'amd64'):
        parser.error('Native oracle runner requires actual Linux x64; other hosts use the already-built consumer directly')
    scratch.mkdir(parents=True, exist_ok=False)
    for name in ('tmp', 'logs', 'fixtures', 'bundle', 'dotnet-home', 'packages'):
        (scratch/name).mkdir()
    environment = dict(os.environ, TMPDIR=str(scratch/'tmp'), LC_ALL='C', PYTHONDONTWRITEBYTECODE='1',
                       DOTNET_CLI_HOME=str(scratch/'dotnet-home'), NUGET_PACKAGES=str(scratch/'packages'),
                       DOTNET_BUNDLE_EXTRACT_BASE_DIR=str(scratch/'bundle'), DOTNET_CLI_TELEMETRY_OPTOUT='1')
    receipt = {'schema': 1, 'passed': False, 'scope': 'focused #UD/#XM and Linux x64 signal recovery',
               'scratch': str(scratch), 'simulatedCapabilityMasks': [], 'commands': [], 'fixtures': {},
               'managedQualification': 'not-run', 'activeProcessGroups': []}
    receipt['sources'] = {str(p.relative_to(HERE)): sha(p) for p in HERE.iterdir() if p.is_file()}

    def save():
        (scratch/'receipt.json').write_text(json.dumps(receipt, indent=2)+'\n')

    def no_core():
        resource.setrlimit(resource.RLIMIT_CORE, (0, 0))

    def run(label, command, expected=0, timeout=30):
        command = [str(item) for item in command]
        stdout = scratch/'logs'/(label+'.stdout')
        stderr = scratch/'logs'/(label+'.stderr')
        row = {'label': label, 'command': command, 'expectedExit': expected}
        receipt['commands'].append(row)
        start = time.monotonic()
        with stdout.open('wb') as out, stderr.open('wb') as err:
            process = subprocess.Popen(command, stdout=out, stderr=err, env=environment,
                                       cwd=scratch, start_new_session=True, preexec_fn=no_core)
            receipt['activeProcessGroups'].append(process.pid)
            save()
            try:
                code = process.wait(timeout=timeout)
                row['exit'] = code
            finally:
                try:
                    os.killpg(process.pid, signal.SIGKILL)
                except ProcessLookupError:
                    pass
                process.wait()
                receipt['activeProcessGroups'].remove(process.pid)
                row['elapsedSeconds'] = time.monotonic()-start
                row['stdoutSha256'] = sha(stdout)
                row['stderrSha256'] = sha(stderr)
                save()
        if code != expected:
            raise RuntimeError(label + ' exit=' + str(code) + ': ' + str(stderr))
        return stdout.read_bytes()

    try:
        run('adapter-guards', [sys.executable, '-B', HERE/'adapter-tests.py'])
        run('compiler-identity', ['cc', '--version'])
        flags = ['cc', '-O2', '-std=gnu11', '-Wall', '-Wextra', '-Werror', '-fno-pie', '-no-pie',
                 '-fno-builtin', '-fno-stack-protector', '-fno-asynchronous-unwind-tables', '-fno-unwind-tables',
                 '-mgeneral-regs-only', '-nostdlib', '-static', '-Wl,--build-id=none', '-Wl,-e,_start']
        fixtures = scratch/'fixtures'
        for name, define in [('caught-ud2', None), ('caught-ud2-recursive', '-DPROBE_RECURSIVE'),
                             ('caught-xgetbv', '-DPROBE_XGETBV')]:
            run(name+'-build', flags + ([define] if define else []) + [HERE/'context.c', HERE/'context.S', '-o', fixtures/(name+'.elf')])
        for name, defines in [('caught-simd', ['-DPROBE_SIMD']),
                              ('caught-simd-recursive', ['-DPROBE_SIMD', '-DPROBE_RECURSIVE']),
                              ('caught-gp', ['-DPROBE_GP']),
                              ('caught-gp-recursive', ['-DPROBE_GP', '-DPROBE_RECURSIVE'])]:
            run(name+'-build', flags + defines + [HERE/'context.c', HERE/'context.S', '-o', fixtures/(name+'.elf')])
        for name, source in [('uncaught-ud2', 'uncaught.S'), ('cpu-policy', 'cpu-policy.S')]:
            run(name+'-build', flags + [HERE/source, '-o', fixtures/(name+'.elf')])
        run('host-capabilities-build', ['cc', '-O2', '-Wall', '-Wextra', '-Werror', HERE/'host-capabilities.c', '-o', scratch/'host-capabilities'])
        receipt['actualHostCapabilities'] = json.loads(run('host-capabilities', [scratch/'host-capabilities']))
        for name, expected in [('caught-ud2', CAUGHT), ('caught-ud2-recursive', RECURSIVE),
                               ('caught-simd', SIMD), ('caught-simd-recursive', SIMD_RECURSIVE),
                               ('caught-gp', GP), ('caught-gp-recursive', GP_RECURSIVE), ('uncaught-ud2', UNCAUGHT)]:
            actual = run(name+'-native', [fixtures/(name+'.elf')], -signal.SIGILL if name == 'uncaught-ud2' else 0)
            if actual != expected:
                raise RuntimeError(name+' native output differs')
            receipt['fixtures'][name] = {'native': 'pass', 'stdoutBase64': base64.b64encode(actual).decode()}
        receipt['fixtures']['caught-xgetbv'] = {'native': 'not-run: actual host may support XGETBV; guest-only unavailable-feature probe'}
        receipt['fixtures']['cpu-policy'] = {'native': 'not-compared: virtual CPU policy intentionally differs from actual host'}
        for name in receipt['fixtures']:
            path = fixtures/(name+'.elf')
            symbols = run(name+'-symbols', ['nm', '--defined-only', path]).decode().splitlines()
            receipt['fixtures'][name].update(sha256=sha(path), bytes=path.stat().st_size,
                symbols={parts[2]: int(parts[0], 16) for line in symbols if len(parts := line.split()) == 3})
        (fixtures/'manifest.json').write_text(json.dumps(receipt['fixtures'], indent=2)+'\n')
        if args.consumer:
            consumer = args.consumer.resolve()
            command = (['dotnet', consumer] if consumer.suffix.lower() == '.dll' else [consumer])
            run('managed', command + [fixtures, scratch/'managed.json'] + (['--native-fp'] if args.native_fp else ['--native-gp'] if args.native_gp else []), timeout=300)
            managed = json.loads((scratch/'managed.json').read_text())
            if len(managed) != (4 if args.native_fp or args.native_gp else 30):
                raise RuntimeError('Expected four focused Full-native or 30 legacy parity observations')
            receipt['managedQualification'] = 'pass'
            receipt['managedRows'] = managed
        receipt['passed'] = True
    finally:
        save()
    print('PASS ' + str(scratch/'receipt.json'))


if __name__ == '__main__':
    main()
