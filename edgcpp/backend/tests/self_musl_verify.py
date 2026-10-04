#!/usr/bin/env python3
"""Bounded EDG musl self-profile execution and honest remaining external contracts."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--libcxx-acquisition', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    acquisition = args.libcxx_acquisition.resolve()
    pin = json.loads((ROOT / 'backend/runtime/libcxx/source.json').read_text())
    manifest = json.loads((acquisition / 'acquisition.json').read_text())
    assert manifest['source']['sha256'] == pin['sha256']
    headers = [ROOT / 'backend/self/include', acquisition / 'config', acquisition / pin['archiveRoot'] / 'include']
    source = Path(__file__).with_name('self_musl.cpp').resolve()
    commands = []
    checks = []

    def run(label, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, text=True, capture_output=True)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        commands.append({'label': label, 'command': command, 'exitCode': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        assert (result.returncode == 0) == success, (label, result.stdout[-5000:], result.stderr[-5000:])
        return result

    common = [sys.executable, ROOT / 'dotnet.py', source, '--compiler', args.compiler.resolve(),
              '--language', 'c++', '--std', 'c++14', '--exceptions', '--no-rtti',
              '--target', 'portable-lp64-binary80-v2', '--libc', 'musl',
              '-DUSE_MMAP_FOR_MEMORY_REGIONS=0', '--unbound-externals', 'partial',
              '--namespace', 'SelfMusl', '--class-name', 'CompilerProbe',
              '--split', 'size', '--split-size', '262144',
              *[value for header in headers for value in ('-I', header)]]
    executable = out / 'executable'
    run('translate-executable', [*common, '--instance-methods', '--output-dir', executable])
    generated_manifest = json.loads((executable / 'dot-gen.json').read_text())
    assert generated_manifest['libcProvider']['id'] == 'musl-selected-bcl-lp64-v1'
    assert len(generated_manifest['libcProvider']['translationUnits']) == 213
    assert generated_manifest['unboundExternals']['symbols'] == [], generated_manifest['unboundExternals']['symbols']
    code = '\n'.join(p.read_text() for p in executable.glob('Translated*.cs'))
    assert '__edg_varcall' not in code
    assert all(name in code for name in ('snprintf', 'strtod', 'strtold', 'sscanf'))
    assert 'global::Edg.Runtime.LibcContext.Memcpy' in code
    assert '_IO_FILE' not in code and '__xstat' not in code
    project = executable / 'Generated.csproj'
    run('build-executable', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
    run('jit-executable', ['dotnet', executable / 'bin/Release/net10.0/Generated.dll'])
    checks.append('C++14 portable host headers and selected musl coexist without host ABI headers')
    checks.append('strict JIT: musl formatting/parsing, binary80, binary128 NaN classification, native heap/memory and exceptions')
    if args.nativeaot:
        run('publish-executable', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
            '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:TreatWarningsAsErrors=true', '-o', executable / 'aot'])
        run('aot-executable', [executable / 'aot/Generated'])
        checks.append('Linux-x64 NativeAOT: same bounded executable')
    contracts = out / 'contracts'
    run('translate-contracts', [*common, '-DEDG_SELF_CONTRACT_PROBE=1', '--emit', 'managedlib', '--output-dir', contracts])
    contract_manifest = json.loads((contracts / 'dot-gen.json').read_text())
    symbols = contract_manifest['unboundExternals']['symbols']
    names = [item['symbol'] for item in symbols]
    assert all(name in names for name in ('stat', 'getcwd', 'strtoflt128')), names
    assert any('bad_alloc' in name for name in names), names
    assert not any(name in names for name in ('snprintf', 'strtod', 'memcpy', 'malloc')), names
    contract_text = '\n'.join(p.read_text() for p in contracts.glob('*Runtime.generated.cs'))
    assert 'partial' in contract_text and 'stat' in contract_text and 'getcwd' in contract_text
    missing = run('missing-contracts', ['dotnet', 'build', contracts / 'Generated.csproj', '-c', 'Release', '--nologo'], success=False)
    assert 'CS8795' in missing.stdout, missing.stdout[-5000:]
    checks.append('host stat/getcwd, quadmath and libc++ bad_alloc remain required partial bodies; no fabricated implementations')
    checks.append('real pinned libc++ <new> coexists in contract-only library; C++ implementations remain unresolved')
    scope_source = Path(__file__).with_name('for_local_scope.c').resolve()
    for language, standard, frontend in [('c', 'c11', 'gcc'), ('c++', 'c++14', 'g++')]:
        label = 'scope-' + language.replace('+', 'x')
        native = out / (label + '-native')
        run(label + '-native-build', [frontend, '-std=' + standard, scope_source, '-o', native])
        run(label + '-native-run', [native])
        directory = out / label
        run(label + '-translate', [sys.executable, ROOT / 'dotnet.py', scope_source,
            '--compiler', args.compiler.resolve(), '--language', language, '--std', standard,
            '--output-dir', directory])
        run(label + '-build', ['dotnet', 'build', directory / 'Generated.csproj', '-c', 'Release',
            '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(label + '-jit', ['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])
        checks.append(language + ': native/JIT adjacent declarations preserve scope beyond for loops')
    (out / 'results.json').write_text(json.dumps({
        'status': 'passed', 'checks': checks, 'contractSymbols': symbols,
        'compiler': {'path': str(args.compiler.resolve()), 'sha256': hashlib.sha256(args.compiler.read_bytes()).hexdigest()},
        'inputs': {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in (source, scope_source, Path(__file__))},
        'hostHeaderHashes': {str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest()
                            for p in sorted(headers[0].rglob('*')) if p.is_file()},
        'libcxxAcquisition': str(acquisition),
        'limitations': ['Bounded profile verification only, not managed EDG compiler execution.',
                        'NativeAOT executed on Linux x64 only; portable host declarations still require adapters.']
    }, indent=2) + '\n')
    print('PASS: ' + '; '.join(checks))


if __name__ == '__main__':
    main()
