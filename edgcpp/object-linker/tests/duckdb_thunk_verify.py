#!/usr/bin/env python3
"""Verify owned virtual wrapper origins and exact forwarding independently of source."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    source = inputs / 'duckdb_thunk_wrappers.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=180)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == (0 if success else 1))

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('build-native-O' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                '-DDUCKDB_THUNK_NATIVE_ORACLE', source, '-o', binary])
            run('run-native-O' + optimization, [binary])
        obj = out / 'thunks.dotobj'
        run('export-wrapper-records', [sys.executable, '-B', tooling / 'compile.py', source,
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions', '--rtti'])
        run('verify-wrapper-records', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            db.row_factory = sqlite3.Row
            wrappers = list(db.execute('SELECT * FROM thunk_wrappers'))
            check('foreign-key-integrity', not list(db.execute('PRAGMA foreign_key_check')))
            check('fixed-receiver-adjustment-owned', any(row['this_delta'] != 0 and row['vcall_index'] == 0 for row in wrappers))
            check('virtual-receiver-adjustment-owned', any(row['vcall_index'] < 0 for row in wrappers))
            check('covariant-virtual-result-adjustment-owned', any(row['vbase_index'] < 0 for row in wrappers))
            check('destructor-alternate-wrapper-targets-owned', db.execute(
                "SELECT COUNT(*) FROM thunk_wrappers w JOIN abi_routine_origins a ON a.entity_id=w.target_entity_id WHERE a.entry_kind IN ('deleting','complete')").fetchone()[0] > 0)
            check('wrapper-formals-not-original-declarations', db.execute(
                'SELECT COUNT(*) FROM source_declarations d JOIN thunk_wrappers w ON w.entity_id=d.entity_id OR w.entity_id=d.function_entity_id').fetchone()[0] == 0)
            check('every-defined-wrapper-forwards-once', not list(db.execute(
                'SELECT w.entity_id FROM thunk_wrappers w JOIN functions f ON f.entity_id=w.entity_id LEFT JOIN wrapper_calls c ON c.wrapper_entity_id=w.entity_id GROUP BY w.entity_id HAVING COUNT(c.node_id)<>1')))
        inputs.rename(out / 'retained-inputs')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        run('link-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--allow-externs', '--out', out / 'linked', obj])

        def reject(name, mutate):
            damaged = out / (name + '.dotobj')
            shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                mutate(db)
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)

        reject('reject-wrong-forward-target', lambda db: db.execute(
            'UPDATE nodes SET entity_id=(SELECT w.overridden_entity_id FROM wrapper_calls c JOIN thunk_wrappers w ON w.entity_id=c.wrapper_entity_id WHERE c.node_id=nodes.id) WHERE id=(SELECT node_id FROM wrapper_calls LIMIT 1)'))
        reject('reject-missing-forwarding-proof', lambda db: db.execute('DELETE FROM wrapper_calls WHERE node_id=(SELECT node_id FROM wrapper_calls LIMIT 1)'))
        reject('reject-wrong-primary-origin', lambda db: db.execute(
            'UPDATE thunk_wrappers SET primary_entity_id=overridden_entity_id WHERE target_entity_id=primary_entity_id'))
    except Exception as exception:
        error = str(exception)
    result = dict(schema=1, status='failed' if error else 'passed', checks=checks, checkCount=len(checks), commands=commands,
        error=error, compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
        linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest(), executionVerified=False)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
