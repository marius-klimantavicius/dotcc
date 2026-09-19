#!/usr/bin/env python3
"""Qualify the declared private process refusal policy against its native baseline."""
import hashlib,json,os
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
base=ROOT/'generated/host-process-policy';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-process-policy'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'explicit private process refusal policy; native baseline has different permitted behavior','passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log: code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    for name in ['probe.c','Program.cs']:shutil.copyfile(Path(__file__).resolve().parent/name,a/name)
    shutil.copyfile(ROOT/'src/Host/HostIdentityBridge.cs',a/'HostIdentityBridge.cs')
    shutil.copyfile(ROOT/'src/Host/HostProcessPolicyBridge.cs',a/'HostProcessPolicyBridge.cs')
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    shutil.copyfile(ROOT/'src/Host/include/host-identity.h',a/'profile/host-identity.h')
    shutil.copyfile(ROOT/'src/Host/include/host-process-policy.h',a/'profile/host-process-policy.h')
    shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host-project',ignore=shutil.ignore_patterns('bin','obj'))
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['hostSources']={p.name:sha(p)for p in (ROOT/'src/Managed.Emulation.Host').glob('*.cs')}
    r['compiler']={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]};save()
    run(['cc','-std=c17','-Wall','-Wextra','-Werror',a/'probe.c','-o',a/'native'],'native-build')
    native=run([a/'native'],'native',30)
    if native!=b'native fork/wait baseline: PASS\n':raise RuntimeError('native baseline failed')
    expected=b'private process creation/exec denied, no children, immutable identity: PASS\n'
    raw=a/'raw';opt=a/'optimized'
    run(['dotnet',cli,'-std=c17','-DBLINK_MANAGED_PROCESS','-I',a/'profile',a/'probe.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
    if r['compiler']!={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]}:raise RuntimeError('compiler changed during emission')
    r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir()
        for name in ['Program.cs','HostIdentityBridge.cs','HostProcessPolicyBridge.cs']:shutil.copyfile(a/name,consumer/name)
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostIdentityProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
        ET.SubElement(items,'ProjectReference',Include=str(a/'host-project/Managed.Emulation.Host.csproj'))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostIdentityProbe')
        project=consumer/'HostIdentityProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',project],label+'-restore')
            run(['dotnet',post,project,'--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        actual=run(['dotnet',consumer/'bin/Release/net10.0/HostIdentityProbe.dll'],label+'-jit',30)
        if actual!=expected:raise RuntimeError(label+' JIT differs from declared policy')
        publish=a/(label+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        actual=run([publish/'HostIdentityProbe'],label+'-aot',30)
        if actual!=expected:raise RuntimeError(label+' AOT differs from declared policy')
        r['results'][label+'-aot']['binarySha256']=sha(publish/'HostIdentityProbe')
    if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
    r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['passed']=True;save();print('native fork/wait baseline passes; raw/optimized JIT/AOT enforce declared private process refusal policy')
finally:save()
