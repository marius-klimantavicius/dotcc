#!/usr/bin/env python3
"""Selected type and expression operands of dependent size/alignment constants."""
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
        assert db.execute('PRAGMA user_version').fetchone()[0]==1023
        queries=db.execute("SELECT * FROM nodes WHERE kind='operation' AND operator IN('sizeof','alignof','gnu-alignof') ORDER BY id").fetchall()
        assert {query['operator'] for query in queries}=={'sizeof','alignof','gnu-alignof'}, [dict(row) for row in queries]
        operand_kinds=set()
        for query in queries:
            assert query['domain']=='template-recipe' and query['function_id'] is None, dict(query)
            assert query['type_id'] is not None and query['value_category']=='prvalue' and query['constant_text'] is None
            assert query['symbol_id'] is None and query['initialization_id'] is None and query['lifetime_id'] is None
            assert query['file_id'] is not None and query['line']>0 and query['column']>0, dict(query)
            operands=db.execute('''SELECT e.role,e.ordinal,n.* FROM edges e JOIN nodes n ON n.id=e.child_id
                WHERE e.parent_id=? ORDER BY e.ordinal''',(query['id'],)).fetchall()
            assert len(operands)==1, [dict(row) for row in operands]
            operand=operands[0]
            assert (operand['role'],operand['ordinal'])==('operand',0), dict(operand)
            assert operand['domain']=='template-recipe' and operand['function_id'] is None and operand['type_id'] is not None
            operand_kinds.add(operand['kind'])
            if operand['kind']=='type-reference':
                assert operand['value_category'] is None and operand['source_node_id']==query['id'], dict(operand)
                selected=db.execute('SELECT * FROM template_type_parameters WHERE type_id=?',(operand['type_id'],)).fetchone()
                assert selected and selected['kind']=='parameter', dict(selected) if selected else None
            else:
                assert operand['kind']=='operation' and operand['operator']=='indirect', dict(operand)
                assert operand['value_category']=='lvalue', dict(operand)
        assert operand_kinds=={'type-reference','operation'}, operand_kinds
        bound_queries=db.execute('''WITH RECURSIVE bound_nodes(id) AS (
                SELECT array_bound_node_id FROM types WHERE array_bound_kind='dependent' AND array_bound_node_id IS NOT NULL
                UNION SELECT e.child_id FROM edges e JOIN bound_nodes b ON b.id=e.parent_id WHERE e.role='operand'
            ) SELECT DISTINCT n.id FROM bound_nodes b JOIN nodes n ON n.id=b.id
              WHERE n.kind='operation' AND n.operator IN('sizeof','alignof','gnu-alignof')''').fetchall()
        assert {row['id'] for row in bound_queries}=={row['id'] for row in queries}
        scalar=db.execute("SELECT size,alignment FROM types WHERE kind='i32' AND builtin_name='int' LIMIT 1").fetchone()
        assert scalar
        for name,value in {'folded_size':str(scalar['size']), 'folded_alignment':str(scalar['alignment'])}.items():
            selected=db.execute('''SELECT n.constant_text FROM symbols s JOIN global_initializers g ON g.symbol_id=s.id
                JOIN nodes n ON n.id=g.initializer_node_id WHERE s.name=?''',(name,)).fetchone()
            assert selected and selected['constant_text']==value, (name, tuple(selected) if selected else None)
        assert not db.execute("SELECT 1 FROM symbols WHERE name IN('sizeof','alignof','gnu-alignof')").fetchone()
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert len(tables)==44 and not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        return '\n'.join([f'{path.name}: {path.stat().st_size} bytes', *(str(dict(row)) for row in queries)])


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/query_constant_recipes.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'query-constant-recipes.dotobj'
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
    print('PASS: exact selected query-constant type/expression operands, metadata isolation, '
          'EDG-folded size/alignment and native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
