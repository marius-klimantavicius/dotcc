"""Pinned musl calendar arithmetic/formatting, native oracle and owner storage."""
import argparse,hashlib,json,os,shutil,subprocess,sys
from pathlib import Path

NATIVE_PREFIX=r'''
#include <stddef.h>
#include <stdlib.h>
#include <time.h>
#include <langinfo.h>
#include <locale.h>
#define newlocale calendar_native_newlocale
#define freelocale calendar_native_freelocale
#define uselocale calendar_native_uselocale
#define setlocale calendar_native_setlocale
locale_t calendar_native_newlocale(int,const char*,locale_t);
void calendar_native_freelocale(locale_t);
locale_t calendar_native_uselocale(locale_t);
char *calendar_native_setlocale(int,const char*);
struct calendar_category { const char *name; };
struct __edg_locale { struct calendar_category *cat[6]; };
#define __tm_gmtoff tm_gmtoff
#define __tm_zone tm_zone
char *__asctime_r(const struct tm*,char*);
extern struct __edg_locale calendar_native_c,calendar_native_u;
extern locale_t calendar_native_current;
extern const char __utc[];
#define weak_alias(a,b) extern __typeof(a) b __attribute__((alias(#a)))
'''
NATIVE_SHIM=r'''
#include <string.h>
#include <errno.h>
#include <stdio.h>
static struct calendar_category utf8={"C.UTF-8"};
struct __edg_locale calendar_native_c={{0,0,0,0,0,0}},calendar_native_u={{&utf8,&utf8,&utf8,&utf8,&utf8,&utf8}};
locale_t calendar_native_current=&calendar_native_c;
const char __utc[]="UTC";
locale_t calendar_native_newlocale(int mask,const char *name,locale_t base){(void)mask;(void)base;return !strcmp(name,"C")?&calendar_native_c:!strcmp(name,"C.UTF-8")?&calendar_native_u:(locale_t)0;}
void calendar_native_freelocale(locale_t l){(void)l;}
locale_t calendar_native_uselocale(locale_t l){locale_t old=calendar_native_current;if(l)calendar_native_current=l;return old;}
char *calendar_native_setlocale(int c,const char *name){(void)c;if(name)calendar_native_current=!strcmp(name,"C")?&calendar_native_c:&calendar_native_u;return calendar_native_current==&calendar_native_c?(char*)"C":(char*)"C.UTF-8";}
int __edg_locale_validate(locale_t l){if(l==&calendar_native_c || l==&calendar_native_u)return 1;errno=EINVAL;return 0;}
int __edg_locale_categories(locale_t l){if(!l)l=calendar_native_current;return l==&calendar_native_c?0:l==&calendar_native_u?63:-1;}
char *__edg_calendar_utc(void){return (char*)__utc;}
char *__edg_calendar_zone(const char *p){return p==__utc?(char*)p:(char*)"";}
#ifdef ORIGINAL
const char *__tm_to_tzname(const struct tm *t){return __edg_calendar_zone(t->tm_zone);}
#endif
int calendar_cases(void);unsigned long calendar_digest(void);
int main(void){int r=calendar_cases();if(r){printf("case %d\n",r);return r;}unsigned long digest=calendar_digest();printf("%lu\n",digest);return !digest;}
'''

def verify_libc_calendar(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);fixture=out/'libc_calendar.c';shutil.copyfile(tests/'libc_calendar.c',fixture);source=out/'calendar';shutil.copytree(root/'backend/runtime/musl/calendar',source)
 manifest=json.loads((source/'source.json').read_text());units=[]
 for name,e in manifest['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==e['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==e['originalSha256']
  units.append(name)
 for name,digest in manifest['adapters'].items():assert hashlib.sha256((source/name).read_bytes()).hexdigest()==digest
 original=source/'originals';(out/'native-prefix.h').write_text(NATIVE_PREFIX);(out/'native.c').write_text(NATIVE_SHIM)
 (original/'locale_impl.h').write_text('#define CURRENT_LOCALE calendar_native_current\n#define C_LOCALE (&calendar_native_c)\n#define LCTRANS(s,c,l) (s)\n')
 (original/'time_impl.h').write_text('int __month_to_secs(int,int);long long __year_to_secs(long long,int*);long long __tm_to_secs(const struct tm*);int __secs_to_tm(long long,struct tm*);const char *__tm_to_tzname(const struct tm*);struct tm *__gmtime_r(const time_t*,struct tm*);char *__asctime_r(const struct tm*,char*);char *__nl_langinfo_l(nl_item,locale_t);size_t __strftime_l(char*,size_t,const char*,const struct tm*,locale_t);\n')
 (original/'atomic.h').write_text('#include <stdlib.h>\n#define a_crash() abort()\n')
 # asctime_r's original private header normally provides this declaration.
 with (original/'locale_impl.h').open('a') as f:f.write('char *__nl_langinfo_l(nl_item,locale_t);\n')
 results=[]
 for kind,path in [('original',original),('adapted',source)]:
  binary=out/f'native-{kind}'
  run(['gcc','-std=c11','-O2','-fno-builtin','-Werror=implicit-function-declaration','-I',root/'backend/runtime/libc/include','-I',path,'-include',out/'native-prefix.h',*(['-DORIGINAL'] if kind=='original' else []),fixture,*[path/n for n in units],*([path/'calendar_wrappers.c'] if kind=='adapted' else []),out/'native.c','-o',binary])
  result=subprocess.run([str(binary)],capture_output=True,text=True,timeout=120);(out/f'native-{kind}.log').write_text(result.stdout+result.stderr);print(kind,result.stdout);result.check_returncode();results.append(int(result.stdout))
 assert results[0]==results[1] and results[0]!=0
 checks=['native original/adapted: all146097 Gregorian-cycle days, extended years, formatting and locale tables']
 if not translate:return checks
 packaged=any(c['directory']=='calendar' for c in json.loads((root/'backend/runtime/musl/provider.json').read_text())['components'])
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*([] if packaged else [*[source/n for n in units],source/'calendar_wrappers.c']),'--language',language,'--target','portable-lp64-binary80-v1','--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','65000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports=json.loads((directory/'dot-gen.json').read_text())['exports'];host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(x['method'] for x in exports if x['symbol']==symbol)
   own='owner' if instance else 'Core'
   isolation=''
   if instance:isolation=f'''using var other=new Core();void* otherTm={call('calendar_static_time','other')}(86400);sbyte* otherText={call('calendar_static_text','other')}(86400);if(tm==otherTm || text==otherText || ((int*)tm)[3]!=1 || ((int*)otherTm)[3]!=2)throw new Exception("static buffer owner alias");if({call('calendar_invalid_locale','other')}(locale)!=1)throw new Exception("foreign locale");{call('calendar_select_locale','other')}(1);if({call('calendar_codeset')}()!=0 || {call('calendar_codeset','other')}()!=1)throw new Exception("global locale owner alias");if({call('calendar_zone','other')}(tm,buffer)==null)throw new Exception("shared UTC metadata");'''
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.Threading;unsafe class Program{{static void Main(string[] args){{
{('using var owner=new Core();' if instance else '')}
if(args.Length!=0){{try{{{call('calendar_fatal')}();throw new Exception("abort returned");}}finally{{Console.WriteLine("calendar cleanup marker");}}}}
int result={call('calendar_cases')}();if(result!=0)throw new Exception($"calendar case {{result}}");ulong digest={call('calendar_digest')}();if(digest!={results[0]}UL)throw new Exception($"calendar native mismatch {{digest}}");
void* tm={call('calendar_static_time')}(0);sbyte* text={call('calendar_static_text')}(0);sbyte* buffer=stackalloc sbyte[16];void* locale={call('calendar_locale_create')}();if(locale==null || {call('calendar_invalid_locale')}(null)!=1 || {call('calendar_invalid_locale')}((void*)(nint)(-1))!=1)throw new Exception("locale validation");
{isolation}
GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();if({call('calendar_static_time')}(0)!=tm || {call('calendar_static_text')}(0)!=text || {call('calendar_zone')}(tm,buffer)==null || buffer[0]!=85 || buffer[1]!=84 || buffer[2]!=67 || buffer[3]!=0)throw new Exception("stable calendar pointers");
Exception? failure=null;Thread worker=new(()=>{{try{{void* previous={call('calendar_thread_locale')}(locale);if({call('calendar_codeset')}()!=1)throw new Exception("thread locale");{call('calendar_thread_locale')}(previous);{own}.ReleaseThread();}}catch(Exception e){{failure=e;}}}});worker.Start();worker.Join();if(failure!=null)throw failure;if({call('calendar_codeset')}()!=0)throw new Exception("thread locale leaked");{call('calendar_locale_free')}(locale);{own}.Shutdown();Console.WriteLine("musl Gregorian cycle/extended years, formatter/native oracle, stable owner buffers and locale TLS passed");}}}}
''')
   def fatal(command):
    command=list(map(str,command))+['fatal'];env=dict(os.environ,DOTNET_DbgEnableMiniDump='0',COMPlus_DbgEnableMiniDump='0')
    def no_core():
     import resource
     resource.setrlimit(resource.RLIMIT_CORE,(0,0))
    r=subprocess.run(command,capture_output=True,text=True,env=env,preexec_fn=no_core if os.name=='posix' else None,timeout=60)
    tag='aot' if len(command)==2 else 'jit';(host/f'fatal-{tag}.command.json').write_text(json.dumps(command,indent=2));(host/f'fatal-{tag}.log').write_text(r.stdout+r.stderr)
    assert r.returncode!=0 and 'Translated libc abort' in r.stdout+r.stderr and 'calendar cleanup marker' not in r.stdout+r.stderr
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);fatal(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT calendar/locale contracts and abnormal termination')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);fatal([host/'aot/Host']);checks.append(directory.name+': NativeAOT calendar/locale contracts and abnormal termination')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_calendar(root,root/'backend/tests',out,a.nativeaot,run,translate=not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
