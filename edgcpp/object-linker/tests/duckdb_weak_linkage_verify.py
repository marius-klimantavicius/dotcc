#!/usr/bin/env python3
"""Compare weak routine selection, missing addresses and provider extraction with native/stock C."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ('compiler','linker','packager','stock-compiler','stock-runtime','output-dir'):
        parser.add_argument('--'+name,type=Path,required=True)
    parser.add_argument('--native-aot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    fixtures=Path(__file__).with_name('duckdb_weak_linkage');checks=[];commands=[];error=None
    def check(name,value):
        if not value:raise AssertionError(name)
        checks.append(name)
    def run(name,cmd,success=True):
        cmd=list(map(str,cmd));p=subprocess.run(cmd,cwd=out,capture_output=True,timeout=600)
        (out/(name+'.stdout')).write_bytes(p.stdout);(out/(name+'.stderr')).write_bytes(p.stderr)
        commands.append(dict(name=name,command=cmd,exitCode=p.returncode));check(name,(p.returncode==0)==success)
    units={}
    try:
        variants=[('fallback','fallback.cpp',''),('override','override.cpp',''),('second','second.cpp',''),
          ('main11','main.cpp','#define EXPECTED 11\n'),('main23','main.cpp','#define EXPECTED 23\n'),
          ('main19','main.cpp','#define EXPECTED 19\n'),('provider','provider.cpp',''),
          ('optional','provider_main.cpp',''),('activated','provider_main.cpp','#define ACTIVATE 1\n')]
        for name,fixture,prefix in variants:
            source=out/(name+'.cpp');source.write_text(prefix+(fixtures/fixture).read_text())
            run(name+'-stock-lower',[args.stock_compiler.resolve(),'--c++17','--gnu_version','130300','--clear_flag','use_predefined_macro_file','--instantiate','used','--no_inlining','--gen_c_file_name',out/(name+'.c'),'-o',out/(name+'.il'),source])
            for mode in ('native','stock'):
                for opt in ('0','2'):
                    run(name+'-'+mode+'-build-'+opt,['c++' if mode=='native' else 'gcc','-w','-falign-functions=4','-O'+opt,'-c',source if mode=='native' else out/(name+'.c'),'-o',out/(name+'-'+mode+opt+'.o')])
            obj=out/(name+'.dotobj')
            run(name+'-export',['python3','-B',args.packager.resolve(),source,'--output',obj,'--work-dir',out/(name+'-work'),'--compiler',args.compiler.resolve()])
            source.rename(out/(name+'.retained.cpp'));run(name+'-verify-hidden',['dotnet',args.linker.resolve(),'verify',obj]);units[name]=obj
        with sqlite3.connect(units['fallback']) as db:
            check('weak-definition-retained',db.execute("SELECT definition,is_weak FROM entities WHERE source_name='selected'").fetchone()==('weak',1))
        with sqlite3.connect(units['main11']) as db:
            check('weak-declaration-retained',db.execute("SELECT definition,is_weak FROM entities WHERE source_name='absent'").fetchone()==('declaration',1))
        scenarios=[('fallback',['main11','fallback'],[]),('strong-last',['main23','fallback','override'],[]),
            ('strong-first',['override','main23','fallback'],[]),('first-weak',['main11','fallback','second'],[]),
            ('second-weak',['main19','second','fallback'],[]),('optional',['optional'],['provider']),
            ('activated',['activated'],['provider'])]
        for name,primary,providers in scenarios:
            for mode in ('native','stock'):
                for opt in ('0','2'):
                    objects=[out/(unit+'-'+mode+opt+'.o') for unit in primary]
                    if providers:
                        archive=out/(name+'-'+mode+opt+'.a');run(name+'-'+mode+'-archive-'+opt,['ar','rcs',archive,*[out/(unit+'-'+mode+opt+'.o') for unit in providers]]);objects.append(archive)
                    binary=out/(name+'-'+mode+opt)
                    run(name+'-'+mode+'-link-'+opt,['c++',*objects,*([args.stock_runtime.resolve()] if mode=='stock' else []),'-lpthread','-lm','-o',binary]);run(name+'-'+mode+'-run-'+opt,[binary])
            generated=out/(name+'-generated')
            command=['dotnet',args.linker.resolve(),'link','--entry','main','--out',generated,*[units[unit] for unit in primary]]
            for unit in providers:command+=['--provider',units[unit]]
            run(name+'-link',command)
            run(name+'-managed-build',['dotnet','build',generated/'Translated.csproj','-c','Release','-v:q'])
            run(name+'-managed-run',['dotnet',generated/'bin/Release/net10.0/Translated.dll'])
            if args.native_aot and name=='strong-last':
                run(name+'-aot-publish',['dotnet','publish',generated/'Translated.csproj','-c','Release','-r','linux-x64','--self-contained','true','-p:PublishAot=true','-o',out/'aot','-v:q']);run(name+'-aot-run',[out/'aot/Translated'])
            report=json.loads((generated/'link-report.json').read_text());check(name+'-no-fabricated-externs',not report['runtimeRequirements'])
        spec=importlib.util.spec_from_file_location('packager',args.packager.resolve());packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
        for name,sql in [('false-strength',"UPDATE entities SET is_weak=0 WHERE definition='weak'"),('internal-weak',"UPDATE entities SET linkage='internal',external_key=NULL WHERE is_weak=1")]:
            damaged=out/(name+'.dotobj');shutil.copy2(units['fallback'],damaged)
            with sqlite3.connect(damaged) as db:
                db.execute('PRAGMA ignore_check_constraints=ON');db.execute(sql);db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
            run(name+'-reject',['dotnet',args.linker.resolve(),'verify',damaged],False)
            check(name+'-diagnostic','Invalid entity summary' in (out/(name+'-reject.stderr')).read_text())
    except Exception as exception:error=str(exception)
    result=dict(status='failed' if error else 'passed',error=error,checks=checks,checkCount=len(checks),commands=commands)
    (out/'results.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({key:result[key] for key in ('status','error','checkCount')}));return int(bool(error))

if __name__=='__main__':raise SystemExit(main())
