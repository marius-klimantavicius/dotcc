"""Synthetic injected malloc/realloc failures in selected musl scanf."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path
from libc_wscanf_verify import translated_wscanf_units

def verify_libc_wscanf_failure(root,tests,out,nativeaot,run,library_source=None):
 out.mkdir(parents=True,exist_ok=True);source=library_source or root/'backend/runtime/musl';provider=out/'provider';provider.mkdir()
 for component in ['strto','wscanf']:shutil.copytree(source/component,provider/component)
 shutil.copytree(root/'backend/runtime/musl/multibyte',provider/'multibyte');fixture=out/'fixture.c';shutil.copy2(tests/'libc_wscanf_allocation_failure.c',fixture)
 units=translated_wscanf_units(root,provider);checks=[]
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*units,'--language',language,'--target','portable-lp64-binary80-v1','--libc','musl','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',directory/'bin/Release/net10.0/Generated.dll']);checks.append(directory.name+': JIT allocation failures')
   if nativeaot:
    run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);run([directory/'aot/Generated']);checks.append(directory.name+': NativeAOT allocation failures')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--source-dir',type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420);(out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_wscanf_failure(root,root/'backend/tests',out,a.nativeaot,run,a.source_dir)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
