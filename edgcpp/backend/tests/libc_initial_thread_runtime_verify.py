"""BCL initial-thread logical completion and actual-last-thread process callback."""
import argparse,json,shutil,subprocess
from pathlib import Path

def verify_libc_initial_thread_runtime(root,tests,out,nativeaot,run,runtime=None):
 out.mkdir(parents=True,exist_ok=True);source=runtime or root/'backend/runtime'
 for name in ['LibcxxThreads.cs','LibcC11Threads.cs','LibcInitialThread.cs']:shutil.copy2(source/name,out/name)
 shutil.copy2(tests/'libc_initial_thread_runtime.cs',out/'Program.cs')
 (out/'Test.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 run(['dotnet','build',out/'Test.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);checks=[]
 for label,prefix in [('jit',['dotnet',out/'bin/Release/net10.0/Test.dll'])]+([('aot',[out/'aot/Test'])] if nativeaot else []):
  if label=='aot':run(['dotnet','publish',out/'Test.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
  for mode,expected in enumerate(['MA','MJWA','MJWCA','MJWA']):
   command=list(map(str,prefix+[str(mode)]));r=subprocess.run(command,capture_output=True,text=True,timeout=30);(out/f'{label}-{mode}.command.json').write_text(json.dumps(command));(out/f'{label}-{mode}.stdout').write_text(r.stdout);(out/f'{label}-{mode}.stderr').write_text(r.stderr)
   assert r.returncode==0 and r.stdout==expected and r.stderr=='',(label,mode,r.returncode,r.stdout,r.stderr)
   checks.append(f'{label} mode{mode}: logical completion/result, last-thread identity and released leases ({expected})')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--runtime-dir',type=Path);a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_initial_thread_runtime(root,root/'backend/tests',out,a.nativeaot,run,a.runtime_dir)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
