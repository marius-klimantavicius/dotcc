#!/usr/bin/env python3
"""Build exact-source native controls using the campaign's portable selection."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from production_support import subprocess

CASES = ("version", "compression-strategies", "empty-frame", "long-distance",
         "streaming-flush-end", "frame-metadata", "ordinary-errors", "single-thread-profile",
         "trained-prepared-dictionary", "raw-dictionary", "context-reuse-reset")


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def cmake_string(value):
    return '"' + str(value).replace('\\', '/').replace('"', '\\"').replace(';', '\\;') + '"'


def build(source: Path, output: Path, *, execute=None):
    """Return the executable. Output is private; upstream references stay intact."""
    source, output = source.resolve(), output.resolve()
    policy = json.loads((ROOT / "config/translation.json").read_text())
    actual = {p.relative_to(source).as_posix() for group in policy['source_groups']
              for p in (source / group).glob('*.c')}
    if actual != set(policy['units']):
        raise RuntimeError("Native source selection differs from translation manifest")
    staged = output / 'source'
    if staged.exists():
        shutil.rmtree(staged)
    shutil.copytree(source / 'lib', staged / 'lib')
    inputs = [staged / name for name in policy['units']]
    inputs += [ROOT / policy['bridge'], ROOT / 'tests/NativeOracle/oracle.c']
    project = output / 'project'
    project.mkdir(parents=True, exist_ok=True)
    (project / 'CMakeLists.txt').write_text('\n'.join([
        'cmake_minimum_required(VERSION 3.19)', 'project(ZstdNativeOracle C)',
        'set(CMAKE_C_STANDARD 11)', 'set(CMAKE_C_STANDARD_REQUIRED ON)',
        'add_executable(native-oracle ' + ' '.join(map(cmake_string, inputs)) + ')',
        'target_include_directories(native-oracle PRIVATE ' + cmake_string(staged / 'lib') + ')',
        'target_compile_definitions(native-oracle PRIVATE ' + ' '.join(policy['defines']) + ')',
        'if(MSVC)', 'target_compile_options(native-oracle PRIVATE /UZSTD_MULTITHREAD)',
        'else()', 'target_compile_options(native-oracle PRIVATE -UZSTD_MULTITHREAD)', 'endif()',
    ]) + '\n')
    output.mkdir(parents=True, exist_ok=True)
    execute = execute or (lambda label, command, **kw: subprocess.run(command, check=True, **kw))
    execute('configure', ['cmake', '-S', str(project), '-B', str(output / 'build'), '-DCMAKE_BUILD_TYPE=Release'], timeout=120)
    execute('build', ['cmake', '--build', str(output / 'build'), '--parallel', str(policy['native_jobs'])], timeout=600)
    binary = output / 'build/native-oracle'
    if os.name == 'nt':
        binary = output / 'build/Release/native-oracle.exe'
        if not binary.is_file():
            binary = output / 'build/native-oracle.exe'
    if not binary.is_file():
        raise RuntimeError(f'Missing native oracle executable: {binary}')
    return binary


def main():
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    results = []

    def execute(label, command, *, expected=0, timeout=120):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, timeout=timeout)
        (output / (label + '.stdout.log')).write_text(result.stdout)
        (output / (label + '.stderr.log')).write_text(result.stderr)
        record = dict(case=label, command=command, exit_code=result.returncode,
                      expected_exit_code=expected, status='pass' if result.returncode == expected else 'fail')
        results.append(record)
        print(f'{label}: {record["status"]}', flush=True)
        if record['status'] != 'pass':
            raise RuntimeError(f'Native {label} failed; see {output / (label + ".stderr.log")}')
        return result

    status, error = 'fail', None
    try:
        binary = build(args.source, output, execute=execute)
        transcript = execute('self-test', [binary, 'self-test'])
        found = [line.removeprefix('CASE PASS ') for line in transcript.stdout.splitlines() if line.startswith('CASE PASS ')]
        if found != list(CASES):
            raise RuntimeError(f'Incomplete native case inventory: {found}')
        results[-1]['passed_cases'] = found
        execute('fail-control', [binary, 'fail-control'], expected=1)
        abi = execute('abi', [binary, 'abi'])
        values = json.loads(abi.stdout)
        if len(values) != 12 or not all(isinstance(x, int) for x in values):
            raise RuntimeError('Incomplete native ABI observations')
        results[-1]['values'] = values
        fixtures = output / 'fixtures'
        fixtures.mkdir(exist_ok=True)
        execute('fixtures', [binary, 'fixtures', fixtures])
        for label, dictionary in [('plain', None), ('dictionary', fixtures / 'native.dict')]:
            frame = fixtures / ('native.zst' if dictionary is None else 'native-dict.zst')
            decoded = fixtures / (label + '.decoded')
            execute(label + '-decode', [binary, 'decompress', frame, decoded, *([dictionary] if dictionary else [])])
            if decoded.read_bytes() != (fixtures / 'payload.bin').read_bytes():
                raise RuntimeError(f'Native {label} fixture roundtrip differs')
        retrained = fixtures / 'retrained.dict'
        execute('train', [binary, 'train', fixtures / 'samples.bin', fixtures / 'sizes.bin', retrained])
        if retrained.read_bytes() != (fixtures / 'native.dict').read_bytes():
            raise RuntimeError('Native dictionary training is not reproducible for the fixed corpus')
        status = 'pass'
    except Exception as caught:
        error = str(caught)
        print(error, file=sys.stderr)
    policy = json.loads((ROOT / 'config/translation.json').read_text())
    report = dict(status=status, source=str(args.source.resolve()), results=results,
                  expected_cases=list(CASES), error=error,
                  configuration=policy,
                  source_sha256={name: digest(args.source / name) for name in policy['units']},
                  harness_sha256={str(p.relative_to(ROOT)): digest(p) for p in
                                  (ROOT / policy['bridge'], ROOT / 'tests/NativeOracle/oracle.c', Path(__file__))})
    (output / 'native-report.json').write_text(json.dumps(report, indent=2) + '\n')
    return 0 if status == 'pass' else 1


if __name__ == '__main__':
    raise SystemExit(main())
