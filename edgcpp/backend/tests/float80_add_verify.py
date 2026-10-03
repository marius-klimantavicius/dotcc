"""Allocation-free binary80 addition, exact integer and native x87 oracles."""
import argparse,json,shutil,subprocess
from pathlib import Path

def verify_float80_add(root,tests,out,nativeaot,run,runtime=None):
 out.mkdir(parents=True,exist_ok=True)
 shutil.copy2(runtime or root/'backend/runtime/Float80.cs',out/'Float80.cs')
 shutil.copy2(tests/'float80_add_runtime.cs',out/'Program.cs')
 shutil.copy2(tests/'float80_add_native.c',out/'native.c')
 (out/'Add.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
 checks=[]
 run(['gcc','-std=c11','-O2','-fno-fast-math',out/'native.c','-lm','-o',out/'native'])
 oracle=run([out/'native']);vectors=oracle.stdout if hasattr(oracle,'stdout') else oracle
 (out/'vectors.txt').write_text(vectors)
 run(['dotnet','build',out/'Add.csproj','-c','Release','--nologo'])
 actual=run(['dotnet',out/'bin/Release/net10.0/Add.dll',out/'vectors.txt']);result=actual.stdout if hasattr(actual,'stdout') else actual
 checks.append({'name':'binary80 exact addition JIT','result':result.strip()})
 if nativeaot:
  run(['dotnet','publish',out/'Add.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
  actual=run([out/'aot/Add',out/'vectors.txt']);aot=actual.stdout if hasattr(actual,'stdout') else actual
  assert aot==result
  checks.append({'name':'binary80 exact addition NativeAOT','result':aot.strip()})
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',required=True,type=Path);p.add_argument('--nativeaot',action='store_true');p.add_argument('--runtime',type=Path);a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];index=0;checks=[]
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2))
  result=subprocess.run(command,capture_output=True,text=True,timeout=300)
  (out/f'{i:03}.stdout').write_text(result.stdout);(out/f'{i:03}.stderr').write_text(result.stderr)
  if result.returncode:print(result.stdout+result.stderr)
  result.check_returncode();return result
 try:checks.extend(verify_float80_add(root,root/'backend/tests',out,a.nativeaot,run,a.runtime))
 finally:(out/'results.json').write_text(json.dumps({'checks':checks,'scope':'binary80 addition; other arithmetic, floating environment and non-Linux platforms not covered'},indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
