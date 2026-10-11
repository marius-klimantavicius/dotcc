#!/usr/bin/env python3
"""Check generic source projections against retained typed roots in an exported object.

Uses copies of a small current-profile object containing source records/routines and
raw expression, type, constant and template nodes. No frontend or build is run.
Every negative is resealed and remains SQLite/FK-valid, so it must fail the specific
managed projection-root integrity check rather than an unrelated digest check.
"""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import time


def camel(name):
    first, *rest = name.split('_')
    return first + ''.join(part.title() for part in rest)


def insert(db, table, row):
    names = ','.join(row)
    db.execute(f'INSERT INTO {table}({names}) VALUES({",".join("?" for _ in row)})', tuple(row.values()))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--driver', type=Path, required=True)
    parser.add_argument('--packager', type=Path, required=True)
    parser.add_argument('--object', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    out = args.output.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands, bridges, expected_maps = [], [], [], {}
    error = None
    spec = importlib.util.spec_from_file_location('bridge_packager', args.packager.resolve())
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, arguments, diagnostic=None):
        command = ['dotnet', str(args.driver.resolve()), *map(str, arguments)]
        started = time.monotonic()
        result = subprocess.run(command, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode,
                             seconds=time.monotonic() - started))
        check(name, result.returncode == 0 if diagnostic is None else
              result.returncode != 0 and diagnostic.encode() in result.stderr)

    def seal(db, name):
        check(name + '-foreign-keys', not list(db.execute('PRAGMA foreign_key_check')))
        check(name + '-sqlite-integrity', db.execute('PRAGMA integrity_check').fetchone()[0] == 'ok')
        db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))

    def rows(db, table):
        return [dict(row) for row in db.execute(f'SELECT * FROM {table}')]

    def next_id(db, table):
        return db.execute(f'SELECT coalesce(max(id),0)+1 FROM {table}').fetchone()[0]

    def selected(db, table, role, kind, extra='1'):
        row = db.execute(f'''SELECT p.*,r.node_id AS raw_node_id FROM {table} p
            JOIN edg_graph_roots r ON r.role=? AND r.ordinal=p.id-1
            JOIN edg_graph_nodes n ON n.id=r.node_id
            WHERE n.entry_kind_name=? AND ({extra}) ORDER BY p.id LIMIT 1''', (role, kind)).fetchone()
        if row is None:
            raise ValueError(f'Input lacks suitable {table} projection with {role} -> {kind}')
        value = dict(row)
        return value, value.pop('raw_node_id')

    def clone(db, table, seed, changes, map_name):
        value = dict(seed, id=next_id(db, table), **changes)
        insert(db, table, value)
        expected_maps.setdefault(map_name, []).append({camel(k): v for k, v in value.items()})
        check(table + '-common-columns-preserved', all(
            value[k] == v for k, v in seed.items() if k not in {'id', *changes}))
        return value['id']

    def clone_children(db, table, owner, before, after, map_name):
        copied = []
        for row in list(db.execute(f'SELECT * FROM {table} WHERE {owner}=?', (before,))):
            value = dict(row)
            value[owner] = after
            insert(db, table, value)
            copied.append({camel(k): v for k, v in value.items()})
        expected_maps.setdefault(map_name, []).extend(copied)

    def root(db, name, role, projection, node, kind, diagnostic):
        db.execute('DELETE FROM edg_graph_roots WHERE role=? AND ordinal=?', (role, projection - 1))
        insert(db, 'edg_graph_roots', dict(node_id=node, context_node_id=None, role=role, ordinal=projection - 1))
        bridges.append(dict(name=name, role=role, ordinal=projection - 1, node=node,
                            kind=kind, diagnostic=diagnostic))

    try:
        run('original-verify', ['verify', args.object.resolve()])
        valid = out / 'generic-valid.dotobj'
        shutil.copy2(args.object, valid)
        with sqlite3.connect(valid) as db:
            db.row_factory = sqlite3.Row
            db.execute('PRAGMA foreign_keys=ON')
            original_execution = rows(db, 'routine_types')
            original_blocks = [tuple(row) for row in db.execute(
                'SELECT ordinal,row_count,decoded_byte_length,sha256 FROM edg_graph_field_blocks ORDER BY ordinal')]
            expression, node = selected(db, 'source_expressions', 'source-expression', 'expr-node',
                                        'p.parameter_count=0 AND p.argument_count=0 AND p.substitution_count=0')
            expr_id = clone(db, 'source_expressions', expression, {'kind': 'edg-node'}, 'sourceExpressions')
            clone_children(db, 'source_expression_edges', 'expression_id', expression['id'], expr_id, 'sourceExpressionEdges')
            root(db, 'expression', 'source-expression', expr_id, node, 'expr-node',
                 'Raw source expression lacks its exact typed EDG expression root')

            scalar, node = selected(db, 'source_types', 'source-type', 'type',
                "p.source_form='plain' AND p.has_namespace_owner=0 AND p.kind IN ('int','bool','void','unsigned-int')")
            type_id = clone(db, 'source_types', scalar, {'kind': 'edg-node'}, 'sourceTypes')
            root(db, 'type', 'source-type', type_id, node, 'type',
                 'Raw source type lacks its exact typed EDG type root')

            constant, node = selected(db, 'source_semantic_constants', 'source-constant', 'constant')
            constant_id = clone(db, 'source_semantic_constants', constant, {'kind': 'edg-node'}, 'sourceSemanticConstants')
            clone_children(db, 'source_semantic_constant_operands', 'constant_id', constant['id'], constant_id,
                           'sourceSemanticConstantOperands')
            root(db, 'constant', 'source-constant', constant_id, node, 'constant',
                 'Raw source constant lacks its exact typed EDG constant root')

            # Template definitions need not have an existing convenience projection.
            # Prefer one when present, otherwise expose an actual raw template node.
            template_row = db.execute('''SELECT p.*,r.node_id AS raw_node_id FROM source_templates p
                JOIN edg_graph_roots r ON r.role='source-template' AND r.ordinal=p.id-1
                JOIN edg_graph_nodes n ON n.id=r.node_id WHERE n.entry_kind_name='template'
                ORDER BY p.id LIMIT 1''').fetchone()
            if template_row is not None:
                template = dict(template_row)
                node = template.pop('raw_node_id')
                template_id = clone(db, 'source_templates', template,
                    dict(kind='edg-node', capture_proof='original-edg-template'), 'sourceTemplates')
                clone_children(db, 'source_template_parameters', 'source_template_id', template['id'], template_id,
                               'sourceTemplateParameters')
            else:
                node_row = db.execute("SELECT id FROM edg_graph_nodes WHERE entry_kind_name='template' ORDER BY id LIMIT 1").fetchone()
                if node_row is None:
                    raise ValueError('Input lacks a retained raw template node')
                node = node_row[0]
                template = dict(id=next_id(db, 'source_templates'), kind='edg-node', source_name='',
                    parameter_depth=0, parameter_position=-1, is_pack=0, parameter_count=0,
                    capture_proof='original-edg-template', source_file_id=None, line=0, column=0)
                insert(db, 'source_templates', template)
                template_id = template['id']
                expected_maps.setdefault('sourceTemplates', []).append({camel(k): v for k, v in template.items()})
            root(db, 'template', 'source-template', template_id, node, 'template',
                 'Raw source template lacks its exact typed EDG template root')

            record, node = selected(db, 'source_record_types', 'source-record', 'type')
            db.execute("UPDATE source_record_types SET navigation_kind='edg-node' WHERE id=?", (record['id'],))
            expected_maps.setdefault('sourceRecords', []).append(
                {camel(k): v for k, v in dict(record, navigation_kind='edg-node').items()})
            root(db, 'record', 'source-record', record['id'], node, 'type',
                 'Raw source record navigation lacks its exact typed EDG type root')

            routine_type, node = selected(db, 'source_types', 'source-type', 'type',
                "p.kind='routine' AND p.source_form='plain' AND p.has_namespace_owner=0")
            routine_id = clone(db, 'source_types', routine_type, {'execution_type_id': None}, 'sourceTypes')
            routine = dict(db.execute('SELECT * FROM source_routine_types WHERE type_id=?', (routine_type['id'],)).fetchone())
            routine.update(type_id=routine_id, exception_policy='unsupported-dynamic', exception_policy_origin='captured',
                           prototype_origin='original', has_borrowed_arguments=0, declarator_context_count=0)
            insert(db, 'source_routine_types', routine)
            expected_maps.setdefault('sourceRoutineTypes', []).append({camel(k): v for k, v in routine.items()})
            clone_children(db, 'source_routine_parameters', 'type_id', routine_type['id'], routine_id, 'sourceRoutineParameters')
            root(db, 'unresolved-policy', 'source-type', routine_id, node, 'type',
                 'Unresolved source exception policy lacks its retained typed EDG routine root')
            check('execution-routine-policies-unchanged', rows(db, 'routine_types') == original_execution)
            check('compressed-typed-fields-unchanged', original_blocks == [tuple(row) for row in db.execute(
                'SELECT ordinal,row_count,decoded_byte_length,sha256 FROM edg_graph_field_blocks ORDER BY ordinal')])
            seal(db, 'generic-valid')

        (out / 'bridges.json').write_text(json.dumps(bridges, indent=2) + '\n')
        run('generic-valid-verify', ['verify', valid])
        catalog = out / 'generic.sqlite'
        run('generic-catalog', ['catalog', '--out', catalog, valid])
        run('generic-maps', ['catalog-maps', '--out', out / 'maps', catalog])
        mapped = json.loads((out / 'maps' / 'source-map.json').read_text())['objects'][0]
        for table, expected in expected_maps.items():
            check(table + '-exact-common-metadata-roundtrip', all(row in mapped[table] for row in expected))
        check('unresolved-source-policy-retained', any(row['exceptionPolicy'] == 'unsupported-dynamic'
              and row['typeId'] == routine_id for row in mapped['sourceRoutineTypes']))
        check('execution-policy-map-unchanged', mapped['routineTypes'] ==
              [{camel(k): v for k, v in row.items()} for row in sorted(original_execution, key=lambda row: row['type_id'])])

        for bridge in bridges:
            for damage in ('missing', 'wrong-kind'):
                name = bridge['name'] + '-' + damage
                bad = out / (name + '.dotobj')
                shutil.copy2(valid, bad)
                with sqlite3.connect(bad) as db:
                    db.execute('PRAGMA foreign_keys=ON')
                    key = (bridge['role'], bridge['ordinal'])
                    if damage == 'missing':
                        count = db.execute('DELETE FROM edg_graph_roots WHERE role=? AND ordinal=?', key).rowcount
                    else:
                        wrong = db.execute('SELECT id FROM edg_graph_nodes WHERE entry_kind_name<>? ORDER BY id LIMIT 1',
                                           (bridge['kind'],)).fetchone()[0]
                        count = db.execute('UPDATE edg_graph_roots SET node_id=? WHERE role=? AND ordinal=?', (wrong, *key)).rowcount
                    check(name + '-one-root-changed', count == 1)
                    seal(db, name)
                run(name, ['verify', bad], bridge['diagnostic'])
    except Exception as exc:
        error = repr(exc)
    (out / 'results.json').write_text(json.dumps(dict(status='failed' if error else 'passed', error=error,
        input=str(args.object.resolve()), checks=checks, commands=commands), indent=2) + '\n')
    if error:
        raise RuntimeError(error)
    print(f'PASS {len(checks)} generic bridge integrity checks')


if __name__ == '__main__':
    main()
