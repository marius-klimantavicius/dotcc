#!/usr/bin/env python3
"""Cleanup-free selected record constructors initialize their aggregate field directly."""
import argparse
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
        assert db.execute('PRAGMA user_version').fetchone()[0]>=1025
        components=db.execute('''SELECT n.*,e.field_id,e.base_subobject_id,f.type_id AS field_type,f.name AS field_name
            FROM record_initializer_elements e JOIN nodes n ON n.id=e.value_node_id
            JOIN fields f ON f.id=e.field_id JOIN initializations i ON i.id=n.initialization_id
            WHERE f.name='view' AND i.kind='constructor' ''').fetchall()
        assert len(components)==2, [dict(row) for row in components]
        for component in components:
            assert component['kind']=='initialize' and component['domain']=='program', dict(component)
            assert component['base_subobject_id'] is None and component['type_id']==component['field_type'], dict(component)
            action=db.execute('SELECT * FROM initializations WHERE id=?',(component['initialization_id'],)).fetchone()
            assert action['type_id']==component['type_id'] and action['variable_symbol_id'] is None
            assert action['destructor_symbol_id'] is None and action['lifetime_id'] is None and action['init_expr_lifetime_id'] is None
            assert action['destination_initialization_id'] is None and not action['free_on_exception'] and not action['partial_aggregate']
            assert action['sequenced_arguments'] and not action['implied_source'], dict(action)
            constructor=db.execute('SELECT * FROM symbols WHERE id=?',(action['constructor_symbol_id'],)).fetchone()
            assert constructor and constructor['name']=='View' and constructor['is_definition'], dict(action)
            args=db.execute('SELECT ordinal,node_id FROM initialization_arguments WHERE initialization_id=? ORDER BY ordinal',
                            (action['id'],)).fetchall()
            assert [row['ordinal'] for row in args]==[0,1], [dict(row) for row in args]
            for argument,expected in zip(args,('data_argument','size_argument')):
                pending=[argument['node_id']];seen=set();calls=[]
                while pending:
                    node_id=pending.pop()
                    if node_id in seen:continue
                    seen.add(node_id)
                    node=db.execute('SELECT * FROM nodes WHERE id=?',(node_id,)).fetchone()
                    assert node['domain']=='program' and node['function_id']==component['function_id'], dict(node)
                    if node['operator']=='call' and node['symbol_id'] is not None:
                        calls.append(db.execute('SELECT name FROM symbols WHERE id=?',(node['symbol_id'],)).fetchone()[0])
                    pending.extend(row[0] for row in db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='operand'",(node_id,)))
                assert calls==[expected], (expected,calls)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', *(str(dict(row)) for row in components)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/record_constructor_components.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'record-constructor-components.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--output',obj,'--work-dir',work/'compile-work',
         '--compiler',args.compiler.resolve()],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    source.rename(work/'original-source.cpp');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',obj],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: exact cleanup-free record field constructor/ordered once-only arguments, '
          'recorded initialized values and native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
