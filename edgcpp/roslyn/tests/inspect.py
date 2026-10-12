#!/usr/bin/env python3
"""Inspect successful EDG exports and measure their normalized representation.

This is storage/projection integration evidence, not C++ semantic validation.
It accepts only real exporter outputs; no corrupted-artifact campaign is used.
"""
import argparse
import csv
import sqlite3
from pathlib import Path

p=argparse.ArgumentParser()
p.add_argument('objects',nargs='+',type=Path)
p.add_argument('--report',type=Path)
args=p.parse_args()
counts=[]
for path in args.objects:
    c=sqlite3.connect(path.resolve().as_uri()+'?mode=ro',uri=True)
    c.execute('BEGIN')
    header=c.execute('SELECT format_major,format_minor,artifact_id,complete FROM object_header').fetchall()
    assert len(header)==1 and header[0][:2]==(1,0) and header[0][3]==1, (path,'complete 1.0 header')
    assert not c.execute('PRAGMA foreign_key_check').fetchall(), (path,'unresolved storage references')
    tables=c.execute("SELECT name FROM sqlite_schema WHERE type='table' ORDER BY name").fetchall()
    for (name,) in tables:
        columns=c.execute('PRAGMA table_info("'+name+'")').fetchall()
        assert all(row[2] not in ('BLOB','REAL') for row in columns), (path,name,'inexact/opaque semantic column')
        count=c.execute('SELECT COUNT(*) FROM "'+name+'"').fetchone()[0]
        counts.append((str(path),name,count))
    dangling=c.execute('SELECT e.id FROM expression e LEFT JOIN type t ON e.type_id=t.id WHERE t.id IS NULL LIMIT 1').fetchall()
    assert not dangling, (path,'accepted expression missing execution type')
    stats=dict(c.execute("SELECT 'functions',COUNT(*) FROM routine_symbol UNION ALL SELECT 'bodies',COUNT(*) FROM routine_symbol WHERE body_root_id IS NOT NULL UNION ALL SELECT 'operations',COUNT(*) FROM semantic_node UNION ALL SELECT 'constants',COUNT(*) FROM constant UNION ALL SELECT 'cleanup_regions',COUNT(*) FROM cleanup_region UNION ALL SELECT 'macros',COUNT(*) FROM macro_invocation"))
    print(path, path.stat().st_size, 'bytes', ', '.join(f'{key}={value}' for key,value in stats.items()))
    c.close()
if args.report:
    args.report.parent.mkdir(parents=True,exist_ok=True)
    with args.report.open('w') as f:
        w=csv.writer(f,delimiter='\t',lineterminator='\n')
        w.writerow(['object','table','rows']);w.writerows(counts)
