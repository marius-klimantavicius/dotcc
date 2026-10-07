#!/usr/bin/env python3
"""Verify native baseline reuse and rejection using a retained small fixture."""
import argparse
import copy
import hashlib
import json
from pathlib import Path
import subprocess
import sys

HARNESS = Path(__file__).with_name('self_bootstrap_verify.py')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--baseline', type=Path, required=True)
    parser.add_argument('--invocation-json', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    baseline = json.loads(args.baseline.read_text())
    compiler = baseline['runs'][0]['hostFiles'][0]['path']
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    command_file = out / 'translated-command.json'
    command_file.write_text(json.dumps([compiler]) + '\n')
    checks = []

    def invoke(name, path, success, reason=''):
        destination = out / name
        command = [sys.executable, str(HARNESS), '--native-baseline', str(path),
                   '--invocation-json', str(args.invocation_json.resolve()),
                   '--translated-command-json', str(command_file), '--native-compiler', compiler,
                   '--output-dir', str(destination)]
        process = subprocess.run(command, capture_output=True, text=True)
        (out / (name + '.stdout')).write_text(process.stdout)
        (out / (name + '.stderr')).write_text(process.stderr)
        result = json.loads((destination / 'results.json').read_text())
        if success:
            assert process.returncode == 0 and result['passed'], name
            assert sum(r.get('reused', False) for r in result['runs']) == len(baseline['runs']), name
            assert len(result['runs']) == len(baseline['runs']) + 1, name
            assert result['runs'][-1]['label'] == 'translated', name
        else:
            assert process.returncode == 2 and not result['passed'], name
            assert reason in result['baselineValidationError'], (name, result)
            assert result['runs'] == [], (name, 'translated process must not run after invalid baseline')
        checks.append({'name': name, 'passed': True, 'command': command})
        (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')

    invoke('valid-reuse', args.baseline.resolve(), True)

    def changed(name, mutate, reason):
        value = copy.deepcopy(baseline)
        mutate(value)
        path = out / (name + '.json'); path.write_text(json.dumps(value, indent=2) + '\n')
        invoke(name, path, False, reason)

    changed('failed-baseline', lambda v: v.update(passed=False), 'passing native-only')
    changed('not-deterministic', lambda v: v.update(nativeDeterministic=False), 'passing native-only')
    changed('normalized-baseline', lambda v: v.update(normalization=['CRLF']), 'unnormalized')
    changed('different-arguments', lambda v: v['invocation']['arguments'].append('-DCHANGED=1'), 'invocation mismatch: arguments')
    changed('different-environment', lambda v: v['invocation']['environment'].update(EDG_DOT_TRACE='1'), 'invocation mismatch: environment')
    changed('different-cwd', lambda v: v['invocation'].update(cwd='/different'), 'invocation mismatch: cwd')
    changed('different-inherited-env', lambda v: v['effectiveEdgEnvironment'].update(TZ='different'), 'controls mismatch')
    changed('one-native-run', lambda v: v.update(runs=v['runs'][:1]), 'at least two')
    changed('different-command', lambda v: v['runs'][0]['command'].append('-DCHANGED=1'), 'command mismatch')
    changed('compiler-hash', lambda v: v['runs'][0]['hostFiles'][0].update(sha256='0' * 64), 'compiler identity/hash')
    changed('stdout-hash', lambda v: v['runs'][0].update(stdoutSha256='0' * 64), 'stdout bytes/hash')
    changed('stdout-size', lambda v: v['runs'][0].update(stdoutBytes=0), 'stdout bytes/hash')
    changed('stderr-hash', lambda v: v['runs'][0].update(stderrSha256='0' * 64), 'stderr bytes/hash')
    changed('stderr-size', lambda v: v['runs'][0].update(stderrBytes=-1), 'stderr bytes/hash')
    changed('failed-native-run', lambda v: v['runs'][0].update(exitCode=1), 'invalid native run')
    altered = out / 'altered.raw.cs'
    altered.write_bytes(Path(baseline['runs'][0]['stdout']).read_bytes() + b'// changed\n')
    changed('retained-bytes-changed', lambda v: v['runs'][0].update(stdout=str(altered)), 'stdout bytes/hash')
    changed('retained-runs-disagree', lambda v: v['runs'][1].update(
        stdout=str(altered), stdoutBytes=altered.stat().st_size,
        stdoutSha256=hashlib.sha256(altered.read_bytes()).hexdigest()), 'not byte-identical')
    (out / 'results.json').write_text(json.dumps({'passed': True, 'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} bootstrap baseline reuse checks passed')


if __name__ == '__main__':
    main()
