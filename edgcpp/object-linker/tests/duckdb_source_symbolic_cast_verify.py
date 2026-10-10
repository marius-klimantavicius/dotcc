#!/usr/bin/env python3
"""Validate exact dependent symbolic casts without inventing integral widths."""
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
    source=out/'constraints.cpp';shutil.copy2(Path(__file__).with_name('duckdb_source_symbolic_cast.cpp'),source)
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
            check('dependent-cast-retains-exact-type',scalar("SELECT count(*) FROM source_semantic_constants c JOIN source_types t ON t.id=c.source_type_id WHERE c.kind='operation' AND c.operator_name='cast' AND t.kind='dependent' AND t.source_name='T'")==2)
            check('literal-retains-resolved-integral-type',scalar("SELECT count(*) FROM source_semantic_constants c JOIN source_types t ON t.id=c.source_type_id WHERE c.kind='integer' AND t.kind='int' AND c.value_text IN('-1','0')")==2)
            check('no-invented-dependent-value',scalar("SELECT count(*) FROM source_semantic_constants c JOIN source_types t ON t.id=c.source_type_id WHERE t.kind='dependent' AND c.value_text!=''")==0)
            try:
                db.execute("UPDATE source_semantic_constants SET value_text='1' WHERE kind='operation' AND operator_name='cast'")
            except sqlite3.IntegrityError: check('schema-rejects-fabricated-symbolic-value',True)
            else: raise AssertionError('schema accepted fabricated symbolic value')
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',obj])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q'])
        run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q'])
            run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('dependent-integer-literal',"UPDATE source_semantic_constants SET source_type_id=(SELECT min(id) FROM source_types WHERE kind='dependent' AND source_name='T') WHERE kind='integer'",'Expected a supported original integral type'),
            ('integer-overflow',"UPDATE source_semantic_constants SET value_text='2147483648' WHERE kind='integer' AND value_text='0'",'Source integer constant exceeds its exact original type'),
            ('wrapper-type-changed',"UPDATE source_semantic_constants SET source_type_id=(SELECT min(id) FROM source_types WHERE kind='int') WHERE kind='expression' AND source_type_id IN(SELECT id FROM source_types WHERE kind='dependent')",'Source constant expression wrapper changes its operand type'),
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
