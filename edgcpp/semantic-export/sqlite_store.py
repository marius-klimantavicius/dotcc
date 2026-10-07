"""Bounded-memory access to semantic SQLite stores and legacy JSON packages.

SQLite literals share payload bytes only. Returned nodes retain their distinct
identities, and structured literal values are decoded afresh for each use.
"""
from collections import Counter
from functools import lru_cache
import hashlib
import json
from pathlib import Path
import re
import sqlite3
import struct

APPLICATION_ID = 1162102611
STORAGE_VERSION = 1
ROOT = Path(__file__).resolve().parent


def sha256_file(path):
    result = hashlib.sha256()
    with Path(path).open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            result.update(block)
    return result.hexdigest()


def quoted(name):
    return '"' + name.replace('"', '""') + '"'


def reference_fields():
    text = (ROOT / 'managed/SemanticGraph.cs').read_text()
    return set(re.findall(r'"([A-Za-z]+)"', text.split('ReferenceFields =', 1)[1].split('};', 1)[0]))


class JsonStream:
    def __init__(self, stream):
        self.stream, self.buffer, self.offset = stream, '', 0
        self.decoder = json.JSONDecoder()

    def more(self):
        data = self.stream.read(1024 * 1024)
        self.buffer = self.buffer[self.offset:] + data
        self.offset = 0
        if not data:
            raise ValueError('Truncated graph JSON')

    def peek(self):
        while True:
            while self.offset < len(self.buffer) and self.buffer[self.offset].isspace():
                self.offset += 1
            if self.offset < len(self.buffer):
                return self.buffer[self.offset]
            self.more()

    def token(self, expected):
        if self.peek() != expected:
            raise ValueError('Expected JSON token ' + expected)
        self.offset += 1

    def value(self):
        self.peek()
        while True:
            try:
                value, self.offset = self.decoder.raw_decode(self.buffer, self.offset)
                return value
            except json.JSONDecodeError:
                self.more()


def iter_json_records(path):
    """Yield ('metadata', (key,value)) or (array-name, record) without a DOM."""
    with Path(path).open() as stream:
        parser = JsonStream(stream)
        parser.token('{')
        while parser.peek() != '}':
            key = parser.value()
            parser.token(':')
            if key in ('nodes', 'macroInvocations', 'diagnostics'):
                parser.token('[')
                while parser.peek() != ']':
                    yield key, parser.value()
                    if parser.peek() != ']':
                        parser.token(',')
                parser.token(']')
            else:
                yield 'metadata', (key, parser.value())
            if parser.peek() != '}':
                parser.token(',')
        parser.token('}')


class SqliteStore:
    def __init__(self, path, unit_id=1, validate=False):
        self.path = Path(path).resolve()
        self.unit_id = unit_id
        self.db = sqlite3.connect(self.path.as_uri() + '?mode=ro', uri=True)
        self.connection = self.db
        self.db.execute('PRAGMA query_only=ON')
        self.db.execute('PRAGMA trusted_schema=OFF')
        self.db.execute('PRAGMA cache_size=-8192')
        self.db.execute('PRAGMA temp_store=FILE')
        # Pin the same read snapshot through hash validation and later queries.
        self.db.execute('BEGIN')
        self._literal_record = lru_cache(maxsize=4096)(self._read_literal_record)
        self._require_reference = lru_cache(maxsize=16384)(self._require_reference)
        try:
            if self.db.execute('PRAGMA application_id').fetchone()[0] != APPLICATION_ID:
                raise ValueError('Unsupported semantic SQLite application ID')
            if self.db.execute('PRAGMA user_version').fetchone()[0] != STORAGE_VERSION:
                raise ValueError('Unsupported semantic SQLite storage schema version')
            self.kinds = {}
            self.fields = {}
            for kind_id, name, table in self.db.execute('SELECT kind_id,name,table_name FROM node_kinds ORDER BY kind_id'):
                if kind_id <= 0 or not table.startswith('node_') or table in ('node_fields', 'node_kinds'):
                    raise ValueError('Invalid node kind/table descriptor')
                self.kinds[kind_id] = (name, table)
                fields = list(self.db.execute('SELECT name,value_kind FROM node_fields WHERE kind_id=? ORDER BY ordinal', (kind_id,)))
                if any(typ not in ('integer', 'boolean', 'text', 'json', 'null') for _, typ in fields):
                    raise ValueError('Unsupported node field value kind')
                if any(name in ('id', 'kind', 'form', 'nativeEdges', 'unit_id', 'node_id') for name, _ in fields):
                    raise ValueError('Reserved field in node subtype descriptor')
                actual = [row[1] for row in self.db.execute('PRAGMA table_info(' + quoted(table) + ')')]
                if actual != ['unit_id', 'node_id', *[name for name, _ in fields]]:
                    raise ValueError('Node subtype columns disagree with node_fields: ' + table)
                self.fields[kind_id] = fields
            self.metadata = {key: json.loads(value) for key, value in self.db.execute(
                'SELECT key,value_json FROM graph_metadata WHERE unit_id=? ORDER BY key', (unit_id,))}
            required = {'schema': str, 'lowered': bool, 'completeForEmission': bool,
                        'producerVersion': str, 'sourceLanguage': str, 'standardVersion': int,
                        'rootScope': int, 'primarySource': int, 'pointerBytes': int,
                        'intBytes': int, 'longBytes': int, 'longDoubleBytes': int,
                        'littleEndian': bool, 'plainCharSigned': bool, 'configuration': dict}
            if any(type(self.metadata.get(key)) is not typ for key, typ in required.items()):
                raise ValueError('Missing or invalid required graph metadata')
            if any(self.metadata[key] <= 0 for key in ('rootScope', 'primarySource', 'pointerBytes', 'intBytes', 'longBytes', 'longDoubleBytes')):
                raise ValueError('Invalid required graph metadata: roots and ABI byte sizes must be positive')
            if self.metadata.get('schema') != 'edg-semantic/1':
                raise ValueError('Unsupported semantic graph schema')
            if self.metadata.get('lowered') is not False:
                raise ValueError('Expected unlowered semantic graph')
            if validate:
                self.validate()
        except Exception:
            self.close()
            raise

    def close(self):
        self._literal_record.cache_clear()
        self._require_reference.cache_clear()
        self.db.close()

    def __enter__(self):
        return self

    def __exit__(self, *unused):
        self.close()

    def _read_literal_record(self, literal_id):
        row = self.db.execute('SELECT encoding,payload FROM literals WHERE literal_id=?', (literal_id,)).fetchone()
        if row is None:
            raise ValueError('Dangling literal ID ' + str(literal_id))
        if not isinstance(row[1], bytes):
            raise ValueError('Literal payload must be a BLOB')
        return row

    def literal(self, literal_id, expected):
        encoding, payload = self._literal_record(literal_id)
        if encoding != expected:
            raise ValueError('Literal encoding mismatch for ' + str(literal_id))
        text = payload.decode('utf-8')
        return text if encoding == 'text' else json.loads(text)

    @staticmethod
    def integer(value):
        if isinstance(value, int):
            return value
        if isinstance(value, bytes) and re.fullmatch(rb'-?(0|[1-9][0-9]*)', value):
            result = int(value)
            if not -(1 << 63) <= result < (1 << 63):
                return result
        raise ValueError('Invalid integer storage value')

    def _decode_node(self, base, row):
        node_id, kind_id, form, edge_count = base
        if row is None or row[0] != node_id:
            raise ValueError('Missing or mismatched node subtype row for ' + str(node_id))
        node = {'id': node_id, 'kind': self.kinds[kind_id][0]}
        if form is not None:
            node['form'] = self.literal(form, 'text')
        for (name, typ), value in zip(self.fields[kind_id], row[1:]):
            if value is None:
                continue
            if typ == 'integer':
                node[name] = self.integer(value)
            elif typ == 'boolean':
                if value not in (0, 1):
                    raise ValueError('Invalid boolean field ' + name)
                node[name] = bool(value)
            elif typ == 'null':
                if value != 0:
                    raise ValueError('Invalid explicit null field ' + name)
                node[name] = None
            else:
                node[name] = self.literal(value, typ)
        return node

    def get_node(self, node_id):
        base = self.db.execute('SELECT node_id,kind_id,form_literal_id,edge_count FROM nodes WHERE unit_id=? AND node_id=?', (self.unit_id, node_id)).fetchone()
        if base is None:
            raise ValueError('Unknown node ID ' + str(node_id))
        table = self.kinds[base[1]][1]
        columns = ','.join(quoted(name) for name, _ in self.fields[base[1]])
        row = self.db.execute('SELECT node_id' + (',' + columns if columns else '') + ' FROM ' + quoted(table) + ' WHERE unit_id=? AND node_id=?', (self.unit_id, node_id)).fetchone()
        node = self._decode_node(base, row)
        if base[3] is not None:
            edges = []
            for ordinal, target, kind in self.db.execute('SELECT ordinal,target_id,target_kind_id FROM native_edges WHERE unit_id=? AND source_id=? ORDER BY ordinal', (self.unit_id, node_id)):
                if ordinal != len(edges):
                    raise ValueError('Invalid native edge ordinal')
                edges.append({'target': target, 'kind': self.kinds[kind][0]})
            if len(edges) != base[3]:
                raise ValueError('Native edge count mismatch')
            node['nativeEdges'] = edges
        return node

    def iter_nodes(self, kind=None):
        selected = {key: value for key, value in self.kinds.items() if kind is None or value[0] == kind}
        readers = {}
        heads = {}
        for key, (_, table) in selected.items():
            columns = ','.join(quoted(name) for name, _ in self.fields[key])
            readers[key] = iter(self.db.execute('SELECT node_id' + (',' + columns if columns else '') + ' FROM ' + quoted(table) + ' WHERE unit_id=? ORDER BY node_id', (self.unit_id,)))
            heads[key] = next(readers[key], None)
        sql = 'SELECT node_id,kind_id,form_literal_id,edge_count FROM nodes WHERE unit_id=?'
        params = [self.unit_id]
        if kind is not None:
            if not selected:
                return
            sql += ' AND kind_id=?'
            params.append(next(iter(selected)))
        # A streaming merge avoids per-node SQL queries, including for edges.
        edges = iter(self.db.execute('SELECT source_id,ordinal,target_id,target_kind_id FROM native_edges WHERE unit_id=? ORDER BY source_id,ordinal', (self.unit_id,))) if kind is None else None
        edge = next(edges, None) if edges is not None else None
        for base in self.db.execute(sql + ' ORDER BY node_id', params):
            node_id, key, _, edge_count = base
            node = self._decode_node(base, heads[key])
            heads[key] = next(readers[key], None)
            if kind is not None:
                if edge_count is not None:
                    node['nativeEdges'] = self.get_node(node_id)['nativeEdges']
            elif edge_count is not None:
                values = []
                while edge is not None and edge[0] == node_id:
                    if edge[1] != len(values):
                        raise ValueError('Invalid native edge ordinal')
                    values.append({'target': edge[2], 'kind': self.kinds[edge[3]][0]})
                    edge = next(edges, None)
                if len(values) != edge_count:
                    raise ValueError('Native edge count mismatch')
                node['nativeEdges'] = values
            yield node
        if edge is not None or any(value is not None for value in heads.values()):
            raise ValueError('Orphan subtype row or native edge')

    def iter_macros(self):
        for index, parent, definition, start, end, arguments in self.db.execute('SELECT invocation_id,parent_index,definition_id,start_literal_id,end_literal_id,arguments_literal_id FROM macros WHERE unit_id=? ORDER BY invocation_id', (self.unit_id,)):
            yield {'index': index, 'parentIndex': parent, 'definition': definition,
                   'start': self.literal(start, 'json'), 'end': self.literal(end, 'json'),
                   'arguments': self.literal(arguments, 'text')}

    def iter_diagnostics(self):
        for node, code, message in self.db.execute('SELECT node_id,code_literal_id,message_literal_id FROM diagnostics WHERE unit_id=? ORDER BY ordinal', (self.unit_id,)):
            yield {'node': node, 'code': self.literal(code, 'text'), 'message': self.literal(message, 'text')}

    def _reject(self, sql, message, parameters=()):
        if self.db.execute(sql + ' LIMIT 1', parameters).fetchone() is not None:
            raise ValueError(message)

    def _require_reference(self, node_id, kind=None):
        if type(node_id) is not int or node_id < 0:
            raise ValueError('Invalid node reference')
        if node_id == 0:
            return
        row = self.db.execute('SELECT kind_id FROM nodes WHERE unit_id=? AND node_id=?', (self.unit_id, node_id)).fetchone()
        if row is None or (kind is not None and self.kinds[row[0]][0] != kind):
            raise ValueError('Dangling or wrong-kind node reference ' + str(node_id))

    def validate(self):
        if self.db.execute('PRAGMA quick_check').fetchone()[0] != 'ok':
            raise ValueError('SQLite integrity check failed')
        self._reject('SELECT 1 FROM nodes n LEFT JOIN node_kinds k USING(kind_id) LEFT JOIN units u USING(unit_id) WHERE n.node_id<=0 OR k.kind_id IS NULL OR u.unit_id IS NULL OR n.edge_count<0', 'Invalid node identity/kind/unit')
        self._reject("SELECT 1 FROM nodes n LEFT JOIN literals l ON l.literal_id=n.form_literal_id WHERE n.form_literal_id IS NOT NULL AND (l.literal_id IS NULL OR l.encoding!='text')", 'Dangling or wrong-kind form literal')
        self._reject('SELECT 1 FROM native_edges e LEFT JOIN nodes s ON s.unit_id=e.unit_id AND s.node_id=e.source_id LEFT JOIN nodes t ON t.unit_id=e.unit_id AND t.node_id=e.target_id WHERE s.node_id IS NULL OR t.node_id IS NULL OR t.kind_id!=e.target_kind_id OR e.ordinal<0', 'Dangling or wrong-kind native edge')
        self._reject('SELECT 1 FROM nodes n LEFT JOIN (SELECT unit_id,source_id,count(*) c,min(ordinal) lo,max(ordinal) hi FROM native_edges GROUP BY unit_id,source_id) e ON e.unit_id=n.unit_id AND e.source_id=n.node_id WHERE coalesce(n.edge_count,0)!=coalesce(e.c,0) OR e.lo!=0 OR e.hi!=e.c-1 OR (n.edge_count IS NULL AND e.c>0)', 'Native edge count/ordinal mismatch')
        fields = reference_fields()
        for key, (kind, table) in self.kinds.items():
            table = quoted(table)
            self._reject(f'SELECT 1 FROM nodes n LEFT JOIN {table} s USING(unit_id,node_id) WHERE n.unit_id=? AND n.kind_id={key} AND s.node_id IS NULL', 'Missing node subtype row', (self.unit_id,))
            self._reject(f'SELECT 1 FROM {table} s LEFT JOIN nodes n USING(unit_id,node_id) WHERE n.kind_id IS NULL OR n.kind_id!={key}', 'Wrong-kind or orphan node subtype row')
            for name, typ in self.fields[key]:
                column = 's.' + quoted(name)
                if typ in ('text', 'json'):
                    self._reject(f'SELECT 1 FROM {table} s LEFT JOIN literals l ON l.literal_id={column} WHERE {column} IS NOT NULL AND (l.literal_id IS NULL OR l.encoding!=?)', 'Dangling or wrong-kind literal in ' + name, (typ,))
                if name in fields and not (kind == 'variable' and name == 'parameter'):
                    if typ != 'integer':
                        raise ValueError('Invalid reference field type: ' + name)
                    self._reject(f'SELECT 1 FROM {table} s LEFT JOIN nodes t ON t.unit_id=s.unit_id AND t.node_id={column} WHERE {column} IS NOT NULL AND {column}!=0 AND t.node_id IS NULL', 'Dangling named reference ' + name)
                if typ in ('boolean', 'null'):
                    allowed = '(0,1)' if typ == 'boolean' else '(0)'
                    self._reject(f'SELECT 1 FROM {table} s WHERE {column} IS NOT NULL AND (typeof({column})!="integer" OR {column} NOT IN {allowed})', 'Invalid ' + typ + ' field ' + name)
                elif typ == 'integer':
                    self._reject(f'SELECT 1 FROM {table} s WHERE {column} IS NOT NULL AND typeof({column}) NOT IN ("integer","blob")', 'Invalid integer field ' + name)
                    for value, in self.db.execute(f'SELECT {column} FROM {table} s WHERE typeof({column})="blob"'):
                        self.integer(value)
        for name, kind in [('rootScope', 'scope'), ('primarySource', 'source-file'), ('mainRoutine', 'routine')]:
            self._require_reference(self.metadata.get(name, 0), kind)
        if not self.metadata.get('rootScope') or not self.metadata.get('primarySource'):
            raise ValueError('Missing root scope or primary source')
        for literal_id, encoding, payload in self.db.execute('SELECT literal_id,encoding,payload FROM literals'):
            if encoding not in ('text', 'json') or not isinstance(payload, bytes):
                raise ValueError('Invalid literal encoding or payload')
            text = payload.decode('utf-8')
            if encoding == 'json':
                value = json.loads(text)
                if not isinstance(value, (dict, list)):
                    raise ValueError('Structured literal must be an object or array')
                self._validate_positions(value)
        for macro in self.iter_macros():
            self._require_reference(macro['definition'], 'macro')
            index, parent = macro['index'], macro['parentIndex']
            if index <= 0 or ((parent < 0) != (macro['definition'] == 0)):
                raise ValueError('Invalid macro placeholder/index')
            if parent > 0:
                row = self.db.execute('SELECT definition_id FROM macros WHERE unit_id=? AND invocation_id=?', (self.unit_id, parent)).fetchone()
                if parent >= index or row is None or row[0] == 0:
                    raise ValueError('Unknown macro parent or invalid parent order')
        for item in self.iter_diagnostics():
            self._require_reference(item['node'])

    def _validate_positions(self, value):
        if isinstance(value, list):
            for item in value:
                self._validate_positions(item)
        elif isinstance(value, dict):
            if 'sequence' in value:
                if any(type(value.get(key)) is not int or value[key] < 0 for key in ('sequence', 'column', 'line')):
                    raise ValueError('Invalid source position')
                self._require_reference(value.get('source'), 'source-file')
            if 'macroContext' in value:
                context = value['macroContext']
                if type(context) is not int or context < 0 or (context and self.db.execute('SELECT 1 FROM macros WHERE unit_id=? AND invocation_id=?', (self.unit_id, context)).fetchone() is None):
                    raise ValueError('Unknown position macro context')
            for child in value.values():
                self._validate_positions(child)

    def content_sha256(self):
        return content_sha256(self.db)

    def inventory(self, validate=True):
        if validate:
            self.validate()
        unit = (self.unit_id,)
        kinds = {name: count for name, count in self.db.execute('SELECT k.name,count(*) c FROM nodes n JOIN node_kinds k USING(kind_id) WHERE unit_id=? GROUP BY k.kind_id ORDER BY c DESC,k.name', unit)}
        forms = {kind + ':' + payload.decode('utf-8'): count for kind, payload, count in self.db.execute('SELECT k.name,l.payload,count(*) c FROM nodes n JOIN node_kinds k USING(kind_id) JOIN literals l ON l.literal_id=n.form_literal_id WHERE unit_id=? GROUP BY k.kind_id,n.form_literal_id ORDER BY c DESC,k.name,n.form_literal_id', unit)}
        gaps, examples = {}, {}
        for payload, count, node in self.db.execute('SELECT l.payload,count(*) c,min(d.node_id) FROM diagnostics d JOIN literals l ON l.literal_id=d.message_literal_id WHERE unit_id=? GROUP BY d.message_literal_id ORDER BY c DESC,d.message_literal_id', unit):
            message = payload.decode('utf-8')
            gaps[message] = count
            examples[message] = node
        header = dict(self.metadata)
        header['macroInvocationCount'] = self.db.execute('SELECT count(*) FROM macros WHERE unit_id=?', unit).fetchone()[0]
        edges = self.db.execute('SELECT count(*),count(DISTINCT target_id) FROM native_edges WHERE unit_id=?', unit).fetchone()
        return {'header': header, 'nodeCount': sum(kinds.values()), 'nodeKinds': kinds,
                'forms': forms, 'adapterDiagnosticCount': sum(gaps.values()),
                'adapterDiagnostics': gaps, 'diagnosticExampleNodes': examples,
                'referenceClosure': {'nativeEdgeCount': edges[0], 'distinctNativeReferencedIds': edges[1],
                                     'duplicateNodeIds': 0, 'missingCount': 0, 'missingIdExamples': [],
                                     'scope': 'SQL joins validate native edges, named node references, and pooled source positions; not full ABI/emission validation'}}


class SemanticPackage:
    def __init__(self, path, validate=True):
        path = Path(path).resolve()
        if path.is_dir():
            path = path / ('semantic.sqlite' if (path / 'semantic.sqlite').is_file() else 'package.json')
        self.store = None
        self.path = path
        self.base = path.parent
        try:
            with path.open('rb') as stream:
                sqlite_file = stream.read(16) == b'SQLite format 3\0'
            if sqlite_file:
                self.graph_path = path
                self.store = SqliteStore(path)
                row = self.store.db.execute("SELECT value_json FROM package_info WHERE key='manifest'").fetchone()
                if row is None:
                    raise ValueError('Incomplete SQLite package: missing manifest')
                self.manifest = json.loads(row[0])
                if self.manifest.get('schema') != 'edg-semantic-package/2' or self.manifest.get('databaseFormat') != 'edg-semantic-sqlite/1':
                    raise ValueError('Unsupported semantic package schema/storage format')
                if self.manifest.get('unitId') != self.store.unit_id:
                    raise ValueError('Package unit ID mismatch')
                self.metadata = self.store.metadata
            else:
                self.manifest = json.loads(path.read_text())
                if self.manifest.get('schema') != 'edg-semantic-package/1':
                    raise ValueError('Unsupported legacy semantic package schema')
                self.graph_path = self._member(self.manifest['graph']['path'])
                self.metadata = {}
                for section, value in iter_json_records(self.graph_path):
                    if section != 'metadata':
                        break
                    self.metadata[value[0]] = value[1]
            self.sources = self.manifest['sources']
            if validate:
                self.validate()
        except Exception:
            self.close()
            raise

    def _member(self, value):
        path = (self.base / value).resolve()
        if not path.is_relative_to(self.base):
            raise ValueError('File path escapes package')
        return path

    def close(self):
        if self.store is not None:
            self.store.close()
            self.store = None

    def __enter__(self):
        return self

    def __exit__(self, *unused):
        self.close()

    def source_bytes(self, source):
        if not source.get('available'):
            raise ValueError('Source snapshot is unavailable')
        if self.store:
            row = self.store.db.execute('SELECT bytes FROM source_blobs WHERE sha256=?', (source['sha256'],)).fetchone()
            if row is None:
                raise ValueError('Missing source BLOB')
            data = row[0]
        else:
            data = self._member(source['path']).read_bytes()
        if not isinstance(data, bytes) or hashlib.sha256(data).hexdigest() != source['sha256'] or len(data) != source['byteLength']:
            raise ValueError('Source SHA256 or byte-length mismatch')
        return data

    def validate(self):
        if self.store:
            self.store.validate()
            row = self.store.db.execute("SELECT value_json FROM package_info WHERE key='producer'").fetchone()
            if row is None or hashlib.sha256(row[0].encode('utf-8')).hexdigest() != self.manifest['producer']['sha256']:
                raise ValueError('Producer SHA256 mismatch')
            producer = json.loads(row[0])
            if producer.get('schema') != 'edg-semantic-producer/1':
                raise ValueError('Unsupported semantic producer schema')
            binary_hash = producer.get('binarySha256')
            invocation_hash = self.manifest['invocation'].get('sha256')
            if (not isinstance(binary_hash, str) or re.fullmatch(r'[0-9a-fA-F]{64}', binary_hash) is None or
                    not isinstance(invocation_hash, str) or binary_hash.lower() != invocation_hash.lower()):
                raise ValueError('Producer/invocation binary SHA256 mismatch')
            content_hash = self.manifest.get('contentSha256')
            if not content_hash or self.store.content_sha256() != content_hash:
                raise ValueError('Content SHA256 mismatch')
            records = {row[0]: row[1:] for row in self.store.db.execute('SELECT node_id,original_path,content_sha256,available,byte_length FROM source_files WHERE unit_id=?', (self.store.unit_id,))}
            source_kind = next((key for key, value in self.store.kinds.items() if value[0] == 'source-file'), None)
            source_count = self.store.db.execute('SELECT count(*) FROM nodes WHERE unit_id=? AND kind_id=?', (self.store.unit_id, source_kind)).fetchone()[0]
            if len(records) != len(self.sources) or len({source['node'] for source in self.sources}) != source_count or source_count != len(records):
                raise ValueError('Source manifest/table count mismatch')
            for source in self.sources:
                if source.get('encoding') != 'bytes' or type(source.get('available')) is not bool:
                    raise ValueError('Invalid source snapshot descriptor')
                expected = (source['originalPath'], source.get('sha256'), int(source['available']), source.get('byteLength'))
                if records.get(source['node']) != expected:
                    raise ValueError('Source manifest/table mismatch')
                self.store._require_reference(source['node'], 'source-file')
                node = self.store.get_node(source['node'])
                if source['originalPath'] != (node.get('fullPath') or node.get('path')):
                    raise ValueError('Source path disagrees with semantic source-file node')
        else:
            if sha256_file(self.graph_path) != self.manifest['graph']['sha256']:
                raise ValueError('Graph SHA256 mismatch')
            producer = self._member(self.manifest['producer']['path'])
            if sha256_file(producer) != self.manifest['producer']['sha256']:
                raise ValueError('Producer SHA256 mismatch')
        checked = {}
        for source in self.sources:
            if source.get('available'):
                if source['sha256'] not in checked:
                    checked[source['sha256']] = len(self.source_bytes(source))
                if checked[source['sha256']] != source['byteLength']:
                    raise ValueError('Source byte-length mismatch')

    def iter_nodes(self, kind=None):
        if self.store:
            yield from self.store.iter_nodes(kind)
        else:
            for section, value in iter_json_records(self.graph_path):
                if section == 'nodes' and (kind is None or value['kind'] == kind):
                    yield value

    def iter_macros(self):
        if self.store:
            yield from self.store.iter_macros()
        else:
            yield from (value for section, value in iter_json_records(self.graph_path) if section == 'macroInvocations')

    def iter_diagnostics(self):
        if self.store:
            yield from self.store.iter_diagnostics()
        else:
            yield from (value for section, value in iter_json_records(self.graph_path) if section == 'diagnostics')


def content_sha256(connection):
    digest = hashlib.sha256(b'edg-semantic-content/1\n')
    def frame(value):
        digest.update(str(len(value)).encode('ascii') + b':' + value)
    tables = list(connection.execute("SELECT name,sql FROM sqlite_master WHERE type='table' AND name!='package_info' AND substr(name,1,7)!='sqlite_' ORDER BY name"))
    for name, sql in tables:
        digest.update(b'T'); frame(name.encode('utf-8'))
        digest.update(b'S'); frame(sql.encode('utf-8'))
        primary = sorted((row[5], row[1]) for row in connection.execute('PRAGMA table_info(' + quoted(name) + ')') if row[5])
        if not primary:
            raise ValueError('Content-hashed table has no primary key: ' + name)
        order = ','.join(quoted(column) for _, column in primary)
        for row in connection.execute('SELECT * FROM ' + quoted(name) + ' ORDER BY ' + order):
            digest.update(b'R')
            for value in row:
                if value is None:
                    digest.update(b'N')
                elif isinstance(value, int):
                    digest.update(b'I' + str(value).encode('ascii') + b';')
                elif isinstance(value, float):
                    digest.update(b'F' + struct.pack('<d', value))
                elif isinstance(value, str):
                    digest.update(b'T'); frame(value.encode('utf-8'))
                elif isinstance(value, bytes):
                    digest.update(b'B'); frame(value)
                else:
                    raise ValueError('Unsupported SQLite storage value')
            digest.update(b'\n')
        digest.update(b'\n')
    return digest.hexdigest()
