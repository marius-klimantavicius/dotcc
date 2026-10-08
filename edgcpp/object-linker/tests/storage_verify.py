"""Linked storage, relocation, callback, record and macro-stack execution."""
import json
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute):
    provider = compile_source('storage-provider', inputs / 'storage_provider.c', std='c11')
    consumer = compile_source('storages', inputs / 'storages.c', std='c11')
    run('native-storages-build', ['cc', '-std=c11', inputs / 'storage_provider.c',
        inputs / 'storages.c', '-o', out / 'native-storages'])
    run('native-storages-run', [out / 'native-storages'])
    for label, obj in [('provider', provider), ('consumer', consumer)]:
        run('verify-storage-' + label, ['dotnet', tool, 'verify', obj])
    directory = link('link-storages', [provider, consumer])
    execute('storages', directory)
    with sqlite3.connect(provider) as db:
        check('storage-owned-binary-data', db.execute('SELECT count(*) FROM storage_data WHERE typeof(bytes)=\'blob\'').fetchone()[0] > 0)
        check('storage-data-relocations', db.execute('SELECT count(*) FROM relocations WHERE target_storage_id IS NOT NULL').fetchone()[0] >= 3)
        check('storage-function-relocations', db.execute('SELECT count(*) FROM relocations WHERE target_entity_id IS NOT NULL').fetchone()[0] >= 2)
        check('storage-tls-recorded', db.execute('SELECT count(*) FROM storages WHERE is_thread_local=1').fetchone()[0] >= 1)
        check('storage-callback-signatures', db.execute('SELECT count(*) FROM routine_types').fetchone()[0] >= 1)
        check('storage-indirect-calls', db.execute("SELECT count(*) FROM nodes WHERE kind='indirect-call'").fetchone()[0] >= 1)
        check('storage-bitfield-metadata', db.execute('SELECT count(*) FROM record_fields WHERE bit_width IS NOT NULL').fetchone()[0] == 3)
    with sqlite3.connect(consumer) as db:
        check('storage-macro-stack-events', db.execute("SELECT count(*) FROM macro_events WHERE kind IN ('push','pop')").fetchone()[0] == 6)
        check('storage-incomplete-record-retained', db.execute("SELECT count(*) FROM record_types WHERE source_name='Opaque' AND size=0").fetchone()[0] == 1)
    generated = '\n'.join(path.read_text() for path in directory.glob('*.cs'))
    check('storage-managed-function-pointers', 'delegate* managed<' in generated)
    check('storage-source-function-names', 'run_operation' in generated and 'opaque_read' in generated)
    check('storage-macro-map-stack', any(event['kind'] == 'pop' for obj in
        json.loads((directory / 'macro-map.json').read_text())['objects'] for event in obj['macroEvents']))
    return provider, consumer
