"""Pinned musl environment algorithms, pointer ownership and independent engines."""
import argparse,hashlib,json,shutil,subprocess,sys
from pathlib import Path

PREFIX=r'''
#define getenv synthetic_getenv
#define setenv synthetic_setenv
#define unsetenv synthetic_unsetenv
#define putenv synthetic_putenv
#define clearenv synthetic_clearenv
#define environ synthetic_environ
#define __environ synthetic_environ
#include <stdlib.h>
extern char **synthetic_environ;
char *__strchrnul(const char *,int);
int __putenv(char *,size_t,char *);
#define weak_alias(a,b) extern __typeof(a) b __attribute__((weak,alias(#a)))
'''
SHIM=r'''
#include <stdio.h>
static char seed[]="EDG_ENV_SYNTHETIC_SEED=first";
static char *initial[]={seed,0};
#ifdef ORIGINAL
char **synthetic_environ=initial;
#endif
char **__edg_environment_snapshot(void){return initial;}
char *__strchrnul(const char *s,int c){while(*s && *s!=c)++s;return (char*)s;}
int environment_seed(void);int environment_startup_seed(void);int environment_cases(void);unsigned long environment_digest(void);
int main(void){if(environment_seed()!=1 || environment_startup_seed()!=1){printf("initial snapshot missing\n");return 99;}int r=environment_cases();if(r){printf("case %d\n",r);return r;}unsigned long digest=environment_digest();printf("%lu\n",digest);return !digest;}
'''

def verify_libc_environment(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);fixture=out/'libc_environment.c';shutil.copyfile(tests/'libc_environment.c',fixture);source=out/'environment';shutil.copytree(root/'backend/runtime/musl/environment',source)
 manifest=json.loads((source/'source.json').read_text());units=[]
 for name,e in manifest['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==e['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==e['originalSha256'];units.append(name)
 for name,digest in manifest['adapters'].items():assert hashlib.sha256((source/name).read_bytes()).hexdigest()==digest
 original=source/'originals';(original/'unistd.h').write_text('extern char **__environ;\n')
 (out/'native-prefix.h').write_text(PREFIX);(out/'native.c').write_text(SHIM);results=[]
 for kind,path in [('original',original),('adapted',source)]:
  binary=out/f'native-{kind}'
  run(['gcc','-std=c11','-O2','-fno-builtin','-Werror=implicit-function-declaration','-I',root/'backend/runtime/libc/include','-I',path,'-include',out/'native-prefix.h',*(['-DORIGINAL'] if kind=='original' else []),fixture,*[path/n for n in units],*([path/'environment_startup.c'] if kind=='adapted' else []),out/'native.c','-o',binary])
  r=subprocess.run([str(binary)],capture_output=True,text=True,timeout=120);(out/f'native-{kind}.log').write_text(r.stdout+r.stderr);print(kind,r.stdout);r.check_returncode();results.append(int(r.stdout))
 assert results[0]==results[1] and results[0]!=0
 checks=['native original/adapted: 20000 environment mutations, raw bytes, borrowed putenv, replaced environ and duplicate names']
 if not translate:return checks
 selected={c['directory'] for c in json.loads((root/'backend/runtime/musl/provider.json').read_text())['components']}
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*([] if 'environment' in selected else [*[source/n for n in units],source/'environment_startup.c']),'--language',language,'--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','65000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports=json.loads((directory/'dot-gen.json').read_text())['exports'];host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(x['method'] for x in exports if x['symbol']==symbol)
   own='owner' if instance else 'Core'
   isolation=''
   if instance:isolation=f'''Environment.SetEnvironmentVariable(key,"second");using var other=new Core();if({call('environment_seed','other')}()!=2 || {call('environment_startup_seed','other')}()!=2 || {call('environment_seed')}()!=1 || {call('environment_table','other')}()==table)throw new Exception("owner initial snapshots");if({call('environment_set_seed','other')}()!=0 || {call('environment_seed','other')}()!=3 || {call('environment_seed')}()!=1 || Environment.GetEnvironmentVariable(key)!="second")throw new Exception("translated environment changed host or other engine");'''
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.Runtime.InteropServices;using System.Threading;unsafe class Program{{static void Main(){{
const string key="EDG_ENV_SYNTHETIC_SEED";Environment.SetEnvironmentVariable(key,"first");{('using var owner=new Core();' if instance else '')}
if({call('environment_seed')}()!=1 || {call('environment_startup_seed')}()!=1)throw new Exception("BCL snapshot before user constructor");void* table={call('environment_table')}();{isolation}
GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();if({call('environment_table')}()!=table || {call('environment_seed')}()!=1)throw new Exception("initial snapshot moved");
int result={call('environment_cases')}();if(result!=0)throw new Exception($"environment case {{result}}");ulong digest={call('environment_digest')}();if(digest!={results[0]}UL)throw new Exception($"environment digest {{digest}}");
if({call('environment_borrow')}(0)!=0)throw new Exception("putenv global");sbyte* value={call('environment_borrow_value')}();GC.Collect();if({call('environment_borrow')}(1)!=0 || value!={call('environment_borrow_value')}() || Marshal.PtrToStringUTF8((nint)value)!="two")throw new Exception("borrowed global address");
Exception? failure=null;Thread worker=new(()=>{{try{{for(int i=0;i<1000;++i)if(Marshal.PtrToStringUTF8((nint){call('environment_borrow_value')}())!="two")throw new Exception("thread shared environment");{own}.ReleaseThread();}}catch(Exception e){{failure=e;}}}});worker.Start();worker.Join();if(failure!=null)throw failure;
{own}.Shutdown();bool disposed=false;try{{{call('environment_seed')}();}}catch(ObjectDisposedException){{disposed=true;}}if(!disposed)throw new Exception("disposed environment callable");Console.WriteLine("musl environment/native mutation oracle, host/owner isolation, GC, borrowed storage and disposal passed");}}}}''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT musl environment ownership')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT musl environment ownership')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command,**kwargs):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420,**kwargs)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_environment(root,root/'backend/tests',out,a.nativeaot,run,translate=not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
