"""Deterministic native exception storage and explicit host ownership."""
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def verify_exception_storage(root, tests, out, nativeaot, run):
    source=out/'exception-storage-oracle.cpp'
    source.write_text('''#include <exception_storage.cpp>
extern "C" int printf(const char*,...);
void hook(){}
int main(){int result=exercise_storage(24,hook);printf("%d:%d:%d:%d\\n",result,live,destroyed,bad);}
''')
    c_source=out/'exception-storage-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,
         '--gen_c_file_name',c_source,'-o',out/'exception-storage-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    candidates=[os.environ.get('EDG_TEST_CLANGXX','clang++')]
    candidates.extend(str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++')))
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang required for the C oracle')
    traces=[]
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        exe=out/f'exception-storage-oracle-{label}'
        run([compiler,'-x','c','-w','-std=gnu11',c_source,'-x','none',root/'build/lib/libC.a','-lm','-o',exe])
        trace=run([exe]).stdout;(out/f'exception-storage-oracle-{label}.txt').write_text(trace);traces.append(trace)
    assert traces[0]==traces[1]=='300:0:48:0\n',traces
    checks=['exception storage: repeated class copy/rethrow and pointer catches match GCC/Clang EDG C']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'exception-storage-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'exception_storage.cpp','--emit','managedlib',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size',
             '--split-size','2400',*(['--instance-methods'] if instance else ['--no-nest-types'])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function,receiver='owner'):
            prefix=f'_Z{len(function)}{function}'
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(prefix)]
            assert len(names)==1,(function,names)
            return (receiver+'.' if instance else 'Core.')+names[0]
        # This fixture injects only the native allocator dependency, not the
        # ownership logic. Preserve the uninstrumented translation as evidence.
        original=directory/'uninstrumented';original.mkdir()
        for p in directory.glob('Translated*.cs'):
            text=p.read_text();(original/p.name).write_text(text)
            text=text.replace('global::System.Runtime.InteropServices.NativeMemory.Alloc(', '__edg_test_alloc(')
            text=text.replace('global::System.Runtime.InteropServices.NativeMemory.Free(', '__edg_test_free(')
            p.write_text(text)
        (directory/'TestAccess.cs').write_text('''using System;
public '''+('' if instance else 'static ')+'''unsafe partial class Core {
  private static readonly object TestSync=new();
  private static readonly global::System.Collections.Generic.HashSet<nint> TestAllocations=new();
  public static int TestFailAt,TestAttempts,TestNullAt;
  public static int TestOutstanding {get {lock(TestSync)return TestAllocations.Count;}}
  private static void* __edg_test_alloc(nuint size) {
    int attempt=global::System.Threading.Interlocked.Increment(ref TestAttempts);
    if(attempt==TestFailAt)throw new OutOfMemoryException("injected native allocation failure");
    if(attempt==TestNullAt)return null;
    void* p=global::System.Runtime.InteropServices.NativeMemory.Alloc(size);
    lock(TestSync)if(!TestAllocations.Add((nint)p))throw new Exception("duplicate allocation");
    return p;
  }
  private static void __edg_test_free(void* p) {
    if(p==null)return;
    lock(TestSync)if(!TestAllocations.Remove((nint)p))throw new Exception("double/unowned native free");
    global::System.Runtime.InteropServices.NativeMemory.Free(p);
  }
}
''')
        project=directory/'Generated.csproj';project.write_text(project.read_text().replace('</Project>','<ItemGroup><Compile Include="TestAccess.cs" /></ItemGroup></Project>'))
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
<Nullable>enable</Nullable>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        receiver='owner' if instance else 'Core'
        owner_helpers=''
        owner_checks=''
        if instance:
            owner_helpers=f'''[MethodImpl(MethodImplOptions.NoInlining)]
  static (Exception,WeakReference) SpawnEscapedOwner() {{
    var other=new Core();
    try {{{method('raise_storage_packet','other')}(72);}}
    catch(Exception error){{return (error,new WeakReference(other));}}
    throw new Exception("did not throw");
  }}'''
            owner_checks='''var owned=SpawnEscapedOwner();
    GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    Check(owned.Item2.IsAlive,"escaped exception lost originating owner");
    ((IDisposable)owned.Item1).Dispose();
    Check(Core.TestOutstanding==0,"owner-specific payload leaked");
    if(owned.Item2.Target is Core other)other.Dispose();'''
        (host/'Program.cs').write_text(f'''using System;
using System.Runtime.CompilerServices;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core();' if instance else '')}
  static void Collect({'Core current' if instance else ''}) {{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
  static void Check(bool ok,string message) {{if(!ok)throw new Exception(message);}}
  [MethodImpl(MethodImplOptions.NoInlining)]
  static Exception Escape() {{try {{{method('raise_storage_packet')}(71);}}catch(Exception error){{return error;}}throw new Exception("did not throw");}}
  [MethodImpl(MethodImplOptions.NoInlining)]
  static WeakReference Abandon() {{return new WeakReference(Escape());}}
  {owner_helpers}
  static Exception? retained;
  static int disposalCallbacks;
  static void CaptureShutdown({'Core current' if instance else ''}) {{
    try {{{method('raise_storage_packet')}(73);}}
    catch(Exception error) {{retained=error;}}
  }}
  static void DisposalCallback({'Core current' if instance else ''}) {{
    Check({method('read_storage_live')}()>=0,"disposal callback lost owner storage");
    ++disposalCallbacks;
  }}
  static int Main(string[] args) {{
    if(args.Length!=0) {{
      int which=int.Parse(args[0]);
      if(which==0)Core.TestFailAt=1;
      if(which==1)Core.TestNullAt=1;
      if(which==2)Core.TestFailAt=3;
      try {{if(which==2){method('exercise_storage')}(1,&Collect);else {method('raise_storage_int')}();}}
      catch(Exception) {{return 91;}}
      return 90;
    }}
    int result={method('exercise_storage')}(24,&Collect);
    Console.WriteLine($"{{result}}:{{{method('read_storage_live')}()}}:{{{method('read_storage_destroyed')}()}}:{{{method('read_storage_bad')}()}}");
    Check(Core.TestOutstanding==0,"normal catches leaked native storage");
    Check({method('read_storage_alignment')}()==0,"exception payload alignment");
    Exception escaped=Escape();
    Check(escaped is IDisposable && Core.TestOutstanding==1,"escaped carrier ownership");
    try {{{receiver}.Shutdown();throw new Exception("shutdown accepted live carrier");}}
    catch(InvalidOperationException) {{}}
    int before={method('read_storage_destroyed')}();
    ((IDisposable)escaped).Dispose();((IDisposable)escaped).Dispose();
    Check(Core.TestOutstanding==0 && {method('read_storage_destroyed')}()==before+1,"host Dispose must destroy once");
    Exception throwing=Escape();{method('set_storage_destructor_throw')}(1);
    try {{((IDisposable)throwing).Dispose();throw new Exception("destructor did not throw");}}
    catch(Exception error) when(error is IDisposable) {{
      {method('set_storage_destructor_throw')}(0);((IDisposable)error).Dispose();
    }}
    Check(Core.TestOutstanding==0,"throwing destructor leaked payload");
    {owner_checks}
    before={method('read_storage_destroyed')}();
    WeakReference abandoned=Abandon();
    for(int i=0;i<8 && (abandoned.IsAlive || Core.TestOutstanding!=0);++i){{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
    Check(!abandoned.IsAlive && Core.TestOutstanding==0,"abandoned native storage leaked");
    Check({method('read_storage_destroyed')}()==before,"finalizer invoked translated destructor");
    {method('install_storage_shutdown')}(&CaptureShutdown);
    {method('install_storage_destructor')}(&DisposalCallback);
    try {{{receiver}.Shutdown();throw new Exception("shutdown discarded new escaped carrier");}}
    catch(InvalidOperationException) {{}}
    Check(retained is IDisposable && Core.TestOutstanding==1,"shutdown carrier retention");
    try {{{method('read_storage_live')}();throw new Exception("ordinary entry accepted deferred shutdown");}}
    catch(ObjectDisposedException) {{}}
    Exception? threadFailure=null;
    var disposer=new global::System.Threading.Thread(()=>{{try {{((IDisposable)retained!).Dispose();}}catch(Exception error){{threadFailure=error;}}}});
    disposer.Start();disposer.Join();if(threadFailure!=null)throw threadFailure;
    Check(disposalCallbacks==1 && Core.TestOutstanding==0,"deferred shutdown disposal/reentry failed");
    {receiver}.Shutdown();{receiver}.Shutdown();
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
            assert actual==traces[0],(mode,runtime,actual)
            for which in range(3):
                result=subprocess.run([*map(str,command),str(which)],capture_output=True,text=True,timeout=30,
                    preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
                (host/f'{runtime}-oom-{which}.txt').write_text(result.stdout+result.stderr)
                assert result.returncode not in (0,90,91) and 'Unable to allocate EDG exception storage' in result.stderr,(which,result.returncode,result.stderr)
            checks.append(f'exception storage/{mode}/{runtime}: C oracle; balanced native allocation, aligned payload, host disposal, shutdown lease, finalizer-only byte release and 3 OOM termination paths')
    return checks
