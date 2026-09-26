#!/usr/bin/env python3
"""Build and run the actual final Kestrel sample with JIT and NativeAOT workers."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['tests/ManagedConsumerDelivery/run.py']

import argparse
from datetime import datetime, timezone
import importlib.util
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
sys.path.insert(0, str(ROOT / 'scripts'))
from semantic_delivery import pin_semantic_delivery

HELPER = ROOT / 'tests/WorkerInstances/run.py'
spec = importlib.util.spec_from_file_location('worker_evidence', HELPER)
helper = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helper)
sha = helper.sha
NATIVE_SHA = _CAMPAIGN_INPUTS['NATIVE_SHA']
PROFILE_SHA = _CAMPAIGN_INPUTS['PROFILE_SHA']
ENVIRONMENT = {'LANG': 'C', 'DOTNET_GCHeapHardLimit': '1000000',
    'DOTNET_GCRegionRange': '2000000', 'DOTNET_GCRegionSize': '100000',
    'DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE': 'false', 'DOTNET_EnableDiagnostics': '0'}
EXPECTED = b'Guest health: ok\nGuest stopped: exit 0\nGuest health: ok\nRestart and cooperative stop: passed\n'


def semantic(data):
    head, separator, body = data.partition(b'\r\n\r\n')
    if not separator:
        raise RuntimeError('Incomplete response')
    lines = head.decode('ascii').split('\r\n')
    version, status, reason = lines[0].split(' ', 2)
    headers = {}
    for line in lines[1:]:
        name, separator, value = line.partition(':')
        name = name.lower()
        if not separator or name in headers:
            raise RuntimeError('Invalid or duplicate HTTP header')
        headers[name] = value.strip()
    date = headers.pop('date')
    parsed = datetime.strptime(date, '%a, %d %b %Y %H:%M:%S GMT').replace(tzinfo=timezone.utc)
    if parsed.strftime('%a, %d %b %Y %H:%M:%S GMT') != date:
        raise RuntimeError('Date is not canonical RFC1123')
    return dict(version=version, status=int(status), reason=reason, headers=headers, body_hex=body.hex())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--guest', type=Path, required=True)
    parser.add_argument('--delivery-receipt', type=Path, required=True)
    parser.add_argument('--delivery-sha256', required=True)
    parser.add_argument('--native-receipt', type=Path, required=True)
    parser.add_argument('--profile-receipt', type=Path, required=True)
    args = parser.parse_args()
    base = ROOT / 'artifacts/managed-consumer-delivery'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    (attempt / 'tmp').mkdir()
    receipt = dict(passed=False, completed=False, commands={}, inputs={}, source_trees={},
        execution_closures={}, evidence={}, scope='Required Kestrel actual solution, JIT/AOT sample and worker pairs')
    env = dict(os.environ, LC_ALL='C', TMPDIR=str(attempt / 'tmp'),
        MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0')
    env.pop('BLINK_SAMPLE_EVIDENCE_DIRECTORY', None)
    print(attempt, flush=True)

    def save():
        (attempt / 'receipt.tmp').write_text(json.dumps(receipt, indent=2) + '\n')
        (attempt / 'receipt.tmp').replace(attempt / 'receipt.json')

    def pin(path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError('Identity differs: ' + str(path))
        previous = receipt['inputs'].get(str(path), digest)
        if digest != previous:
            raise RuntimeError('Previously frozen input differs: ' + str(path))
        receipt['inputs'][str(path)] = digest
        return path

    def tree(path, expected):
        path = Path(path).resolve()
        if helper.manifest(path) != expected:
            raise RuntimeError('Tree differs: ' + str(path))
        receipt['source_trees'][str(path)] = expected

    def check():
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest:
                raise RuntimeError('Input changed: ' + path)
        for path, expected in receipt['source_trees'].items():
            if helper.manifest(Path(path)) != expected:
                raise RuntimeError('Tree changed: ' + path)
        for path, digest in receipt.get('optional_inputs', {}).items():
            if (sha(path) if Path(path).is_file() else None) != digest:
                raise RuntimeError('Build configuration changed: ' + path)

    def run(label, command, timeout=900, folders=(), sample=False):
        check()
        before = {str(p): helper.manifest(p, True) for p in folders}
        row = dict(command=list(map(str, command)), cwd=str(REPO), timeout_seconds=timeout)
        receipt['commands'][label] = row
        receipt['execution_closures'][label] = dict(before=before)
        command_env = dict(env)
        evidence = attempt / (label + '-evidence')
        if sample:
            command_env['BLINK_SAMPLE_EVIDENCE_DIRECTORY'] = str(evidence)
            row['evidence_directory'] = str(evidence)
        save()
        stdout, stderr = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        process = None
        start = time.monotonic()
        try:
            with stdout.open('wb') as out, stderr.open('wb') as err:
                process = subprocess.Popen(row['command'], cwd=REPO, env=command_env, stdin=subprocess.DEVNULL,
                                           stdout=out, stderr=err, start_new_session=True)
                row['process_group'] = process.pid
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            if process is not None:
                row['cleanup'] = helper.cleanup(process)
            row['seconds'] = time.monotonic() - start
            row['files'] = {str(p): sha(p) for p in (stdout, stderr) if p.exists()}
            after = {str(p): helper.manifest(p, True) for p in folders}
            receipt['execution_closures'][label]['after'] = after
            if sample:
                receipt['evidence'][label] = dict(files=helper.manifest(evidence, True))
            save()
        if row['exit_code'] != 0 or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ' did not finish normally')
        if before != after:
            raise RuntimeError('Execution files changed')
        check()
        print(label + ': passed', flush=True)
        return stdout.read_bytes(), stderr.read_bytes()

    def verify_sample(label, profile, profile_path):
        directory = attempt / (label + '-evidence')
        comparisons = []
        for actual_name, native_name in [('health', 'health'), ('stop', 'stop'), ('restart-health', 'health')]:
            native = next(row for row in profile['native_cases'] if row['name'] == native_name)
            request = directory / (actual_name + '.request')
            if sha(request) != native['request_sha256']:
                raise RuntimeError('Sample request differs from native oracle: ' + actual_name)
            actual = semantic((directory / (actual_name + '.response')).read_bytes())
            expected = semantic((profile_path.parent / (native_name + '.response')).read_bytes())
            if actual != expected:
                raise RuntimeError('Sample response differs from native oracle: ' + actual_name)
            comparisons.append(dict(name=actual_name, native_case=native_name, semantic=actual, passed=True))
        results = []
        for prefix, stop_reason, output in [('normal', 'None', b'READY 8080\nSTOPPED\n'),
                                          ('cooperative', 'Requested', b'READY 8080\n')]:
            report = json.loads((directory / (prefix + '-cleanup.json')).read_text())
            if (report['stopReason'] != stop_reason or
                    not all(report[key] for key in ('joined', 'quiescent', 'ioDisposed', 'memoryReleased')) or
                    report['notificationFailure'] or report['error'] is not None or
                    (directory / (prefix + '.stdout')).read_bytes() != output or
                    (directory / (prefix + '.stderr')).read_bytes()):
                raise RuntimeError('Sample cleanup/output differs: ' + prefix)
            result = json.loads((directory / (prefix + '-result.json')).read_text())
            if (result['reason'] != ('exited' if prefix == 'normal' else 'stopped') or
                    (prefix == 'normal' and result['exitStatus'] != 0) or result['workerExitCode'] != 0 or
                    result['signal'] != 0 or result['halt'] != 0 or not 0 < result['instructions'] <= 100_000_000):
                raise RuntimeError('Sample result differs: ' + prefix)
            results.append(result)
        if results[0]['workerProcessId'] == results[1]['workerProcessId']:
            raise RuntimeError('Restart did not create a fresh worker')
        receipt['evidence'][label]['comparisons'] = comparisons
        receipt['evidence'][label]['results'] = results

    try:
        project = ROOT / 'samples/ManagedConsumer/ManagedConsumer.csproj'
        solution = ROOT / 'ManagedConsumer.slnx'
        delivery_path = pin(args.delivery_receipt, args.delivery_sha256)
        native_path = pin(args.native_receipt, NATIVE_SHA)
        profile_path = pin(args.profile_receipt, PROFILE_SHA)
        delivery, native, profile = [json.loads(p.read_text()) for p in (delivery_path, native_path, profile_path)]
        final = ROOT / 'generated/TranslatedBlink'
        if (delivery.get('passed') is not True or delivery.get('authored_sources_unchanged') is not True
                or delivery['selected_profile'] != 'threaded'
                or Path(delivery['stable_output']).resolve() != final.resolve()):
            raise RuntimeError('Passing public threaded delivery required')
        tree(final, delivery['final_files'])
        tree(Path(delivery['raw_snapshot']), delivery['raw_files'])
        if (not native.get('passed') or not native.get('static_elf_verified') or
                native['elf']['interpreter'] or native['elf']['needed']):
            raise RuntimeError('Passing static Kestrel native producer required')
        if (not profile.get('passed') or not profile.get('final_identities_stable') or
                profile['native_environment'] != ENVIRONMENT or profile['binary'] != native['binary'] or
                profile['native_exit_code'] != 0 or not all(row['passed'] for row in profile['native_cases'])):
            raise RuntimeError('Passing exact reviewed Kestrel native profile required')
        guest = pin(args.guest, native['binary']['sha256'])
        for name, digest in native['sources'].items():
            pin(REPO / name, digest)
        for category in ('frozen_inputs', 'artifacts', 'package_manifests'):
            for name, digest in native[category].items():
                pin(native_path.parent / name, digest)
        pin(native_path.parent / 'source/obj/project.assets.json', native['assets_sha256'])
        for row in native['tools'].values():
            pin(row['path'], row['sha256'])
        for name, digest in profile['inputs'].items():
            pin(name, digest)
        for name, digest in profile['artifacts'].items():
            pin(profile_path.parent / name, digest)
        for name, digest in delivery['authored_sources'].items():
            pin(ROOT / name, digest)
        assembly_path = pin(delivery['assembly']['path'], delivery['assembly']['sha256'])
        assembly = json.loads(assembly_path.read_text())
        profile_inputs = pin(Path(delivery['profile']) / 'inputs.json', delivery['profile_inputs_sha256'])
        translation_inputs = json.loads(profile_inputs.read_text())
        if (not assembly['linked'] or assembly['failures'] or len(assembly['objects']) != 108 or
                assembly['identity']['profile_inputs_sha256'] != sha(profile_inputs) or
                translation_inputs['compiler'] != delivery['compiler']):
            raise RuntimeError('Public translation producer identity differs')
        for row in assembly['objects'].values():
            pin(row['object_path'], row['object_sha256'])
        for name, digest in translation_inputs['staged_headers'].items():
            pin(Path(delivery['profile']) / name, digest)
        receipt['semantic_intrinsics'] = pin_semantic_delivery(delivery, assembly, pin)
        for name, digest in delivery['compiler'].items():
            pin(REPO / 'DotCC/bin/Release/net10.0' / name, digest)
        for row in delivery['results'].values():
            if row['exit_code'] != 0:
                raise RuntimeError('Public delivery command failed')
            pin(row['log'], row['log_sha256'])
        for path, digest in helper.source_closure(final / 'TranslatedBlink.csproj').items():
            if not path.is_relative_to(final) and delivery['authored_sources'].get(str(path.relative_to(ROOT))) != digest:
                raise RuntimeError('Unpinned original authored product source: ' + str(path))
        inputs = helper.source_closure(project)
        for path in (Path(__file__), Path(__file__).with_name('README.md'), project.with_name('README.md'),
                     HELPER, ROOT / 'scripts/core_inputs.py', solution, delivery_path, native_path, profile_path, guest):
            inputs[path] = sha(path)
        for path, digest in inputs.items():
            pin(path, digest)
            destination = attempt / 'inputs' / path.relative_to(REPO)
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(path, destination)
            pin(destination, digest)
        pin(sys.executable)
        dotnet = pin(shutil.which('dotnet'))
        receipt['optional_inputs'] = {str(REPO / name): sha(REPO / name) if (REPO / name).is_file() else None
            for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json')}
        receipt['revision'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO, text=True).strip()
        receipt['environment'] = {key: env[key] for key in ('LC_ALL', 'TMPDIR', 'MSBUILDDISABLENODEREUSE', 'DOTNET_CLI_USE_MSBUILD_SERVER')}
        receipt['guest'] = dict(path=str(guest), sha256=sha(guest), environment=ENVIRONMENT,
            limits=dict(memory_bytes=128*1024*1024, instructions=100_000_000, wall_milliseconds=60_000,
                        descriptors=128, output_bytes=16384, created_workers=16),
            memory_scope='Coupled guest AS/DATA allowance and backing cap, not claimed physical use')
        receipt['native'] = dict(path=str(native_path), sha256=NATIVE_SHA, profile=str(profile_path), profile_sha256=PROFILE_SHA)
        receipt['delivery'] = dict(path=str(delivery_path), sha256=args.delivery_sha256, assembly=delivery['assembly'])
        run('dotnet-info', [dotnet, '--info'])
        run('solution-build', [dotnet, 'build', solution, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'])
        sample_bin = ROOT / 'samples/ManagedConsumer/bin/Release/net10.0'
        worker_bin = ROOT / 'src/Managed.Emulation.Worker/bin/Release/net10.0'
        # Preserve the actual solution output bytes before AOT publish can update bin/.
        jit_sample, jit_worker = attempt / 'sample-jit', attempt / 'worker-jit'
        for original, retained in ((sample_bin, jit_sample), (worker_bin, jit_worker)):
            expected = helper.manifest(original, True)
            shutil.copytree(original, retained)
            if helper.manifest(retained, True) != expected:
                raise RuntimeError('Copied actual JIT output differs')
            receipt.setdefault('solution_outputs', {})[str(original)] = dict(retained=str(retained), files=expected)
        output, errors = run('jit-sample', [dotnet, jit_sample / 'ManagedConsumer.dll', guest,
            jit_worker / 'Managed.Emulation.Worker.dll'], timeout=240, folders=(jit_sample, jit_worker), sample=True)
        if output != EXPECTED or errors:
            raise RuntimeError('JIT sample output differs')
        verify_sample('jit-sample', profile, profile_path)
        worker_aot, sample_aot = attempt / 'worker-aot', attempt / 'sample-aot'
        for label, path, target in (
            ('worker-publish', ROOT / 'src/Managed.Emulation.Worker/Managed.Emulation.Worker.csproj', worker_aot),
            ('sample-publish', project, sample_aot)):
            run(label, [dotnet, 'publish', path, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', target], timeout=1800)
        output, errors = run('aot-sample', [sample_aot / 'ManagedConsumer', guest,
            worker_aot / 'Managed.Emulation.Worker'], timeout=240, folders=(sample_aot, worker_aot), sample=True)
        if output != EXPECTED or errors:
            raise RuntimeError('NativeAOT sample output differs')
        verify_sample('aot-sample', profile, profile_path)
        check()
        receipt.update(passed=True, final_identities_stable=True, actual_worker_runs=4,
            checks=['native HTTP semantics: only validated Date value/header order may vary', 'normal exit 0',
                    'fresh worker restart', 'cooperative requested stop', 'joined and quiescent',
                    'IO disposed and memory released', 'bounded instruction count',
                    'exact guest stdout and empty stderr', 'clean worker exit'])
    except BaseException as error:
        receipt['failure'] = repr(error)
    finally:
        receipt['completed'] = True
        save()
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


def interrupted(number, frame):
    raise InterruptedError('Received signal ' + str(number))


if __name__ == '__main__':
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
