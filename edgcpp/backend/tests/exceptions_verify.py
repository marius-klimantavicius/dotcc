"""Scalar C++ exceptions against EDG C, plus managed-boundary and GC tests."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def verify_exceptions(root, tests, out, nativeaot, run):
    source=out/'exceptions-oracle.cpp'
    # Angle brackets avoid an older rejection fixture of the same name in out.
    source.write_text('''#include <exceptions.cpp>
extern "C" int printf(const char*,...);
void hook() {}
int main() { for(int i=0;i<32;++i) { int n=run_case(i,hook); printf("%d:%d:%llu\\n",i,n,read_trace()); } }
''')
    c_source=out/'exceptions-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'exceptions-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang: raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'exceptions-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'exceptions-oracle-{label}.txt').write_text(trace)
        traces.append(trace)
    assert traces[0]==traces[1],traces
    expected=traces[0]
    assert len(expected.splitlines())==32
    checks=['scalar exceptions: 32 EDG C traces agree under GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/f'exceptions-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'exceptions.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size',
             '--split-size','2400',*(['--instance-methods'] if instance else [])])
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
  static void CrossOwner({'Core current' if instance else ''}) {{ {method('again','second')}(); }}
  static int Main(string[] args) {{
    if(args.Length!=0) {{ try {{ {method('terminate_case')}(int.Parse(args[0])); }} catch(Exception) {{ return 91; }} return 90; }}
    for(int i=0;i<32;++i) {{ int n={method('run_case')}(i,&Collect); Console.WriteLine($"{{i}}:{{n}}:{{{method('read_trace')}()}}"); }}
    try {{ {method('foreign')}(&Inject); return 92; }} catch(Exception ex) {{
      if(!Object.ReferenceEquals(ex,injected) || {method('read_trace')}()!=1526) return 93;
    }}
    try {{ {method('run_case')}(24,&Inject); return 95; }} catch(Exception ex) {{
      if(!Object.ReferenceEquals(ex,injected)) return 96;
    }}
    // Rethrow follows the active thread, even through another owner.
    if({method('threaded')}(40,&CrossOwner)!=40) return 94;
    Exception? failure=null;
    var a=new Thread(()=>{{try {{for(int i=0;i<30;++i)if({method('threaded')}(i,&Synchronize)!=i+1)throw new Exception("A");}}catch(Exception ex){{failure=ex;}}}});
    var b=new Thread(()=>{{try {{for(int i=0;i<30;++i)if({method('threaded','second')}(100+i,&Synchronize)!=101+i)throw new Exception("B");}}catch(Exception ex){{failure=ex;}}}});
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
            checks.append(f'scalar exceptions/{mode}/{runtime}: 32 C traces; forced GC, foreign identity, cross-owner rethrow and thread isolation')
            for argument in range(7):
                result=subprocess.run([*map(str,command),str(argument)],capture_output=True,text=True,
                    timeout=15,preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
                message=('Exception escaped noexcept function' if argument==1 else
                         'Destructor threw during unwinding' if argument==2 else
                         'C++ rethrow without active exception')
                assert result.returncode not in (0,90,91) and message in result.stderr,result.stderr
            checks.append(f'scalar exceptions/{mode}/{runtime}: 7 expected terminations (bare/stale rethrow, noexcept, double exception)')
    return checks
