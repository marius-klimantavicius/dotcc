#!/usr/bin/env python3
"""Verify variadic functions preserve observed indirect result and fixed-parameter transport."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'variadic_record_transport.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_variadic_record_transport.cpp'), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)

    def reseal(path, mutate):
        shutil.copy2(obj, path)
        with sqlite3.connect(path) as db:
            mutate(db)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++17', '-O' + opt, source, '-o', binary])
            run('native-run-' + opt, [binary])
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
            '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source])
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, out / 'stock.c',
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary])
        obj = out / 'unit.dotobj'
        run('export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
            '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            make = db.execute("SELECT id,routine_type_id FROM entities WHERE source_name='make'").fetchone()
            use = db.execute("SELECT id,routine_type_id FROM entities WHERE source_name='use'").fetchone()
            check('result-buffer-variadic', db.execute("SELECT count(*) FROM source_routine_abis a JOIN source_routine_types r ON r.type_id=a.source_routine_type_id WHERE a.return_passing='result-buffer' AND r.variadic=1").fetchone()[0] > 0)
            check('fixed-indirect-variadic-parameter', db.execute("SELECT count(*) FROM source_routine_abi_parameters p JOIN source_routine_types r ON r.type_id=p.source_routine_type_id WHERE p.parameter_role='indirect-source' AND r.variadic=1").fetchone()[0] > 0)
            check('va-list-starts-retained', db.execute("SELECT count(*) FROM nodes WHERE kind='va-start'").fetchone()[0] == 2)
            check('indirect-variadic-call-retained', db.execute("SELECT count(*) FROM nodes WHERE kind='indirect-call'").fetchone()[0] > 0)
        providers = [item for p in args.provider for item in ('--provider', p.resolve())]
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', out / 'generated', obj, *providers])
        runtime = Path(__file__).resolve().parents[1] / 'runtime/bind.py'
        run('bind-runtime', ['python3', runtime, '--project', out / 'generated'])
        run('build-managed', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        mutations = [
            ('false-direct-result', "UPDATE source_routine_abis SET return_passing='direct' WHERE return_passing='result-buffer' AND source_routine_type_id IN (SELECT type_id FROM source_routine_types WHERE variadic=1)", (), 'Original routine ABI receiver/result/source mapping is incomplete'),
            ('missing-result-slot', "DELETE FROM source_routine_abi_parameters WHERE parameter_role='result-buffer' AND source_routine_type_id IN (SELECT type_id FROM source_routine_types WHERE variadic=1)", (), 'Invalid original routine ABI slot'),
            ('missing-indirect-source-slot', "DELETE FROM source_routine_abi_parameters WHERE parameter_role='indirect-source' AND source_routine_type_id IN (SELECT type_id FROM source_routine_types WHERE variadic=1)", (), 'Invalid original routine ABI slot'),
            ('false-physical-variadic-policy', "UPDATE routine_types SET variadic=0 WHERE type_id=?", (make[1],), 'Duplicate canonical execution type'),
        ]
        for name, statement, parameters, diagnostic in mutations:
            damaged = out / (name + '.dotobj')
            reseal(damaged, lambda db: db.execute(statement, parameters))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            check(name + '-exact-diagnostic', diagnostic in (out / ('reject-' + name + '.stderr')).read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
