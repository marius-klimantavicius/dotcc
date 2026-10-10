#!/usr/bin/env python3
"""Verify stock dynamic-array zero initialization with its exact no-rollback disposition."""
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
    source = out / 'zero-initialized-array.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_zero_initialized_array.cpp'), source)
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
            check('no-false-uninitialized-source', db.execute('SELECT count(*) FROM source_uninitialized_allocations').fetchone()[0] == 0)
            check('exact-zero-array-disposition', db.execute("SELECT count(*) FROM allocation_elisions WHERE kind='dynamic-array-zero-initialization' AND capture_proof='observed-dynamic-array-zero-initialization'").fetchone()[0] == 1)
            check('original-array-shape-retained', db.execute("SELECT count(*) FROM allocation_elisions z JOIN source_allocation_obligations a ON a.id=z.source_allocation_obligation_id JOIN source_types t ON t.id=a.allocated_source_type_id WHERE a.is_array=1 AND t.kind='array'").fetchone()[0] == 1)
            check('no-invented-rollback', db.execute('SELECT count(*) FROM internal_try_expressions x JOIN allocation_elisions z ON z.source_allocation_obligation_id=x.source_allocation_obligation_id').fetchone()[0] == 0)
            check('no-invented-vector-helper', db.execute('SELECT count(*) FROM array_allocation_operations').fetchone()[0] == 0)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', '--allow-externs', obj, *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
        # Generation is the acceptance boundary: dynamic-array length failure
        # still needs the shared C++ runtime provider. Do not supply test stubs.
        check('generated-csharp-retained', any((out/'generated').glob('*.cs')))
        negatives = [
            ('reject-missing-zero-disposition', 'DELETE FROM allocation_elisions'),
            ('reject-false-scalar-new', 'UPDATE source_allocation_obligations SET is_array=0'),
            ('reject-false-allocated-type', "UPDATE source_allocation_obligations SET allocated_source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1)"),
            ('reject-zero-execution-owner', "UPDATE allocation_elisions SET function_entity_id=(SELECT id FROM entities WHERE source_name='main' LIMIT 1)"),
            ('reject-zero-proof', "UPDATE allocation_elisions SET capture_proof='observed-empty-allocation-initialization'"),
            ('reject-false-uninitialized-disposition', "UPDATE allocation_elisions SET kind='no-initialization',capture_proof='original-new-without-initialization'")]
        for name, sql in negatives:
            damaged = out / (name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                check(name + '-changed', db.execute(sql).rowcount > 0)
                check(name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands, translatedExecutionVerified=False, runtimeGap='Dynamic array length failure still requires the shared C++ runtime provider')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
