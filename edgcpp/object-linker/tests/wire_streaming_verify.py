#!/usr/bin/env python3
"""Check incremental wire parsing and identical SQLite contents when packaging."""
import argparse
import importlib.util
import io
import json
from pathlib import Path
import sqlite3
import time


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--packager', type=Path, required=True)
    parser.add_argument('--wire', type=Path, required=True)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--unit-id', required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=False)
    spec = importlib.util.spec_from_file_location('packager', args.packager)
    package = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(package)
    checks = []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def decoded(text, chunk):
        return {key: list(value) if key in package.ARRAYS else value
            for key, value in package.WireReader(io.StringIO(text), chunk).entries()}

    value = dict(sourceExpressions=[dict(id=1, text='quote" slash\\ unicode☀\n', values=[-129, True, None]),
        dict(id=2)], profile='test', formatVersion=123456789, sourceLanguage='c++')
    text = json.dumps(value, ensure_ascii=False)
    for size in (1, 2, 3, 7, 16, 65536):
        check('chunk-boundary-' + str(size), decoded(text, size) == value)
    for name, damaged in (
        ('duplicate-top-key', '{"profile":1,"profile":2}'),
        ('duplicate-row-key', '{"sourceExpressions":[{"id":1,"id":2}]}'),
        ('truncated-string', '{"profile":"unterminated'),
        ('truncated-row', '{"sourceExpressions":[{"id":1}'),
        ('trailing-row-comma', '{"sourceExpressions":[{"id":1},]}'),
        ('missing-property-comma', '{"profile":1 "formatVersion":1}'),
        ('trailing-content', '{"profile":1}{}'),
        ('wrong-array-shape', '{"sourceExpressions":{}}'),
    ):
        try:
            decoded(damaged, 3)
        except ValueError:
            checks.append('reject-' + name)
        else:
            raise AssertionError('accepted ' + name)
    started = time.time_ns()
    provenance = [('producer', 'wire-streaming-parity')]
    memory = args.output / 'memory.dotobj'
    streamed = args.output / 'streamed.dotobj'
    wire = json.loads(args.wire.read_text(), object_pairs_hook=package.unique_object)
    reference = package.package(wire, memory, args.source.resolve(), args.unit_id, provenance, started)
    del wire
    actual = package.package_file(args.wire, streamed, args.source.resolve(), args.unit_id, provenance, started)
    check('same-row-counts-and-profile', actual == reference)
    with sqlite3.connect(memory) as left, sqlite3.connect(streamed) as right:
        check('identical-logical-contents', package.logical_hash(left) == package.logical_hash(right))
        check('streamed-foreign-keys', not list(right.execute('PRAGMA foreign_key_check')))
        check('streamed-integrity', right.execute('PRAGMA integrity_check').fetchone() == ('ok',))
    (args.output / 'results.json').write_text(json.dumps(dict(status='passed', checks=checks), indent=2) + '\n')
    print(f'PASS {len(checks)} streaming wire checks')


if __name__ == '__main__':
    main()
