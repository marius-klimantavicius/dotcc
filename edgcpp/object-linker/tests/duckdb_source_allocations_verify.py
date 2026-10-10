#!/usr/bin/env python3
"""Validate typed original new/delete expressions without manufacturing runtime allocation."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_allocations.cpp'),source)
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
            check('original-source-allocations',scalar("SELECT count(*) FROM source_expression_allocations")>=7)
            check('concrete-type-deferred-lookup',scalar("SELECT count(*) FROM source_expression_allocations a JOIN source_types t ON t.id=a.object_source_type_id WHERE a.is_lookup_deferred=1 AND t.kind IN('int','array')")>=2)
            check('dependent-new-selection',scalar("SELECT count(*) FROM source_expression_allocations WHERE is_new=1 AND is_dependent_selection=1 AND operator_declaration_id IS NULL")>0)
            check('dependent-delete-families',scalar("SELECT count(DISTINCT is_array_delete) FROM source_expression_allocations WHERE is_new=0 AND is_dependent_selection=1")==2)
            check('original-delete-operands',scalar("SELECT count(*) FROM source_expression_edges WHERE role='delete-operand'")>=2)
            check('selected-global-new-families',scalar("SELECT count(DISTINCT d.operator_kind) FROM source_expression_allocations a JOIN source_declaration_operators d ON d.declaration_id=a.operator_declaration_id WHERE a.is_global=1 AND d.operator_kind IN(1,3)")==2)
            check('absence-retained-independently-of-value-init',scalar("SELECT count(*) FROM source_expression_allocations WHERE initialization_disposition!='absent-in-original' OR rollback_disposition!='absent-in-original' OR has_initializer!=0")==0)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('false-delete-deferred-new-lookup',"UPDATE source_expression_allocations SET is_lookup_deferred=1 WHERE is_new=0",'Original source allocation lost its exact selection or new/delete flags'),
            ('missing-allocation',"DELETE FROM source_expression_allocations WHERE expression_id=(SELECT min(expression_id) FROM source_expression_allocations)",'Original allocation expression is missing its original new/delete provenance'),
            ('false-new-array-delete',"UPDATE source_expression_allocations SET is_array_delete=1 WHERE is_new=1",'Original source allocation lost its exact selection or new/delete flags'),
            ('false-dependent-selection',"UPDATE source_expression_allocations SET is_dependent_selection=1 WHERE operator_declaration_id IS NOT NULL",'Original source allocation lost its exact selection or new/delete flags'),
            ('missing-operator-kind',"DELETE FROM source_declaration_operators",'Original operator declaration is missing its exact operator kind'),
            ('wrong-selected-operator',"UPDATE source_declaration_operators SET operator_kind=2 WHERE operator_kind=1",'Original allocation selected a different operator family'),
            ('wrong-deleted-object-type',"UPDATE source_expression_allocations SET object_source_type_id=(SELECT min(id) FROM source_types WHERE kind='dependent' AND source_name='U') WHERE is_new=0",'Original delete operand lacks its exact pointer/object correspondence'),
            ('missing-delete-operand',"UPDATE source_expressions SET edge_count=edge_count-1 WHERE id=(SELECT min(expression_id) FROM source_expression_edges WHERE role='delete-operand'); DELETE FROM source_expression_edges WHERE role='delete-operand' AND expression_id=(SELECT min(expression_id) FROM source_expression_edges WHERE role='delete-operand')",'Original source allocation lost its exact ordered operands and array bound'),
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
