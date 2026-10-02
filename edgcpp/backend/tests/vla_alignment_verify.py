"""Explicit VLA alignment, independent threads, and allocation-free bookkeeping."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_vla_alignment(root,tests,out,nativeaot,run):
 native=out/'native'
 run(['gcc','-std=c11','-DNATIVE_ORACLE',tests/'vla_alignment.c','-o',native]);run([native])
 checks=['VLA explicit alignment: native C oracle']
 for instance in (False,True):
  mode='instance' if instance else 'static';generated=out/mode
  run([sys.executable,root/'dotnet.py',tests/'vla_alignment.c','--language','c','--emit','managedlib','--namespace','','--class-name','Core','--output-dir',generated,*(['--instance-methods'] if instance else [])])
  manifest=json.loads((generated/'dot-gen.json').read_text());method=next(e['method'] for e in manifest['exports'] if e['symbol']=='vla_alignment')
  owner='owner' if instance else 'Core';host=generated/'host';host.mkdir()
  (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
  (host/'Program.cs').write_text('''using System;using System.Threading;using Edg.Runtime;
unsafe class Program {
 static void Work() {
 OWNER
 for(int i=1;i<=200;++i)if(CALL(i)!=0 || Vla.Mark()!=0)throw new Exception("VLA alignment/frame");
 for(int i=0;i<200;++i){void* p=null;Vla.Allocate(&p,31,64);Vla.Release(p);}
 long before=GC.GetAllocatedBytesForCurrentThread();
 for(int i=0;i<2000;++i){nint mark=Vla.Mark();void* a=null;void* b=null;Vla.Allocate(&a,0,256);Vla.Allocate(&b,64,4096);Vla.Release(a);Vla.Restore(mark);}
 if(GC.GetAllocatedBytesForCurrentThread()!=before)throw new Exception("VLA managed allocation");
 }
 static void Main(){Work();Exception? error=null;var t=new Thread(()=>{try{Work();}catch(Exception e){error=e;}});t.Start();Work();if(!t.Join(30000))throw new Exception("VLA worker timeout");if(error!=null)throw error;}
}'''.replace('OWNER','var owner=new Core();' if instance else '').replace('CALL',owner+'.'+method))
  run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo']);run(['dotnet',host/'bin/Release/net10.0/Host.dll'])
  checks.append(mode+'/JIT: 64/256/4096-byte alignment, TLS chains, zero managed bookkeeping allocations')
  if nativeaot:
   run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host'])
   checks.append(mode+'/NativeAOT: explicit alignment and native bookkeeping')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);commands=[]
 def run(command,**kwargs):
  command=list(map(str,command));r=subprocess.run(command,capture_output=True,text=True,**kwargs);i=len(commands)
  (out/f'{i:03d}.stdout').write_text(r.stdout);(out/f'{i:03d}.stderr').write_text(r.stderr);commands.append({'command':command,'exit':r.returncode});(out/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
  if r.returncode:raise RuntimeError(f'{command}: {r.returncode}\n{r.stdout}\n{r.stderr}')
  return r
 root=Path(__file__).resolve().parents[2];checks=verify_vla_alignment(root,Path(__file__).resolve().parent,out,a.nativeaot,run);(out/'results.json').write_text(json.dumps({'checks':checks},indent=2)+'\n');print(len(checks),'VLA alignment checks passed')
