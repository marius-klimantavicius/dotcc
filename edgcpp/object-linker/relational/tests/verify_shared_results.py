#!/usr/bin/env python3
"""Recorded conditional/comma destinations, self identity and selected cleanup."""
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
        columns={row['name'] for row in db.execute("PRAGMA table_info('initializations')")}
        assert 'destination_initialization_id' in columns
        assert any(row['from']=='destination_initialization_id' and row['table']=='initializations' and row['to']=='id'
                   for row in db.execute("PRAGMA foreign_key_list('initializations')"))
        masters=db.execute('''SELECT i.id,i.destination_initialization_id,i.variable_symbol_id,
            i.destructor_symbol_id,i.lifetime_id,i.temporary_lifetime,i.value_node_id,
            n.kind,n.operator,n.type_id,n.value_category,s.name AS function_name
            FROM initializations i JOIN nodes n ON n.id=i.value_node_id
            JOIN symbols s ON s.id=i.function_id WHERE i.kind='shared-result' ORDER BY i.id''').fetchall()
        dependents=db.execute('''SELECT i.id,i.kind,i.destination_initialization_id,
            i.destructor_symbol_id,i.lifetime_id,m.kind AS master_kind,
            i.function_id,m.function_id AS master_function
            FROM initializations i JOIN initializations m ON m.id=i.destination_initialization_id
            ORDER BY i.id''').fetchall()
        for dependent in dependents:
            assert dependent['master_kind']=='shared-result', dict(dependent)
            assert dependent['function_id']==dependent['master_function'], dict(dependent)
            assert dependent['destructor_symbol_id'] is None, dict(dependent)
            assert not db.execute('SELECT 1 FROM lifetime_destructions WHERE initialization_id=?',
                                  (dependent['id'],)).fetchone(), dict(dependent)
            assert db.execute("SELECT 1 FROM nodes WHERE kind='temporary' AND initialization_id=?",
                              (dependent['id'],)).fetchone(), dict(dependent)
        if path.stem=='provider':
            assert masters and dependents
            assert {'choose_result','choose_comma','choose_nested','local_result'} <= {row['function_name'] for row in masters}, [dict(row) for row in masters]
            assert any(row['operator']=='conditional' for row in masters), [dict(row) for row in masters]
            assert any(row['operator']=='comma' for row in masters), [dict(row) for row in masters]
            assert any(row['destination_initialization_id'] is not None for row in masters), 'Missing nested shared master'
            assert {'constructor','class-result'} <= {row['kind'] for row in dependents}, [dict(row) for row in dependents]
            for master in masters:
                assert master['type_id'] is not None and master['value_category']=='prvalue', dict(master)
                assert db.execute('SELECT 1 FROM initializations WHERE destination_initialization_id=?',
                                  (master['id'],)).fetchone(), dict(master)
            owned=db.execute('''SELECT i.id,count(*) AS owners FROM initializations i
                JOIN lifetime_destructions d ON d.initialization_id=i.id
                WHERE i.kind='shared-result' AND i.destructor_symbol_id IS NOT NULL GROUP BY i.id''').fetchall()
            assert owned and all(row['owners']==1 for row in owned), [dict(row) for row in owned]
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows' for row in tables]
        output += ['', 'shared effect masters', *(str(dict(row)) for row in masters),
                   '', 'exact destination dependents', *(str(dict(row)) for row in dependents)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/shared_results',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(path) for path in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.SharedResults',
         '--class','Core','--entry','main',*objects],work/'link.log')
    code='\n'.join(path.read_text() for path in generated.glob('*.cs'))
    assert 'choose_result' in code and 'choose_comma' in code and 'choose_nested' in code
    assert 'if (' in code and 'else' in code
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: exact shared destinations, conditional/comma selected effects, self identity, copy/ctor failure cleanup, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
