#!/usr/bin/env python3
"""Verify explicit asm-symbol object views against separately compiled native TUs."""
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
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'commands': [], 'checks': [], 'inputs': [],
              'scope': 'Explicit emitted-symbol storage identity; distinct native TUs without LTO; three-TU stock EDG C; pruned full musl provider with static/instance JIT/AOT.'}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command, env=None, error=None):
        command = list(map(str, command))
        item = {'label': label, 'command': command}
        if env:
            item['environmentOverrides'] = {k:v for k,v in env.items() if k.startswith('EDG_')}
        report['commands'].append(item)
        save()
        environment = dict(os.environ, EDG_BASE=str(ROOT / 'upstream/bases/docker/dev-env/gcc'))
        environment.update(env or {})
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, stdout=stdout, stderr=stderr, timeout=900, env=environment)
        item['exitCode'] = result.returncode
        save()
        if error:
            if result.returncode == 0 or error not in (out / (label + '.stderr')).read_text():
                raise RuntimeError(label + ' missed the expected backend diagnostic')
        elif result.returncode:
            raise RuntimeError(label + ' failed; see retained stdout/stderr')
        return (out / (label + '.stdout')).read_text()

    try:
        fixtures = [TESTS / ('variable_asm_' + name + '.cpp') for name in ('views', 'definition', 'other')]
        for path in [Path(__file__), *TESTS.glob('variable_asm_*.cpp'), TESTS / 'variable_asm_type.h',
                     ROOT / 'backend/dot_gen_variable_aliases.h', ROOT / 'backend/dot_gen_provider_pruning.h',
                     ROOT / 'backend/dot_gen_be.c', args.compiler, args.oracle, ROOT / 'build/lib/libC.a']:
            with path.open('rb') as stream:
                report['inputs'].append({'path': str(path.resolve()), 'sha256': hashlib.file_digest(stream, 'sha256').hexdigest()})
        expected = 'asm symbol views: typed storage, initialization and lifetime passed\n'
        objects = []
        for index, source in enumerate(fixtures):
            obj = out / ('native-' + str(index) + '.o')
            run('native-compile-' + str(index), ['g++', '-std=c++17', '-O2', '-c', source, '-o', obj])
            objects.append(obj)
        run('native-link', ['g++', *objects, '-o', out / 'native'])
        if run('native-run', [out / 'native']) != expected:
            raise RuntimeError('Native separate-TU observations differ')
        report['checks'].append('Separate native TUs, GCC O2 without LTO')
        run('c-lower', [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
            '--gen_c_file_name', out / 'original.c', '-o', out / 'original.il', *fixtures])
        run('c-build', ['gcc', '-std=gnu11', '-w', out / 'original.c', ROOT / 'build/lib/libC.a', '-o', out / 'original'])
        if run('c-run', [out / 'original']) != expected:
            raise RuntimeError('Three-TU stock C observations differ')
        report['checks'].append('Stock EDG C with three TUs')
        for mode in ('static', 'instance'):
            options = ['--compiler', args.compiler.resolve(), '--language', 'c++', '--std', 'c++17',
                '--target', 'portable-lp64-binary80-v2', '--libc', 'musl', '--posix', '--unbound-externals', 'reject',
                '--namespace', 'Asm.Tests', '--class-name', 'Core',
                *(['--instance-methods'] if mode == 'instance' else [])]
            baseline = out / (mode + '-baseline')
            invocation = out / (mode + '-invocation.json')
            run(mode + '-translate', [sys.executable, ROOT / 'dotnet.py', *fixtures, *options,
                '--frontend-invocation', invocation, '--output-dir', baseline])
            recorded = json.loads(invocation.read_text())
            environment = recorded['environment']
            provider = out / (mode + '-prunable.txt')
            original_provider = Path(environment['EDG_DOT_PRUNABLE_SOURCES']).read_text()
            # initialized_storage has no ordinary IL reference: only its asm
            # declaration in the root TU requires its storage/initializer.
            provider.write_text(original_provider + str(fixtures[1]) + '\n')
            environment['EDG_DOT_PRUNABLE_SOURCES'] = str(provider)
            raw = run(mode + '-pruned-emit', [args.compiler.resolve(), *recorded['arguments']], env=environment)
            if '// dot-gen:origin initialized_storage\n' not in raw or 'unused_asm_provider_storage' in raw:
                raise RuntimeError('Asm-only target retention or unrelated provider pruning failed')
            report['checks'].append(mode + ': asm-only definition retained; unrelated provider variable pruned')
            generated = out / mode
            run(mode + '-package', [sys.executable, ROOT / 'dotnet.py', *fixtures, *options,
                '--raw-input', out / (mode + '-pruned-emit.stdout'), '--output-dir', generated])
            project = generated / 'Generated.csproj'
            run(mode + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
            if run(mode + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll']) != expected:
                raise RuntimeError('Translated JIT/native observations differ: ' + mode)
            report['checks'].append(mode + ': JIT')
            if args.nativeaot:
                rid = 'linux-arm64' if os.uname().machine == 'aarch64' else 'linux-x64'
                run(mode + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', rid,
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true',
                    '-o', generated / 'aot'])
                if run(mode + '-aot', [generated / 'aot/Generated']) != expected:
                    raise RuntimeError('Translated AOT/native observations differ: ' + mode)
                report['checks'].append(mode + ': NativeAOT')
        for fixture, diagnostic in [('unresolved', 'unresolved variable asm symbol storage'),
                ('duplicate', 'multiple storage definitions for variable asm symbol'),
                ('oversize', 'GNU variable alias view exceeds target storage'),
                ('tls_mismatch', 'GNU variable alias TLS mismatch'),
                ('register', 'hardware-register global storage')]:
            run('reject-' + fixture, [sys.executable, ROOT / 'dotnet.py', TESTS / ('variable_asm_' + fixture + '.cpp'),
                '--compiler', args.compiler.resolve(), '--language', 'c++', '--std', 'c++17',
                '--target', 'portable-lp64-binary80-v2', '--output-dir', out / ('reject-' + fixture)], error=diagnostic)
            report['checks'].append(fixture + ': backend rejection')
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
