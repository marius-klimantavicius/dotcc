#!/usr/bin/env python3
"""Verify unnamed implicit routines retain original source spelling and ownership."""
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
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'anonymous_routine.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_anonymous_routine_name.cpp'), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)

    def reseal(path, mutate):
        shutil.copy2(obj, path)
        with sqlite3.connect(path) as db:
            mutate(db)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++17', '-O' + opt, source, '-o', binary])
            run('native-run-' + opt, [binary])
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
            '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source])
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, out / 'stock.c',
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary])
        obj = out / 'unit.dotobj'
        run('export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
            '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            rows = db.execute("SELECT e.id,e.source_name,d.source_name,field.source_name FROM source_functions f JOIN entities e ON e.id=f.entity_id JOIN source_declarations d ON d.entity_id=e.id AND d.kind='function' JOIN source_routine_types routine ON routine.type_id=f.routine_source_type_id JOIN source_record_types record ON record.source_type_id=routine.object_type_id JOIN source_record_owners owner ON owner.source_record_id=record.id JOIN source_record_fields field ON field.id=owner.owner_field_id WHERE f.function_kind='constructor' AND record.source_name=''").fetchall()
            check('one-original-anonymous-member-constructor', len(rows) == 1)
            entity, execution_name, source_name, owner_field = rows[0]
            check('empty-original-spelling-retained', execution_name == source_name == '')
            check('exact-member-owner-retained', owner_field == 'helpers')
            check('source-name-correspondence-all-routines', db.execute("SELECT count(*) FROM source_declarations d JOIN entities e ON e.id=d.entity_id WHERE d.kind='function' AND d.source_name<>e.source_name").fetchone()[0] == 0)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', obj])
        run('build-managed', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        for name, statement in [
            ('execution-name-mismatch', "UPDATE entities SET source_name='invented_source_name' WHERE id=?"),
            ('source-name-mismatch', "UPDATE source_declarations SET source_name='invented_source_name' WHERE entity_id=?")]:
            damaged = out / (name + '.dotobj')
            reseal(damaged, lambda db: db.execute(statement, (entity,)))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            check(name + '-exact-diagnostic', 'Source function declaration does not match its execution entity' in (out / ('reject-' + name + '.stderr')).read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
