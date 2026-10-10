#!/usr/bin/env python3
"""Validate exact primary/selected class-template headers and their distinct original argument mappings."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for key in ('compiler','linker','packager','output-dir'):
        parser.add_argument('--'+key,type=Path,required=True)
    parser.add_argument('--native-aot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_class_specializations.cpp'),source)
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
            run('native-build-'+opt,['c++','-std=c++20','-O'+opt,source,'-o',out/('native-'+opt)])
            run('native-'+opt,[out/('native-'+opt)])
        obj=out/'unit.dotobj'
        run('export',['python3','-B',tooling/'compile.py',source,'--output',obj,'--work-dir',out/'work','--compiler',args.compiler.resolve(),'--std','c++20','--exceptions'])
        source.rename(out/'retained-source.cpp')
        run('verify-source-hidden',['dotnet',args.linker.resolve(),'verify',obj])
        with sqlite3.connect(obj) as db:
            def scalar(sql):return db.execute(sql).fetchone()[0]
            check('two-selected-specializations',scalar("SELECT count(*) FROM source_class_specializations")>=2)
            check('primary-and-selected-headers',scalar("SELECT count(*) FROM source_class_template_headers")>=3)
            check('distinct-formal-list-lengths',scalar("SELECT count(*) FROM source_class_template_headers s JOIN source_class_template_headers p ON p.source_record_id=s.primary_source_record_id WHERE s.formal_count!=p.formal_count")>0)
            check('primary-two-arguments',scalar("SELECT count(*) FROM source_template_arguments a JOIN source_class_specializations s ON s.source_record_id=a.source_record_id WHERE a.ordinal=1")>=2)
            check('selected-one-argument',scalar("SELECT count(*) FROM source_class_specializations WHERE selected_argument_count=1")>0)
            check('original-nontype-pattern',scalar("SELECT count(*) FROM source_class_template_patterns WHERE kind='constant'")>0)
            check('retained-selected-nontype-argument',scalar("SELECT count(*) FROM source_class_specialization_arguments WHERE kind='constant'")>0)
            check('exact-partial-marker',scalar("SELECT count(*) FROM source_record_instantiations WHERE has_selected_partial_specialization=1")==scalar("SELECT count(*) FROM source_class_specializations"))
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('missing-selected-map',"DELETE FROM source_constraint_argument_details WHERE owner_kind='class-selected'; DELETE FROM source_class_specialization_arguments; DELETE FROM source_class_specializations",'Partial class instance is missing its two original argument mappings'),
            ('false-primary-only-classification',"UPDATE source_record_instantiations SET has_selected_partial_specialization=0 WHERE has_selected_partial_specialization=1",'Partial class instance lost its selected original specialization'),
            ('missing-selected-argument',"DELETE FROM source_constraint_argument_details WHERE owner_kind='class-selected'; DELETE FROM source_class_specialization_arguments",'Selected class argument mapping is incomplete'),
            ('false-selected-argument-count',"UPDATE source_class_specializations SET selected_argument_count=selected_argument_count+1",'Selected class argument mapping is incomplete'),
            ('false-pattern-count',"UPDATE source_class_template_headers SET pattern_count=pattern_count+1 WHERE primary_source_record_id IS NOT NULL",'Original class template header/pattern lists are incomplete'),
            ('wrong-selected-prototype',"UPDATE source_class_specializations SET selected_source_record_id=(SELECT min(source_record_id) FROM source_class_template_headers WHERE primary_source_record_id IS NULL)",'Partial class instance lost its selected original specialization'),
        ]
        for name,sql,diagnostic in negatives:
            damaged=out/(name+'.dotobj');shutil.copy2(obj,damaged)
            with sqlite3.connect(damaged) as db:
                before=db.total_changes
                for statement in sql.split(';'):
                    if statement.strip():db.execute(statement)
                check(name+'-changed',db.total_changes>before)
                check(name+'-foreign-keys',not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
            run(name,['dotnet',args.linker.resolve(),'verify',damaged],False,diagnostic)
    except Exception as ex:error=str(ex)
    result=dict(status='failed' if error else 'passed',error=error,checks=checks,commands=commands)
    (out/'results.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(dict(status=result['status'],checks=len(checks),error=error)))
    return int(error is not None)

if __name__=='__main__':raise SystemExit(main())
