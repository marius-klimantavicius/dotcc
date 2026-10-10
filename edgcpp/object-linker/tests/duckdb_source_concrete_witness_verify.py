#!/usr/bin/env python3
"""Validate exact selected-member witness closure for concrete class substitutions."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_concrete_witness.cpp'),source)
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
            check('unused-template-unemitted',scalar("SELECT count(*) FROM source_record_types WHERE source_name='Unused'")==0)
            check('discovering-expression-unemitted',scalar("SELECT count(*) FROM source_expressions")==0)
            check('owned-concrete-substitution',scalar("SELECT count(*) FROM source_record_instantiations")==1)
            concrete,prototype=db.execute("SELECT r.source_type_id,p.source_type_id FROM source_record_instantiations i JOIN source_record_types r ON r.id=i.source_record_id JOIN source_record_types p ON p.id=i.prototype_source_record_id").fetchone()
            selected,original=db.execute("SELECT id,template_prototype_declaration_id FROM source_expression_declarations WHERE owner_source_type_id=? AND template_origin_kind='class-member-function'",(concrete,)).fetchone()
            check('exact-member-prototype',scalar(f"SELECT count(*) FROM source_expression_declarations WHERE id={original} AND owner_source_type_id={prototype} AND source_name='get'")==1)
            check('semantic-member-no-executable-body',scalar("SELECT count(*) FROM entities WHERE source_name='get'")==0)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        diagnostic='Concrete class substitution lacks its exact selected original member ownership'
        negatives=[
            ('missing-selected-witness',f"DELETE FROM source_expression_declarations WHERE id={selected}",diagnostic),
            ('wrong-original-owner',f"UPDATE source_expression_declarations SET owner_source_type_id={concrete} WHERE id={original}",diagnostic),
            ('invented-ordinary-function',f"UPDATE source_expression_declarations SET template_origin_kind='none',has_template_origin=0,template_prototype_declaration_id=NULL WHERE id={selected}",diagnostic),
        ]
        for name,sql,diagnostic in negatives:
            damaged=out/(name+'.dotobj');shutil.copy2(obj,damaged)
            with sqlite3.connect(damaged) as db:
                db.execute("PRAGMA ignore_check_constraints=ON")
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
