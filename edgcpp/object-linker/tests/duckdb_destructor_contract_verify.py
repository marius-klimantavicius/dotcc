#!/usr/bin/env python3
"""Keep complete-destructor callable noexcept separate from lowered body policy."""
import argparse
import importlib.util
import json
from pathlib import Path
import resource
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
    inputs = out / 'inputs'; inputs.mkdir()
    for suffix in ('h', 'import.cpp', 'provider.cpp'):
        name = 'duckdb_destructor_contract' + ('.' if suffix == 'h' else '_') + suffix
        shutil.copy2(Path(__file__).with_name(name), inputs / name)
    sources = [inputs / ('duckdb_destructor_contract_' + role + '.cpp') for role in ('import', 'provider')]
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True, terminates=False):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        if terminates:
            check(name, result.returncode < 0)
        else:
            check(name, (result.returncode == 0) == success)

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++17', '-O' + opt, *sources, '-o', binary])
            run('native-run-' + opt, [binary])
            run('native-noexcept-' + opt, [binary, 'throw'], terminates=True)
        lowered = []
        for role, source in zip(('import', 'provider'), sources):
            target = out / (role + '.c'); lowered.append(target)
            run('stock-lower-' + role, [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
                '--gen_c_file_name', target, '-o', out / (role + '.il'), source])
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, *lowered,
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary])
            run('stock-noexcept-' + opt, [binary, 'throw'], terminates=True)
        objects = []
        for role, source in zip(('import', 'provider'), sources):
            obj = out / (role + '.dotobj'); objects.append(obj)
            run('export-' + role, ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / (role + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        inputs.rename(out / 'retained-inputs')
        records = []
        for role, obj in zip(('import', 'provider'), objects):
            run('verify-hidden-' + role, ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                rows = db.execute("SELECT e.id,e.routine_type_id,p.routine_type_id,f.routine_source_type_id,r.exception_policy,s.exception_policy,e.external_key FROM abi_routine_origins a JOIN entities e ON e.id=a.entity_id JOIN entities p ON p.id=a.primary_entity_id JOIN source_functions f ON f.entity_id=p.id JOIN routine_types r ON r.type_id=e.routine_type_id JOIN source_routine_types s ON s.type_id=f.routine_source_type_id WHERE a.entry_kind='complete' AND f.function_kind='destructor'").fetchall()
                check(role + '-one-owned-complete-destructor', len(rows) == 1)
                records.append(rows[0])
                check(role + '-original-noexcept', rows[0][5] == 'terminate')
                check(role + '-stock-body-policy-preserved', rows[0][4] == ('unwind' if role == 'import' else 'terminate'))
        check('matching-source-destructor-linkage', records[0][6] == records[1][6])
        providers = [item for p in args.provider for item in ('--provider', p.resolve())]
        for label, values in [('linked', objects), ('reversed', list(reversed(objects)))]:
            run('link-' + label, ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', out / label, *values, *providers])
        runtime = Path(__file__).resolve().parents[1] / 'runtime/bind.py'
        run('bind-runtime', ['python3', runtime, '--project', out / 'linked'])
        run('build-managed', ['dotnet', 'build', out / 'linked/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll'])
        run('managed-noexcept', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll', 'throw'], terminates=True)
        check('managed-terminates-at-noexcept-boundary', 'Exception escaped noexcept' in (out / 'managed-noexcept.stderr').read_text())
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'linked/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
            run('aot-noexcept', [out / 'aot/Translated', 'throw'], terminates=True)
            check('aot-terminates-at-noexcept-boundary', 'Exception escaped noexcept' in (out / 'aot-noexcept.stderr').read_text())
        run('link-unresolved-destructor', ['dotnet', args.linker.resolve(), 'link', '--allow-externs', '--out', out / 'unresolved', objects[0], *providers])
        report = json.loads((out / 'unresolved/link-report.json').read_text())
        requirement = next(item for item in report['runtimeRequirements'] if item['ExternalKey'] == records[0][6])
        check('extern-callable-contract-enforced', requirement['exceptionPolicy'] == 'terminate' and requirement['executionExceptionPolicy'] == 'unwind' and '__dot_noexcept_' in requirement['callReference'])
        # The primary remains internally self-consistent, but its new throwing
        # contract conflicts with the retained complete entry's terminate body.
        damaged = out / 'conflicting-boundary.dotobj'; shutil.copy2(objects[1], damaged)
        with sqlite3.connect(damaged) as db:
            row = records[1]
            db.execute("UPDATE source_routine_types SET exception_policy='unwind' WHERE type_id=?", (row[3],))
            primary = db.execute('SELECT primary_entity_id FROM abi_routine_origins WHERE entity_id=?', (row[0],)).fetchone()[0]
            result, variadic = db.execute('SELECT return_type_id,variadic FROM routine_types WHERE type_id=?', (row[2],)).fetchone()
            parameters = db.execute('SELECT ordinal,parameter_type_id FROM routine_parameters WHERE type_id=? ORDER BY ordinal', (row[2],)).fetchall()
            candidates = [item[0] for item in db.execute("SELECT type_id FROM routine_types WHERE return_type_id=? AND variadic=? AND exception_policy='unwind'", (result, variadic))
                if db.execute('SELECT ordinal,parameter_type_id FROM routine_parameters WHERE type_id=? ORDER BY ordinal', (item[0],)).fetchall() == parameters]
            check('existing-unwind-primary-abi-shape', len(candidates) == 1 and candidates[0] != row[1])
            # D9 already supplies this canonical execution type. Reuse it;
            # changing a shared C2/D2 type would create a duplicate and also
            # corrupt the unrelated constructor's original contract.
            db.execute('UPDATE entities SET routine_type_id=? WHERE id=?', (candidates[0], primary))
            db.execute('UPDATE source_types SET execution_type_id=? WHERE id=?', (candidates[0], row[3]))
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
        run('reject-conflicting-boundary', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        check('conflicting-boundary-diagnostic', 'Complete destructor execution boundary conflicts' in (out / 'reject-conflicting-boundary.stderr').read_text())
        # A deleting entry is a different callable contract: its deallocation
        # work must not inherit D1 normalization. Preserve the otherwise valid
        # destructor-origin shape, then require cross-unit linking to reject it.
        deleting = out / 'deleting-entry-not-normalized.dotobj'; shutil.copy2(objects[0], deleting)
        with sqlite3.connect(deleting) as db:
            db.execute("UPDATE abi_routine_origins SET entry_kind='deleting' WHERE entity_id=?", (records[0][0],))
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
        run('verify-deleting-entry-shape', ['dotnet', args.linker.resolve(), 'verify', deleting])
        run('reject-deleting-entry-normalization', ['dotnet', args.linker.resolve(), 'link', '--allow-externs', '--out', out / 'deleting-link', deleting, objects[1], *providers], False)
        check('deleting-entry-still-compares-raw-policy', 'Signature mismatch' in (out / 'reject-deleting-entry-normalization.stderr').read_text())
        for label, statement in [('wrong-entry-kind', "UPDATE abi_routine_origins SET entry_kind='constructor-default-adapter',capture_proof='observed-default-wrapper-creation',target_entity_id=primary_entity_id,is_alias_entry=0 WHERE entity_id=?"), ('missing-origin', 'DELETE FROM abi_routine_origins WHERE entity_id=?')]:
            damaged = out / (label + '.dotobj'); shutil.copy2(objects[0], damaged)
            with sqlite3.connect(damaged) as db:
                check(label + '-mutation', db.execute(statement, (records[0][0],)).rowcount == 1)
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
