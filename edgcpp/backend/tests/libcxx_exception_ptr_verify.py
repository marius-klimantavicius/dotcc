"""Pinned libc++ local exception handles, construction and nested exceptions.

This focused probe does not establish global/heap owner teardown, host escape,
concurrency, uncaught counts or terminate handler behavior; those remain separate
acceptance gates until their dedicated tests are added.
"""
import json
from pathlib import Path
import shutil
import sys


def write_exception_fixture(root,source,library,combined):
    combined.write_text(''.join('#include '+json.dumps(str(p.resolve()))+'\n' for p in
            [source,root/'backend/runtime/libcxx/abi_core.cpp',root/'backend/runtime/libcxx/abi_exceptions.cpp',library/'src/typeinfo.cpp'])+'''
// Synthetic interface adapter only. Original libc++ new.cpp is tested separately.
#include <cstdlib>
void* operator new(std::size_t n) {if(void* p=std::malloc(n?n:1))return p;throw std::bad_alloc();}
void* operator new(std::size_t n,std::align_val_t a) {
  auto alignment=static_cast<std::size_t>(a);if(!n)n=1;
  if(n>static_cast<std::size_t>(-1)-(alignment-1))throw std::bad_alloc();
  if(void* p=std::aligned_alloc(alignment,(n+alignment-1)&~(alignment-1)))return p;
  throw std::bad_alloc();
}
void operator delete(void* p) noexcept {std::free(p);}
void operator delete(void* p,std::size_t) noexcept {std::free(p);}
void operator delete(void* p,std::align_val_t) noexcept {std::free(p);}
void operator delete(void* p,std::size_t,std::align_val_t) noexcept {std::free(p);}
''')


def verify_libcxx_exception_ptr(root, tests, out, nativeaot, run, acquisition):
    acquisition=Path(acquisition).resolve()
    manifest=json.loads((acquisition/'acquisition.json').read_text())
    library=acquisition/manifest['source']['archiveRoot']
    source=tests/'libcxx/exception_handles.cpp'
    checks=[]
    candidates=['clang++',*[str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'))]]
    clang=next((shutil.which(p) for p in candidates if shutil.which(p)),None)
    if not clang:raise RuntimeError('Clang is required for the synthetic source oracle')
    for label,compiler in [('gcc','g++'),('clang',clang)]:
        executable=out/('exception-handles-source-'+label)
        run([compiler,'-std=c++20','-O2','-DEDG_NATIVE_ALIGNMENT_OBSERVATION=1',source,'-o',executable])
        run([executable])
    checks.append('libc++ exception handles: direct C++ GCC/Clang semantic oracle (native over-alignment limitation recorded separately)')
    for instance in (False,True):
        mode='instance' if instance else 'static'
        directory=out/('exception-handles-'+mode)
        combined=out/('exception-handles-'+mode+'.cpp')
        write_exception_fixture(root,source,library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++20',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','3200',
             *(['--instance-methods'] if instance else [])])
        run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo'])
        run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
        checks.append(f'libc++ exception handles: {mode} JIT copy/rethrow/make/nested/alignment/member pointers')
        if nativeaot:
            run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            run([directory/'aot/Generated'])
            checks.append(f'libc++ exception handles: {mode} NativeAOT')
    return checks
