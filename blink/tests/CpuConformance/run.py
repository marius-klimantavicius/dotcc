#!/usr/bin/env python3
"""Native hardware versus pinned native interpreter; no managed core claim."""
import argparse,hashlib,json,os,platform,shutil,subprocess,tempfile,time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];REPO=ROOT.parent
from features import inventory
parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--observe-differences',action='store_true');parser.add_argument('--staged-fp',action='store_true');args=parser.parse_args()
base=ROOT/'generated/cpu-conformance';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/cpu-conformance'/a.name;out.mkdir(parents=True)
r={'schema':1,'scope':'native x86-64 hardware versus unchanged pinned native ExecuteInstruction; managed execution unqualified','passed':False,'results':{},'comparisons':[]}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=60,allow_mismatch=False):
    start=time.monotonic()
    with(out/(name+'.stdout')).open('wb')as stdout,(out/(name+'.stderr')).open('wb')as stderr:
        code=subprocess.run(list(map(str,cmd)),stdout=stdout,stderr=stderr,timeout=timeout,env=dict(os.environ,LC_ALL='C')).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code,'seconds':time.monotonic()-start,'stdoutSha256':sha(out/(name+'.stdout')),'stderrSha256':sha(out/(name+'.stderr'))};save()
    if code and not (allow_mismatch and code==4):raise RuntimeError(name+' failed; see '+str(out))
    return(out/(name+'.stdout')).read_bytes()
try:
    if platform.system()!='Linux' or platform.machine()!='x86_64':raise RuntimeError('reference requires Linux x86-64, no substituted expectations')
    native=ROOT/'build/native/source';upstream=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
    manifest=ROOT/'config/source-inventory.json'
    for row in json.loads(manifest.read_text())['files']:
        if sha(upstream/row['path'])!=row['sha256']:raise RuntimeError('pinned source changed: '+row['path'])
    for name in ['hardware.c','interpreter.c','describe.c','corpus.h','output.h','features.py','fp-cases.h','make-fp-cases.py']:shutil.copyfile(ROOT/'tests/CpuConformance'/name,a/name)
    # Preserve the archive and the exact header/config input to its consumer.
    shutil.copyfile(native/'o/blink/blink.a',a/'blink.a')
    shutil.copyfile(native/'config.h',a/'config.h')
    (a/'blink').mkdir()
    for p in list((native/'blink').glob('*.h'))+list((native/'blink').glob('*.inc')):shutil.copyfile(p,a/'blink'/p.name)
    stage=ROOT/'src/HostCpu/stage-cpuid.py'
    run(['python3',stage,'--output',a/'cpuid.c','--receipt',out/'cpuid-stage.json'],'stage-cpuid')
    r['cpuid_stage_script_sha256']=sha(stage)
    if args.staged_fp:
        run(['python3',ROOT/'src/UpstreamScalarFp/stage.py','--output',a/'scalar-fp','--receipt',out/'scalar-fp-stage.json'],'stage-scalar-fp')
        r['scalar_fp_stage']=json.loads((out/'scalar-fp-stage.json').read_text())
        r['scope']='hardware versus original pinned native and reviewed staged scalar FP interpreter'
        r['original_comparisons']=[]
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['sourceManifestSha256']=sha(manifest);r['runnerSha256']=sha(Path(__file__))
    r['head']=subprocess.check_output(['git','-C',str(REPO),'rev-parse','HEAD'],text=True).strip()
    r['platform']=platform.platform();r['compiler']=run(['cc','--version'],'compiler-version').decode()
    if Path('/proc/cpuinfo').exists():shutil.copyfile('/proc/cpuinfo',out/'hardware-cpuinfo.txt')
    run(['cc','-std=c17','-O2','-Wall','-Wextra','-Werror',a/'hardware.c','-o',a/'hardware'],'hardware-build')
    run(['cc','-std=c17','-O2','-Wall','-Wextra','-Werror',a/'describe.c','-o',a/'describe'],'describe-build')
    run(['cc','-std=c17','-D_GNU_SOURCE','-D_DEFAULT_SOURCE','-DNOLINEAR','-I',a,a/'interpreter.c',a/'cpuid.c',a/'blink.a','-lz','-lrt','-lm','-pthread','-Wl,-Map='+str(out/'interpreter-link.map'),'-o',a/'interpreter'],'interpreter-build')
    if args.staged_fp:
        run(['cc','-std=c17','-D_GNU_SOURCE','-D_DEFAULT_SOURCE','-DNOLINEAR','-I',a,a/'interpreter.c',a/'cpuid.c',a/'scalar-fp/cvt.c',a/'scalar-fp/ssefloat.c',a/'scalar-fp/throw.c',a/'blink.a','-lz','-lrt','-lm','-pthread','-Wl,-Map='+str(out/'staged-link.map'),'-o',a/'staged'],'staged-build')
    run(['objdump','-d',a/'hardware'],'hardware-disassembly')
    inputs=json.loads(run([a/'describe'],'corpus'))
    cpuid_rows={};hardware_cpuid={}
    for case in inputs['cases']:
        i=case['index'];ref=json.loads(run([a/'hardware',str(i)],f'hardware-{i:02}'))
        original=json.loads(run([a/'interpreter',str(i)],f'interpreter-{i:02}',allow_mismatch=True))
        actual=json.loads(run([a/'staged',str(i)],f'staged-{i:02}',allow_mismatch=True)) if args.staged_fp else original
        # Faulting instructions compare fault/IP/memory only. Full raw capture
        # remains evidence, but unspecified flags/register effects are excluded.
        fields=['name','signal','ip','memory']+([] if case['fault'] and not case['faultState'] else ['xmm','mxcsr']+([]if case['profileReference']else['ax','cx','dx']))
        if case['faultState'] and case['fault']==8:fields.append('rawCode')
        if case['profileReference']:
            cpuid_rows[case['name']]=actual;hardware_cpuid[case['name']]=ref
            if any(int(actual[key],16)>>32 for key in ['ax','bx','cx','dx']):raise RuntimeError('CPUID upper register bits not zero')
        differences=[key for key in fields if ref[key]!=actual[key]]
        mask=int(case['flagMask'],16)
        if (int(ref['flags'],16)^int(actual['flags'],16))&mask:differences.append('definedFlags')
        if case['faultState'] and actual['halt']!=case['expectedHalt']:differences.append('expectedHalt')
        if args.staged_fp:
            old_differences=[key for key in fields if ref[key]!=original[key]]
            if (int(ref['flags'],16)^int(original['flags'],16))&mask:old_differences.append('definedFlags')
            r['original_comparisons'].append({'case':case['name'],'matched':not old_differences,'differences':old_differences})
        record={'case':case['name'],'reference':'native-profile-CPUID'if case['profileReference']else'hardware','fields':fields,'definedFlagsMask':case['flagMask'],'matched':not differences,'differences':differences}
        r['comparisons'].append(record);save()
    r['feature_inventory']=inventory(cpuid_rows,hardware_cpuid)
    r['completed']=True
    r['known_failing_rows']=[row for row in r['comparisons']if not row['matched']]
    r['passed']=not r['known_failing_rows']
    r['binaries']={name:sha(a/name)for name in ['hardware','describe','interpreter']+(['staged']if args.staged_fp else[])}
    if r['inputs']!={name:sha(a/name)for name in r['inputs']}:raise RuntimeError('snapshot changed')
    save();print(str(len(r['comparisons']))+' native cases completed; conformance='+str(r['passed'])+'; receipt '+str(out/'receipt.json'))
    if not r['passed'] and not args.observe_differences:raise SystemExit(1)
finally:save()
