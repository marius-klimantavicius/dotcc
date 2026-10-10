#!/usr/bin/env python3
"""Verify exact observed lowered record copies without granting C++ copy operations."""
import argparse
import hashlib
import importlib.util
import shutil
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--oracle-only', action='store_true')
    parser.add_argument('--require-kind', action='append', default=[], choices=('initializer-storage','bitwise-initialization'))
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--runtime-binder', type=Path, default=Path(__file__).resolve().parents[1] / 'runtime/bind.py')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'duckdb_record_storage_copy.cpp'
    source.write_bytes(Path(__file__).with_name(source.name).read_bytes())
    checks, commands = [], []
    def check(name, passed):
        if not passed: raise AssertionError(name)
        checks.append(name)
    def run(name, argv, env=None):
        argv = list(map(str, argv)); result = subprocess.run(argv, cwd=out, capture_output=True, timeout=300, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=argv, exitCode=result.returncode))
        check(name, result.returncode == 0)
    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-O' + opt)
            run('native-build-O' + opt, ['c++', '-std=c++17', '-O' + opt, source, '-o', binary])
            run('native-run-O' + opt, [binary])
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used', '--no_inlining',
            '--exceptions', '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / 'stock.c',
            '-o', out / 'stock.il', source], env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ['PATH']))
        for opt in ('0', '2'):
            binary = out / ('stock-O' + opt)
            run('stock-build-O' + opt, ['gcc', '-w', '-O' + opt, '-falign-functions=4', out / 'stock.c', args.stock_runtime.resolve(), '-lstdc++', '-lm', '-lpthread', '-o', binary])
            run('stock-run-O' + opt, [binary])
        if not args.oracle_only:
            obj = out / 'unit.dotobj'
            run('export', [sys.executable, '-B', args.packager.resolve(), source, '--output', obj,
                '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
            run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                kinds={row[0] for row in db.execute('SELECT kind FROM record_storage_copies')}
                check('both-stock-copy-observations', {'evaluation-save','class-assignment'} <= kinds)
                for kind in args.require_kind:
                    check('observed-'+kind, kind in kinds)
                factory_rows=list(db.execute("SELECT node_id,target_node_id,value_node_id FROM record_storage_copies WHERE kind IN ('initializer-storage','bitwise-initialization')"))
                row=db.execute("SELECT c.node_id,c.target_node_id,c.value_node_id,c.source_record_id FROM record_storage_copies c WHERE kind='evaluation-save' LIMIT 1").fetchone()
                selected,target,value,source_record=row
                ordinary=db.execute('SELECT bitwise_construction_allowed,bitwise_assignment_allowed FROM source_record_operations WHERE source_record_id=?',(source_record,)).fetchone()
                check('original-copy-permissions-retained', ordinary[0]==0 and db.execute('SELECT is_trivially_copyable FROM source_record_types WHERE id=?',(source_record,)).fetchone()[0]==0)
                check('exact-full-storage-extents', db.execute('SELECT count(*) FROM record_storage_copies c JOIN source_record_types s ON s.id=c.source_record_id WHERE c.byte_count<>s.native_size').fetchone()[0]==0)
                check('no-bulk-copy-proof-on-memcpy', db.execute("SELECT count(*) FROM record_storage_copies c JOIN nodes n ON n.id=c.node_id WHERE n.kind<>'record-transfer'").fetchone()[0]==0)
                check('tail-padding-keeps-stock-memcpy', db.execute("SELECT count(*) FROM nodes n JOIN entities e ON e.id=n.entity_id WHERE n.kind='call' AND e.source_name='memcpy'").fetchone()[0]>0)
                other_source=db.execute("SELECT id FROM source_record_types WHERE source_name='TailBase'").fetchone()[0]
            tooling = out / 'tooling'; tooling.mkdir()
            for name in ('compile.py','schema.sql'): shutil.copy2(args.packager.resolve().parent / name, tooling / name)
            spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
            packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
            mutations = {
                'missing-proof': ('DELETE FROM record_storage_copies WHERE node_id=?',(selected,)),
                'wrong-target': ('UPDATE record_storage_copies SET target_node_id=? WHERE node_id=?',(value,selected)),
                'wrong-value': ('UPDATE record_storage_copies SET value_node_id=? WHERE node_id=?',(target,selected)),
                'wrong-source-record': ('UPDATE record_storage_copies SET source_record_id=? WHERE node_id=?',(other_source,selected)),
                'wrong-extent': ('UPDATE record_storage_copies SET byte_count=byte_count+1 WHERE node_id=?',(selected,)),
                'wrong-capture-proof': ("UPDATE record_storage_copies SET capture_proof='unobserved-copy' WHERE node_id=?",(selected,)),
                'rewrite-as-ordinary-assignment': ("UPDATE nodes SET kind='assign' WHERE id=?",(selected,)),
            }
            for factory_id,factory_target,factory_value in factory_rows:
                for suffix,sql,values in (
                    ('missing', 'DELETE FROM record_storage_copies WHERE node_id=?', (factory_id,)),
                    ('target', 'UPDATE record_storage_copies SET target_node_id=? WHERE node_id=?', (factory_value,factory_id)),
                    ('value', 'UPDATE record_storage_copies SET value_node_id=? WHERE node_id=?', (factory_target,factory_id)),
                    ('extent', 'UPDATE record_storage_copies SET byte_count=byte_count-1 WHERE node_id=?', (factory_id,)),
                    ('proof', "UPDATE record_storage_copies SET capture_proof='unobserved-copy' WHERE node_id=?", (factory_id,)),
                ):
                    mutations['factory-'+str(factory_id)+'-'+suffix]=(sql,values)
            for name,(sql,values) in mutations.items():
                bad=out/(name+'.dotobj'); shutil.copy2(obj,bad)
                with sqlite3.connect(bad) as db:
                    db.execute('PRAGMA foreign_keys=OFF');db.execute('PRAGMA ignore_check_constraints=ON');db.execute(sql,values)
                    db.execute('UPDATE content_digest SET sha256=?',(packager.logical_hash(db),))
                argv=['dotnet',str(args.linker.resolve()),'verify',str(bad)]
                result=subprocess.run(argv,cwd=out,capture_output=True,timeout=300)
                (out/(name+'.stderr')).write_bytes(result.stderr);(out/(name+'.stdout')).write_bytes(result.stdout)
                commands.append(dict(name='reject-'+name,command=argv,exitCode=result.returncode))
                check('reject-'+name,result.returncode!=0)
            source.rename(source.with_suffix('.retained.cpp'))
            generated=out/'linked'
            run('link-hidden-source',['dotnet',args.linker.resolve(),'link','--entry','main','--allow-externs','--out',generated,obj,
                *[argument for provider in args.provider for argument in ('--provider',provider.resolve())]])
            run('bind-shared-runtime',[sys.executable,'-B',args.runtime_binder.resolve(),'--project',generated])
            receipt=json.loads((generated/'runtime-bindings.json').read_text())
            check('complete-shared-runtime-binding',receipt['status']=='bound' and not receipt['unresolved'])
            run('build-managed',['dotnet','build',generated/'Translated.csproj','-c','Release','-v:q'])
            run('run-managed',['dotnet',generated/'bin/Release/net10.0/Translated.dll'])
            if args.native_aot:
                aot=out/'aot'
                run('publish-aot',['dotnet','publish',generated/'Translated.csproj','-c','Release','-r','linux-x64','--self-contained','true','-p:PublishAot=true','-o',aot,'-v:q'])
                run('run-aot',[aot/'Translated'])
    except Exception as exc:
        error = str(exc)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(), linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], error=error, checkCount=len(checks))))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
