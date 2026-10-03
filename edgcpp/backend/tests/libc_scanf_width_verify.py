"""Representable scanf width arithmetic: original UBSan failures and corrected native/generated acceptance."""
import argparse,json,subprocess,sys
from pathlib import Path
from libc_wscanf_verify import native_oracle

def verify_libc_scanf_width_native(root,tests,out,run,provider=None):
 out.mkdir(parents=True,exist_ok=True);provider=provider or root/'backend/runtime/musl';checks=[]
 setup=out/'setup';setup.mkdir();native_oracle(root,tests,setup,run,[],provider)
 for kind in ['narrow','wide']:
  function='scan_width_'+kind;(out/(kind+'-main.c')).write_text(f'int {function}(void);int main(void){{return {function}();}}\n')
  flags=['gcc','-D_GNU_SOURCE','-Dhidden=','-DORIGINAL_'+kind.upper(),'-std=c11','-O1','-fno-builtin','-fsanitize=undefined','-fno-sanitize-recover=undefined']
  if kind=='narrow':
   source=setup/'narrow/original';original=[source/n for n in ['vfscanf.c','entry.c','intscan.c','floatscan.c','shgetc.c']];include=['-I',source]
   adapted=[provider/'scanf/vfscanf.c',provider/'scanf/scan_wrappers.c',provider/'strto/intscan.c',provider/'strto/floatscan.c']
   bridge=out/'narrow-bridge.c';bridge.write_text((setup/'bridge.c').read_text()+'\nint __edg_with_file_read_lock(FILE*f,int(*body)(void*,const char*,va_list),void*ctx,const char*fmt,va_list ap){(void)f;return body(ctx,fmt,ap);}\n');adapted.append(bridge)
  else:
   source=setup/'original';original=[source/'vfwscanf.c',*[setup/f'oracle-{i}.o' for i in range(1,6)]];include=['-I',source]
   adapted=[provider/'wscanf/vfwscanf.c',provider/'wscanf/wscan_wrappers.c',provider/'strto/intscan.c',provider/'strto/floatscan.c',setup/'bridge.c']
  for variant,units in [('original',original),('adapted',adapted)]:
   binary=out/(kind+'-'+variant);run([*flags,*(['-D'+kind.upper()+'_SCAN=original_scan',*include] if variant=='original' else []),tests/'libc_scanf_width.c',out/(kind+'-main.c'),*units,'-lm','-o',binary])
   if variant=='original':
    script='import subprocess,sys; r=subprocess.run(sys.argv[1:],capture_output=True,text=True); print(r.stdout+r.stderr); assert r.returncode==1,r.returncode; assert "signed integer overflow" in r.stderr'
    run([sys.executable,'-c',script,binary]);checks.append(kind+': retained original UBSan intermediate overflow')
   else:run([binary]);checks.append(kind+': eight valid representable widths pass UBSan')
 return checks

def verify_libc_scanf_width(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=verify_libc_scanf_width_native(root,tests,out/'native',run)
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',tests/'libc_scanf_width.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','musl','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','40000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir();methods={x['symbol']:x['method'] for x in manifest['exports']};receiver='owner' if instance else 'Core'
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   body=''.join(f'int {n}={receiver}.{methods["scan_width_"+n]}();if({n}!=0)throw new Exception("{n} width case "+{n});' for n in ['narrow','wide'])
   (host/'Program.cs').write_text('using System;unsafe class Program{static void Main(){'+('for(int i=0;i<2;++i){using var owner=new Core();' if instance else '{')+body+receiver+'.Shutdown();}Console.WriteLine("representable narrow/wide scanf width limits passed");}}')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT narrow/wide valid-width arithmetic')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT narrow/wide valid-width arithmetic')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=420);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_scanf_width(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
