"""Shared virtual bases, construction VTTs and EH against EDG C and NativeAOT."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_virtual_inheritance(root, tests, out, nativeaot, run):
    source=out/'virtual-inheritance-oracle.cpp'
    source.write_text('''#include <virtual_inheritance.cpp>
extern "C" int printf(const char*,...);
void hook() {}
int main() { for(int i=0;i<24;++i) { int n; try { n=run_case(i,hook); } catch(int x) { n=x; }
printf("%d:%d:%d:%llu\\n",i,n,read_live(),read_trace()); } }
''')
    c_source=out/'virtual-inheritance-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'virtual-inheritance-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang: raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'virtual-inheritance-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'virtual-inheritance-oracle-{label}.txt').write_text(trace)
        traces.append(trace)
    assert traces[0]==traces[1],traces
    expected=traces[0]
    values=[166,1,1,1,166,166,101,102,103,109,166,40,176,1,1,30,81,30,40,81,91,90,91,1]
    assert [tuple(map(int,line.split(':')[:3])) for line in expected.splitlines()]==[(i,n,0) for i,n in enumerate(values)],expected
    checks=['virtual inheritance: 24 EDG C layout/value/lifetime traces agree under GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/f'virtual-inheritance-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'virtual_inheritance.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split',
             'size' if instance else 'function','--split-size','2400',
             *(['--instance-methods'] if instance else ['--no-nest-types'])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function, receiver='owner'):
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
  static void Collect({'Core current' if instance else ''}) {{
    GC.Collect(2, GCCollectionMode.Forced, true, true); GC.WaitForPendingFinalizers();
  }}
  static void Inject({'Core current' if instance else ''}) {{ throw injected; }}
  static void CrossOwner({'Core current' if instance else ''}) {{ {method('throw_diamond','second')}(); }}
  static void CrossRethrow({'Core current' if instance else ''}) {{ Collect({('current' if instance else '')}); {method('again','second')}(); }}
  static int Main() {{
    for(int i=0;i<24;++i) {{ int n={method('run_checked')}(i,&Collect); Console.WriteLine($"{{i}}:{{n}}:{{{method('read_live')}()}}:{{{method('read_trace')}()}}"); }}
    try {{ {method('run_case')}(10,&Inject); return 91; }} catch(Exception ex) {{
      if(!Object.ReferenceEquals(ex,injected) || {method('read_live')}()!=0) return 92;
    }}
    {method('reset')}(); {method('reset','second')}();
    if({method('catch_root')}(&CrossOwner)!=1 || {method('read_live','second')}()!=0) return 93;
    if({method('read_trace','second')}()!=47062548446317668UL) return 94;
    {('if('+method('read_trace')+'()!=0) return 95;' if instance else '')}
    if({method('run_checked')}(12,&CrossRethrow)!=176 || {method('read_live')}()!=0) return 96;
    return 0;
  }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        commands=[('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host']))
        for runtime,command in commands:
            actual=run(command,timeout=90).stdout
            (host/f'{runtime}-trace.txt').write_text(actual)
            assert actual==expected,(mode,runtime,expected,actual)
            checks.append(f'virtual inheritance/{mode}/{runtime}: 24 C traces; forced GC, shared virtual bases, construction VTTs, covariant returns, failed construction, origin-owner destruction and foreign identity')
    return checks
