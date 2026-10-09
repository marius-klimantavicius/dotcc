#!/usr/bin/env python3
"""Owned catch-all regions, release states and rethrow against native/stock C."""
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
    for name in ('cpp_catch_all.cpp', 'cpp_catch_all_native.cpp', 'CatchAllHost.cs'):
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
        native = inputs / 'cpp_catch_all_native.cpp'
        oracle = None
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, native, '-o', binary])
            result = run('native-run-O' + optimization, [binary])
            if oracle is None:
                oracle = result.stdout
            check('native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        check('one-hundred-five-native-scenarios', len(oracle.splitlines()) == 105)
        run('stock-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--exceptions', '--no_rtti', native])
        for optimization in ('0', '2'):
            binary = out / ('stock-O' + optimization)
            run('stock-build-O' + optimization, ['cc', '-x', 'c', '-O' + optimization,
                out / 'cpp_catch_all_native.int.c', '-x', 'none', args.stock_runtime.resolve(), '-lm', '-o', binary])
            result = run('stock-run-O' + optimization, [binary])
            check('stock-native-parity-O' + optimization, result.stdout == oracle and not result.stderr)
        obj = out / 'initializer.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', inputs / 'cpp_catch_all.cpp',
            '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('owned-try-and-handler', db.execute('SELECT count(*) FROM source_tries').fetchone()[0] ==
                  db.execute('SELECT count(*) FROM source_handlers').fetchone()[0] == 7)
            check('handler-release-states', db.execute('SELECT count(*) FROM cleanup_states WHERE handler_node_id IS NOT NULL').fetchone()[0] == 7)
            check('ordinary-object-actions-retained', db.execute('SELECT count(*) FROM cleanup_actions').fetchone()[0] >= 7)
            check('try-original-lifetimes', db.execute("SELECT count(*) FROM source_tries t JOIN source_lifetimes l ON l.id=t.source_lifetime_id WHERE l.kind='try-block'").fetchone()[0] == 7)
            marker, handler = db.execute("SELECT node_id,owner_node_id FROM handler_markers WHERE kind='caught' LIMIT 1").fetchone()
            other_handler, = db.execute('SELECT node_id FROM source_handlers WHERE node_id<>? LIMIT 1', (handler,)).fetchone()
            state, = db.execute('SELECT id FROM cleanup_states WHERE handler_node_id=?', (handler,)).fetchone()
            owner, = db.execute('SELECT try_node_id FROM source_handlers WHERE node_id=?', (handler,)).fetchone()
            wrong_lifetime, = db.execute("SELECT id FROM source_lifetimes WHERE kind='block' LIMIT 1").fetchone()
            body_node, = db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='body'", (handler,)).fetchone()
            outside_transition, = db.execute('SELECT t.node_id FROM cleanup_transitions t JOIN nodes n ON n.id=t.node_id WHERE n.function_id=(SELECT function_id FROM nodes WHERE id=?) ORDER BY t.node_id LIMIT 1', (handler,)).fetchone()
        inputs.rename(out / 'retained-source')
        generated = out / 'generated'
        run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
            '--namespace', 'Tests.CatchAll', '--class', 'Core', '--allow-externs', obj])
        source_map = json.loads((generated / 'source-map.json').read_text())['objects'][0]
        check('review-map-handlers', len(source_map['sourceTries']) == len(source_map['sourceHandlers']) == 7)
        check('review-map-release-ownership', len([state for state in source_map['cleanupStates'] if state['handlerNodeId'] is not None]) == 7)
        shutil.copy2(out / 'retained-source/CatchAllHost.cs', generated / 'CatchAllHost.cs')
        project = generated / 'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
            '<OutputType>Exe</OutputType><StartupObject>CatchAllHost</StartupObject>'))
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        result = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        check('jit-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
            result = run('nativeaot', [out / 'publish/Translated'])
            check('nativeaot-native-parity-and-release-order', result.stdout == oracle and not result.stderr)
        # A module containing only a handler-release state still needs the
        # shared unwind hooks; ordinary object-destructor actions are absent.
        minimal_source = out / 'only_handler.cpp'
        minimal_source.write_text('extern "C" int only_handler() { try { throw 16; } catch (...) { return 17; } }\n')
        minimal_object = out / 'only-handler.dotobj'
        run('minimal-export', [sys.executable, '-B', tooling / 'compile.py', minimal_source,
            '--output', minimal_object, '--work-dir', out / 'minimal-work', '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'])
        with sqlite3.connect(minimal_object) as db:
            check('minimal-no-object-actions', db.execute('SELECT count(*) FROM cleanup_actions').fetchone()[0] == 0)
            check('minimal-release-state', db.execute('SELECT count(*) FROM cleanup_states').fetchone()[0] == 1)
        minimal_source.rename(out / 'retained-only-handler.cpp')
        minimal = out / 'minimal-generated'
        run('minimal-link', ['dotnet', args.linker.resolve(), 'link', '--out', minimal,
            '--namespace', 'Tests.CatchAll', '--class', 'Core', '--allow-externs', minimal_object])
        host = (out / 'retained-source/CatchAllHost.cs').read_text()
        start = host.index('    public static int Main()')
        end = host.index('\n}\nnamespace Edg.Runtime', start)
        host = host[:start] + '    public static int Main() { if (Core.only_handler()!=17 || LivePayloads!=0 || global::Edg.Runtime.FixtureCatch.Frames.Count!=0) return 1; return 0; }\n' + host[end:]
        start = host.index('    public static unsafe partial class LibC')
        end = host.index('    internal sealed unsafe class FixturePayload', start)
        host = host[:start] + host[end:]
        (minimal / 'CatchAllHost.cs').write_text(host)
        project = minimal / 'Translated.csproj'
        project.write_text(project.read_text().replace('<OutputType>Library</OutputType>',
            '<OutputType>Exe</OutputType><StartupObject>CatchAllHost</StartupObject>'))
        run('minimal-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        run('minimal-jit', ['dotnet', minimal / 'bin/Release/net10.0/Translated.dll'])
        if args.nativeaot:
            run('minimal-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'minimal-publish', '--nologo', '-v:q'])
            run('minimal-nativeaot', [out / 'minimal-publish/Translated'])
        mutations = [
            ('missing-caught-marker-proof', [('DELETE FROM handler_markers WHERE node_id=?', (marker,))]),
            ('foreign-caught-marker-owner', [('UPDATE handler_markers SET owner_node_id=? WHERE node_id=?', (other_handler, marker))]),
            ('wrong-original-try-lifetime', [('UPDATE source_tries SET source_lifetime_id=? WHERE node_id=?', (wrong_lifetime, owner))]),
            ('handler-body-used-as-protected-body', [("UPDATE edges SET child_id=? WHERE parent_id=? AND role='body'", (body_node, owner))]),
            ('release-armed-before-handler', [('UPDATE cleanup_transitions SET state_id=? WHERE node_id=?', (state, outside_transition))]),
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
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, cases=105,
        commands=commands, rejections=rejections, fixtureProviderOnly=True,
        limitations=['Fixture provider only; typed source matching and catch parameter initialization remain gates.',
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
