#!/usr/bin/env python3
"""Run private openat/fcntl contracts through native and authored C callbacks."""
import hashlib,json,os
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
base=ROOT/'generated/host-file-control';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-file-control'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'private openat/fcntl callbacks and explicit exec descriptor transition','passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log: code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),cwd=a,timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    for name in ['probe.c','native-exec.c','Program.cs']:shutil.copyfile(ROOT/'tests/HostFileControl'/name,a/name)
    shutil.copyfile(ROOT/'src/Host/HostFileMetadataBridge.cs',a/'HostFileMetadataBridge.cs')
    shutil.copyfile(ROOT/'src/Host/HostFileControl.c',a/'HostFileControl.c')
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    shutil.copyfile(ROOT/'src/Host/include/host-io.h',a/'profile/host-io.h')
    shutil.copyfile(ROOT/'src/Host/HostIoBridge.cs',a/'HostIoBridge.cs')
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host',ignore=shutil.ignore_patterns('bin','obj'))
    r['hostSources']={p.name:sha(p)for p in (a/'host').glob('*.cs')}
    r['compiler']={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]};save()
    run(['cc','-std=c17','-D_GNU_SOURCE','-DBLINK_TEST_ROOT="'+str(a)+'"','-Wall','-Wextra','-Werror',a/'probe.c','-o',a/'native'],'native-build')
    (a/'data').mkdir()
    (a/'data/image').write_bytes(b'alpha');(a/'data/image').chmod(0o444)
    expected=run([a/'native'],'native',30)
    run(['cc','-std=c17','-Wall','-Wextra','-Werror',a/'native-exec.c','-o',a/'native-exec'],'native-exec-build')
    run([a/'native-exec'],'native-exec',30)
    raw=a/'raw';opt=a/'optimized'
    run(['dotnet',cli,'-std=c17','-DBLINK_MANAGED_CONTROL','-I',a/'profile',a/'probe.c',a/'HostFileControl.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
    if r['compiler']!={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]}:raise RuntimeError('compiler changed during emission')
    for name in ['HostFiles','InstanceIo']:
        model=a/('regression-'+name);model.mkdir()
        shutil.copyfile(ROOT/'tests'/name/'Program.cs',model/'Program.cs')
        project=model/'Regression.csproj'
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../host/Managed.Emulation.Host.csproj" /></ItemGroup></Project>')
        r['inputs']['regression-'+name+'/Program.cs']=sha(model/'Program.cs')
        run(['dotnet','run','--project',project,'-c','Release'],'regression-'+name,120)
    r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir()
        for name in ['Program.cs','HostFileMetadataBridge.cs','HostIoBridge.cs']:shutil.copyfile(a/name,consumer/name)
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostFileControlProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
        ET.SubElement(items,'ProjectReference',Include=str(a/'host/Managed.Emulation.Host.csproj'))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostFileControlProbe')
        project=consumer/'HostFileControlProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',project],label+'-restore')
            run(['dotnet',post,project,'--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        actual=run(['dotnet',consumer/'bin/Release/net10.0/HostFileControlProbe.dll'],label+'-jit',30)
        if actual!=expected:raise RuntimeError(label+' JIT differs from native')
        publish=a/(label+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        actual=run([publish/'HostFileControlProbe'],label+'-aot',30)
        if actual!=expected:raise RuntimeError(label+' AOT differs from native')
        r['results'][label+'-aot']['binarySha256']=sha(publish/'HostFileControlProbe')
    if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
    r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['passed']=True;save()
    (ROOT/'artifacts/host-file-control/latest.json').write_text(json.dumps({'receipt':str((out/'receipt.json').relative_to(ROOT))},indent=2)+'\n')
    print('native openat/fcntl and raw/optimized JIT/AOT private descriptor controls pass; HostFiles and InstanceIo regressions pass')
finally:save()
