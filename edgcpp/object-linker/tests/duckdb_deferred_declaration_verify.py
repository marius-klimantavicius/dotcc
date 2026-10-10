#!/usr/bin/env python3
"""Verify incomplete imported function declarations resolve to exact completed ABIs."""
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
    parser.add_argument('--case', choices=('parameter', 'return'), required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    sources = []
    for role in ('import', 'provider'):
        source = inputs / (role + '.cpp'); sources.append(source)
        shutil.copy2(Path(__file__).with_name('duckdb_deferred_declaration_' + args.case + '_' + role + '.cpp'), source)
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

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++17', '-O' + opt, *sources, '-o', binary])
            run('native-run-' + opt, [binary])
        lowered = []
        for role, source in zip(('import', 'provider'), sources):
            target = out / (role + '.c'); lowered.append(target)
            run('stock-lower-' + role, [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions', '--rtti',
                '--gen_c_file_name', target, '-o', out / (role + '.il'), source])
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, *lowered,
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary])
        objects = []
        for role, source in zip(('import', 'provider'), sources):
            obj = out / (role + '.dotobj'); objects.append(obj)
            run('export-' + role, ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / (role + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions', '--rtti'])
        inputs.rename(out / 'retained-inputs')
        for role, obj in zip(('import', 'provider'), objects):
            run('verify-hidden-' + role, ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(objects[0]) as db:
            rows = db.execute('SELECT d.entity_id,d.source_routine_type_id,e.routine_type_id FROM deferred_function_declarations d JOIN entities e ON e.id=d.entity_id').fetchall()
            check('owned-imported-declaration', len(rows) == 1)
            entity, source_routine, execution = rows[0]
            check('original-deferred-abi-retained', db.execute('SELECT count(*) FROM deferred_routine_abis WHERE routine_type_id=? AND source_routine_type_id=?', (execution, source_routine)).fetchone()[0] == 1)
            check('no-fabricated-body', db.execute('SELECT count(*) FROM functions WHERE entity_id=?', (entity,)).fetchone()[0] == 0)
            check('no-direct-call', db.execute("SELECT count(*) FROM nodes WHERE entity_id=? AND kind<>'function-address'", (entity,)).fetchone()[0] == 0)
            external_key = db.execute('SELECT external_key FROM entities WHERE id=?', (entity,)).fetchone()[0]
        with sqlite3.connect(objects[1]) as db:
            check('provider-has-completed-entity', db.execute('SELECT count(*) FROM entities e LEFT JOIN deferred_function_declarations d ON d.entity_id=e.id WHERE e.external_key=? AND e.definition<>\'declaration\' AND d.entity_id IS NULL', (external_key,)).fetchone()[0] == 1)
            if args.case == 'return':
                for key, label in [('_ZTV4Base', 'vtable'), ('_ZTI4Base', 'typeinfo'), ('_ZTS4Base', 'type-name')]:
                    check('key-function-tu-retains-' + label, db.execute("SELECT count(*) FROM storages WHERE external_key=? AND definition<>'declaration'", (key,)).fetchone()[0] == 1)
                check('provider-observed-result-buffer', db.execute("SELECT count(*) FROM entities e JOIN source_functions f ON f.entity_id=e.id JOIN source_routine_abis a ON a.source_routine_type_id=f.routine_source_type_id WHERE e.external_key=? AND a.return_passing='result-buffer'", (external_key,)).fetchone()[0] == 1)
        providers = [item for p in args.provider for item in ('--provider', p.resolve())]
        run('reject-uncompleted-import', ['dotnet', args.linker.resolve(), 'link', '--allow-externs', '--out', out / 'unresolved', objects[0], *providers], False)
        check('uncompleted-import-diagnostic', 'Deferred function declaration requires a completed ABI provider' in (out / 'reject-uncompleted-import.stderr').read_text())
        for label, values in [('linked', objects), ('reversed', list(reversed(objects)))]:
            run('link-' + label, ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', out / label, *values, *providers])
        runtime = Path(__file__).resolve().parents[1] / 'runtime/bind.py'
        run('bind-runtime', ['python3', runtime, '--project', out / 'linked'])
        run('build-managed', ['dotnet', 'build', out / 'linked/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'linked/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        mutations = {
            'missing-import-proof': ('DELETE FROM deferred_function_declarations', ()),
            'missing-deferred-abi': ('DELETE FROM deferred_routine_abis WHERE routine_type_id=?', (execution,)),
            'wrong-original-signature': ('UPDATE deferred_function_declarations SET source_routine_type_id=(SELECT routine_source_type_id FROM source_functions WHERE entity_id<>? LIMIT 1)', (entity,)),
            'fabricated-definition': ("UPDATE entities SET definition='strong' WHERE id=?", (entity,)),
        }
        if args.case == 'parameter':
            mutations['direct-call-with-deferred-abi'] = ("UPDATE nodes SET kind='call' WHERE kind='function-address' AND entity_id=?", (entity,))
        for label, (sql, parameters) in mutations.items():
            damaged = out / (label + '.dotobj'); shutil.copy2(objects[0], damaged)
            with sqlite3.connect(damaged) as db:
                check(label + '-mutation', db.execute(sql, parameters).rowcount > 0)
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run('reject-' + label, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
