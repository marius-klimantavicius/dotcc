"""Selected musl restartable conversion, scalar oracle and owner/thread TLS."""
import argparse,json,hashlib,shutil,subprocess,sys
from pathlib import Path

def verify_libc_multibyte(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);fixture=out/'libc_multibyte.c';shutil.copyfile(tests/'libc_multibyte.c',fixture);source=out/'multibyte';shutil.copytree(root/'backend/runtime/musl/multibyte',source)
 manifest=json.loads((source/'source.json').read_text());units=[]
 for name,entry in manifest['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==entry['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==entry['originalSha256']
  if name.endswith('.c'):units.append(name)
 original=source/'originals';(original/'locale_impl.h').write_text('#include <stdlib.h>\n')
 (out/'native.c').write_text('#include <stdio.h>\nunsigned long multibyte_digest(void);int multibyte_cases(void);int main(void){int r=multibyte_cases();if(r){fprintf(stderr,"multibyte case %d\\n",r);return r;}printf("%lu\\n",multibyte_digest());return 0;}\n')
 results=[]
 for kind,path in [('original',original),('adapted',source)]:
  binary=out/f'native-{kind}'
  run(['gcc','-D_GNU_SOURCE','-Dhidden=','-std=c11','-O2','-fno-builtin','-I',path,fixture,*[path/n for n in units],out/'native.c','-o',binary])
  result=subprocess.run([str(binary)],capture_output=True,text=True,timeout=180);(out/f'native-{kind}.log').write_text(result.stdout+result.stderr);result.check_returncode();results.append(int(result.stdout))
 assert results[0]==results[1] and results[0]!=0
 checks=['native original/adapted: every Unicode scalar UTF8 split/resume and UTF32, selected UTF16, errors/string bounds/C byte escapes']
 if not translate:return checks
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*[source/n for n in units],'--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','65000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest_out=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(x['method'] for x in manifest_out['exports'] if x['symbol']==symbol)
   own='owner' if instance else 'Core'
   owner_checks=f'using var other=new Core();if({call("multibyte_activate","other")}()!=1)throw new Exception("other locale");for(int i=0;i<6;++i)if({call("multibyte_hidden","other")}(i,1)!=ulong.MaxValue)throw new Exception("hidden state owner alias");' if instance else ''
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.Threading;unsafe class Program{{static void Main(){{
{('using var owner=new Core();' if instance else '')}
int result={call('multibyte_cases')}();if(result!=0)throw new Exception($"multibyte case {{result}}");ulong actual={call('multibyte_digest')}();if(actual!={results[0]}UL)throw new Exception($"multibyte native mismatch {{actual}}");
ulong[] start={{ulong.MaxValue-1,ulong.MaxValue-1,ulong.MaxValue-1,ulong.MaxValue-1,0,0}},finish={{2,2,2,2,4,1}};
for(int i=0;i<6;++i)if({call('multibyte_hidden')}(i,0)!=start[i])throw new Exception($"hidden start {{i}}");
{owner_checks}
Exception? failure=null;Thread worker=new(()=>{{try{{for(int i=0;i<6;++i){{if({call('multibyte_hidden')}(i,1)!=ulong.MaxValue)throw new Exception("hidden state thread alias");if({call('multibyte_hidden')}(i,0)!=start[i] || {call('multibyte_hidden')}(i,1)!=finish[i])throw new Exception("worker restart");}}{own}.ReleaseThread();}}catch(Exception e){{failure=e;}}}});worker.Start();worker.Join();if(failure!=null)throw failure;
for(int i=0;i<6;++i)if({call('multibyte_hidden')}(i,1)!=finish[i])throw new Exception($"main resume {{i}}");
for(int i=0;i<6;++i)if({call('multibyte_hidden')}(i,0)!=start[i])throw new Exception("release setup");{own}.ReleaseThread();
for(int i=0;i<6;++i)if({call('multibyte_hidden')}(i,1)!=ulong.MaxValue)throw new Exception("hidden state not released");
{own}.Shutdown();Console.WriteLine("all Unicode scalar conversion/resumption, string bounds/errors and per-API owner/thread TLS passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT multibyte scalar/TLS contracts')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT multibyte scalar/TLS contracts')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_multibyte(root,root/'backend/tests',out,a.nativeaot,run,translate=not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
