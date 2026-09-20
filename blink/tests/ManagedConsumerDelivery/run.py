#!/usr/bin/env python3
"""Build and run the real final .NET service sample with JIT and NativeAOT workers."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
HELPER = ROOT / 'tests/WorkerInstances/run.py'
spec = importlib.util.spec_from_file_location('worker_evidence', HELPER)
helper = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helper)
sha = helper.sha
EXPECTED = b'Guest health: ok\nGuest stopped: exit 0\nGuest health: ok\nRestart and cooperative stop: passed\n'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--guest', type=Path, required=True)
    parser.add_argument('--delivery-receipt', type=Path, required=True)
    args = parser.parse_args()
    base = ROOT / 'artifacts/managed-consumer-delivery'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    receipt = dict(passed=False, completed=False, commands={}, inputs={}, execution_closures={})
    env = dict(os.environ, LC_ALL='C', MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0')
    print(attempt, flush=True)

    def save():
        (attempt / 'receipt.tmp').write_text(json.dumps(receipt, indent=2) + '\n')
        (attempt / 'receipt.tmp').replace(attempt / 'receipt.json')

    def check():
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest:
                raise RuntimeError('Input changed: ' + path)
        for path, digest in receipt.get('optional_inputs', {}).items():
            if (sha(path) if Path(path).is_file() else None) != digest:
                raise RuntimeError('Build configuration changed: ' + path)

    def run(label, command, timeout=900, folders=()):
        check()
        before = {str(p): helper.manifest(p, True) for p in folders}
        row = dict(command=list(map(str, command)), cwd=str(REPO), timeout_seconds=timeout)
        receipt['commands'][label] = row
        receipt['execution_closures'][label] = dict(before=before)
        save()
        stdout, stderr = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        process = None
        start = time.monotonic()
        try:
            with stdout.open('wb') as out, stderr.open('wb') as err:
                process = subprocess.Popen(row['command'], cwd=REPO, env=env, stdin=subprocess.DEVNULL,
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
            save()
        if row['exit_code'] != 0 or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ' did not finish normally')
        if before != after:
            raise RuntimeError('Execution files changed')
        check()
        print(label + ': passed', flush=True)
        return stdout.read_bytes(), stderr.read_bytes()

    try:
        project = ROOT / 'ManagedConsumer/ManagedConsumer.csproj'
        solution = ROOT / 'ManagedConsumer.slnx'
        delivery_path = args.delivery_receipt.resolve()
        delivery = json.loads(delivery_path.read_text())
        final = ROOT / 'generated/TranslatedBlink'
        if (delivery.get('passed') is not True or delivery.get('authored_sources_unchanged') is not True
                or Path(delivery['stable_output']).resolve() != final.resolve()
                or helper.manifest(final) != delivery['final_files']):
            raise RuntimeError('Actual final product differs from passing delivery')
        inputs = helper.source_closure(project)
        for path in (Path(__file__), HELPER, solution, delivery_path, args.guest.resolve()):
            inputs[path] = sha(path)
        receipt['inputs'] = {str(p): digest for p, digest in sorted(inputs.items())}
        receipt['optional_inputs'] = {str(REPO / name): sha(REPO / name) if (REPO / name).is_file() else None
            for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json')}
        for path in inputs:
            relative = path.relative_to(REPO)
            destination = attempt / 'inputs' / relative
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(path, destination)
        receipt['revision'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO, text=True).strip()
        receipt['environment'] = {key: env[key] for key in ('LC_ALL', 'MSBUILDDISABLENODEREUSE', 'DOTNET_CLI_USE_MSBUILD_SERVER')}
        receipt['guest'] = dict(path=str(args.guest.resolve()), sha256=sha(args.guest))
        receipt['delivery'] = dict(path=str(delivery_path), sha256=sha(delivery_path))
        run('dotnet-info', ['dotnet', '--info'])
        run('solution-build', ['dotnet', 'build', solution, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'])
        sample_bin = ROOT / 'ManagedConsumer/bin/Release/net10.0'
        worker_bin = ROOT / 'src/Managed.Emulation.Worker/bin/Release/net10.0'
        output, errors = run('jit-sample', ['dotnet', sample_bin / 'ManagedConsumer.dll', args.guest.resolve(),
            worker_bin / 'Managed.Emulation.Worker.dll'], timeout=180, folders=(sample_bin, worker_bin))
        if output != EXPECTED or errors:
            raise RuntimeError('JIT sample output differs')
        worker_aot, sample_aot = attempt / 'worker-aot', attempt / 'sample-aot'
        for label, path, target in (
            ('worker-publish', ROOT / 'src/Managed.Emulation.Worker/Managed.Emulation.Worker.csproj', worker_aot),
            ('sample-publish', project, sample_aot)):
            run(label, ['dotnet', 'publish', path, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', target], timeout=1800)
        output, errors = run('aot-sample', [sample_aot / 'ManagedConsumer', args.guest.resolve(),
            worker_aot / 'Managed.Emulation.Worker'], timeout=180, folders=(sample_aot, worker_aot))
        if output != EXPECTED or errors:
            raise RuntimeError('NativeAOT sample output differs')
        check()
        receipt.update(passed=True, final_identities_stable=True, actual_worker_runs=4,
            checks=['exact HTTP health and stop bytes', 'normal exit 0', 'fresh worker restart',
                    'cooperative requested stop', 'joined and quiescent', 'IO disposed and memory released',
                    'bounded instruction count', 'exact guest stdout and empty stderr', 'clean worker exit'])
    except BaseException as error:
        receipt['failure'] = repr(error)
    finally:
        receipt['completed'] = True
        save()
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
