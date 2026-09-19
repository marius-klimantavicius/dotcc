#!/usr/bin/env python3
"""Qualify the selected ordinary-node filesystem and IPv4 protocol profile."""
import hashlib,json,os
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
base=ROOT/'generated/host-namespace-policy';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-namespace-policy'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'private ordinary-node namespace and IPv4-only pair boundary','passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log: code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    for name in ['probe.c','Program.cs']:shutil.copyfile(ROOT/'tests/HostNamespacePolicy'/name,a/name)
    shutil.copyfile(ROOT/'src/HostIo/HostIoBridge.cs',a/'HostIoBridge.cs')
    shutil.copyfile(ROOT/'src/HostNamespacePolicy/HostNamespacePolicyBridge.cs',a/'HostNamespacePolicyBridge.cs')
    shutil.copyfile(ROOT/'src/HostIo/HostFileControl.c',a/'HostFileControl.c')
    shutil.copyfile(ROOT/'src/HostNamespace/HostNamespaceBridge.cs',a/'HostNamespaceBridge.cs')
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    shutil.copyfile(ROOT/'src/HostNamespacePolicy/host-namespace-policy.h',a/'profile/host-namespace-policy.h')
    shutil.copyfile(ROOT/'src/HostIo/host-io.h',a/'profile/host-io.h')
    shutil.copyfile(ROOT/'src/HostNamespace/host-namespace.h',a/'profile/host-namespace.h')
    shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host-project',ignore=shutil.ignore_patterns('bin','obj'))
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['hostSources']={p.name:sha(p)for p in (a/'host-project').glob('*.cs')}
    import sys
    sys.path.insert(0,str(ROOT/'scripts'))
    from core_inputs import compiler_identity
    r['compiler']=compiler_identity(cli.parent);r['postprocessorSha256']=sha(post);r['runnerSha256']=sha(Path(__file__));r['head']=subprocess.check_output(['git','-C',str(REPO),'rev-parse','HEAD'],text=True).strip();save()
    run(['cc','-std=c17','-Wall','-Wextra','-Werror',a/'probe.c','-o',a/'native'],'native-build')
    expected=run([a/'native',a],'native',30)
    raw=a/'raw';opt=a/'optimized'
    run(['dotnet',cli,'-std=c17','-DBLINK_NAMESPACE_POLICY','-I',a/'profile',a/'probe.c',a/'HostFileControl.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
    if r['compiler']!=compiler_identity(cli.parent) or r['postprocessorSha256']!=sha(post):raise RuntimeError('compiler changed during emission')
    r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir()
        for name in ['Program.cs','HostIoBridge.cs','HostNamespacePolicyBridge.cs','HostNamespaceBridge.cs']:shutil.copyfile(a/name,consumer/name)
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostNamespacePolicyProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
        ET.SubElement(items,'ProjectReference',Include=str(a/'host-project/Managed.Emulation.Host.csproj'))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostNamespacePolicyProbe')
        project=consumer/'HostNamespacePolicyProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',project],label+'-restore')
            run(['dotnet',post,project,'--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        actual=run(['dotnet',consumer/'bin/Release/net10.0/HostNamespacePolicyProbe.dll'],label+'-jit',30)
        if actual!=expected:raise RuntimeError(label+' JIT differs from native')
        publish=a/(label+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        actual=run([publish/'HostNamespacePolicyProbe'],label+'-aot',30)
        if actual!=expected:raise RuntimeError(label+' AOT differs from native')
        r['results'][label+'-aot']['binarySha256']=sha(publish/'HostNamespacePolicyProbe')
    if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
    r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['passed']=True;save();print('native common and raw/optimized JIT/AOT namespace/protocol profile checks pass; receipt '+str(out/'receipt.json'))
finally:save()
