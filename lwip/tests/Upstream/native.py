#!/usr/bin/env python3
"""Pinned native Check control; original lwIP test registrations and fixtures."""
import argparse
import importlib.util
import json
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[2]

def run(profile='unit'):
    source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
    suffix='' if profile=='unit' else '-nosys'
    work=ROOT/('build/upstream-native'+suffix);output=ROOT/('artifacts/upstream-native'+suffix);work.mkdir(parents=True,exist_ok=True);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('profiles',ROOT/'scripts/profiles.py');profiles=importlib.util.module_from_spec(spec);spec.loader.exec_module(profiles)
    profiles.write_header(ROOT,source,profile,work/'include')
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    stack=[name for group,names in groups.items() if group not in ('lwipmbedtls_SRCS','lwipmakefsdata_SRCS') for name in names]
    tests=json.loads((ROOT/'config/unit-sources.json').read_text())
    adapters=[]
    if profile=='unit-nosys':
        tests.remove('test/unit/api/test_sockets.c')
        adapters=[str(ROOT/'tests/Upstream/nosys.c')]
    check=ROOT/'build/check-native'
    check_source=ROOT/'ref'/json.loads((ROOT/'config/check-source.json').read_text())['directory']
    if not check_source.exists():
        raise RuntimeError('Acquire the pinned test-only Check source with scripts/fetch.sh --group tests')
    for label, args in (
        ('check-configure', ['cmake','-S',str(check_source),'-B',str(check),'-DBUILD_TESTING=OFF']),
        ('check-build', ['cmake','--build',str(check),'--target','check','--parallel','2'])):
        built=subprocess.run(args,text=True,capture_output=True,timeout=180)
        (output/(label+'.log')).write_text(built.stdout+built.stderr)
        built.check_returncode()
    control_command=['gcc','-std=gnu17','-DDOTCC_CHECK_CONTROL_MAIN=1','-I'+str(check/'src'),'-I'+str(check),str(ROOT/'tests/Upstream/check_control.c'),str(check/'src/libcheck.a'),'-pthread','-lm','-lrt','-o',str(work/'check-control')]
    subprocess.run(control_command,check=True,capture_output=True,timeout=60)
    control=subprocess.run([str(work/'check-control')],text=True,capture_output=True,timeout=30)
    (output/'check-control.log').write_text(control.stdout+control.stderr)
    (output/'check-control.json').write_text(json.dumps({'command':control_command,'exit':control.returncode,'stdout':control.stdout,'stderr':control.stderr},indent=2)+'\n')
    if control.returncode!=1 or 'CONTROL setups=3 teardowns=2 bodies=2 failures=2' not in control.stdout:
        raise AssertionError('Native Check failed its negative assertion/fixture control')
    command=['gcc','-std=gnu17','-D_POSIX_C_SOURCE=200809L','-DLWIP_UNITTESTS_NOFORK=1','-O1','-g','-pthread','-ffunction-sections','-fdata-sections','-Wl,--gc-sections',
             '-I'+str(check/'src'),'-I'+str(check),'-I'+str(work/'include'),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include'),'-I'+str(source/'test/unit'),
             *[str(source/name) for name in stack+tests],*adapters,str(ROOT/'tests/Upstream/native_port.c'),str(check/'src/libcheck.a'),'-lm','-lrt','-o',str(work/'upstream')]
    build=subprocess.run(command,text=True,capture_output=True,timeout=180)
    (output/'build.log').write_text(build.stdout+build.stderr);(output/'command.json').write_text(json.dumps(command,indent=2)+'\n');build.check_returncode()
    result=subprocess.run([str(work/'upstream')],cwd=work,text=True,capture_output=True,timeout=120)
    (output/'run.log').write_text(result.stdout+result.stderr)
    xml=work/'lwip_unittests.xml'
    if xml.exists():(output/xml.name).write_bytes(xml.read_bytes())
    (output/'result.json').write_text(json.dumps({'exit':result.returncode,'stdout':result.stdout,'stderr':result.stderr},indent=2)+'\n')
    print(result.stdout,result.stderr);result.check_returncode()

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--profile',choices=('unit','unit-nosys'),default='unit')
    run(parser.parse_args().profile)
