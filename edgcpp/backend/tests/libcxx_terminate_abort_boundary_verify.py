"""First-pass containment of a hostile source abort provider and exact source ABI checks."""
import argparse,json,resource,subprocess,sys
from pathlib import Path

def verify_libcxx_terminate_abort_boundary(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 for label,declaration in [('return','extern "C" int abort(void){return 0;}'),('argument','extern "C" void abort(int){}'),('variadic','extern "C" void abort(int,...){}')]:
  source=out/(label+'.cpp');source.write_text('extern "C" void __edg_cxx_terminate_default();\n'+declaration+'\nvoid run(){__edg_cxx_terminate_default();}\n')
  command=[sys.executable,root/'dotnet.py',source,'--libc','dotcc','--emit','managedlib','--output-dir',out/label];r=subprocess.run(list(map(str,command)),capture_output=True,text=True)
  (out/(label+'.command.json')).write_text(json.dumps(list(map(str,command))));(out/(label+'.log')).write_text(r.stdout+r.stderr)
  assert r.returncode!=0 and 'source abort ABI' in r.stderr,(label,r.returncode,r.stderr);checks.append(label+': source abort ABI rejected')
 for instance in [False,True]:
  directory=out/('instance' if instance else 'static')
  run([sys.executable,root/'dotnet.py',tests/'libcxx/terminate_abort_boundary.cpp',root/'backend/runtime/libc/threads/threads.c','--language','c++','--std','c++17','--libc','dotcc','--target','portable-lp64-binary80-v1','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','16000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
  methods={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']};host=directory/'host';host.mkdir();receiver='owner' if instance else 'Core';params='Core owner' if instance else ''
  (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
  (host/'Program.cs').write_text('''using System;unsafe class Program{
 static void Foreign('''+params+'''){try{throw new InvalidOperationException("foreign abort provider");}finally{Console.WriteLine("BAD_FOREIGN_FINALLY");}}
 static void Cleanup('''+params+'''){Console.WriteLine("BAD_SOURCE_CLEANUP");}
 static void Main(string[]args){'''+('var owner=new Core();' if instance else '')+'''try{'''+receiver+'.'+methods['terminate_abort_boundary']+'''(int.Parse(args[0]),&Foreign,&Cleanup);}catch{Console.WriteLine("BAD_HOST_CATCH");}finally{Console.WriteLine("BAD_HOST_FINALLY");}Console.WriteLine("BAD_RETURN");}}
''')
  run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
  for prefix,label in [(['dotnet',host/'bin/Release/net10.0/Host.dll'],'jit')]+([([host/'aot/Host'],'aot')] if nativeaot else []):
   if label=='aot':run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
   for mode,reason in [(0,'Default C++ terminate abort failed'),(1,'Nonlocal jump from C++ terminate handler'),(2,'Thread exit from C++ terminate handler')]:
    def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
    command=[*map(str,prefix),str(mode)];r=subprocess.run(command,capture_output=True,text=True,timeout=30,preexec_fn=no_core);(out/(directory.name+'-'+label+'-'+str(mode)+'.json')).write_text(json.dumps({'command':command,'exit':r.returncode,'stdout':r.stdout,'stderr':r.stderr},indent=2))
    assert r.returncode!=0 and 'BAD_' not in r.stdout and reason in r.stderr,(directory.name,label,mode,r.returncode,r.stdout,r.stderr)
   checks.append(directory.name+'-'+label+': fatal foreign, longjmp and C11 thread-exit filters run before any cleanup')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=480);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libcxx_terminate_abort_boundary(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
