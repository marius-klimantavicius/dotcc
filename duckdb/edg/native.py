#!/usr/bin/env python3
"""Configure/build pinned DuckDB and upstream tests with all bundled static extensions."""
import argparse
import json
from pathlib import Path
import platform
import shlex
import subprocess
import time

from acquire import acquire, PIN, sha
from sources import manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source-dir', type=Path, required=True,
                        help='Acquisition directory containing source/ and the pinned archive')
    parser.add_argument('--build-dir', type=Path, required=True)
    parser.add_argument('--jobs', type=int, default=4)
    parser.add_argument('--std', choices=['17', '20'], default='20',
                        help='C++ standard for this native baseline (match the translated profile when comparing)')
    parser.add_argument('--configure-only', action='store_true')
    parser.add_argument('--unity', action='store_true',
                        help='use upstream CMake unity groups (also for a configure-only unity inventory)')
    args = parser.parse_args()
    if args.jobs < 1:
        parser.error('--jobs must be positive')
    source = acquire(args.source_dir)
    build = args.build_dir.resolve()
    build.mkdir(parents=True, exist_ok=True)
    query = build / '.cmake/api/v1/query'
    query.mkdir(parents=True, exist_ok=True)
    (query / 'codemodel-v2').touch()
    pin = json.loads(PIN.read_text())
    report = dict(pin=pin, source=str(source), build=str(build), standard='c++' + args.std,
                  unity=args.unity, status='running', commands=[])
    receipt = build / 'native-results.json'

    def save():
        receipt.write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command):
        command = list(map(str, command))
        entry = dict(label=label, command=command)
        report['commands'].append(entry)
        save()
        start = time.monotonic()
        with (build / (label + '.stdout')).open('w') as stdout, (build / (label + '.stderr')).open('w') as stderr:
            result = subprocess.run(command, stdout=stdout, stderr=stderr)
        entry.update(exitCode=result.returncode, seconds=round(time.monotonic() - start, 3))
        save()
        if result.returncode:
            raise RuntimeError(label + ' failed: retained logs in ' + str(build))

    try:
        run('configure', ['cmake', '-S', source, '-B', build, '-G', 'Ninja',
            '-DCMAKE_BUILD_TYPE=Release', '-DCMAKE_CXX_STANDARD=' + args.std,
            '-DCMAKE_EXPORT_COMPILE_COMMANDS=ON', '-DDISABLE_UNITY=' + ('OFF' if args.unity else 'ON'),
            '-DBUILD_UNITTESTS=ON', '-DENABLE_UNITTEST_CPP_TESTS=ON', '-DBUILD_SHELL=ON',
            '-DDISABLE_EXTENSION_LOAD=ON', '-DENABLE_EXTENSION_AUTOLOADING=OFF',
            '-DENABLE_EXTENSION_AUTOINSTALL=OFF', '-DENABLE_JEMALLOC=OFF',
            '-DBUILD_EXTENSIONS=core_functions;parquet;icu;json;autocomplete;tpcds;tpch',
            '-DOVERRIDE_GIT_DESCRIBE=' + pin['tag'] + '-0-g' + pin['commit'][:10],
            '-DGIT_COMMIT_HASH=' + pin['commit'][:10],
            '-DCMAKE_PROJECT_INCLUDE=' + str(Path(__file__).with_name('native_hook.cmake').resolve())])
        report['sourceCounts'] = manifest(build, source)['counts']
        first_compile = json.loads((build / 'compile_commands.json').read_text())[0]
        compiler = (first_compile.get('arguments') or shlex.split(first_compile['command']))[0]
        report['environment'] = dict(platform=platform.platform(), python=platform.python_version(),
            compiler=subprocess.check_output([compiler, '--version'], text=True).splitlines()[0],
            cmake=subprocess.check_output(['cmake', '--version'], text=True).splitlines()[0])
        report['inputs'] = [dict(path=str(p.resolve()), sha256=sha(p)) for p in
            [PIN, Path(__file__), Path(__file__).with_name('sources.py'),
             Path(__file__).with_name('native_hook.cmake'),
             Path(__file__).parent / 'tests/reporter.cpp',
             build / 'CMakeCache.txt', build / 'compile_commands.json', build / 'sources.json']]
        if not args.configure_only:
            reply = build / '.cmake/api/v1/reply'
            index = json.loads(sorted(reply.glob('index-*.json'))[-1].read_text())
            model = json.loads((reply / index['reply']['codemodel-v2']['jsonFile']).read_text())
            artifacts = {}
            for target in model['configurations'][0]['targets']:
                if target['name'] in ('unittest', 'duckdb', 'duckdb_static', 'shell'):
                    detail = json.loads((reply / target['jsonFile']).read_text())
                    artifacts[target['name']] = detail['artifacts'][0]['path']
            # The shell output "duckdb" shadows the shared-library target name in
            # Ninja; build its actual CMake-reported artifact instead of that alias.
            run('build', ['cmake', '--build', build, '--target', 'unittest',
                          artifacts['duckdb'], 'duckdb_static', 'shell', '--parallel', str(args.jobs)])
            report['artifacts'] = [dict(target=target, path=str(build / path), sha256=sha(build / path))
                                   for target, path in artifacts.items()]
        report['status'] = 'configured' if args.configure_only else 'built'
    except Exception as error:
        report.update(status='failed', failure=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
