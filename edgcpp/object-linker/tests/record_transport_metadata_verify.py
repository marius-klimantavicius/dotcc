#!/usr/bin/env python3
"""Validate observed record construction, transport and assignment permissions."""
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
    spec = importlib.util.spec_from_file_location('record_transport_packager', tooling / 'compile.py')
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
            transfer = db.execute("""SELECT n.id,n.type_id,r.source_record_id FROM record_value_transfers x
                JOIN nodes n ON n.id=x.node_id JOIN record_types r ON r.type_id=n.type_id
                WHERE x.kind='construction' AND r.value_copy_kind='explicit-only'
                ORDER BY n.id LIMIT 1""").fetchone()
            plain = db.execute("""SELECT s.id FROM source_record_types s
                JOIN source_record_operations o ON o.source_record_id=s.id
                JOIN record_types r ON r.source_record_id=s.id AND r.representation_kind='complete'
                JOIN entities e ON e.return_type_id=r.type_id
                WHERE s.is_trivially_copyable=1 AND o.bitwise_construction_allowed=1
                ORDER BY s.id LIMIT 1""").fetchone()
            check('has-explicit-only-construction', bool(transfer))
            check('has-trivial-by-value-transport', bool(plain))
            node, record_type, record_id = transfer
            child = db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='value'", (node,)).fetchone()[0]
            integer = db.execute("SELECT id FROM types WHERE kind='i32'").fetchone()[0]
            ordinary_target = db.execute("""SELECT x.node_id FROM record_value_transfers x
                JOIN edges e ON e.parent_id=x.node_id AND e.role='target'
                JOIN nodes n ON n.id=e.child_id JOIN bindings b ON b.id=n.binding_id
                WHERE x.kind='construction' AND n.kind='binding' AND b.is_synthetic=0 LIMIT 1""").fetchone()
            check('has-source-local-construction', bool(ordinary_target))
        mutations = [
            ('missing-record-operation', [('DELETE FROM source_record_operations WHERE source_record_id=?', (record_id,))], 'lacks original operation permissions'),
            ('construction-denied', [('UPDATE source_record_operations SET bitwise_construction_allowed=0 WHERE source_record_id=?', (record_id,))], None),
            ('return-storage-required', [('UPDATE source_record_operations SET requires_return_storage=1 WHERE source_record_id=?', (record_id,))], None),
            ('trivial-construction-denied', [('UPDATE source_record_operations SET bitwise_construction_allowed=0 WHERE source_record_id=?', (plain[0],))], None),
            ('missing-transfer-proof', [('DELETE FROM record_value_transfers WHERE node_id=?', (node,))], 'lacks observed operation provenance'),
            ('transfer-replaced-with-assignment', [('DELETE FROM record_value_transfers WHERE node_id=?', (node,)), ("UPDATE nodes SET kind='assign' WHERE id=?", (node,))], None),
            ('temporary-source-destination', [("UPDATE record_value_transfers SET kind='temporary',capture_proof='observed-temporary-assignment' WHERE node_id=?", (ordinary_target[0],))], 'generated local destination'),
            ('transfer-place-category', [("UPDATE nodes SET value_category='place' WHERE id=?", (node,))], None),
            ('transfer-integer-result', [('UPDATE nodes SET type_id=? WHERE id=?', (integer, node))], None),
            ('transfer-extra-value', [("INSERT INTO edges VALUES(?,'value',1,?)", (node, child))], None),
            ('transfer-invalid-order', [("UPDATE nodes SET evaluation_order='none' WHERE id=?", (node,))], 'Unsupported evaluation order'),
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
