#!/usr/bin/env python3
"""Snapshot and qualify the owning private filesystem under JIT and NativeAOT."""
import hashlib,json,os,shutil,subprocess,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
base=ROOT/'generated/host-files';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-files'/a.name;out.mkdir(parents=True)
r={'scope':'owning BCL private filesystem; not translated guest execution','passed':False,'results':{}}
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log:code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    r['inputs']={}
    for source in [ROOT/'src/Managed.Emulation.Host/VirtualFileSystem.cs',ROOT/'tests/HostFiles/Program.cs',Path(__file__)]:
        destination=a/source.name;shutil.copyfile(source,destination)
        r['inputs'][str(source.relative_to(ROOT))]=sha(source)
        if sha(destination)!=sha(source):raise RuntimeError('source changed during snapshot')
    project=a/'HostFiles.csproj'
    project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><TrimmerRootAssembly Include="HostFiles" /></ItemGroup></Project>\n')
    r['projectSha256']=sha(project);save()
    run(['dotnet','build',project,'-c','Release'],'jit-build')
    expected=run(['dotnet',a/'bin/Release/net10.0/HostFiles.dll'],'jit',30)
    run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',a/'publish'],'aot-build',300)
    actual=run([a/'publish/HostFiles'],'aot',30)
    if actual!=expected:raise RuntimeError('JIT and NativeAOT differ')
    r['stdoutSha256']=hashlib.sha256(expected).hexdigest();r['aotSha256']=sha(a/'publish/HostFiles');r['passed']=True;save()
    print('Private filesystem JIT/AOT pass: '+str(out/'receipt.json'))
finally:save()
