"""Original ZEP transport with shared radio service lifetime prototype."""
import concurrent.futures
import importlib.util
import hashlib
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[2]

def run(profile='threaded',output_case=False,original_order=False):
    source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
    family=('zep-output-original-order' if original_order else 'zep-output-native') if output_case else 'zep-native'
    work=ROOT/('build/'+family+'/'+profile);work.mkdir(parents=True,exist_ok=True)
    output=ROOT/('artifacts/'+family+'/'+profile);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('profiles',ROOT/'scripts/profiles.py');profiles=importlib.util.module_from_spec(spec);spec.loader.exec_module(profiles)
    profiles.write_header(ROOT,source,profile,work/'include')
    spec=importlib.util.spec_from_file_location('stage',ROOT/'scripts/stage.py');stage=importlib.util.module_from_spec(spec);spec.loader.exec_module(stage)
    staged_units,repairs=stage.product_units(ROOT,source,work/'source',original_lowpan_order=original_order)
    spec=importlib.util.spec_from_file_location('hop',ROOT/'scripts/ip6-hop.py');hop=importlib.util.module_from_spec(spec);spec.loader.exec_module(hop)
    hop_flags=hop.native_flags(ROOT,source)
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    paths=[staged_units.get(name,source/name) for group,names in groups.items() if group not in ('lwipmbedtls_SRCS','lwipmakefsdata_SRCS') for name in names]
    paths += [ROOT/'tests/NativeOracle/sys_arch.c',ROOT/('tests/Zep/output.c' if output_case else 'tests/Zep/native.c'),ROOT/'src/port/lowpan_service.c',ROOT/'src/Host/bridge.c']
    paths=[path for path in paths if path.name not in ('bridgeif.c','bridgeif_fdb.c','lowpan6.c','zepif.c')]+[ROOT/'src/port/bridge_service.c']
    base=['cc','-std=c17','-Werror=implicit-function-declaration','-D_POSIX_C_SOURCE=200809L','-O1','-g','-ffunction-sections','-fdata-sections','-pthread','-I'+str(work/'include'),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include'),'-I'+str(source/'contrib')]
    commands=[]
    def compile(pair):
        index,path=pair;target=work/(str(index)+'.o')
        args=base+(['-DDOTCC_TCPIP_WORKER=1'] if path.name=='tcpip.c' else [])+(hop_flags if path.name=='ip6.c' else ['-I'+str(source/'src')] if path.name=='bridge_service.c' else ['-I'+str(work/'source/src'),'-I'+str(source/'src')] if path.name=='lowpan_service.c' else [])+['-c',str(path),'-o',str(target)]
        commands.append(args);result=subprocess.run(args,text=True,capture_output=True,timeout=90)
        if result.returncode:raise RuntimeError(result.stderr)
        return target
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:objects=list(pool.map(compile,enumerate(paths)))
    binary=work/'serial';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*map(str,objects),'-o',str(binary)],check=True,capture_output=True,text=True)
    (output/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
    negative=subprocess.run([str(binary),'--negative'],text=True,capture_output=True,timeout=10)
    (output/'negative.log').write_text(negative.stdout+negative.stderr)
    if negative.returncode!=92 or 'FAIL' not in negative.stderr:raise AssertionError('Native negative assertion control missed failure')
    result=subprocess.run([str(binary)],text=True,capture_output=True,timeout=120)
    (output/'run.log').write_text(result.stdout+result.stderr)
    if result.returncode:
        (output/'failure.json').write_text(json.dumps({'profile':profile,'exit':result.returncode,
            'command':[str(binary)],'fixture':str(ROOT/('tests/Zep/output.c' if output_case else 'tests/Zep/native.c')),
            'fixture_sha256':hashlib.sha256((ROOT/('tests/Zep/output.c' if output_case else 'tests/Zep/native.c')).read_bytes()).hexdigest(),
            'wrapper_sha256':hashlib.sha256((ROOT/'src/port/lowpan_service.c').read_bytes()).hexdigest(),
            'lowpan_staged_sha256':hashlib.sha256(staged_units['src/netif/lowpan6.c'].read_bytes()).hexdigest(),
            'stderr':result.stderr},indent=2)+'\n')
    result.check_returncode()
    report=json.loads(result.stdout.splitlines()[-1]);report.update(profile=profile,negative_exit=negative.returncode,source=str(source),adaptations=repairs)
    selected=[staged_units['src/netif/lowpan6.c'],source/'src/netif/zepif.c']
    report['source_wrapper']={'path':str(ROOT/'src/port/lowpan_service.c'),
        'sha256':hashlib.sha256((ROOT/'src/port/lowpan_service.c').read_bytes()).hexdigest(),
        'includes':[{'path':str(path),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()} for path in selected],
        'ble_unit':{'path':str(source/'src/netif/lowpan6_ble.c'),'sha256':hashlib.sha256((source/'src/netif/lowpan6_ble.c').read_bytes()).hexdigest()}}

    (output/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
    (output/'report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
    return report

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser();parser.add_argument('--profile',default='threaded');parser.add_argument('--output-case',action='store_true');parser.add_argument('--original-order',action='store_true');args=parser.parse_args();run(args.profile,args.output_case,args.original_order)
