#!/usr/bin/env python3
"""Qualify the ordinary machine sample with the pinned actual Kestrel guest."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['tests/MachineApi/run-kestrel.py']

import argparse
from datetime import datetime, timezone
import importlib.util
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

# Qualification inputs exclude incidental Python cache writes.
sys.dont_write_bytecode = True

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
SHARED = Path(__file__).with_name('run.py')
spec = importlib.util.spec_from_file_location('machine_api_evidence', SHARED)
shared = importlib.util.module_from_spec(spec)
spec.loader.exec_module(shared)
sha, helper = shared.sha, shared.helper
NATIVE_SHA = _CAMPAIGN_INPUTS['NATIVE_SHA']
PROFILE_SHA = _CAMPAIGN_INPUTS['PROFILE_SHA']
ENVIRONMENT = {'LANG': 'C', 'DOTNET_GCHeapHardLimit': '1000000', 'DOTNET_GCRegionRange': '2000000',
    'DOTNET_GCRegionSize': '100000', 'DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE': 'false', 'DOTNET_EnableDiagnostics': '0'}


def semantic(data):
    head, separator, body = data.partition(b'\r\n\r\n')
    if not separator:
        raise RuntimeError('Incomplete HTTP response')
    lines = head.decode('ascii').split('\r\n')
    version, status, reason = lines[0].split(' ', 2)
    headers = {}
    for line in lines[1:]:
        name, separator, value = line.partition(':')
        name = name.lower()
        if not separator or name in headers:
            raise RuntimeError('Malformed or duplicate HTTP header')
        headers[name] = value.strip()
    date = headers.pop('date')
    parsed = datetime.strptime(date, '%a, %d %b %Y %H:%M:%S GMT').replace(tzinfo=timezone.utc)
    if parsed.strftime('%a, %d %b %Y %H:%M:%S GMT') != date:
        raise RuntimeError('Date is not canonical RFC1123')
    return dict(version=version, status=int(status), reason=reason, headers=headers, body_hex=body.hex())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--delivery-receipt', type=Path, required=True)
    parser.add_argument('--delivery-sha256', required=True)
    parser.add_argument('--producer-tools', type=Path, help='Optional immutable compiler/ and postprocessor/ folders matching the delivery')
    parser.add_argument('--guest', type=Path, required=True)
    parser.add_argument('--native-receipt', type=Path, required=True)
    parser.add_argument('--profile-receipt', type=Path, required=True)
    parser.add_argument('--fixture-receipt', type=Path, required=True)
    parser.add_argument('--fixture-sha256', required=True)
    args = parser.parse_args()
    base = ROOT / 'artifacts/machine-api-kestrel'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base)); (attempt / 'tmp').mkdir()
    receipt = dict(kind='public-machine-api-kestrel-instance-v1', passed=False, completed=False,
        inputs={}, source_trees={}, commands={}, execution_closures={}, modes={},
        scope='Pinned actual Kestrel ELF, ordinary consumer solution and automatic app-local worker; Linux x64 JIT/AOT and both execution modes')
    env = dict(os.environ, LC_ALL='C', TMPDIR=str(attempt / 'tmp'),
        MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0')
    env.pop('BLINK_SAMPLE_EVIDENCE_DIRECTORY', None)
    print(attempt, flush=True)

    def save():
        temporary = attempt / 'receipt.tmp'
        temporary.write_text(json.dumps(receipt, indent=2) + '\n'); temporary.replace(attempt / 'receipt.json')

    def pin(path, expected=None):
        path = Path(path).resolve(); digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError('Identity differs: ' + str(path))
        if receipt['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Pinned input changed: ' + str(path))
        return path

    def tree(path, expected=None):
        path = Path(path).resolve(); actual = helper.manifest(path)
        if expected is not None and actual != expected:
            raise RuntimeError('Source tree differs: ' + str(path))
        receipt['source_trees'][str(path)] = actual
        for name, digest in actual.items(): pin(path / name, digest)

    def check():
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest: raise RuntimeError('Frozen input changed: ' + path)
        for path, files in receipt['source_trees'].items():
            if helper.manifest(Path(path)) != files: raise RuntimeError('Source membership/content changed: ' + path)
        for path, expected in receipt.get('optional_inputs', {}).items():
            if (sha(path) if Path(path).is_file() else None) != expected:
                raise RuntimeError('Build configuration changed: ' + path)

    def run(label, command, timeout=1200, folders=(), evidence=None, stdin=None):
        check()
        before = {str(p): helper.manifest(p, True) for p in folders}
        row = dict(command=list(map(str, command)), cwd=str(REPO), timeout_seconds=timeout)
        receipt['commands'][label] = row
        receipt['execution_closures'][label] = dict(before=before)
        command_env = dict(env)
        if evidence is not None:
            command_env['BLINK_SAMPLE_EVIDENCE_DIRECTORY'] = str(evidence)
            row['evidence_directory'] = str(evidence)
        stdout, stderr = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        input_file = attempt / (label + '.stdin')
        if stdin is not None:
            input_file.write_bytes(stdin); pin(input_file); row['stdin'] = str(input_file)
        process = None; began = time.monotonic(); save()
        try:
            with stdout.open('wb') as out, stderr.open('wb') as err, (input_file.open('rb') if stdin is not None else open(os.devnull, 'rb')) as inp:
                process = subprocess.Popen(row['command'], cwd=REPO, env=command_env,
                    stdin=inp, stdout=out, stderr=err, start_new_session=True)
                row['pid'] = process.pid; row['exit_code'] = process.wait(timeout=timeout)
        finally:
            if process is not None: row['cleanup'] = helper.cleanup(process)
            row['seconds'] = time.monotonic() - began
            row['files'] = {str(p): sha(p) for p in (stdout, stderr) if p.exists()}
            for path, digest in row['files'].items(): pin(path, digest)
            after = {str(p): helper.manifest(p, True) for p in folders}
            receipt['execution_closures'][label]['after'] = after; save()
        if row['exit_code'] != 0 or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError('Command failed or required cleanup: ' + label)
        if before != after: raise RuntimeError('Execution closure changed: ' + label)
        check()
        return stdout.read_bytes(), stderr.read_bytes()

    try:
        if platform.system() != 'Linux' or platform.machine() != 'x86_64':
            raise RuntimeError('This qualification is Linux x64 only')
        delivery_path, delivery, assembly = shared.verify_delivery(args, receipt, pin, tree)
        fixture_path = pin(args.fixture_receipt, args.fixture_sha256)
        fixture = json.loads(fixture_path.read_text())
        if (fixture.get('kind') != 'public-machine-api-instance-v1' or not fixture.get('passed')
                or not fixture.get('completed') or not fixture.get('final_identities_stable')
                or fixture.get('qualified_modes') != 4 or fixture['delivery']['sha256'] != args.delivery_sha256
                or set(fixture['modes']) != {'jit-InProcess', 'jit-SeparateProcess', 'aot-InProcess', 'aot-SeparateProcess'}
                or not all(row['passed'] for row in fixture['modes'].values())
                or any(set(row['evidence']['Cases']) != shared.CASES for row in fixture['modes'].values())
                or not fixture['guest']['static_elf_verified']):
            raise RuntimeError('Passing preceding MachineApi fixture receipt on this exact delivery is required')
        fixture_elf = pin(fixture['guest']['path'], fixture['guest']['sha256'])
        fixture_native = {row['name']: row for row in fixture['native_cases']}
        for name in ('binary-echo', 'store', 'load'):
            if not fixture_native[name]['passed']:
                raise RuntimeError('Missing passing native fixture witness: ' + name)
        # Tie the selected native oracle records back to their retained output.
        for name in ('binary-echo', 'store', 'load'):
            command = fixture['commands']['native-' + name]
            if command['exit_code'] != 0 or command['cleanup']['signals'] or not command['cleanup']['group_gone']:
                raise RuntimeError('Native fixture oracle did not terminate normally')
            for path, digest in command['files'].items(): pin(path, digest)
            if ((fixture_path.parent / ('native-' + name + '.stdout')).read_bytes().hex() != fixture_native[name]['stdout_hex']
                    or (fixture_path.parent / ('native-' + name + '.stderr')).read_bytes().hex() != fixture_native[name]['stderr_hex']):
                raise RuntimeError('Native fixture oracle summary differs from retained bytes')
        receipt['fixture'] = dict(path=str(fixture_path), sha256=args.fixture_sha256, guest=fixture['guest'],
            guest_rebuilt=False, native_cases={name: fixture_native[name] for name in ('binary-echo', 'store', 'load')})
        native_path = pin(args.native_receipt, NATIVE_SHA); native = json.loads(native_path.read_text())
        profile_path = pin(args.profile_receipt, PROFILE_SHA); profile = json.loads(profile_path.read_text())
        if (not native.get('passed') or not native.get('static_elf_verified') or not native.get('final_identities_stable')
                or native['elf']['interpreter'] or native['elf']['needed'] or native['native_exit_code'] != 0):
            raise RuntimeError('Passing pinned actual static Kestrel producer required')
        if (not profile.get('passed') or not profile.get('final_identities_stable') or profile['native_exit_code'] != 0
                or profile['binary'] != native['binary'] or profile['native_environment'] != ENVIRONMENT
                or not all(row['passed'] for row in profile['native_cases'])):
            raise RuntimeError('Pinned Kestrel profile differs')
        # This gate reuses the exact pinned native ELF, not a new guest build.
        # Verify its original archived build inputs rather than unrelated current
        # repository package versions that never enter this guest executable.
        for name, digest in native['sources'].items():
            archived = 'source/' + Path(name).name
            if native['frozen_inputs'].get(archived) != digest:
                raise RuntimeError('Native producer source lacks its exact archived input: ' + name)
            pin(native_path.parent / archived, digest)
        for category in ('frozen_inputs', 'artifacts', 'package_manifests'):
            for name, digest in native[category].items(): pin(native_path.parent / name, digest)
        pin(native_path.parent / 'source/obj/project.assets.json', native['assets_sha256'])
        for row in native['tools'].values(): pin(row['path'], row['sha256'])
        for name, digest in profile['inputs'].items(): pin(name, digest)
        for name, digest in profile['artifacts'].items(): pin(profile_path.parent / name, digest)
        guest = pin(args.guest, native['binary']['sha256'])
        native_rows = {row['name']: row for row in profile['native_cases']}
        if list(native_rows) != ['health', 'large', 'fragmented', 'missing', 'stop']:
            raise RuntimeError('Native profile inventory differs')
        oracle = attempt / 'oracle'; oracle.mkdir()
        for name in ('health', 'stop'):
            for suffix in ('request', 'response'):
                original = pin(profile_path.parent / (name + '.' + suffix), native_rows[name][suffix + '_sha256'])
                copied = oracle / original.name; shutil.copyfile(original, copied); pin(copied, sha(original))
        project = ROOT / 'samples/ManagedConsumer/ManagedConsumer.csproj'
        solution = ROOT / 'ManagedConsumer.slnx'
        sources = shared.source_closure(project)
        for path in (Path(__file__), SHARED, Path(__file__).with_name('README.md'), shared.HELPER,
                     ROOT / 'scripts/core_inputs.py', solution): sources[path] = sha(path)
        for path, digest in sources.items():
            if fixture['inputs'].get(str(path.resolve())) != digest:
                raise RuntimeError('Sample/runner source differs from preceding fixture qualification: ' + str(path))
            pin(path, digest)
            copied = attempt / 'inputs' / path.relative_to(REPO); copied.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(path, copied); pin(copied, digest)
        for directory in ('src/Managed.Emulation', 'src/Managed.Emulation.Worker', 'src/Managed.Emulation.ThreadedExecution',
                          'src/Managed.Emulation.Host', 'ManagedConsumer', 'tests/MachineApi'):
            tree(ROOT / directory)
        if args.producer_tools:
            for category in ('compiler', 'postprocessor'): tree(args.producer_tools / category)
            receipt['producer_source_policy'] = 'Immutable delivery-matching binaries; live producer sources are not build inputs'
        else:
            for directory in ('DotCC.Lib', 'DotCC.Libc', 'DotCC', 'DotCC.PostProcess'): tree(REPO / directory)
        receipt['optional_inputs'] = {str(REPO / name): sha(REPO / name) if (REPO / name).is_file() else None
            for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json')}
        pin(sys.executable); dotnet = pin(shutil.which('dotnet'))
        receipt['delivery'] = dict(path=str(delivery_path), sha256=args.delivery_sha256, assembly=delivery['assembly'])
        receipt['native'] = dict(path=str(native_path), sha256=NATIVE_SHA, profile=str(profile_path), profile_sha256=PROFILE_SHA,
            binary=native['binary'], environment=ENVIRONMENT, guest_rebuilt=False)
        receipt['revision'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO, text=True).strip()
        run('dotnet-info', [dotnet, '--info'])
        run('solution-build', [dotnet, 'build', solution, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'])
        built = project.parent / 'bin/Release/net10.0'; jit = attempt / 'retained-jit'
        expected = helper.manifest(built, True); shutil.copytree(built, jit)
        if helper.manifest(jit, True) != expected or not (jit / 'blink-worker/Managed.Emulation.Worker.dll').is_file():
            raise RuntimeError('Actual ordinary consumer did not deploy its complete JIT worker')
        receipt['retained_jit'] = dict(original=str(built), retained=str(jit), files=expected)

        def qualify(flavor, original):
            for mode in ('InProcess', 'SeparateProcess'):
                folder = attempt / flavor / mode; folder.mkdir(parents=True)
                app = folder / 'app'; shutil.copytree(original, app)
                image = folder / 'guest'; image.mkdir()
                elf = image / guest.name; shutil.copy2(guest, elf); pin(elf, sha(guest))
                evidence = folder / 'evidence'
                command = [dotnet, app / 'ManagedConsumer.dll'] if flavor == 'jit' else [app / 'ManagedConsumer']
                stdout, stderr = run(flavor + '-' + mode, [*command, elf, mode], timeout=240,
                    folders=(app, image), evidence=evidence)
                expected_stdout = f'Kestrel {mode}: two concurrent machines, mounted ELF, HTTP stop, restart and cooperative stop passed\n'.encode()
                if stdout != expected_stdout or stderr: raise RuntimeError('Actual sample console differs')
                results = json.loads(pin(evidence / 'machine-results.json').read_text())
                if len(results) != 3: raise RuntimeError('Actual sample execution count differs')
                for i, row in enumerate(results):
                    if (row['Reason'] != ('Stopped' if i == 2 else 'Exited') or row['ResourcesReleased'] is not True
                            or not 0 < row['Instructions'] <= 100_000_000
                            or row['Output'] != ('READY 8080\n' if i == 2 else 'READY 8080\nSTOPPED\n')):
                        raise RuntimeError('Actual sample owner outcome differs')
                pids = [row['ProcessId'] for row in results]
                if (mode == 'InProcess' and any(pid is not None for pid in pids)
                        or mode == 'SeparateProcess' and (any(not isinstance(pid, int) or pid <= 0 for pid in pids) or len(set(pids)) != 3)):
                    raise RuntimeError('Actual sample execution mode identities differ')
                comparisons = []
                for name, native_name in [('00-health', 'health'), ('01-health', 'health'),
                                          ('02-stop', 'stop'), ('03-stop', 'stop'), ('04-health', 'health')]:
                    request = pin(evidence / (name + '.request'), native_rows[native_name]['request_sha256'])
                    response = pin(evidence / (name + '.response'))
                    actual = semantic(response.read_bytes())
                    if actual != semantic((oracle / (native_name + '.response')).read_bytes()):
                        raise RuntimeError('Response differs from actual native oracle: ' + name)
                    comparisons.append(dict(name=name, native_case=native_name, request_sha256=sha(request),
                        response_sha256=sha(response), semantic=actual, passed=True))
                files = helper.manifest(evidence, True)
                for name, digest in files.items(): pin(evidence / name, digest)
                receipt['modes'][flavor + '-' + mode] = dict(passed=False, results=results, comparisons=comparisons, files=files)
                # Exercise the actual general console/folder entrypoints using
                # the preceding qualified fixture, never rebuilding that ELF.
                general = folder / 'general-work'; general.mkdir()
                probe = general / 'probe'; shutil.copy2(fixture_elf, probe); pin(probe, sha(fixture_elf))
                selector = '--run' if mode == 'InProcess' else '--run-process'
                general_command = [*command, selector, general, '/work/probe']
                echo = fixture_native['binary-echo']
                echo_bytes = bytes.fromhex(echo['stdout_hex'])
                actual, errors = run(flavor + '-' + mode + '-console-example', [*general_command, 'echo'],
                    timeout=60, folders=(app,), stdin=echo_bytes)
                if actual != echo_bytes or errors.hex() != echo['stderr_hex']:
                    raise RuntimeError('General sample binary console/EOF differs from native fixture')
                stored = bytes.fromhex(fixture_native['load']['stdout_hex'])
                actual, errors = run(flavor + '-' + mode + '-store-example',
                    [*general_command, 'store', '/work/value', stored.decode('ascii')], timeout=60, folders=(app,))
                if (actual.hex() != fixture_native['store']['stdout_hex'] or errors.hex() != fixture_native['store']['stderr_hex']
                        or (general / 'value').read_bytes() != stored):
                    raise RuntimeError('General sample mounted store differs from native fixture')
                pin(general / 'value')
                actual, errors = run(flavor + '-' + mode + '-load-example',
                    [*general_command, 'load', '/work/value'], timeout=60, folders=(app, general))
                if actual != stored or errors.hex() != fixture_native['load']['stderr_hex']:
                    raise RuntimeError('General sample mounted restart/load differs from native fixture')
                receipt['modes'][flavor + '-' + mode]['general_examples'] = dict(passed=True,
                    selector=selector, binary_console_eof=True, live_store_and_new_machine_load=True,
                    fixture_sha256=sha(probe), stored_hex=stored.hex())
                receipt['modes'][flavor + '-' + mode]['passed'] = True
                save()

        qualify('jit', jit)
        aot = attempt / 'retained-aot'
        run('consumer-publish', [dotnet, 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
            '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', aot], timeout=2400)
        if not (aot / 'ManagedConsumer').is_file() or not (aot / 'blink-worker/Managed.Emulation.Worker').is_file():
            raise RuntimeError('Ordinary AOT consumer did not package its matching AOT worker')
        qualify('aot', aot)
        if helper.manifest(jit, True) != receipt['retained_jit']['files']:
            raise RuntimeError('Preserved JIT closure changed during AOT qualification')
        check()
        receipt.update(passed=True, final_identities_stable=True, qualified_modes=4,
            actual_guest_executions=24, kestrel_guest_executions=12, general_sample_guest_executions=12,
            native_http_comparisons=20,
            comparison_scope='Exact request bytes and response semantics; only validated Date values/header order may differ')
    except BaseException as error:
        receipt['failure'] = dict(type=type(error).__name__, message=str(error))
    finally:
        receipt['completed'] = True; save()
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    signal.signal(signal.SIGTERM, shared.interrupted)
    raise SystemExit(main())
