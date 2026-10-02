"""Configured terminate handler on console, initialization, thread and array paths."""
import json
import os
from pathlib import Path
import resource
import subprocess
import sys
from libcxx_exception_ptr_verify import write_exception_fixture


def verify_libcxx_terminate_boundaries(root,tests,out,nativeaot,run,acquisition):
    acquisition=Path(acquisition).resolve()
    library=acquisition/json.loads((acquisition/'acquisition.json').read_text())['source']['archiveRoot']
    expected={0:['60/0','-60/1','900/1'],1:['70/0','900/1'],2:['80/0','-80/1','900/1'],
              3:['102/0','-102/1','101/1','900/1'],4:['90/0','-90/1','900/1']}
    reasons={0:'Exception escaped translated main',1:'Exception escaped translated thread',
             2:'Destructor threw during shutdown',3:'Array destructor threw during unwinding',4:'Exception during global initialization'}
    checks=[]
    for instance,global_failure in ((False,False),(False,True),(True,False),(True,True)):
        variant=('instance' if instance else 'static')+('-global' if global_failure else '');directory=out/variant;combined=out/(variant+'.cpp')
        write_exception_fixture(root,tests/'libcxx/terminate_boundaries.cpp',library,combined)
        run([sys.executable,root/'dotnet.py',combined,'--libc','dotcc','--std','c++20',
             '-I',acquisition/'config','-I',library/'include','-I',library/'src','--output-dir',directory,
             '--namespace','','--class-name','Core','--split','size','--split-size','3200',*(['--instance-methods'] if instance else []),*(['-D','EDG_TEST_GLOBAL_FAILURE=1'] if global_failure else [])])
        project=directory/'Generated.csproj';run(['dotnet','build',project,'-c','Release'])
        executions=[('jit',['dotnet',directory/'bin/Release/net10.0/Generated.dll'])]
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            executions.append(('aot',[directory/'aot/Generated']))
        for runtime,command in executions:
            for scenario in ([4] if global_failure else range(4)):
                label=f'{variant}-{runtime}-{scenario}'
                def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
                result=subprocess.run([*map(str,command),str(scenario)],text=True,capture_output=True,timeout=30,preexec_fn=no_core)
                (directory/(label+'.stdout')).write_text(result.stdout);(directory/(label+'.stderr')).write_text(result.stderr)
                (directory/(label+'.json')).write_text(json.dumps({'command':list(map(str,command)),'scenario':scenario,'exit':result.returncode},indent=2))
                assert result.returncode!=0 and result.stdout.splitlines()==expected[scenario] and reasons[scenario] in result.stderr,(label,expected[scenario],result.stdout,result.stderr[:1500])
                checks.append(label+': '+reasons[scenario]+', configured handler and EDG cleanup order')
    return checks
