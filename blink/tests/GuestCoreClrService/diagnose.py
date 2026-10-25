#!/usr/bin/env python3
"""Build an ordinary diagnostic IL service and run it in an existing AOT HOST."""
import sys,json,shutil,socket,http.client,time,argparse
from pathlib import Path
sys.dont_write_bytecode=True
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[1]
sys.path.insert(0,str(ROOT/'tests/CoreClrRuntime'))
from evidence import Evidence,DOTNET,sha,manifest
sys.path.insert(0,str(ROOT.parent/'Scripts'))
from production_support import locked_command

def main():
 ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('--stream',action='store_true');args=ap.parse_args()
 ev=Evidence('service-exception-diagnostic');ev.value.update(fresh_qualification=False,diagnostic_only=True,original_service_qualification=False,prior_filter_stop='Prior coordinator automated filter termination retained; no safeguards changed')
 try:
  for p in HERE.glob('*'):
   if p.is_file():ev.capture(p,ev.directory/'source'/p.name)
  ev.pin(ROOT.parent/'Scripts/production_support.py');ev.pin(DOTNET)
  ev.capture(ROOT/'tests/CoreClrRuntime/global.json',ev.directory/'source/global.json')
  project=ev.directory/'source/Service.csproj'
  published=ev.directory/'published-il'
  ev.run('publish-il',locked_command(list(map(str,[DOTNET,'publish',project,'-c','Release','--disable-build-servers','-m:1','-p:UseSharedCompilation=false','-p:PublishAot=false','-p:PublishReadyToRun=false','-o',published])),env=ev.environment),timeout=120,cwd=project.parent)
  ev.tree(published)
  original=ev.pin(ROOT/'artifacts/coreclr-runtime/native/attempt-h40bq9ta/closure.json');closure=json.loads(original.read_text());prior=Path(closure['Images']);images=ev.directory/'images'
  for row in closure['Files']:
   source=ev.pin(prior/row['File'],row['Sha256']);target=images/row['File'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
   if row['GuestPath'].startswith('/apps/service/'):
    replacement=published/Path(row['File']).name
    if not replacement.is_file():raise RuntimeError('Service output missing: '+str(replacement))
    shutil.copy2(replacement,target);row['Sha256']=sha(target)
  closure['Images']=str(images);cp=ev.directory/'closure.json';cp.write_text(json.dumps(closure,indent=2)+'\n');ev.pin(cp);ev.tree(images)
  env=dict(closure['Environment'],DOTNET_ROOT=str(images/'dotnet'),LD_LIBRARY_PATH=':'.join(str(images/p.lstrip('/')) for p in closure['Environment']['LD_LIBRARY_PATH'].split(':')),TMPDIR=str(ev.directory/'tmp'))
  Path(env['TMPDIR']).mkdir();work=ev.directory/'work';work.mkdir()
  with socket.socket() as reserve:reserve.bind(('127.0.0.1',0));port=reserve.getsockname()[1]
  handle=ev.start('native',[images/'dotnet/dotnet',images/'apps/service/CoreClrService.dll',port],timeout=45,cwd=work,env=env)
  try:
   end=time.monotonic()+20
   while b'SERVICE READY\n' not in Path(handle[1]['stdout']).read_bytes():
    if handle[0].poll() is not None:raise RuntimeError('Native exited before ready')
    if time.monotonic()>end:raise TimeoutError('Native readiness')
    time.sleep(.025)
   responses=[]
   for method,path,body,expected in [('GET','/health','','CORECLR SERVICE OK\n'),('POST','/echo','payload','CORECLR:payload\n'),('POST','/stop','','STOP OK\n')]:
    c=http.client.HTTPConnection('127.0.0.1',port,timeout=10);c.request(method,path,body=body,headers={'Connection':'close'});r=c.getresponse();text=r.read().decode();c.close();responses.append(dict(path=path,status=r.status,body=text))
    if r.status!=200 or text!=expected:raise RuntimeError('Native HTTP mismatch')
  finally:out,err=ev.finish_command(handle)
  if out!=b'SERVICE READY\nSERVICE STOPPED\n' or b'DIAGNOSTIC EXCEPTION' in err:raise RuntimeError('Native service failed')
  ev.value['native_responses']=responses;ev.value['native_passed']=True
  command=None
  if args.stream:
   captured=ROOT/'artifacts/coreclr-runtime/guest/attempt-o9hc9dah/private';private=ev.directory/'private-probe'
   shutil.copytree(captured,private,ignore=shutil.ignore_patterns('bin','obj','__pycache__'))
   source=private/'tests/CoreClrRuntime/Probe/Program.cs';text=source.read_text()
   anchor='Console = new ConsoleOptions { CaptureBytes = 65536, OutputLimit = 131072 }'
   if text.count(anchor)!=1:raise RuntimeError('Private Probe console boundary differs')
   source.write_text(text.replace(anchor,'Console = new ConsoleOptions { CaptureBytes = 65536, OutputLimit = 131072, Error = Console.OpenStandardError() }',1))
   for p in private.rglob('*'):
    if p.is_file():ev.pin(p)
   ev.value['private_probe_instrumentation']='Only attempt-local public console Error attachment; guest/CPU source unchanged'
   proj=private/'tests/CoreClrRuntime/Probe/Probe.csproj';product=private/'generated/TranslatedBlink/TranslatedBlink.csproj'
   ev.run('private-probe-build',locked_command(list(map(str,[DOTNET,'build',proj,'-c','Release','--disable-build-servers','-m:1','-p:UseSharedCompilation=false','-p:BlinkProject='+str(product)])),env=ev.environment),timeout=120)
   folder=proj.parent/'bin/Release/net10.0';ev.tree(folder);command=[DOTNET,ev.pin(folder/'Probe.dll')]
  else:
   folder=ROOT/'artifacts/coreclr-runtime/guest/attempt-o9hc9dah/nativeaot';ev.tree(folder)
   app=ev.pin(folder/'Probe')
   if app.read_bytes()[:4]!=b'\x7fELF':raise RuntimeError('Expected NativeAOT HOST')
   command=[app]
  report=ev.directory/'report.json'
  ev.value['guest_report']=str(report);ev.value['guest_closure']=str(cp);ev.save()
  ev.run('guest-diagnostic',[*command,'InProcess',cp,'service','3000000000','2400','256','wx-default',report,'4096'],timeout=2430,require_success=False)
  value=json.loads(ev.pin(report).read_text());ev.value['guest_result']=value
  result=value.get('Result') or {}
  ev.value['diagnostic_capture_complete']=True
  ev.value['guest_functionally_passed']=(result.get('Reason')==0 and result.get('ExitCode')==0 and
      result.get('ResourcesReleased') is True and value.get('RunDisposed') is True and
      value.get('MachineDisposed') is True and value.get('WorkerGone') is True and
      not value.get('Error') and not value.get('CleanupError') and
      value.get('Stdout')=='SERVICE READY\nSERVICE STOPPED\n' and value.get('Responses')==[
       {'Path':'/health','Status':200,'Body':'CORECLR SERVICE OK\n'},
       {'Path':'/echo','Status':200,'Body':'CORECLR:payload\n'},
       {'Path':'/stop','Status':200,'Body':'STOP OK\n'}] and
      'DIAGNOSTIC EXCEPTION' not in value.get('Stderr',''))
  ev.check();ev.value['passed']=ev.value['guest_functionally_passed']
 except BaseException as error:ev.value['error']=repr(error)
 return ev.finish()
if __name__=='__main__':raise SystemExit(main())
