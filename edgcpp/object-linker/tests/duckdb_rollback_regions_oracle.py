#!/usr/bin/env python3
"""Reference and optional managed acceptance for allocation and guard rollback.

Without --linker, this prepares language oracles only. With --linker it also
requires source-hidden linking, managed execution and malformed-object checks.
Stock EDG guard.c is explicitly single-threaded, so concurrent reference runs
use native C++; translated concurrency must match that independent reference.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import resource
import shutil
import sqlite3
import subprocess
import sys

HOST = '''using Tests.DuckDbRollback;
internal static class Host
{
    public static int Main(string[] arguments)
    {
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        if (arguments.Length != 0)
        {
            const int count = 32;
            using var ready = new global::System.Threading.CountdownEvent(count);
            using var start = new global::System.Threading.ManualResetEventSlim(false);
            var threads = new global::System.Threading.Thread[count];
            var results = new int[count];
            for (int i = 0; i != count; ++i)
            {
                int index = i;
                threads[i] = new global::System.Threading.Thread(() =>
                {
                    ready.Signal(); start.Wait();
                    results[index] = Core.duckdb_rollback_guard_concurrent();
                });
                threads[i].Start();
            }
            ready.Wait(); start.Set();
            foreach (var thread in threads) thread.Join();
            int success = 0, retried = 0, bad = 0;
            foreach (int result in results)
                if (result == 42) ++success; else if (result == -8) ++retried; else ++bad;
            int attempts = Core.duckdb_rollback_guard_concurrent_attempts();
            global::System.Console.WriteLine($"concurrent {success} {retried} {bad} {attempts}");
            return success == 31 && retried == 1 && bad == 0 && attempts == 2 ? 0 : 1;
        }
        for (int scenario = 0; scenario != 3; ++scenario)
            for (int fail = 0; fail != 4; ++fail)
                global::System.Console.WriteLine($"allocation {scenario} {fail} {Core.duckdb_rollback_allocation(scenario, fail)}");
        int first = Core.duckdb_rollback_guard(1);
        int retry = Core.duckdb_rollback_guard(0);
        int published = Core.duckdb_rollback_guard(1);
        int attempts2 = Core.duckdb_rollback_guard_attempts();
        global::System.Console.WriteLine($"guard {first} {retry} {published} {attempts2}");
        return first == -8 && retry == 42 && published == 42 && attempts2 == 2 ? 0 : 1;
    }
}
'''

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--packager', type=Path)
    parser.add_argument('--linker', type=Path)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    if bool(args.compiler) != bool(args.packager): parser.error('--compiler and --packager must be provided together')
    if args.linker and not args.compiler: parser.error('--linker requires --compiler and --packager')
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    names = ('duckdb_rollback_regions_allocation.cpp', 'duckdb_rollback_regions_guard.cpp', 'duckdb_rollback_regions_native.cpp')
    for name in names: shutil.copy2(Path(__file__).with_name(name), inputs / name)
    allocation, guard, host = [inputs / name for name in names]
    checks, commands, exports = [], [], []

    def check(name, condition):
        if not condition: raise AssertionError(name)
        checks.append(name)

    def run(name, command, expect_success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=900,
            preexec_fn=lambda: resource.setrlimit(resource.RLIMIT_CORE, (0, 0)))
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        if expect_success: check(name, result.returncode == 0)
        return result

    error = None
    try:
        reference = []
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, '-pthread', allocation, guard, host, '-o', binary])
            reference.append(run('native-run-O' + optimization, [binary]).stdout)
            for repetition in range(3):
                result = run(f'native-concurrent-O{optimization}-{repetition}', [binary, 'concurrent'])
                check(f'concurrent-publication-O{optimization}-{repetition}', result.stdout == b'concurrent 31 1 0 2\n')
        check('native-optimization-equivalence', reference[0] == reference[1])
        for source in (allocation, guard):
            run('stock-translate-' + source.stem, [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', source])
        for optimization in ('0', '2'):
            objects = []
            for source in (allocation, guard):
                obj = out / (source.stem + '-O' + optimization + '.o'); objects.append(obj)
                run('stock-compile-' + source.stem + '-O' + optimization,
                    ['cc', '-std=c11', '-O' + optimization, '-c', out / (source.stem + '.int.c'), '-o', obj])
            binary = out / ('stock-O' + optimization)
            run('stock-link-O' + optimization, ['c++', '-std=c++17', '-pthread', host, *objects, args.stock_runtime.resolve(), '-o', binary])
            check('stock-sequential-equivalence-O' + optimization,
                run('stock-sequential-O' + optimization, [binary]).stdout == reference[0])
        if args.compiler:
            tooling = out / 'tooling'; tooling.mkdir()
            for name in ('compile.py', 'schema.sql'): shutil.copy2(args.packager.resolve().parent / name, tooling / name)
            for source in (allocation, guard):
                work = out / (source.stem + '-work')
                result = run('export-' + source.stem, [sys.executable, '-B', tooling / 'compile.py', source,
                    '--output', out / (source.stem + '.dotobj'), '--work-dir', work,
                    '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'], expect_success=False)
                exports.append(dict(source=source.name, status='packaged' if result.returncode == 0 else 'blocked',
                    diagnostic=(work / 'stderr').read_text() if (work / 'stderr').exists() else result.stderr.decode(errors='replace')))
                if args.linker: check('export-' + source.stem, result.returncode == 0)
        if args.linker:
            objects = [out / (source.stem + '.dotobj') for source in (allocation, guard)]
            for obj in objects: run('verify-' + obj.stem, ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(objects[0]) as db:
                check('ordinary-allocation-regions-retained', db.execute('SELECT count(*) FROM allocation_cleanup_actions').fetchone()[0] >= 3)
                check('sized-delete-extent-retained', db.execute('SELECT count(*) FROM allocation_cleanup_actions WHERE allocation_size IS NOT NULL').fetchone()[0] > 0)
            with sqlite3.connect(objects[1]) as db:
                check('local-static-guard-identity-retained', db.execute('SELECT count(*) FROM local_static_guards').fetchone()[0] == 2)
                check('acquire-probe-release-retained', db.execute('SELECT count(*) FROM guard_operations').fetchone()[0] >= 6)
                check('guard-abort-actions-retained', db.execute('SELECT count(*) FROM guard_cleanup_actions').fetchone()[0] == 2)
            inputs.rename(out / 'retained-source')
            generated = out / 'generated'
            run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
                '--namespace', 'Tests.DuckDbRollback', '--class', 'Core', *objects,
                *[argument for provider in args.provider for argument in ('--provider', provider.resolve())]])
            report = json.loads((generated / 'link-report.json').read_text())
            check('no-host-function-stubs', not report['runtimeRequirements'])
            check('no-host-storage-stubs', not report['runtimeStorageRequirements'])
            (generated / 'Host.cs').write_text(HOST)
            project = generated / 'Translated.csproj'
            project.write_text(project.read_text().replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType><StartupObject>Host</StartupObject>'))
            run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
            command = ['dotnet', generated / 'bin/Release/net10.0/Translated.dll']
            check('jit-sequential-equivalence', run('jit-sequential', command).stdout == reference[0])
            for repetition in range(3):
                check('jit-concurrent-publication-' + str(repetition), run('jit-concurrent-' + str(repetition),
                    [*command, 'concurrent']).stdout == b'concurrent 31 1 0 2\n')
            if args.nativeaot:
                publish = out / 'publish'
                run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                    '-p:IlcTreatWarningsAsErrors=true', '-o', publish, '--nologo', '-v:q'])
                check('nativeaot-sequential-equivalence', run('nativeaot-sequential', [publish / 'Translated']).stdout == reference[0])
                for repetition in range(3):
                    check('nativeaot-concurrent-publication-' + str(repetition), run('nativeaot-concurrent-' + str(repetition),
                        [publish / 'Translated', 'concurrent']).stdout == b'concurrent 31 1 0 2\n')
            spec = importlib.util.spec_from_file_location('dotobj_packager', tooling / 'compile.py')
            packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
            def reject(name, source, sql):
                bad = out / (name + '.dotobj'); shutil.copy2(source, bad)
                with sqlite3.connect(bad) as db:
                    db.execute(sql); db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),)); db.commit()
                result = run(name, ['dotnet', args.linker.resolve(), 'verify', bad], expect_success=False)
                check(name, result.returncode != 0)
            reject('reject-allocation-size', objects[0], 'UPDATE allocation_cleanup_actions SET allocation_size=allocation_size+1 WHERE allocation_size IS NOT NULL')
            reject('reject-allocation-callback', objects[0], "UPDATE allocation_cleanup_actions SET callback_entity_id=(SELECT entity_id FROM source_functions WHERE function_kind='destructor' LIMIT 1)")
            reject('reject-guard-probe-metadata', objects[1], "DELETE FROM guard_operations WHERE kind='probe'")
            reject('reject-guard-publication-metadata', objects[1], "DELETE FROM guard_operations WHERE kind='release'")
            reject('reject-guard-rollback-target', objects[1], 'UPDATE nodes SET storage_id=(SELECT storage_id FROM local_static_guards ORDER BY id LIMIT 1) WHERE id IN (SELECT target_node_id FROM guard_cleanup_actions)')
            reject('reject-guard-address-type', objects[1], 'UPDATE nodes SET type_id=9 WHERE id IN (SELECT target_node_id FROM guard_cleanup_actions)')
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed' if args.linker else 'oracles-passed', error=error, checks=checks, commands=commands,
        exports=exports, managedVerification='requested' if args.linker else 'not-run', limitations=[
            'Stock EDG guard/runtime is single-threaded; stock concurrency was deliberately not run.',
            'Source-defined class pool allocators exercise language rollback, not libc++ heap completeness.'],
        providers=[dict(path=str(path.resolve()), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
                   for path in args.provider])
    for name in ('stock-compiler', 'stock-runtime', 'compiler', 'packager', 'linker'):
        path = getattr(args, name.replace('-', '_'))
        if path:
            path = path.resolve(); receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), exports=[row['status'] for row in exports], error=error)))
    return 1 if error else 0

if __name__ == '__main__': sys.exit(main())
