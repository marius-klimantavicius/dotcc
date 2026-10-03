"""Function-try-block wrappers, handler exit and termination against EDG C."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def verify_function_try(root, tests, out, nativeaot, run):
    source = out / 'function-try-oracle.cpp'
    source.write_text('''#include <function_try.cpp>
extern "C" int printf(const char*,...);
extern "C" int atoi(const char*);
extern "C" int fflush(void*);
int report;
void hook(int n){if(report){printf("%d\\n",n);fflush(0);}}
int main(int argc,char** argv){
if(argc>1){report=1;terminate_function_try(atoi(argv[1]),hook);return 90;}
for(int i=0;i<32;++i){int n=run_function_try(i,hook);printf("%d:%d:%llu:%d\\n",i,n,trace,live);}
return 0;}
''')
    c_source = out / 'function-try-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'function-try-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang: raise RuntimeError('Clang required for the C oracle')
    def terminated(command, argument, message):
        result=subprocess.run([*map(str,command),str(argument)],capture_output=True,text=True,
            timeout=30,preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
        assert result.returncode not in (0,90) and message in result.stderr,(argument,result.returncode,result.stdout,result.stderr)
        return result.stdout
    traces=[]; fatal_traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'function-try-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'function-try-oracle-{label}.txt').write_text(trace);traces.append(trace)
        fatal_traces.append({str(i):terminated([exe],i,'terminate() called') for i in range(3)})
    assert traces[0]==traces[1],traces
    assert fatal_traces[0]==fatal_traces[1],fatal_traces
    expected=traces[0]
    assert len(expected.splitlines())==32
    assert all(s.endswith(':0') for s in expected.splitlines()),expected
    (out/'function-try-termination-traces.json').write_text(json.dumps(fatal_traces[0],indent=2)+'\n')
    checks=['function try blocks: 32 EDG C lifetime traces and 3 termination cases agree under GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'function-try-{mode}'
        result=run([sys.executable,root/'dotnet.py',tests/'function_try.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split',
             'size' if instance else 'function','--split-size','2400',
             *(['--instance-methods'] if instance else ['--no-nest-types'])],env=dict(os.environ,EDG_DOT_TRACE='1'))
        (directory/'lowered-il.txt').write_text(result.stderr)
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function):
            prefix=f'_Z{len(function)}{function}'
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(prefix)]
            assert len(names)==1,(function,names)
            return ('owner.' if instance else 'Core.')+names[0]
        host=directory/'host';host.mkdir(exist_ok=True)
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core();' if instance else '')}
  static bool report;
  static void Observe({'Core current, ' if instance else ''}int n) {{
    if(report)Console.WriteLine(n);
    GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
  }}
  static int Main(string[] args) {{
    if(args.Length!=0) {{report=true;{method('terminate_function_try')}(int.Parse(args[0]),&Observe);return 90;}}
    for(int i=0;i<32;++i) {{int n={method('run_function_try')}(i,&Observe);Console.WriteLine($"{{i}}:{{n}}:{{{method('read_function_trace')}()}}:{{{method('read_function_live')}()}}");}}
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
            checks.append(f'function try blocks/{mode}/{runtime}: 32 C traces for subobject cleanup, implicit rethrow, delegation, virtual bases and handler returns')
            for i in range(3):
                # Default termination routes through the current C++ terminate handler.
                # Its stable fatal text changed; exact native traces below still
                # distinguish the noexcept and secondary-unwind boundaries.
                message='C++ default terminate'
                actual=terminated(command,i,message)
                (host/f'{runtime}-termination-{i}.txt').write_text(actual)
                assert actual==fatal_traces[0][str(i)],(mode,runtime,i,actual,fatal_traces[0])
            checks.append(f'function try blocks/{mode}/{runtime}: 3 C-matched noexcept/secondary-throw termination cases')
    return checks
