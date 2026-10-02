"""Caller-frame materialization of addressable aggregate prvalues."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_materialize(root,tests,out,nativeaot,run):
    source=out/'materialize-oracle.cpp'
    source.write_text('''#include <materialize.cpp>
extern "C" int printf(const char*,...);
void hook(){}
int main(){for(int i=0;i<16;++i){int n=run_materialize(i,hook);printf("%d:%d:%d:%d:%d\\n",i,n,materialize_calls,materialize_live,materialize_destroyed);}}
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
    assert len(rows)==16 and all(row[3]==0 for row in rows),rows
    assert rows[3][1]==1 and rows[11][4]==1 and rows[15][4]==3,rows
    checks=['aggregate materialization: 16 value/effect/lifetime traces match EDG C/GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'materialize-{mode}'
        result=run([sys.executable,root/'dotnet.py',tests/'materialize.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','2400',
             *(['--instance-methods'] if instance else ['--no-nest-types'])],env=dict(os.environ,EDG_DOT_TRACE='1'))
        (directory/'lowered-il.txt').write_text(result.stderr)
        generated='\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
        helpers=[line for line in generated.splitlines() if '* __edg_materialize' in line]
        assert helpers and all('*storage=value;return storage;' in line for line in helpers),helpers
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
    for(int i=0;i<16;++i){{int n={method('run_materialize')}(i,&Collect);
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
            checks.append(f'aggregate materialization/{mode}/{runtime}: 16 C traces, nested/conditional/comma returns, simultaneous temporaries, loop reuse, pointer callbacks, GC/stack churn and EH cleanup')
    return checks
