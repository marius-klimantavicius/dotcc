"""EDG C differential lifetime/allocation tests plus managed ownership races."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_initialization_allocation(root,tests,out,nativeaot,run):
    checks=[]
    clang=shutil.which('clang') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang'))),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    for fixture in ('initialization','allocation'):
        allocation=fixture=='allocation'
        source=out/f'{fixture}-oracle.cpp'
        source.write_text(f'#include <{fixture}.cpp>\n'+('void* operator new(size_t n,std::align_val_t a,const std::nothrow_t&) noexcept {try{return operator new(n,a);}catch(...){return nullptr;}}\n' if allocation else '')+'''extern "C" int printf(const char*,...);
void hook() {}
void report(int n){printf("event:%d\\n",n);}
int main(){'''+('''for(int i=0;i<18;++i){int n=run_alloc(i,hook);printf("%d:%d:%d:%llu\\n",i,n,read_alloc_live(),read_alloc_trace());}'''
            if allocation else '''printf("startup:%llu\\n",read_init());int n=run_init();printf("run:%d:%llu\\n",n,read_init());observe(report);''')+'}\n')
        c_source=out/f'{fixture}-oracle.c'
        run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,'-I',root/'backend/runtime/include',
             '--gen_c_file_name',c_source,'-o',out/f'{fixture}-oracle.il',source],
            env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
        traces=[]
        for label,compiler in [('gcc','gcc'),('clang',clang)]:
            exe=out/f'{fixture}-oracle-{label}'
            run([compiler,*(['-femit-all-decls'] if label=='clang' else []),'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
            trace=run([exe]).stdout
            (out/f'{fixture}-oracle-{label}.txt').write_text(trace);traces.append(trace)
        assert traces[0]==traces[1],traces
        expected=traces[0]
        checks.append(f'{fixture}: original EDG C traces agree under GCC and Clang')
        for instance in (False,True):
            mode='instance' if instance else 'static';directory=out/f'{fixture}-{mode}'
            command=[sys.executable,root/'dotnet.py',tests/f'{fixture}.cpp','--emit','managedlib',
                     '--output-dir',directory,'--namespace','','--class-name','Core','--split',
                     'size' if instance else 'function','--split-size','2400',
                     *(['--runtime','edg-exceptions'] if allocation else []),
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
            owner='owner' if instance else 'Core'
            host=directory/'host';host.mkdir(exist_ok=True)
            (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
            body=(f'''for(int i=0;i<18;++i) {{int n={method('run_alloc')}(i,&Collect);Console.WriteLine($"{{i}}:{{n}}:{{{method('read_alloc_live')}()}}:{{{method('read_alloc_trace')}()}}");}}
void* p={method('allocate_object')}();if(*(int*)p!=41)return 81;
{method('free_object','second' if instance else 'owner')}(p);
{method('allocate_object')}(); // Deliberately leave storage for owner shutdown.
''' if allocation else f'''Console.WriteLine($"startup:{{{method('read_init')}()}}");
int n={method('run_init')}();Console.WriteLine($"run:{{n}}:{{{method('read_init')}()}}");
{method('observe')}(&Report);
''')
            if not allocation:
                body+=f'''
// One initialization across concurrent callers; GC while holding the guard.
var threads=new Thread[8];for(int i=0;i<threads.Length;++i){{threads[i]=new Thread(()=>{{if({method('local_parallel')}(&Once)!=42)Environment.FailFast("guard");{owner}.ReleaseThread();}});threads[i].Start();}}
foreach(var thread in threads)thread.Join();if(once!=1)return 82;
'''
                if instance:
                    body+=f'''if({method('read_init','second')}()!={expected.splitlines()[0].split(":")[1]}UL)return 83;
// Foreign TLS destruction must happen on its original thread.
using var ready=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
var worker=new Thread(()=>{{{method('tls','second')}();ready.Set();release.Wait();second.ReleaseThread();}});worker.Start();ready.Wait();
try{{second.Dispose();return 84;}}catch(InvalidOperationException){{}}
release.Set();worker.Join();second.Dispose();
'''
            if not allocation:
                body+=f"var callback={method('get_hook')}();\n"
                if instance:
                    body+=f'''
using var race=new Core();
var active=new Thread(()=>{{if({method('enter_hook','race')}(&Hold)!=19)Environment.FailFast("active call");race.ReleaseThread();}});active.Start();held.Wait();
var disposing=new Thread(()=>{{race.Dispose();disposed.Set();}});disposing.Start();
// Wait until the owner rejects new entries, proving Shutdown reached its wait.
bool closing=false;for(int i=0;i<1000;++i){{try{{{method('read_init','race')}();}}catch(ObjectDisposedException){{closing=true;break;}}Thread.Sleep(1);}}
if(!closing || disposed.IsSet)return 86;unblock.Set();active.Join();disposing.Join();if(!disposed.IsSet)return 87;
'''
            body+=f'''{owner}.Shutdown();{owner}.Shutdown();
try{{{method('read_alloc_live' if allocation else 'read_init')}();return 85;}}catch(ObjectDisposedException){{}}
'''
            if not allocation:
                body+=f"try{{callback({('owner' if instance else '')});return 88;}}catch(ObjectDisposedException){{}}\n"
            hold=(f"static void Hold(Core current){{held.Set();unblock.Wait();{method('read_init','current')}();}}" if instance and not allocation else '')
            (host/'Program.cs').write_text(f'''using System;
using System.Threading;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core(), second = new Core();' if instance else '')}
  static int once;
  static readonly ManualResetEventSlim held=new(),unblock=new(),disposed=new();
  {hold}
  static void Collect({'Core current' if instance else ''}) {{GC.Collect(2,GCCollectionMode.Forced,true,true);}}
  static void Once({'Core current' if instance else ''}){{Interlocked.Increment(ref once);Thread.Sleep(20);Collect({('current' if instance else '')});}}
  static void Report({('Core current, ' if instance else '')}int n){{Console.WriteLine($"event:{{n}}");}}
  static int Main(){{{body}return 0;}}
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
                checks.append(f'{fixture}/{mode}/{runtime}: C traces, GC, deterministic splitting, disposal and owner isolation')
    return checks
