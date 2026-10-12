#!/usr/bin/env python3
"""Selected dependent temporary initialization is source metadata, without runtime ownership."""
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
        defaults=db.execute('''SELECT t.name AS template_name,f.* FROM templates t JOIN template_formals f
            ON f.template_id=t.id WHERE t.name IN('BracedInitializer','EmptyInitializer') AND f.kind='constant' ''').fetchall()
        assert {row['template_name'] for row in defaults}=={'BracedInitializer','EmptyInitializer'}
        selected=[]
        for default in defaults:
            assert default['default_value_text'] is None and default['default_expression_id'] is not None, dict(default)
            pending=[default['default_expression_id']];seen=set();actions=[]
            while pending:
                node_id=pending.pop()
                if node_id in seen:continue
                seen.add(node_id)
                node=db.execute('SELECT * FROM nodes WHERE id=?',(node_id,)).fetchone()
                assert node['domain']=='template-recipe' and node['function_id'] is None, dict(node)
                assert node['initialization_id'] is None and node['lifetime_id'] is None, dict(node)
                if node['kind']=='source-initialization':actions.append(node)
                pending.extend(row[0] for row in db.execute('SELECT child_id FROM edges WHERE parent_id=?',(node_id,)))
            assert actions, (dict(default),seen)
            for action in actions:
                assert action['operator'] in ('constructor','value-constructor','zero','none'), dict(action)
                assert action['value_category'] is None and action['symbol_id'] is None, dict(action)
                assert action['file_id'] is not None and action['line']>0 and action['column']>0, dict(action)
                target=db.execute('SELECT * FROM types WHERE id=?',(action['type_id'],)).fetchone()
                assert target and target['is_dependent'], dict(action)
                owner=db.execute('''SELECT n.* FROM edges e JOIN nodes n ON n.id=e.parent_id
                    WHERE e.child_id=? AND e.role='initializer' ''',(action['id'],)).fetchone()
                assert owner and owner['kind']=='temporary' and owner['type_id']==action['type_id'], dict(action)
                args=db.execute('''SELECT e.ordinal,n.* FROM edges e JOIN nodes n ON n.id=e.child_id
                    WHERE e.parent_id=? AND e.role='argument' ORDER BY e.ordinal''',(action['id'],)).fetchall()
                if default['template_name']=='BracedInitializer':
                    assert len(args)==1 and args[0]['ordinal']==0 and args[0]['constant_text']=='1', [dict(row) for row in args]
                else:assert not args, [dict(row) for row in args]
                selected.append(action)
        assert not db.execute('SELECT 1 FROM initializations').fetchone(), 'Recipe actions acquired executable ownership'
        assert not db.execute('''SELECT 1 FROM lifetimes l JOIN nodes n ON n.id=l.owner_node_id
            WHERE n.domain='template-recipe' ''').fetchone(), 'Recipe actions acquired executable lifetime'
        assert not db.execute("SELECT 1 FROM nodes WHERE kind='source-initialization' AND domain='program'").fetchone()
        for name in ('folded_braced','folded_empty'):
            value=db.execute('''SELECT n.constant_text FROM symbols s JOIN global_initializers g ON g.symbol_id=s.id
                JOIN nodes n ON n.id=g.initializer_node_id WHERE s.name=?''',(name,)).fetchone()
            assert value and value['constant_text']=='1', name
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert len(tables)==44 and not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', *(str(dict(row)) for row in selected)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/source_initializers.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'source-initializers.dotobj'
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
    print('PASS: exact selected dependent temporary initializer/arguments/type, no runtime ownership, '
          'folded concrete execution and native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
