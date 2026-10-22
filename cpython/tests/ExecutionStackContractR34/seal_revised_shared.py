from pathlib import Path
import datetime,fcntl,hashlib,json,sys
ROOT=Path(__file__).resolve().parents[3];sys.path.insert(0,str(ROOT/'Scripts'))
PROVENANCE = json.loads((ROOT / 'cpython/config/qualification-provenance.json').read_text())['executionStackContractR34']
from campaigns.recipes import helper
ART=ROOT/'cpython/artifacts/owner-b-r34-20261018';WORK=ROOT/'cpython/build/owner-b-r34-20261018';products=WORK/'revised-shared-products'
def item(p):
 p=Path(p).resolve();return {'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size}
def write(n,v):
 p=ART/n;p.write_text(json.dumps(v,indent=2)+'\n');return item(p)
identity=helper(ROOT/'cpython/build/owner-a-r15-20261018/pe_identity.py');binding=helper(ROOT/'cpython/tests/Audit/actual_profile_input.py');mapper=ROOT/'cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py'
proof={}
for key,pe,project in [('libc',products/'bin/DotCC.Libc/release/DotCC.Libc.dll',ROOT/'DotCC.Libc/DotCC.Libc.csproj'),('aotEntry',products/'obj/SharedPolicyProbe/release_linux-x64/SharedPolicyProbe.dll',ROOT/'cpython/tests/ExecutionStackContractR34/SharedPolicyProbe.csproj')]:
 proof[key]={'identity':identity.pe_identity(pe),'sourceBinding':binding.bind(ROOT/'cpython',pe,project,mapper,products)}
rsp=products/'obj/SharedPolicyProbe/release_linux-x64/native/SharedPolicyProbe.ilc.rsp'
proof['ILC']={'rsp':item(rsp),'references':[item(s[3:].strip('"')) for s in rsp.read_text().splitlines() if s.startswith('-r:')],
 'objects':[item(p) for p in rsp.parent.glob('*.o')],'executedBinary':item(WORK/'revised-shared-aot/SharedPolicyProbe')}
actualProof=write('revised-shared-actual-input-proof.json',proof)
pins=[item(ROOT/p) for p in ['DotCC.Libc/PthreadLib.cs','DotCC.Libc/PthreadAttributesLib.cs','DotCC.Libc/PosixLib.cs']]
old=ROOT/'cpython/build/owner-a-r29-20261018/v19/source-bytes/DotCC.Libc/PthreadLib.cs'
assert item(old)['sha256']==PROVENANCE['legacyPthreadSourceSha256']
ready=write('revised-shared-source-ready.json',{'status':'REVISED SOURCE_READY; nullable legacy binding preserved; actual Python qualification OPEN',
 'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'sources':pins,
 'supersedes':{'oldSourceReady':item(ART/'shared-source-ready.json'),'reason':'Initial green controls missed legacy Current null semantics; d9 candidate unsuitable, never production-qualified.',
 'oldIndependentCopy':item(old),'blocker':item(ART/'legacy-binding-blocker.json')},
 'regression':{'heldOldSource':'RED expected exit1, exactly1 normal unit assertion failure, no fatal','fixedCurrentLibc':'49/49 PASS zero skip including legacy null/heap transfer/getcwd, retired policy descendants/detached/TLS, attrs, existing cancellation/termination/instances',
 'legacyTermination':'Real _Exit(37) on legacy pthread terminates isolated child with37; raw nonzero record retained as expected control, no fabricated success',
 'NativeAOT':'Refreshed corrected shared Libc PASS actual physical64MiB for both default/small routes;65MiB remains65MiB',
 'commands':item(ART/'legacy-revision-commands.json')},'actualInputProof':actualProof,
 'actualFacade':'Source captures nullable binding independently of retained existing runtime owner. Old96d7 and knownbad65a lack corrected service; fresh normal capture required, no generated/resource patch.',
 'remaining':['Actual facade ordinary contracts after corrected product; then separately supervisor-allocated unchanged required JSON case.','Actual allocation/constructor/Start resource failure not induced; source cleanup and admission rollback invariants only.','Other platforms, final four-cell/new193/D10/ABI/rooted/trimmed/delivery/audit/P6 OPEN.']})
commands=json.loads((ART/'legacy-revision-commands.json').read_text());pids=[r['pid'] for r in commands]
assert all(not Path(f'/proc/{p}').exists() for p in pids)
locks=[]
for path in ['/tmp/dotcc-cpython-shared-build.lock','/tmp/dotcc-cpython-native-build.lock',str(ROOT/'cpython/build/campaign/run.lock')]:
 with open(path,'a') as f:fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB);fcntl.flock(f,fcntl.LOCK_UN)
 locks.append({'path':path,'independentAcquireAndRelease':True,'freeAtTransfer':True})
release=write('revised-serial-slot-release-to-a29.json',{'status':'DIRECT GENUINE SAFE RELEASE to A29 for fresh NORMAL corrected-source capture; B34 source only',
 'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'terminalPidsAbsent':pids,'activeOwned':[],'queuedOwned':[],'locks':locks,'commandCount':len(commands),
 'commandSeconds':sum(r['seconds'] for r in commands),'sourceReady':ready,'next':'A29 fresh immutable normal capture and RAW286jobs8, with exact revised4ca source; no d9-resource reuse.'})
print(json.dumps({'ready':ready,'release':release,'sources':pins,'ILCReferences':len(proof['ILC']['references'])}),flush=True)
