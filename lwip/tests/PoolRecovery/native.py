#!/usr/bin/env python3
"""Independent C pool exhaustion/recovery semantics using the identical profile and native port."""
import argparse,importlib.util,json,pathlib,subprocess
ROOT=pathlib.Path(__file__).resolve().parents[2]
def run(profile,output):
    output=pathlib.Path(output);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('oracle',ROOT/'tests/NativeOracle/run.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    base=module.run(profile,output/'base')
    objects=[output/'base'/f'{i}.o' for i,p in enumerate(base['sources']) if not p.endswith('/oracle.c')]
    obj=output/'semantics.o';command=base['compile_flags']+['-c',str(ROOT/'tests/PoolRecovery/native.c'),'-o',str(obj)]
    subprocess.run(command,check=True);exe=output/'semantics';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*[str(p) for p in objects],str(obj),'-o',str(exe)],check=True)
    negative=subprocess.run([str(exe),'--negative'],capture_output=True,text=True,timeout=10)
    assert negative.returncode==92 and 'FAIL' in negative.stderr
    result=subprocess.run([str(exe)],capture_output=True,text=True,timeout=30)
    (output/'stdout.jsonl').write_text(result.stdout);(output/'stderr.txt').write_text(result.stderr)
    (output/'negative.stderr').write_text(negative.stderr)
    if result.returncode:raise RuntimeError(f'native pool exit {result.returncode}: {result.stderr}')
    report={'profile':profile,'compile':command,'negative_exit':negative.returncode,'cases':[json.loads(line) for line in result.stdout.splitlines()]}
    (output/'report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report));return report
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--profile',default='threaded');p.add_argument('--output',required=True);a=p.parse_args();run(a.profile,a.output)
