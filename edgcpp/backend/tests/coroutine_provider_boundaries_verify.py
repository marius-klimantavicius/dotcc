"""Fail closed for missing allocation providers and unmapped function-try scope."""
from pathlib import Path
import shutil
import sys


def verify_coroutine_provider_boundaries(root, tests, out, nativeaot, run, acquisition=None):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    checks = []
    for fixture in ('coroutine_function_try', 'coroutine_allocator_provider'):
        source = out / (fixture + '.cpp')
        shutil.copy2(tests / 'libcxx' / (fixture + '.cpp'), source)
        for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
            for optimization in ('-O0', '-O2'):
                executable = out / (fixture + '-' + label + optimization)
                run([compiler, '-std=c++20', optimization, source, '-o', executable])
                run([executable])
        checks.append(fixture + ': native GCC/Clang O0/O2 agrees')
    cases = [
        ('function-try', 'coroutine_function_try', '', 'coroutine function-try source-body boundary'),
        ('allocator', 'coroutine_allocator_provider', '#define OMIT_ALLOCATOR\n',
         'coroutine selected allocator requires source definition'),
        ('deallocator', 'coroutine_allocator_provider', '#define OMIT_DEALLOCATOR\n',
         'coroutine selected deallocator requires source definition'),
    ]
    for label, fixture, prefix, diagnostic in cases:
        source = out / (label + '.cpp')
        source.write_text(prefix + (tests / 'libcxx' / (fixture + '.cpp')).read_text())
        for instance in (False, True):
            mode = 'instance' if instance else 'static'
            directory = out / (label + '-' + mode)
            result = run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
                          '-I', acquisition / 'config', '-I', acquisition / 'libcxx-18.1.8.src/include',
                          '--output-dir', directory, *(['--instance-methods'] if instance else [])], expected=1)
            assert diagnostic in result.stderr, result.stderr
            assert not list(directory.glob('Translated*.cs')), 'unsupported boundary published generated C#'
            checks.append(label + '-' + mode + ': precise prepublication diagnostic')
    return checks
