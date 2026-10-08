#!/usr/bin/env python3
"""Canonical array-runtime parity, failure cleanup and first-pass termination."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'runtime'))
from arrays import expression, source
from bind import helper_sources


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    env = os.environ.copy()
    env.update(DOTNET_DbgEnableMiniDump='0', COMPlus_DbgEnableMiniDump='0')
    checks, commands = [], []

    def run(label, command, fatal=False):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, env=env, text=True, capture_output=True, timeout=300)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        commands.append(dict(label=label, command=command, exit=result.returncode))
        if fatal:
            if result.returncode in (0, 10, 11) or 'BUG:cleanup-unwound' in result.stdout or \
                    'Array destructor threw during unwinding' not in result.stderr:
                raise ValueError(label + ': expected first-pass termination')
        elif result.returncode:
            raise ValueError(label + ': ' + result.stderr)
        checks.append(label)
        return result.stdout

    error = None
    try:
        for suffix in ('cpp', 'cs'):
            shutil.copy2(ROOT / 'tests' / ('runtime_arrays.' + suffix), out / ('runtime_arrays.' + suffix))
        (out / 'Runtime.Arrays.cs').write_text(source(ROOT.parent / 'backend', threads=True))
        copied = helper_sources({'NonlocalJump', 'LibcxxThreads'})
        for name, path in copied.items():
            shutil.copy2(path, out / (name + '.cs'))
        (out / 'Arrays.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible><TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup></Project>''')
        run('native-build', ['c++', '-std=c++17', '-O2', out / 'runtime_arrays.cpp', '-o', out / 'native'])
        expected = run('native', [out / 'native'])
        run('build', ['dotnet', 'build', out / 'Arrays.csproj', '--nologo', '-v:q'])
        targets = [('jit', ['dotnet', out / 'bin/Debug/net10.0/Arrays.dll'])]
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', out / 'Arrays.csproj', '-r', args.rid,
                            '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '--nologo', '-v:q'])
            targets.append(('aot', [out / 'aot/Arrays']))
        for name, command in targets:
            if run(name, command) != expected:
                raise ValueError(name + ': native trace mismatch')
            checks.append(name + '-native-parity')
            if run(name + '-jump', [*command, 'jump']) != 'c0,c1,c2,jump,\n':
                raise ValueError('Nonlocal jump must bypass C++ destruction')
            if run(name + '-thread', [*command, 'thread']) != 'c0,c1,c2,d1,d0,thread,\n':
                raise ValueError('Thread exit must clean completed construction')
            for mode, expected_trace in [('fatal-ctor', 'c0,c1,c2,d1,'), ('fatal-dtor', 'd3,d2,')]:
                if run(name + '-' + mode, [*command, mode], fatal=True) != expected_trace:
                    raise ValueError(mode + ': unexpected termination trace')
        valid = dict(symbol='__cxa_vec_ctor', result='void', variadic=False,
                     arguments=[('void*', 'p'), ('ulong', 'n'), ('ulong', 's'),
                                ('delegate* managed<void*, void>', 'c'), ('delegate* managed<void*, void>', 'd')])
        assert expression(valid)
        for key, value in [('result', 'void*'), ('variadic', True),
                           ('arguments', [('int*', 'p'), *valid['arguments'][1:]])]:
            try:
                expression(dict(valid, **{key: value}))
            except ValueError:
                checks.append('reject-array-abi-' + key)
            else:
                raise ValueError('Accepted incompatible array ABI: ' + key)
    except Exception as failure:
        error = str(failure)
    receipt = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
                   nativeAotVerified=args.nativeaot and not error,
                   scope='Shared runtime algorithm; full translated C++ exception constructs remain a separate acceptance gate',
                   canonicalTemplateSha256=hashlib.sha256((ROOT.parent / 'backend/dot_gen_arrays.h').read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
