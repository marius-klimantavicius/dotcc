"""Native and translated standard clock(), shared host CPU time and errno."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_cpu_clock(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 (out/'native.c').write_text('int cpu_clock_cases(void);int main(void){return cpu_clock_cases();}\n')
 run(['gcc','-std=c11','-O2','-fno-builtin',tests/'libc_cpu_clock.c',out/'native.c','-o',out/'native']);run([out/'native']);checks.append('native C process CPU clock')
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',tests/'libc_cpu_clock.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','12000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner+'.' if instance else 'Core.')+next(x['method'] for x in manifest['exports'] if x['symbol']==symbol)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   isolated=f'using var other=new Core();long a={call("cpu_clock_sample")}(),b={call("cpu_clock_sample","other")}(),c={call("cpu_clock_sample")}();if(a>b||b>c)throw new Exception("process CPU clock belongs to common host");' if instance else ''
   (host/'Program.cs').write_text(f'''using System;using System.Diagnostics;using System.Threading;unsafe class Program{{
static long CpuTicks(){{var usage=Environment.CpuUsage;return (long)(((Int128)usage.UserTime.Ticks+usage.PrivilegedTime.Ticks)/10);}}
static void Main(){{
{('using var owner=new Core();' if instance else '')}
if({call('cpu_clock_cases')}()!=0)throw new Exception("native clock cases");
for(int i=0;i<1000;++i){{long before=CpuTicks(),sample={call('cpu_clock_sample')}(),after=CpuTicks();if(sample<before||sample>after)throw new Exception("BCL CPU brackets");}}
long initial={call('cpu_clock_sample')}();var timer=Stopwatch.StartNew();long current;do{{{call('cpu_clock_work')}();current={call('cpu_clock_sample')}();}}while(current==initial&&timer.ElapsedMilliseconds<5000);if(current<=initial)throw new Exception("CPU work not observed");
if({call('cpu_clock_errno')}(71)!=71)throw new Exception("success errno");{isolated}
Exception? failure=null;Thread worker=new(()=>{{try{{if({call('cpu_clock_errno')}(91)!=91)throw new Exception("TLS errno");{('owner' if instance else 'Core')}.ReleaseThread();}}catch(Exception e){{failure=e;}}}});worker.Start();worker.Join();if(failure!=null)throw failure;
{('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("standard C clock, indirect calls, BCL bounds and shared process/owner TLS passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT process CPU clock')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT process CPU clock')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_cpu_clock(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
