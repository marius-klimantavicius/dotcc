#!/usr/bin/env python3
"""Verify the pinned libc++ implementation used by cpfe-dot self-translation."""
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
    fixture = ROOT / 'backend/tests/self_dot_libcxx.cpp'
    provider = prepare_provider(ROOT, acquisition, out / 'provider.cpp', dot_backend=fixture)
    manifest = json.loads((acquisition / 'acquisition.json').read_text())
    upstream = acquisition / manifest['source']['archiveRoot']
    generated = out / 'generated'
    report = {'provider': provider, 'checks': [], 'commands': []}

    def run(stage, command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True)
        (out / (stage + '.stdout')).write_text(result.stdout)
        (out / (stage + '.stderr')).write_text(result.stderr)
        report['commands'].append({'stage': stage, 'command': command, 'exitCode': result.returncode})
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'{stage} failed: {result.returncode}; see {out}')
        return result.stdout

    run('native-build', ['g++', '-std=c++17', fixture, '-o', out / 'native'])
    expected = run('native-run', [out / 'native'])
    report['checks'].append('Native C++17 behavior oracle')
    run('translate', [sys.executable, ROOT / 'dotnet.py', out / 'provider.cpp',
        '--compiler', args.compiler.resolve(), '--language', 'c++', '--std', provider['standard'],
        '--libc', 'musl', '-I', acquisition / 'config', '-I', upstream / 'include',
        '--emit', 'exe', '--output-dir', generated])
    generated_manifest = json.loads((generated / 'dot-gen.json').read_text())
    if generated_manifest.get('unboundExternals', {}).get('requiredPartials'):
        raise RuntimeError('Provider retained unresolved partial methods')
    report['checks'].append('Unmodified pinned string.cpp/stdexcept.cpp; strict unresolved rejection')
    run('build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo'])
    actual = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
    if actual != expected:
        raise RuntimeError('JIT output differs from native oracle')
    report['checks'].append('JIT: global map/set/string lifecycle, stable_sort/vector, string edits/move, integer limits, typed range/length exceptions')
    if args.nativeaot:
        run('publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
            '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', generated / 'aot'])
        actual = run('aot', [generated / 'aot/Generated'])
        if actual != expected:
            raise RuntimeError('NativeAOT output differs from native oracle')
        report['checks'].append('NativeAOT: same output and behavior')
    report['passed'] = True
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(f"{len(report['checks'])} cpfe-dot libc++ provider checks passed")


if __name__ == '__main__':
    main()
