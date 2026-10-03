"""Standalone NativeAOT coroutine frame protocol, not source coroutine coverage."""
import json
import resource
import shutil
import subprocess


def verify_coroutine_runtime(root,tests,out,nativeaot,run):
    for source in (root/'backend/runtime/Coroutine.cs',tests/'coroutine_runtime.cs'):
        shutil.copy2(source,out/source.name)
    project=out/'CoroutineProbe.csproj'
    project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>''')
    checks=[]
    def verify(command,label):
        run(command);checks.append(label+': frame state, promise roundtrip, 10000 allocation-free symmetric transfers, self-destruction, owner rooting and abandonment')
        for case in ('done','abandoned','null'):
            def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
            invocation=list(map(str,[*command,case]))
            try:result=subprocess.run(invocation,text=True,capture_output=True,preexec_fn=no_core,timeout=60)
            except subprocess.TimeoutExpired as error:
                (out/(label+'-'+case+'.stdout')).write_bytes(error.stdout or b'');(out/(label+'-'+case+'.stderr')).write_bytes(error.stderr or b'')
                (out/(label+'-'+case+'.json')).write_text(json.dumps({'command':invocation,'timeout':60},indent=2));raise
            (out/(label+'-'+case+'.stdout')).write_text(result.stdout);(out/(label+'-'+case+'.stderr')).write_text(result.stderr)
            (out/(label+'-'+case+'.json')).write_text(json.dumps({'command':invocation,'exit':result.returncode},indent=2))
            reason='Cannot resume a completed coroutine' if case=='done' else 'Invalid or disposed coroutine frame'
            assert result.returncode!=0 and reason in result.stderr,(label,case,result.returncode,result.stderr[:1000])
            checks.append(label+': rejects '+case+' frame')
    run(['dotnet','build',project,'-c','Release'])
    verify(['dotnet',out/'bin/Release/net10.0/CoroutineProbe.dll'],'jit')
    if nativeaot:
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot'])
        verify([out/'aot/CoroutineProbe'],'aot')
    return checks
