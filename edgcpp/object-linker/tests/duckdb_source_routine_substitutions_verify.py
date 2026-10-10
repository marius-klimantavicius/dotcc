#!/usr/bin/env python3
"""Check original type-argument lexical ownership across routine substitution."""
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
    args=parser.parse_args()
    if bool(args.stock_compiler)!=bool(args.stock_runtime):parser.error('both stock tools required')
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_routine_substitutions.cpp'),source)
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
            run('native-build-'+opt,['c++','-std=c++20','-O'+opt,source,'-o',out/('native-'+opt)])
            run('native-'+opt,[out/('native-'+opt)])
        if args.stock_compiler:
            run('stock-lower',[args.stock_compiler.resolve(),'--c++20','--instantiate','used','--no_inlining','--exceptions','--clear_flag','use_predefined_macro_file','--gen_c_file_name',out/'stock.c','-o',out/'stock.il',source],
                env=dict(os.environ,PATH=str(args.stock_compiler.resolve().parent)+os.pathsep+os.environ['PATH']))
            for opt in ('0','2'):
                binary=out/('stock-'+opt)
                run('stock-build-'+opt,['gcc','-w','-falign-functions=4','-O'+opt,out/'stock.c',args.stock_runtime.resolve(),'-lstdc++','-lpthread','-lm','-o',binary])
                run('stock-'+opt,[binary])
        obj=out/'unit.dotobj'
        run('export',['python3','-B',tooling/'compile.py',source,'--output',obj,'--work-dir',out/'work','--compiler',args.compiler.resolve(),'--std','c++20','--exceptions'])
        source.rename(out/'retained-source.cpp')
        run('verify-source-hidden',['dotnet',args.linker.resolve(),'verify',obj])
        with sqlite3.connect(obj) as db:
            def scalar(sql):return db.execute(sql).fetchone()[0]
            rows=list(db.execute("SELECT s.routine_source_type_id,s.selected_source_declaration_id,d.source_name,d.template_prototype_declaration_id,s.borrowed_argument_count FROM source_routine_type_substitutions s JOIN source_expression_declarations d ON d.id=s.selected_source_declaration_id"))
            check('selected-find-if-original-declaration',any(r[2]=='find_if' for r in rows))
            check('selected-nested-pointer-original-declaration',any(r[2]=='identity' for r in rows))
            check('markers-own-exact-relations',scalar('SELECT count(*) FROM source_routine_types WHERE has_borrowed_arguments=1')==len(rows))
            owner,declaration,_,prototype,count=next(r for r in rows if r[2]=='find_if')
            ordinal,borrowed=db.execute('SELECT argument_ordinal,source_type_id FROM source_routine_borrowed_arguments WHERE routine_source_type_id=? ORDER BY argument_ordinal',(owner,)).fetchone()
            check('first-original-argument-retained',ordinal==0)
            check('borrowed-argument-count-exact',scalar(f'SELECT count(*) FROM source_routine_borrowed_arguments WHERE routine_source_type_id={owner}')==count)
            check('same-original-argument-node',scalar(f"SELECT count(*) FROM source_expression_declaration_arguments WHERE declaration_id={declaration} AND ordinal={ordinal} AND kind='type' AND source_type_id={borrowed}")==1)
            check('original-prototype-retained',scalar(f'SELECT is_template_prototype FROM source_expression_declarations WHERE id={prototype}')==1)
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
            nested=next(r for r in rows if r[2]=='identity')
            nested_type=scalar(f'SELECT source_type_id FROM source_routine_borrowed_arguments WHERE routine_source_type_id={nested[0]}')
            check('nested-pointer-argument-recipe-retained',scalar(f"SELECT kind FROM source_types WHERE id={nested_type}")=='pointer' and bool(parameter_references(nested_type)))
            other=scalar(f"SELECT id FROM source_types WHERE kind='int' LIMIT 1")
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('missing-substitution',f'DELETE FROM source_routine_borrowed_arguments WHERE routine_source_type_id={owner}; DELETE FROM source_routine_type_substitutions WHERE routine_source_type_id={owner}',
                'Original routine substitution is missing its required borrowed-argument provenance'),
            ('wrong-selected-declaration',f'UPDATE source_routine_type_substitutions SET selected_source_declaration_id={prototype} WHERE routine_source_type_id={owner}',
                'Borrowed routine type lacks its exact selected original template declaration'),
            ('wrong-ordered-type',f'UPDATE source_routine_borrowed_arguments SET source_type_id={other} WHERE routine_source_type_id={owner} AND argument_ordinal={ordinal}',
                'Borrowed source argument differs from its exact ordered original type argument'),
            ('incomplete-list',f'UPDATE source_routine_type_substitutions SET borrowed_argument_count=borrowed_argument_count+1 WHERE routine_source_type_id={owner}',
                'Original routine borrowed-argument list is incomplete'),
            ('rebound-recipe',f'UPDATE source_parameter_binders SET routine_source_type_id={owner} WHERE id={refs[0][1]}',
                'Borrowed original argument was rebound to its consuming routine prototype'),
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
