#!/usr/bin/env python3
"""Dependent source value recipes with separately loaded executable bodies."""
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
        rows=list(db.execute("SELECT n.id,n.kind,r.name,r.parameter_depth,r.parameter_position,r.qualifier_type_id FROM nodes n LEFT JOIN template_value_references r ON r.node_id=n.id WHERE n.domain='template-recipe' ORDER BY n.id"))
        assert rows, 'Expected source recipe graph'
        assert not list(db.execute("SELECT id FROM nodes WHERE domain='template-recipe' AND function_id IS NOT NULL"))
        assert db.execute("SELECT count(*) FROM template_arguments a JOIN nodes n ON n.id=a.expression_id WHERE n.domain='template-recipe'").fetchone()[0]>0
        assert db.execute("SELECT count(*) FROM template_formals f JOIN nodes n ON n.id=f.default_expression_id WHERE n.domain='template-recipe'").fetchone()[0]>0
        assert db.execute("SELECT count(*) FROM template_value_references WHERE parameter_position IS NOT NULL").fetchone()[0]>0
        assert db.execute("SELECT count(*) FROM template_value_references WHERE qualifier_type_id IS NOT NULL").fetchone()[0]>0
        if path.stem=='main':
            assert db.execute("SELECT count(*) FROM nodes WHERE domain='template-recipe' AND floating_value_id IS NOT NULL").fetchone()[0]>0
        tables=list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output=[f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'source recipe nodes', *('\t'.join(map(str,row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/dependent_values',source)
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
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.DependentValues',
         '--class','Core','--entry','main',*objects],work/'link.log')
    assert 'global nodes 0' in (work/'link.log').read_text(), 'Template recipes entered executable global storage'
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: dependent source recipes and executable isolation, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
