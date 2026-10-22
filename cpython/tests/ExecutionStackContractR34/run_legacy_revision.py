"""RED held shared source versus fixed null binding, then affected JIT/AOT."""
from pathlib import Path
import datetime,fcntl,hashlib,json,sys,time,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[3];sys.path.insert(0,str(ROOT/'Scripts'))
PROVENANCE = json.loads((ROOT / 'cpython/config/qualification-provenance.json').read_text())['executionStackContractR34']
from campaigns.process import execute
ART=ROOT/'cpython/artifacts/owner-b-r34-20261018';WORK=ROOT/'cpython/build/owner-b-r34-20261018';HERE=ROOT/'cpython/tests/ExecutionStackContractR34'
release=ROOT/'cpython/artifacts/owner-a-r29-20261018/v19/serial-slot-release-to-b34-knownbad.json'
assert hashlib.sha256(release.read_bytes()).hexdigest()==PROVENANCE['legacyReleaseSha256']
r=json.loads(release.read_text())
# The exact release field names are retained by its owner; all listed terminal
# PIDs and locks are independently checked before this batch starts.
for pid in r['terminalPidsAbsent']: assert not Path(f'/proc/{pid}').exists(),pid
for p in ['/tmp/dotcc-cpython-shared-build.lock','/tmp/dotcc-cpython-native-build.lock',str(ROOT/'cpython/build/campaign/run.lock')]:
 with open(p,'a') as f: fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB);fcntl.flock(f,fcntl.LOCK_UN)
commands=[];start=time.monotonic()
def run(argv,label,expected=0,timeout=90):
 row={};remaining=300-(time.monotonic()-start);assert remaining>0
 try:
  execute(['flock','-n','--close','/tmp/dotcc-cpython-shared-build.lock',*map(str,argv)],label=label,cwd=ROOT,output=ART/f'{label}.log',timeout=min(timeout,remaining),record=row,check=False)
 finally:
  commands.append(row);(ART/'legacy-revision-commands.json').write_text(json.dumps(commands,indent=2)+'\n')
 assert row['exit_code']==expected,(label,row)
 print(json.dumps({'label':label,'rawExit':row['exit_code'],'expectedExit':expected,'seconds':row['seconds']}),flush=True)
common=['-c','Release','-p:NuGetAudit=false','-p:RestoreIgnoreFailedSources=true','-p:UseSharedCompilation=false','-nodeReuse:false']
old=ROOT/'cpython/build/owner-a-r29-20261018/v19/source-bytes/DotCC.Libc/PthreadLib.cs'
assert hashlib.sha256(old.read_bytes()).hexdigest()==PROVENANCE['legacyPthreadSourceSha256']
run(['dotnet','test',HERE/'SharedPolicyContracts.csproj',*common,'--artifacts-path',WORK/'legacy-baseline-products',f'-p:SharedLibcProject={HERE/"LegacyBaselineLibc.csproj"}',
 '--filter','FullyQualifiedName~Legacy_binding_stays_null','--logger','trx;LogFileName=legacy-red.trx','--results-directory',ART/'legacy-red-results'],'legacy-held-source-red',expected=1,timeout=120)
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
c=ET.parse(ART/'legacy-red-results/legacy-red.trx').find('.//t:Counters',ns).attrib
assert c['failed']=='1' and c['executed']=='1',c
run(['dotnet','test',HERE/'SharedPolicyContracts.csproj',*common,'--artifacts-path',WORK/'revised-shared-products','--logger','trx;LogFileName=revised.trx','--results-directory',ART/'revised-unit-results'],'legacy-fixed-affected-units',timeout=120)
run(['dotnet','build',HERE/'LegacyWorkerBoundary.csproj',*common,'--artifacts-path',WORK/'revised-shared-products'],'legacy-process-boundary-build')
run(['dotnet',WORK/'revised-shared-products/bin/LegacyWorkerBoundary/release/LegacyWorkerBoundary.dll'],'legacy-process-exit-37',expected=37,timeout=30)
run(['dotnet','publish',HERE/'SharedPolicyProbe.csproj',*common,'--artifacts-path',WORK/'revised-shared-products','-r','linux-x64','-p:PublishAot=true','-o',WORK/'revised-shared-aot'],'revised-current-libc-aot-publish',timeout=120)
run([WORK/'revised-shared-aot/SharedPolicyProbe'],'revised-current-libc-aot-physical',timeout=30)
print(json.dumps({'status':'revised shared regression batch passed','seconds':time.monotonic()-start}),flush=True)
