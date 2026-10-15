#!/usr/bin/env python3
"""Finite API observations in the pinned separate native SQLite oracle only."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import struct

SQLITE_SHA = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
ORACLE_SHA = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a"


def receipt(path):
    path = Path(path)
    with path.open("rb") as stream: sha = hashlib.file_digest(stream, "sha256").hexdigest()
    return {"path":str(path.resolve()),"sha256":sha,"bytes":path.stat().st_size}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build-receipt",type=Path,required=True)
    parser.add_argument("--workload",type=Path,default=Path(__file__).with_name("workload.json"))
    parser.add_argument("--output",type=Path,required=True)
    args = parser.parse_args()
    if args.output.exists(): parser.error("Choose a fresh output path.")
    build = json.loads(args.build_receipt.read_text()); library = Path(build["library"]["path"])
    if (build["role"] != "separate-native-test-oracle-only" or build["objectSha256"] != SQLITE_SHA
        or receipt(library) != build["library"] or build["library"]["sha256"] != ORACLE_SHA): raise RuntimeError("Pinned native oracle identity/role changed.")
    work = json.loads(args.workload.read_text())
    if work["workloadVersion"] != 1 or work["session"]["applyPasses"] != 2 or work["session"]["conflictPolicy"] != "OMIT":
        raise RuntimeError("Unsupported finite workload.")
    report = {"status":"incomplete-native-api-callbacks","role":"separate-native-test-oracle-only","generatedExecution":False,
        "nativeApplicationFallbackPermitted":False,"library":receipt(library),"buildReceipt":receipt(args.build_receipt),
        "runner":receipt(__file__),"workload":receipt(args.workload),"observations":{},"limits":work["limits"]}
    api = c.CDLL(str(library),mode=os.RTLD_LOCAL|os.RTLD_NOW); P = c.c_void_p; I = c.c_int; L = c.c_int64
    signatures = {
        "libversion_number":(I,[]),"open_v2":(I,[c.c_char_p,c.POINTER(P),I,c.c_char_p]),"close":(I,[P]),"errmsg":(P,[P]),
        "prepare_v2":(I,[P,P,I,c.POINTER(P),c.POINTER(P)]),"step":(I,[P]),"finalize":(I,[P]),"reset":(I,[P]),
        "clear_bindings":(I,[P]),"bind_blob":(I,[P,I,P,I,P]),"column_count":(I,[P]),"column_name":(P,[P,I]),
        "column_type":(I,[P,I]),"column_int64":(L,[P,I]),"column_double":(c.c_double,[P,I]),"column_text":(P,[P,I]),
        "column_blob":(P,[P,I]),"column_bytes":(I,[P,I]),"preupdate_hook":(P,[P,P,P]),"preupdate_count":(I,[P]),
        "preupdate_depth":(I,[P]),"preupdate_old":(I,[P,I,c.POINTER(P)]),"preupdate_new":(I,[P,I,c.POINTER(P)]),
        "value_type":(I,[P]),"value_int64":(L,[P]),"value_double":(c.c_double,[P]),"value_text":(P,[P]),
        "value_blob":(P,[P]),"value_bytes":(I,[P]),"session_create":(I,[P,c.c_char_p,c.POINTER(P)]),"session_attach":(I,[P,c.c_char_p]),
        "session_changeset":(I,[P,c.POINTER(I),c.POINTER(P)]),"session_delete":(None,[P]),"free":(None,[P]),
        "changeset_start":(I,[c.POINTER(P),I,P]),"changeset_next":(I,[P]),
        "changeset_op":(I,[P,c.POINTER(P),c.POINTER(I),c.POINTER(I),c.POINTER(I)]),
        "changeset_old":(I,[P,I,c.POINTER(P)]),"changeset_new":(I,[P,I,c.POINTER(P)]),"changeset_finalize":(I,[P]),
        "changeset_apply":(I,[P,I,P,P,P,P]),"backup_init":(P,[P,c.c_char_p,P,c.c_char_p]),"backup_step":(I,[P,I]),
        "backup_remaining":(I,[P]),"backup_pagecount":(I,[P]),"backup_finish":(I,[P])}
    functions = {}
    for name,(result,arguments) in signatures.items():
        symbol = "sqlite3"+name if name.startswith(("session_","changeset_")) else "sqlite3_"+name
        fn = getattr(api,symbol); fn.restype=result;fn.argtypes=arguments;functions[name]=fn
    def call(name,*values): return functions[name](*values)
    def text(address):
        if not address: raise RuntimeError("Expected borrowed string.")
        return c.string_at(address).decode("utf-8")
    def check(code,db,where):
        if code != 0: raise RuntimeError(f"{where}: SQLite {code}: {text(call('errmsg',db))}")
    handles,statements,callback_errors = [],[],[]
    def open_db():
        db=P();check(call("open_v2",b":memory:",c.byref(db),6,None),db,"open");handles.append(db);return db
    def prepare(db,sql):
        data=c.create_string_buffer(sql.encode());stmt=P();tail=P()
        check(call("prepare_v2",db,c.cast(data,P),len(data),c.byref(stmt),c.byref(tail)),db,"prepare")
        if not stmt or tail.value != c.addressof(data)+len(sql.encode()):raise RuntimeError("Expected exactly one statement.")
        statements.append(stmt);return stmt
    def finish(db,stmt):
        code=call("finalize",stmt);statements.remove(stmt);check(code,db,"finalize")
    def value(kind,integer,real,address,count):
        if kind==1:return {"type":"integer","decimal":str(integer())}
        if kind==2:return {"type":"real","binary64Hex":struct.pack(">d",real()).hex()}
        if kind in (3,4):
            pointer=address(kind);n=count()
            if n<0 or n>1024 or n and not pointer:raise RuntimeError("Borrowed test value exceeds finite bounds.")
            return {"type":"text-utf8" if kind==3 else "blob","bytes":n,"hex":c.string_at(pointer,n).hex() if n else ""}
        if kind==5:return {"type":"null"}
        raise RuntimeError("Unexpected value type.")
    def row_value(stmt,index):
        return value(call("column_type",stmt,index),lambda:call("column_int64",stmt,index),lambda:call("column_double",stmt,index),
            lambda kind:call("column_text" if kind==3 else "column_blob",stmt,index),lambda:call("column_bytes",stmt,index))
    def protected_value(pointer):
        if not pointer:return {"type":"undefined"}
        return value(call("value_type",pointer),lambda:call("value_int64",pointer),lambda:call("value_double",pointer),
            lambda kind:call("value_text" if kind==3 else "value_blob",pointer),lambda:call("value_bytes",pointer))
    def read(db,stmt):
        n=call("column_count",stmt);rows=[];columns=[text(call("column_name",stmt,i)) for i in range(n)]
        while True:
            code=call("step",stmt)
            if code==101:break
            if code!=100:check(code,db,"step");raise RuntimeError("Expected ROW or DONE.")
            rows.append([row_value(stmt,i) for i in range(n)])
            if len(rows)>16:raise RuntimeError("Finite row bound exceeded.")
        if callback_errors:raise RuntimeError("Callback contained error: "+callback_errors[0])
        return {"columns":columns,"rows":rows}
    def sql(db,statement):
        stmt=prepare(db,statement)
        try:return read(db,stmt)
        finally:finish(db,stmt)
    def change(iterator):
        table=P();n=I();op=I();indirect=I()
        check(call("changeset_op",iterator,c.byref(table),c.byref(n),c.byref(op),c.byref(indirect)),None,"changeset_op")
        def side(name):
            values=[]
            for i in range(n.value):
                p=P();check(call("changeset_"+name,iterator,i,c.byref(p)),None,"changeset_"+name);values.append(protected_value(p))
            return values
        result={"table":text(table.value),"columns":n.value,"op":op.value,"indirect":indirect.value}
        if op.value in (9,23):result["old"]=side("old")
        if op.value in (18,23):result["new"]=side("new")
        return result
    owners,destructors={},[]
    @c.CFUNCTYPE(None,P)
    def destructor(address):
        try:
            owner=owners.pop(address);destructors.append(owner[0])
        except BaseException as error:callback_errors.append(str(error))
    def bind_owner(db,stmt,entry):
        data=bytes.fromhex(entry["hex"]);owner=c.create_string_buffer(data,len(data)+1);address=c.addressof(owner)
        owners[address]=(entry["id"],owner)
        check(call("bind_blob",stmt,1,address,len(data),c.cast(destructor,P)),db,"bind custom owner")
    session=changeset=iterator=backup=None
    try:
        if call("libversion_number")!=3054000:raise RuntimeError("Expected pinned SQLite3.54.0.")
        owner_db=open_db();stmt=prepare(owner_db,work["ownership"]["sql"]);buffers=work["ownership"]["buffers"]
        bind_owner(owner_db,stmt,buffers[0]);first=read(owner_db,stmt);check(call("reset",stmt),owner_db,"reset");counts=[len(destructors)]
        bind_owner(owner_db,stmt,buffers[1]);counts.append(len(destructors));second=read(owner_db,stmt)
        check(call("reset",stmt),owner_db,"reset");check(call("clear_bindings",stmt),owner_db,"clear");counts.append(len(destructors))
        bind_owner(owner_db,stmt,buffers[2]);finish(owner_db,stmt);counts.append(len(destructors))
        report["observations"]["ownership"]={"first":first,"second":second,"releaseCounts":counts,"destructorIds":list(destructors),"ownersReleased":not owners}
        if callback_errors:raise RuntimeError(callback_errors[0])
        db=open_db();sql(db,work["preupdate"]["setup"]);events=[];ctx=c.c_int(0x417);address=c.addressof(ctx)
        @c.CFUNCTYPE(None,P,P,I,P,P,L,L)
        def preupdate(user,database,op,schema,table,old_key,new_key):
            try:
                if user!=address:raise RuntimeError("preupdate userdata mismatch")
                n=call("preupdate_count",database);event={"schema":text(schema),"table":text(table),"op":op,"columns":n,"depth":call("preupdate_depth",database)}
                for side,enabled in (("old",op in (9,23)),("new",op in (18,23))):
                    if not enabled:continue
                    event[side+"Rowid"]=old_key if side=="old" else new_key
                    values=[]
                    for i in range(n):
                        p=P();check(call("preupdate_"+side,database,i,c.byref(p)),database,"preupdate value");values.append(protected_value(p))
                    event[side]=values
                events.append(event)
            except BaseException as error:callback_errors.append(str(error))
        previous=call("preupdate_hook",db,c.cast(preupdate,P),address)
        try:
            for statement in work["preupdate"]["actions"]:sql(db,statement)
        finally:returned=call("preupdate_hook",db,None,None)
        before=len(events);sql(db,work["preupdate"]["afterUnregister"])
        report["observations"]["preupdate"]={"initialPreviousNull":not previous,"unregisteredContextMatches":returned==address,
            "events":events,"eventsAfterUnregister":len(events)-before,"result":sql(db,work["preupdate"]["read"])}
        source,target=open_db(),open_db()
        for database in (source,target):
            for statement in work["session"]["setup"]:sql(database,statement)
        session=P();check(call("session_create",source,b"main",c.byref(session)),source,"session_create")
        check(call("session_attach",session,work["session"]["table"].encode()),source,"session_attach")
        for statement in work["session"]["actions"]:sql(source,statement)
        size=I();changeset=P();check(call("session_changeset",session,c.byref(size),c.byref(changeset)),source,"session_changeset")
        if not changeset or size.value<=0 or size.value>4096:raise RuntimeError("Invalid finite changeset.")
        changeset_hex=c.string_at(changeset,size.value).hex();iterator=P();check(call("changeset_start",c.byref(iterator),size,changeset),source,"changeset_start");changes=[]
        while True:
            code=call("changeset_next",iterator)
            if code==101:break
            if code!=100:check(code,source,"changeset_next");raise RuntimeError("Expected iterator ROW/DONE.")
            changes.append(change(iterator))
            if len(changes)>8:raise RuntimeError("Finite changes bound exceeded.")
        check(call("changeset_finalize",iterator),source,"changeset_finalize");iterator=None
        apply_events=[];apply_context=c.c_int(0);apply_address=c.addressof(apply_context)
        @c.CFUNCTYPE(I,P,P)
        def table_filter(user,table):
            try:
                if user!=apply_address:raise RuntimeError("filter userdata mismatch")
                apply_events.append({"pass":apply_context.value,"kind":"filter","table":text(table)});return 1
            except BaseException as error:callback_errors.append(str(error));return 0
        @c.CFUNCTYPE(I,P,I,P)
        def conflict(user,reason,item):
            try:
                if user!=apply_address:raise RuntimeError("conflict userdata mismatch")
                apply_events.append({"pass":apply_context.value,"kind":"conflict","reason":reason,"change":change(item)});return 0
            except BaseException as error:callback_errors.append(str(error));return 2
        applied=[]
        for pass_number in (1,2):
            apply_context.value=pass_number
            check(call("changeset_apply",target,size,changeset,c.cast(table_filter,P),c.cast(conflict,P),apply_address),target,"changeset_apply")
            applied.append(sql(target,work["session"]["read"]))
        report["observations"]["session"]={"changesetBytes":size.value,"changesetHex":changeset_hex,"changes":changes,
            "applyEvents":apply_events,"source":sql(source,work["session"]["read"]),"applied":applied}
        call("session_delete",session);session=None;call("free",changeset);changeset=None
        destination=open_db();backup=call("backup_init",destination,b"main",source,b"main")
        if not backup:raise RuntimeError("backup_init returned no handle")
        steps=[]
        for _ in range(work["backup"]["maximumSteps"]):
            code=call("backup_step",backup,work["backup"]["pagesPerStep"])
            steps.append({"code":code,"remaining":call("backup_remaining",backup),"pageCount":call("backup_pagecount",backup)})
            if code==101:break
            if code!=0:check(code,destination,"backup_step")
        else:raise RuntimeError("Finite backup step cap exceeded")
        check(call("backup_finish",backup),destination,"backup_finish");backup=None
        report["observations"]["backup"]={"steps":steps,"result":sql(destination,work["session"]["read"]),"integrity":sql(destination,work["backup"]["integritySql"])}
        if callback_errors or owners:raise RuntimeError("A callback failed or custom ownership leaked")
        report["status"]="passed-native-api-callbacks"
    except BaseException as error:
        report["error"]=str(error);raise
    finally:
        if backup:call("backup_finish",backup)
        if iterator:call("changeset_finalize",iterator)
        if session:call("session_delete",session)
        if changeset:call("free",changeset)
        for stmt in statements:call("finalize",stmt)
        codes=[call("close",db) for db in reversed(handles)]
        report["resourcesReleased"]=not owners and all(code==0 for code in codes)
        report["callbackErrors"]=callback_errors
        if not report["resourcesReleased"]:report["status"]="incomplete-native-resource-release"
        loaded=sorted({parts[-1] for line in Path('/proc/self/maps').read_text().splitlines() if len(parts:=line.split())>=6 and parts[-1].startswith('/') and '.so' in parts[-1]})
        provider_ok=[path for path in loaded if 'sqlite' in Path(path).name.lower()]==[str(library.resolve())]
        report["nativeProviderAuditPassed"]=provider_ok
        if not provider_ok:report["status"]="incomplete-native-provider-audit"
        report["loadedLibraries"]=[receipt(path) for path in loaded]
        args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps(report,indent=2)+"\n")
    print(json.dumps({"status":report["status"],"report":receipt(args.output)}))
    return 0 if report["status"]=="passed-native-api-callbacks" else 1


if __name__=="__main__":raise SystemExit(main())
