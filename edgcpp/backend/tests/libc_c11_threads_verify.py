"""Public C11 threads, native oracle and owner/TLS/thread-exit managed contracts."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_c11_threads(root,tests,out,nativeaot,run,translate=True):
 out.mkdir(parents=True,exist_ok=True);checks=[]
 fixture=out/'fixture.c';shutil.copy2(tests/'libc_c11_threads.c',fixture)
 (out/'main.c').write_text('#ifdef __cplusplus\nextern "C"\n#endif\nint c11_thread_cases(void);\n#ifdef __cplusplus\nextern "C"\n#endif\nint c11_destructor_exit_cases(void);int main(void){int r=c11_thread_cases();return r?r:c11_destructor_exit_cases();}\n')
 for cc,language in [('gcc','c'),('g++','c++')]:
  run([cc,'-x',language,'-std='+('c11' if language=='c' else 'c++17'),'-pthread',fixture,out/'main.c','-o',out/cc]);run([out/cc]);checks.append(language+': native C11 thread oracle')
 if not translate:return checks
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,root/'backend/runtime/libc/threads/threads.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','-D','EDG_C11_EXIT_INTEROP','--emit','managedlib','--namespace','','--class-name','Core','--split','function','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   metadata=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
   def call(symbol,owner='owner'):return (owner if instance else 'Core')+'.'+next(v['method'] for v in metadata['exports'] if v['symbol']==symbol)
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   isolated=('using(var other=new Core()){if('+call('c11_thread_count','other')+'()!=0)throw new Exception("initial owner state");Check('+call('c11_thread_cases','other')+'(),"other owner");Check('+call('c11_mixed_cases','other')+'(),"other owner C++ exit");}') if instance else ''
   (host/'Program.cs').write_text(f'''using System;unsafe class Program{{static void Check(int result,string label){{if(result!=0)throw new Exception(label+":"+result);}}static void Main(){{{('using var owner=new Core();' if instance else '')}
Check({call('c11_thread_cases')}(),"C11 API");Check({call('c11_destructor_exit_cases')}(),"exit from TSS destructor");int mixed={call('c11_mixed_cases')}();Console.WriteLine("mixed="+mixed+" trace="+{call('c11_mixed_diagnostic')}()+" flags="+{call('c11_mixed_flags')}());Check(mixed,"C++ automatic/TLS/TSS/noexcept/catch exit");Check({call('c11_tls_exit_cases')}(),"C++ TLS-destructor reentrant exit");{isolated}
if({call('c11_thread_count')}()!=4800)throw new Exception("original owner state");{('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("public C11 threads, integer exit, owner isolation, TSS/TLS cleanup, synchronization and managed C++ interop passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT public C11 threads and managed exit contracts')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host']);checks.append(directory.name+': NativeAOT public C11 threads and managed exit contracts')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');p.add_argument('--native-only',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];index=0
 def run(command,**kwargs):
  global index
  command=list(map(str,command));n=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=360,**kwargs);(out/f'{n:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{n:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libc_c11_threads(root,root/'backend/tests',out,a.nativeaot,run,not a.native_only)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
