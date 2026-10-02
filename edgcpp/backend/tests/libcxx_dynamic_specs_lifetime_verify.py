"""Payload lifetime across unexpected replacement and retained current_exception."""
import json
from pathlib import Path
import shutil
import sys
from libcxx_exception_ptr_verify import write_exception_fixture


def verify_libcxx_dynamic_specs_lifetime(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    source=tests/'libcxx/dynamic_specs_lifetime.cpp';checks=[]
    clang=shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))),None)
    if not clang:raise RuntimeError('Clang required for dynamic-spec lifetime source oracle')
    for label,compiler in [('gcc','g++'),('clang',clang)]:
        executable=out/('native-'+label);run([compiler,'-std=c++14',source,'-o',executable]);run([executable])
    checks.append('dynamic replacement lifetime: direct GCC/Clang original/current/replacement retention and exact final destructor ordering')
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/mode;combined=out/(mode+'.cpp')
        write_exception_fixture(root,source,library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++14',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src','--output-dir',directory,
             '--namespace','','--class-name','Core','--split','size','--split-size','3200',*(['--instance-methods'] if instance else [])])
        project=directory/'Generated.csproj';run(['dotnet','run','--project',project,'-c','Release'])
        checks.append(f'dynamic replacement lifetime: {mode} JIT, saved original/replacement and bad_exception substitution')
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);run([directory/'aot/Generated'])
            checks.append(f'dynamic replacement lifetime: {mode} NativeAOT')
    return checks
