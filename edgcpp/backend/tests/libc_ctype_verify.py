"""Exhaustive C/C++ narrow classification and collation JIT/AOT differential."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_ctype(root,tests,out,nativeaot,run):
    out.mkdir(parents=True,exist_ok=True);checks=[]
    (out/'native.c').write_text('#include <stdio.h>\nunsigned long ctype_digest(int);int ctype_collation(void);int main(void){int i;if(ctype_collation())return 1;for(i=0;i<3;++i)printf("%lu\\n",ctype_digest(i));return 0;}\n')
    run(['gcc','-D_GNU_SOURCE','-std=c11','-O2','-fno-builtin',tests/'libc_ctype.c',out/'native.c','-o',out/'native'])
    # Keep the complete native output as oracle, independent of managed predicates.
    oracle=subprocess.run([str(out/'native')],capture_output=True,text=True,check=True).stdout
    (out/'native-vectors.txt').write_text(oracle);values=[int(x) for x in oracle.splitlines()]
    assert len(values)==3 and all(values) and len(set(values))==1
    checks.append('native exhaustive 257 values x 3 locales x 15 operations plus collation boundaries')
    for language in ['c','c++']:
      for instance in [False,True]:
        directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
        run([sys.executable,root/'dotnet.py',tests/'libc_ctype.c','--language',language,'--target','portable-lp64-binary80-v1','--libc','dotcc','--emit','managedlib','--namespace','','--class-name','Core','--split','size','--split-size','18000','--output-dir',directory,*(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text());host=directory/'host';host.mkdir()
        def call(symbol):return ('owner.' if instance else 'Core.')+next(x['method'] for x in manifest['exports'] if x['symbol']==symbol)
        (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
        (host/'Program.cs').write_text('using System;unsafe class Program{static void Main(){'+('using var owner=new Core();' if instance else '')+f'for(int i=0;i<3;++i)if({call("ctype_digest")}(i)!={values[0]}UL)throw new Exception("classification native mismatch");if({call("ctype_collation")}()!=0)throw new Exception("collation");'+('owner' if instance else 'Core')+'.Shutdown();Console.WriteLine("narrow ctype 11565 vectors and collation boundaries passed");}}')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',host/'bin/Release/net10.0/Host.dll']);checks.append(directory.name+': JIT exhaustive ctype/collation')
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',host/'aot'])
            run([host/'aot/Host']);checks.append(directory.name+': NativeAOT exhaustive ctype/collation')
    return checks

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--output-dir',type=Path,required=True);p.add_argument('--nativeaot',action='store_true');a=p.parse_args()
    out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];checks=[];index=0
    def run(command):
        nonlocal index
        command=list(map(str,command));i=index;index+=1;r=subprocess.run(command,capture_output=True,text=True,timeout=300)
        (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(r.stdout+r.stderr)
        if r.returncode:print(r.stdout+r.stderr)
        r.check_returncode()
    try:checks=verify_libc_ctype(root,root/'backend/tests',out,a.nativeaot,run)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
