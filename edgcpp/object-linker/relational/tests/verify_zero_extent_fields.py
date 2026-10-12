#!/usr/bin/env python3
"""Complete zero-extent fields retain named address views without physical storage."""
import argparse
from pathlib import Path
import re
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = db.execute('''SELECT f.name,f.byte_offset,t.id,t.element_count,t.size,t.alignment,t.is_incomplete,t.is_dependent,
            o.name,o.size,o.alignment,e.kind FROM fields f JOIN types t ON t.id=f.type_id
            JOIN types o ON o.id=f.owner_type_id JOIN types e ON e.id=t.element_type_id
            WHERE t.kind='array' AND t.element_count=0 ORDER BY f.id''').fetchall()
        assert len(rows) == 1, rows
        name, offset, _, count, size, alignment, incomplete, dependent, owner, owner_size, owner_alignment, element = rows[0]
        assert (name, offset, count, size, alignment, incomplete, dependent) == ('padding', 1, 0, 0, 1, 0, 0), rows
        assert (owner, owner_size, owner_alignment, element) == ('ShortShape', 24, 1, 'i8'), rows
        data = db.execute("SELECT f.byte_offset,t.element_count,t.size FROM fields f JOIN types t ON t.id=f.type_id WHERE f.name='data'").fetchall()
        assert data == [(1, 23, 23)], data
        return path.name + ': ' + repr(rows)


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
    shutil.copytree(RELATIONAL / 'tests/zero_extent_fields', source)
    run(['g++', '-std=c++20', source / 'provider.cpp', source / 'main.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = []
    for name in ('provider', 'main', 'unsupported_storage'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        if name != 'unsupported_storage':
            objects.append(obj)
    (work / 'database-summary.txt').write_text('\n'.join(inspect(obj) for obj in objects) + '\n')
    source.rename(work / 'original-source')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Review.ZeroExtent',
         '--class', 'Core', *objects], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    assert 'public sbyte* padding' in code and 'char padding[0], byte offset 1; no storage.' in code
    assert 'return (sbyte*)((byte*)__receiver + 1);' in code
    assert '__array_sbyte_0' not in code and 'Size = 0)' not in code
    assert re.search(r'Size = 24\)\]\s*public struct ShortShape', code)
    assert re.search(r'FieldOffset\(1\)\]\s*public __array_sbyte_23 data;', code)
    assert '&(value)->padding' not in code and '&((value)->padding)' not in code
    (generated / 'Smoke.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><OutputType>Exe</OutputType><AssemblyName>ZeroExtentSmoke</AssemblyName></PropertyGroup></Project>\n''')
    (generated / 'Smoke.cs').write_text('''using Review.ZeroExtent;
internal static unsafe class Smoke
{
    public static int Main()
    {
        int status = Core.main();
        if (status != 0) return status;
        Core.ShortShape* owner = &Core.guarded->value;
        nuint address = (nuint)Core.padding_address(owner);
        for (int pass = 0; pass < 3; ++pass)
        {
            byte[] pressure = new byte[8 * 1024 * 1024];
            global::System.GC.Collect(global::System.GC.MaxGeneration, global::System.GCCollectionMode.Forced, blocking: true, compacting: true);
            global::System.GC.KeepAlive(pressure);
            if (address != (nuint)Core.padding_address(&Core.guarded->value)) return 20;
            if (address != (nuint)Core.data_address(&Core.guarded->value)) return 21;
        }
        return Core.main();
    }
}
''')
    run(['dotnet', 'run', '--project', generated / 'Smoke.csproj', '-c', 'Release'], work / 'managed-run.log')
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Smoke.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        run([work / 'nativeaot/ZeroExtentSmoke'], work / 'nativeaot-run.log')
    run(['dotnet', args.linker.resolve(), 'link', '--out', work / 'unsupported', work / 'unsupported_storage.dotobj'], work / 'unsupported-link.log', expected=1)
    diagnostic = (work / 'unsupported-link.log').read_text()
    assert 'zero-extent array non-field storage emission is not yet implemented' in diagnostic, diagnostic
    assert 'unsupported_storage.cpp:2:' in diagnostic, diagnostic
    print('PASS: complete zero-extent field named address view; exact sizeof24/offset1/data overlap, neighbour canaries, local/global address identity through GC; explicit unsupported non-field storage; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
