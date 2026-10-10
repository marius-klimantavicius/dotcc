#!/usr/bin/env python3
"""Check typed polymorphic floating builtins and ordinary variadic promotions."""
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
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'duckdb_floating_classification.cpp'
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
                operations = {row[0] for row in db.execute("SELECT operator FROM nodes WHERE kind='intrinsic'")}
                expected = {'__builtin_' + name for name in ('signbit','signbitf','signbitl','isnan','isnanf','isnanl','isinf','isinff','isinfl','isfinite')}
                check('exact-builtin-identities', expected <= operations)
                check('no-classification-externs', db.execute("SELECT count(*) FROM entities WHERE source_name IN (" + ','.join('?' for _ in expected) + ")", tuple(expected)).fetchone()[0] == 0)
                widths = {row[0] for row in db.execute("SELECT t.kind FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='argument' JOIN nodes a ON a.id=e.child_id JOIN types t ON t.id=a.type_id WHERE n.kind='intrinsic' AND n.operator='__builtin_signbit'")}
                check('actual-unpromoted-widths', widths == {'f32','f64','f80'})
                check('builtin-locations', db.execute("SELECT count(*) FROM nodes WHERE kind='intrinsic' AND operator LIKE '__builtin_%' AND (source_file_id IS NULL OR line=0)").fetchone()[0] == 0)
                fixed = db.execute("SELECT n.id FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='argument' JOIN nodes a ON a.id=e.child_id JOIN types t ON t.id=a.type_id WHERE n.kind='intrinsic' AND n.operator='__builtin_signbit' AND t.kind='f64' LIMIT 1").fetchone()[0]
                wide = db.execute("SELECT id FROM types WHERE kind='u64' LIMIT 1").fetchone()[0]
                promotion = db.execute("SELECT a.id FROM nodes n JOIN entities f ON f.id=n.entity_id JOIN edges e ON e.parent_id=n.id AND e.role='argument' AND e.ordinal=1 JOIN nodes a ON a.id=e.child_id WHERE n.kind='call' AND f.source_name='ordinary_variadic'").fetchone()[0]
                f32 = db.execute("SELECT id FROM types WHERE kind='f32' LIMIT 1").fetchone()[0]
                check('ordinary-call-promotes', db.execute("SELECT t.kind FROM nodes n JOIN types t ON t.id=n.type_id WHERE n.id=?", (promotion,)).fetchone()[0] == 'f64')
            tooling = out / 'tooling'; tooling.mkdir()
            for name in ('compile.py','schema.sql'): shutil.copy2(args.packager.resolve().parent / name, tooling / name)
            spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
            packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
            mutations = {
                'wrong-fixed-width': ("UPDATE nodes SET operator='__builtin_signbitf' WHERE id=?", (fixed,)),
                'wrong-result': ('UPDATE nodes SET type_id=? WHERE id=?', (wide,fixed)),
                'unknown-builtin': ("UPDATE nodes SET operator='__builtin_signbit_unknown' WHERE id=?", (fixed,)),
                'ordinary-float-without-promotion': ('UPDATE nodes SET type_id=? WHERE id=?', (f32,promotion)),
            }
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
                if name=='ordinary-float-without-promotion':
                    check('ordinary-promotion-rule-unchanged',b'default variadic promotion mismatch' in result.stderr)
            source.rename(source.with_suffix('.retained.cpp'))
            generated=out/'linked'
            run('link-hidden-source',['dotnet',args.linker.resolve(),'link','--entry','main','--out',generated,obj])
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
