"""Source-type EH identities and correctly sized native catch temporaries."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def verify_extended_pointer_exceptions(root, tests, out, nativeaot, run):
    source=out/'extended-pointer-exceptions-oracle.cpp'
    source.write_text('''#include <extended_pointer_exceptions.cpp>
extern "C" int printf(const char*,...);
extern "C" int atoi(const char*);
void hook(){}
int one(int i){try{return run_extended_pointer_exception(i,hook);}catch(...){return -777;}}
int main(int argc,char** argv){if(argc>1){int i=atoi(argv[1]);printf("%d:%d\\n",i,one(i));}
else for(int i=0;i<40;++i)printf("%d:%d\\n",i,one(i));}
''')
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for abi,frontend in [('native','cpfe'),('variant','cpfe-dot-oracle')]:
        c_source=out/f'extended-pointer-exceptions-{abi}.c'
        run([root/'build/bin'/frontend,'--c++17','--gnu_version','130300','-I',tests,
             '--gen_c_file_name',c_source,'-o',out/f'extended-pointer-exceptions-{abi}.il',source],
            env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
        for label,compiler in [('gcc','gcc'),('clang',clang)]:
            exe=out/f'extended-pointer-exceptions-{abi}-{label}'
            run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
            trace=run([exe]).stdout
            (out/f'extended-pointer-exceptions-{abi}-{label}.txt').write_text(trace);traces.append(trace)
    assert all(t==traces[0] for t in traces),traces
    expected=traces[0];assert len(expected.splitlines())==40
    # Keep source-C++ observations separate: the installed runtimes differ on
    # member noexcept/cv matching and on mutable nullptr-conversion references.
    # One process per case preserves later evidence even if a runtime aborts.
    def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
    observations={}
    for label,compiler in [('gcc','g++'),('clang',clang)]:
        exe=out/f'extended-pointer-exceptions-cpp17-{label}'
        run([compiler,'-std=c++17','-I',tests,source,'-o',exe])
        cases=[]
        for i in range(40):
            result=subprocess.run([exe,str(i)],text=True,capture_output=True,timeout=15,preexec_fn=no_core)
            cases.append({'case':i,'exit':result.returncode,'stdout':result.stdout,'stderr':result.stderr})
        observations[label]=cases
    (out/'source-cpp-observations.json').write_text(json.dumps(observations,indent=2)+'\n')
    checks=['extended pointer EH: 40 EDG C traces agree under native/variant ABI and GCC/Clang; source-C++ differences retained separately']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'extended-pointer-exceptions-{mode}'
        result=run([sys.executable,root/'dotnet.py',tests/'extended_pointer_exceptions.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','2400',
             *(['--instance-methods'] if instance else ['--no-nest-types'])],env=dict(os.environ,EDG_DOT_TRACE='1'))
        (directory/'lowered-il.txt').write_text(result.stderr)
        manifest=json.loads((directory/'dot-gen.json').read_text())
        names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith('_Z30run_extended_pointer_exception')]
        assert len(names)==1,names
        receiver='owner' if instance else 'Core';method=receiver+'.'+names[0]
        original=directory/'uninstrumented';original.mkdir()
        for p in directory.glob('Translated*.cs'):
            text=p.read_text();(original/p.name).write_text(text)
            text=text.replace('global::System.Runtime.InteropServices.NativeMemory.Alloc(', '__edg_test_alloc(')
            text=text.replace('global::System.Runtime.InteropServices.NativeMemory.Free(', '__edg_test_free(')
            p.write_text(text)
        # Guard both sides of every native allocation. A one-word buffer used
        # as a two-word member-function null must fail even if the allocator
        # happens to reserve spare space after the requested block.
        (directory/'TestAccess.cs').write_text('''using System;
public '''+('' if instance else 'static ')+'''unsafe partial class Core {
  private static readonly object TestSync=new();
  private static readonly global::System.Collections.Generic.Dictionary<nint,nuint> TestAllocations=new();
  public static int TestOutstanding {get {lock(TestSync)return TestAllocations.Count;}}
  private static void* __edg_test_alloc(nuint size) {
    byte* raw=(byte*)global::System.Runtime.InteropServices.NativeMemory.Alloc(size+32);
    for(int i=0;i<16;++i){raw[i]=0x5a;raw[16+size+(nuint)i]=0xa5;}
    lock(TestSync)TestAllocations.Add((nint)(raw+16),size);return raw+16;
  }
  private static void __edg_test_free(void* pointer) {
    if(pointer==null)return;nuint size;
    lock(TestSync){size=TestAllocations[(nint)pointer];TestAllocations.Remove((nint)pointer);}
    byte* raw=(byte*)pointer-16;
    for(int i=0;i<16;++i)if(raw[i]!=0x5a || raw[16+size+(nuint)i]!=0xa5)
      global::System.Environment.FailFast("Exception native storage boundary overwritten");
    global::System.Runtime.InteropServices.NativeMemory.Free(raw);
  }
}
''')
        project=directory/'Generated.csproj';project.write_text(project.read_text().replace('</Project>','<ItemGroup><Compile Include="TestAccess.cs" /></ItemGroup></Project>'))
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core();' if instance else '')}
  static void Collect({'Core current' if instance else ''}) {{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
  static int Main() {{
    for(int i=0;i<40;++i){{int n;try{{n={method}(i,&Collect);}}
      catch(Exception error) when(error is IDisposable){{n=-777;((IDisposable)error).Dispose();}}
      Console.WriteLine($"{{i}}:{{n}}");if(Core.TestOutstanding!=0)throw new Exception("Native exception storage leaked");}}
    {receiver}.Shutdown();return 0;
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
            checks.append(f'extended pointer EH/{mode}/{runtime}: 40 C traces, member/class/signature/noexcept identity, arrays/floats, rethrow mutation, guarded native allocation balance')
    return checks
