"""Synthetic native and translated wide FILE/fpos version2 contracts."""
import argparse,json,shutil,subprocess,sys
from pathlib import Path

def verify_libc_wide_files(root,tests,out,nativeaot,run):
 out.mkdir(parents=True,exist_ok=True);fixture=out/'fixture.c';shutil.copy2(tests/'libc_wide_files.c',fixture)
 (out/'native.c').write_text('#include <stdio.h>\nint wide_file_cases(const char*);int main(int argc,char**argv){if(argc!=2)return 99;int r=wide_file_cases(argv[1]);printf("wide trace: %d\\n",r);return r;}\n')
 run(['gcc','-std=c11','-O2',fixture,out/'native.c','-o',out/'native']);run([out/'native',out/'native-data.bin']);checks=['native wide FILE locale/position/pushback/string/stdstream trace']
 for language in ['c','c++']:
  for instance in [False,True]:
   directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
   run([sys.executable,root/'dotnet.py',fixture,'--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','30000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
   metadata=json.loads((directory/'dot-gen.json').read_text());method=next(item['method'] for item in metadata['exports'] if item['symbol']=='wide_file_cases');host=directory/'host';host.mkdir()
   (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
   other=f'using var other=new Core();if(other.{method}((sbyte*)path)!=0)throw new Exception("other owner");' if instance else ''
   (host/'Program.cs').write_text(f'''using System;using System.Text;using System.Runtime.InteropServices;unsafe class Program{{static void Main(string[] args){{
{('using var owner=new Core();' if instance else '')}byte[] bytes=Encoding.UTF8.GetBytes(args[0]);byte* path=(byte*)NativeMemory.Alloc((nuint)bytes.Length+1);try{{bytes.CopyTo(new Span<byte>(path,bytes.Length));path[bytes.Length]=0;
int result={('owner' if instance else 'Core')}.{method}((sbyte*)path);if(result!=0)throw new Exception($"wide file case {{result}}");{other}
}}finally{{NativeMemory.Free(path);{('owner' if instance else 'Core')}.Shutdown();}}Console.WriteLine("wide FILE imports, captured locale, fpos2, pushback, orientation, standard cells and owner trace passed");}}}}
''')
   run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true']);run(['dotnet',host/'bin/Release/net10.0/Host.dll',directory/'jit-data.bin']);checks.append(directory.name+': JIT wide FILE/fpos2')
   if nativeaot:
    run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot']);run([host/'aot/Host',directory/'aot-data.bin']);checks.append(directory.name+': NativeAOT wide FILE/fpos2')
 return checks
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
 def run(command):
  global index
  command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300);(out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
  if r.returncode:print(r.stdout+r.stderr)
  r.check_returncode()
 try:checks=verify_libc_wide_files(root,root/'backend/tests',out,a.nativeaot,run)
 finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
 print(json.dumps(checks,indent=2))
