#!/usr/bin/env python3
"""Keep incomplete-result callable declarations as address storage, requiring recorded ABI facts for binding."""
import argparse
import csv
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        rows = db.execute("SELECT id,name,is_incomplete,size,link_name FROM types WHERE kind='record' AND name='Result'").fetchall()
        return rows


def smoke(directory):
    (directory / 'Smoke.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><OutputType>Exe</OutputType><AssemblyName>OpaqueCallableSmoke</AssemblyName></PropertyGroup></Project>\n')
    (directory / 'Smoke.cs').write_text('''using Review.OpaqueCallable;
internal static unsafe class Smoke
{
    public static int Main()
    {
        Core.Holder first = default;
        Core.Holder second = default;
        if (!Core.is_empty(&first)) return 1;
        void* address = Core.bound_factory();
        if (address == null) return 2;
        Core.set_callback(&first, address);
        if (Core.get_callback(&first) != address) return 3;
        Core.copy_holder(&second, &first);
        Core.clear_holder(&first);
        if (!Core.is_empty(&first) || Core.is_empty(&second)) return 4;
        for (int attempt = 0; attempt < 3; ++attempt)
        {
            global::System.GC.Collect(2, global::System.GCCollectionMode.Forced, true, true);
            if (Core.get_callback(&second) != address || Core.bound_factory() != address) return 5;
        }
        return 0;
    }
}
''')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    work = args.out.resolve(); work.mkdir(parents=True, exist_ok=False)
    source = work / 'source'; shutil.copytree(RELATIONAL / 'tests/opaque_callable_storage', source)
    run(['g++', '-std=c++20', *[source / (name + '.cpp') for name in ('storage','binding','provider','oracle')], '-o', work / 'native'], work / 'native-build.log')
    run([work / 'native'], work / 'native-run.log')
    objects = {}
    summaries = []
    for name in ('storage','binding','provider'):
        obj = work / (name + '.dotobj')
        run([sys.executable, RELATIONAL / 'compile.py', source / (name + '.cpp'), '--output', obj,
             '--work-dir', work / (name + '-work'), '--compiler', args.compiler.resolve()], work / (name + '-compile.log'))
        objects[name] = obj
        rows = inspect(obj); summaries.append(name + ': ' + repr(rows))
        assert rows and all(bool(row[2]) == (name != 'provider') for row in rows), rows
    (work / 'database-summary.txt').write_text('\n'.join(summaries) + '\n')
    source.rename(work / 'original-source')
    linker = args.linker.resolve()
    storage = work / 'storage-generated'
    run(['dotnet', linker, 'link', '--out', storage, '--namespace', 'Review.OpaqueCallable', '--class', 'Core', objects['storage']], work / 'storage-link.log')
    code = (storage / 'Translated.cs').read_text()
    assert 'public void* callback;' in code and 'no finalized result ABI' in code
    run(['dotnet', 'build', storage / 'Translated.csproj', '-c', 'Release'], work / 'storage-build.log')
    run(['dotnet', linker, 'link', '--out', work / 'unfinalized-generated', '--unresolved-providers', 'required-partial', objects['binding']], work / 'unfinalized-link.log', expected=1)
    negative = (work / 'unfinalized-link.log').read_text()
    assert 'function address binding requires a recorded finalized result ABI' in negative and 'binding.cpp' in negative, negative
    for label, names in (('linked', ('storage','binding','provider')), ('reverse', ('provider','binding','storage'))):
        generated = work / (label + '-generated')
        run(['dotnet', linker, 'link', '--out', generated, '--namespace', 'Review.OpaqueCallable', '--class', 'Core',
             '--unresolved-providers', 'required-partial', *[objects[name] for name in names]], work / (label + '-link.log'))
        with (generated / 'Imports.tsv').open() as stream:
            assert not list(csv.DictReader(stream, delimiter='\t'))
        code = (generated / 'Translated.cs').read_text()
        assert 'public void* callback;' in code and 'make_result(' in code
        smoke(generated)
        run(['dotnet', 'build', generated / 'Smoke.csproj', '-c', 'Release'], work / (label + '-managed-build.log'))
        run(['dotnet', generated / 'bin/Release/net10.0/OpaqueCallableSmoke.dll'], work / (label + '-managed-run.log'))
        if args.nativeaot:
            output = work / (label + '-nativeaot')
            run(['dotnet', 'publish', generated / 'Smoke.csproj', '-c', 'Release', '-p:PublishAot=true', '-o', output], work / (label + '-nativeaot-build.log'))
            run([output / 'OpaqueCallableSmoke'], work / (label + '-nativeaot-run.log'))
    print('PASS: incomplete-result callback metadata/address storage/copy/null; source-located unfinalized address binding rejection; exact cross-TU finalized function provider address identity in both link orders; native/source-hidden C#' + ('/NativeAOT' if args.nativeaot else ''))
    print(work)


if __name__ == '__main__':
    main()
