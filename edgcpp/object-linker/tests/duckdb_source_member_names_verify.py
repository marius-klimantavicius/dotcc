#!/usr/bin/env python3
"""Verify receiver-owned dependent member-name tokens without permitting unknown template types."""
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
    source = out / 'duckdb_source_member_names.cpp'
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
                rows=db.execute("SELECT e.expression_id,e.constant_id,c.owner_source_type_id FROM source_expression_constants e JOIN source_semantic_constants c ON c.id=e.constant_id JOIN source_dependent_types d ON d.type_id=c.owner_source_type_id WHERE c.kind='member' AND d.kind='unknown'").fetchall()
                check('unknown-member-tokens-retained', len(rows)>=2)
                operations=set()
                for expression,constant,unknown in rows:
                    parent=db.execute("SELECT expression_id FROM source_expression_edges WHERE child_expression_id=? AND role='operand' AND ordinal=1",(expression,)).fetchone()[0]
                    operations.add(db.execute('SELECT operation_name FROM source_expression_operations WHERE expression_id=?',(parent,)).fetchone()[0])
                check('dot-and-arrow-receiver-forms', operations=={'eok_dot_static','eok_points_to_static'})
                expression,constant,unknown=rows[0]
                newconstant=scalar('SELECT max(id)+1 FROM source_semantic_constants')
                typeoperator=scalar('SELECT type_id FROM source_type_operators WHERE expression_id IS NOT NULL LIMIT 1')
                parent=db.execute("SELECT expression_id FROM source_expression_edges WHERE child_expression_id=? AND ordinal=1",(expression,)).fetchone()[0]
                receiver=db.execute("SELECT child_expression_id FROM source_expression_edges WHERE expression_id=? AND ordinal=0",(parent,)).fetchone()[0]
                declaration,ordinal=db.execute("SELECT declaration_id,ordinal FROM source_expression_declaration_arguments WHERE kind='type' ORDER BY declaration_id,ordinal LIMIT 1").fetchone()
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
                'member-expression-root-escape': f"UPDATE source_type_operators SET expression_id={expression} WHERE type_id={typeoperator}",
                'expression-wrapper-member-name': [f"INSERT INTO source_semantic_constants SELECT {newconstant},'expression',source_type_id,'','',NULL,NULL,0,'',NULL,NULL,NULL,0,source_file_id,line,column FROM source_semantic_constants WHERE id={constant}", f"INSERT INTO source_semantic_constant_operands VALUES({newconstant},0,{constant})"],
                'wrong-selection-operation': f"UPDATE source_expression_operations SET operation_name='eok_add',opcode=3 WHERE expression_id={parent}",
                'self-receiver': f"UPDATE source_expression_edges SET child_expression_id={expression} WHERE expression_id={parent} AND ordinal=0",
                'standalone-member-name': f"UPDATE source_expression_declaration_arguments SET kind='constant',source_type_id=NULL,constant_id={constant} WHERE declaration_id={declaration} AND ordinal={ordinal}",
                'standalone-unknown-type': f"UPDATE source_expression_declaration_arguments SET source_type_id={unknown} WHERE declaration_id={declaration} AND ordinal={ordinal}",
            }
            for name, sql in negatives.items():
                damaged = out / ('reject-' + name + '.dotobj'); shutil.copy2(obj, damaged)
                with sqlite3.connect(damaged) as db:
                    db.execute('PRAGMA ignore_check_constraints=ON')
                    for index,statement in enumerate(sql if isinstance(sql,list) else [sql]):
                        check('reject-' + name + '-changed-' + str(index), db.execute(statement).rowcount == 1)
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
