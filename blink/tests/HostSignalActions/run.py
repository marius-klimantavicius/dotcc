#!/usr/bin/env python3
"""Run private signal disposition registration through native and authored BCL callbacks."""
import argparse,hashlib,json,os
from pathlib import Path
import shutil,subprocess,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
parser=argparse.ArgumentParser();parser.add_argument('--object-link',action='store_true');args=parser.parse_args()
base=ROOT/'generated/host-signal-actions';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/host-signal-actions'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
r={'scope':'real authored private signal registration state without delivery, not core execution','objectLink':args.object_link,'passed':False,'results':{}}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    with(out/(name+'.log')).open('wb')as log: code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),timeout=timeout).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code};save()
    if code:raise RuntimeError(name+' failed: '+str(out/(name+'.log')))
    return(out/(name+'.log')).read_bytes()
try:
    for name in ['probe.c','Program.cs']:shutil.copyfile(ROOT/'tests/HostSignalActions'/name,a/name)
    shutil.copytree(ROOT/'config/managed-host',a/'profile')
    for name in ['HostSignalActions.c','HostSignalActions.h']:shutil.copyfile(ROOT/'src/HostSignalActions'/name,a/name)
    shutil.copyfile(ROOT/'tests/HostSignalActions/native-guard.c',a/'native-guard.c')
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['compiler']={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]};save()
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-Wno-cast-function-type',a/'probe.c','-o',a/'native'],'native-build')
    expected=run([a/'native'],'native',30)
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-c',a/'native-guard.c','-o',a/'native-guard.o'],'native-stub-build')
    run(['cc','-std=c17','-Wall','-Wextra','-Werror','-Wno-cast-function-type','-DBLINK_PRIVATE_ACTIONS','-I',a/'profile',a/'probe.c',a/'HostSignalActions.c',a/'native-guard.o','-o',a/'native-staged'],'native-staged-build')
    if run([a/'native-staged'],'native-staged',30)!=expected:raise RuntimeError('native staged differs from untouched libc')
    raw=a/'raw';opt=a/'optimized'
    if args.object_link:
        objects=[]
        for name in ['probe','HostSignalActions']:
            target=a/(name+'.obj.cs');objects.append(target)
            run(['dotnet',cli,'-std=c17','-DBLINK_PRIVATE_ACTIONS','-DBLINK_MANAGED_ACTIONS','-I',a/'profile',a/(name+'.c'),'--emit=obj','-o',target],'object-'+name)
        run(['dotnet',cli,*objects,'--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'link')
    else:
        run(['dotnet',cli,'-std=c17','-DBLINK_PRIVATE_ACTIONS','-DBLINK_MANAGED_ACTIONS','-I',a/'profile',a/'probe.c',a/'HostSignalActions.c','--runtime=c','--emit=managedlib','--nest-types','--class-name','Blink','--namespace','Managed.Emulation','-o',raw],'translate')
    if r['compiler']!={p.name:sha(p)for p in [cli,cli.with_name('DotCC.Lib.dll'),post]}:raise RuntimeError('compiler changed during emission')
    r['rawGenerated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir()
        for name in ['Program.cs']:shutil.copyfile(a/name,consumer/name)
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('Nullable','enable'),('AssemblyName','HostSignalActionsProbe'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'Compile',Include=str(generated/'*.cs'))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostSignalActionsProbe')
        project=consumer/'HostSignalActionsProbe.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',project],label+'-restore')
            run(['dotnet',post,project,'--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        actual=run(['dotnet',consumer/'bin/Release/net10.0/HostSignalActionsProbe.dll'],label+'-jit',30)
        if actual!=expected:raise RuntimeError(label+' JIT differs from native')
        publish=a/(label+'-aot')
        run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        actual=run([publish/'HostSignalActionsProbe'],label+'-aot',30)
        if actual!=expected:raise RuntimeError(label+' AOT differs from native')
        r['results'][label+'-aot']['binarySha256']=sha(publish/'HostSignalActionsProbe')
    if r['rawGenerated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw source changed')
    r['optimizedGenerated']={p.name:sha(p)for p in opt.glob('*.cs')}
    r['passed']=True;save()
    (ROOT/('artifacts/host-signal-actions/latest-object.json' if args.object_link else 'artifacts/host-signal-actions/latest.json')).write_text(json.dumps({'receipt':str((out/'receipt.json').relative_to(ROOT))},indent=2)+'\n')
    print('native/staged and raw/optimized JIT/AOT private signal registration pass; native OS disposition/mask unchanged')
finally:save()
