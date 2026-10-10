#!/usr/bin/env python3
"""Translated array/scalar cleanup state chains retain every tail across runtime transfers."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ('compiler','packager','linker','output-dir'): parser.add_argument('--'+name,type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true'); args=parser.parse_args()
    out=args.output_dir.resolve(); out.mkdir(parents=True,exist_ok=False)
    inputs=out/'inputs';inputs.mkdir();tooling=out/'tooling';tooling.mkdir()
    for name in ('cpp_array_runtime_transfer.cpp','ArrayRuntimeTransferHost.cs'):
        shutil.copy2(Path(__file__).resolve().parent/name,inputs/name)
    for name in ('compile.py','schema.sql'):shutil.copy2(args.packager.resolve().parent/name,tooling/name)
    checks,commands=[],[]
    def check(name,condition):
        if not condition:raise AssertionError(name)
        checks.append(name)
    def run(name,command):
        command=list(map(str,command));r=subprocess.run(command,cwd=out,capture_output=True,timeout=600)
        (out/(name+'.stdout')).write_bytes(r.stdout);(out/(name+'.stderr')).write_bytes(r.stderr)
        commands.append(dict(name=name,command=command,exitCode=r.returncode));check(name,r.returncode==0);return r
    error=None
    try:
        obj=out/'transfer.dotobj'
        run('export',[sys.executable,'-B',tooling/'compile.py',inputs/'cpp_array_runtime_transfer.cpp','--output',obj,
            '--work-dir',out/'export','--compiler',args.compiler.resolve(),'--exceptions','--no-rtti'])
        run('verify',['dotnet',args.linker.resolve(),'verify',obj]);inputs.rename(out/'retained-source')
        generated=out/'generated'
        run('source-hidden-link',['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Tests.ArrayTransfer',
            '--class','Core','--allow-externs',obj])
        shutil.copy2(out/'retained-source/ArrayRuntimeTransferHost.cs',generated/'ArrayRuntimeTransferHost.cs')
        project=generated/'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
            '<OutputType>Exe</OutputType><StartupObject>ArrayRuntimeTransferHost</StartupObject>'))
        run('build',['dotnet','build',project,'-c','Release','--nologo','-v:q'])
        result=run('jit',['dotnet',generated/'bin/Release/net10.0/Translated.dll'])
        check('jit-seven-transfer-scenarios',len(result.stdout.splitlines())==7 and not result.stderr)
        if args.nativeaot:
            run('publish',['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true','-o',out/'publish','--nologo','-v:q'])
            native=run('nativeaot',[out/'publish/Translated'])
            check('jit-nativeaot-parity',native.stdout==result.stdout and not native.stderr)
    except Exception as exception:error=str(exception)
    receipt=dict(status='failed' if error else 'passed',error=error,checks=checks,commands=commands,scenarios=7,
        limitations=['Classified transfers are managed host exceptions; CppException payload abandonment on forced platform transfer needs separate lifecycle integration.'])
    for name in ('compiler','linker','packager'):
        path=getattr(args,name).resolve();receipt[name]=dict(path=str(path),sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out/'results.json').write_text(json.dumps(receipt,indent=2)+'\n');print(json.dumps(dict(status=receipt['status'],checks=len(checks),error=error)));return bool(error)

if __name__=='__main__':raise SystemExit(main())
