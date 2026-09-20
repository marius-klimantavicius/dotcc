#!/usr/bin/env python3
"""Compare managed-policy native layout with a derived actual core object link."""
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
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
sys.path.insert(0, str(ROOT / 'scripts'))
from core_inputs import canonical_emission, compiler_identity, emission_identity, profile_sources, OBJECT_OPTIONS


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--profile', type=Path, required=True)
    parser.add_argument('--assembly-receipt', type=Path, required=True)
    args = parser.parse_args()
    profile = args.profile.resolve()
    assembly_path = args.assembly_receipt.resolve()
    base = ROOT / 'artifacts/threaded-layout'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    temporary = attempt / 'tmp'; temporary.mkdir()
    source = attempt / 'inputs'; source.mkdir()
    cli = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
    post = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
    dotnet = Path(shutil.which('dotnet') or '/missing').resolve()
    native_cc = Path(shutil.which('cc') or '/missing').resolve()
    report = dict(scope='internal managed-policy ABI only; no guest execution', passed=False,
                  profile=str(profile), assembly_receipt=str(assembly_path), frozen={}, commands={}, added_objects=[])

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(report, indent=2) + '\n')

    def freeze(path, expected=None):
        path = path.resolve(); value = sha(path)
        if expected is not None and value != expected:
            raise RuntimeError('Input identity differs: ' + str(path))
        if str(path) in report['frozen'] and report['frozen'][str(path)] != value:
            raise RuntimeError('Input changed: ' + str(path))
        report['frozen'][str(path)] = value
        return path

    def copy(path, destination):
        freeze(path); destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, destination)
        if sha(path) != sha(destination): raise RuntimeError('Copy changed: ' + str(path))

    def run(command, label, timeout=180):
        row = dict(command=list(map(str, command)), exit=None)
        report['commands'][label] = row; save()
        log = attempt / (label + '.log')
        with log.open('wb') as stream:
            process = subprocess.Popen(row['command'], cwd=REPO, stdout=stream, stderr=subprocess.STDOUT,
                env=dict(os.environ, LC_ALL='C', TMPDIR=str(temporary)), start_new_session=True)
            try:
                row['exit'] = process.wait(timeout=timeout)
            except BaseException:
                for sig in (signal.SIGTERM, signal.SIGKILL):
                    try: os.killpg(process.pid, sig)
                    except ProcessLookupError: pass
                    try: process.wait(timeout=5); break
                    except subprocess.TimeoutExpired: pass
                row['exit'] = process.returncode
                raise
            finally:
                row['log_sha256'] = sha(log); save()
        if row['exit']: raise RuntimeError(label + ' failed: ' + str(log))
        return log.read_bytes()

    def execute_checked(command, label, binary, closure=None):
        def capture():
            paths = sorted(path for path in closure.rglob('*') if path.is_file()) if closure else [binary]
            return {str(path): sha(path) for path in paths}
        before = capture()
        report.setdefault('execution_inputs', {})[label] = before
        save()
        try:
            return run(command, label, 30)
        finally:
            after = capture()
            report.setdefault('execution_after', {})[label] = after
            save()
            if before != after: raise RuntimeError('Execution binary closure changed: ' + label)

    signal.signal(signal.SIGTERM, lambda *_: (_ for _ in ()).throw(InterruptedError('SIGTERM')))
    try:
        freeze(Path(__file__))
        freeze(dotnet); freeze(native_cc)
        report['dotnet_executable'] = dict(path=str(dotnet), sha256=sha(dotnet))
        report['native_compiler_executable'] = dict(path=str(native_cc), sha256=sha(native_cc))
        sdk_selection = REPO / 'global.json'
        report['global_json'] = dict(path=str(sdk_selection), present=sdk_selection.is_file())
        if sdk_selection.is_file(): freeze(sdk_selection)
        for name in ('Directory.Build.props', 'Directory.Packages.props', 'nuget.config'):
            freeze(REPO / name)
        freeze(ROOT / 'scripts/core_inputs.py')
        freeze(assembly_path)
        assembly = json.loads(assembly_path.read_text())
        inputs_path = freeze(profile / 'inputs.json')
        inputs = json.loads(inputs_path.read_text())
        compiler = compiler_identity(cli.parent)
        if (not assembly.get('linked') or assembly.get('failures')
                or Path(assembly['profile']).resolve() != profile
                or assembly['identity']['profile_inputs_sha256'] != sha(inputs_path)
                or assembly['identity']['compiler_sha256'] != compiler or inputs['compiler'] != compiler):
            raise RuntimeError('Requires a completed exact profile/compiler object assembly')
        report['compiler'] = compiler
        for name, expected in inputs['staged_headers'].items(): freeze(profile / name, expected)
        inventory = json.loads(freeze(ROOT / 'config/source-inventory.json').read_text())
        upstream = ROOT / 'ref' / json.loads(freeze(ROOT / 'config/source-manifest.json').read_text())['upstream']['directory']
        for row in inventory['files']: freeze(upstream / row['path'], row['sha256'])
        entries = profile_sources(profile, ROOT, inputs)
        if len(entries) != 108 or set(assembly['objects']) != {entry['path'] for entry in entries}:
            raise RuntimeError('Exactly 108 canonical producers are required')
        retained = []
        for entry in entries:
            row = assembly['objects'][entry['path']]
            if row['emission_identity'] != emission_identity(profile, ROOT, inputs, entry):
                raise RuntimeError('Retained object emission identity differs: ' + entry['path'])
            retained.append(freeze(Path(row['object_path']), row['object_sha256']))
        report['retained_objects'] = {str(path): sha(path) for path in retained}
        for label, directory in (('compiler', cli.parent), ('postprocessor', post.parent)):
            closure = {}
            for path in sorted(directory.iterdir()):
                if path.is_file() and (path.suffix == '.dll' or path.name.endswith(('.deps.json', '.runtimeconfig.json'))):
                    freeze(path); closure[path.name] = sha(path)
            report[label + '_executable_closure'] = dict(directory=str(directory), files=closure)
        report['native_intrinsic_sources'] = {}
        for name in ('FileLib.cs', 'TimeLib.cs', 'StdlibLib.cs', 'InttypesLib.cs'):
            path = freeze(REPO / 'DotCC.Libc' / name)
            report['native_intrinsic_sources'][str(path)] = sha(path)
        report['native_intrinsic_scope'] = 'Declaration-only GCC support; FILE is pointer-only in measured closure, no native calls through managed libc declarations'
        for name in ('native-intrinsics.h', 'probe.c', 'native-main.c', 'header-order.c', 'Program.cs', 'rows.json'):
            copy(ROOT / 'tests/ThreadedLayout' / name, source / name)
        freeze(ROOT / 'src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.cs')
        for path in (REPO / 'DotCC.Lib/include').rglob('*'):
            if path.is_file(): copy(path, source / 'generic' / path.relative_to(REPO / 'DotCC.Lib/include'))
        (source / 'header-signal.c').write_bytes(b'#define THREAD_SIGNAL_FIRST 1\n' + (source / 'header-order.c').read_bytes())
        (source / 'header-pthread.c').write_bytes((source / 'header-order.c').read_bytes())
        bindings = json.loads((profile / 'binding-sources.json').read_text())
        for name in bindings['authored_managed']: copy(profile / name, source / 'bridges' / Path(name).name)
        for path in (profile / 'host-project').rglob('*'):
            if path.is_file(): copy(path, source / 'Host' / path.relative_to(profile / 'host-project'))
        report['input_snapshot'] = {str(path.relative_to(source)): sha(path)
                                    for path in source.rglob('*') if path.is_file()}
        save()
        run([dotnet, '--info'], 'dotnet-info')
        run([native_cc, '--version'], 'native-compiler')
        atomic_path = Path(run([native_cc, '-print-file-name=include/stdatomic.h'], 'native-atomic-header').decode().strip())
        if not atomic_path.is_file(): raise RuntimeError('Native compiler atomic header unavailable')
        freeze(atomic_path)
        # GCC's real type-generic C11 primitives implement native atomics; the
        # dotcc generic header intentionally relies on managed interception.
        native_flags = [native_cc, '-std=c17', '-O2', '-fno-builtin', '-D_GNU_SOURCE', '-DNDEBUG', '-DNOLINEAR',
            '-Werror=implicit-function-declaration', '-Werror=incompatible-pointer-types',
            '-include', atomic_path, '-include', source / 'native-intrinsics.h', '-I', profile / 'host', '-I', profile / 'authored',
            '-I', profile, '-I', upstream, '-I', source / 'generic']
        native_objects = []
        for name in ('probe', 'header-signal', 'header-pthread'):
            output = attempt / ('native-' + name + '.o'); native_objects.append(output)
            run([*native_flags, '-c', source / (name + '.c'), '-o', output], 'native-' + name)
        run([native_cc, '-std=c17', source / 'native-main.c', *native_objects, '-o', attempt / 'native'], 'native-link')
        report['native_binary_sha256'] = sha(attempt / 'native')
        expected = execute_checked([attempt / 'native'], 'native', attempt / 'native')
        expected_names = json.loads((source / 'rows.json').read_text())['rows']
        if [line.rsplit(' ', 1)[0] for line in expected.decode().splitlines()] != expected_names:
            raise RuntimeError('Native layout row coverage differs')
        added = []
        for name in ('probe', 'header-signal', 'header-pthread'):
            path = source / (name + '.c')
            entry = dict(path='tests/ThreadedLayout/' + path.name, staged_path=str(path), sha256=sha(path))
            headers, canonical_source, identity, key = canonical_emission(profile, ROOT, inputs, entry)
            output = attempt / (name + '.obj.cs')
            command = [dotnet, cli, *OBJECT_OPTIONS, '-I', headers, '-I', upstream,
                       '-I', headers / 'authored', '-I', headers / 'host']
            # The header-order TUs do not include blink/builtin.h and contain
            # neither required macro definition. No override is active there.
            if name == 'probe': command += ['--overrides-file', headers / 'overrides.json']
            command += [canonical_source, '-o', output]
            run(command, 'emit-' + name)
            row = dict(source=entry, canonical_source=str(canonical_source), canonical_headers=str(headers),
                       emission_identity=identity, emission_key=key, object=str(output), sha256=sha(output))
            report['added_objects'].append(row); added.append(output); save()
        raw = attempt / 'raw'
        run([dotnet, cli, *retained, *added, '--emit=managedlib', '--literal-pool', '--nest-types',
             '--class-name', 'BlinkCore', '--namespace', 'Managed.Emulation', '--runtime=c', '-o', raw], 'link')
        report['raw'] = {path.name: sha(path) for path in raw.glob('*.cs')}
        optimized = attempt / 'optimized'; shutil.copytree(raw, optimized)
        for label, generated in (('raw', raw), ('optimized', optimized)):
            consumer = attempt / (label + '-consumer'); consumer.mkdir()
            shutil.copyfile(source / 'Program.cs', consumer / 'Program.cs')
            shutil.copytree(source / 'bridges', consumer / 'Bridges')
            shutil.copytree(source / 'Host', consumer / 'Host')
            xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk'); props = ET.SubElement(xml, 'PropertyGroup')
            for key, value in dict(TargetFramework='net10.0', OutputType='Exe', AllowUnsafeBlocks='true',
                Nullable='enable', ImplicitUsings='enable', AssemblyName='ThreadedLayoutProbe',
                EnableDefaultCompileItems='false',
                DefineConstants='$(DefineConstants);BLINK_FULL_CORE', WarningsAsErrors='CS8500').items():
                ET.SubElement(props, key).text = value
            items = ET.SubElement(xml, 'ItemGroup')
            ET.SubElement(items, 'Compile', Include='Program.cs')
            ET.SubElement(items, 'Compile', Include=str(generated / '*.cs'))
            ET.SubElement(items, 'Compile', Include='Bridges/*.cs')
            ET.SubElement(items, 'ProjectReference', Include='Host/Managed.Emulation.Host.csproj')
            ET.SubElement(items, 'TrimmerRootAssembly', Include='ThreadedLayoutProbe')
            project = consumer / 'ThreadedLayoutProbe.csproj'; ET.ElementTree(xml).write(project, encoding='unicode')
            if label == 'optimized':
                run([dotnet, 'restore', project], label + '-restore')
                run([dotnet, 'build', consumer / 'Host/Managed.Emulation.Host.csproj', '-c', 'Release'],
                    label + '-host-reference-build', 300)
                run([dotnet, post, project, '--in-place'], 'postprocess', 300)
                # Qualify transformed generated C# with the exact original
                # authored bindings, Host and fixture, not transformed copies.
                for directory in ('Host', 'Bridges'):
                    shutil.rmtree(consumer / directory)
                    shutil.copytree(source / ('bridges' if directory == 'Bridges' else 'Host'), consumer / directory)
                shutil.copyfile(source / 'Program.cs', consumer / 'Program.cs')
                report['optimized_authored_restored'] = {
                    str(path.relative_to(consumer)): sha(path)
                    for directory in ('Host', 'Bridges') for path in (consumer / directory).rglob('*') if path.is_file()}
                run([dotnet, 'restore', project], label + '-restore-authored')
            run([dotnet, 'build', project, '-c', 'Release'], label + '-build', 300)
            binary = consumer / 'bin/Release/net10.0/ThreadedLayoutProbe.dll'
            report[label + '_jit_sha256'] = sha(binary)
            if execute_checked([dotnet, binary], label + '-jit', binary, binary.parent) != expected:
                raise RuntimeError(label + ' JIT native layout mismatch')
            publish = attempt / (label + '-aot')
            run([dotnet, 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                 '-o', publish], label + '-publish', 600)
            binary = publish / 'ThreadedLayoutProbe'
            report[label + '_aot_sha256'] = sha(binary)
            if execute_checked([binary], label + '-aot', binary, publish) != expected:
                raise RuntimeError(label + ' NativeAOT native layout mismatch')
            report[label + '_consumer_sources'] = {
                str(path.relative_to(consumer)): sha(path) for path in consumer.rglob('*')
                if path.is_file() and not {'bin', 'obj'}.intersection(path.relative_to(consumer).parts)}
        if sdk_selection.is_file() != report['global_json']['present']:
            raise RuntimeError('global.json presence changed')
        for path, expected_hash in report['frozen'].items():
            if sha(Path(path)) != expected_hash: raise RuntimeError('Input changed: ' + path)
        for name, expected_hash in report['input_snapshot'].items():
            if sha(source / name) != expected_hash: raise RuntimeError('Snapshot changed: ' + name)
        if report['raw'] != {path.name: sha(path) for path in raw.glob('*.cs')}:
            raise RuntimeError('Raw source changed')
        if compiler_identity(cli.parent) != compiler: raise RuntimeError('Compiler changed')
        for label in ('compiler', 'postprocessor'):
            captured = report[label + '_executable_closure']
            actual = {path.name: sha(path) for path in Path(captured['directory']).iterdir()
                      if path.is_file() and (path.suffix == '.dll' or path.name.endswith(('.deps.json', '.runtimeconfig.json')))}
            if actual != captured['files']: raise RuntimeError(label + ' executable closure changed')
        report['optimized'] = {path.name: sha(path) for path in optimized.glob('*.cs')}
        report['passed'] = True
    except BaseException as error:
        report['passed'] = False; report['failure'] = str(error)
        raise
    finally:
        save(); print(attempt / 'receipt.json')


if __name__ == '__main__':
    main()
