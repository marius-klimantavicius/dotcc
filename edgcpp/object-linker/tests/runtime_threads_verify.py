#!/usr/bin/env python3
"""Exercise owned C11 thread objects against native C and shared BCL services."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
EDG = ROOT.parent


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--packager', type=Path, default=ROOT / 'compile.py')
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands, inputs = [], [], []

    def check(name, condition):
        checks.append(dict(name=name, passed=bool(condition)))
        if not condition:
            raise AssertionError(name)

    def run(name, command, *, input=None, expected=0):
        command = list(map(str, command))
        p = subprocess.run(command, cwd=out, input=input, capture_output=True, timeout=300)
        stdout, stderr = out / (name + '.stdout'), out / (name + '.stderr')
        stdout.write_bytes(p.stdout)
        stderr.write_bytes(p.stderr)
        commands.append(dict(name=name, command=command, exitCode=p.returncode, stdin=None if input is None else input.decode(),
            stdout=dict(path=str(stdout), sha256=sha(stdout)), stderr=dict(path=str(stderr), sha256=sha(stderr))))
        check(name, p.returncode == expected)
        print(name + ': passed', flush=True)
        return p

    def compile(name, source):
        obj = out / (name + '.dotobj')
        run(name + '-compile', [sys.executable, '-B', args.packager.resolve(), source, '--output', obj,
            '--work-dir', out / (name + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c11',
            '-I', EDG / 'backend/runtime/posix/include', '-I', EDG / 'backend/runtime/libc/include',
            '-I', EDG / 'backend/runtime/libcxx/config'])
        return obj

    def link(name, objects):
        project = out / (name + '-generated')
        run(name + '-link', ['dotnet', args.linker.resolve(), 'link', '--out', project, '--entry', 'main', '--allow-externs',
            *objects, '--provider', provider])
        run(name + '-bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', project])
        receipt = json.loads((project / 'runtime-bindings.json').read_text())
        check(name + '-all-bound', receipt['status'] == 'bound' and not receipt['unresolved'])
        check(name + '-shared-thread-runtime', {'LibcxxThreads.cs', 'LibcC11Threads.cs', 'LibcInitialThread.cs'} <= {Path(row['path']).name for row in receipt['implementationInputs']})
        check(name + '-canonical-source-hashes', all(sha(row['path']) == row['sha256'] for row in receipt['implementationInputs']))
        run(name + '-build', ['dotnet', 'build', project / 'Translated.csproj', '--nologo', '-v:q', '-p:TreatWarningsAsErrors=true', '-p:NoWarn=CS8981'])
        executables = [('jit', ['dotnet', project / 'bin/Debug/net10.0/Translated.dll'])]
        if args.nativeaot:
            run(name + '-aot-build', ['dotnet', 'publish', project / 'Translated.csproj', '-r', args.rid,
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / (name + '-aot'), '--nologo', '-v:q'])
            executables.append(('aot', [out / (name + '-aot/Translated')]))
        return executables

    failure = None
    try:
        for name in ('libc_c11_threads.c', 'libc_initial_thread.cpp'):
            source = EDG / 'backend/tests' / name
            inputs.append(dict(path=str(source), sha256=sha(source)))
        fixture = out / 'fixture.c'
        shutil.copy2(EDG / 'backend/tests/libc_c11_threads.c', fixture)
        main = out / 'main.c'
        main.write_text('int c11_thread_cases(void); int c11_destructor_exit_cases(void);\n'
                        'int main(void) { int r=c11_thread_cases(); return r ? r : c11_destructor_exit_cases(); }\n')
        run('native-build', ['cc', '-std=c11', '-pthread', '-O2', fixture, main, '-o', out / 'native'])
        run('native', [out / 'native'])
        provider_source = EDG / 'backend/runtime/libc/threads/threads.c'
        inputs.append(dict(path=str(provider_source), sha256=sha(provider_source)))
        provider = compile('threads-provider', provider_source)
        fixture_obj = compile('fixture', fixture)
        for mode, command in link('c11', [fixture_obj, compile('main', main)]):
            result = run('c11-' + mode, command)
            check('c11-' + mode + '-quiet-success', result.stdout == result.stderr == b'')

        host_source = out / 'host_fixture.c'
        shutil.copy2(ROOT / 'tests/runtime_thread_host.c', host_source)
        host_obj = compile('host-fixture', host_source)
        library = out / 'library'
        run('library-link', ['dotnet', args.linker.resolve(), 'link', '--out', library, '--allow-externs', fixture_obj, host_obj, '--provider', provider])
        run('library-bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', library])
        host = out / 'host'
        host.mkdir()
        (host / 'Host.cs').write_text('''// <auto-generated />
using System;
using Core = Translated.Program;
internal static class Host {
    public static int Main() {
        Edg.Runtime.ObjectProgram.InitializeLibrary();
        if (Core.host_setup() != 0) return 1;
        Edg.Runtime.ObjectProgram.ReleaseThread();
        if (Core.host_after_release() != 0) return 2;
        if (Core.c11_thread_cases() != 0 || Core.c11_destructor_exit_cases() != 0) return 3;
        Edg.Runtime.ObjectProgram.ReleaseThread();
        Console.WriteLine("host survived: C++ TLS, TSS, libc TLS and source TLS cleanup order passed");
        return 0;
    }
}
''')
        host_project = host / 'Host.csproj'
        host_project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
            '<TargetFramework>net10.0</TargetFramework><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup>'
            '<ProjectReference Include="../library/Translated.csproj" /></ItemGroup></Project>\n')
        run('host-build', ['dotnet', 'build', host_project, '--nologo', '-v:q', '-p:TreatWarningsAsErrors=true', '-p:NoWarn=CS8981'])
        result = run('host-jit', ['dotnet', host / 'bin/Debug/net10.0/Host.dll'])
        check('host-jit-cleanup-order', b'host survived:' in result.stdout and result.stderr == b'')
        if args.nativeaot:
            run('host-aot-build', ['dotnet', 'publish', host_project, '-r', args.rid, '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'host-aot', '--nologo', '-v:q'])
            aot_result = run('host-aot', [out / 'host-aot/Host'])
            check('host-aot-cleanup-parity', (result.stdout, result.stderr) == (aot_result.stdout, aot_result.stderr))

        stale = out / 'stale-library'
        run('stale-link', ['dotnet', args.linker.resolve(), 'link', '--out', stale, '--allow-externs', host_obj, '--provider', provider])
        report = json.loads((stale / 'link-report.json').read_text())
        report['runtimeHooks'].pop('ownedThreadStorageRelease')
        (stale / 'link-report.json').write_text(json.dumps(report, indent=2) + '\n')
        rejected = run('stale-bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', stale], expected=1)
        check('stale-no-silent-tls-leak', b'require the owned source TLS release contract' in rejected.stderr and not list(stale.glob('Runtime.Threads*.cs')))

        # Only the test entry's argument transport changes. The thread joins,
        # TSS destructors and six last-source-thread cases remain the EDG fixture.
        initial = out / 'initial.c'
        text = (EDG / 'backend/tests/libc_initial_thread.cpp').read_text()
        text = text.replace('int main(int argc, char **argv) {\n  require(argc == 2); mode = argv[1][0] - \'0\';',
                            'int main(void) {\n  mode = getchar() - \'0\'; require(mode >= 0 && mode < 6);')
        check('initial-fixture-entry-adapted', 'int main(void)' in text and 'argv' not in text)
        initial.write_text(text)
        run('initial-native-build', ['cc', '-std=c11', '-pthread', '-O2', initial, '-o', out / 'initial-native'])
        variants = [('native', [out / 'initial-native']), *link('initial', [compile('initial', initial)])]
        for runtime, command in variants:
            for mode in range(6):
                result = run('initial-' + runtime + '-' + str(mode), command, input=str(mode).encode())
                expected = 'M' + ('JW' + ('C' if mode == 2 else '') if mode not in (0, 5) else '') + 'A'
                check('initial-' + runtime + '-' + str(mode) + '-order', result.stdout == b'' and result.stderr == expected.encode())

        libcpp = out / 'libcpp.c'
        shutil.copy2(ROOT / 'tests/runtime_libcpp_threads.c', libcpp)
        native_adapter = EDG / 'backend/tests/libcxx_threads_oracle.c'
        inputs.append(dict(path=str(native_adapter), sha256=sha(native_adapter)))
        run('libcpp-native-build', ['cc', '-std=c11', '-pthread', '-O2', '-I', EDG / 'backend/runtime/libcxx/config',
            libcpp, native_adapter, '-o', out / 'libcpp-native'])
        run('libcpp-native', [out / 'libcpp-native'])
        libcpp_main = out / 'libcpp_main.c'
        libcpp_main.write_text('int thread_cases(void); int main(void) { return thread_cases(); }\n')
        for mode, command in link('libcpp', [compile('libcpp', libcpp), compile('libcpp-main', libcpp_main)]):
            result = run('libcpp-' + mode, command)
            check('libcpp-' + mode + '-quiet-success', result.stdout == result.stderr == b'')

        bad = out / 'bad.c'
        bad.write_text('int __edg_c11_thread_create(unsigned long*, int (*)(int), void*); '
                       'int f(int x){return x;} int main(void){unsigned long t; return __edg_c11_thread_create(&t,f,0);}\n')
        bad_obj = compile('bad', bad)
        bad_project = out / 'bad-generated'
        run('bad-link', ['dotnet', args.linker.resolve(), 'link', '--out', bad_project, '--entry', 'main', '--allow-externs', bad_obj])
        rejected = run('bad-bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', bad_project], expected=1)
        check('bad-callback-abi-rejected', b'Thread import ABI does not match' in rejected.stderr)
        check('bad-no-provider-bodies', not list(bad_project.glob('Runtime.Bindings.*.cs')))
    except Exception as error:
        failure = str(error)
    result = dict(schema=1, status='failed' if failure else 'passed', error=failure, checks=checks, commands=commands,
        canonicalInputs=inputs, nativeAotVerified=args.nativeaot and not failure,
        compiler=dict(path=str(args.compiler.resolve()), sha256=sha(args.compiler)),
        linker=dict(path=str(args.linker.resolve()), sha256=sha(args.linker)),
        scope='C11 source TUs and canonical BCL runtime; full C++ lifetimes/independent-owner disposal not established')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=failure)))
    return 1 if failure else 0


if __name__ == '__main__':
    raise SystemExit(main())
