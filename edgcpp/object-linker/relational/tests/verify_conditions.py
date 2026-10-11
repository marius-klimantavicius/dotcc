#!/usr/bin/env python3
"""Selected condition initialization, lifetime scope and loop cleanup."""
import argparse
import os
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows=list(db.execute("SELECT n.id,n.initialization_id,n.lifetime_id FROM nodes n WHERE n.kind='condition'"))
        if path.stem=='provider':
            assert len(rows)>=8, rows
            assert any(r[1] is not None and r[2] is not None for r in rows), rows
            assert db.execute("SELECT count(*) FROM nodes n JOIN lifetimes l ON l.id=n.lifetime_id WHERE n.kind='for' AND l.owner_node_id=n.id").fetchone()[0]>0
            assert not list(db.execute("SELECT n.id FROM nodes n JOIN lifetimes l ON l.id=n.lifetime_id WHERE n.kind IN ('for','condition') AND l.owner_node_id!=n.id"))
            assert db.execute("SELECT count(*) FROM edges e JOIN nodes n ON n.id=e.parent_id WHERE n.kind='condition' AND e.role='value'").fetchone()[0]==len(rows)
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'conditions', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    parser.add_argument('--native-cxx',default=os.environ.get('EDG_TEST_CLANGXX'),
                        help='Native reference compiler; defaults to available Clang (GCC bug 86769)')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/conditions',source)
    native_cxx=args.native_cxx or shutil.which('clang++')
    if not native_cxx:
        candidates=sorted(Path('/usr/lib').glob('llvm-*/bin/clang++'),reverse=True)
        native_cxx=str(candidates[0]) if candidates else None
    if not native_cxx: raise RuntimeError('Set --native-cxx to Clang; GCC bug 86769 changes condition cleanup order')
    run([native_cxx,'--version'],work/'native-version.log')
    run([native_cxx,'-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.Conditions',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: condition initialization/lifetimes, loop continue cleanup, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
