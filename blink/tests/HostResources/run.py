#!/usr/bin/env python3
"""Run private resource C behavior through native and authored BCL callbacks."""
import hashlib,json,os
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
base=ROOT/'generated/host-resources';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-resources'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'private owner resource limits and immutable priority, not guest rlim initialization or OS accounting','passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log: code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    for name in ['probe.c','Program.cs','native.c','memory-native.c']:shutil.copyfile(ROOT/'tests/HostResources'/name,a/name)
    shutil.copyfile(ROOT/'src/HostIdentity/HostIdentityBridge.cs',a/'HostIdentityBridge.cs')
    shutil.copyfile(ROOT/'src/HostResources/HostResourcesBridge.cs',a/'HostResourcesBridge.cs')
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    shutil.copyfile(ROOT/'src/HostResources/host-resources.h',a/'profile/host-resources.h')
    shutil.copyfile(ROOT/'src/HostIo/host-io.h',a/'profile/host-io.h')
    shutil.copyfile(ROOT/'src/HostIo/HostIoBridge.cs',a/'HostIoBridge.cs')
    shutil.copyfile(ROOT/'src/HostMemory/HostMemory.c',a/'HostMemory.c')
    shutil.copyfile(ROOT/'src/HostMemory/HostMemory.h',a/'HostMemory.h')
    shutil.copyfile(ROOT/'src/HostIdentity/host-identity.h',a/'profile/host-identity.h')
    shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host-project',ignore=shutil.ignore_patterns('bin','obj'))
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['hostSources']={p.name:sha(p)for p in (a/'host-project').glob('*.cs')}
    r['compiler']={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]};r['head']=subprocess.check_output(['git','-C',str(REPO),'rev-parse','HEAD'],text=True).strip();save()
    run(['cc','-std=c17','-Wall','-Wextra','-Werror',a/'native.c','-o',a/'native'],'native-build')
    expected=run([a/'native'],'native',30)
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-I',a,a/'memory-native.c',a/'HostMemory.c','-o',a/'native-memory'],'native-memory-build')
    run([a/'native-memory'],'native-memory')
    raw=a/'raw';opt=a/'optimized'
    run(['dotnet',cli,'-std=c17','-I',a/'profile',a/'probe.c',a/'HostMemory.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
    if r['compiler']!={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]}:raise RuntimeError('compiler changed during emission')
    r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir()
        for name in ['Program.cs','HostIdentityBridge.cs','HostResourcesBridge.cs','HostIoBridge.cs']:shutil.copyfile(a/name,consumer/name)
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostResourcesProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
        ET.SubElement(items,'ProjectReference',Include=str(a/'host-project/Managed.Emulation.Host.csproj'))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostResourcesProbe')
        project=consumer/'HostResourcesProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',project],label+'-restore')
            run(['dotnet',post,project,'--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        actual=run(['dotnet',consumer/'bin/Release/net10.0/HostResourcesProbe.dll'],label+'-jit',30)
        if actual!=expected:raise RuntimeError(label+' JIT differs from native')
        publish=a/(label+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        actual=run([publish/'HostResourcesProbe'],label+'-aot',30)
        if actual!=expected:raise RuntimeError(label+' AOT differs from native')
        r['results'][label+'-aot']['binarySha256']=sha(publish/'HostResourcesProbe')
    if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
    r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['passed']=True;save();print('native invariants and raw/optimized JIT/AOT private resource policies pass; receipt '+str(out/'receipt.json'))
finally:save()
