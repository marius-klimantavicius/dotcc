#!/usr/bin/env python3
"""Validate source over-alignment and addressable storage without claiming CLR 64-byte value alignment."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for key in ('compiler','linker','packager','output-dir','stock-compiler','stock-runtime'):
        parser.add_argument('--'+key,type=Path,required=True)
    parser.add_argument('--native-aot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    source=out/'overaligned.cpp';shutil.copy2(Path(__file__).with_name('duckdb_overaligned_records.cpp'),source)
    tooling=out/'tooling';tooling.mkdir()
    for name in ('compile.py','schema.sql'):shutil.copy2(args.packager.resolve().parent/name,tooling/name)
    spec=importlib.util.spec_from_file_location('packager',tooling/'compile.py');packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
    checks=[];commands=[];error=None
    def check(name,value):
        if not value:raise AssertionError(name)
        checks.append(name)
    def run(name,command,success=True,diagnostic=None):
        command=list(map(str,command));result=subprocess.run(command,cwd=out,capture_output=True,timeout=600)
        (out/(name+'.stdout')).write_bytes(result.stdout);(out/(name+'.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name,command=command,exitCode=result.returncode))
        check(name,(result.returncode==0)==success)
        if diagnostic:check(name+'-diagnostic',diagnostic in (result.stdout+result.stderr).decode(errors='replace'))
    try:
        for opt in ('0','2'):
            run('native-build-'+opt,['c++','-std=c++17','-O'+opt,source,'-o',out/('native-'+opt)])
            run('native-'+opt,[out/('native-'+opt)])
        run('stock-lower',[args.stock_compiler.resolve(),'--c++17','--gnu_version','130300','--instantiate','used','--no_inlining','--exceptions','--clear_flag','use_predefined_macro_file','--gen_c_file_name',out/'stock.c','-o',out/'stock.il',source])
        for opt in ('0','2'):
            run('stock-build-'+opt,['gcc','-w','-O'+opt,out/'stock.c',args.stock_runtime.resolve(),'-lstdc++','-lpthread','-lm','-o',out/('stock-'+opt)])
            run('stock-'+opt,[out/('stock-'+opt)])
        obj=out/'unit.dotobj'
        run('export',['python3','-B',tooling/'compile.py',source,'--output',obj,'--work-dir',out/'work','--compiler',args.compiler.resolve(),'--std','c++17','--exceptions'])
        source.rename(out/'retained-source.cpp')
        run('verify-source-hidden',['dotnet',args.linker.resolve(),'verify',obj])
        with sqlite3.connect(obj) as db:
            def scalar(sql):return db.execute(sql).fetchone()[0]
            check('physical-overalignment',scalar("SELECT count(DISTINCT alignment) FROM record_types WHERE alignment IN(32,64)")==2)
            check('source-overalignment',scalar("SELECT count(DISTINCT explicit_alignment) FROM source_record_types WHERE explicit_alignment IN(32,64)")==2)
            check('aligned-global-and-tls',scalar("SELECT count(DISTINCT is_thread_local) FROM storages WHERE alignment=64 AND definition='strong'")==2)
            check('packed-record-preserved',scalar("SELECT count(*) FROM record_types WHERE source_name='Packed' AND alignment=1 AND size=65")==1)
            check('nested-record-stride',scalar("SELECT count(*) FROM record_types WHERE source_name='Nested' AND alignment=64 AND size=192")==1)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        generated='\n'.join(path.read_text() for path in (out/'generated').glob('*.cs'))
        check('explicit-full-alignment-storage','& ~(nuint)63' in generated)
        check('no-false-clr-alignment','Pack = 64' not in generated and 'Pack = 32' not in generated)
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('weaken-global-storage',"UPDATE storages SET alignment=32 WHERE alignment=64 AND definition='strong'",'Invalid storage identity, type or alignment'),
            ('change-physical-alignment',"UPDATE record_types SET alignment=32 WHERE source_name='CacheLine'",'Original record does not match its complete execution representation'),
            ('change-original-alignment',"UPDATE source_record_types SET native_alignment=32 WHERE source_name='CacheLine'",'Original record does not match its complete execution representation'),
            ('change-nested-field-offset',"UPDATE record_fields SET byte_offset=32 WHERE record_type_id=(SELECT type_id FROM record_types WHERE source_name='Nested') AND source_name='lines'",'Ordinary field does not satisfy its observed layout alignment'),
        ]
        for name,sql,diagnostic in negatives:
            damaged=out/(name+'.dotobj');shutil.copy2(obj,damaged)
            with sqlite3.connect(damaged) as db:
                check(name+'-changed',db.execute(sql).rowcount>0)
                check(name+'-foreign-keys',not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
            run(name,['dotnet',args.linker.resolve(),'verify',damaged],False,diagnostic)
    except Exception as ex:error=str(ex)
    result=dict(status='failed' if error else 'passed',error=error,checks=checks,commands=commands)
    (out/'results.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(dict(status=result['status'],checks=len(checks),error=error)))
    return int(error is not None)

if __name__=='__main__':raise SystemExit(main())
