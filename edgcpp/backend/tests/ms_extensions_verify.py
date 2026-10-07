#!/usr/bin/env python3
"""Compare named anonymous C struct members with GCC and translated JIT/AOT."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--output-dir', type=Path, required=True)
p.add_argument('--nativeaot', action='store_true')
a = p.parse_args()
out = a.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
source = Path(__file__).with_name('ms_extensions.c')
checks = []
def run(label, command):
    r = subprocess.run(list(map(str, command)), capture_output=True)
    (out / (label + '.stdout')).write_bytes(r.stdout)
    (out / (label + '.stderr')).write_bytes(r.stderr)
    checks.append(dict(label=label, command=list(map(str, command)), exitCode=r.returncode))
    (out/'results.json').write_text(json.dumps(dict(status='running' if r.returncode==0 else 'failed', checks=checks), indent=2)+'\n')
    if r.returncode:
        raise RuntimeError(label + ' failed')
run('native-build', ['gcc', '-std=c11', '-fms-extensions', source, '-o', out/'native'])
run('native-run', [out/'native'])
run('translate', [sys.executable, ROOT/'dotnet.py', source, '--language', 'c', '--ms-extensions', '--output-dir', out/'managed'])
assert json.loads((out/'managed/dot-gen.json').read_text())['msExtensions']
run('jit-build', ['dotnet','build',out/'managed/Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
run('jit-run', ['dotnet',out/'managed/bin/Release/net10.0/Generated.dll'])
if a.nativeaot:
    run('aot-build', ['dotnet','publish',out/'managed/Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:TreatWarningsAsErrors=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
    run('aot-run',[out/'aot/Generated'])
(out/'results.json').write_text(json.dumps(dict(status='passed',checks=checks),indent=2)+'\n')
print('Microsoft anonymous member extension checks passed')
