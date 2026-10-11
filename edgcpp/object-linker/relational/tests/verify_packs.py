#!/usr/bin/env python3
"""Explicit argument-pack containers and selected template identities."""
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
        arguments = list(db.execute('''SELECT t.name,a.instance_id,a.ordinal,a.parent_ordinal,
            a.kind,a.constant_text,selected.name,a.is_pack_expansion
            FROM template_arguments a JOIN template_instances i ON i.id=a.instance_id
            JOIN templates t ON t.id=i.template_id LEFT JOIN templates selected ON selected.id=a.template_id
            ORDER BY a.instance_id,a.ordinal'''))
        assert any(row[4] == 'pack' for row in arguments), arguments
        assert db.execute('''SELECT count(*) FROM template_arguments p
            WHERE p.kind='pack' AND NOT EXISTS(SELECT 1 FROM template_arguments a
            WHERE a.instance_id=p.instance_id AND a.parent_ordinal=p.ordinal)''').fetchone()[0] > 0
        if path.stem == 'provider':
            assert any(row[4] == 'template' and row[6] == 'Box' for row in arguments), arguments
            assert any(row[4] == 'constant' and row[5] == '-3' and row[3] is not None for row in arguments), arguments
            assert any(row[4] == 'type' and row[3] is not None for row in arguments), arguments
            assert db.execute('''SELECT count(*) FROM types
                WHERE kind='record' AND is_dependent=1''').fetchone()[0] > 0
            assert db.execute('''SELECT count(*) FROM template_formals f JOIN templates t ON t.id=f.template_id
                WHERE t.name='Box' AND f.kind='type' ''').fetchone()[0] > 0
            assert db.execute('''SELECT count(*) FROM template_formals f JOIN templates t ON t.id=f.template_id
                WHERE t.name='Wrapped' AND f.kind='template' ''').fetchone()[0] > 0
            assert db.execute("SELECT count(*) FROM empty_parameter_packs WHERE name='values'").fetchone()[0] > 0
        tables = list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert not any('BLOB' in sql.upper() or 'JSON' in sql.upper() for _, sql in tables)
        output = [f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{name}: {db.execute("SELECT COUNT(*) FROM " + name).fetchone()[0]} rows' for name, _ in tables]
        output += ['', 'template arguments', *('\t'.join(map(str,row)) for row in arguments)]
        output += ['', 'template formals', *('\t'.join(map(str,row)) for row in db.execute('''
            SELECT t.name,f.ordinal,f.kind,f.name,f.parameter_depth,f.parameter_position,f.is_pack
            FROM template_formals f JOIN templates t ON t.id=f.template_id ORDER BY f.template_id,f.ordinal'''))]
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
    shutil.copytree(RELATIONAL/'tests/packs', source)
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
         '--namespace', 'Review.Packs', '--class', 'Core', '--entry', 'main', *objects], work/'link.log')
    run(['dotnet', 'run', '--project', generated/'Translated.csproj', '-c', 'Release'], work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated/'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work/'nativeaot'], work/'nativeaot-build.log')
        apps = [p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix == '' and p.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work/'nativeaot-run.log')
    print('PASS: relational template packs and identities, native/source-hidden C#' +
          (', NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
