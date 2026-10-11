#!/usr/bin/env python3
"""Selected builtin names and operands stay source recipe metadata."""
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
        expected={'__is_same': ['T', 'U'], '__is_trivial': ['T'], '__is_constructible': ['T', 'U']}
        formals={row['name']:row for row in db.execute('''SELECT f.* FROM template_formals f
            JOIN templates t ON t.id=f.template_id WHERE t.name='TraitDefaults' AND f.kind='type' ''')}
        assert set(formals)=={'T','U'}, {name:dict(row) for name,row in formals.items()}
        queries=db.execute("SELECT * FROM nodes WHERE kind='operation' AND operator IN(?,?,?) ORDER BY id",
                           tuple(expected)).fetchall()
        assert {query['operator'] for query in queries}==set(expected), [dict(row) for row in queries]
        for query in queries:
            assert query['domain']=='template-recipe' and query['function_id'] is None, dict(query)
            assert query['value_category']=='prvalue' and query['constant_text'] is None, dict(query)
            assert query['symbol_id'] is None and query['initialization_id'] is None and query['lifetime_id'] is None
            assert query['file_id'] is not None and query['line']>0 and query['column']>0, dict(query)
            result=db.execute('SELECT kind FROM types WHERE id=?',(query['type_id'],)).fetchone()
            assert result and result['kind']=='bool', dict(query)
            operands=db.execute('''SELECT e.role,e.ordinal,n.*,t.name AS type_name,p.parameter_position,p.is_pack
                FROM edges e JOIN nodes n ON n.id=e.child_id JOIN types t ON t.id=n.type_id
                JOIN template_type_parameters p ON p.type_id=t.id
                WHERE e.parent_id=? ORDER BY e.ordinal''',(query['id'],)).fetchall()
            names=expected[query['operator']]
            assert len(operands)==len(names), [dict(row) for row in operands]
            for ordinal, operand in enumerate(operands):
                assert (operand['role'],operand['ordinal'])==('operand',ordinal), dict(operand)
                assert operand['kind']=='type-reference' and operand['type_name']==names[ordinal], dict(operand)
                assert operand['domain']=='template-recipe' and operand['function_id'] is None
                assert operand['value_category'] is None and operand['source_node_id']==query['id'], dict(operand)
                formal=formals[names[ordinal]]
                assert operand['type_id']==formal['type_id'] and operand['parameter_position']==formal['parameter_position'], dict(operand)
                assert not operand['is_pack'], dict(operand)
        assert not db.execute("SELECT 1 FROM symbols WHERE name IN(?,?,?)",tuple(expected)).fetchone()
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='program' AND operator IN(?,?,?)",tuple(expected)).fetchone()
        for name,value in {'folded_same':'1','folded_different':'0','folded_trivial':'1','folded_constructible':'1'}.items():
            selected=db.execute('''SELECT n.constant_text FROM symbols s JOIN global_initializers g ON g.symbol_id=s.id
                JOIN nodes n ON n.id=g.initializer_node_id WHERE s.name=?''',(name,)).fetchone()
            assert selected and selected['constant_text']==value, (name, tuple(selected) if selected else None)
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
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/builtin_recipes.cpp',source)
    run(['g++','-std=c++20',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'builtin-recipes.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--output',obj,'--work-dir',work/'compile-work',
         '--compiler',args.compiler.resolve()],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    source.rename(work/'original-source.cpp');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',obj],work/'link.log')
    code='\n'.join(path.read_text() for path in generated.glob('*.cs'))
    assert all(name not in code for name in ('__is_same','__is_trivial','__is_constructible'))
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: exact selected builtin recipe names/ordered operands, metadata isolation, '
          'EDG-folded values and native/source-hidden C#'+(', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
