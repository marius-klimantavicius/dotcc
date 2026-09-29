#!/usr/bin/env python3
"""Build the matching native profile; assertion failure is a mandatory negative control."""
import argparse, concurrent.futures, importlib.util, json, pathlib, subprocess, sys
ROOT = pathlib.Path(__file__).resolve().parents[2]
def run(profile, output=None):
    pin=json.loads((ROOT/'config/sources.json').read_text()); source=ROOT/'ref'/pin['directory']
    build=pathlib.Path(output) if output else ROOT/'build/native'/profile
    build.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('lwip_profiles',ROOT/'scripts/profiles.py'); module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    module.write_header(ROOT,source,profile,build)
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    # Whole matching core/API closure; unused link/application callbacks are outside this oracle.
    paths=[source/name for group,names in groups.items() if group in ('lwipcore_SRCS','lwipcore4_SRCS','lwipcore6_SRCS','lwipapi_SRCS','lwipppp_SRCS') for name in names]
    paths += [source/'src/netif/ethernet.c',source/'src/apps/sntp/sntp.c',ROOT/'src/Host/bridge.c',ROOT/'tests/NativeOracle/sys_arch.c',ROOT/'tests/NativeOracle/oracle.c']
    base=['cc','-std=c17','-D_POSIX_C_SOURCE=200809L','-O1','-g','-ffunction-sections','-fdata-sections','-pthread','-include','errno.h','-I'+str(build),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include')]
    def compile_unit(pair):
        i,path=pair; target=build/(str(i)+'.o');cmd=base+(['-DDOTCC_TCPIP_WORKER=1'] if path.name=='tcpip.c' else [])+['-c',str(path),'-o',str(target)]
        subprocess.run(cmd,check=True,capture_output=True,text=True);return target
    try:
        with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool: objects=list(pool.map(compile_unit,enumerate(paths)))
        executable=build/'oracle';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*[str(p) for p in objects],'-o',str(executable)],check=True,capture_output=True,text=True)
        negative=subprocess.run([str(executable),'--negative'],capture_output=True,text=True,timeout=10)
        (build/'negative.stdout').write_text(negative.stdout);(build/'negative.stderr').write_text(negative.stderr)
        if negative.returncode!=92 or 'FAIL' not in negative.stderr:raise RuntimeError('Native intentional failure was not detected')
        actual=subprocess.run([str(executable)],capture_output=True,text=True,timeout=30)
        (build/'stdout.jsonl').write_text(actual.stdout);(build/'stderr.txt').write_text(actual.stderr)
        if actual.returncode:raise RuntimeError(f'Native oracle failed {actual.returncode}: {actual.stderr}')
        result={'profile':profile,'compiler':subprocess.check_output(['cc','--version'],text=True).splitlines()[0],'negative_exit':negative.returncode,'workload':'single-stack loopback, not frame/two-owner qualification','compile_flags':base,'sources':[str(p) for p in paths],'cases':[json.loads(line) for line in actual.stdout.splitlines()]}
        (build/'report.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result));return result
    except subprocess.CalledProcessError as error:
        print(error.stderr,file=sys.stderr);raise
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--profile',default='threaded');parser.add_argument('--output');args=parser.parse_args();run(args.profile,args.output)
