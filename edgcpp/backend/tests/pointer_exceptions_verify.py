"""Pointer/nullptr/enum EH against EDG C, with pinned catch buffers and owners."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_pointer_exceptions(root, tests, out, nativeaot, run):
    source=out/'pointer-exceptions-oracle.cpp'
    source.write_text('''#include <pointer_exceptions.cpp>
extern "C" int printf(const char*,...);
void hook() {}
int main() { for(int i=0;i<48;++i) { int n=run_case(i,hook); printf("%d:%d:%llu\\n",i,n,read_trace()); } }
''')
    c_source=out/'pointer-exceptions-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'pointer-exceptions-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang: raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'pointer-exceptions-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'pointer-exceptions-oracle-{label}.txt').write_text(trace)
        traces.append(trace)
    assert traces[0]==traces[1],traces
    expected=traces[0]
    values=[17,17,17,17,1,1,17,17,17,17,17,1,1,1,1,1,16,1,18,41,1,11,201,1,11,12,17,17,28,17,1,17,11,11,34,35,17,17,17,17,17,41,1,1,11,1,23,17]
    assert expected==''.join(f'{i}:{n}:{1 if i==0 else 41 if i==30 else 21 if i==31 else 0}\n' for i,n in enumerate(values)),expected
    checks=['pointer/enum exceptions: 48 EDG C traces agree under GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static'
        catchall=out/f'pointer-exceptions-catchall-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'pointer_exceptions_catchall.cpp',
             '--output-dir',catchall,*(['--instance-methods'] if instance else [])])
        run(['dotnet','build',catchall/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',catchall/'bin/Release/net10.0/Generated.dll'])
        checks.append(f'pointer/enum exceptions/{mode}/JIT: catch-all-only RTTI graph and cleanup')
        if nativeaot:
            run(['dotnet','publish',catchall/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',catchall/'aot'])
            run([catchall/'aot/Generated'])
            checks.append(f'pointer/enum exceptions/{mode}/NativeAOT: catch-all-only RTTI graph and cleanup')
        directory=out/f'pointer-exceptions-{mode}'
        command=[sys.executable,root/'dotnet.py',tests/'pointer_exceptions.cpp','--emit','managedlib',
                 '--output-dir',directory,'--namespace','','--class-name','Core','--split','size',
                 '--split-size','2400',*(['--instance-methods'] if instance else ['--no-nest-types'])]
        run(command)
        generated={p.name:p.read_bytes() for p in directory.glob('Translated*.cs')}
        run(command)
        assert generated=={p.name:p.read_bytes() for p in directory.glob('Translated*.cs')}
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
using System.Threading;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core(), second = new Core();' if instance else '')}
  static readonly Exception injected=new InvalidOperationException("foreign");
  static readonly Barrier barrier=new Barrier(2);
  static void Collect({'Core current' if instance else ''}) {{
    GC.Collect(2, GCCollectionMode.Forced, true, true); GC.WaitForPendingFinalizers();
  }}
  static void Inject({'Core current' if instance else ''}) {{ throw injected; }}
  static void Synchronize({'Core current' if instance else ''}) {{ barrier.SignalAndWait(); Collect({('current' if instance else '')}); }}
  static void CrossOwner({'Core current' if instance else ''}) {{ {method('throw_owned','second')}(); }}
  static void CrossRethrow({'Core current' if instance else ''}) {{ Collect({('current' if instance else '')}); {method('again','second')}(); }}
  static int Main() {{
    for(int i=0;i<48;++i) {{ int n={method('run_case')}(i,&Collect); Console.WriteLine($"{{i}}:{{n}}:{{{method('read_trace')}()}}"); }}
    try {{ {method('run_case')}(30,&Inject); return 91; }} catch(Exception ex) {{ if(!Object.ReferenceEquals(ex,injected)) return 92; }}
    if({method('catch_owned')}(&CrossOwner,{method('owned_address','second')}())!=1) return 93;
    if({method('threaded')}(40,&CrossRethrow)!=40) return 94;
    Exception? failure=null;
    var a=new Thread(()=>{{try {{for(int i=0;i<30;++i)if({method('threaded')}(i,&Synchronize)!=i)throw new Exception("A");}}catch(Exception ex){{failure=ex;}}}});
    var b=new Thread(()=>{{try {{for(int i=0;i<30;++i)if({method('threaded','second')}(100+i,&Synchronize)!=100+i)throw new Exception("B");}}catch(Exception ex){{failure=ex;}}}});
    a.Start();b.Start();a.Join();b.Join();if(failure!=null)throw failure;
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
            checks.append(f'pointer/enum exceptions/{mode}/{runtime}: 48 C traces; forced GC, pointer catch buffers, cross-owner throws/rethrows and thread isolation')
    return checks
