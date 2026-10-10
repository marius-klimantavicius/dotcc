#!/usr/bin/env python3
"""Verify original auto/decltype(auto) placeholder forms and constraints."""
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
    source = out / 'duckdb_source_auto_placeholders.cpp'
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
                check('auto-original-form-retained', scalar("SELECT count(*) FROM source_dependent_types WHERE kind='auto-placeholder' AND nesting_depth=-1 AND parameter_position=1") >= 1)
                check('decltype-auto-original-form-retained', scalar("SELECT count(*) FROM source_dependent_types WHERE kind='auto-placeholder' AND nesting_depth=-1 AND parameter_position=2") >= 1)
                placeholder = scalar("SELECT type_id FROM source_dependent_types WHERE kind='auto-placeholder' AND parameter_position=1 ORDER BY type_id LIMIT 1")
                constrained = scalar("SELECT type_id FROM source_dependent_types WHERE kind='auto-placeholder' AND has_type_constraint=1 LIMIT 1")
                check('placeholder-constraint-owned', scalar(f'SELECT count(*) FROM source_type_constraints WHERE source_type_id={constrained}') == 1)
                check('placeholders-source-only', scalar("SELECT count(*) FROM source_types t JOIN source_dependent_types p ON p.type_id=t.id WHERE p.kind='auto-placeholder' AND t.execution_type_id IS NOT NULL") == 0)
                check('placeholders-not-formal-parameters', scalar("SELECT count(*) FROM source_dependent_types WHERE kind='auto-placeholder' AND (identity_key!='' OR owner_type_id IS NOT NULL OR is_pack!=0)") == 0)
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
                'wrong-placeholder-depth': f'UPDATE source_dependent_types SET nesting_depth=0 WHERE type_id={placeholder}',
                'wrong-placeholder-form': f'UPDATE source_dependent_types SET parameter_position=3 WHERE type_id={placeholder}',
                'fabricated-placeholder-name': f"UPDATE source_types SET source_name='T' WHERE id={placeholder}",
                'fabricated-placeholder-formal': f"UPDATE source_dependent_types SET identity_key='guessed-formal' WHERE type_id={placeholder}",
                'placeholder-pack': f'UPDATE source_dependent_types SET is_pack=1 WHERE type_id={placeholder}',
                'missing-original-constraint': f'DELETE FROM source_type_constraints WHERE source_type_id={constrained}',
                'wrong-presence-marker': f'UPDATE source_dependent_types SET has_type_constraint=0 WHERE type_id={constrained}',
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
