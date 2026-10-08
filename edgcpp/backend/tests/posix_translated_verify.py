#!/usr/bin/env python3
"""Portable POSIX ABI and native/translated I/O parity, both owner modes."""
import argparse
import hashlib
import json
import platform
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', type=Path, required=True)
parser.add_argument('--output-dir', type=Path, required=True)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=False)
commands = []
checks = []

def run(label, command, cwd=None, success=True):
    command = list(map(str, command))
    result = subprocess.run(command, text=True, capture_output=True, cwd=cwd)
    (out / (label + '.stdout')).write_text(result.stdout)
    (out / (label + '.stderr')).write_text(result.stderr)
    commands.append({'label': label, 'command': command, 'cwd': str(cwd or Path.cwd()), 'exitCode': result.returncode})
    (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
    assert (result.returncode == 0) == success, (label, result.stdout[-8000:], result.stderr[-8000:])
    return result

source = TESTS / 'posix_translated.c'
layout = out / 'layout-oracle'
native = out / 'native-oracle'
run('layout-build', ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror', '-DPOSIX_LAYOUT_ONLY',
    '-I', ROOT / 'backend/runtime/posix/include', source, '-o', layout])
layout_output = run('layout-run', [layout]).stdout
assert layout_output == ('stat 120 0 8 16 24 32 36 40 48 64 80 96 104 112\n'
                         'timeval 16 0 8\nflock 32 0 2 8 16 24\n'), layout_output
run('native-build', ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror', '-DPOSIX_NATIVE_ORACLE', source, '-o', native])
native_cwd = out / 'native-work'; native_cwd.mkdir()
native_output = run('native-run', [native], native_cwd).stdout
assert native_output == 'io 11 81 33 44; private 93; size-page yes\nmetadata locks time errno paths: passed\n', native_output
expected = layout_output + native_output
checks.append('C compiler against portable headers independently confirms every stat/timeval/flock field offset and size')
checks.append('native system-header POSIX program establishes normalized I/O, errno, mapping and metadata oracle')
common = [sys.executable, ROOT / 'dotnet.py', '--compiler', args.compiler.resolve(), '--language', 'c', '--std', 'c11',
    '--target', 'portable-lp64-binary80-v2', '--libc', 'musl', '--posix', '--namespace', 'Posix.Integration', '--class-name', 'Core',
    '--unbound-externals', 'reject']
for mode in ('static', 'instance'):
    generated = out / mode
    accessor = out / (mode + '-accessor.cs')
    modifier = 'static ' if mode == 'static' else ''
    accessor.write_text('// Test-only access to private executable-mode fixture and retained runtime.\n'
        'namespace Posix.Integration; public unsafe partial class Core { public ' + modifier +
        'global::Edg.Runtime.PosixContext TestRuntime => __edg_posix; public ' + modifier +
        'int TestLeave(sbyte* path) { __edg_enter(); try { return posix_leave_resources(path); } finally { __edg_leave(); } } }\n')
    run(mode + '-translate', [*common, source, '--runtime-source', accessor,
        *(['--instance-methods'] if mode == 'instance' else []), '--output-dir', generated])
    manifest = json.loads((generated / 'dot-gen.json').read_text())
    assert manifest['unboundExternals']['symbols'] == [], manifest['unboundExternals']
    text = '\n'.join(p.read_text() for p in generated.glob('Translated*.cs'))
    assert '__edg_posix_value?.Dispose();' in text and 'ReadOnlySpan<global::Edg.Runtime.VaArg>' in text
    assert '__edg_posix.Fdatasync(' in text
    project = generated / 'Generated.csproj'
    run(mode + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
    work = out / (mode + '-work'); work.mkdir()
    result = run(mode + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'], work)
    assert result.stdout == expected, (mode, result.stdout, expected)
    consumer = out / (mode + '-consumer'); consumer.mkdir()
    shutil.copy2(TESTS / 'posix_translated_consumer.cs', consumer / 'Program.cs')
    consumer_project = consumer / 'Consumer.csproj'
    consumer_project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
        '<OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>'
        '<TreatWarningsAsErrors>true</TreatWarningsAsErrors><DefineConstants>' + ('INSTANCE' if mode == 'instance' else '') +
        '</DefineConstants></PropertyGroup><ItemGroup><ProjectReference Include="../' + mode + '/Generated.csproj" /></ItemGroup></Project>\n')
    run(mode + '-consumer-build', ['dotnet', 'build', consumer_project, '-c', 'Release', '--nologo'])
    run(mode + '-consumer-jit', ['dotnet', consumer / 'bin/Release/net10.0/Consumer.dll'], work)
    checks.append(mode + ': strict managed build; exact portable-layout/native behavior parity; owner Shutdown/Dispose frees descriptors/mappings')
    if args.nativeaot:
        rid = {'Linux': 'linux', 'Darwin': 'osx'}[platform.system()] + ('-arm64' if platform.machine().lower() in ('aarch64', 'arm64') else '-x64')
        for label, target, binary in ((mode, project, 'Generated'), (mode + '-consumer', consumer_project, 'Consumer')):
            aot = out / (label + '-aot')
            run(label + '-publish', ['dotnet', 'publish', target, '-c', 'Release', '-r', rid,
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true', '-o', aot])
            result = run(label + '-aot-run', [aot / binary], work)
            assert result.stdout == (expected if binary == 'Generated' else 'POSIX translated owner lifecycle passed\n'), result.stdout
        checks.append(mode + ': NativeAOT executable parity and owner lifecycle consumer')
failed = run('bad-stat', [*common, TESTS / 'posix_translated_bad_stat.c', '--output-dir', out / 'bad-stat'], success=False)
assert 'POSIX import ABI for stat' in failed.stderr + failed.stdout, failed.stderr
checks.append('mismatched struct stat rejected at translation boundary rather than unsafe runtime cast')
cpp_common = [sys.executable, ROOT / 'dotnet.py', '--compiler', args.compiler.resolve(), '--language', 'c++', '--std', 'c++17',
    '--target', 'portable-lp64-binary80-v2', '--libc', 'musl', '--posix', '--namespace', 'Posix.Integration', '--class-name', 'CppCore',
    '--unbound-externals', 'reject']
cpp_source = TESTS / 'posix_translated_cpp.cpp'
for variant, definitions in [('wrapped-header', []), ('bare-header', ['-DPOSIX_BARE_CPP_HEADER=1'])]:
    generated = out / ('cpp-' + variant)
    run('cpp-' + variant + '-translate', [*cpp_common, cpp_source, *definitions, '--output-dir', generated])
    manifest = json.loads((generated / 'dot-gen.json').read_text())
    assert manifest['unboundExternals']['symbols'] == [], manifest['unboundExternals']
    text = '\n'.join(p.read_text() for p in generated.glob('Translated*.cs'))
    assert '__edg_posix.Getpid()' in text
    project = generated / 'Generated.csproj'
    run('cpp-' + variant + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
    run('cpp-' + variant + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
    if args.nativeaot:
        aot = out / ('cpp-' + variant + '-aot')
        run('cpp-' + variant + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', rid,
            '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true', '-o', aot])
        run('cpp-' + variant + '-aot-run', [aot / 'Generated'])
    checks.append('C++ global getpid with ' + variant + ': POSIX import resolves and executes')
for variant, definition, scope in [('namespace', 'POSIX_NAMESPACE_COLLISION', 'ForeignNamespace'),
                                   ('class', 'POSIX_CLASS_COLLISION', 'ForeignClass')]:
    failed = run('cpp-' + variant + '-reject', [*cpp_common, cpp_source, '-D' + definition + '=1',
        '--output-dir', out / ('cpp-' + variant)], success=False)
    diagnostic = failed.stdout + failed.stderr
    failures = [line for line in diagnostic.splitlines() if 'unsupported external call:' in line]
    assert len(failures) == 1 and 'getpid' in failures[0] and scope in failures[0], diagnostic[-6000:]
    checks.append('C++ ' + variant + ' getpid remains a scoped unresolved external, never rebound to POSIX')
(out / 'results.json').write_text(json.dumps({'status': 'passed', 'checks': checks,
    'compiler': {'path': str(args.compiler.resolve()), 'sha256': hashlib.sha256(args.compiler.read_bytes()).hexdigest()},
    'sources': {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in TESTS.glob('posix_translated*')},
    'expectedOutput': expected}, indent=2) + '\n')
print('POSIX translated integration checks passed: ' + str(len(checks)))
