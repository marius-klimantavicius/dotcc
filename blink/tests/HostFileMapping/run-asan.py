#!/usr/bin/env python3
"""Check native ownership/rollback against the last frozen mapping inputs."""
import hashlib,json,os,subprocess,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
latest=json.loads((ROOT/'artifacts/host-file-mapping/latest.json').read_text())
source=ROOT/latest['receipt'];receipt=json.loads(source.read_text())
if not receipt['passed']:raise RuntimeError('a successful frozen mapping matrix is required')
command=receipt['results']['native-adapter-build']['command'].copy()
binary=Path(command[command.index('-o')+1]);attempt=binary.parent
for name in ['probe.c','HostMemory.c','HostMemory.h','HostFileMapping.h']:
    if hashlib.sha256((attempt/name).read_bytes()).hexdigest()!=receipt['inputs'][name]:
        raise RuntimeError('frozen input changed: '+name)
out=Path(tempfile.mkdtemp(prefix='asan-',dir=ROOT/'artifacts/host-file-mapping'))
command[command.index('-o')+1]=str(out/'native-adapter-asan')
command[1:1]=['-g','-fsanitize=address','-fno-omit-frame-pointer']
result={'sourceReceipt':str(source.relative_to(ROOT)),'sourceReceiptSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'buildCommand':command,'passed':False}
with(out/'build.log').open('wb')as log:
    result['buildExit']=subprocess.run(command,cwd=attempt,stdout=log,stderr=log).returncode
if result['buildExit']==0:
    with(out/'run.log').open('wb')as log:
        result['runExit']=subprocess.run([str(out/'native-adapter-asan')],cwd=attempt,env=dict(os.environ,ASAN_OPTIONS='detect_leaks=1:halt_on_error=1'),stdout=log,stderr=log,timeout=60).returncode
    result['passed']=result['runExit']==0
(out/'receipt.json').write_text(json.dumps(result,indent=2)+'\n')
if not result['passed']:raise RuntimeError('ASan ownership check failed: '+str(out))
print('ASan native ownership and injected read-failure rollback passed: '+str(out/'receipt.json'))
