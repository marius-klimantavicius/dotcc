#!/usr/bin/env python3
"""Verify managed EDG host configuration before translating the entire compiler."""
import argparse
import json
from pathlib import Path
import shlex
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT))
from backend.self.profile import portable_host_defines


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    entry = next(x for x in json.loads((ROOT / 'build/compile_commands.json').read_text()) if '/cpfe.dir/' in x['command'])
    flags = [x for x in shlex.split(entry['command']) if x.startswith(('-D', '-I'))]
    flags += ['-I' + str(ROOT / 'upstream/src')]
    overrides = ['-D' + key + '=' + value for key, value in portable_host_defines().items()]
    acquisition = args.acquisition.resolve()
    acquired = json.loads((acquisition / 'acquisition.json').read_text())
    headers = [ROOT / 'backend/self/include', acquisition / 'config', acquisition / acquired['source']['archiveRoot'] / 'include']
    source = Path(__file__).with_name('self_profile_alignment.cpp')
    report = {'defines': portable_host_defines(), 'commands': [], 'checks': []}

    def run(label, command, expected=True):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        report['commands'].append({'label': label, 'command': command, 'exitCode': result.returncode})
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
        if (result.returncode == 0) != expected:
            raise RuntimeError(f'{label} unexpected exit {result.returncode}: see {out}')
        return result

    run('native-build', ['g++', '-std=c++14', *flags, source, '-o', out / 'native'])
    run('native', [out / 'native'])
    report['checks'].append('Native original cpfe configuration passes actual pointer/IL-prefix/a_constant alignment checks')
    common = [sys.executable, ROOT / 'dotnet.py', source, '--compiler', args.compiler.resolve(),
        '--language', 'c++', '--std', 'c++14', '--libc', 'musl', *flags,
        '-DUSE_MMAP_FOR_MEMORY_REGIONS=0', *[x for h in headers for x in ['-I', h]], '--emit', 'exe']
    before = run('reject-implicit-host-profile', [*common, '--output-dir', out / 'implicit'], expected=False)
    if 'host pointer alignment' not in before.stderr or 'IL prefix alignment' not in before.stderr:
        raise RuntimeError('Expected alignment reduction did not reproduce')
    report['checks'].append('Old implicit portable profile is rejected at compile time with the original startup alignment mismatch')
    generated = out / 'generated'
    run('translate', [*common, *overrides, '--output-dir', generated])
    run('build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo'])
    run('jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
    report['checks'].append('JIT: explicit portable EDG configuration, actual record offsets, binary80/binary128 storage and Half arithmetic; no OS/CPU macro leakage')
    if args.nativeaot:
        run('publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
            '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', generated / 'aot'])
        run('aot', [generated / 'aot/Generated'])
        report['checks'].append('NativeAOT: same real-header profile and layout checks')
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(f"{len(report['checks'])} EDG self-profile checks passed")


if __name__ == '__main__':
    main()
