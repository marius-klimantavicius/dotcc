#!/usr/bin/env python3
"""Selected dynamic aggregate components execute at their recorded destinations."""
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
        rows=list(db.execute("SELECT n.id,n.initialization_id,i.kind FROM nodes n JOIN initializations i ON i.id=n.initialization_id WHERE n.kind='initialize' AND n.symbol_id IS NULL"))
        assert rows, 'Missing selected aggregate component actions'
        assert all(row[2] in ('expression','constant','zero') for row in rows), rows
        assert db.execute("SELECT count(*) FROM initializations WHERE kind='aggregate'").fetchone()[0], 'Missing outer aggregate action'
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'selected dynamic aggregate actions', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/dynamic_aggregates',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    run([sys.executable,RELATIONAL/'compile.py',source/'unsupported_cleanup.cpp','--output',work/'unsupported.dotobj',
         '--work-dir',work/'unsupported-work','--compiler',args.compiler.resolve()],work/'unsupported-compile.log',expected=1)
    diagnostic=(work/'unsupported-work/stderr.txt').read_text()
    assert 'semantic exporter missing' in diagnostic and 'aggregate' in diagnostic, diagnostic
    assert not (work/'unsupported.dotobj').exists(), 'Unsupported aggregate must not publish an object'
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.DynamicAggregates',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected dynamic aggregate actions, ordering and exceptions, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
