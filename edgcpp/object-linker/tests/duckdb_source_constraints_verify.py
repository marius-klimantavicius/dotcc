#!/usr/bin/env python3
"""Validate original concept/requirement graphs without source or executable ABI fabrication."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_constraints.cpp'),source)
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
            check('owned-concepts',scalar('SELECT count(*) FROM source_concepts')>=5)
            check('owned-constrained-parameters',scalar('SELECT count(*) FROM source_type_constraints')>=4)
            check('all-requirement-forms',scalar("SELECT count(DISTINCT kind) FROM source_expressions WHERE kind IN('type-operand','compound-requirement','nested-requirement','parameter-reference')")==4)
            check('compound-noexcept',scalar('SELECT count(*) FROM source_expression_requirements WHERE requires_noexcept=1')>0)
            check('compound-return-concept',scalar("SELECT count(*) FROM source_expression_edges WHERE role='constraint' AND expression_id IN (SELECT expression_id FROM source_expression_requirements)")>0)
            check('nested-lexical-parameter',scalar('SELECT count(*) FROM source_expression_parameter_references WHERE original_levels_up>0')>0)
            check('semantic-only-declaration-refs',scalar("SELECT count(DISTINCT kind) FROM source_expression_declarations WHERE definition_status='reference-only'")==2)
            check('original-cv-fetch-types',scalar('SELECT count(*) FROM source_expressions WHERE original_lvalue_type_id IS NOT NULL')>0)
            check('constexpr-initializer-semantics',scalar("SELECT count(*) FROM source_expression_declarations WHERE is_constexpr=1 AND initializer_kind IN('constant','expression')")>0)
            check('retained-selected-template-declarations',scalar('SELECT count(*) FROM source_expression_declarations WHERE template_prototype_declaration_id IS NOT NULL')>0)
            check('no-missing-constrained-roots',scalar('SELECT count(*) FROM source_dependent_types WHERE has_type_constraint=1')==scalar('SELECT count(*) FROM source_type_constraints'))
            check('ordered-semantic-operands',scalar('SELECT count(*) FROM source_expression_operations')>0)
            check('dependent-variable-constant-bridge',scalar('SELECT count(*) FROM source_semantic_constant_expressions')>0)
            check('owned-aggregate-cpo-initializer',scalar("SELECT count(*) FROM source_aggregate_constants a JOIN source_expression_declarations d ON d.initializer_constant_id=a.constant_id WHERE d.source_name='invoke' AND a.element_count=0")>0)

            check('dependent-function-lookup',scalar('SELECT count(*) FROM source_dependent_function_lookups WHERE has_explicit_template_arguments=1 AND candidate_count>0')>0)
            check('lookup-candidates-are-declarations',scalar('SELECT count(*) FROM source_dependent_function_candidates')>0)
            check('owned-pack-boundary',scalar("SELECT count(*) FROM source_concept_arguments WHERE kind='pack-boundary'")>0)
            check('owned-pack-expansion',scalar('SELECT count(*) FROM source_pack_expansions WHERE reference_count>0')>0)
            check('class-pack-boundaries',scalar("SELECT count(*) FROM source_template_arguments WHERE kind='pack-boundary'")>0)
            check('class-dependent-pack-pattern',scalar('SELECT count(*) FROM source_template_arguments WHERE is_pack=1')>0)
            check('class-pack-provenance',scalar("SELECT count(*) FROM source_constraint_argument_details WHERE owner_kind='class'")==scalar('SELECT count(*) FROM source_template_arguments'))
            check('implicit-parameter-is-not-fake-explicit-argument',scalar('SELECT count(*) FROM source_type_constraints t WHERE NOT EXISTS(SELECT 1 FROM source_concept_arguments a WHERE a.expression_id=t.constraint_expression_id)')>0)

            check('exact-original-pack-reference',scalar("SELECT count(*) FROM source_pack_references WHERE kind='type'")>0)

            check('original-size-align-queries',scalar("WITH RECURSIVE owned(id) AS (SELECT definition_expression_id FROM source_concepts WHERE source_name='Extent' UNION SELECT e.child_expression_id FROM source_expression_edges e JOIN owned o ON o.id=e.expression_id) SELECT count(DISTINCT c.kind) FROM owned o JOIN source_expression_constants x ON x.expression_id=o.id JOIN source_semantic_constants c ON c.id=x.constant_id JOIN source_dependent_types t ON t.type_id=c.operand_source_type_id WHERE c.kind IN('sizeof','alignof') AND t.kind='parameter'")==2)
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('missing-variable-bridge','DELETE FROM source_semantic_constant_expressions WHERE constant_id=(SELECT min(constant_id) FROM source_semantic_constant_expressions)','missing its required original expression'),
            ('missing-cpo-initializer','DELETE FROM source_aggregate_constants WHERE constant_id=(SELECT min(constant_id) FROM source_aggregate_constants)','missing its required initializer metadata'),
            ('changed-cpo-element-count','UPDATE source_aggregate_constants SET element_count=element_count+1','different element list or nonaggregate type'),
            ('concrete-nonboolean-constraint',"UPDATE source_expressions SET source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE id=(SELECT definition_expression_id FROM source_concepts WHERE source_name='Extent')",'Original constraint must retain its boolean semantic result type'),
            ('class-pack-pattern-changed','UPDATE source_template_arguments SET is_pack=0 WHERE is_pack=1','Dependent class identity differs'),
            ('missing-class-pack-facts',"DELETE FROM source_constraint_argument_details WHERE owner_kind='class'",'Original argument lacks exact pack'),
            ('missing-argument-facts','DELETE FROM source_constraint_argument_details WHERE rowid=(SELECT min(rowid) FROM source_constraint_argument_details)','Original argument lacks exact pack'),
            ('missing-lookup-candidate','DELETE FROM source_dependent_function_candidates WHERE rowid=(SELECT min(rowid) FROM source_dependent_function_candidates)','Dependent function lookup lost its original candidates'),
            ('wrong-pack-reference',"UPDATE source_pack_references SET source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE kind='type'",'Pack type reference is not its original template parameter'),
            ('missing-pack-reference','DELETE FROM source_pack_references WHERE rowid=(SELECT min(rowid) FROM source_pack_references)','pack expansion reference list is incomplete'),
            ('missing-constexpr-value',"UPDATE source_expression_declarations SET initializer_expression_id=NULL WHERE initializer_kind='expression'",'initializer lost its exact original kind'),
            ('missing-root','DELETE FROM source_type_constraints WHERE source_type_id=(SELECT min(source_type_id) FROM source_type_constraints)','Original type constraint lost its implicit first-type binding owner'),
            ('missing-requirement',"DELETE FROM source_expression_edges WHERE rowid=(SELECT min(rowid) FROM source_expression_edges WHERE role='requirement')",'Source expression operand order is not contiguous'),
            ('wrong-lexical-reference','UPDATE source_expression_parameter_references SET original_levels_up=original_levels_up+100 WHERE expression_id=(SELECT min(expression_id) FROM source_expression_parameter_references)','exact lexical binders'),
            ('wrong-operation-name',"UPDATE source_expression_operations SET operation_name='eok_error' WHERE expression_id=(SELECT min(expression_id) FROM source_expression_operations WHERE original_type_kind IS NOT NULL)",'opcode, name and operand count'),
            ('wrong-implicit-binding',"UPDATE source_concept_uses SET is_type_constraint=1 WHERE expression_id=(SELECT min(expression_id) FROM source_concept_uses WHERE is_type_constraint=0)",'implicit first-type binding owner'),
            ('missing-formal','DELETE FROM source_concept_parameters WHERE rowid=(SELECT max(rowid) FROM source_concept_parameters)','Original type constraint lost its implicit first-type binding owner'),
            ('wrong-selected-declaration-type',"UPDATE source_expression_declarations SET source_type_id=(SELECT id FROM source_types WHERE kind='int' LIMIT 1) WHERE kind='routine'",'Source declaration'),
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
