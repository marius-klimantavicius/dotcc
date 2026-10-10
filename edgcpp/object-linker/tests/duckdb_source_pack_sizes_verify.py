#!/usr/bin/env python3
"""Verify original sizeof...(pack) operand roles and unevaluated type ownership."""
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
    source = out / 'duckdb_source_pack_sizes.cpp'
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
                rows=db.execute('SELECT constant_id,is_type,is_template_template,source_type_id,expression_id,template_id FROM source_pack_sizes ORDER BY constant_id').fetchall()
                check('all-three-original-operand-roles', {('template' if row[2] else 'type' if row[1] else 'expression') for row in rows} == {'type','expression','template'})
                check('four-pack-size-expressions', len(rows)>=4)
                type_row=next(row for row in rows if row[1] and not row[2])
                template_row=next(row for row in rows if row[2])
                expression_row=next(row for row in rows if not row[1] and not row[2])
                operation=type_row[0]
                ordinary=db.execute("SELECT id FROM source_types WHERE kind='int' LIMIT 1").fetchone()[0]
                check('unevaluated-counts-not-invented', scalar("SELECT count(*) FROM source_semantic_constants WHERE kind='sizeof-pack' AND (value_text<>'' OR operator_name<>'')")==0)
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
                'missing-sizeof-pack': f'DELETE FROM source_pack_sizes WHERE constant_id={operation}',
                'wrong-type-role': f'UPDATE source_pack_sizes SET is_type=0 WHERE constant_id={operation}',
                'not-a-pack-type': f'UPDATE source_pack_sizes SET source_type_id={ordinary} WHERE constant_id={operation}',
                'fabricated-count': f"UPDATE source_semantic_constants SET value_text='3' WHERE id={operation}",
                'wrong-proof': f"UPDATE source_pack_sizes SET capture_proof='guessed-pack' WHERE constant_id={operation}",
                'lost-template-role': f'UPDATE source_pack_sizes SET is_template_template=0 WHERE constant_id={template_row[0]}',
                'lost-expression-role': f'UPDATE source_pack_sizes SET is_type=1 WHERE constant_id={expression_row[0]}',
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
