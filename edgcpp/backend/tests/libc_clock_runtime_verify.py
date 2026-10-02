"""Exact native counter decomposition and BCL clock JIT/AOT contracts."""
import argparse,json,shutil,subprocess
from pathlib import Path

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',required=True,type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2]
 shutil.copyfile(root/'backend/runtime/LibcClock.cs',out/'LibcClock.cs');shutil.copyfile(root/'backend/tests/libc_clock_runtime.cs',out/'Program.cs');shutil.copyfile(root/'backend/tests/libc_clock_native.c',out/'native.c')
 (out/'Clock.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 index=0;checks=[]
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r.stdout
 try:
  run(['gcc','-std=c11','-O2',out/'native.c','-o',out/'native']);(out/'vectors.txt').write_text(run([out/'native']))
  run(['dotnet','build',out/'Clock.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);result=run(['dotnet',out/'bin/Release/net10.0/Clock.dll',out/'vectors.txt']);checks.append('JIT '+result.strip())
  if a.nativeaot:
   run(['dotnet','publish',out/'Clock.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);result=run([out/'aot/Clock',out/'vectors.txt']);checks.append('NativeAOT '+result.strip())
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
