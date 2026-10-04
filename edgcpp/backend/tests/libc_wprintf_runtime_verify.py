"""Wide formatter monitor/error preservation and allocation-free steady calls."""
import argparse,json,shutil,subprocess
from pathlib import Path

def verify_libc_wprintf_runtime(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True)
 for name in ['LibcFiles','LibcFileDescriptors','LibcFilesWide','LibcFilesWideFormat','LibcAppend','LibcPaths','VaList','Float80']:shutil.copy2(root/'backend/runtime'/f'{name}.cs',out/f'{name}.cs')
 shutil.copy2(tests/'libc_wprintf_runtime.cs',out/'Program.cs')
 (out/'Test.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 run(['dotnet','build',out/'Test.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',out/'bin/Release/net10.0/Test.dll']);checks=['JIT wide formatter lock/error/exception/steady allocation']
 if nativeaot:
  run(['dotnet','publish',out/'Test.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);run([out/'aot/Test']);checks.append('NativeAOT wide formatter lock/error/exception/steady allocation')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_wprintf_runtime(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
