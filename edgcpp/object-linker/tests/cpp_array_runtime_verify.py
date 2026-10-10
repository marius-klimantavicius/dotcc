#!/usr/bin/env python3
"""Independent shared-array runtime tests: exact prefixes, tails, transfers and termination."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--runtime', type=Path, default=Path(__file__).resolve().parent.parent / 'runtime/CppArrays.cs')
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    os.environ['DOTNET_DbgEnableMiniDump'] = '0'
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    shutil.copy2(args.runtime.resolve(), out / 'CppArrays.cs')
    shutil.copy2(Path(__file__).resolve().parent / 'ProductionArrayRuntimeTests.cs', out / 'ProductionArrayRuntimeTests.cs')
    project = out / 'ArrayRuntime.csproj'
    project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
        '<OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>'
        '</PropertyGroup></Project>\n')
    checks, commands = [], []
    def check(name, condition):
        if not condition: raise AssertionError(name)
        checks.append(name)
    def run(name, command, fails=False):
        command = list(map(str, command)); result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout); (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode != 0 if fails else result.returncode == 0)
        return result
    error = None
    try:
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
        runtimes = [('jit', ['dotnet', out / 'bin/Release/net10.0/ArrayRuntime.dll'])]
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'publish', '--nologo', '-v:q'])
            runtimes.append(('nativeaot', [out / 'publish/ArrayRuntime']))
        normal = None
        for name, command in runtimes:
            result = run(name, command)
            check(name + '-contracts', b'production-array-runtime checks=' in result.stdout and not result.stderr)
            if normal is None: normal = result.stdout
            check(name + '-runtime-parity', result.stdout == normal)
            for failure, events in [('cleanup', 'D3 D2'), ('destroy', 'D3 D2'), ('construct', 'C0 C1 C2 C3 D2'),
                                    ('copy', 'X0 X1 X2 X3 D2'), ('transfer-then-failure', 'D3 D2')]:
                result = run(name + '-failure-' + failure, command + [failure], True)
                check(name + '-terminates-' + failure, b'Array destructor threw during unwinding' in result.stderr)
                check(name + '-terminates-before-tail-' + failure, result.stdout.decode().splitlines() == events.split())
    except Exception as exception: error = str(exception)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
        runtime=dict(path=str(args.runtime.resolve()), sha256=hashlib.sha256((out / 'CppArrays.cs').read_bytes()).hexdigest()),
        limitations=['Direct runtime contracts only; translated source and native EDG parity are covered separately.',
                     'Primary failures are managed stand-ins; abandoning a propagating CppException carrier through a forced platform transfer requires separate payload/thread-lifecycle integration.'])
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)

if __name__ == '__main__': raise SystemExit(main())
