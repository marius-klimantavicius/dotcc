#!/usr/bin/env python3
"""Run isolated JIT/reflection ownership regressions against built facade binaries."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--assemblies', type=Path, required=True,
                    help='Directory containing ManagedSmb.dll and TranslatedLibsmb2.dll')
parser.add_argument('--label', default='final', help='Artifact receipt label')
args = parser.parse_args()
if not args.label or any(c not in 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_' for c in args.label):
    parser.error('--label must contain only letters, digits, dash or underscore')
assemblies = args.assemblies.resolve()
paths = [assemblies / name for name in ['ManagedSmb.dll', 'TranslatedLibsmb2.dll']]
for path in paths:
    if not path.is_file(): parser.error(f'Missing binary: {path}')
out = ROOT / 'artifacts/facade-lifetime' / args.label
out.mkdir(parents=True, exist_ok=True)
build = ROOT / 'build/facade-lifetime' / args.label
receipt = dict(passed=False, mode='jit-reflection-checked-heap', cases=[],
               excluded_by_scope=['failed-first-close', 'pending-read-abort'],
               assemblies={p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in paths})
try:
    command = ['dotnet', 'build', str(ROOT / 'tests/FacadeLifetime/FacadeLifetime.csproj'),
               '-c', 'Release', '--nologo', '-o', str(build),
               '-p:Libsmb2FacadeAssembly=' + str(paths[0]),
               '-p:Libsmb2TranslationAssembly=' + str(paths[1])]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    (out / 'build.log').write_text(result.stdout)
    receipt['build_exit_code'] = result.returncode
    result.check_returncode()
    env = dict(os.environ, DOTCC_DEBUG_HEAP='1', DOTCC_DEBUG_HEAP_SCAN='1')
    for name in ['finalizer-cleanup', 'actual-finalizer']:
        result = subprocess.run(['dotnet', str(build / 'FacadeLifetime.dll'), name],
                                stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True,
                                env=env, timeout=20)
        (out / (name + '.log')).write_text(result.stdout)
        receipt['cases'].append(dict(name=name, passed=result.returncode == 0, exit_code=result.returncode))
        print(f"{name}: {'PASS' if result.returncode == 0 else 'FAIL'}", flush=True)
    receipt['passed'] = all(case['passed'] for case in receipt['cases'])
finally:
    (out / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
raise SystemExit(0 if receipt['passed'] else 1)
