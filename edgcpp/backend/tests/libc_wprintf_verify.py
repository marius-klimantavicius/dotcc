"""Pinned musl wide formatting, original-core differential and translated checks."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def native_oracle(root,tests,out,run,p):
 original=out/'original';original.mkdir()
 (original/'vfwprintf.c').write_text((p/'wprintf/originals/vfwprintf.c').read_text().replace('vfwprintf(', 'oracle_vfwprintf('))
 (original/'stdio_impl.h').write_text('''#include <stdio.h>
 #include <wchar.h>
 #include <stdarg.h>
 #include <stdint.h>
 #include <limits.h>
 struct oracle_file {wchar_t *next;size_t left;int flags;};
 #undef FILE
 #define FILE struct oracle_file
 #define F_ERR 1
 #define FLOCK(f) ((void)0)
 #define FUNLOCK(f) ((void)0)
 #undef NL_ARGMAX
 #define NL_ARGMAX 9
 #define ferror(f) ((f)->flags & F_ERR)
 #define fwide(f,n) (n)
 #define fputwc oracle_put
 #define fprintf oracle_print
 int oracle_put(wchar_t,FILE*);int oracle_print(FILE*,const char*,...);
 ''')
 (original/'bridge.c').write_text('''#include "stdio_impl.h"
 #include <errno.h>
 #include <stdlib.h>
 #include "format_sink.h"
 int oracle_vfwprintf(FILE*,const wchar_t*,va_list);
 int oracle_put(wchar_t c,FILE*f){char bytes[MB_LEN_MAX];mbstate_t state={0};if(wcrtomb(bytes,c,&state)==(size_t)-1){f->flags|=F_ERR;return -1;}if(f->left){*f->next++=c;--f->left;}*f->next=0;return c;}
 static int write_number(void*ctx,const char*bytes,size_t count){FILE*f=(FILE*)ctx;while(count--)if(oracle_put((unsigned char)*bytes++,f)<0)return -1;return 0;}
 int oracle_print(FILE*f,const char*fmt,...){struct edg_format_sink sink={write_number,f,0};va_list ap;va_start(ap,fmt);int result=__edg_musl_vformat(&sink,fmt,ap);va_end(ap);return result;}
 int oracle_swprintf(wchar_t*text,size_t capacity,const wchar_t*format,...){if(!capacity)return -1;*text=0;FILE f={text,capacity-1,0};va_list args;va_start(args,format);int result=oracle_vfwprintf(&f,format,args);va_end(args);return result<0||(size_t)result>=capacity?-1:result;}
 ''')
 (out/'bridge.c').write_text('''#include <stdio.h>
 #include <wchar.h>
 #include <stdarg.h>
 #include <math.h>
 double __edg_scalbn_nearest(double x,int n){return scalbn(x,n);}
 int __edg_with_file_wide_lock(FILE*f,int(*body)(void*,const wchar_t*,va_list),void*context,const wchar_t*format,va_list args){flockfile(f);fwide(f,1);int result=body(context,format,args);funlockfile(f);return result;}
 ''')
 formats={0:['%d %u%n','%+08d %#08x%n','%2$u:%1$d%3$n','[%20.8d][%-12u]%n','%% %d %u%n','%d|%u%'],1:['%.8Lf|%La%n','%+.3Le|%Lg%n','%2$La:%1$Lf%3$n'],2:['[%s][%ls]%n','[%7.2s][%-6.2ls]%n','%2$ls:%1$s%3$n'],3:['%c|%lc%n','%5lc|%-4lc%n'],4:['%lld %llu%n','%+025lld:%#025llx%n'],5:['%p %p%n','%020p:%20p%n'],6:['%.17g:%a%n','%+.12e:%g%n'],7:['%*.*Lf%n','%3$*1$.*2$Lf%4$n'],8:['%s %ls%n','%.0s %.2ls%n'],9:['%ls %s%n','%.0ls %.1s%n'],10:['%La %.3Le%n'],11:['%La %.3Le%n'],12:['%La %.3Le%n']}
 rows=[(kind,fmt,cap) for kind,fmts in formats.items() for fmt in fmts for cap in [0,1,2,5,40,200]]
 (out/'cases.txt').write_text(''.join(f'{kind} {cap} {fmt.encode("utf-32-le").hex()}\n' for kind,fmt,cap in rows));checks=[]
 for variant in ['original','adapted','pointer-reference']:
  sources=[original/'vfwprintf.c',original/'bridge.c'] if variant!='adapted' else [p/'wprintf/vfwprintf.c',p/'wprintf/wformat_wrappers.c',p/'wprintf/wcsnlen.c']
  command=['gcc','-U_FORTIFY_SOURCE','-D_FORTIFY_SOURCE=0','-D_GNU_SOURCE','-std=c11','-O2','-fno-builtin','-I',str(p/'printf'),*(['-DSWPRINTF=oracle_swprintf'] if variant!='adapted' else []),*(['-DWPRINTF_CASE=wprintf_reference_case',str(tests/'libc_wprintf_pointer_reference.c'),str(p/'printf/vsnprintf.c')] if variant=='pointer-reference' else []),str(tests/'libc_wprintf.c'),str(tests/'libc_wprintf_native.c'),*map(str,sources),str(p/'printf/vfprintf.c'),str(out/'bridge.c'),'-lm','-o',str(out/(variant+'-native'))]
  # Path expression kept simple for command provenance.
  command[-1]=str(out/(variant+'-native'))
  run(command)
  run([out/(variant+'-native'),out/'cases.txt',out/(variant+'.bin'),out/(variant+'-file.bin')])
 original_bytes=(out/'original.bin').read_bytes();a=(out/'pointer-reference.bin').read_bytes();b=(out/'adapted.bin').read_bytes();assert len(a)==len(b)==len(rows)*1040
 pointer_changes=[]
 for i,row in enumerate(rows):
  if original_bytes[i*1040:(i+1)*1040]!=a[i*1040:(i+1)*1040]:
   assert row[0]==5
   pointer_changes.append({'index':i,'case':row})
 (out/'pointer-correction-records.json').write_text(json.dumps(pointer_changes,indent=2))
 errors=[{'index':i,'case':row,'original':a[i*1040:(i+1)*1040].hex(),'adapted':b[i*1040:(i+1)*1040].hex()} for i,row in enumerate(rows) if a[i*1040:(i+1)*1040]!=b[i*1040:(i+1)*1040]]
 (out/'differences.json').write_text(json.dumps(errors,indent=2));print(len(rows),'cases',len(errors),'differences');assert not errors,errors[:1]
 return [f'{len(rows)} selected wide formatter cases (original core plus independent pointer correction)']

def verify_libc_wprintf(root,tests,out,nativeaot,run,translate=True,library_source=None):
 out.mkdir(parents=True,exist_ok=True);source=library_source or root/'backend/runtime/musl';provider=out/'provider';provider.mkdir()
 shutil.copytree(source/'wprintf',provider/'wprintf');shutil.copytree(root/'backend/runtime/musl/printf',provider/'printf')
 checks=native_oracle(root,tests,out,run,provider)
 if not translate:return checks
 selected=json.loads((root/'backend/runtime/musl/provider.json').read_text())['components']
 registered=any(c['directory']=='wprintf' for c in selected)
 if registered:
  metadata=json.loads((provider/'wprintf/source.json').read_text())
  for name in [*metadata['sources'],*metadata['adapters']]:
   assert (provider/'wprintf'/name).read_bytes()==(root/'backend/runtime/musl/wprintf'/name).read_bytes(),'candidate differs from registered provider'
 units=[] if registered else [provider/'wprintf'/n for n in ['vfwprintf.c','wformat_wrappers.c','wcsnlen.c']]
 fixture=out/'fixture.c';shutil.copy2(tests/'libc_wprintf.c',fixture)
 for language in ['c','c++']:
  for instance in [False,True]:
   label=language.replace('+','x')+'-'+('instance' if instance else 'static');directory=out/label
   run([sys.executable,root/'dotnet.py',fixture,*units,'--language',language,'--libc','musl','--target','portable-lp64-binary80-v1','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','45000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports={item['symbol']:item['method'] for item in json.loads((directory/'dot-gen.json').read_text())['exports']};receiver='owner' if instance else 'Core';host=directory/'host';host.mkdir()
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.IO;using System.Text;using System.Runtime.InteropServices;
unsafe class Program{{static void Main(string[]args){{{('using var owner=new Core();' if instance else '')}
if({receiver}.{exports['wprintf_activate']}()!=1)throw new Exception("locale");byte[] golden=File.ReadAllBytes(args[1]);byte* actual=(byte*)NativeMemory.Alloc(1040);int index=0;
try{{foreach(string line in File.ReadLines(args[0])){{string[]parts=line.Split(' ');byte[]format=Convert.FromHexString(parts[2]);byte* p=(byte*)NativeMemory.Alloc((nuint)format.Length+4);format.CopyTo(new Span<byte>(p,format.Length));NativeMemory.Clear(p+format.Length,4);
try{{{receiver}.{exports['wprintf_case']}(int.Parse(parts[0]),(int*)p,uint.Parse(parts[1]),actual);if(!new ReadOnlySpan<byte>(actual,1040).SequenceEqual(golden.AsSpan(index*1040,1040)))throw new Exception($"wide printf case {{index}} {{line}} actual={{Convert.ToHexString(new ReadOnlySpan<byte>(actual,1040))}} expected={{Convert.ToHexString(golden.AsSpan(index*1040,1040))}}");}}finally{{NativeMemory.Free(p);}}index++;}}
byte[] path=Encoding.UTF8.GetBytes(args[2]+"\\0");fixed(byte* p=path){{int result={receiver}.{exports['wprintf_file_case']}((sbyte*)p);if(result!=0)throw new Exception($"wide FILE {{result}}");}}
}}finally{{NativeMemory.Free(actual);{receiver}.Shutdown();}}Console.WriteLine($"{{index}} wide printf records and FILE/va_list/address cases passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll',out/'cases.txt',out/'adapted.bin',directory/'stream.bin']);checks.append(label+': JIT wide formatter differential/FILE cases')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host',out/'cases.txt',out/'adapted.bin',directory/'stream-aot.bin']);checks.append(label+': NativeAOT wide formatter differential/FILE cases')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--source-dir',type=Path);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420);(out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_wprintf(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only,a.source_dir)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
