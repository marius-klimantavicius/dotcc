#!/usr/bin/env python3
"""Check exact stock TLS wrappers and constant per-thread storage across units."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for key in ('compiler','linker','packager','stock-compiler','stock-runtime','output-dir'):
        parser.add_argument('--'+key,type=Path,required=True)
    parser.add_argument('--native-aot',action='store_true')
    args=parser.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    fixtures=Path(__file__).with_name('duckdb_tls_wrappers');checks=[];commands=[];error=None;units={}
    def check(name,value):
        if not value:raise AssertionError(name)
        checks.append(name)
    def run(name,command,success=True,diagnostic=None):
        command=list(map(str,command));result=subprocess.run(command,cwd=out,capture_output=True,timeout=600)
        (out/(name+'.stdout')).write_bytes(result.stdout);(out/(name+'.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name,command=command,exitCode=result.returncode));check(name,(result.returncode==0)==success)
        if diagnostic:check(name+'-diagnostic',diagnostic in (result.stdout+result.stderr).decode(errors='replace'))
    try:
        for name in ('constant-provider','constant-consumer','later-definition'):
            source=out/(name+'.cpp');shutil.copy2(fixtures/source.name,source)
            run(name+'-stock-lower',[args.stock_compiler.resolve(),'--c++17','--gnu_version','130300','--clear_flag','use_predefined_macro_file','--instantiate','used','--no_inlining','--gen_c_file_name',out/(name+'.c'),'-o',out/(name+'.il'),source])
            for mode in ('native','stock'):
                for opt in ('0','2'):
                    run(name+'-'+mode+'-compile-'+opt,['c++' if mode=='native' else 'gcc','-w','-O'+opt,'-c',source if mode=='native' else out/(name+'.c'),'-o',out/(name+'-'+mode+opt+'.o')])
            obj=out/(name+'.dotobj');run(name+'-export',['python3','-B',args.packager.resolve(),source,'--output',obj,'--work-dir',out/(name+'-work'),'--compiler',args.compiler.resolve()]);source.rename(out/(name+'.retained.cpp'))
            run(name+'-verify-hidden',['dotnet',args.linker.resolve(),'verify',obj]);units[name]=obj
        with sqlite3.connect(units['constant-consumer']) as db:
            check('exact-wrapper-alias',db.execute("SELECT kind,count(*) FROM tls_routine_origins GROUP BY kind ORDER BY kind").fetchall()==[('initializer-alias',1),('variable-wrapper',1)])
            check('exact-weak-guard',db.execute("SELECT count(*) FROM tls_routine_origins t JOIN entities e ON e.id=t.target_entity_id WHERE t.kind='variable-wrapper' AND t.guard_kind='weak-address' AND e.is_weak=1 AND e.definition='declaration'").fetchone()[0]==1)
        for name,primary in [('pair',['constant-consumer','constant-provider']),('later',['later-definition'])]:
            for mode in ('native','stock'):
                for opt in ('0','2'):
                    binary=out/(name+'-'+mode+opt)
                    run(name+'-'+mode+'-link-'+opt,['c++',*[out/(u+'-'+mode+opt+'.o') for u in primary],*([args.stock_runtime.resolve()] if mode=='stock' else []),'-lpthread','-lm','-o',binary]);run(name+'-'+mode+'-run-'+opt,[binary])
            generated=out/(name+'-generated');run(name+'-link',['dotnet',args.linker.resolve(),'link','--entry','main','--out',generated,*[units[u] for u in primary]])
            project=generated/'Translated.csproj';project.write_text(project.read_text().replace('<StartupObject>__DotObjEntry</StartupObject>','<StartupObject>TlsTestEntry</StartupObject>'))
            (generated/'TlsTestEntry.cs').write_text('''internal static class TlsTestEntry {
    public static int Main() {
        if (Translated.Program.main() != 0) return 1;
        int first = -1, second = -1;
        var a = new System.Threading.Thread(() => { first = Translated.Program.main(); });
        var b = new System.Threading.Thread(() => { second = Translated.Program.main(); });
        a.Start(); b.Start(); a.Join(); b.Join();
        if (first != 0 || second != 0) return 2;
        return Translated.Program.main() == 1 ? 0 : 3;
    }
}
''')
            run(name+'-managed-build',['dotnet','build',project,'-c','Release','-v:q']);run(name+'-managed-thread-run',['dotnet',generated/'bin/Release/net10.0/Translated.dll'])
            if args.native_aot and name=='pair':
                run(name+'-aot-publish',['dotnet','publish',project,'-c','Release','-r','linux-x64','--self-contained','true','-p:PublishAot=true','-o',out/'aot','-v:q']);run(name+'-aot-thread-run',[out/'aot/Translated'])
        spec=importlib.util.spec_from_file_location('packager',args.packager.resolve());packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
        mutations=[('missing-origin',"DELETE FROM tls_routine_origins WHERE kind='variable-wrapper'"),('missing-guard',"UPDATE tls_routine_origins SET guard_kind='always' WHERE kind='variable-wrapper'"),('missing-target',"UPDATE tls_routine_origins SET target_entity_id=NULL WHERE kind='variable-wrapper'"),('not-thread-local',"UPDATE storages SET is_thread_local=0"),('wrong-proof',"UPDATE tls_routine_origins SET capture_proof='unobserved'"),('wrong-kind',"UPDATE tls_routine_origins SET kind='empty-initializer' WHERE kind='variable-wrapper'")]
        for name,sql in mutations:
            obj=out/(name+'.dotobj');shutil.copy2(units['constant-consumer'],obj)
            with sqlite3.connect(obj) as db:
                db.execute('PRAGMA ignore_check_constraints=ON');db.execute(sql);db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
            run(name+'-reject',['dotnet',args.linker.resolve(),'verify',obj],False)
    except Exception as exception:error=str(exception)
    result=dict(status='failed' if error else 'passed',error=error,checks=checks,checkCount=len(checks),commands=commands)
    (out/'results.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({k:result[k] for k in ('status','error','checkCount')}));return int(bool(error))

if __name__=='__main__':raise SystemExit(main())
