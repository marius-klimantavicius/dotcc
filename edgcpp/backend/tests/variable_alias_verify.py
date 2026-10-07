#!/usr/bin/env python3
"""Verify GNU object aliases against native and the unchanged EDG C backend."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--oracle', type=Path, default=ROOT / 'build/bin/cpfe-dot-oracle')
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--fixture', choices=('variable_alias_storage', 'variable_alias_typed'))
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'commands': [], 'checks': [], 'inputs': [],
              'scope': 'GNU resolved variable alias chains, initialized storage, TLS isolation, aligned typed aliases with explicit lifetime; native, stock EDG C, static/instance C#.'}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command, expected_error=None):
        command = list(map(str, command))
        item = {'label': label, 'command': command}
        report['commands'].append(item)
        save()
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, stdout=stdout, stderr=stderr, timeout=900,
                env=dict(os.environ, EDG_BASE=str(ROOT / 'upstream/bases/docker/dev-env/gcc')))
        item['exitCode'] = result.returncode
        save()
        if expected_error:
            if result.returncode == 0 or expected_error not in (out / (label + '.stderr')).read_text():
                raise RuntimeError(label + ' did not report the expected backend rejection')
        elif result.returncode:
            raise RuntimeError(label + ' failed; see retained stdout/stderr')
        return (out / (label + '.stdout')).read_text()

    try:
        for path in [Path(__file__), TESTS / 'variable_alias_storage.c', TESTS / 'variable_alias_typed.cpp', TESTS / 'variable_alias_unresolved.c',
                     ROOT / 'backend/dot_gen_variable_aliases.h', ROOT / 'backend/runtime/libc/threads/threads.c', ROOT / 'backend/dot_gen_be.c',
                     args.compiler, args.oracle, ROOT / 'build/lib/libC.a']:
            with path.open('rb') as stream:
                report['inputs'].append({'path': str(path.resolve()), 'sha256': hashlib.file_digest(stream, 'sha256').hexdigest()})
        for fixture, language, standard, expected in [
                ('variable_alias_storage', 'c', 'c11', 'variable aliases: initialized storage, chains and TLS passed\n'),
                ('variable_alias_typed', 'c++', 'c++17', 'typed variable aliases: passed\n')]:
            if args.fixture and fixture != args.fixture:
                continue
            source = TESTS / (fixture + ('.c' if language == 'c' else '.cpp'))
            native = out / (fixture + '-native')
            run(fixture + '-native-build', ['gcc' if language == 'c' else 'g++', '-std=' + standard,
                '-O2', source, '-pthread', '-o', native])
            if run(fixture + '-native-run', [native]) != expected:
                raise RuntimeError('Unexpected native observations: ' + fixture)
            report['checks'].append(fixture + ': native')
            original = out / (fixture + '-original.c')
            run(fixture + '-c-lower', [args.oracle.resolve(), *(['--c', '--c11'] if language == 'c' else ['--c++17']),
                '--gnu_version', '130300', *(['-I/usr/include/' + subprocess.check_output(['gcc', '-dumpmachine'], text=True).strip(), '-I' + subprocess.check_output(['gcc', '-print-file-name=include'], text=True).strip()] if language == 'c' else []), *(['--instantiate', 'used'] if language == 'c++' else []), '--gen_c_file_name', original,
                '-o', out / (fixture + '.il'), source])
            lowered = out / (fixture + '-original')
            run(fixture + '-c-build', ['gcc', '-std=gnu11', '-w', original, ROOT / 'build/lib/libC.a', '-pthread', '-o', lowered])
            if run(fixture + '-c-run', [lowered]) != expected:
                raise RuntimeError('Stock C/native observations differ: ' + fixture)
            report['checks'].append(fixture + ': stock EDG C')
            translation_source = source
            if language == 'c':
                # Keep this focused alias test one TU: unchanged stock EDG C
                # also rejects aliases in the three-TU case retained separately.
                # The real C11 runtime shim and original fixture are unchanged.
                translation_source = out / (fixture + '-unit.c')
                translation_source.write_text('#include "' + str(source) + '"\n#include "' +
                    str(ROOT / 'backend/runtime/libc/threads/threads.c') + '"\n')
            for mode in ('static', 'instance'):
                label = fixture + '-' + mode
                generated = out / label
                run(label + '-translate', [sys.executable, ROOT / 'dotnet.py', translation_source,
                    '--compiler', args.compiler.resolve(), '--language', language, '--std', standard,
                    '--target', 'portable-lp64-binary80-v2', '--libc', 'dotcc', '--posix',
                    '--unbound-externals', 'reject', '--namespace', 'Alias.Tests', '--class-name', 'Core',
                    *(['--instance-methods'] if mode == 'instance' else []), '--output-dir', generated])
                metadata = json.loads((generated / 'dot-gen.json').read_text())
                if metadata['unboundExternals']['symbols']:
                    raise RuntimeError('Unexpected unresolved imports: ' + label)
                project = generated / 'Generated.csproj'
                run(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
                if run(label + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll']) != expected:
                    raise RuntimeError('Translated/native observations differ: ' + label)
                report['checks'].append(label + ': JIT')
                if args.nativeaot:
                    rid = 'linux-arm64' if os.uname().machine == 'aarch64' else 'linux-x64'
                    run(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', rid,
                        '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true',
                        '-o', generated / 'aot'])
                    if run(label + '-aot', [generated / 'aot/Generated']) != expected:
                        raise RuntimeError('Translated AOT/native observations differ: ' + label)
                    report['checks'].append(label + ': NativeAOT')
        run('unresolved-alias-rejected', [sys.executable, ROOT / 'dotnet.py', TESTS / 'variable_alias_unresolved.c',
            '--compiler', args.compiler.resolve(), '--language', 'c', '--std', 'c11',
            '--target', 'portable-lp64-binary80-v2', '--output-dir', out / 'unresolved'],
            expected_error='unsupported unresolved GNU variable alias')
        report['checks'].append('Unresolved weak variable alias rejected by backend')
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
