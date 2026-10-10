#!/usr/bin/env python3
"""Check placement-construction boundaries and residual destructor initializers."""
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

HOST = '''using Tests.DuckDbLifetime;
internal static class Host
{
    public static int Main()
    {
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        for (int scenario = 0; scenario != 4; ++scenario)
            foreach (int fail in new[] {0, 1, 2, 3, 10, 12})
                global::System.Console.WriteLine($"{scenario} {fail} {Core.duckdb_lifetime_case(scenario, fail)}");
        return 0;
    }
}
'''

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'stock-compiler', 'stock-runtime', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    source = inputs / 'duckdb_lifetime.cpp'; native = inputs / 'duckdb_lifetime_native.cpp'
    for file in (source, native): shutil.copy2(Path(__file__).with_name(file.name), file)
    checks, commands = [], []

    def check(name, condition):
        if not condition: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=900,
            preexec_fn=lambda: resource.setrlimit(resource.RLIMIT_CORE, (0, 0)))
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)
        return result

    error = None
    try:
        reference = []
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, native, '-o', binary])
            reference.append(run('native-run-O' + optimization, [binary]).stdout)
        check('native-optimization-equivalence', reference[0] == reference[1])
        run('stock-translate', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', source])
        for optimization in ('0', '2'):
            obj = out / ('stock-O' + optimization + '.o'); binary = out / ('stock-O' + optimization)
            run('stock-compile-O' + optimization, ['cc', '-std=c11', '-O' + optimization, '-c', out / 'duckdb_lifetime.int.c', '-o', obj])
            run('stock-link-O' + optimization, ['c++', '-std=c++17', native, obj, args.stock_runtime.resolve(), '-o', binary])
            check('stock-equivalence-O' + optimization, run('stock-run-O' + optimization, [binary]).stdout == reference[0])
        tooling = out / 'tooling'; tooling.mkdir()
        for filename in ('compile.py', 'schema.sql'): shutil.copy2(args.packager.resolve().parent / filename, tooling / filename)
        obj = out / 'lifetime.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj, '--work-dir', out / 'work',
            '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('internal-try-owned-boundary', db.execute('SELECT count(*) FROM internal_try_expressions').fetchone()[0] > 0)
            check('allocation-source-type-retained', db.execute('''SELECT count(*) FROM source_allocation_obligations a
                JOIN source_types t ON t.id=a.allocated_source_type_id WHERE t.kind='record' ''').fetchone()[0] >= 3)
            check('allocation-obligations-owned', db.execute('''SELECT count(*) FROM source_allocation_obligations a
                LEFT JOIN internal_try_expressions t ON t.source_allocation_obligation_id=a.id
                WHERE t.node_id IS NULL''').fetchone()[0] == 0)
            check('internal-try-complete-pairs', db.execute('''SELECT count(*) FROM internal_try_expressions i
                JOIN edges a ON a.parent_id=i.node_id AND a.role='attempt'
                JOIN edges c ON c.parent_id=i.node_id AND c.role='cleanup' ''').fetchone()[0] ==
                db.execute('SELECT count(*) FROM internal_try_expressions').fetchone()[0])
            check('source-destructions-retained', db.execute('SELECT count(*) FROM source_destruction_obligations').fetchone()[0] >= 5)
            check('normal-destructions-retained', db.execute('SELECT count(*) FROM normal_destructions').fetchone()[0] >= 5)
        inputs.rename(out / 'retained-source')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Tests.DuckDbLifetime',
            '--class', 'Core', obj, *[argument for provider in args.provider for argument in ('--provider', provider.resolve())]])
        report = json.loads((generated / 'link-report.json').read_text())
        check('no-host-function-stubs', not report['runtimeRequirements'])
        check('no-host-storage-stubs', not report['runtimeStorageRequirements'])
        (generated / 'Host.cs').write_text(HOST)
        project = generated / 'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType><StartupObject>Host</StartupObject>'))
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        check('jit-equivalence', run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll']).stdout == reference[0])
        if args.nativeaot:
            publish = out / 'publish'
            run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', publish, '--nologo', '-v:q'])
            check('nativeaot-equivalence', run('nativeaot', [publish / 'Translated']).stdout == reference[0])
        spec = importlib.util.spec_from_file_location('dotobj_packager', tooling / 'compile.py')
        packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)

        def reject(name, sql):
            bad = out / (name + '.dotobj'); shutil.copy2(obj, bad)
            with sqlite3.connect(bad) as db:
                db.execute(sql)
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),)); db.commit()
            run(name, ['dotnet', args.linker.resolve(), 'verify', bad], False)

        reject('reject-missing-internal-boundary', 'DELETE FROM internal_try_expressions')
        reject('reject-missing-cleanup', "DELETE FROM edges WHERE role='cleanup' AND parent_id IN (SELECT node_id FROM internal_try_expressions)")
        reject('reject-wrong-internal-type', "UPDATE nodes SET type_id=2 WHERE id IN (SELECT node_id FROM internal_try_expressions)")
        reject('reject-internal-attempt-cycle', "UPDATE edges SET child_id=parent_id WHERE role='attempt' AND parent_id IN (SELECT node_id FROM internal_try_expressions)")
        reject('reject-missing-allocation-owner', 'UPDATE internal_try_expressions SET source_allocation_obligation_id=NULL')
        reject('reject-wrong-allocation-deallocator', '''UPDATE source_allocation_obligations SET deallocator_entity_id=
            (SELECT entity_id FROM source_functions WHERE function_kind='destructor' LIMIT 1)''')
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands, cases=24,
        providers=[dict(path=str(path.resolve()), sha256=hashlib.sha256(path.read_bytes()).hexdigest()) for path in args.provider])
    for name in ('compiler', 'linker', 'packager'):
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return 1 if error else 0

if __name__ == '__main__':
    sys.exit(main())
