"""Native original HTTP application transactions."""
import concurrent.futures
import importlib.util
import json
import shutil
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[2]

def run(profile='threaded'):
    source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
    work=ROOT/('build/http-native/'+profile);work.mkdir(parents=True,exist_ok=True)
    output=ROOT/('artifacts/http-native/'+profile);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('profiles',ROOT/'scripts/profiles.py');profiles=importlib.util.module_from_spec(spec);spec.loader.exec_module(profiles)
    profiles.write_header(ROOT,source,profile,work/'include')
    generated=None
    if profile=='http-content':
        publication=json.loads((ROOT/'artifacts/campaign/current-http-content.json').read_text())
        receipt=json.loads(Path(publication['receipt']).read_text())
        generated=receipt['generated_http_content']
        shutil.copyfile(generated['image'],work/'include/dotcc_fsdata.c')

    spec=importlib.util.spec_from_file_location('stage',ROOT/'scripts/stage.py');stage=importlib.util.module_from_spec(spec);spec.loader.exec_module(stage)
    staged_units,repairs=stage.product_units(ROOT,source,work/'source')
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    paths=[staged_units.get(name,source/name) for group,names in groups.items() if group not in ('lwipmbedtls_SRCS','lwipmakefsdata_SRCS') for name in names]
    paths += [ROOT/'tests/NativeOracle/sys_arch.c',ROOT/'tests/Applications/http.c',ROOT/'src/Host/bridge.c',ROOT/'src/Host/app_port.c']
    base=['cc','-std=c17','-D_POSIX_C_SOURCE=200809L','-O1','-g','-ffunction-sections','-fdata-sections','-pthread','-I'+str(work/'include'),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include'),'-I'+str(source/'contrib')]
    commands=[]
    def compile(pair):
        index,path=pair;target=work/(str(index)+'.o')
        args=base+(['-DDOTCC_TCPIP_WORKER=1'] if path.name=='tcpip.c' else [])+['-c',str(path),'-o',str(target)]
        commands.append(args);result=subprocess.run(args,text=True,capture_output=True,timeout=90)
        if result.returncode:raise RuntimeError(result.stderr)
        return target
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:objects=list(pool.map(compile,enumerate(paths)))
    binary=work/'http';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*map(str,objects),'-o',str(binary)],check=True,capture_output=True,text=True)
    negative=subprocess.run([str(binary),'--negative'],text=True,capture_output=True,timeout=10)
    (output/'negative.log').write_text(negative.stdout+negative.stderr)
    if negative.returncode!=92 or 'FAIL' not in negative.stderr:raise AssertionError('Native negative assertion control missed failure')
    result=subprocess.run([str(binary)]+(['--generated'] if generated else []),text=True,capture_output=True,timeout=15)
    (output/'run.log').write_text(result.stdout+result.stderr);result.check_returncode()
    report=json.loads(result.stdout);report.update(profile=profile,negative_exit=negative.returncode,source=str(source),adaptations=repairs,generated_content=generated)
    (output/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
    (output/'report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
    return report

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser();parser.add_argument('--profile',default='threaded');run(parser.parse_args().profile)
