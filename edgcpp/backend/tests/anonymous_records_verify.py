"""Anonymous parent fields retain their EDG layout and independent value copies."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def verify_anonymous_records(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    source = tests / 'anonymous_records.cpp'
    lowered = out / 'lowered.c'
    run([root / 'build/bin/cpfe-dot-oracle', '--c++20', '--gnu_version', '130300',
         '--gen_c_file_name', lowered, '-o', out / 'lowered.il', source],
        env=dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc')))
    run(['gcc', '-std=gnu11', '-w', lowered, root / 'build/lib/libC.a', '-o', out / 'oracle'])
    run([out / 'oracle'])
    checks = ['EDG C backend: anonymous packed struct/union fields, bitfields and value copies']
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        run([sys.executable, root / 'dotnet.py', source, '--std', 'c++20', '--split', 'function',
             '--output-dir', directory, *(['--instance-methods'] if instance else [])])
        run(['dotnet', 'build', directory / 'Generated.csproj', '-c', 'Release', '--nologo'])
        run(['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])
        checks.append(mode + '/JIT: anonymous aggregates and bitfield copies')
        if nativeaot:
            run(['dotnet', 'publish', directory / 'Generated.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
            run([directory / 'aot/Generated'])
            checks.append(mode + '/NativeAOT: anonymous aggregates and bitfield copies')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    output = args.output_dir.resolve(); output.mkdir(parents=True, exist_ok=True)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command)); result = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(commands)
        (output / f'command-{index:03d}.stdout').write_text(result.stdout)
        (output / f'command-{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (output / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode: raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    checks = verify_anonymous_records(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent,
                                      output, args.nativeaot, run)
    (output / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} anonymous-record checks passed')
