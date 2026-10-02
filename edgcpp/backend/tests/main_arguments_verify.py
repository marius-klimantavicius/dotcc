"""UTF-8 argc/argv, native storage ownership and executable command lines."""
import json
import os
from pathlib import Path
import sys


def verify_main_arguments(root,tests,out,nativeaot,run):
    checks=[]
    source=tests/'main_arguments.cpp'
    native=out/'main-arguments-native'
    run(['g++','-std=c++17',source,'-o',native]);run([native,'α🙂','','tail'])
    checks.append('main arguments: native C++ UTF-8/mutation/null terminator oracle')
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'main-arguments-{mode}'
        options=['--instance-methods'] if instance else []
        run([sys.executable,root/'dotnet.py',source,'--output-dir',directory,'--namespace','','--class-name','Core',
             '--split','size','--split-size','2100',*options])
        manifest=json.loads((directory/'dot-gen.json').read_text());assert manifest['mainArguments']=='utf8-owned-v1'
        run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',directory/'bin/Release/net10.0/Generated.dll','α🙂','','tail'])
        checks.append(f'main arguments/{mode}/JIT: executable command line includes program name and UTF-8 empty arguments')
        if nativeaot:
            run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            run([directory/'aot/Generated','α🙂','','tail'])
            checks.append(f'main arguments/{mode}/NativeAOT: executable argument forwarding')
        managed=out/f'main-arguments-host-{mode}'
        run([sys.executable,root/'dotnet.py',source,'--output-dir',managed,'--namespace','','--class-name','Core',
             '--emit','managedlib','--split','function',*options])
        manifest=json.loads((managed/'dot-gen.json').read_text())
        report=next(e['method'] for e in manifest['exports'] if e['symbol']=='set_report')
        host=managed/'host';host.mkdir()
        (host/'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>
<TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
</PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        receiver='owner' if instance else 'Core'
        code=f'''using System;
unsafe class EntryPoint {{
 static int Main() {{
  ulong report=0;
  {'var owner=new Core();' if instance else ''}
  {receiver}.{report}(&report);
  if({receiver}.Run(Array.Empty<string>())!=13)return 10;
  string[] arguments=["program", "α🙂", "", "tail"];
  if({receiver}.Run(arguments)!=0)return 11;
  if(arguments[0]!="program" || arguments[3]!="tail")return 12;
  try {{{receiver}.Run(["bad\\0argument"]);return 14;}}catch(ArgumentException){{}}
  try {{{receiver}.Run(null!);return 15;}}catch(ArgumentNullException){{}}
  try {{{receiver}.Run([null!]);return 16;}}catch(ArgumentNullException){{}}
  GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
'''
        if instance:
            code+=f'''  ulong otherReport=0;var other=new Core();other.{report}(&otherReport);
  if(other.Run(["second", "α🙂", "", "tail"])!=0)return 17;
  other.Dispose();if(otherReport!=6080 || report!=0)return 18;
'''
        code+=f'''  {receiver}.Shutdown();if(report!=7080)return 19;
  try {{{receiver}.Run(arguments);return 20;}}catch(ObjectDisposedException){{}}
  return 0;
 }}
}}
'''
        (host/'Program.cs').write_text(code)
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',host/'bin/Release/net10.0/Host.dll'])
        checks.append(f'main arguments/{mode}/JIT: caller strings immutable, mutable native argv, argc0, validation, GC, owner isolation and global-destructor lifetime')
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            run([host/'aot/Host'])
            checks.append(f'main arguments/{mode}/NativeAOT: managed host and destructor lifetime')
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'main-arguments-c-{mode}'
        run([sys.executable,root/'dotnet.py',source,'--language','c','--output-dir',directory,
             *(['--instance-methods'] if instance else [])])
        run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',directory/'bin/Release/net10.0/Generated.dll','α🙂','','tail'])
        checks.append(f'main arguments/C/{mode}/JIT: standard C argc/argv')
        if nativeaot:
            run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            run([directory/'aot/Generated','α🙂','','tail'])
            checks.append(f'main arguments/C/{mode}/NativeAOT: standard C argc/argv')
    return checks


if __name__=='__main__':
    import argparse,subprocess
    parser=argparse.ArgumentParser();parser.add_argument('--output-dir',type=Path,required=True);parser.add_argument('--nativeaot',action='store_true');args=parser.parse_args()
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);commands=[]
    def run(command,expected=0,**kwargs):
        command=list(map(str,command));result=subprocess.run(command,text=True,capture_output=True,**kwargs)
        index=len(commands);(out/f'{index:03}.stdout').write_text(result.stdout);(out/f'{index:03}.stderr').write_text(result.stderr)
        commands.append({'command':command,'exit':result.returncode});(out/'commands.json').write_text(json.dumps(commands,indent=2))
        if result.returncode!=expected:raise RuntimeError(f'{command}\n{result.stdout}\n{result.stderr}')
        return result
    root=Path(__file__).resolve().parents[2]
    checks=verify_main_arguments(root,root/'backend/tests',out,args.nativeaot,run)
    (out/'results.json').write_text(json.dumps(checks,indent=2)+'\n');print(json.dumps(checks,indent=2))
