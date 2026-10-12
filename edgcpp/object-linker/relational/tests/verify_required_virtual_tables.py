#!/usr/bin/env python3
"""Exact selected external vtable address-point providers with genuine translated entries."""
import argparse
import csv
from pathlib import Path
import re
import shutil
import sqlite3
import subprocess
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        tables = db.execute('''SELECT v.id,v.link_name,v.address_point_byte_offset,v.entry_size,v.is_definition,
            t.link_name,t.size,t.alignment FROM virtual_tables v JOIN types t ON t.id=v.owner_type_id ORDER BY v.id''').fetchall()
        assert len(tables) == 1 and tables[0][2:4] == (16, 8), tables
        assert tables[0][4] == int(path.stem == 'provider'), tables
        entries = db.execute('SELECT slot_index,entry_kind,target_variant FROM virtual_table_entries ORDER BY table_id,slot_index').fetchall()
        if path.stem == 'provider':
            assert entries == [(0, 'ordinary', 'ordinary')], entries
        else:
            assert not entries, entries
        assert db.execute('SELECT count(*) FROM vptr_writes WHERE table_id IS NOT NULL').fetchone()[0] > 0
        if path.stem == 'consumer':
            assert db.execute('SELECT count(*) FROM virtual_calls').fetchone()[0] > 0
        if path.stem == 'direct':
            assert db.execute('SELECT count(*) FROM virtual_calls').fetchone()[0] == 0
        functions = dict(db.execute("SELECT name,link_name FROM symbols WHERE kind='function' AND name IN ('eval','provided_table_address')"))
        layout = db.execute('''SELECT f.name,f.byte_offset,f.bit_offset,f.bit_width,t.kind,t.size
            FROM virtual_tables v JOIN fields f ON f.owner_type_id=v.owner_type_id
            JOIN types t ON t.id=f.type_id ORDER BY f.ordinal''').fetchall()
        return tables[0], functions, path.name + ': ' + repr((tables, entries, functions, layout)), tuple(layout)


def write_adapters(rows, path, direct=False):
    tables = [row for row in rows if row['kind'] == 'virtual-table']
    assert len(tables) == 1, rows
    table = tables[0]
    assert table['symbol_id'] == '' and int(table['virtual_table_id']) > 0
    assert table['address_point_byte_offset'] == '16' and table['entry_size'] == '8'
    match = re.fullmatch(r'public static partial nint\* (\S+)\(\);', table['declaration'])
    assert match, table
    lines = ['namespace Review.VirtualStorage;', 'public static unsafe partial class Core', '{',
             '    ' + table['declaration'][:-1], '    {',
             '        nint* address = (nint*)Provider.provided_table_address();',
             '        global::System.GC.Collect(global::System.GC.MaxGeneration, global::System.GCCollectionMode.Forced, blocking: true, compacting: true);',
             '        if (address != (nint*)Provider.provided_table_address()) throw new global::System.Exception("Translated table address changed");',
             '        return address;', '    }']
    functions = [row for row in rows if row['kind'] == 'function']
    assert {row['source_name'] for row in functions}.issubset({'eval', 'provided_table_address'}), rows
    assert {row['source_name'] for row in functions} == ({'eval'} if direct else {'provided_table_address'}), rows
    for row in functions:
        match = re.fullmatch(r'public static partial (.+) (\S+)\((.*)\);', row['declaration'])
        assert match, row
        result, name, parameters = match.groups()
        if row['source_name'] == 'provided_table_address':
            assert result == 'void*' and not parameters
            call = 'Provider.provided_table_address()'
        else:
            assert result == 'int' and parameters == 'External* receiver, int argument0', row
            call = 'Provider.eval((Provider.External*)receiver, argument0)'
        lines.append(f'    public static partial {result} {name}({parameters}) => {call};')
    lines.append('}')
    path.write_text('\n'.join(lines) + '\n')


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
    shutil.copytree(RELATIONAL / 'tests/required_virtual_tables', source)
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'consumer.cpp', source / 'main.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'direct.cpp', '-o', work / 'direct-native'], work / 'direct-native-build.log')
    run([work / 'direct-native'], work / 'direct-native-run.log')
    objects = {}
    inspected = {}
    for name in ('provider', 'consumer', 'main', 'direct'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = obj
        inspected[name] = inspect(obj)
    assert len({item[0][1:4] for item in inspected.values()}) == 1, inspected
    assert len({item[0][5:] for item in inspected.values()}) == 1, inspected
    assert len({item[3] for item in inspected.values()}) == 1, inspected
    (work / 'database-summary.txt').write_text('\n'.join(item[2] for item in inspected.values()) + '\n')
    source.rename(work / 'original-source')
    consumers = [objects['consumer'], objects['main']]
    run(['dotnet', args.linker.resolve(), 'link', '--out', work / 'strict', '--entry', 'main', *consumers], work / 'strict-link.log', expected=1)
    diagnostic = (work / 'strict-link.log').read_text()
    assert 'No definition for virtual table ' + inspected['consumer'][0][1] + ' at address point 16' in diagnostic, diagnostic
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Review.VirtualStorage',
         '--class', 'Core', '--entry', 'main', '--unresolved-providers', 'required-partial', *consumers], work / 'link.log')
    with (generated / 'Imports.tsv').open() as manifest:
        rows = list(csv.DictReader(manifest, delimiter='\t'))
    tables = [row for row in rows if row['kind'] == 'virtual-table']
    assert len(tables) == 1 and tables[0]['link_name'] == inspected['consumer'][0][1], rows
    assert {row['source_name'] for row in rows if row['kind'] == 'function'} == {'provided_table_address'}, rows
    assert all(row['link_name'] in inspected['provider'][1].values() for row in rows if row['kind'] == 'function'), rows
    code = (generated / 'Translated.cs').read_text()
    assert 'Return the already selected address point' in code and 'delegate* managed signatures and recorded slots' in code
    assert 'GC.AllocateArray<nint>' not in code and 'NativeLibrary' not in code and 'DllImport' not in code
    with (work / 'missing-provider-build.log').open('w') as log:
        result = subprocess.run(['dotnet', 'build', str(generated / 'Translated.csproj'), '-c', 'Release'], stdout=log, stderr=subprocess.STDOUT)
    assert result.returncode != 0 and 'CS8795' in (work / 'missing-provider-build.log').read_text()
    # This genuine separately translated provider owns the recorded table and
    # its managed-call entries. The adapter does not build or guess any slots.
    provider = work / 'translated-provider'
    run(['dotnet', args.linker.resolve(), 'link', '--out', provider, '--namespace', 'Review.VirtualStorage',
         '--class', 'Provider', objects['provider']], work / 'provider-link.log')
    assert 'GC.AllocateArray<nint>' in (provider / 'Translated.cs').read_text()
    for path in provider.glob('*.cs'):
        target = generated / path.name
        if target.exists() and target.read_bytes() == path.read_bytes():
            continue
        shutil.copyfile(path, generated / ('Provider.' + path.name))
    write_adapters(rows, generated / 'Providers.cs')
    run(['dotnet', 'run', '--project', generated / 'Translated.csproj', '-c', 'Release'], work / 'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        apps = [path for path in (work / 'nativeaot').iterdir() if path.is_file() and path.suffix == '' and path.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one NativeAOT application')
        run(apps, work / 'nativeaot-run.log')
    direct = work / 'direct-required'
    run(['dotnet', args.linker.resolve(), 'link', '--out', direct, '--namespace', 'Review.VirtualStorage',
         '--class', 'Core', '--entry', 'main', '--unresolved-providers', 'required-partial', objects['direct']], work / 'direct-link.log')
    with (direct / 'Imports.tsv').open() as manifest:
        direct_rows = list(csv.DictReader(manifest, delimiter='\t'))
    assert {row['source_name'] for row in direct_rows if row['kind'] == 'function'} == {'eval'}, direct_rows
    assert next(row['link_name'] for row in direct_rows if row['kind'] == 'function') == inspected['provider'][1]['eval']
    for path in provider.glob('*.cs'):
        target = direct / path.name
        if target.exists() and target.read_bytes() == path.read_bytes():
            continue
        shutil.copyfile(path, direct / ('Provider.' + path.name))
    write_adapters(direct_rows, direct / 'Providers.cs', direct=True)
    run(['dotnet', 'run', '--project', direct / 'Translated.csproj', '-c', 'Release'], work / 'direct-managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', direct / 'Translated.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work / 'direct-nativeaot'], work / 'direct-nativeaot-build.log')
        apps = [path for path in (work / 'direct-nativeaot').iterdir() if path.is_file() and path.suffix == '' and path.stat().st_mode & 0o111]
        if len(apps) != 1:
            raise RuntimeError('Expected one direct-call NativeAOT application')
        run(apps, work / 'direct-nativeaot-run.log')
    # In a normal link, the exact local definition wins over the external
    # declarations and requires no partial storage/provider contract.
    local = work / 'local-definition'
    run(['dotnet', args.linker.resolve(), 'link', '--out', local, '--namespace', 'Review.VirtualStorage',
         '--class', 'Core', '--entry', 'main', '--unresolved-providers', 'required-partial',
         objects['provider'], *consumers], work / 'local-link.log')
    with (local / 'Imports.tsv').open() as manifest:
        assert not list(csv.DictReader(manifest, delimiter='\t'))
    run(['dotnet', 'run', '--project', local / 'Translated.csproj', '-c', 'Release'], work / 'local-managed-run.log')
    print('PASS: strict default; exact external vtable/address-point required partial, no synthesized slots, missing-provider compile failure; genuine separately translated provider dispatch/stable addresses and local-definition precedence; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
