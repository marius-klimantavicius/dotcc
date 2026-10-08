#!/usr/bin/env python3
"""Cross-TU C++ exception/RTTI oracle and owned-object acceptance.

Research mode retains export gates without claiming managed execution. Normal
mode requires the independently packaged objects to link and execute together.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--stock-compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--packager', type=Path, default=ROOT / 'compile.py')
    parser.add_argument('--allow-export-rejection', action='store_true')
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    checks, commands, pending, fixtures = [], [], [], []
    tools = out / 'tooling'
    tools.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tools / name)
    for suffix in ('h', 'provider.cpp', 'bridge.cpp', 'main.cpp'):
        name = 'cpp_linked_exceptions' + ('.' if suffix == 'h' else '_') + suffix
        path = inputs / name
        shutil.copy2(ROOT / 'tests' / name, path)
        fixtures.append(dict(path=str(path), sha256=sha(path)))
    sources = [inputs / ('cpp_linked_exceptions_' + name + '.cpp')
               for name in ('provider', 'bridge', 'main')]

    def run(name, command, cwd=out, may_reject=False):
        command = [str(item) for item in command]
        result = subprocess.run(command, cwd=cwd, text=True, stdout=subprocess.PIPE,
                                stderr=subprocess.STDOUT, timeout=300)
        log = out / (name + '.log')
        log.write_text(result.stdout)
        commands.append(dict(name=name, command=command, cwd=str(cwd),
                             exitCode=result.returncode, path=str(log), sha256=sha(log)))
        if result.returncode and may_reject:
            pending.append(dict(gate=name, diagnostic=result.stdout, log=str(log)))
            return False
        checks.append(dict(name=name, passed=result.returncode == 0))
        if result.returncode:
            raise ValueError(name + ' failed: ' + str(log))
        return True

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization,
                ['c++', '-std=c++17', '-O' + optimization, *sources, '-o', binary])
            run('native-run-O' + optimization, [binary])
        objects = []
        for source in sources:
            name = source.stem.removeprefix('cpp_linked_exceptions_')
            directory = out / name
            directory.mkdir()
            run(name + '-stock-c', [args.stock_compiler.resolve(), '--c++17',
                '--gnu_version', '130300', '--clear_flag', 'use_predefined_macro_file',
                '--instantiate', 'used', source], cwd=directory)
            c_file = source.with_suffix('.int.c')
            if not c_file.is_file():
                c_file = directory / source.with_suffix('.int.c').name
            if not c_file.is_file():
                raise ValueError('Missing stock C lowering for ' + name)
            fixtures.append(dict(path=str(c_file), sha256=sha(c_file),
                                 purpose='Stock C lowering reference; not managed execution'))
            obj = directory / 'unit.dotobj'
            if not run(name + '-owned-export', [sys.executable, '-B', tools / 'compile.py',
                source, '--output', obj, '--work-dir', directory / 'work',
                '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions', '--rtti'],
                may_reject=args.allow_export_rejection):
                diagnostic = directory / 'work/stderr'
                if diagnostic.is_file():
                    pending[-1]['frontendDiagnostic'] = diagnostic.read_text()
                    pending[-1]['frontendDiagnosticPath'] = str(diagnostic)
                continue
            run(name + '-verify', ['dotnet', args.linker.resolve(), 'verify', obj])
            objects.append(obj)
        if not pending:
            generated = out / 'generated'
            run('link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
                        '--entry', 'main', '--allow-externs', *objects])
            run('bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', generated])
            run('build', ['dotnet', 'build', generated / 'Translated.csproj', '--nologo', '-v:q'])
            run('jit', ['dotnet', generated / 'bin/Debug/net10.0/Translated.dll'])
            if args.nativeaot:
                run('publish', ['dotnet', 'publish', generated / 'Translated.csproj',
                    '-r', args.rid, '-p:PublishAot=true', '-o', out / 'aot', '--nologo', '-v:q'])
                run('nativeaot', [out / 'aot/Translated'])
    except Exception as failure:
        error = str(failure)
    status = 'failed' if error else 'native-oracles-passed-export-pending' if pending else 'passed'
    result = dict(schema=1, status=status, error=error, checks=checks, commands=commands,
                  fixtures=fixtures, pending=pending, managedExecutionVerified=status == 'passed',
                  nativeAotVerified=args.nativeaot and status == 'passed',
                  compiler=dict(path=str(args.compiler.resolve()), sha256=sha(args.compiler)),
                  stockCompiler=dict(path=str(args.stock_compiler.resolve()), sha256=sha(args.stock_compiler)),
                  linker=dict(path=str(args.linker.resolve()), sha256=sha(args.linker)))
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=status, checks=len(checks), pending=len(pending), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
