#!/usr/bin/env python3
"""Pinned separate native FTS5 oracle with test-owned ASCII tokenizer callbacks."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import struct

ORACLE_SHA='39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a'
OBJECT_SHA='e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c'


def record(path):
    path=Path(path)
    with path.open('rb') as stream:sha=hashlib.file_digest(stream,'sha256').hexdigest()
    return {'path':str(path.resolve()),'sha256':sha,'bytes':path.stat().st_size}


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--build-receipt',type=Path,required=True)
    p.add_argument('--workload',type=Path,default=Path(__file__).with_name('workload.json'))
    p.add_argument('--output',type=Path,required=True);a=p.parse_args()
    if a.output.exists():p.error('Choose a fresh output path.')
    build=json.loads(a.build_receipt.read_text());library=Path(build['library']['path'])
    if (build['role']!='separate-native-test-oracle-only' or build['objectSha256']!=OBJECT_SHA
        or record(library)!=build['library'] or build['library']['sha256']!=ORACLE_SHA):raise RuntimeError('Pinned native oracle changed.')
    if any(os.environ.get(k) for k in ('LD_PRELOAD','LD_AUDIT','LD_LIBRARY_PATH')):raise RuntimeError('Unpinned loader overrides.')
    work=json.loads(a.workload.read_text());P=c.c_void_p;I=c.c_int;L=c.c_int64
    if work['workloadVersion']!=1:raise RuntimeError('Unexpected workload.')
    api=c.CDLL(str(library),mode=os.RTLD_LOCAL|os.RTLD_NOW);functions={}
    specs={'libversion_number':(I,[]),'open_v2':(I,[c.c_char_p,c.POINTER(P),I,c.c_char_p]),'close':(I,[P]),'errmsg':(P,[P]),
        'prepare_v2':(I,[P,P,I,c.POINTER(P),c.POINTER(P)]),'bind_pointer':(I,[P,I,P,c.c_char_p,P]),'step':(I,[P]),'finalize':(I,[P]),
        'column_count':(I,[P]),'column_name':(P,[P,I]),'column_type':(I,[P,I]),'column_int64':(L,[P,I]),'column_double':(c.c_double,[P,I]),
        'column_text':(P,[P,I]),'column_blob':(P,[P,I]),'column_bytes':(I,[P,I])}
    for name,(result,parameters) in specs.items():f=getattr(api,'sqlite3_'+name);f.restype=result;f.argtypes=parameters;functions[name]=f
    def call(name,*args):return functions[name](*args)
    def require(condition,message):
        if not condition:raise RuntimeError(message)
    def text(ptr):return c.string_at(ptr).decode('utf-8') if ptr else '<no message>'
    db=P();pending=[];events=[];errors=[];owners={};creates=deletes=destroys=0
    class Context(c.Structure):_fields_=[('magic',I)]
    class Owner(c.Structure):_fields_=[('context',P),('identifier',I),('magic',I)]
    context=Context(0x513)
    Create=c.CFUNCTYPE(I,P,c.POINTER(P),I,c.POINTER(P));Delete=c.CFUNCTYPE(None,P)
    Token=c.CFUNCTYPE(I,P,I,P,I,I,I);Tokenize=c.CFUNCTYPE(I,P,P,I,P,I,P)
    class Tokenizer(c.Structure):_fields_=[('xCreate',P),('xDelete',P),('xTokenize',P)]
    class FtsApi(c.Structure):_fields_=[('iVersion',I),('xCreateTokenizer',P),('xFindTokenizer',P),('xCreateFunction',P),('xCreateTokenizer_v2',P),('xFindTokenizer_v2',P)]
    Register=c.CFUNCTYPE(I,c.POINTER(FtsApi),c.c_char_p,P,c.POINTER(Tokenizer),P)
    def event(value):
        require(len(events)<work['maximumEvents'],'Bounded event storage.');events.append(value)
    @Create
    def create(user,arguments,count,out):
        nonlocal creates
        try:
            require(user==c.addressof(context) and context.magic==0x513 and count==0 and bool(out),'Create context/arguments.')
            require(len(owners)<8,'Owner bound.');creates+=1;owner=Owner(c.addressof(context),creates,0x514)
            owners[c.addressof(owner)]=owner;out[0]=c.addressof(owner)
            event({'event':'create','owner':creates,'argumentCount':count});return 0
        except BaseException as error:errors.append(repr(error));return 1
    @Delete
    def delete(pointer):
        nonlocal deletes
        try:
            owner=owners[pointer];require(owner.magic==0x514 and owner.context==c.addressof(context),'Delete owner.')
            event({'event':'delete','owner':owner.identifier});owner.magic=0;deletes+=1;del owners[pointer]
        except BaseException as error:errors.append(repr(error))
    @Delete
    def destroy(user):
        nonlocal destroys
        try:
            require(user==c.addressof(context) and context.magic==0x513,'Registration context.');destroys+=1
            event({'event':'registrationDestroy'})
        except BaseException as error:errors.append(repr(error))
    @Tokenize
    def tokenize(pointer,engine_context,flags,address,length,emit):
        try:
            owner=owners[pointer];require(owner.magic==0x514 and 0<=length<=work['maximumInputBytes'] and (not length or address) and emit,'Tokenize ownership/extent.')
            data=c.string_at(address,length) if length else b''
            require(all(ch==32 or 97<=ch<=122 for ch in data),'Finite lowercase ASCII-space input.')
            event({'event':'tokenize','owner':owner.identifier,'flags':flags,'bytes':length,'hex':data.hex()})
            callback=Token(emit);start=0
            while start<length:
                if data[start]==32:start+=1;continue
                end=start+1
                while end<length and data[end]!=32:end+=1
                result=callback(engine_context,0,address+start,end-start,start,end)
                event({'event':'token','owner':owner.identifier,'flags':0,'start':start,'end':end,'hex':data[start:end].hex(),'code':result})
                if result:
                    event({'event':'tokenizeResult','owner':owner.identifier,'code':result});return result
                start=end
            event({'event':'tokenizeResult','owner':owner.identifier,'code':0});return 0
        except BaseException as error:errors.append(repr(error));return 1
    tokenizer=Tokenizer(c.cast(create,P),c.cast(delete,P),c.cast(tokenize,P))
    require(c.sizeof(FtsApi)==48 and c.sizeof(Tokenizer)==24 and FtsApi.xCreateTokenizer.offset==8
        and Tokenizer.xCreate.offset==0 and Tokenizer.xDelete.offset==8 and Tokenizer.xTokenize.offset==16,'Exact native public struct layout.')
    def check(code,where):
        if code:raise RuntimeError(f'{where}: {code}: '+text(call('errmsg',db)))
    def prepare(sql):
        data=c.create_string_buffer(sql.encode());stmt=P();tail=P();code=call('prepare_v2',db,c.cast(data,P),len(data),c.byref(stmt),c.byref(tail))
        if stmt:pending.append(stmt)
        check(code,'prepare');require(stmt and tail.value==c.addressof(data)+len(sql.encode()),'Exactly one SQL statement.');return stmt
    def finish(stmt):pending.remove(stmt);check(call('finalize',stmt),'finalize')
    def read(stmt):
        count=call('column_count',stmt);require(0<=count<=8,'Column bound.');rows=[];columns=[text(call('column_name',stmt,i)) for i in range(count)]
        while True:
            code=call('step',stmt)
            if code==101:break
            require(code==100,'ROW/DONE: '+str(code));values=[]
            for i in range(count):
                kind=call('column_type',stmt,i)
                if kind==1:value={'type':'integer','decimal':str(call('column_int64',stmt,i))}
                elif kind==2:value={'type':'real','binary64Hex':struct.pack('>d',call('column_double',stmt,i)).hex()}
                elif kind in (3,4):
                    address=call('column_text' if kind==3 else 'column_blob',stmt,i);size=call('column_bytes',stmt,i)
                    require(0<=size<=1024 and (not size or address),'Borrowed value bound.')
                    value={'type':'text-utf8' if kind==3 else 'blob','bytes':size,'hex':c.string_at(address,size).hex() if size else ''}
                else:require(kind==5,'Known storage class.');value={'type':'null'}
                values.append(value)
            rows.append(values);require(len(rows)<=16,'Row bound.')
        require(not errors,'Contained callback error: '+repr(errors));return {'columns':columns,'rows':rows}
    report={'status':'incomplete-native-tokenizer','role':'separate-native-test-oracle-only','nativeApplicationFallbackPermitted':False,
        'library':record(library),'buildReceipt':record(a.build_receipt),'runner':record(__file__),'workload':record(a.workload),'limits':work['limits']};result=1
    try:
        require(call('libversion_number')==3054000,'Pinned SQLite version.');check(call('open_v2',b':memory:',c.byref(db),6,None),'open')
        slot=P();tag=c.create_string_buffer(b'fts5_api_ptr');stmt=prepare('SELECT fts5(?1)')
        try:check(call('bind_pointer',stmt,1,c.byref(slot),c.cast(tag,c.c_char_p),None),'bind_pointer');handoff=read(stmt)
        finally:finish(stmt)
        require(bool(slot),'Engine supplies fts5_api.');engine=c.cast(slot,c.POINTER(FtsApi));version=engine.contents.iVersion
        require(version>=2 and engine.contents.xCreateTokenizer,'Legacy API registration entry.');register=Register(engine.contents.xCreateTokenizer)
        check(register(engine,work['tokenizerName'].encode(),c.byref(context),c.byref(tokenizer),c.cast(destroy,P)),'xCreateTokenizer')
        results=[]
        for sql in work['statements']:
            stmt=prepare(sql)
            try:results.append(read(stmt))
            finally:finish(stmt)
        check(call('close',db),'close');db=P()
        require(not owners and creates==deletes and creates>0 and destroys==1 and not errors,'All tokenizer owners/context released after close.')
        report['observations']={'apiVersion':version,'handoff':handoff,'statements':results,'events':events,'creates':creates,'deletes':deletes,'registrationDestroys':destroys}
        report['status']='passed-native-tokenizer';result=0
    except BaseException as error:report['error']=repr(error)
    finally:
        cleanup=[]
        for stmt in list(pending):
            try:finish(stmt)
            except BaseException as error:cleanup.append(repr(error))
        if db:
            try:check(call('close',db),'cleanup close');db=P()
            except BaseException as error:cleanup.append(repr(error))
        report['callbackErrors']=errors;report['cleanupErrors']=cleanup;report['resourcesReleased']=not db and not pending and not owners and not cleanup
        if errors or not report['resourcesReleased']:report['status']='incomplete-native-tokenizer';result=1
        a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps({'status':report['status'],'report':record(a.output)}));return result


if __name__=='__main__':raise SystemExit(main())
