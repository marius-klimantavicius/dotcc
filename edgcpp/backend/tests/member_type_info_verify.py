"""Member-pointer source RTTI alongside dynamic_cast and exception matching."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_member_type_info(root,tests,out,nativeaot,run):
    source=out/'member-type-info-oracle.cpp'
    source.write_text('''#include <member_type_info.cpp>
extern "C" int printf(const char*,...);
void hook(){}
int main(){printf("result:%d\\n",run_member_info(hook));for(int i=0;i<12;++i)printf("%d:%s\\n",i,member_info_name(i));}
''')
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    c_source=out/'member-type-info-oracle.c'
    run([root/'build/bin/cpfe-dot-oracle','--c++17','--gnu_version','130300','-I',tests,
         '-I',root/'backend/runtime/include','--gen_c_file_name',c_source,'-o',out/'member-type-info-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'member-type-info-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout;(out/f'member-type-info-oracle-{label}.txt').write_text(trace);traces.append(trace)
    assert traces[0]==traces[1] and traces[0].startswith('result:1\n'),traces
    expected=traces[0]
    checks=['member type_info: RTTI equality, 12 names, dynamic_cast and exception identity match EDG C/GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'member-type-info-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'member_type_info.cpp','--runtime','edg-exceptions','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','2400',
             *(['--instance-methods'] if instance else ['--no-nest-types'])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function,receiver='owner'):
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(f'_Z{len(function)}{function}')]
            assert len(names)==1,(function,names)
            return (receiver if instance else 'Core')+'.'+names[0]
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
using System.Runtime.InteropServices;
unsafe class EntryPoint {{
  {('static readonly Core owner=new Core(),second=new Core();' if instance else '')}
  static void Collect({'Core current' if instance else ''}) {{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
  static int Main() {{
    Console.WriteLine($"result:{{{method('run_member_info')}(&Collect)}}");
    for(int i=0;i<12;++i) {{
      sbyte* text={method('member_info_name')}(i);void* address={method('member_info_address')}(i);
      Collect({('owner' if instance else '')});
      if(address!={method('member_info_address','second')}(i) || text!={method('member_info_name','second')}(i))return 90;
      Console.WriteLine($"{{i}}:{{Marshal.PtrToStringUTF8((nint)text)}}");
    }}
    System.Threading.Tasks.Parallel.For(0,48,i=>{{
      if({method('member_info_address')}(i%12)!={method('member_info_address','second')}(i%12))throw new Exception("RTTI owner/thread identity");
    }});
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
            checks.append(f'member type_info/{mode}/{runtime}: 12 C names, PMF/MD/function/array identities, cv/noexcept, dynamic_cast, EH and GC/thread/owner token identity')
    return checks
