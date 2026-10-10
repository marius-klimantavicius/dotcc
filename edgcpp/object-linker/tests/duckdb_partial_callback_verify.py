#!/usr/bin/env python3
"""Verify same-semantic partial callback ABIs use opaque storage until completed."""
import argparse
import importlib.util
import json
from pathlib import Path
import re
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--mode', choices=('storage', 'complete'), required=True)
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    roles = ['first', 'second', 'entry' if args.mode == 'storage' else 'complete']
    sources = []
    for role in roles:
        source = inputs / (role + '.cpp'); sources.append(source)
        shutil.copy2(Path(__file__).with_name('duckdb_partial_callback_' + role + '.cpp'), source)
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
        with sqlite3.connect(path) as db:
            mutate(db)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++17', '-O' + opt, *sources, '-o', binary])
            run('native-run-' + opt, [binary])
        lowered = []
        for role, source in zip(roles, sources):
            target = out / (role + '.c'); lowered.append(target)
            run('stock-lower-' + role, [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
                '--gen_c_file_name', target, '-o', out / (role + '.il'), source])
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, *lowered,
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary])
        objects = []
        for role, source in zip(roles, sources):
            obj = out / (role + '.dotobj'); objects.append(obj)
            run('export-' + role, ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / (role + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        inputs.rename(out / 'retained-inputs')
        for role, obj in zip(roles, objects): run('verify-hidden-' + role, ['dotnet', args.linker.resolve(), 'verify', obj])
        for role, expected in [('first', 'result-buffer'), ('second', 'direct')]:
            with sqlite3.connect(out / (role + '.dotobj')) as db:
                check(role + '-retains-original-partial-abi', db.execute("SELECT a.return_passing FROM deferred_routine_abis d JOIN source_routine_abis a ON a.source_routine_type_id=d.source_routine_type_id JOIN types t ON t.pointee_type_id=d.routine_type_id JOIN record_fields f ON f.type_id=t.id WHERE f.source_name='callback'").fetchall() == [(expected,)])
                check(role + '-shared-raw-key-retains-distinct-original-families', db.execute('SELECT max(n) FROM (SELECT count(*) n FROM deferred_routine_abis GROUP BY routine_type_id)').fetchone()[0] == 2)
                check(role + '-has-no-callable-import', db.execute('SELECT count(*) FROM deferred_function_declarations').fetchone()[0] == 0)
        for label, values in [('linked', objects), ('reversed', list(reversed(objects)))]:
            run('link-' + label, ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / label, *values])
        generated = '\n'.join(p.read_text() for p in (out / 'linked').glob('*.cs'))
        if args.mode == 'storage':
            check('unresolved-callback-is-opaque-storage', bool(re.search(r'void\*\s+@?callback\s*;', generated)))
        else:
            check('completed-callback-has-typed-managed-pointer', bool(re.search(r'delegate\* managed<[^;]+>\s+@?callback\s*;', generated)))
        run('build-managed', ['dotnet', 'build', out / 'linked/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'linked/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        changed = out / 'changed-original-parameter.dotobj'; shutil.copy2(objects[1], changed)
        def different_parameter(db):
            execution, source = db.execute("SELECT d.routine_type_id,d.source_routine_type_id FROM deferred_routine_abis d JOIN types t ON t.pointee_type_id=d.routine_type_id JOIN record_fields f ON f.type_id=t.id WHERE f.source_name='callback'").fetchone()
            original_result = db.execute('SELECT return_type_id FROM source_routine_types WHERE type_id=?', (source,)).fetchone()[0]
            execution_result = db.execute('SELECT return_type_id FROM routine_types WHERE type_id=?', (execution,)).fetchone()[0]
            check('mutate-original-parameter', db.execute('UPDATE source_routine_parameters SET parameter_type_id=? WHERE type_id=? AND ordinal=0', (original_result, source)).rowcount == 1)
            check('mutate-matching-provisional-parameter', db.execute('UPDATE routine_parameters SET parameter_type_id=? WHERE type_id=? AND ordinal=0', (execution_result, execution)).rowcount == 1)
        reseal(changed, different_parameter)
        run('verify-different-original-parameter', ['dotnet', args.linker.resolve(), 'verify', changed])
        run('reject-different-original-parameter', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'different-linked', objects[0], changed, objects[2]], False)
        damaged = out / 'unresolved-indirect-call.dotobj'; shutil.copy2(objects[1], damaged)
        def indirect_call(db):
            node = db.execute("SELECT n.id FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='left' JOIN nodes m ON m.id=e.child_id WHERE n.kind='binary' AND n.operator='eq' AND m.kind='member'").fetchone()[0]
            db.execute("UPDATE nodes SET kind='indirect-call',operator=NULL,evaluation_order='left-to-right' WHERE id=?", (node,))
            db.execute("UPDATE edges SET role=CASE role WHEN 'left' THEN 'operand' ELSE 'argument' END WHERE parent_id=?", (node,))
        reseal(damaged, indirect_call)
        run('reject-unresolved-indirect-call', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        check('unresolved-call-diagnostic', 'Indirect call requires a completed original routine ABI' in (out / 'reject-unresolved-indirect-call.stderr').read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
