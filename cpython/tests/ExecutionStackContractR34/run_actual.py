"""B34 actual corrected product contracts. Requires direct allocated lane release.

The optional json phase is a separate supervisor-coordinated batch, never part
of ordinary contracts or an automatic diagnostic. No test selection is waived.
"""
from pathlib import Path
import argparse,fcntl,hashlib,json,sys,time
ROOT=Path(__file__).resolve().parents[3];sys.path.insert(0,str(ROOT/'Scripts'))
from campaigns.process import execute
from campaigns.recipes import helper
parser=argparse.ArgumentParser();parser.add_argument('--publication',required=True,type=Path);parser.add_argument('--release',required=True,type=Path)
parser.add_argument('--phase',choices=['jit','aot','json','aot-json'],default='jit');parser.add_argument('--attempt',default='initial');args=parser.parse_args()
ART=ROOT/'cpython/artifacts/owner-b-r34-20261018';WORK=ROOT/'cpython/build/owner-b-r34-20261018';HERE=ROOT/'cpython/tests/ExecutionStackContractR34'
def item(p):
 p=Path(p).resolve();return {'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size}
def write(n,v):
 p=ART/n;p.write_text(json.dumps(v,indent=2)+'\n');return item(p)
publication=json.loads(args.publication.read_text());release=json.loads(args.release.read_text())
assert 'KNOWN BAD' not in publication.get('status','').upper(),publication.get('status')
for pid in release['terminalPidsAbsent']:assert not Path(f'/proc/{pid}').exists(),pid
for p in ['/tmp/dotcc-cpython-shared-build.lock','/tmp/dotcc-cpython-native-build.lock',str(ROOT/'cpython/build/campaign/run.lock')]:
 with open(p,'a') as f:fcntl.flock(f,fcntl.LOCK_EX|fcntl.LOCK_NB);fcntl.flock(f,fcntl.LOCK_UN)
receipt_ref=publication['receipt'];assert item(receipt_ref['path'])['sha256']==receipt_ref['sha256']
receipt=json.loads(Path(receipt_ref['path']).read_text());assert receipt['status']=='passed' and len(receipt['units'])==286
project=Path(publication['project']['path']);assert item(project)['sha256']==publication['project']['sha256']
source=project.parent/'CPython.cs';text=source.read_text()
for relative in ['DotCC.Libc/PthreadLib.cs','DotCC.Libc/PthreadAttributesLib.cs']:
 p=ROOT/relative;marker='// ---- '+p.name+' ----\n#nullable enable\n';begin=text.index(marker)+len(marker);end=text.index('#nullable restore',begin)
 expected=''.join(line+'\n' for line in p.read_text().split('\n') if not line.lstrip().startswith(('#nullable','namespace ')) and not line.startswith('using '))+'\n'
 assert text[begin:end]==expected,relative
assert 'contextBinding = state.Context?.Enter() ?? default;' in text
products=WORK/'actual-products';output=WORK/('actual-aot' if args.phase in ('aot','aot-json') else 'actual-app')
label_prefix=f'actual-{args.phase}-{args.attempt}'
commands=[];start=time.monotonic()
def run(argv,label,timeout=90):
 row={};remaining=300-(time.monotonic()-start);assert remaining>0
 try:
  execute(['flock','-n','--close','/tmp/dotcc-cpython-shared-build.lock',*map(str,argv)],label=label,cwd=Path('/tmp'),output=ART/f'{label}.log',timeout=min(timeout,remaining),record=row,
          env=dict(__import__('os').environ,DOTNET_CLI_USE_MSBUILD_SERVER='0',MSBUILDDISABLENODEREUSE='1',DOTNET_PROCESSOR_COUNT='2'))
 finally:
  commands.append(row);write(f'{label_prefix}-commands.json',commands)
 print(json.dumps({'label':label,'status':row['status'],'seconds':row['seconds']}),flush=True)
common=['-c','Release','--artifacts-path',products,f'-p:CPythonProject={project}','-p:NuGetAudit=false','-p:RestoreIgnoreFailedSources=true','-p:UseSharedCompilation=false','-nodeReuse:false']
if args.phase not in ('json','aot-json'):
 verb='publish' if args.phase=='aot' else 'build'
 extra=['-r','linux-x64','-p:PublishAot=true'] if args.phase=='aot' else []
 run(['dotnet',verb,HERE/'ExecutionStackContractR34.csproj',*common,*extra,'-o',output],f'{label_prefix}-build',180 if args.phase=='jit' else 260)
identity=helper(ROOT/'cpython/build/owner-a-r15-20261018/pe_identity.py');binding=helper(ROOT/'cpython/tests/Audit/actual_profile_input.py');mapper=ROOT/'cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py'
entry=(products/'obj/ExecutionStackContractR34/release_linux-x64/ExecutionStackContractR34.dll') if args.phase in ('aot','aot-json') else output/'ExecutionStackContractR34.dll'
pepaths={'entry':(entry,HERE/'ExecutionStackContractR34.csproj'),'facade':(output/'Managed.CPython.dll',ROOT/'cpython/src/Managed.CPython/Managed.CPython.csproj'),'library':(output/'TranslatedCPython.dll',project)}
if args.phase in ('aot','aot-json'):
 # Native publish retains actual input assemblies in its ordinary build output.
 pebase=products/'bin/ExecutionStackContractR34/release_linux-x64'
 pepaths['facade']=(pebase/'Managed.CPython.dll',ROOT/'cpython/src/Managed.CPython/Managed.CPython.csproj')
 pepaths['library']=(pebase/'TranslatedCPython.dll',project)
proof={'authority':{'publication':item(args.publication),'release':item(args.release)},'phase':args.phase,'scope':'Actual compiled identities and sources; execution claims only from this batch logs/reports'}
for name,(pe,proj) in pepaths.items():proof[name]={'identity':identity.pe_identity(pe),'sourceBinding':binding.bind(ROOT/'cpython',pe,proj,mapper,products)}
if args.phase in ('aot','aot-json'):
 rsp=products/'obj/ExecutionStackContractR34/release_linux-x64/native/ExecutionStackContractR34.ilc.rsp'
 proof['ILC']={'rsp':item(rsp),'references':[item(line[3:].strip('"')) for line in rsp.read_text().splitlines() if line.startswith('-r:')],
  'objects':[item(p) for p in rsp.parent.glob('*.o')],'executedBinary':item(output/'ExecutionStackContractR34')}
write(f'{label_prefix}-input-proof.json',proof)
stdlib=output/'cpython/stdlib';assert (stdlib/'encodings/__init__.py').exists()
if args.phase in ('json','aot-json'):
 # Prepared only; caller must choose this separate phase under supervisor allocation.
 modes=['json-case']
else:modes=['owner','pool','legacy','bad-init']
for mode in modes:
 command=[output/'ExecutionStackContractR34'] if args.phase in ('aot','aot-json') else ['dotnet',entry]
 report=ART/f'{label_prefix}-{mode}.json';work=WORK/f'{label_prefix}-{mode}-work'
 run([*command,mode,stdlib,work,report],f'{label_prefix}-{mode}',90)
 assert json.loads(report.read_text())['Status']=='passed'
write(f'{label_prefix}-summary.json',{'status':'passed','phase':args.phase,'commands':commands,'inputProof':item(ART/f'{label_prefix}-input-proof.json'),
 'remaining':'Final both forms/JIT/AOT/rooted/new193/D10/ABI/required suite/P6 OPEN; json case only executed if separate json phase selected.'})
