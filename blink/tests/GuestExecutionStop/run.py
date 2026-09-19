#!/usr/bin/env python3
"""Ordinary execution stop through the same authored C# product consumer."""
import argparse
import glob
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

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sys.path.insert(0, str(ROOT/'scripts'))
from core_inputs import compiler_identity, profile_sources

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--delivery-receipt', type=Path, required=True)
parser.add_argument('--native-only', action='store_true')
args = parser.parse_args()
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()
base = ROOT/'generated/guest-execution-stop'
base.mkdir(parents=True, exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT/'artifacts/guest-execution-stop'/attempt.name
out.mkdir(parents=True)
(out/'tmp').mkdir()
receipt = {'kind': 'same-csharp-owner-normal-execution-stop', 'passed': False,
           'results': {}, 'frozen_inputs': {}, 'observations': {},
           'limitations': [
               'Native Linux and pinned native Blink execute only four finite completion controls.',
               'Private cooperative cancellation is not Linux signal equivalence.',
               'One synchronous C# owner Run in each fresh managed process; no C execution loop.',
               'Elapsed time is observed, not required equal across native and managed runs.',
               'No injected faults, malformed images, invalid buffers or P5 controller protocol.']}


def save():
    pending = out/'receipt.tmp'
    pending.write_text(json.dumps(receipt, indent=2) + '\n')
    pending.replace(out/'receipt.json')


def pin(path, expected=None):
    path = Path(path).resolve()
    digest = sha(path)
    if expected is not None and digest != expected:
        raise RuntimeError('Input identity differs: ' + str(path))
    old = receipt['frozen_inputs'].setdefault(str(path), digest)
    if old != digest:
        raise RuntimeError('Input changed: ' + str(path))
    return digest


def manifest(folder):
    return {str(p.relative_to(folder)): sha(p) for p in sorted(folder.rglob('*'))
            if p.is_file() and not any(part in {'bin', 'obj'} for part in p.relative_to(folder).parts)}


def pin_tree(folder, expected):
    if manifest(folder) != expected:
        raise RuntimeError('Source tree differs: ' + str(folder))
    for name, digest in expected.items():
        pin(folder/name, digest)


def original_source_closure(project):
    """Inspect the literal project references actually consumed by delivery.

    This intentionally rejects unknown MSBuild indirection instead of claiming
    a complete closure from guessed path substitutions.
    """
    selected, visited = {}, set()

    def visit(path):
        path = path.resolve()
        if path in visited: return
        visited.add(path)
        if not path.is_relative_to(ROOT):
            raise RuntimeError('Project source is outside the campaign root: '+str(path))
        selected[path] = sha(path)
        tree = ET.parse(path).getroot()
        if tree.findall('.//Import'):
            raise RuntimeError('Unreviewed project import: '+str(path))
        defaults = tree.find('.//EnableDefaultCompileItems')
        if defaults is None or defaults.text != 'false':
            for source in path.parent.rglob('*.cs'):
                if not any(part in {'bin','obj'} for part in source.relative_to(path.parent).parts):
                    selected[source.resolve()] = sha(source)
        for item in tree.findall('.//Compile'):
            include = item.get('Include')
            if include is None or '$(' in include or ';' in include or item.get('Condition'):
                raise RuntimeError('Unreviewed compile selection: '+str(path))
            matches = sorted(glob.glob(str(path.parent/include), recursive=True))
            if not matches: raise RuntimeError('Compile include has no source: '+include)
            for match in matches:
                source = Path(match).resolve()
                if not source.is_relative_to(ROOT): raise RuntimeError('External compile input: '+str(source))
                if source.is_file(): selected[source] = sha(source)
        for item in tree.findall('.//ProjectReference'):
            include = item.get('Include')
            if include is None or '$(' in include or ';' in include or item.get('Condition'):
                raise RuntimeError('Unreviewed project reference: '+str(path))
            visit(path.parent/include)
    visit(project)
    return selected


def stop_group(process):
    try: os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError: return
    try: process.wait(timeout=3)
    except subprocess.TimeoutExpired: pass
    try: os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError: pass
    process.wait()


def interrupted(signum, frame):
    raise InterruptedError('Runner received signal ' + str(signum))


signal.signal(signal.SIGTERM, interrupted)


def run(command, label, timeout=300, binaries=(), stdin_pipe=None):
    command = list(map(str, command))
    before = {str(p): sha(p) for p in binaries}
    start, code = time.monotonic(), None
    stdout, stderr = out/(label+'.stdout'), out/(label+'.stderr')
    try:
        with stdout.open('wb') as output, stderr.open('wb') as errors:
            process = subprocess.Popen(command, cwd=ROOT.parent, stdout=output, stderr=errors,
                stdin=subprocess.PIPE if stdin_pipe is not None else None,
                env=dict(os.environ, LC_ALL='C', TMPDIR=str(out/'tmp')), start_new_session=True)
            try:
                if stdin_pipe is not None:
                    process.stdin.write(stdin_pipe)
                    process.stdin.flush()
                code = process.wait(timeout=timeout)
            except BaseException:
                stop_group(process)
                raise
            finally:
                if process.stdin is not None: process.stdin.close()
    finally:
        receipt['results'][label] = {
            'command': command, 'exit_code': code, 'elapsed_seconds': time.monotonic()-start,
            'stdout': str(stdout), 'stdout_sha256': sha(stdout),
            'stderr': str(stderr), 'stderr_sha256': sha(stderr),
            'stdin_pipe_hex': stdin_pipe.hex() if stdin_pipe is not None else None,
            'binaries_before': before, 'binaries_after': {p: sha(p) for p in before}}
        save()
    if code != 0:
        raise RuntimeError(label + ' failed; see ' + str(out))
    if receipt['results'][label]['binaries_after'] != before:
        raise RuntimeError(label + ' executed binary changed')
    return stdout.read_bytes(), stderr.read_bytes()


def project(path, owner):
    tree = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(tree, 'PropertyGroup')
    for key, value in {'TargetFramework':'net10.0', 'OutputType':'Exe', 'ImplicitUsings':'enable',
                       'Nullable':'enable', 'AssemblyName':'GuestExecutionStop', 'AllowUnsafeBlocks':'true',
                       'EnableDefaultCompileItems':'false'}.items():
        ET.SubElement(props, key).text = value
    items = ET.SubElement(tree, 'ItemGroup')
    ET.SubElement(items, 'Compile', Include='Program.cs')
    ET.SubElement(items, 'ProjectReference', Include=str(owner))
    ET.SubElement(items, 'TrimmerRootAssembly', Include='TranslatedBlink')
    ET.indent(tree)
    ET.ElementTree(tree).write(path, encoding='unicode')


def observe(row, mode, command, binaries):
    label = mode+'-'+row['id']
    report = out/(label+'.json')
    image = attempt/(row['fixture']+'.elf')
    pin(image, receipt['fixtures'][row['fixture']]['sha256'])
    stdout, stderr = run([*command, image, row['fixture'], row['action'], report], label, 40, binaries)
    if stdout or stderr:
        raise RuntimeError(label + ' emitted process diagnostics')
    value = json.loads(report.read_text())
    pin(report)
    receipt['observations'][label] = {'path': str(report), 'sha256': sha(report), 'value': value}
    if value['fixture'] != row['fixture'] or value['action'] != row['action'] or value['image_sha256'] != sha(image):
        raise RuntimeError(label + ' observation identity differs')
    expected_reason = {'complete':0, 'request':1, 'deadline':2, 'budget':3}[row['action']]
    result = value['result']
    if result['StopReason'] != expected_reason or result['Signal'] or result['SignalCode'] or not result['MemoryReleased']:
        raise RuntimeError(label + ' owner result differs')
    if value['notification_failure'] or any(value[k] for k in
        ['pending_after_run','pending_after_dispose','descriptors_after_dispose','pipe_bytes_after_dispose']):
        raise RuntimeError(label + ' cleanup/notification invariant differs')
    if row['action'] == 'complete':
        if not result['Exited'] or result['ExitStatus'] or result['Halt'] != -10:
            raise RuntimeError(label + ' normal exit differs')
        expected_output = ('ok:'+row['fixture']+'\n').encode()
    else:
        if result['Exited'] or result['Halt'] or not value['first_reason_preserved']:
            raise RuntimeError(label + ' stop status differs')
        expected_output = b''
    if bytes.fromhex(value['stdout_hex']) != expected_output or value['stderr_hex']:
        raise RuntimeError(label + ' guest captures differ')
    if row['action'] in {'request','deadline'} and not value['wait_seen']:
        raise RuntimeError(label + ' did not observe actual execution/wait')
    if row['action'] == 'budget' and result['Instructions'] != 128:
        raise RuntimeError(label + ' budget differs')
    if row['fixture'] == 'pipe':
        if not value['inherited_stdin_pipe'] or not value['stdin_pipe_writer_open_after_run'] or value['stdin_pipe_preloaded_bytes'] != (1 if row['action'] == 'complete' else 0):
            raise RuntimeError(label + ' inherited stdin pipe contract differs')
    save()


try:
    if platform.system() != 'Linux' or platform.machine() != 'x86_64':
        raise RuntimeError('Linux x86-64 native witnesses required')
    for name in ['run.py','Program.cs','fixture.S','fixture.ld','cases.json']:
        pin(HERE/name)
        shutil.copyfile(HERE/name, attempt/name)
        pin(attempt/name)
    pin(ROOT/'scripts/core_inputs.py')
    rows = json.loads((attempt/'cases.json').read_text())
    expected_ids = [f+'-complete' for f in ['cpu','poll','sleep','pipe']] + [f+'-request' for f in ['cpu','poll','sleep','pipe']] + ['poll-deadline','sleep-deadline','cpu-budget']
    if [row['id'] for row in rows] != expected_ids or any(row['id'] != row['fixture']+'-'+row['action'] for row in rows):
        raise RuntimeError('Reviewed 11-case inventory differs')
    receipt['selected_cases'] = rows
    receipt['tools'] = {}
    for name in ['cc','as','ld','readelf','dotnet']:
        path = Path(shutil.which(name) or name).resolve()
        receipt['tools'][name] = {'path':str(path),'sha256':pin(path)}
    delivery_path = args.delivery_receipt.resolve()
    receipt['delivery'] = {'path':str(delivery_path),'sha256':pin(delivery_path)}
    delivery = json.loads(delivery_path.read_text())
    if not delivery['passed'] or '--literal-pool' not in delivery['results']['delivery-link']['command']:
        raise RuntimeError('A passed literal-pool product delivery is required')
    if delivery['assembly']['count'] != 108 or not delivery['product_surface']['c_execution_driver_excluded'] or not delivery['product_surface']['test_frontend_excluded']:
        raise RuntimeError('Expected 108 producer product, without C execution/test frontends')
    pin(delivery['assembly']['path'], delivery['assembly']['sha256'])
    assembly = json.loads(Path(delivery['assembly']['path']).read_text())
    if not assembly['linked'] or assembly['failures'] or assembly.get('diagnostic_replay'):
        raise RuntimeError('Product assembly is not complete')
    profile = Path(delivery['profile'])
    pin(profile/'inputs.json', delivery['profile_inputs_sha256'])
    inputs = json.loads((profile/'inputs.json').read_text())
    for name, digest in inputs['staged_headers'].items(): pin(profile/name, digest)
    for entry in profile_sources(profile, ROOT, inputs):
        path = Path(entry.get('staged_path', ROOT/'ref'/('blink-'+delivery['upstream']['revision'])/entry['path']))
        if not path.is_absolute(): path = ROOT/path
        pin(path, entry['sha256'])
    receipt['producer_objects'] = delivery['objects']
    for row in delivery['objects'].values():
        pin(row['object_path'], row['object_sha256'])
        pin(row['receipt'])
        pin(Path(row['producing_profile'])/'inputs.json', row['producing_profile_inputs_sha256'])
    cli = ROOT.parent/'DotCC/bin/Release/net10.0'
    if compiler_identity(cli) != delivery['compiler']:
        raise RuntimeError('Shared compiler differs from delivery')
    for name, digest in delivery['compiler'].items(): pin(cli/name,digest)
    for name, digest in delivery['postprocessor'].items(): pin(ROOT.parent/'DotCC.PostProcess/bin/Release/net10.0'/name,digest)
    receipt['compiler'] = delivery['compiler']
    raw, final = Path(delivery['raw_snapshot']), Path(delivery['stable_output'])
    pin_tree(raw, delivery['raw_files']); pin_tree(final, delivery['final_files'])
    if not delivery.get('authored_sources_unchanged') or delivery.get('product_host_project') != 'src/Managed.Emulation.Host/Managed.Emulation.Host.csproj':
        raise RuntimeError('Reviewed original-source product references are required')
    authored_sources = delivery['authored_sources']
    for name, digest in authored_sources.items(): pin(ROOT/name,digest)
    # Final product uses original Host/bridge references. Follow those exact
    # project inputs, then retain the same relative layout in tests-only copies.
    source_closure = original_source_closure(final/'TranslatedBlink.csproj')
    external = {path:digest for path,digest in source_closure.items() if not path.is_relative_to(final)}
    bindings = json.loads((profile/'binding-sources.json').read_text())
    frozen_bridges = {Path(name).name:sha(profile/name) for name in bindings['authored_managed']}
    for path, digest in external.items():
        relative = path.relative_to(ROOT)
        if authored_sources.get(str(relative)) != digest:
            raise RuntimeError('Original product input is not in the delivery source identity: '+str(path))
        if relative.is_relative_to('src/Managed.Emulation.Host'):
            frozen = profile/'host-project'/relative.relative_to('src/Managed.Emulation.Host')
            if sha(frozen) != digest: raise RuntimeError('Original Host source differs from canonical snapshot: '+str(path))
        elif path.suffix == '.cs' and frozen_bridges.get(path.name) == digest:
            pass
        else:
            raise RuntimeError('Original product input lacks a reviewed canonical producer: '+str(path))
        pin(path,digest)
    receipt['original_product_references'] = {str(path):digest for path,digest in external.items()}
    receipt['delivery_authored_sources'] = authored_sources
    owner = ROOT/'src/Managed.Emulation.Execution'
    owner_manifest = manifest(owner)
    if 'GuestExecution.cs' not in owner_manifest or 'Managed.Emulation.Execution.csproj' not in owner_manifest:
        raise RuntimeError('Separate authored C# owner is missing')
    pin_tree(owner, owner_manifest)
    receipt['owner_sources'] = owner_manifest
    receipt['fixtures'] = {}
    for fixture in ['cpu','poll','sleep','pipe']:
        obj, image = attempt/(fixture+'.o'), attempt/(fixture+'.elf')
        run(['cc','-c','-nostdlib','-DCASE_'+fixture.upper(),attempt/'fixture.S','-o',obj],fixture+'-assemble')
        run(['ld','-static','--build-id=none','-T',attempt/'fixture.ld',obj,'-o',image],fixture+'-link')
        run(['readelf','-h','-l','-W',image],fixture+'-elf-headers')
        data = image.read_bytes()
        if data[:6] != b'\x7fELF\x02\x01' or struct.unpack_from('<HH',data,16) != (2,62):
            raise RuntimeError('Fixture is not static x86-64 ELF')
        phoff = struct.unpack_from('<Q',data,32)[0]
        phentsize, phnum = struct.unpack_from('<HH',data,54)
        kinds = [struct.unpack_from('<I',data,phoff+i*phentsize)[0] for i in range(phnum)]
        if 3 in kinds or 2 in kinds or kinds.count(1) != 2:
            raise RuntimeError('Fixture program-header selection differs')
        receipt['fixtures'][fixture] = {'path':str(image),'sha256':pin(image),'object_sha256':pin(obj),'program_header_types':kinds}
    native_receipt = Path(delivery['native']['receipt'])
    pin(native_receipt, delivery['native']['sha256'])
    native = json.loads(native_receipt.read_text())
    native_cli = ROOT/'build/native/source/o/blink/blink'
    pin(native_cli,native['binarySha256'])
    if native['upstreamRevision'] != delivery['upstream']['revision']:
        raise RuntimeError('Native witness revision differs')
    receipt['native_reference'] = {'receipt':str(native_receipt),'binary':str(native_cli),'revision':native['upstreamRevision']}
    for fixture in receipt['fixtures']:
        image = attempt/(fixture+'.elf')
        for label, command, binaries in [('linux',[image],[image]),('native-blink',[native_cli,'-jm',image],[native_cli,image])]:
            stdout, stderr = run(command,label+'-'+fixture,30,binaries,
                                 stdin_pipe=b'\x5a' if fixture == 'pipe' else None)
            if stdout != ('ok:'+fixture+'\n').encode() or stderr:
                raise RuntimeError(label+'-'+fixture+' native control differs')
    receipt['native_passed'] = True
    if not args.native_only:
        for mode, library, inventory in [('raw',raw,delivery['raw_files']),('optimized',final,delivery['final_files'])]:
            root = attempt/mode
            copied = root/'generated/TranslatedBlink'
            copied_owner = root/'src/Managed.Emulation.Execution'
            shutil.copytree(library,copied,ignore=shutil.ignore_patterns('bin','obj'))
            shutil.copytree(owner,copied_owner,ignore=shutil.ignore_patterns('bin','obj'))
            if mode == 'optimized':
                for path, digest in external.items():
                    target = root/path.relative_to(ROOT)
                    target.parent.mkdir(parents=True,exist_ok=True)
                    shutil.copyfile(path,target)
                    pin(target,digest)
            for path in [copied,*copied.rglob('*')]: path.chmod(0o755 if path.is_dir() else 0o644)
            pin_tree(copied,inventory); pin_tree(copied_owner,owner_manifest)
            consumer = root/'tests/GuestExecutionStop'
            consumer.mkdir(parents=True)
            shutil.copyfile(attempt/'Program.cs',consumer/'Program.cs')
            project(consumer/'GuestExecutionStop.csproj',copied_owner/'Managed.Emulation.Execution.csproj')
            pin(consumer/'Program.cs'); pin(consumer/'GuestExecutionStop.csproj')
            run(['dotnet','build',consumer/'GuestExecutionStop.csproj','-c','Release'],mode+'-build',900)
            binary = consumer/'bin/Release/net10.0/GuestExecutionStop.dll'
            binaries = sorted([p for p in binary.parent.iterdir() if p.suffix in {'.dll','.json'}])
            for row in rows: observe(row,mode+'-jit',['dotnet',binary],binaries)
            publish = root/'publish'
            run(['dotnet','publish',consumer/'GuestExecutionStop.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],mode+'-aot-build',1200)
            binary = publish/'GuestExecutionStop'
            for row in rows: observe(row,mode+'-aot',[binary],[binary])
        expected = {mode+'-'+row['id'] for mode in ['raw-jit','raw-aot','optimized-jit','optimized-aot'] for row in rows}
        if set(receipt['observations']) != expected:
            raise RuntimeError('Selected case by mode coverage is incomplete')
        receipt['managed_comparisons'] = len(expected)
    for path, digest in receipt['frozen_inputs'].items():
        if sha(path) != digest: raise RuntimeError('Frozen input changed: '+path)
    for row in receipt['results'].values():
        for name in ['stdout','stderr']:
            if sha(row[name]) != row[name+'_sha256']: raise RuntimeError('Closed log changed')
        for path, digest in row['binaries_after'].items():
            if sha(path) != digest: raise RuntimeError('Executed binary changed: '+path)
    if compiler_identity(cli) != delivery['compiler']: raise RuntimeError('Compiler changed during run')
    receipt['final_identity_stable'] = True
    receipt['passed'] = not args.native_only
    print(out/'receipt.json')
except BaseException as error:
    receipt['passed'] = False
    receipt['failure'] = {'type':type(error).__name__,'message':str(error)}
    raise
finally:
    save()
