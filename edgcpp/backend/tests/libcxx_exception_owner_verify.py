"""Owner-local, cross-owner and escaped persistent C++ exception references."""
import json
from pathlib import Path
import sys
from libcxx_exception_ptr_verify import write_exception_fixture


def verify_libcxx_exception_owner(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    checks=[]
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/('exception-owner-'+mode)
        combined=out/('exception-owner-'+mode+'.cpp')
        write_exception_fixture(root,tests/'libcxx/exception_owner.cpp',library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++20','--emit','managedlib',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','3200',
             *(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def method(function,receiver='owner'):
            names=[e['method'] for e in manifest['exports'] if e['symbol'].startswith(f'_Z{len(function)}{function}')]
            assert len(names)==1,(function,names)
            return (receiver+'.' if instance else 'Core.')+names[0]
        original=directory/'uninstrumented';original.mkdir()
        for p in directory.glob('Translated*.cs'):
            source=p.read_text();(original/p.name).write_text(source)
            p.write_text(source.replace('global::System.Runtime.InteropServices.NativeMemory.Alloc(', '__edg_test_alloc(')
                         .replace('global::System.Runtime.InteropServices.NativeMemory.Free(', '__edg_test_free('))
        (directory/'TestAccess.cs').write_text('''using System;
public '''+('' if instance else 'static ')+'''unsafe partial class Core {
  private static readonly object TestSync=new();
  private static readonly global::System.Collections.Generic.HashSet<nint> Allocations=new();
  public static int Outstanding {get {lock(TestSync)return Allocations.Count;}}
  public '''+('' if instance else 'static ')+'''int Payloads {get {lock(__edg_sync)return __edg_exception_leases;}}
  public '''+('' if instance else 'static ')+'''int References {get {lock(__edg_sync)return __edg_exception_references;}}
  private static void* __edg_test_alloc(nuint size) {
    void* p=global::System.Runtime.InteropServices.NativeMemory.Alloc(size);
    lock(TestSync)if(!Allocations.Add((nint)p))throw new Exception("duplicate allocation");return p;
  }
  private static void __edg_test_free(void* p) {
    if(p==null)return;
    lock(TestSync)if(!Allocations.Remove((nint)p))throw new Exception("duplicate/unowned free");
    global::System.Runtime.InteropServices.NativeMemory.Free(p);
  }
}
''')
        project=directory/'Generated.csproj'
        project.write_text(project.read_text().replace('</Project>','<ItemGroup><Compile Include="TestAccess.cs" /></ItemGroup></Project>'))
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        receiver='owner' if instance else 'Core'
        cross=''
        if instance:
            cross=f'''
    var a=new Core();var b=new Core();
    {method('set_observer','a')}(&Observe,(void*)2);{method('set_observer','b')}(&Observe,(void*)3);
    {method('seed','a')}(41);{method('copy_saved','b')}({method('saved_address','a')}());{method('clear_saved','a')}();
    Reset();Rejected(a.Shutdown);Check(Events.Count==0,"cross-owner shutdown ran globals");
    {method('clear_saved','b')}();Check(Has(1041,2),"cross-owner payload destructor lost originating globals");
    b.Shutdown();a.Shutdown();Check(Core.Outstanding==0,"cross-owner global handle leaked");
    a=new Core();b=new Core();{method('set_observer','a')}(&Observe,(void*)4);{method('set_observer','b')}(&Observe,(void*)5);
    {method('seed','a')}(42);{method('copy_to_owned_heap','b')}({method('saved_address','a')}());{method('clear_saved','a')}();
    Reset();Rejected(a.Shutdown);b.Shutdown();Check(Has(1042,4),"foreign heap handle invalidated origin");a.Shutdown();
    Check(Core.Outstanding==0,"cross-owner heap handle leaked");
    a=new Core();{method('set_observer','a')}(&Observe,(void*)6);{method('seed','a')}(43);
    Reset();var abandoned=Abandon(a);
    for(int i=0;i<8 && abandoned.IsAlive;++i){{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
    Check(!abandoned.IsAlive && a.Payloads==1 && a.References==0,"abandoned wrapper retained or destroyed shared payload");
    a.Shutdown();Check(Has(1043,6),"wrapper abandonment suppressed surviving handle destructor");
    Check(Core.Outstanding==0,"abandoned wrapper/global handle leaked");
'''
        helpers=''
        if instance:
            helpers=f'''[MethodImpl(MethodImplOptions.NoInlining)]
  static WeakReference Abandon(Core owner) {{try {{{method('raise_saved')}();}}catch(Exception e){{return new WeakReference(e);}}throw new Exception("did not throw");}}
'''
        program=f'''using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
unsafe class Program {{
  static readonly object Sync=new();static readonly List<(int Tag,int Owner)> Events=new();
  static void Check(bool value,string message){{if(!value)throw new Exception(message);}}
  static void Observe({('Core origin,' if instance else '')}int tag,void* state){{lock(Sync)Events.Add((tag,(int)(nint)state));}}
  static bool Has(int tag,int owner){{lock(Sync)return Events.Contains((tag,owner));}}
  static void Reset(){{lock(Sync)Events.Clear();}}
  static void Rejected(Action action){{try{{action();}}catch(InvalidOperationException){{return;}}throw new Exception("shutdown accepted live external reference");}}
  {helpers}
  static void Main() {{
    {('var owner=new Core();' if instance else '')}
    {method('set_observer')}(&Observe,(void*)1);{method('seed')}(10);{method('transfer_saved')}();
    Check({receiver}.Payloads==1 && {receiver}.References==0,"global move/swap retained external references");
    {method('fill_tls')}(11);Reset();{receiver}.ReleaseThread();
    Check(Has(1011,1) && {receiver}.Payloads==1 && {receiver}.References==0,"TLS handle teardown");
    Exception? workerFailure=null;var workers=new Thread[4];
    for(int i=0;i<workers.Length;++i) {{workers[i]=new Thread(()=>{{try {{
      for(int j=0;j<24;++j)try {{{method('raise_saved')}();}}catch(Exception e){{((IDisposable)e).Dispose();}}
      {receiver}.ReleaseThread();
    }}catch(Exception e){{Interlocked.CompareExchange(ref workerFailure,e,null);}}}});workers[i].Start();}}
    foreach(var worker in workers)worker.Join();if(workerFailure!=null)throw workerFailure;
    Check({receiver}.Payloads==1 && {receiver}.References==0,"concurrent fresh rethrow wrappers leaked");
    Exception escaped;
    try {{{method('raise_saved')}();throw new Exception("did not throw");}}catch(Exception e){{escaped=e;}}
    {method('clear_saved')}();Reset();Rejected({receiver}.Shutdown);Check(Events.Count==0,"escaped carrier shutdown ran globals");
    ((IDisposable)escaped).Dispose();((IDisposable)escaped).Dispose();
    Check(Has(1010,1) && {receiver}.Payloads==0,"escaped carrier did not release payload");
    {method('seed')}(12);{method('copy_to_leaked_heap')}();{method('clear_saved')}();
    {method('capture_nested')}();{method('copy_to_owned_heap')}({method('saved_address')}());{method('transfer_saved')}();Reset();
    Check({receiver}.References==0,"internal nested/heap handles classified external");
    {receiver}.Shutdown();Check(Has(1021,1) && Has(1022,1) && Has(-900,1),"normal nested/global cleanup incomplete");
    Check(!Has(1012,1),"invented destructor for leaked C++ heap object");
    Check({receiver}.Payloads==0 && {receiver}.References==0 && Core.Outstanding==0,"owner shutdown retained native payload/heap bytes");
    {cross}
    Console.WriteLine("exception owner checks passed");
  }}
}}
'''
        (host/'Program.cs').write_text(program)
        run(['dotnet','run','--project',host/'Host.csproj','-c','Release'])
        checks.append(f'libc++ exception owners: {mode} JIT global/TLS/heap/nested/concurrent/host ownership')
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            run([host/'aot/Host'])
            checks.append(f'libc++ exception owners: {mode} NativeAOT')
    return checks
