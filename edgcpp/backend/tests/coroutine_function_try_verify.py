"""Bounded function-try view preserves coroutine implicit/source handler order."""
import json
from pathlib import Path
import shutil
import sys


def verify_coroutine_function_try(root, tests, out, nativeaot, run, acquisition=None, protocol=True):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_function_try_protocol.cpp', source)
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    checks = []
    if protocol:
        expected = None
        for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
            for optimization in ('-O0', '-O2'):
                executable = out / (label + optimization)
                run([compiler, '-std=c++20', optimization, source, '-o', executable])
                traces = [run([executable, str(mode)]).stdout for mode in range(11)]
                if expected is None:
                    expected = traces
                assert traces == expected, (compiler, optimization, traces, expected)
        checks = ['native GCC/Clang O0/O2: eleven exact source/implicit handler and suspension traces']
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
                checks.append(mode_name + '-jit-' + str(mode) + ': exact source/implicit handler trace')
            if nativeaot:
                run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                     '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
                for mode, trace in enumerate(expected):
                    actual = run([directory / 'aot/Generated', str(mode)]).stdout
                    assert actual == trace, (mode_name, mode, actual, trace)
                    checks.append(mode_name + '-aot-' + str(mode) + ': exact source/implicit handler trace')
    (out / 'positive-results.json').write_text(json.dumps({'status': 'passed', 'checks': checks}, indent=2))
    cleanup = out / 'cleanup.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_function_try_cleanup.cpp', cleanup)
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / ('cleanup-' + label + optimization)
            run([compiler, '-std=c++20', optimization, cleanup, '-o', executable])
            run([executable])
    checks.append('native GCC/Clang O0/O2: valid nontrivial handler-local cleanup')
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / ('cleanup-' + mode)
        result = run([sys.executable, root / 'dotnet.py', cleanup, '--std', 'c++20', '--libc', 'dotcc',
                      '-I', acquisition / 'config', '-I', acquisition / 'libcxx-18.1.8.src/include',
                      '--output-dir', directory, *(['--instance-methods'] if instance else [])], expected=1)
        assert 'coroutine function-try nontrivial cleanup boundary' in result.stderr, result.stderr
        assert not list(directory.glob('Translated*.cs'))
        checks.append(mode + ': nontrivial cleanup boundary rejected before C# publication')
    return checks
