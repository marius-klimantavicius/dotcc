#!/usr/bin/env python3
"""Local ordinary-netif retirement regression with native ASan callback reads."""
import argparse,importlib.util,json,os,pathlib,subprocess
ROOT=pathlib.Path(__file__).resolve().parents[2]
def run(profile,output):
    output=pathlib.Path(output);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('oracle',ROOT/'tests/NativeOracle/run.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    base=module.run(profile,output/'base');sources=base['sources'];objects=[output/'base'/f'{i}.o' for i,p in enumerate(sources) if not p.endswith(('/oracle.c','/core/netif.c'))];commands=[]
    for name,path in [('netif',next(p for p in sources if p.endswith('/core/netif.c'))),('control',str(ROOT/'tests/OwnedApi/interface_native.c'))]:
        obj=output/f'{name}.o';command=base['compile_flags']+['-fsanitize=address','-fno-omit-frame-pointer','-c',path,'-o',str(obj)];subprocess.run(command,check=True);commands.append(command);objects.append(obj)
    exe=output/'control';subprocess.run(['cc','-pthread','-fsanitize=address','-Wl,--gc-sections',*[str(p) for p in objects],'-o',str(exe)],check=True);cases=[]
    for option in ['--negative','--original','']:
        result=subprocess.run([str(exe),*([option] if option else [])],capture_output=True,text=True,timeout=30,env={**os.environ,'ASAN_OPTIONS':'detect_leaks=0:halt_on_error=1'})
        cases.append({'option':option,'exit':result.returncode,'stdout':result.stdout,'stderr':result.stderr});report={'profile':profile,'compile':commands,'cases':cases};(output/'report.json').write_text(json.dumps(report,indent=2)+'\n')
        if option=='--negative':assert result.returncode==92 and 'FAIL' in result.stderr
        elif option=='--original':assert result.returncode!=0 and ('heap-use-after-free' in result.stderr if profile!='nosys' else 'lwip_stats.mem.used==baseline' in result.stderr)
        elif result.returncode:raise RuntimeError(result.stderr)
    print(json.dumps({'profile':profile,'cases':[(x['option'],x['exit']) for x in cases],'report':str(output/'report.json')}));return report
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--profile',default='threaded-locking');p.add_argument('--output',required=True);a=p.parse_args();run(a.profile,a.output)
