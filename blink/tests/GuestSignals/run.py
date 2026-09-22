#!/usr/bin/env python3
"""Build one valid static signal fixture and retain its actual Linux witness."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import platform
import shutil
import signal
import subprocess
import sys
import tempfile
import time

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
EXPECTED = b'guest-signals: self=1 child=1 sender=checked pending=checked\n'
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()
# Reuse the established process cleanup, valid-ELF checks and split strace
# decoder. Importing this guarded module never builds or runs the old fixture.
HELPER = HERE.parent / 'GuestThreads/run.py'
spec = importlib.util.spec_from_file_location('guest_signal_native_helpers', HELPER)
helpers = importlib.util.module_from_spec(spec)
sys.dont_write_bytecode = True
spec.loader.exec_module(helpers)


def main():
    base = ROOT / 'artifacts/guest-signals'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    (attempt / 'inputs').mkdir(); (attempt / 'tmp').mkdir()
    receipt = dict(kind='normal-linux-guest-signals', passed=False, completed=False,
        scope='Actual Linux user-signal oracle; pinned native Blink sender metadata is known incomplete and is not the oracle',
        inputs={}, commands={}, executions=[], tools={}, attempt=str(attempt))
    print(attempt, flush=True)

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

    def pin(path):
        path = Path(path).resolve(); digest = sha(path)
        if receipt['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Input changed: ' + str(path))
        return path

    def run(command, label, timeout=30):
        row = dict(command=list(map(str, command)), timeout_seconds=timeout)
        receipt['commands'][label] = row; save()
        out, err = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        process = None; started = time.monotonic()
        try:
            with out.open('wb') as stdout, err.open('wb') as stderr:
                process = subprocess.Popen(row['command'], cwd=attempt, stdout=stdout, stderr=stderr,
                    stdin=subprocess.DEVNULL, start_new_session=True,
                    env=dict(os.environ, LC_ALL='C', TMPDIR=str(attempt / 'tmp')))
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            if process is not None: row['cleanup'] = helpers.cleanup(process)
            row['files'] = {str(path): sha(path) for path in (out, err) if path.exists()}
            row['seconds'] = time.monotonic() - started; save()
        if row['exit_code'] or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ' did not complete normally')
        return out.read_bytes(), err.read_bytes()

    try:
        if platform.system() != 'Linux' or platform.machine() != 'x86_64':
            raise RuntimeError('Requires Linux x86-64')
        pin(HELPER); pin(__file__); pin(sys.executable)
        for name in ('fixture.S', 'fixture.ld'):
            source = pin(HERE / name); shutil.copyfile(source, attempt / 'inputs' / name); pin(attempt / 'inputs' / name)
        for name in ('cc', 'ld', 'readelf', 'nm', 'strace'):
            path = pin(shutil.which(name) or '')
            receipt['tools'][name] = dict(path=str(path), sha256=sha(path))
        tool = lambda name: receipt['tools'][name]['path']
        run([tool('cc'), '-c', '-nostdlib', '-o', attempt / 'fixture.o', attempt / 'inputs/fixture.S'], 'assemble')
        image = attempt / 'guest-signals.elf'
        run([tool('ld'), '-static', '-nostdlib', '-T', attempt / 'inputs/fixture.ld', '-o', image, attempt / 'fixture.o'], 'link')
        pin(image)
        receipt['elf'] = dict(path=str(image), sha256=sha(image), **helpers.elf_info(image))
        run([tool('readelf'), '-W', '-l', image], 'program-headers')
        symbols, _ = run([tool('nm'), '-n', image], 'symbols')
        selected = {}
        for line in symbols.decode().splitlines():
            fields = line.split()
            if len(fields) == 3 and fields[2] in ('handler_start_syscall', 'handler_end_syscall'):
                selected[fields[2]] = int(fields[0], 16)
        if len(selected) != 2 or selected['handler_start_syscall'] >= selected['handler_end_syscall']:
            raise RuntimeError('Handler accounting symbols missing or reversed')
        receipt['handler_markers'] = selected
        trace = attempt / 'linux.trace'
        stdout, stderr = run([tool('strace'), '-f', '-s', '160', '-o', trace, image], 'linux', 20)
        if stdout != EXPECTED or stderr: raise RuntimeError('Linux fixture output differs')
        pin(trace)
        decoded = helpers.trace_info(trace, False)
        calls = trace.read_text()
        if (len(decoded['tids']) != 2 or decoded['unfinished_at_end'] or
                calls.count('tkill(') != 2 or calls.count('rt_sigreturn(') != 2 or
                'rt_sigpending([USR1]' not in calls or 'SIG_UNBLOCK, [USR1]' not in calls or
                calls.count('si_code=SI_TKILL') != 2):
            raise RuntimeError('Native actual signal trace coverage differs')
        decoded['trace_domain'] = 'actual guest Linux syscalls and kernel signal notifications'
        receipt['trace'] = dict(path=str(trace), sha256=sha(trace), decoded=decoded)
        receipt['executions'] = [dict(mode='linux', passed=True, stdout_hex=stdout.hex(), stderr_hex=stderr.hex())]
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest: raise RuntimeError('Frozen input changed: ' + path)
        for row in receipt['commands'].values():
            for path, digest in row['files'].items():
                if sha(path) != digest: raise RuntimeError('Closed command artifact changed: ' + path)
        receipt.update(passed=True, final_identities_stable=True)
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        receipt['completed'] = True; save()
    print(f"passed={receipt['passed']} receipt={attempt / 'receipt.json'}", flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    signal.signal(signal.SIGTERM, helpers.interrupted)
    raise SystemExit(main())
