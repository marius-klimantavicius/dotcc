#!/usr/bin/env python3
"""Finite native/JIT/NativeAOT instance ABI qualification using the functional fixture."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import textwrap
import time

ROOT = Path(__file__).resolve().parents[2]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=Path(tempfile.gettempdir()) / 'dotcc-instance-methods')
    parser.add_argument('--compiler', type=Path, default=ROOT / 'DotCC/bin/Release/net10.0/dotcc.dll')
    args = parser.parse_args()
    args.compiler = args.compiler.resolve()
    args.output.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=args.output.resolve()))
    receipt = {'kind': 'explicit-instance-abi-native-jit-aot', 'status': 'running', 'commands': [], 'attempt': str(attempt)}
    receipt_path = attempt / 'receipt.json'
    def save():
        receipt_path.write_text(json.dumps(receipt, indent=2) + '\n')
    def run(command, expected=None):
        index = len(receipt['commands'])
        stdout = attempt / f'{index:02}.stdout'
        stderr = attempt / f'{index:02}.stderr'
        row = {'argv': list(map(str, command)), 'stdout': str(stdout), 'stderr': str(stderr)}
        receipt['commands'].append(row)
        save()
        begin = time.monotonic()
        with stdout.open('wb') as out, stderr.open('wb') as err:
            result = subprocess.run(row['argv'], cwd=attempt, stdout=out, stderr=err, timeout=300,
                                    env=dict(os.environ, DOTNET_CLI_TELEMETRY_OPTOUT='1'))
        row.update(exit_code=result.returncode, seconds=time.monotonic() - begin,
                   stdout_sha256=sha(stdout), stderr_sha256=sha(stderr))
        save()
        if result.returncode:
            raise RuntimeError(f'command {index} failed: {stderr}')
        if expected is not None and stdout.read_text().strip() != expected:
            raise RuntimeError(f'command {index} output mismatch: {stdout}')
    try:
        fixture = ROOT / 'DotCC.FunctionalTests/ManagedLibraryTests.InstanceMethods.cs'
        paths = [Path(__file__).resolve(), fixture]
        for directory in ('DotCC.Lib', 'DotCC.Libc', 'DotCC'):
            paths += [p for p in (ROOT / directory).rglob('*') if p.is_file()
                      and not {'bin', 'obj', '.idea'}.intersection(p.relative_to(ROOT / directory).parts)
                      and p.suffix in ('.cs', '.csproj', '.yaml', '.h')]
        paths += [p for p in args.compiler.parent.iterdir() if p.is_file()]
        receipt['inputs'] = {str(p.resolve()): sha(p) for p in sorted(set(paths))}
        shutil.copytree(args.compiler.parent, attempt / 'compiler')
        compiler = attempt / 'compiler' / args.compiler.name
        receipt['compiler_snapshot'] = {str(p): sha(p) for p in sorted((attempt / 'compiler').rglob('*')) if p.is_file()}
        source = fixture.read_text()
        def extract(name):
            match = re.search(r'internal const string ' + name + r' = """\n(.*?)\n        """;', source, re.S)
            if not match:
                raise RuntimeError(f'missing fixture {name}')
            return textwrap.dedent(match.group(1)) + '\n'
        probe = attempt / 'probe.c'
        probe.write_text(extract('InstanceSource'))
        native = attempt / 'native.c'
        native.write_text(probe.read_text() + '''
#include <stdio.h>
int main(void) {
  if (step(7) || tls()!=101 || spawn()!=8 || sort()!=1 || callbacks(address())!=30 || once_probe()!=108) return 1;
  puts("native-ok"); return 0;
}
''')
        run(['cc', '-std=c11', '-O2', '-pthread', native, '-o', attempt / 'native'])
        run([attempt / 'native'], 'native-ok')
        obj = attempt / 'probe.o.cs'
        run(['dotnet', compiler, probe, '--emit=obj', '--instance-methods', '-o', obj])
        generated = attempt / 'generated'
        run(['dotnet', compiler, obj, '--emit=managedlib', '--instance-methods', '--runtime=c',
             '--nest-types', '--literal-pool', '--deduplicate-inline', '--split=function', '--class-name=Api', '-o', generated])
        consumer = attempt / 'consumer'
        consumer.mkdir()
        (consumer / 'Program.cs').write_text(extract('InstanceHost').replace('POINTERS', 'Api.ApiFunctionPointers') + '''
public static class Entry {
 public static int Main() { string result=InstanceProbe.Run(); System.Console.WriteLine(result); return result=="ok" ? 0 : 1; }
}
''')
        (consumer / 'consumer.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
<ItemGroup><Compile Include="Program.cs"/><Compile Include="../generated/*.cs"/></ItemGroup>
</Project>\n''')
        run(['dotnet', 'build', consumer / 'consumer.csproj', '-c', 'Release', '-o', attempt / 'jit', '--nologo'])
        run(['dotnet', attempt / 'jit/consumer.dll'], 'ok')
        run(['dotnet', 'publish', consumer / 'consumer.csproj', '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-o', attempt / 'aot', '--nologo'])
        run([attempt / 'aot/consumer'], 'ok')
        for path, digest in (receipt['inputs'] | receipt['compiler_snapshot']).items():
            if sha(path) != digest:
                raise RuntimeError(f'input changed during run: {path}')
        outputs = [obj, probe, native, consumer / 'Program.cs', consumer / 'consumer.csproj']
        outputs += list(generated.glob('*.cs')) + [attempt / 'native', attempt / 'jit/consumer.dll', attempt / 'aot/consumer']
        receipt['outputs'] = {str(p): sha(p) for p in outputs}
        receipt['identity_checks'] = len(receipt['inputs']) + len(receipt['compiler_snapshot'])
        receipt['status'] = 'passed'
    except BaseException as error:
        receipt['status'] = 'failed'
        receipt['error'] = f'{type(error).__name__}: {error}'
        raise
    finally:
        save()
        print(receipt_path, flush=True)


if __name__ == '__main__':
    main()
