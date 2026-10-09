#!/usr/bin/env python3
"""Ordered typed catch regions and parameter lifetimes against native/stock C."""
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
    for name in ('cpp_typed_catch.cpp', 'cpp_typed_catch_native.cpp', 'TypedCatchHost.cs'):
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
        native = inputs / 'cpp_typed_catch_native.cpp'
        oracle = None
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, native, '-o', binary])
            result = run('native-run-O' + optimization, [binary])
            if oracle is None:
                oracle = result.stdout
            run('native-copy-failure-terminates-O' + optimization, [binary, 'copy-failure'], 77)
            check('native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        check('fourteen-native-scenarios', len(oracle.splitlines()) == 14)
        run('stock-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', native])
        for optimization in ('0', '2'):
            binary = out / ('stock-O' + optimization)
            run('stock-build-O' + optimization, ['cc', '-x', 'c', '-O' + optimization,
                out / 'cpp_typed_catch_native.int.c', '-x', 'none', args.stock_runtime.resolve(), '-lm', '-lstdc++', '-o', binary])
            result = run('stock-run-O' + optimization, [binary])
            run('stock-copy-failure-terminates-O' + optimization, [binary, 'copy-failure'], 77)
            check('stock-native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        obj = out / 'initializer.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', inputs / 'cpp_typed_catch.cpp',
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('owned-tries', db.execute('SELECT count(*) FROM source_tries').fetchone()[0] == 15)
            check('ordered-handlers', db.execute('SELECT count(*) FROM source_handlers').fetchone()[0] == 21)
            check('typed-handlers', db.execute("SELECT count(*) FROM source_handlers WHERE match_kind='typed'").fetchone()[0] == 18)
            check('handler-release-states', db.execute('SELECT count(*) FROM cleanup_states WHERE handler_node_id IS NOT NULL').fetchone()[0] == 21)
            check('owned-caught-addresses', db.execute('SELECT count(*) FROM caught_object_addresses').fetchone()[0] == 18)
            address, handler = db.execute('SELECT node_id,handler_node_id FROM caught_object_addresses LIMIT 1').fetchone()
            other_handler, = db.execute("SELECT node_id FROM source_handlers WHERE node_id<>? AND match_kind='typed' LIMIT 1", (handler,)).fetchone()
            wrong_type, = db.execute("SELECT id FROM source_types WHERE kind='long' LIMIT 1").fetchone()
            owner, = db.execute('SELECT try_node_id FROM source_handlers WHERE node_id=?', (handler,)).fetchone()
            block, = db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='body'", (handler,)).fetchone()
            wrong_binding, = db.execute("SELECT id FROM bindings WHERE kind='local' AND scope_node_id<>? LIMIT 1", (block,)).fetchone()
        inputs.rename(out / 'retained-source')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
            '--namespace', 'Tests.TypedCatch', '--class', 'Core', '--allow-externs', obj])
        source_map = json.loads((generated / 'source-map.json').read_text())['objects'][0]
        check('review-map-handlers', len(source_map['sourceTries']) == 15 and len(source_map['sourceHandlers']) == 21)
        check('review-map-release-ownership', len([state for state in source_map['cleanupStates'] if state['handlerNodeId'] is not None]) == 21)
        shutil.copy2(out / 'retained-source/TypedCatchHost.cs', generated / 'TypedCatchHost.cs')
        project = generated / 'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
            '<OutputType>Exe</OutputType><StartupObject>TypedCatchHost</StartupObject>'))
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        result = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        check('jit-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        failure = run('jit-copy-failure', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll', 'copy-failure'], None)
        check('jit-copy-failure-terminates-before-unwinding', b'Fixture: catch initialization failure' in failure.stderr)
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
            result = run('nativeaot', [out / 'publish/Translated'])
            check('nativeaot-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
            failure = run('nativeaot-copy-failure', [out / 'publish/Translated', 'copy-failure'], None)
            check('nativeaot-copy-failure-terminates-before-unwinding', b'Fixture: catch initialization failure' in failure.stderr)
        mutations = [
            ('missing-caught-address-proof', [('DELETE FROM caught_object_addresses WHERE node_id=?', (address,))]),
            ('foreign-caught-address-owner', [('UPDATE caught_object_addresses SET handler_node_id=? WHERE node_id=?', (other_handler, address))]),
            ('wrong-match-type', [('UPDATE source_handlers SET match_type_id=? WHERE node_id=?', (wrong_type, handler))]),
            ('foreign-parameter-binding', [('UPDATE source_handlers SET parameter_binding_id=? WHERE node_id=?', (wrong_binding, handler))]),
            ('wrong-handler-order', [('UPDATE source_handlers SET ordinal=99 WHERE node_id=?', (handler,))]),
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
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, cases=14,
        commands=commands, rejections=rejections, fixtureProviderOnly=True,
        limitations=['Fixture provider only; production type matching and exception ownership remain unbound.',
                    'Function-try blocks and flat-control transfers across protected regions remain gates.',
                    'No post-start copy failure, destructor failure during active unwinding, virtual base or thread-transfer execution coverage.'])
    for name in ('compiler', 'stock_compiler', 'stock_runtime', 'linker', 'packager'):
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)


if __name__ == '__main__':
    raise SystemExit(main())
