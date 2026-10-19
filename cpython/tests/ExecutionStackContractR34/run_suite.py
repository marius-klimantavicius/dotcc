"""Conditional B34 full unchanged RAW JIT required suite after ordinary+JSON green.

Reuses the unchanged ordinary RawDiscoveryR30 consumer and inventory runner.
Nonpasses stay nonpasses; durable journals survive a process abort.
"""
from pathlib import Path
from collections import Counter
import datetime,fcntl,hashlib,json,os,sys,time
ROOT=Path(__file__).resolve().parents[3];sys.path.insert(0,str(ROOT/'Scripts'))
from campaigns.process import execute
from campaigns.recipes import helper
ART=ROOT/'cpython/artifacts/owner-b-r34-20261018';WORK=ROOT/'cpython/build/owner-b-r34-20261018'
def item(p):
 p=Path(p).resolve();return {'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size}
def write(n,v):
 p=ART/n;p.write_text(json.dumps(v,indent=2)+'\n');return item(p)
for mode in ['owner','pool','legacy','bad-init']:
 assert json.loads((ART/f'actual-jit-same-owner-{mode}.json').read_text())['Status']=='passed'
assert json.loads((ART/'actual-json-initial-json-case.json').read_text())['Status']=='passed'
publication_path=ROOT/'cpython/artifacts/owner-a-r29-20261018/v20/raw-publication-handoff.json';publication=json.loads(publication_path.read_text())
release=json.loads((ROOT/'cpython/artifacts/owner-a-r29-20261018/v20/serial-slot-release-to-b34.json').read_text())
for pid in release['terminalPidsAbsent']:assert not Path(f'/proc/{pid}').exists()
for p in ['/tmp/dotcc-cpython-shared-build.lock','/tmp/dotcc-cpython-native-build.lock',str(ROOT/'cpython/build/campaign/run.lock')]:
 with open(p,'a') as f:fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB);fcntl.flock(f,fcntl.LOCK_UN)
project=Path(publication['project']['path']);assert item(project)['sha256']==publication['project']['sha256']
products=WORK/'actual-products';output=WORK/'actual-app';commands=[]
env=dict(os.environ,DOTNET_CLI_USE_MSBUILD_SERVER='0',MSBUILDDISABLENODEREUSE='1',DOTNET_PROCESSOR_COUNT='2')
def run(argv,label,timeout,check=True):
 row={}
 try:execute(['flock','-n','--close','/tmp/dotcc-cpython-shared-build.lock',*map(str,argv)],label=label,cwd=Path('/tmp'),output=ART/f'{label}.log',timeout=timeout,record=row,check=check,env=env)
 finally:commands.append(row);write('full-required-commands.json',commands)
 print(json.dumps({'label':label,'status':row['status'],'exit':row['exit_code'],'seconds':row['seconds']}),flush=True)
 return row
entry_project=ROOT/'cpython/tests/RawDiscoveryR30/RawDiscoveryR30.csproj'
run(['dotnet','build',entry_project,'-c','Release','--artifacts-path',products,f'-p:CPythonProject={project}','-p:NuGetAudit=false','-p:RestoreIgnoreFailedSources=true','-p:UseSharedCompilation=false','-nodeReuse:false','-o',output],'full-required-entry-build',120)
identity=helper(ROOT/'cpython/build/owner-a-r15-20261018/pe_identity.py');binding=helper(ROOT/'cpython/tests/Audit/actual_profile_input.py');mapper=ROOT/'cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py'
proof={'publication':item(publication_path),'scope':'Own actual full required-suite consumer; separate from canonical and prior B32 bytes'}
for name,pe,proj in [('entry',output/'RawDiscoveryR30.dll',entry_project),('facade',output/'Managed.CPython.dll',ROOT/'cpython/src/Managed.CPython/Managed.CPython.csproj'),('library',output/'TranslatedCPython.dll',project)]:
 proof[name]={'identity':identity.pe_identity(pe),'sourceBinding':binding.bind(ROOT/'cpython',pe,proj,mapper,products)}
runner=output/'upstream/run_inventory.py';assert item(runner)['sha256']==item(ROOT/'cpython/tests/Upstream/run_inventory.py')['sha256']
inventory=ROOT/'cpython/tests/Upstream/inventory.frozen.json';inv=json.loads(inventory.read_text());assert len(inv['required_cases'])==4867
proof['runner']=item(runner);proof['inventory']=item(inventory);proof['references']=[item(p) for p in output.glob('*.dll')]
write('full-required-actual-input-proof.json',proof)
report=ART/'full-required-raw-jit.json'
try:
 row=run(['dotnet',output/'RawDiscoveryR30.dll','suite',output/'cpython/stdlib',inventory,report],'full-required-raw-jit',900,check=False)
finally:
 journal=report.with_suffix('.journal.jsonl');events=[json.loads(s) for s in journal.read_text().splitlines()] if journal.exists() else []
 started={r['case'] for r in events if r['status']=='started'};completed={r['case'] for r in events if r['status']!='started'}
 counts=Counter(r['status'] for r in events if r['status']!='started')
 summary={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'required':4867,'uniqueStarted':len(started),'uniqueCompleted':len(completed),'outcomeEvents':dict(counts),
  'withoutCompletedOutcome':len(set(inv['required_cases'])-completed),'neverStarted':len(set(inv['required_cases'])-started),'fatalStartedWithoutOutcome':sorted(started-completed),
  'journal':item(journal) if journal.exists() else None,'inputProof':item(ART/'full-required-actual-input-proof.json'),'scope':'Skips/XFAIL/errors/failed/unrun are not passes; final acceptance OPEN'}
 write('full-required-journal-counts.json',summary)
 print(json.dumps(summary),flush=True)
