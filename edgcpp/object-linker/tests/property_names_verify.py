#!/usr/bin/env python3
"""Source function names take priority over generated property accessor names."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--packager', type=Path, default=ROOT / 'compile.py')
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks = []
    failure = None

    def check(name, condition, **details):
        checks.append(dict(name=name, passed=bool(condition), **details))
        if not condition:
            raise AssertionError(name)

    def run(name, command):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=300)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        check(name, result.returncode == 0, command=command, exitCode=result.returncode)
        return result

    try:
        source = out / 'source'
        source.mkdir()
        for name in ['property_names.h', 'property_names.c', 'property_names_provider.c']:
            shutil.copy2(ROOT / 'tests' / name, source / name)
        run('native-build', ['cc', '-std=c11', '-O2', source / 'property_names.c',
                            source / 'property_names_provider.c', '-o', out / 'native'])
        native = run('native', [out / 'native'])
        obj = out / 'names.dotobj'
        run('compile', [sys.executable, '-B', args.packager.resolve(), source / 'property_names.c',
            '--output', obj, '--work-dir', out / 'compile', '--compiler', args.compiler.resolve(), '--std', 'c11'])
        source.rename(out / 'retained-source')
        generated = out / 'generated'
        run('link', ['dotnet', args.linker.resolve(), 'link', '--out', generated, '--entry', 'main',
                    '--namespace', 'NamesProbe', '--class', 'Core', '--allow-externs', obj])
        declarations = (generated / 'Translated.Runtime.generated.cs').read_text()
        check('source-function-names-preserved', all(name + '();' in declarations
              for name in ['get_packet', 'get_packet__2', 'get_event']))
        storage = (generated / 'Translated.Storage.generated.cs').read_text()
        check('getter-suffix-and-keyword-collisions-resolved', all(name + ' => ref' in storage
              for name in ['packet__3', 'event__2', 'other__2']))
        shutil.copy2(ROOT / 'tests/property_names_runtime.cs', generated / 'Runtime.Names.cs')
        run('build', ['dotnet', 'build', generated / 'Translated.csproj', '--nologo', '-v:q', '--disable-build-servers'])
        jit = run('jit', ['dotnet', generated / 'bin/Debug/net10.0/Translated.dll'])
        check('jit-native-parity', (jit.stdout, jit.stderr) == (native.stdout, native.stderr))
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', generated / 'Translated.csproj', '-r', args.rid,
                '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '--nologo', '-v:q', '--disable-build-servers'])
            aot = run('nativeaot', [out / 'aot/Translated'])
            check('nativeaot-native-parity', (aot.stdout, aot.stderr) == (native.stdout, native.stderr))
    except Exception as error:
        failure = str(error)
    (out / 'results.json').write_text(json.dumps(dict(status='failed' if failure else 'passed',
        failure=failure, checks=checks, nativeAotRequested=args.nativeaot,
        compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
        linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest()), indent=2) + '\n')
    if failure:
        raise RuntimeError(failure + '; inspect ' + str(out))
    print(str(len(checks)) + ' property naming checks passed')


if __name__ == '__main__':
    main()
