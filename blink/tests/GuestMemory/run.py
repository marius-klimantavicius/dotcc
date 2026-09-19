#!/usr/bin/env python3
"""Valid upstream guest mapping lifecycle: native and actual managed core."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import signal
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'scripts'))
from core_inputs import compiler_identity

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--assembly-receipt', type=Path, required=True)
parser.add_argument('--native-only', action='store_true')
args = parser.parse_args()
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
base = ROOT / 'generated/guest-memory'
base.mkdir(parents=True, exist_ok=True)
a = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/guest-memory' / a.name
out.mkdir(parents=True)
(out/'tmp').mkdir()
r = {'kind': 'valid-guest-memory-lifecycle', 'passed': False, 'results': {},
     'runner_sha256': sha(Path(__file__)),
     'limitations': ['Valid guest page-table mappings/copies/protection metadata; no forbidden access or fault injection.',
                    'Copy*Read/Write records access metadata but does not itself prove permission enforcement.',
                    'Two complete upstream lifecycles share one host owner; no translated upstream call after final disposal.',
                    'Native original archive versus canonical managed adapters; host slab-pool bytes are separate from guest rss/vss.']}



def save():
    pending = out / 'receipt.tmp'
    pending.write_text(json.dumps(r, indent=2) + '\n')
    pending.replace(out / 'receipt.json')


def interrupted(signum, frame):
    raise InterruptedError('Received signal ' + str(signum))


signal.signal(signal.SIGTERM, interrupted)


def run(command, label, timeout=300):
    command = list(map(str, command))
    start, code = time.monotonic(), None
    stdout_path, stderr_path = out / (label + '.stdout'), out / (label + '.stderr')
    try:
        with stdout_path.open('wb') as stdout, stderr_path.open('wb') as stderr:
            process = subprocess.Popen(command, cwd=ROOT.parent, stdout=stdout, stderr=stderr,
                                       env=dict(os.environ, LC_ALL='C', TMPDIR=str(out/'tmp')), start_new_session=True)
            try:
                code = process.wait(timeout=timeout)
            except BaseException:
                try:
                    os.killpg(process.pid, signal.SIGTERM)
                except ProcessLookupError:
                    pass
                try:
                    process.wait(timeout=3)
                except subprocess.TimeoutExpired:
                    pass
                try:
                    os.killpg(process.pid, signal.SIGKILL)
                except ProcessLookupError:
                    pass
                process.wait()
                raise
    finally:
        r['results'][label] = {'command': command, 'exit_code': code, 'seconds': time.monotonic()-start,
                               'stdout_sha256': sha(stdout_path), 'stderr_sha256': sha(stderr_path)}
        save()
    if code:
        raise RuntimeError(label + ' failed: ' + str(out))
    return stdout_path.read_bytes()


def execute(command, label, binary, managed=False, expected=None):
    paths = list(binary.parent.glob('*.dll')) + list(binary.parent.glob('*.json')) if managed else [binary]
    paths = sorted(set([binary, *paths]))
    before = {str(p): sha(p) for p in paths}
    value = run(command, label, 30)
    after = {str(p): sha(p) for p in paths}
    r['results'][label].update(binaries_before=before, binaries_after=after)
    if before != after:
        raise RuntimeError(label + ' executed inputs changed')
    if (out / (label + '.stderr')).read_bytes():
        raise RuntimeError(label + ' produced unexpected diagnostics')
    if expected is not None and value != expected:
        raise RuntimeError(label + ' output differs from reference')
    save()
    return value


def project(path, name, kind, sources, refs, root=None):
    tree = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(tree, 'PropertyGroup')
    for key, value in dict(TargetFramework='net10.0', AssemblyName=name, OutputType=kind,
                          AllowUnsafeBlocks='true', Nullable='disable', EnableDefaultCompileItems='false',
                          DefineConstants='BLINK_FULL_CORE', WarningsAsErrors='CS8500').items():
        ET.SubElement(props, key).text = value
    items = ET.SubElement(tree, 'ItemGroup')
    for source in sources:
        ET.SubElement(items, 'Compile', Include=str(source))
    for ref in refs:
        ET.SubElement(items, 'ProjectReference', Include=str(ref))
    if root:
        ET.SubElement(items, 'TrimmerRootAssembly', Include=root)
    ET.indent(tree)
    ET.ElementTree(tree).write(path, encoding='unicode')


try:
    if platform.system() != 'Linux' or platform.machine() != 'x86_64':
        raise RuntimeError('Linux x64 reference execution is required')
    assembly_path = args.assembly_receipt.resolve()
    assembly = json.loads(assembly_path.read_text())
    profile = Path(assembly['profile'])
    inputs = json.loads((profile / 'inputs.json').read_text())
    cli = ROOT.parent / 'DotCC/bin/Release/net10.0/dotcc.dll'
    post = ROOT.parent / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
    identity = compiler_identity(cli.parent)
    if identity != inputs['compiler'] or identity != assembly['identity']['compiler_sha256']:
        raise RuntimeError('Compiler differs from qualified baseline')
    if not assembly['linked'] or assembly['failures'] or assembly.get('diagnostic_replay') or sha(profile/'inputs.json') != assembly['identity']['profile_inputs_sha256']:
        raise RuntimeError('Baseline assembly identity is not qualified')
    for name, digest in inputs['staged_headers'].items():
        if sha(profile/name) != digest:
            raise RuntimeError('Frozen input changed: ' + name)
    r.update(assembly_receipt=str(assembly_path), assembly_sha256=sha(assembly_path),
             profile=str(profile), profile_inputs_sha256=sha(profile/'inputs.json'), compiler=identity,
             postprocessor={p.name: sha(p) for p in post.parent.iterdir() if p.is_file() and p.suffix in {'.dll', '.json'}})
    templates = ['probe.c', 'Program.cs']
    r['authored_templates'] = {name: sha(ROOT/'tests/GuestMemory'/name) for name in templates}
    for name in templates:
        shutil.copyfile(ROOT/'tests/GuestMemory'/name, a/name)
    shutil.copyfile(Path(__file__), a/'run.py')
    r['authored_inputs'] = {p.name: sha(p) for p in a.iterdir() if p.is_file()}
    r['tools'] = {}
    for name in ['cc', 'dotnet']:
        tool = Path(shutil.which(name) or name).resolve()
        r['tools'][name] = {'path': str(tool), 'sha256': sha(tool)}
    native = ROOT/'build/native/source'
    archive = native/'o/blink/blink.a'
    closure = json.loads((profile/'closure.json').read_text())
    if sha(archive) != closure['native_archive_sha256'] or sha(native/'config.h') != closure['native_config_sha256']:
        raise RuntimeError('Native archive/config drift')
    source_manifest_path = ROOT/'config/source-manifest.json'
    source_manifest = json.loads(source_manifest_path.read_text())
    r['source_manifest_sha256'] = sha(source_manifest_path)
    r['native_reference'] = {'upstream_revision': source_manifest['upstream']['revision'],
                             'archive_sha256': sha(archive), 'config_sha256': sha(native/'config.h'),
                             'profile_source_overrides': inputs.get('source_overrides', {})}
    native_copy = a/'native-reference'
    (native_copy/'blink').mkdir(parents=True)
    native_inputs = [native/'config.h', *sorted((native/'blink').glob('*.h')), *sorted((native/'blink').glob('*.inc'))]
    r['native_headers'] = {str(path.relative_to(native)): sha(path) for path in native_inputs}
    for name in r['native_headers']:
        shutil.copyfile(native/name, native_copy/name)
    shutil.copyfile(archive, native_copy/'blink.a')
    run(['cc', '-D_GNU_SOURCE', '-D_DEFAULT_SOURCE', '-DNOLINEAR', '-Werror', '-I', native_copy,
         a/'probe.c', native_copy/'blink.a', '-lz', '-lrt', '-lm', '-pthread', '-o', a/'native'], 'native-adapter-build')
    expected = execute([a/'native'], 'native-adapter', a/'native')
    if not expected.startswith(b'cycle=0 initial pages=2 ') or len(expected.splitlines()) != 10 or b'cycle=1 unmapped pages=0 ' not in expected:
        raise RuntimeError('Unexpected native state transcript')
    r['expected_state_transcript'] = expected.decode()
    r['native_passed'] = True
    if not args.native_only:
        objects = {name: Path(row['object_path']) for name, row in assembly['objects'].items()}
        for name, path in objects.items():
            if sha(path) != assembly['objects'][name]['object_sha256']:
                raise RuntimeError('Baseline object changed: ' + name)
        excluded = {'authored/managed-driver.c'}
        r['retained_objects'] = {name: {'path': str(path), 'sha256': sha(path), 'producer': assembly['objects'][name]['receipt']}
                                 for name, path in objects.items() if name not in excluded}

        def emit(template, source, name, extra=()):
            row = assembly['objects'][template]
            command = row['command'].copy()
            canonical = Path(command[command.index('-I')+1])
            for dependency, digest in row['emission_identity']['dependencies'].items():
                if sha(canonical/dependency) != digest:
                    raise RuntimeError('Canonical emission dependency changed: ' + dependency)
            r['canonical_headers'] = str(canonical)
            r['canonical_dependencies'] = row['emission_identity']['dependencies']
            command[command.index(row['canonical_source'])] = str(source)
            command[command.index('-o')+1] = str(a/(name+'.cs'))
            if '--override-report' in command:
                command[command.index('--override-report')+1] = str(out/(name+'.overrides.jsonl'))
            command[3:3] = list(extra)
            run(command, name+'-emit')
            result = a/(name+'.cs')
            r.setdefault('new_objects', {})[name] = {'source': str(source), 'source_sha256': sha(source),
                                                     'object': str(result), 'object_sha256': sha(result),
                                                     'template_command_from': template}
            if compiler_identity(cli.parent) != identity:
                raise RuntimeError('Compiler changed during emission')
            return result

        r['replaced_object'] = assembly['objects']['authored/managed-driver.c']
        del objects['authored/managed-driver.c']
        objects['authored/GuestMemory.c'] = emit('authored/managed-driver.c', a/'probe.c', 'GuestMemory', ['-DGUEST_MEMORY_MANAGED'])
        r['object_counts'] = dict(canonical=len(assembly['objects']), retained=len(r['retained_objects']), replaced=1, derived=len(objects))
        generated = a/'raw-generated'
        run(['dotnet', cli, *objects.values(), '--emit=managedlib', '--nest-types', '--class-name', 'BlinkCore',
             '--namespace', 'Managed.Emulation', '--runtime=c', '-o', generated], 'link')
        r['raw_generated'] = {p.name: sha(p) for p in generated.glob('*.cs')}
        shutil.copytree(generated, a/'optimized-generated')
        shutil.copytree(profile/'host-project', a/'host', ignore=shutil.ignore_patterns('bin', 'obj'))
        (a/'bridges').mkdir()
        bindings = json.loads((profile/'binding-sources.json').read_text())
        for name in bindings['authored_managed']:
            shutil.copyfile(profile/name, a/'bridges'/Path(name).name)
        r['host_snapshot'] = {str(p.relative_to(a)): sha(p) for folder in [a/'host', a/'bridges'] for p in folder.rglob('*') if p.is_file()}
        for mode in ['raw', 'optimized']:
            folder = a/mode
            folder.mkdir()
            lib = folder/'GuestMemoryLibrary.csproj'
            project(lib, 'GuestMemoryLibrary', 'Library', [a/(mode+'-generated')/'*.cs', a/'bridges/*.cs'], [a/'host/Managed.Emulation.Host.csproj'])
            consumer_dir = folder/'consumer'
            consumer_dir.mkdir()
            consumer = consumer_dir/'GuestMemory.csproj'
            project(consumer, 'GuestMemory', 'Exe', [a/'Program.cs'], [lib], 'GuestMemoryLibrary')
            if mode == 'optimized':
                run(['dotnet', 'restore', lib], mode+'-restore')
                run(['dotnet', post, lib, '--in-place'], mode+'-postprocess', 600)
            run(['dotnet', 'build', consumer, '-c', 'Release'], mode+'-build', 600)
            binary = consumer_dir/'bin/Release/net10.0/GuestMemory.dll'
            execute(['dotnet', binary], mode+'-jit', binary, managed=True, expected=expected)
            target = folder/'publish'
            run(['dotnet', 'publish', consumer, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', target], mode+'-aot-build', 900)
            execute([target/'GuestMemory'], mode+'-aot', target/'GuestMemory', expected=expected)
        if r['raw_generated'] != {p.name: sha(p) for p in generated.glob('*.cs')}:
            raise RuntimeError('Raw generated sources changed')
        for name, row in r['retained_objects'].items():
            if sha(Path(row['path'])) != row['sha256']:
                raise RuntimeError('Retained producer changed: ' + name)
        for name, digest in r['host_snapshot'].items():
            if sha(a/name) != digest:
                raise RuntimeError('Host snapshot changed: ' + name)
        labels = ['raw-jit', 'raw-aot', 'optimized-jit', 'optimized-aot']
        if any(r['results'].get(label, {}).get('exit_code') != 0 for label in labels):
            raise RuntimeError('Incomplete four-mode guest-memory matrix')
        r['runtime_matrix'] = labels
        r['runtime_matrix_passed'] = True
        r['optimized_generated'] = {p.name: sha(p) for p in (a/'optimized-generated').glob('*.cs')}
    for name, digest in r['authored_templates'].items():
        if sha(ROOT/'tests/GuestMemory'/name) != digest:
            raise RuntimeError('Authored template changed during qualification: ' + name)
    for name, digest in r['authored_inputs'].items():
        if sha(a/name) != digest:
            raise RuntimeError('Authored snapshot changed: ' + name)
    for name, digest in inputs['staged_headers'].items():
        if sha(profile/name) != digest:
            raise RuntimeError('Frozen profile changed: ' + name)
    for name, digest in r.get('canonical_dependencies', {}).items():
        if sha(Path(r['canonical_headers'])/name) != digest:
            raise RuntimeError('Canonical dependency changed: ' + name)
    for name, digest in r['native_headers'].items():
        if sha(native/name) != digest or sha(native_copy/name) != digest:
            raise RuntimeError('Native header changed: ' + name)
    if sha(native_copy/'blink.a') != r['native_reference']['archive_sha256']:
        raise RuntimeError('Copied native archive changed')
    if compiler_identity(cli.parent) != identity or sha(assembly_path) != r['assembly_sha256']:
        raise RuntimeError('Compiler/assembly changed during qualification')
    if sha(archive) != closure['native_archive_sha256'] or sha(native/'config.h') != closure['native_config_sha256'] or sha(source_manifest_path) != r['source_manifest_sha256']:
        raise RuntimeError('Native source/profile changed during qualification')
    if {p.name: sha(p) for p in post.parent.iterdir() if p.is_file() and p.suffix in {'.dll', '.json'}} != r['postprocessor']:
        raise RuntimeError('Postprocessor changed during qualification')
    if sha(Path(__file__)) != r['runner_sha256']:
        raise RuntimeError('GuestMemory runner changed during qualification')
    for name, item in r['tools'].items():
        if str(Path(shutil.which(name) or name).resolve()) != item['path'] or sha(Path(item['path'])) != item['sha256']:
            raise RuntimeError('Execution tool changed: ' + name)
    r['passed'] = not args.native_only
    print(str(out/'receipt.json'))
except BaseException as error:
    r['passed'] = False
    r['failure'] = {'type': type(error).__name__, 'message': str(error)}
    raise
finally:
    save()
