#!/usr/bin/env python3
"""Native-only JSONL command worker; only the pinned absolute oracle is loaded."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import struct
import sys
import threading

OBJECT_SHA="e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
ORACLE_SHA="39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a"


def record(path):
    path=Path(path)
    with path.open('rb') as stream: sha=hashlib.file_digest(stream,'sha256').hexdigest()
    return {'path':str(path.resolve()),'sha256':sha,'bytes':path.stat().st_size}


def main():
    p=argparse.ArgumentParser(description=__doc__)
    for name in ('build-receipt','workload','database','artifacts'):p.add_argument('--'+name,type=Path,required=True)
    p.add_argument('--worker',choices=('A','B'),required=True);args=p.parse_args()
    args.artifacts.mkdir(parents=True,exist_ok=False);root=args.artifacts.resolve()
    build=json.loads(args.build_receipt.read_text());library=Path(build['library']['path'])
    if (build['role']!='separate-native-test-oracle-only' or build['objectSha256']!=OBJECT_SHA
        or record(library)!=build['library'] or build['library']['sha256']!=ORACLE_SHA):raise RuntimeError('Pinned native oracle changed.')
    if any(os.environ.get(k) for k in ('LD_PRELOAD','LD_AUDIT','LD_LIBRARY_PATH')):raise RuntimeError('Unpinned loader overrides.')
    work=json.loads(args.workload.read_text());P=c.c_void_p;I=c.c_int;L=c.c_int64
    if work['workloadVersion']!=1:raise RuntimeError('Unexpected workload.')
    api=c.CDLL(str(library),mode=os.RTLD_LOCAL|os.RTLD_NOW);functions={}
    specs={'libversion_number':(I,[]),'threadsafe':(I,[]),'open_v2':(I,[c.c_char_p,c.POINTER(P),I,c.c_char_p]),
        'close':(I,[P]),'errmsg':(P,[P]),'prepare_v2':(I,[P,P,I,c.POINTER(P),c.POINTER(P)]),'step':(I,[P]),'finalize':(I,[P]),
        'column_count':(I,[P]),'column_name':(P,[P,I]),'column_type':(I,[P,I]),'column_int64':(L,[P,I]),'column_double':(c.c_double,[P,I]),
        'column_text':(P,[P,I]),'column_blob':(P,[P,I]),'column_bytes':(I,[P,I]),'errcode':(I,[P]),'extended_errcode':(I,[P]),
        'extended_result_codes':(I,[P,I]),'busy_handler':(I,[P,P,P]),'unlock_notify':(I,[P,P,P])}
    for name,(ret,parameters) in specs.items():
        f=getattr(api,'sqlite3_'+name);f.restype=ret;f.argtypes=parameters;functions[name]=f
    def call(name,*values):return functions[name](*values)
    def require(condition,message):
        if not condition:raise RuntimeError(message)
    def text(ptr):return c.string_at(ptr).decode('utf-8') if ptr else '<no message>'
    def check(code,db,where):
        if code:raise RuntimeError(f'{where}: {code}: '+text(call('errmsg',db)))
    handles={};pending={};callback_errors=[];busy_arguments=[];unlock_events=[]
    class Context(c.Structure):_fields_=[('magic',I),('identifier',I)]
    busy_context=Context(0x431,1);unlock_context=Context(0x431,2)
    Busy=c.CFUNCTYPE(I,P,I);Notify=c.CFUNCTYPE(None,c.POINTER(P),I)
    @Busy
    def busy(context,attempt):
        try:
            q=c.cast(context,c.POINTER(Context)).contents
            require(q.magic==0x431 and q.identifier==1 and len(busy_arguments)<16,'Busy callback fixed context/bound.')
            busy_arguments.append(attempt)
        except BaseException as error:callback_errors.append(repr(error))
        return 0
    @Notify
    def notify(contexts,count):
        try:
            require(0<count<=4,'Finite notify group.');ids=[]
            for index in range(count):
                q=c.cast(contexts[index],c.POINTER(Context)).contents
                require(q.magic==0x431 and q.identifier==2,'Notify fixed context.');ids.append(q.identifier)
            unlock_events.append({'count':count,'contexts':ids})
        except BaseException as error:callback_errors.append(repr(error))
    def open_db(name,path,shared=False):
        require(name not in handles,'Unique connection owner.');db=P()
        code=call('open_v2',os.fsencode(path),c.byref(db),6|0x10000|(0x20000 if shared else 0x40000),None)
        if db:handles[name]=db
        check(code,db,'open');check(call('extended_result_codes',db,1),db,'extended_result_codes');return db
    def close_db(name):
        db=handles[name];check(call('busy_handler',db,None,None),db,'cancel busy');check(call('unlock_notify',db,None,None),db,'cancel notify')
        check(call('close',db),db,'close');del handles[name]
    def prepare(name,sql):
        db=handles[name];data=c.create_string_buffer(sql.encode());stmt=P();tail=P()
        code=call('prepare_v2',db,c.cast(data,P),len(data),c.byref(stmt),c.byref(tail))
        if stmt:pending[stmt.value]=(name,stmt)
        check(code,db,'prepare');require(bool(stmt) and tail.value==c.addressof(data)+len(sql.encode()),'Exactly one SQL statement.');return stmt
    def finalize(stmt):
        del pending[stmt.value];return call('finalize',stmt)
    def read(name,stmt):
        db=handles[name];n=call('column_count',stmt);require(0<=n<=8,'Column bound.')
        columns=[text(call('column_name',stmt,i)) for i in range(n)];rows=[]
        while True:
            code=call('step',stmt)
            if code!=100:break
            values=[]
            for i in range(n):
                kind=call('column_type',stmt,i)
                if kind==1:value={'type':'integer','decimal':str(call('column_int64',stmt,i))}
                elif kind==2:value={'type':'real','binary64Hex':struct.pack('>d',call('column_double',stmt,i)).hex()}
                elif kind in (3,4):
                    address=call('column_text' if kind==3 else 'column_blob',stmt,i);size=call('column_bytes',stmt,i)
                    require(0<=size<=work['maximumColumnBytes'] and (not size or address),'Borrowed value bound.')
                    value={'type':'text-utf8' if kind==3 else 'blob','bytes':size,'hex':c.string_at(address,size).hex() if size else ''}
                else:require(kind==5,'Known storage class.');value={'type':'null'}
                values.append(value)
            rows.append(values);require(len(rows)<=work['maximumRows'],'Row bound.')
        return {'columns':columns,'rows':rows,'stepCode':code,'primaryCode':call('errcode',db)&255,'extendedCode':call('extended_errcode',db)}
    def sql(name,statement,expected=101):
        stmt=prepare(name,statement)
        try:result=read(name,stmt)
        finally:code=finalize(stmt)
        result['finalizeCode']=code
        require(result['stepCode']==expected and code==(0 if expected==101 else expected),'Exact step/finalize result.')
        require(not callback_errors,'Callback contained failure.');return result
    def shared():
        path=root/'shared.db';one=open_db('one',path,True)
        for statement in work['sharedSetup']:sql('one',statement)
        two=open_db('two',path,True);sql('one','BEGIN IMMEDIATE');sql('one',work['sharedWrite'])
        blocked=prepare('two',work['read']);observation=read('two',blocked)
        require(observation['stepCode']==262 and observation['primaryCode']==6 and observation['extendedCode']==262,'Real shared-cache table lock.')
        check(call('unlock_notify',two,c.cast(notify,P),c.byref(unlock_context)),two,'unlock_notify')
        before=len(unlock_events);require(before==0,'Writer still holds lock before commit.');sql('one','COMMIT')
        require(len(unlock_events)==1 and not callback_errors,'Commit triggers actual notification.')
        observation['finalizeCode']=finalize(blocked);require(observation['finalizeCode']==262,'Finalize blocked statement once.')
        retried=sql('two',work['read']);check(call('unlock_notify',two,None,None),two,'cancel notify')
        result={'blocked':observation,'notificationsBeforeRelease':before,'notifications':list(unlock_events),'retry':retried}
        close_db('two');close_db('one');return result
    def threads():
        count=work['threadRounds'];require(count==3,'Finite thread rounds.');barrier=threading.Barrier(2);results=[None,None];errors=[]
        def body(index):
            name='thread'+str(index)
            try:
                open_db(name,args.database);barrier.wait(timeout=30);rows=[]
                for _ in range(count):
                    barrier.wait(timeout=30);rows.append(sql(name,work['read']));barrier.wait(timeout=30)
                close_db(name);results[index]={'thread':index,'rounds':rows}
            except BaseException as error:
                errors.append(repr(error));barrier.abort()
        workers=[threading.Thread(target=body,args=(i,),name='sqlite-read-'+str(i)) for i in range(2)]
        for t in workers:t.start()
        for t in workers:t.join(timeout=40)
        require(not any(t.is_alive() for t in workers) and not errors,'Threads joined without failure: '+repr(errors))
        require(results[0]['rounds']==results[1]['rounds'],'Per-thread read observations equal.');return {'threads':results,'joined':True}
    def emit(value):print(json.dumps(value,separators=(',',':')),flush=True)
    require(call('libversion_number')==3054000 and call('threadsafe')==1,'Pinned THREADSAFE=1 SQLite.')
    report={'status':'incomplete-native-concurrency-worker','role':'separate-native-test-oracle-only','library':record(library),
        'buildReceipt':record(args.build_receipt),'workerSource':record(__file__),'workload':record(args.workload),'worker':args.worker,'commands':[]}
    emit({'ready':True,'worker':args.worker,'sqliteVersion':3054000,'threadsafe':1});result=1
    try:
        for line in sys.stdin:
            require(len(report['commands'])<work['maximumCommands'],'Command bound.');command=json.loads(line);op=command['op']
            try:
                if op=='open':open_db('main',args.database);observed={'code':0}
                elif op=='close':close_db('main');observed={'code':0}
                elif op=='busy':check(call('busy_handler',handles['main'],c.cast(busy,P),c.byref(busy_context)),handles['main'],'busy_handler');observed={'code':0}
                elif op=='sql':observed=sql('main',command['sql'],command.get('expectedStep',101))
                elif op=='shared':observed=shared()
                elif op=='threads':observed=threads()
                elif op=='quit':require(not handles and not pending,'Closed before quit.');observed={'resourcesReleased':True};result=0
                else:raise RuntimeError('Unknown finite operation.')
                observed['busyArguments']=list(busy_arguments);ack={'id':command['id'],'ok':True,'observation':observed}
                report['commands'].append(ack);emit(ack)
                if op=='quit':break
            except BaseException as error:
                ack={'id':command['id'],'ok':False,'error':repr(error)};report['commands'].append(ack);emit(ack);break
    finally:
        for _,stmt in list(pending.values()):finalize(stmt)
        for name in list(handles):close_db(name)
        report['resourcesReleased']=not handles and not pending;report['callbackErrors']=callback_errors
        if result==0 and not callback_errors:report['status']='passed-native-concurrency-worker'
        report['files']=[record(p) for p in sorted(root.iterdir()) if p.is_file()]
        (root/'worker-report.json').write_text(json.dumps(report,indent=2)+'\n')
    return result


if __name__=='__main__':raise SystemExit(main())
