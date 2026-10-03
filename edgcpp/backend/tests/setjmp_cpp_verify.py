"""Source longjmp bypasses C++ handlers and preserves owner-entry bookkeeping."""
import json
import os
from pathlib import Path
import shutil
import sys
from libcxx_exception_ptr_verify import write_exception_fixture


def verify_setjmp_cpp(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    checks=[];source=tests/'libcxx/setjmp_cpp.cpp';oracle=out/'oracle.cpp'
    oracle.write_text('#include '+json.dumps(str(source.resolve()))+'\nint main(int argc,char**argv){return !run_nonlocal_cpp(argv[1][0]-\'0\',jump_other_owner);}\n')
    clang=shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))),None)
    if not clang:raise RuntimeError('Clang required for C++ nonlocal jump oracle')
    for label,compiler in [('gcc','g++'),('clang',clang)]:
        executable=out/('native-'+label);run([compiler,'-std=c++14','-DEDG_JUMP_NATIVE_ORACLE=1',oracle,'-o',executable])
        for case in range(5):run([executable,str(case)])
    checks.append('C++ nonlocal jumps: native GCC/Clang catch/noexcept/spec bypass and separately recorded raw-jump active-handler observation')
    include=out/'edg-oracle-include';include.mkdir()
    for name in ('exception','edg_internal_defines'):
        (include/name).write_bytes((root/'upstream/include_c++'/(name+'.stdh')).read_bytes())
    # Match original cpfe's configured portable-EH buffer declaration. Glibc's
    # struct-array spelling is ABI-compatible, but conflicts with the generic
    # backend's additional `_setjmp(long[25])` declaration in the generated C.
    # This header is confined to the Linux-x64 original-C oracle.
    (include/'setjmp.h').write_text('''typedef long jmp_buf[25];
extern "C" int _setjmp(jmp_buf);
extern "C" void longjmp(jmp_buf,int);
#define setjmp(environment) _setjmp(environment)
''')
    run([root/'build/bin/cpfe','--c++14','--gnu_version','130300','-I',include,'-I','/usr/include/x86_64-linux-gnu',
         '-DEDG_JUMP_C_ORACLE=1','--gen_c_file_name',out/'oracle.c','-o',out/'oracle.il',oracle],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    # Use the C driver: clang++ -x c still links native libstdc++, whose exit
    # finalizers can interpose on the demonstration runtime's std EH symbols.
    clang_c=clang[:-2] if clang.endswith('++') else clang
    for label,compiler in [('gcc','gcc'),('clang',clang_c)]:
        executable=out/('edg-'+label)
        run([compiler,'-x','c','-std=gnu11','-w',out/'oracle.c','-x','none',root/'build/lib/libC.a','-lm','-o',executable])
        for case in range(5):run([executable,str(case)])
    checks.append('C++ nonlocal jumps: original EDG C GCC/Clang isolated-case native-jump observations')
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/mode;combined=out/(mode+'.cpp')
        write_exception_fixture(root,source,library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++14','--emit','managedlib',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src','--output-dir',directory,
             '--namespace','','--class-name','Core','--split','size','--split-size','3200',*(['--instance-methods'] if instance else [])])
        exports={e['symbol']:e['method'] for e in json.loads((directory/'dot-gen.json').read_text())['exports']}
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        receiver='owner' if instance else 'Core';jump_receiver='other!' if instance else 'Core'
        (host/'Program.cs').write_text(f'''using System;
unsafe class Program {{
  {('static Core? expected,other;' if instance else '')}
  static int callbacks,completed;
  static void Bridge({('Core callbackOwner,' if instance else '')}void* environment) {{
    {('if(!ReferenceEquals(expected,callbackOwner))throw new Exception("wrong nonlocal callback owner");' if instance else '')}
    ++callbacks;GC.Collect(2,GCCollectionMode.Forced,true,true);
    try {{{jump_receiver}.{exports['jump_other_owner']}(environment);throw new Exception("longjmp returned");}}
    finally {{++completed;}}
  }}
  static void Main() {{
    {('var owner=new Core();expected=owner;other=new Core();' if instance else '')}
    for(int iteration=0;iteration<10;++iteration)
      for(int which=0;which<5;++which)
        if({receiver}.{exports['run_nonlocal_cpp']}(which,&Bridge)!=1)throw new Exception("C++ nonlocal case "+which);
    if(callbacks!=10 || completed!=10)throw new Exception("managed callback finally contract");
    {receiver}.ReleaseThread();{receiver}.Shutdown();
    {('other.ReleaseThread();other.Shutdown();' if instance else '')}
    Console.WriteLine("C++ nonlocal and owner-entry checks passed");
  }}
}}''')
        project=host/'Host.csproj';run(['dotnet','run','--project',project,'-c','Release'])
        checks.append(f'C++ nonlocal jumps: {mode} JIT catch bypass, current_exception cleanup, owner reentry and host finally')
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host'])
            checks.append(f'C++ nonlocal jumps: {mode} NativeAOT')
    return checks
