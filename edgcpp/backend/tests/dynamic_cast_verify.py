"""Pointer RTTI traversals compared with EDG C, under JIT and NativeAOT."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_dynamic_cast(root,tests,out,nativeaot,run):
    source=out/'rtti-oracle.cpp'
    source.write_text('''#include <dynamic_cast.cpp>
extern "C" int printf(const char*,...);
void hook() {}
int main(){for(int i=0;i<42;++i){int n=run_case(i,hook);printf("%d:%d:%d:%llu\\n",i,n,read_live(),read_trace());}}
''')
    c_source=out/'rtti-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'rtti-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'rtti-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'rtti-oracle-{label}.txt').write_text(trace);traces.append(trace)
    assert traces[0]==traces[1],traces
    expected=traces[0]
    assert [tuple(map(int,line.split(':')[:3])) for line in expected.splitlines()]==[(i,1,0) for i in range(42)],expected
    checks=['pointer dynamic_cast: 42 EDG C result/lifetime traces agree under GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'rtti-{mode}'
        command=[sys.executable,root/'dotnet.py',tests/'dynamic_cast.cpp','--emit','managedlib',
                 '--output-dir',directory,'--namespace','','--class-name','Core','--split',
                 'size' if instance else 'function','--split-size','2400',
                 *(['--instance-methods'] if instance else ['--no-nest-types'])]
        run(command)
        before={p.name:p.read_bytes() for p in directory.glob('*.cs')}
        run(command)
        assert before=={p.name:p.read_bytes() for p in directory.glob('*.cs')}
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function,receiver='owner'):
            prefix=f'_Z{len(function)}{function}'
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(prefix)]
            assert len(names)==1,(function,names)
            return (receiver+'.' if instance else 'Core.')+names[0]
        host=directory/'host';host.mkdir(exist_ok=True)
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core(), second = new Core();' if instance else '')}
  static readonly Exception injected=new InvalidOperationException("foreign");
  static void Collect({'Core current' if instance else ''}) {{ GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers(); }}
  static void Inject({'Core current' if instance else ''}) {{ throw injected; }}
  static int Cross({'Core current, ' if instance else ''}void* p) {{ Collect({('current' if instance else '')});return {method('cast_void','second')}(p); }}
  static int Main() {{
    for(int i=0;i<42;++i) {{int n={method('run_case')}(i,&Collect);Console.WriteLine($"{{i}}:{{n}}:{{{method('read_live')}()}}:{{{method('read_trace')}()}}");}}
    if({method('cross_owner')}(&Cross)!=1 || {method('read_live')}()!=0)return 90;
    try {{ {method('run_case')}(34,&Inject);return 91; }} catch(Exception e) {{ if(!Object.ReferenceEquals(e,injected) || {method('read_live')}()!=0)return 92; }}
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
            actual=run(command,timeout=90).stdout
            (host/f'{runtime}-trace.txt').write_text(actual)
            assert actual==expected,(mode,runtime,expected,actual)
            checks.append(f'pointer dynamic_cast/{mode}/{runtime}: 42 C traces; repeated bases, access, virtual bases, construction vtables, side effects, GC, cross-owner objects and foreign cleanup')
    return checks
