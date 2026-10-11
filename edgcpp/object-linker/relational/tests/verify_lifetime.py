#!/usr/bin/env python3
"""Auditable cross-unit construction, destruction, exception and rethrow slice."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    queries = {
        'functions': '''SELECT s.name,f.kind,f.exception_policy FROM functions f
                       JOIN symbols s ON s.id=f.symbol_id ORDER BY s.id''',
        'construction': '''SELECT s.name,c.ordinal,c.kind,f.name,i.id,i.kind,
                          ctor.name,dtor.name,i.partial_aggregate
                          FROM constructor_initializers c JOIN symbols s ON s.id=c.function_id
                          JOIN initializations i ON i.id=c.initialization_id
                          LEFT JOIN fields f ON f.id=c.field_id
                          LEFT JOIN symbols ctor ON ctor.id=i.constructor_symbol_id
                          LEFT JOIN symbols dtor ON dtor.id=i.destructor_symbol_id
                          ORDER BY c.function_id,c.ordinal''',
        'cleanup': '''SELECT l.id,l.kind,l.parent_id,l.owner_node_id,d.ordinal,
                     i.id,v.name,dt.name,i.conditional,i.partial_aggregate,i.follows_exec
                     FROM lifetimes l JOIN lifetime_destructions d ON d.lifetime_id=l.id
                     JOIN initializations i ON i.id=d.initialization_id
                     LEFT JOIN symbols v ON v.id=i.variable_symbol_id
                     LEFT JOIN symbols dt ON dt.id=i.destructor_symbol_id
                     ORDER BY l.id,d.ordinal''',
        'handlers': '''SELECT h.try_node_id,h.ordinal,p.name,t.kind,h.initialization_id
                      FROM try_handlers h LEFT JOIN symbols p ON p.id=h.parameter_symbol_id
                      LEFT JOIN types t ON t.id=h.parameter_type_id
                      ORDER BY h.try_node_id,h.ordinal''',
        'throws': 'SELECT * FROM throws ORDER BY node_id',
    }
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        tables = list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _, sql in tables)
        output = [f'{path.name}: {path.stat().st_size} bytes']
        for name, _ in tables:
            output.append(f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows')
        for name, query in queries.items():
            cursor = db.execute(query)
            output += ['', name, '\t'.join(c[0] for c in cursor.description)]
            output += ['\t'.join('NULL' if value is None else str(value) for value in row) for row in cursor]
        if path.stem == 'provider':
            assert db.execute("SELECT COUNT(*) FROM functions WHERE kind='constructor'").fetchone()[0] >= 3
            assert db.execute('SELECT COUNT(*) FROM constructor_initializers').fetchone()[0] >= 4
            assert db.execute('SELECT COUNT(*) FROM lifetime_destructions').fetchone()[0] >= 3
            assert db.execute('SELECT COUNT(*) FROM throws WHERE is_rethrow=1').fetchone()[0] == 1
            assert db.execute('SELECT COUNT(*) FROM throws WHERE is_rethrow=0').fetchone()[0] == 1
            assert db.execute('SELECT COUNT(*) FROM try_handlers').fetchone()[0] == 2
            for function, order in (('Pair', ['first', 'second']), ('~Pair', ['second', 'first'])):
                fields = [r[0] for r in db.execute('''SELECT f.name FROM constructor_initializers c
                    JOIN symbols s ON s.id=c.function_id JOIN fields f ON f.id=c.field_id
                    WHERE s.name=? ORDER BY c.ordinal''', (function,))]
                assert fields == order
            assert db.execute('''SELECT i.follows_exec FROM initializations i
                JOIN symbols s ON s.id=i.variable_symbol_id WHERE s.name='outside' ''').fetchone() == (1,)
            assert db.execute('''SELECT s.name FROM throws t JOIN symbols s ON s.id=t.destructor_symbol_id
                WHERE t.is_rethrow=0''').fetchone() == ('~Failure',)
        else:
            assert db.execute('SELECT COUNT(*) FROM try_handlers').fetchone()[0] == 1
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
    shutil.copytree(RELATIONAL/'tests/lifetime', source)
    run(['g++', '-std=c++20', source/'provider.cpp', source/'main.cpp', '-o', work/'native'], work/'native-build.log')
    run([work/'native'], work/'native-run.log')
    objects = []
    for name in ('provider', 'main'):
        obj = work/f'{name}.dotobj'
        run([sys.executable, RELATIONAL/'compile.py', source/f'{name}.cpp', '--output', obj,
             '--work-dir', work/f'{name}-work', '--compiler', args.compiler.resolve()], work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects) + '\n')
    source.rename(work/'original-source')
    generated = work/'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated,
         '--namespace', 'Review.Lifetime', '--class', 'Core', '--entry', 'main', *objects], work/'link.log')
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated/'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work/'nativeaot'], work/'nativeaot-build.log')
        apps = [p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix == '' and p.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work/'nativeaot-run.log')
    print('PASS: relational lifecycle/EH rows, native comparison, source-hidden C#' +
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
