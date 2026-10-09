#!/usr/bin/env python3
"""Compare pending owned source exceptions with native and stock-C execution."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--stock-compiler', type=Path, required=True)
    parser.add_argument('--stock-runtime', type=Path, required=True)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--packager', type=Path)
    parser.add_argument('--linker', type=Path)
    parser.add_argument('--oracle-only', action='store_true')
    parser.add_argument('--generation-only', action='store_true')
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    if not args.oracle_only and not all((args.compiler, args.packager, args.linker)):
        parser.error('owned verification requires --compiler, --packager and --linker')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    source = out / 'cpp_source_eh.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    commands, checks = [], []

    def run(name, command):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        if result.returncode:
            raise RuntimeError(name + ' failed')
        checks.append(name)

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, '-o', binary])
            run('native-run-O' + optimization, [binary])
        run('stock-export', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', source])
        for optimization in ('0', '2'):
            binary = out / ('stock-O' + optimization)
            run('stock-build-O' + optimization, ['cc', '-x', 'c', '-O' + optimization,
                out / 'cpp_source_eh.int.c', '-x', 'none', args.stock_runtime.resolve(), '-lm', '-o', binary])
            run('stock-run-O' + optimization, [binary])
        if not args.oracle_only:
            tooling = out / 'tooling'
            tooling.mkdir()
            for name in ('compile.py', 'schema.sql'):
                shutil.copy2(args.packager.resolve().parent / name, tooling / name)
            obj = out / 'source-eh.dotobj'
            run('export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
            run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
            source.rename(out / 'retained-source.cpp')
            generated = out / 'generated'
            run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
                '--namespace', 'Tests.SourceEh', '--class', 'Core', '--entry', 'main', obj])
            if not args.generation_only:
                project = generated / 'Translated.csproj'
                run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
                run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
                if args.nativeaot:
                    run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                        '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
                    run('nativeaot', [out / 'publish/Translated'])
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'native-stock-oracle-only' if args.oracle_only else
                   'generation-only' if args.generation_only else 'passed', error=error,
                   checks=checks, commands=commands,
                   limitations=['Oracle-only mode does not establish owned export, generated C# or runtime support.',
                                'No catch-parameter copy failure, virtual-base matching, thread transfer or terminate-order claim.'])
    for name in ('stock_compiler', 'stock_runtime', 'compiler', 'packager', 'linker'):
        value = getattr(args, name)
        if value:
            path = value.resolve()
            receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
