#!/usr/bin/env python3
"""Verify owned vector allocation protocols and generate C#; runtime pending."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--allocation-only', action='store_true')
    parser.add_argument('--provider', type=Path, action='append', default=[])
    args = parser.parse_args()
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

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)

    error = None
    try:
        run('native-stock-oracles', ['python3', '-B', Path(__file__).with_name('duckdb_vector_allocation_oracle.py').resolve(),
            '--stock-compiler', args.stock_compiler.resolve(), '--stock-runtime', args.stock_runtime.resolve(),
            '--output-dir', out / 'oracles', *(['--allocation-only'] if args.allocation_only else [])])
        fixture = Path(__file__).with_name('duckdb_vector_allocation_protocol.cpp').read_text()
        for name, helper in (('unsized', 'ia64-vector-new2'), ('sized', 'ia64-vector-new3')):
            case = out / name; case.mkdir()
            source = case / 'vector.cpp'
            source.write_text(('#define DUCKDB_VECTOR_SIZED_DELETE\n' if name == 'sized' else '') +
                ('#define DUCKDB_VECTOR_ALLOCATION_ONLY\n' if args.allocation_only else '') + fixture)
            obj = case / 'unit.dotobj'
            run(name + '-export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', case / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
            source.rename(case / 'retained-source.cpp')
            run(name + '-verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                check(name + '-exact-helper', db.execute('SELECT count(*) FROM array_allocation_operations WHERE helper_kind=? AND capture_proof=?',
                    (helper, 'observed-array-new-runtime')).fetchone()[0] == 1)
                check(name + '-original-array-allocation-retained', db.execute('SELECT count(*) FROM array_allocation_operations a JOIN source_allocation_obligations o ON o.id=a.source_allocation_obligation_id WHERE o.is_array=1').fetchone()[0] == 1)
                check(name + '-selected-callbacks-retained', db.execute('SELECT count(*) FROM array_allocation_operations WHERE source_destruction_obligation_id IS NOT NULL AND constructor_entity_id IS NOT NULL AND destructor_entity_id IS NOT NULL AND allocator_entity_id<>deallocator_entity_id').fetchone()[0] == 1)
            run(name + '-generate-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', case / 'generated', '--allow-externs', obj,
                *[item for provider in args.provider for item in ('--provider', provider.resolve())]])
            check(name + '-generated-csharp', any((case / 'generated').glob('*.cs')))
            for negative, sql in (
                ('missing-protocol', 'DELETE FROM array_allocation_operations'),
                ('wrong-delete', 'UPDATE array_allocation_operations SET deallocator_entity_id=allocator_entity_id'),
                ('wrong-constructor', 'UPDATE array_allocation_operations SET constructor_entity_id=destructor_entity_id'),
                ('missing-destructor', 'UPDATE array_allocation_operations SET destructor_entity_id=NULL'),
                ('missing-element-obligation', 'UPDATE array_allocation_operations SET source_destruction_obligation_id=NULL'),
                ('wrong-padding', 'UPDATE array_allocation_operations SET padding_size=padding_size+1'),
                ('wrong-sized-form', "UPDATE array_allocation_operations SET helper_kind=CASE helper_kind WHEN 'ia64-vector-new2' THEN 'ia64-vector-new3' ELSE 'ia64-vector-new2' END"),
                ('wrong-owned-call', "UPDATE array_allocation_operations SET node_id=(SELECT n.id FROM nodes n JOIN entities e ON e.id=n.entity_id WHERE n.kind='call' AND e.source_name='putchar' LIMIT 1)"),
                ('wrong-stride', "UPDATE nodes SET constant_text='99' WHERE id IN (SELECT e.child_id FROM array_allocation_operations a JOIN edges e ON e.parent_id=a.node_id WHERE e.role='argument' AND e.ordinal=1)")):
                damaged = case / (negative + '.dotobj'); shutil.copy2(obj, damaged)
                with sqlite3.connect(damaged) as db:
                    check(name + '-' + negative + '-changed', db.execute(sql).rowcount > 0)
                    check(name + '-' + negative + '-references-valid', not list(db.execute('PRAGMA foreign_key_check')))
                    db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                run(name + '-reject-' + negative, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
                  translatedExecutionVerified=False, allocationOnly=args.allocation_only,
                  exporterGap='Vector delete[] protocol ownership remains pending.' if args.allocation_only else None, runtimeGap='Production __cxa_vec_new2/__cxa_vec_new3 allocation providers remain pending.')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
