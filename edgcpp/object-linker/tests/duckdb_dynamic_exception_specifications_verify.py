#!/usr/bin/env python3
"""Verify legacy throw-spec source metadata and generated unexpected boundaries."""
import argparse
import importlib.util
import json
import os
import re
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--runtime-binder', type=Path, default=Path(__file__).resolve().parents[1] / 'runtime/bind.py')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'duckdb_dynamic_exception_specifications.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, condition):
        if not condition: raise AssertionError(name)
        checks.append(name)

    def run(name, command, expected=0, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode, expected=expected))
        check(name, result.returncode == expected if expected is not None else result.returncode != 0)

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++14', '-fno-inline', '-O' + opt, source, '-o', binary])
            run('native-allowed-' + opt, [binary])
            run('native-unexpected-' + opt, [binary, 'violate'], expected=73)
        run('stock-lower', [args.stock_compiler.resolve(), '--c++14', '--instantiate', 'used', '--no_inlining', '--exceptions',
            '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source],
            env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ.get('PATH', '')))
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, out / 'stock.c', args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-allowed-' + opt, [binary])
            run('stock-unexpected-' + opt, [binary, 'violate'], expected=73)
        obj = out / 'unit.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', source, '--output', obj, '--work-dir', out / 'work',
            '--compiler', args.compiler.resolve(), '--std', 'c++14', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('empty-original-throw-list', db.execute('SELECT count(*) FROM source_dynamic_exception_specifications WHERE allowed_type_count=0').fetchone()[0] > 0)
            check('ordered-nonempty-original-list', db.execute('SELECT count(*) FROM source_dynamic_exception_specifications WHERE allowed_type_count=2').fetchone()[0] > 0)
            empty = db.execute('SELECT source_routine_type_id FROM source_dynamic_exception_specifications WHERE allowed_type_count=0 LIMIT 1').fetchone()[0]
            typed = db.execute('SELECT source_routine_type_id FROM source_dynamic_exception_specifications WHERE allowed_type_count=2 LIMIT 1').fetchone()[0]
            boundary = db.execute('SELECT function_entity_id FROM function_dynamic_exception_boundaries WHERE source_routine_type_id=?', (typed,)).fetchone()[0]
            check('original-list-order-and-types', [row[0] for row in db.execute('SELECT t.kind FROM source_dynamic_exception_types e JOIN source_types t ON t.id=e.declared_source_type_id WHERE e.source_routine_type_id=? ORDER BY e.ordinal', (typed,))] == ['int', 'record'])
            check('dynamic-policy-distinct-from-noexcept', db.execute("SELECT count(*) FROM function_dynamic_exception_boundaries b JOIN entities e ON e.id=b.function_entity_id JOIN routine_types t ON t.type_id=e.routine_type_id WHERE t.exception_policy<>'dynamic'").fetchone()[0] == 0)
            check('all-boundaries-retain-original-function', db.execute('SELECT count(*) FROM function_dynamic_exception_boundaries b JOIN source_functions f ON f.entity_id=b.function_entity_id').fetchone()[0] >= 2)
            check('foreign-keys-valid', not list(db.execute('PRAGMA foreign_key_check')))
        generated = out / 'generated'
        run('link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', generated, obj,
            *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
        report = json.loads((generated / 'link-report.json').read_text())
        check('unexpected-provider-explicitly-unbound', report['sourceExceptionRuntime']['dynamicSpecificationsRequired'] and
              report['sourceExceptionRuntime']['dynamicSpecificationsStatus'] == 'generation-only-unbound-unexpected-provider')
        code = '\n'.join(path.read_text() for path in generated.glob('Translated*.cs'))
        check('violation-provider-call-emitted', 'HandleSpecificationViolation(' in code)
        empty_method = re.search(r'\bvoid\s+@?dynamic_empty_throw_specification\(\)\s*\{([^}]*)\}', code)
        check('empty-dynamic-body-needs-no-wrapper', empty_method is not None and
              'try' not in empty_method.group(1) and 'catch' not in empty_method.group(1))
        with sqlite3.connect(obj) as db:
            check('empty-dynamic-body-retains-source-boundary', db.execute("SELECT count(*) FROM function_dynamic_exception_boundaries b JOIN entities e ON e.id=b.function_entity_id WHERE e.source_name='dynamic_empty_throw_specification'").fetchone()[0] == 1)

        check('typed-allowed-filter-emitted', '.FilterTypedCatch(' in code)
        check('control-transfer-filter-retained', 'ObjectProgram.IsRuntimeTransfer(' in code)
        check('provider-contract-not-noop', 'public static partial global::System.Exception HandleSpecificationViolation(' in
              (generated / 'Edg.Runtime.CppDynamicSpecifications.generated.cs').read_text())
        run('binder-rejects-missing-production-provider', [sys.executable, '-B', args.runtime_binder.resolve(), '--project', generated], expected=None)
        receipt = json.loads((generated / 'runtime-bindings.json').read_text())
        check('binder-identifies-unexpected-provider', any(row['name'] == 'Edg.Runtime.CppExceptions.HandleSpecificationViolation' for row in receipt['unresolved']))
        # A modern-mode legacy empty throw() retains a different execution
        # boundary from its compatible noexcept declaration.
        legacy = out / 'legacy-empty.cpp'
        legacy.write_text('struct Error { ~Error() noexcept; };\nError::~Error() throw() {}\nint main(){Error value;return 0;}\n')
        legacy_obj = out / 'legacy-empty.dotobj'
        run('modern-empty-export', [sys.executable, '-B', tooling / 'compile.py', legacy, '--output', legacy_obj, '--work-dir', out / 'legacy-work',
            '--compiler', args.compiler.resolve(), '--std', 'c++20', '--exceptions'])
        legacy.rename(out / 'legacy-empty.retained.cpp')
        run('modern-empty-verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', legacy_obj])
        with sqlite3.connect(legacy_obj) as db:
            check('modern-empty-preserves-legacy-boundary', db.execute('SELECT count(*) FROM function_dynamic_exception_boundaries').fetchone()[0] > 0)
        importer = out / 'legacy-import.cpp'
        importer.write_text('struct Error { ~Error() noexcept; };\nvoid destroy_imported(Error* p){p->~Error();}\n')
        imported_obj = out / 'legacy-import.dotobj'
        run('modern-noexcept-import-export', [sys.executable, '-B', tooling / 'compile.py', importer, '--output', imported_obj, '--work-dir', out / 'legacy-import-work',
            '--compiler', args.compiler.resolve(), '--std', 'c++20', '--exceptions'])
        importer.rename(out / 'legacy-import.retained.cpp')
        run('modern-noexcept-import-verify', ['dotnet', args.linker.resolve(), 'verify', imported_obj])
        compatible = out / 'generated-compatible-legacy'
        run('empty-throw-noexcept-callable-compatibility', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', compatible, legacy_obj, imported_obj,
            *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
        check('compatible-callable-preserves-dynamic-boundary-metadata', json.loads((compatible / 'link-report.json').read_text())['sourceExceptionRuntime']['dynamicSpecificationsRequired'])
        negatives = [
            ('missing-boundary', [f'DELETE FROM function_dynamic_exception_boundaries WHERE function_entity_id={boundary}']),
            ('false-boundary-owner', [f'UPDATE function_dynamic_exception_boundaries SET source_routine_type_id={empty} WHERE function_entity_id={boundary}']),
            ('false-boundary-proof', [f"UPDATE function_dynamic_exception_boundaries SET capture_proof='invented' WHERE function_entity_id={boundary}"]),
            ('lost-last-allowed-type', [f'DELETE FROM source_dynamic_exception_types WHERE source_routine_type_id={typed} AND ordinal=1']),
            ('allowed-type-order-gap', [f'UPDATE source_dynamic_exception_types SET ordinal=ordinal+99 WHERE source_routine_type_id={typed}']),
            ('false-type-count', [f'UPDATE source_dynamic_exception_specifications SET allowed_type_count=0 WHERE source_routine_type_id={typed}']),
            ('false-source-kind', [f"UPDATE source_dynamic_exception_specifications SET kind='noexcept' WHERE source_routine_type_id={typed}"]),
            ('false-source-proof', [f"UPDATE source_dynamic_exception_specifications SET capture_proof='invented' WHERE source_routine_type_id={typed}"]),
            ('false-match-type', [f'UPDATE source_dynamic_exception_types SET match_source_type_id=(SELECT declared_source_type_id FROM source_dynamic_exception_types WHERE source_routine_type_id={typed} AND ordinal=0) WHERE source_routine_type_id={typed} AND ordinal=1']),
            ('noexcept-downgrade', [f"UPDATE source_routine_types SET exception_policy='terminate' WHERE type_id={typed}"]),
        ]
        for name, statements in negatives:
            damaged = out / ('reject-' + name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                db.execute('PRAGMA ignore_check_constraints=ON')
                check('reject-' + name + '-changed', sum(db.execute(sql).rowcount for sql in statements) > 0)
                check('reject-' + name + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], expected=None)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        translatedExecutionVerified=False, runtimeGap='The shared std::unexpected/replacement provider is explicitly required and not implemented by this generation milestone')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
