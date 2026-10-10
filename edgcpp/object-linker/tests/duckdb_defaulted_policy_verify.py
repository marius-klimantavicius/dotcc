#!/usr/bin/env python3
"""Check consistent lazy defaulted exception policies across translation units."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--runtime-binder', type=Path, default=Path(__file__).resolve().parents[1] / 'runtime/bind.py')
    parser.add_argument('--oracle-only', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    sources = out / 'sources'; sources.mkdir()
    for name in ('common.h', 'first.cpp', 'second.cpp'):
        name = 'duckdb_defaulted_policy_' + name
        shutil.copy2(Path(__file__).with_name(name), sources / name)
    checks, commands = [], []
    def check(name, passed):
        if not passed: raise AssertionError(name)
        checks.append(name)
    def run(name, argv, env=None):
        argv = list(map(str, argv)); result = subprocess.run(argv, cwd=out, capture_output=True, timeout=300, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout); (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=argv, exitCode=result.returncode))
        check(name, result.returncode == 0)
    error = None
    try:
        cpp = [sources / ('duckdb_defaulted_policy_' + name + '.cpp') for name in ('first', 'second')]
        for opt in ('0', '2'):
            binary = out / ('native-O' + opt)
            run('native-build-O' + opt, ['c++', '-std=c++17', '-O' + opt, *cpp, '-o', binary])
            run('native-run-O' + opt, [binary])
        cfiles = []
        for index, source in enumerate(cpp):
            cfile = out / ('stock' + str(index) + '.c'); cfiles.append(cfile)
            run('stock-lower-' + str(index), [args.stock_compiler.resolve(), '--c++17', '--instantiate', 'used', '--no_inlining',
                '--exceptions', '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', cfile,
                '-o', out / ('stock' + str(index) + '.il'), source],
                env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ['PATH']))
        for opt in ('0', '2'):
            binary = out / ('stock-O' + opt)
            run('stock-build-O' + opt, ['gcc', '-w', '-O' + opt, '-falign-functions=4', *cfiles,
                args.stock_runtime.resolve(), '-lstdc++', '-lm', '-lpthread', '-o', binary])
            run('stock-run-O' + opt, [binary])
        if not args.oracle_only:
            objects = []
            for index, source in enumerate(cpp):
                obj = out / ('unit' + str(index) + '.dotobj'); objects.append(obj)
                run('export-' + str(index), [sys.executable, '-B', args.packager.resolve(), source, '--output', obj,
                    '--work-dir', out / ('work' + str(index)), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
                run('verify-' + str(index), ['dotnet', args.linker.resolve(), 'verify', obj])
                with sqlite3.connect(obj) as db:
                    for name, policy in (('PolicyDerived', 'terminate'), ('PolicyMayThrow', 'unwind'), ('PolicyExplicit', 'unwind')):
                        policies = {row[0] for row in db.execute('SELECT t.exception_policy FROM entities e JOIN source_functions f ON f.entity_id=e.id JOIN source_routine_types t ON t.type_id=f.routine_source_type_id WHERE e.source_name=?', (name,))}
                        check(str(index) + '-selected-' + name, policies == {policy})
                    if index == 0:
                        check('absent-to-noexcept-proof', db.execute("SELECT count(*) FROM source_routine_exception_resolutions WHERE initial_present=0 AND is_defaulted=1 AND resolved_policy='terminate'").fetchone()[0] > 0)
                        check('throwing-defaulted-proof', db.execute("SELECT count(*) FROM source_routine_exception_resolutions WHERE initial_present=0 AND is_defaulted=1 AND resolved_policy='unwind'").fetchone()[0] > 0)
            sources.rename(out / 'retained-sources')
            generated = out / 'generated'
            run('link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', generated,
                *objects, *[v for provider in args.provider for v in ('--provider', provider.resolve())]])
            run('bind-runtime', [sys.executable, '-B', args.runtime_binder.resolve(), '--project', generated])
            bindings = json.loads((generated / 'runtime-bindings.json').read_text())
            check('runtime-fully-bound', bindings['status'] == 'bound' and not bindings['unresolved'])
            run('build-managed', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '-v:q'])
            run('run-managed', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
            if args.native_aot:
                aot = out / 'aot'
                run('publish-aot', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', aot, '-v:q'])
                run('run-aot', [aot / 'Translated'])
    except Exception as exc:
        error = str(exc)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(), linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], error=error, checkCount=len(checks))))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
