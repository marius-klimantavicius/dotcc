#!/usr/bin/env python3
"""Initializer cleanup ordering against native/stock C, with a fixture-only provider."""
import argparse
import hashlib
import importlib.util
import json
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
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs, tooling = out / 'inputs', out / 'tooling'
    inputs.mkdir(); tooling.mkdir()
    for name in ('cpp_throw_initializer.cpp', 'cpp_throw_initializer_native.cpp', 'ThrowInitializerHost.cs'):
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
        check(name, result.returncode == expected)
        return result

    error = None
    try:
        native = inputs / 'cpp_throw_initializer_native.cpp'
        oracle = None
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, native, '-o', binary])
            result = run('native-run-O' + optimization, [binary])
            if oracle is None:
                oracle = result.stdout
            check('native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        check('sixty-native-scenarios', len(oracle.splitlines()) == 60)
        run('stock-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', native])
        for optimization in ('0', '2'):
            binary = out / ('stock-O' + optimization)
            run('stock-build-O' + optimization, ['cc', '-x', 'c', '-O' + optimization,
                out / 'cpp_throw_initializer_native.int.c', '-x', 'none', args.stock_runtime.resolve(), '-lm', '-o', binary])
            result = run('stock-run-O' + optimization, [binary])
            check('stock-native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        obj = out / 'initializer.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', inputs / 'cpp_throw_initializer.cpp',
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('every-throw-has-boundary', db.execute('SELECT count(*) FROM throw_initialization_boundaries').fetchone()[0] ==
                  db.execute('SELECT count(*) FROM throw_expressions').fetchone()[0] == 7)
            check('initializer-transitions-owned', db.execute('SELECT count(*) FROM throw_cleanup_transitions').fetchone()[0] > 0)
            check('conditional-cleanup-present', db.execute('SELECT count(*) FROM cleanup_conditions').fetchone()[0] > 0)
            transition, owner = db.execute('SELECT node_id,throw_node_id FROM throw_cleanup_transitions LIMIT 1').fetchone()
            other = db.execute('SELECT node_id FROM throw_expressions WHERE node_id<>? LIMIT 1', (owner,)).fetchone()[0]
            outside = db.execute('SELECT node_id FROM cleanup_transitions WHERE node_id NOT IN (SELECT node_id FROM throw_cleanup_transitions) LIMIT 1').fetchone()[0]
        inputs.rename(out / 'retained-source')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
            '--namespace', 'Tests.ThrowInit', '--class', 'Core', '--allow-externs', obj])
        shutil.copy2(out / 'retained-source/ThrowInitializerHost.cs', generated / 'ThrowInitializerHost.cs')
        project = generated / 'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
            '<OutputType>Exe</OutputType><StartupObject>ThrowInitializerHost</StartupObject>'))
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        result = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        check('jit-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
            result = run('nativeaot', [out / 'publish/Translated'])
            check('nativeaot-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        mutations = [
            ('missing-boundary', [('DELETE FROM throw_cleanup_transitions WHERE throw_node_id=?', (owner,)),
                                  ('DELETE FROM throw_initialization_boundaries WHERE throw_node_id=?', (owner,))]),
            ('missing-transition-owner', [('DELETE FROM throw_cleanup_transitions WHERE node_id=?', (transition,))]),
            ('wrong-transition-owner', [('UPDATE throw_cleanup_transitions SET throw_node_id=? WHERE node_id=?', (other, transition))]),
            ('outer-transition-owned-by-initializer', [('INSERT INTO throw_cleanup_transitions VALUES(?,?,?)',
                                                       (outside, owner, 'observed-throw-initializer-transition'))]),
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
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, cases=60,
        commands=commands, rejections=rejections, fixtureProviderOnly=True,
        limitations=['No production exception runtime or translated source catch matching is implemented.',
                    'Payload subobject rollback is covered separately by cpp_throw_payload_verify.py.',
                    'No post-start copy failure, throwing destructor, virtual base or thread-transfer execution coverage.'])
    for name in ('compiler', 'stock_compiler', 'stock_runtime', 'linker', 'packager'):
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)


if __name__ == '__main__':
    raise SystemExit(main())
