#!/usr/bin/env python3
"""EDG-selected public/ambiguous bases and complete-payload catch offsets."""
import argparse
import csv
from pathlib import Path
import re
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path) as db:
        db.row_factory = sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone()) == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = db.execute('''SELECT b.*,owner.name AS owner,base.name AS base FROM base_subobjects b
            JOIN types owner ON owner.id=b.owner_type_id JOIN types base ON base.id=b.base_type_id
            WHERE owner.name IN ('PublicIndirect','PrivateBase','Ambiguous','Diamond') ORDER BY owner.id,b.ordinal''').fetchall()
        roots = [row for row in rows if row['base'] == 'Root']
        assert any(row['owner'] == 'PublicIndirect' and not row['is_direct'] and row['is_public'] == 1 and row['is_ambiguous'] == 0 and row['byte_offset'] > 0 for row in roots)
        assert any(row['owner'] == 'PrivateBase' and row['is_public'] == 0 and row['is_ambiguous'] == 0 for row in roots)
        ambiguous = [row for row in roots if row['owner'] == 'Ambiguous']
        assert len(ambiguous) == 2 and all(row['is_public'] == 1 and row['is_ambiguous'] == 1 for row in ambiguous)
        shared = [row for row in rows if row['owner'] == 'Diamond' and row['base'] == 'Shared']
        assert len(shared) == 1 and shared[0]['is_virtual'] == 1 and shared[0]['is_public'] == 1 and shared[0]['is_ambiguous'] == 0
        value_offset = db.execute("SELECT byte_offset FROM fields WHERE owner_type_id=? AND name='value'", (shared[0]['base_type_id'],)).fetchone()[0]
        assert not list(db.execute('SELECT * FROM class_conversions c JOIN base_subobjects b ON b.id=c.base_subobject_id WHERE b.is_virtual=1'))
        assert not db.execute('SELECT count(*) FROM vptr_writes').fetchone()[0]
        thrown = db.execute('''WITH RECURSIVE views(id) AS (
            SELECT type_id FROM throws WHERE is_rethrow=0
            UNION SELECT t.underlying_type_id FROM views v JOIN types t ON t.id=v.id WHERE t.underlying_type_id IS NOT NULL
            ) SELECT t.name FROM views v JOIN types t ON t.id=v.id WHERE t.kind='record' ''').fetchall()
        assert {row[0] for row in thrown} == {'PublicIndirect','PrivateBase','Ambiguous','Diamond'}, thrown
        return shared[0]['byte_offset'], value_offset, '\n'.join(str(dict(row)) for row in rows)


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
    shutil.copytree(RELATIONAL / 'tests/class_exception_bases', source)
    run(['g++', '-std=c++20', source / 'program.cpp', source / 'oracle.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-results.tsv')
    obj = work / 'program.dotobj'
    run([sys.executable, RELATIONAL / 'compile.py', source / 'program.cpp', '--output', obj,
         '--work-dir', work / 'compile-work', '--compiler', args.compiler.resolve()], work / 'compile.log')
    offset, value_offset, summary = inspect(obj)
    (work / 'database-summary.txt').write_text(summary + '\n')
    expected = (work / 'native-results.tsv').read_text()
    assert expected == f'Shared-offset\t{offset}\nclass-base-matching\tpass\n', expected
    source.rename(work / 'original-source')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Review.ClassBases',
         '--class', 'Core', '--unresolved-providers', 'required-partial', obj], work / 'link.log')
    with (generated / 'Imports.tsv').open() as stream:
        imports = list(csv.DictReader(stream, delimiter='\t'))
    assert len(imports) == 1 and imports[0]['source_name'] == 'Diamond' and imports[0]['kind'] == 'function', imports
    declaration = imports[0]['declaration']
    signature = re.fullmatch(r'public static partial void (\w+)\((\w+)\* receiver, \2\* argument0\);', declaration)
    assert signature, declaration
    constructor, record = signature.groups()
    (generated / 'Provider.cs').write_text(f'''// Genuine adapter for the byte-copy constructor body in original-source/oracle.cpp.
namespace Review.ClassBases;
public static unsafe partial class Core
{{
    public static partial void {constructor}({record}* receiver, {record}* argument0)
        => global::System.Runtime.InteropServices.NativeMemory.Copy(argument0, receiver, (nuint)sizeof({record}));
}}
''')
    (generated / 'Smoke.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><OutputType>Exe</OutputType><AssemblyName>ClassBasesSmoke</AssemblyName></PropertyGroup></Project>\n')
    (generated / 'Smoke.cs').write_text(f'''using Review.ClassBases;
internal static unsafe class Smoke
{{
    public static int Main()
    {{
        if (Core.public_indirect() != 0 || Core.private_base() != 0 || Core.ambiguous_base() != 0) return 1;
        Core.{record} source = default;
        *(int*)((byte*)&source + {offset + value_offset}) = 31;
        if (Core.virtual_diamond(&source, {offset}U) != 0) return 2;
        global::System.Console.WriteLine("Shared-offset\\t{offset}");
        global::System.Console.WriteLine("class-base-matching\\tpass");
        return 0;
    }}
}}
''')
    run(['dotnet', 'build', generated / 'Smoke.csproj', '-c', 'Release'], work / 'managed-build.log')
    run(['dotnet', generated / 'bin/Release/net10.0/ClassBasesSmoke.dll'], work / 'managed-results.tsv')
    assert (work / 'managed-results.tsv').read_text() == expected
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Smoke.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        run([work / 'nativeaot/ClassBasesSmoke'], work / 'nativeaot-results.tsv')
        assert (work / 'nativeaot-results.tsv').read_text() == expected
    missing = work / 'missing-base-flags.dotobj'
    shutil.copy2(obj, missing)
    with sqlite3.connect(missing) as db:
        db.execute('UPDATE base_subobjects SET is_public=NULL,is_ambiguous=NULL')
    run(['dotnet', args.linker.resolve(), 'link', '--out', work / 'missing-flags-generated',
         '--unresolved-providers', 'required-partial', missing], work / 'missing-flags-link.log', expected=1)
    assert 'exception base matching requires recorded public/ambiguity flags' in (work / 'missing-flags-link.log').read_text()
    print('PASS: exact public indirect/private/ambiguous base flags and virtual-diamond payload address matching/rethrow, genuine supplied byte-copy provider, missing-flag boundary; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
