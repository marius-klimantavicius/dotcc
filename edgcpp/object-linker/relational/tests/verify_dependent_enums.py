#!/usr/bin/env python3
"""Dependent enumerator values retain recipes; concrete selected enums execute."""
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
        values=db.execute("SELECT * FROM enum_values WHERE name IN('extent','alignment') ORDER BY enum_type_id,ordinal").fetchall()
        dependent=[row for row in values if row['expression_node_id'] is not None]
        concrete=[row for row in values if row['value_text'] is not None]
        assert {row['name'] for row in dependent}=={'extent','alignment'}, [dict(row) for row in values]
        assert {row['name'] for row in concrete}=={'extent','alignment'}, [dict(row) for row in values]
        operations=set();selected_nodes=set()
        for value in dependent:
            assert value['value_text'] is None and value['value_type_id'] is not None, dict(value)
            assert value['file_id'] is not None and value['line']>0 and value['column']>0, dict(value)
            pending=[value['expression_node_id']]
            root=db.execute('SELECT * FROM nodes WHERE id=?',(value['expression_node_id'],)).fetchone()
            assert root['type_id']==value['value_type_id'], (dict(value),dict(root))
            assert root['enum_value_id']==value['id'], (dict(value),dict(root))
            while pending:
                node_id=pending.pop()
                if node_id in selected_nodes:continue
                selected_nodes.add(node_id)
                node=db.execute('SELECT * FROM nodes WHERE id=?',(node_id,)).fetchone()
                assert node['domain']=='template-recipe' and node['function_id'] is None, dict(node)
                if node['operator']:operations.add(node['operator'])
                pending.extend(row[0] for row in db.execute(
                    "SELECT child_id FROM edges WHERE parent_id=? AND role='operand' ORDER BY ordinal",(node_id,)))
        assert {'sizeof','add','alignof'}<=operations, operations
        scalar=db.execute("SELECT size,alignment FROM types WHERE kind='i32' AND builtin_name='int' LIMIT 1").fetchone()
        expected={'extent':str(scalar['size']+1),'alignment':str(scalar['alignment'])}
        for value in concrete:
            assert value['expression_node_id'] is None and value['value_text']==expected[value['name']], dict(value)
        for enum_id in {row['enum_type_id'] for row in values}:
            order=[row['name'] for row in values if row['enum_type_id']==enum_id]
            assert order==['extent','alignment'], (enum_id,order)
        assert db.execute("SELECT 1 FROM types WHERE kind='array' AND array_bound_kind='dependent' AND array_bound_node_id IS NOT NULL").fetchone()
        assert db.execute('''SELECT 1 FROM nodes n JOIN enum_values v ON v.id=n.enum_value_id
            WHERE n.domain='program' AND v.value_text IS NOT NULL''').fetchone(), 'Concrete selected enum reference missing'
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert len(tables)==44 and not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', *(str(dict(row)) for row in values)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/dependent_enums.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'dependent-enums.dotobj'
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
    print('PASS: selected dependent enumerator recipes/source order, concrete selected enum values/references '
          'and native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
