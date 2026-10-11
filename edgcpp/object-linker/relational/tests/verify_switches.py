#!/usr/bin/env python3
"""Selected switch cases preserve lexical fallthrough, control flow, and cleanup."""
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
        rows=list(db.execute("SELECT id FROM nodes WHERE kind='switch'"))
        if path.stem=='provider':assert len(rows)>=7, 'Missing selected switches'
        for (node,) in rows:
            body=db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='body'",(node,)).fetchone()[0]
            cases=list(db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='case' ORDER BY ordinal",(node,)))
            assert len(cases)==len(set(cases)), 'Duplicated selected label identity'
            for (case,) in cases:
                count=db.execute("WITH RECURSIVE walk(id) AS (SELECT ? UNION ALL SELECT e.child_id FROM edges e JOIN walk w ON w.id=e.parent_id WHERE e.role!='case') SELECT count(*) FROM walk WHERE id=?",(body,case)).fetchone()[0]
                assert count==1, 'Selected case must reuse its source body identity'
                kind,line=db.execute('SELECT kind,line FROM nodes WHERE id=?',(case,)).fetchone()
                assert kind in ('case','default') and line>0
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'selected switches', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/switches',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    unsupported=work/'unsupported.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',source/'unsupported_nested.cpp','--output',unsupported,
         '--work-dir',work/'unsupported-work','--compiler',args.compiler.resolve()],work/'unsupported-compile.log')
    run(['dotnet',args.linker.resolve(),'link','--out',work/'unsupported-generated','--entry','main',unsupported],work/'unsupported-link.log',expected=1)
    assert 'nested source scope or cleanup region' in (work/'unsupported-link.log').read_text()
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.Switches',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected switches, fallthrough and cleanup, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
