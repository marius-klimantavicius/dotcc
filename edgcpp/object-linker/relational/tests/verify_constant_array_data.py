#!/usr/bin/env python3
"""Recorded fixed integer array actions render as typed constant data spans."""
import argparse
from pathlib import Path
import re
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


ARRAYS = ('byte_values', 'signed_values', 'short_values', 'ushort_values', 'integer_values',
          'unsigned_values', 'wide_values', 'uwide_values', 'repeated_values', 'zero_values')


def inspect(path):
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = db.execute('''SELECT s.name,t.element_count,a.zero_initialize,e.array_index,e.repeat_count,n.kind,n.constant_text
            FROM symbols s JOIN global_initializers g ON g.symbol_id=s.id
            JOIN types t ON t.id=s.type_id JOIN array_initializers a ON a.node_id=g.initializer_node_id
            LEFT JOIN array_initializer_elements e ON e.initializer_node_id=a.node_id
            LEFT JOIN nodes n ON n.id=e.value_node_id ORDER BY s.id,e.ordinal''').fetchall()
        if path.stem == 'provider':
            assert set(ARRAYS).issubset({row[0] for row in rows}), rows
            assert any(row[2] for row in rows if row[0] in ('byte_values', 'integer_values', 'zero_values')), rows
            assert all(row[5] in ('constant', None) for row in rows if row[0] in ARRAYS), rows
            assert any(row[0] == 'relocations' and row[5] != 'constant' for row in rows), rows
        return path.name + ': ' + repr(rows)


def inspect_source(code):
    for name in ARRAYS:
        assert re.search(r'private static global::System.ReadOnlySpan<[^>]+> __' + name + r'_data =>', code), name
        body = re.search(r'private static void __' + name + r'_initialize\(\)\s*\{([^}]+)\}', code)
        assert body and f'__{name}_data.CopyTo(new global::System.Span<' in body.group(1), name
        assert 'NativeMemory.Clear' not in body.group(1) and ')[0] =' not in body.group(1), name
    assert '__relocations_data' not in code
    assert '((int**)relocations)[0] =' in code and '((int**)relocations)[1] =' in code
    # Every backing allocation stays before the explicit static-write phase.
    constructor = code.index('static Core()')
    assert all(match.start() < constructor for match in re.finditer('GC.AllocateArray<byte>', code))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work / 'source'
    shutil.copytree(RELATIONAL / 'tests/constant_array_data', source)
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'main.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = []
    for name in ('provider', 'main'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects.append(obj)
    (work / 'database-summary.txt').write_text('\n'.join(inspect(obj) for obj in objects) + '\n')
    source.rename(work / 'original-source')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--class', 'Core', '--entry', 'main', *objects], work / 'link.log')
    inspect_source((generated / 'Translated.cs').read_text())
    run(['dotnet', 'run', '--project', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        apps = [path for path in (work / 'nativeaot').iterdir() if path.is_file() and path.suffix == '' and path.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work / 'nativeaot-run.log')
    print('PASS: typed native integer constant data, zeros/repeated values, pinned array identity/mutation and exact relocation fallback; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
