#!/usr/bin/env python3
"""Keep referenced inline bodies when class definitions merge across C++ TUs."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
TESTS = Path(__file__).resolve().parent


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    sources = [TESTS / 'merged_inline_members_first.cpp', TESTS / 'merged_inline_members_main.cpp']
    report = {'checks': [], 'commands': []}

    def run(label, command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        report['commands'].append({'label': label, 'command': command, 'exitCode': result.returncode})
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'{label} failed with {result.returncode}: see {out}')

    run('native-build', ['g++', '-std=c++14', *sources, '-o', out / 'native'])
    run('native-run', [out / 'native'])
    report['checks'].append('Native C++14 multi-TU reference oracle')
    # Match the backend we consulted, with minimal inlining disabled so that
    # the oracle, like dot_gen, needs a real out-of-line member definition.
    run('c-lower', [ROOT / 'build/bin/cpfe', '--clear_flag', 'use_predefined_macro_file',
        '--c++14', '--gnu_version', '130300', '--instantiate', 'used', '--no_inlining',
        '--gen_c_file_name', out / 'oracle.c', '-o', out / 'oracle.il', *sources])
    run('c-build', ['gcc', out / 'oracle.c', ROOT / 'build/lib/libC.a', '-lstdc++', '-lpthread', '-lm', '-o', out / 'c-native'])
    run('c-run', [out / 'c-native'])
    report['checks'].append('EDG C backend with no inlining emits and executes the referenced inline member')
    for mode in ['static', 'instance']:
        generated = out / mode
        run(mode + '-translate', [sys.executable, ROOT / 'dotnet.py', *sources,
            '--compiler', args.compiler.resolve(), '--language', 'c++', '--std', 'c++14',
            '--libc', 'none', '--emit', 'exe', *(['--instance-methods'] if mode == 'instance' else []),
            '--output-dir', generated])
        run(mode + '-build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo'])
        run(mode + '-jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
        report['checks'].append(mode + ' JIT: merged derived inline read and inherited template route')
        if args.nativeaot:
            run(mode + '-publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
                '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', generated / 'aot'])
            run(mode + '-aot', [generated / 'aot/Generated'])
            report['checks'].append(mode + ' NativeAOT: same merged inline member behavior')
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(f"{len(report['checks'])} merged inline-member checks passed")


if __name__ == '__main__':
    main()
