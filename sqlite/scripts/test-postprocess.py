#!/usr/bin/env python3
"""Explicit original/optimized snapshot verification; never a normal build hook."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import time
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('snapshot', type=Path)
parser.add_argument('--aot', action='store_true')
parser.add_argument('--corpora', action='store_true', help='postprocess existing core/API/JSONB/FTS5 executables and compare native baselines')
parser.add_argument('--runtime', default='linux-x64')
args = parser.parse_args()
snapshot = args.snapshot.resolve()
manifest = json.loads((snapshot / 'manifest.json').read_text())
artifacts = ROOT / 'artifacts' / 'postprocess'
artifacts.mkdir(parents=True, exist_ok=True)
build = ROOT / 'generated' / 'PostprocessConsumers'
build.mkdir(parents=True, exist_ok=True)
environment = os.environ.copy()
environment['TMPDIR'] = str(ROOT / 'artifacts/tmp')
Path(environment['TMPDIR']).mkdir(parents=True, exist_ok=True)
measurements = []


def execute(label, command, *, runtime=False):
    path = artifacts / (label + '.log')
    started = time.perf_counter()
    with path.open('w') as output:
        result = subprocess.run([str(x) for x in command], stdout=output, stderr=subprocess.STDOUT,
                                env=environment, timeout=120 if runtime else 600)
    measurements.append({'name': label, 'seconds': time.perf_counter() - started})
    if result.returncode:
        raise SystemExit(f'{label} failed ({result.returncode}); see {path}\n{path.read_text()[-6000:]}')
    return path.read_text()


def consumer_project(variant, name, library):
    directory = build / variant / name
    directory.mkdir(parents=True, exist_ok=True)
    project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    properties = ET.SubElement(project, 'PropertyGroup')
    for key, value in {'OutputType': 'Exe', 'TargetFramework': 'net10.0', 'AllowUnsafeBlocks': 'true',
                       'Nullable': 'enable', 'ImplicitUsings': 'enable', 'EnableDefaultCompileItems': 'false'}.items():
        ET.SubElement(properties, key).text = value
    items = ET.SubElement(project, 'ItemGroup')
    ET.SubElement(items, 'ProjectReference', Include=str(library))
    ET.SubElement(items, 'Compile', Include=str(ROOT / 'tests' / name / '*.cs'))
    path = directory / (name + '.csproj')
    ET.indent(project)
    path.write_text(ET.tostring(project, encoding='unicode') + '\n')
    return path


outputs = {}
benchmark = {}
for variant, key in [('Original', 'OriginalProject'), ('Optimized', 'OptimizedProject')]:
    library = snapshot / manifest[key]
    for name in ['ManagedConsumer', 'HostVfsTests', 'ThreadingTests', 'PostprocessBenchmark']:
        project = consumer_project(variant, name, library)
        execute(f'{variant}-{name}-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo'])
        assembly = project.parent / 'bin/Release/net10.0' / (name + '.dll')
        outputs[variant, name, 'jit'] = execute(f'{variant}-{name}-jit', ['dotnet', assembly], runtime=True)
        if name == 'HostVfsTests':
            execute(f'{variant}-host-processes-jit', ['python3', ROOT / 'scripts/test-host-vfs-processes.py',
                    '--managed', 'dotnet', assembly, '--native', ROOT / 'build/host-vfs-native'])
        if args.aot:
            publish = ROOT / 'build/postprocess' / variant / name
            execute(f'{variant}-{name}-publish', ['dotnet', 'publish', project, '-c', 'Release',
                    '-r', args.runtime, '-p:PublishAot=true', '-o', publish, '--nologo'])
            executable = publish / (name + ('.exe' if args.runtime.startswith('win-') else ''))
            outputs[variant, name, 'aot'] = execute(f'{variant}-{name}-aot', [executable], runtime=True)
            if name != 'PostprocessBenchmark' and outputs[variant, name, 'jit'] != outputs[variant, name, 'aot']:
                raise SystemExit(f'{variant} {name}: JIT/AOT transcripts differ')
            if name == 'HostVfsTests':
                execute(f'{variant}-host-processes-aot', ['python3', ROOT / 'scripts/test-host-vfs-processes.py',
                        '--managed', executable, '--native', ROOT / 'build/host-vfs-native'])
        if name == 'PostprocessBenchmark':
            for mode in ['jit', 'aot'] if args.aot else ['jit']:
                benchmark[variant + '-' + mode] = [json.loads(line) for line in outputs[variant, name, mode].splitlines()]
        print(f'PASS {variant} {name} JIT' + ('/NativeAOT' if args.aot else ''), flush=True)
    if variant == 'Optimized':
        for name in ['ManagedConsumer', 'HostVfsTests', 'ThreadingTests']:
            for mode in ['jit', 'aot'] if args.aot else ['jit']:
                if outputs['Original', name, mode] != outputs['Optimized', name, mode]:
                    raise SystemExit(f'{name} {mode}: original/optimized transcripts differ')

if args.corpora:
    tool = ROOT.parent / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
    corpus_root = Path(tempfile.mkdtemp(prefix='PostprocessCorpora-', dir=ROOT / 'generated'))
    for suite, expected_name in [('core', 'native-corpus.expected'), ('api', 'native-api.expected'),
                                 ('upstream', 'upstream-jsonb.expected'), ('fts5', 'native-fts5.expected')]:
        emitted = ROOT / 'generated' / ('Test-' + suite) / ('Test-' + suite + '.csproj')
        output = corpus_root / suite
        execute(f'corpus-{suite}-rewrite', ['dotnet', tool, emitted, '--output', output])
        corpus = json.loads((output / 'manifest.json').read_text())
        expected = (ROOT / 'tests' / expected_name).read_text()
        for variant, key in [('Original', 'OriginalProject'), ('Optimized', 'OptimizedProject')]:
            project = output / corpus[key]
            label = f'corpus-{suite}-{variant}'
            execute(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo'])
            assembly = project.parent / 'bin/Release/net10.0' / (corpus['AssemblyName'] + '.dll')
            if execute(label + '-jit', ['dotnet', assembly], runtime=True) != expected:
                raise SystemExit(label + ' JIT differs from native baseline')
            if args.aot:
                publish = ROOT / 'build/postprocess/corpora' / suite / variant
                execute(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', args.runtime,
                        '-p:PublishAot=true', '-o', publish, '--nologo'])
                executable = publish / (corpus['AssemblyName'] + ('.exe' if args.runtime.startswith('win-') else ''))
                if execute(label + '-aot', [executable], runtime=True) != expected:
                    raise SystemExit(label + ' NativeAOT differs from native baseline')
        print(f'PASS original/optimized {suite} native baseline JIT' + ('/NativeAOT' if args.aot else ''), flush=True)

sizes = {}
for variant, key in [('Original', 'OriginalProject'), ('Optimized', 'OptimizedProject')]:
    project = snapshot / manifest[key]
    sizes[variant] = {
        'source_bytes': sum(p.stat().st_size for p in (project.parent / 'src').glob('*.cs')),
        'assembly_bytes': (project.parent / 'bin/Release/net10.0' / (manifest['AssemblyName'] + '.dll')).stat().st_size,
    }
(artifacts / 'measurements.json').write_text(json.dumps({'rewritten': manifest['Rewritten'],
    'skipped': manifest['Skipped'], 'sizes': sizes, 'measurements': measurements, 'benchmark': benchmark}, indent=2) + '\n')
print('PASS original/optimized SQLite SQL/JSONB/FTS5, threading, mmap/WAL and native-process differentials', flush=True)
