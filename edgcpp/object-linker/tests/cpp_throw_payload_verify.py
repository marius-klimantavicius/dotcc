#!/usr/bin/env python3
"""Partial exception payload rollback against native C++; stock C retained as a lowering reference."""
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
    parser.add_argument('--native-cxx', default='clang++-22')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs, tooling = out / 'inputs', out / 'tooling'
    inputs.mkdir(); tooling.mkdir()
    for name in ('cpp_throw_payload.cpp', 'cpp_throw_payload_native.cpp', 'ThrowPayloadHost.cs'):
        shutil.copy2(Path(__file__).parent / name, inputs / name)
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('throw_payload_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    checks, commands, rejections, stock_comparisons, gcc_comparisons = [], [], [], [], []

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
        native = inputs / 'cpp_throw_payload_native.cpp'
        oracle = None
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, [args.native_cxx, '-std=c++17', '-O' + optimization, native, '-o', binary])
            result = run('native-run-O' + optimization, [binary])
            if oracle is None:
                oracle = result.stdout
            check('native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        # Keep a second native implementation as evidence. GCC 13 omits the
        # completed nested aggregate's destructor body on two failure paths;
        # do not rewrite the fixture or translated callbacks to hide that.
        for optimization in ('0', '2'):
            binary = out / ('gcc-O' + optimization)
            run('gcc-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, native, '-o', binary])
            result = run('gcc-run-O' + optimization, [binary])
            check('gcc-output-shape-O' + optimization, len(result.stdout.splitlines()) == 40 and not result.stderr)
            gcc_comparisons.append(dict(optimization=optimization, mismatches=[dict(native=a.decode(), gcc=b.decode())
                for a, b in zip(oracle.splitlines(), result.stdout.splitlines()) if a != b]))
        check('forty-native-scenarios', len(oracle.splitlines()) == 40)
        run('stock-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', native])
        for optimization in ('0', '2'):
            binary = out / ('stock-O' + optimization)
            run('stock-build-O' + optimization, ['cc', '-x', 'c', '-O' + optimization,
                out / 'cpp_throw_payload_native.int.c', '-x', 'none', args.stock_runtime.resolve(), '-lm', '-o', binary])
            result = run('stock-run-O' + optimization, [binary])
            # Stock C is a reference for region/position lowering. Retain its
            # independently executed result, including aggregate-success cases
            # whose partial-region lifetime differs from native C++ semantics.
            check('stock-output-shape-O' + optimization, len(result.stdout.splitlines()) == 40 and not result.stderr)
            mismatches = [dict(native=a.decode(), stock=b.decode())
                for a, b in zip(oracle.splitlines(), result.stdout.splitlines()) if a != b]
            stock_comparisons.append(dict(optimization=optimization, mismatches=mismatches))
        obj = out / 'initializer.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', inputs / 'cpp_throw_payload.cpp',
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('every-throw-has-boundary', db.execute('SELECT count(*) FROM throw_initialization_boundaries').fetchone()[0] ==
                  db.execute('SELECT count(*) FROM throw_expressions').fetchone()[0] == 10)
            check('payload-subobject-ownership', db.execute('SELECT count(*) FROM throw_payload_subobjects').fetchone()[0] == 20)
            check('payload-rollback-actions', db.execute('SELECT count(*) FROM cleanup_payload_targets').fetchone()[0] == 12)
            check('nested-payload-paths', db.execute('SELECT count(*) FROM throw_payload_paths WHERE ordinal>0').fetchone()[0] > 0)
            check('empty-base-payload-path', db.execute('SELECT count(*) FROM throw_payload_paths WHERE source_base_id IS NOT NULL').fetchone()[0] > 0)
            root, action, owner, obligation = db.execute('SELECT p.node_id,p.action_id,p.throw_node_id,a.source_obligation_id FROM cleanup_payload_targets p JOIN cleanup_actions a ON a.id=p.action_id LIMIT 1').fetchone()
            final_obligation, = db.execute('SELECT source_obligation_id FROM throw_payload_subobjects WHERE source_obligation_id NOT IN (SELECT source_obligation_id FROM cleanup_actions) LIMIT 1').fetchone()
            other, = db.execute('SELECT node_id FROM throw_expressions WHERE node_id<>? AND destructor_entity_id IS NOT NULL LIMIT 1', (owner,)).fetchone()
            same_type_other, = db.execute('SELECT t.node_id FROM throw_expressions t JOIN nodes n ON n.id=t.node_id WHERE t.exception_type_id=(SELECT exception_type_id FROM throw_expressions WHERE node_id=?) AND n.function_id=(SELECT function_id FROM nodes WHERE id=?) AND t.node_id<>? LIMIT 1', (owner, owner, owner)).fetchone()
            added, = db.execute('SELECT max(id)+1 FROM nodes').fetchone()
            path_obligation, field, replacement = db.execute('SELECT p.source_obligation_id,p.field_id,g.id FROM throw_payload_paths p JOIN record_fields f ON f.id=p.field_id JOIN record_fields g ON g.record_type_id=f.record_type_id AND g.type_id=f.type_id AND g.id<>f.id WHERE p.source_obligation_id IN (SELECT source_obligation_id FROM cleanup_actions) AND p.ordinal=0 LIMIT 1').fetchone()
        inputs.rename(out / 'retained-source')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
            '--namespace', 'Tests.ThrowPayload', '--class', 'Core', '--exception-provider', 'external', '--allow-externs', obj])
        shutil.copy2(out / 'retained-source/ThrowPayloadHost.cs', generated / 'ThrowPayloadHost.cs')
        project = generated / 'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
            '<OutputType>Exe</OutputType><StartupObject>ThrowPayloadHost</StartupObject>'))
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        result = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        check('jit-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
            result = run('nativeaot', [out / 'publish/Translated'])
            check('nativeaot-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        mutations = [
            ('same-type-foreign-initializer', [
                ('UPDATE cleanup_payload_targets SET throw_node_id=? WHERE node_id=?', (same_type_other, root)),
                ('UPDATE throw_payload_subobjects SET throw_node_id=? WHERE source_obligation_id=?', (same_type_other, obligation))]),
            ('unowned-payload-pointer-adjustment', [
                ("INSERT INTO nodes SELECT ?,function_id,'binary',type_id,NULL,NULL,'pointer-add',NULL,'value','left-to-right',source_file_id,line,column,NULL,NULL FROM nodes WHERE id=?", (added, root)),
                ("INSERT INTO nodes SELECT ?,function_id,'constant',9,NULL,NULL,NULL,'1','value','none',source_file_id,line,column,NULL,NULL FROM nodes WHERE id=?", (added+1, root)),
                ('UPDATE edges SET child_id=? WHERE child_id=?', (added, root)),
                ("INSERT INTO edges VALUES(?,'left',0,?)", (added, root)),
                ("INSERT INTO edges VALUES(?,'right',0,?)", (added, added+1))]),
            ('missing-payload-address-proof', [('DELETE FROM cleanup_payload_targets WHERE node_id=?', (root,))]),
            ('wrong-payload-throw-owner', [('UPDATE cleanup_payload_targets SET throw_node_id=? WHERE node_id=?', (other, root))]),
            ('missing-final-subobject-owner', [('DELETE FROM throw_payload_paths WHERE source_obligation_id=?', (final_obligation,)),
                ('DELETE FROM throw_payload_subobjects WHERE source_obligation_id=?', (final_obligation,))]),
            ('empty-subobject-path', [('DELETE FROM throw_payload_paths WHERE source_obligation_id=?', (final_obligation,))]),
            ('wrong-same-type-member', [('UPDATE throw_payload_paths SET field_id=? WHERE source_obligation_id=? AND ordinal=0', (replacement, path_obligation))]),
            ('mismatched-action-obligation', [('UPDATE cleanup_actions SET source_obligation_id=? WHERE id=?', (final_obligation, action))]),
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
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, cases=40,
        commands=commands, rejections=rejections, fixtureProviderOnly=True, stockComparisons=stock_comparisons, gccComparisons=gcc_comparisons, nativeCxx=args.native_cxx,
        limitations=['No production exception runtime or translated source catch matching is implemented.',
                    'Array-sequence, virtual-base/VTT and delegated payload rollback remain unsupported.',
                    'No post-start copy failure, throwing destructor, virtual base or thread-transfer execution coverage.'])
    for name in ('compiler', 'stock_compiler', 'stock_runtime', 'linker', 'packager'):
        path = getattr(args, name).resolve()
        receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)


if __name__ == '__main__':
    raise SystemExit(main())
