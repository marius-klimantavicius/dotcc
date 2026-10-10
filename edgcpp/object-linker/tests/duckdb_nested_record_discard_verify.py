#!/usr/bin/env python3
"""Verify nested discarded record initialization and range-for reference-valued steps."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--provider', type=Path, action='append', default=[])
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'uninitialized-new.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_nested_record_discard.cpp'), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True, expected=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)
        if expected is not None: check(name + "-output", result.stdout == expected)

    expected = b'OVII'
    error = None
    try:
        for optimization in ('0', '2'):
            run('native-build-' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, '-o', out / ('native-' + optimization)])
            run('native-run-' + optimization, [out / ('native-' + optimization)], expected=expected)
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
            '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source])
        for optimization in ('0', '2'):
            binary = out / ('stock-' + optimization)
            run('stock-build-' + optimization, ['gcc', '-w', '-falign-functions=4', '-O' + optimization,
                out / 'stock.c', args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + optimization, [binary], expected=expected)
        obj = out / 'unit.dotobj' 
        run('export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj, '--work-dir', out / 'work',
            '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('nested-record-initialization-discard-retained', db.execute("SELECT count(*) FROM nodes i JOIN edges ie ON ie.child_id=i.id JOIN nodes s ON s.id=ie.parent_id JOIN edges se ON se.child_id=s.id JOIN nodes p ON p.id=se.parent_id WHERE i.kind='record-constant-init' AND s.kind='sequence' AND p.kind='sequence' AND se.role='item' AND se.ordinal<(SELECT max(ordinal) FROM edges WHERE parent_id=p.id AND role='item')").fetchone()[0] > 0)
            check('for-step-record-place-retained', db.execute("SELECT count(*) FROM nodes p JOIN edges e ON e.parent_id=p.id JOIN nodes c ON c.id=e.child_id JOIN record_types r ON r.type_id=c.type_id WHERE p.kind='for' AND e.role='step' AND c.kind='dereference' AND c.value_category='place'").fetchone()[0] == 1)
            check('nontrivial-record-transport-remains-explicit', db.execute("SELECT count(*) FROM source_record_operations WHERE requires_return_storage=1").fetchone()[0] >= 2)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', '--allow-externs', obj, *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
        run('bind', ['python3', '-B', Path(__file__).resolve().parents[1] / 'runtime/bind.py', '--project', out / 'generated'])
        receipt = json.loads((out / 'generated/runtime-bindings.json').read_text())
        check('complete-production-runtime-bindings', receipt['status'] == 'bound' and not receipt['unresolved'])
        run('build', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('jit', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'], expected=expected)
        if args.native_aot:
            run('publish', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '-v:q'])
            run('nativeaot', [out / 'aot/Translated'], expected=expected)
        def reject_consumer(name, query):
            damaged = out / (name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                function, value = db.execute(query).fetchone()
                statement = db.execute("SELECT id FROM nodes WHERE function_id=? AND kind='return' ORDER BY id LIMIT 1", (function,)).fetchone()[0]
                db.execute('DELETE FROM edges WHERE parent_id=?', (statement,))
                db.execute("INSERT INTO edges(parent_id,role,ordinal,child_id) VALUES(?,'value',0,?)", (statement,value))
                check(name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
            check(name + '-copy-policy', 'Record value operation lacks' in (out / (name + '.stderr')).read_text())
        reject_consumer('reject-nested-initialization-consumed-as-record', "SELECT s.function_id,s.id FROM nodes i JOIN edges e ON e.child_id=i.id JOIN nodes s ON s.id=e.parent_id WHERE i.kind='record-constant-init' AND s.kind='sequence' LIMIT 1")
        reject_consumer('reject-range-step-consumed-as-record', "SELECT p.function_id,c.id FROM nodes p JOIN edges e ON e.parent_id=p.id JOIN nodes c ON c.id=e.child_id JOIN record_types r ON r.type_id=c.type_id WHERE p.kind='for' AND e.role='step' AND c.value_category='place' LIMIT 1")
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
