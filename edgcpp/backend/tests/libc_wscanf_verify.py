"""Selected musl wide scanf differential and translated C/C++ acceptance."""
import argparse,hashlib,json,shutil,struct,subprocess,sys
from pathlib import Path
from libc_scanf_verify import corpus as narrow_corpus, original_oracle

def corpus():
 rows=[row for row in narrow_corpus() if isinstance(row[2],str)]
 for text in ['\u20acx \U0001f642y!','\u2002\u20ac\u00e9 \u03b1\u03b2!',']-\u20ac\u00e9 \U0001f642!', '123\u20ac34!', 'abc\u3000def!', '']:
  for fmt in ['%3ls%3ls%n%c','%2lc%2lc%n%c','%7l[\u20ac-\u20af]%7ls%n%c']:rows.append((3,fmt,text))
  for fmt in ['%3s%3s%n%c','%2c%2c%n%c','%7[\u20ac-\u20af]%7s%n%c']:rows.append((2,fmt,text))
 for text in ['', '\u20ac'*38+' '+'\U0001f642'*38+'!','abc def!']:
  rows.append((7,'%mls%mls%n%c',text))
 for text in ['12\u20ac34!', '-.5\u20ac +3.25 9!', '0x1p-16445 2 3!']:
  rows.append((0,'%lli %lli%n%c',text));rows.append((1,'%f %lf %Lf%n%c',text))
 return rows

def native_oracle(root,tests,out,run,rows,source):
 p=root/"backend/runtime/musl";t=tests
 cases=out/'cases.txt';cases.write_text(''.join(f'{kind} {fmt.encode("utf-32-le").hex()} {text.encode("utf-32-le").hex()}\n' for kind,fmt,text in rows))
 # Build the already isolated original narrow core as a numerical boundary for
 # the original wide algorithm. Numeric input is ASCII with nonASCII mapped to @.
 old=out/'narrow';old.mkdir();empty=old/'empty';empty.write_text('');original_oracle(root,root/'backend/tests',old,run,empty,p)
 native=out/'original';native.mkdir()
 original=(source/'wscanf/originals/vfwscanf.c').read_text()
 import re
 for a,b in [('getwc','oracle_getwc'),('ungetwc','oracle_ungetwc'),('fscanf','oracle_fscanf'),('fwide','oracle_fwide'),('vfwscanf','oracle_vfwscanf')]:original=re.sub(r'\b'+a+r'\b',b,original)
 (native/'vfwscanf.c').write_text(original)
 (native/'stdio_impl.h').write_text('''#ifndef ORIGINAL_WIDE_FILE
 #define ORIGINAL_WIDE_FILE
 #include <stdio.h>
 #include <stdarg.h>
 #include <wchar.h>
 #include <sys/types.h>
 struct wide_file {unsigned char *rpos,*rend;const wchar_t *position;};
 #define FILE struct wide_file
 #define hidden
 #define FLOCK(f) ((void)0)
 #define FUNLOCK(f) ((void)0)
 #define weak_alias(a,b)
 int oracle_getwc(FILE *);int oracle_ungetwc(int,FILE *);int oracle_fscanf(FILE *,const char *,...);int oracle_vfwscanf(FILE *,const wchar_t *,va_list);
 static int oracle_fwide(FILE *f,int n){(void)f;return n;}
 #endif
 ''')
 for name in ['shgetc.h','intscan.h','floatscan.h']:(native/name).write_text('/* Included by original core but not used outside fscanf boundary. */\n')
 (native/'entry.c').write_text('''#include "stdio_impl.h"
 #include <stdlib.h>
 struct narrow_file {unsigned char *buf,*rpos,*rend,*shend;off_t shlim,shcnt;};
 int __edg_original_vfscanf(struct narrow_file *,const char *,va_list);
 int oracle_getwc(FILE*f){int c=*f->position;if(!c)return -1;++f->position;return c;}
 int oracle_ungetwc(int c,FILE*f){if(c>=0)--f->position;return c;}
 int oracle_fscanf(FILE*f,const char*format,...){size_t n=wcslen(f->position);unsigned char *b=malloc(n+1);if(!b)abort();for(size_t i=0;i<n;++i)b[i]=f->position[i]<128?f->position[i]:'@';b[n]=0;struct narrow_file nf={0};nf.buf=nf.rpos=b;nf.rend=b+n;va_list ap;va_start(ap,format);int result=__edg_original_vfscanf(&nf,format,ap);va_end(ap);f->position+=nf.rpos-b;free(b);return result;}
 int original_scan(const wchar_t*text,const wchar_t*format,...){FILE f={0,0,text};va_list ap;va_start(ap,format);int result=oracle_vfwscanf(&f,format,ap);va_end(ap);return result;}
 ''')
 # Each source keeps its own native FILE shim search path.
 objects=[]
 for i,src in enumerate([native/'vfwscanf.c',native/'entry.c',*[old/'original'/n for n in ['vfscanf.c','intscan.c','floatscan.c','shgetc.c']]]):
  obj=out/f'oracle-{i}.o';run(['gcc','-D_GNU_SOURCE','-Dhidden=','-std=c11','-O2','-fno-builtin','-I',src.parent,'-c',src,'-o',obj]);objects.append(obj)
 run(['gcc','-D_GNU_SOURCE','-DSCAN=original_scan','-std=c11','-O2','-fno-builtin',t/'libc_wscanf.c',t/'libc_wscanf_native.c',*objects,'-lm','-o',out/'original-native']);run([out/'original-native',cases,out/'original.bin'])
 bridge=out/'bridge.c';bridge.write_text((root/'backend/tests/libc_numeric_native.c').read_text().split('int main(')[0]+'''\n#include <stdarg.h>
 #include <wchar.h>
 int __edg_with_file_wide_read_lock(FILE*f,int(*body)(void*,const wchar_t*,va_list),void*ctx,const wchar_t*fmt,va_list ap){(void)f;return body(ctx,fmt,ap);}
 ''')
 units=[source/'wscanf/vfwscanf.c',source/'wscanf/wscan_wrappers.c',source/'strto/intscan.c',source/'strto/floatscan.c']
 run(['gcc','-D_GNU_SOURCE','-std=c11','-O2','-fno-builtin',t/'libc_wscanf.c',t/'libc_wscanf_native.c',*units,bridge,'-lm','-o',out/'native']);run([out/'native',cases,out/'golden.bin'])
 a=(out/'original.bin').read_bytes();b=(out/'golden.bin').read_bytes();assert len(a)==len(b)==len(rows)*512
 errors=[]
 for i,row in enumerate(rows):
  if a[i*512:i*512+512]!=b[i*512:i*512+512]:errors.append({'index':i,'case':row,'expected':a[i*512:i*512+512].hex(),'actual':b[i*512:i*512+512].hex()})
 (out/'native-differences.json').write_text(json.dumps(errors,indent=2));print('cases',len(rows),'diff',len(errors));assert not errors,errors[:2]
 return [f'{len(rows)} original/adapted musl wide scanf cases']

def translated_wscanf_units(root, provider):
 # The automatic package owns registered definitions. A candidate must never
 # silently test canonical algorithms while its native oracle tests other bytes.
 package = root / 'backend/runtime/musl'
 selected = json.loads((package / 'provider.json').read_text())['components']
 registered = any(c['directory'] == 'wscanf' and c.get('sourceBase', 'musl') == 'musl' for c in selected)
 if not registered:
  return [provider / 'wscanf/vfwscanf.c', provider / 'wscanf/wscan_wrappers.c']
 for component in ['wscanf', 'strto']:
  manifest = json.loads((provider / component / 'source.json').read_text())
  for name in [*manifest['sources'], *manifest['adapters']]:
   candidate = provider / component / name
   canonical = package / component / name
   if candidate.read_bytes() != canonical.read_bytes():
    raise ValueError(f'{component}/{name}: candidate differs from registered musl provider; use a separate provider snapshot for translated acceptance')
 return []

def verify_libc_wscanf(root,tests,out,nativeaot,run,translate=True,library_source=None):
 out.mkdir(parents=True,exist_ok=True);source=library_source or root/'backend/runtime/musl';provider=out/'provider';provider.mkdir()
 for component in ['wscanf','strto']:shutil.copytree(source/component,provider/component)
 for component in ['wscanf','strto']:
  metadata=json.loads((provider/component/'source.json').read_text())
  for name,item in metadata['sources'].items():
   assert hashlib.sha256((provider/component/name).read_bytes()).hexdigest()==item['adaptedSha256']
  for name,digest in metadata['adapters'].items():assert hashlib.sha256((provider/component/name).read_bytes()).hexdigest()==digest
 rows=corpus();checks=native_oracle(root,tests,out,run,rows,provider)
 if not translate:return checks
 cases=out/'cases.txt';fixture=out/'fixture.c';shutil.copy2(tests/'libc_wscanf.c',fixture)
 units=translated_wscanf_units(root,provider)
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*units,'--language',language,'--target','portable-lp64-binary80-v1','--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','40000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports=json.loads((directory/'dot-gen.json').read_text())['exports'];host=directory/'host';host.mkdir()
   def call(symbol):return ('owner.' if instance else 'Core.')+next(item['method'] for item in exports if item['symbol']==symbol)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.IO;using System.Runtime.InteropServices;using System.Text;
unsafe class Program{{static byte* Copy(byte[] bytes){{byte* p=(byte*)NativeMemory.Alloc((nuint)bytes.Length+4);bytes.CopyTo(new Span<byte>(p,bytes.Length));NativeMemory.Clear(p+bytes.Length,4);return p;}}
static void Main(string[] args){{{('using var owner=new Core();' if instance else '')}
if({call('wscanf_activate')}()!=1||{call('wscanf_address')}()!=0)throw new Exception("locale/function address");
byte[] golden=File.ReadAllBytes(args[1]);byte* actual=(byte*)NativeMemory.Alloc(512);int index=0;
try{{foreach(string line in File.ReadLines(args[0])){{string[] parts=line.Split(' ');byte* format=Copy(Convert.FromHexString(parts[1])),text=Copy(Convert.FromHexString(parts[2]));
try{{{call('wscanf_case')}(int.Parse(parts[0]),(int*)text,(int*)format,actual);if(!new ReadOnlySpan<byte>(actual,512).SequenceEqual(golden.AsSpan(index*512,512)))throw new Exception($"wide scanf case {{index}} {{line}} actual={{Convert.ToHexString(new ReadOnlySpan<byte>(actual,512))}} expected={{Convert.ToHexString(golden.AsSpan(index*512,512))}}");}}finally{{NativeMemory.Free(format);NativeMemory.Free(text);}}++index;}}
byte* path=Copy(Encoding.UTF8.GetBytes(args[2]));try{{int result={call('wscanf_file_cases')}((sbyte*)path);if(result!=0)throw new Exception($"FILE scan {{result}}");}}finally{{NativeMemory.Free(path);}}
}}finally{{NativeMemory.Free(actual);{('owner' if instance else 'Core')}.Shutdown();}}Console.WriteLine($"{{index}} original musl wide scanf cases and FILE/va_list/address contracts passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll',cases,out/'golden.bin',directory/'scan-data.bin']);checks.append(directory.name+f': JIT {len(rows)} wide scanf cases and FILE contracts')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host',cases,out/'golden.bin',directory/'scan-data-aot.bin']);checks.append(directory.name+f': NativeAOT {len(rows)} wide scanf cases and FILE contracts')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--source-dir',type=Path);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];index=0;checks=[]
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420);(out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_wscanf(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only,a.source_dir)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
