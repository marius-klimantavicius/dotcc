"""Data and function member pointers using EDG's integer-tag variant ABI."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_member_pointers(root, tests, out, nativeaot, run):
    for frontend in ('cpfe-dot','cpfe-dot-oracle'):
        result=run([root/'build/bin'/frontend,'--dump_configuration'])
        configuration=result.stdout+result.stderr
        (out/f'{frontend}-configuration.txt').write_text(configuration)
        flag=[line for line in configuration.splitlines() if line.startswith('#define TARG_IA64_ABI_USE_VARIANT_PTR_TO_MEMBER_FUNCTION_REPR ')]
        assert len(flag)==1 and flag[0].split()[-1] in ('1','TRUE'),flag
    source=out/'member-pointers-oracle.cpp'
    source.write_text('''#include <member_pointers.cpp>
extern "C" int printf(const char*,...);
void hook(){}
int main(){for(int i=0;i<48;++i){int n=run_member_case(i,hook);printf("%d:%d:%llu\\n",i,n,trace);}}
''')
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for abi,frontend in [('native','cpfe'),('variant','cpfe-dot-oracle')]:
        c_source=out/f'member-pointers-{abi}.c'
        run([root/'build/bin'/frontend,'--c++17','--gnu_version','130300','-I',tests,
             '--gen_c_file_name',c_source,'-o',out/f'member-pointers-{abi}.il',source],
            env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
        for label,compiler in [('gcc','gcc'),('clang',clang)]:
            exe=out/f'member-pointers-{abi}-{label}'
            run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
            trace=run([exe]).stdout
            (out/f'member-pointers-{abi}-{label}.txt').write_text(trace);traces.append(trace)
    cpp_traces=[]
    for label,compiler in [('gcc','g++'),('clang',clang)]:
        exe=out/f'member-pointers-cpp17-{label}'
        run([compiler,'-std=c++17','-I',tests,source,'-o',exe])
        trace=run([exe]).stdout
        (out/f'member-pointers-cpp17-{label}.txt').write_text(trace);cpp_traces.append(trace)
    assert cpp_traces[0]==cpp_traces[1],cpp_traces
    expected=cpp_traces[0]
    lines=expected.splitlines();assert len(lines)==48
    assert lines[15]=='15:5:51616' # RHS=3, object=1, member-pointer=2.
    differences=[]
    for trace in traces:
        actual=trace.splitlines();assert len(actual)==48
        for i,(native,defined) in enumerate(zip(actual,lines)):
            if i==15:
                assert native.startswith('15:5:')
                differences.append(native)
            else:assert native==defined,(i,native,defined)
    # Original generated C leaves assignment operands to C evaluation order;
    # source C++17 requires RHS before LHS. Preserve both observations, but
    # require the managed backend to follow the source language's sequence.
    (out/'assignment-order-observations.json').write_text(json.dumps({
        'cpp17':lines[15],'edgC':differences,'sequence':'RHS argument(3), object(1), member pointer(2)'},indent=2)+'\n')
    checks=['member pointers: 48 C++17 traces agree under GCC/Clang; EDG native/variant C agrees except documented C assignment-order freedom']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'member-pointers-{mode}'
        result=run([sys.executable,root/'dotnet.py',tests/'member_pointers.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split',
             'size' if instance else 'function','--split-size','2400',
             *(['--instance-methods'] if instance else ['--no-nest-types'])],env=dict(os.environ,EDG_DOT_TRACE='1'))
        (directory/'lowered-il.txt').write_text(result.stderr)
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function,receiver='owner'):
            prefix=f'_Z{len(function)}{function}'
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(prefix)]
            assert len(names)==1,(function,names)
            return (receiver+'.' if instance else 'Core.')+names[0]
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        independent=''
        if instance:
            independent=f'''for(int i=0;i<48;++i) {{
      int first={method('run_member_case')}(i,&Collect);ulong saved={method('read_member_trace')}();
      int second={method('run_member_case','other')}(i,&Collect);
      if(first!=second || saved!={method('read_member_trace','other')}() || saved!={method('read_member_trace')}())return 91;
    }}'''
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core(), other = new Core();' if instance else '')}
  static void Collect({'Core current' if instance else ''}) {{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
  static int Main() {{
    for(int i=0;i<48;++i){{int n={method('run_member_case')}(i,&Collect);Console.WriteLine($"{{i}}:{{n}}:{{{method('read_member_trace')}()}}");}}
    {independent}
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
            checks.append(f'member pointers/{mode}/{runtime}: 48 native C traces, null/slot-zero, multiple/virtual inheritance, conversions, cv/ref/noexcept, copies/arrays/returns, evaluation order and GC')
    return checks
