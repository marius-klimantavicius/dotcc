"""Focused standalone stdio-management lifecycle/concurrency and process-exit proof."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_stdio_management_runtime(root,tests,out,nativeaot,run,runtime=None):
 out.mkdir(parents=True,exist_ok=True);runtime=runtime or root/'backend/runtime'
 for name in ['LibcFiles','LibcFileDescriptors','LibcFilesManagement','LibcFilesWide','LibcAppend','LibcPaths']:
  source=runtime/f'{name}.cs'
  if not source.exists():source=root/'backend/runtime'/f'{name}.cs'
  shutil.copy2(source,out/f'{name}.cs')
 shutil.copy2(tests/'libc_stdio_management_runtime.cs',out/'Program.cs')
 (out/'Test.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 checks=[]
 def exercise(command,name):
  run([*command,out/(name+'-data')])
  script='import subprocess,sys,pathlib; p=pathlib.Path(sys.argv[1]); r=subprocess.run(sys.argv[2:]+[str(p),"exit"],capture_output=True,text=True); print(r.stdout+r.stderr); assert r.returncode==37,r.returncode; assert list((p/"temp").iterdir())==[]'
  run([sys.executable,'-c',script,out/(name+'-exit'),*command]);checks.append(name+': reopen identity/failure/append/gate, perror orientation, rewind, tmpnam owner storage, anonymous tmpfile plus immediate process exit')
 run(['dotnet','build',out/'Test.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);exercise(['dotnet',out/'bin/Release/net10.0/Test.dll'],'JIT')
 if nativeaot:
  run(['dotnet','publish',out/'Test.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);exercise([out/'aot/Test'],'NativeAOT')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--runtime',type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];index=0
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_stdio_management_runtime(root,root/'backend/tests',out,a.nativeaot,run,a.runtime.resolve() if a.runtime else None)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
