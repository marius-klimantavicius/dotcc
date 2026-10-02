"""Synthetic C/C++ string/memory GNU builtin identity and canonical addresses."""
import argparse,json,subprocess,sys
from pathlib import Path

def verify_libc_string_builtins(root,tests,out,nativeaot,run):
    checks=[]
    for language in ['c','c++']:
        for instance in [False,True]:
            directory=out/f'{language.replace("+","x")}-{"instance" if instance else "static"}'
            command=[sys.executable,root/'dotnet.py',tests/'libc_string_builtins.c','--language',language,
                '--target','portable-lp64-binary80-v1','--libc','dotcc','--split','size','--split-size','12000','--output-dir',directory]
            if instance:command+=['--instance-methods']
            run(command)
            run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
            run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
            checks.append(f'{directory.name}: translated JIT memory/string builtin addresses')
            if nativeaot:
                run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64',
                    '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
                run([directory/'aot/Generated']);checks.append(f'{directory.name}: translated NativeAOT memory/string builtin addresses')
    return checks

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output-dir',type=Path,required=True);parser.add_argument('--nativeaot',action='store_true');args=parser.parse_args()
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);root=Path(__file__).resolve().parents[2];index=0;checks=[]
    def run(command):
        nonlocal index
        command=list(map(str,command));i=index;index+=1
        result=subprocess.run(command,text=True,capture_output=True,timeout=240)
        (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2));(out/f'{i:03}.log').write_text(result.stdout+result.stderr)
        if result.returncode:print(result.stdout+result.stderr)
        result.check_returncode()
    try:
        run(['gcc','-D_GNU_SOURCE','-std=c11','-O0',root/'backend/tests/libc_string_builtins.c','-lm','-o',out/'native'])
        run([out/'native']);checks.append('native synthetic string builtin oracle')
        checks+=verify_libc_string_builtins(root,root/'backend/tests',out,args.nativeaot,run)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))
if __name__=='__main__':main()
