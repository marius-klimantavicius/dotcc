"""Version 2 nonlocal-jump descriptor and VLA checkpoint contract."""
import json
from pathlib import Path
import resource
import shutil
import subprocess


def verify_setjmp_vla_runtime(root,tests,out,nativeaot,run,runtime_directory=None):
    runtime_directory=Path(runtime_directory) if runtime_directory is not None else root/'backend/runtime'
    for source in (runtime_directory/'NonlocalJump.cs',runtime_directory/'Vla.cs',tests/'setjmp_vla_runtime.cs'):
        shutil.copy2(source,out/source.name)
    project=out/'JumpVlaProbe.csproj'
    project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup></Project>''')
    checks=[]
    def verify(command,label):
        run(command);checks.append(label+': VLA before/after setjmp, repeated jumps, callee cleanup, surviving VLA buffer, allocation-free capture')
        for case in ('escaped','generation','resume','version'):
            def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
            invocation=list(map(str,[*command,case]))
            try:result=subprocess.run(invocation,text=True,capture_output=True,preexec_fn=no_core,timeout=60)
            except subprocess.TimeoutExpired as error:
                (out/(label+'-'+case+'.stdout')).write_bytes(error.stdout or b'');(out/(label+'-'+case+'.stderr')).write_bytes(error.stderr or b'')
                (out/(label+'-'+case+'.json')).write_text(json.dumps({'command':invocation,'timeout':60},indent=2));raise
            (out/(label+'-'+case+'.stdout')).write_text(result.stdout);(out/(label+'-'+case+'.stderr')).write_text(result.stderr)
            (out/(label+'-'+case+'.json')).write_text(json.dumps({'command':invocation,'exit':result.returncode},indent=2))
            reason='Unarmed or corrupted longjmp buffer' if case=='version' else 'Nonlocal jump VLA scope has ended'
            assert result.returncode!=0 and reason in result.stderr,(label,case,result.returncode,result.stderr[:1000])
            checks.append(label+': rejects '+case+' checkpoint before destructive restore')
    run(['dotnet','build',project,'-c','Release'])
    verify(['dotnet',out/'bin/Release/net10.0/JumpVlaProbe.dll'],'jit')
    if nativeaot:
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
        verify([out/'aot/JumpVlaProbe'],'aot')
    return checks
