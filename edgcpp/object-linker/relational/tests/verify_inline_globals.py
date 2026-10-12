#!/usr/bin/env python3
"""Selected inline globals coalesce while scoped names and strong data remain exact."""
import argparse
import hashlib
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(objects):
    groups=[]
    for path in objects:
        with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
            db.row_factory=sqlite3.Row
            assert tuple(db.execute('PRAGMA integrity_check').fetchone())==('ok',)
            assert not list(db.execute('PRAGMA foreign_key_check'))
            rows=db.execute("SELECT * FROM symbols WHERE kind='global' ORDER BY id").fetchall()
            shared=[row for row in rows if row['name']!='strong_value']
            assert all(row['definition_kind']=='weak' and row['linkage']=='external' and row['is_definition'] for row in shared)
            for name in ('counter','dormant'):
                scoped=[row for row in shared if row['name']==name]
                assert len(scoped)==2 and len({row['link_name'] for row in scoped})==2
            for row in rows:
                if row['name']=='strong_value':assert row['definition_kind']=='strong'
                else:assert db.execute('SELECT 1 FROM global_initializers WHERE symbol_id=?',(row['id'],)).fetchone()
            groups.append([dict(row) for row in rows])
    for name in ('digits','dormant_shared','counter','dormant'):
        assert {row['link_name'] for row in groups[0] if row['name']==name}=={
            row['link_name'] for row in groups[1] if row['name']==name}
    assert any(row['name']=='strong_value' and row['is_definition'] for row in groups[0])
    assert any(row['name']=='strong_value' and not row['is_definition'] for row in groups[1])
    return '\n'.join(str(row) for rows in groups for row in rows)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    if args.nativeaot and not args.linker:parser.error('--nativeaot requires --linker')
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/inline_globals',source)
    compiler=args.compiler.resolve()
    inputs=[compiler,Path(__file__).resolve(),RELATIONAL/'compile.py',*sorted(source.iterdir())]
    if args.linker:inputs.append(args.linker.resolve())
    (work/'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(p.read_bytes()).hexdigest()+'  '+str(p) for p in inputs)+'\n')
    run(['g++','-std=c++17',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--std','c++17','--output',obj,
             '--work-dir',work/f'{name}-work','--compiler',compiler],work/f'{name}-compile.log')
        objects.append(obj)
    source.rename(work/'original-source')
    (work/'database-summary.txt').write_text(inspect(objects)+'\n')
    if args.linker:
        generated=work/'generated'
        run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.InlineGlobals',
             '--class','Core','--entry','main',*objects],work/'link.log')
        run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
        if args.nativeaot:
            run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
                 '-o',work/'nativeaot'],work/'nativeaot-build.log')
            apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
            assert len(apps)==1
            run(apps,work/'nativeaot-run.log')
    print('PASS: native/source-hidden inline weak identities and distinct scoped globals'+
          ('; C#'+(', NativeAOT' if args.nativeaot else '') if args.linker else ''))
    print(work)


if __name__=='__main__':main()
