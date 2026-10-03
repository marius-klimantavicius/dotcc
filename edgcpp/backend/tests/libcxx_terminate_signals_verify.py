"""Default C++ terminate reaches source abort while deliberate fatal boundaries stay unchanged."""
import argparse,json,os,resource,subprocess,sys
from pathlib import Path
from libcxx_exception_ptr_verify import write_exception_fixture

def verify_libcxx_terminate_signals(root,tests,out,nativeaot,run,acquisition):
 out.mkdir(parents=True,exist_ok=True);checks=[];acquisition=Path(acquisition).resolve();library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
 source=tests/'libcxx/terminate_signals.cpp';native=out/'native.cpp'
 native.write_text('#include '+json.dumps(str(source.resolve()))+'\nint main(int argc,char**argv){terminate_signal_install(70);terminate_signal_case(argc>1?atoi(argv[1]):0);}\n')
 def child(command,label,expected=None,reason=None):
  def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
  command=list(map(str,command));r=subprocess.run(command,capture_output=True,text=True,timeout=40,preexec_fn=no_core)
  (out/(label+'.json')).write_text(json.dumps({'command':command,'exit':r.returncode,'stdout':r.stdout,'stderr':r.stderr},indent=2))
  assert r.returncode==expected if expected is not None else r.returncode!=0,(label,r.returncode,r.stdout,r.stderr)
  assert 'HOST_CAUGHT' not in r.stdout and 'HOST_FINALLY' not in r.stdout,(label,r.stdout)
  if reason:assert reason in r.stderr,(label,r.stderr)
 run(['g++','-std=c++17','-O2',native,'-o',out/'native'])
 for scenario in range(5):child([out/'native',str(scenario)],'native-'+str(scenario),71 if scenario==3 else 73 if scenario==4 else 70)
 checks.append('native C++ default/direct/restored terminate, active exception, wrapper then abort')
 run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-DEDG_TERMINATE_C_ORACLE=1','--gen_c_file_name',out/'edg-oracle.c','-o',out/'edg-oracle.il',native],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
 run(['gcc','-x','c','-w','-std=gnu11',out/'edg-oracle.c','-x','none',root/'build/lib/libC.a','-lm','-o',out/'edg-oracle'])
 for scenario in range(5):child([out/'edg-oracle',str(scenario)],'edg-oracle-'+str(scenario),73 if scenario==4 else 70)
 checks.append('original EDG C/runtime: default/direct/restored/noexcept/wrapper SIGABRT delivery (no exception_ptr introspection)')
 for instance in [False,True]:
  directory=out/('instance' if instance else 'static');combined=out/(directory.name+'.cpp');write_exception_fixture(root,source,library,combined)
  run([sys.executable,root/'dotnet.py',combined,'--libc','musl','--std','c++20','--target','portable-lp64-binary80-v1','--emit','managedlib','-I',acquisition/'config','-I',library/'include','-I',library/'src','--namespace','','--class-name','Core','--split','size','--split-size','20000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
  methods={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']};host=directory/'host';host.mkdir();receiver='owner' if instance else 'Core'
  (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
  (host/'Program.cs').write_text('''using System;unsafe class Program{static void Main(string[]args){'''+('var owner=new Core();var other=new Core();other.'+methods['terminate_signal_install']+'(80);' if instance else '')+receiver+'.'+methods['terminate_signal_install']+'''(70);try{'''+receiver+'.'+methods['terminate_signal_case']+'''(int.Parse(args[0]));}catch{Console.WriteLine("HOST_CAUGHT");}finally{Console.WriteLine("HOST_FINALLY");}}}''')
  run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
  for prefix,label in [(['dotnet',host/'bin/Release/net10.0/Host.dll'],'jit')]+([([host/'aot/Host'],'aot')] if nativeaot else []):
   if label=='aot':run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
   for scenario in range(9):
    expected=(71 if scenario==3 else 73 if scenario==4 else 70) if scenario<5 else None
    reason={5:'C++ terminate',6:'C++ terminate handler threw',7:'Recursive C++ terminate',8:'Translated default signal action'}.get(scenario)
    child(prefix+[str(scenario)],directory.name+'-'+label+'-'+str(scenario),expected,reason)
   checks.append(directory.name+'-'+label+': default/direct/restored SIGABRT, current exception, owner isolation, preserved custom/internal fatal boundaries')
 return checks

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--acquisition',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);index=0;checks=[]
 def run(command,**kwargs):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=480,**kwargs);(out/f'{i:03}.command.json').write_text(json.dumps(command));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode();return r
 try:checks=verify_libcxx_terminate_signals(root,root/'backend/tests',out,a.nativeaot,run,a.acquisition)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2))
 print(json.dumps(checks,indent=2))
