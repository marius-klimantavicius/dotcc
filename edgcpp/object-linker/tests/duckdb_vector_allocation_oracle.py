#!/usr/bin/env python3
"""Retain native/stock C vector-new2/new3 callback protocol oracles."""
import argparse
import json
from pathlib import Path
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--allocation-only', action='store_true')
    parser.add_argument('--two-allocations', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'vector.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_vector_allocation_protocol.cpp'), source)
    checks, commands = [], []

    def run(name, command, output=None):
        result = subprocess.run(list(map(str, command)), cwd=out, capture_output=True, timeout=300)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=list(map(str, command)), exitCode=result.returncode))
        if result.returncode != 0: raise AssertionError(name)
        checks.append(name)
        if output is not None:
            if result.stdout != output: raise AssertionError(name + '-events')
            checks.append(name + '-events')

    expected = b'ADA12baDA!A12aD!' + (b'' if args.allocation_only else b'A123cbaD!')
    error = None
    try:
        for name, define, helper in (('unsized', [], '__cxa_vec_new2'),
                                     ('sized', ['-DDUCKDB_VECTOR_SIZED_DELETE'], '__cxa_vec_new3')):
            if args.allocation_only: define.append('-DDUCKDB_VECTOR_ALLOCATION_ONLY')
            if args.two_allocations: define.append('-DDUCKDB_VECTOR_TWO_ALLOCATIONS')
            for optimization in ('0', '2'):
                binary = out / (name + '-native-' + optimization)
                run(name + '-native-build-' + optimization,
                    ['c++', '-std=c++17', '-O' + optimization, *define, source, '-o', binary])
                run(name + '-native-' + optimization, [binary], expected)
            lowered = out / (name + '.c')
            run(name + '-stock-lower', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
                '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
                *define, '--gen_c_file_name', lowered, '-o', out / (name + '.il'), source])
            if helper not in lowered.read_text(): raise AssertionError(name + '-exact-stock-helper')
            checks.append(name + '-exact-stock-helper')
            for optimization in ('0', '2'):
                binary = out / (name + '-stock-' + optimization)
                run(name + '-stock-build-' + optimization, ['gcc', '-w', '-falign-functions=4', '-O' + optimization,
                    lowered, args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
                run(name + '-stock-' + optimization, [binary], expected)
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
                   translatedExecutionVerified=False, allocationOnly=args.allocation_only, twoAllocations=args.two_allocations)
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return int(error is not None)


if __name__ == '__main__':
    raise SystemExit(main())
