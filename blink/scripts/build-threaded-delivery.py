#!/usr/bin/env python3
"""Build a separately staged threaded prototype; never replace TranslatedBlink."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import tempfile
import time
import xml.etree.ElementTree as ET

from core_inputs import compiler_identity, profile_sources

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()


def manifest(directory):
    return {str(p.relative_to(directory)): sha(p) for p in sorted(directory.rglob('*'))
            if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}


def project(path, bridges=None):
    root = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(root, 'PropertyGroup')
    for name, value in dict(TargetFramework='net10.0', LangVersion='14', OutputType='Library',
                           AssemblyName='TranslatedBlink', AllowUnsafeBlocks='true', Nullable='disable',
                           ImplicitUsings='enable', EnableDefaultCompileItems='false',
                           DefineConstants='$(DefineConstants);BLINK_FULL_CORE',
                           WarningsAsErrors='$(WarningsAsErrors);CS8500').items():
        ET.SubElement(props, name).text = value
    items = ET.SubElement(root, 'ItemGroup')
    ET.SubElement(items, 'Compile', Include='Sources/**/*.cs')
    if bridges is None:
        ET.SubElement(items, 'Compile', Include='Bridges/*.cs')
        ET.SubElement(items, 'ProjectReference', Include='Host/Managed.Emulation.Host.csproj')
    else:
        for name in bridges:
            node = ET.SubElement(items, 'Compile', Include='../../' + name)
            ET.SubElement(node, 'Link').text = 'Bindings/' + Path(name).name
        ET.SubElement(items, 'ProjectReference', Include='../../src/Managed.Emulation.Host/Managed.Emulation.Host.csproj')
    ET.indent(root)
    ET.ElementTree(root).write(path, encoding='unicode')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--assembly-receipt', type=Path, required=True)
    args = parser.parse_args()
    base = ROOT / 'artifacts/threaded-delivery'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    tmp = attempt / 'tmp'; tmp.mkdir()
    raw = attempt / 'raw'; candidate = attempt / 'postprocessed'
    output = ROOT / 'generated/ThreadedBlink'
    backup = attempt / 'previous-output'
    receipt = dict(passed=False, runtime_qualified=False, scope='Separate threaded prototype project builds only',
                   raw_snapshot=str(raw), output=str(output), commands={}, inputs={}, raw_files={})
    publication_committed = False
    publication_started = False
    cli = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
    post = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'

    def save():
        temporary = attempt / 'receipt.tmp'
        temporary.write_text(json.dumps(receipt, indent=2) + '\n')
        temporary.replace(attempt / 'receipt.json')

    def freeze(path, expected=None):
        path = Path(path).resolve(); value = sha(path)
        if expected is not None and value != expected:
            raise RuntimeError('Identity differs: ' + str(path))
        if str(path) in receipt['inputs'] and receipt['inputs'][str(path)] != value:
            raise RuntimeError('Input changed: ' + str(path))
        receipt['inputs'][str(path)] = value
        return path

    def run(command, label, timeout=300):
        log = attempt / (label + '.log'); started = time.monotonic()
        row = dict(command=list(map(str, command)), exit_code=None)
        receipt['commands'][label] = row; save()
        with log.open('wb') as stream:
            process = subprocess.Popen(row['command'], cwd=REPO, stdout=stream, stderr=subprocess.STDOUT,
                env=dict(os.environ, LC_ALL='C', TMPDIR=str(tmp)), start_new_session=True)
            try:
                row['exit_code'] = process.wait(timeout=timeout)
            except BaseException:
                for sig in (signal.SIGTERM, signal.SIGKILL):
                    try: os.killpg(process.pid, sig)
                    except ProcessLookupError: pass
                    try: process.wait(timeout=5)
                    except subprocess.TimeoutExpired: continue
                row['exit_code'] = process.returncode
                raise
            finally:
                row.update(log=str(log), log_sha256=sha(log), seconds=time.monotonic() - started)
                save()
        if row['exit_code']:
            raise RuntimeError(label + ' failed: ' + str(log))

    def verify():
        for name, value in receipt['inputs'].items():
            if sha(name) != value: raise RuntimeError('Frozen input changed: ' + name)
        if compiler_identity(cli.parent) != receipt['compiler']:
            raise RuntimeError('Compiler identity changed')
        if manifest(raw) != receipt['raw_files']:
            raise RuntimeError('Immutable raw snapshot changed')
        if manifest(ROOT / 'src/Managed.Emulation.Host') != receipt['host_source_files']:
            raise RuntimeError('Authored Host project closure changed')

    signal.signal(signal.SIGTERM, lambda *_: (_ for _ in ()).throw(InterruptedError('SIGTERM')))
    print(attempt, flush=True)
    try:
        freeze(__file__)
        freeze(ROOT / 'scripts/core_inputs.py')
        dotnet = freeze(shutil.which('dotnet'))
        run([dotnet, '--info'], 'dotnet-info')
        receipt['dotnet_executable'] = str(dotnet)
        for name in ('Directory.Build.props', 'Directory.Packages.props', 'nuget.config'):
            freeze(REPO / name)
        receipt['compiler'] = compiler_identity(cli.parent)
        for path in post.parent.iterdir():
            if path.is_file() and path.suffix in ('.dll', '.json'): freeze(path)
        assembly_path = freeze(args.assembly_receipt)
        assembly = json.loads(assembly_path.read_text())
        profile = Path(assembly['profile'])
        inputs_path = freeze(profile / 'inputs.json', assembly['identity']['profile_inputs_sha256'])
        inputs = json.loads(inputs_path.read_text())
        if not assembly['linked'] or assembly['failures'] or assembly.get('diagnostic_replay'):
            raise RuntimeError('Complete linked assembly required')
        if inputs['compiler'] != receipt['compiler'] or assembly['identity']['compiler_sha256'] != receipt['compiler']:
            raise RuntimeError('Fresh current compiler identity required')
        for name, value in inputs['staged_headers'].items(): freeze(profile / name, value)
        config = (profile / 'config.h').read_text()
        if '#define BLINK_MANAGED_GUEST_THREADS 1' not in config or '#define DISABLE_THREADS 1' in config:
            raise RuntimeError('Explicit reviewed threaded profile required')
        entries = profile_sources(profile, ROOT, inputs)
        names = [row['path'] for row in entries]
        if len(names) != 108 or assembly['selected'] != names or set(assembly['objects']) != set(names):
            raise RuntimeError('Exact 108-producer product closure required')
        for row in entries:
            freeze(ROOT / row['staged_path'], row['sha256'])
            produced = assembly['objects'][row['path']]
            freeze(produced['object_path'], produced['object_sha256'])
        linked = Path(assembly['link']['output'])
        for name, value in assembly['generated'].items(): freeze(linked / name, value)
        bindings = json.loads((profile / 'host-bindings.json').read_text())
        bridges = bindings['managedSources']
        receipt.update(profile=str(profile), profile_inputs_sha256=sha(inputs_path),
                       assembly=dict(path=str(assembly_path), sha256=sha(assembly_path)),
                       authored_sources={}, product_source_links=bridges)
        for name, value in json.loads((profile / 'host-binding-receipt.json').read_text())['source_inputs'].items():
            freeze(ROOT / name, value)
            if name.endswith(('.cs', '.csproj')): receipt['authored_sources'][name] = value
        raw.mkdir(); (raw / 'Sources').mkdir(); (raw / 'Bridges').mkdir()
        expected_sources = {name for name in assembly['generated'] if name.endswith('.cs') and '/' not in name}
        if {p.name for p in linked.glob('*.cs')} != expected_sources:
            raise RuntimeError('Linked generated source closure differs')
        for name in sorted(expected_sources):
            path = linked / name
            if 'CoreProbe(' in path.read_text(): raise RuntimeError('Test frontend present')
            shutil.copyfile(path, raw / 'Sources' / path.name)
        if not any((raw / 'Sources').iterdir()): raise RuntimeError('No generated sources')
        for name in bridges:
            path = profile / 'managed' / Path(name).name
            freeze(path, sha(ROOT / name))
            shutil.copyfile(path, raw / 'Bridges' / path.name)
        shutil.copytree(profile / 'host-project', raw / 'Host')
        receipt['host_source_files'] = manifest(ROOT / 'src/Managed.Emulation.Host')
        if receipt['host_source_files'] != manifest(raw / 'Host'):
            raise RuntimeError('Live Host source closure differs from profile')
        project(raw / 'TranslatedBlink.csproj')
        receipt['raw_files'] = manifest(raw)
        shutil.copytree(raw, candidate)
        for path in [raw, *raw.rglob('*')]: path.chmod(0o555 if path.is_dir() else 0o444)
        run(['dotnet', 'build', candidate / 'TranslatedBlink.csproj', '-c', 'Release'], 'raw-build')
        run(['dotnet', post, candidate / 'TranslatedBlink.csproj', '--in-place'], 'postprocess', 900)
        shutil.rmtree(candidate / 'Bridges'); shutil.copytree(raw / 'Bridges', candidate / 'Bridges')
        shutil.rmtree(candidate / 'Host'); shutil.copytree(raw / 'Host', candidate / 'Host')
        for directory in ('Bridges', 'Host'):
            for path in [candidate / directory, *(candidate / directory).rglob('*')]:
                path.chmod(0o755 if path.is_dir() else 0o644)
        run(['dotnet', 'build', candidate / 'TranslatedBlink.csproj', '-c', 'Release'], 'restored-authored-build')
        verify()
        for path in sorted(candidate.rglob('*'), key=lambda p: len(p.parts), reverse=True):
            if path.is_dir() and path.name in ('bin', 'obj'): shutil.rmtree(path)
        shutil.rmtree(candidate / 'Bridges'); shutil.rmtree(candidate / 'Host')
        project(candidate / 'TranslatedBlink.csproj', bridges)
        receipt['final_files'] = manifest(candidate)
        if output.exists() and (output.is_symlink() or not output.is_dir()):
            raise RuntimeError('Prototype output must be an ordinary directory')
        publication_started = True
        if output.exists(): output.rename(backup)
        candidate.rename(output)
        run(['dotnet', 'build', output / 'TranslatedBlink.csproj', '-c', 'Release'], 'direct-source-build')
        verify()
        if manifest(output) != receipt['final_files']:
            raise RuntimeError('Published project sources changed during build')
        receipt.update(passed=True)
        save()
        publication_committed = True
    except BaseException as error:
        if publication_committed: raise
        receipt['passed'] = False
        receipt['failure'] = repr(error)
        if publication_started and not candidate.exists() and output.exists():
            output.rename(attempt / 'failed-publication')
        if backup.exists(): backup.rename(output)
        save(); raise
    print(attempt / 'receipt.json')


if __name__ == '__main__':
    main()
