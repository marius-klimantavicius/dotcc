#!/usr/bin/env python3
"""Validate skip/failure/empty-run detection using pinned upstream Catch.

This tests only the harness. It cannot produce a DuckDB translation pass.
"""
import argparse
import json
from pathlib import Path
import subprocess

from verify import observations, digest, HERE


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--catch-include', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--cxx', default='g++')
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {'scope': 'Harness controls only; no DuckDB engine or translation acceptance', 'status': 'running',
              'inputs': {str(path): digest(path) for path in [HERE / 'reporter.cpp',
                  HERE / 'reporter_controls.cpp', HERE / 'verify.py', args.catch_include / 'catch.hpp']}, 'checks': []}
    try:
        binary = output / 'reporter-controls'
        command = [args.cxx, '-std=c++20', '-O0', '-pthread', '-I', str(args.catch_include),
                   str(HERE / 'reporter_controls.cpp'), str(HERE / 'reporter.cpp'), '-o', str(binary)]
        report['compileCommand'] = command
        with (output / 'compile.stdout').open('wb') as stdout, (output / 'compile.stderr').open('wb') as stderr:
            subprocess.run(command, check=True, stdout=stdout, stderr=stderr)
        for index, (selector, should_pass) in enumerate([
                ('control pass*', True), ('control skip', False), ('control failure', False),
                ('control expected failure', False), ('control empty', False), ('missing control', False)]):
            command = [str(binary), selector, '--reporter', 'edg-json', '--use-colour', 'no']
            result = subprocess.run(command, capture_output=True, text=True, timeout=60)
            (output / f'{index}.stdout').write_text(result.stdout)
            (output / f'{index}.stderr').write_text(result.stderr)
            try:
                actual = observations(result.stdout)
            except ValueError as error:
                if should_pass:
                    raise
                report['checks'].append({'selector': selector, 'rejected': str(error), 'exitCode': result.returncode})
            else:
                if not should_pass:
                    raise AssertionError('Invalid control accepted: ' + selector)
                assert result.returncode == 0 and actual[0]['assertionsPassed'] == 4
                assert actual[0]['name'] == 'control pass with "quotes" and backslash \\'
                report['checks'].append({'selector': selector, 'accepted': actual, 'exitCode': result.returncode})
        report['status'] = 'harness-controls-passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        (output / 'results.json').write_text(json.dumps(report, indent=2) + '\n')


if __name__ == '__main__':
    main()
