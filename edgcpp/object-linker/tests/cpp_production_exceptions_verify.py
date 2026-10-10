#!/usr/bin/env python3
"""Production C++ exception runtime: native traces, direct contracts and two-TU linking."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'stock-compiler', 'stock-runtime', 'packager', 'linker', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    os.environ['DOTNET_DbgEnableMiniDump'] = '0'
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    packager = tooling / 'compile.py'
    checks, commands = [], []
    here = Path(__file__).resolve().parent

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, cwd, expected=0):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=cwd, capture_output=True, timeout=900)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == expected if expected is not None else result.returncode != 0)
        return result

    fixtures = [
        ('typed', 'TYPED', ['cpp_typed_catch.cpp'], 'cpp_typed_catch_native.cpp', 14),
        ('function-try', 'FUNCTION_TRY', ['cpp_function_try.cpp'], 'cpp_function_try_native.cpp', 34),
        ('cross-module', 'CROSS_MODULE', ['cpp_production_cross_throw.cpp', 'cpp_production_cross_catch.cpp'],
         'cpp_production_cross_native.cpp', 4),
    ]
    error = None
    try:
        for label, define, sources, native_name, count in fixtures:
            work = out / label
            inputs = work / 'inputs'
            inputs.mkdir(parents=True)
            names = sources + [native_name, 'ProductionExceptionHost.cs', 'ProductionRuntimeTests.cs']
            if label == 'cross-module': names.append('cpp_production_cross.h')
            for name in names: shutil.copy2(here / name, inputs / name)
            native_inputs = [inputs / native_name]
            if label == 'cross-module': native_inputs += [inputs / name for name in sources]
            oracle = None
            for opt in ('0', '2'):
                binary = work / ('native-O' + opt)
                run(label + '-native-build-O' + opt, ['c++', '-std=c++17', '-O' + opt, *native_inputs, '-o', binary], work)
                result = run(label + '-native-run-O' + opt, [binary], work)
                oracle = oracle or result.stdout
                check(label + '-native-parity-O' + opt, result.stdout == oracle and not result.stderr)
            check(label + '-native-scenario-count', len(oracle.splitlines()) == count)
            stock_sources = []
            for i, source in enumerate(native_inputs):
                run(label + '-stock-lower-' + str(i), [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                    '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', source], work)
                stock_sources.append(work / (source.stem + '.int.c'))
            for opt in ('0', '2'):
                binary = work / ('stock-O' + opt)
                run(label + '-stock-build-O' + opt, ['cc', '-x', 'c', '-O' + opt, *stock_sources, '-x', 'none',
                    args.stock_runtime.resolve(), '-lm', '-lstdc++', '-o', binary], work)
                result = run(label + '-stock-run-O' + opt, [binary], work)
                check(label + '-stock-native-parity-O' + opt, result.stdout == oracle and not result.stderr)
            objects = []
            for i, source in enumerate(sources):
                obj = work / (str(i) + '.dotobj'); objects.append(obj)
                run(label + '-export-' + str(i), [sys.executable, '-B', packager, inputs / source,
                    '--output', obj, '--work-dir', work / ('export-' + str(i)), '--compiler', args.compiler.resolve(),
                    '--exceptions', '--no-rtti'], work)
                run(label + '-verify-' + str(i), ['dotnet', args.linker.resolve(), 'verify', obj], work)
            inputs.rename(work / 'retained-source')
            generated = work / 'generated'
            run(label + '-source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
                '--namespace', 'Tests.Production', '--class', 'Core', '--allow-externs', *objects], work)
            report = json.loads((generated / 'link-report.json').read_text())
            status = report['sourceExceptionRuntime']['status']
            check(label + '-production-provider-selected', 'unbound' not in status and 'external' not in status)
            for name in ('ProductionExceptionHost.cs', 'ProductionRuntimeTests.cs'):
                shutil.copy2(work / 'retained-source' / name, generated / name)
            project = generated / 'Translated.csproj'
            project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
                '<OutputType>Exe</OutputType><StartupObject>ProductionExceptionHost</StartupObject>'
                '<DefineConstants>' + define + '</DefineConstants>'))
            run(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'], work)
            execution = [('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])]
            if args.nativeaot:
                run(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', work / 'publish', '--nologo', '-v:q'], work)
                execution.append(('nativeaot', [work / 'publish/Translated']))
            for runtime, command in execution:
                result = run(label + '-' + runtime, command, work)
                check(label + '-' + runtime + '-native-parity', result.stdout == oracle and not result.stderr)
                if label == 'typed':
                    result = run(runtime + '-provider-contracts', command + ['runtime'], work)
                    check(runtime + '-provider-contracts-completed', b'production-runtime checks=' in result.stdout and not result.stderr)
                    result = run(runtime + '-translated-catch-copy-failure', command + ['copy-failure'], work, None)
                    check(runtime + '-translated-catch-copy-terminates', b'C++ exception runtime: catch parameter initialization failed' in result.stderr)
                    for failure in ('rethrow', 'throw-init', 'catch-init', 'destructor'):
                        result = run(runtime + '-provider-failure-' + failure, command + ['runtime-failure-' + failure], work, None)
                        check(runtime + '-provider-terminates-' + failure, b'C++ exception runtime: ' in result.stderr)
        work = out / 'uncaught'
        work.mkdir()
        source = work / 'cpp_production_uncaught.cpp'
        shutil.copy2(here / source.name, source)
        obj = work / 'uncaught.dotobj'
        run('uncaught-export', [sys.executable, '-B', packager, source, '--output', obj,
            '--work-dir', work / 'export', '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'], work)
        source.rename(work / 'retained-source.cpp')
        generated = work / 'generated'
        run('uncaught-strict-link-with-provider', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
            '--entry', 'main', obj], work)
        run('external-provider-strict-link-requires-binding', ['dotnet', args.linker.resolve(), 'link', '--out',
            work / 'external-strict', '--exception-provider', 'external', obj], work, 1)
        run('external-provider-explicit-unbound-link', ['dotnet', args.linker.resolve(), 'link', '--out',
            work / 'external-unbound', '--exception-provider', 'external', '--allow-externs', obj], work)
        report = json.loads((work / 'external-unbound/link-report.json').read_text())
        check('external-provider-remains-unbound', 'unbound' in report['sourceExceptionRuntime']['status'])
        project = generated / 'Translated.csproj'
        run('uncaught-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'], work)
        commands_to_run = [('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])]
        if args.nativeaot:
            run('uncaught-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', work / 'publish', '--nologo', '-v:q'], work)
            commands_to_run.append(('nativeaot', [work / 'publish/Translated']))
        for runtime, command in commands_to_run:
            result = run(runtime + '-uncaught-termination', command, work, None)
            check(runtime + '-uncaught-program-policy', b'Uncaught C++ exception at program boundary' in result.stderr)
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
        translatedScenarios=52, productionProvider=True, nativeaot=args.nativeaot,
        limitations=['Thread test uses independent payloads and thread-local catch stacks, not std::exception_ptr thread transfer.',
                     'Source traces exercise the supported nonvirtual class hierarchy; direct descriptor tests do not prove frontend coverage.'])
    for name in ('compiler', 'stock_compiler', 'stock_runtime', 'linker', 'packager'):
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)


if __name__ == '__main__':
    raise SystemExit(main())
