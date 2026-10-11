#!/usr/bin/env python3
"""Nonreal selected variable declarations remain source metadata, without storage."""
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
        assert db.execute('PRAGMA user_version').fetchone()[0]==1024
        declarations=db.execute('''SELECT n.*,s.name,s.is_definition,s.link_name,s.linkage
            FROM nodes n JOIN symbols s ON s.id=n.symbol_id
            WHERE n.kind='variable-declaration' AND s.kind='source-variable' ORDER BY n.id''').fetchall()
        assert declarations, 'Selected nonreal variable declaration missing'
        operations=set()
        for declaration in declarations:
            assert declaration['name']=='selected_size', dict(declaration)
            assert declaration['domain']=='template-recipe' and declaration['function_id'] is None
            assert declaration['value_category'] is None, dict(declaration)
            assert declaration['type_id'] is not None and declaration['file_id'] is not None
            assert declaration['line']>0 and declaration['column']>0, dict(declaration)
            assert declaration['initialization_id'] is None and declaration['lifetime_id'] is None
            symbol=declaration['symbol_id']
            assert not db.execute('SELECT 1 FROM global_initializers WHERE symbol_id=?',(symbol,)).fetchone()
            assert not db.execute('SELECT 1 FROM functions WHERE symbol_id=?',(symbol,)).fetchone()
            instance=db.execute('''SELECT i.* FROM template_instances i JOIN templates t ON t.id=i.template_id
                WHERE i.symbol_id=? AND t.name='selected_size' ''',(symbol,)).fetchone()
            assert instance and instance['is_dependent'], dict(declaration)
            assert db.execute('SELECT 1 FROM template_arguments WHERE instance_id=? AND kind=?',
                              (instance['id'],'type')).fetchone()
            pending=[row[0] for row in db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='initializer'",
                                                (declaration['id'],))]
            if declaration['operator']=='none':
                assert not declaration['is_definition'] and not pending, dict(declaration)
                continue
            assert declaration['operator']=='constant' and declaration['is_definition'] and pending, dict(declaration)
            seen=set()
            while pending:
                node_id=pending.pop()
                if node_id in seen:continue
                seen.add(node_id)
                node=db.execute('SELECT * FROM nodes WHERE id=?',(node_id,)).fetchone()
                assert node['domain']=='template-recipe' and node['function_id'] is None, dict(node)
                assert node['initialization_id'] is None and node['lifetime_id'] is None, dict(node)
                if node['operator']:operations.add(node['operator'])
                pending.extend(row[0] for row in db.execute('SELECT child_id FROM edges WHERE parent_id=?',(node_id,)))
        assert {'sizeof','add'}<=operations, operations
        assert {row['is_definition'] for row in declarations}=={0,1}
        assert len({row['symbol_id'] for row in declarations})==len(declarations)
        assert not db.execute('''SELECT 1 FROM nodes n JOIN symbols s ON s.id=n.symbol_id
            WHERE s.kind='source-variable' AND n.domain='program' ''').fetchone()
        assert db.execute("SELECT 1 FROM types WHERE kind='array' AND array_bound_kind='dependent'").fetchone()
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert len(tables)==44 and not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', *(str(dict(row)) for row in declarations)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/source_variables.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'source-variables.dotobj'
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
    print('PASS: exact nonreal variable declaration/type/template/initializer metadata, no runtime storage, '
          'native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
