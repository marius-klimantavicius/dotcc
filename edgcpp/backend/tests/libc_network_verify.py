#!/usr/bin/env python3
"""Compare selected musl network code as native C and translated C/C++ (JIT/AOT)."""
import argparse
import json
from pathlib import Path
import platform
import subprocess
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--compiler', type=Path, required=True)
parser.add_argument('--output-dir', type=Path, required=True)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
results = {'passed': False, 'runs': []}
def run(label, command):
    command = list(map(str, command))
    result = subprocess.run(command, capture_output=True)
    (out / (label + '.stdout')).write_bytes(result.stdout)
    (out / (label + '.stderr')).write_bytes(result.stderr)
    results['runs'].append({'label': label, 'command': command, 'exit': result.returncode})
    (out / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
    if result.returncode:
        raise RuntimeError(label + '\n' + result.stdout.decode()[-4000:] + result.stderr.decode()[-4000:])
    return result.stdout
source = Path(__file__).with_name('libc_network.c')
network = root / 'backend/runtime/musl/network'
network_sources = sorted(network.glob('*.c'))
run('native-build', ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror',
                    '-Wno-sign-compare', '-Wno-parentheses', source, *network_sources, '-o', out / 'native'])
expected = run('native', [out / 'native'])
for language, mode in (('c', 'static'), ('c', 'instance'), ('c++', 'instance')):
    label = language.replace('++', 'xx') + '-' + mode
    generated = out / label
    run(label + '-translate', [sys.executable, root / 'dotnet.py', '--compiler', args.compiler.resolve(),
        '--language', language, '--std', 'c11' if language == 'c' else 'c++17',
        '--target', 'portable-lp64-binary80-v2', '--libc', 'musl', '--posix',
        '--unbound-externals', 'reject', source, '--output-dir', generated,
        *(['--instance-methods'] if mode == 'instance' else [])])
    manifest = json.loads((generated / 'dot-gen.json').read_text())
    assert not manifest['unboundExternals']['symbols']
    run(label + '-build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release',
                          '--nologo', '-p:TreatWarningsAsErrors=true'])
    actual = run(label + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
    assert actual == expected, (label, actual, expected)
    if args.nativeaot:
        system = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}[platform.system()]
        arch = 'arm64' if platform.machine().lower() in ('arm64', 'aarch64') else 'x64'
        run(label + '-publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
            '-r', system + '-' + arch, '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true',
            '-o', generated / 'aot'])
        actual = run(label + '-aot', [generated / 'aot' / ('Generated.exe' if system == 'win' else 'Generated')])
        assert actual == expected, (label, actual, expected)
results['passed'] = True
results['nativeOutput'] = expected.decode()
(out / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
print(expected.decode().strip())
print('C static, C instance, C++ instance match native musl; JIT' + (' and NativeAOT' if args.nativeaot else ''))
