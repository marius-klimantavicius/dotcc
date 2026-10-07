#!/usr/bin/env python3
"""Export a self-contained normalized SQLite semantic package and build provenance."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import sys

from sqlite_store import SqliteStore


def digest(path):
    h = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            h.update(block)
    return h.hexdigest()


def json_text(value):
    return json.dumps(value, ensure_ascii=True, indent=2) + '\n'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path, help='Fresh package directory')
    parser.add_argument('source', type=Path)
    parser.add_argument('flags', nargs=argparse.REMAINDER)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    compiler, source, output = args.compiler.resolve(), args.source.resolve(), args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    temporary = output / 'semantic.sqlite.partial'
    destination = output / 'semantic.sqlite'
    flags = args.flags[1:] if args.flags[:1] == ['--'] else args.flags
    if not any(value.startswith('--c') for value in flags):
        flags = ['--c11' if source.suffix == '.c' else '--c++17', *flags]
    cpp = any(value.startswith('--c++') for value in flags)
    command = [str(compiler), '--clear_flag', 'use_predefined_macro_file',
               *(['--instantiate', 'used'] if cpp else []), *flags, str(source)]
    invocation = {'executable': str(compiler), 'sha256': digest(compiler),
                  'arguments': command[1:], 'cwd': str(Path.cwd())}
    (output / 'invocation.json').write_text(json_text(invocation))
    producer_path = compiler.parent / 'cpfe-semantic.producer.json'
    producer_bytes = producer_path.read_bytes()
    producer = json.loads(producer_bytes)
    if producer['binarySha256'] != invocation['sha256']:
        raise ValueError('Compiler provenance hash mismatch')
    with (output / 'stdout.log').open('w') as stdout, (output / 'stderr.log').open('w') as stderr:
        result = subprocess.run(command, env=dict(os.environ, EDG_SEMANTIC_OUTPUT=str(temporary)),
                                stdout=stdout, stderr=stderr)
    if result.returncode:
        print((output / 'stderr.log').read_text(), file=sys.stderr)
        return result.returncode

    # Native serialization is already SQLite. The wrapper only adds snapshots
    # and provenance; there is no intermediate whole-graph JSON file or DOM.
    with SqliteStore(temporary, validate=False) as graph:
        graph.validate()
        source_nodes = list(graph.iter_nodes('source-file'))
        node_count = graph.connection.execute('SELECT count(*) FROM nodes WHERE unit_id=1').fetchone()[0]
        diagnostic_count = graph.connection.execute('SELECT count(*) FROM diagnostics WHERE unit_id=1').fetchone()[0]
    sources, diagnostics = [], []
    with sqlite3.connect(temporary) as database:
        database.execute('PRAGMA synchronous=FULL')
        database.execute('BEGIN')
        for node in source_nodes:
            spelling = node.get('fullPath') or node.get('path')
            entry = {'node': node['id'], 'originalPath': spelling, 'encoding': 'bytes'}
            candidate = Path(spelling)
            if not candidate.is_absolute():
                candidate = Path.cwd() / candidate
            if candidate.is_file():
                # Source files are the package's immutable snapshots, separate
                # from the graph's semantic identities and include occurrences.
                content = candidate.read_bytes()
                sha = hashlib.sha256(content).hexdigest()
                entry.update(available=True, sha256=sha, byteLength=len(content))
                database.execute('INSERT OR IGNORE INTO source_blobs(sha256,bytes) VALUES(?,?)', (sha, content))
                database.execute('INSERT INTO source_files VALUES(?,?,?,?,?,?)',
                                 (1, node['id'], spelling, sha, 1, len(content)))
            else:
                entry['available'] = False
                diagnostics.append({'code': 'SEM_SOURCE_UNAVAILABLE', 'source': node['id'], 'path': spelling})
                database.execute('INSERT INTO source_files VALUES(?,?,?,?,?,?)',
                                 (1, node['id'], spelling, None, 0, None))
            sources.append(entry)
        database.execute('INSERT INTO package_info VALUES(?,?)', ('producer', producer_bytes.decode('utf-8')))
        database.commit()
    with SqliteStore(temporary, validate=False) as graph:
        content_hash = graph.content_sha256()
    inputs = []
    for item in [root / 'upstream/src/il_def.h', root / 'upstream/src/basics.h',
                 root / 'upstream/src/host_envir.h', root / 'semantic-export/build.py',
                 root / 'semantic-export/export.py', root / 'semantic-export/native/semantic_export_be.c',
                 root / 'semantic-export/native/semantic_sqlite.h', root / 'semantic-export/native/generate_names.py',
                 root / 'semantic-export/storage/schema.sql']:
        inputs.append({'path': str(item.relative_to(root)), 'sha256': digest(item)})
    manifest = {
        'schema': 'edg-semantic-package/2', 'databaseFormat': 'edg-semantic-sqlite/1', 'unitId': 1,
        'contentSha256': content_hash, 'invocation': invocation,
        'adapterSourceInputsAtPackaging': inputs,
        'producer': {'sha256': hashlib.sha256(producer_bytes).hexdigest()},
        'sources': sources, 'diagnostics': diagnostics,
        'capabilities': {'unlowered': True, 'sourceOrder': True, 'fullSourcePositions': True,
                         'macroDefinitions': True, 'macroInvocations': True, 'macroArguments': True,
                         'templateInstantiationPolicy': 'used' if cpp else 'not-applicable',
                         'semanticAdapterCoverage': 'partial-explicit-diagnostics', 'codeEmissionReady': False},
        'sourceSnapshotPolicy': 'read-after-frontend; caller must keep inputs immutable during export',
        'locationContract': 'EDG sequence/column plus presumed/physical paths, not byte offsets'}
    with sqlite3.connect(temporary) as database:
        database.execute('INSERT INTO package_info VALUES(?,?)', ('manifest', json_text(manifest)))
    temporary.rename(destination)
    print(f'{destination} ({node_count} graph nodes, {len(sources)} source files, {diagnostic_count} adapter diagnostics)')
    return 0


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except (ValueError, OSError, sqlite3.Error) as error:
        print(f'{type(error).__name__}: {error}', file=sys.stderr)
        raise SystemExit(1)
