#!/usr/bin/env python3
"""EDG-selected base layout, conversions and construction/destruction order."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        tables = list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _, sql in tables)
        output = [f'{path.name}: {path.stat().st_size} bytes']
        for name, _ in tables:
            output.append(f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows')
        queries = {
            'base layout': '''SELECT owner.name,base.name,b.ordinal,b.direct_ordinal,b.byte_offset,
                b.is_direct,b.is_virtual,b.is_optimized_empty,b.access
                FROM base_subobjects b JOIN types owner ON owner.id=b.owner_type_id
                JOIN types base ON base.id=b.base_type_id ORDER BY b.owner_type_id,b.ordinal''',
            'selected casts': '''SELECT n.id,c.direction,t.name,c.byte_adjustment,c.preserves_null,
                c.source_representation,c.result_representation FROM class_conversions c
                JOIN nodes n ON n.id=c.node_id JOIN base_subobjects b ON b.id=c.base_subobject_id
                JOIN types t ON t.id=b.base_type_id ORDER BY n.id''',
            'construction and destruction': '''SELECT s.name,c.ordinal,c.kind,t.name,f.name,
                b.byte_offset,i.kind,ctor.name,dtor.name FROM constructor_initializers c
                JOIN symbols s ON s.id=c.function_id JOIN initializations i ON i.id=c.initialization_id
                LEFT JOIN base_subobjects b ON b.id=c.base_subobject_id
                LEFT JOIN types t ON t.id=b.base_type_id LEFT JOIN fields f ON f.id=c.field_id
                LEFT JOIN symbols ctor ON ctor.id=i.constructor_symbol_id
                LEFT JOIN symbols dtor ON dtor.id=i.destructor_symbol_id ORDER BY c.function_id,c.ordinal''',
        }
        for name, query in queries.items():
            cursor = db.execute(query)
            output += ['', name, '\t'.join(c[0] for c in cursor.description)]
            output += ['\t'.join('NULL' if value is None else str(value) for value in row) for row in cursor]
        if path.stem == 'provider':
            bases = list(db.execute('''SELECT t.name,b.byte_offset FROM base_subobjects b
                JOIN types owner ON owner.id=b.owner_type_id JOIN types t ON t.id=b.base_type_id
                WHERE owner.name='Derived' AND b.is_direct=1 ORDER BY b.direct_ordinal'''))
            assert bases == [('Left', 0), ('Right', 4)], bases
            for function, expected in (('Derived', ['Left', 'Right']), ('~Derived', ['Right', 'Left'])):
                selected = [r[0] for r in db.execute('''SELECT t.name FROM constructor_initializers c
                    JOIN symbols s ON s.id=c.function_id JOIN base_subobjects b ON b.id=c.base_subobject_id
                    JOIN types t ON t.id=b.base_type_id WHERE s.name=? ORDER BY c.ordinal''', (function,))]
                assert selected == expected, (function, selected)
            for direction, adjustment in (('to-base', 4), ('to-derived', -4)):
                assert db.execute('''SELECT COUNT(*) FROM class_conversions
                    WHERE direction=? AND byte_adjustment=? AND preserves_null=1''',
                    (direction, adjustment)).fetchone()[0] >= 1
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
    shutil.copytree(RELATIONAL/'tests/bases', source)
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
         '--namespace', 'Review.Bases', '--class', 'Core', '--entry', 'main', *objects], work/'link.log')
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated/'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work/'nativeaot'], work/'nativeaot-build.log')
        apps = [p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix == '' and p.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work/'nativeaot-run.log')
    print('PASS: relational bases/layout/casts, partial construction, native/source-hidden C#' +
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
