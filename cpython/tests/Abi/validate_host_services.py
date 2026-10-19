"""Focused native/emitted JIT/AOT control for the R5 shared-service repair.

Uses the campaign process/evidence helpers; consumes one authored C fixture,
never the CPython manifest, a native Python backend, or a product publisher.
"""
import argparse
import json
from pathlib import Path
import re
import shutil
import sys
import platform

REPO = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO/'Scripts'))
from campaigns.identity import digest, write_json
from campaigns.process import execute

LOCK = '/tmp/dotcc-cpython-shared-build.lock'
NATIVE_LOCK = '/tmp/dotcc-cpython-native-build.lock'


def math_rows(path):
    text = path.read_text()
    rows = re.findall(r'^math (\d+) ([0-9a-f]{16}) ([0-9a-f]{16}) ([0-9a-f]{16})$', text, re.M)
    if len(rows) != 11 or [int(row[0]) for row in rows] != list(range(11)):
        raise AssertionError('Incomplete native/emitted math transcript: '+str(path))
    count = re.findall(r'^host-services passed checks=(\d+)$', text, re.M)
    if len(count) != 1 or int(count[0]) < 90:
        raise AssertionError('Missing completed semantic assertions: '+str(path))
    return rows, int(count[0])


def identity_rows(path):
    rows = [row for row in path.read_text().splitlines() if row.startswith(('langitem ', 'langutf8item ', 'langinfo ', 'langinfo-utf8 ', 'identity ', 'passwd '))]
    if len(rows) != 112:
        raise AssertionError('Incomplete process/passwd identity observations: '+str(path))
    return rows


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work', type=Path, required=True)
    parser.add_argument('--artifacts', type=Path, required=True)
    parser.add_argument('--capture-tools', action='store_true', help=argparse.SUPPRESS)
    args = parser.parse_args()
    if sys.platform != 'linux' or platform.machine().lower() not in ('x86_64', 'amd64'):
        raise RuntimeError('This focused native/emitted execution control requires Linux x64.')
    work, artifacts = args.work.resolve(), args.artifacts.resolve()
    if args.capture_tools:
        # Called under flock --close. Match the framework's dependency snapshot
        # shape, so A's later compiler rebuild cannot alter this focused control.
        tools = work/'tools'
        tools.mkdir()
        source = REPO/'DotCC/bin/Release/net10.0'
        paths = [p for p in source.iterdir() if p.suffix in ('.dll', '.json')]
        for path in paths:
            shutil.copy2(path, tools/path.name)
        write_json(artifacts/'tool-identity.json', {p.name: digest(p) for p in paths})
        return
    work.mkdir(parents=True, exist_ok=False)
    artifacts.mkdir(parents=True, exist_ok=False)
    commands = []

    def run(label, command, cwd=REPO, timeout=600):
        record = {}
        commands.append(record)
        try:
            return execute(command, label=label, cwd=cwd, output=artifacts/(label+'.log'),
                           timeout=timeout, record=record)
        finally:
            write_json(artifacts/'commands.json', commands)

    fixture = REPO/'cpython/tests/Abi/host_services_r5.c'
    write_json(artifacts/'input.json', {'source': str(fixture), 'sha256': digest(fixture),
        'scope': 'focused shared services only; no translated CPython qualification',
        'wchar': {'native': 'Linux signed 32-bit scalar', 'emitted': 'unsigned 16-bit UTF-16 units'}})
    run('compiler-build', ['flock', '--close', LOCK, 'dotnet', 'build', 'DotCC/DotCC.csproj',
        '-c', 'Release', '-p:UseLocalLalrCc=false', '--disable-build-servers', '-m:1'])
    run('capture-tools', ['flock', '--close', LOCK, sys.executable, __file__,
        '--work', work, '--artifacts', artifacts, '--capture-tools'])
    run('native-build', ['flock', '--close', NATIVE_LOCK, 'cc', '-std=c11', '-D_DEFAULT_SOURCE',
        '-Wall', '-Wextra', '-Werror', '-fno-builtin', fixture, '-lm', '-pthread', '-o', work/'host-native'])
    (work/'native-run').mkdir()
    run('native-run', [work/'host-native'], work/'native-run', timeout=30)
    run('emit', ['flock', '--close', LOCK, 'dotnet', work/'tools/dotcc.dll', '--emit=csproj',
        '--runtime=c', '-std=c11', '-DDOTCC_EXPECT_UTF16', fixture, '-o', work/'host-emitted'])
    project = work/'host-emitted/host-emitted.csproj'
    run('jit-build', ['flock', '--close', LOCK, 'dotnet', 'build', project, '-c', 'Release',
        '--artifacts-path', work/'jit-products', '--disable-build-servers', '-m:1'])
    (work/'jit-run').mkdir()
    run('jit-run', ['flock', '--close', LOCK, 'dotnet', work/'jit-products/bin/host-emitted/release/host-emitted.dll'], work/'jit-run', timeout=30)
    run('aot-build', ['flock', '--close', LOCK, 'dotnet', 'publish', project, '-c', 'Release',
        '-r', 'linux-x64', '-p:PublishAot=true', '-p:StripSymbols=true', '--artifacts-path',
        work/'aot-products', '--disable-build-servers', '-m:1', '-o', work/'aot-app'])
    (work/'aot-run').mkdir()
    run('aot-run', ['flock', '--close', LOCK, work/'aot-app/host-emitted'], work/'aot-run', timeout=30)
    native, native_count = math_rows(artifacts/'native-run.log')
    identities = identity_rows(artifacts/'native-run.log')
    report = {'native_checks': native_count, 'cells': {}}
    for mode in ('jit', 'aot'):
        candidate, count = math_rows(artifacts/(mode+'-run.log'))
        if identity_rows(artifacts/(mode+'-run.log')) != identities:
            raise AssertionError(mode+' identity/passwd fields differ from native NSS')
        distances = []
        for expected, observed in zip(native, candidate):
            # erf/erfc use the exact host math primitive; exp2 is the BCL
            # binary64 primitive and allows one ULP against the Linux oracle.
            for column, limit in ((1, 0), (2, 0), (3, 1)):
                distance = abs(int(expected[column], 16) - int(observed[column], 16))
                if distance > limit:
                    raise AssertionError(f'{mode} vector {expected[0]}, column {column}: {distance} ULP > {limit}')
                distances.append(distance)
        report['cells'][mode] = {'checks': count, 'vectors': 33, 'maximum_ulp': max(distances),
                                'identity_and_passwd': 'exact native match', 'locale_items': 54,
                                'locales': ['C', 'C.UTF-8'], 'status': 'passed'}
    write_json(artifacts/'comparison.json', report)
    print(json.dumps(report, indent=2))


if __name__ == '__main__':
    main()
