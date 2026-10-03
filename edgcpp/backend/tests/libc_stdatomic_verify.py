"""C11 public atomics and direct qualified accesses against EDG C and managed threads."""
import argparse,json,os,shutil,subprocess,sys
from pathlib import Path

def verify_libc_stdatomic(root,tests,out,nativeaot,run,include=None):
 out.mkdir(parents=True,exist_ok=True);headers=out/'include';headers.mkdir()
 shutil.copy2((include or root/'backend/runtime/libc/include')/'stdatomic.h',headers/'stdatomic.h')
 checks=[]
 for source_name,entry,oracle_macro in [('libc_c11_atomic.c','c11_implicit_cases','C11_ATOMIC_ORACLE_MAIN'),('libc_stdatomic.c','stdatomic_cases','STDATOMIC_ORACLE_MAIN')]:
  source=out/source_name;shutil.copy2(tests/source_name,source)
  label=source.stem;lowered=out/(label+'.lowered.c')
  run([root/'build/bin/cpfe','--c11','--gnu_version','130300','-I',headers,'-I',root/'backend/runtime/libc/include','-D'+oracle_macro,'--gen_c_file_name',lowered,'-o',out/(label+'.il'),source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
  run(['gcc','-std=gnu11','-w',lowered,root/'build/lib/libC.a','-latomic','-o',out/label]);run([out/label]);checks.append(label+': EDG C backend native oracle')
 for instance in [False,True]:
  directory=out/('instance' if instance else 'static')
  run([sys.executable,root/'dotnet.py',out/'libc_c11_atomic.c',out/'libc_stdatomic.c','--language','c','--target','portable-lp64-binary80-v1','--libc','dotcc','-I',headers,'--emit','managedlib','--namespace','','--class-name','Core','--output-dir',directory,*(['--instance-methods'] if instance else [])])
  metadata=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
  def call(symbol):return ('owner.' if instance else 'Core.')+next(v['method'] for v in metadata['exports'] if v['symbol']==symbol)
  (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
  other_check=('using(var other=new Core()){if(other.'+call('c11_counter').split('.',1)[1]+'()!=0||other.'+call('stdatomic_counter').split('.',1)[1]+'()!=0)throw new Exception("owner atomic state");}') if instance else ''
  (host/'Program.cs').write_text(f'''using System;using System.Threading;unsafe class Program{{
static void Check(int code,string name){{if(code!=0)throw new Exception($"{{name}}:{{code}}");}}
static void Main(){{{('using var owner=new Core();' if instance else '')}
Check({call('c11_implicit_cases')}(),"implicit");Check({call('stdatomic_cases')}(),"public");
{call('c11_reset')}();{call('stdatomic_reset')}();Exception? failure=null;Thread[] threads=new Thread[4];
for(int i=0;i<threads.Length;++i){{threads[i]=new Thread(()=>{{try{{for(int n=0;n<5000;++n){{{call('c11_increment')}();{call('stdatomic_increment')}();}}}}catch(Exception error){{Interlocked.CompareExchange(ref failure,error,null);}}}});threads[i].Start();}}
foreach(var thread in threads)if(!thread.Join(30000))throw new Exception("atomic thread timeout");if(failure!=null)throw failure;
if({call('c11_counter')}()!=20000||{call('stdatomic_counter')}()!=20000)throw new Exception("lost atomic increments");
for(int n=0;n<1000;++n){{{call('stdatomic_increment')}();}}
long allocation=GC.GetAllocatedBytesForCurrentThread();for(int n=0;n<20000;++n){{{call('stdatomic_increment')}();}}
if(GC.GetAllocatedBytesForCurrentThread()!=allocation)throw new Exception("public integer atomic allocation");
{other_check}{('owner.Shutdown();' if instance else '')}Console.WriteLine("C11 scalar/pointer/binary80/aggregate/NaN/public APIs and concurrent increments passed");}}}}
''')
  run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT C11 semantic and threaded contracts')
  if nativeaot:
   run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT C11 semantic and threaded contracts')
 cpp=out/'include.cpp';cpp.write_text('#include <stdatomic.h>\nint main(){bool value=true;return value?0:1;}\n');directory=out/'cpp-include'
 run([sys.executable,root/'dotnet.py',cpp,'--language','c++','--target','portable-lp64-binary80-v1','--libc','dotcc','-I',headers,'--output-dir',directory])
 run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',directory/'bin/Release/net10.0/Generated.dll']);checks.append('C++17 compatibility include: JIT')
 if nativeaot:
  run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);run([directory/'aot/Generated']);checks.append('C++17 compatibility include: NativeAOT')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--include',type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];index=0
 def run(command,**kwargs):
  global index
  command=list(map(str,command));n=index;index+=1;proc=subprocess.run(command,capture_output=True,text=True,timeout=420,**kwargs);(out/f'{n:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{n:03}.log').write_text(proc.stdout+proc.stderr)
  if proc.returncode:print(proc.stdout+proc.stderr)
  proc.check_returncode();return proc
 try:checks=verify_libc_stdatomic(root,root/'backend/tests',out,a.nativeaot,run,a.include)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
