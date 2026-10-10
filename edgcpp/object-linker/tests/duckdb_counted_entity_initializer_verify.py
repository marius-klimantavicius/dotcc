#!/usr/bin/env python3
"""Verify stock counted entity initializer factory and direct-call ownership."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--oracle-only', action='store_true')
    parser.add_argument('--runtime-binder', type=Path, default=Path(__file__).resolve().parents[1] / 'runtime/bind.py')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'duckdb_counted_entity_initializer.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    checks, commands = [], []

    def check(name, passed):
        if not passed: raise AssertionError(name)
        checks.append(name)

    def run(name, command, expected=0, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout); (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == expected if expected is not None else result.returncode != 0)

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++20', '-fno-inline', '-O' + opt, source, '-o', binary])
            run('native-run-' + opt, [binary])
        run('stock-lower', [args.stock_compiler.resolve(), '--c++20', '--instantiate', 'used', '--no_inlining', '--exceptions',
            '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source],
            env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ['PATH']))
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, out / 'stock.c', args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary])
        if not args.oracle_only:
            tooling = out / 'tooling'; tooling.mkdir()
            for name in ('compile.py', 'schema.sql'): shutil.copy2(args.packager.resolve().parent / name, tooling / name)
            spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
            packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
            obj = out / 'unit.dotobj'
            run('export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj, '--work-dir', out / 'work',
                '--compiler', args.compiler.resolve(), '--std', 'c++20', '--exceptions'])
            source.rename(out / 'retained-source.cpp')
            run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                def scalar(sql): return db.execute(sql).fetchone()[0]
                check('counted-helper-retained', scalar('SELECT count(*) FROM generated_entity_initializers') == 1)
                helper,owner,allocation,element,element_type,destination,count,model,copy = db.execute('SELECT function_entity_id,owner_function_entity_id,source_allocation_obligation_id,source_element_type_id,element_type_id,destination_binding_id,count_binding_id,model_binding_id,copy_node_id FROM generated_entity_initializers').fetchone()
                call=scalar(f'SELECT node_id FROM generated_entity_initializer_calls WHERE helper_entity_id={helper}')
                check('factory-parameters-exact', scalar(f"SELECT count(*) FROM parameters WHERE entity_id={helper} AND parameter_role IN ('entity-destination','entity-count','entity-model')") == 3)
                check('no-invented-source-function', scalar(f'SELECT count(*) FROM source_functions WHERE entity_id={helper}') == 0)
                check('typed-complete-copy-proof', scalar(f"SELECT count(*) FROM record_storage_copies WHERE node_id={copy} AND kind='entity-model' AND byte_count=64") == 1)
                decrement=scalar(f"SELECT id FROM nodes WHERE function_id={helper} AND kind='mutate' AND operator='post-dec'")
                advance=scalar(f"SELECT id FROM nodes WHERE function_id={helper} AND kind='mutate' AND operator='post-inc'")
                check('foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
            generated = out / 'generated'
            run('link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', generated, obj,
                *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
            (out/'runtime-scope.json').write_text(json.dumps(dict(status='generation-only',reason='This test verifies generated helper semantics and preserves unresolved production allocator/vector runtime imports; no managed runtime stubs are supplied.'),indent=2)+'\n')
            negatives = {
                'missing-observed-call': f'DELETE FROM generated_entity_initializer_calls WHERE node_id={call}',
                'wrong-caller-owner': f'UPDATE generated_entity_initializers SET owner_function_entity_id={helper} WHERE function_entity_id={helper}',
                'wrong-count-role': f"UPDATE parameters SET parameter_role='entity-model' WHERE entity_id={helper} AND ordinal=1",
                'wrong-copy-extent': f'UPDATE record_storage_copies SET byte_count=4 WHERE node_id={copy}',
                'wrong-count-direction': f"UPDATE nodes SET operator='post-inc' WHERE id={decrement}",
                'wrong-destination-direction': f"UPDATE nodes SET operator='post-dec' WHERE id={advance}",
                'wrong-factory-proof': f"UPDATE generated_entity_initializers SET capture_proof='guessed-loop' WHERE function_entity_id={helper}",
                'wrong-model-parameter': f'UPDATE generated_entity_initializers SET model_binding_id={destination} WHERE function_entity_id={helper}',
            }
            for name, sql in negatives.items():
                damaged = out / ('reject-' + name + '.dotobj'); shutil.copy2(obj, damaged)
                with sqlite3.connect(damaged) as db:
                    db.execute('PRAGMA ignore_check_constraints=ON')
                    check('reject-' + name + '-changed', db.execute(sql).rowcount == 1)
                    check('reject-' + name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                    db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], expected=None)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
