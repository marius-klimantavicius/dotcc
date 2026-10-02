"""C++ uncaught counts, catch construction and explicit managed escape boundary."""
import json
import os
from pathlib import Path
import shutil
import sys
from libcxx_exception_ptr_verify import write_exception_fixture


def verify_libcxx_exception_counts(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    checks=[]
    clang=shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))),None)
    if not clang:raise RuntimeError('Clang is required for uncaught counts oracle')
    for name,compiler in [('gcc','g++'),('clang',clang)]:
        executable=out/('uncaught-source-'+name)
        run([compiler,'-std=c++20','-O2',tests/'libcxx/uncaught_counts.cpp','-o',executable]);run([executable])
    checks.append('direct GCC/Clang: nested unwinding, catch parameter copy, current_exception outer-or-null, rethrow counts')
    marker=tests/'libcxx/uncaught_copy_marker.cpp';lowered=out/'copy-marker.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','--gen_c_file_name',lowered,
         '-o',out/'copy-marker.il',marker],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    for name,compiler in [('gcc','gcc'),('clang',clang)]:
        executable=out/('copy-marker-edg-'+name)
        run([compiler,'-x','c','-w','-std=gnu11',lowered,'-x','none',root/'build/lib/libC.a','-lm','-o',executable]);run([executable])
    checks.append('original EDG C GCC/Clang: explicit started marker precedes non-elided payload copy')
    for name,compiler in [('gcc','g++'),('clang',clang)]:
        executable=out/('copy-marker-native-'+name)
        run([compiler,'-std=c++20','-DEDG_SOURCE_NATIVE_COPY_COUNT=0',marker,'-o',executable]);run([executable])
    checks.append('direct native GCC/Clang: recorded throw-copy count 0 differs from EDG marker count 1')
    for instance in (False,True):
        mode='instance' if instance else 'static'
        for fixture in ('uncaught_counts','uncaught_copy_marker','exception_counts_host','exception_counts_main'):
            directory=out/(fixture+'-'+mode);combined=out/(fixture+'-'+mode+'.cpp')
            write_exception_fixture(root,tests/('libcxx/'+fixture+'.cpp'),library,combined)
            run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++20',
                 '-I',acquisition/'config','-I',library/'include','-I',library/'src',
                 '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','3200',
                 *(['--instance-methods'] if instance else []),*(['--emit','managedlib'] if fixture.startswith('exception_counts_') else [])])
            project=directory/'Generated.csproj';assembly='Generated';execution=directory
            if fixture.startswith('exception_counts_'):
                exports={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']}
                receiver='owner' if instance else 'Core'
                def method(name):return receiver+'.'+exports.get(name,name)
                execution=directory/'host';execution.mkdir();assembly='Host';project=execution/'Host.csproj'
                project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
                (execution/'Program.cs').write_text(f'''using System;
using System.Runtime.ExceptionServices;
unsafe class Program {{
  static Exception? saved;static int count=-1,handled=-1;
  static void Report({('Core callbackOwner,' if instance else '')}int a,int b) {{count=a;handled=b;}}
  static void Reinject({('Core callbackOwner' if instance else '')}) {{ExceptionDispatchInfo.Capture(saved!).Throw();}}
  static void Foreign({('Core callbackOwner' if instance else '')}) {{throw new InvalidOperationException("foreign");}}
  static void Check(bool ok,string what) {{if(!ok)throw new Exception(what+": "+count+","+handled);}}
  static void Main() {{
    {('using var owner=new Core();' if instance else '')}
    Check({method('counts_now')}()==0 && {method('handled_now')}()==0,"initial");
    for(int nested=0;nested<2;++nested) {{
      count=handled=-1;
      try {{{method('counts_escape')}(&Report);throw new Exception("missing throw");}}
      catch(Exception error) {{saved=error;}}
      Check(count==1 && handled==0,"cleanup before managed escape");
      Check({method('counts_now')}()==0 && {method('handled_now')}()==0,"managed boundary closes uncaught marker");
      count=handled=-1;
      int result=nested==0?{method('counts_callback')}(&Reinject,&Report):{method('counts_nested_callback')}(&Reinject,&Report);
      Check(result==0 && count==1 && handled==nested,"managed reinjection first-pass marker");
      ((IDisposable)saved!).Dispose();saved=null;
      Check({method('counts_now')}()==0 && {method('handled_now')}()==0,"reinjection retired marker");
    }}
    count=handled=-1;
    Check({method('counts_callback')}(&Foreign,&Report)==0 && count==0 && handled==0,"foreign CLR exception is not a C++ uncaught payload");
    {receiver}.ReleaseThread();{receiver}.Shutdown();
    Console.WriteLine("exception count host checks passed");
  }}
}}''')
            if fixture=='exception_counts_main':
                (execution/'Program.cs').write_text(f'''using System;
unsafe class Program {{
  static void Main() {{
    {('using var owner=new Core();' if instance else '')}
    Exception? escaped=null;
    try {{{receiver}.Run();}}catch(Exception error){{escaped=error;}}
    if(escaped==null || {method('counts_now')}()!=0 || {method('main_cleanup_ok')}()!=1)
      throw new Exception("Run host boundary did not complete C++ cleanup/marker");
    ((IDisposable)escaped).Dispose();{receiver}.ReleaseThread();{receiver}.Shutdown();
    Console.WriteLine("Run host boundary passed");
  }}
}}''')
            run(['dotnet','run','--project',project,'-c','Release'])
            checks.append(f'{fixture}: {mode} JIT')
            if nativeaot:
                run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',execution/'aot'])
                run([execution/'aot'/assembly]);checks.append(f'{fixture}: {mode} NativeAOT')
    return checks
