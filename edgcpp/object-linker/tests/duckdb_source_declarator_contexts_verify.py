#!/usr/bin/env python3
"""Check anonymous routine parser-factory ownership of borrowed source types."""
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
    for key in ('compiler','linker','packager','output-dir'):
        parser.add_argument('--'+key,type=Path,required=True)
    for key in ('stock-compiler','stock-runtime'):parser.add_argument('--'+key,type=Path)
    parser.add_argument('--native-aot',action='store_true')
    parser.add_argument('--std',choices=('c++17','c++20'),default='c++20')
    args=parser.parse_args()
    if bool(args.stock_compiler)!=bool(args.stock_runtime):parser.error('both stock tools required')
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_declarator_contexts.cpp'),source)
    tooling=out/'tooling';tooling.mkdir()
    for name in ('compile.py','schema.sql'):shutil.copy2(args.packager.resolve().parent/name,tooling/name)
    spec=importlib.util.spec_from_file_location('packager',tooling/'compile.py');packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
    checks=[];commands=[];error=None
    def check(name,value):
        if not value:raise AssertionError(name)
        checks.append(name)
    def run(name,command,success=True,diagnostic=None,env=None):
        command=list(map(str,command));result=subprocess.run(command,cwd=out,capture_output=True,timeout=600,env=env)
        (out/(name+'.stdout')).write_bytes(result.stdout);(out/(name+'.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name,command=command,exitCode=result.returncode))
        check(name,(result.returncode==0)==success)
        if diagnostic:check(name+'-diagnostic',diagnostic in (result.stdout+result.stderr).decode(errors='replace'))
    try:
        for opt in ('0','2'):
            run('native-build-'+opt,['c++','-std='+args.std,'-O'+opt,source,'-o',out/('native-'+opt)])
            run('native-'+opt,[out/('native-'+opt)])
        if args.stock_compiler:
            run('stock-lower',[args.stock_compiler.resolve(),'--'+args.std,'--instantiate','used','--no_inlining','--exceptions','--clear_flag','use_predefined_macro_file','--gen_c_file_name',out/'stock.c','-o',out/'stock.il',source],
                env=dict(os.environ,PATH=str(args.stock_compiler.resolve().parent)+os.pathsep+os.environ['PATH']))
            for opt in ('0','2'):
                binary=out/('stock-'+opt)
                run('stock-build-'+opt,['gcc','-w','-falign-functions=4','-O'+opt,out/'stock.c',args.stock_runtime.resolve(),'-lstdc++','-lpthread','-lm','-o',binary])
                run('stock-'+opt,[binary])
        obj=out/'unit.dotobj'
        run('export',['python3','-B',tooling/'compile.py',source,'--output',obj,'--work-dir',out/'work','--compiler',args.compiler.resolve(),'--std',args.std,'--exceptions'])
        source.rename(out/'retained-source.cpp')
        run('verify-source-hidden',['dotnet',args.linker.resolve(),'verify',obj])
        with sqlite3.connect(obj) as db:
            def scalar(sql):return db.execute(sql).fetchone()[0]
            rows=list(db.execute("SELECT id,routine_source_type_id,scope_ordinal,kind,selected_source_declaration_id,source_record_id,header_source_record_id,argument_count,borrowed_argument_count FROM source_declarator_contexts ORDER BY id"))
            check('owned-routine-and-class-contexts',{'routine','class','class-partial'} <= {r[3] for r in rows})
            check('incomplete-symbol-factory-retained',scalar("SELECT count(*) FROM source_declarator_contexts WHERE capture_proof='observed-function-instantiation-declarator-context' AND kind='routine'")>0)
            check('exact-required-context-counts',scalar('SELECT sum(declarator_context_count) FROM source_routine_types')==len(rows))
            context,owner,scope,kind,declaration,record,header,count,borrowcount=next(r for r in rows if r[3]=='routine')
            prototype=scalar(f'SELECT template_prototype_declaration_id FROM source_expression_declarations WHERE id={declaration}')
            ordinal,borrowed=db.execute('SELECT argument_ordinal,source_type_id FROM source_declarator_borrowed_arguments WHERE context_id=? ORDER BY argument_ordinal',(context,)).fetchone()
            check('borrowed-argument-count-exact',scalar(f'SELECT count(*) FROM source_declarator_borrowed_arguments WHERE context_id={context}')==borrowcount)
            check('same-original-argument-node',scalar(f"SELECT count(*) FROM source_expression_declaration_arguments WHERE declaration_id={declaration} AND ordinal={ordinal} AND kind='type' AND source_type_id={borrowed}")==1)
            check('original-prototype-retained',scalar(f'SELECT is_template_prototype FROM source_expression_declarations WHERE id={prototype}')==1)
            check('anonymous-signatures-have-no-selected-routine',all(scalar(f"SELECT count(*) FROM source_expression_declarations WHERE kind='routine' AND source_type_id={row[1]}")==0 for row in rows))
            check('factory-source-locations-retained',scalar('SELECT count(*) FROM source_declarator_contexts WHERE source_file_id IS NULL OR line<=0 OR column<=0')==0)
            class_context=next(r for r in rows if r[3]=='class')
            check('class-selected-original-header',scalar(f'SELECT prototype_source_record_id FROM source_record_instantiations WHERE source_record_id={class_context[5]}')==class_context[6])
            partial_context=next(r for r in rows if r[3]=='class-partial')
            check('partial-selected-original-header',scalar(f'SELECT selected_source_record_id FROM source_class_specializations WHERE source_record_id={partial_context[5]}')==partial_context[6])
            # Walk only owned transparent type wrappers, matching the production
            # boundary; the selected recipe remains owned by begin, not find_if.
            def parameter_references(root,seen=None):
                seen=set() if seen is None else seen
                if root in seen:return []
                seen.add(root);result=[]
                op=db.execute('SELECT expression_id FROM source_type_operators WHERE type_id=?',(root,)).fetchone()
                if op and op[0]:
                    pending=[op[0]];visited=set()
                    while pending:
                        expression=pending.pop()
                        if expression in visited:continue
                        visited.add(expression)
                        ref=db.execute('SELECT binder_id FROM source_expression_parameter_references WHERE expression_id=?',(expression,)).fetchone()
                        if ref:result.append((expression,ref[0]))
                        pending.extend(x[0] for x in db.execute('SELECT child_expression_id FROM source_expression_edges WHERE expression_id=?',(expression,)))
                kind,underlying=db.execute('SELECT kind,underlying_type_id FROM source_types WHERE id=?',(root,)).fetchone()
                if underlying and kind in ('qualified','typedef','pointer','lvalue-reference','rvalue-reference','array'):
                    result.extend(parameter_references(underlying,seen))
                return result
            refs=parameter_references(borrowed)
            check('borrowed-source-parameter-recipe-retained',bool(refs))
            check('original-recipe-not-rebound',all(scalar(f'SELECT routine_source_type_id FROM source_parameter_binders WHERE id={binder}')!=owner for _,binder in refs))
            all_borrows=[row[0] for row in db.execute('SELECT DISTINCT source_type_id FROM source_declarator_borrowed_arguments')]
            check('class-context-borrows-original-recipes',all(parameter_references(t) for t in all_borrows))
            other=scalar(f"SELECT id FROM source_types WHERE kind='int' LIMIT 1")
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('function-factory-on-class',f"UPDATE source_declarator_contexts SET capture_proof='observed-function-instantiation-declarator-context' WHERE id={class_context[0]}",
                'Anonymous routine declarator lacks its original instantiation factory context'),
            ('missing-context',f'DELETE FROM source_declarator_borrowed_arguments WHERE context_id={context}; DELETE FROM source_declarator_contexts WHERE id={context}',
                'Anonymous routine is missing its required original declarator contexts'),
            ('wrong-selected-declaration',f'UPDATE source_declarator_contexts SET selected_source_declaration_id={prototype} WHERE id={context}',
                'Routine declarator context lost its exact original function template instance'),
            ('wrong-ordered-type',f'UPDATE source_declarator_borrowed_arguments SET source_type_id={other} WHERE context_id={context} AND argument_ordinal={ordinal}',
                'Anonymous routine borrow differs from its exact ordered original type argument'),
            ('wrong-argument-count',f'UPDATE source_declarator_contexts SET argument_count=argument_count+1 WHERE id={context}',
                'Routine declarator context original argument count disagrees with its selected instance'),
            ('incomplete-list',f'DELETE FROM source_declarator_borrowed_arguments WHERE context_id={context} AND argument_ordinal={ordinal}',
                'Anonymous routine original borrowed argument list is incomplete'),
            ('wrong-selected-class-header',f'UPDATE source_declarator_contexts SET header_source_record_id={partial_context[6]} WHERE id={class_context[0]}',
                'Routine declarator context lost its exact original class template instance'),
            ('rebound-recipe',f'UPDATE source_parameter_binders SET routine_source_type_id={owner} WHERE id={refs[0][1]}',
                'Anonymous routine borrow was rebound to the consuming declarator prototype'),
        ]
        for name,sql,diagnostic in negatives:
            damaged=out/(name+'.dotobj');shutil.copy2(obj,damaged)
            with sqlite3.connect(damaged) as db:
                db.execute('PRAGMA ignore_check_constraints=ON');before=db.total_changes
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
