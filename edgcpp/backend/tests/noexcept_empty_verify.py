"""Omit noexcept boundaries only for generated empty bodies and bare returns."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

from source_correspondence_verify import function_body


def verify(root, out, compiler, nativeaot):
    source = Path(__file__).with_suffix('').with_name('noexcept_empty.cpp')
    commands = []
    checks = []

    def run(command):
        command = list(map(str, command))
        completed = subprocess.run(command, text=True, capture_output=True)
        index = len(commands)
        (out / f'command-{index:03}.stdout').write_text(completed.stdout)
        (out / f'command-{index:03}.stderr').write_text(completed.stderr)
        commands.append({'command': command, 'exit': completed.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if completed.returncode:
            raise RuntimeError(f'{command}\n{completed.stdout}\n{completed.stderr}')

    inputs = out / 'inputs'
    inputs.mkdir()
    hashes = {}
    for path in [source, Path(__file__), root / 'backend/dot_gen_be.c', root / 'backend/dot_gen_source_storage.h']:
        data = path.read_bytes()
        (inputs / path.name).write_bytes(data)
        hashes[path.name] = hashlib.sha256(data).hexdigest()
    (inputs / 'sha256.json').write_text(json.dumps(hashes, indent=2) + '\n')
    oracle = out / 'oracle'
    run(['g++', '-std=c++17', '-O2', source, '-o', oracle])
    run([oracle])
    checks.append('native C++ oracle: empty functions and synthesized member destruction')
    for instance in (False, True):
        generated = out / ('instance' if instance else 'static')
        run([sys.executable, root / 'dotnet.py', source, '--exceptions', '--output-dir', generated,
             *(['--compiler', compiler] if compiler else []),
             *(['--instance-methods'] if instance else [])])
        text = '\n'.join(path.read_text() for path in generated.glob('Translated*.cs'))
        bodies = {}
        for name in ('empty_noexcept', 'return_noexcept', 'empty_blocks_noexcept', 'Empty__ctor', 'Empty__destroy'):
            body = bodies[name] = function_body(text, name)
            assert '__edg_noexcept_error' not in body and 'try' not in body, body
        for name in ('expression_return_noexcept', 'call_noexcept', 'Member__destroy', 'Owner__destroy', 'cleanup_noexcept'):
            body = bodies[name] = function_body(text, name)
            assert '__edg_noexcept_error' in body and '__edg_terminate_runtime' in body, body
        assert 'Member__destroy' in bodies['Owner__destroy'], bodies['Owner__destroy']
        (generated / 'noexcept-review.json').write_text(json.dumps(bodies, indent=2)+'\n')
        project = generated / 'Generated.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
        checks.append(generated.name + ': inert guards omitted; calls, return expressions and synthesized member cleanup guarded; JIT')
        if nativeaot:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true', '-p:DebugType=none', '-p:DebugSymbols=false', '-o', generated / 'aot'])
            run([generated / 'aot/Generated'])
            checks.append(generated.name + ': Linux x64 NativeAOT')
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2)+'\n')
    print(f'{len(checks)} empty-noexcept checks passed')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    verify(Path(__file__).resolve().parents[2], out, args.compiler, args.nativeaot)
