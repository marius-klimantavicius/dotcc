#!/usr/bin/env python3
"""Export the genuine current lifetime and variable fixtures, serially."""
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
}

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
for name in args.fixture or CASES:
    destination = output / (name + '.roslynobj')
    source = (ROOT / 'managed/RoslynObj.Tests/Fixtures' / (name + '.cpp') if name in ('reflection', 'source_attributes')
              else ROOT / 'tests/fixtures' / (name + '.cpp'))
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
        failed |= result['fk_errors'] != 0
    else:
        failed = True
    results.append(result)
    print(json.dumps(result), flush=True)
    (output / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
raise SystemExit(1 if failed else 0)
