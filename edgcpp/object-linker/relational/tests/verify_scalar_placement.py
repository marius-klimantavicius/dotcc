#!/usr/bin/env python3
"""Use EDG-selected scalar zero and expression placement actions."""
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
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work / 'source.cpp'
    shutil.copy2(RELATIONAL / 'tests/scalar_placement.cpp', source)
    run(['g++', '-std=c++17', source, '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    obj = work / 'scalar-placement.dotobj'
    run([sys.executable, RELATIONAL / 'compile.py', source, '--std', 'c++17', '--output', obj,
         '--work-dir', work / 'compile-work', '--compiler', args.compiler.resolve()], work / 'compile.log')
    with sqlite3.connect(obj) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = db.execute('''SELECT n.id,a.construction_null_check,t.kind,i.kind,
            protected.child_id=initialize.child_id,i.constructor_symbol_id,i.destructor_symbol_id
            FROM nodes n JOIN allocations a ON a.node_id=n.id
            JOIN types t ON t.id=a.allocated_type_id
            JOIN edges initialize ON initialize.parent_id=n.id AND initialize.role='initialize'
            JOIN edges protected ON protected.parent_id=n.id AND protected.role='exception-protected'
            JOIN nodes action ON action.id=initialize.child_id
            JOIN initializations i ON i.id=action.initialization_id
            WHERE n.kind='new' AND n.operator='placement' ORDER BY n.id''').fetchall()
        assert len(rows) == 3 and [row[3] for row in rows].count('zero') == 1 and [row[3] for row in rows].count('expression') == 2, rows
        assert all(row[1] == 0 and row[2] == 'i32' and row[4] and row[5] is None and row[6] is None for row in rows), rows
        (work / 'database-summary.txt').write_text(repr(rows) + '\n')
    source.rename(work / 'original-source.cpp')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Review.ScalarPlacement',
         '--class', 'Core', '--entry', 'main', obj], work / 'link.log')
    run(['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-build.log')
    run(['dotnet', generated / 'bin/Release/net10.0/Translated.dll'], work / 'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release',
             '-p:PublishAot=true', '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        run([work / 'nativeaot/Translated'], work / 'nativeaot-run.log')
    print('PASS: recorded scalar zero/literal-expression/reference-expression placement actions and protected aliases, once-only placement/allocator calls; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
