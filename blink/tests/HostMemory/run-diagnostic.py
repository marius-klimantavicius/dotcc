#!/usr/bin/env python3
"""Qualify owned diagnostic reads against untouched native signal-guarded reads."""
import difflib, hashlib, json, os
from pathlib import Path
import shutil, subprocess, tempfile, time
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]
REPO=ROOT.parent
BASE=ROOT/'generated/host-diagnostic'; BASE.mkdir(parents=True,exist_ok=True)
ATTEMPT=Path(tempfile.mkdtemp(prefix='attempt-',dir=BASE))
OUT=ROOT/'artifacts/host-diagnostic'/ATTEMPT.name; OUT.mkdir(parents=True)
UPSTREAM=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
CLI=REPO/'DotCC/bin/Release/net10.0/dotcc.dll'
POST=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
ENV=dict(os.environ,LC_ALL='C')
receipt={'kind':'owned-host-diagnostic-read-not-general-address-validity','passed':False,'results':{}}
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
    (ATTEMPT/'native-profile/sys').mkdir(parents=True)
    shutil.copyfile(ROOT/'config/managed-host/sys/mman.h',ATTEMPT/'native-profile/sys/mman.h')
    shutil.copyfile(ROOT/'config/core-config.h',ATTEMPT/'config.h')
    shutil.copyfile(ROOT/'config/target-storage.h',ATTEMPT/'target-storage.h')
    shutil.copyfile(ROOT/'config/core-overrides.json',ATTEMPT/'overrides.json')
    for path in [ROOT/'src/Host/HostMemory.c', ROOT/'src/Host/include/HostMemory.h',
                 UPSTREAM/'blink/pte32.c']:
        shutil.copyfile(path,ATTEMPT/path.name)
    shutil.copyfile(ROOT/'tests/HostMemory/diagnostic-probe.c',ATTEMPT/'probe.c')
    shutil.copyfile(ROOT/'tests/HostMemory/DiagnosticConsumer.cs',ATTEMPT/'Consumer.cs')
    run(['python3',ROOT/'src/Host/scripts/stage-debug.py','--output',ATTEMPT/'debug.c',
         '--receipt',OUT/'debug-adaptation.json'],'stage-debug')
    # The native oracles link the entire unchanged/staged debug.c and real bus.c.
    # The focused managed probe extracts exact function text, not implementations
    # written for the test, avoiding the unrelated unfinished complete core link.
    debug=(ATTEMPT/'debug.c').read_text()
    read_start=debug.index('static i64 ReadWord(int mode, u8 *p) {')
    read_end=debug.index('// TODO(jart): This function should be immune',read_start)
    read_slice=debug[read_start:read_end]
    bus=(UPSTREAM/'blink/bus.c').read_text()
    lock_slice=bus[bus.index('void LockBus('):bus.index('i64 Load8(')]
    loads_slice=bus[bus.index('i64 Load16('):bus.index('void Store8(')]
    stores_slice=bus[bus.index('void Store8('):bus.index('u64 ReadRegister(')]
    header='#include "HostMemory.h"\n#include "blink/bus.h"\n#include "blink/endian.h"\n#include "blink/x86.h"\n#define FAKE_WORD 0x6660666066660666\n'
    (ATTEMPT/'diagnostic-slice.c').write_text(header+lock_slice+loads_slice+stores_slice+read_slice)
    receipt['extractedFunctions']={name:hashlib.sha256(value.encode()).hexdigest()
        for name,value in [('ReadWord-and-staged-ReadWordSafely',read_slice),('LockBus-UnlockBus',lock_slice),('Load16-through-Load64Unlocked',loads_slice),('Store8-through-Store64Unlocked',stores_slice)]}
    receipt['inputs']={str(p.relative_to(ATTEMPT)):sha(p) for p in ATTEMPT.rglob('*') if p.is_file()}
    receipt['compiler']={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}; save()
    args=['-std=c17','-D_GNU_SOURCE','-DNDEBUG','-DNOLINEAR','-I',ATTEMPT,'-I',UPSTREAM]
    for label,extra,debug_file,host_sources in [
        ('native-untouched',['-DBLINK_NATIVE_UNTOUCHED'],UPSTREAM/'blink/debug.c',[]),
        ('native-staged',['-I',ATTEMPT/'native-profile'],ATTEMPT/'debug.c',[ATTEMPT/'HostMemory.c'])]:
        run(['cc',*args,*extra,'-O2','-ffunction-sections','-fdata-sections',ATTEMPT/'probe.c',
             debug_file,UPSTREAM/'blink/bus.c',*host_sources,'-Wl,--gc-sections','-o',ATTEMPT/label],label+'-build')
        run([ATTEMPT/label],label,30)
    expected=(OUT/'native-untouched.log').read_bytes()
    if (OUT/'native-staged.log').read_bytes()!=expected: raise RuntimeError('staged diagnostic differs from untouched native')
    imports=run(['nm','-u',ATTEMPT/'native-staged'],'native-staged-imports').decode()
    receipt['nativeStagedImports']=[line.split()[-1].split('@')[0] for line in imports.splitlines() if line.strip()]
    if {'sigaction','sigprocmask','pthread_sigmask','mmap','mprotect'}.intersection(receipt['nativeStagedImports']):
        raise RuntimeError('staged diagnostic retains native host signal/probing imports')
    raw=ATTEMPT/'raw/Diagnostic'; opt=ATTEMPT/'optimized/Diagnostic'
    inputs=[ATTEMPT/name for name in ['probe.c','diagnostic-slice.c','pte32.c','HostMemory.c']]
    run(['dotnet',CLI,*args,'-DBLINK_TEST_MANAGED','-I',ATTEMPT/'host',*inputs,
         '--overrides-file',ATTEMPT/'overrides.json','--runtime=c','--emit=managedlib','--nest-types',
         '--class-name','DiagnosticHost','--namespace','Managed.Emulation','-o',raw],'translate',180)
    if receipt['compiler']!={p.name:sha(p) for p in [CLI,CLI.with_name('DotCC.Lib.dll'),POST]}: raise RuntimeError('compiler changed during emission')
    receipt['rawGenerated']={str(p.relative_to(raw)):sha(p) for p in raw.rglob('*.cs')}
    shutil.copytree(raw,opt)
    for label,project in [('raw',raw),('optimized',opt)]:
        library=project/'Diagnostic.csproj'
        consumer=ATTEMPT/(label+'-consumer'); consumer.mkdir()
        shutil.copyfile(ATTEMPT/'Consumer.cs',consumer/'Program.cs')
        xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk')
        props=ET.SubElement(xml,'PropertyGroup')
        for key,value in [('TargetFramework','net10.0'),('OutputType','Exe'),('AllowUnsafeBlocks','true'),('WarningsAsErrors','CS8500'),('AssemblyName','DiagnosticConsumer')]: ET.SubElement(props,key).text=value
        items=ET.SubElement(xml,'ItemGroup'); ET.SubElement(items,'ProjectReference',Include=str(library))
        ET.SubElement(items,'TrimmerRootAssembly',Include='Diagnostic')
        csproj=consumer/'DiagnosticConsumer.csproj'; ET.ElementTree(xml).write(csproj,encoding='unicode')
        if label=='optimized':
            run(['dotnet','restore',library],'optimized-restore')
            run(['dotnet',POST,library,'--in-place'],'postprocess')
        run(['dotnet','build',csproj,'-c','Release'],label+'-build')
        check(['dotnet',consumer/'bin/Release/net10.0/DiagnosticConsumer.dll'],label+'-jit')
        publish=ATTEMPT/(label+'-aot')
        run(['dotnet','publish',csproj,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',300)
        check([publish/'DiagnosticConsumer'],label+'-aot')
    if receipt['rawGenerated']!={str(p.relative_to(raw)):sha(p) for p in raw.glob('*.cs')}: raise RuntimeError('raw source changed')
    receipt['optimizedGenerated']={str(p.relative_to(opt)):sha(p) for p in opt.glob('*.cs')}
    receipt['binaries']={str(p.relative_to(ATTEMPT)):sha(p) for p in [ATTEMPT/'native-untouched',ATTEMPT/'native-staged',ATTEMPT/'raw-aot/DiagnosticConsumer',ATTEMPT/'optimized-aot/DiagnosticConsumer',raw/'bin/Release/net10.0/Diagnostic.dll',opt/'bin/Release/net10.0/Diagnostic.dll']}
    receipt['passed']=True; receipt['outputRows']=len(expected.splitlines()); save()
    (ROOT/'artifacts/host-diagnostic/latest.json').write_text(json.dumps({'receipt':str((OUT/'receipt.json').relative_to(ROOT))},indent=2)+'\n')
    print(str(receipt['outputRows'])+' diagnostic rows match untouched/staged native in raw/optimized JIT/AOT; foreign-owner reads rejected')
finally: save()
