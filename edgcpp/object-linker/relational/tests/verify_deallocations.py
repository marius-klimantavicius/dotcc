#!/usr/bin/env python3
"""Selected plain deallocation calls preserve pointer evaluation and stock arguments."""
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
        rows=list(db.execute("SELECT a.node_id,s.name,a.count_value_node_id,a.count_symbol_id,a.size_node_id,a.length_check_node_id,f.link_name,a.result_byte_offset FROM allocations a JOIN symbols s ON s.id=a.allocator_symbol_id LEFT JOIN symbols f ON f.id=a.length_failure_symbol_id ORDER BY a.node_id"))
        assert rows, 'Missing selected allocation actions'
        dynamic=[r for r in rows if r[3] is not None]
        assert dynamic and all(r[2] is not None and r[4] is not None and r[5] is not None for r in dynamic), 'Missing selected count/check/size graph'
        assert all(r[6]=='__cxa_throw_bad_array_new_length' and r[7]==0 for r in dynamic), 'Missing selected failure or result adjustment'
        if path.stem=='main':
            assert any(r[2] is None and r[3] is None and r[5] is None and r[6] is None for r in rows), 'Fixed allocation must not retain a runtime count/check action'
        assert db.execute("SELECT count(*) FROM nodes WHERE source_node_id IN(SELECT node_id FROM allocations) AND is_synthetic=1").fetchone()[0], 'Missing source-linked selected expression nodes'
        deletes=list(db.execute("SELECT n.id,n.operator,n.symbol_id,e.child_id FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='execution' WHERE n.kind='delete'"))
        assert deletes, 'Missing selected delete execution graph'
        for node,operator,symbol,execution in deletes:
            assert operator=='array'
            call=db.execute("SELECT kind,operator,symbol_id,source_node_id,is_synthetic FROM nodes WHERE id=?",(execution,)).fetchone()
            assert call==('operation','call',symbol,node,1), call
            pointer=db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='operand'",(node,)).fetchone()[0]
            count=db.execute("WITH RECURSIVE walk(id) AS (SELECT ? UNION ALL SELECT e.child_id FROM edges e JOIN walk w ON e.parent_id=w.id) SELECT count(*) FROM walk WHERE id=?",(execution,pointer)).fetchone()[0]
            assert count==1, 'Delete source pointer must be evaluated exactly once'
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'selected allocation actions', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/deallocations',source)
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
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.Deallocations',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected plain deallocation calls and pointer evaluation, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
