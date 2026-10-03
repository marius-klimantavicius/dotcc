"""C17 host-command profile: native normalized observation and translated owner isolation."""
import argparse,json,subprocess,sys
from pathlib import Path

PROFILE={'version':1,'status':'bcl-exit-code','environment':'owner-source-environ','workingDirectory':'owner-service-snapshot','standardIO':'host-os-handles','posixSignalPolicy':False}

def verify_libc_system(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 (out/'native.c').write_text('#include <stdlib.h>\n#include <sys/wait.h>\nint main(void){if(!system(0))return 1;int r=system("exit 7");if(r<0||!WIFEXITED(r)||WEXITSTATUS(r)!=7)return 2;r=system("exit 137");return r<0||!WIFEXITED(r)||WEXITSTATUS(r)!=137?3:0;}\n')
 run(['gcc','-std=c11','-O2',out/'native.c','-o',out/'native']);run([out/'native']);checks.append('native command processor and normal7/137 status observation (explicit wait-status normalization)')
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   provider=json.loads((root/'backend/runtime/musl/provider.json').read_text())
   registered=any(c.get('sourceBase')=='libc' and c['directory']=='system' for c in provider['components'])
   extra=[] if registered else [root/'backend/runtime/libc/system/system.c']
   run([sys.executable,root/'dotnet.py',tests/'libc_system.c',*extra,'--language',language,'--target','portable-lp64-binary80-v1','--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','40000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());assert manifest['hostCommand']==PROFILE,manifest.get('hostCommand');host=directory/'host';host.mkdir();methods={x['symbol']:x['method'] for x in manifest['exports']};receiver='owner' if instance else 'Core'
   def call(n):return receiver+'.'+methods[n]
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   declarations='using Core owner=new Core(),other=new Core();' if instance else ''
   isolated='''fixed(byte* key=C("EDG_SYSTEM_VALUE"),value=C("19")){if(other.'''+methods['system_environment']+'''((sbyte*)key,(sbyte*)value)!=0)throw new Exception("other environment");}fixed(byte* command=C("exit \\\"$EDG_SYSTEM_VALUE\\\"")){if(other.'''+methods['system_call']+'''((sbyte*)command)!=19)throw new Exception("other command");}other.Shutdown();''' if instance else ''
   (host/'Program.cs').write_text('''using System;using System.IO;using System.Text;unsafe class Program{
 static byte[] C(string text)=>Encoding.UTF8.GetBytes(text+"\\0");
 static void Main(string[]args){string previous=Environment.CurrentDirectory;string dir=Path.GetFullPath(args[0]);Directory.CreateDirectory(dir);Environment.CurrentDirectory=dir;
'''+declarations+'''
try{
 fixed(byte* path=C("source-output.txt")){if('''+call('system_stdout')+'''((sbyte*)path)!=0)throw new Exception("FILE anchor");}
 Environment.CurrentDirectory=previous;
 if('''+call('system_call')+'''(null)!=1)throw new Exception("processor query");
 fixed(byte* key=C("EDG_SYSTEM_VALUE"),value=C("7")){if('''+call('system_environment')+'''((sbyte*)key,(sbyte*)value)!=0)throw new Exception("owner environment");}
'''+isolated+'''
 fixed(byte* command=C("printf '%s' \\\"$EDG_SYSTEM_VALUE\\\" > command-result.txt; exit \\\"$EDG_SYSTEM_VALUE\\\"")){if('''+call('system_call')+'''((sbyte*)command)!=7)throw new Exception("source environment/status");}
 if(File.ReadAllText(Path.Combine(dir,"command-result.txt"))!="7")throw new Exception("FILE and command owner working directory differ");
 fixed(byte* command=C("printf 'EDG_GENERATED_HOST_STDOUT\\\\n'")){if('''+call('system_call')+'''((sbyte*)command)!=0)throw new Exception("host stdout");}
 if(File.ReadAllBytes(Path.Combine(dir,"source-output.txt")).Length!=0)throw new Exception("host stdio profile changed");
 sbyte* invalid=stackalloc sbyte[2];invalid[0]=-1;invalid[1]=0;if('''+call('system_call')+'''(invalid)!=-1||'''+call('system_error')+'''()!=84)throw new Exception("strict UTF8 errno");
}finally{'''+receiver+'''.Shutdown();Environment.CurrentDirectory=previous;}Console.WriteLine("generated system C17 host-command profile passed");}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);result=run(['dotnet',host/'bin/Release/net10.0/Host.dll',host/'jit-data']);assert 'EDG_GENERATED_HOST_STDOUT' in result.stdout;checks.append(directory.name+': JIT C17 system/env/owner cwd/host stdio')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);result=run([host/'aot/Host',host/'aot-data']);assert 'EDG_GENERATED_HOST_STDOUT' in result.stdout;checks.append(directory.name+': NativeAOT C17 system/env/owner cwd/host stdio')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_system(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
