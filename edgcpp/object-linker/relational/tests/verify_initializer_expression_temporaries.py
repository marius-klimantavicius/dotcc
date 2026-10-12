#!/usr/bin/env python3
"""Selected initializer-expression temporary storage, success flags and cleanup scope."""
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
    source = work / 'source'; shutil.copytree(RELATIONAL / 'tests/initializer_expression_temporaries', source)
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'main.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = {}
    for name in ('provider','main','unsupported_throwing_cleanup'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = obj
        with sqlite3.connect(obj.as_uri() + '?mode=ro', uri=True) as db:
            assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
            assert not list(db.execute('PRAGMA foreign_key_check'))
    with sqlite3.connect(objects['provider']) as db:
        rows = db.execute("""SELECT outer_init.id,outer_init.kind,outer_init.init_expr_lifetime_id,
            temporary.id,temporary.kind,temporary.temporary_lifetime,temporary.variable_symbol_id,n.id,n.kind
            FROM symbols f JOIN initializations outer_init ON outer_init.function_id=f.id
            JOIN lifetime_destructions d ON d.lifetime_id=outer_init.init_expr_lifetime_id
            JOIN initializations temporary ON temporary.id=d.initialization_id
            JOIN nodes n ON n.initialization_id=temporary.id
            WHERE f.name='wrapper'""").fetchall()
        assert len(rows) == 1 and rows[0][1] == 'class-result' and rows[0][4:7] == ('class-result',1,None) and rows[0][8] == 'temporary', rows
    (work / 'database-summary.txt').write_text(repr(rows) + '\n')
    source.rename(work / 'original-source')
    linker = args.linker.resolve()
    run(['dotnet', linker, 'link', '--out', work / 'unsupported-generated', objects['unsupported_throwing_cleanup']], work / 'unsupported-link.log', expected=1)
    negative = (work / 'unsupported-link.log').read_text()
    assert 'Caller-provided return with potentially throwing cleanup requires explicit return-result unwinding facts' in negative, negative
    generated = work / 'generated'
    run(['dotnet', linker, 'link', '--out', generated, '--entry', 'main', objects['provider'], objects['main']], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    assert '__temporary_' in code and '__unwinding_initializer_' in code
    run(['dotnet', 'run', '--project', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-run.log')
    if args.nativeaot:
        output = work / 'nativeaot'
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / 'nativeaot-build.log')
        apps = [path for path in output.iterdir() if path.is_file() and path.suffix == '' and path.stat().st_mode & 0o111]
        assert len(apps) == 1, apps
        run(apps, work / 'nativeaot-run.log')
    print('PASS: exact initializer expression lifetime/temporary action identity; complete caller result self-address, success order, temporary-construction failure and outer-call failure cleanup; throwing return cleanup explicit boundary; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
