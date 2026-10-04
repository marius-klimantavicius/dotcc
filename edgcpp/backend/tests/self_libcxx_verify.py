#!/usr/bin/env python3
"""Verify EDG's pinned libc++ source closure across real translation units."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT))
from backend.self.libcxx import prepare_provider


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    acquisition = args.acquisition.resolve()
    profile = prepare_provider(ROOT, acquisition, out / 'provider.cpp')
    (out / 'provider.json').write_text(json.dumps(profile, indent=2) + '\n')
    acquired = json.loads((acquisition / 'acquisition.json').read_text())
    upstream = acquisition / acquired['source']['archiveRoot']
    fixture = ROOT / 'backend/tests/self_libcxx.cpp'
    generated = out / 'generated'
    report = {'provider': profile, 'checks': [], 'commands': []}

    def run(stage, command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True)
        (out / (stage + '.stdout')).write_text(result.stdout)
        (out / (stage + '.stderr')).write_text(result.stderr)
        report['commands'].append({'stage': stage, 'command': command, 'exitCode': result.returncode})
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'{stage} failed: {result.returncode}; see {out}')

    run('native-build', ['g++', '-std=c++14', fixture, '-o', out / 'native'])
    run('native-run', [out / 'native'])
    report['checks'].append('Native C++14 allocation/EH behavior oracle')
    # Provider is primary: its out-of-line public class definitions establish
    # ownership before EDG merges the consumer and musl translation units.
    run('translate', [sys.executable, ROOT / 'dotnet.py', out / 'provider.cpp', fixture,
        '--compiler', args.compiler.resolve(), '--language', 'c++', '--std', 'c++14',
        '--libc', 'musl', '-I', acquisition / 'config', '-I', upstream / 'include',
        '--emit', 'exe', '--output-dir', generated])
    manifest = json.loads((generated / 'dot-gen.json').read_text())
    if manifest.get('unboundExternals', {}).get('requiredPartials'):
        raise RuntimeError('Provider retained unresolved partial methods')
    report['checks'].append('Separate C++14 provider/consumer TUs; strict unresolved rejection; no bootstrap std types')
    run('build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo'])
    run('jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
    report['checks'].append('JIT: scalar/array/sized/nothrow allocation, failing new_handler, typed bad_alloc/bad_exception/bad_array_new_length, RAII unwinding and exception_ptr')
    if args.nativeaot:
        run('publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
            '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', generated / 'aot'])
        run('aot', [generated / 'aot/Generated'])
        report['checks'].append('NativeAOT: same allocation and exception behavior')
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(f"{len(report['checks'])} EDG libc++ provider checks passed")


if __name__ == '__main__':
    main()
