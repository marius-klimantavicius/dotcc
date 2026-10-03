"""C/C++ assertion header and owner-aware process termination, JIT/NativeAOT."""
import argparse,json,resource,subprocess,sys
from pathlib import Path

def verify_libc_assert(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 def child(command,label,diagnostic=None):
  def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
  command=list(map(str,command));r=subprocess.run(command,capture_output=True,text=True,timeout=60,preexec_fn=no_core)
  (out/(label+'.command.json')).write_text(json.dumps(command));(out/(label+'.stdout')).write_text(r.stdout);(out/(label+'.stderr')).write_text(r.stderr)
  assert r.returncode!=0 and 'caught' not in r.stdout and 'unwound' not in r.stdout,(label,r.returncode,r.stdout,r.stderr)
  text=Path(diagnostic).read_text() if diagnostic else r.stderr
  assert 'side_effect == 7' in text and 'synthetic-assert.c' in text and '71' in text,(label,text)
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',tests/'libc_assert.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','12000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   def call(symbol):return ('owner.' if instance else 'Core.')+next(x['method'] for x in manifest['exports'] if x['symbol']==symbol)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   (host/'Program.cs').write_text(f'''using System;using System.Text;unsafe class Program{{static void Main(string[] args){{
{('using var owner=new Core();' if instance else '')}
if(args.Length!=0){{try{{if(args.Length==1){call('assertion_failure')}();else{{byte[]path=Encoding.UTF8.GetBytes(args[1]+"\\0");fixed(byte*p=path){call('assertion_redirect_failure')}((sbyte*)p);}}}}catch{{Console.WriteLine("caught");}}finally{{Console.WriteLine("unwound");}}throw new Exception("assert returned");}}
if({call('assertion_enabled')}()!=1||{call('assertion_disabled')}()!=0||{call('assertion_reenabled')}()!=1||{call('assertion_contextual_bool')}()!=1)throw new Exception("header contract");
Console.WriteLine("assert header enabled/disabled/reincluded/contextual bool passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
   for prefix,runtime in [(['dotnet',host/'bin/Release/net10.0/Host.dll'],'jit')]+([([host/'aot/Host'],'aot')] if nativeaot else []):
    if runtime=='aot':run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
    run(prefix);label=directory.name+'-'+runtime;child(prefix+['fail'],label+'-fatal');path=host/(runtime+'-stderr.txt');child(prefix+['redirect',path],label+'-redirect',path);checks.append(label+': header and uncatchable owner stderr assertion')
 return checks

def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  nonlocal index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
  (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_assert(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
