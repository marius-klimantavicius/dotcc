#!/usr/bin/env python3
"""Qualify real private fd-set storage against native Linux macros."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'scripts'))
from core_inputs import compiler_identity
base = ROOT / 'generated/host-fd-sets'
base.mkdir(parents=True, exist_ok=True)
a = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/host-fd-sets' / a.name
out.mkdir(parents=True)
compiler_dir = ROOT.parent / 'DotCC/bin/Release/net10.0'
cli = compiler_dir / 'dotcc.dll'
post = ROOT.parent / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
receipt = dict(kind='private fd-set memory operations; full guest select remains separate', passed=False, results={})

def save():
    (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

def run(command, label, timeout=180):
    with (out / (label + '.log')).open('wb') as log:
        try:
            code = subprocess.run(list(map(str, command)), stdout=log, stderr=subprocess.STDOUT,
                                  env=dict(os.environ, LC_ALL='C'), timeout=timeout).returncode
        except subprocess.TimeoutExpired:
            code = 124
    receipt['results'][label] = dict(command=list(map(str, command)), exit_code=code)
    save()
    if code:
        raise RuntimeError(label + ' failed: ' + str(out / (label + '.log')))
    return (out / (label + '.log')).read_bytes()

try:
    for name in ('probe.c', 'Program.cs'):
        shutil.copyfile(ROOT / 'tests/HostFdSets' / name, a / name)
    for name in ('HostFdSets.c', 'HostFdSets.h'):
        source = ROOT / 'src/Host' / ('include' if name.endswith('.h') else '') / name
        shutil.copyfile(source, a / name)
    receipt['inputs'] = {p.name: sha(p) for p in a.iterdir() if p.is_file()}
    receipt['compiler'] = compiler_identity(compiler_dir)
    receipt['postprocessor_sha256'] = sha(post)
    receipt['runner_sha256'] = sha(Path(__file__))
    save()
    common = ['cc', '-std=c17', '-D_GNU_SOURCE', '-Wall', '-Wextra', '-Werror']
    run([*common, a / 'probe.c', '-o', a / 'native'], 'native-build')
    expected = run([a / 'native'], 'native', 30)
    run([*common, '-DBLINK_PRIVATE_FD_SETS', '-I', a, a / 'probe.c', a / 'HostFdSets.c',
         '-o', a / 'native-private'], 'native-private-build')
    if run([a / 'native-private'], 'native-private', 30) != expected:
        raise RuntimeError('authored fd-set operations differ from native macros')
    objects = []
    for name in ('probe', 'HostFdSets'):
        target = a / (name + '.obj.cs')
        run(['dotnet', cli, '-std=c17', '-D_GNU_SOURCE', '-DBLINK_PRIVATE_FD_SETS',
             '-DBLINK_MANAGED_FD_SETS', '-I', a, a / (name + '.c'), '--emit=obj', '-o', target], 'emit-' + name)
        objects.append(target)
    raw, optimized = a / 'raw', a / 'optimized'
    run(['dotnet', cli, *objects, '--emit=managedlib', '--nest-types', '--class-name', 'Blink',
         '--namespace', 'Managed.Emulation', '--runtime=c', '-o', raw], 'link')
    if compiler_identity(compiler_dir) != receipt['compiler']:
        raise RuntimeError('compiler changed during generation')
    receipt['raw_generated'] = {p.name: sha(p) for p in raw.glob('*.cs')}
    shutil.copytree(raw, optimized)
    for label, generated in [('raw', raw), ('optimized', optimized)]:
        consumer = a / (label + '-consumer')
        consumer.mkdir()
        shutil.copyfile(a / 'Program.cs', consumer / 'Program.cs')
        xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
        props = ET.SubElement(xml, 'PropertyGroup')
        for key, value in [('TargetFramework', 'net10.0'), ('OutputType', 'Exe'), ('AllowUnsafeBlocks', 'true'),
                           ('Nullable', 'enable'), ('AssemblyName', 'HostFdSetsProbe'), ('WarningsAsErrors', 'CS8500')]:
            ET.SubElement(props, key).text = value
        items = ET.SubElement(xml, 'ItemGroup')
        ET.SubElement(items, 'Compile', Include=str(generated / '*.cs'))
        ET.SubElement(items, 'TrimmerRootAssembly', Include='HostFdSetsProbe')
        project = consumer / 'HostFdSetsProbe.csproj'
        ET.ElementTree(xml).write(project, encoding='unicode')
        if label == 'optimized':
            run(['dotnet', 'restore', project], 'optimized-restore')
            run(['dotnet', post, project, '--in-place'], 'postprocess')
        run(['dotnet', 'build', project, '-c', 'Release'], label + '-build')
        if run(['dotnet', consumer / 'bin/Release/net10.0/HostFdSetsProbe.dll'], label + '-jit', 40) != expected:
            raise RuntimeError(label + ' JIT differs from native')
        publish = a / (label + '-aot')
        run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', publish],
            label + '-aot-build', 300)
        if run([publish / 'HostFdSetsProbe'], label + '-aot', 40) != expected:
            raise RuntimeError(label + ' AOT differs from native')
        receipt['results'][label + '-aot']['binary_sha256'] = sha(publish / 'HostFdSetsProbe')
    if receipt['raw_generated'] != {p.name: sha(p) for p in raw.glob('*.cs')}:
        raise RuntimeError('raw generated source changed')
    receipt['optimized_generated'] = {p.name: sha(p) for p in optimized.glob('*.cs')}
    receipt['passed'] = True
    print(str(out / 'receipt.json'))
finally:
    save()
