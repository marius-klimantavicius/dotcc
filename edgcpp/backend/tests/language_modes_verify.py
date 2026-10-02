"""Explicit modern language modes preserve frontend evaluation in emitted C#."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_language_modes(root,tests,out,nativeaot,run):
    checks=[]
    for standard in ('c11','c17','c23','c++17','c++20','c++23','c++26'):
        language='c++' if '++' in standard else 'c'
        source=tests/('unsigned_negation.c' if standard in ('c11','c++17') else
                      'language_modes.cpp' if language=='c++' else 'language_modes.c')
        directory=out/('language-'+standard.replace('+','x'));directory.mkdir(parents=True,exist_ok=True)
        lowered=directory/'oracle.c';native=directory/'native'
        run([root/'build/bin/cpfe','--'+standard,'--gnu_version','130300',
             *(['--instantiate','used'] if language=='c++' else []),
             '--gen_c_file_name',lowered,'-o',directory/'oracle.il',source],
            env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
        run(['gcc','-std=gnu11',lowered,root/'build/lib/libC.a','-lm','-o',native]);run([native])
        checks.append(f'{standard}: original EDG C oracle')
        generated=directory/'managed'
        run([sys.executable,root/'dotnet.py',source,'--language',language,'--std',standard,
             '--output-dir',generated])
        manifest=json.loads((generated/'dot-gen.json').read_text())
        assert manifest['standard']==standard and manifest['target']['id']=='portable-lp64-binary80-v2'
        run(['dotnet','build',generated/'Generated.csproj','-c','Release','--nologo'])
        run(['dotnet',generated/'bin/Release/net10.0/Generated.dll'])
        checks.append(f'{standard}: JIT runtime/constant evaluation and manifest')
        if nativeaot:
            run(['dotnet','publish',generated/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',generated/'aot'])
            run([generated/'aot/Generated']);checks.append(f'{standard}: Linux x64 NativeAOT')
    return checks


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',required=True,type=Path)
    parser.add_argument('--nativeaot',action='store_true')
    options=parser.parse_args();out=options.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    commands=[]
    def run(command,**kwargs):
        command=list(map(str,command));result=subprocess.run(command,text=True,capture_output=True,**kwargs)
        index=len(commands);(out/f'{index:03d}.stdout').write_text(result.stdout);(out/f'{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command':command,'exit':result.returncode});(out/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
        if result.returncode:raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    root=Path(__file__).resolve().parents[2]
    checks=verify_language_modes(root,Path(__file__).resolve().parent,out,options.nativeaot,run)
    (out/'results.json').write_text(json.dumps({'checks':checks},indent=2)+'\n');print(f'{len(checks)} language mode checks passed')
