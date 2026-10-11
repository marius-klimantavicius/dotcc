#!/usr/bin/env python3
"""Check the public typed codec and compare native compact/expanded field output."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--linker', type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--tooling', type=Path)
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    tooling = args.tooling.resolve() if args.tooling else Path(__file__).resolve().parents[1]
    spec = importlib.util.spec_from_file_location('graph_fields', tooling / 'graph_fields.py')
    codec = importlib.util.module_from_spec(spec); spec.loader.exec_module(codec)
    checks, commands = [], []
    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)
    def rejects(name, call):
        try: call()
        except (ValueError, UnicodeError): checks.append(name); return
        raise AssertionError(name)
    def run(name, command, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, env=env, timeout=900)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0)
    error = None
    try:
        for number, encoded in [(0, '00'), (127, '7f'), (128, '8001'), (16384, '808001'), ((1 << 64)-1, 'ffffffffffffffffff01')]:
            check('golden-varuint-' + str(number), codec.varuint(number).hex() == encoded)
            check('decode-golden-' + str(number), codec.read_varuint(bytes.fromhex(encoded), 0) == (number, len(bytes.fromhex(encoded))))
        for encoded in ('80', '8000', '8100', 'ffffffffffffffffff02', 'ffffffffffffffffffff00'):
            rejects('bad-varuint-' + encoded, lambda e=encoded: codec.read_varuint(bytes.fromhex(e), 0))
        cases = [('signed', str(-(1 << 63))), ('signed', str((1 << 63)-1)), ('signed', '-1'),
                 ('unsigned', str((1 << 64)-1)), ('boolean', '0'), ('boolean', '1'),
                 ('text', 'α\x00世界'), ('bytes', '0001ff80'), ('reference', None)]
        for index, (kind, value) in enumerate(cases, 1):
            definition = dict(id=1, ownerEntryKind=6, name='value', valueKind=kind, targetEntryKind=6 if kind=='reference' else None)
            row = dict(nodeId=1, name='value', ordinal=0, valueKind=kind, value=value,
                       targetId=1 if kind=='reference' else None, targetEntryKind=definition['targetEntryKind'])
            data = codec.encode_row(row, 1, definition)
            check('typed-roundtrip-' + str(index), list(codec.decode_block(data, 1, {1: definition})) == [row])
            rejects('truncated-row-' + str(index), lambda d=data: list(codec.decode_block(d[:-1], 1, {1: definition})))
            rejects('extra-row-bytes-' + str(index), lambda d=data: list(codec.decode_block(d+b'\0', 1, {1: definition})))
            stored = codec.stored_block(0, 1, data)
            check('storage-roundtrip-' + str(index), codec.read_stored(*stored[2:]) == data)
        if args.compiler:
            source = out / 'fixture.cpp'
            source.write_bytes(Path(__file__).with_name('duckdb_source_member_substitutions.cpp').read_bytes())
            digests, sizes = [], []
            for mode in ('compact', 'expanded'):
                obj = out / (mode + '.dotobj')
                env = dict(os.environ)
                if mode == 'expanded': env['EDG_DOT_GRAPH_EXPANDED_FIELDS'] = '1'
                else: env.pop('EDG_DOT_GRAPH_EXPANDED_FIELDS', None)
                run(mode + '-export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
                    '--work-dir', out / (mode + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c++20', '--exceptions'], env)
                if args.linker:
                    run(mode + '-verify', ['dotnet', args.linker.resolve(), 'verify', obj])
                    run(mode + '-inspect', ['dotnet', args.linker.resolve(), 'inspect-edg-graph', '--out', out / (mode + '.jsonl'), obj])
                digest = hashlib.sha256(); count = 0
                with sqlite3.connect(obj) as db:
                    for row in codec.rows(db):
                        digest.update(json.dumps(row, sort_keys=True, separators=(',', ':'), ensure_ascii=False).encode()); digest.update(b'\n'); count += 1
                digests.append((count, digest.hexdigest())); sizes.append(dict(mode=mode, objectBytes=obj.stat().st_size))
                if args.linker:
                    inspected = hashlib.sha256(); inspected_count = 0
                    with (out / (mode + '.jsonl')).open() as stream:
                        for line in stream:
                            inspected.update(json.dumps(json.loads(line), sort_keys=True, separators=(',', ':'), ensure_ascii=False).encode()); inspected.update(b'\n'); inspected_count += 1
                    check(mode + '-independent-decoder-parity', (inspected_count, inspected.hexdigest()) == digests[-1])
            check('native-compact-expanded-exact-field-parity', digests[0] == digests[1])
            (out / 'sizes.json').write_text(json.dumps(sizes, indent=2)+'\n')
    except Exception as exc: error = repr(exc)
    (out / 'results.json').write_text(json.dumps(dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands), indent=2)+'\n')
    print(json.dumps(dict(status='failed' if error else 'passed', error=error, checks=len(checks))))
    return int(error is not None)


if __name__ == '__main__': raise SystemExit(main())
