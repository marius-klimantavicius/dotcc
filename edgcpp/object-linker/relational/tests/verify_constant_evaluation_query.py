#!/usr/bin/env python3
"""Stock EDG intrinsic runtime result and already folded compile-time values."""
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
        queries=db.execute("""
            SELECT n.id,n.domain,n.function_id,n.type_id,n.value_category,n.constant_text,
                   n.symbol_id,n.file_id,n.line,n.column,s.name AS function_name
            FROM nodes n LEFT JOIN symbols s ON s.id=n.function_id
            WHERE n.operator='is-constant-evaluated' ORDER BY n.id
        """).fetchall()
        assert queries and any(row['function_name']=='selected_query' for row in queries), [dict(row) for row in queries]
        for query in queries:
            assert query['type_id'] is not None and query['value_category']=='prvalue' and query['symbol_id'] is None
            assert query['file_id'] is not None and query['line']>0 and query['column']>0
            if query['domain']=='program':
                assert query['constant_text']=='0' and query['function_id'] is not None, dict(query)
            else:
                assert query['constant_text'] is None and query['function_id'] is None, dict(query)
            children=db.execute("""
                SELECT e.role,e.ordinal,n.kind,n.domain,n.function_id,n.type_id,n.value_category,
                       n.constant_text,n.source_node_id,n.file_id,n.line,n.column
                FROM edges e JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=?
            """,(query['id'],)).fetchall()
            assert len(children)==1, [dict(row) for row in children]
            name=children[0]
            assert (name['role'],name['ordinal'],name['kind'])==('source-name',0,'reference-name'), dict(name)
            assert name['domain']=='template-recipe' and name['function_id'] is None
            assert name['type_id'] is None and name['value_category'] is None
            assert name['constant_text']=='__builtin_is_constant_evaluated' and name['source_node_id']==query['id']
            assert name['file_id'] is not None and name['line']>0 and name['column']>0
        assert not db.execute("SELECT 1 FROM symbols WHERE name='__builtin_is_constant_evaluated'").fetchone(), 'Intrinsic acquired a callable ABI/provider'
        folded=db.execute("""
            SELECT n.constant_text FROM symbols s JOIN global_initializers g ON g.symbol_id=s.id
            JOIN nodes n ON n.id=g.initializer_node_id WHERE s.name='folded_marker'
        """).fetchone()
        assert folded and folded['constant_text']=='7', tuple(folded) if folded else None
        selected_assertion=db.execute("""
            SELECT n.constant_text FROM nodes a JOIN edges e ON e.parent_id=a.id AND e.role='condition'
            JOIN nodes n ON n.id=e.child_id WHERE a.kind='static-assertion'
        """).fetchall()
        assert len(selected_assertion)==1 and selected_assertion[0]['constant_text']=='1', [dict(row) for row in selected_assertion]
        assert not db.execute("SELECT 1 FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL").fetchone()
        tables=db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows' for row in tables]
        output += ['', 'selected intrinsic queries', *(str(dict(row)) for row in queries)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source.cpp';shutil.copy2(RELATIONAL/'tests/constant_evaluation_query.cpp',source)
    run(['g++','-std=c++17',source,'-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    obj=work/'constant-evaluation-query.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source,'--output',obj,'--std','c++17',
         '--work-dir',work/'compile-work','--compiler',args.compiler.resolve()],work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    source.rename(work/'original-source.cpp');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',obj],work/'link.log')
    code='\n'.join(path.read_text() for path in generated.glob('*.cs'))
    assert '__builtin_is_constant_evaluated' not in code, 'Intrinsic emitted a callable ABI/provider'
    assert 'compile_time_query' not in code, 'Compile-time metadata entered executable output'
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected intrinsic runtime false, EDG-folded compile-time true, source navigation and native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
