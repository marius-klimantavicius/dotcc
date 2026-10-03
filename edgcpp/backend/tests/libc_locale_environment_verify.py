"""Owner environment locale precedence and atomic category updates."""
import argparse,hashlib,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_locale_environment(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[];component=root/'backend/runtime/libc/locale_environment';native=out/'native';native.mkdir()
 manifest=json.loads((component/'source.json').read_text());original=component/'reference/locale_map.c';assert hashlib.sha256(original.read_bytes()).hexdigest()==manifest['references']['locale_map.c'];shutil.copy2(original,native/'locale_map.c');shutil.copy2(tests/'libc_locale_environment_musl.c',native/'oracle.c')
 (native/'locale_impl.h').write_text('''#include <stddef.h>
#define LOCALE_NAME_MAX 23
struct __locale_map {const void *map;size_t map_size;char name[24];const struct __locale_map *next;};
extern const struct __locale_map __c_dot_utf8;
const char *__mo_lookup(const void*,size_t,const char*);
const void *__map_file(const char*,size_t*);
char *__strchrnul(const char*,int);
void *__libc_malloc(size_t);
int __munmap(void*,size_t);
const struct __locale_map *__get_locale(int,const char*);
''')
 (native/'libc.h').write_text('extern struct test_libc {int secure;} libc;\n');(native/'lock.h').write_text('');(native/'fork_impl.h').write_text('')
 run(['gcc','-std=c11','-O2','-fsanitize=undefined','-fno-sanitize-recover=undefined','-I',native,native/'locale_map.c',native/'oracle.c','-o',native/'musl']);run([native/'musl']);checks.append('original musl locale_map:125 environment precedence/default LC_CTYPE cases under UBSan')
 (native/'host.c').write_text('''#define _GNU_SOURCE
#include <locale.h>
#include <langinfo.h>
#include <stdlib.h>
#include <string.h>
int main(void){setenv("LC_ALL","C",1);setenv("LC_CTYPE","C.UTF-8",1);setenv("LANG","C.UTF-8",1);
if(!setlocale(LC_ALL,"")||strcmp(setlocale(LC_CTYPE,0),"C"))return 1;
unsetenv("LC_ALL");if(!setlocale(LC_CTYPE,"")||strcmp(nl_langinfo(CODESET),"UTF-8"))return 2;
locale_t value=newlocale(LC_CTYPE_MASK,"",0);if(!value||strcmp(nl_langinfo_l(CODESET,value),"UTF-8"))return 3;freelocale(value);
setenv("LC_CTYPE","C",1);setenv("LC_NUMERIC","invalid-owner-locale",1);if(setlocale(LC_ALL,"")||strcmp(nl_langinfo(CODESET),"UTF-8"))return 4;return 0;}
''')
 run(['gcc','-std=c11','-O2',native/'host.c','-o',native/'host']);run([native/'host']);checks.append('native host: LC_ALL/category precedence, newlocale empty and failed LC_ALL transaction')
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   provider=json.loads((root/'backend/runtime/musl/provider.json').read_text());registered=any(c.get('sourceBase')=='libc' and c['directory']=='locale_environment' for c in provider['components'])
   extra=[] if registered else [component/'locale_environment.c']
   run([sys.executable,root/'dotnet.py',tests/'libc_locale_environment.c',*extra,'--language',language,'--libc','musl','--target','portable-lp64-binary80-v1','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','40000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   methods={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']};host=directory/'host';host.mkdir();receiver='owner' if instance else 'Core'
   def call(n):return receiver+'.'+methods[n]
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   isolation=('''using var other=new Core();other.'''+methods['locale_environment_clear']+'''();fixed(byte*key=C("LANG"),value=C("C.UTF-8")){if(other.'''+methods['locale_environment_set']+'''((sbyte*)key,(sbyte*)value)!=0)throw new Exception("other environment");}
if(other.'''+methods['locale_environment_select']+'''()!=63||'''+call('locale_environment_categories')+'''()!=0)throw new Exception("owner locale state");other.Shutdown();''') if instance else ''
   (host/'Program.cs').write_text('''using System;using System.Text;unsafe class Program{
 static byte[] C(string value)=>Encoding.UTF8.GetBytes(value+"\\0");
 static void Main(){string? previous=Environment.GetEnvironmentVariable("LANG");Environment.SetEnvironmentVariable("LANG","HOST_UNSUPPORTED_LOCALE");'''+('using var owner=new Core();' if instance else '')+'''try{
 int result='''+call('locale_environment_contract')+'''();if(result!=0)throw new Exception("locale case "+result);
 '''+call('locale_environment_clear')+'''();fixed(byte*key=C("LANG"),value=C("C")){if('''+call('locale_environment_set')+'''((sbyte*)key,(sbyte*)value)!=0)throw new Exception("source environment");}
 if('''+call('locale_environment_select')+'''()!=0)throw new Exception("source LANG");
 '''+isolation+'''
 if(Environment.GetEnvironmentVariable("LANG")!="HOST_UNSUPPORTED_LOCALE")throw new Exception("host environment changed");
 Environment.SetEnvironmentVariable("LANG","C.UTF-8");if('''+call('locale_environment_select')+'''()!=0)throw new Exception("live host environment leaked");
 '''+call('locale_environment_repeat')+'''();'''+call('locale_environment_repeat')+'''();long before=GC.GetAllocatedBytesForCurrentThread();int mask='''+call('locale_environment_repeat')+'''();long bytes=GC.GetAllocatedBytesForCurrentThread()-before;if(mask!=0||bytes!=0)throw new Exception("selection allocation/state "+bytes);
 }finally{'''+receiver+'''.Shutdown();Environment.SetEnvironmentVariable("LANG",previous);}Console.WriteLine("owner environment locale precedence/transaction/isolation and zero-allocation selection passed");}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT750 precedence cases, transaction/base/owner/host separation, zero-allocation selection')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT750 precedence cases, transaction/base/owner/host separation, zero-allocation selection')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=480);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_locale_environment(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
