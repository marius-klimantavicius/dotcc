#!/usr/bin/env python3
"""Run private file mmap through native and authored positional-read callbacks."""
import argparse,hashlib,json,os,signal,sys
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
sys.path.insert(0,str(REPO/'Scripts'))
from production_support import locked_command
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--native-only',action='store_true',help='Run only native ordinary mapping checks; never emit managed code')
args=parser.parse_args()
base=ROOT/'generated/host-file-mapping';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-file-mapping'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'ordinary private file snapshots, EOF availability metadata, protection and configured-budget recovery; historical custom fault injection and native beyond-EOF crash probes excluded','passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    requested=list(map(str,cmd));effective=locked_command(requested);code=None
    try:
        with(out/(name+'.log')).open('wb')as log:
            process=subprocess.Popen(effective,stdout=log,stderr=subprocess.STDOUT,
                env=dict(os.environ,LC_ALL='C'),cwd=a,start_new_session=True)
            try:code=process.wait(timeout=timeout)
            except BaseException:
                try:os.killpg(process.pid,signal.SIGKILL)
                except ProcessLookupError:pass
                process.wait();raise
    finally:
        r['results'][name]={'command':requested,'effectiveCommand':effective,'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()

try:
    for name in ['probe.c','Program.cs']:shutil.copyfile(ROOT/'tests/HostFileMapping'/name,a/name)
    shutil.copyfile(ROOT/'src/Host/HostFileMetadataBridge.cs',a/'HostFileMetadataBridge.cs')
    for name in ['HostMemory.c','HostMemory.h','HostFileMapping.c','HostFileMapping.h']:
        source=ROOT/'src/Host'/('include' if name.endswith('.h') else '')/name
        shutil.copyfile(source,a/name)
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    shutil.copyfile(ROOT/'src/Host/include/host-io.h',a/'profile/host-io.h')
    shutil.copyfile(ROOT/'src/Host/HostIoBridge.cs',a/'HostIoBridge.cs')
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    shutil.copytree(ROOT/'src/Managed.Emulation.Host',a/'host',ignore=shutil.ignore_patterns('bin','obj'))
    r['hostSources']={p.name:sha(p)for p in (a/'host').glob('*.cs')}
    r['compiler']={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]};save()
    (a/'native-profile/sys').mkdir(parents=True)
    shutil.copyfile(a/'profile/sys/mman.h',a/'native-profile/sys/mman.h')
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-I',a,a/'probe.c','-o',a/'native'],'native-build')
    (a/'image').write_bytes(bytes(i%251 for i in range(131123)))
    (a/'empty').write_bytes(b'')
    expected=run([a/'native'],'native',30)
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-DBLINK_NATIVE_ADAPTER','-I',a,'-I',a/'native-profile',a/'probe.c',a/'HostMemory.c','-o',a/'native-adapter'],'native-adapter-build')
    if run([a/'native-adapter'],'native-adapter',30)!=expected:raise RuntimeError('native adapter differs from supported native mapping behavior')
    if not args.native_only:
        raw=a/'raw';opt=a/'optimized'
        run(['dotnet',cli,'-std=c17','-DBLINK_MANAGED_FILEMAP','-I',a,'-I',a/'profile',a/'probe.c',a/'HostMemory.c',a/'HostFileMapping.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
        if r['compiler']!={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]}:raise RuntimeError('compiler changed during emission')
        for name in ['HostFiles','InstanceIo']:
            model=a/('regression-'+name);model.mkdir()
            shutil.copyfile(ROOT/'tests'/name/'Program.cs',model/'Program.cs')
            project=model/'Regression.csproj'
            project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../host/Managed.Emulation.Host.csproj" /></ItemGroup></Project>')
            r['inputs']['regression-'+name+'/Program.cs']=sha(model/'Program.cs')
            run(['dotnet','build',project,'-c','Release','-m:1'],'regression-'+name+'-build',120)
            run(['dotnet',model/'bin/Release/net10.0/Regression.dll'],'regression-'+name,30)
        r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
        for label,generated in [('raw',raw),('optimized',opt)]:
            consumer=a/(label+'-consumer');consumer.mkdir()
            for name in ['Program.cs','HostFileMetadataBridge.cs','HostIoBridge.cs']:shutil.copyfile(a/name,consumer/name)
            xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
            for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostFileMappingProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
            items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
            ET.SubElement(items,'ProjectReference',Include=str(a/'host/Managed.Emulation.Host.csproj'))
            ET.SubElement(items,'TrimmerRootAssembly',Include='HostFileMappingProbe')
            project=consumer/'HostFileMappingProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
            if label=='optimized':
                run(['dotnet','restore',project],label+'-restore')
                run(['dotnet',post,project,'--in-place'],'postprocess')
            run(['dotnet','build',project,'-c','Release','-m:1'],label+'-build')
            actual=run(['dotnet',consumer/'bin/Release/net10.0/HostFileMappingProbe.dll'],label+'-jit',30)
            if actual!=expected:raise RuntimeError(label+' JIT differs from native')
            publish=a/(label+'-aot')
            run(['dotnet','publish',project,'-c','Release','-m:1','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
            actual=run([publish/'HostFileMappingProbe'],label+'-aot',30)
            if actual!=expected:raise RuntimeError(label+' AOT differs from native')
            r['results'][label+'-aot']['binarySha256']=sha(publish/'HostFileMappingProbe')
        if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
        r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['native_passed']=True;r['managed_passed']=not args.native_only
    r['passed']=True;save()
    (ROOT/'artifacts/host-file-mapping/latest.json').write_text(json.dumps({'receipt':str((out/'receipt.json').relative_to(ROOT))},indent=2)+'\n')
    print('ordinary native private mapping checks pass' if args.native_only else 'ordinary native and raw/optimized JIT/AOT private mapping checks pass')
finally:save()
