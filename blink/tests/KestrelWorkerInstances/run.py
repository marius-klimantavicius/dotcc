#!/usr/bin/env python3
"""Actual Kestrel worker/controller lifecycle over a reviewed public threaded delivery."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['tests/KestrelWorkerInstances/run.py']

import argparse
import glob
import hashlib
import json
import os
import runpy
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(ROOT / 'scripts'))
from core_inputs import compiler_identity
from semantic_delivery import pin_semantic_delivery

MODES = {'raw-jit', 'raw-aot', 'optimized-jit', 'optimized-aot'}
EXPECTED = b'actual Kestrel workers: two simultaneous isolated images; ten native HTTP comparisons; cooperative stop; fresh-process restart; idle deadline\n'
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def manifest(folder, build_outputs=False):
    return {str(p.relative_to(folder)): sha(p) for p in sorted(folder.rglob('*'))
            if p.is_file() and (build_outputs or not {'bin', 'obj'}.intersection(p.relative_to(folder).parts))}


def source_closure(project):
    result, visited = {}, set()
    def visit(path):
        path = path.resolve()
        if path in visited: return
        visited.add(path)
        if not path.is_relative_to(ROOT): raise RuntimeError('External project')
        result[path] = sha(path)
        tree = ET.parse(path).getroot()
        if tree.findall('.//Import'): raise RuntimeError('Unreviewed project import')
        defaults = tree.find('.//EnableDefaultCompileItems')
        if defaults is None or defaults.text != 'false':
            for name, value in manifest(path.parent).items():
                if name.endswith('.cs'): result[path.parent / name] = value
        for node in tree.findall('.//Compile'):
            include = node.get('Include')
            if not include or '$(' in include or ';' in include or node.get('Condition'): raise RuntimeError('Unreviewed Compile')
            paths = [Path(p).resolve() for p in glob.glob(str(path.parent / include), recursive=True)]
            if not paths: raise RuntimeError('Empty Compile')
            for source in paths:
                if not source.is_relative_to(ROOT): raise RuntimeError('External source')
                if source.is_file(): result[source] = sha(source)
        for node in tree.findall('.//ProjectReference'):
            include = node.get('Include')
            if not include or '$(' in include or ';' in include or node.get('Condition'): raise RuntimeError('Unreviewed reference')
            visit(path.parent / include)
    visit(project)
    return result


def group_exists(pid):
    try: os.killpg(pid, 0); return True
    except ProcessLookupError: return False


def cleanup(process):
    sent = []
    for sig, seconds in ((signal.SIGTERM, 3), (signal.SIGKILL, 5)):
        process.poll()
        if not group_exists(process.pid): break
        try: os.killpg(process.pid, sig); sent.append(sig.name)
        except ProcessLookupError: break
        end = time.monotonic() + seconds
        while time.monotonic() < end:
            process.poll()
            if not group_exists(process.pid): break
            time.sleep(.05)
    return dict(signals=sent, group_gone=not group_exists(process.pid), leader_exit=process.poll())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--delivery-receipt', type=Path, required=True)
    parser.add_argument('--delivery-sha256', required=True)
    parser.add_argument('--native-receipt', type=Path, required=True)
    parser.add_argument('--profile-receipt', type=Path, required=True)
    args = parser.parse_args()
    if len(args.delivery_sha256) != 64 or any(c not in '0123456789abcdef' for c in args.delivery_sha256):
        parser.error('--delivery-sha256 must contain exactly 64 lowercase hexadecimal characters')
    base = ROOT / 'artifacts/kestrel-worker-instances'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base)); (attempt / 'tmp').mkdir()
    receipt = dict(passed=False, completed=False, inputs={}, source_trees={}, commands={}, modes={},
                   scope='Four actual worker processes per mode: simultaneous image paths, normal HTTP, cooperative stop, restart, idle deadline')
    env = dict(os.environ, LC_ALL='C', TMPDIR=str(attempt / 'tmp'), MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0')
    receipt['environment'] = {k: env[k] for k in ('LC_ALL', 'TMPDIR', 'MSBUILDDISABLENODEREUSE', 'DOTNET_CLI_USE_MSBUILD_SERVER')}
    print(attempt, flush=True)
    def save():
        (attempt / 'receipt.tmp').write_text(json.dumps(receipt, indent=2) + '\n')
        (attempt / 'receipt.tmp').replace(attempt / 'receipt.json')
    def pin(path, expected=None):
        path = Path(path).resolve(); value = sha(path)
        if expected is not None and expected != value: raise RuntimeError('Identity differs: ' + str(path))
        if receipt['inputs'].setdefault(str(path), value) != value: raise RuntimeError('Input changed: ' + str(path))
        return path
    def tree(folder, expected=None):
        value = manifest(folder)
        if expected is not None and value != expected: raise RuntimeError('Source tree differs: ' + str(folder))
        receipt['source_trees'][str(folder)] = value
        for name, digest in value.items(): pin(folder / name, digest)
        return value
    def check():
        for name, digest in receipt['inputs'].items():
            if sha(name) != digest: raise RuntimeError('Frozen input changed: ' + name)
        for name, expected in receipt['source_trees'].items():
            if manifest(Path(name)) != expected: raise RuntimeError('Source closure changed: ' + name)
        for name, expected in receipt['optional_inputs'].items():
            if (sha(name) if Path(name).is_file() else None) != expected: raise RuntimeError('Build configuration changed: ' + name)
    def run(command, label, timeout=900, binaries=()):
        check()
        row = dict(command=list(map(str, command)), binary_before={str(p): sha(p) for p in binaries}, timeout_seconds=timeout)
        receipt['commands'][label] = row; save()
        out, err = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        process = None; start = time.monotonic()
        try:
            with out.open('wb') as stdout, err.open('wb') as stderr:
                process = subprocess.Popen(row['command'], cwd=REPO, env=env, stdin=subprocess.DEVNULL,
                    stdout=stdout, stderr=stderr, start_new_session=True)
                row['process_group'] = process.pid
                row['exit_code'] = process.wait(timeout=timeout)
        except BaseException as error:
            row['error'] = repr(error); raise
        finally:
            if process is not None: row['cleanup'] = cleanup(process)
            row['files'] = {str(p): sha(p) for p in (out, err) if p.exists()}
            row['binary_after'] = {name: sha(name) for name in row['binary_before']}
            row['seconds'] = time.monotonic() - start; save()
        if row['exit_code'] != 0 or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ' did not finish normally')
        if row['binary_before'] != row['binary_after']: raise RuntimeError('Execution binary changed')
        check(); print(label + ': passed', flush=True)
        return out.read_bytes(), err.read_bytes()
    try:
        pin(__file__); pin(ROOT / 'scripts/core_inputs.py')
        receipt['optional_inputs'] = {str(REPO / n): sha(REPO / n) if (REPO / n).is_file() else None for n in
            ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json')}
        native_path = pin(args.native_receipt, _CAMPAIGN_INPUTS['native_receipt_sha256'])
        native = json.loads(native_path.read_text())
        profile_path = pin(args.profile_receipt, _CAMPAIGN_INPUTS['profile_receipt_sha256'])
        profile = json.loads(profile_path.read_text())
        expected_env = {'LANG': 'C', 'DOTNET_GCHeapHardLimit': '1000000', 'DOTNET_GCRegionRange': '2000000',
                        'DOTNET_GCRegionSize': '100000', 'DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE': 'false', 'DOTNET_EnableDiagnostics': '0'}
        if (not native.get('passed') or not native.get('static_elf_verified') or not native.get('final_identities_stable')
                or native['elf']['interpreter'] or native['elf']['needed'] or native['native_exit_code'] != 0):
            raise RuntimeError('Passing static Kestrel native producer required')
        if (not profile.get('passed') or not profile.get('final_identities_stable') or profile['native_exit_code'] != 0
                or profile['binary'] != native['binary'] or profile['native_environment'] != expected_env):
            raise RuntimeError('Passing exact native Kestrel profile required')
        for name, digest in native['sources'].items(): pin(REPO / name, digest)
        for name, digest in native['frozen_inputs'].items(): pin(native_path.parent / name, digest)
        for name, digest in native['package_manifests'].items(): pin(native_path.parent / name, digest)
        pin(native_path.parent / 'source/obj/project.assets.json', native['assets_sha256'])
        for row in native['tools'].values(): pin(row['path'], row['sha256'])
        for name, digest in native['artifacts'].items(): pin(native_path.parent / name, digest)
        for name, digest in profile['inputs'].items(): pin(name, digest)
        for name, digest in profile['artifacts'].items(): pin(profile_path.parent / name, digest)
        native_helper = pin(native_path.parent / 'runner.py', native['runner_sha256'])
        semantic_response = runpy.run_path(str(native_helper))['response_semantics']
        def canonical_response(data):
            value = semantic_response(data)
            value.pop('date')
            return value
        oracle = attempt / 'oracle'; oracle.mkdir()
        native_rows = {row['name']: row for row in profile['native_cases']}
        if list(native_rows) != ['health', 'large', 'fragmented', 'missing', 'stop'] or not all(row['passed'] for row in native_rows.values()):
            raise RuntimeError('Native Kestrel case inventory differs')
        if native_rows['fragmented']['write_end_offsets'] != [1, 17, 59, 571, 1595, 3591, 3592]:
            raise RuntimeError('Native fragmented request schedule differs')
        for name, row in native_rows.items():
            for suffix in ('request', 'response'):
                source = pin(profile_path.parent / (name + '.' + suffix), row[suffix + '_sha256'])
                target = oracle / source.name; shutil.copyfile(source, target); pin(target, sha(source))
        tree(oracle)
        elf = pin(native['binary']['path'], native['binary']['sha256'])
        copied_elf = attempt / 'KestrelService'; shutil.copyfile(elf, copied_elf); pin(copied_elf, sha(elf))
        receipt['native'] = dict(path=str(native_path), sha256=sha(native_path), binary=native['binary'],
                                profile=str(profile_path), profile_sha256=sha(profile_path), environment=expected_env)
        receipt['limits'] = dict(image_bytes=16*1024*1024, frame_bytes=24*1024*1024, memory_bytes=128*1024*1024,
            instruction_limit=100_000_000, wall_milliseconds=60000, scenario_seconds=240, outer_seconds=270,
            request_seconds=15, stop_grace_seconds=10, idle_health_before_milliseconds=54000)
        delivery_path = pin(args.delivery_receipt, args.delivery_sha256); delivery = json.loads(delivery_path.read_text())
        if delivery.get('passed') is not True or delivery.get('authored_sources_unchanged') is not True or 'stable_output' not in delivery:
            raise RuntimeError('Passing public stable delivery required, not a prototype receipt')
        final, raw = Path(delivery['stable_output']), Path(delivery['raw_snapshot'])
        if final.resolve() != (ROOT / 'generated/TranslatedBlink').resolve(): raise RuntimeError('Wrong public product path')
        tree(final, delivery['final_files']); tree(raw, delivery['raw_files'])
        for name, digest in delivery['authored_sources'].items(): pin(ROOT / name, digest)
        assembly_path = pin(delivery['assembly']['path'], delivery['assembly']['sha256']); assembly = json.loads(assembly_path.read_text())
        if not assembly['linked'] or assembly['failures'] or len(assembly['objects']) != 108: raise RuntimeError('Complete product closure required')
        for row in assembly['objects'].values(): pin(row['object_path'], row['object_sha256'])
        profile = Path(delivery['profile']); inputs_path = pin(profile / 'inputs.json', delivery['profile_inputs_sha256'])
        inputs = json.loads(inputs_path.read_text())
        for name, digest in inputs['staged_headers'].items(): pin(profile / name, digest)
        receipt['semantic_intrinsics'] = pin_semantic_delivery(delivery, assembly, pin)
        if '#define BLINK_MANAGED_GUEST_THREADS 1' not in (profile / 'config.h').read_text(): raise RuntimeError('Threaded product required')
        cli = REPO / 'DotCC/bin/Release/net10.0'
        if compiler_identity(cli) != delivery['compiler']: raise RuntimeError('Compiler differs')
        receipt['compiler'] = delivery['compiler']
        for name, digest in delivery['compiler'].items(): pin(cli / name, digest)
        for row in delivery['results'].values():
            if row['exit_code'] != 0: raise RuntimeError('Public delivery has failed command')
            pin(row['log'], row['log_sha256'])
        product_closure = source_closure(final / 'TranslatedBlink.csproj')
        external = {path: value for path, value in product_closure.items() if not path.is_relative_to(final)}
        for path, value in external.items():
            if delivery['authored_sources'].get(str(path.relative_to(ROOT))) != value: raise RuntimeError('Unpinned product reference')
            pin(path, value)
        receipt['delivery'] = dict(path=str(delivery_path), sha256=sha(delivery_path), assembly=delivery['assembly'])
        source_folders = [ROOT / 'src' / name for name in ('Managed.Emulation', 'Managed.Emulation.Worker', 'Managed.Emulation.ThreadedExecution')]
        source_folders.append(HERE)
        closures = {str(folder): tree(folder) for folder in source_folders}
        dotnet = pin(shutil.which('dotnet'))
        info, _ = run([dotnet, '--info'], 'dotnet-info'); version, _ = run([dotnet, '--version'], 'dotnet-version')
        receipt['dotnet_info'] = info.decode()
        sdk = dotnet.parent / 'sdk' / version.decode().strip()
        for name in ('dotnet.dll', 'MSBuild.dll', 'Roslyn/bincore/csc.dll'): pin(sdk / name)
        runtimes, _ = run([dotnet, '--list-runtimes'], 'dotnet-runtimes')
        for line in runtimes.decode().splitlines():
            fields = line.split()
            if len(fields) == 3 and fields[0] == 'Microsoft.NETCore.App':
                directory = Path(fields[2].strip('[]')) / fields[1]
                for name in ('libcoreclr.so', 'libhostpolicy.so', 'System.Private.CoreLib.dll'): pin(directory / name)
        for mode, library, inventory in (('raw', raw, delivery['raw_files']), ('optimized', final, delivery['final_files'])):
            private = attempt / mode; copied = private / 'generated/TranslatedBlink'
            shutil.copytree(library, copied, ignore=shutil.ignore_patterns('bin', 'obj'))
            for path in [copied, *copied.rglob('*')]: path.chmod(0o755 if path.is_dir() else 0o644)
            tree(copied, inventory)
            for folder in source_folders:
                target = private / folder.relative_to(ROOT)
                shutil.copytree(folder, target, ignore=shutil.ignore_patterns('bin', 'obj', '__pycache__'))
                tree(target, closures[str(folder)])
            if mode == 'optimized':
                for original, digest in external.items():
                    target = private / original.relative_to(ROOT); target.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copyfile(original, target); pin(target, digest)
            worker_project = private / 'src/Managed.Emulation.Worker/Managed.Emulation.Worker.csproj'
            test_project = private / 'tests/KestrelWorkerInstances/KestrelWorkerInstances.csproj'
            flags = ['-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false']
            run([dotnet, 'build', worker_project, *flags], mode + '-worker-build')
            run([dotnet, 'build', test_project, *flags], mode + '-controller-build')
            test_binary = test_project.parent / 'bin/Release/net10.0/KestrelWorkerInstances.dll'
            worker_binary = worker_project.parent / 'bin/Release/net10.0/Managed.Emulation.Worker.dll'
            test_run = private / 'controller-execution'; worker_run = private / 'worker-jit-execution'
            shutil.copytree(test_binary.parent, test_run); shutil.copytree(worker_binary.parent, worker_run)
            for kind in ('jit', 'aot'):
                if kind == 'jit': executable = worker_run / worker_binary.name; worker_directory = worker_run
                else:
                    worker_directory = private / 'worker-aot'
                    run([dotnet, 'publish', worker_project, *flags, '-r', 'linux-x64', '-p:PublishAot=true', '-o', worker_directory], mode + '-worker-aot-build', 1200)
                    executable = worker_directory / 'Managed.Emulation.Worker'
                label = mode + '-' + kind; report_dir = attempt / (label + '-results')
                binary_before = {str(folder): manifest(folder, True) for folder in (test_run, worker_directory)}
                binaries = [p for folder in (test_run, worker_directory) for p in folder.rglob('*') if p.is_file()]
                receipt.setdefault('execution_closures', {})[label] = dict(before=binary_before)
                try:
                    stdout, stderr = run([dotnet, test_run / test_binary.name, dotnet, executable, copied_elf, oracle, report_dir], label, 270, binaries)
                finally:
                    receipt.setdefault('artifacts', {})[label] = manifest(report_dir, True) if report_dir.exists() else {}
                    save()
                if stdout != EXPECTED or stderr: raise RuntimeError(label + ' controller output differs')
                binary_after = {str(folder): manifest(folder, True) for folder in (test_run, worker_directory)}
                if binary_after != binary_before: raise RuntimeError('Execution closure changed')
                receipt['execution_closures'][label]['after'] = binary_after
                result_path = pin(report_dir / 'result.json'); result = json.loads(result_path.read_text())
                if not result.get('passed') or result['image_sha256'] != sha(copied_elf) or len(result['workers']) != 4 or len(result['exchanges']) != 10:
                    raise RuntimeError(label + ' actual scenario coverage differs')
                final_observations = result['final_observations']
                if (len(final_observations) != 4 or
                        {row['pid'] for row in final_observations} != {row['pid'] for row in result['workers']} or
                        any(not row['completed_before_cleanup'] or row['result'] is None or
                            row['completion_error'] is not None or row['cleanup_error'] is not None
                            for row in final_observations)):
                    raise RuntimeError('A successful lifecycle must finish before diagnostic cleanup')
                if {x['name'] for x in result['workers']} != {'a', 'b', 'restart', 'deadline'} or len({x['pid'] for x in result['workers']}) != 4:
                    raise RuntimeError('Worker process identities differ')
                expected_names = {'a-health', 'b-health', 'a-large', 'a-fragmented', 'a-missing', 'a-stop', 'b-after-a-health', 'restart-health', 'restart-stop', 'deadline-health'}
                if {x['name'] for x in result['exchanges']} != expected_names: raise RuntimeError('HTTP coverage differs')
                fragment = next(x for x in result['exchanges'] if x['name'] == 'a-fragmented')
                if fragment['write_end_offsets'] != [1, 17, 59, 571, 1595, 3591, 3592] or fragment['request_bytes'] != 3592:
                    raise RuntimeError('Managed write schedule differs')
                if (result.get('image_bytes') != native['binary']['size'] or result.get('image_limit') != 16*1024*1024
                        or result.get('maximum_frame') != 24*1024*1024 or result.get('idle_wait_milliseconds', 0) < 1000):
                    raise RuntimeError('Kestrel admission or observed idle interval differs')
                for exchange in result['exchanges']:
                    kind = exchange['name'].split('-')[-1]
                    actual = (report_dir / (exchange['name'] + '.response')).read_bytes()
                    if canonical_response(actual) != canonical_response((oracle / (kind + '.response')).read_bytes()):
                        raise RuntimeError('Actual Kestrel response semantics differ from native')
                for worker in result['workers']:
                    detail = json.loads(worker['detail'])
                    expected_stop = {'a': 'None', 'b': 'Requested', 'restart': 'None', 'deadline': 'Deadline'}[worker['name']]
                    if (worker['worker_exit_code'] != 0 or worker['worker_diagnostics'] or detail['stopReason'] != expected_stop
                            or not all(detail[k] is True for k in ('joined', 'quiescent', 'ioDisposed', 'memoryReleased'))
                            or detail['notificationFailure'] is not False or detail['error'] is not None):
                        raise RuntimeError('Actual worker outcome or cleanup differs')
                for name, digest in receipt['artifacts'][label].items(): pin(report_dir / name, digest)
                receipt['modes'][label] = dict(report=str(result_path), sha256=sha(result_path), value=result); save()
        if set(receipt['modes']) != MODES: raise RuntimeError('Four worker modes missing')
        if source_closure(final / 'TranslatedBlink.csproj') != product_closure: raise RuntimeError('Product source closure changed')
        if compiler_identity(cli) != receipt['compiler']: raise RuntimeError('Compiler changed')
        check()
        for row in receipt['commands'].values():
            for name, digest in {**row['files'], **row['binary_after']}.items():
                if sha(name) != digest: raise RuntimeError('Closed artifact changed')
        for observed in receipt['execution_closures'].values():
            for folder, expected in observed['before'].items():
                if manifest(Path(folder), True) != expected: raise RuntimeError('Executed dependency closure changed')
        receipt.update(passed=True, final_identities_stable=True, worker_runs=16, native_http_comparisons=40)
    except BaseException as error: receipt['failure'] = repr(error)
    finally:
        receipt['completed'] = True; save()
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


def interrupted(number, frame):
    raise InterruptedError('Received signal ' + str(number))


if __name__ == '__main__':
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
