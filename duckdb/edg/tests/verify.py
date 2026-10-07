#!/usr/bin/env python3
"""Run DuckDB's upstream test runner in native, translated JIT and AOT builds.

This orchestrates the upstream SQLLogicTest/C++ assertions; it is not a SQL or
SQLLogicTest interpreter. Each executable must include reporter.cpp.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import time

HERE = Path(__file__).resolve().parent
PREFIX = 'EDG_DUCKDB_RECEIPT '
COUNTS = ('assertionsPassed', 'assertionsFailed', 'assertionsExpectedFailure',
          'casesPassed', 'casesFailed', 'casesExpectedFailure', 'skipped')


def digest(path):
    with Path(path).open('rb') as source:
        return hashlib.file_digest(source, 'sha256').hexdigest()


def observations(text):
    records = [json.loads(line[len(PREFIX):]) for line in text.splitlines() if line.startswith(PREFIX)]
    if not records or records[-1].get('kind') != 'run':
        raise ValueError('Missing completed upstream runner receipt')
    if sum(item.get('kind') == 'run' for item in records) != 1:
        raise ValueError('Expected exactly one run footer')
    cases = records[:-1]
    if not cases:
        raise ValueError('No upstream test cases executed')
    if any(item.get('kind') != 'case' for item in cases):
        raise ValueError('Unexpected reporter event')
    names = [item.get('name') for item in cases]
    if any(not isinstance(name, str) or not name for name in names) or len(names) != len(set(names)):
        raise ValueError('Missing or duplicate upstream test identity')
    for item in records:
        if item.get('aborting') is not False:
            raise ValueError('Upstream runner aborted')
        for key in COUNTS:
            if type(item.get(key)) is not int or item[key] < 0:
                raise ValueError('Invalid reporter counter: ' + key)
        for key in ('assertionsFailed', 'assertionsExpectedFailure', 'casesFailed',
                    'casesExpectedFailure', 'skipped'):
            if item[key]:
                raise ValueError('Upstream case/run did not fully pass: ' + key)
        if item['assertionsPassed'] == 0 or item['casesPassed'] == 0:
            raise ValueError('Empty upstream case/run is not a pass')
    if any(item['casesPassed'] != 1 for item in cases):
        raise ValueError('Case record must describe exactly one case')
    for key in COUNTS:
        if sum(item[key] for item in cases) != records[-1][key]:
            raise ValueError('Run totals do not match case records: ' + key)
    return sorted(cases, key=lambda item: item['name'])


def snapshot(source):
    """Detect all source-tree changes, including added root-level test outputs."""
    paths = {path for path in source.rglob('*') if path.is_file() or path.is_symlink()}
    return [dict(path=str(path.relative_to(source)), **(
        {'target': str(path.readlink())} if path.is_symlink() else {'sha256': digest(path)}))
        for path in sorted(paths)]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', required=True, type=Path, help='Pristine pinned DuckDB source tree')
    parser.add_argument('--runners', required=True, type=Path,
                        help='JSON object mapping native/jit/aot to command argument arrays')
    parser.add_argument('--suite', type=Path, default=HERE / 'initial-suite.json')
    parser.add_argument('--provenance', required=True, action='append', type=Path,
                        help='Translation/build/source receipt to hash; repeat as needed')
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--timeout', type=int, default=1800, help='Seconds per selected group and runtime')
    parser.add_argument('--baseline-only', action='store_true', help='Native only; never a translation pass')
    args = parser.parse_args()
    source = args.source.resolve(strict=True)
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'source': str(source), 'commands': [], 'checks': [], 'inputs': [],
              'scope': 'Selected unchanged upstream test cases, checked by the upstream runner',
              'limitations': ['Not the complete DuckDB test suite unless the supplied selection covers it.',
                             'Passing means upstream assertions passed; it is not a raw result byte comparison.',
                             'Binary and supplied provenance hashes do not themselves audit the full dependency closure.']}

    def save():
        (output / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def record(path):
        path = path.resolve(strict=True)
        report['inputs'].append({'path': str(path), 'sha256': digest(path)})

    try:
        runners = json.loads(args.runners.read_text())
        expected_modes = {'native'} if args.baseline_only else {'native', 'jit', 'aot'}
        if not isinstance(runners, dict) or set(runners) != expected_modes:
            raise ValueError('Expected exactly these runner modes: ' + ', '.join(sorted(expected_modes)))
        for mode, command in runners.items():
            if not isinstance(command, list) or not command or any(not isinstance(arg, str) for arg in command):
                raise ValueError('Each runner command must be a nonempty string argument array')
            executable = shutil.which(command[0])
            if not executable:
                raise ValueError('Runner executable not found: ' + command[0])
            command[0] = str(Path(executable).resolve())
            record(Path(command[0]))
            for index, argument in enumerate(command[1:], 1):
                candidate = Path(argument)
                if candidate.is_file():
                    command[index] = str(candidate.resolve())
                    record(candidate)
        suite = json.loads(args.suite.read_text())
        groups = suite['groups']
        if not groups or len({group['name'] for group in groups}) != len(groups):
            raise ValueError('Suite groups must be nonempty and uniquely named')
        for group in groups:
            if not group['selectors'] or any(not isinstance(x, str) or not x for x in group['selectors']):
                raise ValueError('Each group requires nonempty Catch selectors')
        for path in [Path(__file__), HERE / 'reporter.cpp', args.suite, args.runners, *args.provenance]:
            record(path)
        inputs = snapshot(source)
        (output / 'source-inputs.json').write_text(json.dumps(inputs, indent=2) + '\n')
        report['sourceInputsSha256'] = digest(output / 'source-inputs.json')
        report['sourceInputCount'] = len(inputs)
        report['suite'] = suite
        # Do not inherit configurations that silently skip tests, suppress error
        # matching, install extensions, or change test data/connection settings.
        env = {key: value for key, value in os.environ.items()
               if not key.startswith('DUCKDB_TEST_') and key not in ('LOCAL_EXTENSION_REPO', 'SUMMARIZE_FAILURES')}
        env.update(DUCKDB_TEST_AUTOLOADING='none', DUCKDB_TEST_MAX_THREADS='4',
                   DUCKDB_TEST_MAX_TEST_THREADS='4', SUMMARIZE_FAILURES='1', DOTNET_PROCESSOR_COUNT='4')
        report['controlledEnvironment'] = {key: env[key] for key in (
            'DUCKDB_TEST_AUTOLOADING', 'DUCKDB_TEST_MAX_THREADS', 'DUCKDB_TEST_MAX_TEST_THREADS',
            'SUMMARIZE_FAILURES', 'DOTNET_PROCESSOR_COUNT')}
        modes = ['native'] if args.baseline_only else ['native', 'jit', 'aot']
        for group_index, group in enumerate(groups):
            expected = None
            for mode in modes:
                label = str(group_index).zfill(3) + '-' + mode
                temp = output / (label + '-data')
                # Upstream caches TEMP_DIR before parsing --test-temp-dir and
                # refreshes it only when --test-dir changes the working dir.
                # Start elsewhere so both its initial default directory and
                # the configured test outputs stay outside the source tree.
                workdir = output / (label + '-work')
                workdir.mkdir()
                command = [*runners[mode], *group['selectors'], '--reporter', 'edg-json',
                           '--use-colour', 'no', '--order', 'lex', '--rng-seed', '1729',
                           '--test-dir', str(source), '--test-temp-dir', str(temp)]
                entry = {'group': group['name'], 'mode': mode, 'command': command, 'cwd': str(workdir),
                         'stdout': label + '.stdout', 'stderr': label + '.stderr'}
                report['commands'].append(entry)
                save()
                start = time.monotonic()
                with (output / entry['stdout']).open('wb') as stdout, (output / entry['stderr']).open('wb') as stderr:
                    try:
                        result = subprocess.run(command, cwd=workdir, env=env, stdout=stdout, stderr=stderr,
                                                timeout=args.timeout)
                    except subprocess.TimeoutExpired:
                        entry.update(timedOut=True, seconds=round(time.monotonic() - start, 3))
                        raise
                entry.update(exitCode=result.returncode, seconds=round(time.monotonic() - start, 3))
                save()
                if result.returncode:
                    raise RuntimeError(f'{group["name"]}/{mode} exited {result.returncode}; see retained logs')
                actual = observations((output / entry['stdout']).read_text())
                (output / (label + '.cases.json')).write_text(json.dumps(actual, indent=2) + '\n')
                if expected is None:
                    expected = actual
                elif actual != expected:
                    raise RuntimeError(f'{group["name"]}/{mode}: case identities or assertion totals differ from native')
                report['checks'].append({'group': group['name'], 'mode': mode, 'cases': len(actual),
                                        'assertions': sum(item['assertionsPassed'] for item in actual)})
                print(f'{group["name"]}/{mode}: {len(actual)} upstream cases passed', flush=True)
                save()
        for item in report['inputs']:
            if digest(item['path']) != item['sha256']:
                raise RuntimeError('Verification input changed: ' + item['path'])
        if snapshot(source) != inputs:
            raise RuntimeError('Upstream test sources or data changed during verification')
        report['status'] = 'native-baseline-passed' if args.baseline_only else 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
