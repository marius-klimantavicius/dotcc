#!/usr/bin/env python3
"""Required partial providers, with genuine native storage and RTTI identity."""
import argparse
import csv
import os
from pathlib import Path
import re
import shutil
import sqlite3
import subprocess
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone()==('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        refs=db.execute('''SELECT s.kind,s.link_name,s.is_definition,t.exception_policy
            FROM symbols s JOIN types t ON t.id=s.type_id
            WHERE s.name IN ('imported_value','imported_add','get_imported_value','provider_void_info')
               OR s.kind='rtti-object' ORDER BY s.id''').fetchall()
        assert refs and all(not defined for _,_,defined,_ in refs), refs
        assert any(kind=='rtti-object' and link=='_ZTIv' for kind,link,_,_ in refs), refs
        assert db.execute("SELECT COUNT(*) FROM nodes WHERE kind='routine-address'").fetchone()[0]>0
        return path.name+': '+str(refs)


def write_provider(rows, path):
    declarations={row['link_name']:row['declaration'] for row in rows}
    expected={'imported_value','imported_add','get_imported_value','provider_void_info','_ZTIv'}
    assert set(declarations)==expected, declarations
    assert declarations['get_imported_value']=='public static partial int get_imported_value();', declarations
    methods={}
    for link, declaration in declarations.items():
        match=re.fullmatch(r'public static partial (.+) (\S+)\((.*)\);', declaration)
        assert match, declaration
        methods[link]=match.groups()
    lines=['namespace Review.Providers;', 'public static unsafe partial class Core', '{',
           '    private static readonly nint fixtureLibrary = global::System.Runtime.InteropServices.NativeLibrary.Load(',
           '        global::System.Environment.GetEnvironmentVariable("DOTOBJ_FIXTURE_PROVIDER")',
           '        ?? throw new global::System.InvalidOperationException("Fixture provider path is required"));',
           '    private static void* FixtureExport(string name) =>',
           '        (void*)global::System.Runtime.InteropServices.NativeLibrary.GetExport(fixtureLibrary, name);']
    for link,(result,name,parameters) in methods.items():
        if link=='imported_value':
            assert result=='int*' and not parameters
            expression='(int*)FixtureExport("imported_value")'
        elif link=='imported_add':
            assert result=='int' and parameters=='int argument0'
            expression='((delegate* unmanaged<int, int>)FixtureExport("imported_add"))(argument0)'
        elif link=='get_imported_value':
            assert result=='int' and not parameters
            expression='((delegate* unmanaged<int>)FixtureExport("get_imported_value"))()'
        else:
            assert result.endswith('*') and not parameters
            # The handwritten provider exposes the genuine native type_info
            # object. It does not fabricate managed RTTI storage or tokens.
            expression=f'({result})((delegate* unmanaged<void*>)FixtureExport("provider_void_info"))()'
        lines.append(f'    public static partial {result} {name}({parameters}) => {expression};')
    lines.append('}')
    path.write_text('\n'.join(lines)+'\n')


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,required=True)
    parser.add_argument('--compiler',type=Path,default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker',type=Path,required=True)
    parser.add_argument('--nativeaot',action='store_true')
    args=parser.parse_args()
    work=args.out.resolve();work.mkdir(parents=True,exist_ok=False)
    source=work/'source';shutil.copytree(RELATIONAL/'tests/required_providers',source)
    run(['g++','-std=c++20',source/'provider.cpp',source/'consumer.cpp',source/'main.cpp','-o',work/'native'],work/'native-build.log')
    run([work/'native'],work/'native-run.log')
    library=work/'fixture-provider.so'
    run(['g++','-std=c++20','-shared','-fPIC',source/'provider.cpp','-o',library],work/'provider-build.log')
    objects=[]
    for name in ('consumer','main'):
        obj=work/(name+'.dotobj')
        run([sys.executable,RELATIONAL/'compile.py',source/(name+'.cpp'),'--output',obj,
             '--work-dir',work/(name+'-work'),'--compiler',args.compiler.resolve(),
             '-I',EDG/'backend/runtime/include'],work/(name+'-compile.log'))
        objects.append(obj)
    (work/'database-summary.txt').write_text('\n'.join(inspect(obj) for obj in objects)+'\n')
    source.rename(work/'original-source')
    run(['dotnet',args.linker.resolve(),'link','--out',work/'strict','--entry','main',*objects],work/'strict-link.log',expected=1)
    strict=(work/'strict-link.log').read_text()
    assert 'Unresolved RTTI provider for _ZTIv' in strict, strict
    generated=work/'generated'
    run(['dotnet',args.linker.resolve(),'link','--out',generated,'--entry','main',
         '--namespace','Review.Providers','--class','Core','--unresolved-providers','required-partial',*objects],work/'link.log')
    with (generated/'Imports.tsv').open() as manifest:
        rows=list(csv.DictReader(manifest,delimiter='\t'))
    assert len(rows)==len({row['link_name'] for row in rows}), rows
    assert all(row['unit'] and int(row['symbol_id'])>0 and int(row['type_id'])>0 for row in rows)
    assert all(row['exception_policy']=='terminate' for row in rows if row['kind']=='function'), rows
    code=(generated/'Translated.cs').read_text()
    assert 'unused_declaration' not in code and 'unused_declaration' not in (generated/'Imports.tsv').read_text()
    assert 'DllImport' not in code and 'NativeLibrary' not in code
    with (work/'missing-provider-build.log').open('w') as log:
        result=subprocess.run(['dotnet','build',str(generated/'Translated.csproj'),'-c','Release'],stdout=log,stderr=subprocess.STDOUT)
    diagnostic=(work/'missing-provider-build.log').read_text()
    assert result.returncode!=0 and 'CS8795' in diagnostic, diagnostic
    write_provider(rows,generated/'Providers.cs')
    os.environ['DOTOBJ_FIXTURE_PROVIDER']=str(library)
    run(['dotnet','run','--project',generated/'Translated.csproj','-c','Release'],work/'managed-run.log')
    if args.nativeaot:
        run(['dotnet','publish',generated/'Translated.csproj','-c','Release','-p:PublishAot=true','-o',work/'nativeaot'],work/'nativeaot-build.log')
        apps=[p for p in (work/'nativeaot').iterdir() if p.is_file() and p.suffix=='' and p.stat().st_mode & 0o111]
        if len(apps)!=1:raise RuntimeError('Expected one NativeAOT application')
        run(apps,work/'nativeaot-run.log')
    print('PASS: strict default; source-hidden exact required partial imports, missing implementation compilation failure; '
          'genuine provider native/C#'+('/NativeAOT' if args.nativeaot else '')+' function/global/RTTI addresses and cross-TU identity')
    print(work)


if __name__=='__main__':main()
