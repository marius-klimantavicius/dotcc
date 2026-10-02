"""Pinned integer helpers and smoothsort: original/native oracle, callbacks and owner isolation."""
import argparse,hashlib,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_stdlib_algorithms(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);source=out/'sources';shutil.copytree(root/'backend/runtime/musl/stdlib_algorithms',source);fixture=out/'fixture.c';shutil.copy2(tests/'libc_stdlib_algorithms.c',fixture)
 manifest=json.loads((source/'source.json').read_text());units=[]
 for name,item in manifest['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==item['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==item['originalSha256']
  if name.endswith('.c'):units.append(name)
 original=out/'original';shutil.copytree(source/'originals',original);shutil.copy2(source/'atomic.h',original/'atomic.h');shutil.copy2(source/'algorithms_compat.h',original/'algorithms_compat.h')
 (out/'native-glue.h').write_text('#include "algorithms_compat.h"\n#define weak_alias(old,new) extern __typeof(old) new __attribute__((alias(#old)));\n')
 (out/'native.c').write_text('#include <stdio.h>\nunsigned long algorithms_digest(void);int algorithms_cases(void);int algorithms_exception(void);int main(void){int r=algorithms_cases();if(r){fprintf(stderr,"algorithm case %d\\n",r);return r;}if(algorithms_exception())return 90;printf("%lu\\n",algorithms_digest());return 0;}\n')
 golden=[]
 for kind,directory in [('original',original),('adapted',source)]:
  binary=out/f'native-{kind}'
  run(['gcc','-D_GNU_SOURCE','-std=c11','-O2','-fno-builtin','-fno-inline','-I',directory,'-include',out/'native-glue.h',fixture,*[directory/n for n in units],out/'native.c','-o',binary])
  result=subprocess.run([str(binary)],capture_output=True,text=True,timeout=180);(out/f'native-{kind}.log').write_text(result.stdout+result.stderr);result.check_returncode();golden.append(int(result.stdout))
 assert golden[0]==golden[1] and golden[0]!=0
 checks=['native original/adapted integer boundaries, smoothsort patterns/record widths, reentrant callbacks and bsearch agreed']
 if not translate:return checks
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*[source/n for n in units],'--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','40000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports=json.loads((directory/'dot-gen.json').read_text())['exports'];host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(item['method'] for item in exports if item['symbol']==symbol)
   other=f'using var other=new Core();{call("algorithms_owner","other")}(29);if({call("algorithms_cases","other")}()!=0||{call("algorithms_tag","other")}()!=29||{call("algorithms_tag")}()!=17)throw new Exception("owner callback isolation");' if instance else ''
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;unsafe class Program{{static void Main(){{
{('using var owner=new Core();' if instance else '')}
{call('algorithms_owner')}(17);int result={call('algorithms_cases')}();if(result!=0)throw new Exception($"algorithm case {{result}}");ulong digest={call('algorithms_digest')}();if(digest!={golden[0]}UL)throw new Exception($"native mismatch {{digest}}");
{other}
if({call('algorithms_exception')}()!=0)throw new Exception("exception through sort callback");
{('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("integer utilities, allocation-free smoothsort, context/reentrant/owner callbacks, binary search and C++ callback unwinding passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT algorithms and callback contracts')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT algorithms and callback contracts')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_stdlib_algorithms(root,root/'backend/tests',out,a.nativeaot,run,translate=not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
