#!/usr/bin/env python3
"""Check default-argument temporary adoption into stock constructor adapters."""
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
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--runtime-binder', type=Path, default=Path(__file__).resolve().parents[1] / 'runtime/bind.py')
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'duckdb_default_argument_lifetimes.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True, expected=None, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)
        if expected is not None: check(name + '-output', result.stdout == expected)

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++17', '-fno-inline', '-O' + opt, source, '-o', binary])
            run('native-run-' + opt, [binary], expected=b'')
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
            '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source],
            env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ.get('PATH', '')))
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, out / 'stock.c',
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary], expected=b'')
        obj = out / 'unit.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj, '--work-dir', out / 'work',
            '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            rows = db.execute('SELECT function_entity_id,owner_function_entity_id,source_function_entity_id,target_entity_id,forwarding_call_node_id,root_source_lifetime_id FROM default_argument_evaluations ORDER BY function_entity_id').fetchall()
            check('two-independent-array-sites', len(rows) == 2 and len({row[1] for row in rows}) == 2)
            first, second = rows
            check('selected-constructor-shared-without-source-owner-alias', first[3] == second[3] and first[2] != second[2])
            check('original-lifetimes-retain-callers', db.execute('SELECT count(*) FROM default_argument_lifetime_executions e JOIN default_argument_evaluations d USING(function_entity_id) JOIN source_lifetimes l ON l.id=e.source_lifetime_id WHERE l.function_entity_id<>d.source_function_entity_id').fetchone()[0] == 0)
            check('adapter-has-no-fabricated-source-routine', db.execute('SELECT count(*) FROM source_functions s JOIN default_argument_evaluations d ON d.function_entity_id=s.entity_id').fetchone()[0] == 0)
            check('original-destruction-temporaries-retained', db.execute('SELECT count(*) FROM default_argument_temporary_executions e JOIN source_record_temporaries t ON t.id=e.source_temporary_id WHERE t.source_destruction_obligation_id IS NOT NULL').fetchone()[0] >= 2)
            check('each-adapter-has-cleanup-and-normal-destruction', db.execute('SELECT count(DISTINCT a.function_entity_id) FROM cleanup_actions a JOIN default_argument_evaluations d USING(function_entity_id)').fetchone()[0] == 2 and db.execute('SELECT count(DISTINCT n.function_id) FROM normal_destructions a JOIN nodes n ON n.id=a.node_id JOIN default_argument_evaluations d ON d.function_entity_id=n.function_id').fetchone()[0] == 2)
            check('foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
            obligation2 = db.execute('SELECT source_obligation_id FROM default_argument_obligation_executions WHERE function_entity_id=? AND source_obligation_id IS NOT NULL', (second[0],)).fetchone()[0]
            temporary2 = db.execute('SELECT source_temporary_id FROM default_argument_temporary_executions WHERE function_entity_id=?', (second[0],)).fetchone()[0]
        generated = out / 'generated'
        run('link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', generated, '--allow-externs', obj,
            *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
        run('bind-production-runtime', [sys.executable, '-B', args.runtime_binder.resolve(), '--project', generated])
        receipt = json.loads((generated / 'runtime-bindings.json').read_text())
        check('production-runtime-fully-bound', receipt['status'] == 'bound' and not receipt['unresolved'])
        run('build-managed', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'], expected=b'')
        if args.native_aot:
            aot = out / 'aot'
            run('publish-aot', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-r', 'linux-x64',
                '--self-contained', 'true', '-p:PublishAot=true', '-o', aot, '-v:q'])
            run('run-aot', [aot / 'Translated'], expected=b'')
        helper, owner, original, target, call, lifetime = first
        negatives = [
            ('missing-adoption', ['DELETE FROM default_argument_temporary_executions', 'DELETE FROM default_argument_obligation_executions', 'DELETE FROM default_argument_lifetime_executions', 'DELETE FROM default_argument_evaluations']),
            ('false-source-owner', [f'UPDATE default_argument_evaluations SET source_function_entity_id={second[2]} WHERE function_entity_id={helper}']),
            ('false-factory-owner', [f'UPDATE default_argument_evaluations SET owner_function_entity_id={second[1]} WHERE function_entity_id={helper}']),
            ('false-selected-constructor', [f'UPDATE default_argument_evaluations SET target_entity_id={owner} WHERE function_entity_id={helper}']),
            ('missing-copy-proof', [f"UPDATE default_argument_evaluations SET capture_proof='invented' WHERE function_entity_id={helper}"]),
            ('wrong-forwarding-call', [f"UPDATE default_argument_evaluations SET forwarding_call_node_id=(SELECT id FROM nodes WHERE function_id={helper} AND kind='call' AND entity_id<>{target} LIMIT 1) WHERE function_entity_id={helper}"]),
            ('foreign-root-lifetime', [f'UPDATE default_argument_evaluations SET root_source_lifetime_id={second[5]} WHERE function_entity_id={helper}']),
            ('cyclic-copied-lifetime', [f'UPDATE default_argument_lifetime_executions SET copied_parent_source_lifetime_id=source_lifetime_id,copied_sibling_ordinal=0 WHERE function_entity_id={helper}']),
            ('missing-copied-obligation', [f'DELETE FROM default_argument_obligation_executions WHERE function_entity_id={helper}']),
            ('copied-order-gap', [f'UPDATE default_argument_obligation_executions SET copied_ordinal=copied_ordinal+99 WHERE function_entity_id={helper}']),
            ('foreign-same-type-destruction', [f'UPDATE default_argument_obligation_executions SET source_obligation_id={obligation2} WHERE function_entity_id={helper} AND source_obligation_id IS NOT NULL']),
            ('foreign-same-type-temporary', [f'UPDATE default_argument_temporary_executions SET source_temporary_id={temporary2} WHERE function_entity_id={helper}']),
            ('cross-site-adapter-use', [f"UPDATE nodes SET entity_id={second[0]} WHERE kind='function-address' AND entity_id={helper}"]),
        ]
        for name, statements in negatives:
            damaged = out / ('reject-' + name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                db.execute('PRAGMA ignore_check_constraints=ON')
                changed = sum(db.execute(sql).rowcount for sql in statements)
                check('reject-' + name + '-changed', changed > 0)
                check('reject-' + name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        translatedExecutionVerified='run-managed' in checks, nativeAotVerified='run-aot' in checks)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
