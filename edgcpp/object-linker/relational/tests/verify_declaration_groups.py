#!/usr/bin/env python3
"""Contiguous selected declarator initializations execute once in recorded order."""
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
        assert db.execute('PRAGMA user_version').fetchone()[0]==1025
        summaries=[]
        for function_name,expected in [('scalar_group',('1','2')),('record_group',('3','4')),('failed_group',('3','5'))]:
            function=db.execute("SELECT id FROM symbols WHERE kind='function' AND name=? AND is_definition=1",
                                (function_name,)).fetchone()
            assert function, function_name
            groups=db.execute('''SELECT n.id FROM nodes n WHERE n.function_id=? AND n.kind='decl'
                AND (SELECT count(*) FROM edges e WHERE e.parent_id=n.id AND e.role='declaration')=2''',
                (function['id'],)).fetchall()
            assert len(groups)==1, (function_name,groups)
            declarations=db.execute('''SELECT e.ordinal,n.*,s.name FROM edges e JOIN nodes n ON n.id=e.child_id
                JOIN symbols s ON s.id=n.symbol_id WHERE e.parent_id=? AND e.role='declaration' ORDER BY e.ordinal''',
                (groups[0]['id'],)).fetchall()
            assert [row['name'] for row in declarations]==['first','second'], [dict(row) for row in declarations]
            actions=[]
            for ordinal,declaration in enumerate(declarations):
                assert declaration['ordinal']==ordinal and declaration['kind']=='initialize', dict(declaration)
                action=db.execute('SELECT * FROM initializations WHERE id=?',(declaration['initialization_id'],)).fetchone()
                assert action and action['variable_symbol_id']==declaration['symbol_id'], dict(declaration)
                assert action['type_id']==declaration['type_id'] and action['function_id']==function['id'], dict(action)
                assert db.execute('SELECT count(*) FROM nodes WHERE function_id=? AND initialization_id=?',
                                  (function['id'],action['id'])).fetchone()[0]==1, dict(action)
                if function_name=='scalar_group':
                    assert action['kind']=='expression' and action['destructor_symbol_id'] is None, dict(action)
                    pending=[action['value_node_id']]
                else:
                    assert action['kind']=='constructor' and action['constructor_symbol_id'] is not None
                    assert action['destructor_symbol_id'] is not None, dict(action)
                    pending=[row[0] for row in db.execute('SELECT node_id FROM initialization_arguments WHERE initialization_id=? ORDER BY ordinal',
                                                        (action['id'],))]
                    assert len(pending)==1, dict(action)
                    assert db.execute('SELECT 1 FROM lifetime_destructions WHERE initialization_id=?',(action['id'],)).fetchone()
                values=[];seen=set()
                while pending:
                    node_id=pending.pop()
                    if node_id in seen:continue
                    seen.add(node_id)
                    node=db.execute('SELECT * FROM nodes WHERE id=?',(node_id,)).fetchone()
                    assert node['domain']=='program' and node['function_id']==function['id'], dict(node)
                    if node['constant_text'] is not None:values.append(node['constant_text'])
                    pending.extend(row[0] for row in db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='operand'",(node_id,)))
                assert expected[ordinal] in values, (function_name,ordinal,values)
                actions.append(action['id'])
            assert len(set(actions))==2
            summaries.append(f'{function_name}: group {groups[0][0]}, ordered actions {actions}')
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', *summaries])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/declaration_groups.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'declaration-groups.dotobj'
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
    print('PASS: exact contiguous declaration/action identities/order, once-only effects, '
          'successful/failing constructor cleanup and native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
