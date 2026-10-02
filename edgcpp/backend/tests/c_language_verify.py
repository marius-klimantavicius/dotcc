"""C11 driver, explicit defines and incomplete opaque pointer types."""
import sys


def verify_c_language(root,tests,out,nativeaot,run):
    source=tests/'c_language.c'
    native=out/'c-language-native'
    run(['gcc','-std=c11','-DEDG_C_TEST=9',source,'-o',native]);run([native])
    checks=['C11: native reference with opaque pointer roundtrip, bitfields, floating point, switch and volatile']
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/f'c-language-{mode}'
        run([sys.executable,root/'dotnet.py',source,'--language','c','-DEDG_C_TEST=9','-I',tests,
             '--output-dir',directory,'--namespace','Edg.C','--class-name','CProgram','--split','function',
             *(['--instance-methods'] if instance else ['--no-nest-types'])])
        run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
        checks.append(f'C11/{mode}: managed execution with -I/-D and incomplete opaque handle/flexible array tail')
        if nativeaot:
            run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            run([directory/'aot/Generated'])
            checks.append(f'C11/{mode}: linux-x64 NativeAOT execution')
    return checks
