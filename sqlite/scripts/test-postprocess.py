#!/usr/bin/env python3
"""Explicit original/optimized snapshot verification; never a normal build hook."""
import sys
import argparse
import json
import os
from pathlib import Path
import sys as _production_sys
from pathlib import Path as _ProductionPath
_production_sys.path.insert(0, str(next(parent for parent in _ProductionPath(__file__).resolve().parents
    if (parent / "Scripts/production_support.py").is_file()) / "Scripts"))
from production_support import subprocess, selected_tool
import time
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('snapshot', type=Path)
parser.add_argument('--aot', action='store_true')
parser.add_argument('--corpora', action='store_true', help='postprocess existing core/API/JSONB/FTS5 executables and compare native baselines')
parser.add_argument('--runtime', default='linux-x64')
P6_CHECKS = tuple(
    [f'Original-{name}-{mode}' for name in ('HostVfsTests', 'ThreadingTests') for mode in ('jit', 'aot')]
    + [f'Optimized-{name}-{mode}' for name in ('core', 'api', 'upstream', 'fts5') for mode in ('jit', 'aot')]
    + [f'Original-{name}-aot' for name in ('core', 'upstream', 'fts5')])
parser.add_argument('--check', action='append', choices=P6_CHECKS,
                    help='run only this existing uncovered P6 cell; repeat to share corpus preparation')
parser.add_argument('--artifacts', type=Path, help='owned logs for explicitly selected checks')
args = parser.parse_args()
snapshot = args.snapshot.resolve()
manifest = json.loads((snapshot / 'manifest.json').read_text())
artifacts = ROOT / 'artifacts' / 'postprocess'
if args.check:
    artifacts = args.artifacts or artifacts / ('p6-' + str(time.time_ns()))
artifacts.mkdir(parents=True, exist_ok=True)
build = ROOT / 'generated' / 'PostprocessConsumers'
if args.check:
    build = build / ('p6-' + artifacts.name)
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
    sources = ROOT / ('samples/ManagedConsumer' if name == 'ManagedConsumer' else 'tests/' + name)
    ET.SubElement(items, 'Compile', Include=str(sources / '*.cs'))
    path = directory / (name + '.csproj')
    ET.indent(project)
    path.write_text(ET.tostring(project, encoding='unicode') + '\n')
    return path


def run_selected_checks():
    selected = set(args.check)
    completed = []
    outputs = {}
    for name in ('HostVfsTests', 'ThreadingTests'):
        modes = [mode for mode in ('jit', 'aot') if f'Original-{name}-{mode}' in selected]
        if not modes:
            continue
        project = consumer_project('Original', name, snapshot / manifest['OriginalProject'])
        for mode in modes:
            label = f'Original-{name}-{mode}'
            if mode == 'jit':
                execute(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo'])
                command = ['dotnet', project.parent / 'bin/Release/net10.0' / (name + '.dll')]
            else:
                publish = build / 'publish' / name
                execute(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release',
                        '-r', args.runtime, '-p:PublishAot=true', '-o', publish, '--nologo'])
                command = [publish / (name + ('.exe' if args.runtime.startswith('win-') else ''))]
            outputs[name, mode] = execute(label, command, runtime=True)
            if name == 'HostVfsTests':
                execute(label + '-processes', [sys.executable, ROOT / 'scripts/test-host-vfs-processes.py',
                        '--managed', *command, '--native', ROOT / 'build/host-vfs-native'])
            completed.append(label)
            print('PASS ' + label, flush=True)
        if len(modes) == 2 and outputs[name, 'jit'] != outputs[name, 'aot']:
            raise SystemExit(name + ': JIT/AOT transcripts differ')

    tool = selected_tool('postprocessor')
    corpus_root = Path(tempfile.mkdtemp(prefix='P6Corpora-', dir=ROOT / 'generated'))
    for suite, expected_name in [('core', 'native-corpus.expected'), ('api', 'native-api.expected'),
                                 ('upstream', 'upstream-jsonb.expected'), ('fts5', 'native-fts5.expected')]:
        emitted = ROOT / 'generated' / ('Test-' + suite) / ('Test-' + suite + '.csproj')
        expected = (ROOT / 'tests' / expected_name).read_text()
        optimized = None
        if any(f'Optimized-{suite}-{mode}' in selected for mode in ('jit', 'aot')):
            output = corpus_root / suite
            execute(f'corpus-{suite}-rewrite', ['dotnet', tool, emitted, '--output', output])
            corpus = json.loads((output / 'manifest.json').read_text())
            optimized = output / corpus['OptimizedProject']
        for variant, project in [('Original', emitted), ('Optimized', optimized)]:
            assembly_name = (corpus['AssemblyName'] if variant == 'Optimized' and optimized else
                             ET.parse(emitted).findtext('.//AssemblyName') or emitted.stem)
            for mode in ('jit', 'aot'):
                label = f'{variant}-{suite}-{mode}'
                if label not in selected:
                    continue
                if mode == 'jit':
                    execute(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo'])
                    command = ['dotnet', project.parent / 'bin/Release/net10.0' / (assembly_name + '.dll')]
                else:
                    publish = build / 'publish/corpora' / suite / variant
                    execute(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', args.runtime,
                            '-p:PublishAot=true', '-o', publish, '--nologo'])
                    command = [publish / (assembly_name + ('.exe' if args.runtime.startswith('win-') else ''))]
                if execute(label, command, runtime=True) != expected:
                    raise SystemExit(label + ': differs from existing native transcript')
                completed.append(label)
                print('PASS ' + label, flush=True)
    if set(completed) != selected or len(completed) != len(selected):
        raise SystemExit('Selected P6 checks were not each executed exactly once')
    (artifacts / 'selected-checks.json').write_text(json.dumps(dict(status='passed', checks=completed,
        measurements=measurements), indent=2) + '\n')


if args.check:
    run_selected_checks()
    raise SystemExit(0)


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
            execute(f'{variant}-host-processes-jit', [sys.executable, ROOT / 'scripts/test-host-vfs-processes.py',
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
                execute(f'{variant}-host-processes-aot', [sys.executable, ROOT / 'scripts/test-host-vfs-processes.py',
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
    tool = selected_tool('postprocessor')
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
        'source_bytes': sum(p.stat().st_size for p in (project.parent / 'src').rglob('*.cs')),
        'assembly_bytes': (project.parent / 'bin/Release/net10.0' / (manifest['AssemblyName'] + '.dll')).stat().st_size,
    }
(artifacts / 'measurements.json').write_text(json.dumps({'rewritten': manifest['Rewritten'],
    'skipped': manifest['Skipped'], 'sizes': sizes, 'measurements': measurements, 'benchmark': benchmark}, indent=2) + '\n')
print('PASS original/optimized SQLite SQL/JSONB/FTS5, threading, mmap/WAL and native-process differentials', flush=True)
