"""Branch prediction hint preserves values and single argument evaluation."""
import argparse,json,os,subprocess,sys
from pathlib import Path

def verify_branch_expect(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[];source=tests/'branch_expect.c'
 for language in ['c','c++']:
  cfile=out/(language.replace('+','x')+'-oracle.c');native=out/(language.replace('+','x')+'-oracle')
  run([root/'build/bin/cpfe','--c11' if language=='c' else '--c++17','--gnu_version','130300','--gen_c_file_name',cfile,'-o',out/(language.replace('+','x')+'.il'),source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
  run(['gcc','-std=gnu11','-O2',cfile,root/'build/lib/libC.a','-lm','-o',native]);run([native]);checks.append(language+': original EDG C hint/evaluation oracle')
  for instance in [False,True]:
   directory=out/(language.replace('+','x')+('-instance' if instance else '-static'))
   run([sys.executable,root/'dotnet.py',source,'--language',language,'--output-dir',directory,*(['--instance-methods'] if instance else [])])
   run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',directory/'bin/Release/net10.0/Generated.dll']);checks.append(directory.name+': JIT value, side effects and volatile operand')
   if nativeaot:
    run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);run([directory/'aot/Generated']);checks.append(directory.name+': NativeAOT hint semantics')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command,**kwargs):
  global index
  command=list(map(str,command));i=index;index+=1;(out/f'{i:03}.command.json').write_text(json.dumps(command));r=subprocess.run(command,capture_output=True,text=True,timeout=300,**kwargs);(out/f'{i:03}.stdout').write_text(r.stdout);(out/f'{i:03}.stderr').write_text(r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_branch_expect(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
