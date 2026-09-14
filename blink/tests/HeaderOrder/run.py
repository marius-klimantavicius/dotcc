#!/usr/bin/env python3
"""Native-check socket/guest records in both include orders, then link objects."""
import argparse,hashlib,json,os
from pathlib import Path
import shutil,subprocess,sys,tempfile,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]; REPO=ROOT.parent
sys.path.insert(0,str(ROOT/'scripts'))
from core_inputs import compiler_identity
parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--native-only',action='store_true');args=parser.parse_args()
base=ROOT/'generated/header-order';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base));out=ROOT/'artifacts/header-order'/a.name;out.mkdir(parents=True)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
upstream=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
r={'scope':'opposite socket/guest header orders and real record layout, not guest execution','passed':False,'results':{}}
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,label,timeout=180):
    with(out/(label+'.log')).open('wb')as log:code=subprocess.run(list(map(str,cmd)),stdout=log,stderr=subprocess.STDOUT,env=dict(os.environ,LC_ALL='C'),timeout=timeout).returncode
    r['results'][label]={'command':list(map(str,cmd)),'exit_code':code};save()
    if code:raise RuntimeError(label+' failed: '+str(out/(label+'.log')))
    return(out/(label+'.log')).read_bytes()
def check(cmd,label):
    if run(cmd,label,30)!=expected:raise RuntimeError(label+' differs from native')
try:
    shutil.copytree(ROOT/'config/managed-host',a/'host')
    for name in ['first.c','second.c','main.c']:shutil.copyfile(ROOT/'tests/HeaderOrder'/name,a/name)
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    inventory=json.loads((ROOT/'config/source-inventory.json').read_text())
    for row in inventory['files']:
        if sha(upstream/row['path'])!=row['sha256']:raise RuntimeError('upstream source mismatch: '+row['path'])
    r['upstream_inventory_sha256']=sha(ROOT/'config/source-inventory.json')
    sources=[a/name for name in ['first.c','second.c','main.c']]
    run(['cc','-std=c17','-D_GNU_SOURCE','-Wall','-Wextra','-Werror','-I',upstream,*sources,'-o',a/'native'],'native-build')
    expected=run([a/'native'],'native',30)
    run(['cc','-std=c17','-D_GNU_SOURCE','-Wall','-Wextra','-Werror','-I',upstream,'-I',a/'host',*sources,'-o',a/'native-profile'],'native-profile-build')
    check([a/'native-profile'],'native-profile');r['native_passed']=True;save()
    if args.native_only:
        print('native header order/layout passed: '+str(out/'receipt.json'));sys.exit(0)
    r['compiler']=compiler_identity(cli.parent)
    r['postprocessor']=compiler_identity(post.parent) if (post.parent/'dotcc.dll').exists() else {p.name:sha(p)for p in post.parent.glob('*.dll')}
    objects=[]
    for source in sources:
        target=a/(source.stem+'.cs')
        run(['dotnet',cli,'-std=c17','-D_GNU_SOURCE','-I',upstream,'-I',a/'host',source,'--emit=obj','-o',target],source.stem+'-emit')
        objects.append(target)
    raw=a/'raw';opt=a/'optimized'
    run(['dotnet',cli,*objects,'--emit=managedlib','--nest-types','--class-name','HeaderOrder','--namespace','Managed.Emulation','--runtime=c','-o',raw],'link')
    if r['compiler']!=compiler_identity(cli.parent):raise RuntimeError('compiler changed during object emission/link')
    r['object_hashes']={p.name:sha(p)for p in objects}
    r['raw_generated']={p.name:sha(p)for p in raw.glob('*.cs')};shutil.copytree(raw,opt)
    for label,generated in [('raw',raw),('optimized',opt)]:
        consumer=a/(label+'-consumer');consumer.mkdir();(consumer/'Program.cs').write_text('return Managed.Emulation.HeaderOrder.main();\n')
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('AssemblyName','HeaderOrderConsumer'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup');ET.SubElement(items,'ProjectReference',Include=str(generated/'raw.csproj'));ET.SubElement(items,'TrimmerRootAssembly',Include='raw')
        project=consumer/'HeaderOrderConsumer.csproj';ET.ElementTree(xml).write(project,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',generated/'raw.csproj'],'optimized-restore')
            run(['dotnet',post,generated/'raw.csproj','--in-place'],'postprocess')
        run(['dotnet','build',project,'-c','Release'],label+'-build')
        check(['dotnet',consumer/'bin/Release/net10.0/HeaderOrderConsumer.dll'],label+'-jit')
        publish=a/(label+'-aot');run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        check([publish/'HeaderOrderConsumer'],label+'-aot')
    if r['raw_generated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw generated source changed')
    r['passed']=True;save();print('native and raw/optimized JIT/AOT header orders/layout pass: '+str(out/'receipt.json'))
finally:save()
