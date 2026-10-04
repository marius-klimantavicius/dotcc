"""Portable typed variadic packs compared with EDG's native C backend."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import shutil
import sys


def verify_varargs_source(root, tests, out, nativeaot, run, language, compiler=None):
    source=tests/('varargs.cpp' if language=='c++' else 'varargs_c.c')
    clang_name='clang++' if language=='c++' else 'clang'
    clang=shutil.which(clang_name) or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/'+clang_name))),None)
    if not clang:raise RuntimeError('Clang source oracle required')
    for label,frontend in [('gcc','g++' if language=='c++' else 'gcc'),('clang',clang)]:
        executable=out/('source-oracle-'+label)
        run([frontend,'-std=c++17' if language=='c++' else '-std=c11','-O2',source,'-o',executable])
        run([executable])
    native_source=out/'varargs-oracle.c'
    run([root/'build/bin/cpfe','--c++17' if language=='c++' else '--c11','--gnu_version','130300',
         '-I',root/'backend/runtime/libc/include','--gen_c_file_name',native_source,
         '-o',out/'varargs-oracle.il',source],
        env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    native=out/'varargs-oracle'
    run(['gcc','-std=gnu11',native_source,root/'build/lib/libC.a','-lm','-o',native]);run([native])
    checks=['varargs: source GCC/Clang and original EDG C oracle']
    for instance in (False,True):
        directory=out/('instance' if instance else 'static')
        run([sys.executable,root/'dotnet.py',source,'--language',language,'--libc','dotcc','--output-dir',directory,
             '--split','size','--split-size','3000',*(['--compiler',compiler] if compiler else []),
             *(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        assert manifest['varargs']=='portable-span-pack-v2', manifest['varargs']
        generated='\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
        assert '__edg_varcall' not in generated, 'variadic calls must not synthesize per-call helpers'
        assert 'ReadOnlySpan<global::Edg.Runtime.VaArg>' in generated
        assert 'VaPack.GetStackBytes' in generated and 'new global::Edg.Runtime.VaPack(' in generated
        project=directory/'Generated.csproj'
        run(['dotnet','build',project,'-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
        checks.append(f'varargs {instance=}: JIT direct/indirect span calls, promotions, aggregate/128/binary80, snapshots, copy/forwarding, >64KiB packs, void/empty calls, function pointers and EH; no varcall helpers')
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            run([directory/'aot/Generated']);checks.append(f'varargs {instance=}: Linux x64 NativeAOT')
    return checks


def verify_varargs(root, tests, out, nativeaot, run, compiler=None):
    checks=[]
    for language in ('c','c++'):
        directory=out/('varargs-c' if language=='c' else 'varargs-cpp')
        directory.mkdir(parents=True,exist_ok=True)
        checks.extend(f'{language}: {check}' for check in verify_varargs_source(root,tests,directory,nativeaot,run,language,compiler))
    return checks


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir',required=True,type=Path)
    parser.add_argument('--nativeaot',action='store_true')
    parser.add_argument('--compiler',type=Path)
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
    compiler=args.compiler.resolve() if args.compiler else None
    checks=verify_varargs(root,Path(__file__).resolve().parent,out,args.nativeaot,run,compiler)
    source_paths=[Path(__file__).resolve(),Path(__file__).with_name('varargs.cpp'),Path(__file__).with_name('varargs_c.c')]
    (out/'results.json').write_text(json.dumps({'status':'passed','checks':checks,
        'inputs':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in source_paths},
        'compiler':{'path':str(compiler),'sha256':hashlib.sha256(compiler.read_bytes()).hexdigest()} if compiler else None},indent=2)+'\n')
    print(f'{len(checks)} variadic checks passed')
