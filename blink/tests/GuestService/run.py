#!/usr/bin/env python3
"""Exact six-case native HTTP oracle against an actual translated guest service.

Consumes a frozen product link. No compiler rebuild, C frontend, source patch,
native emulator fallback, controller protocol, or restart path is involved.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
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
parser.add_argument('--assembly-receipt', type=Path, help='Canonical link (historical variant when no delivery receipt is supplied)')
parser.add_argument('--translation-receipt', type=Path, help='Passed delivery receipt: use exact raw and delivered optimized source bytes')
parser.add_argument('--prepare-only', action='store_true')
parser.add_argument('--raw-only', action='store_true', help='Diagnostic raw JIT only; never marks the full matrix passed')
args = parser.parse_args()
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
base = ROOT / 'generated/guest-service'
base.mkdir(parents=True, exist_ok=True)
a = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/guest-service' / a.name
out.mkdir(parents=True)
runtime_tmp = a / 'tmp'
runtime_tmp.mkdir()
receipt = dict(kind='actual translated pinned-service HTTP qualification', passed=False,
               runtime_matrix_passed=False, results={}, runner_sha256=sha(Path(__file__)),
               temporary_directory=str(runtime_tmp),
               limitations=['One pinned valid static service and six existing native wire cases.',
                            'One synchronous owning execution thread, then process discard; no restart or multiple-instance claim.',
                            'Exact guest request/response bytes; private guest port 8080 maps to an ephemeral host loopback port.',
                            'The direct IL inventory roots the translated library and host initializers; the separate execution owner is source-reviewed, not automatically included in that graph.',
                            'Direct IL and publication inventories do not prove isolation of indirect or framework calls.'])


def save():
    (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')


def interrupted(signum, frame):
    raise InterruptedError('GuestService runner received signal ' + str(signum))


signal.signal(signal.SIGTERM, interrupted)


def stop_group(process):
    """Graceful direct-child wait, then kill any remaining group members.

    This does not claim control of descendants that deliberately create a new
    session. The commands here are existing local build tools and this fixture.
    """
    for signum, grace in ((signal.SIGTERM, 2), (signal.SIGKILL, 5)):
        try:
            os.killpg(process.pid, signum)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=grace)
        except subprocess.TimeoutExpired:
            if signum == signal.SIGKILL:
                raise RuntimeError('Child did not exit after process-group kill')


def run(command, label, timeout=180):
    command = list(map(str, command))
    started = time.monotonic()
    with (out / (label + '.log')).open('wb') as log:
        process = subprocess.Popen(command, stdout=log, stderr=subprocess.STDOUT,
                                   start_new_session=True, env=dict(os.environ, LC_ALL='C', TMPDIR=str(runtime_tmp)))
        try:
            code = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            stop_group(process)
            code = 124
        except BaseException as error:
            try:
                stop_group(process)
            finally:
                receipt['results'][label] = dict(command=command, exit_code=process.poll(),
                    seconds=time.monotonic() - started, interrupted=type(error).__name__ + ': ' + str(error))
                save()
            raise
    receipt['results'][label] = dict(command=command, exit_code=code, seconds=time.monotonic() - started)
    save()
    if code:
        raise RuntimeError(label + ' failed: ' + str(out / (label + '.log')))
    return (out / (label + '.log')).read_text()


def project(path, name, output, sources, references, root=None, constants=None):
    tree = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(tree, 'PropertyGroup')
    for key, value in dict(TargetFramework='net10.0', AssemblyName=name, OutputType=output,
                           AllowUnsafeBlocks='true', ImplicitUsings='enable', Nullable='enable',
                           EnableDefaultCompileItems='false', WarningsAsErrors='CS8500').items():
        ET.SubElement(props, key).text = value
    if constants:
        ET.SubElement(props, 'DefineConstants').text = constants
    items = ET.SubElement(tree, 'ItemGroup')
    for source in sources:
        ET.SubElement(items, 'Compile', Include=str(source))
    for reference in references:
        ET.SubElement(items, 'ProjectReference', Include=str(reference))
    if root:
        ET.SubElement(items, 'TrimmerRootAssembly', Include=root)
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.ElementTree(tree).write(path, encoding='unicode')


try:
    translation = None
    if args.translation_receipt:
        translation_path = args.translation_receipt.resolve()
        translation = json.loads(translation_path.read_text())
        if not translation.get('passed'):
            raise RuntimeError('A passing delivery receipt is required')
        assembly_path = Path(translation['assembly']['path'])
        if args.assembly_receipt and args.assembly_receipt.resolve() != assembly_path:
            raise RuntimeError('Supplied canonical receipt differs from delivery')
        if sha(assembly_path) != translation['assembly']['sha256']:
            raise RuntimeError('Delivery assembly receipt changed')
        receipt.update(translation_receipt=str(translation_path), translation_receipt_sha256=sha(translation_path),
                       source_scope='Exact delivery raw snapshot and final optimized source bytes; original authored source closure copied privately')
    elif args.assembly_receipt:
        assembly_path = args.assembly_receipt.resolve()
        receipt['source_scope'] = 'Historical canonical-link variant with independent postprocessing; not byte-identical delivery qualification'
    else:
        raise RuntimeError('Supply --translation-receipt or --assembly-receipt')
    assembly = json.loads(assembly_path.read_text())
    profile = Path(assembly['profile'])
    inputs_path = profile / 'inputs.json'
    inputs = json.loads(inputs_path.read_text())
    if not assembly['linked'] or assembly.get('diagnostic_replay'):
        raise RuntimeError('A canonical product link is required')
    if sha(inputs_path) != assembly['identity']['profile_inputs_sha256']:
        raise RuntimeError('Profile/assembly identity differs')
    if 'authored/managed-driver.c' in assembly['objects'] or any('GuestExecution.c' in name for name in assembly['objects']):
        raise RuntimeError('Product must contain neither CoreProbe nor a C execution frontend')
    cli = ROOT.parent / 'DotCC/bin/Release/net10.0/dotcc.dll'
    post = ROOT.parent / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
    identity = compiler_identity(cli.parent)
    if identity != inputs['compiler']:
        raise RuntimeError('Compiler differs from canonical product provenance')
    frozen = {str(profile / name): digest for name, digest in inputs['staged_headers'].items()}
    frozen[str(inputs_path)] = sha(inputs_path)
    frozen[str(assembly_path)] = sha(assembly_path)
    if translation:
        if translation['profile'] != str(profile) or translation['profile_inputs_sha256'] != sha(inputs_path):
            raise RuntimeError('Delivery/profile identity differs')
        frozen[str(translation_path)] = sha(translation_path)
        delivery_raw = Path(translation['raw_snapshot'])
        delivery_final = Path(translation['stable_output'])
        for directory, files in ((delivery_raw, translation['raw_files']), (delivery_final, translation['final_files'])):
            for relative, digest in files.items():
                frozen[str(directory / relative)] = digest
        for relative, digest in translation['authored_sources'].items():
            frozen[str(ROOT / relative)] = digest
    for name, row in assembly['objects'].items():
        frozen[row['object_path']] = row['object_sha256']
    generated = ROOT / 'generated/core-objects' / assembly['key'] / 'ManagedCore'
    for name, digest in assembly['generated'].items():
        frozen[str(generated / name)] = digest
    for path, digest in frozen.items():
        if sha(Path(path)) != digest:
            raise RuntimeError('Frozen product input differs: ' + path)
    receipt.update(assembly_receipt=str(assembly_path), assembly_receipt_sha256=sha(assembly_path),
                   profile=str(profile), profile_inputs_sha256=sha(inputs_path), compiler=identity,
                   frozen_inputs=frozen, producer_count=len(assembly['objects']))
    receipt['postprocessor'] = {p.name: sha(p) for p in post.parent.iterdir()
                                if p.is_file() and (p.suffix in ('.dll', '.json'))}
    (a / 'bridges').mkdir()
    if translation:
        host_prefix = str(Path(translation['product_host_project']).parent) + '/'
        for relative in translation['authored_sources']:
            if relative.startswith(host_prefix):
                target = a / 'host' / relative[len(host_prefix):]
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(ROOT / relative, target)
        for relative in translation['product_source_links']:
            target = a / 'bridges' / Path(relative).name
            if target.exists():
                raise RuntimeError('Bridge basename collision')
            shutil.copyfile(ROOT / relative, target)
    else:
        shutil.copytree(profile / 'host-project', a / 'host')
        bindings = json.loads((profile / 'binding-sources.json').read_text())
        for relative in bindings['authored_managed']:
            shutil.copyfile(profile / relative, a / 'bridges' / Path(relative).name)
    if not (a / 'bridges/HostGuestSignalsBridge.cs').exists():
        raise RuntimeError('The actual product guest signal bridge is required')
    for mode in ('raw', 'optimized'):
        (a / (mode + '-generated')).mkdir()
        selected_sources = ((delivery_raw if mode == 'raw' else delivery_final) / 'Sources') if translation else generated
        sources = list(selected_sources.glob('*.cs'))
        if not sources:
            raise RuntimeError('No selected generated source files')
        for source in sources:
            shutil.copyfile(source, a / (mode + '-generated') / source.name)
    shutil.copyfile(ROOT / 'tests/GuestService/Program.cs', a / 'Program.cs')
    shutil.copyfile(ROOT / 'src/Managed.Emulation.Execution/GuestExecution.cs', a / 'GuestExecution.cs')
    shutil.copytree(ROOT / 'tools/BoundaryAudit', a / 'audit-tool', ignore=shutil.ignore_patterns('bin', 'obj'))
    service = ROOT / 'build/guest/service'
    fixture = ROOT / 'tests/ServiceFixture/instance.txt'
    qualified = json.loads((ROOT / 'tests/ServiceFixture/qualified-build.json').read_text())
    if sha(service) != qualified['executable_sha256']:
        raise RuntimeError('Pinned ELF differs')
    shutil.copyfile(service, a / 'service')
    shutil.copyfile(fixture, a / 'instance.txt')
    cases = ('health', 'file', 'fragmented', 'large', 'missing', 'stop')
    receipt['native_oracles'] = {}
    for mode in ('native-linux', 'native-blink'):
        source = ROOT / 'artifacts/guest' / mode
        oracle = json.loads((source / 'result.json').read_text())
        if not oracle['passed'] or oracle['guest_sha256'] != sha(service) or oracle['fixture_sha256'] != sha(fixture):
            raise RuntimeError('Native oracle is not qualified for these exact fixture bytes')
        if tuple(row['name'] for row in oracle['cases']) != cases or not all(row['passed'] for row in oracle['cases']):
            raise RuntimeError('Native oracle must have all six cases')
        target = a / mode
        target.mkdir()
        shutil.copyfile(source / 'result.json', target / 'result.json')
        for row in oracle['cases']:
            for kind in ('request', 'response'):
                name = row['name'] + '.' + kind
                if sha(source / name) != row[kind + '_sha256']:
                    raise RuntimeError('Native wire artifact changed: ' + name)
                shutil.copyfile(source / name, target / name)
                if mode == 'native-blink' and (target / name).read_bytes() != (a / 'native-linux' / name).read_bytes():
                    raise RuntimeError('Native Linux and native Blink wire oracles differ')
        receipt['native_oracles'][mode] = dict(receipt=str(source / 'result.json'), sha256=sha(source / 'result.json'), historical=True)
    for mode in ('raw', 'optimized'):
        folder = a / mode
        library = folder / 'library/TranslatedBlink.csproj'
        execution_project = folder / 'execution/Managed.Emulation.Execution.csproj'
        consumer = folder / 'consumer/GuestService.csproj'
        project(library, 'TranslatedBlink', 'Library', [a / (mode + '-generated') / '*.cs', a / 'bridges/*.cs'],
                [a / 'host/Managed.Emulation.Host.csproj'], constants='BLINK_FULL_CORE')
        project(execution_project, 'Managed.Emulation.Execution', 'Library', [a / 'GuestExecution.cs'], [library])
        project(consumer, 'GuestService', 'Exe', [a / 'Program.cs'], [execution_project], root='TranslatedBlink')
    snapshots = {str(p.relative_to(a)): sha(p) for p in a.rglob('*') if p.is_file()}
    receipt['prepared_inputs'] = snapshots
    receipt['prepared'] = True
    save()
    if args.prepare_only:
        print(out / 'receipt.json')
        sys.exit(0)
    run(['dotnet', '--info'], 'dotnet-info', 30)
    run(['dotnet', 'build', a / 'audit-tool/BoundaryAudit.csproj', '-c', 'Release'], 'audit-build', 180)
    auditor = a / 'audit-tool/bin/Release/net10.0/BoundaryAudit.dll'
    for mode in ('raw', 'optimized'):
        folder = a / mode
        library = folder / 'library/TranslatedBlink.csproj'
        consumer = folder / 'consumer/GuestService.csproj'
        if mode == 'optimized' and not translation:
            run(['dotnet', 'restore', library], 'optimized-restore')
            run(['dotnet', post, library, '--in-place'], 'optimized-postprocess', 600)
        run(['dotnet', 'build', consumer, '-c', 'Release'], mode + '-build', 600)
        audit_path = out / (mode + '-boundary-audit.json')
        run(['dotnet', auditor, folder / 'consumer/bin/Release/net10.0/TranslatedBlink.dll', audit_path], mode + '-boundary-audit')
        audit = json.loads(audit_path.read_text())
        if not audit['complete'] or audit['traversedNativeImports']:
            raise RuntimeError('Incomplete direct IL inventory or traversed native imports')
        forbidden = ('System.Environment::Void Exit(', 'System.Diagnostics.Process::', 'System.Runtime.InteropServices.NativeLibrary::')
        if any(any(value in call for value in forbidden) for call in audit['externalManagedCalls']):
            raise RuntimeError('Direct process/native-loader escape in generated library')
        for runtime in ('jit', 'aot'):
            label = mode + '-' + runtime
            if runtime == 'jit':
                binary = folder / 'consumer/bin/Release/net10.0/GuestService.dll'
                command = ['dotnet', binary]
                dependencies = {str(p): sha(p) for p in binary.parent.glob('*.dll')}
            else:
                publish = folder / 'publish'
                run(['dotnet', 'publish', consumer, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', publish], label + '-build', 900)
                binary = publish / 'GuestService'
                command = [binary]
                dependencies = {}
            digest = sha(binary)
            receipt.setdefault('executables', {})[label] = dict(path=str(binary), sha256=digest,
                managed_dependencies=dependencies)
            save()
            result_dir = out / label
            result_dir.mkdir()
            run([*command, a / 'service', a / 'instance.txt', a / 'native-linux', result_dir], label, 145)
            result_path = result_dir / 'result.json'
            result = json.loads(result_path.read_text())
            if not result['passed'] or len(result['cases']) != 6 or not all(case['Passed'] for case in result['cases']):
                raise RuntimeError(label + ' missing successful six-case service result')
            if sha(binary) != digest or any(sha(Path(p)) != value for p, value in dependencies.items()):
                raise RuntimeError(label + ' execution-time binary changed')
            receipt['results'][label].update(binary_sha256=digest, managed_dependencies=dependencies,
                service_result=str(result_path), service_result_sha256=sha(result_path))
            save()
            if args.raw_only:
                for path, value in frozen.items():
                    if sha(Path(path)) != value:
                        raise RuntimeError('Frozen product changed during raw qualification: ' + path)
                for name, value in snapshots.items():
                    if sha(a / name) != value:
                        raise RuntimeError('Prepared source changed during raw qualification: ' + name)
                receipt['raw_jit_passed'] = True
                print(out / 'receipt.json')
                sys.exit(0)
    for path, digest in frozen.items():
        if sha(Path(path)) != digest:
            raise RuntimeError('Frozen product changed during qualification: ' + path)
    for name, digest in snapshots.items():
        if not (not translation and name.startswith('optimized-generated/')) and sha(a / name) != digest:
            raise RuntimeError('Consumer snapshot changed: ' + name)
    if compiler_identity(cli.parent) != identity:
        raise RuntimeError('Compiler process inputs changed')
    if sha(Path(__file__)) != receipt['runner_sha256']:
        raise RuntimeError('Qualification runner changed')
    receipt['runtime_matrix_passed'] = True
    receipt['passed'] = True
    save()
    run(['python3', ROOT / 'scripts/audit-published-core.py', out / 'receipt.json'], 'publication-audit', 120)
    receipt['publication_audit_receipt'] = (out / 'publication-audit.log').read_text().strip()
    print(out / 'receipt.json')
except BaseException as error:
    if isinstance(error, SystemExit) and error.code == 0:
        raise
    receipt['passed'] = False
    receipt['runtime_matrix_passed'] = False
    receipt['error'] = type(error).__name__ + ': ' + str(error)
    raise
finally:
    save()
