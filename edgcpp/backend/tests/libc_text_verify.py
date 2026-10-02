"""Owner text runtime and synthetic UTF-8 native differential."""
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
    shutil.copy2(tests/'libc_text_native.c',out/'native.c');shutil.copy2(tests/'libc_text_runtime.cs',out/'Program.cs')
    (out/'Text.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>
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
        run(['gcc','-std=c11','-O2','-frounding-math',out/'native.c','-lm','-o',out/'native'])
        vectors=run([out/'native']);(out/'vectors.txt').write_text(vectors)
        run(['dotnet','build',out/'Text.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        result=run(['dotnet',out/'bin/Release/net10.0/Text.dll',out/'vectors.txt'])
        checks.append({'name':'managed JIT UTF-8/owner text contracts','result':result.strip()})
        if args.nativeaot:
            run(['dotnet','publish',out/'Text.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
            aot=run([out/'aot/Text',out/'vectors.txt']);assert aot==result
            checks.append({'name':'managed NativeAOT UTF-8/owner text contracts','result':aot.strip()})
    finally:
        (out/'results.json').write_text(json.dumps({'checks':checks,'scope':'bounded C/C.UTF-8 provider; not arbitrary locales or complete multibyte library'},indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
