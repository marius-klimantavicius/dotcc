#!/usr/bin/env python3
"""Unordered temporary groups follow chosen completion order, preserving ordered neighbors."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve(); work.mkdir(parents=True, exist_ok=False)
    source = work / 'source'; shutil.copytree(RELATIONAL / 'tests/unordered_expression_temporaries', source)
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'main.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = {}
    for name in ('provider','main','unsupported_conditional'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = obj
        with sqlite3.connect(obj.as_uri() + '?mode=ro', uri=True) as db:
            assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
            assert not list(db.execute('PRAGMA foreign_key_check'))
    with sqlite3.connect(objects['provider']) as db:
        rows = db.execute("""SELECT d.ordinal,i.id,i.kind,i.unordered,t.name,i.conditional
            FROM symbols f JOIN initializations outer_init ON outer_init.function_id=f.id
            JOIN lifetime_destructions d ON d.lifetime_id=outer_init.init_expr_lifetime_id
            JOIN initializations i ON i.id=d.initialization_id JOIN types t ON t.id=i.type_id
            WHERE f.name='evaluate' ORDER BY d.ordinal""").fetchall()
        assert len(rows) == 3, rows
        assert [(unordered,name,conditional) for _,_,_,unordered,name,conditional in rows] == [(1,'Value',0),(1,'Value',0),(0,'Receiver',0)], rows
    (work / 'database-summary.txt').write_text(repr(rows) + '\n')
    source.rename(work / 'original-source')
    run(['dotnet', args.linker.resolve(), 'link', '--out', work / 'unsupported-generated',
         '--unresolved-providers', 'required-partial', objects['unsupported_conditional']], work / 'unsupported-link.log', expected=1)
    negative = (work / 'unsupported-link.log').read_text()
    assert 'Unordered temporary cleanup requires linear expression expansion' in negative, negative
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', objects['provider'], objects['main']], work / 'link.log')
    run(['dotnet', 'run', '--project', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-run.log')
    if args.nativeaot:
        output = work / 'nativeaot'
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / 'nativeaot-build.log')
        apps = [path for path in output.iterdir() if path.is_file() and path.suffix == '' and path.stat().st_mode & 0o111]
        assert len(apps) == 1, apps
        run(apps, work / 'nativeaot-run.log')
    print('PASS: exact contiguous unordered expression-temp group; reverse chosen completed constructions, ordered receiver neighbor preserved, failed-construction flags and call-failure cleanup, caller-result self identity; oracle accepts either unspecified argument order; conditional branch producer explicit boundary; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
