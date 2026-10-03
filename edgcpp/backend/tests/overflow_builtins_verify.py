"""GNU integer overflow builtins: exact mathematical inputs and modulo stores."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys


def verify_overflow_builtins(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    source = out / 'input.c'
    shutil.copy2(tests / 'overflow_builtins.c', source)
    clang = shutil.which('clang') or shutil.which('clang-18') or shutil.which('clang-19')
    if not clang:
        raise RuntimeError('Clang required for overflow arithmetic oracle')
    checks = []
    all_traces = []
    for language, standard in [('c', 'c17'), ('c++', 'c++20')]:
        label = 'c' if language == 'c' else 'cxx'
        oracle = out / (label + '-oracle')
        oracle.mkdir()
        traces = []
        for name, compiler in [('gcc', 'gcc'), ('clang', clang)]:
            executable = oracle / name
            run([compiler, '-x', language, '-std=' + standard, '-O2', source, '-o', executable])
            trace = run([executable]).stdout
            (oracle / (name + '-trace.txt')).write_text(trace)
            traces.append(trace)
        assert traces[0] == traces[1], (label, traces)
        expected = traces[0]
        assert len(expected.splitlines()) == 40, expected
        all_traces.append(expected)
        lowered = oracle / 'original.c'
        run([root / 'build/bin/cpfe-dot-oracle', '--' + standard, '--gnu_version', '130300',
             '--gen_c_file_name', lowered, '-o', oracle / 'original.il', source],
            env=dict(os.environ, EDG_BASE=str(root / 'upstream/bases/docker/dev-env/gcc')))
        run(['gcc', '-std=gnu11', '-w', lowered, root / 'build/lib/libC.a', '-o', oracle / 'original'])
        actual = run([oracle / 'original']).stdout
        (oracle / 'original-trace.txt').write_text(actual)
        assert actual == expected, (label, actual, expected)
        checks.append(f'overflow/{label}: 40 boundary/random digests agree in GCC, Clang and original EDG C')
        for instance in (False, True):
            mode = label + ('-instance' if instance else '-static')
            directory = out / mode
            run([sys.executable, root / 'dotnet.py', source, '--language', language, '--std', standard,
                 '--libc', 'dotcc', '--output-dir', directory, '--namespace', 'Checked.Arithmetic',
                 '--class-name', 'Core', '--split', 'size', '--split-size', '30000',
                 *(['--instance-methods'] if instance else [])])
            generated = '\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
            assert '__edg_overflow' in generated and 'bool overflow=((a^value)&(b^value))<0;' in generated
            assert 'BigInteger' not in generated
            run(['dotnet', 'build', directory / 'Generated.csproj', '-c', 'Release', '--nologo',
                 '-p:TreatWarningsAsErrors=true', '-p:Nullable=enable'])
            commands = [('JIT', ['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])]
            if nativeaot:
                run(['dotnet', 'publish', directory / 'Generated.csproj', '-c', 'Release', '-r', 'linux-x64',
                     '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:Nullable=enable',
                     '-p:DebugType=None', '-p:DebugSymbols=false', '-o', directory / 'aot'])
                commands.append(('NativeAOT', [directory / 'aot/Generated']))
            for runtime, command in commands:
                actual = run(command).stdout
                (directory / (runtime + '-trace.txt')).write_text(actual)
                assert actual == expected, (mode, runtime, actual, expected)
                checks.append(f'overflow/{mode}/{runtime}: add/sub/mul widths8..128, mixed signs, narrower/wider stores, signed minima, typed variants, volatile and aliased exactly-once arguments')
    assert all_traces[0] == all_traces[1], all_traces
    (out / 'input-hashes.json').write_text(json.dumps({
        'fixture': hashlib.sha256(source.read_bytes()).hexdigest(),
        'compiler': hashlib.sha256((root / 'build/bin/cpfe-dot').read_bytes()).hexdigest(),
        'helper': hashlib.sha256((root / 'backend/dot_gen_overflow.h').read_bytes()).hexdigest()}, indent=2) + '\n')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(commands)
        (output / f'command-{index:03d}.stdout').write_text(result.stdout)
        (output / f'command-{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (output / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    checks = verify_overflow_builtins(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent,
                                    output, args.nativeaot, run)
    (output / 'results.json').write_text(json.dumps({'status': 'passed', 'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} overflow-builtin checks passed')
