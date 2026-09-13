#!/usr/bin/env python3
"""Compare unchanged upstream Linux-header ABI observations against dotcc JIT/AOT.

This is an ABI reference experiment, not approval of the final managed host ABI.
No reference files, generated C#, or replacement platform declarations are edited.
"""
import argparse
import difflib
import hashlib
import json
import os
from pathlib import Path
import platform
import subprocess
import tarfile
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
PIN = json.loads((ROOT / 'config/source.json').read_text())
SOURCE = ROOT / 'ref' / PIN['directory']
COMPILER = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
DEFINES = ['CX_PLATFORM_LINUX=1', '__linux__=1', '_GNU_SOURCE=1', 'NDEBUG=1',
           'QUIC_BUILD_STATIC=1', 'QUIC_EVENTS_STUB=1', 'QUIC_LOGS_STUB=1',
           'QUIC_API_ENABLE_PREVIEW_FEATURES=1', 'QUIC_API_ENABLE_INSECURE_FEATURES=1']


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def verify_reference():
    archive = ROOT / 'ref' / PIN['archive']
    if sha256(archive) != PIN['sha256']:
        raise RuntimeError('Pinned upstream archive checksum mismatch')
    count = 0
    with tarfile.open(archive) as contents:
        for member in contents:
            if not member.isfile():
                continue
            relative = Path(member.name).relative_to(PIN['directory'])
            target = SOURCE / relative
            if '..' in relative.parts or target.is_symlink() or not target.is_file():
                raise RuntimeError('Unsafe or missing reference file: ' + str(target))
            if target.read_bytes() != contents.extractfile(member).read():
                raise RuntimeError('Modified upstream reference file: ' + str(target))
            count += 1
    return count


def observations(text):
    lines = [line.rstrip() for line in text.splitlines() if line.strip()]
    if not lines or any(not line.startswith(('layout ', 'offset ', 'bytes ', 'callback ')) for line in lines):
        raise RuntimeError('Probe output is empty or contains unexpected records')
    return lines


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--groups', nargs='+', choices=['public', 'tls', 'core'], default=['public', 'tls', 'core'])
    parser.add_argument('--native-only', action='store_true', help='Record native controls only; does not claim managed ABI compatibility')
    parser.add_argument('--jit-only', action='store_true', help='Skip NativeAOT; records JIT-only evidence')
    parser.add_argument('--timeout', type=int, default=240)
    parser.add_argument('--compiler', type=Path, default=COMPILER, help='dotcc.dll path; accepts a frozen compiler directory')
    args = parser.parse_args()
    compiler = args.compiler.resolve()
    if platform.system() != 'Linux' or platform.machine() not in ('x86_64', 'amd64'):
        raise SystemExit('This first ABI reference profile requires Linux x64 (LP64); do not silently substitute another ABI')
    if hasattr(os, 'sched_getaffinity'):
        os.sched_setaffinity(0, sorted(os.sched_getaffinity(0))[:4])
    artifacts = ROOT / 'artifacts/abi'
    artifacts.mkdir(parents=True, exist_ok=True)
    build = ROOT / 'build/abi'
    build.mkdir(parents=True, exist_ok=True)
    commands = []
    report = {
        'profile': 'upstream-linux-x64-lp64-preview-reference',
        'mode': 'native-only' if args.native_only else 'jit-only' if args.jit_only else 'jit-and-aot',
        'snapshot': PIN, 'defines': DEFINES,
        'system_headers': {'native': 'host GCC/glibc', 'managed': 'dotcc embedded runtime headers'},
        'limits': ['Not a final managed host ABI contract.',
                   'Source-level callback calls are compared separately in each runtime; no native-to-managed callback export is claimed.',
                   'Only listed fields, layouts, and initialized byte observations are covered.',
                   'No diagnostic source copies or declaration-only replacement headers are used.'],
        'verified_reference_files': verify_reference(),
        'source_sha256': {str(p.relative_to(ROOT)): sha256(p) for p in [Path(__file__).resolve(), *sorted((ROOT / 'tests/Abi').glob('*'))] if p.is_file()},
        'translation_profile_sha256': sha256(ROOT / 'config/dotcc-overrides.json'),
        'compiler_sha256': {str(p): sha256(p) for p in
            [compiler, compiler.parent / 'DotCC.Lib.dll'] if p.is_file()},
        'groups': [], 'commands': commands, 'passed': False,
    }

    def write_report():
        (artifacts / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(group, stage, command):
        destination = artifacts / group
        destination.mkdir(parents=True, exist_ok=True)
        start = time.monotonic()
        try:
            result = subprocess.run(command, capture_output=True, text=True, timeout=args.timeout)
            code, stdout, stderr = result.returncode, result.stdout, result.stderr
        except subprocess.TimeoutExpired as error:
            code, stdout, stderr = 124, '', 'Timed out after ' + str(args.timeout) + ' seconds\n'
        (destination / (stage + '.stdout')).write_text(stdout)
        (destination / (stage + '.stderr')).write_text(stderr)
        record = {'group': group, 'stage': stage, 'arguments': command, 'exit_code': code,
                  'seconds': round(time.monotonic() - start, 3),
                  'stdout': str((destination / (stage + '.stdout')).relative_to(ROOT)),
                  'stderr': str((destination / (stage + '.stderr')).relative_to(ROOT))}
        commands.append(record)
        write_report()
        print(group, stage, code, flush=True)
        return code, stdout

    run('environment', 'gcc-version', ['gcc', '--version'])
    run('environment', 'dotnet-info', ['dotnet', '--info'])
    include_args = ['-I', str(SOURCE / 'src/inc'), '-I', str(SOURCE / 'src/core')]
    define_args = ['-D' + value for value in DEFINES]
    for group in args.groups:
        destination = build / group
        destination.mkdir(parents=True, exist_ok=True)
        source = ROOT / 'tests/Abi' / (group + '.c')
        outcome = {'name': group, 'status': 'started', 'passed': False, 'runtimes': {}}
        report['groups'].append(outcome)
        native = destination / 'native'
        code, _ = run(group, 'native-build', ['gcc', '-std=c17', '-fms-extensions', '-O2',
            '-ffunction-sections', '-fdata-sections', '-Wl,--gc-sections'] + define_args + include_args + [str(source), '-o', str(native)])
        if code:
            outcome['status'] = 'blocked-native-build'
            continue
        code, stdout = run(group, 'native', [str(native)])
        if code:
            outcome['status'] = 'blocked-native-run'
            continue
        reference = observations(stdout)
        outcome['native_records'] = len(reference)
        if args.native_only:
            outcome['status'] = 'native-reference-only'
            continue
        generated = destination / 'generated'
        code, _ = run(group, 'emit', ['dotnet', str(compiler), '-std=c17', '--overrides-file', str(ROOT / 'config/dotcc-overrides.json')] + define_args + include_args +
            [str(source), '--emit=csproj', '-o', str(generated)])
        if code:
            outcome['status'] = 'blocked-translation'
            continue
        outcome['generated_sha256'] = {str(p.relative_to(ROOT)): sha256(p) for p in sorted(generated.glob('*.cs'))}
        projects = sorted(generated.glob('*.csproj'))
        if len(projects) != 1:
            outcome['status'] = 'blocked-generated-project-count'
            continue
        output = destination / 'jit'
        code, _ = run(group, 'managed-build', ['dotnet', 'build', str(projects[0]), '-c', 'Release', '--nologo', '-o', str(output)])
        if code:
            outcome['status'] = 'blocked-managed-build'
            continue
        configs = sorted(output.glob('*.runtimeconfig.json'))
        if len(configs) != 1:
            outcome['status'] = 'blocked-runtime-config-count'
            continue
        assembly = configs[0].name.removesuffix('.runtimeconfig.json')
        runtimes = [('jit', ['dotnet', str(output / (assembly + '.dll'))])]
        if not args.jit_only:
            aot = destination / 'aot'
            code, _ = run(group, 'aot-build', ['dotnet', 'publish', str(projects[0]), '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '--nologo', '-o', str(aot)])
            if code:
                outcome['runtimes']['aot'] = {'passed': False, 'status': 'blocked-build'}
            else:
                runtimes.append(('aot', [str(aot / assembly)]))
        for runtime, command in runtimes:
            code, stdout = run(group, runtime, command)
            if code:
                outcome['runtimes'][runtime] = {'passed': False, 'status': 'blocked-run'}
                continue
            actual = observations(stdout)
            passed = actual == reference
            outcome['runtimes'][runtime] = {'passed': passed, 'records': len(actual)}
            (artifacts / group / (runtime + '.diff')).write_text(''.join(difflib.unified_diff(
                [line + '\n' for line in reference], [line + '\n' for line in actual], fromfile='native', tofile=runtime)))
        outcome['passed'] = bool(outcome['runtimes']) and all(value['passed'] for value in outcome['runtimes'].values())
        outcome['status'] = ('passed-jit-only' if args.jit_only else 'passed-jit-and-aot') if outcome['passed'] else 'failed-runtime-or-comparison'
        write_report()
    report['translation_profile_sha256_after'] = sha256(ROOT / 'config/dotcc-overrides.json')
    report['compiler_sha256_after'] = {str(p): sha256(p) for p in
        [compiler, compiler.parent / 'DotCC.Lib.dll'] if p.is_file()}
    report['compiler_stable'] = report['compiler_sha256'] == report['compiler_sha256_after']
    report['passed'] = not args.native_only and report['compiler_stable'] and all(group['passed'] for group in report['groups'])
    write_report()
    print(json.dumps({group['name']: group['status'] for group in report['groups']}, indent=2))
    if args.native_only:
        return 0 if all(group['status'] == 'native-reference-only' for group in report['groups']) else 1
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
