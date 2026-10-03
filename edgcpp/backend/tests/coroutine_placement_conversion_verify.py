"""Placement conversions require a retained semantic conversion expression."""
from pathlib import Path
import shutil
import sys


def verify_coroutine_placement_conversion(root, tests, out, nativeaot, run, acquisition=None):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_placement_conversion.cpp', source)
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / (label + optimization)
            run([compiler, '-std=c++20', optimization, source, '-o', executable])
            run([executable])
    checks = ['native GCC/Clang O0/O2: valid class-to-int placement conversion with observable side effect']
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        result = run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
                      '-I', acquisition / 'config', '-I', acquisition / 'libcxx-18.1.8.src/include',
                      '--output-dir', directory, *(['--instance-methods'] if instance else [])], expected=1)
        assert 'coroutine placement value conversion' in result.stderr, result.stderr
        assert not list(directory.glob('Translated*.cs')), 'unsupported placement conversion published generated source'
        checks.append(mode + ': rejects unsupported placement conversion before publishing C#')
    return checks
