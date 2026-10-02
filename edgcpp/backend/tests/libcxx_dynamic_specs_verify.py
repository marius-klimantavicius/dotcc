"""C++14 dynamic exception specifications and libc++ unexpected replacement."""
import json
import os
from pathlib import Path
import shutil
import sys
from libcxx_exception_ptr_verify import write_exception_fixture


def verify_libcxx_dynamic_specs(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    source=tests/'libcxx/dynamic_specs.cpp';checks=[]
    clang=shutil.which('clang++') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))),None)
    if not clang:raise RuntimeError('Clang required for dynamic specification oracles')
    for label,compiler in [('gcc','g++'),('clang',clang)]:
        executable=out/('native-'+label);run([compiler,'-std=c++14',source,'-o',executable]);run([executable])
    checks.append('dynamic throw specifications: direct source GCC/Clang allowed types, replacements, bad_exception, nesting, pointers/members and references')
    include=out/'edg-oracle-include';include.mkdir()
    for name in ('exception','edg_internal_defines'):
        (include/name).write_bytes((root/'upstream/include_c++'/(name+'.stdh')).read_bytes())
    run([root/'build/bin/cpfe','--c++14','--gnu_version','130300','-I',include,'-DEDG_DYNAMIC_C_ORACLE=1',
         '--gen_c_file_name',out/'oracle.c','-o',out/'oracle.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        executable=out/('edg-'+label)
        run([compiler,'-x','c','-w','-std=gnu11',out/'oracle.c','-x','none',root/'build/lib/libC.a','-lm','-o',executable]);run([executable])
    checks.append('dynamic throw specifications: original EDG C GCC/Clang (EDG headers/runtime isolated from libc++)')
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/mode;combined=out/(mode+'.cpp')
        write_exception_fixture(root,source,library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++14',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src','--output-dir',directory,
             '--namespace','','--class-name','Core','--split','size','--split-size','3200',*(['--instance-methods'] if instance else [])])
        project=directory/'Generated.csproj';run(['dotnet','run','--project',project,'-c','Release'])
        checks.append(f'dynamic throw specifications: {mode} JIT, pinned libc++ unexpected/current_exception and bad_exception')
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot']);run([directory/'aot/Generated'])
            checks.append(f'dynamic throw specifications: {mode} NativeAOT')
    return checks
