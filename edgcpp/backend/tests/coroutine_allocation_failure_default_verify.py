"""Retained allocation-failure calls must not invent discarded default arguments."""
from pathlib import Path
import shutil
import sys


def verify_coroutine_allocation_failure_default(root, tests, out, nativeaot, run, acquisition=None):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_allocation_failure_default.cpp', source)
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / (label + optimization)
            run([compiler, '-std=c++20', optimization, source, '-o', executable])
            run([executable])
    checks = ['native GCC/Clang O0/O2: valid default-argument allocation failure hook with side effects and throw']
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        result = run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
                      '-I', acquisition / 'config', '-I', acquisition / 'libcxx-18.1.8.src/include',
                      '--output-dir', directory, *(['--instance-methods'] if instance else [])], expected=1)
        assert 'coroutine allocation-failure descriptor requires default-argument materialization' in result.stderr, result.stderr
        assert not list(directory.glob('Translated*.cs')), 'unsupported default argument published generated source'
        checks.append(mode + ': rejects missing descriptor actual before publishing C#')
    return checks
