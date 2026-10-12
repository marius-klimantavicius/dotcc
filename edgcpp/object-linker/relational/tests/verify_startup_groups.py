#!/usr/bin/env python3
"""Selected startup groups own ordered source actions independently of static data."""
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
        assert db.execute('PRAGMA user_version').fetchone()[0]==1027
        groups=db.execute('SELECT * FROM startup_groups ORDER BY ordinal').fetchall()
        assert len(groups)==1 and groups[0]['ordinal']==0 and groups[0]['init_priority']==0
        group=groups[0]
        assert not group['is_thread_local']
        function=db.execute('SELECT * FROM functions WHERE symbol_id=?',(group['function_symbol_id'],)).fetchone()
        assert function and function['kind']=='ordinary' and function['receiver_symbol_id'] is None
        statements=db.execute("SELECT e.ordinal,n.*,s.name AS target_name,s.kind AS target_kind FROM edges e "
            "JOIN nodes n ON n.id=e.child_id JOIN symbols s ON s.id=n.symbol_id "
            "WHERE e.parent_id=? AND e.role='statement' ORDER BY e.ordinal",(function['body_node_id'],)).fetchall()
        assert [row['ordinal'] for row in statements]==[0,1,2]
        assert [row['target_name'] for row in statements]==['first','middle','second']
        actions=[]
        for statement in statements:
            assert statement['kind']=='initialize' and statement['domain']=='program'
            assert statement['target_kind']=='global' and statement['function_id']==group['function_symbol_id']
            action=db.execute('SELECT * FROM initializations WHERE id=?',(statement['initialization_id'],)).fetchone()
            assert action and action['function_id']==group['function_symbol_id']
            assert action['variable_symbol_id']==statement['symbol_id'] and action['type_id']==statement['type_id']
            assert action['destructor_symbol_id'] is None and action['init_expr_lifetime_id'] is None
            assert not db.execute('SELECT 1 FROM global_initializers WHERE symbol_id=?',(statement['symbol_id'],)).fetchone()
            assert statement['line'] and statement['column'] and statement['file_id'] is not None
            assert action['value_node_id'] is not None
            value=db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='value'",(statement['id'],)).fetchone()
            assert value and value[0]==action['value_node_id']
            actions.append(action)
        assert [row['kind'] for row in actions]==['aggregate','expression','aggregate'], [dict(row) for row in actions]
        constructors=[]
        for action in (actions[0],actions[2]):
            elements=db.execute('SELECT * FROM array_initializer_elements WHERE initializer_node_id=? ORDER BY ordinal',
                                (action['value_node_id'],)).fetchall()
            assert len(elements)==2
            for element in elements:
                node=db.execute('SELECT * FROM nodes WHERE id=?',(element['value_node_id'],)).fetchone()
                selected=db.execute('SELECT * FROM initializations WHERE id=?',(node['initialization_id'],)).fetchone()
                assert selected and selected['kind']=='constructor' and selected['constructor_symbol_id'] is not None
                constructors.append(selected['id'])
        assert len(set(constructors))==4
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', str(dict(group)),
                          *(str(dict(row)) for row in statements)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/startup_groups.cpp',source)
    compiler=args.compiler.resolve()
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(path.read_bytes()).hexdigest()+'  '+str(path) for path in
        (compiler,source,RELATIONAL/'compile.py',Path(__file__).resolve()))+'\n')
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'startup-groups.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--std','c++17','--output',obj,'--work-dir',work/'compile-work',
         '--compiler',compiler],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    print('PASS: native selected startup order and exact mixed record-array/scalar action ownership')
    print(work)


if __name__=='__main__':main()
