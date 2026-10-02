"""EDG runtime termination ordering and per-owner libc++ terminate handlers."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys
from libcxx_exception_ptr_verify import write_exception_fixture

CASES=[(0,0),(0,1),(0,2),*[(n,0) for n in range(1,8)]]
EVENTS={0:[900],1:[10,-10,900],2:[20,900],3:[30,900],4:[40,900],5:[50,901],6:[900],7:[900]}
COUNTS={0:[0],1:[0,1,1],2:[1,1],3:[1,1],4:[1,1],5:[1,0],6:[0],7:[0]}


def verify_libcxx_terminate(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    checks=[]
    def fatal(command,directory,label,which,mode,edg_oracle=False):
        def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
        result=subprocess.run(list(map(str,command)),text=True,capture_output=True,preexec_fn=no_core,timeout=30)
        (directory/(label+'.stdout')).write_text(result.stdout);(directory/(label+'.stderr')).write_text(result.stderr)
        (directory/(label+'.json')).write_text(json.dumps({'command':list(map(str,command)),'exit':result.returncode},indent=2))
        lines=[line for line in result.stdout.splitlines() if '/' in line]
        expected=[f'{event}/{-1 if edg_oracle and event>=900 else count}' for event,count in zip(EVENTS[which],COUNTS[which])]
        assert result.returncode!=0 and lines==expected,(label,result.returncode,expected,lines,result.stderr[:1000])
        if not edg_oracle:
            reason=('C++ terminate handler threw' if mode==1 else 'Recursive C++ terminate' if mode==2 else
                    {0:'C++ terminate',1:'Exception escaped noexcept function',2:'Destructor threw during unwinding',
                     3:'Exception escaped catch parameter initialization',4:'Exception escaped EDG throw copy',
                     5:'C++ terminate',6:'C++ rethrow without active exception',7:'C++ terminate'}[which])
            assert reason in result.stderr,(label,reason,result.stderr[:1000])
        checks.append(label+': correct terminate handler, uncaught count and destructor ordering')
    source=out/'terminate-oracle.cpp'
    source.write_text('#include '+json.dumps(str((tests/'libcxx/terminate_handlers.cpp').resolve()))+'\n'+'''extern "C" int printf(const char*,...);extern "C" int fflush(void*);
static void report(int event,int value){printf("%d/%d\\n",event,value);fflush(nullptr);}
int main(int argc,char**argv){terminate_install(report,argc>2?argv[2][0]-'0':0);terminate_case(argc>1?argv[1][0]-'0':0);return 0;}
''')
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-DEDG_TERMINATE_C_ORACLE=1','--gen_c_file_name',out/'terminate-oracle.c','-o',out/'terminate-oracle.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    clang=shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))),None)
    if not clang:raise RuntimeError('Clang is required for C-lowered terminate oracle')
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        executable=out/('edg-'+label)
        run([compiler,'-x','c','-w','-std=gnu11',out/'terminate-oracle.c','-x','none',root/'build/lib/libC.a','-lm','-o',executable])
        for which,mode in CASES:
            if which==7:continue # generic original runtime has no exception_ptr provider
            fatal([executable,str(which),str(mode)],out,f'edg-{label}-{which}-{mode}',which,mode,True)
    for instance in (False,True):
        variant='instance' if instance else 'static';directory=out/variant;combined=out/(variant+'.cpp')
        write_exception_fixture(root,tests/'libcxx/terminate_handlers.cpp',library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++20','--emit','managedlib',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src','--output-dir',directory,
             '--namespace','','--class-name','Core','--split','size','--split-size','3200',*(['--instance-methods'] if instance else [])])
        exports={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']}
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        receiver='owner' if instance else 'Core'
        (host/'Program.cs').write_text(f'''using System;
unsafe class Program {{
  {('static Core? expected;' if instance else '')}
  static void Report({('Core callbackOwner,' if instance else '')}int what,int value) {{
    {('if(!ReferenceEquals(expected,callbackOwner))what=-999;' if instance else '')}
    Console.WriteLine(what+"/"+value);Console.Out.Flush();
  }}
  static void Main(string[] args) {{
    {('var owner=new Core();expected=owner;' if instance else '')}
    {receiver}.{exports['terminate_install']}(&Report,int.Parse(args[1]));
    {('var other=new Core();other.'+exports['terminate_install']+'(&Report,0);' if instance else '')}
    {receiver}.{exports['terminate_case']}(int.Parse(args[0]));
    throw new Exception("terminate returned");
  }}
}}''')
        project=host/'Host.csproj';run(['dotnet','build',project,'-c','Release'])
        for which,mode in CASES:
            fatal(['dotnet',host/'bin/Release/net10.0/Host.dll',str(which),str(mode)],directory,f'{variant}-jit-{which}-{mode}',which,mode)
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            for which,mode in CASES:
                fatal([host/'aot/Host',str(which),str(mode)],directory,f'{variant}-aot-{which}-{mode}',which,mode)
    return checks
