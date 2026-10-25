#!/usr/bin/env python3
"""Isolated native Linux/original Blink/corrected Blink syscall comparisons.

Never builds or generates the shared managed product. The managed runner needs
a separately produced current delivery; see README.md.
"""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import tempfile
import time

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sys.dont_write_bytecode = True
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()


def main():
    base = ROOT / 'artifacts/guest-memory-futex'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    print(attempt, flush=True)
    receipt = dict(passed=False, scope='native Linux and isolated native Blink only',
                   inputs={}, commands=[], managed_execution=False)
    env = dict(os.environ, LC_ALL='C', CC='gcc', AR='ar')
    for key in ('CFLAGS', 'CPPFLAGS', 'LDFLAGS', 'MAKEFLAGS', 'MODE', 'm'):
        env.pop(key, None)

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

    def pin(path):
        path = Path(path).resolve()
        digest = sha(path)
        if receipt['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Input changed: ' + str(path))
        return path

    def run(name, args, cwd=attempt, expected=0, timeout=60):
        row = dict(name=name, command=list(map(str, args)), cwd=str(cwd), expected=expected)
        receipt['commands'].append(row)
        start = time.monotonic()
        with (attempt / (name + '.log')).open('wb') as output:
            result = subprocess.run(row['command'], cwd=cwd, env=env,
                                    stdout=output, stderr=subprocess.STDOUT, timeout=timeout)
        row.update(exit_code=result.returncode, seconds=time.monotonic() - start)
        save()
        if result.returncode != expected:
            raise RuntimeError(f'{name}: exit {result.returncode}, expected {expected}; see {attempt / (name + ".log")}')
        print(name + ': passed', flush=True)

    try:
        manifest = json.loads(pin(ROOT / 'config/source-manifest.json').read_text())
        upstream = manifest['upstream']
        archive = pin(ROOT / 'ref' / upstream['archive'])
        if sha(archive) != upstream['sha256']:
            raise RuntimeError('Archive identity differs')
        stage_path = pin(ROOT / 'src/UpstreamGuestThreads/stage.py')
        spec = importlib.util.spec_from_file_location('memory_futex_stage', stage_path)
        stage = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(stage)
        for name in stage.AUTHORED:
            pin(stage_path.parent / name)
        pin(__file__)
        for tool in ('cc', 'gcc', 'as', 'ld', 'make'):
            pin(shutil.which(tool))
        pin(HERE.parent / 'GuestThreads/fixture.ld')
        (attempt / 'advice-data').write_bytes(bytes([99]) * 4096)
        pin(attempt / 'advice-data')
        for name in ('memory', 'shared', 'futex'):
            pin(HERE / (name + '.S'))
            for profile in (False, True):
                label = name + ('-profile' if profile else '')
                run(label + '-assemble', ['cc', '-c', '-nostdlib', *(['-DPROFILE'] if profile else []),
                                          HERE / (name + '.S'), '-o', attempt / (label + '.o')])
                run(label + '-link', ['ld', '-static', '-z', 'noexecstack', '-z', 'max-page-size=4096',
                                     '-T', HERE.parent / 'GuestThreads/fixture.ld',
                                     attempt / (label + '.o'), '-o', attempt / label])
                pin(attempt / label)
            run(name + '-linux', [attempt / name])
        native = attempt / 'native'
        native.mkdir()
        with tarfile.open(archive) as stream:
            stream.extractall(native, filter='data')
        source = native / upstream['directory']
        args = manifest['nativeProfile']['configureArguments'].copy()
        args[args.index('--disable-threads')] = '--enable-threads'
        run('configure', [source / 'configure', *args], cwd=source, timeout=180)
        target = manifest['nativeProfile']['makeTarget']
        run('build-original', ['make', '-j2', target], cwd=source, timeout=900)
        original = attempt / 'blink-original'
        shutil.copy2(source / 'o/blink/blink', original)
        pin(original)
        for name in ('memory', 'futex'):
            run(name + '-original-failure', [original, '-jm', attempt / name], expected=42)
        syscall, memory = stage.adapt_memory_and_futex((source / 'blink/syscall.c').read_text(),
                                                       (source / 'blink/memorymalloc.c').read_text())
        for name, data in (('syscall.c', syscall), ('memorymalloc.c', memory)):
            (source / 'blink' / name).write_text(data)
            pin(source / 'blink' / name)
        run('build-corrected', ['make', '-j2', target], cwd=source, timeout=900)
        corrected = pin(source / 'o/blink/blink')
        for name in ('memory-profile', 'shared-profile'):
            run(name + '-corrected', [corrected, '-jm', attempt / name])
        for index in range(10):
            run(f'futex-corrected-{index}', [corrected, '-jm', attempt / 'futex-profile'])
        # Existing normal TLS/clear-TID fixture also runs with corrected futexes.
        pin(HERE.parent / 'GuestThreads/fixture.S')
        run('threads-assemble', ['cc', '-c', '-nostdlib', HERE.parent / 'GuestThreads/fixture.S',
                                 '-o', attempt / 'threads.o'])
        run('threads-link', ['ld', '-static', '-z', 'noexecstack', '-T', HERE.parent / 'GuestThreads/fixture.ld',
                            attempt / 'threads.o', '-o', attempt / 'threads'])
        pin(attempt / 'threads')
        run('threads-linux', [attempt / 'threads'])
        run('threads-corrected', [corrected, '-jm', attempt / 'threads'])
        if any(sha(path) != digest for path, digest in receipt['inputs'].items()):
            raise RuntimeError('Frozen input changed during test')
        receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        receipt['completed'] = True
        save()
    print('receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
