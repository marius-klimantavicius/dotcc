"""Caller-frame materialization of addressable aggregate prvalues."""
import json
import os
from pathlib import Path
import re
import shutil
import sys


def verify_materialize(root,tests,out,nativeaot,run):
    source=out/'materialize-oracle.cpp'
    source.write_text('''#include <materialize.cpp>
extern "C" int printf(const char*,...);
void hook(){}
int main(){for(int i=0;i<18;++i){int n=run_materialize(i,hook);printf("%d:%d:%d:%d:%d\\n",i,n,materialize_calls,materialize_live,materialize_destroyed);}}
''')
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    c_source=out/'materialize-oracle.c'
    run([root/'build/bin/cpfe-dot-oracle','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'materialize-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'materialize-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout;(out/f'materialize-oracle-{label}.txt').write_text(trace);traces.append(trace)
    assert traces[0]==traces[1],traces
    expected=traces[0];rows=[tuple(map(int,line.split(':'))) for line in expected.splitlines()]
    assert len(rows)==18 and all(row[3]==0 for row in rows),rows
    assert rows[3][1]==1 and rows[11][4]==1 and rows[15][4]==3,rows
    assert rows[16][1:]==(51,3,0,1) and rows[17][1:]==(54,2,0,1),rows
    checks=['aggregate materialization: 18 value/effect/lifetime traces match EDG C/GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'materialize-{mode}'
        result=run([sys.executable,root/'dotnet.py',tests/'materialize.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','2400',
             *(['--instance-methods'] if instance else ['--no-nest-types'])],env=dict(os.environ,EDG_DOT_TRACE='1'))
        (directory/'lowered-il.txt').write_text(result.stderr)
        generated='\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
        helpers=re.findall(r'\bprivate\s+static\s+\w+\*\s+(__edg_materialize\d+)\([^)]*\)\s*\{([^}]+)\}',generated)
        assert helpers and all(re.sub(r'\s+','',body)=='*storage=value;returnstorage;' for _,body in helpers),helpers
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function):
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(f'_Z{len(function)}{function}')]
            assert len(names)==1,(function,names)
            return ('owner' if instance else 'Core')+'.'+names[0]
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  {('static readonly Core owner=new Core();' if instance else '')}
  static void Collect({'Core current' if instance else ''}) {{
    int* occupied=stackalloc int[256];for(int i=0;i<256;++i)occupied[i]=i;
    GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    if(occupied[255]!=255)throw new Exception("stack churn");
  }}
  static int Main() {{
    for(int i=0;i<18;++i){{int n={method('run_materialize')}(i,&Collect);
      Console.WriteLine($"{{i}}:{{n}}:{{{method('read_materialize_calls')}()}}:{{{method('read_materialize_live')}()}}:{{{method('read_materialize_destroyed')}()}}");}}
    return 0;
  }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        commands=[('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host']))
        for runtime,command in commands:
            actual=run(command,timeout=90).stdout;(host/f'{runtime}-trace.txt').write_text(actual)
            assert actual==expected,(mode,runtime,expected,actual)
            checks.append(f'aggregate materialization/{mode}/{runtime}: 18 C traces, nested/conditional/comma returns, simultaneous temporaries, loop reuse, pointer callbacks, GC/stack churn and EH cleanup')
    checks.extend(verify_c_materialize(root,tests,out,nativeaot,run))
    return checks


def verify_c_materialize(root,tests,out,nativeaot,run):
    source=out/'materialize-c-oracle.c'
    source.write_text('''#include <materialize.c>
extern int printf(const char*,...);
void hook(void){}
int main(void){for(int i=0;i<5;++i)printf("%d:%d\\n",i,run_materialize_c(i,hook));}
''')
    traces=[]
    for name in ('gcc','clang'):
        candidates=[os.environ.get('EDG_TEST_CLANG','clang')] if name=='clang' else ['gcc']
        if name=='clang':candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang')))
        compiler=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
        if not compiler:raise RuntimeError(name+' required for the C materialization oracle')
        executable=out/('materialize-c-'+name)
        run([compiler,'-std=gnu11','-O2','-I',tests,source,'-o',executable])
        trace=run([executable]).stdout
        (out/('materialize-c-'+name+'.txt')).write_text(trace)
        traces.append(trace)
    assert traces[0]==traces[1]=='0:55\n1:55\n2:220\n3:1255\n4:112355\n',traces
    checks=['C aggregate array-decay: GCC/Clang native oracles agree for no-local, ordinary-local and aligned-local cases']
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/('materialize-c-'+mode)
        run([sys.executable,root/'dotnet.py',tests/'materialize.c','--language','c','--std','c11',
             '--emit','managedlib','--output-dir',directory,'--namespace','','--class-name','Core',
             *(['--instance-methods'] if instance else [])])
        generated='\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
        assert '__edg_materialize' in generated
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  {('static readonly Core owner=new Core();' if instance else '')}
  static void Collect({'Core current' if instance else ''}) {{
    int* occupied=stackalloc int[256];for(int i=0;i<256;++i)occupied[i]=i;
    GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    if(occupied[255]!=255)throw new Exception("stack churn");
  }}
  static void Main() {{
    for(int i=0;i<5;++i)Console.WriteLine($"{{i}}:{{{('owner' if instance else 'Core')}.run_materialize_c(i,&Collect)}}");
  }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        commands=[('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host']))
        for runtime,command in commands:
            actual=run(command,timeout=90).stdout
            (host/(runtime+'-trace.txt')).write_text(actual)
            assert actual==traces[0],(mode,runtime,traces[0],actual)
            checks.append(f'C aggregate array-decay/{mode}/{runtime}: simultaneous returned arrays survive pointer callback, GC/stack churn and ordinary/aligned local sentinels')
    return checks


if __name__=='__main__':
    import argparse
    import hashlib
    import subprocess
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    parser.add_argument('--c-only',action='store_true',help='Run only C aggregate-array frame overlap controls')
    args=parser.parse_args()
    root=Path(__file__).resolve().parents[2]
    tests=Path(__file__).resolve().parent
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    inputs=[Path(__file__).resolve(),tests/'materialize.cpp',tests/'materialize.c',
            root/'build/bin/cpfe-dot',root/'build/bin/cpfe-dot-oracle']
    report={'status':'running','commands':[],'checks':[],
            'inputSha256':{str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs}}
    def save():
        (out/'results.json').write_text(json.dumps(report,indent=2)+'\n')
    def run(command,env=None,timeout=900):
        command=list(map(str,command));index=len(report['commands'])
        environment=dict(os.environ if env is None else env,DOTNET_PROCESSOR_COUNT='4')
        result=subprocess.run(command,capture_output=True,text=True,env=environment,timeout=timeout)
        (out/f'{index:03}.stdout').write_text(result.stdout)
        (out/f'{index:03}.stderr').write_text(result.stderr)
        report['commands'].append({'command':command,'exitCode':result.returncode})
        save();print(f'{index:03}: exit {result.returncode}',flush=True)
        if result.returncode:raise RuntimeError(f'Command {index} failed; see retained logs')
        return result
    try:
        report['checks']=(verify_c_materialize if args.c_only else verify_materialize)(root,tests,out,args.nativeaot,run)
        assert report['inputSha256']=={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs}
        report['status']='passed'
    except Exception as error:
        report.update(status='failed',error=str(error));raise
    finally:save()
