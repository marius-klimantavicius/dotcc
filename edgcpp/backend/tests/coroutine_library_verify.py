"""Managed library coroutine factories, owner isolation and shutdown lifetimes."""
import json
from pathlib import Path
import shutil
import sys


def verify_coroutine_library(root, tests, out, nativeaot, run, acquisition=None, instances=(False, True)):
    acquisition = acquisition or root / 'artifacts/campaign/libcxx-acquired-003'
    source = out / 'input.cpp'
    shutil.copy2(tests / 'libcxx/coroutine_library.cpp', source)
    clang = shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))), None)
    if not clang:
        raise RuntimeError('Clang required for coroutine source oracle')
    for label, compiler in [('gcc', 'g++'), ('clang', clang)]:
        for optimization in ('-O0', '-O2'):
            executable = out / (label + optimization)
            run([compiler, '-std=c++20', optimization, '-DNATIVE_LIBRARY_ORACLE', source, '-o', executable])
            run([executable])
    checks = ['Native GCC/Clang O0/O2: direct POD factory, resume, early destroy and global task destruction']
    for instance in instances:
        mode = 'instance' if instance else 'static'
        directory = out / mode
        run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--libc', 'dotcc',
             '-I', acquisition / 'config', '-I', acquisition / 'libcxx-18.1.8.src/include',
             '--output-dir', directory, '--namespace', 'CoroutineAcceptance', '--class-name', 'Core',
             '--emit', 'managedlib', '--split', 'size', '--split-size', '3200',
             *(['--instance-methods'] if instance else [])])
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        exports = {item['symbol']: item['method'] for item in manifest['exports']}
        create, resume, destroy, total, live, observe = [exports[name] for name in
            ('create_task', 'resume_task', 'destroy_task', 'total_value', 'live_count', 'set_observer')]
        host = directory / 'host'
        host.mkdir()
        (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        receiver = 'first' if instance else 'Core'
        setup = 'using var first = new Core();' if instance else ''
        shutdown = 'first.Dispose();' if instance else 'Core.Shutdown();'
        second_setup = f'''using var second = new Core();second.{observe}(2,&Notify);
var secondTask=second.{create}(4);
Require(second.{resume}(&secondTask)==0 && second.{total}()==4,"second owner first resume");''' if instance else ''
        second_finish = f'''Require(second.{total}()==4,"second owner survived first shutdown");
Require(second.{resume}(&secondTask)==1 && second.{total}()==44,"second owner completion");
second.{destroy}(&secondTask);second.Dispose();
Require(events.Count==5 && events[3]==2004 && events[4]==2007,"second destruction order");''' if instance else ''
        owner_field = 'static Core? closingOwner;' if instance else 'static bool closingOwner;'
        owner_set = 'closingOwner=first;' if instance else 'closingOwner=true;'
        owner_clear = 'closingOwner=null;' if instance else 'closingOwner=false;'
        callback_parameters = 'Core owner,int value' if instance else 'int value'
        callback_check = f'if(value==1007 && closingOwner!=null)Require(closingOwner.{live}()==1,"reentrant shutdown observer");' if instance else f'if(value==1007 && closingOwner)Require(Core.{live}()==1,"reentrant shutdown observer");'
        result = 'return new WeakReference(first);' if instance else 'return null;'
        (host / 'Program.cs').write_text(f'''using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CoroutineAcceptance;
unsafe class EntryPoint {{
 static readonly List<int> events=new();
 {owner_field}
 static void Require(bool value,string message){{if(!value)throw new Exception(message+":"+string.Join(",",events));}}
 static void Notify({callback_parameters}){{events.Add(value);{callback_check}}}
 [MethodImpl(MethodImplOptions.NoInlining)]
 static WeakReference? Exercise(){{
  {setup}
  {receiver}.{observe}(1,&Notify);
  Require({receiver}.{live}()==1 && {receiver}.{total}()==0,"global owning task initialized");
  var task={receiver}.{create}(3);
  Require({receiver}.{live}()==2,"factory returned suspended promise");
  Require({receiver}.{resume}(&task)==0 && {receiver}.{total}()==3,"first suspension");
  {second_setup}
  GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
  Require({receiver}.{resume}(&task)==1 && {receiver}.{total}()==33,"resume after GC");
  {receiver}.{destroy}(&task);
  var early={receiver}.{create}(5);{receiver}.{destroy}(&early);
  Require(events.Count==2 && events[0]==1003 && events[1]==1005,"explicit destruction");
  _={receiver}.{create}(99); // Deliberately leaked raw frame: shutdown must not invent its promise destructor.
  {owner_set}{shutdown}{owner_clear}
  Require(events.Count==3 && events[2]==1007,"global owning task destroyed before frame abandonment");
  bool rejected=false;try{{{receiver}.{live}();}}catch(ObjectDisposedException){{rejected=true;}}
  Require(rejected,"entry after owner shutdown");
  {second_finish}
  {result}
 }}
 static int Main(){{var weak=Exercise();for(int i=0;i<3;++i){{GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}}
  Require(weak==null || !weak.IsAlive,"disposed owner retained by abandoned coroutine registry");return 0;}}
}}''')
        project = host / 'Host.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
        checks.append(mode + '-jit: exported factory, owner/GC lifetime, callbacks, global destruction and raw-frame abandonment')
        if nativeaot:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run([host / 'aot/Host'])
            checks.append(mode + '-aot: exported coroutine factory and deterministic owner shutdown')
    return checks
