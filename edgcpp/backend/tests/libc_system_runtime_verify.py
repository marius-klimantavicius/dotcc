"""BCL host-command profile standalone and NativeAOT acceptance."""
import argparse,json,shutil,subprocess
from pathlib import Path

def verify_libc_system_runtime(root,tests,out,nativeaot,run,runtime=None):
 out.mkdir(parents=True,exist_ok=True);runtime=runtime or root/'backend/runtime'
 for name in ['LibcCommand','LibcFiles','LibcFilesManagement','LibcFilesWide','LibcAppend','LibcPaths']:
  source=runtime/f'{name}.cs'
  if not source.exists():source=root/'backend/runtime'/f'{name}.cs'
  shutil.copy2(source,out/f'{name}.cs')
 shutil.copy2(tests/'libc_system_runtime.cs',out/'Program.cs')
 (out/'Test.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 checks=[]
 def execute(command,name):
  result=run([*command,out/(name+'-data')]);assert 'EDG_HOST_STDOUT' in result.stdout;checks.append(name+': system processor query, raw status, owner environment/cwd, UTF8, errors, explicit host stdio inheritance')
 run(['dotnet','build',out/'Test.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);execute(['dotnet',out/'bin/Release/net10.0/Test.dll'],'JIT')
 if nativeaot:
  run(['dotnet','publish',out/'Test.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);execute([out/'aot/Test'],'NativeAOT')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--runtime',type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_system_runtime(root,root/'backend/tests',out,a.nativeaot,run,a.runtime.resolve() if a.runtime else None)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
