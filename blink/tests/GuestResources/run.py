#!/usr/bin/env python3
"""Run private resource C behavior through native and authored BCL callbacks."""
import hashlib,json,os
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
base=ROOT/'generated/guest-resources';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/guest-resources'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'actual upstream native NewSystem plus reduced managed System pointer/layout initialization; no guest syscall execution','passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log: code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    for name in ['probe.c','Program.cs','native.c','native-profile.c']:shutil.copyfile(ROOT/'tests/GuestResources'/name,a/name)
    shutil.copyfile(ROOT/'src/GuestResources/GuestResourcesBridge.cs',a/'GuestResourcesBridge.cs')
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    for name in ['GuestResources.c','GuestResources.h']:shutil.copyfile(ROOT/'src/GuestResources'/name,a/name)
    shutil.copyfile(ROOT/'config/core-config.h',a/'config.h')
    shutil.copyfile(ROOT/'config/target-storage.h',a/'target-storage.h')
    shutil.copyfile(ROOT/'config/core-overrides.json',a/'overrides.json')
    shutil.copyfile(ROOT/'src/Host/include/host-io.h',a/'profile/host-io.h')
    shutil.copyfile(ROOT/'src/Host/HostIoBridge.cs',a/'HostIoBridge.cs')
    shutil.copyfile(ROOT/'src/Host/HostMemory.c',a/'HostMemory.c')
    shutil.copyfile(ROOT/'src/Host/include/HostMemory.h',a/'HostMemory.h')
    shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host-project',ignore=shutil.ignore_patterns('bin','obj'))
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['hostSources']={p.name:sha(p)for p in (a/'host-project').glob('*.cs')}
    import sys
    sys.path.insert(0,str(ROOT/'scripts'))
    from core_inputs import compiler_identity
    r['compiler']=compiler_identity(cli.parent);r['postprocessorSha256']=sha(post);r['runnerSha256']=sha(Path(__file__));r['head']=subprocess.check_output(['git','-C',str(REPO),'rev-parse','HEAD'],text=True).strip();save()
    upstream=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
    native=ROOT/'build/native/source'
    r['nativeArchiveSha256']=sha(native/'o/blink/blink.a')
    manifest=ROOT/'config/source-inventory.json'
    for row in json.loads(manifest.read_text())['files']:
        if sha(upstream/row['path'])!=row['sha256']:raise RuntimeError('upstream source changed')
    r['sourceManifestSha256']=sha(manifest)
    run(['cc','-std=c17','-D_GNU_SOURCE','-D_DEFAULT_SOURCE','-DNOLINEAR','-I',native,'-I',a,a/'native.c',a/'GuestResources.c',a/'HostMemory.c',native/'o/blink/blink.a','-lz','-lrt','-lm','-pthread','-o',a/'native'],'native-build')
    run([a/'native'],'native',30)
    args=['-std=c17','-D_GNU_SOURCE','-DNDEBUG','-DNOLINEAR','-I',a,'-I',upstream]
    run(['cc',*args,'-DBLINK_NATIVE_PROFILE','-iquote',a/'profile',a/'native-profile.c',a/'HostMemory.c','-o',a/'native-profile'],'native-profile-build')
    expected=run([a/'native-profile'],'native-profile',30)
    raw=a/'raw';opt=a/'optimized'
    run(['dotnet',cli,*args,'-I',a/'profile',a/'probe.c',a/'HostMemory.c','--overrides-file',a/'overrides.json','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
    if r['compiler']!=compiler_identity(cli.parent) or r['postprocessorSha256']!=sha(post):raise RuntimeError('compiler changed during emission')
    r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir()
        for name in ['Program.cs','GuestResourcesBridge.cs','HostIoBridge.cs']:shutil.copyfile(a/name,consumer/name)
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','GuestResourcesProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
        ET.SubElement(items,'ProjectReference',Include=str(a/'host-project/Managed.Emulation.Host.csproj'))
        ET.SubElement(items,'TrimmerRootAssembly',Include='GuestResourcesProbe')
        project=consumer/'GuestResourcesProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',project],label+'-restore')
            run(['dotnet',post,project,'--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        actual=run(['dotnet',consumer/'bin/Release/net10.0/GuestResourcesProbe.dll'],label+'-jit',30)
        if actual!=expected:raise RuntimeError(label+' JIT differs from native')
        publish=a/(label+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        actual=run([publish/'GuestResourcesProbe'],label+'-aot',30)
        if actual!=expected:raise RuntimeError(label+' AOT differs from native')
        r['results'][label+'-aot']['binarySha256']=sha(publish/'GuestResourcesProbe')
    if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
    r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['passed']=True;save();print('native actual NewSystem and raw/optimized JIT/AOT reduced resource initialization pass; receipt '+str(out/'receipt.json'))
finally:save()
