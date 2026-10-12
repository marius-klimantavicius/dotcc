#!/usr/bin/env python3
"""Discarded void conversions preserve evaluation and do not bind a source '_' global."""
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
    source = work / 'source'; source.mkdir()
    for name in ('void_discards', 'unsupported_void_volatile'):
        shutil.copy2(RELATIONAL / 'tests' / (name + '.cpp'), source)
    run(['g++', '-std=c++20', source / 'void_discards.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = {}
    for name in ('void_discards', 'unsupported_void_volatile'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = obj
        with sqlite3.connect(obj.as_uri() + '?mode=ro', uri=True) as db:
            assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
            assert not list(db.execute('PRAGMA foreign_key_check'))
    source.rename(work / 'original-source')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', objects['void_discards']], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    assert 'unchecked((void)' not in code and '// (void)0;' in code, code
    assert 'public static int* _ =>' not in code, code
    run(['dotnet', 'run', '--project', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-run.log')
    run(['dotnet', args.linker.resolve(), 'link', '--out', work / 'unsupported-generated', '--entry', 'main', objects['unsupported_void_volatile']], work / 'unsupported-link.log', expected=1)
    diagnostic = (work / 'unsupported-link.log').read_text()
    assert 'volatile access emission is not yet implemented' in diagnostic and 'unsupported_void_volatile.cpp' in diagnostic, diagnostic
    if args.nativeaot:
        output = work / 'nativeaot'
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / 'nativeaot-build.log')
        apps = [p for p in output.iterdir() if p.is_file() and p.suffix == '' and p.stat().st_mode & 0o111]
        assert len(apps) == 1, apps
        run(apps, work / 'nativeaot-run.log')
    print('PASS: void casts preserve once-only calls/assignments and iterator effects; source underscore global unchanged; real volatile read explicitly rejected; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
