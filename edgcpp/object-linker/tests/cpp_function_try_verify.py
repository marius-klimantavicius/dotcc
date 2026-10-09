#!/usr/bin/env python3
"""Constructor/destructor function-try cleanup and implicit rethrow against native/stock C."""
import argparse
import hashlib
import importlib.util
import json
import os
import resource
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'stock-compiler', 'stock-runtime', 'packager', 'linker', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    resource.setrlimit(resource.RLIMIT_CORE, (0,0))
    os.environ["DOTNET_DbgEnableMiniDump"] = "0"
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs, tooling = out / 'inputs', out / 'tooling'
    inputs.mkdir(); tooling.mkdir()
    for name in ('cpp_function_try.cpp', 'cpp_function_try_native.cpp', 'FunctionTryHost.cs'):
        shutil.copy2(Path(__file__).parent / name, inputs / name)
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('throw_init_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    checks, commands, rejections = [], [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, expected=0):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == expected if expected is not None else result.returncode != 0)
        return result

    error = None
    try:
        native = inputs / 'cpp_function_try_native.cpp'
        oracle = None
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, native, '-o', binary])
            result = run('native-run-O' + optimization, [binary])
            if oracle is None:
                oracle = result.stdout
            check('native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        check('thirty-four-native-scenarios', len(oracle.splitlines()) == 34)
        run('stock-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', native])
        for optimization in ('0', '2'):
            binary = out / ('stock-O' + optimization)
            run('stock-build-O' + optimization, ['cc', '-x', 'c', '-O' + optimization,
                out / 'cpp_function_try_native.int.c', '-x', 'none', args.stock_runtime.resolve(), '-lm', '-lstdc++', '-o', binary])
            result = run('stock-run-O' + optimization, [binary])
            check('stock-native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        obj = out / 'initializer.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', inputs / 'cpp_function_try.cpp',
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('ordinary-expression-lifetime-retained', db.execute('SELECT count(*) FROM expression_lifetimes').fetchone()[0] > 0)
            check('owned-try-kinds', db.execute('SELECT try_kind,count(*) FROM source_tries GROUP BY try_kind ORDER BY try_kind').fetchall() == [('constructor',2),('destructor',2),('ordinary',1)])
            check('implicit-rethrows', db.execute('SELECT count(*) FROM rethrow_expressions').fetchone()[0] >= 3)
            check('owned-member-base-cleanup', db.execute('SELECT count(*) FROM cleanup_actions').fetchone()[0] >= 8)
            constructor, = db.execute("SELECT node_id FROM source_tries WHERE try_kind='constructor'").fetchone()
            destructor, = db.execute("SELECT node_id FROM source_tries WHERE try_kind='destructor'").fetchone()
            ordinary, = db.execute("SELECT node_id FROM source_tries WHERE try_kind='ordinary'").fetchone()
            wrong_lifetime, = db.execute("SELECT id FROM source_lifetimes WHERE kind='try-block' AND id<>(SELECT source_lifetime_id FROM source_tries WHERE node_id=?) LIMIT 1",(constructor,)).fetchone()
        inputs.rename(out / 'retained-source')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
            '--namespace', 'Tests.FunctionTry', '--class', 'Core', '--allow-externs', obj])
        report = json.loads((generated / 'link-report.json').read_text())
        check('advertised-source-try-kinds', report['sourceExceptionRuntime']['sourceTryKinds'] == ['ordinary','constructor','destructor'])
        source_map = json.loads((generated / 'source-map.json').read_text())['objects'][0]
        check('review-map-expression-lifetime-shape', all(set(row)=={'nodeId','sourceLifetimeId','captureProof'} and row['captureProof']=='observed-lifetime-wrapper' for row in source_map['expressionLifetimes']) and len(source_map['expressionLifetimes'])>0)
        check('review-map-handlers', len(source_map['sourceTries']) == len(source_map['sourceHandlers']) == 5)
        check('review-map-release-ownership', len([state for state in source_map['cleanupStates'] if state['handlerNodeId'] is not None]) == 5)
        shutil.copy2(out / 'retained-source/FunctionTryHost.cs', generated / 'FunctionTryHost.cs')
        project = generated / 'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
            '<OutputType>Exe</OutputType><StartupObject>FunctionTryHost</StartupObject>'))
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        result = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        check('jit-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
            result = run('nativeaot', [out / 'publish/Translated'])
            check('nativeaot-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        mutations = [
            ('constructor-as-destructor', [("UPDATE source_tries SET try_kind='destructor' WHERE node_id=?", (constructor,))]),
            ('destructor-as-constructor', [("UPDATE source_tries SET try_kind='constructor' WHERE node_id=?", (destructor,))]),
            ('ordinary-as-constructor', [("UPDATE source_tries SET try_kind='constructor' WHERE node_id=?", (ordinary,))]),
            ('foreign-original-lifetime', [('UPDATE source_tries SET source_lifetime_id=? WHERE node_id=?', (wrong_lifetime,constructor))]),
        ]
        for name, statements in mutations:
            bad = out / (name + '.dotobj')
            shutil.copy2(obj, bad)
            with sqlite3.connect(bad) as db:
                for sql, parameters in statements:
                    db.execute(sql, parameters)
                check(name + '-foreign-keys-valid', not db.execute('PRAGMA foreign_key_check').fetchall())
                check(name + '-schema-valid', db.execute('PRAGMA integrity_check').fetchall() == [('ok',)])
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            result = run(name, ['dotnet', args.linker.resolve(), 'verify', bad], 1)
            check(name + '-semantic-rejection', 'digest' not in result.stderr.decode(errors='replace').lower())
            rejections.append(name)
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, cases=34,
        commands=commands, rejections=rejections, fixtureProviderOnly=True,
        limitations=['Fixture provider only; production type matching and exception ownership remain unbound.',
                    'Virtual-base/VTT and array cleanup and flat-control transfers across protected regions remain gates.',
                    'No post-start copy failure, destructor failure during active unwinding, virtual base or thread-transfer execution coverage.'])
    for name in ('compiler', 'stock_compiler', 'stock_runtime', 'linker', 'packager'):
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)


if __name__ == '__main__':
    raise SystemExit(main())
