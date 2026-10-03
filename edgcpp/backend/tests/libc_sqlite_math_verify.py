"""Selected SQLite double exp/pow/trig/hyperbolic closure, exact bits and errno."""
import argparse,hashlib,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_sqlite_math(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);source=out/'sqlite_math';shutil.copytree(root/'backend/runtime/musl/sqlite_math',source);fixture=out/'fixture.c';shutil.copy2(tests/'libc_sqlite_math.c',fixture)
 meta=json.loads((source/'source.json').read_text());names=[n for n in meta['sources'] if n.endswith('.c')]
 logs=out/'math_logs';shutil.copytree(root/'backend/runtime/musl/math_logs',logs);logmeta=json.loads((logs/'source.json').read_text());lognames=[n for n in logmeta['sources'] if n.endswith('.c')]
 replay=out/'replay';shutil.copytree(source/'originals',replay)
 for name,item in meta['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==item['adaptedSha256'];assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==item['originalSha256']
  patch=source/item['patch'];assert hashlib.sha256(patch.read_bytes()).hexdigest()==item['patchSha256']
  if patch.stat().st_size:run(['patch','--batch','--fuzz=0','-p1','-i',patch],cwd=replay)
  assert (replay/name).read_bytes()==(source/name).read_bytes()
 (source/'originals/fp_arch.h').write_text('/* Generic musl math configuration for native oracle. */\n')
 shim=out/'native_math_shim.h';shim.write_text('#include <math.h>\n'+''.join('#define '+name+' __edg_oracle'+name+'\n' for name in ['__sin','__cos','__tan','__sinl','__cosl','__tanl']))
 (logs/'originals/fp_arch.h').write_text('/* Generic musl configuration. */\n')
 outputs=[]
 for original in (True,False):
  directory=source/'originals' if original else source;binary=out/('native-original' if original else 'native-adapted')
  run(['gcc','-std=c11','-O2','-fno-builtin','-ffp-contract=off','-Dhidden=','-include',shim,*([] if original else ['-DADAPTED']),fixture,*[directory/n for n in names],*[(logs/'originals' if original else logs)/n for n in lognames],'-lm','-o',binary]);outputs.append(run([binary]).stdout)
 assert outputs[0]==outputs[1] and outputs[0].strip().isdigit()
 checks=['Native original/adapted double exp/pow/trig/hyperbolic bit matrix and authored errno; exact source patches']
 if not translate:return checks
 selected={c['directory'] for c in json.loads((root/'backend/runtime/musl/provider.json').read_text())['components']};extra=[] if 'sqlite_math' in selected else [source/n for n in names]
 for language in ('c','c++'):
  for instance in (False,True):
   label=language.replace('+','x')+('-instance' if instance else '-static');directory=out/label
   run([sys.executable,root/'dotnet.py',fixture,*extra,'--libc','musl','--define','ADAPTED','--language',language,'--emit','managedlib','--namespace','','--class-name','Core','--split','size','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']};host=directory/'host';host.mkdir()
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   receiver='first' if instance else 'Core';set_=exports['sqlite_math_set_errno'];get_=exports['sqlite_math_errno'];errors=exports['sqlite_math_errors'];isolation=f'second.{set_}(721);if(first.{get_}()!=611)throw new Exception("owner errno");' if instance else ''
   (host/'Program.cs').write_text(f'''using System;using System.Threading.Tasks;class Program{{static void Main(){{
{('using var first=new Core();using var second=new Core();' if instance else '')}
if({receiver}.Run()!=0)throw new Exception("math matrix");{receiver}.{set_}(611);{isolation}
Task.Run(()=>{{try{{{receiver}.{set_}(811);if({receiver}.{errors}()!=0)throw new Exception("worker domain");}}finally{{{receiver}.ReleaseThread();}}}}).Wait();
GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();if({receiver}.{get_}()!=611)throw new Exception("TLS errno");
{receiver}.Shutdown();
}}}}''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);assert run(['dotnet',host/'bin/Release/net10.0/Host.dll']).stdout==outputs[0];checks.append(label+': JIT exact finite bits/signed zero, NaN classes, domain/pole errno and owner/TLS isolation')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-p:DebugType=none','-p:DebugSymbols=false','-o',host/'aot']);assert run([host/'aot/Host']).stdout==outputs[0];checks.append(label+': NativeAOT math matrix')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0
 def run(command,**kwargs):
  global index
  r=subprocess.run(list(map(str,command)),capture_output=True,text=True,**kwargs);prefix=out/f'command-{index:04d}';index+=1;prefix.with_suffix('.json').write_text(json.dumps(list(map(str,command))));prefix.with_suffix('.stdout').write_text(r.stdout);prefix.with_suffix('.stderr').write_text(r.stderr)
  if r.returncode:raise RuntimeError(f'{command}: {r.returncode}\n{r.stdout}\n{r.stderr}')
  return r
 root=Path(__file__).resolve().parents[2];checks=verify_libc_sqlite_math(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only);(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n');print(json.dumps(checks,indent=2))
