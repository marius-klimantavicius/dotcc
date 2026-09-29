#!/usr/bin/env python3
"""Attempt native AST closure, native recompilation and DotCC objects for all Linux units."""
import argparse
import hashlib
import json
import pathlib
import subprocess
import sys

HERE = pathlib.Path(__file__).resolve().parent
ROOT = HERE.parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', type=pathlib.Path, default=ROOT / 'libuv/build/compiler/bin/DotCC/release/dotcc.dll')
args = parser.parse_args()
config = json.loads((ROOT / 'libuv/config/sources.json').read_text())
platforms = json.loads((ROOT / 'libuv/config/platforms.json').read_text())
units = next(row['source_units'] for row in platforms['backends'] if row['id'] == 'linux-glibc')
out = ROOT / 'libuv/artifacts/native-closure-survey'
out.mkdir(parents=True, exist_ok=True)
receipt = out / 'result.json'
receipt.write_text(json.dumps({'status': 'running', 'diagnostic_only': True}) + '\n')
rows = []
for unit in units:
    directory = ROOT / 'libuv/artifacts/native-closures/linux-glibc-x64' / pathlib.Path(unit).with_suffix('')
    stages = [
        ('materialize', [sys.executable, HERE / 'materialize-native-closure.py', '--records', unit]),
        ('native-compile', ['cc', '-std=gnu11', '-c', directory / 'closure.c', '-o', directory / 'native-closure.o']),
        ('emit-object', ['dotnet', args.compiler, directory / 'closure.c', '--emit=obj', '-o', directory / 'closure.o']),
    ]
    commands = []
    log = ''
    for stage, command in stages:
        commands.append([str(item) for item in command])
        try:
            result = subprocess.run(commands[-1], cwd=ROOT, capture_output=True, text=True, timeout=120)
            log += stage + '\n' + result.stdout + result.stderr
            exit_code = result.returncode
        except subprocess.TimeoutExpired:
            log += stage + '\nTimed out after 120 seconds.\n'
            exit_code = 124
        if exit_code:
            break
    (out / (unit.replace('/', '_') + '.log')).write_text(log)
    diagnostic = log.strip().splitlines()[-1] if log.strip() else ''
    rows.append({'source': unit, 'stage': stage, 'exit_code': exit_code, 'commands': commands, 'diagnostic': diagnostic})
    print(unit, stage, exit_code, diagnostic[:150], flush=True)
receipt.write_text(json.dumps({'status': 'completed', 'diagnostic_only': True, 'source_revision': config['product']['revision'],
    'compiler_library_sha256': hashlib.sha256((args.compiler.parent / 'DotCC.Lib.dll').read_bytes()).hexdigest(),
    'units': rows}, indent=2) + '\n')
