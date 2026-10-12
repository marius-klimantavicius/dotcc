#!/usr/bin/env python3
"""Configure and build the isolated roslynobj native producer."""
import argparse,pathlib,subprocess
ROOT=pathlib.Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--jobs',type=int,default=8);a=p.parse_args()
subprocess.run(['python3',str(ROOT/'build/generate.py')],check=True)
subprocess.run(['python3',str(ROOT/'build/prepare.py')],check=True)
stage=ROOT.parents[0]/'artifacts/roslynobj/upstream';build=stage.parent/'build'
subprocess.run(['cmake','-S',str(stage),'-B',str(build),'-G','Ninja','-DCMAKE_BUILD_TYPE=RelWithDebInfo','-DEDG_MACRO_CONF=roslynobj','-DEDG_BUILD_LIB=OFF','-DEDG_ENABLE_CONVERSION_WARNINGS=OFF','-DEDG_CPFE_USE_ASAN=OFF','-DTOOL_PATH_EDG_CMAKEDEF='+str(stage/'dev_tools/bin/edg-cmakedef')],check=True)
subprocess.run(['cmake','--build',str(build),'--target','roslyn-cpfe','-j',str(a.jobs)],check=True)
