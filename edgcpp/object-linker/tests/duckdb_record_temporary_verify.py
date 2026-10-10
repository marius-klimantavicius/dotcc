#!/usr/bin/env python3
"""Verify original record temporary storage initialization against native and stock C."""
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
    parser.add_argument('--two-temporaries', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    parser.add_argument('--runtime-binder', type=Path, default=Path(__file__).resolve().parents[1] / 'runtime/bind.py')
    parser.add_argument('--provider', type=Path, action='append', default=[])
    args = parser.parse_args()
    if bool(args.stock_compiler) != bool(args.stock_runtime):
        parser.error('--stock-compiler and --stock-runtime must be provided together')
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
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
        for case in ('temporary',):
            source = out / ('duckdb_record_initialization_' + case + '.cpp')
            source.write_text(('#define DUCKDB_RECORD_TWO_TEMPORARIES 1\n' if args.two_temporaries else '') + '#define DUCKDB_RECORD_NATIVE_ORACLE 1\n' +
                '// Genuine deallocation required by the minimal RTTI provider closure.\n' +
                'extern "C" void free(void *);\n' +
                'void operator delete(void *p, unsigned long) noexcept { free(p); }\n' +
                Path(__file__).with_name(source.name).read_text())
            for optimization in ('0', '2'):
                binary = out / (case + '-native-O' + optimization)
                run(case + '-build-native-O' + optimization, ['c++', '-std=c++17', '-O' + optimization, source, '-o', binary])
                run(case + '-run-native-O' + optimization, [binary])
            if args.stock_compiler:
                env = dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ.get('PATH', ''))
                run(case + '-stock-lower', [args.stock_compiler.resolve(), '--c++17', '--instantiate', 'used', '--no_inlining',
                    '--exceptions', '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / (case + '.c'),
                    '-o', out / (case + '.il'), source], env=env)
                for optimization in ('0', '2'):
                    binary = out / (case + '-stock-O' + optimization)
                    run(case + '-build-stock-O' + optimization, ['gcc', '-w', '-O' + optimization, '-falign-functions=4',
                        out / (case + '.c'), args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
                    run(case + '-run-stock-O' + optimization, [binary])
            obj = out / (case + '.dotobj')
            run(case + '-export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', out / (case + '-work'), '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
            run(case + '-verify', ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                check(case + '-foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
                if args.two_temporaries:
                    check(case + '-two-original-temporaries', db.execute('SELECT count(*) FROM source_record_temporaries').fetchone()[0] == 2)
                    check(case + '-two-generated-bindings', db.execute('SELECT count(DISTINCT binding_id) FROM record_temporary_initializations').fetchone()[0] == 2)
                check(case + '-original-temporary-retained', db.execute('SELECT count(*) FROM source_record_temporaries').fetchone()[0] > 0)
                check(case + '-owned-temporary-initialization', db.execute('SELECT count(*) FROM record_temporary_initializations').fetchone()[0] > 0)
                check(case + '-no-fake-source-local', db.execute('SELECT count(*) FROM record_temporary_initializations x JOIN source_declarations d ON d.binding_id=x.binding_id').fetchone()[0] == 0)
                check(case + '-original-temporary-destruction', db.execute('SELECT count(*) FROM source_record_temporaries WHERE source_destruction_obligation_id IS NOT NULL').fetchone()[0] > 0)
                check(case + '-no-whole-record-zero-prefix', db.execute('SELECT count(*) FROM record_temporary_initializations WHERE zero_storage<>0').fetchone()[0] == 0)
                check(case + '-no-invented-record-default-value', db.execute("SELECT count(*) FROM nodes n JOIN record_types r ON r.type_id=n.type_id WHERE n.kind='default-value'").fetchone()[0] == 0)
                # A deleted union copy operation may retain a bitwise ABI
                # representation. Its nontrivial destructor independently
                # requires parameter/return storage; preserve EDG's facts.
                check(case + '-nontrivial-original-policy', db.execute('SELECT count(*) FROM source_record_operations WHERE bitwise_construction_allowed=0 OR bitwise_assignment_allowed=0 OR requires_return_storage=1').fetchone()[0] > 0)
            source.rename(source.with_suffix('.retained.cpp'))
            generated = out / (case + '-linked')
            run(case + '-link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', generated, obj,
                *[argument for provider in args.provider for argument in ('--provider', provider.resolve())]])
            run(case + '-bind-runtime', [sys.executable, '-B', args.runtime_binder.resolve(), '--project', generated])
            receipt = json.loads((generated / 'runtime-bindings.json').read_text())
            check(case + '-runtime-fully-bound', receipt['status'] == 'bound' and not receipt['unresolved'])
            run(case + '-build-managed', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '-v:q'])
            run(case + '-run-managed', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
            if args.native_aot:
                aot = out / (case + '-aot')
                run(case + '-publish-aot', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-r', args.rid,
                    '--self-contained', 'true', '-p:PublishAot=true', '-p:StripSymbols=false', '-o', aot, '-v:q'])
                run(case + '-run-aot', [aot / 'Translated'])
            for kind in ('proof', 'source-proof', 'zero-policy', 'missing-metadata', 'foreign-binding', 'wrong-owner', *(('swapped-originals',) if args.two_temporaries else ())):
                damaged = out / (case + '-bad-' + kind + '.dotobj'); shutil.copy2(obj, damaged)
                with sqlite3.connect(damaged) as db:
                    db.execute('PRAGMA ignore_check_constraints=ON')
                    if kind == 'proof':
                        mutation = "UPDATE record_temporary_initializations SET capture_proof='invented'"
                    elif kind == 'source-proof':
                        mutation = "UPDATE source_record_temporaries SET capture_proof='invented'"
                    elif kind == 'zero-policy':
                        mutation = "UPDATE record_temporary_initializations SET zero_storage=1-zero_storage"
                    elif kind == 'missing-metadata':
                        mutation = 'DELETE FROM record_temporary_initializations'
                    elif kind == 'foreign-binding':
                        mutation = "UPDATE record_temporary_initializations SET binding_id=(SELECT id FROM bindings WHERE kind='parameter' LIMIT 1)"
                    elif kind == 'swapped-originals':
                        mutation = "UPDATE record_temporary_initializations SET source_temporary_id=(SELECT max(id)+min(id) FROM source_record_temporaries)-source_temporary_id"
                    else:
                        mutation = "UPDATE source_record_temporaries SET function_entity_id=(SELECT entity_id FROM source_functions WHERE entity_id<>source_record_temporaries.function_entity_id LIMIT 1)"
                    check(case + '-mutated-' + kind, db.execute(mutation).rowcount > 0)
                    db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                run(case + '-reject-' + kind, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(), linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest(),
        providers=[dict(path=str(path.resolve()), sha256=hashlib.sha256(path.read_bytes()).hexdigest()) for path in args.provider])
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
