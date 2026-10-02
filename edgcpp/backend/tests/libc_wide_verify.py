"""Pinned musl wide algorithms on exhaustive synthetic Unicode/string fixtures."""
import argparse,json,hashlib,shutil,subprocess,sys
from pathlib import Path

def verify_libc_wide(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);fixture=out/'libc_wide.c';shutil.copyfile(tests/'libc_wide.c',fixture);source=out/'wide';shutil.copytree(root/'backend/runtime/musl/wide',source)
 manifest=json.loads((source/'source.json').read_text());units=[]
 for name,entry in manifest['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==entry['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==entry['originalSha256']
  if name.endswith('.c'):units.append(name)
 # Original and adapted sources compile separately; preserve the exact oracle.
 original=source/'originals'
 (original/'locale_impl.h').write_text('#define CURRENT_LOCALE ((locale_t)0)\n')
 (original/'aliases.h').write_text('#define weak_alias(a,b) extern __typeof(a) b __attribute__((alias(#a)))\n')
 (out/'native.c').write_text('#include <stdio.h>\n#include <locale.h>\nint __edg_locale_validate(locale_t l){return l && l!=(locale_t)-1;}\nunsigned long wide_digest(void);int wide_cases(void);int main(void){int r=wide_cases();if(r){fprintf(stderr,"wide case %d\\n",r);return r;}printf("%lu\\n",wide_digest());return 0;}\n')
 oracle_headers=out/'native-include';oracle_headers.mkdir()
 # musl iswblank deliberately calls its full-integer-domain isblank predicate;
 # glibc's ctype table cannot be indexed with arbitrary Unicode codepoints.
 (oracle_headers/'ctype.h').write_text('#include_next <ctype.h>\n#undef isblank\n#define isblank(c) ((c)==32 || (c)==9)\n')
 results=[]
 for kind,path in [('original',original),('adapted',source)]:
  binary=out/f'native-{kind}'
  run(['gcc','-D_GNU_SOURCE','-std=c11','-O2','-fno-builtin',*(['-include',original/'aliases.h'] if kind=='original' else []),'-I',oracle_headers,'-I',path,fixture,*[path/n for n in units],out/'native.c','-o',binary])
  result=subprocess.run([str(binary)],capture_output=True,text=True,timeout=120);(out/f'native-{kind}.log').write_text(result.stdout+result.stderr);result.check_returncode();results.append(int(result.stdout))
 assert results[0]==results[1] and results[0]!=0
 checks=['native original/adapted: all 1114112 Unicode values x 14 operations and wide API boundaries']
 if not translate:return checks
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,*[source/n for n in units],'--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','65000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest_out=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(x['method'] for x in manifest_out['exports'] if x['symbol']==symbol)
   owner_checks=f'if({call("wide_invalid_locale")}(null)!=0 || {call("wide_invalid_locale")}((void*)(nint)(-1))!=0)throw new Exception("invalid locale");'
   if instance:owner_checks+=f'using var other=new Core();void* handle={call("wide_locale_create")}();if(handle==null || {call("wide_invalid_locale","other")}(handle)!=0)throw new Exception("foreign locale");{call("wide_locale_free")}(handle);'
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text('using System;unsafe class Program{static void Main(){'+('using var owner=new Core();' if instance else '')+f'int result={call("wide_cases")}();if(result!=0)throw new Exception($"wide case {{result}}");ulong actual={call("wide_digest")}();if(actual!={results[0]}UL)throw new Exception($"wide native mismatch {{actual}}");'+owner_checks+('owner' if instance else 'Core')+'.Shutdown();Console.WriteLine("all Unicode values wide classification/case oracle and strings passed");}}')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT exhaustive wide algorithms')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT exhaustive wide algorithms')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_wide(root,root/'backend/tests',out,a.nativeaot,run,translate=not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
