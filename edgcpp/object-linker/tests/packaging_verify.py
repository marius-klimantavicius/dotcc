#!/usr/bin/env python3
"""Check bulk semantic packaging parity and fail-closed relational validation."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import sqlite3
import time


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--baseline-object', type=Path, required=True)
    parser.add_argument('--wire', type=Path, required=True)
    parser.add_argument('--packager', type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    spec = importlib.util.spec_from_file_location('candidate_packager', args.packager.resolve())
    package = importlib.util.module_from_spec(spec); spec.loader.exec_module(package)
    checks, timings = [], {}

    def check(name, passed):
        checks.append(dict(name=name, passed=bool(passed)))
        if not passed:
            raise AssertionError(name)

    with sqlite3.connect(args.baseline_object.resolve().as_uri() + '?mode=ro', uri=True) as db:
        unit, source = db.execute('SELECT unit_id,primary_source FROM object_info').fetchone()
        provenance = list(db.execute('SELECT kind,value FROM provenance ORDER BY ordinal'))
        baseline_digest = db.execute('SELECT sha256 FROM content_digest').fetchone()[0]
    started_ns = time.time_ns()
    wire = json.loads(args.wire.read_text(), object_pairs_hook=package.unique_object)

    def emit(name, graph):
        start = time.monotonic()
        try:
            package.package(graph, out / (name + '.sqlite'), Path(source), unit, provenance, started_ns)
        finally:
            timings[name] = time.monotonic() - start

    failure = None
    try:
        emit('candidate', wire)
        with sqlite3.connect(out / 'candidate.sqlite') as db:
            check('logical-digest-parity', db.execute('SELECT sha256 FROM content_digest').fetchone()[0] == baseline_digest)
            check('all-foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
            check('database-integrity', db.execute('PRAGMA integrity_check').fetchone() == ('ok',))
        check('byte-identical-to-baseline', sha(out / 'candidate.sqlite') == sha(args.baseline_object))

        typed = next(index for index, node in enumerate(wire['nodes']) if node['typeId'] is not None)
        mutations = [
            ('missing-type', 'nodes', typed, {'typeId': max(row['id'] for row in wire['types']) + 100}),
            ('missing-node', 'edges', 0, {'childId': max(row['id'] for row in wire['nodes']) + 100}),
            ('invalid-type-kind', 'types', 0, {'kind': 'not-an-execution-type'}),
        ]
        for name, table, index, change in mutations:
            graph = dict(wire)
            graph[table] = list(wire[table])
            graph[table][index] = dict(graph[table][index], **change)
            error = None
            try:
                emit(name, graph)
            except (ValueError, sqlite3.IntegrityError) as exception:
                error = str(exception)
            check(name + '-rejected', error is not None)
            (out / (name + '.error')).write_text(error or '')
            with sqlite3.connect(out / (name + '.sqlite')) as db:
                check(name + '-rolled-back-no-digest', db.execute('SELECT count(*) FROM content_digest').fetchone()[0] == 0 and
                      db.execute('SELECT count(*) FROM nodes').fetchone()[0] == 0)
    except Exception as exception:
        failure = str(exception)
    result = dict(schema=1, status='failed' if failure else 'passed', error=failure,
                  checks=checks, seconds=timings,
                  baseline=dict(path=str(args.baseline_object.resolve()), sha256=sha(args.baseline_object)),
                  wire=dict(path=str(args.wire.resolve()), sha256=sha(args.wire)),
                  packager=dict(path=str(args.packager.resolve()), sha256=sha(args.packager)),
                  note='Provenance intentionally held constant for byte parity; this receipt identifies the candidate packager actually executed')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), seconds=timings, error=failure)))
    return 1 if failure else 0


if __name__ == '__main__':
    raise SystemExit(main())
