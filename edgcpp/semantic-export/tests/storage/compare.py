#!/usr/bin/env python3
"""Stream an exact semantic comparison of two independently sealed SQLite packages.

Producer identity may differ. Only identified predefined __TIME__ records and
unreferenced EDG ctime text can differ within the graph. Clock exceptions are
reported with their actual node IDs, fields, and before/after values.
"""
import argparse
from datetime import datetime
import hashlib
from itertools import zip_longest
import json
from pathlib import Path
import re
import resource
import sys
import time

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from sqlite_store import SemanticPackage, quoted

TIME_DEFINITION = re.compile(r'#define __TIME__ "(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d"\Z')
CTIME = re.compile(r'(Mon|Tue|Wed|Thu|Fri|Sat|Sun) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) (?: [1-9]|[12]\d|3[01]) (?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d \d{4}\n\x00\Z')
MISSING = object()


def canonical(value):
    # JSON distinguishes false from 0 and missing fields from explicit null.
    return json.dumps(value, sort_keys=True, ensure_ascii=True, separators=(',', ':'), allow_nan=False).encode('utf-8')


def brief(value):
    if value is MISSING:
        return {'missing': True}
    data = canonical(value)
    return {'type': type(value).__name__, 'jsonBytes': len(data),
            'sha256': hashlib.sha256(data).hexdigest(), 'preview': data[:256].decode('ascii')}


class Difference(ValueError):
    def __init__(self, message, **context):
        super().__init__(message)
        self.context = context


def exact(before, after, section, identity):
    if before is MISSING or after is MISSING or canonical(before) != canonical(after):
        raise Difference('Semantic records differ', section=section, identity=identity,
                         before=brief(before), after=brief(after))


def clock_records(store):
    """Keep only the tiny predefined clock whitelist, never a graph ID set."""
    macro = next(((key, table) for key, (name, table) in store.kinds.items() if name == 'macro'), None)
    if macro is None:
        return {'macros': {}, 'payloads': {}}
    fields = dict(store.fields[macro[0]])
    if fields.get('name') != 'text' or fields.get('predefined') != 'boolean':
        return {'macros': {}, 'payloads': {}}
    rows = store.connection.execute(
        'SELECT m.node_id FROM ' + quoted(macro[1]) + ' m JOIN literals l ON l.literal_id=m.name '
        'WHERE m.unit_id=? AND m.predefined=1 AND l.encoding=? AND l.payload=? ORDER BY m.node_id',
        (store.unit_id, 'text', b'__TIME__'))
    result = {'macros': {}, 'payloads': {}}
    for node_id, in rows:
        # One frontend unit has one predefined __TIME__. Do not turn malformed
        # or unexpectedly broad matches into an unbounded whitelist.
        if len(result['macros']) >= 1:
            raise Difference('More than one predefined __TIME__ definition in a unit')
        node = store.get_node(node_id)
        if (node.get('commandLine') is not False or node.get('objectLike') is not True or
                not isinstance(node.get('text'), str) or TIME_DEFINITION.fullmatch(node['text']) is None):
            raise Difference('Unexpected predefined __TIME__ representation', node=node_id)
        result['macros'][node_id] = node['text']
        payloads = [edge['target'] for edge in node.get('nativeEdges', []) if edge['kind'] == 'other-text']
        if len(payloads) != 1:
            raise Difference('Unexpected predefined __TIME__ payload edges', node=node_id)
        payload = store.get_node(payloads[0])
        if payload.get('bytesLatin1') != node['text'] + '\0' or payload['kind'] != 'other-text':
            raise Difference('Predefined __TIME__ payload disagrees with macro', node=node_id)
        result['payloads'][payload['id']] = payload['bytesLatin1']
    return result


def ctime_text(value):
    if not isinstance(value, str) or CTIME.fullmatch(value) is None:
        return False
    try:
        date = datetime.strptime(value[:-2], '%a %b %d %H:%M:%S %Y')
    except ValueError:
        return False
    return date.strftime('%a') == value[:3]


def unreferenced(store, node_id):
    # native_edges is the exporter's complete pointer-walker audit relation.
    # Query just the exceptional candidate; never materialize all target IDs.
    return store.connection.execute('SELECT 1 FROM native_edges WHERE unit_id=? AND target_id=? LIMIT 1',
                                    (store.unit_id, node_id)).fetchone() is None


def allowed_clock_difference(before, after, left, right, old_clock, new_clock):
    if before.keys() != after.keys() or before['id'] != after['id'] or before['kind'] != after['kind']:
        return None
    fields = [key for key in before if canonical(before[key]) != canonical(after[key])]
    node_id = before['id']
    if (fields == ['text'] and node_id in old_clock['macros'] and node_id in new_clock['macros'] and
            before['text'] == old_clock['macros'][node_id] and after['text'] == new_clock['macros'][node_id]):
        category = 'predefined __TIME__ macro'
    elif (fields == ['bytesLatin1'] and node_id in old_clock['payloads'] and node_id in new_clock['payloads'] and
          before['bytesLatin1'] == old_clock['payloads'][node_id] and after['bytesLatin1'] == new_clock['payloads'][node_id]):
        category = 'linked __TIME__ macro text payload'
    elif (fields == ['bytesLatin1'] and before.keys() == {'id', 'kind', 'bytesLatin1'} and
          before['kind'] == 'other-text' and ctime_text(before['bytesLatin1']) and ctime_text(after['bytesLatin1']) and
          unreferenced(left, node_id) and unreferenced(right, node_id)):
        # EDG's ctime record uses the real invocation clock even when the
        # caller fixes __DATE__/__TIME__. Do not require those clocks to agree.
        category = 'unreferenced EDG ctime payload'
    else:
        return None
    return {'node': node_id, 'kind': before['kind'], 'field': fields[0], 'category': category,
            'before': before[fields[0]], 'after': after[fields[0]]}


def compare(before_path, after_path):
    started = time.monotonic()
    report = {'schema': 'edg-semantic-package-comparison/1', 'status': 'running',
              'before': str(Path(before_path).resolve()), 'after': str(Path(after_path).resolve()),
              'allowedClockDifferences': [],
              'scope': 'metadata, ID-ordered nodes with exact JSON value types, macros, diagnostics, source descriptors and bytes; producer identity intentionally excluded'}
    before_hash, after_hash, normalized_hash = (hashlib.sha256(b'edg-semantic-logical-records/1\n') for _ in range(3))

    def hash_bytes(section, old_bytes, new_bytes, normalized_bytes=None):
        prefix = section.encode('ascii') + b':'
        before_hash.update(prefix + old_bytes + b'\n')
        after_hash.update(prefix + new_bytes + b'\n')
        normalized_hash.update(prefix + (new_bytes if normalized_bytes is None else normalized_bytes) + b'\n')

    def hash_record(section, old, new):
        hash_bytes(section, canonical(old), canonical(new))

    try:
        with SemanticPackage(before_path) as before, SemanticPackage(after_path) as after:
            if before.store is None or after.store is None:
                raise Difference('Comparison requires two SQLite packages')
            report['producerIdentities'] = {
                'before': {'binarySha256': before.manifest['invocation']['sha256'], 'producerSha256': before.manifest['producer']['sha256']},
                'after': {'binarySha256': after.manifest['invocation']['sha256'], 'producerSha256': after.manifest['producer']['sha256']}}
            exact(before.metadata, after.metadata, 'metadata', None)
            hash_record('metadata', before.metadata, after.metadata)
            old_clock, new_clock = clock_records(before.store), clock_records(after.store)
            report['nodeCount'] = 0
            for old, new in zip_longest(before.iter_nodes(), after.iter_nodes(), fillvalue=MISSING):
                if old is MISSING or new is MISSING:
                    exact(old, new, 'nodes', report['nodeCount'])
                old_bytes, new_bytes = canonical(old), canonical(new)
                normalized_bytes = new_bytes
                if old_bytes != new_bytes:
                    allowed = allowed_clock_difference(old, new, before.store, after.store, old_clock, new_clock)
                    if allowed is None:
                        fields = [key for key in old.keys() | new.keys()
                                  if key not in old or key not in new or canonical(old[key]) != canonical(new[key])]
                        raise Difference('Node semantic fields differ', node=old['id'], kind=old['kind'],
                                         fields=[{'name': key, 'before': brief(old.get(key, MISSING)),
                                                  'after': brief(new.get(key, MISSING))} for key in sorted(fields)])
                    if len(report['allowedClockDifferences']) >= 3:
                        raise Difference('More than the three known frontend clock records differ')
                    report['allowedClockDifferences'].append(allowed)
                    normalized_bytes = old_bytes
                hash_bytes('node', old_bytes, new_bytes, normalized_bytes)
                report['nodeCount'] += 1
            for section, left, right, counter in [
                    ('macro', before.iter_macros(), after.iter_macros(), 'macroCount'),
                    ('diagnostic', before.iter_diagnostics(), after.iter_diagnostics(), 'diagnosticCount')]:
                report[counter] = 0
                for old, new in zip_longest(left, right, fillvalue=MISSING):
                    exact(old, new, section, report[counter])
                    hash_record(section, old, new)
                    report[counter] += 1
            report['sourceCount'], report['sourceBytesCompared'] = 0, 0
            # Source descriptors are already part of the small package manifest;
            # graph/source order itself is represented by semantic node links.
            for old, new in zip_longest(sorted(before.sources, key=lambda item: item['node']),
                                       sorted(after.sources, key=lambda item: item['node']), fillvalue=MISSING):
                exact(old, new, 'source', report['sourceCount'])
                hash_record('source', old, new)
                if old['available']:
                    old_bytes, new_bytes = before.source_bytes(old), after.source_bytes(new)
                    if old_bytes != new_bytes:
                        raise Difference('Source snapshot bytes differ', node=old['node'], originalPath=old['originalPath'])
                    report['sourceBytesCompared'] += len(old_bytes)
                report['sourceCount'] += 1
        report.update(status='passed', beforeLogicalSha256=before_hash.hexdigest(),
                      afterLogicalSha256=after_hash.hexdigest(), afterClockNormalizedSha256=normalized_hash.hexdigest())
        if report['beforeLogicalSha256'] != report['afterClockNormalizedSha256']:
            raise AssertionError('Internal normalized comparison hash mismatch')
    except Exception as error:
        report.update(status='failed', error=str(error))
        if isinstance(error, Difference):
            report['difference'] = error.context
    report['seconds'] = round(time.monotonic() - started, 3)
    report['peakProcessRssKiB'] = resource.getrusage(resource.RUSAGE_SELF).ru_maxrss
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--before', required=True, type=Path)
    parser.add_argument('--after', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path, help='Fresh JSON evidence file (also written on rejection)')
    args = parser.parse_args()
    if args.output.exists():
        parser.error('Output evidence file already exists')
    report = compare(args.before, args.after)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print(report['status'].upper() + ' semantic comparison: ' + str(args.output))
    if report['status'] != 'passed':
        print(report['error'], file=sys.stderr)
        return 1
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
