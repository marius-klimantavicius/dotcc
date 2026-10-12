#!/usr/bin/env python3
"""Source-shaped for iterator commas preserve selected order and loop transfers."""
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
    source = work / 'source'; shutil.copytree(RELATIONAL / 'tests/for_comma_iterators', source)
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'main.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = []
    for name in ('provider','main'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects.append(obj)
        with sqlite3.connect(obj.as_uri() + '?mode=ro', uri=True) as db:
            assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
            assert not list(db.execute('PRAGMA foreign_key_check'))
    with sqlite3.connect(objects[0]) as db:
        rows = db.execute("SELECT n.id,p.kind,p.operator,e.role FROM nodes n JOIN edges e ON e.child_id=n.id JOIN nodes p ON p.id=e.parent_id WHERE n.operator='comma' AND n.domain='program'").fetchall()
        assert len(rows) == 4 and all((kind == 'for' and role == 'step') or (operator == 'comma' and role == 'operand') for _,kind,operator,role in rows), rows
    (work / 'database-summary.txt').write_text(repr(rows) + '\n')
    source.rename(work / 'original-source')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', *objects], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    assert '++index, ++cursor)' in code and 'append(1), ++count, append(2)' in code, code
    run(['dotnet', 'run', '--project', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-run.log')
    if args.nativeaot:
        output = work / 'nativeaot'
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / 'nativeaot-build.log')
        apps = [path for path in output.iterdir() if path.is_file() and path.suffix == '' and path.stat().st_mode & 0o111]
        assert len(apps) == 1, apps
        run(apps, work / 'nativeaot-run.log')
    print('PASS: ordered scalar/pointer and nested comma iterator headers; once-only call effects, continue and break; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
