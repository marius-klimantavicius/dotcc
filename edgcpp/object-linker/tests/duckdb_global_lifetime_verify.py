#!/usr/bin/env python3
"""Verify owned static destruction, startup temporaries and shared finalizer ordering."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

CASES = {'scalar': b'120657', 'array': b'12065', 'temporary': b'13204',
         'interleave': b'CMrnldb', 'partial_array': b'12Bc345MFED', 'no_effect': b'CM'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'packager', 'linker', 'stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands, objects = [], [], {}
    bind = Path(__file__).resolve().parents[1] / 'runtime/bind.py'

    def check(name, condition):
        if not condition: raise AssertionError(name)
        checks.append(name)

    def run(name, command, expected=None, succeeds=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=900)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0 if succeeds else result.returncode != 0)
        if expected is not None: check(name + '-output', result.stdout == expected)
        return result

    def link(name, obj, executable):
        generated = out / name
        run(name + '-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Tests.StaticLifetime',
            '--class', 'Core', '--allow-externs', *(['--entry', 'main'] if executable else []), obj,
            *[argument for provider in args.provider for argument in ('--provider', provider.resolve())]])
        run(name + '-bind', [sys.executable, '-B', bind, '--project', generated])
        receipt = json.loads((generated / 'runtime-bindings.json').read_text())
        check(name + '-complete-shared-runtime-binding', receipt['status'] == 'bound' and not receipt['unresolved'])
        return generated

    def execute(name, project, expected):
        run(name + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        run(name + '-jit', ['dotnet', project.parent / 'bin/Release/net10.0' / (project.stem + '.dll')], expected)
        if args.nativeaot:
            publish = out / (name + '-aot')
            run(name + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', args.rid,
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', publish, '--nologo', '-v:q'])
            run(name + '-nativeaot', [publish / project.stem], expected)

    error = None
    try:
        run('oracles', [sys.executable, '-B', Path(__file__).with_name('duckdb_global_lifetime_oracle.py'),
            '--stock-compiler', args.stock_compiler.resolve(), '--stock-runtime', args.stock_runtime.resolve(),
            '--output-dir', out / 'oracles'])
        for case, expected in CASES.items():
            source = out / 'oracles' / ('duckdb_global_lifetime_' + case + '.cpp')
            obj = out / (case + '.dotobj'); objects[case] = obj
            run(case + '-export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / (case + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
            source.rename(source.with_suffix('.retained.cpp'))
            run(case + '-verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                check(case + '-persistent-source-owned', db.execute("SELECT count(*) FROM source_static_destructions WHERE kind='persistent'").fetchone()[0] > 0)
                if case == 'no_effect':
                    check('stock-no-effect-disposition-retained', db.execute("SELECT count(*) FROM static_destruction_dispositions WHERE kind='stock-proven-no-effect'").fetchone()[0] == 1)
                    check('suppressed-constexpr-source-type-retained', db.execute("SELECT count(*) FROM source_initializers WHERE source_name='index' AND execution_disposition='stock-not-emitted' AND storage_id IS NULL AND source_type_id IS NOT NULL").fetchone()[0] == 1)
                else:
                    check(case + '-owned-registration-present', db.execute('SELECT count(*) FROM static_destruction_registrations').fetchone()[0] > 0)
                check(case + '-no-synthetic-dso-import', db.execute("SELECT count(*) FROM storages WHERE source_name='__dso_handle'").fetchone()[0] == 0)
                if case == 'array':
                    check('generated-array-destructor-owned', db.execute('SELECT count(*) FROM generated_static_destructors WHERE element_count=2 AND element_size=4').fetchone()[0] == 1)
                    check('original-static-array-rollback-owned', db.execute('SELECT count(*) FROM cleanup_actions WHERE source_static_obligation_id IS NOT NULL AND source_obligation_id IS NULL').fetchone()[0] > 0)
                    check('original-partial-array-obligations-retained', db.execute("SELECT count(*) FROM source_static_destructions WHERE kind='partial-aggregate' AND source_initializer_id IS NOT NULL AND storage_id IS NULL").fetchone()[0] == 2)
                    check('terminal-partial-elision-observed', db.execute("SELECT count(*) FROM static_destruction_dispositions WHERE kind='stock-terminal-no-throw'").fetchone()[0] == 1)
                if case == 'temporary':
                    check('original-initializer-temporary-adopted', db.execute('SELECT count(*) FROM initializer_lifetime_executions').fetchone()[0] > 0)
                    check('initializer-eh-frame-scaffolds-owned', db.execute("SELECT count(*) FROM generated_routine_scaffolds WHERE kind='cleanup-frame' AND binding_id IS NULL AND capture_proof='observed-function-cleanup-frame'").fetchone()[0] == 2)
            generated = link(case + '-generated', obj, True)
            execute(case, generated / 'Translated.csproj', expected)

        library = link('library', objects['interleave'], False)
        host = out / 'host'; host.mkdir()
        (host / 'Host.cs').write_text('''internal static class Host {
    public static int Main() {
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        int result = global::Tests.StaticLifetime.Core.library_body();
        global::Edg.Runtime.ObjectProgram.FinalizeLibrary();
        global::Edg.Runtime.ObjectProgram.FinalizeLibrary();
        return result;
    }
}
''')
        project = host / 'Host.csproj'
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
            '<TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>'
            '<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup>'
            '<ProjectReference Include="../library/Translated.csproj" /></ItemGroup></Project>\n')
        execute('library-double-disposal', project, CASES['interleave'])

        def reject(name, case, sql):
            damaged = out / (name + '.dotobj'); shutil.copy2(objects[case], damaged)
            with sqlite3.connect(damaged) as db:
                check(name + '-mutated', db.execute(sql).rowcount > 0)
                check(name + '-foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], succeeds=False)
        reject('reject-missing-static-disposition', 'scalar', 'DELETE FROM static_destruction_dispositions')
        reject('reject-missing-static-registration', 'scalar', 'DELETE FROM static_destruction_registrations')
        reject('reject-unowned-static-callback', 'array', 'DELETE FROM generated_static_destructors')
        reject('reject-static-array-stride', 'array', 'UPDATE generated_static_destructors SET element_size=element_size+1')
        reject('reject-static-array-count', 'array', 'UPDATE generated_static_destructors SET element_count=element_count+1')
        reject('reject-missing-initializer-adoption', 'temporary', 'DELETE FROM initializer_lifetime_executions')
        reject('reject-original-initializer-owner-lost', 'temporary', 'UPDATE source_static_lifetimes SET source_initializer_id=NULL WHERE source_initializer_id IS NOT NULL')
        reject('reject-original-partial-owner-lost', 'array', "UPDATE source_static_destructions SET source_initializer_id=NULL WHERE kind='partial-aggregate'")
        reject('reject-missing-initializer-frame', 'temporary', "DELETE FROM generated_routine_scaffolds WHERE kind='cleanup-frame'")
        reject('reject-missing-partial-disposition', 'array', "DELETE FROM static_destruction_dispositions WHERE kind='stock-terminal-no-throw'")
        reject('reject-rollback-elision-mismatch', 'array', "UPDATE static_destruction_dispositions SET kind='stock-terminal-no-throw' WHERE kind='cleanup-action'")
        reject('reject-static-rollback-wrong-target', 'array', "UPDATE cleanup_actions SET target_node_id=(SELECT id FROM nodes WHERE kind='function-address' AND function_id=cleanup_actions.function_entity_id LIMIT 1) WHERE source_static_obligation_id IS NOT NULL")
        reject('reject-static-rollback-wrong-callback', 'array', "UPDATE cleanup_actions SET callback_entity_id=(SELECT id FROM entities WHERE source_name='main' LIMIT 1) WHERE source_static_obligation_id IS NOT NULL")
        reject('reject-callback-array-count-operand', 'array', "UPDATE nodes SET constant_text='3' WHERE id IN (SELECT e.child_id FROM array_runtime_operations a JOIN edges e ON e.parent_id=a.node_id AND e.role='argument' AND e.ordinal=1 WHERE a.source_static_obligation_id IS NOT NULL)")
        reject('reject-registration-selected-callback', 'scalar', "UPDATE static_destruction_registrations SET callback_entity_id=(SELECT id FROM entities WHERE source_name='main' LIMIT 1)")
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
        tools={name: dict(path=str(getattr(args, name.replace('-', '_')).resolve()),
                         sha256=hashlib.sha256(getattr(args, name.replace('-', '_')).read_bytes()).hexdigest())
               for name in ('compiler', 'packager', 'linker', 'stock-compiler', 'stock-runtime')})
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    sys.exit(main())
