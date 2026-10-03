"""Exact source catch-parameter lifetime across coroutine handlers."""
from pathlib import Path
import shutil
import sys


def verify_coroutine_function_try_parameter(root, tests, out, nativeaot, run, acquisition=None):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_function_try_parameter.cpp', source)
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    expected = None
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / (label + optimization)
            run([compiler, '-std=c++20', optimization, source, '-o', executable])
            traces = [run([executable, str(mode)]).stdout for mode in range(2)]
            if expected is None:
                expected = traces
            assert traces == expected, (compiler, optimization, traces, expected)
    checks = ['native GCC/Clang O0/O2: catch-copy/primary-payload destruction on rethrow and replacement']
    for instance in (False, True):
        mode_name = 'instance' if instance else 'static'
        directory = out / mode_name
        run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
             '-I', acquisition / 'config', '-I', acquisition / 'libcxx-18.1.8.src/include',
             '--output-dir', directory, '--split', 'size', '--split-size', '3200',
             *(['--instance-methods'] if instance else [])])
        project = directory / 'Generated.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '-p:TreatWarningsAsErrors=true'])
        for mode, trace in enumerate(expected):
            actual = run(['dotnet', directory / 'bin/Release/net10.0/Generated.dll', str(mode)]).stdout
            assert actual == trace, (mode_name, mode, actual, trace)
            checks.append(mode_name + '-jit-' + str(mode) + ': exact catch-parameter lifetime')
        if nativeaot:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
            for mode, trace in enumerate(expected):
                actual = run([directory / 'aot/Generated', str(mode)]).stdout
                assert actual == trace, (mode_name, mode, actual, trace)
                checks.append(mode_name + '-aot-' + str(mode) + ': exact catch-parameter lifetime')
    return checks
