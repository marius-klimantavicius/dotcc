#!/usr/bin/env python3
"""Verify C compound-literal initialization uses the same owned pooled-storage proof."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    p=argparse.ArgumentParser(description=__doc__)
    for name in ('compiler','linker','packager','stock-compiler','stock-runtime','output-dir'):p.add_argument('--'+name,type=Path,required=True)
    p.add_argument('--native-aot',action='store_true');a=p.parse_args();out=a.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    source=out/'pooled.c';shutil.copy2(Path(__file__).with_name('c_pooled_record_initialization.c'),source)
    checks=[];commands=[];error=None
    def check(name,value):
        if not value:raise AssertionError(name)
        checks.append(name)
    def run(name,cmd,success=True):
        cmd=list(map(str,cmd));r=subprocess.run(cmd,cwd=out,capture_output=True,timeout=600)
        (out/(name+'.stdout')).write_bytes(r.stdout);(out/(name+'.stderr')).write_bytes(r.stderr)
        commands.append(dict(name=name,command=cmd,exitCode=r.returncode));check(name,(r.returncode==0)==success)
    try:
        for opt in ('0','2'):
            binary=out/('native'+opt);run('native-build'+opt,['gcc','-std=c11','-O'+opt,source,'-o',binary]);run('native-run'+opt,[binary])
        run('stock-lower',[a.stock_compiler.resolve(),'--c11','--gnu_version','130300','--clear_flag','use_predefined_macro_file','--gen_c_file_name',out/'stock.c','-o',out/'stock.il',source])
        for opt in ('0','2'):
            binary=out/('stock'+opt);run('stock-build'+opt,['gcc','-w','-O'+opt,out/'stock.c',a.stock_runtime.resolve(),'-lstdc++','-lpthread','-lm','-o',binary]);run('stock-run'+opt,[binary])
        obj=out/'unit.dotobj';run('export',['python3','-B',a.packager.resolve(),source,'--std','c11','--output',obj,'--work-dir',out/'work','--compiler',a.compiler.resolve()]);source.rename(out/'retained.c')
        with sqlite3.connect(obj) as db:
            check('source-language-c',db.execute('SELECT source_language FROM object_info').fetchone()[0]=='c')
            check('pooled-constant-proof',db.execute('SELECT count(*) FROM record_constant_initializations').fetchone()[0]>0)
        run('verify-hidden',['dotnet',a.linker.resolve(),'verify',obj]);generated=out/'generated';run('link',['dotnet',a.linker.resolve(),'link','--entry','main','--out',generated,obj]);run('managed-build',['dotnet','build',generated/'Translated.csproj','-c','Release','-v:q']);run('managed-run',['dotnet',generated/'bin/Release/net10.0/Translated.dll'])
        if a.native_aot:
            run('aot-publish',['dotnet','publish',generated/'Translated.csproj','-c','Release','-r','linux-x64','--self-contained','true','-p:PublishAot=true','-o',out/'aot','-v:q']);run('aot-run',[out/'aot/Translated'])
        spec=importlib.util.spec_from_file_location('packager',a.packager.resolve());packager=importlib.util.module_from_spec(spec);spec.loader.exec_module(packager)
        for name,sql in [('missing-proof','DELETE FROM record_constant_initializations'),('tls-pooled-storage','UPDATE storages SET is_thread_local=1 WHERE id IN (SELECT storage_id FROM record_constant_initializations)')]:
            bad=out/(name+'.dotobj');shutil.copy2(obj,bad)
            with sqlite3.connect(bad) as db:db.execute(sql);db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
            run(name+'-reject',['dotnet',a.linker.resolve(),'verify',bad],False);check(name+'-diagnostic','Record constant initialization lacks' in (out/(name+'-reject.stderr')).read_text())
    except Exception as exception:error=str(exception)
    result=dict(status='failed' if error else 'passed',error=error,checks=checks,checkCount=len(checks),commands=commands);(out/'results.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps({k:result[k] for k in ('status','error','checkCount')}));return int(bool(error))

if __name__=='__main__':raise SystemExit(main())
