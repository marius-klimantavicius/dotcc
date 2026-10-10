#!/usr/bin/env python3
"""Verify original field initializer templates and per-constructor lifetime adoption."""
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
    parser.add_argument('--case', choices=('array', 'temporary', 'no_effect', 'finalized_destructor'), default='array')
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--provider', type=Path, action='append', default=[])
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'field.cpp'
    fixture = {'array': 'duckdb_field_initializer_lifetimes.cpp', 'temporary': 'duckdb_field_initializer_temporary.cpp', 'no_effect': 'duckdb_field_initializer_no_effect.cpp', 'finalized_destructor': 'duckdb_field_initializer_finalized_destructor.cpp'}[args.case]
    shutil.copy2(Path(__file__).with_name(fixture), source)
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

    expected = {'array': b'12a!123cba123cba456fed', 'temporary': b'12a!12Sbas', 'no_effect': b'C', 'finalized_destructor': b'CD!CD'}[args.case]
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
            check('original-field-templates-retained', db.execute("SELECT count(*) FROM source_initializers WHERE kind='field-default' AND source_field_id IS NOT NULL AND storage_id IS NULL AND execution_disposition='source-template'").fetchone()[0] >= 1)
            if args.case == 'array':
                check('two-constructors-share-original-template', db.execute("SELECT count(*) FROM (SELECT source_initializer_id FROM field_initializer_executions WHERE execution_disposition='copied' GROUP BY source_initializer_id HAVING count(*)>=2)").fetchone()[0] >= 1)
                check('unused-template-preserved', db.execute("SELECT count(*) FROM source_initializers i JOIN source_record_fields f ON f.id=i.source_field_id JOIN source_record_types r ON r.id=f.source_record_id WHERE r.source_name='UnusedHolder' AND NOT EXISTS(SELECT 1 FROM field_initializer_executions e WHERE e.source_initializer_id=i.id)").fetchone()[0] == 1)
            elif args.case == 'finalized_destructor':
                check('finalized-root-destructor-cleanup', db.execute("SELECT count(*) FROM cleanup_actions WHERE source_field_initializer_id IS NOT NULL").fetchone()[0] >= 1)
            elif args.case == 'temporary':
                check('nested-temporary-lifetime-adopted', db.execute('SELECT count(*) FROM initializer_lifetime_executions').fetchone()[0] > 0)
                check('partial-temporary-original-and-execution-linked', db.execute("SELECT count(*) FROM field_initializer_partial_destruction_executions WHERE kind='adopted-automatic'").fetchone()[0] > 0)
            else:
                check('exact-root-destruction-elision-retained', db.execute("SELECT count(*) FROM field_initializer_destruction_elisions WHERE kind='stock-no-effect' AND capture_proof='observed-no-effect-destruction-removal'").fetchone()[0] == 1)
            check('root-selected-destructor-retained', db.execute('SELECT count(*) FROM source_field_initializer_destructions').fetchone()[0] == 1)
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
        negatives = [
            ('reject-missing-field-execution', 'DELETE FROM field_initializer_executions'),
            ('reject-finalized-destructor-type', "UPDATE source_field_initializer_destructions SET source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1)") if args.case == 'finalized_destructor' else
            ('reject-missing-root-elision', 'DELETE FROM field_initializer_destruction_elisions') if args.case == 'no_effect' else
            ('reject-missing-partial-execution', 'DELETE FROM field_initializer_partial_destruction_executions'),
            ('reject-field-owner-function', "UPDATE field_initializer_executions SET function_entity_id=(SELECT id FROM entities WHERE source_name='main' LIMIT 1) WHERE rowid=(SELECT min(rowid) FROM field_initializer_executions)"),
            ('reject-selected-field-destructor', "UPDATE source_field_initializer_destructions SET selected_routine_entity_id=(SELECT entity_id FROM source_functions WHERE function_kind='constructor' LIMIT 1)"),
            ('reject-false-no-effect-field-execution', "UPDATE field_initializer_executions SET execution_disposition='stock-no-effect',capture_proof='observed-no-effect-field-initializer'")]
        for name, sql in negatives:
            damaged = out / (name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                check(name + '-changed', db.execute(sql).rowcount > 0)
                check(name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
