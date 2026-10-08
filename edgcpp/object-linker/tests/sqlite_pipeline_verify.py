#!/usr/bin/env python3
"""Rebuild SQLite and musl as independent objects, bind, and run engine suites."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--manifest', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--project-driver', type=Path, default=ROOT / 'project.py')
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands = [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=3600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0)
        print(name + ': passed', flush=True)

    failure = None
    package = None
    runtime = None
    try:
        manifest = json.loads(args.manifest.read_text())
        check('threadsafe-musl-manifest', manifest['threadsafe'] == 1 and manifest['libcSourcesIncluded']
              and manifest['nativeExtensionLoading'] is False)
        main_units = [unit for unit in manifest['units'] if unit['id'] == 'sqlite:sqlite3.c']
        providers = [unit for unit in manifest['units'] if unit['role'] == 'libc']
        check('exact-main-and-libc-inputs', len(main_units) == 1 and len(providers) + 1 == len(manifest['units']))
        run('package', [sys.executable, '-B', args.project_driver.resolve(), 'build', '--manifest', args.manifest.resolve(),
            '--output-dir', out / 'objects', '--compiler', args.compiler.resolve(), '--jobs', '1', '--memory-limit-mib', '8192'])
        package = json.loads((out / 'objects/results.json').read_text())
        check('all-original-units-packaged', package['status'] == 'packaged' and package['packagedUnits'] == len(manifest['units']))
        objects = package['units']
        generated = out / 'generated'
        command = ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--namespace', 'Edg.SQLite',
                   '--class', 'SqliteCore', '--allow-externs', objects[main_units[0]['id']]['object']]
        for unit in providers:
            command += ['--provider', objects[unit['id']]['object']]
        run('link', command)
        run('sqlite-bind', [sys.executable, '-B', ROOT / 'runtime/sqlite.py', '--project', generated])
        run('runtime-bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', generated,
                            '--provider-receipt', generated / 'runtime-sqlite.json'])
        runtime = json.loads((generated / 'runtime-bindings.json').read_text())
        check('all-runtime-imports-bound', runtime['status'] == 'bound' and not runtime['unresolved'])
        run('engine-suites', [sys.executable, '-B', ROOT / 'tests/sqlite_engine_verify.py', '--project', generated,
            '--output-dir', out / 'engine', '--rid', args.rid, *(['--nativeaot'] if args.nativeaot else [])])
        engine = json.loads((out / 'engine/results.json').read_text())
        check('engine-evidence', engine['status'] == 'passed' and (not args.nativeaot or engine['nativeAotVerified']))
    except Exception as error:
        failure = str(error)
    result = dict(status='failed' if failure else 'passed', error=failure, checks=checks, commands=commands,
                  manifest=dict(path=str(args.manifest.resolve()), sha256=sha(args.manifest)),
                  compiler=dict(path=str(args.compiler.resolve()), sha256=sha(args.compiler)),
                  linker=dict(path=str(args.linker.resolve()), sha256=sha(args.linker)),
                  packagedUnits=None if package is None else package.get('packagedUnits'),
                  boundRuntimeImports=None if runtime is None else len(runtime['bound']),
                  nativeAotVerified=args.nativeaot and not failure,
                  scope='Original SQLite and musl translation, managed linking/runtime binding, real threadsafe engine tests; static assembly mode')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=failure)))
    return 1 if failure else 0


if __name__ == '__main__':
    raise SystemExit(main())
