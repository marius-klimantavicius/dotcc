#!/usr/bin/env python3
"""Map the wrapper's -o to ROSLYNOBJ_OUTPUT without changing EDG CLI code."""
import os, pathlib, subprocess, sys, tempfile
args=sys.argv[1:]; forwarded=[]; output=None; i=0
while i<len(args):
    if args[i]=='-o':
        i+=1
        if i==len(args): raise SystemExit('-o requires a destination')
        output=args[i]
    else: forwarded.append(args[i])
    i+=1
root=pathlib.Path(__file__).resolve().parents[1]
compiler=os.environ.get('ROSLYNOBJ_COMPILER',str(root.parents[0]/'artifacts/roslynobj/build/bin/roslyn-cpfe'))
env=os.environ.copy()
forwarded=['--clear_flag','use_predefined_macro_file',*forwarded]
env['ROSLYNOBJ_ARGUMENT_COUNT']=str(len(forwarded))
for ordinal,arg in enumerate(forwarded): env['ROSLYNOBJ_ARGUMENT_'+str(ordinal)]=arg
if output is not None: env['ROSLYNOBJ_OUTPUT']=output
with tempfile.TemporaryDirectory(prefix='roslynobj-status-') as status_directory:
    status_path=pathlib.Path(status_directory)/'publication'
    env['ROSLYNOBJ_STATUS_PATH']=str(status_path)
    result=subprocess.call([compiler,*forwarded],env=env)
    if result==0 and not status_path.is_file():
        print('roslynobj: EDG did not publish an object for this invocation/profile',file=sys.stderr)
        result=1
    sys.exit(result)
