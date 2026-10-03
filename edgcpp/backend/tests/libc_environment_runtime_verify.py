"""Standalone initial environment snapshots and NativeAOT-compatible ownership."""
import argparse,json,shutil,subprocess
from pathlib import Path

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',required=True,type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2]
 shutil.copyfile(root/'backend/runtime/LibcEnvironment.cs',out/'LibcEnvironment.cs');shutil.copyfile(root/'backend/tests/libc_environment_runtime.cs',out/'Program.cs')
 (out/'Environment.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 index=0;checks=[]
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r.stdout
 try:
  run(['dotnet','build',out/'Environment.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);checks.append('JIT '+run(['dotnet',out/'bin/Release/net10.0/Environment.dll']).strip())
  if a.nativeaot:
   run(['dotnet','publish',out/'Environment.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);checks.append('NativeAOT '+run([out/'aot/Environment']).strip())
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
