from pathlib import Path
import datetime,fcntl,hashlib,json,sys
ROOT=Path(__file__).resolve().parents[3];ART=ROOT/'cpython/artifacts/owner-b-r34-20261018'
def item(p):
 p=Path(p).resolve();return {'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size}
def write(n,v):
 p=ART/n;p.write_text(json.dumps(v,indent=2)+'\n');return item(p)
files=sorted(ART.glob('*commands.json'));records=[r for p in files for r in json.loads(p.read_text())];pids=[r['pid'] for r in records]
assert all(r['status']!='running' for r in records)
assert all(not Path(f'/proc/{pid}').exists() for pid in pids)
locks=[]
for p in ['/tmp/dotcc-cpython-shared-build.lock','/tmp/dotcc-cpython-native-build.lock',str(ROOT/'cpython/build/campaign/run.lock')]:
 with open(p,'a') as f:fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB);fcntl.flock(f,fcntl.LOCK_UN)
 locks.append({'path':p,'independentAcquireAndRelease':True,'freeAtTransfer':True})
refs=json.loads((ART/'actual-aot-initial-input-proof.json').read_text())['ILC']['references'];assert len(refs)==177
assert not any(Path(r['path']).name=='DotCC.Libc.dll' for r in refs)
paths=list((ROOT/'cpython/src/Managed.CPython').glob('*.cs'))+[ROOT/'DotCC.Libc/PthreadLib.cs',ROOT/'DotCC.Libc/PthreadAttributesLib.cs',ROOT/'DotCC.Tests/PthreadStackPolicyTests.cs',ROOT/'cpython/tests/Contracts/Program.cs']
pins=write('final-source-pins.json',[item(p) for p in paths])
summary={'status':'B34 implementation + focused actual RAW JIT/NativeAOT stack contract complete; campaign acceptance OPEN',
 'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'sourcePins':pins,
 'sharedTests':{'correctedAffectedUnits':49,'skips':0,'heldOldLegacyRegression':'RED retained','nativeAttributes':'PASS target4/host56; logical/physical difference documented','JIT_AOTBounds':'PASS bothroutes null/default/small64MiB larger65MiB preserved'},
 'actualContracts':{'JIT':{'owner':225,'pool':114,'legacy':30,'uncertainInitReservation':2},'NativeAOT':{'owner':225,'pool':114,'legacy':30,'uncertainInitReservation':2},
 'exactRequiredJSON':'PASS unchanged in focused JIT, full RAW JIT suite, focused NativeAOT; same interpreter reuse PASS',
 'aotActualReferences':177,'secondLibcAssembly':False,'finalSupport':'49 assertions +3 fault-reservation; executor physical67108864'},
 'actualJITProof':item(ART/'actual-jit-same-owner-input-proof.json'),'actualAOTProof':item(ART/'actual-aot-initial-input-proof.json'),
 'JSONJIT':item(ART/'actual-json-initial-json-case.log'),'JSONAOT':item(ART/'actual-aot-json-initial-json-case.log'),
 'fullRequired':json.loads((ART/'full-required-journal-counts.json').read_text()),'IOHandoff':item(ART/'io-failure-handoff-b35.json'),
 'retainedFailures':[item(ART/'legacy-binding-blocker.json'),item(ART/'first-actual-blocker.json')],
 'limits':['Full RAW JIT:4751 UNIQUE passes/12IO failures/101requiredskips/3requiredXFAIL;4759pass EVENTS include8 repeated IDs. No errors/fatal/unrun. Nonpasses remain blocking.',
 'Static VM retains original program context+nullable binding across Python generations; different owner/binding rejects before translated entry. Exact first/second initialization of original AV cannot be recovered from old log.',
 'No actual resource exhaustion/constructor/Start failure induced; rollback invariants/source cleanup reviewed.',
 'Processed/new193/full four-cell/rooted/trimmed/delivery/audit/downstream/P6 and other platforms remain OPEN.']}
handoff=write('handoff.json',summary)
release=write('serial-slot-release-to-b35.json',{'status':'DIRECT GENUINE SAFE RELEASE to B35 priority native/current-Libc ftruncate proof; B34 no heavy/queued work',
 'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'terminalPidsAbsent':pids,'activeOwned':[],'queuedOwned':[],
 'commandCount':len(records),'commandSeconds':sum(r['seconds'] for r in records),'locks':locks,'commandRecords':[item(p) for p in files],'handoff':handoff,
 'next':'B35 nativeRED/currentLibc ftruncate validation under supervisor assignment; A30 source-only until coordinated lane. B34 Bdocs finalization source-only, then released.'})
print(json.dumps({'handoff':handoff,'release':release,'commands':len(records),'sources':pins}),flush=True)
