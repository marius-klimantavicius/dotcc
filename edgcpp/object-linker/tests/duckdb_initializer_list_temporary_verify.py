#!/usr/bin/env python3
"""Check initializer-list array temporary destruction identity and extent."""
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
    source = out / 'duckdb_initializer_list_temporary.cpp'
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
            rows = db.execute('SELECT t.id,t.source_type_id,t.source_destruction_obligation_id,s.kind,s.element_count,d.source_type_id FROM source_record_temporaries t JOIN source_types s ON s.id=t.source_type_id JOIN source_destruction_obligations d ON d.id=t.source_destruction_obligation_id ORDER BY t.id').fetchall()
            check('two-original-array-temporaries',len(rows)==2 and all(row[3]=='array' and row[4]==3 for row in rows))
            check('destructor-retains-element-identity',all(row[1]!=row[5] for row in rows))
            check('full-array-cleanup-proven',db.execute('SELECT count(*) FROM cleanup_arrays c JOIN cleanup_actions a ON a.id=c.action_id JOIN source_record_temporaries t ON t.source_destruction_obligation_id=a.source_obligation_id WHERE c.element_count=3').fetchone()[0]==2)
            check('foreign-keys-valid',not list(db.execute('PRAGMA foreign_key_check')))
            temporary,array_type,obligation,_,_,element=rows[0]
            action=db.execute('SELECT a.id FROM cleanup_actions a JOIN cleanup_arrays c ON c.action_id=a.id WHERE a.source_obligation_id=?',(obligation,)).fetchone()[0]
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
        negatives = [
            ('short-cleanup',[f'UPDATE cleanup_arrays SET element_count=2 WHERE action_id={action}']),
            ('long-cleanup',[f'UPDATE cleanup_arrays SET element_count=4 WHERE action_id={action}']),
            ('wrong-source-extent',[f'UPDATE source_types SET element_count=4 WHERE id={array_type}']),
            ('missing-array-proof',[f'DELETE FROM cleanup_arrays WHERE action_id={action}']),
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
