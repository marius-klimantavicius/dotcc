#!/usr/bin/env python3
"""Native original-failure/adapted-success IPv4 forwarding controls."""
import argparse,importlib.util,json,os,pathlib,subprocess
ROOT=pathlib.Path(__file__).resolve().parents[2]
def run(profile,output):
    output=pathlib.Path(output);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('oracle',ROOT/'tests/NativeOracle/run.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    cases=[]
    for adapted in (False,True):
        build=output/('adapted' if adapted else 'original');build.mkdir(parents=True,exist_ok=True)
        base=module.run(profile,build/'base',forwarding_fix=adapted)
        objects=[build/'base'/f'{i}.o' for i,p in enumerate(base['sources']) if not p.endswith('/oracle.c')]
        obj=build/'routing.o';command=base['compile_flags']+['-c',str(ROOT/'tests/Routing/native.c'),'-o',str(obj)]
        subprocess.run(command,check=True);exe=build/'routing';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*[str(p) for p in objects],str(obj),'-o',str(exe)],check=True)
        negative=subprocess.run([str(exe),'--negative'],capture_output=True,text=True,timeout=10)
        if negative.returncode!=92 or 'FAIL' not in negative.stderr:raise RuntimeError('Missing deliberate native assertion failure')
        result=subprocess.run([str(exe)],capture_output=True,text=True,timeout=30,env={**os.environ,"LWIP_ROUTING_PCAP":str(build/"routing.pcap")})
        (build/'stdout.jsonl').write_text(result.stdout);(build/'stderr.txt').write_text(result.stderr);(build/'negative.stderr').write_text(negative.stderr)
        if adapted:
            if result.returncode:raise RuntimeError(f'adapted native routing exit {result.returncode}: {result.stderr}')
        elif result.returncode!=92 or 'checksum(frames[0]+14,20)==0' not in result.stderr:
            raise RuntimeError('Unmodified upstream must reproduce the known forwarding checksum failure: '+result.stderr)
        cases.append({'adapted':adapted,'pcap':str(build/'routing.pcap'),'compile':command,'status':'pass' if adapted else 'expected-upstream-failure','exit':result.returncode,'negative_exit':negative.returncode,'adaptations':base['adaptations'],'assertions':[json.loads(line) for line in result.stdout.splitlines()]})
    report={'profile':profile,'cases':cases};(output/'report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report));return report
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--profile',default='threaded');p.add_argument('--output',required=True);a=p.parse_args();run(a.profile,a.output)
