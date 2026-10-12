#!/usr/bin/env python3
"""Address-only volatile designation and direct record initializer array destinations."""
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
    source = work / 'source'; shutil.copytree(RELATIONAL / 'tests/address_record_elements', source)
    run(['g++', '-std=c++20', *[source / (name + '.cpp') for name in ('provider','consumer','oracle')], '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = {}
    for name in ('provider','consumer','unsupported_pointer'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = obj
        with sqlite3.connect(obj.as_uri() + '?mode=ro', uri=True) as db:
            assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
            assert not list(db.execute('PRAGMA foreign_key_check'))
    with sqlite3.connect(objects['consumer']) as db:
        addresses = db.execute("SELECT n.id,n.operator,n.value_category FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='operand' AND e.ordinal=0 JOIN nodes c ON c.id=e.child_id WHERE n.operator='address-of' AND c.operator IN ('points-to-field','subscript')").fetchall()
        assert len(addresses) == 2, addresses
    with sqlite3.connect(objects['provider']) as db:
        rows = db.execute("SELECT n.id,n.kind FROM array_initializer_elements a JOIN nodes n ON n.id=a.value_node_id JOIN record_initializers r ON r.node_id=n.id").fetchall()
        assert len(rows) == 2, rows
    (work / 'database-summary.txt').write_text(repr(addresses) + '\n' + repr(rows) + '\n')
    source.rename(work / 'original-source')
    linker = args.linker.resolve()
    run(['dotnet', linker, 'link', '--out', work / 'unsupported-generated', '--unresolved-providers', 'required-partial', objects['unsupported_pointer']], work / 'unsupported-link.log', expected=1)
    negative = (work / 'unsupported-link.log').read_text()
    assert 'volatile access emission is not yet implemented' in negative and 'unsupported_pointer.cpp' in negative, negative
    generated = work / 'generated'
    run(['dotnet', linker, 'link', '--out', generated, '--namespace', 'Review.Addresses', '--class', 'Core', objects['provider'], objects['consumer']], work / 'link.log')
    (generated / 'Smoke.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><OutputType>Exe</OutputType><AssemblyName>AddressRecordSmoke</AssemblyName></PropertyGroup></Project>\n')
    (generated / 'Smoke.cs').write_text('''using Review.Addresses;
internal static unsafe class Smoke
{
    public static int Main()
    {
        if (Core.inspect_entries() != 0) return 1;
        Core.reset_calls();
        int* field = Core.field_address();
        if (field != &Core.holder->field || *field != 23 || Core.receiver_calls() != 1 || Core.index_calls() != 0) return 2;
        int* indexed = Core.indexed_address();
        int* values = (int*)&Core.holder->values;
        if (indexed != &values[1] || *indexed != 37 || Core.receiver_calls() != 2 || Core.index_calls() != 1) return 3;
        *field = 47;
        *indexed = 53;
        if (Core.holder->canary != 11 || Core.holder->field != 47 || values[0] != 31 || values[1] != 53 || values[2] != 41) return 4;
        if ((byte*)Core.entry_address(1) - (byte*)Core.entry_address(0) != sizeof(Core.Entry)) return 5;
        for (int attempt = 0; attempt < 3; ++attempt)
        {
            global::System.GC.Collect(2, global::System.GCCollectionMode.Forced, true, true);
            if (field != &Core.holder->field || Core.inspect_entries() != 0) return 6;
        }
        return 0;
    }
}
''')
    run(['dotnet', 'build', generated / 'Smoke.csproj', '-c', 'Release'], work / 'managed-build.log')
    run(['dotnet', generated / 'bin/Release/net10.0/AddressRecordSmoke.dll'], work / 'managed-run.log')
    if args.nativeaot:
        output = work / 'nativeaot'
        run(['dotnet', 'publish', generated / 'Smoke.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / 'nativeaot-build.log')
        run([output / 'AddressRecordSmoke'], work / 'nativeaot-run.log')
    print('PASS: addressed volatile field/indexed location, receiver/index once, volatile pointer-load rejection; direct enum/char[48] record array data, exact stride and neighbor bytes; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
