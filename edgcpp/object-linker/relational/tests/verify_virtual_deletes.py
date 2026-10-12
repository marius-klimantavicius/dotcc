#!/usr/bin/env python3
"""Run the exact selected EDG deleting entry, complete body and null guard."""
import argparse
import csv
import hashlib
from pathlib import Path
import re
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run
from verify_virtual_delete_metadata import inspect


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--object', type=Path, help='Reuse a retained exact fixture object without another export')
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work / 'source.cpp'
    shutil.copy2(RELATIONAL / 'tests/virtual_deletes.cpp', source)
    compiler, linker = args.compiler.resolve(), args.linker.resolve()
    (work / 'tool-source-hashes.txt').write_text('\n'.join(
        hashlib.sha256(path.read_bytes()).hexdigest() + '  ' + str(path)
        for path in (compiler, linker, source, Path(__file__).resolve())) + '\n')
    run(['g++', '-std=c++17', source, '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    obj = work / 'virtual-deletes.dotobj'
    if args.object:
        shutil.copy2(args.object.resolve(), obj)
        (work / 'object-provenance.txt').write_text(str(args.object.resolve()) + '\n' + hashlib.sha256(obj.read_bytes()).hexdigest() + '\n')
    else:
        run([sys.executable, RELATIONAL / 'compile.py', source, '--std', 'c++17', '--output', obj,
             '--work-dir', work / 'compile-work', '--compiler', compiler], work / 'compile.log')
    (work / 'database-summary.txt').write_text(inspect(obj) + '\n')
    external_source = work / 'external-source'
    external_source.mkdir()
    for name in ('external_virtual_delete.h', 'external_virtual_delete.cpp', 'external_direct_delete.cpp'):
        shutil.copy2(RELATIONAL / 'tests' / name, external_source / name)
    external_objects = {}
    for name in ('external_virtual_delete', 'external_direct_delete'):
        target = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', external_source / (name + '.cpp'), '--std', 'c++17',
             '--output', target, '--work-dir', work / (name + '-work'), '--compiler', compiler], work / (name + '-compile.log'))
        external_objects[name] = target
    external_source.rename(work / 'original-external-source')
    for name, target in external_objects.items():
        directory = work / (name + '-generated')
        run(['dotnet', linker, 'link', '--out', directory, '--unresolved-providers', 'required-partial', target], work / (name + '-link.log'))
        with (directory / 'Imports.tsv').open() as stream:
            imports = list(csv.DictReader(stream, delimiter='\t'))
        if name == 'external_virtual_delete':
            assert not imports, imports
        else:
            assert len(imports) == 1 and imports[0]['source_name'] == '~ExternalBase' and imports[0]['kind'] == 'function', imports
    source.rename(work / 'original-source.cpp')
    generated = work / 'generated'
    run(['dotnet', linker, 'link', '--out', generated, '--namespace', 'Review.VirtualDelete',
         '--class', 'Core', '--entry', 'main', obj], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    with sqlite3.connect(obj) as db:
        deletion = db.execute("SELECT id FROM nodes WHERE kind='delete' AND operator='virtual-destroy'").fetchone()[0]
    helper = re.search(r'private static void __delete_object_' + str(deletion) + r'\(.*?\n    \}', code, re.S)
    assert helper, 'Saved-pointer deletion helper missing'
    assert ' != null)' in helper[0] and helper[0].count('.Invoke(') == 1
    assert '__dtor_' not in helper[0], 'Virtual deletion separately calls a source destructor'
    # The only destructor/deallocation sequence is the captured genuine D0
    # body. main checks trace 1234, exact pointer/size, once-only evaluation,
    # and a null operand which leaves destruction/deallocation unchanged.
    run(['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-build.log')
    run(['dotnet', generated / 'bin/Release/net10.0/Translated.dll'], work / 'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release',
             '-p:PublishAot=true', '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        run([work / 'nativeaot/Translated'], work / 'nativeaot-run.log')
    print('PASS: exact complete/D0 entries, genuine supplied deleting body, receiver once, saved pointer/null guard, member/base/destructor/delete trace and sized deallocator, external logical destructor metadata versus direct provider use; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
