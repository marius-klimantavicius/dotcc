#!/usr/bin/env python3
"""Qualify the owning controller protocol using disposable managed fixture processes.
This does not claim translated guest execution; ServiceExecution owns that gate.
"""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile
import time
ROOT = Path(__file__).resolve().parents[2]
base = ROOT / 'artifacts/instance-lifecycle'
base.mkdir(parents=True, exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
receipt = {'kind': 'managed-controller-process-lifecycle', 'passed': False, 'results': {}}
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
inputs = [*Path(__file__).parent.glob('*'), *(ROOT / 'src/Managed.Emulation').glob('*')]
receipt['sources'] = {str(p.relative_to(ROOT)): sha(p) for p in inputs if p.is_file()}
def save(): (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
def run(command, label, timeout=180):
    start = time.monotonic()
    with (out / (label + '.log')).open('wb') as log:
        result = subprocess.run(list(map(str, command)), stdout=log, stderr=subprocess.STDOUT, timeout=timeout, env=dict(os.environ, LC_ALL='C'))
    receipt['results'][label] = {'command':list(map(str,command)), 'exit_code':result.returncode,'seconds':time.monotonic()-start}
    save()
    if result.returncode: raise RuntimeError(label + ' failed: ' + str(out))
try:
    project = Path(__file__).parent / 'InstanceLifecycle.csproj'
    run(['dotnet','run','--project',project,'-c','Release'], 'jit')
    run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',out / 'publish'], 'aot-build',300)
    run([out / 'publish/InstanceLifecycle'], 'aot')
    for name,digest in receipt['sources'].items():
        if sha(ROOT / name) != digest: raise RuntimeError('Source drift: ' + name)
    receipt['aot_sha256']=sha(out / 'publish/InstanceLifecycle')
    receipt['passed']=True
    print(out / 'receipt.json')
finally: save()
