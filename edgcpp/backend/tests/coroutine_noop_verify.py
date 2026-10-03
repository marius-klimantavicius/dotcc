"""Source noop handles, erased observers, and symmetric coroutine transfer."""
import json
from pathlib import Path
import shutil
import sys


def verify_coroutine_noop(root, tests, out, nativeaot, run, acquisition=None):
    if acquisition is None or not (acquisition / 'config/__coroutine/noop_coroutine_handle.h').is_file():
        raise RuntimeError('Noop source acceptance requires a fresh managed-overlay libc++ acquisition')
    library = acquisition / 'libcxx-18.1.8.src'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_noop.cpp', source)
    checks = []
    # Legacy GNU libc++ constructs a native-only frame. Reject it before any
    # generated handle operation could inspect that incompatible layout.
    legacy = root / 'artifacts/campaign/libcxx-acquired-003'
    negative = out / 'legacy-noop-negative'
    result = run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
                  '-I', legacy / 'config', '-I', legacy / 'libcxx-18.1.8.src/include',
                  '--output-dir', negative], expected=1)
    assert 'native libc++ noop coroutine frame requires managed ABI configuration overlay' in result.stderr, result.stderr
    assert not list(negative.glob('Translated*.cs')), 'unsupported native frame published generated source'
    checks.append('rejects immutable legacy native noop frame before C# publication')
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / (label + optimization)
            run([compiler, '-std=c++20', optimization, source, '-o', executable])
            run([executable])
    checks.append('native GCC/Clang O0/O2: noop promise, erased resume/destroy and symmetric transfer')
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
        assert any('Coroutine.Noop()' in file.read_text() for file in directory.glob('Translated*.cs')), 'libc++ noop handle selected unsupported native-layout fallback'
        project = directory / 'Generated.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])
        checks.append(mode + '-jit: noop identity/promise and handle-returning await_suspend')
        if nativeaot:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
            run([directory / 'aot/Generated'])
            checks.append(mode + '-aot: noop and symmetric transfer')
    return checks
