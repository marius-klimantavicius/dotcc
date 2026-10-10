#!/usr/bin/env python3
"""Verify original routine/requires parameter scopes and exact reference resolution."""
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
    source = out / 'duckdb_source_parameter_binders.cpp'
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
                check('routine-and-requires-owners', scalar("SELECT count(DISTINCT kind) FROM source_parameter_binders") == 2)
                check('inside-and-outside-prototype-phases', scalar("SELECT count(DISTINCT outside_parameter_list) FROM source_parameter_binders") == 2)
                check('nested-prototype-scopes', scalar("SELECT count(*) FROM source_parameter_binders WHERE kind='routine-prototype' AND parent_binder_id IS NOT NULL") > 0)
                check('original-parameter-names', scalar("SELECT count(DISTINCT source_name) FROM source_binder_parameters WHERE source_name IN('value','inner','different','unused')") >= 3)
                check('nonzero-original-reference-levels', scalar('SELECT count(*) FROM source_expression_parameter_references WHERE original_levels_up>0') > 0)
                reference,binder,lexical = db.execute("SELECT r.expression_id,r.binder_id,b.lexical_binder_id FROM source_expression_parameter_references r JOIN source_expression_bindings b ON b.expression_id=r.expression_id JOIN source_parameter_binders p ON p.id=r.binder_id WHERE p.kind='routine-prototype' LIMIT 1").fetchone()
                other = db.execute('SELECT binder_id FROM source_binder_parameters WHERE ordinal=0 AND binder_id!=? LIMIT 1',(binder,)).fetchone()[0]
                wrong_type = db.execute("SELECT id FROM source_types WHERE kind='double' LIMIT 1").fetchone()[0]
                origin,original,origin_binder=db.execute('SELECT routine_source_type_id,original_routine_source_type_id,original_binder_id FROM source_routine_prototype_origins ORDER BY routine_source_type_id LIMIT 1').fetchone()
                check('copied-lambda-uses-original-recipe-scope', scalar('SELECT count(*) FROM source_parameter_binders WHERE routine_source_type_id IN (SELECT routine_source_type_id FROM source_routine_prototype_origins)') == 0)
                check('foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
            generated = out / 'generated'
            run('link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', generated, obj,
                *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
            run('bind-production-runtime', [sys.executable, '-B', args.runtime_binder.resolve(), '--project', generated])
            bindings = json.loads((generated / 'runtime-bindings.json').read_text())
            check('production-runtime-fully-bound', bindings['status'] == 'bound' and not bindings['unresolved'])
            run('build-managed', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '-v:q'])
            run('run-managed', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
            if args.native_aot:
                aot = out / 'aot'
                run('publish-aot', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', aot, '-v:q'])
                run('run-aot', [aot / 'Translated'])
            negatives = {
                'missing-lambda-prototype-origin': f'DELETE FROM source_routine_prototype_origins WHERE routine_source_type_id={origin}',
                'wrong-lambda-original-prototype': f'UPDATE source_routine_prototype_origins SET original_routine_source_type_id={origin} WHERE routine_source_type_id={origin}',
                'wrong-lambda-original-binder': f'UPDATE source_routine_prototype_origins SET original_binder_id={other} WHERE routine_source_type_id={origin}',
                'wrong-lambda-copy-proof': f"UPDATE source_routine_prototype_origins SET capture_proof='guessed-signature' WHERE routine_source_type_id={origin}",
                'wrong-lambda-receiver': f'UPDATE source_routine_types SET object_type_id=(SELECT object_type_id FROM source_routine_types WHERE type_id={original}) WHERE type_id={origin}',
                'wrong-lambda-exception-policy': f"UPDATE source_routine_types SET exception_policy=CASE exception_policy WHEN 'unwind' THEN 'terminate' ELSE 'unwind' END WHERE type_id={origin}",
                'missing-lexical-binding': f'DELETE FROM source_expression_bindings WHERE expression_id={reference}',
                'wrong-original-owner': f'UPDATE source_expression_parameter_references SET binder_id={other} WHERE expression_id={reference}',
                'wrong-original-level': f'UPDATE source_expression_parameter_references SET original_levels_up=999 WHERE expression_id={reference}',
                'cyclic-prototype-context': f'UPDATE source_parameter_binders SET parent_binder_id=id WHERE id={lexical}',
                'wrong-prototype-phase': f'UPDATE source_parameter_binders SET outside_parameter_list=1-outside_parameter_list WHERE id={lexical}',
                'wrong-adjusted-parameter-type': f'UPDATE source_binder_parameters SET source_type_id={wrong_type},declared_source_type_id=NULL WHERE binder_id={binder} AND ordinal=0',
                'wrong-reference-lvalue-type': f'UPDATE source_expressions SET original_lvalue_type_id={wrong_type} WHERE id={reference}',
                'wrong-parameter-count': f'UPDATE source_parameter_binders SET parameter_count=parameter_count+1 WHERE id={binder}',
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
