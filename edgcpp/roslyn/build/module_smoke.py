#!/usr/bin/env python3
"""Create stock IFC dependencies and export their genuine source-profile consumers."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
ARTIFACTS = ROOT.parents[0] / 'artifacts/roslynobj'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-directory', type=Path)
parser.add_argument('--case', action='append',
                    choices=('module_partition', 'module_primary',
                             'module_header', 'module_header_consumer'))
args = parser.parse_args()
output = (args.output_directory or ARTIFACTS / 'module-smoke/source').resolve()
output.mkdir(parents=True, exist_ok=True)
cache = output / 'ifc'
cache.mkdir(exist_ok=True)
partition_ifc = cache / 'RoslynParts-Part.eifc'
primary_ifc = cache / 'RoslynParts.eifc'
header_ifc = cache / 'Header.eifc'
selected = set(args.case or ('module_partition', 'module_primary',
                            'module_header', 'module_header_consumer'))
if 'module_primary' in selected:
    selected.add('module_partition')
if 'module_header_consumer' in selected:
    selected.add('module_header')
for name, path in (('module_partition', partition_ifc), ('module_primary', primary_ifc),
                   ('module_header', header_ifc)):
    if name in selected:
        path.unlink(missing_ok=True)
fixtures = ROOT / 'tests/fixtures'
header = (fixtures / 'module_header.hpp').resolve()
compiler = ARTIFACTS / 'profiles/source/build/bin/roslyn-cpfe'
environment = os.environ.copy()
environment['ROSLYNOBJ_COMPILER'] = str(compiler)
cases = [
    ('module_partition', fixtures / 'module_partition.cpp',
     ['--create_module_interface', str(partition_ifc)]),
    ('module_primary', fixtures / 'module_primary.cpp',
     ['--modules_directory', str(cache), '--create_module_interface', str(primary_ifc)]),
    ('module_header', header, ['--create_header_unit', str(header_ifc)]),
    ('module_header_consumer', fixtures / 'module_header_consumer.cpp',
     ['--header_unit', str(header) + '=' + str(header_ifc), '-I', str(fixtures.resolve())]),
]
results = []
failed = 0
for name, source, options in cases:
    if name not in selected:
        continue
    destination = output / (name + '.roslynobj')
    command = ['python3', str(ROOT / 'build/roslyn-cpfe.py'), '--c++20', '--modules',
               *options, str(source), '-o', str(destination)]
    start = time.monotonic()
    with (output / (name + '.log')).open('w') as log:
        result = subprocess.run(command, env=environment, cwd=output,
                                stdout=log, stderr=subprocess.STDOUT)
    row = dict(profile='source', fixture=name, argv=command, compiler=str(compiler),
               path=str(destination), exit_code=result.returncode,
               seconds=round(time.monotonic() - start, 3))
    if not result.returncode:
        with sqlite3.connect(destination.as_uri() + '?mode=ro', uri=True) as database:
            row['entities'] = database.execute('SELECT count(*) FROM entity').fetchone()[0]
            row['tables'] = database.execute("SELECT count(*) FROM sqlite_schema WHERE type='table'").fetchone()[0]
            row['fk_errors'] = len(database.execute('PRAGMA foreign_key_check').fetchall())
            row['page_size'] = database.execute('PRAGMA page_size').fetchone()[0]
            capabilities = dict(database.execute('SELECT n.value,v.value FROM build_capability b JOIN text_atom n ON n.id=b.name_id JOIN text_atom v ON v.id=b.value_id'))
            row['schema_contract'] = capabilities['normalized_semantic_schema_v1']
            assert capabilities['roslyn_build_profile'] == 'source'
            assert not row['fk_errors'] and row['page_size'] == 1024
    results.append(row)
    print(json.dumps(row), flush=True)
    (output / 'attempt-results.json').write_text(json.dumps(results, indent=2) + '\n')
    (output / 'all-profile-results-final.json').write_text(
        json.dumps([item for item in results if not item['exit_code']], indent=2) + '\n')
    if result.returncode:
        failed = result.returncode
dependencies = [dict(path=str(path), bytes=path.stat().st_size,
                     sha256=hashlib.sha256(path.read_bytes()).hexdigest())
                for path in (partition_ifc, primary_ifc, header_ifc) if path.is_file()]
(output / 'ifc-dependencies.json').write_text(json.dumps(dependencies, indent=2) + '\n')
raise SystemExit(failed)
