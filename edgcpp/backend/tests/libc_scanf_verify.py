"""Selected musl scanf core versus original isolated musl FILE oracle."""
import argparse,hashlib,json,shutil,struct,subprocess,sys
from pathlib import Path

def wide_field_corpus():
 rows=[]
 for text in [b'abc def!',b'\xc3\xa9x y!',b'\xe2\x82\xacx y!',b'\xf0\x9f\x99\x82x y!',b'a\xe2\x82\xac b!',b'\xe2\x82\xac',b'\xc3',b'\xe2\x82',b'\xf0\x9f\x99',b'A\xff!',b'\xc3\xa9\xc3\xa9 xyz!']:
  for width in range(1,8):
   for conversion in ['lc','ls','l[^ ]']:
    rows.append((3,f'%{width}{conversion}%7ls%n%c',text))
 for width in [1,2,3,4,7]:
  for text in [b'\xe2\x82\xac abc def!',b'abc def!',b'\xf0\x9f\x99\x82x yz!']:
   rows.append((3,f'%*{width}lc%7ls%7ls%n%c',text))
 return rows


def corpus():
 rows=[]
 for text in ['', ' ', 'x','12','-17 23!','0xff 077tail','0xg 5','+ 3','123abc','9223372036854775807 -9223372036854775808!', '18446744073709551615 0!', '12 34!', '12X34!', '12%34!']:
  for fmt in ['%lli %lli%n%c','%3lli%2lli%n%c','%lld %lld%n%c','%llx %llo%n%c','%lliX%lli%n%c','%lli%%%lli%n%c','%*s%lli %lli%n%c']:
   rows.append((0,fmt,text))
 for text in ['', 'x','1 2 3!','-.5 +3.25 -0x1.8p2!','inf -infinity nan!','1e 2 3','0xg 2 3','1.000000059604644775390625 1.00000000000000011102230246251565404236316680908203125 1.000000000000000000108420217248550443400745280086994171142578125!', '0x1p-149 0x1p-1074 0x1p-16445!', '0x1p128 0x1p1024 0x1p16384!','nan(payload) nan(bad!) 0!']:
  for fmt in ['%f %lf %Lf%n%c','%6f%6lf%6Lf%n%c','%a %la %La%n%c']:rows.append((1,fmt,text))
 for text in ['', ' ', 'abc def!', ']--abc XYZ!', '123,456;', 'abcdefg', 'abc\nxyz', 'a'*120+' '+'b'*120+'!']:
  for fmt in ['%7s%7s%n%c','%3c%2c%n%c','%7[a-z]%7[^!]%n%c','%7[]-]%7s%n%c','%7[0-9],%7[0-9]%n%c','%*3c%7s%7s%n%c']:rows.append((2,fmt,text))
 for text in ['', 'abc def!', '\u20acx \U0001f642y!', 'a'*120+' '+'b'*120+'!']:
  for fmt in ['%7ls%7ls%n%c','%3lc%2lc%n%c','%7l[a-z]%7ls%n%c']:rows.append((3,fmt,text))
 for text in ['', 'abc def!', 'a'*120+' '+'b'*120+'!', '123,456;']:
  for fmt in ['%ms%ms%n%c','%m[a-z]%m[^!]%n%c','%*3c%ms%ms%n%c']:rows.append((4,fmt,text))
 for text in ['', '17 29', '17x', '11 22 33']:
  for fmt in ['%2$d %1$d%3$n','%1$d%1$d%3$n','%*d%1$d%2$d%3$n']:rows.append((5,fmt,text))
 for text in ['-128 -32768 -2147483648 -9223372036854775808 -17 -19 23 -29','1 2 3 4 5 6 7 8','x','1 2']:
  rows.append((6,'%hhd %hd %d %ld %lld %jd %zu %td%n',text))
 for data in [b'\xc0\x80',b'\xc3',b'\xe2\x82',b'\xed\xa0\x80',b'\xf4\x90\x80\x80',b'\xff',b'A\xe2\x82']:
  rows.append((3,'%7ls%7ls%n%c',data))
 for fmt in ['%','%q','%[','%m[','%0s%7s%n%c']:
  rows.append((2,fmt,'abc'))
 rows.append((6,'%hhn%hn%n%ln%lln%jn%zn%tn%n',''))
 rows.append((6,'%*7c%hhn%hn%n%ln%lln%jn%zn%tn%n','1234567tail'))
 rows.extend(wide_field_corpus())
 return rows

def original_oracle(root,tests,out,run,cases,provider):
 native=out/'original';native.mkdir()
 for name in ['shgetc.c','shgetc.h','intscan.h','floatscan.h']:shutil.copy2(provider/'scanf/reference'/name,native/name)
 shutil.copy2(provider/'scanf/originals/vfscanf.c',native/'vfscanf.c')
 for name in ['intscan','floatscan']:shutil.copy2(provider/'strto'/(name+'.original.c'),native/(name+'.c'))
 (native/'stdio_impl.h').write_text('''#ifndef EDG_NATIVE_SCAN_FILE
#define EDG_NATIVE_SCAN_FILE
#include <stdio.h>
#include <stdarg.h>
#include <sys/types.h>
struct edg_original_file {unsigned char *buf,*rpos,*rend,*shend;off_t shlim,shcnt;};
#define FILE struct edg_original_file
#define hidden
#define FLOCK(f) ((void)0)
#define FUNLOCK(f) ((void)0)
#define weak_alias(a,b)
#define vfscanf __edg_original_vfscanf
#define __uflow __edg_original_uflow
static int __toread(FILE *f){(void)f;return 0;}
static int __uflow(FILE *f){(void)f;return EOF;}
int __edg_original_vfscanf(FILE *,const char *,va_list);
#endif
''')
 (native/'entry.c').write_text('''#include "stdio_impl.h"
#include <string.h>
int original_scan(const char *text,const char *format,...){FILE f={0};f.buf=f.rpos=(unsigned char*)text;f.rend=f.rpos+strlen(text);va_list ap;va_start(ap,format);int result=__edg_original_vfscanf(&f,format,ap);va_end(ap);return result;}
''')
 mb=native/'multibyte';mb.mkdir()
 for name in ['mbrtowc.c','mbsinit.c','internal.c','internal.h']:shutil.copy2(provider/'multibyte/originals'/name,mb/name)
 (mb/'locale_impl.h').write_text('#include <stdlib.h>\n')
 run(['gcc','-D_GNU_SOURCE','-Dhidden=','-DSCAN=original_scan','-std=c11','-O2','-fno-builtin','-I',native,tests/'libc_scanf.c',tests/'libc_scanf_native.c',*[native/n for n in ['vfscanf.c','intscan.c','floatscan.c','shgetc.c','entry.c']],*[mb/n for n in ['mbrtowc.c','mbsinit.c','internal.c']],'-lm','-o',out/'original-native'])
 run([out/'original-native',cases,out/'original.bin'])

def verify_libc_scanf(root,tests,out,nativeaot,run,translate=True,library_source=None):
 out.mkdir(parents=True,exist_ok=True);source=library_source or root/'backend/runtime/musl';provider=out/'provider';provider.mkdir()
 for component in ['strto','scanf']:shutil.copytree(source/component,provider/component)
 shutil.copytree(root/'backend/runtime/musl/multibyte',provider/'multibyte')
 for component in ['scanf','strto','multibyte']:
  metadata=json.loads((provider/component/'source.json').read_text())
  for name,item in metadata['sources'].items():
   assert hashlib.sha256((provider/component/name).read_bytes()).hexdigest()==item['adaptedSha256']
   original=provider/component/(name.replace('.c','.original.c') if component=='strto' else 'originals/'+name)
   assert hashlib.sha256(original.read_bytes()).hexdigest()==item['originalSha256']
  for name,digest in metadata['adapters'].items():assert hashlib.sha256((provider/component/name).read_bytes()).hexdigest()==digest
  for name,item in metadata.get('referenceOnly',{}).items():assert hashlib.sha256((provider/component/'reference'/name).read_bytes()).hexdigest()==item['sha256']
 fixture=out/'fixture.c';shutil.copy2(tests/'libc_scanf.c',fixture);rows=corpus();cases=out/'cases.txt';cases.write_text(''.join(f'{kind} {fmt.encode().hex()} {(text if isinstance(text,bytes) else text.encode()).hex()}\n' for kind,fmt,text in rows))
 original_oracle(root,tests,out,run,cases,provider)
 units=[provider/'scanf/vfscanf.c',provider/'scanf/scan_wrappers.c',provider/'strto/intscan.c',provider/'strto/floatscan.c',*[provider/'multibyte'/n for n in ['mbrtowc.c','mbsinit.c','internal.c']]]
 bridge=out/'bridge.c';bridge.write_text((tests/'libc_numeric_native.c').read_text().split('int main(')[0]+'''\n#include <stdarg.h>
int __edg_with_file_read_lock(FILE *f,int(*body)(void*,const char*,va_list),void *context,const char *format,va_list arguments){(void)f;return body(context,format,arguments);}
''')
 run(['gcc','-D_GNU_SOURCE','-std=c11','-O2','-fno-builtin',fixture,tests/'libc_scanf_native.c',*units,bridge,'-lm','-o',out/'native'])
 run([out/'native',cases,out/'golden.bin']);expected=(out/'original.bin').read_bytes();actual=(out/'golden.bin').read_bytes();assert len(actual)==len(rows)*512
 differences=[]
 for index,row in enumerate(rows):
  if actual[index*512:index*512+512]!=expected[index*512:index*512+512]:differences.append({'index':index,'case':repr(row),'expected':expected[index*512:index*512+512].hex(),'actual':actual[index*512:index*512+512].hex()})
 (out/'native-differences.json').write_text(json.dumps(differences,indent=2)+'\n');assert not differences,differences[:2]
 checks=[f'{len(rows)} original/adapted musl scanf memory cases']
 if not translate:return checks
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*units,'--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','40000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports=json.loads((directory/'dot-gen.json').read_text())['exports'];host=directory/'host';host.mkdir()
   def call(symbol):return ('owner.' if instance else 'Core.')+next(item['method'] for item in exports if item['symbol']==symbol)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.IO;using System.Runtime.InteropServices;using System.Text;
unsafe class Program{{static byte* Copy(byte[] bytes){{byte* p=(byte*)NativeMemory.Alloc((nuint)bytes.Length+1);bytes.CopyTo(new Span<byte>(p,bytes.Length));p[bytes.Length]=0;return p;}}
static void Main(string[] args){{{('using var owner=new Core();' if instance else '')}
if({call('scanf_activate')}()!=1||{call('scanf_address')}()!=0)throw new Exception("locale/function address");
byte[] golden=File.ReadAllBytes(args[1]);byte* actual=(byte*)NativeMemory.Alloc(512);int index=0;
try{{foreach(string line in File.ReadLines(args[0])){{string[] parts=line.Split(' ');byte* format=Copy(Convert.FromHexString(parts[1])),text=Copy(Convert.FromHexString(parts[2]));
try{{{call('scanf_case')}(int.Parse(parts[0]),(sbyte*)text,(sbyte*)format,actual);if(!new ReadOnlySpan<byte>(actual,512).SequenceEqual(golden.AsSpan(index*512,512)))throw new Exception($"scanf case {{index}} {{line}} actual={{Convert.ToHexString(new ReadOnlySpan<byte>(actual,512))}} expected={{Convert.ToHexString(golden.AsSpan(index*512,512))}}");}}finally{{NativeMemory.Free(format);NativeMemory.Free(text);}}++index;}}
byte* path=Copy(Encoding.UTF8.GetBytes(args[2]));try{{int result={call('scanf_file_cases')}((sbyte*)path);if(result!=0)throw new Exception($"FILE scan {{result}}");}}finally{{NativeMemory.Free(path);}}
}}finally{{NativeMemory.Free(actual);{('owner' if instance else 'Core')}.Shutdown();}}Console.WriteLine($"{{index}} original musl scanf cases and FILE/va_list/address contracts passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll',cases,out/'golden.bin',directory/'scan-data.bin']);checks.append(directory.name+f': JIT {len(rows)} scanf cases and FILE contracts')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host',cases,out/'golden.bin',directory/'scan-data-aot.bin']);checks.append(directory.name+f': NativeAOT {len(rows)} scanf cases and FILE contracts')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--source-dir',type=Path);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];index=0;checks=[]
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420);(out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_scanf(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only,a.source_dir)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
