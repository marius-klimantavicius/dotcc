#!/usr/bin/env python3
"""Verify original enum declarations, dependent values, source owners and underlying type provenance."""
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
    source = out / 'duckdb_source_enumerators.cpp'
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
                check('all-nine-original-enum-types', scalar('SELECT count(*) FROM source_enum_types') == 9)
                check('all-original-enumerators', scalar('SELECT count(*) FROM source_enumerators') == 22)
                check('scoped-prototype-retained', scalar('SELECT count(*) FROM source_enum_types WHERE is_scoped=1 AND is_nonreal=1') == 2)
                check('anonymous-prototype-retained', scalar('SELECT count(*) FROM source_enum_types WHERE originally_unnamed=1 AND is_nonreal=1') == 1)
                check('dependent-explicit-base-retained', scalar("SELECT count(*) FROM source_enum_types e JOIN source_types t ON t.id=e.underlying_type_id WHERE e.has_explicit_base=1 AND e.is_nonreal=1 AND e.underlying_origin='declared' AND t.kind='dependent'") == 1)
                check('prototype-has-no-invented-execution-type', scalar('SELECT count(*) FROM source_enum_types e JOIN source_types t ON t.id=e.type_id WHERE e.is_nonreal=1 AND t.execution_type_id IS NOT NULL') == 0)
                check('resolved-and-symbolic-values-retained', scalar("SELECT count(*) FROM source_enumerators WHERE constant_id IS NOT NULL AND value_text='' ")>0 and scalar("SELECT count(*) FROM source_enumerators WHERE constant_id IS NULL AND value_text<>'' ")>0)
                check('implicit-increment-provenance-retained', scalar('SELECT count(*) FROM source_enum_increments') == 4)
                enum=db.execute("SELECT type_id FROM source_enum_types WHERE underlying_origin='prototype-placeholder' LIMIT 1").fetchone()[0]
                owner=db.execute('SELECT owner_type_id FROM source_enum_types WHERE type_id=?',(enum,)).fetchone()[0]
                ordinary=db.execute("SELECT id FROM source_types WHERE kind='int' LIMIT 1").fetchone()[0]
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
                'wrong-owner': f'UPDATE source_enum_types SET owner_type_id={ordinary} WHERE type_id={enum}',
                'lost-owner': f'UPDATE source_enum_types SET owner_type_id=NULL WHERE type_id={enum}',
                'wrong-enumerator-count': f'UPDATE source_enum_types SET enumerator_count=999 WHERE type_id={enum}',
                'wrong-underlying-origin': f"UPDATE source_enum_types SET underlying_origin='declared' WHERE type_id={enum}",
                'wrong-prototype-identity': f"UPDATE source_enum_types SET identity_key='invented' WHERE type_id={enum}",
                'wrong-completeness': f'UPDATE source_enum_types SET enumerator_list_complete=0 WHERE type_id={enum}',
                'fabricated-symbolic-value': f"UPDATE source_enumerators SET value_text='7' WHERE type_id={enum} AND ordinal=0",
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
