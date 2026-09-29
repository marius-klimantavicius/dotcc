#!/usr/bin/env python3
"""Local C compatibility controls for a scoped IPv6 hop-limit setter override."""
import argparse,importlib.util,json,pathlib,re,subprocess
ROOT=pathlib.Path(__file__).resolve().parents[2]
HERE=pathlib.Path(__file__).resolve().parent

def check_contract(source):
    spec=importlib.util.spec_from_file_location('hop',ROOT/'scripts/ip6-hop.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    return module.check_contract(ROOT,source)

def run(profile,output):
    output=pathlib.Path(output);output.mkdir(parents=True,exist_ok=True)
    source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory'];contract=check_contract(source)
    spec=importlib.util.spec_from_file_location('oracle',ROOT/'tests/NativeOracle/run.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    base=module.run(profile,output/'base',hop_override=False);sources=base['sources'];objects=[output/'base'/f'{i}.o' for i,p in enumerate(sources) if not p.endswith('/oracle.c')]
    obj=output/'controls.o';control_compile=base['compile_flags']+['-c',str(HERE/'native.c'),'-o',str(obj)];subprocess.run(control_compile,check=True)
    ip6_index=next(i for i,p in enumerate(sources) if p==str(source/contract['unit']))
    candidate_obj=output/'ip6-override.o';candidate_compile=base['compile_flags']+['-include',str(ROOT/'src/port/ip6-hop-limit-native.h'),'-c',str(source/contract['unit']),'-o',str(candidate_obj)];subprocess.run(candidate_compile,check=True)
    report={'profile':profile,'contract':contract,'adaptations':base['adaptations'],'compile':[control_compile,candidate_compile],'cases':[]}
    for adapted in (False,True):
        label='override' if adapted else 'original';exe=output/label
        selected=[candidate_obj if adapted and p==output/'base'/f'{ip6_index}.o' else p for p in objects]
        subprocess.run(['cc','-pthread','-Wl,--gc-sections',*[str(p) for p in selected],str(obj),'-o',str(exe)],check=True)
        for option in ('--negative','','--hop=0','--hop=1','--hop=2','--hop=255'):
            result=subprocess.run([str(exe),*([option] if option else [])],capture_output=True,text=True,timeout=30)
            expected=92 if option=='--negative' or option=='--hop=0' and not adapted else 0
            case={'implementation':label,'option':option,'exit':result.returncode,'expected_exit':expected,'stdout':result.stdout,'stderr':result.stderr}
            report['cases'].append(case);(output/'report.json').write_text(json.dumps(report,indent=2)+'\n')
            if result.returncode!=expected:raise RuntimeError(f'{label} {option}: {result.returncode} != {expected}: {result.stderr}')
            if option=='--hop=0' and not adapted and 'port=2 protocol=17 hop=255' not in result.stderr:raise RuntimeError('Original wrap witness missing')
    setter=output/'setter';setter_compile=base['compile_flags']+[str(HERE/'hop-limit-setter.c'),'-o',str(setter)];subprocess.run(setter_compile,check=True)
    result=subprocess.run([str(setter)],capture_output=True,text=True,timeout=10);report['compile'].append(setter_compile);report['setter']={'exit':result.returncode,'stdout':result.stdout,'stderr':result.stderr};(output/'report.json').write_text(json.dumps(report,indent=2)+'\n')
    if result.returncode:raise RuntimeError('Setter value/evaluation parity failed')
    print(json.dumps({'profile':profile,'cases':len(report['cases']),'setter':report['setter'],'report':str(output/'report.json')}));return report

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--profile',default='threaded');p.add_argument('--output',required=True);a=p.parse_args();run(a.profile,a.output)
