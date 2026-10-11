#!/usr/bin/env python3
"""Lossless graph-fields-v1 codec and streaming dotobj field inspector.

Each row is shortest ULEB128(node ID, definition ID, ordinal), then its declared
payload: unsigned ULEB64, signed zigzag64 ULEB, boolean byte, UTF-8/raw bytes with
ULEB byte length, or reference ID ULEB (zero is null). Blocks contain whole rows.
This is serialization, not interpretation of EDG source-language semantics.
"""
import argparse
import hashlib
import json
import sqlite3
import zlib
import importlib.util
from pathlib import Path

_spec = importlib.util.spec_from_file_location('_dotobj_packager', Path(__file__).with_name('compile.py'))
_packager = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_packager)
varuint = _packager._graph_varuint
canonical_hex = _packager._graph_canonical_hex
encode_row = _packager._graph_encode_row
stored_block = _packager._graph_stored_block

MAX_BYTES = 2147483647
BLOCK_BYTES = 65536
UINT64_MAX = (1 << 64) - 1




def read_varuint(data, offset):
    value = 0
    for index in range(10):
        if offset == len(data):
            raise ValueError('Truncated graph integer')
        byte = data[offset]
        offset += 1
        if index == 9 and byte > 1:
            raise ValueError('Overflowing graph integer')
        value |= (byte & 127) << (7 * index)
        if byte < 128:
            if index and byte == 0:
                raise ValueError('Noncanonical graph integer')
            return value, offset
    raise ValueError('Overflowing graph integer')






def decode_block(data, row_count, definitions):
    offset = 0
    for _ in range(row_count):
        node, offset = read_varuint(data, offset)
        ident, offset = read_varuint(data, offset)
        ordinal, offset = read_varuint(data, offset)
        if node == 0 or ident not in definitions:
            raise ValueError('Unknown graph node/field definition')
        definition = definitions[ident]
        kind = definition['valueKind']
        value, target = None, None
        if kind in ('signed', 'unsigned', 'reference'):
            raw, offset = read_varuint(data, offset)
            if kind == 'reference':
                target = raw or None
            else:
                value = str((raw >> 1) ^ -(raw & 1)) if kind == 'signed' else str(raw)
        elif kind == 'boolean':
            if offset == len(data) or data[offset] > 1:
                raise ValueError('Invalid graph boolean')
            value = str(data[offset])
            offset += 1
        elif kind in ('text', 'bytes'):
            length, offset = read_varuint(data, offset)
            if length > MAX_BYTES or length > len(data) - offset:
                raise ValueError('Truncated or oversized graph field')
            payload = data[offset:offset + length]
            offset += length
            value = payload.decode('utf-8', errors='strict') if kind == 'text' else payload.hex()
        else:
            raise ValueError('Unknown graph primitive kind')
        yield dict(nodeId=node, name=definition['name'], ordinal=ordinal, valueKind=kind,
                   value=value, targetId=target, targetEntryKind=definition['targetEntryKind'])
    if offset != len(data):
        raise ValueError('Trailing graph block bytes')




def read_stored(data, codec, length, digest):
    if not 0 < length <= MAX_BYTES:
        raise ValueError('Invalid graph decoded length')
    if codec == 'raw':
        decoded = data
    elif codec == 'zlib':
        decoder = zlib.decompressobj()
        decoded = decoder.decompress(data, length + 1)
        if not decoder.eof or decoder.unused_data or decoder.unconsumed_tail:
            raise ValueError('Invalid compressed graph block')
    else:
        raise ValueError('Unknown graph storage codec')
    if len(decoded) != length or hashlib.sha256(decoded).hexdigest() != digest:
        raise ValueError('Graph block length/hash differs')
    return decoded


def rows(db):
    definitions = {row[0]: dict(id=row[0], ownerEntryKind=row[1], name=row[2], valueKind=row[3], targetEntryKind=row[4])
                   for row in db.execute('SELECT id,owner_entry_kind,name,value_kind,target_entry_kind FROM edg_graph_field_definitions')}
    for expected, row in enumerate(db.execute('SELECT ordinal,row_count,bytes,codec,decoded_byte_length,sha256 FROM edg_graph_field_blocks ORDER BY ordinal')):
        ordinal, count, data, codec, length, digest = row
        if ordinal != expected:
            raise ValueError('Noncontiguous graph blocks')
        yield from decode_block(read_stored(data, codec, length, digest), count, definitions)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('object')
    args = parser.parse_args()
    with sqlite3.connect(args.object) as db:
        for row in rows(db):
            print(json.dumps(row, ensure_ascii=False, separators=(',', ':')))


if __name__ == '__main__':
    main()
