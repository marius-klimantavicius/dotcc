#!/usr/bin/env python3
"""Retain original record definitions independently of executable layout."""
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
    source = out / 'source-records.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_source_record_metadata.cpp'), source)
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
            check('complete-dependent-prototype', db.execute("SELECT count(*) FROM source_record_types WHERE source_name='DuckdbSourceOnlyGuard' AND is_complete=1 AND is_dependent=1 AND is_prototype_instantiation=1 AND layout_known=0 AND native_size=0 AND native_alignment=0 AND is_trivially_copyable IS NULL").fetchone()[0] > 0)
            check('source-only-not-executable', db.execute("SELECT count(*) FROM source_record_types r JOIN source_types t ON t.id=r.source_type_id WHERE r.is_dependent=1 AND t.execution_type_id IS NOT NULL").fetchone()[0] == 0)
            check('dependent-field-type', db.execute("SELECT count(*) FROM source_record_fields f JOIN source_record_types r ON r.id=f.source_record_id JOIN source_types t ON t.id=f.source_type_id WHERE r.source_name='DuckdbSourceOnlyGuard' AND f.source_name='rollback' AND t.kind='dependent'").fetchone()[0] > 0)
            check('field-initializer-retained', db.execute("SELECT count(*) FROM source_record_fields f JOIN source_record_types r ON r.id=f.source_record_id WHERE r.source_name='DuckdbSourceOnlyGuard' AND f.source_name='complete' AND f.has_initializer=1").fetchone()[0] > 0)
            check('dependent-base-retained', db.execute("SELECT count(*) FROM source_record_bases b JOIN source_record_types r ON r.id=b.source_record_id JOIN source_types t ON t.id=b.base_source_type_id WHERE r.source_name='DuckdbSourceOnlyDerived' AND t.kind='dependent'").fetchone()[0] > 0)
            check('no-fabricated-operations', db.execute("SELECT count(*) FROM source_record_operations o JOIN source_record_types r ON r.id=o.source_record_id WHERE r.is_dependent=1").fetchone()[0] == 0)
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', obj])
        run('build', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('jit', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '-v:q'])
            run('nativeaot', [out / 'aot/Translated'])
        for name, sql in (
            ('reject-fabricated-layout', "UPDATE source_record_types SET is_dependent=0,is_prototype_instantiation=0,layout_known=1,native_size=1,native_alignment=1 WHERE is_dependent=1 AND identity_key LIKE 'source-prototype:%'"),
            ('reject-dependent-facts', "UPDATE source_record_types SET is_dependent=0,is_prototype_instantiation=0 WHERE is_dependent=1 AND identity_key LIKE 'source-prototype:%'"),
            ('reject-prototype-facts', "UPDATE source_record_types SET is_prototype_instantiation=0 WHERE is_dependent=1 AND identity_key LIKE 'source-prototype:%'"),
            ('reject-incomplete-members', "UPDATE source_record_types SET is_complete=0 WHERE is_dependent=1 AND source_name='DuckdbSourceOnlyGuard'"),
            ('reject-physical-template', "UPDATE source_types SET execution_type_id=(SELECT type_id FROM record_types WHERE size>0 LIMIT 1) WHERE id IN (SELECT source_type_id FROM source_record_types WHERE is_dependent=1)")):
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
