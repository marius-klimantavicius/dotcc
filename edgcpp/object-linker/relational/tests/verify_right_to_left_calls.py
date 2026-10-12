#!/usr/bin/env python3
"""Recorded RTL operator call arguments precede receiver designation, with exact result lifetime."""
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
    source = work / 'source'; shutil.copytree(RELATIONAL / 'tests/right_to_left_calls', source)
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'main.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = {}
    for name in ('provider','main','unsupported_shortcircuit'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = obj
        with sqlite3.connect(obj.as_uri() + '?mode=ro', uri=True) as db:
            assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
            assert not list(db.execute('PRAGMA foreign_key_check'))
    with sqlite3.connect(objects['provider']) as db:
        calls = db.execute("SELECT n.id,n.operator,n.evaluation_order FROM nodes n JOIN symbols f ON f.id=n.function_id WHERE f.name='perform' AND n.operator='dot-member-call'").fetchall()
        assert len(calls) == 1 and calls[0][2] == 'right-to-left', calls
        scopes = db.execute("SELECT n.id,n.kind,t.kind,d.initialization_id FROM nodes n JOIN symbols f ON f.id=n.function_id JOIN types t ON t.id=n.type_id JOIN lifetime_destructions d ON d.lifetime_id=n.lifetime_id WHERE f.name='perform' AND n.kind='lifetime-scope'").fetchall()
        assert len(scopes) == 1 and scopes[0][2] == 'lvalue-reference', scopes
    (work / 'database-summary.txt').write_text(repr(calls) + '\n' + repr(scopes) + '\n')
    source.rename(work / 'original-source')
    linker = args.linker.resolve()
    run(['dotnet', linker, 'link', '--out', work / 'unsupported-generated', '--unresolved-providers', 'required-partial', objects['unsupported_shortcircuit']], work / 'unsupported-link.log', expected=1)
    negative = (work / 'unsupported-link.log').read_text()
    assert 'right-to-left call requires ordered statement expansion in this context' in negative and 'unsupported_shortcircuit.cpp' in negative, negative
    generated = work / 'generated'
    run(['dotnet', linker, 'link', '--out', generated, '--entry', 'main', objects['provider'], objects['main']], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    assert '__scope_value_' in code and '__call_argument_' in code
    run(['dotnet', 'run', '--project', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-run.log')
    if args.nativeaot:
        output = work / 'nativeaot'
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / 'nativeaot-build.log')
        apps = [path for path in output.iterdir() if path.is_file() and path.suffix == '' and path.stat().st_mode & 0o111]
        assert len(apps) == 1, apps
        run(apps, work / 'nativeaot-run.log')
    print('PASS: recorded RHS-before-receiver operator call; receiver/reference-result identity after temporary cleanup; once-only constructor/receiver/call effects and each failure cleanup path; short-circuit unexpanded boundary; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
