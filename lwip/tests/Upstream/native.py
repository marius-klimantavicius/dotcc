#!/usr/bin/env python3
"""Pinned native Check control; original lwIP test registrations and fixtures."""
import importlib.util
import json
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[2]

def run():
    source=ROOT/'ref'/json.loads((ROOT/'config/sources.json').read_text())['directory']
    work=ROOT/'build/upstream-native';output=ROOT/'artifacts/upstream-native';work.mkdir(parents=True,exist_ok=True);output.mkdir(parents=True,exist_ok=True)
    spec=importlib.util.spec_from_file_location('profiles',ROOT/'scripts/profiles.py');profiles=importlib.util.module_from_spec(spec);spec.loader.exec_module(profiles)
    profiles.write_header(ROOT,source,'unit',work/'include')
    groups=json.loads((ROOT/'config/source-groups.json').read_text())
    stack=[name for group,names in groups.items() if group not in ('lwipmbedtls_SRCS','lwipmakefsdata_SRCS') for name in names]
    tests=json.loads((ROOT/'config/unit-sources.json').read_text())
    check=ROOT/'build/check-native'
    command=['gcc','-std=gnu17','-D_POSIX_C_SOURCE=200809L','-DLWIP_UNITTESTS_NOFORK=1','-O1','-g','-pthread','-ffunction-sections','-fdata-sections','-Wl,--gc-sections',
             '-I'+str(check/'src'),'-I'+str(check),'-I'+str(work/'include'),'-I'+str(ROOT/'src/port'),'-I'+str(source/'src/include'),'-I'+str(source/'test/unit'),
             *[str(source/name) for name in stack+tests],str(ROOT/'tests/Upstream/native_port.c'),str(check/'src/libcheck.a'),'-lm','-lrt','-o',str(work/'upstream')]
    build=subprocess.run(command,text=True,capture_output=True,timeout=180)
    (output/'build.log').write_text(build.stdout+build.stderr);(output/'command.json').write_text(json.dumps(command,indent=2)+'\n');build.check_returncode()
    result=subprocess.run([str(work/'upstream')],cwd=work,text=True,capture_output=True,timeout=120)
    (output/'run.log').write_text(result.stdout+result.stderr)
    xml=work/'lwip_unittests.xml'
    if xml.exists():(output/xml.name).write_bytes(xml.read_bytes())
    (output/'result.json').write_text(json.dumps({'exit':result.returncode,'stdout':result.stdout,'stderr':result.stderr},indent=2)+'\n')
    print(result.stdout,result.stderr);result.check_returncode()

if __name__=='__main__':run()
