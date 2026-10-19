"""Bounded B34 support/current-Libc contracts; no Python execution claim."""
from pathlib import Path
import fcntl, hashlib, json, os, sys, time
ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'Scripts'))
from campaigns.process import execute
ART = ROOT / 'cpython/artifacts/owner-b-r34-20261018'
WORK = ROOT / 'cpython/build/owner-b-r34-20261018'
ART.mkdir(parents=True, exist_ok=True)
WORK.mkdir(parents=True, exist_ok=True)
release = ROOT / 'cpython/artifacts/owner-a-r29-20261018/serial-slot-release-all.json'
assert hashlib.sha256(release.read_bytes()).hexdigest() == 'f522a414ad0e97df7805711a151fe8c0f0ec73e8502fafecadd37264786bb09b'
for pid in json.loads(release.read_text())['terminalPidsAbsent']:
    assert not Path(f'/proc/{pid}').exists(), pid
for path in ['/tmp/dotcc-cpython-shared-build.lock','/tmp/dotcc-cpython-native-build.lock',str(ROOT/'cpython/build/campaign/run.lock')]:
    with open(path, 'a') as stream:
        fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
        fcntl.flock(stream, fcntl.LOCK_UN)
records = []
started = time.monotonic()
def run(args, label, timeout=90, lock='shared-build'):
    remaining = 300 - (time.monotonic()-started)
    assert remaining > 0, 'B34 initial batch budget exhausted'
    row = {}
    try:
        execute(['flock','-n','--close',f'/tmp/dotcc-cpython-{lock}.lock',*map(str,args)],
                label=label,cwd=ROOT,output=ART/f'{label}.log',timeout=min(timeout,remaining),record=row)
    finally:
        records.append(row)
        (ART/'focused-commands.json').write_text(json.dumps(records,indent=2)+'\n')
    print(json.dumps({'label':label,'status':row['status'],'seconds':row['seconds']}),flush=True)
common = ['-c','Release','--artifacts-path',str(WORK/'focused-products'),'-p:NuGetAudit=false','-p:RestoreIgnoreFailedSources=true']
run(['dotnet','build',ROOT/'cpython/tests/Contracts/Contracts.csproj',*common], 'support-build')
support = WORK/'focused-products/bin/Contracts/release/Contracts.dll'
run(['dotnet',support], 'support-contracts',30)
run(['dotnet',support,'--faulted-owner'], 'support-fault-reservation',30)
run(['dotnet','test',ROOT/'cpython/tests/ExecutionStackContractR34/SharedPolicyContracts.csproj',*common,
     '--logger',f'trx;LogFileName=stack-policy.trx','--results-directory',ART/'unit-results'], 'current-libc-contracts',120)
print(json.dumps({'status':'support/current-Libc batch passed','seconds':time.monotonic()-started,'interpreter':'UNRUN; normal capture/regeneration required'}),flush=True)
