#!/usr/bin/env python3
"""Measure valid snapshots at alternate SQLite page sizes, without changing input."""
import argparse
import csv
from pathlib import Path
import sqlite3
import time

parser = argparse.ArgumentParser()
parser.add_argument('objects', nargs='+', type=Path)
parser.add_argument('--output-directory', required=True, type=Path)
args = parser.parse_args()
args.output_directory.mkdir(parents=True, exist_ok=True)
writer = csv.writer(__import__('sys').stdout, delimiter='\t', lineterminator='\n')
writer.writerow(['object', 'page_bytes', 'file_bytes', 'pages', 'entities',
                 'repackage_ms', 'scan_ms', 'indexes_bytes'])
for path in args.objects:
    source = sqlite3.connect(path.resolve().as_uri() + '?mode=ro', uri=True)
    source.execute('BEGIN')
    entities = source.execute('SELECT count(*) FROM entity').fetchone()[0]
    for page_bytes in (1024, 2048, 4096, 8192):
        destination = args.output_directory / f'{path.stem}-{page_bytes}.roslynobj'
        if destination.exists():
            destination.unlink()
        output = sqlite3.connect(destination)
        started = time.perf_counter()
        source.backup(output)
        output.execute(f'PRAGMA page_size={page_bytes}')
        output.execute('VACUUM')
        repackage_ms = (time.perf_counter() - started) * 1000
        assert not output.execute('PRAGMA foreign_key_check').fetchall()
        assert output.execute('SELECT count(*) FROM entity').fetchone()[0] == entities
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
        writer.writerow([path.name, page_bytes, destination.stat().st_size, pages,
                         entities, f'{repackage_ms:.2f}', f'{scan_ms:.2f}', indexes_bytes])
    source.close()
