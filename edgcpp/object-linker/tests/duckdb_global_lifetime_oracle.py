#!/usr/bin/env python3
"""Retain native/stock-C oracles for pending owned global destruction export."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands = [], []
    error = None
    env = dict(os.environ, EDG_BASE=str(Path(__file__).resolve().parents[2] / 'upstream/bases/docker/dev-env/gcc'))

    def run(name, command, expected=None):
        result = subprocess.run(list(map(str, command)), cwd=out, env=env, capture_output=True, timeout=300)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=list(map(str, command)), exitCode=result.returncode))
        if result.returncode or (expected is not None and result.stdout != expected):
            raise AssertionError(name)
        checks.append(name)

    try:
        for case, expected in [('scalar', b'120657'), ('array', b'12065'), ('temporary', b'13204'),
                               ('interleave', b'CMrnldb'), ('partial_array', b'12Bc345MFED'), ('no_effect', b'CM')]:
            source = out / ('duckdb_global_lifetime_' + case + '.cpp')
            shutil.copy2(Path(__file__).with_name(source.name), source)
            for optimization in ('0', '2'):
                binary = out / (case + '-native-O' + optimization)
                run(binary.name + '-build', ['c++', '-std=c++17', '-O' + optimization, source, '-o', binary])
                run(binary.name + '-run', [binary], expected)
            lowered = out / (case + '.c')
            run(case + '-stock-lowering', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining',
                '--exceptions', '--rtti', '--gen_c_file_name', lowered, '-o', out / (case + '.il'), source])
            for optimization in ('0', '2'):
                binary = out / (case + '-stock-O' + optimization)
                run(binary.name + '-build', ['gcc', '-w', '-falign-functions=4', '-O' + optimization,
                    lowered, args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
                run(binary.name + '-run', [binary], expected)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checkCount=len(checks), checks=checks,
        commands=commands, dotobjVerified=False, managedExecutionVerified=False,
        stockCompilerSha256=hashlib.sha256(args.stock_compiler.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
