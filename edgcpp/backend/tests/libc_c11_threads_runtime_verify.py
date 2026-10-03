"""Standalone C11 owner-specific thread service and accepted C++ regressions."""
import argparse,json,shutil,subprocess
from pathlib import Path

def verify_libc_c11_threads_runtime(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 for fixture in ['libc_c11_threads_runtime.cs','libcxx_threads_runtime.cs']:
  directory=out/Path(fixture).stem;directory.mkdir()
  for name in ['LibcxxThreads.cs','LibcC11Threads.cs','LibcInitialThread.cs']:shutil.copy2(root/'backend/runtime'/name,directory/name)
  shutil.copy2(tests/fixture,directory/'Program.cs')
  (directory/'Threads.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
  run(['dotnet','build',directory/'Threads.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
  for prefix,label in [(['dotnet',directory/'bin/Release/net10.0/Threads.dll'],'jit')]+([([directory/'aot/Threads'],'aot')] if nativeaot else []):
   if label=='aot':run(['dotnet','publish',directory/'Threads.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
   run(prefix);checks.append(fixture+': '+label+' owner/service contract')
   if fixture=='libcxx_threads_runtime.cs':
    r=subprocess.run(list(map(str,prefix+['fatal'])),capture_output=True,text=True,timeout=60);(directory/(label+'-fatal.stdout')).write_text(r.stdout);(directory/(label+'-fatal.stderr')).write_text(r.stderr)
    assert r.returncode==73 and r.stdout=='body\nterminate\n',(r.returncode,r.stdout,r.stderr);checks.append(fixture+': '+label+' first-pass termination')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];index=0
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_c11_threads_runtime(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
