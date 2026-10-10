#!/usr/bin/env python3
"""Anonymous member identities are owned declarations, independent of TU C-tag counters."""
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
    stem = 'duckdb_anonymous_record_identity'
    for suffix in ('.h', '_first.cpp', '_second.cpp'):
        shutil.copy2(Path(__file__).with_name(stem + suffix), inputs / (stem + suffix))
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    checks, commands, snapshots, objects = [], [], {}, []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=240)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == (0 if success else 1))

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('build-native-O' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                inputs / (stem + '_first.cpp'), inputs / (stem + '_second.cpp'), '-o', binary])
            run('run-native-O' + optimization, [binary])
        for label in ('first', 'second'):
            obj = out / (label + '.dotobj')
            run('export-' + label, [sys.executable, '-B', tooling / 'compile.py', inputs / (stem + '_' + label + '.cpp'),
                '--output', obj, '--work-dir', out / ('work-' + label), '--compiler', args.compiler.resolve(), '--std', 'c++17'])
            objects.append(obj)
            run('verify-' + label, ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                db.row_factory = sqlite3.Row
                check(label + '-foreign-key-integrity', not list(db.execute('PRAGMA foreign_key_check')))
                rows = list(db.execute('SELECT c.identity_key,c.source_name,p.identity_key AS owner_identity,f.ordinal,f.source_name AS member_name '
                    'FROM source_record_owners o JOIN source_record_types c ON c.id=o.source_record_id '
                    'JOIN source_record_types p ON p.id=o.owner_source_record_id JOIN source_record_fields f ON f.id=o.owner_field_id'))
                shared = [dict(row) for row in rows if 'DuckdbAnonymousBox' in row['owner_identity']]
                snapshots[label] = sorted(shared, key=lambda row: row['identity_key'])
                check(label + '-eight-distinct-template-member-identities', len(shared) == 8 and len({r['identity_key'] for r in shared}) == 8)
                check(label + '-anonymous-original-name-preserved', all(row['source_name'] == '' for row in shared))
                check(label + '-all-original-member-ordinals-preserved', sorted(row['ordinal'] for row in shared) == [0, 0, 1, 1, 2, 2, 4, 4])
                check(label + '-no-fabricated-C-tag-identity', not list(db.execute("SELECT type_id FROM record_types WHERE identity_key GLOB 'record-abi:__C[0-9]*'")))
                check(label + '-typedef-record-retains-real-name', db.execute(
                    "SELECT COUNT(*) FROM source_record_types r LEFT JOIN source_record_owners o ON o.source_record_id=r.id WHERE r.source_name='DuckdbTypedefRecord' AND o.source_record_id IS NULL").fetchone()[0] == 1)
        check('shared-member-identities-independent-of-unrelated-instantiation', snapshots['first'] == snapshots['second'])
        inputs.rename(out / 'retained-inputs')
        for label, obj in zip(('first', 'second'), objects):
            run('verify-source-hidden-' + label, ['dotnet', args.linker.resolve(), 'verify', obj])
        run('link-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'linked', *objects])
        run('build-managed', ['dotnet', 'build', out / 'linked/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll'])

        def reject(name, mutate):
            damaged = out / (name + '.dotobj')
            shutil.copy2(objects[0], damaged)
            with sqlite3.connect(damaged) as db:
                check(name + '-mutated', mutate(db).rowcount > 0)
                check(name + '-foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        reject('reject-missing-owner', lambda db: db.execute('DELETE FROM source_record_owners WHERE source_record_id=(SELECT source_record_id FROM source_record_owners LIMIT 1)'))
        reject('reject-wrong-owner-field', lambda db: db.execute(
            "UPDATE source_record_owners SET owner_field_id=(SELECT id FROM source_record_fields f WHERE f.source_record_id=source_record_owners.owner_source_record_id AND f.source_name='unrelated') WHERE source_record_id=(SELECT source_record_id FROM source_record_owners LIMIT 1)"))
        def wrong_key(db):
            record = db.execute('SELECT source_record_id FROM source_record_owners LIMIT 1').fetchone()[0]
            db.execute("UPDATE record_types SET identity_key=identity_key||':wrong' WHERE source_record_id=?", (record,))
            return db.execute("UPDATE source_record_types SET identity_key=identity_key||':wrong' WHERE id=?", (record,))
        reject('reject-inconsistent-owner-identity', wrong_key)
    except Exception as exception:
        error = str(exception)
    result = dict(schema=1, status='failed' if error else 'passed', checks=checks, checkCount=len(checks), commands=commands,
        snapshots=snapshots, error=error, compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
        linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest(), executionVerified=error is None)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
