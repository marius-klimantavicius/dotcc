#!/usr/bin/env python3
"""Verify incomplete callback ABI storage and cross-TU completion without source input."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import re
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
        parser.error('Both stock compiler and runtime are required')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    stem = 'duckdb_deferred_routine_abi'
    for suffix in ('.h', '_first.cpp', '_second.cpp'):
        shutil.copy2(Path(__file__).with_name(stem + suffix), inputs / (stem + suffix))
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

    def run(name, command, success=True):
        command = list(map(str, command))
        env = dict(os.environ, EDG_BASE=str(args.packager.resolve().parent.parent / 'upstream/bases/docker/dev-env/gcc'))
        result = subprocess.run(command, cwd=out, env=env, capture_output=True, timeout=300)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == (0 if success else 1))

    error = None
    try:
        sources = [inputs / (stem + '_' + unit + '.cpp') for unit in ('first', 'second')]
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('build-native-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, *sources, '-o', binary])
            run('run-native-O' + optimization, [binary])
        if args.stock_compiler:
            lowered = []
            for unit, source in zip(('first', 'second'), sources):
                target = out / (unit + '.c')
                lowered.append(target)
                run('stock-lower-' + unit, [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
                    '--no_inlining', '--exceptions', '--rtti', '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', target, source])
            for optimization in ('0', '2'):
                binary = out / ('stock-O' + optimization)
                run('build-stock-O' + optimization, ['gcc', '-w', '-falign-functions=4', '-O' + optimization,
                    *lowered, args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
                run('run-stock-O' + optimization, [binary])
        objects = []
        for unit, source in zip(('first', 'second'), sources):
            obj = out / (unit + '.dotobj')
            objects.append(obj)
            run('export-' + unit, [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / ('work-' + unit), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions', '--rtti'])
            run('verify-' + unit, ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(objects[0]) as db:
            check('first-has-owned-deferred-callback', db.execute('SELECT COUNT(*) FROM deferred_routine_abis').fetchone()[0] > 0)
            check('original-incomplete-result-retained', db.execute("SELECT COUNT(*) FROM source_record_types WHERE source_name='DeferredResult' AND is_complete=0").fetchone()[0] == 1)
        with sqlite3.connect(objects[1]) as db:
            check('second-has-complete-callback-abi', db.execute('SELECT COUNT(*) FROM deferred_routine_abis').fetchone()[0] == 0)
            check('complete-result-uses-buffer', db.execute("SELECT COUNT(*) FROM source_functions f JOIN entities e ON e.id=f.entity_id JOIN source_routine_abis a ON a.source_routine_type_id=f.routine_source_type_id WHERE e.source_name='make_result' AND a.return_passing='result-buffer'").fetchone()[0] == 1)
        inputs.rename(out / 'retained-inputs')
        run('link-unresolved-storage-only', ['dotnet', args.linker.resolve(), 'link', '--out', out / 'unresolved', objects[0]])
        run('build-unresolved-storage-only', ['dotnet', 'build', out / 'unresolved/Translated.csproj', '-c', 'Release', '-v:q'])
        text = '\n'.join(path.read_text() for path in (out / 'unresolved').glob('*.cs'))
        check('unresolved-callback-emitted-as-address-storage', bool(re.search(r'void\*\s+@?make\s*;', text)))
        run('link-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'linked', *objects])
        run('build-managed', ['dotnet', 'build', out / 'linked/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll'])
        run('link-reversed-inputs', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'reversed', *reversed(objects)])
        if args.native_aot:
            run('publish-native-aot', ['dotnet', 'publish', out / 'linked/Translated.csproj', '-c', 'Release', '-r', args.rid,
                '--self-contained', 'true', '-p:PublishAot=true', '-p:StripSymbols=false', '-o', out / 'aot', '-v:q'])
            run('run-native-aot', [out / 'aot/Translated'])
        damaged = out / 'missing-deferred-proof.dotobj'
        shutil.copy2(objects[0], damaged)
        with sqlite3.connect(damaged) as db:
            check('removed-deferred-proof', db.execute('DELETE FROM deferred_routine_abis').rowcount > 0)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
        run('reject-missing-deferred-proof', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        executionVerified='run-managed' in checks, nativeAotVerified='run-native-aot' in checks, stockCVerified='run-stock-O2' in checks)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
