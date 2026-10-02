"""Synthetic byte-stream FILE behavior; no real project inputs."""
import json
import os
from pathlib import Path
import sys


def verify_libc_files(root, tests, out, nativeaot, run):
    out.mkdir(parents=True,exist_ok=True)
    # Keep managed runtime error injection/concurrency tests separate from the
    # native oracle. Native FILE/fpos_t layouts never cross the managed boundary.
    standalone=out/'libc-files-runtime';standalone.mkdir(exist_ok=True)
    sources=''.join(f'<Compile Include="{root / "backend/runtime" / name}" />' for name in ('LibcFiles.cs','LibcAppend.cs','LibcPaths.cs'))
    (standalone/'Runtime.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup>{sources}</ItemGroup></Project>''')
    (standalone/'Program.cs').write_text((tests/'libc_files_runtime.cs').read_text())
    run(['dotnet','build',standalone/'Runtime.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
    output=run(['dotnet',standalone/'bin/Release/net10.0/Runtime.dll',standalone/'jit-data']).stdout
    (standalone/'jit.log').write_text(output)
    checks=['libc FILE/runtime/JIT: byte buffering, injected failures, owner globals, 4000 concurrent append records']
    if nativeaot:
        run(['dotnet','publish',standalone/'Runtime.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
             '-p:IlcTreatWarningsAsErrors=true','-o',standalone/'aot'])
        actual=run([standalone/'aot/Runtime',standalone/'aot-data']).stdout
        (standalone/'aot.log').write_text(actual);assert actual==output
        checks.append('libc FILE/runtime/NativeAOT: buffering/errors/ownership/concurrent append')

    headers=root/'backend/runtime/libc/include'
    native_data=out/'libc-files-native-data';native_data.mkdir(exist_ok=True)
    oracle=out/'libc-files-oracle.cpp'
    # Do not call file_positions against the host's incompatible fpos_t ABI.
    oracle.write_text('#include <libc_files.cpp>\nextern "C" int printf(const char*,...);\nint main(){'+
        ''.join(f'printf("{i}:%d\\n",run_file_case({i},{json.dumps(str(native_data/f"case-{i}.bin"))}));' for i in range(11))+'}\n')
    lowered=out/'libc-files-oracle.c'
    run([root/'build/bin/cpfe','--c++17','--gnu_version','130300','-I',tests,'-I',headers,
         '-I',root/'backend/runtime/libc/include','--gen_c_file_name',lowered,'-o',out/'libc-files-oracle.il',oracle],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    run(['gcc','-x','c','-w','-std=gnu11',lowered,'-x','none',root/'build/lib/libC.a','-o',out/'libc-files-oracle'])
    expected=run([out/'libc-files-oracle']).stdout
    (out/'libc-files-oracle.txt').write_text(expected)
    assert expected==''.join(f'{i}:1\n' for i in range(11)),expected
    checks.append('libc FILE: 11 original EDG C byte/position/append traces')

    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'libc-files-{mode}'
        run([sys.executable,root/'dotnet.py',tests/'libc_files.cpp','--emit','managedlib','--libc','dotcc',
             '-I',headers,'--output-dir',directory,'--namespace','','--class-name','Core',
             *(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        def call(symbol,receiver='owner'):
            methods=[entry['method'] for entry in manifest['exports'] if entry['symbol']==symbol]
            assert len(methods)==1,(symbol,manifest['exports'])
            return (receiver+'.' if instance else 'Core.')+methods[0]
        host=directory/'host';host.mkdir(exist_ok=True)
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        owners=f'using var other=new Core();if((nint){call("stdio_global_address","other")}()==saved)throw new Exception("owner standard globals alias");' if instance else ''
        (host/'Program.cs').write_text(f'''using System;using System.IO;using System.Text;
unsafe class Program {{
  {('static readonly Core owner=new Core();' if instance else '')}
  static int Main(string[] args) {{
    Directory.CreateDirectory(args[0]);
    for(int i=0;i<11;++i) {{byte[] path=Encoding.UTF8.GetBytes(Path.Combine(args[0],$"case-{{i}}.bin")+"\\0");
      fixed(byte* p=path)Console.WriteLine($"{{i}}:{{{call('run_file_case')}(i,(sbyte*)p)}}");}}
    if({call('stdio_globals_distinct')}()!=1)throw new Exception("standard globals");
    nint saved=(nint){call('stdio_global_address')}();GC.Collect(2,GCCollectionMode.Forced,true,true);
    if(saved!=(nint){call('stdio_global_address')}())throw new Exception("stdio global moved");
    {owners}
    byte[] positionPath=Encoding.UTF8.GetBytes(Path.Combine(args[0],"positions.bin")+"\\0");
    fixed(byte* p=positionPath)if({call('file_positions')}((sbyte*)p)!=1)throw new Exception("fpos");
    {('owner' if instance else 'Core')}.Shutdown();Console.WriteLine("stdio globals/position:ok");return 0;
  }}
}}''')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        commands=[('JIT',['dotnet',host/'bin/Release/net10.0/Host.dll',host/'jit-data'])]
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            commands.append(('NativeAOT',[host/'aot/Host',host/'aot-data']))
        for runtime,command in commands:
            actual=run(command,timeout=90).stdout;(host/f'{runtime}.log').write_text(actual)
            assert actual==expected+'stdio globals/position:ok\n',(mode,runtime,actual)
            checks.append(f'libc FILE/{mode}/{runtime}: 11 native traces, opaque FILE call addresses, owner standard globals and fpos')
    return checks
