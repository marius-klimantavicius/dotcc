#!/usr/bin/env python3
"""Translate real upstream assertions with the portable C harness, then compare native."""
import argparse,hashlib,json,os,pathlib,platform,re,shutil,subprocess,sys,tempfile,time
from fixture_paths import fixture_directory, check_resolution
from test_diagnostics import failure_details, print_details
ROOT=pathlib.Path(__file__).resolve().parents[1];REPO=ROOT.parent
COMPILER=REPO/'DotCC/bin/Release/net10.0/dotcc.dll'
POST=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def run(command,log,timeout=900,env=None,cwd=REPO):
    log.parent.mkdir(parents=True,exist_ok=True);started=time.monotonic()
    with log.open('w') as output:
        try:code=subprocess.run(list(map(str,command)),cwd=cwd,stdout=output,stderr=subprocess.STDOUT,timeout=timeout,env=env).returncode
        except subprocess.TimeoutExpired:code=124
    print(f'{log.name}: exit {code}',flush=True)
    details=failure_details(code,log.read_text(errors='replace')) if code else []
    print_details(details)
    return {'command':list(map(str,command)),'exit':code,'seconds':round(time.monotonic()-started,3),'log':str(log.relative_to(ROOT)),'sha256':sha(log),'failure_details':details}
def files(directory):return {str(f.relative_to(directory)):sha(f) for f in sorted(directory.iterdir()) if f.is_file() and f.suffix in ('.cs','.csproj','.txt')}
def normalize(text):
    # SPUT's elapsed time is not an interpreter observation; keep every assertion/result.
    text=re.sub(r'finished after [0-9.]+ second\(s\)', 'finished after <time> second(s)',text).replace('\r\n','\n')
    # Native runner captures streams separately; retain fixture callback order
    # while comparing its stderr records independently from SPUT stdout.
    lines=text.splitlines();opens=[line for line in lines if line.startswith('fixture-open ')]
    return '\n'.join([line for line in lines if not line.startswith('fixture-open ')]+opens)
def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('action',choices=['translate','run','all'],default='all',nargs='?');p.add_argument('--profile',choices=['release','debug'],default='release');p.add_argument('--form',choices=['all','raw','optimized'],default='all');p.add_argument('--mode',choices=['all','jit','aot'],default='all');p.add_argument('--case',action='append',dest='selected');p.add_argument('--fixtures',help='Fixture directory (default: PINTA_FIXTURES or pinned source fixtures)');args=p.parse_args()
    fixtures=fixture_directory(args.fixtures)
    source=ROOT/'generated/native-input';dest=ROOT/'generated/upstream-tests'/args.profile;artifacts=ROOT/'artifacts/upstream-tests'/args.profile;artifacts.mkdir(parents=True,exist_ok=True)
    state=artifacts/'translation.json';receipt={'platform':platform.platform(),'profile':args.profile,'fixture_root':str(fixtures),'runs':[]}
    configs=[f for f in (ROOT/'config').rglob('*') if f.is_file()];authored=[ROOT/'src/native-adapter.c',ROOT/'tests/native-main.c',ROOT/'tests/native-cases.inc',ROOT/'scripts/fixture_paths.py',ROOT/'scripts/test_diagnostics.py',pathlib.Path(__file__)]
    def identity():return {str(f.relative_to(REPO)):sha(f) for f in sorted(configs+authored+[COMPILER,COMPILER.parent/'DotCC.Lib.dll',POST])}
    if args.action in ('translate','all'):
        state.unlink(missing_ok=True)
        subprocess.run([sys.executable,str(ROOT/'scripts/stage.py')],check=True)
        core=[source/name for name in (ROOT/'config/core-sources.txt').read_text().splitlines()]
        if len(core)!=29:raise RuntimeError('Expected complete 29-core manifest')
        tests=sorted((source/'Marius.Pinta/tests').glob('*_tests*.c'))+[source/'Marius.Pinta/tests/tests_common.c']
        inputs=core+tests+[ROOT/'src/native-adapter.c',ROOT/'tests/native-main.c']
        wrappers=dest/'inputs';wrappers.mkdir(parents=True,exist_ok=True);units=[]
        for index,original in enumerate(inputs):
            wrapper=wrappers/f'{index:02d}-{original.name}';wrapper.write_text('#include "native-preinclude.h"\n#include '+json.dumps(original.relative_to(ROOT).as_posix())+'\n');units.append(wrapper)
        receipt['inputs']=identity();receipt['source_units']={str(f.relative_to(ROOT)):sha(f) for f in inputs}
        with tempfile.TemporaryDirectory(prefix='emit-',dir=dest) as temp:
            emitted=pathlib.Path(temp)/'app'
            command=['dotnet',COMPILER,'-std=c17','-DPINTA_HAVE_LONG_LITERAL=1','-DPINTA_DEBUG='+str(int(args.profile=='debug')),'-DPINTA_TEST_FIXTURE_ROOT='+json.dumps(str(fixtures)),'-I',ROOT/'config','-I',ROOT/'tests','-I',ROOT,'-I',source/'Marius.Pinta/inc','-I',source/'Marius.Pinta/tests',*units,'--emit=csproj','--runtime=c','--split=size','--split-size=102400','-o',emitted]
            receipt['emission']=run(command,artifacts/'emit.log')
            if receipt['emission']['exit']:
                (artifacts/'attempt.json').write_text(json.dumps(receipt,indent=2)+'\n');return receipt['emission']['exit']
            for form in ['raw','optimized']:
                output=dest/form
                if output.exists():shutil.rmtree(output)
                shutil.copytree(emitted,output)
        projects=list((dest/'optimized').glob('*.csproj'))
        if len(projects)!=1:raise RuntimeError('Expected exactly one generated executable project')
        receipt['project']=projects[0].name
        receipt['restore']=run(['dotnet','restore',projects[0],'--nologo'],artifacts/'restore.log')
        if receipt['restore']['exit']:(artifacts/'attempt.json').write_text(json.dumps(receipt,indent=2)+'\n');return receipt['restore']['exit']
        receipt['postprocess']=run(['dotnet',POST,projects[0],'--in-place'],artifacts/'postprocess.log')
        if receipt['postprocess']['exit']:(artifacts/'attempt.json').write_text(json.dumps(receipt,indent=2)+'\n');return receipt['postprocess']['exit']
        if identity()!=receipt['inputs'] or receipt['source_units']!={str(f.relative_to(ROOT)):sha(f) for f in inputs}:raise RuntimeError('Inputs changed during translation')
        receipt['outputs']={form:files(dest/form) for form in ['raw','optimized']};state.write_text(json.dumps(receipt,indent=2)+'\n')
    if args.action=='translate':return 0
    saved=json.loads(state.read_text())
    if saved['inputs']!=identity():raise RuntimeError('Compiler/config/harness changed; translate upstream tests again')
    if platform.machine().lower() not in ('x86_64','amd64'):raise RuntimeError('Only x64 is configured')
    rid='win-x64' if sys.platform=='win32' else 'linux-x64' if sys.platform.startswith('linux') else None
    if not rid:raise RuntimeError('Only Linux/Windows execution is configured')
    cases=re.findall(r'PINTA_CASE\((\w+)\)',(ROOT/'tests/native-cases.inc').read_text())
    if args.selected:
        if any(c not in cases for c in args.selected):raise RuntimeError('Unknown upstream case')
        cases=args.selected
    forms=['raw','optimized'] if args.form=='all' else [args.form];modes=['jit','aot'] if args.mode=='all' else [args.mode]
    reference=ROOT/'artifacts'/('native-corrected-'+args.profile)
    native_receipt=reference/'receipt.json'
    native_cases={c['group']:c for c in json.loads(native_receipt.read_text())['cases']} if native_receipt.is_file() else {}
    env=dict(os.environ,PINTA_FIXTURES=str(fixtures))
    failed=False
    try:
        for form in forms:
            if saved['outputs'][form]!=files(dest/form):raise RuntimeError('Generated output changed')
            project=dest/form/saved['project'];assembly=project.stem
            for mode in modes:
                output=ROOT/'build/upstream-tests'/args.profile/rid/form/mode
                cmd=['dotnet','build' if mode=='jit' else 'publish',project,'-c','Release','--nologo','--artifacts-path',output/'intermediate','-o',output/'app']
                if mode=='aot':cmd+=['-r',rid,'-p:PublishAot=true']
                build=run(cmd,artifacts/f'{form}-{mode}-build.log');item={'form':form,'mode':mode,'build':build,'cases':[]};receipt['runs'].append(item)
                if build['exit']:failed=True;continue
                exe=output/'app'/(assembly+('.dll' if mode=='jit' else '.exe' if sys.platform=='win32' else ''))
                invoke=['dotnet',exe] if mode=='jit' else [exe]
                item['binary_sha256']=sha(exe)
                item['fixture_resolution']=check_resolution(invoke,fixtures,artifacts,f'{form}-{mode}')
                failed|=any(not result['pass'] for result in item['fixture_resolution'])
                for case in cases:
                    log=artifacts/f'{form}-{mode}-{case}.log';result=run([*invoke,'--case',case],log,60,env,cwd=output/'app')
                    native=reference/f'case-{case}.log'
                    result['native_available']=native.is_file()
                    result['native_stdout_equal']=native.is_file() and normalize(native.read_text(errors='replace'))==normalize(log.read_text(errors='replace'))
                    # A crash on both sides is recorded as failure, never parity success.
                    result['pass']=result['exit']==0 and result['native_stdout_equal'];failed|=not result['pass'];item['cases'].append({'case':case,**result})
                    if not result['native_stdout_equal']:
                        print(f'  Native output mismatch: {native}',flush=True)
                        native_case=native_cases.get('case-'+case)
                        if native_case and native.is_file():
                            print_details(failure_details(native_case['exit'],native.read_text(errors='replace')),prefix='  Native: ')
    finally:(artifacts/'runs.json').write_text(json.dumps(receipt,indent=2)+'\n')
    return int(failed)
if __name__=='__main__':
    try:sys.exit(main())
    except (RuntimeError,subprocess.CalledProcessError) as error:print('pinta upstream tests: '+str(error),file=sys.stderr);sys.exit(1)
