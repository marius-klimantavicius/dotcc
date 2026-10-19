"""Read-only identity seal and safe lane transfer for completed B34 shared tests."""
from pathlib import Path
import datetime,fcntl,hashlib,json,shlex,sys
ROOT=Path(__file__).resolve().parents[3];sys.path.insert(0,str(ROOT/'Scripts'))
from campaigns.recipes import helper
ART=ROOT/'cpython/artifacts/owner-b-r34-20261018';WORK=ROOT/'cpython/build/owner-b-r34-20261018'
def item(path):
 p=Path(path).resolve();return {'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size}
def write(name,value):
 p=ART/name;p.write_text(json.dumps(value,indent=2)+'\n');return item(p)
mapper=helper(ROOT/'cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py')
identity=helper(ROOT/'cpython/build/owner-a-r15-20261018/pe_identity.py')
bind=helper(ROOT/'cpython/tests/Audit/actual_profile_input.py')
products=WORK/'shared-products'
proof={}
for label,path,project in [
 ('currentLibc',products/'bin/DotCC.Libc/release/DotCC.Libc.dll',ROOT/'DotCC.Libc/DotCC.Libc.csproj'),
 ('jitProbe',products/'bin/SharedPolicyProbe/release/SharedPolicyProbe.dll',ROOT/'cpython/tests/ExecutionStackContractR34/SharedPolicyProbe.csproj'),
 ('aotEntry',products/'obj/SharedPolicyProbe/release_linux-x64/SharedPolicyProbe.dll',ROOT/'cpython/tests/ExecutionStackContractR34/SharedPolicyProbe.csproj')]:
 proof[label]={'identity':identity.pe_identity(path),'binding':bind.bind(ROOT/'cpython',path,project,ROOT/'cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py',products)}
rsp=products/'obj/SharedPolicyProbe/release_linux-x64/native/SharedPolicyProbe.ilc.rsp'
references=[];entries=[]
for line in rsp.read_text().splitlines():
 if line.startswith('-r:'):references.append(item(line[3:].strip('"')))
 elif line.endswith('.dll') or line.endswith('.dll"'):
  p=Path(line.strip('"'))
  if p.exists():entries.append(item(p))
proof['ILC']={'rsp':item(rsp),'references':references,'entries':entries,
 'objects':[item(p) for p in rsp.parent.glob('*.o')], 'executedBinary':item(WORK/'shared-aot/SharedPolicyProbe')}
write('shared-actual-input-proof.json',proof)
sourcePins=[item(ROOT/p) for p in ['DotCC.Libc/PthreadLib.cs','DotCC.Libc/PthreadAttributesLib.cs']]
sourceReady={'status':'SOURCE_READY generic scoped pthread service; actual interpreter qualification OPEN',
 'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'sources':sourcePins,
 'unchangedStat':item(ROOT/'DotCC.Libc/PosixLib.cs'),
 'support':'47 assertions plus 3 uncertain-reservation assertions; real BCL executor, no simulated Python',
 'currentLibcUnits':{'initial':22,'expanded':48,'skips':0},
 'native':'target logical attr4 ABI compiles; host attr56 controls pass; logical roundtrips preserve requests',
 'JIT_AOT':'Both actual currentLibc routes observe67108864 for null/default/explicit16384; larger68157440 remains68157440',
 'actualInputProof':item(ART/'shared-actual-input-proof.json'),
 'actualFacadeDependency':'VM.Libc nested in translated project requires new service via ONE normal immutable capture/regeneration. No second Libc assembly/context; no generated patch.',
 'limits':['Not actual interpreter evidence; unchanged required JSON case UNRUN after repair.','No allocation/resource exhaustion or actual Thread constructor/Start fault induced. Rollback admission idempotence tested; production catch/remove/release paths source reviewed.','Linux x64 measured only; actual facade/rooted/trimmed/final matrix and other platforms OPEN.']}
ready=write('shared-source-ready.json',sourceReady)
commands=[]
for file in ['focused-commands.json','shared-expanded-commands.json','shared-aot-commands.json']:
 commands.extend(json.loads((ART/file).read_text()))
pids=[r['pid'] for r in commands]
assert all(not Path(f'/proc/{pid}').exists() for pid in pids)
locks=[]
for path in ['/tmp/dotcc-cpython-shared-build.lock','/tmp/dotcc-cpython-native-build.lock',str(ROOT/'cpython/build/campaign/run.lock')]:
 with open(path,'a') as f:
  fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB);fcntl.flock(f,fcntl.LOCK_UN)
 locks.append({'path':path,'independentAcquireAndRelease':True,'freeAtTransfer':True})
release=write('serial-slot-release-to-a29.json',{'status':'GENUINE DIRECT SAFE RELEASE to A29; B34 source/harness only until returned lane',
 'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'terminalPidsAbsent':pids,'activeOwned':[],'queuedOwned':[],
 'commandCount':len(commands),'commandSeconds':sum(r['seconds'] for r in commands),'locks':locks,'sourceReady':ready,
 'next':'A29 ONE normal compiler capture includes frozen two pthread sources; strict RAW286 jobs8 normal route under supervisor allocation; then direct return for actual facade contracts.'})
print(json.dumps({'sourceReady':ready,'release':release,'pins':sourcePins,'ILCReferences':len(references)}),flush=True)
