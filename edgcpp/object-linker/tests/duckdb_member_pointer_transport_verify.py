#!/usr/bin/env python3
"""Verify scalar member pointers retain their owned representation across value transport."""
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
    source = out / 'member_pointer.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_member_pointer_transport.cpp'), source)
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
            rows = db.execute("SELECT type_id,size,alignment,representation_kind,value_copy_kind,capture_proof FROM record_types WHERE identity_key='abi:member-function-pointer'").fetchall()
            check('one-stock-member-function-representation', len(rows) == 1)
            representation, size, alignment, kind, copy_kind, proof = rows[0]
            check('owned-scalar-representation', (size, alignment, kind, copy_kind, proof) == (16, 8, 'abi', 'explicit-only', 'observed-abi-creation'))
            check('original-member-owner-and-signature-retained', db.execute("SELECT count(*) FROM source_member_pointer_types m JOIN source_types t ON t.id=m.type_id JOIN source_types member ON member.id=m.member_type_id WHERE t.execution_type_id=? AND member.kind='routine'", (representation,)).fetchone()[0] > 0)
            check('by-value-return-representation', db.execute("SELECT count(*) FROM entities WHERE source_name='select_method' AND return_type_id=?", (representation,)).fetchone()[0] == 1)
            check('by-value-parameter-representation', db.execute("SELECT count(*) FROM parameters p JOIN entities e ON e.id=p.entity_id WHERE e.source_name='invoke' AND p.type_id=?", (representation,)).fetchone()[0] == 1)
            check('holder-source-remains-trivially-copyable', db.execute("SELECT count(*) FROM source_record_types WHERE source_name='Holder' AND is_trivially_copyable=1").fetchone()[0] == 1)
            check('null-member-pointer-owned-initialization', db.execute("SELECT count(*) FROM record_storage_initializations x JOIN bindings b ON b.id=x.binding_id WHERE b.source_name='null_method' AND b.type_id=?", (representation,)).fetchone()[0] == 1)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', obj])
        run('build-managed', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        for name, statement, diagnostic in [
            ('wrong-abi-identity', "UPDATE record_types SET identity_key='abi:unrelated-record' WHERE type_id=?", 'Function cannot return an aggregate by value'),
            ('invented-class-copy-policy', "UPDATE record_types SET value_copy_kind='ordinary' WHERE type_id=?", 'ABI record requires observed creation, hidden fields and explicit copy operations'),
            ('wrong-delta-type', "UPDATE record_fields SET type_id=(SELECT id FROM types WHERE kind='u64') WHERE record_type_id=? AND source_name='d'", 'Observed member-function-pointer ABI must retain its exact LP64 code-address and adjustment layout')]:
            damaged = out / (name + '.dotobj')
            reseal(damaged, lambda db: db.execute(statement, (representation,)))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            check(name + '-exact-diagnostic', diagnostic in (out / ('reject-' + name + '.stderr')).read_text())
        damaged = out / 'wrong-initializer-source-member-type.dotobj'
        reseal(damaged, lambda db: db.execute("UPDATE record_storage_initializations SET source_type_id=(SELECT m.type_id FROM source_member_pointer_types m JOIN source_types t ON t.id=m.member_type_id WHERE t.kind='int' LIMIT 1)"))
        run('reject-wrong-initializer-source-member-type', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        check('wrong-initializer-source-member-type-exact-diagnostic', 'Record storage initialization lacks exact original local and observed zeroing provenance' in (out / 'reject-wrong-initializer-source-member-type.stderr').read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
