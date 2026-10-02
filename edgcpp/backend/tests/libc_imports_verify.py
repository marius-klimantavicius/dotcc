"""Synthetic exact-ABI libc calls/addresses, owner/TLS state, and native memory."""
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_libc_imports(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    headers = root / 'backend/runtime/libc/include'
    source = out / 'libc-imports-oracle.cpp'
    source.write_text('''#include <libc_imports.cpp>
extern "C" int printf(const char*,...);
int main(){for(int i=0;i<24;++i)printf("%d:%d\\n",i,run_libc_case(i));}
''')
    lowered = out / 'libc-imports-oracle.c'
    run([root/'build/bin/cpfe', '--c++17', '--gnu_version', '130300',
         '-I', tests, '-I', headers, '--gen_c_file_name', lowered,
         '-o', out/'libc-imports-oracle.il', source],
        env=dict(os.environ, EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    native = out/'libc-imports-oracle'
    run(['gcc', '-x', 'c', '-w', '-std=gnu11', lowered, '-x', 'none',
         root/'build/lib/libC.a', '-lm', '-o', native])
    expected = run([native]).stdout
    (out/'libc-imports-oracle.txt').write_text(expected)
    assert expected == ''.join(f'{i}:1\n' for i in range(24)), expected
    checks = ['libc imports: 24 synthetic C results verified through original EDG C backend']

    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out/f'libc-imports-{mode}'
        command = [sys.executable, root/'dotnet.py', tests/'libc_imports.cpp',
                   '--emit', 'managedlib', '--libc', 'dotcc', '--output-dir', directory,
                   '--namespace', '', '--class-name', 'Core', '--split', 'function',
                   *(['--instance-methods'] if instance else [])]
        run(command)
        manifest = json.loads((directory/'dot-gen.json').read_text())

        def call(symbol, receiver='owner'):
            methods = [entry['method'] for entry in manifest['exports'] if entry['symbol'] == symbol]
            assert len(methods) == 1, (symbol, manifest['exports'])
            return (receiver+'.' if instance else 'Core.') + methods[0]

        owner_checks = f'''
    using (var other = new Core()) {{
      Require({call('read_libc_errno','other')}() == 0,"second errno starts zero");
      {call('write_libc_errno','other')}(32);
      Require({call('read_libc_errno')}() == 91,"owner errno isolated");
      Require({call('libc_errno_address','other')}() != saved,"owner errno addresses differ");
      Require({call('libc_token_begin')}(0) == 65,"first owner token");
      Require({call('libc_token_begin','other')}(1) == 66,"second owner token");
      Require({call('libc_token_next')}() == 97,"first strtok state isolated");
      Require({call('libc_token_next','other')}() == 98,"second strtok state isolated");
    }}
''' if instance else ''
        host = directory/'host'; host.mkdir(exist_ok=True)
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host/'Program.cs').write_text(f'''using System;
using System.Threading;
unsafe class EntryPoint {{
  {('static readonly Core owner = new Core();' if instance else '')}
  static void Require(bool value,string message) {{ if(!value)throw new Exception(message); }}
  static int Main() {{
    for(int i=0;i<24;++i)Console.WriteLine($"{{i}}:{{{call('run_libc_case')}(i)}}");
    {call('write_libc_errno')}(91);
    int* saved={call('libc_errno_address')}();
    GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    Require(*saved==91 && saved=={call('libc_errno_address')}(),"errno address pinned");
    {owner_checks}
    Exception? failure=null;
    var thread=new Thread(()=>{{try {{
      Require({call('read_libc_errno')}()==0,"thread errno starts zero");
      {call('write_libc_errno')}(73);
      GC.Collect(2,GCCollectionMode.Forced,true,true);
      Require({call('read_libc_errno')}()==73,"thread errno survives GC");
      {('owner' if instance else 'Core')}.ReleaseThread();
      Require({call('read_libc_errno')}()==0,"released thread starts fresh");
      {('owner' if instance else 'Core')}.ReleaseThread();
    }} catch(Exception ex) {{failure=ex;}} }});
    thread.Start();thread.Join();if(failure!=null)throw failure;
    Require({call('read_libc_errno')}()==91,"thread errno isolated");
    {('owner' if instance else 'Core')}.Shutdown();
    bool rejected=false;try {{{call('read_libc_errno')}();}}catch(ObjectDisposedException){{rejected=true;}}
    Require(rejected,"use after shutdown rejected");
    Console.WriteLine("owner/TLS/GC/shutdown:ok");return 0;
  }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        commands = [('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host']))
        for runtime, invocation in commands:
            actual = run(invocation,timeout=90).stdout
            (host/f'{runtime}-trace.txt').write_text(actual)
            assert actual == expected+'owner/TLS/GC/shutdown:ok\n', (mode,runtime,actual)
            checks.append(f'libc imports/{mode}/{runtime}: exact direct/address ABI, memory/string/math/allocation, owner/TLS errno and disposal')

    # Both declarations are syntactically valid but incompatible with the fixed
    # provider ABI. Failing before C# output avoids address-call stack corruption.
    for label, declaration, expression in [
        ('wrong-result','extern "C" int strlen(const char*);','strlen("x")'),
        ('wrong-argument','extern "C" void* malloc(unsigned int);','malloc(8)!=0'),
    ]:
        probe = out/f'libc-{label}.cpp'; probe.write_text(declaration+'\nint main(){return '+expression+';}\n')
        result = subprocess.run(list(map(str,[sys.executable,root/'dotnet.py',probe,'--libc','dotcc',
                      '--output-dir',out/f'libc-{label}'])),text=True,capture_output=True)
        (out/f'libc-{label}.log').write_text(result.stdout+result.stderr)
        assert result.returncode != 0 and 'ABI' in result.stderr, result.stderr
    checks.append('libc imports: incompatible result/parameter widths rejected before emission')
    probes = [
        ('c-headers', 'c', [], '''#include <stddef.h>
#include <stdint.h>
#include <limits.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
_Static_assert(sizeof(size_t)==8 && sizeof(long)==8 && CHAR_BIT==8,"LP64");
int main(void){char *p=malloc(8);if(!p)return 1;strcpy(p,"abc");int ok=strlen(p)==3;free(p);return !ok;}
'''),
        ('bootstrap', 'c++', ['--runtime','edg-exceptions'], '''#include <new>
#include <stdlib.h>
#include <errno.h>
int main(){int *p=new int(37);void *q=malloc(16);int good=*p==37 && q;delete p;free(q);return !good;}
'''),
        ('external-variable', 'c++', [], '''extern "C" int errno;
int main(){errno=17;int *p=&errno;*p=29;return errno!=29;}
'''),
    ]
    for label, language, options, code in probes:
        probe = out/f'libc-{label}.{("c" if language=="c" else "cpp")}'
        probe.write_text(code); directory = out/f'libc-{label}'
        run([sys.executable,root/'dotnet.py',probe,'--language',language,'--libc','dotcc',
             '--output-dir',directory,*options])
        run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
        if nativeaot:
            run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            run([directory/'aot/Generated'])
    checks.append('libc imports: C11 portable headers, C++ allocation bootstrap coexistence, external errno lvalue')
    return checks
