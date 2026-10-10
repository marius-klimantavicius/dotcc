#!/usr/bin/env python3
"""Verify distinct original C++ direct-argument and indirect-result transport policies."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--stock-compiler', type=Path)
    parser.add_argument('--stock-runtime', type=Path)
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    if bool(args.stock_compiler) != bool(args.stock_runtime):
        parser.error('--stock-compiler and --stock-runtime must be provided together')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    source = inputs / 'duckdb_record_argument_transport.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=300, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == (0 if success else 1))

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('build-native-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, '-o', binary])
            run('run-native-O' + optimization, [binary])
        if args.stock_compiler:
            import os
            env = dict(os.environ, EDG_BASE=str(args.packager.resolve().parent.parent / 'upstream/bases/docker/dev-env/gcc'))
            run('stock-c-lowering', [args.stock_compiler.resolve(), '--c++17', '--instantiate', 'used', '--no_inlining',
                '--exceptions', '--rtti', '--clear_flag', 'use_predefined_macro_file',
                '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source], env=env)
            for optimization in ('0', '2'):
                binary = out / ('stock-O' + optimization)
                run('build-stock-O' + optimization, ['gcc', '-w', '-O' + optimization, '-falign-functions=4', out / 'stock.c',
                    args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
                run('run-stock-O' + optimization, [binary])
        obj = out / 'argument-transport.dotobj'
        run('export-argument-transport', [sys.executable, '-B', tooling / 'compile.py', source,
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions', '--rtti'])
        run('verify-argument-transport', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            db.row_factory = sqlite3.Row
            check('foreign-key-integrity', not list(db.execute('PRAGMA foreign_key_check')))
            policy = db.execute("SELECT o.bitwise_construction_allowed,o.requires_return_storage,o.requires_parameter_storage FROM source_record_operations o JOIN source_record_types r ON r.id=o.source_record_id WHERE r.source_name='ArgumentOnlyTransport'").fetchone()
            check('argument-result-policies-differ', tuple(policy) == (1, 1, 0))
            check('direct-argument-source-slot', db.execute("SELECT COUNT(*) FROM entities e JOIN parameters p ON p.entity_id=e.id WHERE e.source_name='consume_argument' AND p.parameter_role='source'").fetchone()[0] == 1)
            check('result-buffer-source-slot', db.execute("SELECT COUNT(*) FROM entities e JOIN parameters p ON p.entity_id=e.id WHERE e.source_name='make_argument' AND p.parameter_role='result-buffer'").fetchone()[0] == 1)
        inputs.rename(out / 'retained-inputs')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        run('link-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'linked', obj])
        run('build-managed', ['dotnet', 'build', out / 'linked/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-native-aot', ['dotnet', 'publish', out / 'linked/Translated.csproj', '-c', 'Release', '-r', args.rid,
                '--self-contained', 'true', '-p:PublishAot=true', '-p:StripSymbols=false', '-o', out / 'aot', '-v:q'])
            run('run-native-aot', [out / 'aot/Translated'])

        def reject(name, mutate):
            damaged = out / (name + '.dotobj')
            shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                check(name + '-mutated', mutate(db).rowcount > 0)
                check(name + '-foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        reject('reject-indirect-parameter-policy', lambda db: db.execute("UPDATE source_record_operations SET requires_parameter_storage=1 WHERE source_record_id=(SELECT id FROM source_record_types WHERE source_name='ArgumentOnlyTransport')"))
    except Exception as exception:
        error = str(exception)
    result = dict(schema=1, status='failed' if error else 'passed', checks=checks, checkCount=len(checks), commands=commands,
        error=error, compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
        linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest(), executionVerified='run-managed' in checks,
        nativeAotVerified='run-native-aot' in checks, stockCVerified='run-stock-O2' in checks)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
