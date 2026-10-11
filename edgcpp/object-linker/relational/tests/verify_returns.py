#!/usr/bin/env python3
"""EDG-selected return storage, returned construction and exceptional cleanup."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        returns = list(db.execute('''SELECT s.name,t.return_storage,n.id,i.kind,
            result.name,ctor.name,i.value_node_id FROM functions f JOIN symbols s ON s.id=f.symbol_id
            JOIN types t ON t.id=s.type_id JOIN nodes n ON n.function_id=f.symbol_id
            LEFT JOIN initializations i ON i.id=n.initialization_id
            LEFT JOIN types result ON result.id=n.type_id
            LEFT JOIN symbols ctor ON ctor.id=i.constructor_symbol_id
            WHERE n.kind='return' ORDER BY s.name,n.id'''))
        calls = list(db.execute('''SELECT i.id,i.kind,s.name,i.value_node_id FROM initializations i
            LEFT JOIN symbols s ON s.id=i.variable_symbol_id WHERE i.kind='class-result' ORDER BY i.id'''))
        if path.stem == 'provider':
            assert any(row[0]=='make_result' and row[1]=='caller-provided' and row[3] is not None for row in returns), returns
            assert any(row[0]=='make_scalar' and row[1]=='value' and row[3] is not None for row in returns), returns
        else:
            assert calls, 'Expected selected class-result initialization for the returned Result'
            assert db.execute('''SELECT count(*) FROM constructor_initializers c
                JOIN initializations i ON i.id=c.initialization_id
                WHERE i.implied_source=1 AND c.copy_source_symbol_id IS NOT NULL''').fetchone()[0] > 0
        tables = list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _,sql in tables)
        output = [f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name,_ in tables]
        output += ['', 'returns', *('\t'.join(map(str,row)) for row in returns), '',
                   'class result initializations', *('\t'.join(map(str,row)) for row in calls)]
        return '\n'.join(output)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work/'source'
    shutil.copytree(RELATIONAL/'tests/returns', source)
    run(['g++', '-std=c++20', source/'provider.cpp', source/'main.cpp', '-o', work/'native'], work/'native-build.log')
    run([work/'native'], work/'native-run.log')
    objects=[]
    for name in ('provider','main'):
        obj=work/f'{name}.dotobj'
        run([sys.executable, RELATIONAL/'compile.py', source/f'{name}.cpp', '--output', obj,
             '--work-dir', work/f'{name}-work', '--compiler', args.compiler.resolve()], work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source')
    generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--namespace','Review.Returns',
         '--class','Core','--entry','main',*objects],work/'link.log')
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true',
             '-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    unsupported=work/'unsupported-cleanup.dotobj'
    run([sys.executable,RELATIONAL/'compile.py',work/'original-source/unsupported_cleanup.cpp',
         '--output',unsupported,'--work-dir',work/'unsupported-cleanup-work',
         '--compiler',args.compiler.resolve()],work/'unsupported-cleanup-compile.log')
    with (work/'unsupported-cleanup-link.log').open('w') as log:
        result=subprocess.run(['dotnet',str(args.linker.resolve()),'link','--out',str(work/'unsupported-cleanup-generated'),
                               str(unsupported)],stdout=log,stderr=subprocess.STDOUT)
    assert result.returncode!=0
    assert 'explicit return-result unwinding facts' in (work/'unsupported-cleanup-link.log').read_text()
    print('PASS: explicit return construction/storage, self-pointer identity, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__=='__main__':main()
