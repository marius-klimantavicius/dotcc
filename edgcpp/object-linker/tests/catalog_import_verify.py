#!/usr/bin/env python3
"""Verify import receipts, exact dependency projection and released per-unit graphs.
No frontend or resident whole-program linker is run by this test.
"""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--driver',type=Path,required=True)
    p.add_argument('--packager',type=Path,required=True)
    p.add_argument('--inputs',type=Path,required=True,help='JSON array of {path,provider}')
    p.add_argument('--output',type=Path,required=True)
    a=p.parse_args(); out=a.output.resolve();out.mkdir(parents=True,exist_ok=False)
    inputs=json.loads(a.inputs.read_text());checks=[];commands=[];error=None
    spec=importlib.util.spec_from_file_location('packager',a.packager.resolve())
    packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
    def check(name,yes):
        if not yes:raise AssertionError(name)
        checks.append(name)
    def run(name,items,rejected=False):
        target=out/(name+'.sqlite'); request=out/(name+'.request.json')
        request.write_text(json.dumps(dict(catalog=str(target),inputs=items,auditLifetimes=True),indent=2)+'\n')
        env=dict(os.environ,DOTNET_GCHeapHardLimit='0x80000000')
        command=['dotnet',str(a.driver.resolve()),'catalog','--out',str(target),'--audit-object-lifetimes']
        for item in items:
            if item['provider']:command.append('--provider')
            command.append(item['path'])
        result=subprocess.run(command,capture_output=True,timeout=600,env=env)
        (out/(name+'.stdout')).write_bytes(result.stdout);(out/(name+'.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name,command=command,exitCode=result.returncode))
        check(name,result.returncode!=0 if rejected else result.returncode==0)
        return target,result
    try:
        check('nonempty-inputs',len(inputs)>0)
        for item in inputs:item['path']=str(Path(item['path']).resolve())
        catalog,positive=run('import',inputs)
        events=[json.loads(line) for line in positive.stdout.splitlines()]
        check('every-object-released',events[-2]['auditedReleasedObjects']==len(inputs))
        check('explicit-import-only-status',events[-1]['status']=='imported-not-resolved')
        with sqlite3.connect(catalog) as db:
            check('catalog-foreign-keys',not list(db.execute('PRAGMA foreign_key_check')))
            check('catalog-integrity',db.execute('PRAGMA integrity_check').fetchone()==('ok',))
            check('unit-count',db.execute('SELECT count(*) FROM units').fetchone()[0]==len(inputs))
            check('no-semantic-source-blob-duplication',not db.execute("SELECT name FROM sqlite_schema WHERE name IN ('nodes','source_files','source_expressions')").fetchall())
            for ordinal,item in enumerate(inputs):
                unit=ordinal+1;path=Path(item['path'])
                with sqlite3.connect(path) as source:
                    receipt=db.execute('SELECT path,file_sha256,byte_length,logical_sha256,provider,input_ordinal,function_count,verified_body_count FROM units WHERE id=?',(unit,)).fetchone()
                    funcs=source.execute('SELECT count(*) FROM functions').fetchone()[0]
                    check(f'unit-{unit}-receipt',receipt==(str(path),hashlib.file_digest(path.open('rb'),'sha256').hexdigest(),path.stat().st_size,source.execute('SELECT sha256 FROM content_digest').fetchone()[0],int(item['provider']),ordinal,funcs,funcs))
                    functions=dict((row[0],row[1:]) for row in db.execute('SELECT entity_id,symbol_key,is_weak,definition FROM functions WHERE unit_id=?',(unit,)))
                    storages=dict(db.execute('SELECT storage_id,symbol_key FROM storages WHERE unit_id=?',(unit,)))
                    expected=set()
                    def function(kind,owner,target,origin,origin_id):
                        key,weak,definition=functions[target]
                        expected.add((kind,owner,'function',key,target,int(weak and definition=='declaration'),origin,origin_id))
                    def storage(kind,owner,target,origin,origin_id):
                        expected.add((kind,owner,'storage',storages[target],target,0,origin,origin_id))
                    for id,owner,entity,static in source.execute('SELECT id,function_id,entity_id,storage_id FROM nodes'):
                        if entity is not None:function('function',owner,entity,'body-node',id)
                        if static is not None:storage('function',owner,static,'body-node',id)
                    for table,origin in [('cleanup_actions','cleanup'),('allocation_cleanup_actions','allocation-cleanup')]:
                        for id,owner,callback in source.execute(f'SELECT id,function_entity_id,callback_entity_id FROM {table}'):
                            function('function',owner,callback,origin,id)
                    for id,owner,callback in source.execute('SELECT t.node_id,n.function_id,t.destructor_entity_id FROM throw_expressions t JOIN nodes n ON n.id=t.node_id WHERE t.destructor_entity_id IS NOT NULL'):
                        function('function',owner,callback,'throw-destructor',id)
                    for owner,offset,target_fn,target_storage in source.execute('SELECT storage_id,byte_offset,target_entity_id,target_storage_id FROM relocations'):
                        if target_fn is not None:function('storage',owner,target_fn,'relocation',offset)
                        else:storage('storage',owner,target_storage,'relocation',offset)
                    actual=set(db.execute('SELECT from_kind,from_local_id,to_kind,target_key,target_local_id,is_weak,origin,origin_id FROM dependencies WHERE unit_id=?',(unit,)))
                    check(f'unit-{unit}-exact-dependencies',actual==expected)
                    for table,catalog_table in [('entities','functions'),('storages','storages')]:
                        check(f'unit-{unit}-{table}-count',source.execute(f'SELECT count(*) FROM {table}').fetchone()[0]==db.execute(f'SELECT count(*) FROM {catalog_table} WHERE unit_id=?',(unit,)).fetchone()[0])
                    for source_table,kind in [('record_types','record')]:
                        check(f'unit-{unit}-{kind}-contracts',source.execute(f'SELECT count(*) FROM {source_table}').fetchone()[0]==db.execute('SELECT count(*) FROM facts WHERE unit_id=? AND kind=?',(unit,kind)).fetchone()[0])
                    check(f'unit-{unit}-startup',list(source.execute('SELECT ordinal,priority,entity_id FROM initialization_actions ORDER BY ordinal'))==list(db.execute('SELECT ordinal,priority,entity_id FROM startup WHERE unit_id=? ORDER BY ordinal',(unit,))))
            check('all-contract-hashes',all(hashlib.sha256(payload.encode()).hexdigest()==digest for digest,payload in db.execute('SELECT digest,payload FROM contracts')))
        before=hashlib.sha256(catalog.read_bytes()).hexdigest()
        collision=subprocess.run(['dotnet',str(a.driver.resolve()),'catalog','--out',str(catalog),inputs[0]['path']],capture_output=True,timeout=120)
        (out/'existing-path.stderr').write_bytes(collision.stderr)
        check('existing-catalog-not-overwritten',collision.returncode!=0 and hashlib.sha256(catalog.read_bytes()).hexdigest()==before)
        duplicate,_=run('reject-duplicate-unit',[inputs[0],inputs[0]],True)
        with sqlite3.connect(duplicate) as db:
            check('duplicate-rollback-preserves-first',db.execute('SELECT count(*) FROM units').fetchone()==(1,) and db.execute('SELECT status FROM catalog_info').fetchone()==('importing',))
        def mutated(name,sql,params=()):
            bad=out/(name+'.dotobj');shutil.copy2(inputs[0]['path'],bad)
            with sqlite3.connect(bad) as db:
                changed=db.execute(sql,params).rowcount;check(name+'-mutation-selected',changed>0)
                db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
            return dict(path=str(bad),provider=False)
        body=mutated('invalid-body',"UPDATE nodes SET kind='invented-catalog-node' WHERE id=(SELECT min(id) FROM nodes)")
        run('reject-invalid-body',[body],True)
        snapshot=mutated('invalid-source-snapshot',"UPDATE source_files SET sha256=? WHERE id=(SELECT min(id) FROM source_files WHERE available=1)",('0'*64,))
        run('reject-invalid-source-snapshot',[snapshot],True)
        bad_second,_=run('reject-second-unit-rollback',[inputs[0],body],True)
        with sqlite3.connect(bad_second) as db:check('failed-second-preserves-first',db.execute('SELECT count(*) FROM units').fetchone()==(1,))
        # Reopening a catalog must verify the exact originally imported bytes,
        # even if a changed object's own logical digest has been resealed.
        reopen_input=out/'reopen.dotobj';shutil.copy2(inputs[0]['path'],reopen_input)
        reopen_catalog,_=run('reopen-import',[dict(path=str(reopen_input),provider=False)])
        maps_command=['dotnet',str(a.driver.resolve()),'catalog-maps','--out',str(out/'reopened-maps'),str(reopen_catalog)]
        maps=subprocess.run(maps_command,capture_output=True,timeout=600)
        (out/'reopened-maps.stderr').write_bytes(maps.stderr)
        check('catalog-reopens-verified-unit-for-maps',maps.returncode==0)
        source_map=json.loads((out/'reopened-maps'/'source-map.json').read_text())
        check('catalog-maps-preserve-typed-graph',len(source_map['objects'])==1 and all(key in source_map['objects'][0] for key in ('edgGraphNodes','edgGraphFields','edgGraphRoots','edgGraphCoverage')))
        with sqlite3.connect(reopen_input) as db:
            db.execute("UPDATE object_info SET unit_id=unit_id||'-changed'")
            db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
        stale=subprocess.run(['dotnet',str(a.driver.resolve()),'catalog-maps','--out',str(out/'changed-input-maps'),str(reopen_catalog)],capture_output=True,timeout=600)
        (out/'changed-input-maps.stderr').write_bytes(stale.stderr)
        check('changed-cached-input-rejected',stale.returncode!=0 and b'bytes changed after import' in stale.stderr)
        wal=out/'live-wal.dotobj';shutil.copy2(inputs[0]['path'],wal);Path(str(wal)+'-wal').write_bytes(b'')
        run('reject-live-wal',[dict(path=str(wal),provider=False)],True)
    except Exception as exc:error=repr(exc)
    (out/'results.json').write_text(json.dumps(dict(status='failed' if error else 'passed',error=error,checks=checks,commands=commands),indent=2)+'\n')
    if error:raise RuntimeError(error)
    print(f'PASS {len(checks)} catalog import checks')

if __name__=='__main__':main()
