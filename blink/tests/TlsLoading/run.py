#!/usr/bin/env python3
"""Fixed valid PT_TLS image: Linux/native Blink assertions and actual managed core."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import signal
import struct
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
base = ROOT / 'generated/tls-loading'
base.mkdir(parents=True, exist_ok=True)
a = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/tls-loading' / a.name
out.mkdir(parents=True)
r = {'kind': 'fixed-valid-static-TLS-runtime-startup', 'passed': False, 'results': {},
     'runner_sha256': sha(Path(__file__)),
     'limitations': ['One authored valid static image and explicit TLS runtime startup; no general libc TLS ABI claim.',
                    'Linux hardware/native CLI exit zero proves guest assertions, not an observed hardware register dump.',
                    'Unchanged configured native archive versus explicitly derived staged managed core; no separate staged-native archive run.',
                    'No HTTP/service worker, malformed input, custom fault injection or reusable post-disposal core state.']}


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
                                       env=dict(os.environ, LC_ALL='C'), start_new_session=True)
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
    if sha(a / 'fixture') != r['fixture_sha256']:
        raise RuntimeError(label + ' fixture changed before execution')
    paths = list(binary.parent.glob('*.dll')) + list(binary.parent.glob('*.json')) if managed else [binary]
    paths = sorted(set([binary, *paths]))
    before = {str(p): sha(p) for p in paths}
    value = run(command, label, 30)
    after = {str(p): sha(p) for p in paths}
    r['results'][label].update(binaries_before=before, binaries_after=after)
    if before != after or sha(a / 'fixture') != r['fixture_sha256']:
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
    templates = ['fixture.S', 'fixture.ld', 'probe.c', 'Program.cs']
    r['authored_templates'] = {name: sha(ROOT/'tests/TlsLoading'/name) for name in templates}
    for name in ['fixture.S', 'fixture.ld']:
        shutil.copyfile(ROOT/'tests/TlsLoading'/name, a/name)
    r['tools'] = {}
    for name in ['cc', 'ld', 'nm', 'readelf', 'dotnet']:
        tool = Path(shutil.which(name) or name).resolve()
        r['tools'][name] = {'path': str(tool), 'sha256': sha(tool)}
    run(['cc', '-c', '-nostdlib', a/'fixture.S', '-o', a/'fixture.o'], 'fixture-assemble')
    run(['ld', '-static', '--build-id=none', '-T', a/'fixture.ld', a/'fixture.o', '-o', a/'fixture'], 'fixture-link')
    run(['readelf', '-h', '-l', '-W', a/'fixture'], 'fixture-elf')
    image = (a/'fixture').read_bytes()
    if image[:6] != b'\x7fELF\x02\x01' or struct.unpack_from('<HH', image, 16) != (2, 62):
        raise RuntimeError('Fixture must be static x86-64 ET_EXEC')
    entry, phoff = struct.unpack_from('<QQ', image, 24)
    phent, phnum = struct.unpack_from('<HH', image, 54)
    headers = [struct.unpack_from('<IIQQQQQQ', image, phoff+i*phent) for i in range(phnum)]
    tls = [h for h in headers if h[0] == 7]
    if len(tls) != 1 or any(h[0] == 3 for h in headers):
        raise RuntimeError('Expected exactly one TLS segment and no interpreter')
    tls = tls[0]
    if tls[5:] != (8, 16, 8) or image[tls[2]:tls[2]+8] != struct.pack('<Q', 0x1122334455667788):
        raise RuntimeError('Unexpected TLS template bytes/size/alignment')
    if any(h[1]&3 == 3 for h in headers if h[0] == 1) or not any(h[0] == 0x6474e551 and h[1] == 6 for h in headers):
        raise RuntimeError('Fixture requires separate executable/read-write mappings and nonexecutable stack')
    symbols = {}
    for line in run(['nm', '-n', a/'fixture'], 'fixture-symbols').decode().splitlines():
        fields = line.split()
        if len(fields) == 3:
            symbols[fields[2]] = int(fields[0], 16)
    runtime, exit_ip = symbols['runtime_tls'], symbols['tls_exit']
    if symbols['__tls_template'] != tls[3] or symbols['_start'] != entry or runtime % 16:
        raise RuntimeError('Fixture symbol/segment alignment differs')
    r.update(fixture_sha256=sha(a/'fixture'), fixture_headers=headers,
             tls={'template_address': tls[3], 'file_size': 8, 'memory_size': 16, 'alignment': 8,
                  'runtime_address': runtime, 'entry': entry, 'exit_ip': exit_ip})
    constants = '\n'.join('#define ' + key + ' ' + str(value) + 'UL' for key, value in
                          [('FIXTURE_ENTRY', entry), ('FIXTURE_TEMPLATE', tls[3]),
                           ('FIXTURE_RUNTIME', runtime), ('FIXTURE_EXIT', exit_ip),
                           ('FIXTURE_PHNUM', phnum), ('FIXTURE_TLS_INDEX', headers.index(tls)),
                           ('FIXTURE_TLS_OFFSET', tls[2]), ('FIXTURE_TLS_PADDR', tls[4])])
    probe = (ROOT/'tests/TlsLoading/probe.c').read_text()
    if probe.count('/* FIXTURE_CONSTANTS */') != 1:
        raise RuntimeError('Fixture constant insertion marker differs')
    (a/'probe.c').write_text(probe.replace('/* FIXTURE_CONSTANTS */', constants))
    program = (ROOT/'tests/TlsLoading/Program.cs').read_text()
    if program.count('FIXTURE_SHA256') != 1:
        raise RuntimeError('Fixture hash insertion marker differs')
    (a/'Program.cs').write_text(program.replace('FIXTURE_SHA256', r['fixture_sha256']))
    shutil.copyfile(ROOT/'src/HostMemory/HostMemory.c', a/'HostMemory.c')
    r['authored_inputs'] = {p.name: sha(p) for p in a.iterdir() if p.is_file()}
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
    execute([a/'fixture'], 'linux-guest-assertions', a/'fixture', expected=b'')
    native_cli = native/'o/blink/blink'
    native_receipt_path = ROOT/'artifacts/native/receipt.json'
    native_receipt = json.loads(native_receipt_path.read_text())
    if native_receipt['binarySha256'] != sha(native_cli) or native_receipt['upstreamRevision'] != source_manifest['upstream']['revision']:
        raise RuntimeError('Native CLI differs from pinned oracle receipt')
    r['native_cli_receipt'] = {'path': str(native_receipt_path), 'sha256': sha(native_receipt_path)}
    execute([native_cli, '-jm', a/'fixture'], 'native-blink-guest-assertions', native_cli, expected=b'')
    run(['cc', '-D_GNU_SOURCE', '-D_DEFAULT_SOURCE', '-DNOLINEAR', '-Werror', '-I', native,
         a/'probe.c', archive, '-lz', '-lrt', '-lm', '-pthread', '-o', a/'native'], 'native-adapter-build')
    expected = execute([a/'native', a/'fixture'], 'native-adapter', a/'native')
    if not expected.startswith(b'tls filesz=8 memsz=16 align=8 ') or len(expected.splitlines()) != 1:
        raise RuntimeError('Unexpected native state transcript')
    r['expected_state_transcript'] = expected.decode()
    r['native_passed'] = True
    if not args.native_only:
        objects = {name: Path(row['object_path']) for name, row in assembly['objects'].items()}
        for name, path in objects.items():
            if sha(path) != assembly['objects'][name]['object_sha256']:
                raise RuntimeError('Baseline object changed: ' + name)
        excluded = {'authored/managed-driver.c', 'authored/HostMemory.c'}
        r['retained_objects'] = {name: {'path': str(path), 'sha256': sha(path), 'producer': assembly['objects'][name]['receipt']}
                                 for name, path in objects.items() if name not in excluded}

        def emit(template, source, name, extra=()):
            row = assembly['objects'][template]
            command = row['command'].copy()
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

        del objects['authored/managed-driver.c']
        objects['authored/HostMemory.c'] = emit('authored/HostMemory.c', a/'HostMemory.c', 'HostMemory')
        objects['authored/TlsLoading.c'] = emit('blink/loader.c', a/'probe.c', 'TlsLoading', ['-DBLINK_TLS_MANAGED'])
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
            lib = folder/'TlsLibrary.csproj'
            project(lib, 'TlsLibrary', 'Library', [a/(mode+'-generated')/'*.cs', a/'bridges/*.cs'], [a/'host/Managed.Emulation.Host.csproj'])
            consumer_dir = folder/'consumer'
            consumer_dir.mkdir()
            consumer = consumer_dir/'TlsLoading.csproj'
            project(consumer, 'TlsLoading', 'Exe', [a/'Program.cs'], [lib], 'TlsLibrary')
            if mode == 'optimized':
                run(['dotnet', 'restore', lib], mode+'-restore')
                run(['dotnet', post, lib, '--in-place'], mode+'-postprocess', 600)
            run(['dotnet', 'build', consumer, '-c', 'Release'], mode+'-build', 600)
            binary = consumer_dir/'bin/Release/net10.0/TlsLoading.dll'
            execute(['dotnet', binary, a/'fixture'], mode+'-jit', binary, managed=True, expected=expected)
            target = folder/'publish'
            run(['dotnet', 'publish', consumer, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', target], mode+'-aot-build', 900)
            execute([target/'TlsLoading', a/'fixture'], mode+'-aot', target/'TlsLoading', expected=expected)
        if r['raw_generated'] != {p.name: sha(p) for p in generated.glob('*.cs')}:
            raise RuntimeError('Raw generated sources changed')
        for name, row in r['retained_objects'].items():
            if sha(Path(row['path'])) != row['sha256']:
                raise RuntimeError('Retained producer changed: ' + name)
        for name, digest in r['host_snapshot'].items():
            if sha(a/name) != digest:
                raise RuntimeError('Host snapshot changed: ' + name)
        r['runtime_matrix_passed'] = True
        r['optimized_generated'] = {p.name: sha(p) for p in (a/'optimized-generated').glob('*.cs')}
    for name, digest in r['authored_templates'].items():
        if sha(ROOT/'tests/TlsLoading'/name) != digest:
            raise RuntimeError('Authored template changed during qualification: ' + name)
    for name, digest in r['authored_inputs'].items():
        if sha(a/name) != digest:
            raise RuntimeError('Authored snapshot changed: ' + name)
    for name, digest in inputs['staged_headers'].items():
        if sha(profile/name) != digest:
            raise RuntimeError('Frozen profile changed: ' + name)
    if compiler_identity(cli.parent) != identity or sha(assembly_path) != r['assembly_sha256']:
        raise RuntimeError('Compiler/assembly changed during qualification')
    if sha(archive) != closure['native_archive_sha256'] or sha(native/'config.h') != closure['native_config_sha256'] or sha(source_manifest_path) != r['source_manifest_sha256']:
        raise RuntimeError('Native source/profile changed during qualification')
    if {p.name: sha(p) for p in post.parent.iterdir() if p.is_file() and p.suffix in {'.dll', '.json'}} != r['postprocessor']:
        raise RuntimeError('Postprocessor changed during qualification')
    if sha(Path(__file__)) != r['runner_sha256']:
        raise RuntimeError('TLS runner changed during qualification')
    r['passed'] = not args.native_only
    print(str(out/'receipt.json'))
except BaseException as error:
    r['passed'] = False
    r['failure'] = {'type': type(error).__name__, 'message': str(error)}
    raise
finally:
    save()
