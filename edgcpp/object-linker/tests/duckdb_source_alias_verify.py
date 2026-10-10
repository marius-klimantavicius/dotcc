#!/usr/bin/env python3
"""Validate original alias substitution and type-operator graphs without source or executable ABI fabrication."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_alias.cpp'),source)
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
            check('original-named-alias-declarations',scalar("SELECT count(*) FROM source_named_templates")>=7)
            check('original-alias-RHS-operators',scalar("SELECT count(*) FROM source_type_operators WHERE expression_id IS NOT NULL AND opcode=1")>=3)
            check('type-operand-queries',scalar("SELECT count(DISTINCT opcode) FROM source_type_operators WHERE opcode IN(5,7) AND operand_source_type_id IS NOT NULL")==2)
            check('decltype-parentheses-retained',scalar("SELECT count(DISTINCT is_unparenthesized_decltype) FROM source_type_operators WHERE opcode=1")==2)
            check('dependent-alias-substitutions',scalar("SELECT count(*) FROM source_alias_instances WHERE is_nonreal=1 AND is_dependent=1")>0)
            check('named-template-arguments',scalar("SELECT count(*) FROM source_alias_arguments a JOIN source_templates t ON t.id=a.template_id WHERE a.kind='template' AND t.kind='alias'")>0)
            check('original-argument-boundaries',scalar("SELECT count(*) FROM source_constraint_argument_details WHERE owner_kind='alias-selected'")==scalar("SELECT count(*) FROM source_alias_arguments WHERE list_kind='alias-selected'"))
            check('source-expression-selected-overloads',scalar("SELECT count(*) FROM source_dependent_function_candidates")>0)
            check('alias-prototype-owns-original-RHS',scalar("SELECT count(*) FROM source_named_templates n JOIN source_alias_instances a ON a.type_id=n.prototype_source_type_id WHERE a.is_prototype=1 AND a.template_id=n.template_id")==scalar("SELECT count(*) FROM source_named_templates"))
            check('no-fabricated-template-parameter-coordinates',scalar("SELECT count(*) FROM source_templates WHERE kind='alias' AND (parameter_depth IS NOT NULL OR parameter_position IS NOT NULL OR is_pack!=0)")==0)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('missing-type-operator',"DELETE FROM source_type_operators WHERE type_id=(SELECT min(type_id) FROM source_type_operators)",'Original source type is missing its required alias/operator provenance'),
            ('operator-name-mismatch',"UPDATE source_type_operators SET operator_name='__remove_const' WHERE opcode=1",'Original source type operator lost its exact opcode/operand or decltype semantics'),
            ('false-decltype-bit',"UPDATE source_type_operators SET is_unparenthesized_decltype=1 WHERE opcode=7",'Original source type operator lost its exact opcode/operand or decltype semantics'),
            ('false-alias-prototype',"UPDATE source_alias_instances SET is_prototype=0 WHERE type_id=(SELECT min(prototype_source_type_id) FROM source_named_templates)",'Alias template does not own its exact original prototype type'),
            ('missing-alias-argument',"UPDATE source_alias_instances SET argument_count=argument_count+1 WHERE type_id=(SELECT min(type_id) FROM source_alias_instances)",'Alias instance original argument lists/prototype ownership are incomplete'),
            ('foreign-alias-prototype',"UPDATE source_named_templates SET prototype_source_type_id=(SELECT max(prototype_source_type_id) FROM source_named_templates) WHERE template_id=(SELECT min(template_id) FROM source_named_templates)",'Alias template does not own its exact original prototype type'),
            ('missing-argument-expansion-facts',"DELETE FROM source_constraint_argument_details WHERE owner_kind='alias-selected'",'Original argument lacks exact pack, expansion and substitution facts'),
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
