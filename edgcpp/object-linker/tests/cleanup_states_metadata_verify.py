#!/usr/bin/env python3
"""Resealed cleanup/lifetime graph corruption must fail semantic verification."""
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
    parser.add_argument('--object', required=True, type=Path)
    parser.add_argument('--normal-only-object', type=Path,
                        help='Optional no-exceptions object with normal destructor calls and no unwind actions')
    parser.add_argument('--linker', required=True, type=Path)
    parser.add_argument('--packager', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ['compile.py', 'schema.sql']:
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('cleanup_states_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    original = out / 'original.dotobj'
    shutil.copy2(args.object.resolve(), original)
    checks, commands, rejected, skipped = [], [], [], []

    def check(label, condition):
        if not condition:
            raise AssertionError(label)
        checks.append(label)

    def verify(label, path, expected):
        command = ['dotnet', str(args.linker.resolve()), 'verify', str(path)]
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=120)
        (out / (label + '.stdout')).write_bytes(result.stdout)
        (out / (label + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=label, command=command, exitCode=result.returncode))
        check(label, result.returncode == expected)
        if expected:
            check(label + '-not-digest', 'digest' not in result.stderr.decode(errors='replace').lower())

    error = None
    try:
        verify('baseline', original, 0)
        with sqlite3.connect(original) as db:
            states = db.execute('SELECT id,function_entity_id,action_id,next_state_id FROM cleanup_states ORDER BY id').fetchall()
            actions = db.execute('SELECT id,function_entity_id,source_obligation_id,callback_entity_id,target_node_id,capture_proof FROM cleanup_actions ORDER BY id').fetchall()
            lifetimes = db.execute('SELECT id,function_entity_id,parent_lifetime_id,sibling_ordinal,parent_obligation_id,kind,source_file_id,line,column,capture_proof FROM source_lifetimes ORDER BY id').fetchall()
            obligations = db.execute('SELECT id,function_entity_id,lifetime_id,ordinal,destructor_entity_id,source_type_id,source_file_id,line,column,capture_proof FROM source_destruction_obligations ORDER BY id').fetchall()
            transitions = db.execute('SELECT t.node_id,t.state_id,n.function_id FROM cleanup_transitions t JOIN nodes n ON n.id=t.node_id WHERE t.is_unreachable=0 ORDER BY t.node_id').fetchall()
            prologues = db.execute("SELECT node_id FROM cleanup_frame_markers WHERE kind='prologue' ORDER BY node_id").fetchall()
            nodes = db.execute('SELECT id,function_id,kind,type_id FROM nodes ORDER BY id').fetchall()
            i32 = db.execute("SELECT id FROM types WHERE kind='i32'").fetchone()[0]
            entities = db.execute("SELECT entity_id FROM source_functions WHERE function_kind<>'destructor' ORDER BY entity_id").fetchall()
            base_views = db.execute('SELECT node_id,source_base_id FROM base_subobject_expressions ORDER BY node_id').fetchall()
            expression_lifetimes = db.execute('SELECT x.node_id,x.source_lifetime_id,n.type_id,n.value_category FROM expression_lifetimes x JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id').fetchall()
            types = db.execute('SELECT id FROM types ORDER BY id').fetchall()
            normal_destructions = db.execute('SELECT d.node_id,d.source_obligation_id,n.function_id FROM normal_destructions d JOIN nodes n ON n.id=d.node_id ORDER BY d.node_id').fetchall()
        check('has-owned-cleanup-graph', bool(states and actions and lifetimes and obligations and transitions and prologues))
        state, action, lifetime, obligation, transition = states[0], actions[0], lifetimes[0], obligations[0], transitions[0]
        other_state = next((row for row in states if row[1] != state[1]), None)
        other_action = next((row for row in actions if row[1] != action[1]), None)
        foreign_node = next((row for row in nodes if row[1] != action[1] and row[0] not in {value[4] for value in actions}), None)
        foreign_obligation = next((row for row in obligations if row[1] != obligation[1]), None)
        other_callback = next((row[0] for row in entities if row[0] != action[3]), None)
        mutations = [
            ('state-cycle', [('UPDATE cleanup_states SET next_state_id=id WHERE id=?', (state[0],))]),
            ('source-lifetime-cycle', [('UPDATE source_lifetimes SET parent_lifetime_id=id,sibling_ordinal=sibling_ordinal+10000 WHERE id=?', (lifetime[0],))]),
            ('source-sibling-order-gap', [('UPDATE source_lifetimes SET sibling_ordinal=sibling_ordinal+10000 WHERE id=?', (lifetime[0],))]),
            ('source-destruction-order-gap', [('UPDATE source_destruction_obligations SET ordinal=ordinal+10000 WHERE id=?', (obligation[0],))]),
            ('wrong-parent-obligation', [('UPDATE source_lifetimes SET parent_obligation_id=? WHERE id=?', (obligation[0], lifetime[0]))]),
            ('missing-prologue', [('DELETE FROM cleanup_frame_markers WHERE node_id=?', (prologues[0][0],))]),
            ('wrong-frame-kind', [("UPDATE cleanup_frame_markers SET kind='epilogue' WHERE node_id=?", (prologues[0][0],))]),
            ('transition-nonvoid', [('UPDATE nodes SET type_id=? WHERE id=?', (i32, transition[0]))]),
            ('missing-transition', [('DELETE FROM cleanup_transitions WHERE node_id=?', (transition[0],))]),
            ('frame-nonvoid', [('UPDATE nodes SET type_id=? WHERE id=?', (i32, prologues[0][0]))]),
            ('impure-target', [("UPDATE nodes SET kind='call',entity_id=?,binding_id=NULL,field_id=NULL,storage_id=NULL,operator=NULL,constant_text=NULL,value_category='value',evaluation_order='left-to-right' WHERE id=?", (action[3], action[4]))]),
        ]
        if base_views:
            base_node = base_views[0][0]
            mutations += [
                ('missing-base-identity', [('DELETE FROM base_subobject_expressions WHERE node_id=?', (base_node,))]),
                ('wrong-base-value-type', [('UPDATE nodes SET type_id=? WHERE id=?', (i32, base_node))]),
                ('base-materialized-value', [("UPDATE nodes SET value_category='value' WHERE id=?", (base_node,))]),
            ]
        else:
            skipped.append('No empty base address-only recipe in this object')
        if expression_lifetimes:
            boundary = expression_lifetimes[0]
            source_lifetime = next(row for row in lifetimes if row[0] == boundary[1])
            wrong_kind = next(row[0] for row in lifetimes if row[1] == source_lifetime[1] and row[5] != 'expression-temporary')
            wrong_owner = next((row[0] for row in lifetimes if row[1] != source_lifetime[1] and row[5] == 'expression-temporary'), None)
            wrong_type = next(row[0] for row in types if row[0] != boundary[2])
            mutations += [
                ('missing-expression-lifetime', [('DELETE FROM expression_lifetimes WHERE node_id=?', (boundary[0],))]),
                ('wrong-expression-lifetime-kind', [('UPDATE expression_lifetimes SET source_lifetime_id=? WHERE node_id=?', (wrong_kind, boundary[0]))]),
                ('expression-lifetime-type-mismatch', [('UPDATE nodes SET type_id=? WHERE id=?', (wrong_type, boundary[0]))]),
                ('expression-lifetime-category-mismatch', [('UPDATE nodes SET value_category=? WHERE id=?', ('place' if boundary[3] == 'value' else 'value', boundary[0]))]),
            ]
            if wrong_owner is not None:
                mutations.append(('wrong-expression-lifetime-owner', [('UPDATE expression_lifetimes SET source_lifetime_id=? WHERE node_id=?', (wrong_owner, boundary[0]))]))
            else:
                skipped.append('No foreign expression lifetime for boundary owner corruption')
        else:
            skipped.append('No expression lifetime boundary in this object')
        if normal_destructions:
            normal = normal_destructions[0]
            normal_obligation = next(row for row in obligations if row[0] == normal[1])
            wrong_normal_owner = next((row[0] for row in obligations if row[1] != normal_obligation[1]), None)
            noncall = next(row[0] for row in nodes if row[1] == normal[2] and row[2] != 'call' and row[0] not in {item[0] for item in normal_destructions})
            mutations.append(('normal-destruction-noncall', [('UPDATE normal_destructions SET node_id=? WHERE node_id=?', (noncall, normal[0]))]))
            if wrong_normal_owner is not None:
                mutations.append(('normal-destruction-wrong-owner', [('UPDATE normal_destructions SET source_obligation_id=? WHERE node_id=?', (wrong_normal_owner, normal[0]))]))
            if other_callback is not None:
                mutations.append(('normal-destruction-wrong-callback', [('UPDATE nodes SET entity_id=? WHERE id=?', (other_callback, normal[0]))]))
            normal_only = next((row[1] for row in normal_destructions if row[1] not in {item[2] for item in actions}), None)
            if normal_only is not None:
                mutations.append(('unaccounted-normal-only-obligation', [('DELETE FROM normal_destructions WHERE source_obligation_id=?', (normal_only,))]))
        else:
            skipped.append('No observed normal destructor calls in this object')
        if other_state:
            mutations += [
                ('cross-function-successor', [('UPDATE cleanup_states SET next_state_id=? WHERE id=?', (other_state[0], state[0]))]),
                ('cross-function-transition', [('UPDATE cleanup_transitions SET state_id=? WHERE node_id=?', (other_state[0], next(row[0] for row in transitions if row[2] == state[1])))]),
                ('cross-function-action', [('UPDATE cleanup_states SET action_id=-action_id WHERE id IN (?,?)', (state[0], other_state[0])),
                                           ('UPDATE cleanup_states SET action_id=? WHERE id=?', (other_state[2], state[0])),
                                           ('UPDATE cleanup_states SET action_id=? WHERE id=?', (state[2], other_state[0]))]),
            ]
        else:
            skipped.append('No second cleanup owner for cross-function state/action tests')
        if foreign_node:
            mutations.append(('foreign-target', [('UPDATE cleanup_actions SET target_node_id=? WHERE id=?', (foreign_node[0], action[0]))]))
        if foreign_obligation:
            mutations.append(('wrong-action-source-owner', [('UPDATE cleanup_actions SET source_obligation_id=? WHERE id=?', (foreign_obligation[0], action[0]))]))
        if other_callback:
            mutations.append(('wrong-callback-origin', [('UPDATE cleanup_actions SET callback_entity_id=? WHERE id=?', (other_callback, action[0]))]))
        # Add an otherwise well-formed original obligation which no state owns.
        new_obligation = list(obligation)
        new_obligation[0] = max(row[0] for row in obligations) + 1
        new_obligation[3] = max(row[3] for row in obligations if row[2] == obligation[2]) + 1
        mutations.append(('unaccounted-source-obligation', [('INSERT INTO source_destruction_obligations VALUES(?,?,?,?,?,?,?,?,?,?)', tuple(new_obligation))]))
        unused_node = next(row[0] for row in nodes if row[1] == action[1] and row[0] not in {value[4] for value in actions})
        new_action = list(action)
        new_action[0] = max(row[0] for row in actions) + 1
        new_action[4] = unused_node
        mutations.append(('orphan-action', [('INSERT INTO cleanup_actions VALUES(?,?,?,?,?,?)', tuple(new_action))]))
        def reject_mutation(label, source, sql):
            malformed = out / (label + '.dotobj')
            shutil.copy2(source, malformed)
            with sqlite3.connect(malformed) as db:
                for statement, parameters in sql:
                    db.execute(statement, parameters)
                check(label + '-foreign-keys-valid', not db.execute('PRAGMA foreign_key_check').fetchall())
                check(label + '-schema-constraints-valid', db.execute('PRAGMA integrity_check').fetchall() == [('ok',)])
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            verify(label, malformed, 1)
            rejected.append(label)
        for label, sql in mutations:
            reject_mutation(label, original, sql)
        if args.normal_only_object:
            normal_original = out / 'normal-only-original.dotobj'
            shutil.copy2(args.normal_only_object.resolve(), normal_original)
            verify('normal-only-baseline', normal_original, 0)
            with sqlite3.connect(normal_original) as db:
                normal_rows = db.execute('SELECT node_id,source_obligation_id FROM normal_destructions ORDER BY node_id').fetchall()
                check('normal-only-has-destruction-proof', bool(normal_rows))
                check('normal-only-no-unwind-actions', db.execute('SELECT COUNT(*) FROM cleanup_actions').fetchone()[0] == 0)
                normal_node, normal_obligation_id = normal_rows[0]
                noncall = db.execute("SELECT n.id FROM nodes n WHERE n.kind<>'call' AND n.id NOT IN (SELECT node_id FROM normal_destructions) ORDER BY n.id LIMIT 1").fetchone()[0]
            reject_mutation('normal-only-missing-coverage', normal_original,
                            [('DELETE FROM normal_destructions WHERE source_obligation_id=?', (normal_obligation_id,))])
            reject_mutation('normal-only-noncall-proof', normal_original,
                            [('UPDATE normal_destructions SET node_id=? WHERE node_id=?', (noncall, normal_node))])
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks,
                   rejected=rejected, skipped=skipped, commands=commands,
                   linker=dict(path=str(args.linker.resolve()), sha256=hashlib.sha256(args.linker.read_bytes()).hexdigest()))
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), rejected=len(rejected), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
