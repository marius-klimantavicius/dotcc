#!/usr/bin/env python3
"""CLR name limits: expanded templates, full source maps and bounded collisions."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def bounded(identifier):
    value = 14695981039346656037
    for byte in identifier.encode('ascii'):
        value = ((value ^ byte) * 1099511628211) & ((1 << 64) - 1)
    return identifier[:128] + '__h' + f'{value:016x}' + '__' + identifier[-48:]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    long_name = 'public_' + 'identifier_' * 120
    sibling = long_name[:-1] + 'X'
    collision = bounded(long_name)
    atom = 'PayloadWithAnIntentionallyLongButStillOrdinaryCppSourceTypeNameForExpandedTemplates'
    fixture = out / 'name-limits.cpp'
    fixture.write_text(f'''namespace Expanded {{
struct {atom} {{ }};
template<class T, class U> struct Pair {{ }};
using L0 = {atom};
using L1 = Pair<L0,L0>;
using L2 = Pair<L1,L1>;
using L3 = Pair<L2,L2>;
using L4 = Pair<L3,L3>;
template<class T> struct Owner {{
  struct Record {{ int value; }};
  static int read(const Record& record) {{ return record.value; }}
}};
int check() {{
  Owner<L4>::Record left{{17}};
  Owner<Pair<L4,int>>::Record right{{23}};
  return Owner<L4>::read(left) + Owner<Pair<L4,int>>::read(right);
}}
}}
extern "C" int {collision}(int value) {{ return value+3; }}
extern "C" int {long_name}(int value) {{ return value+5; }}
extern "C" int {sibling}(int value) {{ return value+7; }}
extern "C" int ordinary_public_api(int value) {{
  int (*first)(int) = &{long_name};
  int (*second)(int) = &{sibling};
  return Expanded::check()+{collision}(value)+first(value)+second(value);
}}
int main() {{ return ordinary_public_api(1)==58 ? 0 : 61; }}
''')
    report = {'checks': [], 'commands': [], 'compiler': str(args.compiler.resolve()),
              'compilerSha256': hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
              'fixtureSha256': hashlib.sha256(fixture.read_bytes()).hexdigest()}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True)
        index = len(report['commands'])
        (out / f'{index:03d}.stdout').write_text(result.stdout)
        (out / f'{index:03d}.stderr').write_text(result.stderr)
        report['commands'].append({'command': command, 'exitCode': result.returncode})
        save()
        if result.returncode:
            raise RuntimeError(f'Command {index} failed: see {out}/{index:03d}.stderr')
        return result

    run(['g++', '-std=c++14', fixture, '-o', out / 'native'])
    run([out / 'native'])
    report['checks'].append('Native C++14 execution of nested templates and distinct colliding source functions')
    for mode, options in [('static', []), ('instance', ['--instance-methods', '--no-nest-types'])]:
        generated = out / mode
        raw = out / (mode + '.raw.cs')
        command = [sys.executable, ROOT / 'dotnet.py', fixture, '--compiler', args.compiler,
                   '--std', 'c++14', '--emit', 'managedlib', '--namespace', 'Names.Limits',
                   '--class-name', 'Core', '--output-dir', generated, '--raw-output', raw, *options]
        run(command)
        manifest = json.loads((generated / 'dot-gen.json').read_text())
        rows = json.loads((generated / manifest['sourceMap']['file']).read_text())['symbols']
        methods = {row['method'] for row in manifest['exports']}
        assert 'ordinary_public_api' in methods
        assert collision in methods and collision + '__export' in methods, 'Intentional bounded-name collision was not allocated distinct exports'
        assert bounded(sibling) in methods
        assert any(row['sourceName'] == long_name and len(row['managedName']) < 256 for row in rows)
        assert any(row['sourceName'] == sibling and len(row['managedName']) < 256 for row in rows)
        assert any(row['kind'] == 'type' and len(row['scope']) > 1023 and '__h' in row['managedName'] for row in rows), 'Expanded template metadata not exercised'
        assert all(len(row['managedName']) <= 900 for row in rows)
        report['checks'].append(mode + ': bounded expanded template names; distinct near-identical names and intentional hash-spelling collision; full original source and scope mapping')
        # Repeat actual frontend emission on identical paths, not a comparison
        # of normalized/postprocessed files. No libc snapshots are involved.
        replay = out / (mode + '-repeat')
        replay_raw = out / (mode + '-repeat.raw.cs')
        repeated = list(command)
        repeated[repeated.index('--output-dir') + 1] = replay
        repeated[repeated.index('--raw-output') + 1] = replay_raw
        run(repeated)
        assert raw.read_bytes() == replay_raw.read_bytes()
        report['checks'].append(mode + ': repeated raw output byte-identical')
        host = generated / 'host'; host.mkdir()
        (host / 'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
        setup = 'using var core=new Core();' if mode == 'instance' else ''
        receiver = 'core' if mode == 'instance' else 'Core'
        (host / 'Program.cs').write_text('using Names.Limits; class Program { static int Main() {' + setup + f'return {receiver}.ordinary_public_api(1)==58 ? 0 : 62;' + '} }')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
        report['checks'].append(mode + ': JIT direct original API and all collision paths execute correctly')
        if args.nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run([host / 'aot/Host'])
            report['checks'].append(mode + ': NativeAOT direct original API')
    report['passed'] = True
    save()
    print(f"{len(report['checks'])} source name limit checks passed")


if __name__ == '__main__':
    main()
