#!/usr/bin/env python3
"""Linux and isolated native Blink oracle for the authored futex algorithms."""
import argparse, hashlib, importlib.util, json, os
from pathlib import Path
import shutil, subprocess, sys, tarfile, tempfile
sys.dont_write_bytecode=True
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[1]
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--isolated-phases',action='store_true',help='Diagnostic only: run mask/deadline and store-without-wake phases separately; this does not qualify the combined fixture')
    parser.add_argument('--native-blink',type=Path,help='Reuse an existing native Blink binary; its build/source qualification is external to this receipt')
    args=parser.parse_args()
    base=ROOT/'artifacts/coreclr-coordination';base.mkdir(parents=True,exist_ok=True)
    attempt=Path(tempfile.mkdtemp(prefix='futex-bitset-',dir=base));print(attempt,flush=True)
    receipt=dict(passed=False,scope='Linux + isolated native Blink actual syscall snippets and focused queue; no managed Blink qualification',isolated_phases=args.isolated_phases,combined_fixture_passed=False,native_binary_origin='external' if args.native_blink else 'private authored adaptation',inputs={},commands=[])
    env=dict(os.environ,LC_ALL='C',CC='gcc',AR='ar')
    for k in ('CFLAGS','CPPFLAGS','LDFLAGS','MAKEFLAGS','MODE','m'):env.pop(k,None)
    def save():(attempt/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
    def pin(p):
        p=Path(p).resolve();d=sha(p)
        if receipt['inputs'].setdefault(str(p),d)!=d:raise RuntimeError('Input changed: '+str(p))
        return p
    def run(name,args,cwd=attempt,timeout=60):
        row=dict(name=name,command=list(map(str,args)),cwd=str(cwd));receipt['commands'].append(row);save()
        with(attempt/(name+'.log')).open('wb')as log:r=subprocess.run(row['command'],cwd=cwd,env=env,stdout=log,stderr=subprocess.STDOUT,timeout=timeout)
        row['exit']=r.returncode;save()
        if r.returncode:raise RuntimeError(name+': '+str(attempt/(name+'.log')))
        print(name+': passed',flush=True)
    try:
        pin(__file__)
        manifest=json.loads(pin(ROOT/'config/source-manifest.json').read_text());up=manifest['upstream']
        archive=pin(ROOT/'ref'/up['archive'])
        if sha(archive)!=up['sha256']:raise RuntimeError('Archive differs')
        fixtures={'bitset':HERE/'fixture.S','wait':HERE.parent/'GuestMemoryFutex/futex.S','wait-profile':HERE.parent/'GuestMemoryFutex/futex.S','threads':HERE.parent/'GuestThreads/fixture.S'}
        if args.isolated_phases:
            del fixtures['bitset']
            fixtures={'bitset-masks':HERE/'fixture.S','bitset-change':HERE/'fixture.S',**fixtures}
        linker=pin(HERE.parent/'GuestThreads/fixture.ld')
        for name,p in fixtures.items():
            definitions={'wait-profile':['-DPROFILE'],'bitset-masks':['-DMASKS_ONLY'],'bitset-change':['-DCHANGED_ONLY']}.get(name,[])
            run(name+'-assemble',['cc','-c','-nostdlib',*definitions,pin(p),'-o',attempt/(name+'.o')])
            run(name+'-link',['ld','-static','-z','noexecstack','-z','max-page-size=4096','-T',linker,attempt/(name+'.o'),'-o',attempt/name])
            pin(attempt/name)
            if name!='wait-profile':run(name+'-linux',[attempt/name])
        src=attempt/'native';src.mkdir()
        with tarfile.open(archive)as stream:stream.extractall(src,filter='data')
        src/=up['directory']
        path=pin(ROOT/'src/UpstreamGuestThreads/stage.py')
        spec=importlib.util.spec_from_file_location('futex_stage',path);stage=importlib.util.module_from_spec(spec);spec.loader.exec_module(stage)
        for name in stage.AUTHORED:pin(path.parent/name)
        syscall=src/'blink/syscall.c';memory=src/'blink/memorymalloc.c'
        if sha(syscall)!=stage.ORIGINAL_PIN:raise RuntimeError('Original syscall identity differs')
        a,b=stage.adapt_memory_and_futex(syscall.read_text(),memory.read_text());syscall.write_text(a);memory.write_text(b)
        pin(syscall);pin(memory)
        if args.native_blink:
            blink=pin(args.native_blink)
        else:
            configure_args=manifest['nativeProfile']['configureArguments'].copy();configure_args[configure_args.index('--disable-threads')]='--enable-threads'
            run('configure',[src/'configure',*configure_args],cwd=src,timeout=180)
            run('build-corrected',['make','-j2',manifest['nativeProfile']['makeTarget']],cwd=src,timeout=900)
            blink=pin(src/'o/blink/blink')
        failures=[]
        for name in (n for n in fixtures if n!='wait'):
            for i in range(5 if name!='threads'else 1):
                try:run(name+'-corrected-'+str(i),[blink,'-jm',attempt/name])
                except RuntimeError as error:failures.append(str(error))
        receipt['functional_failures']=failures
        include=attempt/'include';include.mkdir();(include/'config.h').write_text('#define DISABLE_JIT 1\n#define NOLINEAR 1\n')
        queue=pin(HERE.parent/'GuestMemoryFutex/queue.c')
        run('queue-build',['cc','-std=c17','-I',include,'-I',src,'-I',path.parent,queue,src/'blink/dll.c',src/'blink/pte32.c','-pthread','-o',attempt/'queue'])
        run('queue',[attempt/'queue'])
        if any(sha(p)!=d for p,d in receipt['inputs'].items()):raise RuntimeError('Frozen input changed')
        if failures:raise RuntimeError('; '.join(failures))
        receipt['combined_fixture_passed']=not args.isolated_phases
        receipt['passed']=True
    except BaseException as e:receipt['error']=f'{type(e).__name__}: {e}'
    finally:receipt['completed']=True;save()
    print('receipt='+str(attempt/'receipt.json'),flush=True)
    return 0 if receipt['passed']else 1
if __name__=='__main__':raise SystemExit(main())
