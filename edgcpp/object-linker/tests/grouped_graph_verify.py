#!/usr/bin/env python3
"""Exercise grouped graph format corruption using copies of a small v66 object.

Only one existing compressed-field block is changed per negative. Other graph
and source/execution data remain intact. Standalone codec boundary cases are
explicitly separate from managed whole-object validation; no synthetic source
or frontend graph nodes are added to the exported object.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import time
import zlib


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    value = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(value)
    return value


def parse_groups(data, definitions, codec):
    """Locate encoded header fields and retain payload bytes without re-encoding."""
    offset = 0

    def integer():
        nonlocal offset
        start = offset
        value, offset = codec.read_varuint(data, offset)
        return value, (start, offset)

    layout_count, count_span = integer()
    layouts, spans = [], {'layout-count': count_span}
    for index in range(layout_count):
        count, span = integer()
        if index == 0:
            spans['field-count'] = span
        layout = []
        for field in range(count):
            definition, span = integer()
            if index == 0 and field == 0:
                spans['definition'] = span
            ordinal, _ = integer()
            layout.append((definition, ordinal))
        layouts.append(layout)
    group_count, spans['group-count'] = integer()
    groups = []
    for index in range(group_count):
        node, _ = integer()
        layout, span = integer()
        if index == 0:
            spans['layout-id'] = span
        start = offset
        for ident, _ in layouts[layout - 1]:
            kind = definitions[ident]['valueKind']
            if kind in ('signed', 'unsigned', 'reference'):
                integer()
            elif kind == 'boolean':
                offset += 1
            elif kind in ('text', 'bytes'):
                length, _ = integer()
                offset += length
            else:
                raise ValueError('Unknown original field kind: ' + kind)
        groups.append((node, layout, data[start:offset]))
    if offset != len(data):
        raise ValueError('Original grouped block was not consumed exactly')
    return layouts, groups, spans


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--driver', type=Path, required=True)
    parser.add_argument('--packager', type=Path, required=True)
    parser.add_argument('--object', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    out = args.output.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands, cases = [], [], []
    error = None
    packager = module('group_test_packager', args.packager.resolve())
    codec = module('group_test_codec', args.packager.resolve().with_name('graph_fields.py'))
    u = codec.varuint

    def check(name, value):
        if not value:
            raise AssertionError(name)
        checks.append(name)

    def run(name, path, diagnostic=None):
        command = ['dotnet', str(args.driver.resolve()), 'verify', str(path)]
        started = time.monotonic()
        result = subprocess.run(command, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode,
                             seconds=time.monotonic() - started))
        check(name, result.returncode == 0 if diagnostic is None else
              result.returncode != 0 and diagnostic.encode() in result.stderr)

    def grouped(layouts, groups):
        result = bytearray(u(len(layouts)))
        for layout in layouts:
            result.extend(u(len(layout)))
            for ident, ordinal in layout:
                result.extend(u(ident)); result.extend(u(ordinal))
        result.extend(u(len(groups)))
        for node, layout, payload in groups:
            result.extend(u(node)); result.extend(u(layout)); result.extend(payload)
        return bytes(result)

    try:
        # Boundary fixtures exercise the public Python codec only. The long
        # multibyte text straddles the 64KiB boundary in the encoded block.
        long_text = 'a' + 'λ' * 40000 + '🙂'
        definitions = {
            1: dict(id=1, ownerEntryKind=6, name='longText', valueKind='text', targetEntryKind=None),
            2: dict(id=2, ownerEntryKind=6, name='emptyText', valueKind='text', targetEntryKind=None),
            3: dict(id=3, ownerEntryKind=6, name='nullPointer', valueKind='reference', targetEntryKind=0),
        }
        payload = long_text.encode('utf-8')
        boundary = grouped([[(1, 0), (2, 0), (3, 0)]], [(1, 1, u(len(payload)) + payload + u(0) + u(0))])
        expected = [dict(nodeId=1, name=name, ordinal=0, valueKind=kind, value=value,
                         targetId=None, targetEntryKind=target)
                    for name, kind, value, target in [('longText', 'text', long_text, None),
                        ('emptyText', 'text', '', None), ('nullPointer', 'reference', None, 0)]]
        (out / 'standalone-boundary.bin').write_bytes(boundary)
        (out / 'standalone-boundary.json').write_text(json.dumps(dict(
            scope='standalone Python codec; no object mutation', definitions=definitions,
            expectedRows=expected), ensure_ascii=False, indent=2) + '\n')
        check('python-long-utf8-exceeds-64k', len(payload) > 65536)
        check('python-grouped-long-empty-null-roundtrip',
              list(codec.decode_block(boundary, 3, definitions, 'graph-groups-v1')) == expected)
        for storage, data in [('raw', boundary), ('zlib', zlib.compress(boundary))]:
            check('python-boundary-' + storage + '-storage-roundtrip',
                  codec.read_stored(data, storage, len(boundary), hashlib.sha256(boundary).hexdigest()) == boundary)

        original = args.object.resolve()
        run('valid-grouped-object', original)
        with sqlite3.connect(f'file:{original}?mode=ro', uri=True) as db:
            definitions = {row[0]: dict(id=row[0], ownerEntryKind=row[1], name=row[2],
                valueKind=row[3], targetEntryKind=row[4]) for row in db.execute(
                    'SELECT id,owner_entry_kind,name,value_kind,target_entry_kind FROM edg_graph_field_definitions')}
            selected = None
            for block in db.execute('''SELECT ordinal,row_count,bytes,codec,decoded_byte_length,sha256
                FROM edg_graph_field_blocks WHERE encoding='graph-groups-v1'
                AND decoded_byte_length<=2097152 AND row_count<=100000 ORDER BY ordinal'''):
                data = codec.read_stored(*block[2:])
                layouts, groups, spans = parse_groups(data, definitions, codec)
                # One spare layout can be declared without breaking row bounds,
                # so unused-layout corruption reaches the intended check.
                if sum(map(len, layouts)) + len(layouts[0]) <= block[1]:
                    selected = block, data, layouts, groups, spans
                    break
            if selected is None:
                raise ValueError('Input needs a bounded grouped block with a repeated layout')
        block, data, layouts, groups, spans = selected
        block_ordinal, row_count = block[:2]
        check('original-selected-block-typed-row-count',
              sum(1 for _ in codec.decode_block(data, row_count, definitions, 'graph-groups-v1')) == row_count)
        check('header-reconstruction-preserves-every-payload-byte', grouped(layouts, groups) == data)
        (out / 'selected-block.json').write_text(json.dumps(dict(ordinal=block_ordinal,
            rowCount=row_count, decodedBytes=len(data), layouts=len(layouts), groups=len(groups)), indent=2) + '\n')

        def replace(key, encoded):
            begin, end = spans[key]
            return data[:begin] + encoded + data[end:]

        def negative(name, decoded, diagnostic, rows=row_count, storage='raw',
                     encoded=None, length=None, digest=None):
            path = out / (name + '.dotobj')
            shutil.copy2(original, path)
            if encoded is None:
                encoded = decoded if storage == 'raw' else zlib.compress(decoded)
            declared_length = len(decoded) if length is None else length
            declared_hash = hashlib.sha256(decoded).hexdigest() if digest is None else digest
            with sqlite3.connect(path) as db:
                db.execute('PRAGMA foreign_keys=ON')
                changed = db.execute('''UPDATE edg_graph_field_blocks SET row_count=?,bytes=?,codec=?,
                    decoded_byte_length=?,sha256=? WHERE ordinal=?''',
                    (rows, encoded, storage, declared_length, declared_hash, block_ordinal)).rowcount
                check(name + '-single-block-changed', changed == 1)
                check(name + '-sqlite-integrity', db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
                      and not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            cases.append(dict(name=name, blockOrdinal=block_ordinal, expectedDiagnostic=diagnostic))
            run(name, path, diagnostic)

        negative('layout-count-zero', replace('layout-count', u(0)), 'Invalid EDG graph group layout count')
        negative('layout-count-oversized', replace('layout-count', u(row_count + 1)), 'Invalid EDG graph group layout count')
        negative('layout-unused', grouped(layouts + [layouts[0]], groups), 'EDG graph group row count or layout usage mismatch')
        negative('field-count-zero', replace('field-count', u(0)), 'Invalid EDG graph group layout extent')
        negative('unknown-definition', replace('definition', u(max(definitions) + 1)), 'EDG graph field has an invalid definition ID')
        negative('group-count-zero', replace('group-count', u(0)), 'Invalid EDG graph group count')
        negative('group-count-oversized', replace('group-count', u(row_count + 1)), 'Invalid EDG graph group count')
        negative('invalid-layout-id', replace('layout-id', u(len(layouts) + 1)), 'EDG graph group has an invalid owner or layout ID')
        negative('row-count-mismatch', data, 'EDG graph group row count or layout usage mismatch', rows=row_count + 1)
        negative('trailing-data', data + b'\0', 'EDG graph block row count, decoded length, or frame termination mismatch')
        negative('truncated-data', data[:-1], 'Truncated EDG graph field payload')
        encoded_count = bytearray(u(len(layouts)))
        encoded_count[-1] |= 128
        encoded_count.append(0)
        negative('overlong-varuint', replace('layout-count', encoded_count), 'EDG graph varuint is not shortest form')
        negative('wrong-hash', data, 'EDG graph block digest mismatch', digest='0' * 64)
        compressed = zlib.compress(data)
        check('selected-block-compresses-for-frame-negatives', len(compressed) + 1 < len(data))
        negative('wrong-decoded-length', data, 'EDG graph block row count, decoded length, or frame termination mismatch',
                 storage='zlib', encoded=compressed, length=len(data) + 1)
        negative('zlib-trailing-data', data, 'EDG graph block row count, decoded length, or frame termination mismatch',
                 storage='zlib', encoded=compressed + b'\0')
    except Exception as exc:
        error = repr(exc)
    (out / 'results.json').write_text(json.dumps(dict(status='failed' if error else 'passed', error=error,
        input=str(args.object.resolve()), checks=checks, cases=cases, commands=commands), indent=2) + '\n')
    if error:
        raise RuntimeError(error)
    print(f'PASS {len(checks)} grouped graph format checks')


if __name__ == '__main__':
    main()
