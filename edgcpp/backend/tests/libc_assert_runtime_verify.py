"""Standalone assertion diagnostics and fatal path, preserving subprocess logs."""
import argparse,json,resource,shutil,subprocess
from pathlib import Path

def verify_libc_assert_runtime(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 for name in ['LibcFiles.cs','LibcFilesWide.cs','LibcFilesAssert.cs','LibcAppend.cs','LibcPaths.cs','LibcProcess.cs']:
  shutil.copy2(root/'backend/runtime'/name,out/name)
 shutil.copy2(tests/'libc_assert_runtime.cs',out/'Program.cs')
 (out/'Assert.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
 run(['dotnet','build',out/'Assert.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
 for prefix,label in [(['dotnet',out/'bin/Release/net10.0/Assert.dll'],'jit')]+([([out/'aot/Assert'],'aot')] if nativeaot else []):
  if label=='aot':run(['dotnet','publish',out/'Assert.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
  run(prefix);checks.append(label+': owner/wide diagnostics, bounds and zero temporary allocation')
  for scenario in ['abort','throw']:
   def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
   r=subprocess.run(list(map(str,prefix+[scenario])),capture_output=True,text=True,timeout=60,preexec_fn=no_core)
   (out/f'{label}-{scenario}.stdout').write_text(r.stdout);(out/f'{label}-{scenario}.stderr').write_text(r.stderr)
   assert r.returncode!=0 and 'caught' not in r.stdout and 'unwound' not in r.stdout and 'Translated libc abort' in r.stderr
   if scenario=='abort':assert 'Assertion failed: counter == 7 (synthetic.c: probe: 23)' in r.stderr
   checks.append(label+': fatal '+scenario)
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];index=0
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_assert_runtime(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
