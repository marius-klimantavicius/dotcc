#!/usr/bin/env python3
"""Compare precise CPUID exclusion deltas and execute representative native handlers."""
import difflib, hashlib, json, os
from pathlib import Path
import shutil, subprocess, tempfile, time
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]
REPO=ROOT.parent
BASE=ROOT/'generated/host-cpu'; BASE.mkdir(parents=True,exist_ok=True)
ATTEMPT=Path(tempfile.mkdtemp(prefix='attempt-',dir=BASE))
OUT=ROOT/'artifacts/host-cpu'/ATTEMPT.name; OUT.mkdir(parents=True)
UPSTREAM=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
CLI=REPO/'DotCC/bin/Release/net10.0/dotcc.dll'
POST=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
ENV=dict(os.environ,LC_ALL='C')
receipt={'kind':'cpuid-exclusion-advertisements-and-selected-native-handlers','passed':False,'results':{}}
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
    (ATTEMPT/'host/config.h').unlink()
    shutil.copyfile(ROOT/'config/core-config.h',ATTEMPT/'config.h')
    shutil.copyfile(ROOT/'config/target-storage.h',ATTEMPT/'target-storage.h')
    shutil.copyfile(ROOT/'config/core-overrides.json',ATTEMPT/'overrides.json')
    shutil.copyfile(ROOT/'tests/HostCpu/probe.c',ATTEMPT/'probe.c')
    shutil.copyfile(ROOT/'tests/HostCpu/native-instructions.c',ATTEMPT/'native-instructions.c')
    run(['python3',ROOT/'src/HostCpu/stage-cpuid.py','--output',ATTEMPT/'cpuid.c',
         '--receipt',OUT/'cpuid-adaptation.json'],'stage-cpuid')
    receipt['inputs']={str(p.relative_to(ATTEMPT)):sha(p) for p in ATTEMPT.rglob('*') if p.is_file()}
    receipt['compiler']={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}; save()
    receipt['nativeConfigurations']=[]
    def rows(data):
        values=[list(map(lambda x:int(x,16),line.split())) for line in data.decode().splitlines()]
        return {(row[0],row[1]):row[2:] for row in values}
    # Eight independent feature-selection configurations establish that guards
    # remove only disabled bits and retain enabled feature advertisements.
    for flags in range(8):
        profile=ATTEMPT/('native-config-'+str(flags)); profile.mkdir()
        config=(ROOT/'config/core-config.h').read_text()
        disabled={}
        for index,name in enumerate(['DISABLE_X87','DISABLE_MMX','DISABLE_BMI2']):
            disabled[name]=bool(flags & (1<<index))
            if not disabled[name]: config=config.replace('#define '+name+' 1\n','')
        (profile/'config.h').write_text(config)
        shutil.copyfile(ROOT/'config/target-storage.h',profile/'target-storage.h')
        outputs={}
        for variant,cpuid in [('original',UPSTREAM/'blink/cpuid.c'),('staged',ATTEMPT/'cpuid.c')]:
            binary=profile/variant
            run(['cc','-std=c17','-D_GNU_SOURCE','-DNDEBUG','-I',profile,'-I',UPSTREAM,
                 ATTEMPT/'probe.c',cpuid,'-o',binary],'native-'+str(flags)+'-'+variant+'-build')
            outputs[variant]=run([binary],'native-'+str(flags)+'-'+variant,30)
        before,after=rows(outputs['original']),rows(outputs['staged'])
        deltas=[]
        for key,original in before.items():
            wanted=original.copy()
            if key==(1,0) and disabled['DISABLE_MMX']: wanted[3] &= ~(1<<23)
            if key==(0x80000001,0):
                if disabled['DISABLE_MMX']: wanted[3] &= ~(1<<23)
                if disabled['DISABLE_X87']: wanted[3] &= ~1
            if key==(1,0): wanted[2] &= ~sum(1<<bit for bit in [0,1,9,13,23,30])
            if key==(7,0):
                wanted[1] &= ~sum(1<<bit for bit in [0,9,18])
                wanted[2] &= ~(1<<22)
            if key==(0x80000001,0):
                wanted[2] &= ~1
                wanted[3] &= ~(1<<27)
            if key==(0x80000007,0): wanted[3] &= ~(1<<8)
            if key==(6,0): wanted=[0,0,0,0]
            if after[key]!=wanted: raise RuntimeError('unexpected CPUID delta '+str(key))
            if after[key]!=original: deltas.append({'leaf':key,'before':original,'after':after[key]})
        expected_fpu=0 if disabled['DISABLE_X87'] else 1
        expected_mmx=0 if disabled['DISABLE_MMX'] else 1
        expected_bmi=0 if disabled['DISABLE_BMI2'] else 1
        if (after[(1,0)][3]&1)!=expected_fpu or (after[(0x80000001,0)][3]&1)!=expected_fpu:
            raise RuntimeError('FPU advertisements disagree with exclusions')
        if ((after[(1,0)][3]>>23)&1)!=expected_mmx or ((after[(0x80000001,0)][3]>>23)&1)!=expected_mmx:
            raise RuntimeError('MMX advertisements disagree with exclusions')
        if ((after[(7,0)][1]>>8)&1)!=expected_bmi or ((after[(7,0)][1]>>19)&1)!=expected_bmi:
            raise RuntimeError('BMI2/ADX advertisements disagree with exclusions')
        receipt['nativeConfigurations'].append({'disabled':disabled,'deltas':deltas}); save()
        if flags==7: expected=outputs['staged']
    native=ROOT/'build/native/source'
    archive=native/'o/blink/blink.a'
    receipt['nativeArchiveSha256']=sha(archive)
    run(['cc','-D_GNU_SOURCE','-D_DEFAULT_SOURCE','-DNOLINEAR','-I',native,
         ATTEMPT/'native-instructions.c',archive,'-lz','-lrt','-lm','-pthread',
         '-Wl,-Map='+str(OUT/'native-instructions.map'),'-o',ATTEMPT/'native-instructions'],'native-instructions-build')
    run([ATTEMPT/'native-instructions'],'native-instructions',30)
    raw=ATTEMPT/'raw/HostCpu'; opt=ATTEMPT/'optimized/HostCpu'
    args=['-std=c17','-D_GNU_SOURCE','-DNDEBUG','-DNOLINEAR','-DBLINK_TEST_MANAGED',
          '-I',ATTEMPT,'-I',UPSTREAM,'-I',ATTEMPT/'host']
    run(['dotnet',CLI,*args,ATTEMPT/'probe.c',ATTEMPT/'cpuid.c',
         '--overrides-file',ATTEMPT/'overrides.json','--runtime=c','--emit=managedlib',
         '--nest-types','--class-name','CpuProfile','--namespace','Managed.Emulation','-o',raw],'translate',180)
    if receipt['compiler']!={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}: raise RuntimeError('compiler changed during emission')
    receipt['rawGenerated']={str(p.relative_to(raw)):sha(p) for p in raw.rglob('*.cs')}
    shutil.copytree(raw,opt)
    for label,project in [('raw',raw),('optimized',opt)]:
        library=project/'HostCpu.csproj'
        consumer=ATTEMPT/(label+'-consumer'); consumer.mkdir()
        (consumer/'Program.cs').write_text('return Managed.Emulation.CpuProfile.CpuidProbe();\n')
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk')
        props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('AssemblyName','HostCpuConsumer')]: ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup'); ET.SubElement(items,'ProjectReference',Include=str(library))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostCpu')
        csproj=consumer/'HostCpuConsumer.csproj'; ET.ElementTree(xml).write(csproj,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',library],'optimized-restore')
            run(['dotnet',POST,library,'--in-place'],'postprocess')
        run(['dotnet','build',csproj,'-c','Release'],label+'-build')
        check(['dotnet',consumer/'bin/Release/net10.0/HostCpuConsumer.dll'],label+'-jit')
        publish=ATTEMPT/(label+'-aot')
        run(['dotnet','publish',csproj,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        check([publish/'HostCpuConsumer'],label+'-aot')
    if receipt['rawGenerated']!={str(p.relative_to(raw)):sha(p) for p in raw.glob('*.cs')}: raise RuntimeError('raw source changed')
    receipt['optimizedGenerated']={str(p.relative_to(opt)):sha(p) for p in opt.glob('*.cs')}
    receipt['binaries']={str(p.relative_to(ATTEMPT)):sha(p) for p in [ATTEMPT/'native-instructions',ATTEMPT/'raw-aot/HostCpuConsumer',ATTEMPT/'optimized-aot/HostCpuConsumer']}
    receipt['passed']=True; receipt['outputRows']=len(expected.splitlines()); save()
    (ROOT/'artifacts/host-cpu/latest.json').write_text(json.dumps({'receipt':str((OUT/'receipt.json').relative_to(ROOT))},indent=2)+'\n')
    print(str(receipt['outputRows'])+' CPUID rows match staged native in raw/optimized JIT/AOT; eight native flag combinations and seven actual handler probes passed')
finally: save()
