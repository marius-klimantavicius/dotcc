"""Dynamic-spec fatal boundaries, unexpected handlers and payload cleanup."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys
from libcxx_exception_ptr_verify import write_exception_fixture

EVENTS={0:['10/0','-10/1','100/0','900/0'],
        1:['20/0','-20/1','100/0','900/0'],
        2:['20/0','-20/1','100/0','900/0'],
        3:['30/0','-30/1','100/0','31/1','32/1','900/0'],
        4:['40/0','-40/1','100/0','41/1','42/1','900/0']}


def verify_libcxx_dynamic_specs_fatal(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    checks=[]
    def fatal(command,directory,label,which,oracle=False):
        def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
        command=list(map(str,command))
        try:result=subprocess.run(command,text=True,capture_output=True,preexec_fn=no_core,timeout=60)
        except subprocess.TimeoutExpired as error:
            (directory/(label+'.stdout')).write_bytes(error.stdout or b'')
            (directory/(label+'.stderr')).write_bytes(error.stderr or b'')
            (directory/(label+'.json')).write_text(json.dumps({'command':command,'timeout':60},indent=2));raise
        (directory/(label+'.stdout')).write_text(result.stdout);(directory/(label+'.stderr')).write_text(result.stderr)
        (directory/(label+'.json')).write_text(json.dumps({'command':command,'exit':result.returncode},indent=2))
        expected=EVENTS[which]+([] if oracle else ['901/1'])
        actual=[line for line in result.stdout.splitlines() if '/' in line]
        assert result.returncode!=0 and actual==expected,(label,result.returncode,expected,actual,result.stderr[:1000])
        if not oracle:
            reason=('C++ terminate' if which==1 else 'Destructor threw during unexpected replacement' if which in (3,4)
                    else 'Unexpected handler threw a forbidden exception')
            assert reason in result.stderr,(label,reason,result.stderr[:1000])
        checks.append(label+': unexpected/terminate owner, count, current_exception and destructor order')
    source=out/'oracle.cpp'
    source.write_text('#include '+json.dumps(str((tests/'libcxx/dynamic_specs_fatal.cpp').resolve()))+'\n'+'''
extern "C" int printf(const char*,...);extern "C" int fflush(void*);
static void report(int a,int b){printf("%d/%d\\n",a,b);fflush(nullptr);}
int main(int argc,char**argv){dynamic_fatal_install(report,argv[1][0]-'0');dynamic_fatal_case();}
''')
    include=out/'edg-oracle-include';include.mkdir()
    for name in ('exception','edg_internal_defines'):
        (include/name).write_bytes((root/'upstream/include_c++'/(name+'.stdh')).read_bytes())
    run([root/'build/bin/cpfe','--c++14','--gnu_version','130300','-I',include,'-DEDG_DYNAMIC_C_ORACLE=1',
         '--gen_c_file_name',out/'oracle.c','-o',out/'oracle.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    clang=shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))),None)
    if not clang:raise RuntimeError('Clang required for dynamic-spec fatal oracle')
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        executable=out/('edg-'+label)
        run([compiler,'-x','c','-w','-std=gnu11',out/'oracle.c','-x','none',root/'build/lib/libC.a','-lm','-o',executable])
        # Original runtime has no replacement-boundary cleanup entry for the
        # original payload (case 3). See the separately retained lifetime audit.
        for which in (0,1,2,4):fatal([executable,str(which)],out,f'edg-{label}-{which}',which,True)
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/mode;combined=out/(mode+'.cpp')
        write_exception_fixture(root,tests/'libcxx/dynamic_specs_fatal.cpp',library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++14','--emit','managedlib',
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
    {('if(!ReferenceEquals(expected,callbackOwner))what=-997;' if instance else '')}
    Console.WriteLine(what+"/"+value);Console.Out.Flush();
  }}
  static void Main(string[] args) {{
    {('var owner=new Core();expected=owner;' if instance else '')}
    {receiver}.{exports['dynamic_fatal_install']}(&Report,int.Parse(args[0]));
    {('var other=new Core();other.'+exports['dynamic_fatal_install']+'(&Report,0);' if instance else '')}
    {receiver}.{exports['dynamic_fatal_case']}();
    throw new Exception("dynamic specification failure returned");
  }}
}}''')
        project=host/'Host.csproj';run(['dotnet','build',project,'-c','Release'])
        for which in range(5):fatal(['dotnet',host/'bin/Release/net10.0/Host.dll',str(which)],directory,f'{mode}-jit-{which}',which)
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            for which in range(5):fatal([host/'aot/Host',str(which)],directory,f'{mode}-aot-{which}',which)
    return checks
