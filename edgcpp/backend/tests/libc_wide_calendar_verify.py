"""Original/adapted musl wide calendar formatting and translated C/C++ modes."""
import argparse,hashlib,json,os,shutil,subprocess,sys
from pathlib import Path
from libc_calendar_verify import NATIVE_PREFIX,NATIVE_SHIM,verify_libc_calendar

def verify_libc_wide_calendar(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True)
 # The narrow dependency oracle also stages untouched native headers/sources.
 narrow=out/'narrow';verify_libc_calendar(root,tests,narrow,False,run,translate=False)
 source=out/'wide_calendar';shutil.copytree(root/'backend/runtime/musl/wide_calendar',source);shutil.copytree(narrow/'calendar',out/'calendar')
 fixture=out/'libc_wide_calendar.c';shutil.copyfile(tests/'libc_wide_calendar.c',fixture)
 manifest=json.loads((source/'source.json').read_text())
 for name,e in manifest['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==e['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==e['originalSha256']
 for name,digest in manifest['adapters'].items():assert hashlib.sha256((source/name).read_bytes()).hexdigest()==digest
 original=source/'originals';(original/'locale_impl.h').write_text('#include "../../calendar/originals/locale_impl.h"\n')
 (original/'time_impl.h').write_text('#include "../../calendar/originals/time_impl.h"\nconst char *__strftime_fmt_1(char (*)[100],size_t*,int,const struct tm*,locale_t,int);\n')
 (out/'native-prefix.h').write_text(NATIVE_PREFIX);(out/'native.c').write_text(NATIVE_SHIM.replace('calendar_cases','wide_calendar_cases').replace('calendar_digest','wide_calendar_digest'))
 units=list(json.loads((out/'calendar/source.json').read_text())['sources']);results=[]
 for kind,path in [('original',original),('adapted',source)]:
  calendar=out/'calendar'/('originals' if kind=='original' else '.');binary=out/f'native-{kind}'
  run(['gcc','-std=c11','-O2','-fno-builtin','-Werror=implicit-function-declaration','-I',root/'backend/runtime/libc/include','-I',path,'-include',out/'native-prefix.h',*(['-DORIGINAL'] if kind=='original' else []),fixture,path/'wcsftime.c',*[calendar/n for n in units],*([path/'wide_calendar_wrappers.c',calendar/'calendar_wrappers.c'] if kind=='adapted' else []),out/'native.c','-o',binary])
  r=subprocess.run([str(binary)],capture_output=True,text=True,timeout=120);(out/f'native-{kind}.log').write_text(r.stdout+r.stderr);print(kind,r.stdout);r.check_returncode();results.append(int(r.stdout))
 assert results[0]==results[1] and results[0]!=0
 checks=['native original/adapted: wide format matrix, extended years, Unicode literals and bounds']
 if not translate:return checks
 selected={c['directory'] for c in json.loads((root/'backend/runtime/musl/provider.json').read_text())['components']}
 if not {'calendar','wide_numeric','multibyte'}<=selected:raise RuntimeError('wide calendar acceptance requires registered calendar+wide numeric+multibyte dependencies')
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*([] if 'wide_calendar' in selected else [source/'wcsftime.c',source/'wide_calendar_wrappers.c']),'--language',language,'--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','65000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports=json.loads((directory/'dot-gen.json').read_text())['exports'];host=directory/'host';host.mkdir()
   def call(symbol):return ('owner.' if instance else 'Core.')+next(x['method'] for x in exports if x['symbol']==symbol)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;unsafe class Program{{static void Main(){{{('using var owner=new Core();' if instance else '')}
int result={call('wide_calendar_cases')}();if(result!=0)throw new Exception($"wide calendar case {{result}}");ulong digest={call('wide_calendar_digest')}();if(digest!={results[0]}UL)throw new Exception($"wide calendar digest {{digest}}");if({call('wide_calendar_invalid')}(null)!=1 || {call('wide_calendar_invalid')}((void*)(nint)(-1))!=1)throw new Exception("wide locale validation");{('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("wide calendar native formatting/Unicode/bounds/locale oracle passed");}}}}''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT wide calendar formatting')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT wide calendar formatting')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command,**kwargs):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420,**kwargs)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_wide_calendar(root,root/'backend/tests',out,a.nativeaot,run,translate=not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
