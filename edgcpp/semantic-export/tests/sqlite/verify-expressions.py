#!/usr/bin/env python3
"""Native/JIT/NativeAOT regressions for project-emitter expression semantics."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--package', type=Path, required=True)
parser.add_argument('--generated', type=Path, required=True)
parser.add_argument('--output', type=Path, required=True)
args = parser.parse_args()
args.package = args.package.resolve()
args.generated = args.generated.resolve()
args.output = args.output.resolve()
args.output.mkdir(parents=True, exist_ok=False)
checks = []


def run(name, command):
    command = [str(value) for value in command]
    (args.output / (name + '.command.json')).write_text(json.dumps(command, indent=2) + '\n')
    result = subprocess.run(command, capture_output=True, text=True, timeout=600)
    (args.output / (name + '.stdout')).write_text(result.stdout)
    (args.output / (name + '.stderr')).write_text(result.stderr)
    if result.returncode:
        raise AssertionError(f'{name}: exit {result.returncode}; see retained stdout/stderr')
    checks.append(name)
    return result.stdout


manifest = json.loads((args.package / 'package.json').read_text())
source = next(s for s in manifest['sources'] if Path(s['originalPath']).name == 'expressions.c')
data = (args.package / source['path']).read_bytes()
assert hashlib.sha256(data).hexdigest() == source['sha256']
(args.output / 'expressions.c').write_bytes(data)
cases = []
for value in [-2, 0, 1, 7, 13]:
    cases += [('volatile_operations', [value]), ('callback_comma', [value]), ('bitfield_evaluate_once', [value])]
for value in [0, 1, 3, 7, 8, 15]:
    for divisor in [-3, -1, 1, 2, 5]:
        cases.append(('narrow_division', [value, divisor]))
for value in [0, 1, 7, 2147483647]:
    cases.append(('wide_bitfield_division', [value]))
for value in [-8, -3, -1, 0, 1, 7]:
    for shift in [0, 1, 3]:
        cases.append(('signed_bitfield_shift', [value, shift]))

native = '#include "expressions.c"\n#include <stdio.h>\nint main(void) {\n'
managed = 'using System;\nusing static Edg.Semantic.Generated.SqliteCore;\nclass Program { static void Main() {\n'
for index, (name, values) in enumerate(cases):
    call = name + '(' + ', '.join(map(str, values)) + ')'
    native += f'printf("{index}/{name}:%d\\n", {call});\n'
    managed += f'Console.WriteLine("{index}/{name}:" + {call});\n'
native += 'return 0; }\n'
managed += '} }\n'
(args.output / 'oracle.c').write_text(native)
consumer = args.output / 'consumer'
consumer.mkdir()
(consumer / 'Program.cs').write_text(managed)
(consumer / 'Consumer.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="{args.generated / 'Generated.csproj'}" /></ItemGroup></Project>
''')

# Ordinary RAM cannot count volatile accesses. Inspect the generated concrete
# mutation helpers in addition to execution: at most one RHS dereference of
# their target and no reread after the store or directly in the return value.
helpers = (args.generated / 'ExpressionHelpers.cs').read_text()
mutations = [line for line in helpers.splitlines() if 'MemoryBarrier' in line and 'p1)' in line]
assert mutations, 'No volatile mutation helpers found'
for helper in mutations:
    assert 'return *p1' not in helper, helper
    assert len(re.findall(r'=\s*\*p1\s*;', helper)) <= 1, helper
    assert '*p1 = value;' in helper and ('return value;' in helper or 'return old;' in helper), helper
    assert '*p1' not in helper.split('*p1 = value;', 1)[1], helper
checks.append('volatile-helper-single-access')

run('native-build', ['gcc', '-std=c17', '-O2', '-Wall', '-Wextra', args.output / 'oracle.c', '-o', args.output / 'oracle'])
expected = run('native-run', [args.output / 'oracle'])
run('jit-build', ['dotnet', 'build', consumer / 'Consumer.csproj', '-c', 'Release', '-o', args.output / 'jit'])
actual = run('jit-run', ['dotnet', args.output / 'jit' / 'Consumer.dll'])
assert actual == expected, 'JIT differs from native oracle'
run('aot-build', ['dotnet', 'publish', consumer / 'Consumer.csproj', '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', args.output / 'aot'])
actual = run('aot-run', [args.output / 'aot' / 'Consumer'])
assert actual == expected, 'NativeAOT differs from native oracle'
(args.output / 'results.json').write_text(json.dumps({'checks': checks, 'passed': len(checks), 'observations': len(cases)}, indent=2) + '\n')
print(f'PASS expression semantics: {len(checks)} checks, {len(cases)} observations')
