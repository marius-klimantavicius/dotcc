#!/usr/bin/env python3
"""Validate original namespace ownership and separate private class identities across translation units."""
import argparse, importlib.util, json, shutil, sqlite3, subprocess
from pathlib import Path

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for key in ('compiler','linker','packager','output-dir'):parser.add_argument('--'+key,type=Path,required=True)
    parser.add_argument('--native-aot',action='store_true');args=parser.parse_args()
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);checks=[];commands=[];error=None
    tooling=out/'tooling';tooling.mkdir()
    for name in ('compile.py','schema.sql'):shutil.copy2(args.packager.resolve().parent/name,tooling/name)
    spec=importlib.util.spec_from_file_location('packager',tooling/'compile.py');packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
    sources=[]
    for name in ('duckdb_source_namespaces_first.cpp','duckdb_source_namespaces_second.cpp','duckdb_source_namespaces.hpp'):
        p=out/name;shutil.copy2(Path(__file__).with_name(name),p);sources.append(p)
    def check(name,value):
        if not value:raise AssertionError(name)
        checks.append(name)
    def run(name,command,success=True,diagnostic=None):
        command=list(map(str,command));result=subprocess.run(command,cwd=out,capture_output=True,timeout=600)
        (out/(name+'.stdout')).write_bytes(result.stdout);(out/(name+'.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name,command=command,exitCode=result.returncode));check(name,(result.returncode==0)==success)
        if diagnostic:check(name+'-diagnostic',diagnostic in (result.stdout+result.stderr).decode(errors='replace'))
    try:
        for opt in ('0','2'):
            exe=out/('native-'+opt);run('native-build-'+opt,['c++','-std=c++20','-O'+opt,*sources[:2],'-o',exe]);run('native-'+opt,[exe])
        objects=[];identities=[]
        for index,source in enumerate(sources[:2]):
            obj=out/('unit'+str(index)+'.dotobj');objects.append(obj)
            run('export'+str(index),['python3','-B',tooling/'compile.py',source,'--output',obj,'--work-dir',out/('work'+str(index)),'--compiler',args.compiler.resolve(),'--std','c++20','--exceptions'])
        for source in sources:source.rename(source.with_suffix(source.suffix+'.retained'))
        for index,obj in enumerate(objects):
            run('verify-hidden'+str(index),['dotnet',args.linker.resolve(),'verify',obj])
            with sqlite3.connect(obj) as db:
                def scalar(sql):return db.execute(sql).fetchone()[0]
                check('one-reopened-unnamed-owner'+str(index),scalar("SELECT count(*) FROM source_namespaces n JOIN source_namespaces p ON p.id=n.parent_id WHERE n.is_unnamed=1 AND p.source_name='original_public'")==1)
                check('named-internal-descendant'+str(index),scalar("SELECT count(*) FROM source_namespaces n JOIN source_namespaces p ON p.id=n.parent_id WHERE n.source_name='named_child' AND n.has_internal_linkage=1 AND p.is_unnamed=1")==1)
                for kind in ('type','declaration','concept','template'):
                    check('owned-'+kind+str(index),scalar("SELECT count(*) FROM source_namespace_owners WHERE owner_kind='"+kind+"'")>0)
                rows=list(db.execute("SELECT identity_key,native_size FROM source_record_types WHERE source_name='Hidden'"));check('private-record-root'+str(index),len(rows)==1 and rows[0][0].startswith('namespace-private:'))
                identities.append(rows[0]);check('retained-private-enum'+str(index),scalar("SELECT count(*) FROM source_enum_types WHERE identity_key LIKE 'namespace-private:%'")>0)
        check('cross-unit-private-identities-differ',identities[0][0]!=identities[1][0]);check('cross-unit-private-layouts-differ',sorted(x[1] for x in identities)==[8,12])
        run('link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',out/'generated',*objects])
        run('build',['dotnet','build',out/'generated/Translated.csproj','-c','Release','-v:q']);run('jit',['dotnet',out/'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish',['dotnet','publish',out/'generated/Translated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','-v:q']);run('nativeaot',[out/'aot/Translated'])
        negatives=[
            ('duplicate-namespace-declaration',"INSERT INTO source_namespaces SELECT (SELECT max(id)+1 FROM source_namespaces),parent_id,source_name,is_unnamed,is_inline,has_internal_linkage,source_file_id,line,column,capture_proof FROM source_namespaces WHERE source_name='named_child'",'Repeated original namespace identity must reuse its exact declaration row'),
            ('fabricated-unnamed-spelling',"UPDATE source_namespaces SET source_name='invented' WHERE is_unnamed=1",'Original namespace declaration lost its name, flags or location'),
            ('false-external-internal-flag',"UPDATE source_namespaces SET has_internal_linkage=1 WHERE source_name='original_public'",'Internal namespace lacks its original unnamed ancestor'),
            ('missing-declaration-edge',"DELETE FROM source_namespace_owners WHERE owner_kind='declaration'",'Original namespace ownership markers differ from the owned declaration edges'),
            ('false-type-marker',"UPDATE source_types SET has_namespace_owner=0 WHERE has_namespace_owner=1",'Original namespace ownership markers differ from the owned declaration edges'),
            ('namespace-parent-cycle',"UPDATE source_namespaces SET parent_id=(SELECT id FROM source_namespaces WHERE source_name='named_child') WHERE source_name='original_public'",'Original namespace parent graph is absent or cyclic'),
        ]
        for name,sql,diagnostic in negatives:
            damaged=out/(name+'.dotobj');shutil.copy2(objects[0],damaged)
            with sqlite3.connect(damaged) as db:
                db.execute('PRAGMA ignore_check_constraints=ON');before=db.total_changes;db.execute(sql);check(name+'-changed',db.total_changes>before)
                check(name+'-foreign-keys',not list(db.execute('PRAGMA foreign_key_check')));db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
            run(name,['dotnet',args.linker.resolve(),'verify',damaged],False,diagnostic)
    except Exception as ex:error=str(ex)
    result=dict(status='failed' if error else 'passed',error=error,checks=checks,commands=commands);(out/'results.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(dict(status=result['status'],checks=len(checks),error=error)));return int(error is not None)
if __name__=='__main__':raise SystemExit(main())
