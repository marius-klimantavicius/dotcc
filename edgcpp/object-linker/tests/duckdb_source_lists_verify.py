#!/usr/bin/env python3
"""Validate original brace lists, fold operands/association and selected source fields."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_lists.cpp'),source)
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
            check('empty-original-brace-list',scalar("SELECT count(*) FROM source_expressions WHERE kind='braced-initializer-list' AND edge_count=0")>0)
            check('ordered-original-brace-list',scalar("SELECT count(*) FROM source_expressions WHERE kind='braced-initializer-list' AND edge_count=2")>0)
            check('fold-left-and-right',scalar("SELECT count(DISTINCT is_left_associative) FROM source_expression_folds")==2)
            check('unary-and-binary-fold-operands',scalar("SELECT count(DISTINCT edge_count) FROM source_expressions WHERE kind='fold-expression' AND edge_count IN(1,2)")==2)
            check('configured-logical-fold-token',scalar("SELECT count(*) FROM source_expression_folds WHERE operator_token=56 AND operator_spelling='&&'")>=4)
            check('original-pack-pattern-markers',scalar("SELECT count(*) FROM source_expressions WHERE is_pack_expansion=1")>0)
            check('selected-original-member',scalar("SELECT count(*) FROM source_expression_field_references r JOIN source_record_fields f ON f.id=r.source_field_id WHERE f.source_name='first'")>0)
            check('no-runtime-fold-node',scalar("SELECT count(*) FROM nodes WHERE kind IN('fold-expression','braced-initializer-list','field-reference')")==0)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('missing-fold-provenance',"DELETE FROM source_expression_folds WHERE expression_id=(SELECT min(expression_id) FROM source_expression_folds)",'Original fold expression is missing its operator provenance'),
            ('wrong-fold-token',"UPDATE source_expression_folds SET operator_token=38 WHERE operator_token=56",'Original fold expression lost its exact operator and association'),
            ('nonfold-token',"UPDATE source_expression_folds SET operator_token=52",'Original fold operator is not a configured C++17 fold token'),
            ('missing-fold-operand',"UPDATE source_expressions SET edge_count=0 WHERE id=(SELECT min(expression_id) FROM source_expression_folds); DELETE FROM source_expression_edges WHERE expression_id=(SELECT min(expression_id) FROM source_expression_folds)",'Original source expression operands do not match fold-expression'),
            ('missing-original-member',"DELETE FROM source_expression_field_references",'Original field reference is missing its declaration provenance'),
            ('wrong-member-type',"UPDATE source_expression_field_references SET source_field_id=(SELECT min(id) FROM source_record_fields WHERE source_name='other')",'Original field reference differs from its selected declaration type'),
            ('wrong-brace-role',"UPDATE source_expression_edges SET role='operand' WHERE expression_id IN(SELECT id FROM source_expressions WHERE kind='braced-initializer-list')",'Original source expression operands do not match braced-initializer-list'),
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
