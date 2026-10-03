"""Isolated prototype of Wide-oriented FILE scanning locks using managed function pointers."""
import argparse,json,shutil,subprocess
from pathlib import Path


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output-dir',type=Path,required=True);parser.add_argument('--nativeaot',action='store_true');args=parser.parse_args()
    root=Path(__file__).resolve().parents[2];out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    for name in ['LibcFiles.cs','LibcFilesWide.cs','LibcFilesWideScan.cs','LibcAppend.cs','LibcPaths.cs','VaList.cs']:shutil.copy2(root/'backend/runtime'/name,out/name)
    # During the campaign freeze the active class remains unchanged. Apply only
    # this mechanical declaration change to the preserved isolated prototype.
    source=out/'LibcFiles.cs';s=source.read_text().replace('public sealed unsafe class LibcFiles','public sealed unsafe partial class LibcFiles');source.write_text(s)
    shutil.copy2(root/'backend/tests/libc_files_wide_scan_runtime.cs',out/'Program.cs')
    (out/'ScanTest.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>''')
    i=0;checks=[]
    def run(command):
        nonlocal i
        command=list(map(str,command));n=i;i+=1;r=subprocess.run(command,text=True,capture_output=True,timeout=180)
        (out/f'{n:02}.command.json').write_text(json.dumps(command,indent=2));(out/f'{n:02}.log').write_text(r.stdout+r.stderr)
        if r.returncode:print(r.stdout+r.stderr)
        r.check_returncode();return r.stdout
    try:
        run(['dotnet','build',out/'ScanTest.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        output=run(['dotnet',out/'bin/Release/net10.0/ScanTest.dll']);checks.append({'runtime':'JIT','result':output.strip()})
        if args.nativeaot:
            run(['dotnet','publish',out/'ScanTest.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
            aot=run([out/'aot/ScanTest']);assert aot==output;checks.append({'runtime':'NativeAOT','result':aot.strip()})
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
