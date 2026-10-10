#!/usr/bin/env python3
"""Validate owned unevaluated temporary initialization and ordinary class-template member declarations."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_temporary.cpp'),source)
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
            check('dependent-selected-constructor-recipe',scalar("SELECT count(*) FROM source_expression_temporaries WHERE initialization_kind=5 AND is_dependent_constructor=1 AND constructor_declaration_id IS NULL")>0)
            check('original-selected-constructor-recipe',scalar("SELECT count(*) FROM source_expression_temporaries t JOIN source_expression_declarations d ON d.id=t.constructor_declaration_id WHERE d.routine_kind='constructor' AND t.is_dependent_constructor=0")>0)
            check('original-unevaluated-destructor',scalar("SELECT count(*) FROM source_expression_temporaries t JOIN source_expression_declarations d ON d.id=t.destructor_declaration_id WHERE d.routine_kind='destructor'")>0)
            check('temporary-ordered-arguments',scalar("SELECT count(*) FROM source_expression_temporaries t JOIN source_expression_edges e ON e.expression_id=t.expression_id WHERE e.role='argument' AND e.ordinal=0")>0)
            check('no-fabricated-execution-lifetime',scalar("SELECT count(*) FROM source_expression_temporaries WHERE lifetime_disposition!='no-linked-lifetime'")==0)
            check('class-member-original-template-owner',scalar("SELECT count(*) FROM source_expression_declarations WHERE template_origin_kind='class-member-function' AND owner_source_type_id IS NOT NULL AND formal_count=0 AND argument_count=0")>0)
            check('selected-routine-kind-retained',scalar("SELECT count(DISTINCT routine_kind) FROM source_expression_declarations WHERE routine_kind IN('constructor','destructor','function')")==3)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('missing-temporary',"DELETE FROM source_expression_temporary_flags WHERE expression_id=(SELECT min(expression_id) FROM source_expression_temporaries); DELETE FROM source_expression_temporaries WHERE expression_id=(SELECT min(expression_id) FROM source_expression_temporaries)",'Original temporary expression is missing its initialization provenance'),
            ('false-dependent-constructor',"UPDATE source_expression_temporaries SET is_dependent_constructor=1 WHERE constructor_declaration_id IS NOT NULL",'Dependent class identity differs from its exact original template arguments'),
            ('false-constructor-recipe',"UPDATE source_expression_temporaries SET initialization_kind=0 WHERE constructor_declaration_id IS NOT NULL",'Dependent class identity differs from its exact original template arguments'),
            ('false-constructor-kind',"UPDATE source_expression_declarations SET routine_kind='function' WHERE id IN(SELECT constructor_declaration_id FROM source_expression_temporaries WHERE constructor_declaration_id IS NOT NULL)",'Dependent class identity differs from its exact original template arguments'),
            ('false-destructor-kind',"UPDATE source_expression_declarations SET routine_kind='function' WHERE id IN(SELECT destructor_declaration_id FROM source_expression_temporaries WHERE destructor_declaration_id IS NOT NULL)",'Dependent class identity differs from its exact original template arguments'),
            ('missing-member-owner',"UPDATE source_expression_declarations SET owner_source_type_id=NULL WHERE template_origin_kind='class-member-function'",'Concrete class substitution lacks its exact selected original member ownership'),
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
