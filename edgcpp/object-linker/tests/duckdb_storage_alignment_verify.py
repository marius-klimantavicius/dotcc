#!/usr/bin/env python3
"""Verify incomplete extern array alignment and merged explicit requirements."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--stock-compiler', type=Path)
    parser.add_argument('--stock-runtime', type=Path)
    parser.add_argument('--native-aot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    if bool(args.stock_compiler) != bool(args.stock_runtime):
        parser.error('--stock-compiler and --stock-runtime must be provided together')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    sources = [inputs / ('duckdb_storage_alignment_' + unit + '.cpp') for unit in ('consumer', 'provider')]
    for source in sources:
        shutil.copy2(Path(__file__).with_name(source.name), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=300, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) if success else (result.returncode != 0))

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('build-native-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, *sources, '-o', binary])
            run('run-native-O' + optimization, [binary])
        if args.stock_compiler:
            env = dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ.get('PATH', ''))
            for index, source in enumerate(sources):
                run('stock-lower-' + str(index), [args.stock_compiler.resolve(), '--c++17', '--instantiate', 'used', '--no_inlining',
                    '--exceptions', '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / ('stock' + str(index) + '.c'),
                    '-o', out / ('stock' + str(index) + '.il'), source], env=env)
            for optimization in ('0', '2'):
                binary = out / ('stock-O' + optimization)
                run('build-stock-O' + optimization, ['gcc', '-w', '-O' + optimization, '-falign-functions=4', out / 'stock0.c',
                    out / 'stock1.c', args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
                run('run-stock-O' + optimization, [binary])
        objects = [out / (unit + '.dotobj') for unit in ('consumer', 'provider')]
        for source, obj in zip(sources, objects):
            run('export-' + obj.stem, [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / (obj.stem + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
            run('verify-' + obj.stem, ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(objects[0]) as db:
            ordinary = db.execute("SELECT alignment FROM storages WHERE source_name='duckdb_alignment_table' AND definition='declaration'").fetchone()
            over = db.execute("SELECT alignment FROM storages WHERE source_name='duckdb_alignment_overaligned_table' AND definition='declaration'").fetchone()
            check('raw-natural-alignment-retained', ordinary is not None and ordinary[0] == 8)
            check('raw-explicit-alignment-retained', over is not None and over[0] == 32)
        # Compiler-created VTT declarations in DuckDB retain alignment=1.
        # Ordinary source externs carry their natural 8, so exercise the same
        # legal declaration-only marker explicitly without weakening a definition.
        marker = out / 'consumer-layout-marker.dotobj'; shutil.copy2(objects[0], marker)
        with sqlite3.connect(marker) as db:
            check('declaration-placeholder-mutated', db.execute("UPDATE storages SET alignment=1 WHERE source_name='duckdb_alignment_table' AND definition='declaration'").rowcount == 1)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
        run('verify-declaration-placeholder', ['dotnet', args.linker.resolve(), 'verify', marker])
        objects[0] = marker
        inputs.rename(out / 'retained-inputs')
        run('link-external-requirements', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', out / 'external', objects[0]])
        report = json.loads((out / 'external/link-report.json').read_text())
        requirements = {row['SourceName']: row for row in report['runtimeStorageRequirements']}
        check('runtime-natural-minimum', requirements['duckdb_alignment_table']['alignment'] == 8)
        check('runtime-explicit-minimum', requirements['duckdb_alignment_overaligned_table']['alignment'] == 32)
        run('link-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'linked', *objects])
        run('build-managed', ['dotnet', 'build', out / 'linked/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'linked/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-native-aot', ['dotnet', 'publish', out / 'linked/Translated.csproj', '-c', 'Release', '-r', args.rid,
                '--self-contained', 'true', '-p:PublishAot=true', '-p:StripSymbols=false', '-o', out / 'aot', '-v:q'])
            run('run-native-aot', [out / 'aot/Translated'])
        for label, symbol, alignment in (('natural', 'duckdb_alignment_table', 1), ('explicit', 'duckdb_alignment_overaligned_table', 8)):
            damaged = out / ('bad-' + label + '.dotobj'); shutil.copy2(objects[1], damaged)
            with sqlite3.connect(damaged) as db:
                check(label + '-mutated', db.execute('UPDATE storages SET alignment=? WHERE source_name=?', (alignment, symbol)).rowcount == 1)
                check(label + '-foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            if label == 'natural':
                run('reject-natural-definition', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            else:
                run('verify-natural-but-not-explicit-definition', ['dotnet', args.linker.resolve(), 'verify', damaged])
                run('reject-weaker-merged-definition', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'bad-linked', objects[0], damaged], False)
                check('reject-exact-merged-alignment', 'required declaration alignment' in (out / 'reject-weaker-merged-definition.stderr').read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(), linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
