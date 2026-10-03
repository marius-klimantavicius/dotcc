"""Compiler-assisted source setjmp continuations, native and generated oracles."""
import json
import os
from pathlib import Path
import shutil
import sys


def verify_setjmp_control(root,tests,out,nativeaot,run):
    source=tests/'setjmp_control.c';checks=[]
    clang=shutil.which('clang') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang'))),None)
    if not clang:raise RuntimeError('Clang required for source setjmp oracle')
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        for optimization in ('-O0','-O2'):
            executable=out/(label+optimization)
            run([compiler,'-std=c11',optimization,source,'-o',executable]);run([executable])
    checks.append('source setjmp: native GCC/Clang O0/O2 function-wide targets, recursion, repeated buffers, zero normalization')
    run([root/'build/bin/cpfe','--c11','--gnu_version','130300','-I','/usr/include/x86_64-linux-gnu',
         '--gen_c_file_name',out/'oracle.c','-o',out/'oracle.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        executable=out/('edg-'+label)
        run([compiler,'-x','c','-std=gnu11','-w',out/'oracle.c','-o',executable]);run([executable])
    checks.append('source setjmp: original EDG C GCC/Clang native returns-twice oracle')
    for language in ('c','c++'):
        for instance in (False,True):
            mode=language.replace('+','x')+('-instance' if instance else '-static');directory=out/mode
            run([sys.executable,root/'dotnet.py',source,'--language',language,'--libc','dotcc',
                 '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','3200',
                 *(['--instance-methods'] if instance else [])])
            project=directory/'Generated.csproj';run(['dotnet','run','--project',project,'-c','Release'])
            checks.append(f'source setjmp: {mode} JIT whole-activation resume, locals and side-effect-once buffers')
            if nativeaot:
                run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
                run([directory/'aot/Generated']);checks.append(f'source setjmp: {mode} NativeAOT')
    for label,language,body,reason in (
        ('assignment','c','int f(void){jmp_buf b;int result=setjmp(b);return result;}','setjmp requires a supported controlling or standalone expression'),
        ('return','c','int f(void){jmp_buf b;return setjmp(b);}','setjmp requires a supported controlling or standalone expression'),
        ('protected','c++','int f(){jmp_buf b;try{if(setjmp(b))return 1;}catch(...){}return 0;}','setjmp continuation through C++ protected/lifetime regions'),
        ('vla','c','int f(int n){jmp_buf b;int a[n];a[0]=1;if(setjmp(b))return a[0];return 0;}','setjmp continuation with variable-length storage')):
        directory=out/('reject-'+label);case=out/(label+('.c' if language=='c' else '.cpp'))
        case.write_text('#include <setjmp.h>\n'+body+'\nint main(){return f('+('2' if label=='vla' else '')+');}\n')
        result=run([sys.executable,root/'dotnet.py',case,'--language',language,'--libc','dotcc','--output-dir',directory],expected=1)
        assert reason in result.stderr+result.stdout,(label,reason,result.stderr,result.stdout)
        checks.append('source setjmp: explicit diagnostic for '+label+' continuation')
    return checks
