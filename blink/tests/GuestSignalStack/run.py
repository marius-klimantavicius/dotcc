#!/usr/bin/env python3
"""Linux and private native Blink guarded alternate-stack functional regression.

Never translates or builds a shared delivery. All native compilation is serial.
"""
import argparse
import ast
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import platform
import shutil
import signal
import sys
import tempfile
import time

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import locked_command, subprocess

EXPECTED = b'guest-signal-stack: guard=4096 registered=16384 onstack=2 returned=2 disabled=1\n'
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--native-source', required=True, type=Path,
                        help='existing configured threaded native Blink source tree with o/blink/blink.a and blink.o')
    args = parser.parse_args()
    base = ROOT / 'artifacts/guest-signal-stack'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    receipt = dict(kind='ordinary-guarded-guest-signal-stack', passed=False, completed=False,
        scope='Linux oracle and isolated native Blink before/after registration adaptation; managed integration belongs to parent',
        attempt=str(attempt), invocation=[sys.executable, *sys.argv],
        inputs={}, commands=[], executions=[], tools={}, managed_passed=False,
        expected_stdout_hex=EXPECTED.hex(), max_build_jobs=1,
        prior_coordinator_stop='Parent reports prior coordinator stopped by automated content filter citing possible cyber risk; no safeguards disabled or circumvented')
    print(attempt, flush=True)

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

    def pin(path):
        path = Path(path).resolve()
        digest = sha(path)
        if receipt['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Input changed: ' + str(path))
        return path

    def copy(original, destination):
        original = pin(original)
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(original, destination)
        pin(destination)

    def load_module(path, name):
        spec = importlib.util.spec_from_file_location(name, pin(path))
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        return module

    helpers = load_module(HERE.parent / 'GuestThreads/run.py', 'signal_stack_helpers')
    env = dict(os.environ, LC_ALL='C', TMPDIR=str(attempt / 'tmp'))
    (attempt / 'tmp').mkdir()

    def run(label, command, expected=0, timeout=60):
        row = dict(label=label, command=list(map(str, command)), cwd=str(attempt),
                   timeout_seconds=timeout, expected_exit_code=expected)
        row['effective_command'] = locked_command(row['command'], env=env)
        receipt['commands'].append(row)
        out, err = [attempt / (label + suffix) for suffix in ('.stdout', '.stderr')]
        process = None
        started = time.monotonic()
        save()
        try:
            with out.open('wb') as stdout, err.open('wb') as stderr:
                process = subprocess.Popen(row['effective_command'], cwd=attempt, env=env,
                    stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr, start_new_session=True)
                row['pid'] = process.pid
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            if process is not None:
                row['cleanup'] = helpers.cleanup(process)
            row['seconds'] = time.monotonic() - started
            row['logs'] = {str(path): sha(path) for path in (out, err) if path.exists()}
            save()
        if (row.get('exit_code') != expected or row['cleanup']['signals'] or
                not row['cleanup']['group_gone']):
            raise RuntimeError(label + ': see retained output in ' + str(attempt))
        return out.read_bytes(), err.read_bytes()

    try:
        if platform.system() != 'Linux' or platform.machine() != 'x86_64':
            raise RuntimeError('Requires Linux x86-64')
        pin(__file__)
        pin(HERE / 'README.md')
        pin(sys.executable)
        pin(REPO / 'Scripts/production_support.py')
        for name in ('cc', 'ld', 'strace', 'readelf'):
            tool = shutil.which(name)
            if tool is None:
                raise RuntimeError('Missing tool: ' + name)
            receipt['tools'][name] = dict(path=str(pin(tool)), sha256=sha(tool))
        tool = lambda name: receipt['tools'][name]['path']
        for name in ('fixture.S', 'fixture.ld'):
            copy(HERE / name, attempt / 'inputs' / name)
        # Freeze product sources and check syntax without writing pycache.
        for name in ('signal_bindings.py', 'stage-host-bindings.py', 'stage-threaded-core.py'):
            path = pin(ROOT / 'scripts' / name)
            ast.parse(path.read_text())
            copy(path, attempt / 'inputs' / name)
        bindings = load_module(attempt / 'inputs/signal_bindings.py', 'frozen_signal_bindings')
        run('assemble', [tool('cc'), '-c', '-nostdlib', attempt / 'inputs/fixture.S', '-o', attempt / 'fixture.o'])
        image = attempt / 'guest-signal-stack.elf'
        run('link', [tool('ld'), '-static', '-nostdlib', '-T', attempt / 'inputs/fixture.ld',
                     '-o', image, attempt / 'fixture.o'])
        receipt['elf'] = dict(path=str(pin(image)), sha256=sha(image), **helpers.elf_info(image))
        run('program-headers', [tool('readelf'), '-W', '-l', image])
        trace = attempt / 'linux.trace'
        out, err = run('linux', [tool('strace'), '-f', '-s', '160', '-o', trace, image], timeout=20)
        if out != EXPECTED or err:
            raise RuntimeError('Linux output differs')
        calls = trace.read_text()
        if (calls.count('rt_sigreturn(') != 2 or calls.count('tkill(') != 2 or
                calls.count('ss_flags=SS_ONSTACK') != 2 or
                'ss_flags=0, ss_size=16384},' not in calls or
                '4096, PROT_NONE) = 0' not in calls or
                'SA_RESTORER|SA_ONSTACK|SA_SIGINFO' not in calls):
            raise RuntimeError('Linux trace lacks guarded registration/delivery/return coverage')
        receipt['trace'] = dict(path=str(pin(trace)), sha256=sha(trace),
                               domain='actual Linux guest syscalls and kernel signal notifications')
        receipt['executions'].append(dict(mode='linux', passed=True, stdout_hex=out.hex(), stderr_hex=err.hex()))

        source = args.native_source.resolve()
        receipt['native_source'] = str(source)
        if any(line.strip() == '#define DISABLE_THREADS'
               for line in pin(source / 'config.h').read_text().splitlines()):
            raise RuntimeError('Native witness must have threads enabled for ordinary self tkill')
        native = attempt / 'native'
        # Snapshot all local native headers, configured ABI, archive and main.
        # Only syscall.c is recompiled; every output stays in this attempt.
        for path in sorted((source / 'blink').rglob('*')):
            if path.is_file() and path.suffix in ('.h', '.inc'):
                copy(path, native / path.relative_to(source))
        for name in ('config.h', 'config.mk'):
            copy(source / name, native / name)
        for name in ('blink.a', 'blink.o'):
            copy(source / 'o/blink' / name, native / name)
        original = pin(source / 'blink/syscall.c').read_bytes()
        original_signal = pin(source / 'blink/signal.c').read_bytes()
        if b'if (CopyToUserWrite(m, sp, &sf, sizeof(sf)) == -1)' not in original_signal:
            raise RuntimeError('Existing signal-frame write validation differs')
        adapted = bindings.adapt('blink/syscall.c', original)
        start = original.index(b'static int SysSigaltstack(')
        end = original.index(b'\n}', start) + 3
        adapted_end = adapted.index(b'\n}', start) + 3
        if adapted[:start] != original[:start] or adapted[adapted_end:] != original[end:]:
            raise RuntimeError('Adaptation changed another function')
        if b'IsValidMemory' in adapted[start:adapted_end]:
            raise RuntimeError('Registration still probes stack memory')
        receipt['adaptation'] = dict(original_sha256=hashlib.sha256(original).hexdigest(),
            staged_sha256=hashlib.sha256(adapted).hexdigest(),
            actual_signal_frame_write_check_retained=True, other_functions_unchanged=True)
        common = ['-std=gnu17', '-O1', '-fno-common', '-fpie', '-D_FILE_OFFSET_BITS=64',
                  '-D_GNU_SOURCE', '-D_DEFAULT_SOURCE', '-DNOLINEAR', '-I', native]
        for label, content, expected in (('before', original, 24), ('after', adapted, 0)):
            path = native / (label + '-syscall.c')
            path.write_bytes(content)
            pin(path)
            obj = native / (label + '-syscall.o')
            exe = native / ('blink-' + label)
            run(label + '-compile', [tool('cc'), *common, '-c', path, '-o', obj])
            run(label + '-link', [tool('cc'), '-pie', '-Wl,-z,noseparate-code',
                '-Wl,-z,norelro', '-Wl,-z,common-page-size=65536,-z,max-page-size=65536',
                native / 'blink.o', obj, native / 'blink.a', '-lz', '-lrt', '-lm', '-pthread', '-o', exe])
            pin(obj)
            pin(exe)
            out, err = run('blink-' + label, [exe, '-jm', image], expected=expected, timeout=20)
            if out != (EXPECTED if label == 'after' else b'') or err:
                raise RuntimeError('Native Blink ' + label + ' output differs')
            receipt['executions'].append(dict(mode='native-blink-' + label,
                passed=label == 'after', expected_exit_code=expected,
                regression_reproduced=label == 'before', stdout_hex=out.hex(), stderr_hex=err.hex()))
        for name, digest in receipt['inputs'].items():
            if sha(name) != digest:
                raise RuntimeError('Frozen input changed: ' + name)
        for row in receipt['commands']:
            for name, digest in row['logs'].items():
                if sha(name) != digest:
                    raise RuntimeError('Closed output changed: ' + name)
        receipt.update(passed=True, final_identities_stable=True)
    except BaseException as error:
        receipt['error'] = type(error).__name__ + ': ' + str(error)
    finally:
        receipt['completed'] = True
        save()
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    def interrupted(number, frame):
        raise InterruptedError('Received signal ' + str(number))
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
