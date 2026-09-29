"""Native extended-stack layout and addon controls, with fixed host clock."""
import concurrent.futures
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[2]

def run():
    source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
    work=ROOT/'build/contrib-native';work.mkdir(parents=True,exist_ok=True)
    output=ROOT/'artifacts/contrib-native';output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('profiles',ROOT/'scripts/profiles.py');profiles=importlib.util.module_from_spec(spec);spec.loader.exec_module(profiles)
    profiles.write_header(ROOT,source,'contrib',work/'include')
    spec=importlib.util.spec_from_file_location('stage',ROOT/'scripts/stage.py');stage=importlib.util.module_from_spec(spec);spec.loader.exec_module(stage)
    staged_units,repairs=stage.product_units(ROOT,source,work/'source')
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    paths=[staged_units.get(name,source/name) for group,names in groups.items() if group not in ('lwipmbedtls_SRCS','lwipmakefsdata_SRCS') for name in names]
    paths += [source/name for name in json.loads((ROOT/'config/contrib-sources.json').read_text())]
    paths += [ROOT/'tests/NativeOracle/sys_arch.c',ROOT/'tests/Contrib/contract.c']
    base=['cc','-std=c17','-D_POSIX_C_SOURCE=200809L','-O1','-g','-ffunction-sections','-fdata-sections','-pthread','-I'+str(work/'include'),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include'),'-I'+str(source/'contrib')]
    commands=[]
    def compile(pair):
        index,path=pair;target=work/(str(index)+'.o')
        args=base+(['-Dsys_now=native_realtime_now'] if path.name=='sys_arch.c' else [])+['-c',str(path),'-o',str(target)]
        commands.append(args);result=subprocess.run(args,text=True,capture_output=True,timeout=90)
        if result.returncode:raise RuntimeError(result.stderr)
        return target
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:objects=list(pool.map(compile,enumerate(paths)))
    binary=work/'contract';subprocess.run(['cc','-pthread','-Wl,--gc-sections',*map(str,objects),'-o',str(binary)],check=True,capture_output=True,text=True)
    negative=subprocess.run([str(binary),'--negative'],text=True,capture_output=True,timeout=10)
    (output/'negative.log').write_text(negative.stdout+negative.stderr)
    if negative.returncode!=92 or 'FAIL' not in negative.stderr:raise AssertionError('Native negative assertion control missed failure')
    result=subprocess.run([str(binary)],text=True,capture_output=True,timeout=15)
    (output/'run.log').write_text(result.stdout+result.stderr);result.check_returncode()
    report=json.loads(result.stdout);report.update(profile='contrib',negative_exit=negative.returncode,source=str(source),fixed_clock_ms=1000,adaptations=repairs,
        route_empty_behavior='ip6_find_route_entry returns zero for unused zero-prefix slot; ip6_static_route returns NULL')
    (output/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
    (output/'report.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
    return report

if __name__=='__main__':run()
