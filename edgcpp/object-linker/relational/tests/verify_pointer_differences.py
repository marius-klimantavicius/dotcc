#!/usr/bin/env python3
"""EDG-selected pointer differences preserve element counts and operand effects."""
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
        rows=list(db.execute("SELECT n.id,t.kind,n.value_category,n.evaluation_order FROM nodes n JOIN types t ON t.id=n.type_id WHERE n.operator='pointer-difference' ORDER BY n.id"))
        assert rows, 'Missing selected pointer differences'
        layouts=[]
        for node,result,category,order in rows:
            assert result in ('i32','i64') and category=='prvalue', rows
            assert order in ('unspecified','left-to-right'), rows
            operands=list(db.execute("SELECT e.ordinal,COALESCE(n.execution_type_id,n.type_id) FROM edges e JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=? AND e.role='operand' ORDER BY e.ordinal",(node,)))
            assert [r[0] for r in operands]==[0,1], operands
            for _,pointer in operands:
                kind,pointee=db.execute('SELECT kind,pointee_type_id FROM types WHERE id=?',(pointer,)).fetchone()
                assert kind=='pointer' and pointee is not None, operands
                kind,size=db.execute('SELECT kind,size FROM types WHERE id=?',(pointee,)).fetchone()
                while kind=='qualified':
                    pointee=db.execute('SELECT underlying_type_id FROM types WHERE id=?',(pointee,)).fetchone()[0]
                    kind,size=db.execute('SELECT kind,size FROM types WHERE id=?',(pointee,)).fetchone()
                assert kind in ('i8','i32') and size in (1,4), (kind,size)
                layouts.append(size)
        if path.stem=='provider':
            assert {1,4}<=set(layouts), layouts
        else:
            assert db.execute("SELECT count(*) FROM nodes WHERE operator='assign' AND evaluation_order='right-to-left'").fetchone()[0], 'Missing selected RHS-before-destination order'
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'selected pointer differences', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/pointer_differences',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.PointerDifferences',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected pointer differences and operand effects, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
