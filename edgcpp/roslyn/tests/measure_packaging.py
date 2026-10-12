#!/usr/bin/env python3
"""Measure valid snapshots at alternate SQLite page sizes, without changing input."""
import argparse
import csv
from pathlib import Path
import sqlite3
import time
import re


def copy_composite_without_rowid(source, output):
    """Repackage ordinary valid rows; retain rowid on all identity tables."""
    schema = source.execute(
        "SELECT type,name,sql FROM sqlite_schema WHERE sql IS NOT NULL "
        "AND name NOT LIKE 'sqlite_%' ORDER BY type='index',name").fetchall()
    changed = []
    for kind, name, sql in schema:
        if kind != 'table':
            continue
        columns = source.execute(f'PRAGMA table_info("{name}")').fetchall()
        if sum(column[5] > 0 for column in columns) > 1:
            assert 'WITHOUT ROWID' not in sql.upper(), name
            sql = re.sub(r'\)\s*(STRICT)?\s*$',
                         lambda match: ') WITHOUT ROWID' +
                         (', STRICT' if match[1] else ''), sql)
            changed.append(name)
        output.execute(sql)
    output.execute('BEGIN')
    for kind, name, _ in schema:
        if kind != 'table':
            continue
        cursor = source.execute(f'SELECT * FROM "{name}"')
        placeholders = ','.join('?' for _ in cursor.description)
        output.executemany(f'INSERT INTO "{name}" VALUES ({placeholders})', cursor)
    for kind, _, sql in schema:
        if kind != 'table':
            output.execute(sql)
    output.commit()
    return changed

parser = argparse.ArgumentParser()
parser.add_argument('objects', nargs='+', type=Path)
parser.add_argument('--output-directory', required=True, type=Path)
parser.add_argument('--compare-without-rowid', action='store_true',
                    help='Also repackage composite-PK ordered relations without rowid')
args = parser.parse_args()
args.output_directory.mkdir(parents=True, exist_ok=True)
writer = csv.writer(__import__('sys').stdout, delimiter='\t', lineterminator='\n')
writer.writerow(['object', 'layout', 'page_bytes', 'file_bytes', 'pages', 'entities',
                 'repackage_ms', 'scan_ms', 'indexes_bytes', 'composite_tables'])
for path in args.objects:
    source = sqlite3.connect(path.resolve().as_uri() + '?mode=ro', uri=True)
    source.execute('BEGIN')
    entities = source.execute('SELECT count(*) FROM entity').fetchone()[0]
    layouts = ('rowid', 'composite-without-rowid') if args.compare_without_rowid else ('rowid',)
    for layout, page_bytes in ((layout, size) for layout in layouts
                             for size in (1024, 2048, 4096, 8192)):
        destination = args.output_directory / f'{path.stem}-{layout}-{page_bytes}.roslynobj'
        if destination.exists():
            destination.unlink()
        output = sqlite3.connect(destination)
        started = time.perf_counter()
        output.execute(f'PRAGMA page_size={page_bytes}')
        changed = []
        if layout == 'rowid':
            source.backup(output)
            output.execute(f'PRAGMA page_size={page_bytes}')
        else:
            changed = copy_composite_without_rowid(source, output)
        output.execute('VACUUM')
        repackage_ms = (time.perf_counter() - started) * 1000
        assert not output.execute('PRAGMA foreign_key_check').fetchall()
        assert output.execute('SELECT count(*) FROM entity').fetchone()[0] == entities
        for name, in source.execute("SELECT name FROM sqlite_schema WHERE type='table' "
                                    "AND name NOT LIKE 'sqlite_%'"):
            assert source.execute(f'SELECT count(*) FROM "{name}"').fetchone() == \
                output.execute(f'SELECT count(*) FROM "{name}"').fetchone(), name
        # A full valid entity/relation scan isolates packaging effects. Managed
        # public traversal performance is measured separately in its harness.
        started = time.perf_counter()
        for table in ('entity', 'structural_child', 'constant', 'text_atom'):
            for _ in output.execute(f'SELECT * FROM {table}'):
                pass
        scan_ms = (time.perf_counter() - started) * 1000
        pages = output.execute('PRAGMA page_count').fetchone()[0]
        indexes_bytes = output.execute(
            "SELECT coalesce(sum(pgsize),0) FROM dbstat WHERE name IN "
            "(SELECT name FROM sqlite_schema WHERE type='index')").fetchone()[0]
        output.close()
        writer.writerow([path.name, layout, page_bytes, destination.stat().st_size, pages,
                         entities, f'{repackage_ms:.2f}', f'{scan_ms:.2f}', indexes_bytes,
                         len(changed)])
    source.close()
