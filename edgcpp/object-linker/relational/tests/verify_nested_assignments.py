#!/usr/bin/env python3
"""Recorded inline local assignments retain conditional/short-circuit effects."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone()==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows=db.execute('''SELECT n.id,n.value_category,n.evaluation_order,l.kind,s.kind,s.name
            FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='operand' AND e.ordinal=0
            JOIN nodes l ON l.id=e.child_id JOIN symbols s ON s.id=l.symbol_id
            WHERE n.operator='assign' AND n.evaluation_order='right-to-left' AND s.name='value' ''').fetchall()
        assert rows and all(category=='prvalue' and kind=='variable' and symbol=='local'
                            for _,category,_,kind,symbol,_ in rows), rows
        return path.name+': '+str(rows)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/nested_assignments',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve()],work/(name+'-compile.log'))
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n'.join(inspect(obj) for obj in objects)+'\n')
    source.rename(work/'original-source')
    generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true','-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: EDG recorded prvalue/RTL local assignments remain inline in selected conditional/short-circuit branches; '
          'once-only calls, byte conversion/result; native/source-hidden C#'+('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
