#!/usr/bin/env python3
"""Compile all selected C units and run each upstream group in isolation."""
import argparse, hashlib, json, os, pathlib, platform, subprocess, sys, time
from fixture_paths import fixture_directory, check_resolution
from test_diagnostics import failure_details, print_details
ROOT=pathlib.Path(__file__).resolve().parents[1]
GROUPS='gc array memory code code_v2 buffer format integer decimal weak encoding property object function pattern json'.split()
def main():
    p=argparse.ArgumentParser();p.add_argument('--profile',choices=['release','debug'],default='release');p.add_argument('--stage',choices=['pristine','corrected'],default='pristine');p.add_argument('--build-only',action='store_true');p.add_argument('--cases',action='store_true');p.add_argument('--fixtures',help='Fixture directory (default: PINTA_FIXTURES or pinned source fixtures)');args=p.parse_args()
    subprocess.run([sys.executable,str(ROOT/'scripts/fetch.py'),'--offline'],check=True)
    fixtures=fixture_directory(args.fixtures)
    source=ROOT/'ref/upstream'
    if args.stage=='corrected':
        subprocess.run([sys.executable,str(ROOT/'scripts/stage.py')],check=True);source=ROOT/'generated/native-input'
    out=ROOT/'build'/('native-'+args.stage+'-'+args.profile);out.mkdir(parents=True,exist_ok=True)
    artifacts=ROOT/'artifacts'/out.name;artifacts.mkdir(parents=True,exist_ok=True)
    cc=os.environ.get('CC','cc');flags=['-std=c11','-fshort-wchar','-funsigned-char','-fno-strict-aliasing','-g','-O0' if args.profile=='debug' else '-O2','-DPINTA_DEBUG='+str(int(args.profile=='debug')),'-DPINTA_HAVE_LONG_LITERAL=1','-include',str(ROOT/'config/native-preinclude.h'),'-I'+str(source/'Marius.Pinta/inc'),'-I'+str(source/'Marius.Pinta/tests')]
    flags+=['-DPINTA_TEST_FIXTURE_ROOT='+json.dumps(str(fixtures))]
    sources=[source/n for n in (ROOT/'config/core-sources.txt').read_text().splitlines()]
    tests=sorted((source/'Marius.Pinta/tests').glob('*_tests*.c'))+[source/'Marius.Pinta/tests/tests_common.c']
    command=[cc,*flags,*map(str,sources+tests),str(ROOT/'src/native-adapter.c'),str(ROOT/'tests/native-main.c'),'-o',str(out/'tests')]
    receipt={'platform':platform.platform(),'machine':platform.machine(),'profile':args.profile,'stage':args.stage,'command':command,'compiler':subprocess.run([cc,'--version'],capture_output=True,text=True).stdout,'source_lock_sha256':hashlib.sha256((ROOT/'config/source-lock.json').read_bytes()).hexdigest(),'cases':[]}
    receipt['campaign_inputs']={str(f.relative_to(ROOT)):hashlib.sha256(f.read_bytes()).hexdigest() for folder in ['config','src','tests'] for f in (ROOT/folder).rglob('*') if f.is_file() and f.suffix in ['.h','.c','.inc','.txt','.json','.patch']}
    result=subprocess.run(command,capture_output=True,text=True);(artifacts/'build.log').write_text(result.stdout+result.stderr)
    receipt['build_exit']=result.returncode
    if result.returncode:
        (artifacts/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n');print(result.stderr[-12000:]);return result.returncode
    abi=[cc,*flags,str(ROOT/'tests/native-abi.c'),'-o',str(out/'abi')]
    subprocess.run(abi,check=True);result=subprocess.run([str(out/'abi')],capture_output=True,text=True);(artifacts/'abi.txt').write_text(result.stdout+result.stderr);receipt['abi_exit']=result.returncode
    if not args.build_only:
        receipt['fixture_root']=str(fixtures)
        receipt['fixture_resolution']=check_resolution([out/'tests'],fixtures,artifacts,'native')
        env=dict(os.environ,PINTA_FIXTURES=str(fixtures))
        runs=[(group,[group]) for group in GROUPS]
        if args.cases:
            import re
            runs += [('case-'+name,['--case',name]) for name in re.findall(r'PINTA_CASE\((\w+)\)',(ROOT/'tests/native-cases.inc').read_text())]
        for group,arguments in runs:
            started=time.monotonic()
            try:r=subprocess.run([str(out/'tests'),*arguments],cwd=out,env=env,capture_output=True,text=True,timeout=60);status=r.returncode;log=r.stdout+r.stderr
            except subprocess.TimeoutExpired as e:status=124;log=(e.stdout or b'').decode(errors='replace')+(e.stderr or b'').decode(errors='replace')
            details=failure_details(status,log)
            (artifacts/(group+'.log')).write_text(log);receipt['cases'].append({'group':group,'exit':status,'seconds':round(time.monotonic()-started,3),'failure_details':details})
            print(group,status,flush=True)
            print_details(details)
    receipt['binary_sha256']=hashlib.sha256((out/'tests').read_bytes()).hexdigest();(artifacts/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
    return int(receipt['abi_exit']!=0 or any(c['exit'] for c in receipt['cases']) or any(not c['pass'] for c in receipt.get('fixture_resolution',[])))
if __name__=='__main__':sys.exit(main())
