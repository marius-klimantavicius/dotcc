#!/usr/bin/env python3
"""Reject malformed conditional cleanup metadata without changing the source."""
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
    spec = importlib.util.spec_from_file_location('conditional_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    original = out / 'original.dotobj'
    shutil.copy2(args.object.resolve(), original)
    checks, commands, rejected, schema_rejected, skipped = [], [], [], [], []

    def check(name, success):
        if not success:
            raise AssertionError(name)
        checks.append(name)

    def verify(name, path, expected, diagnostic=None):
        command = ['dotnet', str(args.linker.resolve()), 'verify', str(path)]
        result = subprocess.run(command, capture_output=True, timeout=120, cwd=out)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == expected)
        if expected:
            stderr = result.stderr.decode(errors='replace')
            check(name + '-not-digest', 'digest' not in stderr.lower())
            if diagnostic:
                check(name + '-specific-diagnostic', diagnostic in stderr)

    def insert_row(db, table, row):
        names = ','.join(row)
        parameters = ','.join('?' for _ in row)
        db.execute(f'INSERT INTO {table}({names}) VALUES({parameters})', tuple(row.values()))

    error = None
    try:
        verify('baseline', original, 0)
        with sqlite3.connect(original) as db:
            db.row_factory = sqlite3.Row
            conditions = db.execute('SELECT c.action_id,c.flag_binding_id,a.function_entity_id FROM cleanup_conditions c JOIN cleanup_actions a ON a.id=c.action_id ORDER BY c.action_id').fetchall()
            check('has-conditional-cleanup', bool(conditions))
            # Prefer an original ordinary function so a forged source declaration
            # remains valid independently of the new condition verifier.
            source_functions = {row[0] for row in db.execute('SELECT entity_id FROM source_functions')}
            condition = next((row for row in conditions if row[2] in source_functions), conditions[0])
            action, flag, function = tuple(condition)
            binding = dict(db.execute('SELECT * FROM bindings WHERE id=?', (flag,)).fetchone())
            foreign = db.execute('SELECT id FROM bindings WHERE function_id<>? ORDER BY id LIMIT 1', (function,)).fetchone()
            wrong_type = db.execute("SELECT id FROM types WHERE kind<>'i32' AND kind<>'void' ORDER BY id LIMIT 1").fetchone()[0]
            integer_source = db.execute("SELECT id FROM source_types WHERE kind='int' AND execution_type_id=? ORDER BY id LIMIT 1", (binding['type_id'],)).fetchone()
        mutations = [
            ('flag-not-synthetic', lambda db: db.execute('UPDATE bindings SET is_synthetic=0 WHERE id=?', (flag,)), None),
            ('flag-wrong-type', lambda db: db.execute('UPDATE bindings SET type_id=? WHERE id=?', (wrong_type, flag)), None),
        ]
        if foreign:
            mutations.append(('flag-foreign-owner', lambda db: db.execute('UPDATE cleanup_conditions SET flag_binding_id=? WHERE action_id=?', (foreign[0], action)), None))
        else:
            skipped.append('No foreign binding for owner corruption')

        def parameter_flag(db):
            clone = dict(binding)
            clone['id'] = db.execute('SELECT MAX(id)+1 FROM bindings').fetchone()[0]
            clone['kind'] = 'parameter'
            clone['ordinal'] = db.execute("SELECT COALESCE(MAX(ordinal),-1)+1 FROM bindings WHERE function_id=? AND kind='parameter'", (function,)).fetchone()[0]
            clone['source_name'] = '__forged_condition_parameter'
            insert_row(db, 'bindings', clone)
            db.execute('UPDATE cleanup_conditions SET flag_binding_id=? WHERE action_id=?', (clone['id'], action))
        mutations.append(('flag-parameter', parameter_flag, None))

        if integer_source and function in source_functions:
            def source_flag(db):
                declaration = dict(id=db.execute('SELECT COALESCE(MAX(id),0)+1 FROM source_declarations').fetchone()[0],
                                   kind='local', source_name=binding['source_name'], source_type_id=integer_source[0],
                                   entity_id=None, binding_id=flag, function_entity_id=function,
                                   source_file_id=binding['source_file_id'], line=binding['line'], column=binding['column'])
                insert_row(db, 'source_declarations', declaration)
            mutations.append(('flag-forged-source-identity', source_flag, 'Synthetic execution binding must not have fabricated source capture'))
        else:
            skipped.append('No original ordinary owner/source int for independent source-identity corruption')

        def late_flag(db):
            root = db.execute('SELECT body_node_id FROM functions WHERE entity_id=?', (function,)).fetchone()[0]
            clone = dict(binding)
            clone['id'] = db.execute('SELECT MAX(id)+1 FROM bindings').fetchone()[0]
            clone['source_name'] = '__late_condition_flag'
            clone['scope_node_id'] = root
            insert_row(db, 'bindings', clone)
            db.row_factory = sqlite3.Row
            declaration = dict(db.execute("SELECT * FROM nodes WHERE kind='declare' AND binding_id=? ORDER BY id LIMIT 1", (flag,)).fetchone())
            declaration['id'] = db.execute('SELECT MAX(id)+1 FROM nodes').fetchone()[0]
            declaration['binding_id'] = clone['id']
            insert_row(db, 'nodes', declaration)
            ordinal = db.execute("SELECT COALESCE(MAX(ordinal),-1)+1 FROM edges WHERE parent_id=? AND role='statement'", (root,)).fetchone()[0]
            db.execute("INSERT INTO edges VALUES(?,'statement',?,?)", (root, ordinal, declaration['id']))
            db.execute('UPDATE cleanup_conditions SET flag_binding_id=? WHERE action_id=?', (clone['id'], action))
        mutations.append(('flag-not-live-at-arming', late_flag, 'outside its live declaration scope'))

        for name, mutate, diagnostic in mutations:
            target = out / (name + '.dotobj')
            shutil.copy2(original, target)
            with sqlite3.connect(target) as db:
                mutate(db)
                check(name + '-foreign-keys-valid', not db.execute('PRAGMA foreign_key_check').fetchall())
                # late_flag changes row_factory for copying node records.
                db.row_factory = None
                check(name + '-schema-valid', db.execute('PRAGMA integrity_check').fetchall() == [('ok',)])
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            verify(name, target, 1, diagnostic)
            rejected.append(name)

        # These are schema-invalid rather than resealed semantic corruptions.
        # Report that distinction; optional-row deletion intentionally means an
        # unconditional action and is not treated as an invalid object.
        for name, statement, parameters in [
            ('unknown-action-schema', 'UPDATE cleanup_conditions SET action_id=? WHERE action_id=?', (9223372036854775807, action)),
            ('unknown-flag-schema', 'UPDATE cleanup_conditions SET flag_binding_id=? WHERE action_id=?', (9223372036854775807, action)),
            ('invalid-proof-schema', "UPDATE cleanup_conditions SET capture_proof='unobserved' WHERE action_id=?", (action,)),
        ]:
            target = out / (name + '.dotobj')
            shutil.copy2(original, target)
            failed = False
            with sqlite3.connect(target) as db:
                db.execute('PRAGMA foreign_keys=ON')
                try:
                    db.execute(statement, parameters)
                except sqlite3.IntegrityError as failure:
                    failed = True
                    (out / (name + '.stderr')).write_text(str(failure) + '\n')
            check(name, failed)
            schema_rejected.append(name)
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks,
                   rejected=rejected, schemaRejected=schema_rejected, skipped=skipped, commands=commands,
                   linker=dict(path=str(args.linker.resolve()), sha256=hashlib.sha256(args.linker.read_bytes()).hexdigest()))
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), rejected=len(rejected), schemaRejected=len(schema_rejected), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
