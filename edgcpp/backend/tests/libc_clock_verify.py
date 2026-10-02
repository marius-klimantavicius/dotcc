"""Native and translated C/C++ BCL clock imports including opaque ABI checks."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_clock(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 (out/'native.c').write_text('int clock_cases(void);int main(void){return clock_cases();}\n')
 run(['gcc','-D_POSIX_C_SOURCE=200809L','-std=c11','-O2','-fno-builtin',tests/'libc_clock.c',out/'native.c','-o',out/'native']);run([out/'native']);checks.append('native synthetic clock/time/difftime contract')
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',tests/'libc_clock.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','12000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(x['method'] for x in manifest['exports'] if x['symbol']==symbol)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   isolated=f'using var other=new Core();if({call("clock_set_error","other")}(91,0)!=91 || {call("clock_read_error")}()!=22)throw new Exception("owner errno isolation");' if instance else ''
   (host/'Program.cs').write_text(f'''using System;using System.Threading;unsafe class Program{{static void Main(){{
{('using var owner=new Core();' if instance else '')}
int status={call('clock_cases')}();if(status!=0)throw new Exception($"clock cases {{status}}");
long* value=stackalloc long[2];long before=DateTimeOffset.UtcNow.ToUnixTimeSeconds();if({call('clock_sample')}(0,value)!=0)throw new Exception("realtime");long after=DateTimeOffset.UtcNow.ToUnixTimeSeconds();if(value[0]<before || value[0]>after || value[1]<0 || value[1]>=1000000000L)throw new Exception("realtime host bounds");
long previousSec=0,previousNsec=0;for(int i=0;i<10000;++i){{if({call('clock_sample')}(1,value)!=0 || value[0]<previousSec || (value[0]==previousSec && value[1]<previousNsec))throw new Exception("monotonic loop");previousSec=value[0];previousNsec=value[1];}}
if({call('clock_set_error')}(71,-1)!=22)throw new Exception("invalid clock");{isolated}
Exception? failure=null;Thread worker=new(()=>{{try{{if({call('clock_set_error')}(91,0)!=91)throw new Exception("worker errno");{('owner' if instance else 'Core')}.ReleaseThread();}}catch(Exception e){{failure=e;}}}});worker.Start();worker.Join();if(failure!=null)throw failure;if({call('clock_read_error')}()!=22)throw new Exception("TLS errno isolation");
{('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("clock realtime/monotonic, native pointer calls, time/difftime and owner/TLS errno passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT clocks')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT clocks')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_clock(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
