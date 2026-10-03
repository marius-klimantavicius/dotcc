"""Standalone wide FILE prototype against native UTF8 scalar bytes."""
import argparse,json,shutil,subprocess
from pathlib import Path
from libc_c_locale_oracle import build_c_locale_oracle
p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--source-dir',type=Path);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();root=Path(__file__).resolve().parents[2];out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);source=a.source_dir or root/'backend/runtime'
for name in ['LibcFiles.cs','LibcFilesWide.cs']:shutil.copy2(source/name,out/name)
for name in ['LibcAppend.cs','LibcPaths.cs']:shutil.copy2(root/'backend/runtime'/name,out/name)
shutil.copy2(root/'backend/tests/libc_wide_files_runtime.cs',out/'Program.cs')
(out/'WideFiles.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup></Project>')
(out/'native.c').write_text('#include <stdio.h>\n#include <wchar.h>\n#include <locale.h>\nint main(int argc,char**argv){if(argc!=2||!setlocale(LC_ALL,"C.UTF-8"))return 1;FILE*f=fopen(argv[1],"wb");if(!f||fwide(f,1)<=0)return 2;for(unsigned c=0;c<=0x10ffff;++c)if(c<0xd800||c>0xdfff)if(fputwc(c,f)!=c)return 3;return fclose(f);}\n')
checks=[];index=0
def run(command):
 global index
 command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,text=True,capture_output=True,timeout=240);(out/f'{i:02}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:02}.log').write_text(r.stdout+r.stderr)
 if r.returncode:print(r.stdout+r.stderr)
 r.check_returncode();return r.stdout
try:
 mapping=build_c_locale_oracle(root,out/'c-locale',run);checks.append('original musl all256 C-locale byte mappings and reverse encoding')
 run(['gcc','-std=c11','-O2',out/'native.c','-o',out/'native']);run([out/'native',out/'native.bin']);checks.append('native all Unicode scalar UTF8 bytes')
 run(['dotnet','build',out/'WideFiles.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',out/'bin/Release/net10.0/WideFiles.dll',out/'native.bin',out/'jit.bin',mapping]);checks.append('JIT exhaustive scalar and wide FILE contracts')
 if a.nativeaot:
  run(['dotnet','publish',out/'WideFiles.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot']);run([out/'aot/WideFiles',out/'native.bin',out/'aot.bin',mapping]);checks.append('NativeAOT exhaustive scalar and wide FILE contracts')
finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
print(json.dumps(checks,indent=2))
