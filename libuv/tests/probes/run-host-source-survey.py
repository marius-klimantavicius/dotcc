#!/usr/bin/env python3
"""Attempt every Linux glibc product unit with actual host headers.

Preprocessing preserves native attribute/type declarations. Passing this survey
is object emission only, never native ABI or product qualification.
"""
import argparse
import hashlib
import json
import pathlib
import subprocess

ROOT = pathlib.Path(__file__).resolve().parents[3]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', type=pathlib.Path, default=ROOT / 'libuv/build/compiler/bin/DotCC/release/dotcc.dll')
args = parser.parse_args()
config = ROOT / 'libuv/config'
sources = json.loads((config / 'sources.json').read_text())
ref = ROOT / 'libuv/ref' / sources['product']['directory']
platforms = json.loads((config / 'platforms.json').read_text())
units = next(row['source_units'] for row in platforms['backends'] if row['id'] == 'linux-glibc')
out = ROOT / 'libuv/artifacts/host-source-survey'
out.mkdir(parents=True, exist_ok=True)
rows = []
for unit in units:
    stem = unit.replace('/', '_')
    preprocessed = out / (stem + '.i')
    preprocess = ['cc', '-E', '-P', '-std=gnu11', '-D_GNU_SOURCE', '-D_FILE_OFFSET_BITS=64',
                  '-I' + str(ref / 'include'), '-I' + str(ref / 'src'), str(ref / unit), '-o', str(preprocessed)]
    compile_command = ['dotnet', str(args.compiler), str(preprocessed), '--emit=obj', '-o', str(out / (stem + '.o'))]
    p = subprocess.run(preprocess, capture_output=True, text=True, timeout=60)
    phase = 'preprocess'
    if p.returncode == 0:
        phase = 'emit-object'
        p = subprocess.run(compile_command, capture_output=True, text=True, timeout=60)
    diagnostic = p.stdout + p.stderr
    (out / (stem + '.log')).write_text(diagnostic)
    rows.append({'source': unit, 'source_sha256': hashlib.sha256((ref / unit).read_bytes()).hexdigest(),
                 'phase': phase, 'exit_code': p.returncode, 'diagnostic': diagnostic,
                 'preprocess_command': preprocess, 'compiler_command': compile_command})
    print(unit, phase, p.returncode, diagnostic.splitlines()[0][:180] if diagnostic else '', flush=True)
(out / 'result.json').write_text(json.dumps({'product_qualification': False,
    'compiler_library_sha256': hashlib.sha256((args.compiler.parent / 'DotCC.Lib.dll').read_bytes()).hexdigest(),
    'host_compiler': subprocess.check_output(['cc', '--version'], text=True).splitlines()[0],
    'source_revision': sources['product']['revision'], 'units': rows}, indent=2) + '\n')
