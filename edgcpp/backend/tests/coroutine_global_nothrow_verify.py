"""Selected nothrow allocator and allocation-failure return object."""
import hashlib
import json
from pathlib import Path
import shutil
import sys


def verify_coroutine_global_nothrow(root, tests, out, nativeaot, run, acquisition=None):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    library = acquisition / 'libcxx-18.1.8.src'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_global_nothrow.cpp', source)
    checks = []
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / (label + optimization)
            run([compiler, '-std=c++20', optimization, source, '-o', executable])
            run([executable])
    checks.append('native GCC/Clang O0/O2: canonical global std::nothrow identity, allocation failure and ordinary lifetime')
    # Use the pinned library's actual source definition of std::nothrow.
    provider = library / 'src/new_helpers.cpp'
    combined = out / 'combined.cpp'
    providers = [root / 'backend/runtime/libcxx/core-source-preamble.h', root / 'backend/runtime/libcxx/abi_core.cpp', provider, library / 'src/typeinfo.cpp']
    combined.write_text(''.join('#include ' + json.dumps(str(path.resolve())) + '\n' for path in [*providers, source]))
    (out / 'provider.json').write_text(json.dumps([{'path': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()} for path in providers], indent=2))
    missing = out / 'missing-provider'
    result = run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
                  '-I', acquisition / 'config', '-I', library / 'include', '--output-dir', missing], expected=1)
    assert 'global coroutine nothrow allocation requires source definition of std::nothrow' in result.stderr, result.stderr
    assert not list(missing.glob('Translated*.cs'))
    checks.append('missing canonical source object diagnosed before C# publication')
    for variant in ('identity', 'descriptor-only'):
        for instance in (False, True):
            mode = 'instance' if instance else 'static'
            directory = out / (variant + '-' + mode)
            run([sys.executable, root / 'dotnet.py', combined, '--std', 'c++20', '--libc', 'dotcc',
                 '-I', acquisition / 'config', '-I', library / 'include',
                 '--output-dir', directory, '--namespace', '', '--class-name', 'Core',
                 '--split', 'size', '--split-size', '3200',
                 *(['-D', 'EDG_TEST_UNUSED_NOTHROW'] if variant == 'descriptor-only' else []),
                 *(['--instance-methods'] if instance else [])])
            manifest = json.loads((directory / 'dot-gen.json').read_text())
            assert 'Coroutine' in manifest['runtimeHelpers'] and (directory / 'Runtime.Coroutine.cs').exists(), manifest
            project = directory / 'Generated.csproj'
            run(['dotnet', 'build', project, '-c', 'Release', '-p:TreatWarningsAsErrors=true'])
            run(['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])
            checks.append(variant + '-' + mode + '-jit: global nothrow tag identity, failure hook and normal lifetime')
            if nativeaot:
                run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                     '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
                run([directory / 'aot/Generated'])
                checks.append(variant + '-' + mode + '-aot: global nothrow tag identity, failure hook and normal lifetime')
    return checks
