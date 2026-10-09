#!/usr/bin/env python3
"""Library-free C++ dynamic initialization: native oracle and owned export gates."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--compiler', required=True, type=Path)
    parser.add_argument('--stock-compiler', required=True, type=Path)
    parser.add_argument('--linker', required=True, type=Path)
    parser.add_argument('--packager', required=True, type=Path)
    parser.add_argument('--allow-export-rejection', action='store_true')
    parser.add_argument('--generation-only', action='store_true',
                        help='Permit external runtime declarations; do not compile or run generated C#')
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    tooling = out / 'tooling'
    tooling.mkdir()
    for filename in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / filename, tooling / filename)
    checks, commands, pending, artifacts = [], [], [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, expected=0, cwd=out):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=cwd, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, cwd=str(cwd), exitCode=result.returncode))
        if expected is not None:
            check(name, result.returncode == expected)
        return result

    def artifact(path, purpose):
        artifacts.append(dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest(), purpose=purpose))

    error = None
    try:
        objects = []
        for name in ('scalar', 'array', 'local'):
            source = inputs / ('cpp_dynamic_startup_' + name + '.cpp')
            shutil.copy2(Path(__file__).parent / source.name, source)
            artifact(source, 'Independent standard-library-free fixture')
            for optimization in ('0', '2'):
                executable = out / (name + '-native-O' + optimization)
                run(name + '-native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                    '-fno-exceptions', '-fno-rtti', source, '-o', executable])
                result = run(name + '-native-run-O' + optimization, [executable])
                check(name + '-native-silent-O' + optimization, not result.stdout and not result.stderr)
            stock = out / (name + '-stock')
            stock.mkdir()
            run(name + '-stock-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used',
                '--no_exceptions', '--no_rtti', source], cwd=stock)
            lowered = source.with_suffix('.int.c')
            if not lowered.exists():
                lowered = stock / lowered.name
            check(name + '-stock-source-retained', lowered.exists())
            artifact(lowered, 'Unmodified EDG C backend initialization/guard scheduling reference')
            obj = out / (name + '.dotobj')
            work = out / (name + '-work')
            result = run(name + '-export', [sys.executable, '-B', tooling / 'compile.py', source,
                '--output', obj, '--work-dir', work, '--compiler', args.compiler.resolve(), '--std', 'c++17'],
                expected=None if args.allow_export_rejection else 0)
            if result.returncode:
                diagnostic = (work / 'stderr').read_text()
                check(name + '-explicit-owned-export-gate', 'unsupported dotobj' in diagnostic)
                pending.append(dict(unit=name, diagnostic=diagnostic))
            else:
                run(name + '-verify', ['dotnet', args.linker.resolve(), 'verify', obj])
                objects.append((name, obj))
        # Linking must depend on the database, including all startup semantics,
        # rather than re-reading the original source to rediscover initialization.
        inputs.rename(out / 'retained-source')
        for name, obj in objects:
            generated = out / (name + '-generated')
            command = ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', obj]
            if args.generation_only:
                command.append('--allow-externs')
            run(name + '-source-hidden-link', command)
            if args.generation_only:
                check(name + '-generated-source-present', any(generated.glob('Translated.*.cs')))
                continue
            run(name + '-build', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '--nologo', '-v:q'])
            result = run(name + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
            check(name + '-jit-silent', not result.stdout and not result.stderr)
            if args.nativeaot:
                published = out / (name + '-aot')
                run(name + '-publish', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release',
                    '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true',
                    '-o', published, '--nologo', '-v:q'])
                result = run(name + '-nativeaot', [published / 'Translated'])
                check(name + '-nativeaot-silent', not result.stdout and not result.stderr)
    except Exception as exception:
        error = str(exception)
    status = ('failed' if error else 'native-oracles-passed-export-pending' if pending else
              'generated-not-executed' if args.generation_only else 'passed')
    receipt = dict(status=status, error=error, checks=checks, pending=pending, artifacts=artifacts, commands=commands,
                   generatedExecutionRequested=not args.generation_only,
                   limitations=['No cross-translation-unit initialization order assertion',
                                'No thread-race, guard-abort, destructor or exception-unwind coverage',
                                'Stock C output is retained as a lowering reference, not claimed as an executed oracle'])
    for name in ('compiler', 'stock_compiler', 'linker', 'packager'):
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=status, checks=len(checks), error=error, pending=len(pending))))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
