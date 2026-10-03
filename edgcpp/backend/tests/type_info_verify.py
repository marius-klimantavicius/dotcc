"""Public type_info and bad_typeid against EDG's original C/runtime backend."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def verify_type_info(root,tests,out,nativeaot,run):
    source=out/'typeinfo-oracle.cpp'
    source.write_text('''#include <type_info.cpp>
extern "C" int printf(const char*,...);
void hook() {}
void fail_hook(){fail_typeid();}
int main(int argc,char**){if(argc>1){try{fail_typeid_noexcept();}catch(...){return 91;}return 90;}
for(int i=0;i<34;++i){int n=run_type_case(i,i==21?fail_hook:hook);printf("%d:%d:%d:%llu\\n",i,n,read_live(),read_trace());}
for(int i=0;i<24;++i)printf("name-%d:%s\\n",i,type_name(i));}
''')
    c_source=out/'typeinfo-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,'-I',root/'backend/runtime/include',
         '--gen_c_file_name',c_source,'-o',out/'typeinfo-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'typeinfo-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout
        (out/f'typeinfo-oracle-{label}.txt').write_text(trace);traces.append(trace)
        fatal=subprocess.run([str(exe),'fatal'],capture_output=True,text=True,timeout=15,
            preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
        assert fatal.returncode<0,(label,fatal)
        (out/f'typeinfo-termination-{label}.txt').write_text(fatal.stderr)
    assert traces[0]==traces[1],traces
    expected=traces[0]
    assert [tuple(map(int,line.split(':')[:3])) for line in expected.splitlines()[:34]]==[(i,0 if i==16 else 1,0) for i in range(34)],expected
    checks=['type_info: 34 result/lifetime traces, 24 type names and bad_typeid noexcept termination agree under EDG C/GCC and Clang']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'typeinfo-{mode}'
        command=[sys.executable,root/'dotnet.py',tests/'type_info.cpp','--runtime','edg-exceptions','--emit','managedlib',
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
using System.Runtime.InteropServices;
unsafe class EntryPoint {{
  {('static readonly Core owner=new Core(),second=new Core();' if instance else '')}
  static readonly Exception injected=new InvalidOperationException("foreign");
  static void Collect({'Core current' if instance else ''}) {{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
  static void Inject({'Core current' if instance else ''}) {{throw injected;}}
  static void CrossFail({'Core current' if instance else ''}) {{Collect({('current' if instance else '')});{method('fail_typeid','second')}();}}
  static int Main(string[] args) {{
    if(args.Length!=0) {{try {{{method('fail_typeid_noexcept')}();}}catch(Exception){{return 91;}}return 90;}}
    for(int i=0;i<34;++i) {{int n={method('run_type_case')}(i,i==21?&CrossFail:&Collect);Console.WriteLine($"{{i}}:{{n}}:{{{method('read_live')}()}}:{{{method('read_trace')}()}}");}}
    for(int i=0;i<24;++i) {{
      sbyte* name={method('type_name')}(i);void* info={method('type_address')}(i);
      Collect({('owner' if instance else '')});
      if(name!={method('type_name','second')}(i) || info!={method('type_address','second')}(i))return 93;
      Console.WriteLine($"name-{{i}}:{{Marshal.PtrToStringUTF8((nint)name)}}");
    }}
    try {{{method('run_type_case')}(31,&Inject);return 91;}}
    catch(Exception e) {{if(!Object.ReferenceEquals(e,injected) || {method('read_live')}()!=0)return 92;}}
    // The generated runtime's immutable metadata is shared, even across threads.
    System.Threading.Tasks.Parallel.For(0,64,i=>{{
      if({method('type_address')}(i%24)!={method('type_address','second')}(i%24) ||
         {method('type_name')}(i%24)!={method('type_name','second')}(i%24))throw new Exception("RTTI identity");
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
            actual=run(command,timeout=90).stdout
            (host/f'{runtime}-trace.txt').write_text(actual)
            assert actual==expected,(mode,runtime,expected,actual)
            checks.append(f'type_info/{mode}/{runtime}: 34 C traces, 24 names, ordering laws, GC/thread/owner identity, bad_typeid rethrow and foreign cleanup')
            fatal=subprocess.run([*map(str,command),'fatal'],capture_output=True,text=True,timeout=15,
                preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
            assert fatal.returncode<0 and 'C++ default terminate' in fatal.stderr,(mode,runtime,fatal)
            (host/f'{runtime}-termination.txt').write_text(fatal.stderr)
            checks.append(f'type_info/{mode}/{runtime}: expected bad_typeid noexcept termination')
    negatives={
        'metadata-typeid': '#include <typeinfo>\nint main(){return typeid(typeid(int))==typeid(std::type_info);}',
        'metadata-dynamic-cast': '#include <typeinfo>\nint main(){return dynamic_cast<const void*>(&typeid(int))==nullptr;}',
        'metadata-destructor': '#include <typeinfo>\nint main(){typeid(int).~type_info();}',
    }
    for label,code in negatives.items():
        path=out/f'typeinfo-{label}.cpp';path.write_text(code)
        directory=out/f'rejected-typeinfo-{label}'
        result=run([sys.executable,root/'dotnet.py',path,'--runtime','edg-exceptions','--output-dir',directory],expected=1)
        assert 'dot_gen_be: unsupported' in result.stderr and not directory.exists(),(label,result)
        checks.append(f'rejected type_info {label} without publishing output')
    return checks
