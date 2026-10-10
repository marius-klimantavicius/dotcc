#!/usr/bin/env python3
"""Verify virtual array destructor selection and generate C#; runtime pending."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--two-allocations', action='store_true')
    parser.add_argument('--member-view', action='store_true')
    parser.add_argument('--provider', type=Path, action='append', default=[])
    args = parser.parse_args()
    args.allocation_only = False
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
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
        run('native-stock-oracles', ['python3', '-B', Path(__file__).with_name('duckdb_virtual_array_delete_oracle.py').resolve(),
            '--stock-compiler', args.stock_compiler.resolve(), '--stock-runtime', args.stock_runtime.resolve(),
            '--output-dir', out / 'oracles', *(['--allocation-only'] if args.allocation_only else []), *(['--two-allocations'] if args.two_allocations else []), *(['--member-view'] if args.member_view else [])])
        fixture = Path(__file__).with_name('duckdb_virtual_array_delete.cpp').read_text()
        for name, helper in (('unsized', 'ia64-vector-new2'), ('sized', 'ia64-vector-new3')):
            case = out / name; case.mkdir()
            source = case / 'vector.cpp'
            source.write_text(('#define DUCKDB_VECTOR_SIZED_DELETE\n' if name == 'sized' else '') +
                ('#define DUCKDB_VECTOR_MEMBER_VIEW\n' if args.member_view else '') +
                ('#define DUCKDB_VECTOR_ALLOCATION_ONLY\n' if args.allocation_only else '') +
                ('#define DUCKDB_VECTOR_TWO_ALLOCATIONS\n' if args.two_allocations else '') + fixture)
            obj = case / 'unit.dotobj'
            run(name + '-export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', case / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
            source.rename(case / 'retained-source.cpp')
            run(name + '-verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
            expected_operations = 2 if args.two_allocations else 1
            with sqlite3.connect(obj) as db:
                check(name + '-exact-helper', db.execute('SELECT count(*) FROM array_allocation_operations WHERE helper_kind=? AND capture_proof=?',
                    (helper, 'observed-array-new-runtime')).fetchone()[0] == expected_operations)
                check(name + '-original-array-allocation-retained', db.execute('SELECT count(*) FROM array_allocation_operations a JOIN source_allocation_obligations o ON o.id=a.source_allocation_obligation_id WHERE o.is_array=1').fetchone()[0] == expected_operations)
                check(name + '-selected-callbacks-retained', db.execute('SELECT count(*) FROM array_allocation_operations WHERE source_destruction_obligation_id IS NOT NULL AND constructor_entity_id IS NOT NULL AND destructor_entity_id IS NOT NULL AND allocator_entity_id<>deallocator_entity_id').fetchone()[0] == expected_operations)
                check(name + '-exact-original-allocation-element-pairs', db.execute('SELECT count(*) FROM source_array_allocation_elements p JOIN array_allocation_operations a ON a.source_allocation_obligation_id=p.source_allocation_obligation_id AND a.source_destruction_obligation_id=p.source_destruction_obligation_id').fetchone()[0] == expected_operations)
                if not args.allocation_only:
                    check(name + '-original-delete-selection-retained', db.execute('SELECT count(*) FROM source_array_deletions').fetchone()[0] == 1)
                    check(name + '-exact-delete-protocol', db.execute('SELECT count(*) FROM array_deletion_operations WHERE helper_kind=? AND capture_proof=?', ('ia64-vector-delete2' if name == 'unsized' else 'ia64-vector-delete3', 'observed-array-delete-runtime')).fetchone()[0] == 1)
            with sqlite3.connect(obj) as db:
                check(name + '-virtual-callback-discriminator', db.execute("SELECT count(*) FROM array_deletion_operations WHERE callback_kind='virtual-cookie-guarded'").fetchone()[0] == 1)
                if args.member_view:
                    check(name + '-original-zero-parameter-member-view-retained', db.execute("SELECT count(*) FROM nodes n JOIN types t ON t.id=n.type_id JOIN routine_types r ON r.type_id=t.pointee_type_id WHERE n.kind='dereference' AND NOT EXISTS(SELECT 1 FROM routine_parameters p WHERE p.type_id=r.type_id)").fetchone()[0] > 0)
                check(name + '-exact-virtual-cookie-provenance', db.execute("SELECT count(*) FROM array_deletion_virtual_callbacks WHERE cookie_size=8 AND vtable_index>0 AND capture_proof='observed-array-delete-virtual-dispatch'").fetchone()[0] == 1)
            run(name + '-generate-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', case / 'generated', '--allow-externs', obj,
                *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
            check(name + '-generated-csharp', any((case / 'generated').glob('*.cs')))
            negatives = [
                ('missing-protocol', 'DELETE FROM array_allocation_operations'),
                ('missing-original-pair', 'DELETE FROM source_array_allocation_elements'),
                ('wrong-delete', 'UPDATE array_allocation_operations SET deallocator_entity_id=allocator_entity_id'),
                ('wrong-constructor', 'UPDATE array_allocation_operations SET constructor_entity_id=destructor_entity_id'),
                ('missing-destructor', 'UPDATE array_allocation_operations SET destructor_entity_id=NULL'),
                ('missing-element-obligation', 'UPDATE array_allocation_operations SET source_destruction_obligation_id=NULL'),
                ('wrong-padding', 'UPDATE array_allocation_operations SET padding_size=padding_size+1'),
                ('wrong-sized-form', "UPDATE array_allocation_operations SET helper_kind=CASE helper_kind WHEN 'ia64-vector-new2' THEN 'ia64-vector-new3' ELSE 'ia64-vector-new2' END"),
                ('wrong-owned-call', "UPDATE array_allocation_operations SET node_id=(SELECT n.id FROM nodes n JOIN entities e ON e.id=n.entity_id WHERE n.kind='call' AND e.source_name='putchar' LIMIT 1) WHERE node_id=(SELECT min(node_id) FROM array_allocation_operations)"),
                ('wrong-stride', "UPDATE nodes SET constant_text='99' WHERE id IN (SELECT e.child_id FROM array_allocation_operations a JOIN edges e ON e.parent_id=a.node_id WHERE e.role='argument' AND e.ordinal=1)")]
            if args.two_allocations:
                negatives.append(('swapped-same-type-element', 'UPDATE array_allocation_operations SET source_destruction_obligation_id=(SELECT max(source_destruction_obligation_id)+min(source_destruction_obligation_id) FROM source_array_allocation_elements)-source_destruction_obligation_id'))
            if not args.allocation_only:
                negatives.extend([
                    ('missing-virtual-dispatch', 'DELETE FROM array_deletion_virtual_callbacks'),
                    ('wrong-delete-callback', 'UPDATE array_deletion_operations SET destructor_entity_id=deallocator_entity_id'),
                    ('wrong-source-delete-selection', 'UPDATE source_array_deletions SET deallocator_entity_id=(SELECT destructor_entity_id FROM source_destruction_obligations WHERE id=source_obligation_id)'),
                    ('wrong-delete-padding', 'UPDATE array_deletion_operations SET padding_size=padding_size+1'),
                    ('wrong-delete-sized-form', "UPDATE array_deletion_operations SET helper_kind=CASE helper_kind WHEN 'ia64-vector-delete2' THEN 'ia64-vector-delete3' ELSE 'ia64-vector-delete2' END"),
                    ('wrong-callback-kind', "UPDATE array_deletion_operations SET callback_kind='direct'"),
                    ('wrong-virtual-callback-node', 'UPDATE array_deletion_virtual_callbacks SET callback_node_id=call_node_id'),
                    ('wrong-virtual-slot', 'UPDATE array_deletion_virtual_callbacks SET vtable_index=vtable_index+1'),
                    ('wrong-cookie-width', 'UPDATE array_deletion_virtual_callbacks SET cookie_size=4'),
                    ('wrong-null-array-guard', "UPDATE edges SET child_id=(SELECT id FROM nodes WHERE kind='constant' AND constant_text='0' AND type_id=(SELECT id FROM types WHERE kind='i32') LIMIT 1) WHERE role='condition' AND parent_id IN (SELECT e.parent_id FROM edges e JOIN array_deletion_operations a ON a.node_id=e.child_id WHERE e.role='then')"),
                    ('nonnull-zero-count-callback', "WITH RECURSIVE sub(id) AS (SELECT callback_node_id FROM array_deletion_virtual_callbacks UNION ALL SELECT e.child_id FROM edges e JOIN sub s ON e.parent_id=s.id) UPDATE nodes SET constant_text='1' WHERE id IN sub AND kind='constant' AND constant_text='0'"),
                    ('wrong-cookie-displacement', "UPDATE nodes SET constant_text='16' WHERE id IN (SELECT c.child_id FROM edges c JOIN nodes p ON p.id=c.parent_id WHERE p.kind='binary' AND p.operator='pointer-subtract' AND c.role='right')"),
                    ('wrong-zero-count-guard', "UPDATE edges SET child_id=(SELECT id FROM nodes WHERE kind='constant' AND constant_text='0' AND type_id=(SELECT id FROM types WHERE kind='i32') LIMIT 1) WHERE role='condition' AND parent_id IN (SELECT n.id FROM nodes n JOIN edges e ON e.parent_id=n.id JOIN nodes c ON c.id=e.child_id WHERE n.kind='conditional' AND e.role='then' AND c.kind='dereference')"),
                    ('wrong-nonempty-arm', "UPDATE edges SET child_id=(SELECT f.child_id FROM edges f WHERE f.parent_id=edges.parent_id AND f.role='else') WHERE role='then' AND parent_id IN (SELECT n.id FROM nodes n JOIN edges e ON e.parent_id=n.id JOIN nodes c ON c.id=e.child_id WHERE n.kind='conditional' AND e.role='then' AND c.kind='dereference')")])
            if args.member_view:
                negatives.extend([
                    ('member-view-explicit-argument', "INSERT INTO routine_parameters(type_id,ordinal,parameter_type_id) SELECT DISTINCT t.pointee_type_id,0,(SELECT id FROM types WHERE kind='i32') FROM nodes n JOIN types t ON t.id=n.type_id JOIN routine_types r ON r.type_id=t.pointee_type_id WHERE n.kind='dereference' AND NOT EXISTS(SELECT 1 FROM routine_parameters p WHERE p.type_id=r.type_id)"),
                    ('member-view-nonvoid-result', "UPDATE routine_types SET return_type_id=(SELECT id FROM types WHERE kind='i32') WHERE type_id IN(SELECT DISTINCT t.pointee_type_id FROM nodes n JOIN types t ON t.id=n.type_id WHERE n.kind='dereference') AND NOT EXISTS(SELECT 1 FROM routine_parameters p WHERE p.type_id=routine_types.type_id)"),
                    ('member-view-wrong-exception-policy', "UPDATE routine_types SET exception_policy=CASE exception_policy WHEN 'unwind' THEN 'terminate' ELSE 'unwind' END WHERE type_id IN(SELECT DISTINCT t.pointee_type_id FROM nodes n JOIN types t ON t.id=n.type_id WHERE n.kind='dereference') AND NOT EXISTS(SELECT 1 FROM routine_parameters p WHERE p.type_id=routine_types.type_id)"),
                    ('missing-original-member-receiver', "UPDATE source_routine_types SET object_type_id=NULL WHERE type_id IN(SELECT f.routine_source_type_id FROM source_functions f JOIN array_deletion_operations a ON f.entity_id=a.destructor_entity_id)")])
            for negative, sql in negatives:
                damaged = case / (negative + '.dotobj'); shutil.copy2(obj, damaged)
                with sqlite3.connect(damaged) as db:
                    db.execute(sql)
                    check(name + '-' + negative + '-changed', db.execute('SELECT changes()').fetchone()[0] > 0)
                    check(name + '-' + negative + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                    db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                run(name + '-reject-' + negative, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
                  translatedExecutionVerified=False, allocationOnly=args.allocation_only, twoAllocations=args.two_allocations,
                  exporterGap='Vector delete[] protocol ownership remains pending.' if args.allocation_only else None, runtimeGap='Production vector allocation/deletion providers remain pending.')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
