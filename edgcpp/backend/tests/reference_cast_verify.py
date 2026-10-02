"""C++ bad_cast provider and reference casts against the original EDG runtime."""
import json
import os
from pathlib import Path
import resource
import subprocess
import shutil
import sys


def verify_reference_cast(root,tests,out,nativeaot,run):
    source=out/'reference-oracle.cpp'
    source.write_text('''#include <reference_cast.cpp>
extern "C" int printf(const char*,...);
void hook() {}
int main(int argc,char** argv){if(argc>1){try{terminate_case(argv[1][0]-'0');}catch(...){return 91;}return 90;}for(int i=0;i<24;++i){int n=run_ref_case(i,hook);printf("%d:%d:%d:%llu\\n",i,n,read_live(),read_trace());}}
''')
    c_source=out/'reference-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,'-I',root/'backend/runtime/include',
         '--gen_c_file_name',c_source,'-o',out/'reference-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'reference-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'reference-oracle-{label}.txt').write_text(trace);traces.append(trace)
        for case in range(2):
            fatal=subprocess.run([str(exe),str(case)],capture_output=True,text=True,timeout=15,
                preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
            assert fatal.returncode<0,(label,case,fatal)
            (out/f'reference-termination-{label}-{case}.txt').write_text(fatal.stderr)
    assert traces[0]==traces[1],traces
    expected=traces[0]
    assert [tuple(map(int,line.split(':')[:3])) for line in expected.splitlines()]==[(i,1,0) for i in range(24)],expected
    checks=['reference dynamic_cast: 24 EDG C result/lifetime traces and two termination cases agree under GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'reference-{mode}'
        command=[sys.executable,root/'dotnet.py',tests/'reference_cast.cpp','--runtime','edg-exceptions','--emit','managedlib',
                 '--output-dir',directory,'--namespace','','--class-name','Core','--split',
                 'size' if instance else 'function','--split-size','2400',
                 *(['--instance-methods'] if instance else ['--no-nest-types'])]
        run(command)
        before={p.name:p.read_bytes() for p in directory.glob('*.cs')}
        run(command)
        assert before=={p.name:p.read_bytes() for p in directory.glob('*.cs')}
        manifest=json.loads((directory/'dot-gen.json').read_text())
        assert manifest['runtime']=='edg-exceptions'
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
  static void CrossFail({'Core current' if instance else ''}) {{ Collect({('current' if instance else '')});{method('fail_reference','second')}(); }}
  static void CrossRethrow({'Core current' if instance else ''}) {{ Collect({('current' if instance else '')});{method('again','second')}(); }}
  static int Main(string[] args) {{
    if(args.Length!=0) {{try {{{method('terminate_case')}(int.Parse(args[0]));}}catch(Exception){{return 91;}}return 90;}}
    for(int i=0;i<24;++i) {{int n={method('run_ref_case')}(i,&Collect);Console.WriteLine($"{{i}}:{{n}}:{{{method('read_live')}()}}:{{{method('read_trace')}()}}");}}
    {method('reset')}();{method('reset','second')}();
    if({method('invoke_handler')}(&CrossFail)!=1 || {method('read_live','second')}()!=0)return 90;
    if({method('run_ref_case')}(22,&CrossRethrow)!=1 || {method('read_live')}()!=0)return 93;
    try {{ {method('run_ref_case')}(1,&Inject);return 91; }} catch(Exception e) {{ if(!Object.ReferenceEquals(e,injected) || {method('read_live')}()!=0)return 92; }}
    return 0;
  }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        commands=[('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host']))
        # An outer handler forces unwinding; NativeAOT may terminate an
        # unhandled exception during search without running finally blocks.
        for runtime,command in commands:
            actual=run(command,timeout=90).stdout
            (host/f'{runtime}-trace.txt').write_text(actual)
            assert actual==expected,(mode,runtime,expected,actual)
            checks.append(f'reference dynamic_cast/{mode}/{runtime}: 24 C traces; bad_cast/base/value catches, rethrow, GC, origin-owner throws, foreign cleanup and scalar allocation')
            for case in range(2):
                fatal=subprocess.run([*map(str,command),str(case)],capture_output=True,text=True,timeout=15,
                    preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
                assert fatal.returncode<0 and ('noexcept' in fatal.stderr if case==0 else 'Destructor threw during unwinding' in fatal.stderr),(mode,runtime,case,fatal)
                (host/f'{runtime}-termination-{case}.txt').write_text(fatal.stderr)
            checks.append(f'reference dynamic_cast/{mode}/{runtime}: two expected noexcept/secondary-exception terminations')
    return checks
