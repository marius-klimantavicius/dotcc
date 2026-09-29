#!/usr/bin/env python3
"""Matching native option contracts; no prerequisite executable or packet mutation."""
import argparse
import concurrent.futures
import hashlib
import importlib.util
import json
import pathlib
import subprocess
import traceback

ROOT=pathlib.Path(__file__).resolve().parents[2]

def helper(name):
    spec=importlib.util.spec_from_file_location(name,ROOT/'scripts'/name)
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module


def check_wire(path):
    def total(data):
        value=sum(int.from_bytes(data[i:i+2].ljust(2,b'\0'),'big') for i in range(0,len(data),2))
        while value>>16:value=(value&65535)+(value>>16)
        return value
    counts={'peer':0,'stack':0}
    for line in pathlib.Path(path).read_text().splitlines():
        direction,encoded=line.split();packet=bytes.fromhex(encoded)
        assert direction in counts and len(packet)>=20 and packet[0]==0x45
        assert int.from_bytes(packet[2:4],'big')==len(packet) and total(packet[:20])==65535
        protocol=packet[9];assert protocol in (6,17)
        payload=packet[20:];pseudo=packet[12:20]+bytes([0,protocol])+len(payload).to_bytes(2,'big')
        assert total(pseudo+payload)==65535
        if protocol==6:assert len(payload)>=20 and 20<=(payload[12]>>4)*4<=len(payload)
        else:assert len(payload)>=8 and int.from_bytes(payload[4:6],'big')==len(payload)
        counts[direction]+=1
    assert all(counts.values())
    return {'frames':counts,'sha256':hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest(),'coherence':'IPv4 header/length and TCP/UDP pseudo-header checksums; whole independent peer packets; no input mutation'}


def run(output):
    output=pathlib.Path(output);output.mkdir(parents=True,exist_ok=False)
    pin=json.loads((ROOT/'config/sources.json').read_text());source=ROOT/'ref'/pin['directory'];profile='threaded-enhanced'
    helper('profiles.py').write_header(ROOT,source,profile,output)
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    paths=[source/name for group,names in groups.items() if group in ('lwipcore_SRCS','lwipcore4_SRCS','lwipcore6_SRCS','lwipapi_SRCS','lwipppp_SRCS') for name in names]
    paths += [source/'src/netif/ethernet.c',source/'src/apps/sntp/sntp.c',ROOT/'src/Host/bridge.c',ROOT/'tests/NativeOracle/sys_arch.c',ROOT/'tests/Enhanced/native.c']
    original_paths=list(paths);staged,adaptations=helper('stage.py').product_units(ROOT,source,output/'staged');closure={str(p.relative_to(source)) for p in paths if p.is_relative_to(source)}
    adaptations=[row for row in adaptations if row['path'] in closure]
    paths=[staged.get(str(p.relative_to(source)),p) if p.is_relative_to(source) else p for p in paths]
    base=['cc','-std=c17','-D_POSIX_C_SOURCE=200809L','-O1','-g','-ffunction-sections','-fdata-sections','-pthread','-include','errno.h','-I'+str(output),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include')]
    hop=helper('ip6-hop.py').native_flags(ROOT,source)
    commands=[];objects=[]
    for i,(original,path) in enumerate(zip(original_paths,paths)):
        flags=(['-DDOTCC_TCPIP_WORKER=1'] if original.name=='tcpip.c' else [])+(hop if original==source/'src/core/ipv6/ip6.c' else [])
        if original==ROOT/'tests/NativeOracle/sys_arch.c':flags+=['-Dsys_now=enhanced_native_wall_now']
        obj=output/f'{i}.o';objects.append(obj);commands.append(base+flags+['-c',str(path),'-o',str(obj)])
    manifest={'profile':profile,'scope':'Native original algorithms, matching enhanced options, scoped existing product repairs; fixed fixture clock overrides sys_now at host boundary only. Unmodified native sys_arch uses its wall clock for waits.','commands':commands,'adaptations':adaptations,'profile_sha256':hashlib.sha256((output/'profile.h').read_bytes()).hexdigest(),'sources':{str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in paths}}
    (output/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    def execute(command,label,timeout=120):
        result=subprocess.run([str(p) for p in command],capture_output=True,text=True,timeout=timeout)
        (output/(label+'.stdout')).write_text(result.stdout);(output/(label+'.stderr')).write_text(result.stderr)
        return result
    def compile_one(item):
        index,command=item;result=execute(command,f'compile-{index}')
        if result.returncode:raise RuntimeError(f'Compile {index} failed: '+result.stderr)
    try:
        with concurrent.futures.ThreadPoolExecutor(max_workers=4) as workers:list(workers.map(compile_one,enumerate(commands)))
        executable=output/'enhanced';command=['cc','-pthread','-Wl,--gc-sections',*objects,'-o',executable];result=execute(command,'link')
        manifest['link']=[str(p) for p in command];(output/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
        if result.returncode:raise RuntimeError('Link failed: '+result.stderr)
        control=execute([executable,'--assert-control'],'assert-control',10)
        if control.returncode!=92 or 'FAIL' not in control.stderr:raise RuntimeError('Intentional assertion control was not detected')
        result=execute([executable,output/'wire.txt'],'run',30)
        if result.returncode:raise RuntimeError(f'Native execution failed ({result.returncode}): '+result.stderr)
        report={'profile':profile,'assert_control_exit':control.returncode,'result':json.loads(result.stdout),'wire':check_wire(output/'wire.txt'),'manifest_sha256':hashlib.sha256((output/'manifest.json').read_bytes()).hexdigest()}
        (output/'report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report));return report
    except BaseException as error:
        (output/'failure.json').write_text(json.dumps({'error':str(error),'traceback':traceback.format_exc()},indent=2)+'\n');raise

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--output',required=True);args=parser.parse_args();run(args.output)
