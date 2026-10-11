#!/usr/bin/env python3
"""Compare resident and bounded catalog generation of an existing executable target."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--driver', type=Path, required=True)
    parser.add_argument('--inputs', type=Path, required=True, help='JSON array of {path,provider}')
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--entry', default='main')
    parser.add_argument('--expected-exit', type=int, default=0)
    parser.add_argument('--support', type=Path, action='append', default=[], help='Existing runtime/host C# source to copy into both outputs')
    parser.add_argument('--bind-runtime', action='store_true', help='Bind each project with the existing runtime/bind.py')
    expected = parser.add_mutually_exclusive_group()
    expected.add_argument('--expected-output', help='Exact UTF-8 standard output')
    expected.add_argument('--expected-output-file', type=Path, help='File containing exact expected standard output bytes')
    parser.add_argument('--timeout', type=int, default=1800)
    args = parser.parse_args()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    inputs = json.loads(args.inputs.read_text())
    checks, commands = [], []
    error = None

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command):
        result = subprocess.run(command, capture_output=True, timeout=args.timeout,
                                env=dict(os.environ, DOTNET_GCHeapHardLimit='0x80000000'))
        (output / (name + '.stdout')).write_bytes(result.stdout)
        (output / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        return result

    def objects(path):
        return {item['unitId']: item for item in json.loads(path.read_text())['objects']}

    try:
        check('nonempty-inputs', bool(inputs))
        operands = []
        for item in inputs:
            item['path'] = str(Path(item['path']).resolve())
            if item['provider']:
                operands.append('--provider')
            operands.append(item['path'])
        driver = ['dotnet', str(args.driver.resolve())]
        catalog = output / 'catalog.sqlite'
        imported = run('import-resolve', [*driver, 'catalog', '--resolve', '--allow-externs',
                       '--audit-object-lifetimes', '--out', str(catalog), *operands])
        check('catalog-import-and-resolution', imported.returncode == 0)
        resident, bounded = output / 'resident', output / 'bounded'
        linked = run('resident-generate', [*driver, 'link', '--allow-externs', '--entry', args.entry,
                     '--out', str(resident), *operands])
        check('resident-generation', linked.returncode == 0)
        generated = run('catalog-generate', [*driver, 'catalog-generate', '--audit-object-lifetimes',
                        '--entry', args.entry, '--out', str(bounded), str(catalog)])
        check('catalog-generation-and-release-audit', generated.returncode == 0)
        events = [json.loads(line) for line in generated.stdout.splitlines() if line.startswith(b'{')]
        check('each-input-reopened-and-released', len(events) == len(inputs) and events[-1]['units'] == len(inputs))
        with sqlite3.connect(catalog) as db:
            check('generation-published-only-on-success', db.execute('SELECT status FROM catalog_info').fetchone() == ('generated',))
            check('catalog-integrity', db.execute('PRAGMA integrity_check').fetchone() == ('ok',))
            check('catalog-references', not db.execute('PRAGMA foreign_key_check').fetchall())
        left = json.loads((resident / 'link-report.json').read_text())
        right = json.loads((bounded / 'link-report.json').read_text())
        check('same-provider-and-storage-selection', json.loads((resident / 'link-selection.json').read_text()) ==
              json.loads((bounded / 'link-selection.json').read_text()))
        for field in ('capabilities', 'runtimeHooks', 'sourceExceptionRuntime'):
            check('same-' + field, left[field] == right[field])
        for field, keys in [('symbols', ('unitId', 'entityId', 'generatedName', 'signature')),
                            ('records', ('unitId', 'typeId', 'size', 'alignment')),
                            ('objects', ('path', 'unitId', 'semanticDigest')),
                            ('runtimeRequirements', ('ExternalKey', 'Signature')),
                            ('runtimeStorageRequirements', ('ExternalKey', 'size', 'alignment'))]:
            check('same-' + field, {tuple(item[key] for key in keys) for item in left[field]} ==
                  {tuple(item[key] for key in keys) for item in right[field]})
        for name in ('source-map.json', 'macro-map.json'):
            check('same-complete-' + name, objects(resident / name) == objects(bounded / name))
        source_objects=objects(bounded / 'source-map.json')
        for index,item in enumerate(inputs):
            path=Path(item['path'])
            with sqlite3.connect(path) as db:
                unit=db.execute('SELECT unit_id FROM object_info').fetchone()[0]
                field_count=db.execute('SELECT coalesce(sum(row_count),0) FROM edg_graph_field_blocks').fetchone()[0]
            graph=source_objects[unit]['edgGraph']
            with path.open('rb') as stream:digest=hashlib.file_digest(stream,'sha256').hexdigest()
            check(f'graph-{index}-exact-retained-object-receipt',graph['path']==str(path) and graph['sha256']==digest and graph['byteLength']==path.stat().st_size)
            check(f'graph-{index}-typed-row-count',graph['counts']['fields']==field_count and graph['fieldRows']['encoding']=='graph-fields-v1')
            check(f'graph-{index}-no-expanded-duplication','edgGraphFields' not in source_objects[unit])
        def lifetimes(path):
            return {key: value for key, value in objects(path).items() if value['functions']}
        check('same-recorded-lifetimes', lifetimes(resident / 'lifetime-map.json') == lifetimes(bounded / 'lifetime-map.json'))
        executions = []
        for name, directory in [('resident', resident), ('bounded', bounded)]:
            for source in args.support:
                target = directory / source.name
                check(name + '-support-does-not-overwrite-' + source.name, not target.exists())
                shutil.copy2(source, target)
            if args.bind_runtime:
                bound = run(name + '-bind-runtime', [sys.executable, str(Path(__file__).resolve().parents[1] / 'runtime' / 'bind.py'),
                            '--project', str(directory)])
                check(name + '-runtime-binding', bound.returncode == 0)
            built = run(name + '-build', ['dotnet', 'build', str(directory / 'Translated.csproj'),
                        '-c', 'Release', '-o', str(directory / 'run'), '--nologo', '-v:q'])
            check(name + '-generated-csharp-compiles', built.returncode == 0)
            executed = run(name + '-run', ['dotnet', str(directory / 'run' / 'Translated.dll')])
            check(name + '-expected-execution', executed.returncode == args.expected_exit)
            expected_output = args.expected_output_file.read_bytes() if args.expected_output_file else (
                args.expected_output.encode('utf-8') if args.expected_output is not None else None)
            if expected_output is not None:
                check(name + '-expected-trace', executed.stdout == expected_output)
            executions.append((executed.returncode, executed.stdout, executed.stderr))
        check('identical-executable-behavior', executions[0] == executions[1])
    except Exception as exc:
        error = repr(exc)
    (output / 'results.json').write_text(json.dumps(dict(status='failed' if error else 'passed', error=error,
                                                       checks=checks, commands=commands), indent=2) + '\n')
    if error:
        raise RuntimeError(error)
    print(f'PASS {len(checks)} catalog generation checks')


if __name__ == '__main__':
    main()
