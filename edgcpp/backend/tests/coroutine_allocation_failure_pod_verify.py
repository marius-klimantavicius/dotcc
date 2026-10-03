"""Selected nothrow allocator and allocation-failure return object."""
import json
from pathlib import Path
import shutil
import sys


def verify_coroutine_allocation_failure_pod(root, tests, out, nativeaot, run, acquisition=None):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    library = acquisition / 'libcxx-18.1.8.src'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_allocation_failure_pod.cpp', source)
    checks = []
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / (label + optimization)
            run([compiler, '-std=c++20', optimization, source, '-o', executable])
            run([executable])
    checks.append('native GCC/Clang O0/O2: POD failure result, throwing hook and ordinary lifetime')
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
             '-I', acquisition / 'config', '-I', library / 'include',
             '--output-dir', directory, '--namespace', '', '--class-name', 'Core',
             '--split', 'size', '--split-size', '3200',
             *(['--instance-methods'] if instance else [])])
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        assert 'Coroutine' in manifest['runtimeHelpers'] and (directory / 'Runtime.Coroutine.cs').exists(), manifest
        project = directory / 'Generated.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])
        checks.append(mode + '-jit: POD failure result/throw and normal lifetime')
        if nativeaot:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
            run([directory / 'aot/Generated'])
            checks.append(mode + '-aot: POD failure result/throw and normal lifetime')
    return checks
