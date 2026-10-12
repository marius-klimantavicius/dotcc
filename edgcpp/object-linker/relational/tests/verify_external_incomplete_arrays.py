#!/usr/bin/env python3
"""Exact incomplete-array address contracts without invented extents."""
import argparse
import csv
import re
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def smoke(directory, class_name):
    (directory / 'Smoke.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><OutputType>Exe</OutputType><AssemblyName>IncompleteArraysSmoke</AssemblyName></PropertyGroup></Project>\n')
    (directory / 'Smoke.cs').write_text(f'''using Review.IncompleteArrays;
internal static unsafe class Smoke
{{
    public static int Main()
    {{
        ulong* values = {class_name}.values_address();
        byte* bytes = {class_name}.bytes_address();
        for (int attempt = 0; attempt < 3; ++attempt)
        {{
            global::System.GC.Collect(2, global::System.GCCollectionMode.Forced, true, true);
            if ({class_name}.values_address() != values || {class_name}.bytes_address() != bytes) return 4;
            if ({class_name}.run_arrays() != 0) return 5;
        }}
        return 0;
    }}
}}
''')


def execute(directory, work, label, nativeaot):
    run(['dotnet', 'build', directory / 'Smoke.csproj', '-c', 'Release'], work / (label + '-managed-build.log'))
    run(['dotnet', directory / 'bin/Release/net10.0/IncompleteArraysSmoke.dll'], work / (label + '-managed-run.log'))
    if nativeaot:
        output = work / (label + '-nativeaot')
        run(['dotnet', 'publish', directory / 'Smoke.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / (label + '-nativeaot-build.log'))
        run([output / 'IncompleteArraysSmoke'], work / (label + '-nativeaot-run.log'))


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
    shutil.copytree(RELATIONAL / 'tests/external_incomplete_arrays', source)
    run(['g++', '-std=c++20', source / 'consumer.cpp', source / 'provider.cpp', source / 'oracle.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = {}
    for name in ('consumer', 'provider'):
        target = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', target,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = target
    with sqlite3.connect(objects['consumer']) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = db.execute("SELECT s.name,s.is_definition,t.kind,t.element_count,t.size,t.is_incomplete FROM symbols s JOIN types t ON t.id=s.type_id WHERE s.name IN ('values','bytes','matrix') AND s.kind='global'").fetchall()
        assert len(rows) == 3 and all(row[1:] == (0, 'array', None, 0, 1) for row in rows), rows
        (work / 'database-summary.txt').write_text(repr(rows) + '\n')
    source.rename(work / 'original-source')
    linker = args.linker.resolve()
    run(['dotnet', linker, 'link', '--out', work / 'strict-generated', objects['consumer']], work / 'strict-link.log', expected=1)
    assert 'No definition for ' in (work / 'strict-link.log').read_text()
    provider = work / 'provider-generated'
    run(['dotnet', linker, 'link', '--out', provider, '--namespace', 'Review.IncompleteArrays', '--class', 'Data', objects['provider']], work / 'provider-link.log')
    generated = work / 'required-generated'
    run(['dotnet', linker, 'link', '--out', generated, '--namespace', 'Review.IncompleteArrays', '--class', 'Core', '--unresolved-providers', 'required-partial', objects['consumer']], work / 'required-link.log')
    code = (generated / 'Translated.cs').read_text()
    assert code.count('Fixed array storage') == 1 and '// Fixed array storage: int[2].' in code and 'AllocateArray' not in code
    with (generated / 'Imports.tsv').open() as stream:
        imports = list(csv.DictReader(stream, delimiter='\t'))
    assert {item['source_name'] for item in imports} == {'values','bytes','matrix'} and len(imports) == 3, imports
    glue = ['namespace Review.IncompleteArrays;', 'public static unsafe partial class Core', '{']
    for item in imports:
        declaration = item['declaration']
        signature = re.fullmatch(r'public static partial (\w+\*) \w+\(\);', declaration)
        assert signature, declaration
        expected_type = signature[1]
        if item['source_name'] == 'matrix':
            assert expected_type.startswith('__array_int_2'), declaration
        else:
            assert expected_type == ('ulong*' if item['source_name'] == 'values' else 'byte*'), declaration
        glue.append('    ' + declaration[:-1] + ' => (' + expected_type + ')Data.' + item['source_name'] + ';')
    glue.append('}')
    (generated / 'Provider.cs').write_text('\n'.join(glue) + '\n')
    shutil.copy2(provider / 'Translated.cs', generated / 'Data.cs')
    smoke(generated, 'Core')
    execute(generated, work, 'required', args.nativeaot)
    linked = work / 'linked-generated'
    run(['dotnet', linker, 'link', '--out', linked, '--namespace', 'Review.IncompleteArrays', '--class', 'Core', '--unresolved-providers', 'required-partial', objects['consumer'], objects['provider']], work / 'linked-link.log')
    with (linked / 'Imports.tsv').open() as stream:
        assert not list(csv.DictReader(stream, delimiter='\t'))
    smoke(linked, 'Core')
    execute(linked, work, 'linked', args.nativeaot)
    reverse = work / 'reverse-generated'
    run(['dotnet', linker, 'link', '--out', reverse, '--namespace', 'Review.IncompleteArrays', '--class', 'Core', '--unresolved-providers', 'required-partial', objects['provider'], objects['consumer']], work / 'reverse-link.log')
    with (reverse / 'Imports.tsv').open() as stream:
        assert not list(csv.DictReader(stream, delimiter='\t'))
    smoke(reverse, 'Core')
    execute(reverse, work, 'reverse', args.nativeaot)
    print('PASS: strict unresolved arrays; exact typed element-address imports, no invented extent/storage, genuine separately translated provider, cross-object complete definition selection, indexing/whole-array address identity/GC stability; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
