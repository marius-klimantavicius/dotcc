"""Portable libc declaration/macro acceptance in translated C and C++."""
import argparse,json,subprocess,sys
from pathlib import Path


def verify_portable_libc_headers(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    checks=[]
    for source in ['portable_integer_headers.c','portable_libc_headers.c']:
        for language in ['c','c++']:
            directory=out/f'{Path(source).stem}-{language.replace("+","x")}'
            run([sys.executable,root/'dotnet.py',tests/source,'--language',language,
                '--target','portable-lp64-binary80-v1','--libc','dotcc','--output-dir',directory])
            run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
            run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
            checks.append(f'{source}/{language}: generated JIT header contracts')
            if nativeaot:
                run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64',
                    '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
                run([directory/'aot/Generated'])
                checks.append(f'{source}/{language}: generated NativeAOT header contracts')
    return checks


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    options=parser.parse_args();out=options.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    root=Path(__file__).resolve().parents[2];index=0;checks=[]
    def run(command):
        nonlocal index
        command=list(map(str,command));i=index;index+=1
        result=subprocess.run(command,text=True,capture_output=True,timeout=180)
        (out/f'{i:03}.command.json').write_text(json.dumps(command,indent=2))
        (out/f'{i:03}.log').write_text(result.stdout+result.stderr)
        if result.returncode:print(result.stdout+result.stderr)
        result.check_returncode()
    try:
        checks=verify_portable_libc_headers(root,root/'backend/tests',out,options.nativeaot,run)
    finally:(out/'results.json').write_text(json.dumps(checks,indent=2)+'\n')
    print(json.dumps(checks,indent=2))


if __name__=='__main__':main()
