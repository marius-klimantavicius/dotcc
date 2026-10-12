#!/usr/bin/env python3
"""Configure and build the isolated roslynobj native producer."""
import argparse,pathlib,subprocess
ROOT=pathlib.Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--jobs',type=int,default=8);p.add_argument('--profile',choices=('native','source','cfront','exported'),default='native');p.add_argument('--upstream',type=pathlib.Path,default=pathlib.Path('/tmp/roslynobj-edg-upstream-20261012'));a=p.parse_args()
subprocess.run(['python3',str(ROOT/'build/generate.py'),'--upstream',str(a.upstream)],check=True)
artifact=ROOT.parents[0]/'artifacts/roslynobj'
if a.profile!='native':artifact=artifact/'profiles'/a.profile
stage=artifact/'upstream';build=artifact/'build'
subprocess.run(['python3',str(ROOT/'build/prepare.py'),'--upstream',str(a.upstream),'--profile',a.profile,'--stage',str(stage)],check=True)
subprocess.run(['cmake','-S',str(stage),'-B',str(build),'-G','Ninja','-DCMAKE_BUILD_TYPE=RelWithDebInfo','-DEDG_MACRO_CONF=roslynobj','-DEDG_BUILD_LIB=OFF','-DEDG_ENABLE_CONVERSION_WARNINGS=OFF','-DEDG_CPFE_USE_ASAN=OFF','-DTOOL_PATH_EDG_CMAKEDEF='+str(stage/'dev_tools/bin/edg-cmakedef')],check=True)
subprocess.run(['cmake','--build',str(build),'--target','roslyn-cpfe','-j',str(a.jobs)],check=True)
