"""Virtual dispatch, covariant thunks and EH against EDG C and real NativeAOT."""
import json
import os
from pathlib import Path
import shutil
import resource
import subprocess
import sys


def verify_virtual_dispatch(root, tests, out, nativeaot, run):
    source=out/'virtual-oracle.cpp'
    source.write_text('''#include <virtual_dispatch.cpp>
extern "C" int printf(const char*,...);
void hook() {}
int main(int argc,char**) { if(argc>1){terminate_case();return 90;} for(int i=0;i<32;++i) { int n; try { n=run_case(i,hook); } catch(int x) { n=x; }
printf("%d:%d:%d:%llu\\n",i,n,read_live(),read_trace()); } }
''')
    c_source=out/'virtual-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'virtual-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang: raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'virtual-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'virtual-oracle-{label}.txt').write_text(trace)
        traces.append(trace)
        fatal=subprocess.run([str(exe),'fatal'],capture_output=True,text=True,timeout=15,
            preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
        assert fatal.returncode<0,(label,fatal)
        (out/f'virtual-pure-{label}.txt').write_text(fatal.stderr)
    assert traces[0]==traces[1],traces
    expected=traces[0]
    # The C backend's deleting-destructor entry calls D1, then delete. Its
    # throwing-D1 path in case 30 therefore has no delete event; preserve that
    # oracle behavior rather than changing EDG lowering in this backend.
    values=[12,23,71,1025,1,1,1,12,35,65,110,101,102,103,35,113,588,35,712,812,57,67,1,1,23,1,1,23,1104,1,105,23]
    assert [tuple(map(int,line.split(':')[:3])) for line in expected.splitlines()]==[(i,n,0) for i,n in enumerate(values)],expected
    checks=['virtual dispatch: 32 EDG C layout/value/lifetime traces agree under GCC and Clang']
    checks.append('virtual dispatch: pure virtual call terminates under both EDG C oracles')
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/f'virtual-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'virtual_dispatch.cpp','--emit','managedlib',
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
  static void CrossOwner({'Core current' if instance else ''}) {{ {method('throw_multi','second')}(); }}
  static void CrossRethrow({'Core current' if instance else ''}) {{ Collect({('current' if instance else '')}); {method('again','second')}(); }}
  static int Main(string[] args) {{
    if(args.Length!=0) {{ try {{ {method('terminate_case')}(); }} catch(Exception) {{ return 91; }} return 90; }}
    for(int i=0;i<32;++i) {{ int n={method('run_checked')}(i,&Collect); Console.WriteLine($"{{i}}:{{n}}:{{{method('read_live')}()}}:{{{method('read_trace')}()}}"); }}
    try {{ {method('run_case')}(14,&Inject); return 91; }} catch(Exception ex) {{
      if(!Object.ReferenceEquals(ex,injected) || {method('read_live')}()!=0) return 92;
    }}
    {method('reset')}(); {method('reset','second')}();
    if({method('catch_other')}(&CrossOwner)!=1 || {method('read_live','second')}()!=0) return 93;
    if({method('read_trace','second')}()!=2751406334955UL) return 94;
    {('if('+method('read_trace')+'()!=0) return 95;' if instance else '')}
    if({method('run_checked')}(16,&CrossRethrow)!=522 || {method('read_live')}()!=0) return 96;
    ulong token={method('type_token')}(); Collect({('owner' if instance else '')});
    if(token==0 || token!={method('type_token','second')}()) return 97;
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
            checks.append(f'virtual dispatch/{mode}/{runtime}: 32 C traces; forced GC, secondary vtables, covariant returns, constructor/destructor dispatch, failed construction, origin-owner destruction and foreign identity')
            fatal=subprocess.run([*map(str,command),'fatal'],capture_output=True,text=True,timeout=15,
                preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
            assert fatal.returncode<0 and 'EDG pure virtual call' in fatal.stderr,(mode,runtime,fatal)
            (host/f'{runtime}-pure-termination.txt').write_text(fatal.stderr)
            checks.append(f'virtual dispatch/{mode}/{runtime}: expected pure virtual termination')
    return checks
