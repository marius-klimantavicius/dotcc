"""Native/public translated stream-management APIs and owner-addressable state."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_stdio_management(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 (out/'main.c').write_text('int stdio_management_cases(const char*,const char*,const char*,const char*);int main(int n,char**v){return n==5?stdio_management_cases(v[1],v[2],v[3],v[4]):90;}\n')
 run(['gcc','-O2','-std=c11','-fno-builtin',tests/'libc_stdio_management.c',out/'main.c','-o',out/'native']);run([out/'native',out/'native-first',out/'native-second',out/'absent/none',out/'native-diagnostic']);checks.append('native synthetic public stream-management contract')
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',tests/'libc_stdio_management.c',root/'backend/runtime/libc/stdio_management/perror.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','12000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   methods={x['symbol']:x['method'] for x in manifest['exports']}
   call=('owner.' if instance else 'Core.')+methods['stdio_management_cases']
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text('''using System;using System.IO;using System.Text;unsafe class Program{
 static byte[] C(string s)=>Encoding.UTF8.GetBytes(s+"\\0");
 static void Main(string[]args){string d=Path.GetFullPath(args[0]);Directory.CreateDirectory(d);
'''+('for(int i=0;i<2;++i){using var owner=new Core();' if instance else 'int i=0;{')+'''
 fixed(byte*a=C(Path.Combine(d,i+"-first")),b=C(Path.Combine(d,i+"-second")),c=C(Path.Combine(d,"absent/none")),e=C(Path.Combine(d,i+"-diagnostic"))){
 int status='''+call+'''((sbyte*)a,(sbyte*)b,(sbyte*)c,(sbyte*)e);if(status!=0)throw new Exception("stdio case "+status);
 }
'''+('owner' if instance else 'Core')+'''.Shutdown();}Console.WriteLine("public stdio management passed");}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll',host/'jit-data']);checks.append(directory.name+': JIT public stdio management')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host',host/'aot-data']);checks.append(directory.name+': NativeAOT public stdio management')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_stdio_management(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
