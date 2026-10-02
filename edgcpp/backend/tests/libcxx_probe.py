"""Run synthetic libc++ seed consumers; report every incomplete stage explicitly."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
SEEDS = Path(__file__).resolve().parent / 'libcxx'


def probe(acquisition, output, fixtures, include, sources, nativeaot, std='c++20', unity=False, libc='dotcc'):
    acquisition = acquisition.resolve()
    manifest = json.loads((acquisition / 'acquisition.json').read_text())
    library = acquisition / manifest['source']['archiveRoot']
    output = output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    results = []
    for fixture in fixtures:
        result = {'fixture': fixture, 'commands': [], 'passed': False}
        directory = output / fixture
        directory.mkdir()
        seed = directory / 'input.cpp'
        seed.write_bytes((SEEDS / (fixture + '.cpp')).read_bytes())
        inputs = [seed, *sources]
        if unity:
            # Explicit source bring-up mode. This establishes one-TU coverage
            # only; separate-TU identity/linkage remains independently required.
            combined = directory / 'combined.cpp'
            # Library implementation feature macros (e.g. memory.cpp's legacy
            # shared-count definitions) must precede their first header include.
            combined.write_text(''.join('#include ' + json.dumps(str(p.resolve())) + '\n' for p in [*sources, seed]))
            inputs = [combined]
        result['compilationMode'] = 'single-tu-source-bring-up' if unity else 'merged-translation-units'
        generated = directory / 'generated'
        command = [sys.executable, ROOT / 'dotnet.py', *inputs, '--libc', libc, '--std', std,
                   '-I', acquisition / 'config', '-I', library / 'include',
                   *[item for path in include for item in ('-I', path.resolve())],
                   '--output-dir', generated]
        commands = [('translate', command),
                    ('build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release', '--nologo']),
                    ('jit', ['dotnet', generated / 'bin/Release/net10.0/Generated.dll'])]
        if nativeaot:
            commands += [('publish', ['dotnet', 'publish', generated / 'Generated.csproj', '-c', 'Release',
                                      '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true',
                                      '-o', generated / 'aot']),
                         ('aot', [generated / 'aot/Generated'])]
        for stage, command in commands:
            command = list(map(str, command))
            completed = subprocess.run(command, text=True, capture_output=True)
            (directory / (stage + '.stdout')).write_text(completed.stdout)
            (directory / (stage + '.stderr')).write_text(completed.stderr)
            result['commands'].append({'stage': stage, 'command': command, 'exit': completed.returncode})
            if completed.returncode:
                result['incompleteStage'] = stage
                break
        else:
            result['passed'] = True
        results.append(result)
        (output / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
        print(f'{fixture}: ' + ('passed' if result['passed'] else 'incomplete at ' + result['incompleteStage']), flush=True)
    return all(result['passed'] for result in results)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--acquisition', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--fixture', action='append', choices=sorted(p.stem for p in SEEDS.glob('*.cpp')))
    parser.add_argument('-I', '--include', action='append', default=[], type=Path)
    parser.add_argument('--source', action='append', default=[], type=Path,
                        help='explicit additional library/adaptation translation unit; no implicit EDG std bootstrap')
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--std', default='c++20', help='library sources use C++20 constinit')
    parser.add_argument('--unity', action='store_true', help='explicit one-TU source bring-up; does not establish multi-TU support')
    parser.add_argument('--libc', choices=['dotcc', 'musl'], default='dotcc', help='select the audited BCL bridge or selected musl+BCL sources')
    args = parser.parse_args()
    ok = probe(args.acquisition, args.output_dir, args.fixture or ['traits', 'utility', 'array', 'vector', 'memory', 'exception_ptr'],
               args.include, [p.resolve() for p in args.source], args.nativeaot, args.std, args.unity, args.libc)
    raise SystemExit(0 if ok else 1)
