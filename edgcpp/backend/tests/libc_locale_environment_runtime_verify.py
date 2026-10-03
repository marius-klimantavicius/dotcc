"""Candidate category bridge before owner-environment source integration."""
import argparse,json,shutil,subprocess
from pathlib import Path

def verify_libc_locale_environment_runtime(root,tests,out,nativeaot,run,runtime=None):
 out.mkdir(parents=True,exist_ok=True);runtime=runtime or root/'backend/runtime';checks=[]
 for name in ['LibcLocale.cs','LibcLocaleEnvironment.cs']:shutil.copy2(runtime/name,out/name)
 shutil.copy2(tests/'libc_locale_environment_runtime.cs',out/'Program.cs')
 (out/'Locale.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 run(['dotnet','build',out/'Locale.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',out/'bin/Release/net10.0/Locale.dll']);checks.append('JIT transactional categories, base/owner/TLS/pinning/lifetime, 100k zero-allocation commits')
 if nativeaot:
  run(['dotnet','publish',out/'Locale.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);run([out/'aot/Locale']);checks.append('NativeAOT transactional categories, base/owner/TLS/pinning/lifetime, 100k zero-allocation commits')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--runtime',type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_locale_environment_runtime(root,root/'backend/tests',out,a.nativeaot,run,a.runtime.resolve() if a.runtime else None)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
