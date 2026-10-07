#!/usr/bin/env python3
"""Check DuckDB's real CMake translation units against the portable EDG headers.

This is frontend evidence, not a translated-program correctness claim. Native
compiler code-generation flags are recorded, never passed through to EDG.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
import hashlib
import json
import os
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import time

ROOT = Path(__file__).resolve().parents[2]
EDG = ROOT / 'edgcpp'


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def frontend_flags(unit):
    """Keep each TU's header search order, defines and exception/RTTI choices."""
    args = list(unit.get('arguments') or shlex.split(unit['command']))
    flags, recorded = [], []
    base = Path(unit['directory'])
    index = 1
    while index < len(args):
        arg = args[index]
        index += 1
        if arg in ('-I', '-isystem', '-iquote', '-include', '-D', '-U'):
            value = args[index]
            index += 1
            if arg in ('-D', '-U'):
                flags.append(arg + value)
            else:
                path = Path(value)
                path = path if path.is_absolute() else base / path
                # EDG has no separate GNU system-header warning search tier.
                flags += ['--preinclude' if arg == '-include' else '-I', str(path.resolve())]
        elif arg.startswith(('-D', '-U')):
            flags.append(arg)
        elif arg.startswith('-I'):
            path = Path(arg[2:])
            flags += ['-I', str((path if path.is_absolute() else base / path).resolve())]
        elif arg == '-fno-exceptions':
            flags.append('--no_exceptions')
        elif arg == '-fno-rtti':
            flags.append('--no_rtti')
        elif arg in ('-o', '-MF', '-MT', '-MQ'):
            recorded.extend([arg, args[index]])
            index += 1
        else:
            recorded.append(arg)
    flags.append('-DDUCKDB_CUSTOM_PLATFORM=edg_dotnet_lp64')
    return flags, recorded


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compile-commands', type=Path, required=True)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--select', default='.*', help='regex against absolute source path')
    parser.add_argument('--limit', type=int)
    parser.add_argument('--jobs', type=int, default=2)
    parser.add_argument('--timeout', type=int, default=180)
    parser.add_argument('--std', default='c++20', choices=['c++17', 'c++20'])
    parser.add_argument('--preprocess', action='store_true', help='retain preprocessed source instead of semantic checking')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    acquisition = args.acquisition.resolve()
    manifest = json.loads((acquisition / 'acquisition.json').read_text())
    headers = acquisition / manifest['source']['archiveRoot'] / 'include'
    includes = [acquisition / 'config', headers, EDG / 'backend/runtime/libcxx/include',
                EDG / 'backend/runtime/posix/include', EDG / 'backend/runtime/libc/include']
    compiler_input = args.compiler.resolve()
    compiler = out / 'cpfe-dot'
    shutil.copy2(compiler_input, compiler)
    env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'),
               EDG_DOT_TARGET='portable-lp64-binary80-v2')
    units = json.loads(args.compile_commands.read_text())
    selected, seen = [], set()
    for unit in units:
        if not re.search(args.select, unit['file']):
            continue
        flags, recorded = frontend_flags(unit)
        identity = (unit['file'], tuple(flags))
        if identity in seen:
            continue
        seen.add(identity)
        selected.append((unit, flags, recorded))
    if args.limit is not None:
        selected = selected[:args.limit]
    if not selected:
        parser.error('No translation units selected')
    report = dict(status='running', phase='preprocess' if args.preprocess else 'semantic-check',
                  compiler=dict(path=str(compiler), originalPath=str(compiler_input), sha256=sha(compiler)),
                  compileCommands=dict(path=str(args.compile_commands.resolve()), sha256=sha(args.compile_commands)),
                  libcxxAcquisition=dict(path=str(acquisition), manifestSha256=sha(acquisition / 'acquisition.json')),
                  standard=args.std, selectedUnits=len(selected), checks=[],
                  claim='Individual frontend checks only; no whole-program linking, C# execution or NativeAOT verification')
    def save():
        temporary = out / 'results.json.tmp'
        temporary.write_text(json.dumps(report, indent=2) + '\n')
        temporary.replace(out / 'results.json')
    save()

    def check(index, item):
        unit, flags, recorded = item
        source = Path(unit['file'])
        if not source.is_absolute():
            source = Path(unit['directory']) / source
        directory = out / f'{index:05d}-{source.stem}'
        directory.mkdir()
        standard = 'c11' if source.suffix == '.c' else args.std
        command = [str(compiler), '--' + standard, '--gnu_version', '130300',
                   '--clear_flag', 'use_predefined_macro_file',
                   *[value for inc in includes for value in ('-I', str(inc))],
                   *flags, '--preprocess' if args.preprocess else '--no_code_gen', str(source)]
        result = dict(index=index, source=str(source), sourceSha256=sha(source), command=command,
                      nativeCommand=unit, nativeFlagsNotForwarded=recorded, directory=str(directory))
        start = time.monotonic()
        try:
            with (directory / ('source.ii' if args.preprocess else 'stdout')).open('wb') as stdout, (directory / 'stderr').open('wb') as stderr:
                completed = subprocess.run(command, cwd=directory, env=env, stdout=stdout, stderr=stderr, timeout=args.timeout)
            result.update(exitCode=completed.returncode, passed=completed.returncode == 0)
        except subprocess.TimeoutExpired:
            result.update(passed=False, timeout=True)
        except OSError as error:
            result.update(passed=False, executionError=str(error))
        result['seconds'] = round(time.monotonic() - start, 3)
        diagnostics = (directory / 'stderr').read_text(errors='replace')
        result['diagnosticSummary'] = diagnostics[:3000]
        (directory / 'command.json').write_text(json.dumps(result, indent=2) + '\n')
        return result

    with ThreadPoolExecutor(max_workers=args.jobs) as pool:
        futures = [pool.submit(check, index, item) for index, item in enumerate(selected)]
        for future in as_completed(futures):
            result = future.result()
            report['checks'].append(result)
            report['checks'].sort(key=lambda r: r['index'])
            if len(report['checks']) % 25 == 0 or not result['passed']:
                save()
            print(('PASS ' if result['passed'] else 'FAIL ') + result['source'], flush=True)
    report['passedUnits'] = sum(item['passed'] for item in report['checks'])
    report['status'] = 'passed' if report['passedUnits'] == len(selected) else 'failed'
    save()
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
