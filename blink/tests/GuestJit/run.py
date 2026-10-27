#!/usr/bin/env python3
"""Bounded serial JIT qualification, with private frozen build inputs.

The producer and generated C# remain untouched. Native guest fixtures are normal
Linux ELF programs built with the system assembler/linker and run natively first.
Use --prepare-only on Linux then copy fixtures and a source snapshot to Windows.
"""
import argparse, hashlib, json, os, platform, resource, shutil, signal, subprocess, time, sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
BLINK = REPO / 'blink'
sys.path.insert(0, str(REPO/'Scripts'))
from production_support import locked_command
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def producer_provenance():
    pointer = BLINK/'artifacts/campaign/current-threaded.json'
    try:
        producer = json.loads(pointer.read_text())
    except FileNotFoundError:
        return None, {'status':'unavailable', 'pointer':str(pointer),
            'qualification':'source-snapshot-only',
            'reason':'Producer pointer absent; frozen source/generated hashes qualify this snapshot only.'}
    return producer, {'status':'pointer-recorded', 'pointer':str(pointer),
        'sha256':sha(pointer), 'qualification':'source-snapshot',
        'reason':'Pointer retained as provenance; this runner does not regenerate or authenticate a fresh producer.'}

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--prepare-only', action='store_true')
    parser.add_argument('--form', choices=['processed','raw'], default='processed')
    parser.add_argument('--aot', action='store_true')
    parser.add_argument('--bench', action='store_true')
    parser.add_argument('--emitter-contract', action='store_true', help='Also build/run focused native descriptor/exit checks serially')
    parser.add_argument('--skip-prepare', action='store_true')
    parser.add_argument('--fixtures', type=Path)
    args = parser.parse_args()
    out = args.output.resolve(); out.mkdir(parents=True,exist_ok=True)
    producer, provenance = producer_provenance()
    receipt = {'status':'running','baseline_head':subprocess.check_output(['git','rev-parse','HEAD'],cwd=REPO,text=True).strip(),
        'platform':platform.platform(),'machine':platform.machine(),'commands':[], 'inputs':{},
        'producer':producer, 'producer_provenance':provenance}
    def save(): (out/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
    def run(label, argv, timeout=300, cwd=None, expected=0, env=None, expected_stdout=None):
        argv=locked_command(list(map(str,argv))); start=time.monotonic(); before=resource.getrusage(resource.RUSAGE_CHILDREN)
        row={'label':label,'argv':argv,'cwd':str(cwd or out),'expected':expected};receipt['commands'].append(row);save()
        with (out/(label+'.stdout')).open('wb') as stdout,(out/(label+'.stderr')).open('wb') as stderr:
            p=subprocess.Popen(argv,cwd=cwd or out,env=env,stdout=stdout,stderr=stderr,start_new_session=True)
            sent=[]
            try: code=p.wait(timeout=timeout)
            except subprocess.TimeoutExpired:
                os.killpg(p.pid,signal.SIGKILL);sent.append('SIGKILL-timeout');code=p.wait()
            try: os.killpg(p.pid,0);gone=False
            except ProcessLookupError: gone=True
            if not gone: os.killpg(p.pid,signal.SIGKILL);sent.append('SIGKILL-descendants')
        after=resource.getrusage(resource.RUSAGE_CHILDREN)
        row.update(exit_code=code, wall_seconds=time.monotonic()-start,
            cpu_seconds=after.ru_utime+after.ru_stime-before.ru_utime-before.ru_stime,
            pid=p.pid,group_gone=gone,signals=sent,stdout_sha256=sha(out/(label+'.stdout')),stderr_sha256=sha(out/(label+'.stderr')))
        if expected_stdout is not None:
            row.update(expected_stdout=expected_stdout,
                output_matches=(out/(label+'.stdout')).read_bytes() == expected_stdout.encode() and
                    (out/(label+'.stderr')).read_bytes() == b'')
        save();print(label,code,flush=True)
        if code != expected or not gone or sent or row.get('output_matches') is False:
            raise RuntimeError(label+' failed; see '+str(out))
    fixtures=args.fixtures.resolve() if args.fixtures else out/'fixtures';fixtures.mkdir(parents=True,exist_ok=True)
    try:
        if not args.skip_prepare:
            specs={
                'negative-change':[HERE/'negative-change.S'],'negative-pressure':[HERE/'negative-pressure.S'],'address-plan':[HERE/'address-plan.S'],'arithmetic':[HERE/'arithmetic.S'],'limits':[HERE/'limits.S'],'stop':[HERE/'stop.S'],
                'semantics':[BLINK/'tests/Performance/semantics.S'],
                'private-code':[HERE/'private-code.c'],'resume':[HERE/'resume.S'],'scalar':[HERE/'scalar.S'],
                'static-workers':[HERE/'static-workers.S'],'private-data':[HERE/'private-data.S'],
                'loads':[HERE/'loads.S'],'stack-memory':[HERE/'stack-memory.S'],'memory-flags':[HERE/'memory-flags.S'],
                'call-return':[HERE/'call-return.S'],
                'shared':[BLINK/'tests/GuestSharedMapping/fixture.c'],
                'regular':[BLINK/'tests/GuestRegularShared/fixture.c'],
                'identity':[BLINK/'tests/GuestSharedIdentity/fixture.c',BLINK/'tests/GuestSharedIdentity/entry.S'],
                'signals':[BLINK/'tests/GuestSignals/fixture.S'],
                'futex':[BLINK/'tests/GuestFutexBitset/fixture.S'],
                'churn':[BLINK/'tests/GuestThreadChurn/fixture.S']}
            for name,defs in [('register',[]),('stack',['-DSTACK']),('file',['-DFILE'])]:
                specs[name]=[BLINK/'tests/Performance/loop.S',*defs]
            linker=BLINK/'tests/GuestThreads/fixture.ld'
            for name, sources in specs.items():
                for source in sources:
                    if isinstance(source,Path):receipt['inputs'][str(source)]=sha(source)
                run(name+'-build',['cc','-nostdlib','-static','-O2','-ffreestanding','-fno-stack-protector','-fno-pie','-no-pie',
                    '-Wl,--build-id=none','-Wl,-z,noexecstack','-Wl,-z,max-page-size=4096','-Wl,-T,'+str(linker),*sources,'-o',fixtures/(name+'.elf')])
                run(name+'-identity',['readelf','-h','-lW',fixtures/(name+'.elf')])
                native=out/('native-'+name);native.mkdir(exist_ok=True)
                if name != 'stop':
                    run(name+'-linux',[fixtures/(name+'.elf'),*(['s'] if name=='churn' else [])],60,cwd=native,
                        expected_stdout=name+': ok\n' if name in ['negative-change','negative-pressure'] else
                            'private-data: 32768\n' if name=='private-data' else
                            'call-return: ok\n' if name=='call-return' else None)
                if name == 'private-data':
                    run(name+'-single-linux',[fixtures/(name+'.elf'),'s'],60,cwd=native,
                        expected_stdout='private-data: 32768\n')
        receipt['fixtures']={p.name:sha(p) for p in fixtures.glob('*.elf')};save()
        if not args.prepare_only:
            private=out/'source'
            # Freeze source separately from builds; all measurements use this exact tree.
            for relative in ['Directory.Build.props','blink/src/Managed.Emulation','blink/src/Managed.Emulation.Host',
                'blink/src/Managed.Emulation.ThreadedExecution','blink/src/Managed.Emulation.Worker','blink/src/Host','blink/src/GuestResources',
                'blink/tests/GuestJit','blink/tests/Performance','blink/tests/PerformanceService','blink/tests/CoreClrRuntime/Probe',
                'blink/generated/TranslatedBlink','blink/generated/TranslatedBlink.Raw']:
                source=REPO/relative; target=private/relative;target.parent.mkdir(parents=True,exist_ok=True)
                if source.is_dir():shutil.copytree(source,target,ignore=shutil.ignore_patterns('bin','obj','__pycache__'),dirs_exist_ok=True)
                else:shutil.copy2(source,target)
            receipt['source_hashes']={str(p.relative_to(private)):sha(p) for p in private.rglob('*') if p.is_file()};save()
            project=private/'blink/tests/GuestJit/GuestJit.csproj'
            product=private/'blink/generated'/('TranslatedBlink.Raw' if args.form=='raw' else 'TranslatedBlink')/'TranslatedBlink.csproj'
            common=['-c','Release','--disable-build-servers','-m:2','-p:UseSharedCompilation=false','-p:BuildInParallel=false',
                '-p:IlcSingleThreaded=true','-p:BlinkProject='+str(product)]
            run('build',['dotnet','build',project,*common],1200,cwd=private)
            deployed=out/'coreclr';shutil.copytree(project.parent/'bin/Release/net10.0',deployed,dirs_exist_ok=True)
            runtimes=[('coreclr',['dotnet',deployed/'GuestJit.dll'])]
            if args.aot:
                aot=out/'nativeaot'
                run('publish',['dotnet','publish',project,*common,'-r','linux-x64','-p:PublishAot=true','-o',aot],2400,cwd=private)
                runtimes.append(('nativeaot',[aot/'GuestJit']))
            for runtime,command in runtimes:
                run(runtime+'-qualify',[*command,'qualify',fixtures,out/(runtime+'-qualify.json')],600)
                if args.bench:run(runtime+'-bench',[*command,'bench',fixtures,out/(runtime+'-bench.json'),'5'],600)
            if args.emitter_contract:
                contract=project.parent/'ContractTests/ContractTests.csproj'
                run('contract-build',['dotnet','build',contract,*common],1200,cwd=private)
                run('coreclr-emitter-contract',['dotnet',contract.parent/'bin/Release/net10.0/ContractTests.dll'],60)
                if args.aot:
                    contract_aot=out/'contract-nativeaot'
                    run('contract-publish',['dotnet','publish',contract,*common,'-r','linux-x64','-p:PublishAot=true','-o',contract_aot],2400,cwd=private)
                    run('nativeaot-emitter-contract',[contract_aot/'ContractTests'],60)
        receipt['status']='passed';save()
    except BaseException as error:
        receipt['status']='failed';receipt['error']=repr(error);save();raise
    print('receipt='+str(out/'receipt.json'),flush=True)

if __name__=='__main__':main()
