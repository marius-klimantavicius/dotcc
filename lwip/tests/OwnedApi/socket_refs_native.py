#!/usr/bin/env python3
"""Native deterministic descriptor reuse proof for original socket lifetime pins."""
import argparse,importlib.util,json,pathlib,subprocess
ROOT=pathlib.Path(__file__).resolve().parents[2]
def run(profile,output):
    output=pathlib.Path(output);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('oracle',ROOT/'tests/NativeOracle/run.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    base=module.run(profile,output/'base');source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
    objects=[output/'base'/f'{i}.o' for i,p in enumerate(base['sources']) if not p.endswith(('/oracle.c','/sockets.c'))];commands=[]
    for name in ('socket_refs','socket_refs_native'):
        obj=output/f'{name}.o';command=base['compile_flags']+['-I'+str(source/'src'),'-c',str(ROOT/('src/port/socket_refs.c' if name=='socket_refs' else 'tests/OwnedApi/socket_refs_native.c')),'-o',str(obj)];subprocess.run(command,check=True);commands.append(command);objects.append(obj)
    exe=output/'control';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*[str(p) for p in objects],'-o',str(exe)],check=True);cases=[]
    for option,expected in [('--negative',92),('--original',92),('',0)]:
        result=subprocess.run([str(exe),*([option] if option else [])],capture_output=True,text=True,timeout=30);cases.append({'option':option,'expected_exit':expected,'exit':result.returncode,'stdout':result.stdout,'stderr':result.stderr})
        report={'profile':profile,'compile':commands,'cases':cases};(output/'report.json').write_text(json.dumps(report,indent=2)+'\n')
        if result.returncode!=expected:raise RuntimeError(result.stderr)
    print(json.dumps(report));return report
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--profile',default='threaded');p.add_argument('--output',required=True);a=p.parse_args();run(a.profile,a.output)
