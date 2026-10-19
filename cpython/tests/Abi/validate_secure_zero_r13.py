"""Native / provisional emitted secure-zero controls; never builds compiler tools.

An old immutable CLI needs an explicit typed binding to the current shared
runtime project. That diagnostic binding does not qualify embedded resources;
the compiler owner must snapshot/regen full products after the service is READY.
"""
import argparse
import json
from pathlib import Path
import sys

REPO = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO / 'Scripts'))
from campaigns.identity import digest, write_json
from campaigns.process import execute

LOCK = '/tmp/dotcc-cpython-shared-build.lock'
NATIVE_LOCK = '/tmp/dotcc-cpython-native-build.lock'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work', required=True, type=Path)
    parser.add_argument('--artifacts', required=True, type=Path)
    parser.add_argument('--tools', required=True, type=Path)
    parser.add_argument('--blake2', required=True, type=Path)
    parser.add_argument('--before', action='store_true')
    parser.add_argument('--embedded-runtime', action='store_true', help='Use a newly captured CLI with the actual service embedded; no diagnostic binding.')
    args = parser.parse_args()
    work, artifacts = args.work.resolve(), args.artifacts.resolve()
    work.mkdir(parents=True, exist_ok=False)
    artifacts.mkdir(parents=True, exist_ok=False)
    commands = []

    def run(label, command, *, check=True, env=None):
        record = {}
        commands.append(record)
        try:
            execute(command, label=label, cwd=REPO, output=artifacts / (label + '.log'),
                    record=record, timeout=600, check=check, env=env)
        finally:
            write_json(artifacts / 'commands.json', commands)
        return record

    source = REPO / 'cpython/tests/Abi/secure_zero_r13.c'
    flags = ['-std=c11', '-DHAVE_EXPLICIT_BZERO=1', '-DDOTCC_BLAKE2_HEADER=1',
             '-I', args.blake2.resolve()]
    write_json(artifacts / 'inputs.json', {
        'scope': 'provisional focused shared service; no interpreter/full-profile qualification',
        'source': {str(source): digest(source)},
        'upstream': {str(p): digest(p) for p in sorted(args.blake2.resolve().glob('*')) if p.is_file()},
        'tools': {p.name: digest(p) for p in args.tools.resolve().iterdir() if p.is_file()},
        'header': digest(REPO / 'DotCC.Lib/include/string.h'),
        'runtime': {p.name: digest(p) for p in sorted((REPO / 'DotCC.Libc').glob('*.cs'))},
    })
    run('native-build', ['flock', '--close', NATIVE_LOCK, 'cc', '-O3', '-D_DEFAULT_SOURCE',
                        '-Wall', '-Wextra', '-Werror', *flags, source, '-o', work / 'native'])
    run('native-run', [work / 'native'])
    if not args.before:
        run('native-wide', [work / 'native', 'wide'])
    run('native-symbols', ['objdump', '-dr', work / 'native'])

    binding = work / 'current-runtime-binding.json'
    write_json(binding, {'version': 1, 'functionOverrides': [{
        'name': 'explicit_bzero',
        'signature': {'returnType': 'void', 'parameterTypes': ['void *', 'size_t']},
        'target': {'kind': 'managedMethod', 'method': 'global::DotCC.Libc.Libc.explicit_bzero'},
        'requireMatch': True,
    }]})
    emit = ['flock', '--close', LOCK, 'dotnet', args.tools.resolve() / 'dotcc.dll',
            '--emit=csproj', '--runtime=c', '-I', REPO / 'DotCC.Lib/include', *flags,
            source, '-o', work / 'emitted']
    if not args.embedded_runtime:
        emit += ['--overrides-file', binding, '--override-report', artifacts / 'binding.jsonl']
    result = run('emit', emit, check=not args.before)
    if args.before:
        if result['exit_code'] == 0:
            # Old resources may accept an implicit call. Its ordinary build must
            # still reject the absent actual service; never call a stub instead.
            result = run('missing-service-build', ['flock', '--close', LOCK, 'dotnet', 'build',
                         work / 'emitted/emitted.csproj', '-c', 'Release', '--disable-build-servers', '-m:1'], check=False)
        assert result['exit_code'] != 0
        assert 'explicit_bzero' in Path(result['stdout']).read_text(), result
        write_json(artifacts / 'comparison.json', {'status': 'before-missing-service-reproduced'})
        return

    # Authored project metadata adds an ordinary reference, without changing a
    # byte of generated C#. The only typed replacement calls the real service.
    project = work / 'emitted/emitted.csproj' if args.embedded_runtime else work / 'ProvisionalSecureZero.csproj'
    if not args.embedded_runtime:
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
                       '<AllowUnsafeBlocks>true</AllowUnsafeBlocks><EnableDefaultCompileItems>false</EnableDefaultCompileItems>'
                       '</PropertyGroup><ItemGroup><Compile Include="emitted/*.cs" />'
                       f'<ProjectReference Include="{REPO}/DotCC.Libc/DotCC.Libc.csproj" />'
                       '</ItemGroup></Project>\n')
    common = ['-c', 'Release', '--disable-build-servers', '-m:1']
    run('jit-build', ['flock', '--close', LOCK, 'dotnet', 'build', project, *common, '--artifacts-path', work / 'jit'])
    assembly = 'emitted' if args.embedded_runtime else 'ProvisionalSecureZero'
    dll = work / f'jit/bin/{assembly}/release/{assembly}.dll'
    run('jit-run', ['flock', '--close', LOCK, 'dotnet', dll])
    run('jit-wide', ['flock', '--close', LOCK, 'dotnet', dll, 'wide'])
    run('aot-build', ['flock', '--close', LOCK, 'dotnet', 'publish', project, *common,
                    '-r', 'linux-x64', '-p:PublishAot=true', '-p:StripSymbols=false', '--artifacts-path', work / 'aot', '-o', work / 'aot-app'])
    app = work / f'aot-app/{assembly}'
    run('aot-run', ['flock', '--close', LOCK, app])
    run('aot-wide', ['flock', '--close', LOCK, app, 'wide'])
    for cell in ('jit', 'aot'):
        for suffix in ('run', 'wide'):
            assert (artifacts / f'{cell}-{suffix}.log').read_bytes() == (artifacts / f'native-{suffix}.log').read_bytes()
    write_json(artifacts / 'comparison.json', {'status': 'passed', 'cells': ['native', 'jit', 'aot'],
               'ordinary': (artifacts / 'native-run.log').read_text().strip(),
               'wide': (artifacts / 'native-wide.log').read_text().strip(),
               'generated': {str(p): digest(p) for p in (work / 'emitted').glob('*.cs')}})


if __name__ == '__main__':
    main()
