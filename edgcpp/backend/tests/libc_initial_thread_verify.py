"""Initial source thread completion, join, descendant lifetime and process exit."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_initial_thread(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);fixture=out/'fixture.cpp';shutil.copy2(tests/'libc_initial_thread.cpp',fixture);checks=[]
 def expected(language,mode):
  return ('DT' if language=='c++' else '')+'M'+('J'+('XU' if language=='c++' else '')+'W'+('C' if mode==2 else '') if mode not in [0,5] else '')+'A'+('G' if language=='c++' else '')
 def child(prefix,directory,language,strict=True):
  directory.mkdir();observed=[]
  for mode in range(6):
   command=list(map(str,prefix+[str(mode)]));r=subprocess.run(command,capture_output=True,text=True,timeout=30)
   (directory/f'{mode}.command.json').write_text(json.dumps(command));(directory/f'{mode}.stdout').write_text(r.stdout);(directory/f'{mode}.stderr').write_text(r.stderr)
   observed.append({'mode':mode,'status':r.returncode,'stderr':r.stderr})
   assert r.returncode==0 and r.stdout=='',(directory,mode,r.returncode,r.stdout,r.stderr)
   if strict:assert r.stderr==expected(language,mode),(directory,mode,r.stderr,expected(language,mode))
  (directory/'observed.json').write_text(json.dumps(observed,indent=2))
 for cc,language in [('gcc','c'),('g++','c++')]:
  run([cc,'-x',language,'-std='+('c11' if language=='c' else 'c++17'),'-pthread',fixture,'-o',out/cc]);child([out/cc],out/(cc+'-oracle'),language,language=='c')
  checks.append(language+(': native six-mode exact C oracle' if language=='c' else ': native host C++ interop observations (not translated TLS policy oracle)'))
 if not translate:return checks
 for language in ['c','c++']:
  for instance in [False,True]:
   label=language.replace('+','x')+'-'+('instance' if instance else 'static');directory=out/label
   run([sys.executable,root/'dotnet.py',fixture,root/'backend/runtime/libc/threads/threads.c','--language',language,'--libc','dotcc','--target','portable-lp64-binary80-v1','--namespace','','--class-name','Core','--split','function','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   assert json.loads((directory/'dot-gen.json').read_text())['processExit']=={'mode':'process-wide','entryPoint':'normal-exit','initialThread':'logical-completion-v1'}
   run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);child(['dotnet',directory/'bin/Release/net10.0/Generated.dll'],directory/'jit-cases',language);checks.append(label+': JIT six initial/last-thread exit modes')
   if nativeaot:
    run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);child([directory/'aot/Generated'],directory/'aot-cases',language);checks.append(label+': NativeAOT six initial/last-thread exit modes')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_initial_thread(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
