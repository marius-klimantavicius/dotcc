#!/usr/bin/env python3
"""Compare actual upstream types in native/profile and raw/optimized JIT/AOT."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]

import difflib, hashlib, json, os
from pathlib import Path
import shutil, subprocess, tempfile, time
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]
REPO=ROOT.parent
BASE=ROOT/'generated/core-abi'; BASE.mkdir(parents=True,exist_ok=True)
ATTEMPT=Path(tempfile.mkdtemp(prefix='attempt-',dir=BASE))
OUT=ROOT/'artifacts/core-abi'/ATTEMPT.name; OUT.mkdir(parents=True)
UPSTREAM=ROOT/("ref/" + _CAMPAIGN_SOURCE["directory"])
CLI=REPO/'DotCC/bin/Release/net10.0/dotcc.dll'
POST=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
ENV=dict(os.environ,LC_ALL='C')
receipt={'kind':'actual-upstream-storage-not-instruction-execution','passed':False,'results':{}}
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def save(): (OUT/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
def run(cmd,name,timeout=180):
    start=time.monotonic()
    with (OUT/(name+'.log')).open('wb') as stream:
        r=subprocess.run(list(map(str,cmd)),stdout=stream,stderr=subprocess.STDOUT,env=ENV,timeout=timeout)
    receipt['results'][name]={'command':list(map(str,cmd)),'exit':r.returncode,'seconds':time.monotonic()-start}; save()
    if r.returncode: raise RuntimeError(name+' failed: '+str(OUT/(name+'.log')))
    return (OUT/(name+'.log')).read_bytes()
def check(cmd,name):
    value=run(cmd,name,30)
    receipt['results'][name]['sha256']=hashlib.sha256(value).hexdigest(); save()
    if value!=expected:
        (OUT/(name+'.diff')).write_text(''.join(difflib.unified_diff(expected.decode().splitlines(True),value.decode().splitlines(True),fromfile='native-profile',tofile=name)))
        raise RuntimeError(name+' differs from native profile')
try:
    manifest=ROOT/'config/source-inventory.json'
    for row in json.loads(manifest.read_text())['files']:
        if sha(UPSTREAM/row['path'])!=row['sha256']: raise RuntimeError('source checksum mismatch: '+row['path'])
    receipt['sourceManifestSha256']=sha(manifest)
    shutil.copytree(ROOT/'config/managed-host',ATTEMPT/'host')
    shutil.copyfile(ROOT/'config/core-config.h',ATTEMPT/'config.h')
    shutil.copyfile(ROOT/'config/target-storage.h',ATTEMPT/'target-storage.h')
    shutil.copyfile(ROOT/'config/core-overrides.json',ATTEMPT/'overrides.json')
    shutil.copyfile(ROOT/'tests/CoreAbi/probe.c',ATTEMPT/'probe.c')
    receipt['inputs']={str(p.relative_to(ATTEMPT)):sha(p) for p in ATTEMPT.rglob('*') if p.is_file()}
    receipt['compiler']={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}; save()
    args=['-std=c17','-D_GNU_SOURCE','-DNDEBUG','-DNOLINEAR','-I',ATTEMPT,'-I',UPSTREAM]
    for name,extra in [('native',[]),('native-profile',['-DBLINK_NATIVE_PROFILE','-iquote',ATTEMPT/'host'])]:
        run(['cc',*args,*extra,ATTEMPT/'probe.c','-o',ATTEMPT/name],name+'-build')
        run([ATTEMPT/name],name,30)
    expected=(OUT/'native-profile.log').read_bytes()
    baseline=dict(line.rsplit(' ',1) for line in (OUT/'native.log').read_text().splitlines() if not line.startswith('register.'))
    current=dict(line.rsplit(' ',1) for line in expected.decode().splitlines() if not line.startswith('register.'))
    receipt['nativeProfileDeltas']={key:{'native':value,'profile':current[key]} for key,value in baseline.items() if current[key]!=value}
    raw=ATTEMPT/'raw/CoreAbi'; opt=ATTEMPT/'optimized/CoreAbi'
    run(['dotnet',CLI,*args,'-I',ATTEMPT/'host',ATTEMPT/'probe.c','--overrides-file',ATTEMPT/'overrides.json','--runtime=c','--emit=managedlib','--nest-types','--class-name','CoreTypes','--namespace','Managed.Emulation','-o',raw],'translate',180)
    if receipt['compiler']!={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}: raise RuntimeError('compiler changed during emission')
    receipt['rawGenerated']={str(p.relative_to(raw)):sha(p) for p in raw.rglob('*.cs')}
    shutil.copytree(raw,opt)
    for label,project in [('raw',raw),('optimized',opt)]:
        library=project/'CoreAbi.csproj'
        consumer=ATTEMPT/(label+'-consumer'); consumer.mkdir()
        (consumer/'Program.cs').write_text('return Managed.Emulation.CoreTypes.main();\n')
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk')
        props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('AssemblyName','CoreAbiConsumer')]: ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup'); ET.SubElement(items,'ProjectReference',Include=str(library))
        ET.SubElement(items,'TrimmerRootAssembly',Include='CoreAbi')
        csproj=consumer/'CoreAbiConsumer.csproj'; ET.ElementTree(xml).write(csproj,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',library],'optimized-restore')
            run(['dotnet',POST,library,'--in-place'],'postprocess')
        run(['dotnet','build',csproj,'-c','Release'],label+'-build')
        check(['dotnet',consumer/'bin/Release/net10.0/CoreAbiConsumer.dll'],label+'-jit')
        publish=ATTEMPT/(label+'-aot')
        run(['dotnet','publish',csproj,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        check([publish/'CoreAbiConsumer'],label+'-aot')
    if receipt['rawGenerated']!={str(p.relative_to(raw)):sha(p) for p in raw.glob('*.cs')}: raise RuntimeError('raw source changed')
    receipt['optimizedGenerated']={str(p.relative_to(opt)):sha(p) for p in opt.glob('*.cs')}
    receipt['binaries']={str(p.relative_to(ATTEMPT)):sha(p) for p in [ATTEMPT/'native',ATTEMPT/'native-profile',ATTEMPT/'raw-aot/CoreAbiConsumer',ATTEMPT/'optimized-aot/CoreAbiConsumer',raw/'bin/Release/net10.0/CoreAbi.dll',opt/'bin/Release/net10.0/CoreAbi.dll']}
    receipt['passed']=True; receipt['outputRows']=len(expected.splitlines()); save()
    print(str(receipt['outputRows'])+' actual upstream ABI/register outputs match native profile in raw/optimized JIT/AOT')
finally: save()
