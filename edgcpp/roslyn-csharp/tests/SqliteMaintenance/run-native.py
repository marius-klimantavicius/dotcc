#!/usr/bin/env python3
"""Separate pinned native oracle for the finite snapshot/RBU maintenance slice."""
from __future__ import annotations
import argparse
import ctypes as c
import hashlib
import json
import os
from pathlib import Path
import struct

OBJECT_SHA = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
ORACLE_SHA = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a"


def receipt(path):
    path = Path(path)
    with path.open("rb") as stream: sha = hashlib.file_digest(stream, "sha256").hexdigest()
    return {"path":str(path.resolve()),"sha256":sha,"bytes":path.stat().st_size}


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build-receipt",type=Path,required=True)
    parser.add_argument("--workload",type=Path,default=Path(__file__).with_name("workload.json"))
    parser.add_argument("--artifacts",type=Path,required=True)
    args=parser.parse_args()
    if args.artifacts.exists(): parser.error("Choose a fresh artifact directory.")
    build=json.loads(args.build_receipt.read_text()); library=Path(build["library"]["path"])
    if (build["role"]!="separate-native-test-oracle-only" or build["objectSha256"]!=OBJECT_SHA
        or receipt(library)!=build["library"] or build["library"]["sha256"]!=ORACLE_SHA):
        raise RuntimeError("Pinned native SQLite oracle identity/role changed.")
    work=json.loads(args.workload.read_text())
    if work["workloadVersion"]!=1: raise RuntimeError("Unsupported workload.")
    args.artifacts.mkdir(parents=True); root=args.artifacts.resolve(); report_path=root/"native-maintenance.json"
    report={"status":"incomplete-native-maintenance","role":"separate-native-test-oracle-only","generatedExecution":False,
        "nativeApplicationFallbackPermitted":False,"library":receipt(library),"buildReceipt":receipt(args.build_receipt),
        "runner":receipt(__file__),"workload":receipt(args.workload),"observations":{},"limits":work["limits"]}
    api=c.CDLL(str(library),mode=os.RTLD_LOCAL|os.RTLD_NOW); P=c.c_void_p; I=c.c_int; L=c.c_int64
    specs={"sqlite3_libversion_number":(I,[]),"sqlite3_open_v2":(I,[c.c_char_p,c.POINTER(P),I,c.c_char_p]),
        "sqlite3_close":(I,[P]),"sqlite3_errmsg":(P,[P]),"sqlite3_prepare_v2":(I,[P,P,I,c.POINTER(P),c.POINTER(P)]),
        "sqlite3_step":(I,[P]),"sqlite3_finalize":(I,[P]),"sqlite3_column_count":(I,[P]),"sqlite3_column_name":(P,[P,I]),
        "sqlite3_column_type":(I,[P,I]),"sqlite3_column_int64":(L,[P,I]),"sqlite3_column_double":(c.c_double,[P,I]),
        "sqlite3_column_text":(P,[P,I]),"sqlite3_column_blob":(P,[P,I]),"sqlite3_column_bytes":(I,[P,I]),"sqlite3_free":(None,[P]),
        "sqlite3_snapshot_get":(I,[P,c.c_char_p,c.POINTER(P)]),"sqlite3_snapshot_open":(I,[P,c.c_char_p,P]),
        "sqlite3_snapshot_recover":(I,[P,c.c_char_p]),"sqlite3_snapshot_cmp":(I,[P,P]),"sqlite3_snapshot_free":(None,[P]),
        "sqlite3rbu_open":(P,[c.c_char_p,c.c_char_p,c.c_char_p]),"sqlite3rbu_step":(I,[P]),"sqlite3rbu_savestate":(I,[P]),
        "sqlite3rbu_close":(I,[P,c.POINTER(P)]),"sqlite3rbu_progress":(L,[P]),"sqlite3rbu_state":(I,[P]),
        "sqlite3rbu_bp_progress":(None,[P,c.POINTER(I),c.POINTER(I)]),"sqlite3rbu_db":(P,[P,I])}
    functions={}
    for name,(result,parameters) in specs.items():
        f=getattr(api,name);f.restype=result;f.argtypes=parameters;functions[name]=f
    def call(name,*values): return functions["sqlite3_"+name](*values)
    def rcall(name,*values): return functions["sqlite3rbu_"+name](*values)
    def require(condition,message):
        if not condition: raise RuntimeError(message)
    def text(address):
        require(bool(address),"Missing borrowed string.");return c.string_at(address).decode("utf-8")
    def check(code,db,operation):
        require(code==0,f"{operation}: SQLite {code}: "+(text(call("errmsg",db)) if db else "no database"))
    handles=[]; statements=[]; snapshots=[]; rbu=None
    def open_db(path):
        db=P();code=call("open_v2",os.fsencode(path),c.byref(db),6,None)
        if db: handles.append(db)
        check(code,db,"open");require(bool(db),"Open returned no database.");return db
    def close_db(db):
        check(call("close",db),db,"close");handles.remove(db)
    def sql(db,statement):
        encoded=statement.encode();data=c.create_string_buffer(encoded);stmt=P();tail=P()
        code=call("prepare_v2",db,c.cast(data,P),len(data),c.byref(stmt),c.byref(tail))
        if stmt: statements.append((db,stmt))
        check(code,db,"prepare");require(bool(stmt) and tail.value==c.addressof(data)+len(encoded),"Expected exactly one SQL statement.")
        try:
            count=call("column_count",stmt);require(0<=count<=8,"Finite column bound.")
            columns=[text(call("column_name",stmt,i)) for i in range(count)];rows=[]
            while True:
                code=call("step",stmt)
                if code==101: break
                if code!=100: check(code,db,"step");raise RuntimeError("Expected ROW or DONE.")
                values=[]
                for i in range(count):
                    kind=call("column_type",stmt,i)
                    if kind==1: value={"type":"integer","decimal":str(call("column_int64",stmt,i))}
                    elif kind==2: value={"type":"real","binary64Hex":struct.pack(">d",call("column_double",stmt,i)).hex()}
                    elif kind in (3,4):
                        address=call("column_text" if kind==3 else "column_blob",stmt,i);n=call("column_bytes",stmt,i)
                        require(0<=n<=1024 and (not n or address),"Finite borrowed value.")
                        value={"type":"text-utf8" if kind==3 else "blob","bytes":n,"hex":c.string_at(address,n).hex() if n else ""}
                    else: require(kind==5,"Known value storage class.");value={"type":"null"}
                    values.append(value)
                rows.append(values);require(len(rows)<=32,"Finite row bound.")
            return {"columns":columns,"rows":rows}
        finally:
            statements.remove((db,stmt));check(call("finalize",stmt),db,"finalize")
    def setup(db,items):
        for statement in items: sql(db,statement)
    def snapshot_get(db):
        result=P();check(call("snapshot_get",db,b"main",c.byref(result)),db,"snapshot_get")
        require(bool(result),"Missing snapshot handle.");snapshots.append(result);return result
    def snapshot_phase():
        w=work["snapshot"];path=root/"snapshot.db";writer=open_db(path);setup(writer,w["setup"])
        pin=open_db(path);sql(pin,"BEGIN");old=snapshot_get(pin);old_rows=sql(pin,w["read"])
        setup(writer,w["mutations"]);sql(writer,"BEGIN");new=snapshot_get(writer);new_rows=sql(writer,w["read"]);sql(writer,"COMMIT")
        signs={"oldNew":(call("snapshot_cmp",old,new)>0)-(call("snapshot_cmp",old,new)<0),
            "newOld":(call("snapshot_cmp",new,old)>0)-(call("snapshot_cmp",new,old)<0),"same":call("snapshot_cmp",old,old)}
        require(signs=={"oldNew":-1,"newOld":1,"same":0},"Snapshot chronology/same generation.")
        pinned=sql(pin,w["read"]);replay=open_db(path);sql(replay,w["initializeReplay"])
        check(call("snapshot_recover",replay,b"main"),replay,"snapshot_recover")
        replayed=[]
        for snapshot in (old,new):
            sql(replay,"BEGIN");check(call("snapshot_open",replay,b"main",snapshot),replay,"snapshot_open")
            replayed.append(sql(replay,w["read"]));sql(replay,"COMMIT")
        require(pinned==old_rows and replayed==[old_rows,new_rows] and old_rows!=new_rows,"Retained historical visibility.")
        for snapshot in (old,new): snapshots.remove(snapshot);call("snapshot_free",snapshot)
        sql(pin,"COMMIT");close_db(replay);close_db(pin)
        observation={"old":old_rows,"new":new_rows,"pinnedAfterWrite":pinned,"comparisonSigns":signs,"recoverCode":0,
            "replayed":replayed,"checkpointAfterRelease":sql(writer,w["checkpoint"]),"integrity":sql(writer,w["integrity"]),"snapshotsFreed":2}
        close_db(writer);return observation
    def rbu_state():
        state=rcall("state",rbu);progress=rcall("progress",rbu);one=I();two=I();rcall("bp_progress",rbu,c.byref(one),c.byref(two))
        require(1<=state<=4 and progress>=0 and -1<=one.value<=10000 and 0<=two.value<=10000,"RBU documented state/progress bounds.")
        return {"state":state,"progress":str(progress),"stageOne":one.value,"stageTwo":two.value}
    def rbu_close(expected):
        nonlocal rbu
        owned=rbu;rbu=None;message=P();code=rcall("close",owned,c.byref(message))
        try: require(code==expected,"rbu_close "+str(code)+(": "+text(message) if message else ""))
        finally:
            if message: call("free",message)
        return code
    def rbu_phase():
        nonlocal rbu
        w=work["rbu"];target=root/"target.db";update=root/"update.db";state=root/"rbu-state.db"
        db=open_db(target);setup(db,w["targetSetup"]);before=sql(db,w["read"]);close_db(db)
        db=open_db(update);setup(db,w["updateSetup"]);close_db(db)
        def begin():
            nonlocal rbu
            rbu=rcall("open",os.fsencode(target),os.fsencode(update),os.fsencode(state));require(bool(rbu),"RBU handle allocation.")
            require(bool(rcall("db",rbu,0)) and bool(rcall("db",rbu,1)),"Borrowed RBU-owned connections.")
        begin();opened=rbu_state();paused=[]
        for _ in range(w["maximumPauseSteps"]):
            code=rcall("step",rbu);require(code==0,"Partial RBU step must remain unfinished.")
            entry={"code":code,**rbu_state()};paused.append(entry)
            if int(entry["progress"])>0: break
        require(paused and int(paused[-1]["progress"])>0 and paused[-1]["state"]==1,"Pause after staged work before publication.")
        require(rcall("savestate",rbu)==0,"Save RBU state.");saved=rbu_state();partial_close=rbu_close(0)
        db=open_db(target);unpublished=sql(db,w["read"]);require(unpublished==before,"Unfinished OAL changes remain unpublished.");close_db(db)
        begin();resumed=rbu_state();require(int(resumed["progress"])==int(saved["progress"]),"RBU saved progress restored.")
        steps=[];previous=int(resumed["progress"]);done=False
        for _ in range(w["maximumSteps"]):
            code=rcall("step",rbu);require(code in (0,101),"RBU step success/DONE.")
            entry={"code":code,**rbu_state()};steps.append(entry)
            require(int(entry["progress"])>=previous,"Monotonic RBU progress.");previous=int(entry["progress"])
            if code==101: done=True;break
        require(done and steps[-1]["state"]==4,"RBU completion within finite cap.")
        repeated_done=rcall("step",rbu);require(repeated_done==101,"Completed RBU step remains DONE.")
        complete_close=rbu_close(101);db=open_db(target)
        observation={"before":before,"opened":opened,"pauseSteps":paused,"saved":saved,"partialCloseCode":partial_close,
            "unpublished":unpublished,"resumed":resumed,"completionSteps":steps,"repeatedDoneCode":repeated_done,
            "completeCloseCode":complete_close,"result":sql(db,w["read"]),"indexResult":sql(db,w["indexRead"]),"integrity":sql(db,w["integrity"])}
        close_db(db);return observation
    result=1
    try:
        require(call("libversion_number")==3054000,"Pinned SQLite version.")
        report["observations"]["snapshot"]=snapshot_phase();report["observations"]["rbu"]=rbu_phase()
        report["status"]="passed-native-maintenance";result=0
    except Exception as error: report["error"]=repr(error)
    finally:
        errors=[]
        if rbu:
            try: rbu_close(0)
            except Exception as error: errors.append(repr(error))
        for snapshot in list(snapshots): snapshots.remove(snapshot);call("snapshot_free",snapshot)
        for db,stmt in list(statements):
            statements.remove((db,stmt));code=call("finalize",stmt)
            if code: errors.append("cleanup finalize "+str(code))
        for db in list(reversed(handles)):
            code=call("close",db)
            if code: errors.append("cleanup close "+str(code))
            else: handles.remove(db)
        report["resourcesReleased"]=not errors and not handles and not statements and not snapshots and not rbu
        report["cleanupErrors"]=errors;report["files"]=[receipt(p) for p in sorted(root.iterdir()) if p.is_file()]
        if errors or not report["resourcesReleased"]: result=1;report["status"]="incomplete-native-maintenance"
        report_path.write_text(json.dumps(report,indent=2)+"\n")
    print(json.dumps({"status":report["status"],"report":receipt(report_path)}));return result


if __name__=="__main__": raise SystemExit(main())
