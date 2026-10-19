from pathlib import Path
import json,sys
ROOT=Path(__file__).resolve().parents[3];sys.path.insert(0,str(ROOT/'Scripts'))
from campaigns.process import execute
ART=ROOT/'cpython/artifacts/owner-b-r34-20261018';WORK=ROOT/'cpython/build/owner-b-r34-20261018';records=[]
def run(argv,label):
 r={}
 try:execute(['flock','-n','--close','/tmp/dotcc-cpython-shared-build.lock',*map(str,argv)],label=label,cwd=ROOT,output=ART/f'{label}.log',timeout=60,record=r)
 finally:records.append(r);(ART/'final-support-commands.json').write_text(json.dumps(records,indent=2)+'\n')
 print(json.dumps({'label':label,'status':r['status'],'seconds':r['seconds']}),flush=True)
run(['dotnet','build',ROOT/'cpython/tests/Contracts/Contracts.csproj','-c','Release','--artifacts-path',WORK/'final-support-products','-p:NuGetAudit=false','-p:UseSharedCompilation=false','-nodeReuse:false'],'final-support-build')
pe=WORK/'final-support-products/bin/Contracts/release/Contracts.dll'
run(['dotnet',pe],'final-support-contracts')
run(['dotnet',pe,'--faulted-owner'],'final-support-faulted-owner')
