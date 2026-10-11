#!/usr/bin/env python3
"""Selected record component actions preserve copy values, aliases and ordering."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows=list(db.execute("""
            SELECT n.id,i.id,i.kind,v.operator,v.kind,n.file_id,n.line,n.column
            FROM nodes n JOIN initializations i ON i.id=n.initialization_id
            JOIN types t ON t.id=i.type_id LEFT JOIN nodes v ON v.id=i.value_node_id
            WHERE n.kind='initialize' AND t.kind='record' AND t.name='TestValue'
                AND (EXISTS(SELECT 1 FROM record_initializer_elements e WHERE e.value_node_id=n.id)
                    OR EXISTS(SELECT 1 FROM array_initializer_elements e WHERE e.value_node_id=n.id))
            ORDER BY n.id
        """))
        assert rows, 'Missing selected record aggregate component actions'
        assert all(row[2] in ('expression','constant','zero') for row in rows), rows
        assert all(row[5] is not None and row[6]>0 and row[7]>0 for row in rows), rows
        for node,action,*_ in rows:
            facts=db.execute("""
                SELECT variable_symbol_id,constructor_symbol_id,destructor_symbol_id,
                       lifetime_id,init_expr_lifetime_id,overlap_lifetime_id,
                       unordered,overlaps_inner,partial_aggregate,free_on_exception
                FROM initializations WHERE id=?
            """,(action,)).fetchone()
            assert facts==(None,None,None,None,None,None,0,0,0,0), facts
            field=db.execute('SELECT field_id,base_subobject_id FROM record_initializer_elements WHERE value_node_id=?',(node,)).fetchall()
            array=db.execute('SELECT array_index,repeat_count FROM array_initializer_elements WHERE value_node_id=?',(node,)).fetchall()
            assert field or array, (node,action)
            assert all(selected is not None and base is None for selected,base in field), field
            assert all(count==1 for _,count in array), array
        if path.stem=='provider':
            assert db.execute("""
                SELECT count(*) FROM initializations i JOIN types t ON t.id=i.type_id
                JOIN nodes v ON v.id=i.value_node_id JOIN symbols f ON f.id=i.function_id
                WHERE t.name='TestValue' AND i.kind='expression'
                    AND v.operator='ref-indirect' AND f.name='from_reference'
            """).fetchone()[0], 'Missing exact selected reference-to-record source'
        else:
            assert db.execute("""
                SELECT count(*) FROM record_initializer_elements e JOIN nodes n ON n.id=e.value_node_id
                JOIN initializations i ON i.id=n.initialization_id JOIN types t ON t.id=i.type_id
                WHERE n.kind='initialize' AND t.kind='record' AND t.name='TestValue' AND e.field_id IS NOT NULL
            """).fetchone()[0]>=2, 'Missing selected record field component actions'
            assert any(row[3]=='call' for row in rows), 'Missing selected returned record field source'
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'selected record aggregate component actions', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/record_aggregate_components',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve(),'--std','c++17'],work/f'{name}-compile.log')
        objects.append(obj)
    unsupported=work/'unsupported.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',RELATIONAL/'tests/dynamic_aggregates/unsupported_cleanup.cpp',
         '--output',unsupported,'--work-dir',work/'unsupported-work','--compiler',args.compiler.resolve(),
         '--std','c++17'],work/'unsupported-compile.log',expected=1)
    diagnostic=(work/'unsupported-work/stderr.txt').read_text()
    assert 'semantic exporter missing' in diagnostic and 'aggregate' in diagnostic, diagnostic
    assert not unsupported.exists(), 'Unsupported cleanup must not publish an object'
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.RecordAggregateComponents',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected record aggregate components, copy values, aliases and ordering, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
