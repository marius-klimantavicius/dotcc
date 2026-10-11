#!/usr/bin/env python3
"""EDG-selected folded record defaults use recorded fields, bases and zeroing."""
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
            SELECT c.function_id,c.ordinal,f.name,i.id,i.kind,i.value_node_id,n.kind,
                   i.constructor_symbol_id,i.destructor_symbol_id,i.lifetime_id,
                   i.init_expr_lifetime_id,i.overlap_lifetime_id
            FROM constructor_initializers c JOIN symbols s ON s.id=c.function_id
            JOIN fields f ON f.id=c.field_id JOIN initializations i ON i.id=c.initialization_id
            LEFT JOIN nodes n ON n.id=i.value_node_id
            WHERE s.name='Holder' ORDER BY c.function_id,c.ordinal
        """))
        if path.stem=='provider':
            assert rows, 'Missing selected Holder member actions'
            constants=[row for row in rows if row[2] in ('direct','nested','partial') and row[4]=='constant']
            assert {row[2] for row in constants}=={'direct','nested','partial'}, rows
            assert any(row[2]=='direct' and row[4]=='constructor' for row in rows), 'Missing explicit record override'
            for row in constants:
                assert row[6]=='record-initializer' and row[7:]==(None,None,None,None,None), row
                assert db.execute('SELECT 1 FROM record_initializers WHERE node_id=?',(row[5],)).fetchone()
            for function in {row[0] for row in rows}:
                fields=[row[2] for row in rows if row[0]==function]
                assert fields==['direct','nested','partial','trailing'], fields
            def values(node):
                selected=db.execute('SELECT kind,constant_text FROM nodes WHERE id=?',(node,)).fetchone()
                if selected[0]=='constant':return [selected[1]]
                assert selected[0]=='record-initializer', selected
                return [value for (child,) in db.execute('SELECT value_node_id FROM record_initializer_elements WHERE initializer_node_id=? ORDER BY ordinal',(node,)) for value in values(child)]
            for row in constants:
                actual=values(row[5])
                expected={'direct':['5','7'],'nested':['11','13','17'],'partial':['19']}
                assert actual==expected[row[2]] or (row[2]=='partial' and actual==['19','0']), (row,actual)
                if row[2]=='partial':
                    extent=db.execute('SELECT r.zero_byte_count,t.size FROM record_initializers r JOIN nodes n ON n.id=r.node_id JOIN types t ON t.id=n.type_id WHERE r.node_id=?',(row[5],)).fetchone()
                    assert extent[1]==8 and extent[0] in (0,8), extent
                    assert extent[0]==8 or actual==['19','0'], 'Missing selected trailing zero action/value'
                elif row[2]=='nested':
                    assert db.execute('SELECT 1 FROM record_initializer_elements WHERE initializer_node_id=? AND base_subobject_id IS NOT NULL',(row[5],)).fetchone(), 'Missing selected empty-base initializer'
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'selected folded record default actions', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/default_record_constants',source)
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,'--std','c++17',
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.DefaultRecordConstants',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected folded record defaults, member/base/zero actions and overrides, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
