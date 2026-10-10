#!/usr/bin/env python3
"""Verify the owned generated semantic-zero model and constructor vector callback."""
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
    parser.add_argument('--provider', type=Path, action='append', default=[])
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'generated-array-initializer.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_generated_array_initializer.cpp'), source)
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

    expected = b''
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
            check('one-generated-array-initializer', db.execute('SELECT count(*) FROM generated_array_initializers').fetchone()[0] == 1)
            row=db.execute('SELECT function_entity_id,model_initialization_node_id,model_copy_node_id,constructor_call_node_id FROM generated_array_initializers').fetchone()
            helper,model_init,model_copy,ctor_call=row
            check('no-fabricated-source-routine', db.execute('SELECT count(*) FROM source_functions WHERE entity_id=?',(helper,)).fetchone()[0] == 0)
            check('semantic-zero-model-not-byte-zero', db.execute("SELECT count(*) FROM nodes WHERE function_id=? AND kind='constant' AND constant_text='-1'",(helper,)).fetchone()[0] >= 1)
            check('selected-constructor-retained', ctor_call is not None)
            check('owned-model-copy', db.execute("SELECT count(*) FROM record_storage_copies WHERE node_id=? AND kind='array-zero-model'",(model_copy,)).fetchone()[0] == 1)
            check('owned-vector-new3', db.execute("SELECT count(*) FROM array_allocation_operations WHERE helper_kind='ia64-vector-new3' AND constructor_entity_id=?",(helper,)).fetchone()[0] == 1)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', '--allow-externs', obj, *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
        # Generation is the acceptance boundary: dynamic-array length failure
        # still needs the shared C++ runtime provider. Do not supply test stubs.
        check('generated-csharp-retained', any((out/'generated').glob('*.cs')))
        negatives = [
            ('reject-missing-helper-origin', 'DELETE FROM generated_array_initializers'),
            ('reject-false-owner', "UPDATE generated_array_initializers SET owner_function_entity_id=(SELECT id FROM entities WHERE source_name='main' LIMIT 1)"),
            ('reject-false-element-type', "UPDATE generated_array_initializers SET element_type_id=(SELECT id FROM types WHERE kind='i32' LIMIT 1)"),
            ('reject-model-parameter-swap', 'UPDATE generated_array_initializers SET object_parameter_binding_id=model_binding_id'),
            ('reject-model-init-node', 'UPDATE generated_array_initializers SET model_initialization_node_id=model_copy_node_id'),
            ('reject-missing-copy-proof', "DELETE FROM record_storage_copies WHERE kind='array-zero-model'"),
            ('reject-missing-constructor-metadata', 'UPDATE generated_array_initializers SET constructor_entity_id=NULL,constructor_call_node_id=NULL'),
            ('reject-member-pointer-byte-zero', f"UPDATE nodes SET constant_text='0' WHERE function_id={helper} AND kind='constant' AND constant_text='-1'"),
            ('reject-short-model-copy', "UPDATE record_storage_copies SET byte_count=byte_count-1 WHERE kind='array-zero-model'"),
            ('reject-model-copy-target', "UPDATE record_storage_copies SET target_node_id=value_node_id WHERE kind='array-zero-model'"),
            ('reject-model-init-kind', 'UPDATE generated_array_initializers SET model_lowered_init_kind=3,model_zero_storage=0'),
            ('reject-callback-parameter-role', f"UPDATE parameters SET parameter_role='static-object' WHERE entity_id={helper}"),
        ]
        for name, sql in negatives:
            damaged = out / (name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                db.execute('PRAGMA ignore_check_constraints=ON')
                check(name + '-changed', db.execute(sql).rowcount > 0)
                check(name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands, translatedExecutionVerified=False, runtimeGap='Vector allocation and array-length failure still require shared C++ runtime providers')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
