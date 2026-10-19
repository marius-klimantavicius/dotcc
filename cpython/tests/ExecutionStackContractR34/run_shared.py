"""B34 current-Libc ordinary pthread/native ABI/AOT contracts, no interpreter."""
from pathlib import Path
import argparse,json,sys,time
ROOT=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'Scripts'))
from campaigns.process import execute
parser=argparse.ArgumentParser();parser.add_argument('--aot',action='store_true');args=parser.parse_args()
ART=ROOT/'cpython/artifacts/owner-b-r34-20261018';WORK=ROOT/'cpython/build/owner-b-r34-20261018'
phase='shared-aot' if args.aot else 'shared-expanded'; records=[]; start=time.monotonic()
def run(argv,label,timeout=90,native=False):
    record={};remaining=300-(time.monotonic()-start);assert remaining>0
    try:
        execute(['flock','-n','--close',f'/tmp/dotcc-cpython-{"native-build" if native else "shared-build"}.lock',*map(str,argv)],label=label,cwd=ROOT,
                output=ART/f'{label}.log',timeout=min(timeout,remaining),record=record)
    finally:
        records.append(record);(ART/f'{phase}-commands.json').write_text(json.dumps(records,indent=2)+'\n')
    print(json.dumps({'label':label,'status':record['status'],'seconds':record['seconds']}),flush=True)
common=['-c','Release','--artifacts-path',WORK/'shared-products','-p:NuGetAudit=false','-p:RestoreIgnoreFailedSources=true']
here=ROOT/'cpython/tests/ExecutionStackContractR34'
if not args.aot:
    run(['dotnet','test',here/'SharedPolicyContracts.csproj',*common,'--logger','trx;LogFileName=expanded.trx','--results-directory',ART/'expanded-unit-results'],'current-libc-expanded',120)
    run(['gcc','-std=c11','-DTARGET_ABI_ONLY','-I',ROOT/'DotCC.Lib/include','-c',here/'native_attributes.c','-o',WORK/'target-attributes.o'],'native-target-attribute-abi',native=True)
    run(['gcc','-std=c11','-Wall','-Wextra','-Werror','-pthread',here/'native_attributes.c','-o',WORK/'native-attributes'],'native-host-attributes-build',native=True)
    run([WORK/'native-attributes'],'native-host-attributes',30,native=True)
    run(['dotnet','build',here/'SharedPolicyProbe.csproj',*common],'current-libc-probe-build')
    run(['dotnet',WORK/'shared-products/bin/SharedPolicyProbe/release/SharedPolicyProbe.dll'],'current-libc-probe-jit',30)
else:
    run(['dotnet','publish',here/'SharedPolicyProbe.csproj',*common,'-r','linux-x64','-p:PublishAot=true','-o',WORK/'shared-aot'],'current-libc-aot-publish',240)
    run([WORK/'shared-aot/SharedPolicyProbe'],'current-libc-probe-aot',30)
print(json.dumps({'phase':phase,'seconds':time.monotonic()-start,'status':'passed'}),flush=True)
