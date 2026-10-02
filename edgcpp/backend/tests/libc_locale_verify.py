"""Bounded locale handles, owner/TLS and pinned lconv contracts."""
import argparse
import json
from pathlib import Path
import shutil
import subprocess


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',required=True,type=Path)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    root=Path(__file__).resolve().parents[2];runtime=root/'backend/runtime';tests=root/'backend/tests'
    for name in ['LibcContext.cs','LibcText.cs','LibcErrorMessages.cs','LibcLocale.cs']:shutil.copy2(runtime/name,out)
    shutil.copy2(tests/'libc_locale_runtime.cs',out/'Program.cs')
    (out/'Locale.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>
<TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>''')
    checks=[];index=0
    def run(command,timeout=180):
        nonlocal index
        command=list(map(str,command));current=index;index+=1
        result=subprocess.run(command,text=True,capture_output=True,timeout=timeout)
        (out/f'{current:02}.command.json').write_text(json.dumps(command,indent=2))
        (out/f'{current:02}.log').write_text(result.stdout+result.stderr)
        if result.returncode:print(result.stdout+result.stderr)
        result.check_returncode();return result.stdout
    try:
        run(['dotnet','build',out/'Locale.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        result=run(['dotnet',out/'bin/Release/net10.0/Locale.dll',out/'vectors.txt'])
        checks.append({'name':'managed JIT locale/owner/TLS contracts','result':result.strip()})
        if args.nativeaot:
            run(['dotnet','publish',out/'Locale.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
            aot=run([out/'aot/Locale',out/'vectors.txt']);assert aot==result
            checks.append({'name':'managed NativeAOT locale/owner/TLS contracts','result':aot.strip()})
    finally:
        (out/'results.json').write_text(json.dumps({'checks':checks,'scope':'bounded C/C.UTF-8 provider; not arbitrary locales or complete multibyte library'},indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
