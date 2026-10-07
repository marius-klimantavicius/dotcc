#!/usr/bin/env python3
"""Native semaphore oracle and BCL/translated semaphore JIT/NativeAOT checks."""
import argparse
import hashlib
import json
import platform
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--runtime-only', action='store_true', help='Check BCL semantics while coordinated compiler build is pending')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'commands': [], 'checks': [], 'inputs': []}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def record(path):
        path = path.resolve()
        with path.open('rb') as stream:
            report['inputs'].append({'path': str(path), 'sha256': hashlib.file_digest(stream, 'sha256').hexdigest()})

    def run(label, command, success=True):
        command = list(map(str, command))
        entry = {'label': label, 'command': command}
        report['commands'].append(entry)
        save()
        result = subprocess.run(command, capture_output=True, text=True, timeout=900)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        entry['exitCode'] = result.returncode
        save()
        if (result.returncode == 0) != success:
            raise RuntimeError(label + ' failed: ' + (result.stdout + result.stderr)[-6000:])
        return result.stdout + result.stderr

    os_name = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}[platform.system()]
    rid = os_name + ('-arm64' if platform.machine().lower() in ('arm64', 'aarch64') else '-x64')
    try:
        for path in [Path(__file__), TESTS / 'posix_semaphores.c', TESTS / 'posix_semaphores_runtime.cs',
                     ROOT / 'backend/runtime/PosixSemaphores.cs', ROOT / 'backend/dot_gen_semaphore.h',
                     ROOT / 'backend/runtime/posix/include/semaphore.h']:
            record(path)
        runtime = out / 'runtime'
        runtime.mkdir()
        for path in [*sorted((ROOT / 'backend/runtime').glob('Posix*.cs')),
                     ROOT / 'backend/runtime/LibcFileDescriptors.cs', ROOT / 'backend/runtime/VaArg.cs',
                     TESTS / 'posix_semaphores_runtime.cs']:
            shutil.copy2(path, runtime / path.name)
        project = runtime / 'SemaphoreRuntime.csproj'
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
            '<TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>'
            '<Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>'
            '<IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>\n')
        run('runtime-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo'])
        observed = run('runtime-jit', ['dotnet', runtime / 'bin/Release/net10.0/SemaphoreRuntime.dll'])
        if 'checks passed' not in observed:
            raise RuntimeError('Runtime controls did not produce a completion receipt')
        report['checks'].append({'name': 'runtime-jit', 'output': observed.strip()})
        if args.nativeaot:
            run('runtime-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', rid,
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', runtime / 'aot'])
            actual = run('runtime-aot', [runtime / 'aot' / ('SemaphoreRuntime.exe' if os_name == 'win' else 'SemaphoreRuntime')])
            if actual != observed:
                raise RuntimeError('Runtime JIT/AOT observations differ')
            report['checks'].append({'name': 'runtime-aot', 'output': actual.strip()})
        if args.runtime_only:
            report['status'] = 'runtime-only-passed'
            return
        record(args.compiler)
        source = TESTS / 'posix_semaphores.c'
        native = out / 'native'
        run('native-build', ['cc', '-std=c11', '-O2', '-Wall', '-Wextra', '-Werror', '-pthread', source, '-o', native])
        expected = run('native-run', [native])
        report['checks'].append({'name': 'native', 'output': expected.strip()})
        for mode in ('static', 'instance'):
            generated = out / mode
            run(mode + '-translate', [sys.executable, ROOT / 'dotnet.py', source,
                '--compiler', args.compiler.resolve(), '--language', 'c', '--std', 'c11',
                '--target', 'portable-lp64-binary80-v2', '--libc', 'musl', '--posix',
                '--unbound-externals', 'reject', '--namespace', 'Semaphore.Tests', '--class-name', 'Core',
                *(['--instance-methods'] if mode == 'instance' else []), '--output-dir', generated])
            metadata = json.loads((generated / 'dot-gen.json').read_text())
            if metadata['unboundExternals']['symbols']:
                raise RuntimeError('Semaphore fixture contains unbound externals')
            project = generated / 'Generated.csproj'
            run(mode + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
            actual = run(mode + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
            if actual != expected:
                raise RuntimeError(mode + ' translated/native observations differ')
            report['checks'].append({'name': mode + '-jit', 'output': actual.strip()})
            if args.nativeaot:
                run(mode + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', rid,
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true',
                    '-o', generated / 'aot'])
                actual = run(mode + '-aot', [generated / 'aot' / ('Generated.exe' if os_name == 'win' else 'Generated')])
                if actual != expected:
                    raise RuntimeError(mode + ' translated AOT/native observations differ')
                report['checks'].append({'name': mode + '-aot', 'output': actual.strip()})
        bad = out / 'bad.c'
        bad.write_text('struct __edg_posix_semaphore { unsigned long __token, wrong; };\n'
            'int sem_init(struct __edg_posix_semaphore *, int, unsigned int);\n'
            'int main(void) { struct __edg_posix_semaphore s; return sem_init(&s, 0, 0); }\n')
        failed = run('bad-layout', [sys.executable, ROOT / 'dotnet.py', bad, '--compiler', args.compiler.resolve(),
            '--language', 'c', '--std', 'c11', '--libc', 'musl', '--posix', '--output-dir', out / 'bad-layout'], success=False)
        if 'POSIX import ABI for sem_init' not in failed:
            raise RuntimeError('Mismatched semaphore ABI was not explicitly rejected')
        report['checks'].append({'name': 'bad-layout-rejected'})
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
