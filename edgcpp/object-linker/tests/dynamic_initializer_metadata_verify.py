#!/usr/bin/env python3
"""Resealed malformed dynamic startup objects must fail semantic verification."""
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
    parser.add_argument('--object', dest='objects', action='append', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--packager', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ['compile.py', 'schema.sql']:
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('dynamic_metadata_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    checks, commands, tested, skipped = [], [], [], []

    def check(label, condition):
        if not condition:
            raise AssertionError(label)
        checks.append(label)

    def verify(label, path, reject=False):
        command = ['dotnet', str(args.linker.resolve()), 'verify', str(path)]
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=120)
        (out / (label + '.stdout')).write_bytes(result.stdout)
        (out / (label + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=label, command=command, exitCode=result.returncode))
        check(label, result.returncode == (1 if reject else 0))
        if reject:
            error = result.stderr.decode(errors='replace').lower()
            check(label + '-not-digest', 'digest' not in error)
        return result

    error = None
    try:
        for index, source in enumerate(args.objects):
            source = source.resolve()
            prefix = str(index) + '-' + source.stem
            original = out / (prefix + '-original.dotobj')
            shutil.copy2(source, original)
            verify(prefix + '-baseline', original)
            with sqlite3.connect(original) as db:
                origin = db.execute('SELECT entity_id FROM generated_routine_origins ORDER BY entity_id LIMIT 1').fetchone()
                if origin is None:
                    skipped.append(dict(object=str(source), reason='No generated file-scope initializer'))
                    continue
                entity = origin[0]
                action = db.execute('SELECT id FROM initialization_actions WHERE entity_id=?', (entity,)).fetchone()[0]
                initializers = db.execute('SELECT id,storage_id,source_ordinal,ordinal FROM dynamic_initializers WHERE action_id=? ORDER BY ordinal', (action,)).fetchall()
                maps = db.execute('''SELECT s.initializer_id,s.ordinal,s.node_id FROM dynamic_initializer_statements s
                    JOIN dynamic_initializers i ON i.id=s.initializer_id WHERE i.action_id=? ORDER BY i.ordinal,s.ordinal''', (action,)).fetchall()
                scaffolds = db.execute('SELECT ordinal,node_id,binding_id,kind FROM generated_routine_scaffolds WHERE entity_id=? ORDER BY ordinal', (entity,)).fetchall()
                foreign = db.execute('SELECT id FROM nodes WHERE function_id<>? ORDER BY id LIMIT 1', (entity,)).fetchone()
                nested = db.execute('''SELECT n.id FROM nodes n JOIN functions f ON f.entity_id=n.function_id
                    WHERE n.function_id=? AND n.id<>f.body_node_id AND NOT EXISTS
                    (SELECT 1 FROM edges e WHERE e.parent_id=f.body_node_id AND e.child_id=n.id AND e.role='statement')
                    ORDER BY n.id LIMIT 1''', (entity,)).fetchone()
                source_max = db.execute('SELECT max(source_ordinal) FROM dynamic_initializers').fetchone()[0]
            check(prefix + '-has-initializers', bool(initializers))
            initializer, storage = initializers[0][:2]
            mutations = [
                ('missing-origin', [('DELETE FROM generated_routine_scaffolds WHERE entity_id=?', (entity,)),
                                    ('DELETE FROM generated_routine_origins WHERE entity_id=?', (entity,))]),
                ('missing-action', [('DELETE FROM dynamic_initializer_statements WHERE initializer_id IN (SELECT id FROM dynamic_initializers WHERE action_id=?)', (action,)),
                                    ('DELETE FROM dynamic_initializers WHERE action_id=?', (action,)),
                                    ('DELETE FROM initialization_actions WHERE id=?', (action,))]),
                ('tls-storage', [('UPDATE storages SET is_thread_local=1 WHERE id=?', (storage,))]),
                ('undefined-storage', [('DELETE FROM storage_data WHERE storage_id=?', (storage,)),
                                       ('DELETE FROM relocations WHERE storage_id=?', (storage,)),
                                       ("UPDATE storages SET definition='declaration',alignment=0 WHERE id=?", (storage,))]),
                ('initializer-order-gap', [('UPDATE dynamic_initializers SET ordinal=ordinal+10000 WHERE id=?', (initializer,))]),
            ]
            if len(initializers) > 1:
                mutations.append(('source-order-reversal', [('UPDATE dynamic_initializers SET source_ordinal=? WHERE id=?', (source_max + 1, initializer))]))
            if maps:
                map_initializer, map_ordinal, node = maps[0]
                mutations += [
                    ('missing-statement', [('DELETE FROM dynamic_initializer_statements WHERE initializer_id=? AND ordinal=?', (map_initializer, map_ordinal))]),
                    ('statement-order-gap', [('UPDATE dynamic_initializer_statements SET ordinal=ordinal+10000 WHERE initializer_id=? AND ordinal=?', (map_initializer, map_ordinal))]),
                    ('missing-initializer', [('DELETE FROM dynamic_initializer_statements WHERE initializer_id=?', (map_initializer,)),
                                             ('DELETE FROM dynamic_initializers WHERE id=?', (map_initializer,))]),
                ]
                if foreign:
                    mutations.append(('foreign-statement', [('UPDATE dynamic_initializer_statements SET node_id=? WHERE initializer_id=? AND ordinal=?', (foreign[0], map_initializer, map_ordinal))]))
                if nested:
                    mutations.append(('nested-statement', [('UPDATE dynamic_initializer_statements SET node_id=? WHERE initializer_id=? AND ordinal=?', (nested[0], map_initializer, map_ordinal))]))
                if len(maps) > 1:
                    # Reinsert after deletion to respect UNIQUE(node_id) while
                    # presenting individually valid, semantically reversed rows.
                    first, last = maps[0], maps[-1]
                    mutations.append(('reversed-statements', [
                        ('DELETE FROM dynamic_initializer_statements WHERE node_id IN (?,?)', (first[2], last[2])),
                        ('INSERT INTO dynamic_initializer_statements VALUES(?,?,?)', (*first[:2], last[2])),
                        ('INSERT INTO dynamic_initializer_statements VALUES(?,?,?)', (*last[:2], first[2])),
                    ]))
                if scaffolds:
                    mutations.append(('duplicate-scaffold-statement', [('UPDATE dynamic_initializer_statements SET node_id=? WHERE initializer_id=? AND ordinal=?', (scaffolds[0][1], map_initializer, map_ordinal))]))
            if scaffolds:
                ordinal, node, binding, kind = scaffolds[0]
                mutations += [
                    ('missing-scaffold', [('DELETE FROM generated_routine_scaffolds WHERE entity_id=? AND ordinal=?', (entity, ordinal))]),
                    ('scaffold-order-gap', [('UPDATE generated_routine_scaffolds SET ordinal=ordinal+10000 WHERE entity_id=? AND ordinal=?', (entity, ordinal))]),
                    ('wrong-scaffold-kind', [('UPDATE generated_routine_scaffolds SET kind=? WHERE entity_id=? AND ordinal=?', ('zero-initialization' if kind == 'declaration' else 'declaration', entity, ordinal))]),
                ]
            else:
                skipped.append(dict(object=str(source), reason='No scope scaffolds in this fixture'))
            for suffix, sql in mutations:
                label = prefix + '-' + suffix
                malformed = out / (label + '.dotobj')
                shutil.copy2(original, malformed)
                with sqlite3.connect(malformed) as db:
                    for statement, parameters in sql:
                        db.execute(statement, parameters)
                    check(label + '-foreign-keys-valid', not db.execute('PRAGMA foreign_key_check').fetchall())
                    check(label + '-schema-constraints-valid', db.execute('PRAGMA integrity_check').fetchall() == [('ok',)])
                    db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                verify(label, malformed, reject=True)
                tested.append(label)
            # A failure-policy outside the wire contract must be rejected even
            # if a writer bypassed SQLite CHECK constraints while constructing it.
            label = prefix + '-forged-failure-policy'
            malformed = out / (label + '.dotobj')
            shutil.copy2(original, malformed)
            with sqlite3.connect(malformed) as db:
                db.execute('PRAGMA ignore_check_constraints=ON')
                db.execute("UPDATE generated_routine_origins SET failure_policy='propagate' WHERE entity_id=?", (entity,))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            verify(label, malformed, reject=True)
            tested.append(label)
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks,
                   rejected=tested, skipped=skipped, commands=commands,
                   linker=dict(path=str(args.linker.resolve()), sha256=hashlib.sha256(args.linker.read_bytes()).hexdigest()))
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), rejected=len(tested), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
