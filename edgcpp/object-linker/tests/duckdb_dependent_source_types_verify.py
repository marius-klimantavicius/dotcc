#!/usr/bin/env python3
"""Retain unused dependent API types without inventing runtime representations."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'dependent.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_dependent_source_types.cpp'), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)

    error = None
    try:
        for optimization in ('0', '2'):
            run('native-build-' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, '-o', out / ('native-' + optimization)])
            run('native-run-' + optimization, [out / ('native-' + optimization)])
        obj = out / 'unit.dotobj'
        run('export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj, '--work-dir', out / 'work',
            '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('original-parameter', db.execute("SELECT count(*) FROM source_dependent_types WHERE kind='parameter'").fetchone()[0] > 0)
            check('member-owner-type', db.execute("SELECT count(*) FROM source_dependent_types d JOIN source_types t ON t.id=d.type_id WHERE d.kind='member' AND d.owner_type_id IS NOT NULL AND t.source_name='result_type'").fetchone()[0] > 0)
            check('no-dependent-runtime-layout', db.execute("SELECT count(*) FROM source_types WHERE kind='dependent' AND execution_type_id IS NOT NULL").fetchone()[0] == 0)
            check('unused-constexpr-source-preserved', db.execute("SELECT count(*) FROM source_initializers WHERE source_name='minimum' AND execution_disposition='stock-not-emitted'").fetchone()[0] > 0)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', obj])
        run('build', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('jit', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '-v:q'])
            run('nativeaot', [out / 'aot/Translated'])
        for name, sql in (
            ('reject-layout', "UPDATE source_types SET execution_type_id=(SELECT id FROM types WHERE kind='i32' LIMIT 1) WHERE kind='dependent'"),
            ('reject-member-owner', "UPDATE source_dependent_types SET owner_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE kind='member'"),
            ('reject-parameter-identity', "UPDATE source_dependent_types SET identity_key=identity_key||'wrong' WHERE kind='parameter'")):
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
