"""Process termination ordering, handler addresses, streams and live workers."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_process_exit(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);checks=[];fixture=out/'fixture.cpp';shutil.copy2(tests/'libc_process_exit.cpp',fixture)
 def expected(language,mode):
  normal=('M' if mode in [0,4,6] and language=='c++' else '')+('T' if language=='c++' else '')+('R'*40 if mode==4 else '')+'BCA'+('G' if language=='c++' else '')
  stderr='yzx' if mode==2 else '' if mode==3 else normal
  status=18 if mode in [1,5] else 19 if mode==2 else 20 if mode==3 else 17
  return status,stderr,b'' if mode in [2,3] else ('body:'+normal).encode()
 def child(prefix,directory,language):
  directory.mkdir(parents=True,exist_ok=True)
  for mode in range(7):
   output=directory/f'{mode}.bin';command=list(map(str,prefix+[str(mode),output]));r=subprocess.run(command,capture_output=True,text=True,timeout=30)
   (directory/f'{mode}.command.json').write_text(json.dumps(command));(directory/f'{mode}.stdout').write_text(r.stdout);(directory/f'{mode}.stderr').write_text(r.stderr)
   status,stderr,data=expected(language,mode)
   assert r.returncode==status and r.stdout=='' and r.stderr==stderr and output.read_bytes()==data,(directory,mode,r.returncode,r.stdout,r.stderr,stderr,output.read_bytes())
 for cc,language in [('gcc','c'),('g++','c++')]:
  run([cc,'-x',language,'-std='+('c11' if language=='c' else 'c++17'),'-pthread',fixture,'-o',out/cc]);child([out/cc],out/(cc+'-oracle'),language);checks.append(language+': native seven-mode exit oracle')
 if not translate:return checks
 for language in ['c','c++']:
  for instance in [False,True]:
   label=language.replace('+','x')+'-'+('instance' if instance else 'static');directory=out/label
   run([sys.executable,root/'dotnet.py',fixture,root/'backend/runtime/libc/threads/threads.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--namespace','','--class-name','Core','--split','function','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   assert json.loads((directory/'dot-gen.json').read_text())['processExit']=={'mode':'process-wide','entryPoint':'normal-exit','initialThread':'logical-completion-v1'}
   run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);child(['dotnet',directory/'bin/Release/net10.0/Generated.dll'],directory/'jit-cases',language);checks.append(label+': JIT seven process-exit modes')
   if nativeaot:
    run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);child([directory/'aot/Generated'],directory/'aot-cases',language);checks.append(label+': NativeAOT seven process-exit modes')
   library=out/(label+'-managed')
   run([sys.executable,root/'dotnet.py',fixture,root/'backend/runtime/libc/threads/threads.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','function','--output-dir',library,*(['--instance-methods'] if instance else [])])
   assert json.loads((library/'dot-gen.json').read_text())['processExit']=={'mode':'process-wide','entryPoint':'host-controlled','initialThread':None}
   host=library/'host';host.mkdir();(host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   main=('using var first=new Core();using var second=new Core();if(first.Run(new[]{"synthetic","0",args[0]})!=17||second.Run(new[]{"synthetic","0",args[1]})!=17)throw new Exception("Run status");Console.Error.Write("H");first.ReleaseThread();first.Shutdown();second.ReleaseThread();second.Shutdown();' if instance else 'if(Core.Run(new[]{"synthetic","0",args[0]})!=17)throw new Exception("Run status");Console.Error.Write("H");Core.ReleaseThread();Core.Shutdown();')
   (host/'Program.cs').write_text('using System;class Program{static void Main(string[]args){'+main+'Console.Error.Write("D");}}')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
   for prefix,runtime in [(['dotnet',host/'bin/Release/net10.0/Host.dll'],'jit')]+([([host/'aot/Host'],'aot')] if nativeaot else []):
    if runtime=='aot':run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
    outputs=[host/(runtime+'-one.bin'),host/(runtime+'-two.bin')];r=subprocess.run(list(map(str,prefix+outputs)),capture_output=True,text=True,timeout=30);(host/(runtime+'.stdout')).write_text(r.stdout);(host/(runtime+'.stderr')).write_text(r.stderr)
    count=2 if instance else 1;tail=('T' if language=='c++' else '')+'KBCA'+('G' if language=='c++' else '');stderr=('M'*count if language=='c++' else '')+'H'+tail*count+'D';data=('body:'+('M' if language=='c++' else '')+tail).encode()
    assert r.returncode==0 and r.stderr==stderr and all(path.read_bytes()==data for path in outputs[:count]),(label,runtime,r.returncode,r.stdout,r.stderr,stderr)
    checks.append(label+': '+runtime+' managed Run returns and explicit owner cleanup')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];index=0
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360);(out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_process_exit(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
