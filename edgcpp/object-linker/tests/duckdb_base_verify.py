#!/usr/bin/env python3
"""Recursive source records must preserve base/field provenance during export."""
import argparse
import hashlib
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
    source = inputs / 'duckdb_base_recursive.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    checks, commands = [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=180)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0)

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('build-native-O' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                '-DDUCKDB_BASE_NATIVE_ORACLE', source, '-o', binary])
            run('run-native-O' + optimization, [binary])
        obj = out / 'recursive.dotobj'
        run('export-recursive-record', [sys.executable, '-B', tooling / 'compile.py', source,
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17'])
        with sqlite3.connect(obj) as db:
            db.row_factory = sqlite3.Row
            check('foreign-key-integrity', not list(db.execute('PRAGMA foreign_key_check')))
            original = dict(db.execute('SELECT source_name,id FROM source_record_types'))
            base_id = original['Base']
            views = list(db.execute('SELECT * FROM record_types WHERE source_record_id=?', (base_id,)))
            check('complete-and-subobject-views-retained', {row['representation_kind'] for row in views} == {'complete', 'subobject'})
            bases = list(db.execute('SELECT * FROM source_record_bases WHERE source_record_id=? ORDER BY ordinal', (base_id,)))
            check('original-direct-and-virtual-bases-once', len(bases) == 2 and [row['is_virtual'] for row in bases] == [0, 1])
            direct_id = bases[0]['id']
            mappings = list(db.execute('SELECT b.*,f.source_base_id AS field_source_base FROM record_base_subobjects b '
                'JOIN record_fields f ON f.id=b.field_id WHERE b.record_type_id IN '
                '(SELECT type_id FROM record_types WHERE source_record_id=?)', (base_id,)))
            check('both-physical-views-share-original-direct-base', len(mappings) == 2 and all(
                row['source_base_id'] == direct_id and row['field_source_base'] == direct_id for row in mappings))
            check('direct-base-placement-retained', len({(row['byte_offset'], row['extent'], row['complete_base_type_id']) for row in mappings}) == 1)
            fields = list(db.execute('SELECT sf.id,sf.source_name,COUNT(f.id) AS copies FROM source_record_fields sf '
                'LEFT JOIN record_fields f ON f.source_field_id=sf.id WHERE sf.source_record_id=? GROUP BY sf.id', (base_id,)))
            check('original-fields-shared-between-views', {row['source_name'] for row in fields} == {'peer', 'own'} and
                all(row['copies'] == 2 for row in fields))
            virtuals = list(db.execute('SELECT v.* FROM virtual_base_subobjects v JOIN record_types r ON r.type_id=v.record_type_id '
                'WHERE r.source_record_id=?', (base_id,)))
            check('virtual-base-only-in-complete-view', len(virtuals) == 1 and virtuals[0]['record_type_id'] ==
                next(row['type_id'] for row in views if row['representation_kind'] == 'complete'))
        inputs.rename(out / 'retained-inputs')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        run('link-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--out', out / 'linked', obj])
    except Exception as exception:
        error = str(exception)
    result = dict(schema=1, status='failed' if error else 'passed', checks=checks, checkCount=len(checks),
        commands=commands, error=error, compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
        linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest(), executionVerified=False)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
