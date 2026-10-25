#!/usr/bin/env python3
"""Retain an ordinary, futex-free native emulator regression and every failure."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import signal
import subprocess
import tempfile

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--blink', required=True, type=Path,
                        help='Existing native Blink binary; never built or modified here')
    parser.add_argument('--runs', default=20, type=int)
    args = parser.parse_args()
    if not 1 <= args.runs <= 100:
        parser.error('--runs must be between 1 and 100')
    base = ROOT / 'artifacts/coreclr-coordination'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='native-clone-memory-', dir=base))
    print(attempt, flush=True)
    receipt = dict(passed=False, completed=False,
                   scope='Linux and supplied native Blink clone/memory only; no futex or managed qualification',
                   native_binary_origin='external; source/build provenance belongs to its producer',
                   native_limitation_reproduced=False, inputs={}, commands=[],
                   cleanup='Every subprocess runs in a private process group, killed and reaped on timeout; artifacts retained')

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

    def pin(path):
        path = Path(path).resolve()
        receipt['inputs'][str(path)] = hashlib.sha256(path.read_bytes()).hexdigest()
        return path

    def run(name, command):
        row = dict(name=name, command=list(map(str, command)), cwd=str(attempt), timeout_seconds=10)
        receipt['commands'].append(row)
        save()
        with (attempt / (name + '.log')).open('wb') as log:
            process = subprocess.Popen(row['command'], cwd=attempt, stdout=log,
                                       stderr=subprocess.STDOUT, start_new_session=True,
                                       env=dict(os.environ, LC_ALL='C'))
            try:
                row['exit'] = process.wait(timeout=row['timeout_seconds'])
            except subprocess.TimeoutExpired:
                row['timed_out'] = True
                os.killpg(process.pid, signal.SIGKILL)
                row['exit'] = process.wait()
        save()
        print(name + ': ' + str(row['exit']), flush=True)
        return row

    try:
        pin(__file__)
        blink = pin(args.blink)
        source = pin(HERE / 'native-clone-memory.S')
        linker = pin(HERE.parent / 'GuestThreads/fixture.ld')
        guest = attempt / 'clone-memory'
        for name, command in [
            ('assemble', ['cc', '-c', '-nostdlib', source, '-o', attempt / 'clone-memory.o']),
            ('link', ['ld', '-static', '-z', 'noexecstack', '-z', 'max-page-size=4096',
                      '-T', linker, attempt / 'clone-memory.o', '-o', guest]),
        ]:
            if run(name, command)['exit']:
                raise RuntimeError(name + ' failed')
        pin(guest)
        linux = [run('linux-' + str(i), [guest]) for i in range(args.runs)]
        native = [run('native-' + str(i), [blink, '-jm', guest]) for i in range(args.runs)]
        receipt['linux_failures'] = [row['name'] for row in linux if row['exit']]
        receipt['native_failures'] = [row['name'] for row in native if row['exit']]
        receipt['native_sigsegv_count'] = sum(row['exit'] == -signal.SIGSEGV for row in native)
        receipt['native_limitation_reproduced'] = (not receipt['linux_failures']
                                                   and receipt['native_sigsegv_count'] > 0)
        for path, digest in receipt['inputs'].items():
            if hashlib.sha256(Path(path).read_bytes()).hexdigest() != digest:
                raise RuntimeError('Input changed: ' + path)
        receipt['passed'] = not receipt['linux_failures'] and not receipt['native_failures']
    except Exception as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        receipt['completed'] = True
        save()
    print('receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
