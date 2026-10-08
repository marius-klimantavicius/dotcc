#!/usr/bin/env python3
"""Original DuckDB yyjson/Mbed TLS sources: native oracle and owned C# parity."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
DEPENDENCIES = {
    'yyjson': ['third_party/yyjson/yyjson.cpp'],
    'mbedtls': ['third_party/mbedtls/library/sha256.cpp', 'third_party/mbedtls/library/platform_util.cpp'],
}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def verify_sha_output(output):
    rows = output.decode().splitlines()
    expected = []
    for pattern in range(3):
        for size in (0, 1, 55, 56, 63, 64, 65, 127, 128, 129, 4096, 65536):
            state = 0x12345678
            data = bytearray()
            for index in range(size):
                state = (1664525 * state + 1013904223) & 0xffffffff
                data.append(state >> 24 if pattern == 0 else index % 7 if pattern == 1 else ord('a'))
            expected.append(f'{pattern} {size} {hashlib.sha256(data).hexdigest()}')
    if rows != expected:
        raise AssertionError('SHA-256 output differs from independent hashlib reference')


def verify_json_output(output):
    rows = output.decode().splitlines()
    if len(rows) != 42 or rows[-1] != 'extensions and mutation passed':
        raise AssertionError('Incomplete JSON corpus')
    for index in range(12):
        first, second = rows[index * 2:index * 2 + 2]
        if not first.startswith(f'valid {index} 0 ') or not second.startswith(f'valid {index} 1 '):
            raise AssertionError('Missing JSON in-situ or ordinary parse')
        left, right = first.split(' ', 3)[3], second.split(' ', 3)[3]
        if left != right:
            raise AssertionError('JSON in-situ output mismatch')
        json.loads(left)
    for index in range(16):
        fields = rows[24 + index].split()
        if fields[:2] != ['invalid', str(index)] or int(fields[2]) == 0:
            raise AssertionError('Missing JSON error case')
    mutated = json.loads(rows[-2].removeprefix('mutated '))
    if mutated != dict(text='žąsis 🦆', largest=2**64 - 1, missing=None, values=list(range(-512, 512))):
        raise AssertionError('Mutable JSON document differs from reference')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--manifest', type=Path, required=True)
    parser.add_argument('--fixture', choices=tuple(DEPENDENCIES), required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--project-driver', type=Path, default=ROOT / 'project.py')
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--allow-export-rejection', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands, inputs, pending = [], [], [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, allow_failure=False):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=1800)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        if not allow_failure or result.returncode == 0:
            check(name, result.returncode == 0)
        return result

    failure = None
    try:
        manifest = json.loads(args.manifest.read_text())
        units = []
        for relative in DEPENDENCIES[args.fixture]:
            matches = [unit for unit in manifest['units'] if unit['source'].endswith('/' + relative)]
            check('original-source-' + Path(relative).stem, len(matches) == 1)
            unit = matches[0]
            units.append(unit)
            inputs.append(dict(path=unit['source'], sha256=sha(unit['source'])))
        root = Path(units[0]['source'])
        for part in Path(DEPENDENCIES[args.fixture][0]).parts:
            root = root.parent
        (out / 'inputs').mkdir()
        fixture = out / 'inputs' / ('duckdb_' + args.fixture + '.cpp')
        shutil.copy2(ROOT / 'tests' / fixture.name, fixture)
        inputs.append(dict(path=str(fixture), sha256=sha(fixture)))
        arguments = units[0]['arguments']
        native_arguments = []
        index = 0
        while index < len(arguments):
            argument = arguments[index]
            index += 1
            if argument == '-I':
                path = Path(arguments[index])
                index += 1
                if path.is_relative_to(root):
                    native_arguments += ['-I', str(path)]
            elif argument.startswith(('-D', '-U')):
                native_arguments.append(argument)
        oracle = None
        for optimization in ('0', '2'):
            run('native-build-' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                *native_arguments, fixture, *[unit['source'] for unit in units], '-o', out / ('native-' + optimization)])
            result = run('native-' + optimization, [out / ('native-' + optimization)])
            (verify_sha_output if args.fixture == 'mbedtls' else verify_json_output)(result.stdout)
            check('independent-reference-' + optimization, not result.stderr)
            if oracle is not None:
                check('native-optimization-parity', result.stdout == oracle)
            oracle = result.stdout
        units.append(dict(id='fixture:' + args.fixture, source=str(fixture), standard='c++17',
                          arguments=arguments, role='fixture'))
        (out / 'project.json').write_text(json.dumps(dict(schema=1, project='duckdb-' + args.fixture, units=units), indent=2) + '\n')
        package = run('package', [sys.executable, '-B', args.project_driver.resolve(), 'build',
            '--manifest', out / 'project.json', '--output-dir', out / 'objects', '--compiler', args.compiler.resolve(),
            '--jobs', '1', '--memory-limit-mib', '8192'], allow_failure=args.allow_export_rejection)
        receipt = json.loads((out / 'objects/results.json').read_text())
        if package.returncode:
            for key, unit in receipt['units'].items():
                if unit['status'] != 'packaged':
                    pending.append(dict(id=key, result=unit))
            if not pending:
                raise ValueError('Packaging failed without a retained unit rejection')
        else:
            objects = receipt['units']
            for index, unit in enumerate(objects.values()):
                run('verify-' + str(index), ['dotnet', args.linker.resolve(), 'verify', unit['object']])
            fixture.rename(out / 'retained-fixture.cpp')
            generated = out / 'generated'
            command = ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main', '--allow-externs']
            command += [unit['object'] for unit in objects.values()]
            for provider in args.provider:
                command += ['--provider', provider.resolve()]
            run('link', command)
            run('bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', generated])
            run('build', ['dotnet', 'build', generated / 'Translated.csproj', '--nologo', '-v:q'])
            result = run('jit', ['dotnet', generated / 'bin/Debug/net10.0/Translated.dll'])
            check('jit-native-parity', result.stdout == oracle and not result.stderr)
            if args.nativeaot:
                run('publish', ['dotnet', 'publish', generated / 'Translated.csproj', '-r', args.rid,
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '--nologo', '-v:q'])
                result = run('nativeaot', [out / 'aot/Translated'])
                check('nativeaot-native-parity', result.stdout == oracle and not result.stderr)
    except Exception as error:
        failure = str(error)
    status = 'failed' if failure else 'native-oracles-passed-export-pending' if pending else 'passed'
    result = dict(status=status, error=failure, fixture=args.fixture, checks=checks, commands=commands,
                  inputs=inputs, pending=pending, managedExecutionVerified=status == 'passed',
                  nativeAotVerified=args.nativeaot and status == 'passed',
                  compiler=dict(path=str(args.compiler.resolve()), sha256=sha(args.compiler)),
                  linker=dict(path=str(args.linker.resolve()), sha256=sha(args.linker)))
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=status, checks=len(checks), pending=len(pending), error=failure)))
    return 1 if failure else 0


if __name__ == '__main__':
    raise SystemExit(main())
