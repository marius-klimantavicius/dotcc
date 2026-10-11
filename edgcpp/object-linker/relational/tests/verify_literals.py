#!/usr/bin/env python3
"""Exact target literal bytes, selected identity and explicit copy/zero actions."""
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
        assert not list(db.execute('''SELECT l.node_id FROM literal_initializers l
            WHERE NOT EXISTS (SELECT 1 FROM initializations i WHERE i.value_node_id=l.node_id)
            AND NOT EXISTS (SELECT 1 FROM global_initializers g WHERE g.initializer_node_id=l.node_id)''')), \
            'Read-only literal expressions must not retain unused initialization actions'
        rows=list(db.execute("SELECT l.id,l.byte_count,t.size,s.link_name,s.linkage FROM literal_objects l JOIN types t ON t.id=l.type_id LEFT JOIN symbols s ON s.id=l.selected_symbol_id ORDER BY l.id"))
        assert rows, 'Missing exact literal objects'
        for literal,count,*_ in rows:
            values=list(db.execute('SELECT byte_offset,value FROM literal_bytes WHERE literal_id=? ORDER BY byte_offset',(literal,)))
            assert len(values)==count and [r[0] for r in values]==list(range(count)), 'Missing literal bytes'
        if path.stem=='provider':
            assert db.execute('SELECT count(*) FROM literal_initializers WHERE zero_byte_count>0').fetchone()[0], 'Missing selected zero action'
            assert db.execute('SELECT count(*) FROM nodes WHERE literal_byte_offset>0').fetchone()[0], 'Missing selected literal offset'
            assert any(r[4]=='external' for r in rows), 'Missing selected inline literal linker identity'
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'literal storage objects', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/literals',source)
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
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.Literals',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: exact literals, storage identity and copy/zero actions, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
