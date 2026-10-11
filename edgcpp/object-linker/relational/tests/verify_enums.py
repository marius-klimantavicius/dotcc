#!/usr/bin/env python3
"""Named enum metadata, exact unsigned values and cross-unit execution."""
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
        values = list(db.execute('''SELECT t.name,v.name,v.value_text,underlying.builtin_name,
            e.is_scoped,e.has_explicit_base FROM enum_values v
            JOIN types t ON t.id=v.enum_type_id JOIN enums e ON e.type_id=t.id
            JOIN types underlying ON underlying.id=t.underlying_type_id
            ORDER BY t.name,v.ordinal'''))
        assert ('Status', 'ready', '513', 'unsigned short', 1, 1) in values, values
        assert ('Legacy', 'negative', '-3', 'int', 0, 0) in values, values
        assert ('Wide', 'high', '18446744073709551615', 'unsigned long long', 1, 1) in values, values
        selected = list(db.execute('''SELECT n.id,t.name,v.name FROM nodes n
            JOIN enum_values v ON v.id=n.enum_value_id JOIN types t ON t.id=v.enum_type_id
            ORDER BY n.id'''))
        assert selected, 'Expected selected named enumerator references'
        tables = list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _, sql in tables)
        output = [f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name, _ in tables]
        output += ['', 'enum values', *('\t'.join(map(str, row)) for row in values), '',
                   'selected references', *('\t'.join(map(str, row)) for row in selected)]
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
    shutil.copytree(RELATIONAL/'tests/enums', source)
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
         '--namespace', 'Review.Enums', '--class', 'Core', '--entry', 'main', *objects], work/'link.log')
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    code = (generated/'Translated.cs').read_text()
    assert 'enum Status' in code and 'enum Wide' in code and 'Status.ready' in code
    if args.nativeaot:
        run(['dotnet', 'publish', generated/'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work/'nativeaot'], work/'nativeaot-build.log')
        apps = [p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix == '' and p.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work/'nativeaot-run.log')
    print('PASS: relational named enums, exact values, native/source-hidden C#' +
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
