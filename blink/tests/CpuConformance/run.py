#!/usr/bin/env python3
"""Native hardware versus pinned native interpreter; no managed core claim."""
import hashlib,json,os,platform,shutil,subprocess,tempfile,time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];REPO=ROOT.parent
base=ROOT/'generated/cpu-conformance';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/cpu-conformance'/a.name;out.mkdir(parents=True)
r={'schema':1,'scope':'native x86-64 hardware versus unchanged pinned native ExecuteInstruction; managed execution unqualified','passed':False,'results':{},'comparisons':[]}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=60):
    start=time.monotonic()
    with(out/(name+'.stdout')).open('wb')as stdout,(out/(name+'.stderr')).open('wb')as stderr:
        code=subprocess.run(list(map(str,cmd)),stdout=stdout,stderr=stderr,timeout=timeout,env=dict(os.environ,LC_ALL='C')).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code,'seconds':time.monotonic()-start,'stdoutSha256':sha(out/(name+'.stdout')),'stderrSha256':sha(out/(name+'.stderr'))};save()
    if code:raise RuntimeError(name+' failed; see '+str(out))
    return(out/(name+'.stdout')).read_bytes()
try:
    if platform.system()!='Linux' or platform.machine()!='x86_64':raise RuntimeError('reference requires Linux x86-64, no substituted expectations')
    native=ROOT/'build/native/source';upstream=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
    manifest=ROOT/'config/source-inventory.json'
    for row in json.loads(manifest.read_text())['files']:
        if sha(upstream/row['path'])!=row['sha256']:raise RuntimeError('pinned source changed: '+row['path'])
    for name in ['hardware.c','interpreter.c','describe.c','corpus.h','output.h']:shutil.copyfile(ROOT/'tests/CpuConformance'/name,a/name)
    # Preserve the archive and the exact header/config input to its consumer.
    shutil.copyfile(native/'o/blink/blink.a',a/'blink.a')
    shutil.copyfile(native/'config.h',a/'config.h')
    (a/'blink').mkdir()
    for p in (native/'blink').glob('*.h'):shutil.copyfile(p,a/'blink'/p.name)
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['sourceManifestSha256']=sha(manifest);r['runnerSha256']=sha(Path(__file__))
    r['head']=subprocess.check_output(['git','-C',str(REPO),'rev-parse','HEAD'],text=True).strip()
    r['platform']=platform.platform();r['compiler']=run(['cc','--version'],'compiler-version').decode()
    if Path('/proc/cpuinfo').exists():shutil.copyfile('/proc/cpuinfo',out/'hardware-cpuinfo.txt')
    run(['cc','-std=c17','-O2','-Wall','-Wextra','-Werror',a/'hardware.c','-o',a/'hardware'],'hardware-build')
    run(['cc','-std=c17','-O2','-Wall','-Wextra','-Werror',a/'describe.c','-o',a/'describe'],'describe-build')
    run(['cc','-std=c17','-D_GNU_SOURCE','-D_DEFAULT_SOURCE','-DNOLINEAR','-I',a,a/'interpreter.c',a/'blink.a','-lz','-lrt','-lm','-pthread','-Wl,-Map='+str(out/'interpreter-link.map'),'-o',a/'interpreter'],'interpreter-build')
    run(['objdump','-d',a/'hardware'],'hardware-disassembly')
    inputs=json.loads(run([a/'describe'],'corpus'))
    for case in inputs['cases']:
        i=case['index'];ref=json.loads(run([a/'hardware',str(i)],f'hardware-{i:02}'))
        actual=json.loads(run([a/'interpreter',str(i)],f'interpreter-{i:02}'))
        # Faulting instructions compare fault/IP/memory only. Full raw capture
        # remains evidence, but unspecified flags/register effects are excluded.
        fields=['name','signal','ip','memory']+([] if case['fault'] else ['ax','cx','dx','xmm'])
        differences=[key for key in fields if ref[key]!=actual[key]]
        mask=int(case['flagMask'],16)
        if (int(ref['flags'],16)^int(actual['flags'],16))&mask:differences.append('definedFlags')
        record={'case':case['name'],'fields':fields,'definedFlagsMask':case['flagMask'],'matched':not differences,'differences':differences}
        r['comparisons'].append(record);save()
    if any(not item['matched']for item in r['comparisons']):raise RuntimeError('hardware/interpreter differences preserved in receipt')
    r['binaries']={name:sha(a/name)for name in ['hardware','describe','interpreter']}
    if r['inputs']!={name:sha(a/name)for name in r['inputs']}:raise RuntimeError('snapshot changed')
    r['passed']=True;save();print('12 native hardware/interpreter cases pass; managed conformance not yet qualified; receipt '+str(out/'receipt.json'))
finally:save()
