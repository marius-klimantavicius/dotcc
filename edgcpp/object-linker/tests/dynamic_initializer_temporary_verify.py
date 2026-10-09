#!/usr/bin/env python3
"""Native/JIT/AOT parity and malformed metadata for startup temporary scaffolds."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--stock-compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--packager', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ['compile.py', 'schema.sql']:
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    source = inputs / 'cpp_dynamic_startup_temporary.cpp'
    shutil.copy2(Path(__file__).parent / source.name, source)
    checks, commands = [], []

    def check(label, condition):
        if not condition:
            raise AssertionError(label)
        checks.append(label)

    def run(label, command):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (label + '.stdout')).write_bytes(result.stdout)
        (out / (label + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=label, command=command, exitCode=result.returncode))
        check(label, result.returncode == 0)
        return result

    error = None
    try:
        for optimization in ['0', '2']:
            executable = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                '-fno-exceptions', '-fno-rtti', source, '-o', executable])
            run('native-run-O' + optimization, [executable])
        run('stock-c-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used',
            '--no_exceptions', '--no_rtti', source])
        lowered = out / source.with_suffix('.int.c').name
        if not lowered.exists():
            lowered = source.with_suffix('.int.c')
        check('stock-c-retained', lowered.exists())
        run('stock-c-build', ['cc', '-O2', lowered, '-o', out / 'stock-native'])
        run('stock-c-run', [out / 'stock-native'])
        obj = out / 'temporary.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj,
            '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            scaffolds = db.execute('SELECT entity_id,ordinal,node_id,binding_id,kind,capture_proof FROM generated_routine_scaffolds').fetchall()
            check('has-observed-declaration-scaffold', any(row[4:] == ('declaration', 'observed-scope-binding') for row in scaffolds))
            check('has-owned-temporary-binding', all(db.execute('SELECT kind,is_synthetic FROM bindings WHERE id=?', (row[3],)).fetchone() == ('local', 1) for row in scaffolds))
        inputs.rename(out / 'retained-source')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', obj])
        text = (generated / 'Translated.Startup.generated.cs').read_text()
        check('action-termination-boundary', 'Exception escaped a file-scope initializer' in text and 'IsRuntimeTransfer(error)' in text)
        run('managed-build', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '--nologo', '-v:q'])
        run('managed-jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        if args.nativeaot:
            run('nativeaot-publish', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release',
                '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '--nologo', '-v:q'])
            run('nativeaot-execute', [out / 'aot/Translated'])
        run('malformed-metadata', [sys.executable, '-B', Path(__file__).with_name('dynamic_initializer_metadata_verify.py').resolve(),
            '--object', obj, '--linker', args.linker.resolve(), '--packager', tooling / 'compile.py', '--output-dir', out / 'metadata'])
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
                   limitations=['No zero-initialization scaffold fixture', 'No nontrivial destruction or exception unwinding'])
    for name in ['compiler', 'stock_compiler', 'linker']:
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
