#!/usr/bin/env python3
"""Qualify the actual public machine consumer against a fresh instance-v1 delivery."""
import argparse
import glob
import hashlib
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
import xml.etree.ElementTree as ET

# Qualification inputs exclude incidental Python cache writes.
sys.dont_write_bytecode = True

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
sys.path.insert(0, str(ROOT / 'scripts'))
from semantic_delivery import pin_semantic_delivery

HELPER = ROOT / 'tests/WorkerInstances/run.py'
spec = importlib.util.spec_from_file_location('worker_evidence', HELPER)
helper = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helper)
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()
DEPLOYMENT = ROOT / 'src/Managed.Emulation/Managed.Emulation.Consumer.targets'
WORKER = ROOT / 'src/Managed.Emulation.Worker/Managed.Emulation.Worker.csproj'
CASES = {
    'argv-env-cwd-private-persistence-binary-eof', 'mounted-executable-without-import',
    'live-rw-ro-cow-export-reset', 'concurrent-independent-machines-same-machine-exclusion',
    'wait-cancel-stop-or-kill-acknowledged-write-restart', 'natural-deadline', 'instruction-bound',
    'concurrent-stop-run-and-machine-dispose', 'owned-console-stream-disposal',
    'normal-memory-descriptor-storage-limits'
}


def source_closure(project):
    """Review the one supported deployment import instead of evaluating arbitrary MSBuild."""
    files, visited = {}, set()

    def visit(path):
        path = Path(path).resolve()
        if path in visited:
            return
        visited.add(path)
        if not path.is_relative_to(ROOT):
            raise RuntimeError('Project/source outside the reviewed Blink tree: ' + str(path))
        files[path] = sha(path)
        xml = ET.parse(path).getroot()
        defaults = xml.find('.//EnableDefaultCompileItems')
        if path.suffix == '.csproj' and (defaults is None or defaults.text != 'false'):
            for name, digest in helper.manifest(path.parent).items():
                if name.endswith('.cs'):
                    files[path.parent / name] = digest
        for node in xml.findall('.//Import'):
            target = (path.parent / node.get('Project', '')).resolve()
            if target != DEPLOYMENT.resolve() or node.get('Condition'):
                raise RuntimeError('Unreviewed MSBuild import: ' + str(target))
            visit(target)
        for node in xml.findall('.//Compile'):
            name = node.get('Include', '')
            if not name or '$(' in name or ';' in name or node.get('Condition'):
                raise RuntimeError('Unreviewed Compile item')
            matches = [Path(p).resolve() for p in glob.glob(str(path.parent / name), recursive=True)]
            if not matches:
                raise RuntimeError('Empty source inclusion')
            for source in matches:
                if not source.is_relative_to(ROOT):
                    raise RuntimeError('External compiled source')
                if source.is_file():
                    files[source] = sha(source)
        for node in xml.findall('.//ProjectReference'):
            name = node.get('Include', '')
            if path == DEPLOYMENT.resolve() and name == '$(BlinkWorkerProject)':
                visit(WORKER)
            elif name and '$(' not in name and ';' not in name and not node.get('Condition'):
                visit(path.parent / name)
            else:
                raise RuntimeError('Unreviewed project reference')
    visit(project)
    return files


def verify_delivery(args, receipt, pin, tree):
    delivery_path = pin(args.delivery_receipt, args.delivery_sha256)
    delivery = json.loads(delivery_path.read_text())
    final = ROOT / 'generated/TranslatedBlink'
    if (delivery.get('passed') is not True or not delivery.get('authored_sources_unchanged')
            or delivery.get('selected_profile') != 'threaded'
            or delivery.get('product_surface', {}).get('instance_abi') != 'instance-v1'
            or Path(delivery['stable_output']).resolve() != final.resolve()):
        raise RuntimeError('A passing refreshed public instance-v1 delivery is required')
    tree(final, delivery['final_files'])
    tree(Path(delivery['raw_snapshot']), delivery['raw_files'])
    profile = Path(delivery['profile'])
    inputs_path = pin(profile / 'inputs.json', delivery['profile_inputs_sha256'])
    inputs = json.loads(inputs_path.read_text())
    marker = pin(profile / 'instance-abi.json', inputs['staged_headers']['instance-abi.json'])
    if json.loads(marker.read_text()) != {'abi': 'instance-v1'}:
        raise RuntimeError('Profile does not select the explicit instance ABI')
    assembly_path = pin(delivery['assembly']['path'], delivery['assembly']['sha256'])
    assembly = json.loads(assembly_path.read_text())
    if (not assembly['linked'] or assembly['failures'] or len(assembly['objects']) != 108
            or assembly['identity']['profile_inputs_sha256'] != sha(inputs_path)
            or assembly['identity']['compiler_sha256'] != delivery['compiler']
            or inputs['compiler'] != delivery['compiler']
            or '--instance-methods' not in assembly['identity']['link_options']):
        raise RuntimeError('Instance producer identity differs')
    for row in assembly['objects'].values():
        obj = pin(row['object_path'], row['object_sha256'])
        if '//!!dotcc-obj calling-convention:instance-v1' not in obj.read_text().splitlines()[:20]:
            raise RuntimeError('Producer object is not instance-v1: ' + str(obj))
        pin(row['receipt'])
    for name, digest in inputs['staged_headers'].items():
        pin(profile / name, digest)
    receipt['semantic_intrinsics'] = pin_semantic_delivery(delivery, assembly, pin)
    for name, digest in delivery['authored_sources'].items():
        pin(ROOT / name, digest)
    for path, digest in source_closure(final / 'TranslatedBlink.csproj').items():
        if not path.is_relative_to(final) and delivery['authored_sources'].get(str(path.relative_to(ROOT))) != digest:
            raise RuntimeError('Product references an unpinned authored source: ' + str(path))
    for folder, category in [('DotCC', 'compiler'), ('DotCC.PostProcess', 'postprocessor')]:
        tools = args.producer_tools.resolve() / category if args.producer_tools else REPO / folder / 'bin/Release/net10.0'
        for name, digest in delivery[category].items():
            pin(tools / name, digest)
    for row in delivery['results'].values():
        if row['exit_code'] != 0:
            raise RuntimeError('Delivery command failed')
        pin(row['log'], row['log_sha256'])
    return delivery_path, delivery, assembly


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--delivery-receipt', type=Path, required=True)
    parser.add_argument('--delivery-sha256', required=True)
    parser.add_argument('--producer-tools', type=Path, help='Optional immutable compiler/ and postprocessor/ folders; hashes must exactly match the delivery')
    args = parser.parse_args()
    base = ROOT / 'artifacts/machine-api'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    (attempt / 'tmp').mkdir()
    receipt = dict(kind='public-machine-api-instance-v1', passed=False, completed=False,
        inputs={}, source_trees={}, commands={}, execution_closures={}, modes={}, native_cases=[],
        scope='Actual ordinary consumer project and app-local worker deployment; Linux x64 JIT/AOT, both execution modes; no Windows or hardened sandbox claim')
    build_env = dict(os.environ, LC_ALL='C', TMPDIR=str(attempt / 'tmp'),
        MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0')
    print(attempt, flush=True)

    def save():
        pending = attempt / 'receipt.tmp'
        pending.write_text(json.dumps(receipt, indent=2) + '\n')
        pending.replace(attempt / 'receipt.json')

    def pin(path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError('Identity differs: ' + str(path))
        if receipt['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Previously pinned input changed: ' + str(path))
        return path

    def tree(path, expected=None):
        path = Path(path).resolve()
        actual = helper.manifest(path)
        if expected is not None and actual != expected:
            raise RuntimeError('Source tree differs: ' + str(path))
        receipt['source_trees'][str(path)] = actual
        for name, digest in actual.items():
            pin(path / name, digest)

    def check():
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest:
                raise RuntimeError('Frozen input changed: ' + path)
        for path, expected in receipt['source_trees'].items():
            if helper.manifest(Path(path)) != expected:
                raise RuntimeError('Source membership/content changed: ' + path)
        for path, expected in receipt.get('optional_inputs', {}).items():
            if (sha(path) if Path(path).is_file() else None) != expected:
                raise RuntimeError('Build configuration changed: ' + path)

    def run(label, command, timeout=900, folders=(), cwd=REPO, environment=None,
            stdin=None, executable=None, expected_code=0):
        check()
        before = {str(p): helper.manifest(p, True) for p in folders}
        row = dict(command=list(map(str, command)), cwd=str(cwd), timeout_seconds=timeout,
                   executable=str(executable) if executable else None, environment=environment)
        receipt['commands'][label] = row
        receipt['execution_closures'][label] = dict(before=before)
        stdout, stderr = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        input_file = attempt / (label + '.stdin')
        if stdin is not None:
            input_file.write_bytes(stdin)
            pin(input_file)
            row['stdin'] = str(input_file)
        process = None
        started = time.monotonic()
        save()
        try:
            with stdout.open('wb') as out, stderr.open('wb') as err, (input_file.open('rb') if stdin is not None else open(os.devnull, 'rb')) as inp:
                process = subprocess.Popen(row['command'], executable=executable, cwd=cwd,
                    env=build_env if environment is None else environment, stdin=inp,
                    stdout=out, stderr=err, start_new_session=True)
                row['pid'] = process.pid
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            if process is not None:
                row['cleanup'] = helper.cleanup(process)
            row['seconds'] = time.monotonic() - started
            row['files'] = {str(p): sha(p) for p in (stdout, stderr) if p.exists()}
            for path, digest in row['files'].items():
                pin(path, digest)
            after = {str(p): helper.manifest(p, True) for p in folders}
            receipt['execution_closures'][label]['after'] = after
            save()
        check()
        if before != after:
            raise RuntimeError('Execution closure changed: ' + label)
        if row['exit_code'] != expected_code or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError('Command failed or required external process cleanup: ' + label)
        return stdout.read_bytes(), stderr.read_bytes()

    def native(label, arguments, expected, error=b'', stdin=None, cwd=None, environment=None):
        output, errors = run('native-' + label, ['/bin/probe', *arguments], timeout=15,
            folders=(native_bin,), cwd=cwd or native_work,
            environment=environment if environment is not None else {}, stdin=stdin, executable=guest)
        if output != expected or errors != error:
            raise RuntimeError('Native fixture semantics differ: ' + label)
        receipt['native_cases'].append(dict(name=label, stdout_hex=output.hex(), stderr_hex=errors.hex(), passed=True))

    try:
        if platform.system() != 'Linux' or platform.machine() != 'x86_64':
            raise RuntimeError('This guest/native consumer runner qualifies Linux x64 only')
        delivery_path, delivery, assembly = verify_delivery(args, receipt, pin, tree)
        project = Path(__file__).with_name('Probe.csproj')
        solution = ROOT / 'ManagedConsumer.slnx'
        closure = source_closure(project)
        closure.update(source_closure(ROOT / 'samples/ManagedConsumer/ManagedConsumer.csproj'))
        for path in [Path(__file__), Path(__file__).with_name('README.md'), Path(__file__).with_name('guest.c'), HELPER,
                     ROOT / 'scripts/core_inputs.py', solution]:
            closure[path] = sha(path)
        for path, digest in closure.items():
            pin(path, digest)
            copied = attempt / 'inputs' / path.relative_to(REPO)
            copied.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(path, copied)
            pin(copied, digest)
        # Inventory membership as well as current compiled files; added sources
        # must invalidate a run, not silently enter the later AOT compilation.
        for directory in ['src/Managed.Emulation', 'src/Managed.Emulation.Worker', 'src/Managed.Emulation.ThreadedExecution',
                          'src/Managed.Emulation.Host', 'tests/MachineApi', 'ManagedConsumer']:
            tree(ROOT / directory)
        if args.producer_tools:
            # These runners build only the Blink consumer solution. Archived
            # producers are bound above to exact delivery hashes; unrelated live
            # producer sources do not enter consumer compilation.
            for category in ('compiler', 'postprocessor'): tree(args.producer_tools / category)
            receipt['producer_source_policy'] = 'Immutable delivery-matching binaries; live producer sources are not build inputs'
        else:
            for directory in ['DotCC.Lib', 'DotCC.Libc', 'DotCC', 'DotCC.PostProcess']:
                tree(REPO / directory)
        receipt['optional_inputs'] = {str(REPO / name): sha(REPO / name) if (REPO / name).is_file() else None
            for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json')}
        pin(sys.executable)
        cc, dotnet, readelf = [pin(shutil.which(name)) for name in ('cc', 'dotnet', 'readelf')]
        receipt['delivery'] = dict(path=str(delivery_path), sha256=args.delivery_sha256, assembly=delivery['assembly'])
        receipt['revision'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO, text=True).strip()
        run('dotnet-info', [dotnet, '--info'])
        run('cc-version', [cc, '--version'])
        native_bin = attempt / 'native-bin'; native_bin.mkdir()
        native_work = attempt / 'native-work'; native_work.mkdir()
        guest = native_bin / 'probe'
        run('native-build', [cc, '-O2', '-std=c17', '-Wall', '-Wextra', '-Werror', '-static', '-nostdlib',
            '-fno-pie', '-no-pie', '-fno-stack-protector', '-fno-builtin', '-fno-asynchronous-unwind-tables',
            '-Wl,-e,_start', Path(__file__).with_name('guest.c'), '-o', guest])
        pin(guest)
        elf, errors = run('native-elf', [readelf, '-h', '-lW', '-dW', guest], folders=(native_bin,))
        if errors or b'INTERP' in elf or b'(NEEDED)' in elf or b'EXEC (Executable file)' not in elf or b'Advanced Micro Devices X86-64' not in elf:
            raise RuntimeError('Native fixture is not the expected static Linux x86-64 ELF')
        receipt['guest'] = dict(path=str(guest), sha256=sha(guest), static_elf_verified=True)
        native('mkdir', ['mkdir', 'work'], b'mkdir-ok\n')
        native('inspect', ['inspect', 'argument with spaces'],
            ('cwd=' + str(native_work / 'work') + '\narg=/bin/probe\narg=inspect\narg=argument with spaces\nenv=FOO=run\nenv=EXTRA=seen\n').encode(),
            b'inspect-ok\n', cwd=native_work / 'work', environment={'FOO': 'run', 'EXTRA': 'seen'})
        native('store', ['store', 'work/value', 'first'], b'store-ok\n')
        native('append', ['append', 'work/value', '-second'], b'store-ok\n')
        native('rename', ['rename', 'work/value', 'work/moved'], b'rename-ok\n')
        native('load', ['load', 'work/moved'], b'first-second')
        binary = bytes([0, 255, 17, 10, 128])
        native('binary-echo', ['echo'], binary, b'echo-eof\n', stdin=binary)
        native('wait-eof', ['wait'], b'WAIT\nleft', b'echo-eof\n', stdin=b'left')
        # Native controls deliberately do not inherit private machine quotas.
        # Their normal operations succeed; the managed API separately requires
        # ENOMEM/EMFILE/ENOSPC under its explicit smaller capacities.
        native('memory-cap', ['memory-cap'], b'memory-available\n')
        native('descriptor-cap', ['descriptor-cap', 'quota-fds'], b'descriptors=16 first=3 last=18\n')
        native('storage-cap', ['storage-cap', 'quota-data'], b'stored=64\n')
        expected_storage = bytes(ord('A') + i % 26 for i in range(64))
        if (native_work / 'quota-data').read_bytes() != expected_storage:
            raise RuntimeError('Native storage witness bytes differ')
        pin(native_work / 'quota-data')
        receipt['resource_oracle_scope'] = 'Native succeeds at 64MiB mmap,16 opens,64 bytes; managed explicit32MiB/8 descriptors/16 private bytes rejects excess without corrupting retained prefix'
        run('solution-build', [dotnet, 'build', solution, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'], timeout=1200)
        run('consumer-build', [dotnet, 'build', project, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'], timeout=1200)
        jit = attempt / 'retained-jit'
        built = project.parent / 'bin/Release/net10.0'
        expected = helper.manifest(built, True)
        shutil.copytree(built, jit)
        if helper.manifest(jit, True) != expected or not (jit / 'blink-worker/Managed.Emulation.Worker.dll').is_file():
            raise RuntimeError('Ordinary build did not deploy the complete app-local JIT worker')
        receipt['retained_jit'] = dict(original=str(built), retained=str(jit), files=expected)

        def qualify(flavor, source):
            for mode in ('InProcess', 'SeparateProcess'):
                directory = attempt / flavor / mode; directory.mkdir(parents=True)
                app = directory / 'app'; shutil.copytree(source, app)
                observed = directory / 'evidence'
                command = [dotnet, app / 'Probe.dll'] if flavor == 'jit' else [app / 'Probe']
                stdout, stderr = run(flavor + '-' + mode, [*command, mode, guest, observed],
                    timeout=240, folders=(app, native_bin))
                evidence_path = pin(observed / 'observations.json')
                evidence = json.loads(evidence_path.read_text())
                if (evidence['Mode'] != mode or evidence['HostStatePreserved'] is not True
                        or set(evidence['Cases']) != CASES or len(evidence['Cases']) != len(CASES)
                        or len(evidence['Workers']) != len(set(evidence['Workers']))
                        or (mode == 'InProcess' and evidence['Workers'])
                        or (mode == 'SeparateProcess' and len(evidence['Workers']) < 4)
                        or stdout != f'machine-api {mode} {len(CASES)} PASS\n'.encode() or stderr):
                    raise RuntimeError('Public API evidence differs: ' + flavor + '/' + mode)
                receipt['modes'][flavor + '-' + mode] = dict(passed=True, evidence=evidence,
                    evidence_path=str(evidence_path), evidence_sha256=sha(evidence_path), files=helper.manifest(observed, True))
                for name, digest in receipt['modes'][flavor + '-' + mode]['files'].items():
                    pin(observed / name, digest)
                save()

        qualify('jit', jit)
        aot = attempt / 'retained-aot'
        run('consumer-publish', [dotnet, 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
            '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', aot], timeout=2400)
        if not (aot / 'Probe').is_file() or not (aot / 'blink-worker/Managed.Emulation.Worker').is_file():
            raise RuntimeError('Consumer publish did not deploy its matching NativeAOT worker')
        qualify('aot', aot)
        if helper.manifest(jit, True) != receipt['retained_jit']['files']:
            raise RuntimeError('Preserved JIT closure changed during AOT qualification')
        check()
        receipt.update(passed=True, final_identities_stable=True,
            native_scope='Eleven finite normal stdout/stderr/file/argv/env/cwd/resource witnesses; private quota and lifecycle expectations are separate API assertions',
            qualified_modes=4)
    except BaseException as error:
        receipt['failure'] = dict(type=type(error).__name__, message=str(error))
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
