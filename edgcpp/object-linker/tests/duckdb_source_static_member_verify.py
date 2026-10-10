#!/usr/bin/env python3
"""Validate static class-template member origins without inventing template headers."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_static_member.cpp'),source)
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
            check('static-member-origins',scalar("SELECT count(*) FROM source_expression_declarations WHERE template_origin_kind='static-data-member'")==2)
            check('exact-primary-declaration',scalar("SELECT count(*) FROM source_expression_declarations WHERE template_origin_kind='static-data-member' AND template_prototype_declaration_id=id AND owner_source_type_id IS NOT NULL")==2)
            check('class-header-not-invented-as-member-header',scalar("SELECT count(*) FROM source_expression_declarations WHERE template_origin_kind='static-data-member' AND formal_count=0 AND argument_count=0")==2)
            check('same-member-name-distinct-original-owners',scalar("SELECT count(DISTINCT owner_source_type_id) FROM source_expression_declarations WHERE source_name='extent'")==2)
            check('original-member-initializer-owned',scalar("SELECT count(*) FROM source_expression_declarations WHERE source_name='extent' AND initializer_kind IN('expression','constant')")==2)
            check('member-value-template-argument-bridge',scalar('SELECT count(*) FROM source_semantic_constant_expressions')>=2)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('missing-primary',"UPDATE source_expression_declarations SET template_prototype_declaration_id=NULL WHERE template_origin_kind='static-data-member'",'Static template member lacks its exact original declaration'),
            ('foreign-primary',"UPDATE source_expression_declarations SET template_prototype_declaration_id=(SELECT max(id) FROM source_expression_declarations) WHERE id=(SELECT min(id) FROM source_expression_declarations)",'Static template member owner does not instantiate its original class'),
            ('missing-class-owner',"UPDATE source_expression_declarations SET owner_source_type_id=NULL WHERE template_origin_kind='static-data-member'",'Dependent class identity differs from its exact original template arguments'),
            ('false-variable-template',"UPDATE source_expression_declarations SET template_origin_kind='variable-template' WHERE template_origin_kind='static-data-member'",'Dependent class identity differs from its exact original template arguments'),
            ('missing-template-origin',"UPDATE source_expression_declarations SET has_template_origin=0 WHERE template_origin_kind='static-data-member'",'Source expression declaration'),
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
