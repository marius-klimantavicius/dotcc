#!/usr/bin/env python3
"""Round-trip EDG graph data without repeating its source-language analysis."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, required=True, type=Path)
    parser.add_argument('--projection-only', action='store_true',
        help='Validate semantic-check removal with existing projections; does not accept complete raw graph capture')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    checks, commands, summaries = [], [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(directory, name, command, success=True, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=directory, env=env, capture_output=True, timeout=600)
        (directory / (name + '.stdout')).write_bytes(result.stdout)
        (directory / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, directory=str(directory), command=command, exitCode=result.returncode))
        check(directory.name + ':' + name, (result.returncode == 0) == success)

    error = None
    try:
        for fixture in ('duckdb_source_member_substitutions', 'duckdb_source_implicit_this', 'duckdb_source_member_names'):
            work = out / fixture
            work.mkdir()
            source = work / 'source.cpp'
            shutil.copy2(Path(__file__).with_name(fixture + '.cpp'), source)
            run(work, 'native-build', ['c++', '-std=c++20', '-O2', source, '-o', work / 'native'])
            run(work, 'native', [work / 'native'])
            run(work, 'stock-lower', [args.stock_compiler.resolve(), '--c++20', '--instantiate', 'used',
                '--no_inlining', '--exceptions', '--clear_flag', 'use_predefined_macro_file',
                '--gen_c_file_name', work / 'stock.c', '-o', work / 'stock.il', source],
                env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ['PATH']))
            run(work, 'stock-build', ['gcc', '-w', '-falign-functions=4', '-O2', work / 'stock.c',
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', work / 'stock'])
            run(work, 'stock', [work / 'stock'])
            obj = work / 'unit.dotobj'
            run(work, 'export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
                '--work-dir', work / 'export', '--compiler', args.compiler.resolve(), '--std', 'c++20', '--exceptions'])
            source.rename(work / 'retained-source.cpp')
            run(work, 'verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                scalar = lambda sql: db.execute(sql).fetchone()[0]
                nodes = scalar('SELECT count(*) FROM edg_graph_nodes')
                fields = scalar('SELECT count(*) FROM edg_graph_fields')
                if not args.projection_only:
                    check(fixture + ':typed-graph-retained', nodes > 0 and fields > nodes)
                check(fixture + ':configured-field-schema-retained', scalar("SELECT count(*) FROM edg_graph_coverage WHERE field='$configuration' AND disposition='configuration'") == 1)
                if not args.projection_only:
                    check(fixture + ':shared-nodes-retained', scalar('SELECT count(*) FROM (SELECT target_id FROM edg_graph_fields WHERE target_id IS NOT NULL GROUP BY target_id HAVING count(*)>1)') > 0)
                check(fixture + ':source-expressions-retained', scalar('SELECT count(*) FROM source_expressions') > 0)
                if fixture.endswith('member_substitutions'):
                    check(fixture + ':selected-argument-occurrences-retained', scalar('SELECT count(*) FROM source_routine_borrowed_arguments') > 0)
                if fixture.endswith('implicit_this'):
                    check(fixture + ':raw-implicit-this-number', scalar('SELECT count(*) FROM source_expression_parameter_references WHERE parameter_number=0') > 0)
                summaries.append(dict(fixture=fixture, nodes=nodes, fields=fields,
                    coverageGaps=list(db.execute("SELECT entry_kind,field,detail FROM edg_graph_coverage WHERE disposition='gap'"))))
            run(work, 'link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', work / 'generated', obj])
            run(work, 'build', ['dotnet', 'build', work / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
            run(work, 'jit', ['dotnet', work / 'generated/bin/Release/net10.0/Translated.dll'])
            # Corrupt only serialization structure. A reader is not asked to
            # decide whether a different, well-formed C++ recipe is valid.
            corruptions = [
                ('dangling-typed-edge', "UPDATE edg_graph_fields SET target_id=9223372036854775807 WHERE rowid=(SELECT rowid FROM edg_graph_fields WHERE target_id IS NOT NULL LIMIT 1)"),
                ('invalid-integer-encoding', "UPDATE edg_graph_fields SET value='01' WHERE rowid=(SELECT rowid FROM edg_graph_fields WHERE value_kind='unsigned' LIMIT 1)"),
                ('missing-configuration', "DELETE FROM edg_graph_coverage WHERE field='$configuration'"),
            ]
            if args.projection_only:
                corruptions = [('dangling-source-edge', 'UPDATE source_expression_edges SET child_expression_id=9223372036854775807 WHERE rowid=(SELECT rowid FROM source_expression_edges LIMIT 1)'), corruptions[-1]]
            for name, sql in corruptions:
                damaged = work / (name + '.dotobj')
                shutil.copy2(obj, damaged)
                with sqlite3.connect(damaged) as db:
                    db.execute('PRAGMA ignore_check_constraints=ON')
                    db.execute(sql)
                    check(fixture + ':' + name + '-changed', db.total_changes > 0)
                    db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
                run(work, name, ['dotnet', args.linker.resolve(), 'verify', damaged], success=False)
    except Exception as ex:
        error = str(ex)
    complete = not args.projection_only and not error and all(not row['coverageGaps'] for row in summaries)
    result = dict(status='failed' if error else 'passed-projections-only' if args.projection_only else 'passed-graph-integration',
        completeRawGraphAccepted=complete, error=error, checks=checks, commands=commands, graphSummaries=summaries)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
