#!/usr/bin/env python3
"""Run private TCP message C behavior through native and authored BCL callbacks."""
import hashlib,json,os,sys
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
sys.path.insert(0,str(ROOT/'scripts'))
from core_inputs import compiler_identity
base=ROOT/'generated/host-messages';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-messages'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'private TCP sendmsg/recvmsg/getpeername, no ancillary/dgram/Unix support or core execution','passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log: code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),cwd=a,timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    for name in ['probe.c','Program.cs','native.py','layout.c']:shutil.copyfile(ROOT/'tests/HostMessages'/name,a/name)
    shutil.copyfile(ROOT/'src/Host/HostNetworkBridge.cs',a/'HostNetworkBridge.cs')
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    shutil.copyfile(ROOT/'src/Host/include/host-io.h',a/'profile/host-io.h')
    shutil.copyfile(ROOT/'src/Host/HostIoBridge.cs',a/'HostIoBridge.cs')
    shutil.copyfile(ROOT/'src/Host/HostMessagesBridge.cs',a/'HostMessagesBridge.cs')
    shutil.copyfile(ROOT/'src/Host/include/host-messages.h',a/'profile/host-messages.h')
    shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host',ignore=shutil.ignore_patterns('bin','obj'))
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['hostSources']={p.name:sha(p)for p in (a/'host').glob('*.cs')}
    r['compiler']=compiler_identity(cli.parent)
    r['postprocessor_sha256']=sha(post)
    r['runner_sha256']=sha(Path(__file__))
    r['head']=subprocess.check_output(['git','rev-parse','HEAD'],cwd=REPO,text=True).strip();save()
    run(['cc','-std=c17','-Wall','-Wextra','-Werror',a/'layout.c','-o',a/'native-layout'],'native-layout-build')
    layout=run([a/'native-layout'],'native-layout')
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-I',a/'profile',a/'layout.c','-o',a/'profile-layout'],'profile-layout-build')
    if run([a/'profile-layout'],'profile-layout')!=layout:raise RuntimeError('native/profile message layouts differ')
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-shared','-fPIC',a/'probe.c','-o',a/'native.so'],'native-build')
    expected=run(['python3',a/'native.py',a/'native.so'],'native',30)
    raw=a/'raw';opt=a/'optimized'
    run(['dotnet',cli,'-std=c17','-DBLINK_MANAGED_MESSAGES','-I',a/'profile',a/'probe.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
    if r['compiler']!=compiler_identity(cli.parent) or r['postprocessor_sha256']!=sha(post):raise RuntimeError('compiler changed during emission')
    model=a/'regression';model.mkdir()
    shutil.copyfile(ROOT/'tests/InstanceIo/Program.cs',model/'Program.cs')
    project=model/'Regression.csproj'
    project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../host/Managed.Emulation.Host.csproj" /></ItemGroup></Project>')
    run(['dotnet','run','--project',project,'-c','Release'],'regression-instance-io',120)
    r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir()
        for name in ['Program.cs','HostNetworkBridge.cs','HostIoBridge.cs','HostMessagesBridge.cs']:shutil.copyfile(a/name,consumer/name)
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostMessagesProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
        ET.SubElement(items,'ProjectReference',Include=str(a/'host/Managed.Emulation.Host.csproj'))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostMessagesProbe')
        project=consumer/'HostMessagesProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',project],label+'-restore')
            run(['dotnet',post,project,'--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        actual=run(['dotnet',consumer/'bin/Release/net10.0/HostMessagesProbe.dll'],label+'-jit',30)
        if actual!=expected:raise RuntimeError(label+' JIT differs from native')
        publish=a/(label+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        actual=run([publish/'HostMessagesProbe'],label+'-aot',30)
        if actual!=expected:raise RuntimeError(label+' AOT differs from native')
        r['results'][label+'-aot']['binarySha256']=sha(publish/'HostMessagesProbe')
    if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
    r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['passed']=True;save();print('native and raw/optimized JIT/AOT private TCP message operations agree; simultaneous owners/GC and cancellation pass; receipt '+str(out/'receipt.json'))
finally:save()
