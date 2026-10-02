"""Portable typed variadic packs compared with EDG's native C backend."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_varargs_source(root, tests, out, nativeaot, run, language):
    source=tests/('varargs.cpp' if language=='c++' else 'varargs_c.c')
    native_source=out/'varargs-oracle.c'
    run([root/'build/bin/cpfe','--c++17' if language=='c++' else '--c11','--gnu_version','130300',
         '-I',root/'backend/runtime/libc/include','--gen_c_file_name',native_source,
         '-o',out/'varargs-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    native=out/'varargs-oracle'
    run(['gcc','-std=gnu11',native_source,root/'build/lib/libC.a','-lm','-o',native]);run([native])
    checks=['varargs: original EDG C oracle']
    for instance in (False,True):
        directory=out/('instance' if instance else 'static')
        run([sys.executable,root/'dotnet.py',source,'--language',language,'--libc','dotcc','--output-dir',directory,
             '--split','size','--split-size','3000',*(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        assert manifest['varargs']=='portable-typed-pack-v1'
        project=directory/'Generated.csproj'
        run(['dotnet','build',project,'-c','Release','--nologo'])
        run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
        checks.append(f'varargs {instance=}: JIT promotions, aggregate/128/binary80, copy, forwarding, recursion, pointer call and EH')
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            run([directory/'aot/Generated']);checks.append(f'varargs {instance=}: Linux x64 NativeAOT')
    return checks


def verify_varargs(root, tests, out, nativeaot, run):
    checks=[]
    for language in ('c','c++'):
        directory=out/('varargs-c' if language=='c' else 'varargs-cpp')
        directory.mkdir(parents=True,exist_ok=True)
        checks.extend(f'{language}: {check}' for check in verify_varargs_source(root,tests,directory,nativeaot,run,language))
    return checks


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',required=True,type=Path)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    commands=[]
    def run(command,**kwargs):
        command=list(map(str,command));result=subprocess.run(command,text=True,capture_output=True,**kwargs)
        index=len(commands)
        (out/f'command-{index:03d}.stdout').write_text(result.stdout)
        (out/f'command-{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command':command,'exit':result.returncode})
        (out/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
        if result.returncode:raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    root=Path(__file__).resolve().parents[2]
    checks=verify_varargs(root,Path(__file__).resolve().parent,out,args.nativeaot,run)
    (out/'results.json').write_text(json.dumps({'checks':checks},indent=2)+'\n')
    print(f'{len(checks)} variadic checks passed')
