#!/usr/bin/env python3
"""EDG-selected pointer compound offsets, lvalue identity and operand order."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def original(db, type_id):
    while True:
        kind, underlying, pointee, size = db.execute(
            'SELECT kind,underlying_type_id,pointee_type_id,size FROM types WHERE id=?',
            (type_id,)).fetchone()
        if kind not in ('typedef', 'qualified'):
            return kind, pointee, size
        type_id = underlying


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = list(db.execute("SELECT id,operator,type_id,operation_type_id,value_category,evaluation_order "
                               "FROM nodes WHERE operator IN ('pointer-add-assign','pointer-subtract-assign') ORDER BY id"))
        assert {r[1] for r in rows} == {'pointer-add-assign', 'pointer-subtract-assign'}, rows
        layouts = []
        for node, operator, result, operation, category, order in rows:
            assert category in ('lvalue', 'prvalue') and order in ('unspecified', 'left-to-right', 'right-to-left'), rows
            assert operation is not None and original(db, operation)[0] == 'pointer', rows
            assert original(db, result)[0] == 'pointer', rows
            operands = list(db.execute("SELECT e.ordinal,COALESCE(n.execution_type_id,n.type_id) FROM edges e "
                                       "JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=? AND e.role='operand' ORDER BY e.ordinal", (node,)))
            assert [r[0] for r in operands] == [0, 1], operands
            left, right = (original(db, r[1]) for r in operands)
            assert left[0] == 'pointer' and right[0] in ('i8', 'u8', 'i16', 'u16', 'i32', 'u32', 'i64', 'u64'), operands
            layouts.append(original(db, left[1])[2])
        if path.stem == 'provider':
            assert {1, 4} <= set(layouts), layouts
        else:
            assert any(r[5] == 'right-to-left' for r in rows), rows
            lvalue_uses = list(db.execute("SELECT p.operator,c.value_category FROM nodes p JOIN edges e "
                                         "ON e.parent_id=p.id AND e.role='operand' AND e.ordinal=0 JOIN nodes c ON c.id=e.child_id "
                                         "WHERE p.operator IN ('address-of','reference-to','assign','pointer-add-assign','pointer-subtract-assign') "
                                         "AND c.operator IN ('pointer-add-assign','pointer-subtract-assign')"))
            assert {r[0] for r in lvalue_uses} == {'address-of', 'reference-to', 'assign', 'pointer-add-assign', 'pointer-subtract-assign'}, lvalue_uses
            assert all(r[1] == 'lvalue' for r in lvalue_uses), lvalue_uses
        tables = list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _, sql in tables)
        output = [f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name, _ in tables]
        output += ['', 'selected pointer compounds', *('\t'.join(map(str, row)) for row in rows)]
        return '\n'.join(output)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve(); work.mkdir(parents=True, exist_ok=False)
    source = work/'source'; shutil.copytree(RELATIONAL/'tests/pointer_compounds', source)
    run(['g++', '-std=c++20', source/'provider.cpp', source/'main.cpp', '-o', work/'native'], work/'native-build.log')
    run([work/'native'], work/'native-run.log')
    objects = []
    for name in ('provider', 'main'):
        obj = work/f'{name}.dotobj'
        run([sys.executable, RELATIONAL/'compile.py', source/f'{name}.cpp', '--output', obj,
             '--work-dir', work/f'{name}-work', '--compiler', args.compiler.resolve()], work/f'{name}-compile.log')
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n\n'.join(inspect(p) for p in objects)+'\n')
    source.rename(work/'original-source'); generated = work/'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Review.PointerCompounds',
         '--class', 'Core', '--entry', 'main', *objects], work/'link.log')
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated/'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work/'nativeaot'], work/'nativeaot-build.log')
        apps = [p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix == '' and p.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work/'nativeaot-run.log')
    print('PASS: selected pointer compounds, lvalue identity and operand effects, native/source-hidden C#'+
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
