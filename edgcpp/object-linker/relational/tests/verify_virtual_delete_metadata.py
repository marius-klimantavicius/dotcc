#!/usr/bin/env python3
"""Inspect exact EDG virtual deleting-entry selection and genuine generated bodies."""
import argparse
import hashlib
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory=sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        assert db.execute('PRAGMA user_version').fetchone()[0]>=1026
        deletes=db.execute("SELECT * FROM nodes WHERE kind='delete' AND operator='virtual-destroy'").fetchall()
        assert len(deletes)==1, [dict(row) for row in deletes]
        delete=deletes[0]
        children={row['role']:row['child_id'] for row in db.execute(
            'SELECT role,child_id FROM edges WHERE parent_id=?',(delete['id'],))}
        assert set(children)=={'operand','destroy','null-check','execution'}, children
        guard=db.execute('SELECT * FROM nodes WHERE id=?',(children['null-check'],)).fetchone()
        assert guard['constant_text'] in ('true','1') and guard['source_node_id']==delete['id']
        action_node=db.execute('SELECT * FROM nodes WHERE id=?',(children['destroy'],)).fetchone()
        action=db.execute('SELECT * FROM initializations WHERE id=?',(action_node['initialization_id'],)).fetchone()
        assert action['kind']=='none' and action['destructor_symbol_id'] is not None
        source=db.execute('SELECT * FROM symbols WHERE id=?',(action['destructor_symbol_id'],)).fetchone()
        assert source['name']=='~Base', dict(source)
        dispatch=db.execute('SELECT * FROM virtual_calls WHERE node_id=?',(children['execution'],)).fetchone()
        assert dispatch['receiver_node_id']==children['operand'] and dispatch['selected_symbol_id']==source['id']
        assert dispatch['abi_callable_type_id'] is not None and dispatch['vptr_byte_offset']==0
        operands=db.execute("SELECT ordinal,child_id FROM edges WHERE parent_id=? AND role='operand' ORDER BY ordinal",
                            (children['execution'],)).fetchall()
        assert [row['ordinal'] for row in operands]==[0,1] and operands[1]['child_id']==children['operand']
        callee=db.execute('SELECT * FROM nodes WHERE id=?',(operands[0]['child_id'],)).fetchone()
        selected=db.execute('SELECT * FROM symbols WHERE id=?',(callee['symbol_id'],)).fetchone()
        assert selected['id']!=source['id'] and selected['link_name'].endswith('D0Ev'), dict(selected)
        abi=db.execute('SELECT * FROM types WHERE id=?',(dispatch['abi_callable_type_id'],)).fetchone()
        assert abi['id']==selected['type_id'] and abi['parameters_include_receiver']==1
        assert abi['id']!=dispatch['callable_type_id'] and abi['object_type_id'] is not None
        assert db.execute('SELECT count(*) FROM type_parameters WHERE type_id=?',(abi['id'],)).fetchone()[0]==1
        assert db.execute('SELECT count(*) FROM type_parameters WHERE type_id=?',(dispatch['callable_type_id'],)).fetchone()[0]==0
        name=db.execute("SELECT name FROM symbols WHERE id=(SELECT symbol_id FROM nodes WHERE id=?)",
                        (children['operand'],)).fetchone()
        assert name and name['name']=='selected_pointer'
        entries=db.execute("SELECT e.*,s.link_name,s.is_definition FROM virtual_table_entries e "
                           "JOIN symbols s ON s.id=e.abi_entry_symbol_id WHERE e.target_variant='deleting'").fetchall()
        assert entries and all(row['abi_callable_type_id'] is not None for row in entries)
        definitions=[]
        for entry in entries:
            assert entry['link_name'].endswith('D0Ev') and entry['abi_entry_symbol_id']!=entry['target_symbol_id'], dict(entry)
            if not entry['is_definition']:
                continue
            details=db.execute('SELECT * FROM functions WHERE symbol_id=?',(entry['abi_entry_symbol_id'],)).fetchone()
            assert details and details['receiver_symbol_id'] is not None, dict(entry)
            parameters=db.execute('SELECT symbol_id FROM parameters WHERE function_id=? ORDER BY ordinal',
                                  (entry['abi_entry_symbol_id'],)).fetchall()
            assert len(parameters)==1 and parameters[0]['symbol_id']==details['receiver_symbol_id']
            calls=db.execute("SELECT n.id,n.source_node_id,s.link_name,s.name FROM nodes n JOIN symbols s ON s.id=n.symbol_id "
                             "WHERE n.function_id=? AND n.operator='call' ORDER BY n.id",(entry['abi_entry_symbol_id'],)).fetchall()
            assert calls and all(row['source_node_id'] is not None for row in calls), [dict(row) for row in calls]
            if any(row['link_name'].startswith('_ZN7Derived') for row in calls):
                assert len(calls)==2 and calls[0]['link_name'].endswith('D1Ev') and calls[1]['name']=='operator delete', [dict(row) for row in calls]
                definitions.append(entry['abi_entry_symbol_id'])
        assert definitions, 'No genuine Derived D0 body with selected complete destructor + delete'
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', str(dict(delete)), str(dict(dispatch)),
                          *(str(dict(row)) for row in entries)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/virtual_deletes.cpp',source)
    compiler=args.compiler.resolve()
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(path.read_bytes()).hexdigest()+'  '+str(path) for path in
        (compiler,source,RELATIONAL/'compile.py',Path(__file__).resolve()))+'\n')
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'virtual-deletes.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--std','c++17','--output',obj,'--work-dir',work/'compile-work',
         '--compiler',compiler],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    print('PASS: native virtual-delete once/null/chain + exact source/ABI signature, selected slot and genuine D0 body')
    print(work)


if __name__=='__main__':main()
