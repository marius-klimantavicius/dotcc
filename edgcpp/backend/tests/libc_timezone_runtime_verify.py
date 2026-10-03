"""Synthetic TZif/BCL boundary, pinned metadata and NativeAOT verification."""
import argparse,hashlib,json,shutil,subprocess
from pathlib import Path

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',required=True,type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2]
 runtime=root/'backend/runtime';data=runtime/'tzdb';manifest=json.loads((data/'bundle.json').read_text());digest=lambda path:hashlib.sha256(path.read_bytes()).hexdigest()
 assert manifest['sourceManifestSha256']==digest(data/'source.json')
 assert manifest['bundleSha256']==digest(data/'tzdb.bin')
 for name in ['LibcTimezone.cs','LibcTimezoneData.cs']:shutil.copyfile(runtime/name,out/name)
 shutil.copyfile(root/'backend/tests/libc_timezone_runtime.cs',out/'Program.cs');shutil.copyfile(data/'tzdb.bin',out/'tzdb.bin')
 (out/'Timezone.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><EmbeddedResource Include="tzdb.bin" LogicalName="Edg.Runtime.Tzdb.bin" /></ItemGroup></Project>')
 index=0;checks=[]
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r.stdout
 try:
  run(['dotnet','build',out/'Timezone.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);checks.append('JIT '+run(['dotnet',out/'bin/Release/net10.0/Timezone.dll',out/'jit-data']).strip())
  if a.nativeaot:
   run(['dotnet','publish',out/'Timezone.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);checks.append('NativeAOT '+run([out/'aot/Timezone',out/'aot-data']).strip())
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
