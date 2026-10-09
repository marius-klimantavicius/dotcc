#!/usr/bin/env python3
"""Verify the bounded 128-bit mathematical overflow contract rejects corruptions."""
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
    spec = importlib.util.spec_from_file_location('wide_overflow_packager', tooling / 'compile.py')
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
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=180)
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
            intrinsic = db.execute("""SELECT n.id,n.operator,a.id FROM nodes n
                JOIN edges e ON e.parent_id=n.id AND e.role='argument' AND e.ordinal=2
                JOIN nodes a ON a.id=e.child_id JOIN types p ON p.id=a.type_id
                JOIN types t ON t.id=p.pointee_type_id
                WHERE n.kind='intrinsic' AND n.operator IN ('add-overflow','sub-overflow','mul-overflow')
                AND t.kind IN ('i128','u128') ORDER BY n.id LIMIT 1""").fetchone()
            check('has-wide-overflow-destination', bool(intrinsic))
            node, operation, destination = intrinsic
            first = db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='argument' AND ordinal=0", (node,)).fetchone()[0]
            wide = db.execute("SELECT id FROM types WHERE kind='u128'").fetchone()[0]
            nonboolean = db.execute("SELECT id FROM types WHERE kind='i64'").fetchone()[0]
            float_type = db.execute('SELECT MAX(id)+1 FROM types').fetchone()[0]
        mutations = [
            ('unsupported-wide-intrinsic', [("UPDATE nodes SET operator='bswap' WHERE id=?", (node,))], '128-bit intrinsics require'),
            ('wide-intrinsic-result', [('UPDATE nodes SET type_id=? WHERE id=?', (wide, node))], None),
            ('wide-volatile-destination', [('UPDATE nodes SET operator=? WHERE id=?', (operation + '-volatile', node))], 'Volatile 128-bit overflow destination'),
            ('overflow-nonpointer-destination', [('UPDATE nodes SET type_id=? WHERE id=?', (wide, destination))], 'Invalid mathematical overflow intrinsic'),
            ('overflow-nonboolean-result', [('UPDATE nodes SET type_id=? WHERE id=?', (nonboolean, node))], None),
            ('overflow-floating-operand', [("INSERT INTO types VALUES(?,'f64',NULL,NULL,NULL)", (float_type,)), ('UPDATE nodes SET type_id=? WHERE id=?', (float_type, first))], None),
            ('overflow-extra-argument', [("INSERT INTO edges VALUES(?,'argument',3,?)", (node, destination))], 'Invalid mathematical overflow intrinsic'),
            ('overflow-missing-destination', [("DELETE FROM edges WHERE parent_id=? AND role='argument' AND ordinal=2", (node,))], 'Invalid mathematical overflow intrinsic'),
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
