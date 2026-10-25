#!/usr/bin/env python3
"""Pinned-libc valid getcpu fixture and private authored ABI-boundary regression.

No translation or shared builds. The parent supplies fresh managed integration.
"""
import argparse
import ast
import importlib.util
import json
from pathlib import Path
import shutil
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(HERE.parent / 'CoreClrRuntime'))
from evidence import Evidence, sha
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import locked_command

EXPECTED = b'guest-getcpu: main/worker affinity, libc, nullable outputs, width, cache: PASS\n'
FOCUSED = b'guest-getcpu-boundary: nullable outputs and exact 32-bit guest writes: PASS\n'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--native-closure', type=Path, default=ROOT / 'artifacts/coreclr-runtime/native/attempt-h40bq9ta')
    args = parser.parse_args()
    ev = Evidence('getcpu')
    ev.value.update(scope=__doc__, max_build_jobs=1, managed_passed=False,
        prior_coordinator_stop='Prior coordinator reported automated content filter possible cyber risk; safeguards neither disabled nor circumvented')

    def run(label, command, expected, timeout=60):
        out, err = ev.run(label, locked_command(list(map(str, command)), env=ev.environment), timeout=timeout)
        if out != expected or err:
            raise RuntimeError(label + ': output differs')

    try:
        for path in (Path(__file__), HERE / 'getcpu.c', HERE / 'getcpu-focused.c', HERE / 'README.md'):
            ev.capture(path, ev.directory / 'inputs' / path.name)
        ev.pin(REPO / 'Scripts/production_support.py')
        ev.pin(HERE.parent / 'CoreClrRuntime/evidence.py')
        for name in ('cpu_bindings.py', 'stage-host-bindings.py', 'stage-threaded-core.py'):
            path = ROOT / 'scripts' / name
            ast.parse(path.read_text())
            ev.capture(path, ev.directory / 'inputs' / name)
        helper = ev.directory / 'inputs/cpu_bindings.py'
        spec = importlib.util.spec_from_file_location('frozen_cpu_bindings', helper)
        bindings = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(bindings)
        original = ev.pin(ROOT / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/syscall.c').read_bytes()
        adapted = bindings.adapt('blink/syscall.c', original)
        if adapted.count(b'SYSCALL(3, 0x135, "getcpu", SysGetcpu, STRACE_3);') != 1:
            raise RuntimeError('Missing unique syscall dispatch')
        start = adapted.index(b'static int SysGetcpu(')
        end = adapted.index(b'\n}', start) + 3
        frozen = ev.directory / 'inputs/getcpu-handler.inc'
        frozen.write_bytes(adapted[start:end]); ev.pin(frozen)
        ev.value['adaptation'] = dict(original_sha256=sha(ROOT / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/syscall.c'),
            staged_sha256=__import__('hashlib').sha256(adapted).hexdigest(), dispatch=True,
            scope='Actual adapted handler compiled with a bounded guest-memory boundary; no generated integration')
        cc = ev.pin(shutil.which('cc'))
        image = ev.directory / 'guest-getcpu.elf'
        ev.run('fixture-build', locked_command([str(cc), '-std=c17', '-O1', '-Wall', '-Wextra', '-Werror', '-pthread',
            str(ev.directory / 'inputs/getcpu.c'), '-o', str(image)], env=ev.environment))
        ev.pin(image)
        images = args.native_closure.resolve() / 'images'
        loader = ev.pin(images / 'lib64/ld-linux-x86-64.so.2')
        libc = ev.pin(images / 'lib/x86_64-linux-gnu/libc.so.6')
        trace = ev.directory / 'linux.trace'
        run('linux', [ev.pin(shutil.which('strace')), '-f', '-s', '160', '-o', trace,
            loader, '--library-path', libc.parent, image], EXPECTED, timeout=30)
        ev.pin(trace)
        if trace.read_text().count('getcpu(') < 8:
            raise RuntimeError('Native trace lacks main/worker raw getcpu calls')
        closure = dict(Images=str(ev.directory), Environment={}, Files=[dict(
            GuestPath='/apps/guest-getcpu.elf', File=image.name, Sha256=sha(image), Executable=True)])
        for guest, path in (('/lib64/ld-linux-x86-64.so.2', loader), ('/lib/x86_64-linux-gnu/libc.so.6', libc)):
            target = ev.directory / guest.removeprefix('/')
            ev.capture(path, target)
            closure['Files'].append(dict(GuestPath=guest, File=target.relative_to(ev.directory).as_posix(),
                Sha256=sha(target), Executable=guest.startswith('/lib64/')))
        closure_path = ev.directory / 'closure.json'
        closure_path.write_text(json.dumps(closure, indent=2) + '\n'); ev.pin(closure_path)
        ev.value['fixture'] = dict(path=str(image), sha256=sha(image), closure=str(closure_path),
            expected_stdout=EXPECTED.decode(), native_passed=True, fixture_ready=True)
        ev.save()
        print('FIXTURE_READY ' + str(closure_path), flush=True)
        focused = ev.directory / 'getcpu-focused'
        ev.run('focused-build', locked_command([str(cc), '-std=c17', '-O1', '-Wall', '-Wextra', '-Werror',
            str(ev.directory / 'inputs/getcpu-focused.c'), '-o', str(focused)], env=ev.environment))
        ev.pin(focused)
        run('focused', [focused], FOCUSED)
        ev.value['focused_passed'] = True
        ev.check()
        ev.value['passed'] = True
    except BaseException as error:
        ev.value['error'] = type(error).__name__ + ': ' + str(error)
    return ev.finish()


if __name__ == '__main__':
    raise SystemExit(main())
