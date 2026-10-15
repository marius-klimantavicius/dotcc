#!/usr/bin/env python3
"""Drive two real worker processes using command/ack barriers, never sleeps."""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import queue
import subprocess
import sys
import threading


def record(path):
    path=Path(path)
    with path.open('rb') as stream:sha=hashlib.file_digest(stream,'sha256').hexdigest()
    return {'path':str(path.resolve()),'sha256':sha,'bytes':path.stat().st_size}


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--mode',choices=('native','generated'),required=True)
    p.add_argument('--artifacts',type=Path,required=True)
    p.add_argument('--workload',type=Path,default=Path(__file__).with_name('workload.json'))
    p.add_argument('--build-receipt',type=Path,default=Path(__file__).resolve().parents[2]/'acceptance/sqlite/native-oracle-build.json')
    p.add_argument('--worker-command-json',type=Path,help='Generated worker argv JSON, including its --api-surface argument.')
    p.add_argument('--native-report',type=Path,help='Passed separate native transcript, required for generated mode.')
    p.add_argument('--command-timeout',type=float,default=120,help='Safety watchdog only; never a success/timing assertion.')
    args=p.parse_args()
    if (args.mode=='generated')!=(args.worker_command_json is not None and args.native_report is not None):
        p.error('Generated mode requires both --worker-command-json and --native-report; native mode uses neither.')
    args.artifacts.mkdir(parents=True,exist_ok=False);root=args.artifacts.resolve();work=json.loads(args.workload.read_text())
    if work['workloadVersion']!=1 or len(work['actions'])>work['maximumCommands']:raise RuntimeError('Finite workload changed.')
    sources=[Path(__file__),args.workload]
    if args.mode=='native':
        worker=Path(__file__).with_name('native-worker.py');sources.extend([worker,args.build_receipt])
        prefix=[sys.executable,str(worker.resolve()),'--build-receipt',str(args.build_receipt.resolve())]
    else:
        prefix=json.loads(args.worker_command_json.read_text());sources.extend([args.worker_command_json,args.native_report])
        if not isinstance(prefix,list) or not prefix or not all(isinstance(v,str) for v in prefix):raise RuntimeError('Explicit generated worker argv required.')
        if '--api-surface' not in prefix:raise RuntimeError('Generated worker must receive its actual validated API surface.')
        for part in prefix:
            if Path(part).is_file():sources.append(Path(part))
    frozen={str(v.resolve()):record(v) for v in sources}
    report={'status':'incomplete-concurrency','mode':args.mode,'role':'separate-native-test-oracle-only' if args.mode=='native' else 'generated-csharp-application-only',
        'nativeApplicationFallbackPermitted':False,'workload':record(args.workload),'orchestrator':record(__file__),
        'barriers':'every acknowledged command completes before the next command is sent; no sleeps or elapsed assertions',
        'observations':[],'workers':{},'limits':work['limits']}
    processes={};readers={};logs={};queues={};result=1
    def require(condition,message):
        if not condition:raise RuntimeError(message)
    def line_reader(stream,messages):
        try:
            for line in stream:messages.put(line)
        finally:messages.put(None)
    def receive(name):
        try:line=queues[name].get(timeout=args.command_timeout)
        except queue.Empty:raise RuntimeError('Worker safety watchdog expired: '+name)
        require(line is not None,'Worker exited before acknowledgment: '+name)
        logs[name].write(line);logs[name].flush();value=json.loads(line)
        require(isinstance(value,dict),'JSON object acknowledgment required.');return value
    try:
        for name in ('A','B'):
            stderr=(root/(name+'-stderr.txt')).open('w');logs[name]=(root/(name+'-stdout.jsonl')).open('w')
            command=prefix+['--workload',str(args.workload.resolve()),'--database',str(root/'shared.db'),'--artifacts',str(root/name),'--worker',name]
            process=subprocess.Popen(command,stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=stderr,text=True,bufsize=1)
            processes[name]=(process,stderr);queues[name]=queue.Queue()
            reader=threading.Thread(target=line_reader,args=(process.stdout,queues[name]),daemon=True);readers[name]=reader;reader.start()
            ready=receive(name);require(ready=={'ready':True,'worker':name,'sqliteVersion':3054000,'threadsafe':1},'Exact ready handshake.')
            report['workers'][name]={'command':command,'ready':ready}
        for action in work['actions']:
            name=action['worker'];process=processes[name][0]
            process.stdin.write(json.dumps({k:v for k,v in action.items() if k!='worker'})+'\n');process.stdin.flush()
            ack=receive(name);require(ack.get('id')==action['id'] and ack.get('ok') is True,'Worker command failed: '+repr(ack))
            report['observations'].append({'worker':name,**ack})
        for name,(process,stderr) in processes.items():
            process.stdin.close();require(process.wait(timeout=args.command_timeout)==0,'Worker nonzero exit: '+name)
            readers[name].join(timeout=2);worker_path=root/name/'worker-report.json';worker=json.loads(worker_path.read_text())
            expected='passed-'+args.mode+'-concurrency-worker'
            require(worker['status']==expected and worker['resourcesReleased'] is True and not worker['callbackErrors'],'Worker closure/contained callback proof.')
            require(worker['workload']['sha256']==record(args.workload)['sha256'],'Worker workload identity.')
            if args.mode=='native':require(worker['library']['sha256']=='39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a','Pinned separate oracle.')
            report['workers'][name]['report']=record(worker_path)
        seen={a['id']:a['observation'] for a in report['observations']}
        busy=seen['competing-writer'];require(busy['stepCode']==5 and busy['primaryCode']==5 and busy['extendedCode']==5 and busy['finalizeCode']==5
            and busy['busyArguments']==[0],'Actual no-retry busy callback/result.')
        old=seen['old-before-commit']['rows'];require(old==seen['old-after-commit']['rows'],'Reader snapshot survives writer commit.')
        new=seen['new-after-reader-end']['rows'];require(new!=old and new==seen['reopened-rows']['rows'],'Released snapshot observes persisted writer state.')
        def checkpoint(name):
            rows=seen[name]['rows'];require(len(rows)==1 and len(rows[0])==3,'Checkpoint result shape.')
            return [int(value['decimal']) for value in rows[0]]
        passive=checkpoint('passive-pinned');require(passive[0]==0 and passive[1]>passive[2]>=0,'Pinned PASSIVE checkpoint leaves later frames uncheckpointed.')
        require(checkpoint('truncate-released')==[0,0,0],'Released TRUNCATE completed.')
        shared=seen['shared-cache'];require(shared['notificationsBeforeRelease']==0 and shared['notifications']==[{'count':1,'contexts':[2]}],
            'One actual shared-cache notification after release.')
        require(shared['blocked']['stepCode']==262 and shared['blocked']['finalizeCode']==262,'Blocked extended error finalized exactly once.')
        require(seen['thread-reads']['joined'] and len(seen['thread-reads']['threads'])==2,'Both read threads joined.')
        if args.mode=='generated':
            native=json.loads(args.native_report.read_text())
            require(native['status']=='passed-native-concurrency' and native['role']=='separate-native-test-oracle-only'
                and native['workload']['sha256']==record(args.workload)['sha256'],'Exact native reference report.')
            require(native['observations']==report['observations'],'Exact native/generated command/error/callback/thread observations.')
            report['nativeReference']=record(args.native_report);report['nativeComparison']='exact-observations-equal'
        report['status']='passed-'+args.mode+'-concurrency';result=0
    except BaseException as error:report['error']=repr(error)
    finally:
        for name,(process,stderr) in processes.items():
            if process.poll() is None:
                process.kill()
                try:process.wait(timeout=5)
                except subprocess.TimeoutExpired:pass
            stderr.close()
        for stream in logs.values():stream.close()
        report['sourceFreeze']={path:record(path)==value for path,value in frozen.items()};report['sourceReceipts']=list(frozen.values())
        if not all(report['sourceFreeze'].values()):result=1;report['status']='incomplete-concurrency'
        report['logs']=[record(v) for v in sorted(root.glob('*.txt'))]+[record(v) for v in sorted(root.glob('*.jsonl'))]
        report_path=root/'concurrency-report.json';report_path.write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps({'status':report['status'],'report':record(report_path)}));return result


if __name__=='__main__':raise SystemExit(main())
