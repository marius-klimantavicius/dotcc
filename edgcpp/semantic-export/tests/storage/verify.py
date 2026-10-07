#!/usr/bin/env python3
"""Verify SQLite semantic reconstruction, integrity rejection, and identity.

Consumes existing exports/readers; this test never builds EDG or a managed
reader. Fresh frontend clock records are the only permitted legacy differences.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shlex
import shutil
import sqlite3
import subprocess
import sys

SEMANTIC = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(SEMANTIC))
from sqlite_store import SemanticPackage, SqliteStore, quoted, reference_fields, content_sha256


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--plain', required=True, type=Path)
    parser.add_argument('--probe', required=True, type=Path)
    parser.add_argument('--legacy-root', type=Path, help='Directory containing legacy plain/ and probe/ packages')
    parser.add_argument('--native-fixture', type=Path, help='Raw database made by native-writer.cpp')
    parser.add_argument('--reader', help='Existing managed reader command prefix')
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=False)
    checks = []
    reader = shlex.split(args.reader) if args.reader else None

    def record(name, **facts):
        checks.append({'name': name, **facts})

    def managed(name, package, *, reject=False, emission=False):
        if reader is None:
            return
        output = args.output / (name + '.cs')
        mode = ['emit-csharp', str(package), '--output', str(output)] if emission else ['inspect', str(package)]
        command = [*reader, *mode]
        result = subprocess.run(command, capture_output=True, text=True, timeout=300)
        (args.output / (name + '.command.json')).write_text(json.dumps(command, indent=2) + '\n')
        (args.output / (name + '.stdout')).write_text(result.stdout)
        (args.output / (name + '.stderr')).write_text(result.stderr)
        if (result.returncode != 0) != reject:
            raise AssertionError(f'{name}: unexpected exit {result.returncode}: {result.stderr}')
        if reject and emission and output.exists():
            raise AssertionError('Rejected emission wrote output: ' + str(output))
        record(name, exitCode=result.returncode, outputWritten=output.exists())

    def clock_differences(before, after):
        assert before.keys() == after.keys(), 'Node IDs changed'
        times = []
        macro_ids, text_ids = set(), set()
        for nodes in (before, after):
            time_nodes = [node for node in nodes.values() if node['kind'] == 'macro' and node.get('name') == '__TIME__' and node.get('predefined') is True]
            assert len(time_nodes) == 1
            match = re.fullmatch(r'#define __TIME__ "(\d\d:\d\d:\d\d)"', time_nodes[0]['text'])
            assert match
            times.append(match[1])
            macro_ids.add(time_nodes[0]['id'])
            text_ids.update(edge['target'] for edge in time_nodes[0]['nativeEdges'] if edge['kind'] == 'other-text')
        incoming = {edge['target'] for node in before.values() for edge in node.get('nativeEdges', [])}
        differences = []
        for node_id, old in before.items():
            new = after[node_id]
            if old == new:
                continue
            fields = [key for key in old.keys() | new.keys() if old.get(key) != new.get(key)]
            if node_id in macro_ids:
                assert fields == ['text'] and old['name'] == new['name'] == '__TIME__'
                category = 'predefined __TIME__ macro'
            elif node_id in text_ids:
                assert fields == ['bytesLatin1']
                assert old['bytesLatin1'] == '#define __TIME__ "' + times[0] + '"\0'
                assert new['bytesLatin1'] == '#define __TIME__ "' + times[1] + '"\0'
                category = '__TIME__ macro text payload'
            else:
                assert fields == ['bytesLatin1'] and old['kind'] == new['kind'] == 'other-text' and node_id not in incoming, (node_id, old, new)
                for node, expected_time in zip((old, new), times):
                    match = re.fullmatch(r'(?:Mon|Tue|Wed|Thu|Fri|Sat|Sun) [A-Z][a-z]{2} [ \d]\d (\d\d:\d\d:\d\d) \d{4}\n\x00', node['bytesLatin1'])
                    assert match and match[1] == expected_time, node
                category = 'unreferenced EDG ctime payload'
            differences.append({'node': node_id, 'fields': fields, 'category': category,
                                'before': old[fields[0]], 'after': new[fields[0]]})
        return differences

    for name, path in [('plain', args.plain), ('probe', args.probe)]:
        with SemanticPackage(path) as package:
            assert package.store is not None, 'Storage test needs native SQLite packages'
            nodes = {node['id']: node for node in package.iter_nodes()}
            for node_id, expected in nodes.items():
                assert package.store.get_node(node_id) == expected, node_id
            record(name + '-validated-and-random-access', nodes=len(nodes), contentSha256=package.manifest['contentSha256'])
            if args.legacy_root:
                legacy_path = args.legacy_root / name
                with SemanticPackage(legacy_path) as legacy:
                    old = json.loads(legacy.graph_path.read_text())
                    before = {node['id']: node for node in old['nodes']}
                    differences = clock_differences(before, nodes)
                    assert package.metadata == {key: value for key, value in old.items() if key not in ('nodes', 'macroInvocations', 'diagnostics')}
                    assert list(package.iter_macros()) == old['macroInvocations']
                    assert list(package.iter_diagnostics()) == old['diagnostics']
                    old_sources = {source['node']: source for source in legacy.sources}
                    assert set(old_sources) == {source['node'] for source in package.sources}
                    for source in package.sources:
                        old_source = old_sources[source['node']]
                        assert source['originalPath'] == old_source['originalPath']
                        assert source['available'] == old_source['available']
                        if source['available']:
                            assert package.source_bytes(source) == legacy.source_bytes(old_source)
                    record(name + '-legacy-roundtrip', nodes=len(nodes), clockOnlyDifferences=differences,
                           macros=len(old['macroInvocations']), diagnostics=len(old['diagnostics']), sources=len(package.sources))
            shared = package.store.db.execute('SELECT form_literal_id,count(*) FROM nodes WHERE form_literal_id IS NOT NULL GROUP BY form_literal_id HAVING count(*)>1 LIMIT 1').fetchone()
            assert shared is not None, 'Fixture does not exercise pooled payload reuse'
            ids = [row[0] for row in package.store.db.execute('SELECT node_id FROM nodes WHERE form_literal_id=? ORDER BY node_id LIMIT 2', (shared[0],))]
            first, second = map(package.store.get_node, ids)
            assert first['id'] != second['id'] and first['form'] == second['form'] and first is not second
            record(name + '-literal-sharing-preserves-identities', nodeIds=ids)
        managed(name + '-managed-inspect', path)

    if args.native_fixture:
        with SqliteStore(args.native_fixture, validate=True) as store:
            nodes = {node['id']: node for node in store.iter_nodes()}
            assert len(nodes) == 9
            assert nodes[3]['probeUnsigned'] == (1 << 64) - 1
            assert nodes[3]['probeSigned'] == -(1 << 63)
            assert nodes[3]['probeFlag'] is False and nodes[3]['probeNull'] is None
            assert nodes[3]['supplement'] == {'nested': [False, None, (1 << 64) - 1]}
            for key in ('probeFlag', 'probeNull', 'lateField', 'nativeEdges'):
                assert key not in nodes[4]
            assert nodes[3]['nativeEdges'] == [] and nodes[7]['probeFlag'] is True and nodes[7]['lateField'] == 29
            assert all(nodes[node_id]['payload'] == 'same\0é' for node_id in (3, 4, 7, 8))
            assert list(store.iter_macros())[0]['arguments'] == 'same\0é'
            assert [item['message'] for item in store.iter_diagnostics()] == ['retained diagnostic'] * 2
            nodes[3]['supplement']['nested'].append('mutation')
            assert nodes[7]['supplement'] == {'nested': [False, None, (1 << 64) - 1]}, 'Interning leaked mutable object identity'
            assert store.get_node(3)['supplement'] == nodes[7]['supplement']
            record('native-writer-exact-values', nodes=9, contentSha256=store.content_sha256())
        try:
            SemanticPackage(args.native_fixture)
        except ValueError as error:
            assert 'missing manifest' in str(error)
        else:
            raise AssertionError('Unpackaged native graph accepted as a package')
        record('incomplete-native-package-rejected')
        native_log = args.native_fixture.with_suffix('.log')
        if native_log.is_file():
            log_bytes = native_log.read_bytes()
            assert log_bytes.startswith(b'PASS native semantic SQLite writer:')
            record('native-writer-regression-log', path=str(native_log.resolve()),
                   sha256=hashlib.sha256(log_bytes).hexdigest(),
                   hashCollisionRegression=b'hash collisions' in log_bytes)

    with SemanticPackage(args.plain) as package:
        original = package.graph_path

    def refresh(path):
        with sqlite3.connect(path) as db:
            digest = content_sha256(db)
            manifest = json.loads(db.execute("SELECT value_json FROM package_info WHERE key='manifest'").fetchone()[0])
            manifest['contentSha256'] = digest
            db.execute("UPDATE package_info SET value_json=? WHERE key='manifest'", (json.dumps(manifest),))

    def mutate(name, action, contains, *, rehash=False, check_managed=True):
        target = args.output / (name + '.sqlite')
        shutil.copyfile(original, target)
        with sqlite3.connect(target) as db:
            action(db)
        if rehash:
            refresh(target)
        try:
            with SemanticPackage(target):
                pass
        except (ValueError, KeyError, sqlite3.DatabaseError) as error:
            if contains not in str(error):
                raise AssertionError(f'{name}: expected {contains!r}, got {error}') from error
            record(name, rejected=str(error))
        else:
            raise AssertionError(name + ': malformed database accepted')
        if check_managed:
            managed(name + '-managed', target, reject=True)
        return target

    mutate('unknown-application-id', lambda db: db.execute('PRAGMA application_id=0'), 'application ID')
    mutate('unknown-storage-version', lambda db: db.execute('PRAGMA user_version=999'), 'storage schema')
    mutate('unknown-graph-schema', lambda db: db.execute("UPDATE graph_metadata SET value_json='\"edg-semantic/999\"' WHERE key='schema'"), 'graph schema', rehash=True)
    mutate('missing-graph-metadata', lambda db: db.execute("DELETE FROM graph_metadata WHERE key='pointerBytes'"), 'required graph metadata', rehash=True)
    mutate('dangling-form-literal', lambda db: db.execute('UPDATE nodes SET form_literal_id=999999999 WHERE form_literal_id IS NOT NULL'), 'form literal', rehash=True)
    mutate('wrong-form-literal-kind', lambda db: db.execute("UPDATE nodes SET form_literal_id=(SELECT min(literal_id) FROM literals WHERE encoding='json') WHERE form_literal_id IS NOT NULL"), 'form literal', rehash=True)
    mutate('dangling-native-edge', lambda db: db.execute('UPDATE native_edges SET target_id=999999999 WHERE ordinal=0'), 'native edge', rehash=True)
    mutate('wrong-edge-count', lambda db: db.execute('UPDATE nodes SET edge_count=edge_count+1 WHERE edge_count IS NOT NULL'), 'edge count', rehash=True)
    mutate('missing-subtype', lambda db: db.execute('DELETE FROM node_type WHERE node_id=(SELECT min(node_id) FROM node_type)'), 'subtype', rehash=True)
    mutate('unknown-field-kind', lambda db: db.execute("UPDATE node_fields SET value_kind='unknown' WHERE name='name'"), 'field value kind', rehash=True)
    mutate('invalid-macro-parent', lambda db: db.execute('UPDATE macros SET parent_index=invocation_id WHERE definition_id!=0'), 'macro parent', rehash=True)

    def invalid_flag(db):
        table, name = db.execute("SELECT k.table_name,f.name FROM node_fields f JOIN node_kinds k USING(kind_id) WHERE f.value_kind='boolean' LIMIT 1").fetchone()
        db.execute('UPDATE ' + quoted(table) + ' SET ' + quoted(name) + '=2')
    mutate('invalid-typed-flag', invalid_flag, 'boolean field', rehash=True)

    def invalid_position(db):
        def change(value):
            if isinstance(value, dict):
                if 'sequence' in value:
                    value['source'] = 999999999
                    return True
                return any(change(child) for child in value.values())
            if isinstance(value, list):
                return any(change(child) for child in value)
            return False
        for literal_id, payload in db.execute("SELECT literal_id,payload FROM literals WHERE encoding='json'"):
            value = json.loads(payload)
            if change(value):
                db.execute('UPDATE literals SET payload=? WHERE literal_id=?', (json.dumps(value).encode(), literal_id))
                return
        raise AssertionError('No position literal to mutate')
    mutate('dangling-source-position', invalid_position, 'node reference', rehash=True)

    def dangling_named(db):
        for table, name, typ, kind in db.execute('SELECT k.table_name,f.name,f.value_kind,k.name FROM node_fields f JOIN node_kinds k USING(kind_id)'):
            if name in reference_fields() and typ == 'integer' and not (kind == 'variable' and name == 'parameter'):
                count = db.execute('SELECT count(*) FROM ' + quoted(table) + ' WHERE ' + quoted(name) + ' IS NOT NULL').fetchone()[0]
                if count:
                    db.execute('UPDATE ' + quoted(table) + ' SET ' + quoted(name) + '=999999999 WHERE ' + quoted(name) + ' IS NOT NULL')
                    return
        raise AssertionError('No reference field to mutate')
    mutate('dangling-named-reference', dangling_named, 'named reference', rehash=True)
    mutate('producer-hash', lambda db: db.execute("UPDATE package_info SET value_json='{}' WHERE key='producer'"), 'Producer SHA256')
    def rehashed_producer(db, field, value):
        producer = json.loads(db.execute("SELECT value_json FROM package_info WHERE key='producer'").fetchone()[0])
        producer[field] = value
        text = json.dumps(producer)
        manifest = json.loads(db.execute("SELECT value_json FROM package_info WHERE key='manifest'").fetchone()[0])
        manifest['producer']['sha256'] = hashlib.sha256(text.encode('utf-8')).hexdigest()
        db.execute("UPDATE package_info SET value_json=? WHERE key='producer'", (text,))
        db.execute("UPDATE package_info SET value_json=? WHERE key='manifest'", (json.dumps(manifest),))
    mutate('unknown-producer-schema', lambda db: rehashed_producer(db, 'schema', 'edg-semantic-producer/999'), 'producer schema')
    mutate('producer-invocation-mismatch', lambda db: rehashed_producer(db, 'binarySha256', '0' * 64), 'binary SHA256')
    mutate('content-hash', lambda db: db.execute("UPDATE graph_metadata SET value_json='\"changed\"' WHERE key='producerVersion'"), 'Content SHA256')
    mutate('source-blob-hash', lambda db: db.execute("UPDATE source_blobs SET bytes=cast(bytes || x'00' AS BLOB)"), 'Source SHA256', rehash=True)
    mutate('source-blob-missing', lambda db: db.execute('DELETE FROM source_blobs'), 'Missing source BLOB', rehash=True)
    mutate('source-table-mismatch', lambda db: db.execute("UPDATE source_files SET original_path='changed.c'"), 'Source manifest/table', rehash=True)

    def wrong_source_path(db):
        manifest = json.loads(db.execute("SELECT value_json FROM package_info WHERE key='manifest'").fetchone()[0])
        for source in manifest['sources']:
            source['originalPath'] = 'changed.c'
        db.execute("UPDATE source_files SET original_path='changed.c'")
        db.execute("UPDATE package_info SET value_json=? WHERE key='manifest'", (json.dumps(manifest),))
    mutate('source-node-path-mismatch', wrong_source_path, 'Source path disagrees', rehash=True)

    # Diagnostics remain explicit storage facts and must stop emission before
    # an output file is opened. This is a well-formed package, not a hash failure.
    diagnostic_package = args.output / 'emission-diagnostic.sqlite'
    shutil.copyfile(original, diagnostic_package)
    with sqlite3.connect(diagnostic_package) as db:
        def literal(text):
            payload = text.encode('utf-8')
            existing = db.execute("SELECT literal_id FROM literals WHERE encoding='text' AND payload=?", (payload,)).fetchone()
            if existing:
                return existing[0]
            columns = {row[1] for row in db.execute('PRAGMA table_info(literals)')}
            if 'payload_hash' in columns:
                return db.execute("INSERT INTO literals(encoding,payload_hash,payload) VALUES('text',0,?)", (payload,)).lastrowid
            return db.execute("INSERT INTO literals(encoding,payload) VALUES('text',?)", (payload,)).lastrowid
        node_id = db.execute('SELECT min(node_id) FROM nodes').fetchone()[0]
        db.execute('INSERT INTO diagnostics VALUES(1,0,?,?,?)', (node_id, literal('SEM001'), literal('storage rejection regression')))
    refresh(diagnostic_package)
    with SemanticPackage(diagnostic_package) as package:
        assert len(list(package.iter_diagnostics())) == 1
    record('diagnostics-survive-storage')
    managed('diagnostics-stop-emission', diagnostic_package, reject=True, emission=True)

    # A committed concurrent update must not change the validated reader view.
    # WAL is confined to this disposable test copy; published packages use DELETE.
    snapshot_package = args.output / 'read-snapshot.sqlite'
    shutil.copyfile(original, snapshot_package)
    with sqlite3.connect(snapshot_package) as db:
        db.execute('PRAGMA journal_mode=WAL')
    with SqliteStore(snapshot_package) as old_view:
        before = old_view.content_sha256()
        with sqlite3.connect(snapshot_package) as writer:
            writer.execute("UPDATE graph_metadata SET value_json='\"concurrent change\"' WHERE key='producerVersion'")
        assert old_view.content_sha256() == before, 'Reader lost its validated SQLite snapshot'
        with SqliteStore(snapshot_package) as new_view:
            assert new_view.content_sha256() != before, 'Concurrent writer did not commit'
    record('stable-read-snapshot')

    result = {'schema': 'edg-semantic-storage-tests/1', 'passed': len(checks), 'managedReaderTested': reader is not None,
              'nativeFixtureTested': args.native_fixture is not None, 'checks': checks}
    (args.output / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print('PASS semantic SQLite storage:', len(checks), 'checks')


if __name__ == '__main__':
    main()
