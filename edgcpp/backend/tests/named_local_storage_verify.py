"""Named aggregate locals, layout/alignment fallbacks and CLR unwinding."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

from source_correspondence_verify import function_body


def verify(root, out, compiler, nativeaot):
    source = Path(__file__).with_suffix('').with_name('named_local_storage.cpp')
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
    checks.append('native C++ oracle: native layout, address alignment, normal/exception cleanup')
    for instance in (False, True):
        generated = out / ('instance' if instance else 'static')
        run([sys.executable, root / 'dotnet.py', source, '--exceptions', '--output-dir', generated,
             *(['--compiler', compiler] if compiler else []),
             *(['--instance-methods'] if instance else [])])
        text = '\n'.join(path.read_text() for path in generated.glob('Translated*.cs'))
        ordinary = function_body(text, 'ordinary_storage')
        unwind = function_body(text, 'unwind_named_locals')
        aligned = function_body(text, 'aligned_storage')
        large = function_body(text, 'large_storage')
        assert '__edg_frame' not in ordinary, ordinary
        assert '__edg_frame' not in unwind, unwind
        assert 'item = ' in ordinary and 'items = ' in ordinary and '&(item)' in ordinary, ordinary
        assert 'first = default' in unwind and 'second = default' in unwind, unwind
        assert 'finally' in unwind and '__edg_cleanup_' in unwind, unwind
        assert '__edg_frame_raw' in aligned and 'Unsafe.AsPointer(ref item)' in aligned, aligned
        assert 'NativeMemory.Alloc' in large and 'NativeMemory.Free' in large, large
        (generated / 'storage-review.json').write_text(json.dumps({
            'ordinary': ordinary, 'unwind': unwind, 'aligned': aligned, 'large': large}, indent=2)+'\n')
        project = generated / 'Generated.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])
        checks.append(generated.name + ': named locals, stable addresses, packed/base layouts, aligned/heap fallbacks, destructor unwinding JIT')
        if nativeaot:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true', '-p:DebugType=none', '-p:DebugSymbols=false', '-o', generated / 'aot'])
            run([generated / 'aot/Generated'])
            checks.append(generated.name + ': Linux x64 NativeAOT')
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2)+'\n')
    print(f'{len(checks)} named-local storage checks passed')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    verify(Path(__file__).resolve().parents[2], out, args.compiler, args.nativeaot)
