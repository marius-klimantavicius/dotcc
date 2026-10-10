#!/usr/bin/env python3
"""Reseal an array cleanup recipe using a pointer to a zero-extent array."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import resource
import shutil
import sqlite3
import subprocess


def zero_stride_recipe(source, target, packager):
    shutil.copy2(source, target)
    with sqlite3.connect(target) as db:
        db.row_factory = sqlite3.Row
        action = db.execute('SELECT a.target_node_id,v.element_type_id FROM cleanup_actions a JOIN cleanup_arrays v ON v.action_id=a.id ORDER BY a.id LIMIT 1').fetchone()
        root = dict(db.execute('SELECT * FROM nodes WHERE id=?', (action['target_node_id'],)).fetchone())
        node_id = db.execute('SELECT max(id)+1 FROM nodes').fetchone()[0]
        type_id = db.execute('SELECT max(id)+1 FROM types').fetchone()[0]
        integer = db.execute("SELECT id FROM types WHERE kind='i32'").fetchone()[0]
        db.execute("INSERT INTO types VALUES(?,'array',NULL,?,0)", (type_id, action['element_type_id']))
        db.execute("INSERT INTO types VALUES(?,'pointer',?,NULL,NULL)", (type_id+1, type_id))
        def insert(values):
            db.execute('INSERT INTO nodes VALUES('+','.join('?' for _ in values)+')', tuple(values.values()))
        clone = dict(root, id=node_id)
        insert(clone)
        db.execute('UPDATE edges SET parent_id=? WHERE parent_id=?', (node_id, root['id']))
        db.execute("UPDATE nodes SET kind='convert',evaluation_order='left-to-right' WHERE id=?", (root['id'],))
        cast = dict(root, id=node_id+1, kind='convert', type_id=type_id+1, evaluation_order='left-to-right')
        add = dict(root, id=node_id+2, kind='binary', type_id=type_id+1, operator='pointer-add', evaluation_order='left-to-right')
        zero = dict(root, id=node_id+3, kind='constant', type_id=integer, constant_text='0', evaluation_order='none')
        for node in (cast, add, zero): insert(node)
        for parent, role, child in ((root['id'],'operand',add['id']), (cast['id'],'operand',clone['id']),
                                    (add['id'],'left',cast['id']), (add['id'],'right',zero['id'])):
            db.execute('INSERT INTO edges VALUES(?,?,0,?)', (parent, role, child))
        assert not list(db.execute('PRAGMA foreign_key_check'))
        db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
        db.commit()


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ('object','linker','packager','output-dir'): parser.add_argument('--'+name,type=Path,required=True)
    args=parser.parse_args(); out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    spec=importlib.util.spec_from_file_location('dotobj_packager',args.packager.resolve())
    packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
    malformed=out/'zero-stride.dotobj';zero_stride_recipe(args.object.resolve(),malformed,packager)
    command=['dotnet',str(args.linker.resolve()),'verify',str(malformed)]
    result=subprocess.run(command,capture_output=True,timeout=60,preexec_fn=lambda:resource.setrlimit(resource.RLIMIT_CORE,(0,0)))
    (out/'verify.stdout').write_bytes(result.stdout);(out/'verify.stderr').write_bytes(result.stderr)
    expected=b'Array cleanup offset is outside its owned span'
    passed=result.returncode==1 and expected in result.stderr and b'DivideByZeroException' not in result.stderr
    receipt=dict(status='passed' if passed else 'failed',command=command,exitCode=result.returncode,
                 input=dict(path=str(args.object.resolve()),sha256=hashlib.sha256(args.object.read_bytes()).hexdigest()),
                 linker=dict(path=str(args.linker.resolve()),sha256=hashlib.sha256(args.linker.read_bytes()).hexdigest()),
                 checks=['SQL-valid resealed zero-extent pointer recipe','typed semantic rejection without arithmetic crash'] if passed else [])
    (out/'results.json').write_text(json.dumps(receipt,indent=2)+'\n');print(json.dumps(receipt))
    return 0 if passed else 1
if __name__=='__main__':raise SystemExit(main())
