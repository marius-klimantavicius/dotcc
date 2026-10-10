#!/usr/bin/env python3
"""Verify exact terminal partial-array elision and exceptional temporary cleanup."""
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
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'partial.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_lifetime_terminal_partial.cpp'), source)
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

    expected = b'123Xcba1!12a!123ba!'
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
            check('original-partial-obligation', db.execute("SELECT count(*) FROM source_destruction_obligations WHERE is_partial_aggregate=1").fetchone()[0] > 0)
            check('exact-terminal-elision', db.execute('SELECT count(*) FROM destruction_elisions').fetchone()[0] > 0)
            check('throwing-prefix-cleanup-retained', db.execute('SELECT count(*) FROM cleanup_actions').fetchone()[0] > 0)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', '--allow-externs', obj])
        run('bind', ['python3', '-B', Path(__file__).resolve().parents[1] / 'runtime/bind.py', '--project', out / 'generated'])
        run('build', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('jit', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'], expected=expected)
        if args.native_aot:
            run('publish', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '-v:q'])
            run('nativeaot', [out / 'aot/Translated'], expected=expected)
        for name, sql in (
            ('reject-missing-elision', 'DELETE FROM destruction_elisions'),
            ('reject-not-original-partial', 'UPDATE source_destruction_obligations SET is_partial_aggregate=0 WHERE id IN (SELECT source_obligation_id FROM destruction_elisions)'),
            ('reject-wrong-execution', "UPDATE destruction_elisions SET function_entity_id=(SELECT id FROM entities WHERE source_name='consume' LIMIT 1)")):
            damaged = out / (name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                check(name + '-changed', db.execute(sql).rowcount > 0)
                check(name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
