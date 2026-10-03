"""Implicit C11 qualified accesses: native oracle, contention, exact width and allocations."""
import argparse,json,os,shutil,subprocess,sys
from pathlib import Path

def verify_c11_implicit_atomics(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);source=out/'atomic.c';shutil.copy2(tests/'libc_c11_atomic.c',source);checks=[]
 run([root/'build/bin/cpfe','--c11','--gnu_version','130300','-DC11_ATOMIC_ORACLE_MAIN','--gen_c_file_name',out/'lowered.c','-o',out/'oracle.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
 run(['gcc','-std=gnu11','-w',out/'lowered.c',root/'build/lib/libC.a','-latomic','-o',out/'oracle']);run([out/'oracle']);checks.append('EDG C backend implicit atomic oracle')
 for instance in [False,True]:
  directory=out/('instance' if instance else 'static')
  run([sys.executable,root/'dotnet.py',source,'--language','c','--target','portable-lp64-binary80-v1','--emit','managedlib','--namespace','','--class-name','Core','--output-dir',directory,*(['--instance-methods'] if instance else [])])
  metadata=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
  def method(symbol):return next(item['method'] for item in metadata['exports'] if item['symbol']==symbol)
  def call(symbol):return ('owner.' if instance else 'Core.')+method(symbol)
  other=('using(var other=new Core()){if(other.'+method('c11_counter')+'()!=0||other.'+method('c11_byte_guard')+'()!=1)throw new Exception("other owner state");}') if instance else ''
  (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
  (host/'Program.cs').write_text(f'''using System;using System.Threading;unsafe class Program{{static void Main(){{
{('using var owner=new Core();' if instance else '')}int result={call('c11_implicit_cases')}();if(result!=0)throw new Exception($"implicit case {{result}}");
{call('c11_reset')}();Exception? failure=null;Thread[] threads=new Thread[4];
for(int i=0;i<4;++i){{threads[i]=new Thread(()=>{{try{{for(int n=0;n<5000;++n){{{call('c11_increment')}();{call('c11_byte_increment')}();}}}}catch(Exception e){{Interlocked.CompareExchange(ref failure,e,null);}}}});threads[i].Start();}}
foreach(Thread thread in threads)if(!thread.Join(30000))throw new Exception("contention timeout");if(failure!=null)throw failure;
if({call('c11_counter')}()!=20000||{call('c11_byte_guard')}()!=1)throw new Exception("contention/neighbor bytes");
for(int n=0;n<1000;++n){{{call('c11_increment')}();{call('c11_byte_increment')}();}}
long allocation=GC.GetAllocatedBytesForCurrentThread();for(int n=0;n<20000;++n){{{call('c11_increment')}();{call('c11_byte_increment')}();}}
if(GC.GetAllocatedBytesForCurrentThread()!=allocation)throw new Exception("integer atomic allocation");if({call('c11_byte_guard')}()!=1)throw new Exception("neighbor guard");
{other}{('owner.Shutdown();' if instance else '')}Console.WriteLine("implicit scalar/pointer/float80/aggregate atoms, contention, byte guards and zero integer allocation passed");}}}}
''')
  run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT implicit C11 contracts')
  if nativeaot:
   run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT implicit C11 contracts')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];index=0
 def run(command,**kwargs):
  global index
  command=list(map(str,command));n=index;index+=1;proc=subprocess.run(command,capture_output=True,text=True,timeout=420,**kwargs);(out/f'{n:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{n:03}.log').write_text(proc.stdout+proc.stderr)
  if proc.returncode:print(proc.stdout+proc.stderr)
  proc.check_returncode();return proc
 try:checks=verify_c11_implicit_atomics(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
