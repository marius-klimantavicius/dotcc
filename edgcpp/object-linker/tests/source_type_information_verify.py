#!/usr/bin/env python3
"""Export original type identities without depending on C# generation or runtime support."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import resource
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
    fixture_names = ['source_type_information.h', 'source_type_information_provider.cpp',
                     'source_type_information_main.cpp', 'source_type_information_members.cpp',
                     'source_type_information_member_call.cpp', 'source_type_information_atomic.c',
                     'source_type_information_c_api.h', 'source_type_information_c_provider.c',
                     'source_type_information_c_main.c']
    for name in fixture_names:
        shutil.copy2(Path(__file__).with_name(name), inputs / name)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    checks, commands, objects, snapshots = [], [], {}, {}

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600,
            preexec_fn=lambda: resource.setrlimit(resource.RLIMIT_CORE, (0, 0)))
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0 if success else result.returncode == 1)
        return result

    def inspect(label, obj):
        with sqlite3.connect(obj) as db:
            db.row_factory = sqlite3.Row
            types = {row['id']: dict(row) for row in db.execute('SELECT * FROM source_types')}
            def original(type_id):
                seen = set()
                while types[type_id]['kind'] in ('typedef', 'qualified'):
                    if type_id in seen:
                        raise AssertionError(label + '-cyclic-source-type-alias')
                    seen.add(type_id)
                    type_id = types[type_id]['underlying_type_id']
                return types[type_id]
            def chain(type_id):
                result = []
                while True:
                    node = types[type_id]
                    result.append(node)
                    if node['kind'] not in ('typedef', 'qualified'):
                        return result
                    type_id = node['underlying_type_id']
            fields = {row['source_name']: dict(row) for row in db.execute('SELECT * FROM source_record_fields')}
            check(label + '-sqlite-foreign-keys', not list(db.execute('PRAGMA foreign_key_check')))
            check(label + '-all-source-types-owned', bool(types) and all(
                row['underlying_type_id'] is None or row['underlying_type_id'] in types
                for row in types.values()))
            if label == 'members':
                member_types = {row['type_id']: dict(row) for row in db.execute('SELECT * FROM source_member_pointer_types')}
                member_ids = {name: original(fields[name]['source_type_id'])['id']
                              for name in ('data', 'other_data', 'read', 'write', 'unmatched', 'source_only', 'qualified_data')}
                check('member-pointers-have-original-category', all(types[value]['kind'] == 'member-pointer'
                      for value in member_ids.values()))
                check('member-pointer-qualifier-preserved', any(row['is_const'] for row in chain(fields['qualified_data']['source_type_id'])))
                check('member-pointer-const-alias-same-original', member_ids['data'] == member_ids['qualified_data'])
                data, other = member_types[member_ids['data']], member_types[member_ids['other_data']]
                check('member-pointer-owners-distinct', original(data['owner_type_id'])['id'] != original(other['owner_type_id'])['id'])
                check('member-pointer-data-type-owned', original(data['member_type_id'])['kind'] == 'int')
                routines = {row['type_id']: dict(row) for row in db.execute('SELECT * FROM source_routine_types')}
                read = routines[original(member_types[member_ids['read']]['member_type_id'])['id']]
                write = routines[original(member_types[member_ids['write']]['member_type_id'])['id']]
                check('member-function-const-lvalue-ref-noexcept', read['object_is_const'] == 1 and
                      read['object_is_volatile'] == 0 and read['object_ref_qualifier'] == 'lvalue' and read['exception_policy'] == 'terminate')
                check('member-function-rvalue-ref-potentially-throwing', write['object_is_const'] == 0 and
                      write['object_ref_qualifier'] == 'rvalue' and write['exception_policy'] == 'unwind')
                for name, routine in (('read', read), ('write', write)):
                    parameters = list(db.execute('SELECT parameter_type_id FROM source_routine_parameters WHERE type_id=? ORDER BY ordinal',
                        (routine['type_id'],)))
                    check('member-function-' + name + '-declared-result-and-parameters', original(routine['return_type_id'])['kind'] == 'int' and
                        [original(row[0])['kind'] for row in parameters] == ['int'])
                    check('member-function-' + name + '-declared-owner', routine['object_type_id'] is not None and
                        original(routine['object_type_id'])['id'] == original(member_types[member_ids[name]]['owner_type_id'])['id'])
                    if types[routine['type_id']]['execution_type_id'] is None:
                        check('member-function-' + name + '-source-only-without-fabricated-ABI', db.execute(
                            'SELECT count(*) FROM source_routine_abis WHERE source_routine_type_id=?', (routine['type_id'],)).fetchone()[0] == 0)
                unmatched = routines[original(member_types[member_ids['unmatched']]['member_type_id'])['id']]
                check('unmatched-supported-member-routine-has-observed-mapping', types[unmatched['type_id']]['execution_type_id'] is not None and
                    db.execute('SELECT capture_proof FROM source_routine_abis WHERE source_routine_type_id=?',
                    (unmatched['type_id'],)).fetchone()[0] == 'observed-routine-lowering')
                source_only = routines[original(member_types[member_ids['source_only']]['member_type_id'])['id']]
                check('variadic-member-function-has-observed-ABI', types[source_only['type_id']]['execution_type_id'] is not None and db.execute(
                    'SELECT capture_proof FROM source_routine_abis WHERE source_routine_type_id=?', (source_only['type_id'],)).fetchone()[0] == 'observed-routine-lowering')
                observed_parameters = [tuple(row) for row in db.execute(
                    'SELECT execution_ordinal,parameter_role,source_ordinal FROM source_routine_abi_parameters WHERE source_routine_type_id=? ORDER BY execution_ordinal', (source_only['type_id'],))]
                check('variadic-member-function-preserves-indirect-record-parameter', observed_parameters == [(0, 'this', None), (1, 'source', 0), (2, 'indirect-source', 1)])
                parameters = list(db.execute('SELECT parameter_type_id FROM source_routine_parameters WHERE type_id=? ORDER BY ordinal',
                    (source_only['type_id'],)))
                check('source-only-member-function-complete-original-signature', source_only['object_is_volatile'] == 1 and
                    source_only['object_is_const'] == 0 and source_only['object_ref_qualifier'] == 'rvalue' and
                    source_only['exception_policy'] == 'terminate' and original(source_only['return_type_id'])['kind'] == 'long' and
                    [original(row[0])['kind'] for row in parameters] == ['short', 'record'] and source_only['variadic'] == 1 and
                    original(source_only['object_type_id'])['id'] == original(data['owner_type_id'])['id'])
                snapshots[label] = dict(types=list(types.values()), memberTypes=list(member_types.values()))
                return
            field_kinds = {
                'null_value': 'nullptr', 'character': 'char', 'signed_character': 'signed-char',
                'unsigned_character': 'unsigned-char', 'wide_character': 'wchar-t',
                'utf8_character': 'char8-t', 'utf16_character': 'char16-t', 'utf32_character': 'char32-t',
            }
            for name, kind in field_kinds.items():
                source = original(fields[name]['source_type_id'])
                check(label + '-field-' + name + '-original-kind', source['kind'] == kind)
                check(label + '-field-' + name + '-execution-owned', source['execution_type_id'] is not None)
            nullptr_source = original(fields['null_value']['source_type_id'])
            null_execution = db.execute('SELECT p.kind,v.kind FROM types p JOIN types v ON v.id=p.pointee_type_id WHERE p.id=?',
                (nullptr_source['execution_type_id'],)).fetchone()
            check(label + '-nullptr-lowered-to-void-pointer-only-in-execution', tuple(null_execution or ()) == ('pointer', 'void'))
            check(label + '-nullptr-alias-chain-retained', {'Null', 'NullAlias'}.issubset(
                {row['source_name'].split('::')[-1] for row in chain(fields['null_value']['source_type_id'])}))
            qualified = original(fields['qualified_integer']['source_type_id'])
            qualifiers = chain(qualified['underlying_type_id'])
            check(label + '-const-volatile-pointee-retained', any(row['is_const'] for row in qualifiers) and
                  any(row['is_volatile'] for row in qualifiers))
            check(label + '-typedef-chain-retained', {'Integer', 'IntegerAlias', 'QualifiedInteger'}.issubset(
                {row['source_name'].split('::')[-1] for row in qualifiers}))
            check(label + '-restrict-pointer-retained', any(row['is_restrict'] for row in chain(fields['restricted_pointer']['source_type_id'])))
            check(label + '-constant-pointer-retained', any(row['is_const'] for row in chain(fields['constant_pointer']['source_type_id'])))
            check(label + '-pointer-to-const-distinct', not any(row['is_const'] for row in chain(fields['const_pointer']['source_type_id'])) and
                  any(row['is_const'] for row in chain(original(fields['const_pointer']['source_type_id'])['underlying_type_id'])))
            enum_types = {row['type_id']: dict(row) for row in db.execute('SELECT * FROM source_enum_types')}
            enum_fields = ['signed_enum', 'same_storage_enum', 'unsigned_enum', 'huge_enum',
                           'signed_huge_enum', 'opaque_enum', 'unscoped_enum', 'other_signed_enum']
            named_enums = {name: enum_types[original(fields[name]['source_type_id'])['id']] for name in enum_fields}
            check(label + '-all-enums-have-nominal-identity', len({row['identity_key'] for row in named_enums.values()}) == len(enum_fields))
            check(label + '-enum-underlying-types-owned', all(row['underlying_type_id'] in types for row in named_enums.values()))
            check(label + '-scoped-enum-distinction', named_enums['signed_enum']['is_scoped'] == 1 and
                  named_enums['unscoped_enum']['is_scoped'] == 0)
            check(label + '-explicit-enum-base-retained', all(named_enums[name]['has_explicit_base'] == (name != 'unscoped_enum') for name in enum_fields))
            check(label + '-opaque-enum-list-state', named_enums['opaque_enum']['enumerator_list_complete'] == (label == 'provider'))
            expected = {
                'signed_enum': [('Negative', -17), ('Zero', 0), ('Alias', 0), ('Positive', 23)],
                'same_storage_enum': [('Negative', -17), ('Zero', 0), ('Positive', 23)],
                'unsigned_enum': [('Zero', 0), ('High', 1 << 63), ('Maximum', (1 << 64) - 1)],
                'huge_enum': [('Zero', 0), ('Bit100', 1 << 100), ('Maximum', (1 << 128) - 1)],
                'signed_huge_enum': [('Low', -(1 << 100)), ('High', 1 << 100)],
                'opaque_enum': [('Known', 42)] if label == 'provider' else [],
                'unscoped_enum': [('UnscopedNegative', -3), ('UnscopedPositive', 7)],
                'other_signed_enum': [('Different', 91)],
            }
            enumerators = {}
            for name, metadata in named_enums.items():
                values = list(db.execute('SELECT * FROM source_enumerators WHERE type_id=? ORDER BY ordinal', (metadata['type_id'],)))
                check(label + '-' + name + '-exact-enumerators', [(row['source_name'], int(row['value_text'])) for row in values] == expected[name])
                check(label + '-' + name + '-enumerator-order-and-locations', all(row['ordinal'] == ordinal and
                    row['source_file_id'] is not None and row['line'] > 0 for ordinal, row in enumerate(values)))
                enumerators[name] = [dict(row) for row in values]
            signatures = {}
            for name, expected_kinds in [('select', {'nullptr', 'pointer'}), ('character_kind', set(field_kinds.values()) - {'nullptr'}), ('enum_kind', {'enum'})]:
                rows = list(db.execute('SELECT e.id,e.external_key,p.source_type_id FROM entities e JOIN source_parameters p ON p.entity_id=e.id AND p.ordinal=0 WHERE e.source_name=?', (name,)))
                kinds = [original(row['source_type_id'])['kind'] for row in rows]
                check(label + '-' + name + '-original-overload-parameter-kinds', set(kinds) == expected_kinds)
                check(label + '-' + name + '-distinct-overload-identities', len({row['external_key'] for row in rows}) == len(rows) == (3 if name == 'enum_kind' else len(expected_kinds)))
                signatures[name] = sorted((row['external_key'], original(row['source_type_id'])['kind']) for row in rows)
                if label == 'main':
                    calls = list(db.execute('''SELECT n.id,n.line,n.column,e.external_key,p.source_type_id
                        FROM nodes n JOIN entities e ON e.id=n.entity_id
                        JOIN source_parameters p ON p.entity_id=e.id AND p.ordinal=0
                        WHERE n.kind='call' AND e.source_name=? ORDER BY n.line,n.column,n.id''', (name,)))
                    expected_order = {'select': ['nullptr', 'pointer'],
                        'character_kind': ['char', 'signed-char', 'unsigned-char', 'wchar-t', 'char8-t', 'char16-t', 'char32-t'],
                        'enum_kind': ['enum', 'enum', 'enum']}[name]
                    check('main-' + name + '-resolved-call-targets', [original(row['source_type_id'])['kind'] for row in calls] == expected_order and
                        len({row['external_key'] for row in calls}) == len(calls))
                    if name == 'enum_kind':
                        check('main-enum-overload-selects-correct-nominal-type',
                            [original(row['source_type_id'])['id'] for row in calls] ==
                            [named_enums[field]['type_id'] for field in ('signed_enum', 'same_storage_enum', 'other_signed_enum')])
                    check('main-' + name + '-call-source-locations', all(row['line'] > 0 for row in calls))
            snapshots[label] = dict(enums=named_enums, enumerators=enumerators, signatures=signatures,
                                    sourceTypeCount=len(types))

    error = None
    try:
        for optimization in ('0', '2'):
            executable = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++20', '-O' + optimization,
                inputs / 'source_type_information_provider.cpp', inputs / 'source_type_information_main.cpp', '-o', executable])
            run('native-run-O' + optimization, [executable])
        run('native-member-pointer-syntax', ['c++', '-std=c++20', '-fsyntax-only', inputs / 'source_type_information_members.cpp'])
        for label in ('provider', 'main'):
            obj = out / (label + '.dotobj')
            run('export-' + label, [sys.executable, '-B', tooling / 'compile.py', inputs / ('source_type_information_' + label + '.cpp'),
                '--output', obj, '--work-dir', out / ('work-' + label), '--compiler', args.compiler.resolve(), '--std', 'c++20'])
            objects[label] = obj
            run('verify-' + label, ['dotnet', args.linker.resolve(), 'verify', obj])
            inspect(label, obj)
        check('cross-unit-overload-identities-shared', snapshots['provider']['signatures'] == snapshots['main']['signatures'])
        check('cross-unit-enum-identities-shared', {name: row['identity_key'] for name, row in snapshots['provider']['enums'].items()} ==
              {name: row['identity_key'] for name, row in snapshots['main']['enums'].items()})
        inputs.rename(out / 'retained-inputs')
        for label, obj in objects.items():
            run('verify-source-hidden-' + label, ['dotnet', args.linker.resolve(), 'verify', obj])
        run('link-cross-unit-complete-and-opaque-enums', ['dotnet', args.linker.resolve(), 'link', '--out', out / 'linked',
            objects['provider'], objects['main']])
        source_map = json.loads((out / 'linked/source-map.json').read_text())
        check('source-map-retains-source-type-relations', len(source_map['objects']) == 2 and all(
            all(key in obj for key in ('sourceTypes', 'sourceTypeQualifiers', 'sourceEnumTypes', 'sourceEnumerators', 'sourceMemberPointerTypes'))
            for obj in source_map['objects']))
        check('source-map-retains-enum-values', all(any(row['valueText'] == str((1 << 128) - 1) for row in obj['sourceEnumerators'])
            for obj in source_map['objects']))
        check('source-map-retains-restrict-qualifiers', all(any(row['isRestrict'] == 1 for row in obj['sourceTypes'])
            for obj in source_map['objects']))
        spec = importlib.util.spec_from_file_location('source_type_packager', tooling / 'compile.py')
        packager = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(packager)
        def reject(name, source, sql, arguments=()):
            target = out / (name + '.dotobj')
            shutil.copy2(source, target)
            with sqlite3.connect(target) as db:
                statements = [(sql, arguments)] if isinstance(sql, str) else sql
                changed = [db.execute(statement, parameters).rowcount for statement, parameters in statements]
                check(name + '-metadata-mutated', all(count > 0 for count in changed))
                check(name + '-sqlite-foreign-keys', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                db.commit()
            return run(name, ['dotnet', args.linker.resolve(), 'verify', target], False)
        reject('reject-enum-value-out-of-range', objects['provider'],
            "UPDATE source_enumerators SET value_text=? WHERE type_id=? AND ordinal=0",
            (str(1 << 130), snapshots['provider']['enums']['signed_enum']['type_id']))
        reject('reject-enum-noninteger-underlying', objects['provider'],
            "UPDATE source_enum_types SET underlying_type_id=(SELECT id FROM source_types WHERE kind='nullptr' LIMIT 1) WHERE type_id=?",
            (snapshots['provider']['enums']['signed_enum']['type_id'],))
        reject('reject-enum-duplicate-ordinal-gap', objects['provider'],
            'UPDATE source_enumerators SET ordinal=99 WHERE type_id=? AND ordinal=0',
            (snapshots['provider']['enums']['signed_enum']['type_id'],))
        reject('reject-used-nullptr-missing-execution', objects['provider'],
            "UPDATE source_types SET execution_type_id=NULL WHERE kind='nullptr'")
        reject('reject-atomic-qualified-type-with-execution', objects['provider'],
            "UPDATE source_types SET is_atomic=1 WHERE id=(SELECT min(id) FROM source_types WHERE kind='qualified' AND execution_type_id IS NOT NULL)")
        for qualifier in ('unaligned', 'near', 'far'):
            rejected = reject('reject-mapped-' + qualifier + '-qualifier', objects['provider'],
                "INSERT INTO source_type_qualifiers SELECT min(id),? FROM source_types WHERE kind='qualified' AND execution_type_id IS NOT NULL",
                (qualifier,))
            check('reject-mapped-' + qualifier + '-qualifier-diagnostic',
                b'Extended source qualifier requires an owned access or address-space execution contract' in rejected.stderr)
        rejected = reject('reject-unmapped-UPC-qualifier', objects['provider'], [
            ("UPDATE source_types SET execution_type_id=NULL WHERE id=(SELECT min(id) FROM source_types WHERE kind='qualified')", ()),
            ("INSERT INTO source_type_qualifiers SELECT min(id),'upc-shared' FROM source_types WHERE kind='qualified'", ()),
        ])
        check('reject-unmapped-UPC-qualifier-diagnostic',
            b'UPC source qualifiers require an owned block-size and shared-memory type contract' in rejected.stderr)
        mismatch = out / 'enum-definition-mismatch.dotobj'
        shutil.copy2(objects['main'], mismatch)
        with sqlite3.connect(mismatch) as db:
            changed = db.execute("UPDATE source_enumerators SET value_text='24' WHERE type_id=? AND source_name='Positive'",
                (snapshots['main']['enums']['signed_enum']['type_id'],)).rowcount
            check('cross-unit-enum-mismatch-one-value-changed', changed == 1)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            db.commit()
        run('verify-individually-valid-enum-definition-mismatch', ['dotnet', args.linker.resolve(), 'verify', mismatch])
        run('reject-cross-unit-enum-definition-mismatch', ['dotnet', args.linker.resolve(), 'link', '--out', out / 'mismatched-link',
            objects['provider'], mismatch], False)
        obj = out / 'members.dotobj'
        run('export-members', [sys.executable, '-B', tooling / 'compile.py', out / 'retained-inputs/source_type_information_members.cpp',
            '--output', obj, '--work-dir', out / 'work-members', '--compiler', args.compiler.resolve(), '--std', 'c++20'])
        objects['members'] = obj
        run('verify-members', ['dotnet', args.linker.resolve(), 'verify', obj])
        inspect('members', obj)
        orphan_record = reject('reject-physical-record-without-original-execution-mapping', obj,
            "UPDATE source_types SET execution_type_id=NULL WHERE id=(SELECT source_type_id FROM source_record_types WHERE source_name='OtherOwner')")
        check('reject-physical-record-reverse-mapping-diagnostic',
            b"Physical record requires its original type's exact complete execution mapping" in orphan_record.stderr)
        reject('reject-data-member-with-method-ABI-mapping', obj,
            '''UPDATE source_types SET execution_type_id=(SELECT type_id FROM record_types WHERE identity_key='abi:member-function-pointer')
                WHERE id=(SELECT m.type_id FROM source_member_pointer_types m JOIN source_types t ON t.id=m.member_type_id WHERE t.kind='int' LIMIT 1)''')
        reject('reject-method-member-with-integer-mapping', obj,
            '''UPDATE source_types SET execution_type_id=(SELECT id FROM types WHERE kind='i32' LIMIT 1)
                WHERE id=(SELECT m.type_id FROM source_member_pointer_types m JOIN source_types t ON t.id=m.member_type_id WHERE t.kind='routine' LIMIT 1)''')
        member_call = out / 'member-call.dotobj'
        run('export-member-call', [sys.executable, '-B', tooling / 'compile.py', out / 'retained-inputs/source_type_information_member_call.cpp',
            '--output', member_call, '--work-dir', out / 'work-member-call', '--compiler', args.compiler.resolve(), '--std', 'c++20'])
        objects['member-call'] = member_call
        run('verify-member-call', ['dotnet', args.linker.resolve(), 'verify', member_call])
        run('link-source-only-and-observed-member-routines', ['dotnet', args.linker.resolve(), 'link', '--out', out / 'linked-members',
            objects['members'], member_call])
        with sqlite3.connect(member_call) as db:
            targets = list(db.execute('''SELECT t.kind,r.type_id FROM nodes n
                JOIN edges e ON e.parent_id=n.id AND e.role='operand'
                JOIN nodes target ON target.id=e.child_id JOIN types t ON t.id=target.type_id
                LEFT JOIN routine_types r ON r.type_id=t.pointee_type_id WHERE n.kind='indirect-call' '''))
            check('member-call-restores-typed-routine-signature', bool(targets) and all(kind == 'pointer' and signature is not None for kind, signature in targets))
        reject('reject-directly-calling-erased-code-address', member_call,
            '''UPDATE nodes SET type_id=(SELECT p.id FROM types p JOIN types v ON v.id=p.pointee_type_id WHERE p.kind='pointer' AND v.kind='void' LIMIT 1)
                WHERE id=(SELECT e.child_id FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='operand' WHERE n.kind='indirect-call' LIMIT 1)''')
        atomic_result = run('export-atomic-layout-expected-gate', [sys.executable, '-B', tooling / 'compile.py',
            out / 'retained-inputs/source_type_information_atomic.c', '--output', out / 'atomic.dotobj',
            '--work-dir', out / 'work-atomic', '--compiler', args.compiler.resolve(), '--std', 'c11'], False)
        atomic_diagnostic = (out / 'work-atomic/stderr').read_text()
        check('atomic-layout-explicitly-rejected-without-losing-qualifier', 'atomic type' in atomic_diagnostic or
              'atomic source type has no supported execution representation' in atomic_diagnostic)
        c_inputs = out / 'retained-inputs'
        run('native-c-provider', ['cc', '-std=c11', '-c', c_inputs / 'source_type_information_c_provider.c', '-o', out / 'c-provider.o'])
        c_objects, c_enums = {}, {}
        for label, source_name, defines in (
                ('c-provider', 'provider', []), ('c-main', 'main', []),
                ('c-reordered-main', 'main', ['-D', 'SOURCE_TYPE_C_REORDERED=1'])):
            source = c_inputs / ('source_type_information_c_' + source_name + '.c')
            if source_name == 'main':
                binary = out / ('native-' + label)
                run('native-build-' + label, ['cc', '-std=c11', *defines, source, out / 'c-provider.o', '-o', binary])
                run('native-run-' + label, [binary])
            obj = out / (label + '.dotobj')
            run('export-' + label, [sys.executable, '-B', tooling / 'compile.py', source,
                '--output', obj, '--work-dir', out / ('work-' + label), '--compiler', args.compiler.resolve(), '--std', 'c11', *defines])
            c_objects[label] = objects[label] = obj
            run('verify-' + label, ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                db.row_factory = sqlite3.Row
                enums = list(db.execute('''SELECT t.type_id,t.identity_key,e.ordinal,e.source_name,e.value_text
                    FROM source_enum_types t JOIN source_enumerators e ON e.type_id=t.type_id ORDER BY t.type_id,e.ordinal'''))
                anonymous = [dict(row) for row in enums if row['source_name'].startswith('Anonymous')]
                named = [dict(row) for row in enums if row['source_name'].startswith('Named')]
                expected_anonymous = ['AnonymousLow', 'AnonymousMiddle', 'AnonymousHigh']
                expected_named = ['NamedLow', 'NamedHigh']
                if defines:
                    expected_anonymous.reverse()
                    expected_named.reverse()
                check(label + '-retains-original-enumerator-order', [row['source_name'] for row in anonymous] == expected_anonymous and
                      [row['source_name'] for row in named] == expected_named)
                c_enums[label] = dict(anonymous=anonymous, named=named)
        check('c-anonymous-types-retain-distinct-nominal-identities', c_enums['c-provider']['anonymous'][0]['identity_key'] !=
            c_enums['c-main']['anonymous'][0]['identity_key'])
        for label in ('c-main', 'c-reordered-main'):
            run('link-compatible-' + label, ['dotnet', args.linker.resolve(), 'link', '--out', out / ('linked-' + label),
                c_objects['c-provider'], c_objects[label]])
        mismatch_c = out / 'c-enum-definition-mismatch.dotobj'
        shutil.copy2(c_objects['c-main'], mismatch_c)
        with sqlite3.connect(mismatch_c) as db:
            changed = db.execute("UPDATE source_enumerators SET value_text='10' WHERE type_id=? AND source_name='AnonymousHigh'",
                (c_enums['c-main']['anonymous'][0]['type_id'],)).rowcount
            check('c-enum-mismatch-one-value-changed', changed == 1)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            db.commit()
        run('verify-individually-valid-c-enum-mismatch', ['dotnet', args.linker.resolve(), 'verify', mismatch_c])
        run('reject-incompatible-c-enumerators', ['dotnet', args.linker.resolve(), 'link', '--out', out / 'mismatched-c-link',
            c_objects['c-provider'], mismatch_c], False)
        snapshots['c-enums'] = c_enums
        (out / 'retained-inputs').rename(out / 'retained-source')
        run('verify-source-hidden-members', ['dotnet', args.linker.resolve(), 'verify', objects['members']])
        run('verify-source-hidden-member-call', ['dotnet', args.linker.resolve(), 'verify', member_call])
        for label, obj in c_objects.items():
            run('verify-source-hidden-' + label, ['dotnet', args.linker.resolve(), 'verify', obj])
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
        objects={name: dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest()) for name, path in objects.items()},
        compiler=dict(path=str(args.compiler.resolve()), sha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest()),
        linker=dict(path=str(args.linker.resolve()), sha256=hashlib.sha256(args.linker.read_bytes()).hexdigest()),
        metadata=snapshots,
        limitations=['Exporter and semantic-object validation only; C# emission and execution are deliberately outside this fixture.',
                     'Atomic-containing execution layouts remain an explicit expected rejection, not positive atomic export coverage.'])
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)


if __name__ == '__main__':
    raise SystemExit(main())
