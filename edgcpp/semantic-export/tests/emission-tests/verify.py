#!/usr/bin/env python3
"""Differential C -> package -> C# execution and fail-closed emission checks.

Consumes existing packages: this script never builds or runs the EDG frontend.
Every command, emitted source, oracle output and diagnostic is retained.
"""
import argparse
import copy
import hashlib
import json
from pathlib import Path
import platform
import shlex
import shutil
import subprocess
import time


HERE = Path(__file__).resolve().parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--emitter', required=True, help='Command prefix, e.g. "dotnet /path/Edg.Semantic.dll"')
parser.add_argument('--packages', required=True, type=Path,
                    help='Directory containing all fixture packages (see README.md)')
parser.add_argument('--output', required=True, type=Path, help='Fresh evidence directory')
parser.add_argument('--cc', default='gcc', help='Native C oracle compiler command')
parser.add_argument('--runtime', default='linux-x64', help='NativeAOT RID (default: linux-x64)')
parser.add_argument('--compare-generated', type=Path, help='Compare generated source to an earlier complete verification directory')
parser.add_argument('--emission-only', action='store_true', help='With --compare-generated, verify an alternate emitter without rebuilding identical generated programs')
args = parser.parse_args()
if args.emission_only and not args.compare_generated:
    parser.error('--emission-only requires --compare-generated')
args.packages = args.packages.resolve()
args.output = args.output.resolve()
args.output.mkdir(parents=True, exist_ok=False)
emitter = shlex.split(args.emitter)
checks = []


def run(name, command, *, cwd=None, failure=False):
    started = time.monotonic()
    result = subprocess.run([str(a) for a in command], cwd=cwd, capture_output=True, text=True, timeout=300)
    (args.output / (name + '.stdout')).write_text(result.stdout)
    (args.output / (name + '.stderr')).write_text(result.stderr)
    (args.output / (name + '.command.json')).write_text(json.dumps([str(a) for a in command], indent=2) + '\n')
    if (result.returncode == 0) == failure:
        raise AssertionError(f'{name}: exit {result.returncode}: {result.stderr}\n{result.stdout}')
    if failure and (result.returncode != 1 or not result.stderr.strip() or 'Unhandled exception' in result.stderr):
        raise AssertionError(f'{name}: expected a controlled rejection, got exit {result.returncode}: {result.stderr}')
    checks.append({'name': name, 'exit': result.returncode, 'seconds': round(time.monotonic() - started, 3)})
    return result.stdout


def native_include(source):
    # JSON quoting also produces a valid C quoted path on supported hosts.
    return '#include ' + json.dumps(str(source)) + '\n#include <stdio.h>\n'


def package_source(package_name, filename):
    package = args.packages / package_name
    manifest = json.loads((package / 'package.json').read_text())
    matches = [s for s in manifest['sources']
               if s.get('available', True) and Path(s.get('originalPath', s.get('path', ''))).name == filename]
    if len(matches) != 1:
        # Snapshot paths may be node-prefixed. Original path is retained on the
        # source-file node by every supported exporter version.
        graph = json.loads((package / manifest['graph']['path']).read_text())
        nodes = {n['id']: n for n in graph['nodes']}
        matches = [s for s in manifest['sources'] if s.get('available', True)
                   and Path(nodes[s['node']].get('path', nodes[s['node']].get('name', ''))).name == filename]
    if len(matches) != 1:
        raise AssertionError(f'Cannot identify {filename} in {package}')
    source = package / matches[0]['path']
    # Compile the exact exported snapshot, while keeping adjacent headers under
    # their include names; no dependency on the current checkout's source bytes.
    destination = args.output / (package_name + '-native-source')
    destination.mkdir()
    for entry in manifest['sources']:
        if not entry.get('available', True):
            continue
        original = entry.get('originalPath', entry.get('path', ''))
        shutil.copyfile(package / entry['path'], destination / Path(original).name)
    target = destination / filename
    shutil.copyfile(source, target)
    return target


def plain_runners(source):
    arrays = [[], [-1], [0], [7], [-1, 2, -3, 4], [-2147483648, 0, 17], list(range(-10, 11))]
    c = native_include(source) + 'int main(void) {\n'
    cs = 'using System;\nunsafe class Program { static void Main() {\n'
    c += 'printf("size:%zu\\n", sizeof(Item));\n'
    cs += 'Console.WriteLine("size:" + sizeof(SemanticModule.Item));\n'
    for index, values in enumerate(arrays):
        safe_values = values or [0]
        c += 'Item items%d[] = {%s};\n' % (index, ', '.join('{%d}' % v for v in safe_values))
        cs += 'SemanticModule.Item* items%d = stackalloc SemanticModule.Item[%d];\n' % (index, len(safe_values))
        for position, value in enumerate(safe_values):
            cs += f'items{index}[{position}].value = {value};\n'
        for count in sorted(set([-1, 0, len(values)])):
            c += f'printf("{index}/{count}:%d\\n", sum(items{index}, {count}));\n'
            cs += f'Console.WriteLine("{index}/{count}:" + SemanticModule.sum(items{index}, {count}));\n'
    return c + 'return 0; }\n', cs + '} }\n'


def scalar_runners(source):
    c = native_include(source) + 'int main(void) {\n'
    cs = 'using System;\nclass Program { static void Main() {\n'
    cases = []
    for value in [-97, -7, -1, 0, 1, 2, 7, 31, 97]:
        for name in ['twice', 'classify', 'names', 'short_circuit']:
            cases.append((name, [str(value)], False))
        for right in [-3, 0, 5]:
            cases.append(('truth', [str(value), str(right)], False))
            cases.append(('dangling_else', [str(value), str(right)], False))
        for divisor in [-7, -2, 1, 3]:
            cases.append(('signed_ops', [str(value), str(divisor)], False))
    for count in [-2, 0, 1, 2, 3, 4, 7, 20, 100]:
        for name in ['loop_sum', 'while_sum', 'nested_loops']:
            cases.append((name, [str(count)], False))
    for value in [0, 1, 0x7fffffff, 0x80000000, 0xffffffff, 0xdeadbeef]:
        for shift in [0, 1, 7, 31]:
            cases.append(('unsigned_ops', [f'{value}U', f'{shift}U'], True))
    for index, (name, values, unsigned) in enumerate(cases):
        call = f'{name}({", ".join(values)})'
        c += f'printf("{index}/{name}:%{ "u" if unsigned else "d" }\\n", {call});\n'
        cs += f'Console.WriteLine("{index}/{name}:" + SemanticModule.{call});\n'
    for value in [0, 1, 2, 0x7fffffffffffffff, 0x8000000000000000, 0xffffffffffffffff]:
        c += f'printf("wide/{value}:%llu\\n", unsigned_wide({value}ULL));\n'
        cs += f'Console.WriteLine("wide/{value}:" + SemanticModule.unsigned_wide({value}UL));\n'
    for value in [0, 1, 9223372036854775807]:
        c += f'printf("signed-wide/{value}:%lld\\n", signed_wide({value}LL));\n'
        cs += f'Console.WriteLine("signed-wide/{value}:" + SemanticModule.signed_wide({value}L));\n'
    for value in [-257, -256, -1, 0, 1, 255, 256, 257, 2147483647]:
        c += f'printf("narrow/{value}:%d\\n", narrow({value}));\n'
        cs += f'Console.WriteLine("narrow/{value}:" + SemanticModule.narrow({value}));\n'
    return c + 'return 0; }\n', cs + '} }\n'


def pointer_runners(source):
    c = native_include(source) + 'int main(void) { int value = 7;\n'
    cs = 'using System;\nunsafe class Program { static void Main() { int value = 7;\n'
    for pointer in ['(int*)0', '&value']:
        for name, arguments in [('pointer_truth', [pointer]), ('pointer_not', [pointer])] + [
                ('pointer_and', [pointer, str(flag)]) for flag in [-1, 0, 1]]:
            call = f'{name}({", ".join(arguments)})'
            c += f'printf("%d\\n", {call});\n'
            cs += f'Console.WriteLine(SemanticModule.{call});\n'
    return c + 'return 0; }\n', cs + '} }\n'


PROJECT = '''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsAotCompatible>true</IsAotCompatible>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <CheckForOverflowUnderflow>true</CheckForOverflowUnderflow>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup><Compile Include="Program.cs" /><Compile Include="Generated.cs" /></ItemGroup>
</Project>
'''


for name, make_runners in [('plain', plain_runners), ('scalars', scalar_runners), ('pointers', pointer_runners)]:
    directory = args.output / name
    directory.mkdir()
    generated = directory / 'Generated.cs'
    run(name + '-emit', [*emitter, 'emit-csharp', args.packages / name, '--output', generated])
    generated_again = directory / 'Generated-again.cs'
    run(name + '-emit-deterministic', [*emitter, 'emit-csharp', args.packages / name, '--output', generated_again])
    if generated.read_bytes() != generated_again.read_bytes():
        raise AssertionError(f'{name}: emission is not deterministic')
    if name == 'plain':
        golden = HERE / 'plain.golden.cs'
        if generated.read_text() != golden.read_text():
            raise AssertionError(f'plain: generated source differs from reviewed golden {golden}')
    if args.compare_generated:
        previous = args.compare_generated / name / 'Generated.cs'
        if generated.read_bytes() != previous.read_bytes():
            raise AssertionError(f'{name}: alternate emitter output differs from {previous}')
    if args.emission_only:
        checks.append({'name': name + '-matches-verified-source',
                       'generatedSha256': hashlib.sha256(generated.read_bytes()).hexdigest()})
        continue
    native, managed = make_runners(package_source(name, name + '.c'))
    (directory / 'oracle.c').write_text(native)
    (directory / 'Program.cs').write_text(managed)
    (directory / 'Runner.csproj').write_text(PROJECT)
    run(name + '-gcc', [*shlex.split(args.cc), '-std=c11', '-O2', '-Wall', '-Wextra', '-Werror',
                       directory / 'oracle.c', '-o', directory / 'oracle'])
    expected = run(name + '-native', [directory / 'oracle'])
    run(name + '-jit-build', ['dotnet', 'build', directory / 'Runner.csproj', '-c', 'Release', '-o', directory / 'jit'])
    actual = run(name + '-jit', ['dotnet', directory / 'jit' / 'Runner.dll'])
    if actual != expected:
        raise AssertionError(f'{name}: JIT result differs from native C oracle; inspect *.stdout')
    run(name + '-aot-publish', ['dotnet', 'publish', directory / 'Runner.csproj', '-c', 'Release',
                              '-r', args.runtime, '-p:PublishAot=true', '-o', directory / 'aot'])
    actual = run(name + '-aot', [directory / 'aot' / 'Runner'])
    if actual != expected:
        raise AssertionError(f'{name}: NativeAOT result differs from native C oracle; inspect *.stdout')
    checks.append({'name': name + '-differential', 'cases': len(expected.splitlines()),
                   'generatedSha256': hashlib.sha256(generated.read_bytes()).hexdigest()})


def reject(name, package):
    target = args.output / (name + '-rejected.cs')
    run(name + '-reject', [*emitter, 'emit-csharp', package, '--output', target], failure=True)
    if target.exists():
        raise AssertionError(f'{name}: failed emission created an output file')
    sentinel = '// Existing output must survive failed emission.\n'
    target.write_text(sentinel)
    run(name + '-preserve', [*emitter, 'emit-csharp', package, '--output', target], failure=True)
    if target.read_text() != sentinel:
        raise AssertionError(f'{name}: failed emission overwrote an existing output file')


for name in ['probe', 'global', 'volatile', 'static-local', 'external-call', 'mixed-compound', 'cleanup', 'unsigned-negate']:
    reject(name, args.packages / name)
reject('standalone-graph', args.packages / 'scalars' / 'graph.json')


def mutated(name, mutate, *, rehash=True):
    package = args.output / ('malformed-' + name)
    shutil.copytree(args.packages / 'scalars', package)
    manifest_path = package / 'package.json'
    manifest = json.loads(manifest_path.read_text())
    graph_path = package / manifest['graph']['path']
    graph = json.loads(graph_path.read_text())
    mutate(graph)
    graph_path.write_text(json.dumps(graph) + '\n')
    if rehash:
        manifest['graph']['sha256'] = hashlib.sha256(graph_path.read_bytes()).hexdigest()
        manifest_path.write_text(json.dumps(manifest) + '\n')
    reject(name, package)


def manifest_mutated(name, mutate):
    package = args.output / ('malformed-' + name)
    shutil.copytree(args.packages / 'scalars', package)
    path = package / 'package.json'
    manifest = json.loads(path.read_text())
    mutate(manifest)
    path.write_text(json.dumps(manifest) + '\n')
    reject(name, package)


def boolean_cycle(graph):
    expression = next(n for n in graph['nodes'] if n.get('operator') == 'eok_not')
    expression.update(operands=expression['id'])


mutated('duplicate-id', lambda g: g['nodes'].append(copy.deepcopy(g['nodes'][0])))
mutated('dangling-root', lambda g: g.update(rootScope=999999999))
mutated('wrong-root-kind', lambda g: g.update(rootScope=g['primarySource']))
mutated('unknown-operator', lambda g: next(n for n in g['nodes'] if n.get('operator') == 'eok_multiply').update(operator='eok_future_extension'))
mutated('missing-operands', lambda g: next(n for n in g['nodes'] if n.get('operator') == 'eok_multiply').update(operands=0))
mutated('statement-cycle', lambda g: next(n for n in g['nodes'] if n['kind'] == 'statement' and n.get('next', 0)).update(next=next(n['id'] for n in g['nodes'] if n['kind'] == 'statement' and n.get('next', 0))))
mutated('boolean-expression-cycle', boolean_cycle)
mutated('wrong-signedness-type', lambda g: next(n for n in g['nodes'] if n.get('form') == 'tk_integer' and n.get('size') == 4 and n.get('signed') is True).update(signed='yes'))
mutated('changed-graph-hash', lambda g: g.update(completeForEmission=True), rehash=False)
manifest_mutated('missing-graph-hash', lambda m: m['graph'].pop('sha256'))
manifest_mutated('missing-producer', lambda m: m.pop('producer'))
manifest_mutated('missing-producer-hash', lambda m: m['producer'].pop('sha256'))
manifest_mutated('missing-source-hash', lambda m: m['sources'][0].pop('sha256'))
manifest_mutated('missing-source-size', lambda m: m['sources'][0].pop('byteLength'))
manifest_mutated('missing-source-availability', lambda m: m['sources'][0].pop('available'))
manifest_mutated('source-unavailable', lambda m: m['sources'][0].update(available=False))
manifest_mutated('missing-source-node', lambda m: m['sources'].pop())
manifest_mutated('missing-package-diagnostics', lambda m: m.pop('diagnostics'))

(args.output / 'results.json').write_text(json.dumps({
    'host': platform.platform(), 'runtime': args.runtime, 'checks': checks, 'passed': len(checks),
    'emissionOnly': args.emission_only,
    'comparedGenerated': str(args.compare_generated.resolve()) if args.compare_generated else None,
}, indent=2) + '\n')
coverage = 'identical previously verified source, golden, rejection' if args.emission_only else 'native C, JIT, NativeAOT, golden, rejection'
print(f'PASS {len(checks)} semantic C# emission checks ({coverage})')
