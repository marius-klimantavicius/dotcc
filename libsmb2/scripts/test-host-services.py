#!/usr/bin/env python3
"""Native/JIT/NativeAOT controls for services used by the libsmb2 host profile."""
import argparse
import json
from common import ROOT, REPO, run, sha

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--no-build-tools', action='store_true')
args = parser.parse_args()
logs = ROOT / 'artifacts/host-services'
logs.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, cases=[])
try:
    if not args.no_build_tools:
        run(['dotnet', 'build', REPO / 'DotCC/DotCC.csproj', '-c', 'Release', '--nologo'],
            logs / 'compiler-build.log', receipt)
    compiler = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
    receipt['compiler'] = {p.name: sha(p) for p in compiler.parent.glob('*.dll')}
    for name in ['linux-endian-headers', 'socket-nonblocking', 'network-services']:
        fixture = REPO / 'DotCC.FunctionalTests/Fixtures' / name
        expected = (fixture / 'expected-stdout.txt').read_text().strip()
        out = ROOT / 'build/host-services' / name
        out.mkdir(parents=True, exist_ok=True)
        run(['cc', '-std=c17', '-D_DEFAULT_SOURCE', '-pthread', fixture / 'main.c', '-o', out / 'native'],
            logs / (name + '-native-build.log'), receipt)
        native = run([out / 'native'], logs / (name + '-native.log'), receipt, timeout=30).strip()
        if native != expected:
            raise RuntimeError('Native fixture differs: ' + name)
        generated = out / 'ManagedHostProbe'
        run(['dotnet', compiler, fixture / 'main.c', '--emit=csproj', '-o', generated],
            logs / (name + '-translate.log'), receipt)
        project = generated / 'ManagedHostProbe.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo'], logs / (name + '-jit-build.log'), receipt)
        jit = run(['dotnet', generated / 'bin/Release/net10.0/ManagedHostProbe.dll'],
                  logs / (name + '-jit.log'), receipt, timeout=30).strip()
        run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
             '-p:PublishAot=true', '-o', out / 'aot', '--nologo'],
            logs / (name + '-aot-build.log'), receipt)
        aot = run([out / 'aot/ManagedHostProbe'], logs / (name + '-aot.log'), receipt, timeout=30).strip()
        if jit != expected or aot != expected:
            raise RuntimeError('Managed fixture differs: ' + name)
        receipt['cases'].append(dict(name=name, passed=True, native=native, jit=jit, aot=aot))
        print(name + ': native/JIT/NativeAOT PASS', flush=True)
    if receipt['compiler'] != {p.name: sha(p) for p in compiler.parent.glob('*.dll')}:
        raise RuntimeError('Compiler changed during host-service validation')
    receipt['passed'] = True
finally:
    (logs / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
