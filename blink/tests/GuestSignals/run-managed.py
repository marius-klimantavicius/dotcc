#!/usr/bin/env python3
"""Qualify one valid threaded ELF through the same authored C# owner in four modes."""
import argparse
import glob
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

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
from core_inputs import compiler_identity
from semantic_delivery import pin_semantic_delivery

EXPECTED = b'guest-signals: self=1 child=1 sender=checked pending=checked\n'
MODES = {'raw-jit', 'raw-aot', 'optimized-jit', 'optimized-aot'}
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def manifest(folder):
    return {str(p.relative_to(folder)): sha(p) for p in sorted(folder.rglob('*'))
            if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(folder).parts)}


def source_closure(project):
    selected, visited = {}, set()

    def visit(path):
        path = path.resolve()
        if path in visited: return
        visited.add(path)
        if not path.is_relative_to(ROOT): raise RuntimeError('External project: ' + str(path))
        selected[path] = sha(path)
        tree = ET.parse(path).getroot()
        if tree.findall('.//Import'): raise RuntimeError('Unreviewed MSBuild import')
        defaults = tree.find('.//EnableDefaultCompileItems')
        if defaults is None or defaults.text != 'false':
            for name, digest in manifest(path.parent).items():
                if name.endswith('.cs'): selected[path.parent / name] = digest
        for node in tree.findall('.//Compile'):
            include = node.get('Include')
            if not include or '$(' in include or ';' in include or node.get('Condition'):
                raise RuntimeError('Unreviewed Compile selection')
            paths = [Path(name).resolve() for name in glob.glob(str(path.parent / include), recursive=True)]
            if not paths: raise RuntimeError('Compile selection is empty')
            for source in paths:
                if not source.is_relative_to(ROOT): raise RuntimeError('External source')
                if source.is_file(): selected[source] = sha(source)
        for node in tree.findall('.//ProjectReference'):
            include = node.get('Include')
            if not include or '$(' in include or ';' in include or node.get('Condition'):
                raise RuntimeError('Unreviewed ProjectReference selection')
            visit(path.parent / include)
    visit(project)
    return selected


def project(path, owner):
    root = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(root, 'PropertyGroup')
    for key, value in dict(TargetFramework='net10.0', OutputType='Exe', ImplicitUsings='enable',
                          Nullable='enable', AssemblyName='GuestSignals', AllowUnsafeBlocks='true',
                          EnableDefaultCompileItems='false').items():
        ET.SubElement(props, key).text = value
    items = ET.SubElement(root, 'ItemGroup')
    ET.SubElement(items, 'Compile', Include='Program.cs')
    ET.SubElement(items, 'ProjectReference', Include=str(owner))
    ET.SubElement(items, 'TrimmerRootAssembly', Include='TranslatedBlink')
    ET.indent(root)
    ET.ElementTree(root).write(path, encoding='unicode')


def group_exists(pid):
    try: os.killpg(pid, 0); return True
    except ProcessLookupError: return False


def cleanup(process):
    signals = []
    for sig, seconds in ((signal.SIGTERM, 3), (signal.SIGKILL, 5)):
        process.poll()
        if not group_exists(process.pid): break
        try: os.killpg(process.pid, sig); signals.append(sig.name)
        except ProcessLookupError: break
        end = time.monotonic() + seconds
        while time.monotonic() < end:
            process.poll()
            if not group_exists(process.pid): break
            time.sleep(0.05)
    return dict(signals=signals, group_gone=not group_exists(process.pid), leader_exit=process.poll())


def interrupted(number, frame):
    raise InterruptedError('Received signal ' + str(number))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--delivery-receipt', type=Path, required=True)
    parser.add_argument('--native-receipt', type=Path, required=True)
    parser.add_argument('--delivery-sha256', required=True)
    args = parser.parse_args()
    base = ROOT / 'artifacts/guest-signals-managed'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    (attempt / 'tmp').mkdir(); (attempt / 'inputs').mkdir()
    receipt = dict(passed=False, completed=False, scope='One valid ELF, same C# threaded owner, four fresh managed processes',
                   inputs={}, commands={}, observations={}, trees={}, tools={}, attempt=str(attempt))
    receipt['environment_overrides'] = dict(LC_ALL='C', TMPDIR=str(attempt / 'tmp'), MSBUILDDISABLENODEREUSE='1')
    print(attempt, flush=True)

    def save():
        temporary = attempt / 'receipt.tmp'
        temporary.write_text(json.dumps(receipt, indent=2) + '\n')
        temporary.replace(attempt / 'receipt.json')

    def pin(path, expected=None):
        path = Path(path).resolve(); digest = sha(path)
        if expected is not None and digest != expected: raise RuntimeError('Identity differs: ' + str(path))
        if receipt['inputs'].setdefault(str(path), digest) != digest: raise RuntimeError('Input changed: ' + str(path))
        return path

    def pin_tree(folder, expected):
        if manifest(folder) != expected: raise RuntimeError('Tree differs: ' + str(folder))
        receipt['trees'][str(folder)] = expected
        for name, digest in expected.items(): pin(folder / name, digest)

    def run(command, label, timeout=900, binaries=()):
        row = dict(command=list(map(str, command)), timeout_seconds=timeout,
                   binaries_before={str(p): sha(p) for p in binaries})
        receipt['commands'][label] = row; save()
        out, err = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        process, start = None, time.monotonic()
        try:
            with out.open('wb') as stdout, err.open('wb') as stderr:
                process = subprocess.Popen(row['command'], cwd=ROOT.parent, stdout=stdout, stderr=stderr,
                    stdin=subprocess.DEVNULL, env=dict(os.environ, **receipt['environment_overrides']),
                    start_new_session=True)
                row['process_group'] = process.pid
                row['exit_code'] = process.wait(timeout=timeout)
        except BaseException as error:
            row['error'] = f'{type(error).__name__}: {error}'
            raise
        finally:
            if process is not None: row['cleanup'] = cleanup(process)
            row['files'] = {str(path): sha(path) for path in (out, err) if path.exists()}
            row['binaries_after'] = {name: sha(name) for name in row['binaries_before']}
            row['seconds'] = time.monotonic() - start
            save()
        if row['exit_code'] != 0 or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ' did not complete normally')
        if row['binaries_before'] != row['binaries_after']: raise RuntimeError('Binary changed during ' + label)
        print(label + ': passed', flush=True)
        return out.read_bytes(), err.read_bytes()

    try:
        if platform.system() != 'Linux' or platform.machine() != 'x86_64':
            raise RuntimeError('This qualification requires Linux x86-64')
        for name in ('Program.cs', 'run-managed.py', 'fixture.S', 'fixture.ld'):
            source = pin(HERE / name); shutil.copy2(source, attempt / 'inputs' / name); pin(attempt / 'inputs' / name)
        pin(ROOT / 'scripts/core_inputs.py')
        for name in ('Directory.Build.props', 'Directory.Packages.props', 'nuget.config'):
            pin(ROOT.parent / name)
        native_path = pin(args.native_receipt)
        native = json.loads(native_path.read_text())
        if (native.get('kind') != 'normal-linux-guest-signals' or
                not all(native.get(key) is True for key in ('passed', 'completed', 'final_identities_stable'))):
            raise RuntimeError('Passing native GuestSignals receipt required')
        if len(native['executions']) != 1 or native['executions'][0]['mode'] != 'linux' or not native['executions'][0]['passed']:
            raise RuntimeError('Actual Linux signal oracle required')
        for name, digest in native['inputs'].items(): pin(name, digest)
        for row in native['commands'].values():
            for name, digest in row['files'].items(): pin(name, digest)
        for name in ('fixture.S', 'fixture.ld'):
            pin(native_path.parent / 'inputs' / name, sha(HERE / name))
        original_image = pin(native['elf']['path'], native['elf']['sha256'])
        image = attempt / 'guest-signals.elf'; shutil.copy2(original_image, image); pin(image, native['elf']['sha256'])
        markers = native['handler_markers']
        if set(markers) != {'handler_start_syscall', 'handler_end_syscall'}:
            raise RuntimeError('Actual native symbol markers required')
        receipt['native'] = dict(path=str(native_path), sha256=sha(native_path), elf_sha256=sha(image), handler_markers=markers)
        delivery_path = pin(args.delivery_receipt, args.delivery_sha256)
        delivery = json.loads(delivery_path.read_text())
        if (delivery.get('passed') is not True or delivery.get('selected_profile') != 'threaded' or
                delivery.get('authored_sources_unchanged') is not True or
                Path(delivery['stable_output']).resolve() != (ROOT / 'generated/TranslatedBlink').resolve()):
            raise RuntimeError('Passing current public threaded delivery required')
        for name, digest in delivery['authored_sources'].items(): pin(ROOT / name, digest)
        for name, digest in delivery['postprocessor'].items(): pin(ROOT.parent / 'DotCC.PostProcess/bin/Release/net10.0' / name, digest)
        for row in delivery['results'].values():
            if row['exit_code'] != 0: raise RuntimeError('Public producer command failed')
            pin(row['log'], row['log_sha256'])
        cli = ROOT.parent / 'DotCC/bin/Release/net10.0'
        if compiler_identity(cli) != delivery['compiler']: raise RuntimeError('Compiler differs from threaded producer')
        receipt['compiler'] = delivery['compiler']
        for name, digest in delivery['compiler'].items(): pin(cli / name, digest)
        receipt['delivery'] = dict(path=str(delivery_path), sha256=sha(delivery_path), assembly=delivery['assembly'])
        assembly_path = pin(delivery['assembly']['path'], delivery['assembly']['sha256'])
        assembly = json.loads(assembly_path.read_text())
        if assembly['failures'] or not assembly['linked'] or len(assembly['selected']) != 108:
            raise RuntimeError('Complete unchanged 108-producer threaded assembly required')
        for name, row in assembly['objects'].items():
            pin(row['object_path'], row['object_sha256'])
            if any(delivery['objects'][name][key] != row[key] for key in ('object_path', 'object_sha256')):
                raise RuntimeError('Public object differs: ' + name)
        receipt['semantic_intrinsics'] = pin_semantic_delivery(delivery, assembly, pin)
        profile = Path(delivery['profile'])
        inputs = json.loads(pin(profile / 'inputs.json', delivery['profile_inputs_sha256']).read_text())
        for name, digest in inputs['staged_headers'].items(): pin(profile / name, digest)
        raw, final = Path(delivery['raw_snapshot']), Path(delivery['stable_output'])
        pin_tree(raw, delivery['raw_files']); pin_tree(final, delivery['final_files'])
        closure = source_closure(final / 'TranslatedBlink.csproj')
        external = {p: digest for p, digest in closure.items() if not p.is_relative_to(final)}
        for path, digest in external.items():
            if delivery['authored_sources'].get(str(path.relative_to(ROOT))) != digest:
                raise RuntimeError('Unpinned original product source: ' + str(path))
            pin(path, digest)
        receipt['original_product_sources'] = {str(p): digest for p, digest in external.items()}
        owner = ROOT / 'src/Managed.Emulation.ThreadedExecution'
        owner_files = manifest(owner); pin_tree(owner, owner_files)
        receipt['owner_sources'] = owner_files
        dotnet = pin(shutil.which('dotnet') or '')
        receipt['tools']['dotnet'] = dict(path=str(dotnet), sha256=sha(dotnet))
        pin(sys.executable)
        receipt['tools']['python'] = dict(path=str(Path(sys.executable).resolve()), sha256=sha(sys.executable), version=sys.version)
        info, _ = run([dotnet, '--info'], 'dotnet-info')
        version, _ = run([dotnet, '--version'], 'dotnet-version')
        receipt['tools']['dotnet']['info'] = info.decode(); receipt['tools']['dotnet']['sdk_version'] = version.decode().strip()
        sdk = dotnet.parent / 'sdk' / version.decode().strip()
        for name in ('dotnet.dll', 'MSBuild.dll', 'Roslyn/bincore/csc.dll'): pin(sdk / name)
        runtimes, _ = run([dotnet, '--list-runtimes'], 'dotnet-runtimes')
        receipt['tools']['dotnet']['runtimes'] = runtimes.decode()
        for line in runtimes.decode().splitlines():
            fields = line.split()
            if len(fields) == 3 and fields[0] == 'Microsoft.NETCore.App':
                directory = Path(fields[2].strip('[]')) / fields[1]
                for name in ('libcoreclr.so', 'libhostpolicy.so', 'System.Private.CoreLib.dll'): pin(directory / name)
        for mode, library, inventory in (('raw', raw, delivery['raw_files']), ('optimized', final, delivery['final_files'])):
            private = attempt / mode
            copied = private / 'generated/TranslatedBlink'
            copied_owner = private / 'src/Managed.Emulation.ThreadedExecution'
            shutil.copytree(library, copied, ignore=shutil.ignore_patterns('bin', 'obj'))
            shutil.copytree(owner, copied_owner, ignore=shutil.ignore_patterns('bin', 'obj'))
            if mode == 'optimized':
                for original, digest in external.items():
                    target = private / original.relative_to(ROOT)
                    target.parent.mkdir(parents=True, exist_ok=True); shutil.copyfile(original, target); pin(target, digest)
            for path in [copied, *copied.rglob('*')]: path.chmod(0o755 if path.is_dir() else 0o644)
            pin_tree(copied, inventory); pin_tree(copied_owner, owner_files)
            consumer = private / 'tests/GuestSignals'; consumer.mkdir(parents=True)
            shutil.copy2(attempt / 'inputs/Program.cs', consumer / 'Program.cs')
            csproj = consumer / 'GuestSignals.csproj'
            project(csproj, copied_owner / 'Managed.Emulation.ThreadedExecution.csproj')
            pin(csproj); pin(consumer / 'Program.cs')
            run([dotnet, 'build', csproj, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'], mode + '-build')
            binary = consumer / 'bin/Release/net10.0/GuestSignals.dll'
            jit_binaries = sorted(p for p in binary.parent.iterdir() if p.suffix in ('.dll', '.json'))
            for execution, command, binaries in (('jit', [dotnet, binary], jit_binaries), ('aot', None, None)):
                if execution == 'aot':
                    publish = private / 'publish'
                    run([dotnet, 'publish', csproj, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', publish], mode + '-aot-build', 1200)
                    binary = publish / 'GuestSignals'; command, binaries = [binary], [binary]
                label = mode + '-' + execution
                report_path = attempt / (label + '.json')
                try:
                    stdout, stderr = run([*command, image, report_path, format(markers['handler_start_syscall'], 'x'), format(markers['handler_end_syscall'], 'x')], label, 45, [image, *binaries])
                finally:
                    diagnostics = [report_path, Path(str(report_path) + '.guest.stdout'), Path(str(report_path) + '.guest.stderr')]
                    receipt.setdefault('execution_diagnostics', {})[label] = {
                        str(path): sha(pin(path)) for path in diagnostics if path.exists()}
                    save()
                if stdout != EXPECTED or stderr: raise RuntimeError(label + ' output differs from actual Linux witness')
                pin(report_path)
                report = json.loads(report_path.read_text())
                required = ('passed', 'owner_joined', 'is_quiescent', 'all_workers_joined', 'memory_released', 'exited')
                if not all(report.get(key) is True for key in required) or report['exit_status'] != 0 or report['stop_reason'] != 'None':
                    raise RuntimeError(label + ' owner result differs')
                if report['image_sha256'] != sha(image) or report['stdout_hex'] != EXPECTED.hex() or report['stderr_hex']:
                    raise RuntimeError(label + ' report bytes differ')
                threads = report['threads']
                if len(threads) != 2 or len({t['tid'] for t in threads}) != 2 or any(t['tid'] <= 0 or not t['machine_released'] or
                        t['status'] != 0 or t['halt'] != 0 or t['signal'] != 0 or t['instructions'] < 12288 for t in threads):
                    raise RuntimeError(label + ' thread state differs')
                if sorted(t['termination'] for t in threads) != ['GroupExit', 'ThreadExit']:
                    raise RuntimeError(label + ' thread exits differ')
                if report['notification_failure'] or report['trace_overflow'] or report['descriptors_after_dispose'] or report['pending_after_dispose']:
                    raise RuntimeError(label + ' cleanup/observer failure')
                accounting = report['handler_accounting']
                if (len(accounting) != 2 or {row['tid'] for row in accounting} != {t['tid'] for t in threads} or
                        any(row['end_owner_instructions'] - row['start_owner_instructions'] != row['owner_count_delta'] or
                            row['owner_count_delta'] < 12288 for row in accounting)):
                    raise RuntimeError(label + ' handler instruction accounting differs')
                receipt['observations'][label] = dict(report=str(report_path), sha256=sha(report_path), value=report)
                save()
        if set(receipt['observations']) != MODES: raise RuntimeError('Exact four-mode coverage missing')
        if compiler_identity(cli) != receipt['compiler']: raise RuntimeError('Compiler changed')
        if source_closure(final / 'TranslatedBlink.csproj') != closure: raise RuntimeError('Original product source closure changed')
        for name, digest in receipt['inputs'].items():
            if sha(name) != digest: raise RuntimeError('Frozen input changed: ' + name)
        for folder, files in receipt['trees'].items():
            if manifest(Path(folder)) != files: raise RuntimeError('Frozen source tree changed: ' + folder)
        for row in receipt['commands'].values():
            for name, digest in {**row['files'], **row['binaries_after']}.items():
                if sha(name) != digest: raise RuntimeError('Closed artifact changed: ' + name)
        receipt.update(passed=True, final_identities_stable=True, managed_comparisons=4)
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        receipt['completed'] = True; save()
    print(f"passed={receipt['passed']} receipt={attempt / 'receipt.json'}", flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
