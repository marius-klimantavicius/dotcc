#!/usr/bin/env python3
"""Compare folded C++14 void values with native C++, stock C and managed execution."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'packager', 'linker', 'stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    source = inputs / 'duckdb_constexpr_void.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    checks, commands = [], []

    def check(name, condition):
        if not condition: raise AssertionError(name)
        checks.append(name)

    def run(name, command, succeeds=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0 if succeeds else result.returncode != 0)

    error = None
    try:
        for optimization in ('0', '2'):
            executable = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, '-o', executable])
            run('native-run-O' + optimization, [executable])
        stock = out / 'stock.c'
        run('stock-translate', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_rtti',
            '--gen_c_file_name', stock, source])
        for optimization in ('0', '2'):
            executable = out / ('stock-O' + optimization)
            run('stock-build-O' + optimization, ['cc', '-std=c11', '-O' + optimization, stock,
                args.stock_runtime.resolve(), '-lstdc++', '-lm', '-o', executable])
            run('stock-run-O' + optimization, [executable])
        obj = out / 'void.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj,
            '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('typed-void-zero-values-retained', db.execute("""SELECT count(*) FROM nodes n
                JOIN types t ON t.id=n.type_id JOIN edges e ON e.parent_id=n.id AND e.role='operand'
                JOIN nodes z ON z.id=e.child_id JOIN types zt ON zt.id=z.type_id
                WHERE n.kind='convert' AND t.kind='void' AND z.kind='constant'
                AND zt.kind='i32' AND z.constant_text='0'""").fetchone()[0] >= 3)
        inputs.rename(out / 'retained-inputs')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', generated, obj])
        project = generated / 'Translated.csproj'
        run('managed-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        run('managed-run', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        if args.nativeaot:
            run('nativeaot-build', ['dotnet', 'publish', project, '-c', 'Release', '-r', args.rid,
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
            run('nativeaot-run', [out / 'publish/Translated'])
        spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
        packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
        bad = out / 'reject-void-scalar-literal.dotobj'; shutil.copy2(obj, bad)
        with sqlite3.connect(bad) as db:
            changed = db.execute("UPDATE nodes SET type_id=1 WHERE kind='constant' AND constant_text='0'").rowcount
            check('void-scalar-negative-mutated', changed > 0)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
        run('reject-void-scalar-literal', ['dotnet', args.linker.resolve(), 'verify', bad], succeeds=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
        toolDigests={name: hashlib.sha256(getattr(args, name.replace('-', '_')).read_bytes()).hexdigest()
                     for name in ('compiler', 'packager', 'linker', 'stock-compiler', 'stock-runtime')})
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    sys.exit(main())
