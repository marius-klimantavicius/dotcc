#!/usr/bin/env python3
"""Translate the complete configured DuckDB library or its upstream test runner.

Preprocess each CMake TU separately so target-specific defines/include order do
not leak into other sources. Keep those files and the exact command receipts.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
import json
import os
from pathlib import Path
import subprocess
import sys
import shutil
import time

from preflight import EDG, frontend_flags, sha
from libcxx import prepare_provider
from portable import verify_staged


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--portable-manifest', type=Path, required=True)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--target', choices=['library', 'tests'], default='library')
    parser.add_argument('--std', choices=['c++17', 'c++20'], default='c++17')
    parser.add_argument('--libcxx-layout', choices=['unity', 'separate'], default='unity',
                        help='source assembly of the real libc++ implementation; does not merge DuckDB source scopes')
    parser.add_argument('--jobs', type=int, default=4)
    parser.add_argument('--timeout', type=int, default=3600)
    parser.add_argument('--preprocess-only', action='store_true')
    args = parser.parse_args()
    if args.jobs < 1 or args.timeout < 1:
        parser.error('--jobs and --timeout must be positive')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    portable = json.loads(args.portable_manifest.read_text())
    verify_staged(portable)
    roles = ('library', 'test') if args.target == 'tests' else ('library',)
    units = [unit for unit in portable['units'] if unit['role'] in roles]
    compiler_input = args.compiler.resolve()
    compiler = out / 'cpfe-dot'
    shutil.copy2(compiler_input, compiler)
    provider = prepare_provider(EDG, args.acquisition, out / 'libcxx', standard=args.std,
                                unity_provider=args.libcxx_layout == 'unity')
    report = dict(status='running', target=args.target, selectedUnits=len(units),
                  portableManifest=dict(path=str(args.portable_manifest.resolve()), sha256=sha(args.portable_manifest)),
                  compiler=dict(path=str(compiler), originalPath=str(compiler_input), sha256=sha(compiler)), provider=provider,
                  preprocessing=[], commands=[], executionVerified=False)
    def save():
        temporary = out / 'results.json.tmp'
        temporary.write_text(json.dumps(report, indent=2) + '\n')
        temporary.replace(out / 'results.json')
    save()
    env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'),
               EDG_DOT_TARGET='portable-lp64-binary80-v2')
    includes = [*map(Path, provider['includeDirectories']), EDG / 'backend/runtime/libcxx/include', EDG / 'backend/runtime/posix/include',
                EDG / 'backend/runtime/libc/include']
    pp = out / 'preprocessed'
    pp.mkdir()

    def preprocess(index, unit):
        source = Path(unit['file'])
        flags, recorded = frontend_flags(unit)
        if unit['language'] != 'c++':
            raise ValueError('Mixed-language whole-program input needs an explicit bridge: ' + str(source))
        path = pp / f'{index:05d}-{source.stem}.ii'
        work = pp / f'{index:05d}'
        work.mkdir()
        command = [str(compiler), '--' + provider['standard'], '--gnu_version', '130300',
                   '--clear_flag', 'use_predefined_macro_file', '--preprocess',
                   *[v for inc in includes for v in ('-I', str(inc))], *flags, str(source)]
        entry = dict(index=index, source=str(source), sourceSha256=sha(source), command=command,
                     path=str(path), nativeFlagsNotForwarded=recorded)
        start = time.monotonic()
        try:
            with path.open('wb') as stdout, (work / 'stderr').open('wb') as stderr:
                result = subprocess.run(command, env=env, cwd=work, stdout=stdout, stderr=stderr, timeout=args.timeout)
            entry.update(exitCode=result.returncode, passed=result.returncode == 0)
        except subprocess.TimeoutExpired:
            entry.update(passed=False, timeout=True)
        entry.update(seconds=round(time.monotonic() - start, 3), sha256=sha(path))
        (work / 'command.json').write_text(json.dumps(entry, indent=2) + '\n')
        return entry

    def run(label, command):
        command = list(map(str, command))
        step = dict(label=label, command=command)
        report['commands'].append(step)
        save()
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, stdout=stdout, stderr=stderr, timeout=args.timeout)
        step['exitCode'] = result.returncode
        save()
        if result.returncode:
            raise RuntimeError(label + ' failed; retained logs in ' + str(out))

    try:
        with ThreadPoolExecutor(max_workers=args.jobs) as pool:
            futures = [pool.submit(preprocess, i, unit) for i, unit in enumerate(units)]
            for future in as_completed(futures):
                entry = future.result()
                report['preprocessing'].append(entry)
                report['preprocessing'].sort(key=lambda r: r['index'])
                if len(report['preprocessing']) % 25 == 0 or not entry['passed']:
                    save()
                if not entry['passed'] or len(report['preprocessing']) % 100 == 0:
                    print(str(len(report['preprocessing'])) + '/' + str(len(units)) + ' preprocessed; ' +
                          ('PASS ' if entry['passed'] else 'FAIL ') + entry['source'], flush=True)
        failed = [r for r in report['preprocessing'] if not r['passed']]
        if failed:
            raise RuntimeError(str(len(failed)) + ' preprocessing failures; all source logs retained')
        if args.preprocess_only:
            report['status'] = 'preprocessed'
            return
        command = [sys.executable, EDG / 'dotnet.py', *provider['translationUnits'],
                   *[r['path'] for r in report['preprocessing']], '--compiler', compiler,
                   '--std', provider['standard'], '--libc', 'musl', '--posix',
                   *[v for inc in provider['includeDirectories'] for v in ('-I', inc)],
                   '--namespace', 'Managed.Analytics', '--class-name', 'DuckDb', '--split', 'source',
                   '--emit', 'exe' if args.target == 'tests' else 'managedlib',
                   '--output-dir', out / 'managed', '--raw-output', out / 'raw.cs',
                   '--frontend-invocation', out / 'invocation.json']
        run('translate', command)
        report['status'] = 'translated'
    except Exception as error:
        report.update(status='failed', failure=str(error))
        raise
    finally:
        try:
            verify_staged(portable)
            if sha(compiler) != report['compiler']['sha256']:
                raise ValueError('EDG compiler changed during translation')
            report['stagedInputsUnchanged'] = True
        except Exception as error:
            report.update(status='failed', provenanceFailure=str(error))
            save()
            raise
        save()


if __name__ == '__main__':
    main()
