#!/usr/bin/env python3
"""Export the genuine semantic-family fixtures, serially."""
import argparse
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
ARTIFACTS = ROOT.parents[0] / 'artifacts/roslynobj'
CASES = {
    'basic': ['--c++20'],
    'm0': ['--c++20'],
    'cleanup_semantics': ['--c++20', '--gnu_version', '130000'],
    'variables': ['--c++20', '--gnu_version', '130000'],
    'reflection': ['--c++26', '--g++', '--set_flag', 'reflection'],
    'source_attributes': ['--c++20', '--gnu_version', '130000'],
    'layout': ['--c++20', '--gnu_version', '130000'],
    'layout_c': ['--c99', '--gnu_version', '130000'],
    'routines': ['--c++20', '--g++', '--gnu_version', '130000'],
    'routines_exception': ['--c++14', '--gnu_version', '130000'],
    'constant_occurrences': ['--c++20', '--g++', '--gnu_version', '130000'],
    'constant_aggregates': ['--c99', '--gnu_version', '130000'],
    'constant_embed': ['--c23', '--gnu_version', '130000'],
    'upc_dynamic': ['--c99', '--upc'],
    'upc_static': ['--c99', '--upc', '--upc_threads', '4'],
    'vector_values': ['--c++20', '--g++', '--gnu_version', '150000'],
    'vector_arm': ['--c++20', '--g++', '--gnu_version', '150000', '--target', 'linux_aarch64'],
    'vector_riscv': ['--c++20', '--g++', '--gnu_version', '150000', '--target', 'linux_riscv64'],
    'near_far': ['--c++20', '--microsoft_16'],
    'execution_c': ['--c11', '--gcc', '--gnu_version', '130000'],
    'execution_cpp': ['--c++20', '--g++', '--gnu_version', '130000'],
    'layout_microsoft': ['--c++20', '--target', 'win64', '--microsoft', '--microsoft_version', '1930'],
    'routines_microsoft': ['--c++20', '--target', 'win64', '--microsoft', '--microsoft_version', '1930'],
}
SOURCE_ONLY = {'layout_microsoft', 'routines_microsoft'}

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--profile', choices=('native', 'source', 'cfront', 'exported'), default='native')
parser.add_argument('--fixture', choices=tuple(CASES), action='append')
parser.add_argument('--output-directory', type=Path)
args = parser.parse_args()
output = args.output_directory or ARTIFACTS / 'semantic-smoke' / args.profile
output.mkdir(parents=True, exist_ok=True)
compiler_root = ARTIFACTS if args.profile == 'native' else ARTIFACTS / 'profiles' / args.profile
environment = os.environ.copy()
environment['ROSLYNOBJ_COMPILER'] = str(compiler_root / 'build/bin/roslyn-cpfe')
results = []
failed = False
selected = args.fixture or [name for name in CASES if args.profile == 'source' or name not in SOURCE_ONLY]
if args.profile != 'source' and any(name in SOURCE_ONLY for name in selected):
    parser.error('Microsoft layout cases require the source profile configured for Microsoft ABI')
for name in selected:
    destination = output / (name + '.roslynobj')
    fixture = ('layout.cpp' if name == 'layout_microsoft' else 'upc_values.c' if name in ('upc_dynamic', 'upc_static')
               else 'execution.c' if name == 'execution_c' else 'execution.cpp' if name == 'execution_cpp'
               else name + ('.c' if name in ('layout_c', 'constant_aggregates', 'constant_embed') else '.cpp'))
    source = (ROOT / 'managed/RoslynObj.Tests/Fixtures' / fixture if name in ('reflection', 'source_attributes', 'routines', 'routines_exception', 'routines_microsoft', 'execution_c', 'execution_cpp')
              else ROOT / 'tests/fixtures' / fixture)
    command = ['python3', str(ROOT / 'build/roslyn-cpfe.py'), *CASES[name],
               str(source), '-o', str(destination)]
    start = time.monotonic()
    with (output / (name + '.log')).open('w') as log:
        completed = subprocess.run(command, env=environment, stdout=log, stderr=subprocess.STDOUT)
    result = {'profile': args.profile, 'fixture': name, 'argv': command,
              'path': str(destination), 'exit_code': completed.returncode,
              'seconds': round(time.monotonic() - start, 3)}
    if completed.returncode == 0:
        with sqlite3.connect(destination.resolve().as_uri() + '?mode=ro', uri=True) as database:
            result['entities'] = database.execute('SELECT count(*) FROM entity').fetchone()[0]
            result['fk_errors'] = len(database.execute('PRAGMA foreign_key_check').fetchall())
            result['page_size'] = database.execute('PRAGMA page_size').fetchone()[0]
        failed |= result['fk_errors'] != 0
    else:
        failed = True
    results.append(result)
    print(json.dumps(result), flush=True)
    (output / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
raise SystemExit(1 if failed else 0)
