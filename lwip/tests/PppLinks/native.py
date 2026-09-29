"""Native original PPPoS authentication and dual-stack control negotiations."""
import concurrent.futures
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[2]

def run(profile='threaded'):
    source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
    work=ROOT/('build/ppp-links-native/'+profile);work.mkdir(parents=True,exist_ok=True)
    output=ROOT/('artifacts/ppp-links-native/'+profile);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('profiles',ROOT/'scripts/profiles.py');profiles=importlib.util.module_from_spec(spec);spec.loader.exec_module(profiles)
    profiles.write_header(ROOT,source,profile,work/'include')
    spec=importlib.util.spec_from_file_location('stage',ROOT/'scripts/stage.py');stage=importlib.util.module_from_spec(spec);spec.loader.exec_module(stage)
    staged_units,repairs=stage.product_units(ROOT,source,work/'source')
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    paths=[staged_units.get(name,source/name) for group,names in groups.items() if group not in ('lwipmbedtls_SRCS','lwipmakefsdata_SRCS') for name in names]
    paths += [ROOT/'tests/NativeOracle/sys_arch.c',ROOT/'tests/PppLinks/serial.c',ROOT/'src/Host/bridge.c']
    base=['cc','-std=c17','-D_POSIX_C_SOURCE=200809L','-O1','-g','-ffunction-sections','-fdata-sections','-pthread','-I'+str(work/'include'),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include'),'-I'+str(source/'contrib')]
    commands=[]
    def compile(pair):
        index,path=pair;target=work/(str(index)+'.o')
        args=base+(['-Dsys_now=native_realtime_now','-Dsys_jiffies=native_realtime_jiffies'] if path.name=='sys_arch.c' else ['-DDOTCC_TCPIP_WORKER=1'] if path.name=='tcpip.c' else [])+['-c',str(path),'-o',str(target)]
        commands.append(args);result=subprocess.run(args,text=True,capture_output=True,timeout=90)
        if result.returncode:raise RuntimeError(result.stderr)
        return target
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:objects=list(pool.map(compile,enumerate(paths)))
    binary=work/'serial';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*map(str,objects),'-o',str(binary)],check=True,capture_output=True,text=True)
    # Exactly the same final fixture/other objects; only EAP's staged local release differs.
    eap_index=next(index for index,path in enumerate(paths) if path.name=='eap.c')
    original_object=work/'eap-original.o'
    original_command=base+['-c',str(source/'src/netif/ppp/eap.c'),'-o',str(original_object)]
    commands.append(original_command);subprocess.run(original_command,check=True,capture_output=True,text=True)
    original_objects=objects.copy();original_objects[eap_index]=original_object
    original_binary=work/'serial-original-eap'
    subprocess.run(['cc','-pthread','-Wl,--gc-sections',*map(str,original_objects),'-o',str(original_binary)],check=True,capture_output=True,text=True)
    original=subprocess.run([str(original_binary),'--eap-only'],text=True,capture_output=True,timeout=120)
    (output/'original-eap.log').write_text(original.stdout+original.stderr)
    if original.returncode!=92 or 'lwip_stats.mem.used==memory' not in original.stderr:
        raise AssertionError('Pinned original EAP did not reproduce retained pbuf baseline failure')

    negative=subprocess.run([str(binary),'--negative'],text=True,capture_output=True,timeout=10)
    (output/'negative.log').write_text(negative.stdout+negative.stderr)
    if negative.returncode!=92 or 'FAIL' not in negative.stderr:raise AssertionError('Native negative assertion control missed failure')
    result=subprocess.run([str(binary)],text=True,capture_output=True,timeout=120)
    (output/'run.log').write_text(result.stdout+result.stderr);result.check_returncode()
    report=json.loads(result.stdout.splitlines()[-1]);report.update(profile=profile,negative_exit=negative.returncode,source=str(source),adaptations=repairs,original_eap_exit=original.returncode)
    (output/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
    (output/'report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
    return report

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser();parser.add_argument('--profile',default='threaded');run(parser.parse_args().profile)
