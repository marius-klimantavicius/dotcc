#!/usr/bin/env python3
"""C linkage with namespace-scoped POSIX record declarations, as used by EDG."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', type=Path, required=True)
parser.add_argument('--output-dir', type=Path, required=True)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
work = out/'work';work.mkdir();(work/'nested-header-entry').write_text('namespaced C linkage\n')
commands=[];checks=[]
def run(label,cmd,cwd=None):
    cmd=list(map(str,cmd));result=subprocess.run(cmd,cwd=cwd,capture_output=True,text=True,timeout=600)
    (out/(label+'.stdout')).write_text(result.stdout);(out/(label+'.stderr')).write_text(result.stderr)
    commands.append({'label':label,'command':cmd,'exitCode':result.returncode})
    (out/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
    if result.returncode: raise RuntimeError(f'{label} failed; logs in {out}')
source=Path(__file__).with_name('posix_nested_header.cpp')
run('native-build',['g++','-std=c++17','-O2',source,'-o',out/'native'])
run('native-run',[out/'native'],work)
for instance in (False,True):
    mode='instance' if instance else 'static';generated=out/mode
    run(mode+'-translate',[sys.executable,ROOT/'dotnet.py',source,'--compiler',args.compiler.resolve(),
        '--std','c++17','--emit','exe','--target','portable-lp64-binary80-v2','--libc','dotcc','--posix',
        '--unbound-externals','reject','--namespace','Posix.NestedHeader','--class-name','Core',
        '--output-dir',generated,*(['--instance-methods'] if instance else [])])
    manifest=json.loads((generated/'dot-gen.json').read_text());assert not manifest['unboundExternals']['symbols']
    run(mode+'-build',['dotnet','build',generated/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
    run(mode+'-jit',['dotnet',generated/'bin/Release/net10.0/Generated.dll'],work)
    checks.append(mode+'/JIT: C linkage and namespaced directory records resolve to POSIX, listing agrees with native')
    if args.nativeaot:
        run(mode+'-publish',['dotnet','publish',generated/'Generated.csproj','-c','Release','-r','linux-x64',
            '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-p:TreatWarningsAsErrors=true','-o',generated/'aot'])
        run(mode+'-aot',[generated/'aot/Generated'],work)
        checks.append(mode+'/NativeAOT: same directory listing')
results={'status':'passed','checks':checks};(out/'results.json').write_text(json.dumps(results,indent=2)+'\n');print(json.dumps(results,indent=2))
