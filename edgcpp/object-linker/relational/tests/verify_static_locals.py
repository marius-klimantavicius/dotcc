#!/usr/bin/env python3
"""Selected constant/zero static locals retain stable storage and cross-TU identity."""
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
        rows=list(db.execute("SELECT s.id,s.name,s.link_name,s.linkage,s.definition_kind,s.parent_symbol_id FROM symbols s WHERE s.kind='static-local' ORDER BY s.id"))
        assert rows and all(row[5] is not None for row in rows), 'Missing source static storage/parent facts'
        assert any(row[3]=='external' and row[4]=='weak' for row in rows), rows
        for row in rows:
            declarations=db.execute("SELECT count(*) FROM nodes WHERE kind='static-declaration' AND symbol_id=? AND function_id=?",(row[0],row[5])).fetchone()[0]
            assert declarations==1, 'Static declaration must preserve lexical ownership without runtime initialization'
            initializers=db.execute("SELECT n.function_id FROM global_initializers g JOIN nodes n ON n.id=g.initializer_node_id WHERE g.symbol_id=?",(row[0],)).fetchall()
            assert initializers and all(value==(None,) for value in initializers), 'Static initializer must belong to persistent storage'
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'selected static storage', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def inspect_link_identities(objects):
    selected=[]
    for path in objects:
        with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
            rows=list(db.execute("SELECT p.name,s.id,s.link_name,s.linkage,s.definition_kind FROM symbols s JOIN symbols p ON p.id=s.parent_symbol_id WHERE s.kind='static-local' ORDER BY p.name,s.id"))
            selected.append(rows)
    shared=('shared_counter','shared_seed','shared_pointer_address','shared_forward_address','shared_ratio','shared_pair','shared_array')
    for parent in shared:
        definitions=[[row for row in rows if row[0]==parent] for rows in selected]
        assert all(len(rows)==1 for rows in definitions), (parent,definitions)
        first,second=(rows[0] for rows in definitions)
        assert first[2]==second[2] and first[3:]==second[3:]==('external','weak'), (parent,definitions)
    internal=[[row for row in rows if row[0]=='local_counter'] for rows in selected]
    assert all(len(rows)==1 and rows[0][3:]==('internal','strong') for rows in internal), internal
    # Internal linkage keeps the TU identity even when EDG's linker spelling is
    # identical. Native/managed execution below also checks distinct addresses.
    identities=[(path.resolve(),rows[0][1]) for path,rows in zip(objects,internal)]
    assert len(set(identities))==2, identities
    scoped=[row for row in selected[0] if row[0]=='scoped']
    assert len(scoped)==2 and len({row[1] for row in scoped})==2, scoped
    assert all(row[3:]==('internal','strong') for row in scoped), scoped


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/static_locals',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable,RELATIONAL/'compile.py',source/f'{name}.cpp','--output',obj,
             '--work-dir',work/f'{name}-work','--compiler',args.compiler.resolve()],work/f'{name}-compile.log')
        objects.append(obj)
    run([sys.executable,RELATIONAL/'compile.py',source/'unsupported_dynamic.cpp','--output',work/'unsupported.dotobj',
         '--work-dir',work/'unsupported-work','--compiler',args.compiler.resolve()],work/'unsupported-compile.log',expected=1)
    diagnostic=(work/'unsupported-work/stderr.txt').read_text()
    assert 'semantic exporter missing' in diagnostic, diagnostic
    assert not (work/'unsupported.dotobj').exists(), 'Unsupported dynamic static must not publish an object'
    inspect_link_identities(objects)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source');generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.StaticLocals',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: selected static local storage and stable addresses, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
