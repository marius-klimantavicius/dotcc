"""musl local-time rules and TZif source oracle, ownership and translated modes."""
import argparse,hashlib,json,shutil,subprocess,sys
from pathlib import Path

PREFIX=r'''
#include <stddef.h>
#include <stdlib.h>
#include <stdint.h>
#include <time.h>
#include <limits.h>
#include <stdio.h>
#include <locale.h>
#include <langinfo.h>
#define __tm_gmtoff tm_gmtoff
#define __tm_zone tm_zone
#define NAME_MAX 255
#define PATH_MAX 4096
#define TZNAME_MAX 6
extern const char __utc[];
struct tm *__localtime_r(const time_t*,struct tm*);
char *__asctime_r(const struct tm*,char*);
void __secs_to_zone(long long,int,int*,long*,long*,const char**);
unsigned char *__map_file(const char*,size_t*);
int __munmap(void*,size_t);
void *__libc_malloc(size_t);
#define weak_alias(a,b) extern __typeof(a) b __attribute__((alias(#a)))
'''
SHIM=r'''
#include <string.h>
#include <errno.h>
const char __utc[]="UTC";
void *__libc_malloc(size_t n){return malloc(n);}
unsigned char *__map_file(const char *path,size_t *length){FILE *f=fopen(path,"rb");if(!f)return 0;if(fseek(f,0,2)){fclose(f);return 0;}long n=ftell(f);if(n<44){fclose(f);return 0;}rewind(f);unsigned char *p=(unsigned char*)malloc(n);if(!p){fclose(f);return 0;}if(fread(p,1,n,f)!=(size_t)n){free(p);fclose(f);return 0;}fclose(f);if(!p[4])p[4]='1';*length=n;return p;}
int __munmap(void *p,size_t n){(void)n;free(p);return 0;}
void __edg_timezone_enter(void){}
void __edg_timezone_exit(void){}
unsigned char *__edg_timezone_map(const char *p,size_t *n){return __map_file(p,n);}
int __edg_timezone_unmap(void *p,size_t n){return __munmap(p,n);}
char *__edg_calendar_utc(void){return (char*)__utc;}
int __edg_locale_categories(locale_t l){(void)l;return 0;}
char *asctime_r(const struct tm *t,char *p){return __asctime_r(t,p);}
#ifndef ORIGINAL
const char *__tm_to_tzname(const struct tm *t){extern char *__edg_calendar_zone(const char*);return __edg_calendar_zone(t->tm_zone);}
#endif
int timezone_cases(const char*);unsigned long timezone_digest(void);
int main(int argc,char **argv){if(argc!=2)return 99;int r=timezone_cases(argv[1]);if(r){printf("case %d\n",r);return r;}printf("%lu\n",timezone_digest());return 0;}
'''

def verify_libc_timezone(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);fixture=out/'libc_timezone.c';shutil.copyfile(tests/'libc_timezone.c',fixture);source=out/'timezone';shutil.copytree(root/'backend/runtime/musl/timezone',source)
 spec=json.loads((source/'source.json').read_text());units=list(spec['sources'])
 for name,e in spec['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==e['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==e['originalSha256']
 for name,h in spec['adapters'].items():assert hashlib.sha256((source/name).read_bytes()).hexdigest()==h
 # Two transitions and second-resolution offsets; no external timezone database
 # enters this oracle. A v2 footer supplies future POSIX rules.
 import struct
 def block(width):
  header=b'TZif2'+bytes(15)+struct.pack('>6I',0,0,0,2,2,8)
  return header+struct.pack('>2'+('q' if width==8 else 'i'),-10000,10000)+b'\0\1'+struct.pack('>iBBiBB',1234,0,0,5678,1,4)+b'STD\0DST\0'
 tzfile=out/'synthetic.tzif';tzfile.write_bytes(block(4)+block(8)+b'\nSTD0DST,M3.2.0,M11.1.0\n')
 original=source/'originals';(out/'native-prefix.h').write_text(PREFIX);(out/'native.c').write_text(SHIM)
 (original/'time_impl.h').write_text('int __month_to_secs(int,int);long long __year_to_secs(long long,int*);long long __tm_to_secs(const struct tm*);int __secs_to_tm(long long,struct tm*);const char *__tm_to_tzname(const struct tm*);\n')
 (original/'libc.h').write_text('static const struct {int secure;} libc={0};\n');(original/'lock.h').write_text('#define LOCK(x) ((void)0)\n#define UNLOCK(x) ((void)0)\n');(original/'fork_impl.h').write_text('')
 (original/'sys').mkdir();(original/'sys/mman.h').write_text('')
 cal=root/'backend/runtime/musl/calendar';support=['__year_to_secs.c','__month_to_secs.c','__tm_to_secs.c','__secs_to_tm.c','asctime_r.c','asctime.c','langinfo.c','strftime.c']
 # Calendar arithmetic/formatting is identical in both oracles and supplied
 # by the accepted musl adapter, avoiding native platform locale ABI mixing.
 values=[]
 for kind,path in [('original',original),('adapted',source)]:
  binary=out/f'native-{kind}'
  run(['gcc','-std=c11','-O2','-fno-builtin','-Werror=implicit-function-declaration','-I',root/'backend/runtime/libc/include','-I',path,'-include',out/'native-prefix.h',*(['-DORIGINAL'] if kind=='original' else []),fixture,*[path/n for n in units],*([path/'timezone_wrappers.c'] if kind=='adapted' else []),*[cal/n for n in support],out/'native.c','-o',binary])
  result=subprocess.run([str(binary),str(tzfile)],capture_output=True,text=True,timeout=120)
  (out/f'native-{kind}.log').write_text(result.stdout+result.stderr);result.check_returncode();values.append(int(result.stdout))
 assert values[0]==values[1] and values[0]!=0
 checks=['native original/adapted POSIX/TZif offsets, DST overlap/gap hints and extended years']
 if not translate:return checks
 packaged=any(c['directory']=='timezone' for c in json.loads((root/'backend/runtime/musl/provider.json').read_text())['components'])
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   command=[sys.executable,root/'dotnet.py',fixture,*([] if packaged else [*[source/n for n in units],source/'timezone_wrappers.c']),'--language',language,'--target','portable-lp64-binary80-v1','--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','65000','--output-dir',directory,*(['--instance-methods'] if instance else [])]
   run(command)
   bundle=directory/'Runtime.Tzdb.bin';expected=(root/'backend/runtime/tzdb/tzdb.bin').read_bytes();assert bundle.read_bytes()==expected
   if language=='c' and not instance:
    previous=expected+b'\nretained synthetic previous revision\n';bundle.write_bytes(previous);run(command)
    assert bundle.read_bytes()==expected
    assert any(p.read_bytes()==previous for p in (directory/'.dot-gen-history').glob('*/Runtime.Tzdb.bin'))

   exports=json.loads((directory/'dot-gen.json').read_text())['exports'];host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(x['method'] for x in exports if x['symbol']==symbol)
   isolation=''
   if instance:isolation=f'''using var other=new Core();fixed(byte* zone="XST-5:45"u8)if({call('timezone_select','other')}((sbyte*)zone)!=0)throw new Exception("other select");if({call('timezone_offset','other')}(0)!=20700 || {call('timezone_offset')}(0)!=-18000)throw new Exception("TZ owner alias");if({call('timezone_static','other')}(0)==tm)throw new Exception("tm owner alias");'''
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.Text;using System.Threading.Tasks;unsafe class Program{{static void Main(string[] args){{
{('using var owner=new Core();' if instance else '')}
byte[] path=Encoding.UTF8.GetBytes(args[0]+"\\0");fixed(byte* p=path){{int result={call('timezone_cases')}((sbyte*)p);if(result!=0)throw new Exception($"timezone case {{result}}");}}ulong digest={call('timezone_digest')}();if(digest!={values[0]}UL)throw new Exception($"native mismatch {{digest}}");
fixed(byte* zone="EST5EDT,M3.2.0,M11.1.0"u8)if({call('timezone_select')}((sbyte*)zone)!=0)throw new Exception("select");void* tm={call('timezone_static')}(0);sbyte* name={call('timezone_name')}(0);if(!{call('timezone_globals')}().Equals(1))throw new Exception("globals");
{isolation}
GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();sbyte* text=stackalloc sbyte[32];if({call('timezone_static')}(0)!=tm || {call('timezone_name')}(0)!=name || {call('timezone_format_zone')}(tm,text)!=3 || text[0]!='E')throw new Exception("stable owner data");
if({call('timezone_validate_name')}((sbyte*)123,text)!=0)throw new Exception("arbitrary name pointer");
Parallel.For(0,1000,i=>{{if({call('timezone_offset')}(i)!=-18000)throw new Exception("concurrent timezone query");}});
fixed(byte* zone="Europe/Vilnius"u8){call('timezone_select')}((sbyte*)zone);if({call('timezone_offset')}(1704067200)!=7200 || {call('timezone_offset')}(1719792000)!=10800)throw new Exception("bundled named zone");
{call('timezone_hot')}(1704067200,1000);long before=GC.GetAllocatedBytesForCurrentThread();{call('timezone_hot')}(1704067200,1000);if(GC.GetAllocatedBytesForCurrentThread()!=before)throw new Exception("hot conversion allocated");
{('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("musl timezone/native digest, DST rules, extended years, stable owners and zero-allocation hot queries passed");}}}}''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll',tzfile]);checks.append(directory.name+': JIT timezone/native contracts')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host',tzfile]);checks.append(directory.name+': NativeAOT timezone/native contracts')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r.stdout
 try:checks=verify_libc_timezone(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
