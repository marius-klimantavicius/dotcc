"""Selected musl PRNG exact sequences, state switching, concurrency and owners."""
import argparse,hashlib,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_prng(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);source=out/'prng';shutil.copytree(root/'backend/runtime/musl/prng',source)
 fixture=out/'fixture.c';shutil.copy2(tests/'libc_prng.c',fixture)
 metadata=json.loads((source/'source.json').read_text());names=[n for n in metadata['sources'] if n.endswith('.c')]
 for name,item in metadata['sources'].items():
  assert hashlib.sha256((source/name).read_bytes()).hexdigest()==item['adaptedSha256']
  assert hashlib.sha256((source/'originals'/name).read_bytes()).hexdigest()==item['originalSha256']
 replay=out/'replay';shutil.copytree(source/'originals',replay)
 for name,item in metadata['sources'].items():
  if 'patch' in item:
   assert hashlib.sha256((source/item['patch']).read_bytes()).hexdigest()==item['patchSha256']
   run(['patch','--batch','--fuzz=0','-p1','-i',source/item['patch']],cwd=replay)
  assert (replay/name).read_bytes()==(source/name).read_bytes()
 # Native reference retains original algorithms and provides only lock plumbing.
 (source/'originals/lock.h').write_text((source/'prng_lock.h').read_text())
 (source/'originals/fork_impl.h').write_text('/* No fork in this synthetic source component. */\n')
 results=[]
 for original in (True,False):
  label='original' if original else 'adapted';native=out/label;directory=source/'originals' if original else source
  run(['gcc','-std=c11','-O2','-D_DEFAULT_SOURCE','-Dhidden=','-fno-builtin','-pthread',fixture,*[directory/n for n in names],'-o',native])
  result=run([native]);results.append(result.stdout)
 assert results[0]==results[1] and results[0].strip().isdigit()
 checks=['Native original/adapted exact PRNG streams and state switching; zero-fuzz patch replay']
 if not translate:return checks
 selected={c['directory'] for c in json.loads((root/'backend/runtime/musl/provider.json').read_text())['components']}
 extra=[] if 'prng' in selected else [source/n for n in names]
 for language in ('c','c++'):
  for instance in (False,True):
   label=language.replace('+','x')+('-instance' if instance else '-static');directory=out/label
   run([sys.executable,root/'dotnet.py',fixture,*extra,'--libc','musl','--language',language,'--emit','managedlib','--namespace','','--class-name','Core','--split','size','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   exports={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']}
   host=directory/'host';host.mkdir();(host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   receiver='first' if instance else 'Core';seed=exports['prng_seed'];next_=exports['prng_next'];rnd=exports['prng_random'];sum_=exports['prng_sum']
   isolation=f'''second.{seed}(17);if(second.{next_}()!=firstValue)throw new Exception("owner seed");first.{seed}(991);if(second.{next_}()!=secondValue)throw new Exception("owner progression");''' if instance else ''
   code=f'''using System;using System.Threading.Tasks;class Program{{static void Main(){{
{('using var first=new Core();using var second=new Core();' if instance else '')}
if({receiver}.Run()!=0)throw new Exception("fixture");
{receiver}.{seed}(17);int firstValue={receiver}.{next_}(),secondValue={receiver}.{next_}();
GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
{isolation}
{receiver}.{seed}(123);ulong expected={receiver}.{sum_}(4000);{receiver}.{seed}(123);
using var gate=new System.Threading.Barrier(4);
Task<ulong>[] tasks=new Task<ulong>[4];for(int i=0;i<tasks.Length;i++)tasks[i]=Task.Run(()=>{{gate.SignalAndWait();try{{return {receiver}.{sum_}(1000);}}finally{{{receiver}.ReleaseThread();}}}});
ulong actual=0;foreach(var task in tasks)actual+=task.Result;if(actual!=expected)throw new Exception("concurrent random sequence");
{receiver}.Shutdown();
}}}}'''
   (host/'Program.cs').write_text(code)
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);r=run(['dotnet',host/'bin/Release/net10.0/Host.dll']);assert r.stdout==results[0];checks.append(label+': JIT exact streams, independent owners, forced GC and concurrent random state')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);r=run([host/'aot/Host']);assert r.stdout==results[0];checks.append(label+': NativeAOT PRNG matrix')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0
 def run(command,**kwargs):
  global index
  r=subprocess.run(list(map(str,command)),capture_output=True,text=True,**kwargs);prefix=out/f'command-{index:04d}';index+=1;prefix.with_suffix('.json').write_text(json.dumps(list(map(str,command))));prefix.with_suffix('.stdout').write_text(r.stdout);prefix.with_suffix('.stderr').write_text(r.stderr)
  if r.returncode:raise RuntimeError(f'{command}: {r.returncode}\n{r.stdout}\n{r.stderr}')
  return r
 root=Path(__file__).resolve().parents[2];checks=verify_libc_prng(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only);(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n');print(json.dumps(checks,indent=2))
