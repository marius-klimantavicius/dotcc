#!/usr/bin/env python3
"""Differentially execute three real primitive libuv units and isolated domains."""
import argparse
import hashlib
import json
import pathlib
import shutil
import subprocess
import sys

HERE = pathlib.Path(__file__).resolve().parent
ROOT = HERE.parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--aot', action='store_true')
parser.add_argument('--postprocess', action='store_true')
parser.add_argument('--compiler', type=pathlib.Path, default=ROOT / 'libuv/build/compiler/bin/DotCC/release/dotcc.dll')
args = parser.parse_args()
config = json.loads((ROOT / 'libuv/config/sources.json').read_text())
ref = ROOT / 'libuv/ref' / config['product']['directory']
out = ROOT / 'libuv/artifacts/common-closure'
out.mkdir(parents=True, exist_ok=True)
(out / 'result.json').write_text(json.dumps({'status': 'running', 'diagnostic_only': True}) + '\n')
commands = []
def run(command, capture=False):
    commands.append([str(item) for item in command])
    return subprocess.run(commands[-1], check=True, cwd=ROOT, capture_output=capture, text=True).stdout
units = ['version', 'strtok', 'strscpy']
objects = []
for unit in units:
    run([sys.executable, HERE / 'materialize-native-closure.py', 'src/' + unit + '.c'])
    source = ROOT / 'libuv/artifacts/native-closures/linux-glibc-x64/src' / unit / 'closure.c'
    obj = out / (unit + '.o')
    run(['dotnet', args.compiler, source, '--emit=obj', '-o', obj])
    objects.append(obj)
for name in ['DomainA', 'DomainB']:
    run(['dotnet', args.compiler, *objects, '--emit=managedlib', '--class-name', name,
         '--namespace', 'Managed.Libuv.Probes', '--nest-types', '--runtime=c', '-o', out / name])
project = out / 'DomainA/DomainA.csproj'
project.write_text(project.read_text().replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType>'))
shutil.copyfile(out / 'DomainB/DomainB.cs', out / 'DomainA/DomainB.cs')
shutil.copyfile(HERE / 'CommonConsumer.cs', out / 'DomainA/Program.cs')
oracle = out / 'native'
run(['cc', '-std=gnu11', '-D_GNU_SOURCE', '-I' + str(ref / 'include'), '-I' + str(ref / 'src'),
     HERE / 'common-oracle.c', *[ref / ('src/' + unit + '.c') for unit in units], '-o', oracle])
expected = run([oracle], capture=True)
# Recompile the selected native declaration slices as a second oracle leg.
closure_oracle = out / 'native-closure'
run(['cc', '-std=gnu11', '-D_GNU_SOURCE', '-I' + str(ref / 'include'), '-I' + str(ref / 'src'),
     HERE / 'common-oracle.c', *[ROOT / 'libuv/artifacts/native-closures/linux-glibc-x64/src' / unit / 'closure.c' for unit in units], '-o', closure_oracle])
if run([closure_oracle], capture=True) != expected:
    raise SystemExit('Native declaration snapshot differs from original native source.')
results = []
def check(label, command):
    actual = run(command, capture=True)
    if actual != expected:
        raise SystemExit(label + ' mismatch: ' + repr(actual) + ' != ' + repr(expected))
    results.append({'mode': label, 'status': 'passed', 'stdout': actual})
run(['dotnet', 'build', project, '-c', 'Release', '--verbosity', 'quiet'])
check('raw-jit', ['dotnet', out / 'DomainA/bin/Release/net10.0/DomainA.dll'])
if args.aot:
    run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', out / 'aot'])
    check('raw-nativeaot', [out / 'aot/DomainA'])
if args.postprocess:
    run(['dotnet', ROOT / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll', project, '--in-place'])
    run(['dotnet', 'build', project, '-c', 'Release', '--verbosity', 'quiet'])
    check('processed-jit', ['dotnet', out / 'DomainA/bin/Release/net10.0/DomainA.dll'])
    if args.aot:
        run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', out / 'aot-processed'])
        check('processed-nativeaot', [out / 'aot-processed/DomainA'])
(out / 'result.json').write_text(json.dumps({'status': 'passed', 'diagnostic_only': True, 'target': 'linux-glibc-x64',
    'source_revision': config['product']['revision'], 'units': units, 'modes': results,
    'compiler_library_sha256': hashlib.sha256((args.compiler.parent / 'DotCC.Lib.dll').read_bytes()).hexdigest(),
    'native_snapshot_matches_original': True, 'native_expected': expected, 'commands': commands}, indent=2) + '\n')
