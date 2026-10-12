#!/usr/bin/env python3
"""Run selected startup after static storage; preserve record-array bytes and bounds."""
import argparse
from pathlib import Path
import shutil
import sys

from verify import EDG, RELATIONAL, run
from verify_startup_groups import inspect


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work / 'source.cpp'
    shutil.copy2(RELATIONAL / 'tests/startup_groups.cpp', source)
    oracle = work / 'oracle.cpp'
    oracle.write_text('''#include <cstddef>
#include <cstdio>
#define main fixture_main
#include "source.cpp"
#undef main
struct Guarded { unsigned char before; Item items[2]; unsigned char after; };
int main() {
    if (fixture_main() != 0) return 1;
    Guarded value{37, {Item(7), Item(8)}, 41};
    Guarded copy = value;
    if (copy.before != 37 || copy.after != 41 || copy.items[0].value != 7 || copy.items[1].value != 8) return 2;
    std::printf("%zu\\t%zu\\t%zu\\t%zu\\t%zu\\t%d\\n", sizeof(Item), sizeof(value.items), sizeof(Guarded), offsetof(Guarded, items), offsetof(Guarded, after), trace);
    return 0;
}
''')
    run(['g++', '-std=c++17', oracle, '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-results.tsv')
    obj = work / 'startup.dotobj'
    run([sys.executable, RELATIONAL / 'compile.py', source, '--output', obj,
         '--compiler', args.compiler.resolve(), '--work-dir', work / 'compile-work'], work / 'compile.log')
    (work / 'database-summary.txt').write_text(inspect(obj) + '\n')
    source.rename(work / 'original-source.cpp')
    oracle.rename(work / 'original-oracle.cpp')
    generated = work / 'generated'
    run(['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Review.Startup',
         '--class', 'Core', obj], work / 'link.log')
    code = (generated / 'Translated.cs').read_text()
    assert 'private fixed byte __record_bytes[8];' in code and 'private uint __record_alignment;' in code
    assert '__ctor_Item(&((Item*)first)[0], 1);' in code
    assert '__ctor_Item(&((Item*)second)[1], 5);' in code
    assert code.index('private static readonly byte[] __second_storage') < code.index('static Core()')
    assert 'Exception escaped nonlocal initialization' in code
    (generated / 'Smoke.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><OutputType>Exe</OutputType><AssemblyName>StartupSmoke</AssemblyName></PropertyGroup></Project>
''')
    (generated / 'Smoke.cs').write_text('''using Review.Startup;
using System;
using System.Runtime.InteropServices;
internal static unsafe class Smoke {
    [StructLayout(LayoutKind.Sequential)] private struct Guarded { public byte before; public Core.__array_Item_2 items; public byte after; }
    public static int Main() {
        if (Core.main() != 0) return 1;
        Guarded value = default; value.before = 37; value.after = 41;
        Core.Item* items = (Core.Item*)&value.items;
        Core.__ctor_Item(items, 7); Core.__ctor_Item(items + 1, 8);
        Guarded copy = value;
        if (copy.before != 37 || copy.after != 41 || ((Core.Item*)&copy.items)[0].value != 7 || ((Core.Item*)&copy.items)[1].value != 8) return 2;
        Console.WriteLine(sizeof(Core.Item) + "\\t" + sizeof(Core.__array_Item_2) + "\\t" + sizeof(Guarded) + "\\t" + ((byte*)&value.items - (byte*)&value) + "\\t" + ((byte*)&value.after - (byte*)&value) + "\\t" + *Core.trace);
        nuint first = (nuint)Core.first, second = (nuint)Core.second, middle = (nuint)Core.middle;
        for (int pass = 0; pass < 3; ++pass) {
            byte[] pressure = new byte[2 * 1024 * 1024];
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true); GC.KeepAlive(pressure);
            if (first != (nuint)Core.first || second != (nuint)Core.second || middle != (nuint)Core.middle) return 3;
        }
        return 0;
    }
}
''')
    run(['dotnet', 'build', generated / 'Smoke.csproj', '-c', 'Release'], work / 'managed-build.log')
    run(['dotnet', generated / 'bin/Release/net10.0/StartupSmoke.dll'], work / 'managed-results.tsv')
    expected = (work / 'native-results.tsv').read_text()
    assert (work / 'managed-results.tsv').read_text() == expected
    if args.nativeaot:
        run(['dotnet', 'publish', generated / 'Smoke.csproj', '-c', 'Release', '-p:PublishAot=true',
             '-o', work / 'nativeaot'], work / 'nativeaot-build.log')
        run([work / 'nativeaot/StartupSmoke'], work / 'nativeaot-results.tsv')
        assert (work / 'nativeaot-results.tsv').read_text() == expected
    print('PASS: selected startup order, pinned storage before actions, distinct element constructor writes, record-array copy/extent/alignment/canaries, GC address identity; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
