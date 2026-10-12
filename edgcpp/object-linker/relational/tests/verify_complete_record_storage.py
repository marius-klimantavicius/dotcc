#!/usr/bin/env python3
"""Recorded complete virtual-base object bytes and alignment; no virtual construction."""
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
        rows = db.execute("SELECT id,name,size,alignment,is_incomplete,is_dependent FROM types WHERE kind='record' AND name IN ('Layout','Fieldless','Packed') ORDER BY id").fetchall()
        assert len(rows) == 3, rows
        assert {name: alignment for _, name, _, alignment, _, _ in rows} == {'Layout': 8, 'Fieldless': 8, 'Packed': 1}, rows
        for id, _, size, alignment, incomplete, dependent in rows:
            assert size > 0 and not incomplete and not dependent
            assert db.execute('SELECT count(*) FROM base_subobjects WHERE owner_type_id=? AND is_virtual=1', (id,)).fetchone()[0] > 0
        assert db.execute("SELECT count(*) FROM fields f JOIN types t ON t.id=f.owner_type_id WHERE t.name='Fieldless'").fetchone()[0] == 0
        assert not db.execute('SELECT count(*) FROM class_conversions').fetchone()[0]
        assert not db.execute('SELECT count(*) FROM vptr_writes').fetchone()[0]
        return repr(rows)


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
    shutil.copytree(RELATIONAL / 'tests/complete_record_storage', source)
    run(['g++', '-std=c++20', source / 'program.cpp', source / 'oracle.cpp', '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-results.tsv')
    obj = work / 'program.dotobj'
    run([sys.executable, RELATIONAL / 'compile.py', source / 'program.cpp', '--output', obj,
         '--work-dir', work / 'compile-work', '--compiler', args.compiler.resolve()], work / 'compile.log')
    (work / 'database-summary.txt').write_text(inspect(obj) + '\n')
    source.rename(work / 'original-source')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Review.RecordStorage', '--class', 'Core', obj], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    assert 'private fixed byte __record_bytes__2[' in code and 'private ulong __record_alignment__2;' in code
    assert 'public ulong __record_bytes;' in code and 'public ulong __record_alignment;' in code
    assert 'Pack = 8' in code and 'Pack = 1' in code
    (generated / 'Smoke.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><OutputType>Exe</OutputType><AssemblyName>RecordStorageSmoke</AssemblyName></PropertyGroup></Project>\n''')
    (generated / 'Smoke.cs').write_text('''using Review.RecordStorage;
using System;
using System.Runtime.InteropServices;
internal static unsafe class Smoke
{
    [StructLayout(LayoutKind.Sequential)] private struct LayoutEmbedded { public byte before; public Core.Layout value; public byte after; }
    [StructLayout(LayoutKind.Sequential)] private struct FieldlessEmbedded { public byte before; public Core.Fieldless value; public byte after; }
    [StructLayout(LayoutKind.Sequential)] private struct PackedEmbedded { public byte before; public Core.Packed value; public byte after; }
    private static void Copy<T>() where T : unmanaged
    {
        T original = default;
        byte* bytes = (byte*)&original;
        for (int i = 0; i < sizeof(T); ++i) bytes[i] = (byte)(i * 17 + 3);
        T copy = original;
        byte* copied = (byte*)&copy;
        for (int i = 0; i < sizeof(T); ++i) if (copied[i] != (byte)(i * 17 + 3)) throw new Exception("Object bytes lost in value copy: " + typeof(T));
    }
    public static int Main()
    {
        LayoutEmbedded layout = default;
        FieldlessEmbedded fieldless = default;
        PackedEmbedded packed = default;
        if ((ulong)sizeof(Core.Layout) != Core.layout_size() || (ulong)sizeof(Core.Fieldless) != Core.fieldless_size() || (ulong)sizeof(Core.Packed) != Core.packed_size()) return 1;
        if ((ulong)sizeof(LayoutEmbedded) != Core.layout_embedded_size() || (ulong)sizeof(FieldlessEmbedded) != Core.fieldless_embedded_size() || (ulong)sizeof(PackedEmbedded) != Core.packed_embedded_size()) return 2;
        Console.WriteLine("Layout\\t" + sizeof(Core.Layout) + "\\t" + Core.layout_alignment() + "\\t" + ((byte*)&layout.value - (byte*)&layout) + "\\t" + sizeof(LayoutEmbedded));
        Console.WriteLine("Fieldless\\t" + sizeof(Core.Fieldless) + "\\t" + Core.fieldless_alignment() + "\\t" + ((byte*)&fieldless.value - (byte*)&fieldless) + "\\t" + sizeof(FieldlessEmbedded));
        Console.WriteLine("Packed\\t" + sizeof(Core.Packed) + "\\t" + Core.packed_alignment() + "\\t" + ((byte*)&packed.value - (byte*)&packed) + "\\t" + sizeof(PackedEmbedded));
        Copy<Core.Layout>(); Copy<Core.Fieldless>(); Copy<Core.Packed>();
        Core.Layout image = default;
        byte* bytes = (byte*)&image;
        for (int i = 0; i < sizeof(Core.Layout); ++i) bytes[i] = (byte)(i * 17 + 3);
        Core.layout_touch(&image);
        if (image.__record_bytes != 43 || image.__record_alignment != 47) return 3;
        if (bytes[0] != 3 || bytes[sizeof(Core.Layout) - 1] != (byte)((sizeof(Core.Layout) - 1) * 17 + 3)) return 4;
        layout.before = 29; layout.value = image; layout.after = 31;
        for (int i = 0; i < sizeof(Core.Layout); ++i) if (((byte*)&layout.value)[i] != bytes[i]) return 5;
        if (layout.before != 29 || layout.after != 31) return 6;
        packed.before = 37; Core.Packed packedImage = default;
        byte* packedBytes = (byte*)&packedImage;
        for (int i = 0; i < sizeof(Core.Packed); ++i) packedBytes[i] = (byte)(i * 17 + 3);
        Core.packed_touch(&packedImage);
        packed.value = packedImage; packed.after = 41;
        for (int i = 0; i < sizeof(Core.Packed); ++i) if (((byte*)&packed.value)[i] != packedBytes[i]) return 7;
        if (packed.before != 37 || packed.after != 41 || packed.value.marker != 11) return 8;
        Console.WriteLine("byte-image-copy\\tpass");
        return 0;
    }
}
''')
    # Generated declarations are used as physical byte images. This does not
    # replace or claim support for the source C++ virtual-base copy constructor.
    run(['dotnet', 'build', generated / 'Smoke.csproj', '-c', 'Release'], work / 'managed-build.log')
    run(['dotnet', generated / 'bin/Release/net10.0/RecordStorageSmoke.dll'], work / 'managed-results.tsv')
    expected = (work / 'native-results.tsv').read_text()
    assert (work / 'managed-results.tsv').read_text() == expected
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Smoke.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        run([work / 'nativeaot/RecordStorageSmoke'], work / 'nativeaot-results.tsv')
        assert (work / 'nativeaot-results.tsv').read_text() == expected
    print('PASS: complete virtual-base declaration bytes, recorded natural/packed alignment and embedded layout, C# value-copy byte preservation/overlays/helper naming; no virtual construction/conversion/copyctor implementation; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
