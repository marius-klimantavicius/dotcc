#!/usr/bin/env python3
"""Keep C++17 overloaded assignment sequencing when emitted as a direct call."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'operator_call_order.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_operator_call_order.cpp'), source)
    commands = []

    def run(name, command):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, text=True, timeout=300,
            env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ.get('PATH', '')))
        (out / (name + '.stdout')).write_text(result.stdout)
        (out / (name + '.stderr')).write_text(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        if result.returncode: raise RuntimeError(name)

    error = None
    try:
        for opt in ('0', '2'):
            run('native-build-' + opt, ['c++', '-std=c++17', '-O' + opt, source, '-o', out / ('native' + opt)])
            run('native-run-' + opt, [out / ('native' + opt)])
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--instantiate', 'used', '--no_inlining',
            '--exceptions', '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / 'stock.c',
            '-o', out / 'stock.il', source])
        for opt in ('0', '2'):
            run('stock-build-' + opt, ['gcc', '-w', '-O' + opt, '-falign-functions=4', out / 'stock.c',
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', out / ('stock' + opt)])
            run('stock-run-' + opt, [out / ('stock' + opt)])
        obj = out / 'unit.dotobj'
        run('export', ['python3', '-B', args.packager.resolve(), source, '--output', obj,
            '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(source.with_suffix('.retained.cpp'))
        run('verify-hidden-source', ['dotnet', args.linker.resolve(), 'verify', obj])
        generated = out / 'generated'
        run('link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', generated, obj])
        run('build', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '-v:q'])
        run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '-v:q'])
            run('nativeaot', [out / 'aot/Translated'])
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(commands), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
