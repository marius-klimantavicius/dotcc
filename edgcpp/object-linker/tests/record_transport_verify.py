"""Construct/transport records while preserving nontrivial assignment and ABI."""
import shutil
import sqlite3

FILES = ('record_transport.cpp', 'record_transport_shared.h', 'record_transport_construct.cpp',
         'record_transport_assign.cpp', 'record_transport_incomplete.cpp', 'record_transport_main.cpp')


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    directory = out / 'record-transport-source'
    directory.mkdir()
    for name in FILES:
        shutil.copy2(inputs / name, directory / name)
    single = directory / FILES[0]
    multiple = [directory / name for name in FILES[2:]]
    for label, sources in [('single', [single]), ('cross', multiple)]:
        for optimization in ('0', '2'):
            binary = out / ('record-transport-' + label + '-native-O' + optimization)
            run('record-transport-' + label + '-native-build-O' + optimization,
                ['c++', '-std=c++17', '-O' + optimization, *sources, '-o', binary])
            run('record-transport-' + label + '-native-run-O' + optimization, [binary])
    obj = compile_source('record-transport', single, extra=['--no-exceptions', '--no-rtti'])
    run('record-transport-verify', ['dotnet', tool, 'verify', obj])
    with sqlite3.connect(obj) as db:
        facts = dict((row[0], row[1:]) for row in db.execute('''SELECT s.source_name,
            s.is_trivially_copyable,o.bitwise_construction_allowed,o.bitwise_assignment_allowed,o.requires_return_storage
            FROM source_record_types s JOIN source_record_operations o ON o.source_record_id=s.id'''))
        check('record-transport-separate-construction-assignment', facts['Value'] == (0, 1, 0, 0))
        check('record-transport-ordinary-record-explicit-permission', facts['PlainValue'] == (1, 1, 1, 0))
        check('record-transport-deleted-copy-requires-result-storage', facts['DeletedCopy'][0] == 1 and facts['DeletedCopy'][3] == 1)
        check('record-transport-nontrivial-copy-return-storage', facts['ExplicitCopy'][1] == 0 and facts['ExplicitCopy'][3] == 1)
        check('record-transport-nontrivial-destructor-return-storage', facts['GuardValue'][3] == 1)
        check('record-transport-owned-construction-transfers', db.execute("SELECT count(*) FROM record_value_transfers WHERE kind='construction'").fetchone()[0] > 0)
    objects = []
    cross_facts = []
    for source in multiple:
        name = source.stem.replace('_', '-')
        item = compile_source(name, source, extra=['--no-exceptions', '--no-rtti'])
        objects.append(item)
        run(name + '-verify', ['dotnet', tool, 'verify', item])
        with sqlite3.connect(item) as db:
            rows = db.execute('''SELECT o.bitwise_construction_allowed,o.bitwise_assignment_allowed,o.requires_return_storage
                FROM source_record_types s JOIN source_record_operations o ON o.source_record_id=s.id
                WHERE s.source_name='CrossValue' ''').fetchall()
            if source.name == 'record_transport_incomplete.cpp':
                check('record-transport-incomplete-has-no-operation-facts', not rows)
            else:
                check(name + '-has-one-complete-shared-class', len(rows) == 1)
                cross_facts.extend(rows)
    check('record-transport-cross-unit-facts-agree', len(cross_facts) == 3 and set(cross_facts) == {(1, 0, 0)})
    bad = mutate('record-transport-cross-operation-mismatch', objects[1], [
        ("UPDATE source_record_operations SET bitwise_assignment_allowed=1 WHERE source_record_id=(SELECT id FROM source_record_types WHERE source_name='CrossValue')", ())])
    run('record-transport-cross-mismatch-individually-valid', ['dotnet', tool, 'verify', bad])
    run('record-transport-cross-mismatch-rejected', ['dotnet', tool, 'link',
        '--out', out / 'record-transport-cross-mismatch-output', '--entry', 'main',
        objects[0], bad, *objects[2:]], None, 'Record layout mismatch')
    directory.rename(out / 'retained-record-transport-source')
    execute('record-transport', link('link-record-transport-source-hidden', [obj]))
    execute('record-transport-cross', link('link-record-transport-cross-source-hidden', objects))
    return obj
