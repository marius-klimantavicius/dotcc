"""Bounded direct current-runtime controls; never emits or changes products."""
import datetime, hashlib, json, os, pathlib, signal, subprocess, sys, time
ROOT = pathlib.Path(__file__).resolve().parents[3]
attempt = 'controls-' + datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%S') + '-' + str(os.getpid())
WORK = ROOT / 'cpython/build/owner-b-r23-20261018' / attempt
OUT = ROOT / 'cpython/artifacts/owner-b-r23-20261018' / attempt
SHARED = '/tmp/dotcc-cpython-shared-build.lock'
NATIVE = '/tmp/dotcc-cpython-native-build.lock'
LOCKS = [SHARED, NATIVE, str(ROOT/'cpython/build/campaign/run.lock')]
WORK.mkdir(parents=True, exist_ok=True); OUT.mkdir(parents=True, exist_ok=True)
start = time.monotonic(); records = []; comparisons = []
def save():
    (OUT/'current-libc-results.json').write_text(json.dumps({'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'elapsedSeconds':time.monotonic()-start,'scope':'native and direct current source Libc JIT/AOT only; no compiler/tool resources or product qualification','commands':records,'comparisons':comparisons},indent=2)+'\n')
def run(label, args, expected=0, timeout=240):
    if time.monotonic()-start > 850: raise RuntimeError('group safe boundary reached before next command')
    log=OUT/(label+'.log'); print('START',label,flush=True)
    t=time.monotonic(); p=subprocess.run([str(a) for a in args],cwd=ROOT,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=timeout)
    log.write_text(p.stdout)
    r={'label':label,'argv':[str(a) for a in args],'cwd':str(ROOT),'exitCode':p.returncode,'expectedExitCode':expected,'seconds':time.monotonic()-t,'log':str(log),'logSha256':hashlib.sha256(log.read_bytes()).hexdigest()};records.append(r);save()
    print('END',label,p.returncode,round(r['seconds'],2),flush=True)
    if expected is not None and p.returncode!=expected: raise RuntimeError(f'{label}: exit {p.returncode}; see {log}')
    return p
try:
    for i,lock in enumerate(LOCKS):run('preflight-lock-'+str(i),['flock','-n','--close',lock,'true'])
    native=WORK/'runtime-native'
    run('native-build',['flock','-n','--close',NATIVE,'cc','-pthread','-O2','-Wall','-Wextra','-Werror',ROOT/'cpython/tests/RuntimeRepair/native.c','-o',native])
    project=ROOT/'cpython/tests/RuntimeRepair/RuntimeRepair.csproj'
    common=['-c','Release','--disable-build-servers','-p:UseSharedCompilation=false','-m:1','-v:minimal']
    run('jit-build',['flock','-n','--close',SHARED,'dotnet','build',project,*common,'--artifacts-path',WORK/'jit-artifacts','-o',WORK/'jit'])
    run('aot-publish',['flock','-n','--close',SHARED,'dotnet','publish',project,*common,'-r','linux-x64','-p:PublishAot=true','--artifacts-path',WORK/'aot-artifacts','-o',WORK/'aot'],timeout=360)
    apps={'native':[native],'jit':['dotnet',WORK/'jit/DotCC.Tests.dll'],'aot':[WORK/'aot/DotCC.Tests']}
    fixture=WORK/'fixtures';fixture.mkdir(exist_ok=True)
    (fixture/'item').write_text('x');(fixture/'directory').mkdir(exist_ok=True);(fixture/'directory/item').write_text('x')
    (fixture/'denied').mkdir(exist_ok=True);os.chmod(fixture/'denied',0)
    for name,target in [('file-link','item'),('dir-link','directory'),('dangling','missing'),('loop','loop')]:
        p=fixture/name
        if not p.is_symlink():p.symlink_to(target)
    paths={'file':(fixture/'item',20),'file-child':(fixture/'item/child',20),'file-slash':(str(fixture/'item')+'/',20),'file-dotdot':(str(fixture/'item')+'/..',20),'file-link':(fixture/'file-link',20),'file-link-child':(fixture/'file-link/child',20),'missing':(fixture/'missing/child',2),'dangling':(fixture/'dangling',2),'loop':(fixture/'loop',40),'long':(fixture/('z'*260),36),'access':(fixture/'denied',13),'directory':(fixture/'directory',None),'directory-link':(fixture/'dir-link',None)}
    observations={}
    for form,app in apps.items():
        for name,(path,error) in paths.items():
            command=app+['directory',path]
            if name=='access' and os.geteuid()==0:command=['runuser','-u','nobody','--',*command]
            p=run(form+'-directory-'+name,command)
            expected='directory=1,1,1 close=0' if error is None else f'errno={error}'
            comparisons.append({'label':form+'-'+name,'expected':expected,'observed':p.stdout.strip(),'passed':p.stdout.strip()==expected});save()
            if p.stdout.strip()!=expected:raise RuntimeError('directory mismatch '+form+' '+name)
        for mode in ['host-abi','sync','ignore','error','reentrant','blocked','default']:
            p=run(form+'-signal-'+mode,app+[mode],expected=None)
            observations[(form,mode)]=(p.returncode,p.stdout)
            if mode=='default':
                ok=p.returncode == -signal.SIGINT and 'before-default' in p.stdout and 'unexpected-return' not in p.stdout
            elif form=='native':ok=p.returncode==0
            else:ok=(p.returncode,p.stdout)==observations[('native',mode)]
            comparisons.append({'label':form+'-'+mode,'passed':ok,'exitCode':p.returncode,'stdout':p.stdout});save()
            if not ok:raise RuntimeError('signal mismatch '+form+' '+mode)
        if form!='native':
            run(form+'-ownership-and-directory-regressions',app+['all'])
            # A real OS-delivered event after repeated installation. Parent
            # drives only this disposable child and checks exact callback count.
            label=form+'-host-event'; command=[str(a) for a in app+['host']]
            p=subprocess.Popen(command,cwd=ROOT,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
            ready=p.stdout.readline()
            if ready.strip()=='ready':os.kill(p.pid,signal.SIGINT)
            rest,_=p.communicate(timeout=20); text=ready+rest
            (OUT/(label+'.log')).write_text(text)
            records.append({'label':label,'argv':command,'exitCode':p.returncode,'stdout':text,'log':str(OUT/(label+'.log'))});save()
            if p.returncode!=0 or 'host=1' not in text:raise RuntimeError('OS event callback count/ownership failed')
    run('runtime-unit-tests',['flock','-n','--close',SHARED,'dotnet','test',ROOT/'cpython/tests/RuntimeRepair/Unit/RuntimeRepairUnit.csproj',*common,'--artifacts-path',WORK/'unit-artifacts','--logger','trx;LogFileName=runtime-unit.trx','--results-directory',OUT/'unit-results'],timeout=240)
    (OUT/'control-group-status.json').write_text(json.dumps({'status':'passed','seconds':time.monotonic()-start,'ownedActiveCommands':[],'ownedQueuedCommands':[]},indent=2)+'\n')
except Exception as e:
    (OUT/'control-group-status.json').write_text(json.dumps({'status':'failed','error':str(e),'seconds':time.monotonic()-start,'ownedActiveCommands':[],'ownedQueuedCommands':[]},indent=2)+'\n')
    print(str(e),file=sys.stderr);sys.exit(1)
