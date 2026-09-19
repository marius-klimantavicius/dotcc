#!/usr/bin/env python3
"""Qualify staged upstream InitMap and bounded anonymous host allocation."""
import difflib, hashlib, json, os
from pathlib import Path
import shutil, subprocess, tempfile, time
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]
REPO=ROOT.parent
BASE=ROOT/'generated/host-memory'; BASE.mkdir(parents=True,exist_ok=True)
ATTEMPT=Path(tempfile.mkdtemp(prefix='attempt-',dir=BASE))
OUT=ROOT/'artifacts/host-memory'/ATTEMPT.name; OUT.mkdir(parents=True)
UPSTREAM=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
CLI=REPO/'DotCC/bin/Release/net10.0/dotcc.dll'
POST=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
ENV=dict(os.environ,LC_ALL='C')
receipt={'kind':'normal-bounded-host-memory-and-upstream-initmap-not-guest-page-algorithms','passed':False,'results':{},
         'scope':'Normal allocation/protection/growth/cross-page copy/unmap/disposal and concurrent owners; no custom injected failure or invalid access',
         'excluded_historical_cases':['forced quota exhaustion','invalid mapping/protection/unmap arguments','foreign-owner mutation','operations outside owner lifetime','unsupported mapping modes']}
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
receipt['runner_sha256']=sha(Path(__file__))
def save(): (OUT/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
def run(cmd,name,timeout=180):
    start=time.monotonic()
    with (OUT/(name+'.log')).open('wb') as stream:
        r=subprocess.run(list(map(str,cmd)),stdout=stream,stderr=subprocess.STDOUT,env=ENV,timeout=timeout)
    receipt['results'][name]={'command':list(map(str,cmd)),'exit':r.returncode,'seconds':time.monotonic()-start}; save()
    if r.returncode: raise RuntimeError(name+' failed: '+str(OUT/(name+'.log')))
    return (OUT/(name+'.log')).read_bytes()
def check(cmd,name):
    binary=Path(cmd[-1] if name.endswith('-jit') else cmd[0])
    before={str(p):sha(p) for p in ([binary]+list(binary.parent.glob('*.dll')))}
    value=run(cmd,name,30)
    if any(sha(Path(p))!=digest for p,digest in before.items()): raise RuntimeError(name+' execution binary changed')
    receipt['results'][name]['binary_inputs']=before
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
    (ATTEMPT/'native-profile/sys').mkdir(parents=True)
    shutil.copyfile(ROOT/'config/managed-host/sys/mman.h',ATTEMPT/'native-profile/sys/mman.h')
    config=(ROOT/'config/core-config.h').read_text()
    (ATTEMPT/'config.h').write_text(config)
    shutil.copyfile(ROOT/'config/target-storage.h',ATTEMPT/'target-storage.h')
    shutil.copyfile(ROOT/'config/core-overrides.json',ATTEMPT/'overrides.json')
    for path in [ROOT/'tests/HostMemory/probe.c', ROOT/'tests/HostMemory/constants.c',
                 ROOT/'tests/HostMemory/Consumer.cs',
                 ROOT/'src/HostMemory/HostMemory.c', ROOT/'src/HostMemory/HostMemory.h',
                 UPSTREAM/'blink/flag.c', UPSTREAM/'blink/pte32.c']:
        shutil.copyfile(path,ATTEMPT/path.name)
    run(['python3',ROOT/'src/HostMemory/stage-map.py','--output',ATTEMPT/'map.c',
         '--receipt',OUT/'map-adaptation.json'],'stage-map')
    receipt['inputs']={str(p.relative_to(ATTEMPT)):sha(p) for p in ATTEMPT.rglob('*') if p.is_file()}
    receipt['compiler']={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}; save()
    args=['-std=c17','-D_GNU_SOURCE','-DNDEBUG','-DNOLINEAR','-I',ATTEMPT,'-I',UPSTREAM]
    inputs=[ATTEMPT/name for name in ['probe.c','map.c','flag.c','pte32.c','HostMemory.c']]
    run(['cc',*args,'-I',ATTEMPT/'native-profile','-c',ATTEMPT/'map.c','-o',ATTEMPT/'map-audit.o'],'map-audit-build')
    imports=run(['nm','-u',ATTEMPT/'map-audit.o'],'map-imports').decode()
    receipt['mapImports']=[line.split()[-1] for line in imports.splitlines() if line.strip()]
    forbidden={'mmap','munmap','mprotect','msync','sysconf','mkstemp','unlink','ftruncate','close'}
    if forbidden.intersection(receipt['mapImports']): raise RuntimeError('staged map retains native OS discovery or fallback imports')
    run(['cc',*args,'-I',ATTEMPT/'native-profile',*inputs,'-o',ATTEMPT/'native-profile-test'],'native-profile-build')
    expected=run([ATTEMPT/'native-profile-test'],'native-profile',30)
    for label,extra in [('system',[]),('profile',['-I',ATTEMPT/'native-profile'])]:
        run(['cc','-std=c17',*extra,ATTEMPT/'constants.c','-o',ATTEMPT/('constants-'+label)],'constants-'+label+'-build')
        run([ATTEMPT/('constants-'+label)],'constants-'+label,30)
    if (OUT/'constants-system.log').read_bytes() != (OUT/'constants-profile.log').read_bytes():
        raise RuntimeError('mman constant profile differs from native')
    # Untouched discovery algorithm runs only in this separate native oracle.
    (ATTEMPT/'native-discovery.c').write_text('#include <stdio.h>\n#include "blink/map.h"\n#include "blink/flag.h"\nint main(void) { InitMap(); printf("native pagesize=%ld addressbits=%d\\n", FLAG_pagesize, FLAG_vabits); return 0; }\n')
    run(['cc',*args,ATTEMPT/'native-discovery.c',UPSTREAM/'blink/map.c',UPSTREAM/'blink/abort.c',ATTEMPT/'flag.c',
         '-o',ATTEMPT/'native-discovery'],'native-discovery-build')
    run([ATTEMPT/'native-discovery'],'native-discovery',30)
    # Run unchanged instruction/page-management algorithms natively through the
    # same memory boundary. Override only the staged map and thread-disabled Bus.
    shutil.copyfile(ROOT/'tests/HostMemory/native-core-driver.c',ATTEMPT/'native-core-driver.c')
    run(['cc',*args,'-I',ATTEMPT/'native-profile','-Dmain=NativeCoreProbe','-c',
         ROOT/'src/core-probe/probe.c','-o',ATTEMPT/'native-core-probe.o'],'native-core-probe-build')
    native_archive=ROOT/'build/native/source/o/blink/blink.a'
    receipt['nativeArchiveSha256']=sha(native_archive)
    run(['cc',*args,'-I',ATTEMPT/'native-profile',ATTEMPT/'native-core-driver.c',
         ATTEMPT/'native-core-probe.o',ATTEMPT/'map.c',ATTEMPT/'flag.c',ATTEMPT/'HostMemory.c',
         UPSTREAM/'blink/bus.c',native_archive,'-lz','-lrt','-lm','-pthread',
         '-Wl,-Map='+str(OUT/'native-core-link.map'),'-o',ATTEMPT/'native-core'],'native-core-link')
    run([ATTEMPT/'native-core'],'native-core',30)
    raw=ATTEMPT/'raw/HostMemory'; opt=ATTEMPT/'optimized/HostMemory'
    run(['dotnet',CLI,*args,'-DBLINK_TEST_MANAGED','-I',ATTEMPT/'host',*inputs,
         '--overrides-file',ATTEMPT/'overrides.json','--runtime=c','--emit=managedlib','--nest-types',
         '--class-name','MemoryHost','--namespace','Managed.Emulation','-o',raw],'translate',180)
    if receipt['compiler']!={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}: raise RuntimeError('compiler changed during emission')
    receipt['rawGenerated']={str(p.relative_to(raw)):sha(p) for p in raw.rglob('*.cs')}
    shutil.copytree(raw,opt)
    for label,project in [('raw',raw),('optimized',opt)]:
        library=project/'HostMemory.csproj'
        consumer=ATTEMPT/(label+'-consumer'); consumer.mkdir()
        shutil.copyfile(ATTEMPT/'Consumer.cs',consumer/'Program.cs')
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk')
        props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('WarningsAsErrors','CS8500'),('AssemblyName','HostMemoryConsumer')]: ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup'); ET.SubElement(items,'ProjectReference',Include=str(library))
        ET.SubElement(items,'TrimmerRootAssembly',Include='HostMemory')
        csproj=consumer/'HostMemoryConsumer.csproj'; ET.ElementTree(xml).write(csproj,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',library],'optimized-restore')
            run(['dotnet',POST,library,'--in-place'],'postprocess')
        run(['dotnet','build',csproj,'-c','Release'],label+'-build')
        check(['dotnet',consumer/'bin/Release/net10.0/HostMemoryConsumer.dll'],label+'-jit')
        publish=ATTEMPT/(label+'-aot')
        run(['dotnet','publish',csproj,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        check([publish/'HostMemoryConsumer'],label+'-aot')
    if receipt['rawGenerated']!={str(p.relative_to(raw)):sha(p) for p in raw.glob('*.cs')}: raise RuntimeError('raw source changed')
    receipt['optimizedGenerated']={str(p.relative_to(opt)):sha(p) for p in opt.glob('*.cs')}
    receipt['binaries']={str(p.relative_to(ATTEMPT)):sha(p) for p in [ATTEMPT/'native-discovery',ATTEMPT/'native-profile-test',ATTEMPT/'raw-aot/HostMemoryConsumer',ATTEMPT/'optimized-aot/HostMemoryConsumer',raw/'bin/Release/net10.0/HostMemory.dll',opt/'bin/Release/net10.0/HostMemory.dll']}
    receipt['passed']=True; receipt['outputRows']=len(expected.splitlines()); save()
    (ROOT/'artifacts/host-memory/latest.json').write_text(json.dumps({'receipt':str((OUT/'receipt.json').relative_to(ROOT))},indent=2)+'\n')
    print(str(receipt['outputRows'])+' host memory rows match staged native in raw/optimized JIT/AOT; two workers retain allocations through compacting GC')
finally: save()
