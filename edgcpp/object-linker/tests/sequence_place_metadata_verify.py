#!/usr/bin/env python3
"""Resealed sequence-place and record-materialization metadata must stay typed."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--object', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--packager', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('sequence_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    original = out / 'original.dotobj'
    shutil.copy2(args.object.resolve(), original)
    checks, rejected, commands = [], [], []

    def check(label, success):
        if not success:
            raise AssertionError(label)
        checks.append(label)

    def verify(label, path, expected, diagnostic=None):
        command = ['dotnet', str(args.linker.resolve()), 'verify', str(path)]
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=120)
        (out / (label + '.stdout')).write_bytes(result.stdout)
        (out / (label + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=label, command=command, exitCode=result.returncode))
        check(label, result.returncode == expected)
        if expected:
            stderr = result.stderr.decode(errors='replace')
            check(label + '-not-digest', 'digest' not in stderr.lower())
            if diagnostic:
                check(label + '-specific-diagnostic', diagnostic in stderr)

    error = None
    try:
        verify('baseline', original, 0)
        with sqlite3.connect(original) as db:
            materialize = db.execute("SELECT id,type_id FROM nodes WHERE kind='materialize' ORDER BY id LIMIT 1").fetchone()
            sequence = db.execute("SELECT id,type_id FROM nodes WHERE kind='sequence' AND value_category='place' ORDER BY id LIMIT 1").fetchone()
            check('has-record-materialization-and-sequence-place', bool(materialize and sequence))
            materialized_child = db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='operand'", (materialize[0],)).fetchone()[0]
            integer = db.execute("SELECT id FROM types WHERE kind='i32'").fetchone()[0]
            wrong_sequence_type = db.execute('SELECT id FROM types WHERE id<>? ORDER BY id LIMIT 1', (sequence[1],)).fetchone()[0]
        mutations = [
            ('materialize-value-category', [("UPDATE nodes SET value_category='value' WHERE id=?", (materialize[0],))], 'Incorrect value category'),
            ('materialize-wrong-type', [('UPDATE nodes SET type_id=? WHERE id=?', (integer, materialize[0]))], None),
            ('materialize-place-operand', [("UPDATE nodes SET value_category='place' WHERE id=?", (materialized_child,))], 'Record materialization requires'),
            ('materialize-wrong-order', [("UPDATE nodes SET evaluation_order='none' WHERE id=?", (materialize[0],))], 'Unsupported evaluation order'),
            ('materialize-extra-operand', [("INSERT INTO edges VALUES(?,'operand',1,?)", (materialize[0], materialized_child))], 'Wrong edge cardinality'),
            ('sequence-value-category', [("UPDATE nodes SET value_category='value' WHERE id=?", (sequence[0],))], None),
            ('sequence-wrong-type', [('UPDATE nodes SET type_id=? WHERE id=?', (wrong_sequence_type, sequence[0]))], None),
            ('sequence-wrong-order', [("UPDATE nodes SET evaluation_order='unspecified' WHERE id=?", (sequence[0],))], 'Sequence must evaluate expressions in order'),
        ]
        for label, statements, diagnostic in mutations:
            target = out / (label + '.dotobj')
            shutil.copy2(original, target)
            with sqlite3.connect(target) as db:
                for statement, parameters in statements:
                    db.execute(statement, parameters)
                check(label + '-foreign-keys-valid', not db.execute('PRAGMA foreign_key_check').fetchall())
                check(label + '-schema-valid', db.execute('PRAGMA integrity_check').fetchall() == [('ok',)])
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            verify(label, target, 1, diagnostic)
            rejected.append(label)
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks,
                   rejected=rejected, commands=commands,
                   linker=dict(path=str(args.linker.resolve()), sha256=hashlib.sha256(args.linker.read_bytes()).hexdigest()))
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), rejected=len(rejected), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
