"""Source-like record initializer shape, relocation and storage-isolation checks."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys


def audit_generated(directory):
    text = '\n'.join(p.read_text() for p in sorted(directory.glob('Translated*.cs')))
    alias = re.search(r'\bvar\s+(@?table(?:_\w+)?)\s*=\s*\(\s*(?:global::)?(?:@?\w+\.)*@?TokenRow\s*\*\s*\)', text)
    assert alias, 'record table initialization should use a named typed pointer alias'
    rows = re.findall(r'\b' + re.escape(alias.group(1)) + r'\s*\[\s*(\d+)\s*\]\s*=\s*new\s*\(\s*\)\s*\{(.*?)\}\s*;', text, re.S)
    assert rows, 'record table rows should use indexed target-typed object initializers'
    first = dict(rows)['0']
    assert re.search(r'\blength\s*=\s*5\s*,', first), first
    assert re.search(r'\btoken\s*=\s*65535\s*,', first), first
    assert re.search(r'\bflags\s*=\s*32768\s*,', first), first
    assert not re.search(r'\(\s*ushort\s*\)', first), first
    assert all(not body.strip() or body.rstrip().endswith(',') for _, body in rows), 'retain trailing field commas'
    assert re.search(r'__edg_literal\s*\(\s*\d+\s*/\*\s*"alpha"', first), first
    assert '__edg_literal(' in text
    assert re.search(r'\bnested_rows\s*\[\s*0\s*\]\s*=\s*new\s*\(\s*\)\s*\{.*?\bpoint\s*=\s*new\s*\(\s*\)\s*\{', text, re.S), 'nested plain records should use nested object initializers'
    # Only record tables without overlapping/bitfield/array layout are eligible.
    assert not re.search(r'\b(?:union_rows|bit_rows|array_rows)\s*\[\s*\d+\s*\]\s*=\s*new\s*\(\s*\)', text)
    return {'named_table_alias': alias.group(1), 'table_object_initializer_count': len(rows),
            'literal_helper_calls': len(re.findall(r'__edg_literal\s*\(\s*\d+', text)),
            'generated_bytes': len(text.encode())}


def verify(root, tests, out, nativeaot, run, compiler=None):
    source = tests / 'record_initializer_readability.cpp'
    oracle = out / 'oracle.cpp'
    oracle.write_text('#include <cstdio>\n#include <thread>\n#include <initializer_list>\n'
                      f'#include "{source}"\n' + '''
int main() {
  std::printf("initial:%u\\n", ri_snapshot());
  std::printf("tls-initial:%u\\n", ri_tls_snapshot());
  for (unsigned n : {0u, 65u, 65535u}) std::printf("local:%u\\n", ri_local(n));
  std::printf("fallback:%u\\n", ri_fallback());
  ri_mutate(); ri_tls_mutate();
  std::printf("mutated:%u\\n", ri_snapshot());
  std::printf("independent:%u\\n", ri_independent());
  std::printf("tls-mutated:%u\\n", ri_tls_snapshot());
  unsigned fresh = 0;
  std::thread worker([&] { fresh = ri_tls_snapshot(); ri_tls_mutate(); });
  worker.join();
  std::printf("tls-fresh:%u\\n", fresh);
  std::printf("tls-retained:%u\\n", ri_tls_snapshot());
}
''')
    clang = shutil.which('clang++') or shutil.which('clang++-18') or shutil.which('clang++-19')
    if not clang:
        raise RuntimeError('Clang required for the second independent source oracle')
    traces = []
    for label, frontend in [('gcc', 'g++'), ('clang', clang)]:
        executable = out / ('oracle-' + label)
        run([frontend, '-std=c++17', '-O2', '-pthread', oracle, '-o', executable])
        traces.append(run([executable]).stdout)
    assert traces[0] == traces[1], traces
    expected = traces[0]
    (out / 'expected.txt').write_text(expected)
    values = dict(line.split(':') for line in expected.splitlines() if not line.startswith('local:'))
    assert values['initial'] != values['mutated']
    assert values['tls-initial'] == values['tls-fresh']
    assert values['tls-mutated'] == values['tls-retained'] != values['tls-initial']
    assert values['independent'] == values['fallback'] == '1'
    checks = ['GCC and Clang agree on record fields and relocations, local copies, independent arrays and per-thread storage']
    audits = {}
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        run([sys.executable, root / 'dotnet.py', source, '--std', 'c++17', '--emit', 'managedlib',
             '--class-name', 'Core', '--namespace', 'Record.Initializers', '--split', 'function',
             '--output-dir', directory, *(['--compiler', compiler] if compiler else []),
             *(['--instance-methods'] if instance else [])])
        audits[mode] = audit_generated(directory)
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        def call(name, receiver='owner'):
            matches = [item['method'] for item in manifest['exports'] if item['symbol'] == name]
            assert len(matches) == 1, (name, matches)
            return (receiver if instance else 'Core') + '.' + matches[0]
        host = out / ('host-' + mode)
        host.mkdir()
        (host / 'Host.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
<Nullable>enable</Nullable></PropertyGroup><ItemGroup>
<ProjectReference Include="../{mode}/Generated.csproj" /></ItemGroup></Project>''')
        owner_decl = 'using var owner = new Core(); using var second = new Core();' if instance else ''
        isolation = f'''
if ({call('ri_snapshot', 'second')}() != {values['initial']}u) return 21;
if ({call('ri_fallback', 'second')}() != 1u) return 24;
if ({call('ri_tls_snapshot', 'second')}() != {values['tls-initial']}u) return 22;
{call('ri_tls_mutate', 'second')}();
if ({call('ri_tls_snapshot', 'second')}() != {values['tls-mutated']}u) return 23;
''' if instance else ''
        release = 'owner.ReleaseThread();' if instance else 'Core.ReleaseThread();'
        (host / 'Program.cs').write_text(f'''using System;
using System.Threading;
using Record.Initializers;
unsafe class EntryPoint {{
static int Main() {{
{owner_decl}
Console.WriteLine($"initial:{{{call('ri_snapshot')}()}}");
Console.WriteLine($"tls-initial:{{{call('ri_tls_snapshot')}()}}");
foreach (uint seed in new uint[] {{0, 65, 65535}}) Console.WriteLine($"local:{{{call('ri_local')}(seed)}}");
Console.WriteLine($"fallback:{{{call('ri_fallback')}()}}");
{call('ri_mutate')}(); {call('ri_tls_mutate')}();
GC.Collect(2, GCCollectionMode.Forced, true, true);
if ({call('ri_fallback')}() != 1u) return 25;
Console.WriteLine($"mutated:{{{call('ri_snapshot')}()}}");
Console.WriteLine($"independent:{{{call('ri_independent')}()}}");
Console.WriteLine($"tls-mutated:{{{call('ri_tls_snapshot')}()}}");
uint fresh = 0;
var worker = new Thread(() => {{fresh = {call('ri_tls_snapshot')}(); {call('ri_tls_mutate')}(); {release}}});
worker.Start(); worker.Join();
Console.WriteLine($"tls-fresh:{{fresh}}");
Console.WriteLine($"tls-retained:{{{call('ri_tls_snapshot')}()}}");
{isolation}
{release}
return 0;
}}
}}
''')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo',
             '-p:TreatWarningsAsErrors=true'])
        commands = [('JIT', ['dotnet', host / 'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:DebugType=None',
                 '-p:DebugSymbols=false', '-o', host / 'aot'])
            commands.append(('NativeAOT', [host / 'aot/Host']))
        for runtime, command in commands:
            actual = run(command).stdout
            (host / (runtime + '-trace.txt')).write_text(actual)
            assert actual == expected, (mode, runtime, actual, expected)
            checks.append(f'{mode}/{runtime}: native record traces, forced GC, independent copies, owner and TLS isolation')
    return {'status': 'passed', 'checks': checks, 'audits': audits,
            'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest()}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, timeout=600, **kwargs)
        index = len(commands)
        (output / f'command-{index:03d}.stdout').write_text(result.stdout)
        (output / f'command-{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (output / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'{command}: exit {result.returncode}; see retained logs')
        return result
    results = verify(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent,
                     output, args.nativeaot, run, args.compiler.resolve() if args.compiler else None)
    (output / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
    print(json.dumps(results, indent=2))
