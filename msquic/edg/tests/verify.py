#!/usr/bin/env python3
"""Compare actual EDG MsQuic layouts and run the existing owning transport tests.

All generated sources are linked unchanged. Only authored host, facade and test
sources receive explicit, recorded ABI spelling adapters. This is a focused EDG
acceptance suite, not a claim that the full existing dotcc campaign was rerun.
"""
from __future__ import annotations

import argparse
import difflib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import time

from abi import LAYOUTS, sources as abi_sources
from authored import adapt as adapt_authored

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
SPEC = importlib.util.spec_from_file_location('msquic_edg_provider', HERE.parent / 'provider.py')
PROVIDER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PROVIDER)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--core', required=True, type=Path)
    parser.add_argument('--picotls-core', required=True, type=Path)
    parser.add_argument('--stage', required=True, type=Path,
                        help='Exact staged source directory from the translation receipt')
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--prepared', type=Path, help='Existing provider.py output to link unchanged')
    parser.add_argument('--aot', action='store_true')
    parser.add_argument('--runtime', default='linux-x64')
    parser.add_argument('--modes', nargs='+', default=['configuration', 'transport'],
                        choices=['configuration', 'transport', 'packets', 'resumption'])
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'commands': [], 'checks': [], 'runtime': args.runtime,
              'scope': 'Actual generated ABI; platform allocation; selected existing owning-facade controls',
              'limits': ['No claim of full existing dotcc test campaign coverage.',
                         'Native ABI oracle is the explicit staged host profile, not an OS-specific MsQuic PAL.',
                         'Owning-facade transport uses BCL UDP with translated MsQuic and picotls on both endpoints.'],
              'inputs': [], 'adaptedSources': []}

    def save():
        (output / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def record(path):
        report['inputs'].append({'path': str(path.resolve()), 'sha256': PROVIDER.digest(path)})

    def run(label, command, timeout=900):
        command = list(map(str, command))
        entry = {'label': label, 'command': command}
        report['commands'].append(entry)
        save()
        start = time.monotonic()
        with (output / (label + '.stdout')).open('wb') as stdout, (output / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, stdout=stdout, stderr=stderr, timeout=timeout,
                                    env=dict(os.environ, DOTNET_PROCESSOR_COUNT='4'))
        entry.update(exitCode=result.returncode, seconds=round(time.monotonic() - start, 3))
        save()
        print(label + ': exit ' + str(result.returncode), flush=True)
        if result.returncode:
            raise RuntimeError(label + ' failed; see retained stdout/stderr')
        return (output / (label + '.stdout')).read_text()

    try:
        record(Path(__file__).resolve())
        record(HERE / 'abi.py')
        record(HERE / 'authored.py')
        record(HERE.parent / 'provider.py')
        stage = args.stage.resolve()
        record(stage / 'manifest.json')
        staged = json.loads((stage / 'manifest.json').read_text())
        for item in staged['files']:
            path = stage / item['path']
            if PROVIDER.digest(path) != item['sha256']:
                raise RuntimeError('Staged source differs from translation manifest: ' + str(path))
            record(path)
        if args.prepared:
            manifest_path = args.prepared.resolve() / 'provider-manifest.json'
            manifest = json.loads(manifest_path.read_text())
            if Path(manifest['generatedProject']).parent != args.core.resolve():
                raise RuntimeError('Prepared provider references a different core')
        else:
            prepared = output / 'provider'
            manifest = PROVIDER.prepare(args.core, args.picotls_core, prepared)
            manifest_path = prepared / 'provider-manifest.json'
        record(manifest_path)
        report['providerManifest'] = str(manifest_path)
        for info in (manifest, manifest['picotls']):
            record(Path(info['generatedProject']))
            for project_path in info['projects'].values():
                record(Path(project_path))
            for item in info['generatedSources']:
                path = Path(item['path'])
                if PROVIDER.digest(path) != item['sha256']:
                    raise RuntimeError('Generated source differs from provider manifest: ' + str(path))
                record(path)
            for item in info['authoredSources']:
                path = Path(item['path'])
                if PROVIDER.digest(path) != item['sha256']:
                    raise RuntimeError('Authored provider input changed: ' + str(path))
                record(path)
                if item.get('adaptedPath'):
                    path = Path(item['adaptedPath'])
                    if PROVIDER.digest(path) != item['adaptedSha256']:
                        raise RuntimeError('Adapted provider input changed: ' + str(path))
                    record(path)
        emitted = '\n'.join(p.read_text() for p in PROVIDER.generated_sources(args.core)
                            if p.name.startswith('Translated'))
        pico = '\n'.join(p.read_text() for p in PROVIDER.generated_sources(args.picotls_core)
                         if p.name.startswith('Translated'))

        def adapt(source, destination):
            original = source.read_text()
            text, changes = adapt_authored(PROVIDER, original, source.name, emitted, pico)
            destination.write_text(text)
            record(source)
            report['adaptedSources'].append({'path': str(source), 'sha256': PROVIDER.digest(source),
                'adaptedPath': str(destination), 'adaptedSha256': PROVIDER.digest(destination),
                'replacements': changes})
            return destination

        abi_types = {'CXPLAT_RW_LOCK': 'CXPLAT_DISPATCH_RW_LOCK', 'CXPLAT_EVENTQ': 'QUIC_EVENTQ'}
        abi_fields = {name: PROVIDER.field_paths(emitted, abi_types.get(name, name)) for name in LAYOUTS}
        native_source, original_source = abi_sources()
        _, managed_source = abi_sources(abi_types, abi_fields)
        report['abiTypeSpellings'] = abi_types
        report['abiFieldPaths'] = abi_fields
        abi_dir = output / 'Abi'
        abi_dir.mkdir()
        (abi_dir / 'native.c').write_text(native_source)
        record(abi_dir / 'native.c')
        (abi_dir / 'Original.cs').write_text(original_source)
        adapted_abi = abi_dir / 'Program.cs'
        adapted_abi.write_text(managed_source)
        record(adapted_abi)
        native = abi_dir / 'native'
        flags = ['-D' + value for value in staged['defines']]
        includes = ['-I' + str(stage / name) for name in ['system', 'src/inc', 'src/core', 'src/platform', 'host']]
        run('abi-native-build', ['gcc', '-std=gnu17', '-fms-extensions', '-O2', '-Wno-multichar',
            '-ffunction-sections', '-fdata-sections', '-Wl,--gc-sections', *flags, *includes,
            abi_dir / 'native.c', '-o', native])
        expected_abi = run('abi-native-run', [native])
        if not expected_abi or any(not row.startswith(('layout ', 'offset ', 'bytes '))
                                   for row in expected_abi.splitlines()):
            raise RuntimeError('Invalid native ABI observations')
        abi_project = abi_dir / 'Abi.csproj'
        PROVIDER.project(abi_project, 'EdgMsQuicAbi', [adapted_abi],
                         [Path(manifest['projects']['Core'])], executable=True)
        memory_dir = output / 'Memory'
        memory_dir.mkdir()
        memory_source = adapt(HERE / 'MemoryControls.cs', memory_dir / 'Program.cs')
        memory_project = memory_dir / 'Memory.csproj'
        PROVIDER.project(memory_project, 'EdgMsQuicTests', [memory_source],
                         [Path(manifest['projects']['BclHost'])], executable=True)
        facade_dir = output / 'Facade'
        facade_dir.mkdir()
        facade_sources = []
        for category, directory in [('production', ROOT / 'src/ManagedApi'), ('tests', ROOT / 'tests/ManagedApi')]:
            target = facade_dir / category
            target.mkdir()
            for source in sorted(directory.glob('*.cs')):
                facade_sources.append(adapt(source, target / source.name))
        facade_sources.append(adapt(ROOT / 'tests/TlsAdapter/Credentials.cs', facade_dir / 'Credentials.cs'))
        facade_project = facade_dir / 'Facade.csproj'
        PROVIDER.project(facade_project, 'MsQuic.ManagedApi', facade_sources,
                         [Path(manifest['projects']['BclHost'])], executable=True)
        projects = [('Abi', 'EdgMsQuicAbi', abi_project), ('Memory', 'EdgMsQuicTests', memory_project),
                    ('Facade', 'MsQuic.ManagedApi', facade_project)]
        for group, assembly, project in projects:
            record(project)
            run(group + '-jit-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo',
                                      '-p:TreatWarningsAsErrors=true'])
            commands = [('jit', ['dotnet', project.parent / 'bin/Release/net10.0' / (assembly + '.dll')])]
            if args.aot:
                destination = output / ('aot-' + group)
                run(group + '-aot-build', ['dotnet', 'publish', project, '-c', 'Release', '-r', args.runtime,
                    '-p:PublishAot=true', '-p:TreatWarningsAsErrors=true', '-p:IlcTreatWarningsAsErrors=true',
                    '-o', destination], timeout=1800)
                commands.append(('aot', [destination / assembly]))
            for runtime, command in commands:
                modes = args.modes if group == 'Facade' else [None]
                for mode in modes:
                    arguments = ['--' + mode] if mode and mode != 'configuration' else []
                    label = group + '-' + runtime + ('-' + mode if mode else '')
                    actual = run(label, [*command, *arguments], timeout=600)
                    if group == 'Abi':
                        (output / (label + '.diff')).write_text(''.join(difflib.unified_diff(
                            expected_abi.splitlines(True), actual.splitlines(True), fromfile='native', tofile=runtime)))
                        if actual != expected_abi:
                            raise RuntimeError('Actual generated ABI differs from native: ' + label)
                        report['checks'].append({'name': label, 'records': len(actual.splitlines())})
                    else:
                        prefix = {'Memory': 'PASS EDG platform memory:',
                                  'Facade': 'PASS facade stream/lifetime' if mode == 'transport' else
                                      'PASS facade packet controls' if mode == 'packets' else
                                      'PASS managed API configuration control:'}[group]
                        if not any(line.startswith(prefix) for line in actual.splitlines()):
                            raise RuntimeError('Missing acceptance receipt: ' + label)
                        report['checks'].append({'name': label, 'output': actual.strip()})
                    save()
        for item in report['inputs']:
            if PROVIDER.digest(Path(item['path'])) != item['sha256']:
                raise RuntimeError('Input changed during verification: ' + item['path'])
        for item in report['adaptedSources']:
            if PROVIDER.digest(Path(item['adaptedPath'])) != item['adaptedSha256']:
                raise RuntimeError('Adapted source changed during verification: ' + item['adaptedPath'])
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
