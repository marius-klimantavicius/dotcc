#!/usr/bin/env python3
"""Run the unchanged combined bitset ELF through private public API consumers.

--prepare-only needs no delivery and qualifies only the Linux fixture/preparation.
Otherwise --clean-delivery selects a strict completed producer through the normal
load_clean_delivery gate. Raw/processed and both public modes run by default;
--aot adds matching NativeAOT host/worker deployments. No shared build is run.
Every attempt, including failures, is retained under this test's .artifacts/.
"""
import argparse
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

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
FORMS = ('raw', 'processed')
MODES = ('InProcess', 'SeparateProcess')
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()


def module(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    value = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(value)
    return value


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    selection = parser.add_mutually_exclusive_group(required=True)
    selection.add_argument('--prepare-only', action='store_true')
    selection.add_argument('--clean-delivery', type=Path, help='Producer pointer accepted by load_clean_delivery')
    parser.add_argument('--form', action='append', choices=FORMS)
    parser.add_argument('--mode', action='append', choices=MODES)
    parser.add_argument('--aot', action='store_true')
    parser.add_argument('--compile-only', action='store_true', help='Compile private hosts/workers without guest execution')
    parser.add_argument('--signal-stack-receipt', type=Path,
        help='Optional passing ordinary-guarded-guest-signal-stack native receipt; consumes its exact ELF')
    parser.add_argument('--process-maps-receipt', type=Path,
        help='Optional passing ordinary-guest-process-maps native receipt and its pinned libc/loader')
    parser.add_argument('--getcpu-receipt', type=Path,
        help='Optional passing getcpu native receipt and its complete pinned fixture closure')
    parser.add_argument('--segment-read-receipt', type=Path,
        help='Optional passing Linux user segment-read native receipt')
    parser.add_argument('--runs', type=int, default=3, help='Runs per public mode/host/form, 1..10 (default 3)')
    args = parser.parse_args()
    if not 1 <= args.runs <= 10:
        parser.error('--runs must be in 1..10')
    if args.prepare_only and (args.aot or args.compile_only):
        parser.error('--aot/--compile-only require --clean-delivery')
    forms = list(dict.fromkeys(args.form or FORMS))
    modes = list(dict.fromkeys(args.mode or MODES))
    runtimes = ['jit', 'nativeaot'] if args.aot else ['jit']
    base = HERE / '.artifacts'
    base.mkdir(exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='managed-', dir=base))
    (attempt / 'tmp').mkdir()
    receipt = dict(passed=False, completed=False, invocation=[sys.executable, *sys.argv],
        invocation_cwd=str(Path.cwd()), inputs={}, captured_inputs={}, frozen_trees={},
        commands=[], executions=[], deployments={}, managed_execution=False,
        fresh_qualification=False, complete_matrix=False, prepare_only=args.prepare_only,
        compile_only=args.compile_only, requested_matrix=dict(forms=forms, modes=modes,
        host_runtimes=runtimes, runs=args.runs),
        scope='Original combined ordinary fixture; public translated BlinkMachine consumer; bounded Linux x86-64 evidence',
        native_blink_oracle_resolved=False,
        prior_coordinator_termination='User reports automated content filter citing possible cyber risk; preserved, no safeguard workaround attempted')
    env = dict(os.environ, LC_ALL='C', TMPDIR=str(attempt / 'tmp'),
        MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0',
        DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_PROCESSOR_COUNT='2')
    for key in list(env):
        if key.startswith(('COMPlus_', 'CORECLR_', 'DOTNET_GC', 'DOTNET_Enable', 'DOTNET_Jit', 'LD_')):
            env.pop(key)
    for key in ('BlinkProject', 'MSBuildSDKsPath', 'MSBUILD_EXE_PATH', 'MSBuildExtensionsPath',
                'CFLAGS', 'CPPFLAGS', 'LDFLAGS', 'MAKEFLAGS'):
        env.pop(key, None)
    print('attempt=' + str(attempt), flush=True)

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
            raise RuntimeError('Pinned input changed: ' + str(path))
        return path

    def capture(source, target, expected=None):
        source, target = Path(source).resolve(), Path(target)
        digest = sha(source)
        if expected is not None and digest != expected:
            raise RuntimeError('Capture identity differs: ' + str(source))
        previous = receipt['captured_inputs'].get(str(source))
        if previous and previous['sha256'] != digest:
            raise RuntimeError('Source changed between captures: ' + str(source))
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
        if sha(source) != digest:
            raise RuntimeError('Source changed during capture: ' + str(source))
        pin(target, digest)
        receipt['captured_inputs'][str(source)] = dict(sha256=digest, retained=str(target))
        return target

    def manifest(folder):
        return {p.relative_to(folder).as_posix(): sha(p) for p in sorted(folder.rglob('*')) if p.is_file()}

    def freeze(folder):
        value = manifest(folder)
        if not value:
            raise RuntimeError('Empty execution closure')
        receipt['frozen_trees'][str(folder)] = value
        return value

    def check():
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest:
                raise RuntimeError('Pinned input changed: ' + path)
        for path, files in receipt['frozen_trees'].items():
            if manifest(Path(path)) != files:
                raise RuntimeError('Execution closure changed: ' + path)

    def cleanup(process):
        # Same private process-group containment used by WorkerInstances.
        def exists():
            process.poll()
            try:
                os.killpg(process.pid, 0)
                return True
            except ProcessLookupError:
                return False
        sent = []
        for sig, seconds in ((signal.SIGTERM, 2), (signal.SIGKILL, 3)):
            if not exists():
                break
            try:
                os.killpg(process.pid, sig)
                sent.append(sig.name)
            except ProcessLookupError:
                break
            end = time.monotonic() + seconds
            while exists() and time.monotonic() < end:
                time.sleep(.025)
        process.poll()
        return dict(signals=sent, group_gone=not exists(), exit_code=process.returncode)

    def run(label, command, timeout=60, cwd=attempt, require_success=True):
        check()
        row = dict(label=label, command=list(map(str, command)), cwd=str(cwd),
            timeout_seconds=timeout, exit_code=None)
        receipt['commands'].append(row)
        stdout, stderr = [attempt / (label + suffix) for suffix in ('.stdout', '.stderr')]
        row.update(stdout=str(stdout), stderr=str(stderr))
        process = None
        started = time.monotonic()
        save()
        try:
            with stdout.open('wb') as out, stderr.open('wb') as err:
                process = subprocess.Popen(row['command'], cwd=cwd, env=env,
                    stdin=subprocess.DEVNULL, stdout=out, stderr=err, start_new_session=True)
                row['pid'] = process.pid
                save()
                row['exit_code'] = process.wait(timeout=timeout)
        except BaseException as error:
            row['error'] = repr(error)
            if not isinstance(error, subprocess.TimeoutExpired):
                raise
        finally:
            if process is not None:
                row['cleanup'] = cleanup(process)
            row['seconds'] = time.monotonic() - started
            row['logs'] = {str(p): sha(p) for p in (stdout, stderr) if p.exists()}
            save()
        normal = row['exit_code'] == 0 and not row['cleanup']['signals'] and row['cleanup']['group_gone']
        row['normal'] = normal
        save()
        if not normal and require_success:
            raise RuntimeError(label + ' failed: ' + str(stderr))
        check()
        print(label + ': ' + ('passed' if normal else 'failed'), flush=True)
        return row, stdout.read_bytes(), stderr.read_bytes()

    def deployment(directory, runtime, form):
        native = runtime == 'nativeaot'
        app = directory / ('GuestFutexBitsetConsumer' if native else 'GuestFutexBitsetConsumer.dll')
        worker = directory / 'blink-worker' / ('Managed.Emulation.Worker' if native else 'Managed.Emulation.Worker.dll')
        for path in (app, worker):
            pin(path)
            if native and (path.read_bytes()[:4] != b'\x7fELF' or not os.access(path, os.X_OK)):
                raise RuntimeError('Expected executable NativeAOT host/worker ELF: ' + str(path))
        if native and (worker.parent / 'Managed.Emulation.Worker.dll').exists():
            raise RuntimeError('NativeAOT worker closure contains an ambiguous portable worker')
        receipt['deployments'][form + '-' + runtime] = dict(app=str(app), worker=str(worker),
            app_sha256=sha(app), worker_sha256=sha(worker), files=freeze(directory))
        return [str(app)] if native else [str(dotnet), str(app)]

    try:
        if (platform.system(), platform.machine()) != ('Linux', 'x86_64'):
            raise RuntimeError('Linux x86-64 required')
        receipt['baseline'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO, text=True).strip()
        harness = attempt / 'harness-inputs'
        for name in ('run-managed.py', 'Consumer.cs', 'Consumer.csproj', 'fixture.S'):
            capture(HERE / name, harness / name)
        linker = capture(HERE.parent / 'GuestThreads/fixture.ld', harness / 'fixture.ld')
        prior = capture(ROOT / 'artifacts/coreclr-coordination/bitset-r6-handoff-receipt.json',
            harness / 'bitset-r6-handoff-receipt.json')
        prior_value = json.loads(prior.read_text())
        if (prior_value.get('native_oracle_resolved') is not False or
                prior_value['source_sha256'][str(HERE / 'fixture.S')] != sha(harness / 'fixture.S')):
            raise RuntimeError('Expected unchanged r6 combined fixture/native limitation record')
        receipt['native_limitation'] = dict(handoff=str(prior), sha256=sha(prior),
            diagnosis=prior_value['diagnosis'], evidence=prior_value['evidence'], resolved=False)
        for tool in ('cc', 'as', 'ld', 'readelf'):
            pin(shutil.which(tool))
        cc, ld, readelf = [shutil.which(tool) for tool in ('cc', 'ld', 'readelf')]
        run('assemble', [cc, '-c', '-nostdlib', harness / 'fixture.S', '-o', attempt / 'fixture.o'])
        guest = attempt / 'futex-bitset.elf'
        run('link', [ld, '-static', '-z', 'noexecstack', '-z', 'max-page-size=4096',
            '-T', linker, attempt / 'fixture.o', '-o', guest])
        pin(guest)
        _, out, err = run('elf-identity', [readelf, '-h', '-lW', '-dW', guest])
        if (err or b'INTERP' in out or b'(NEEDED)' in out or b'EXEC (Executable file)' not in out
                or b'Advanced Micro Devices X86-64' not in out):
            raise RuntimeError('Expected ordinary static x86-64 Linux ELF')
        receipt['guest'] = dict(path=str(guest), sha256=sha(guest), fixture_sha256=sha(harness / 'fixture.S'),
            static_elf_verified=True, combined_fixture=True)
        scenarios = [('bitset', guest)]
        for index in range(args.runs):
            _, out, err = run('linux-' + str(index), [guest], timeout=20)
            if out or err:
                raise RuntimeError('Linux fixture stdout/stderr differs')
        receipt['linux_passed'] = True
        if args.signal_stack_receipt:
            # This owner consumes the native owner's emitted ordinary image;
            # it never compiles or edits the separate signal-stack fixture.
            native_path = capture(args.signal_stack_receipt, harness / 'signal-stack-native-receipt.json')
            native = json.loads(native_path.read_text())
            expected = b'guest-signal-stack: guard=4096 registered=16384 onstack=2 returned=2 disabled=1\n'
            if (native.get('kind') != 'ordinary-guarded-guest-signal-stack' or
                    native.get('passed') is not True or native.get('completed') is not True or
                    native.get('expected_stdout_hex') != expected.hex() or
                    native.get('final_identities_stable') is not True):
                raise RuntimeError('Passing complete signal-stack native producer required')
            linux = [row for row in native['executions'] if row['mode'] == 'linux']
            if (len(linux) != 1 or linux[0].get('passed') is not True or
                    linux[0]['stdout_hex'] != expected.hex() or linux[0]['stderr_hex']):
                raise RuntimeError('Exact signal-stack Linux output required')
            oracle = [row for row in native['commands'] if row['label'] == 'linux']
            if (len(oracle) != 1 or oracle[0].get('exit_code') != 0 or
                    oracle[0]['cleanup']['signals'] or not oracle[0]['cleanup']['group_gone'] or
                    oracle[0]['command'][-1] != native['elf']['path']):
                raise RuntimeError('Signal-stack Linux command/cleanup differs')
            for name, digest in oracle[0]['logs'].items():
                capture(name, harness / 'signal-stack-native' / Path(name).name, digest)
            for name in ('fixture.S', 'fixture.ld'):
                original = ROOT / 'tests/GuestSignalStack' / name
                capture(original, harness / 'signal-stack-native' / name, native['inputs'][str(original)])
            pin(native['trace']['path'], native['trace']['sha256'])
            signal_image = capture(native['elf']['path'], attempt / 'guest-signal-stack.elf', native['elf']['sha256'])
            _, out, err = run('signal-stack-elf-identity', [readelf, '-h', '-lW', '-dW', signal_image])
            if (err or b'INTERP' in out or b'(NEEDED)' in out or b'EXEC (Executable file)' not in out
                    or b'Advanced Micro Devices X86-64' not in out):
                raise RuntimeError('Expected static signal-stack x86-64 Linux ELF')
            receipt['signal_stack_native'] = dict(path=str(args.signal_stack_receipt.resolve()), sha256=sha(native_path),
                retained=str(native_path), guest=str(signal_image), guest_sha256=sha(signal_image), expected_stdout_hex=expected.hex())
            scenarios.append(('signal-stack', signal_image))
        scenario_dependencies = {}
        if args.process_maps_receipt:
            native_path = capture(args.process_maps_receipt, harness / 'process-maps-native-receipt.json')
            native = json.loads(native_path.read_text())
            expected = b'guest-process-maps: main/worker stacks, rwx/none, private/shared, unmap: PASS\n'
            if (native.get('kind') != 'ordinary-guest-process-maps' or not native.get('passed') or
                    not native.get('completed') or not native.get('fixture_ready') or
                    native.get('expected_stdout_hex') != expected.hex()):
                raise RuntimeError('Passing process-map native fixture required')
            linux = [row for row in native['executions'] if row['mode'] == 'linux-pinned-libc']
            if (len(linux) != 1 or not linux[0]['passed'] or linux[0]['stdout_hex'] != expected.hex() or linux[0]['stderr_hex']):
                raise RuntimeError('Process-map Linux output differs')
            for row in native['commands']:
                if row.get('exit_code') != 0 or row.get('cleanup', {}).get('signals') or not row.get('cleanup', {}).get('group_gone'):
                    raise RuntimeError('Process-map native command did not cleanly pass')
                for name, digest in row.get('logs', {}).items():
                    capture(name, harness / 'process-maps-native' / Path(name).name, digest)
            for name in ('fixture.c', 'memory-native.c', 'Program.cs', 'run.py'):
                source = ROOT / 'tests/GuestProcessMaps' / name
                capture(source, harness / 'process-maps-native' / name, native['inputs'][str(source)])
            pin(native['trace']['path'], native['trace']['sha256'])
            image = capture(native['elf']['path'], attempt / 'guest-process-maps.elf', native['elf']['sha256'])
            _, out, err = run('process-maps-elf-identity', [readelf, '-h', '-lW', '-dW', image])
            text = out.decode()
            import re
            interpreters = re.findall(r'Requesting program interpreter: ([^\]]+)', text)
            needed = re.findall(r'\(NEEDED\).*\[([^\]]+)\]', text)
            if err or len(interpreters) != 1 or needed != ['libc.so.6'] or interpreters[0] != '/lib64/ld-linux-x86-64.so.2':
                raise RuntimeError('Expected process-map ELF with only pinned libc and loader')
            dependencies = attempt / 'process-maps-dependencies'
            files = []
            for guest_path, original, digest, executable in (
                    ('/lib/x86_64-linux-gnu/libc.so.6', native['pinned_libc']['path'], native['pinned_libc']['sha256'], False),
                    (interpreters[0], native['pinned_libc']['loader'], native['pinned_libc']['loader_sha256'], True)):
                retained = capture(original, dependencies / guest_path.lstrip('/'), digest)
                files.append(dict(GuestPath=guest_path, File=retained.relative_to(dependencies).as_posix(), Sha256=digest, Executable=executable))
            process_dependencies = attempt / 'process-maps-dependencies.json'
            process_dependencies.write_text(json.dumps(dict(Images=str(dependencies), Files=files), indent=2) + '\n')
            pin(process_dependencies); freeze(dependencies)
            scenario_dependencies['process-maps'] = process_dependencies
            receipt['process_maps_native'] = dict(path=str(args.process_maps_receipt.resolve()), sha256=sha(native_path),
                guest=str(image), guest_sha256=sha(image), dependencies=str(process_dependencies), expected_stdout_hex=expected.hex())
            scenarios.append(('process-maps', image))
        if args.getcpu_receipt:
            native_path = capture(args.getcpu_receipt, harness / 'getcpu-native-receipt.json')
            native = json.loads(native_path.read_text())
            expected = 'guest-getcpu: main/worker affinity, libc, nullable outputs, width, cache: PASS\n'
            fixture = native.get('fixture', {})
            if (not native.get('passed') or not native.get('completed') or not fixture.get('native_passed') or
                    not fixture.get('fixture_ready') or fixture.get('expected_stdout') != expected):
                raise RuntimeError('Passing getcpu native fixture required')
            linux = [row for row in native['commands'] if row['label'] == 'linux']
            if (len(linux) != 1 or linux[0].get('exit_code') != 0 or linux[0]['cleanup']['signals'] or
                    not linux[0]['cleanup']['group_gone']):
                raise RuntimeError('getcpu Linux command/cleanup differs')
            for name, digest in linux[0]['logs'].items():
                capture(name, harness / 'getcpu-native' / Path(name).name, digest)
            if Path(linux[0]['stdout']).read_bytes() != expected.encode() or Path(linux[0]['stderr']).read_bytes():
                raise RuntimeError('getcpu Linux output differs')
            source = ROOT / 'tests/GuestCoreClrExecution/getcpu.c'
            capture(source, harness / 'getcpu-native/getcpu.c', native['captured_inputs'][str(source)]['sha256'])
            closure_path = capture(fixture['closure'], harness / 'getcpu-native-closure.json')
            closure = json.loads(closure_path.read_text())
            image = capture(fixture['path'], attempt / 'guest-getcpu.elf', fixture['sha256'])
            dependencies = attempt / 'getcpu-dependencies'
            files = []
            for row in closure['Files']:
                if row['GuestPath'] == '/apps/guest-getcpu.elf':
                    if row['Sha256'] != sha(image): raise RuntimeError('getcpu closure executable differs')
                    continue
                if row['GuestPath'] not in ('/lib64/ld-linux-x86-64.so.2', '/lib/x86_64-linux-gnu/libc.so.6'):
                    raise RuntimeError('Unexpected getcpu dependency')
                capture(Path(closure['Images']) / row['File'], dependencies / row['File'], row['Sha256'])
                files.append(row)
            if len(files) != 2: raise RuntimeError('Incomplete getcpu dependency closure')
            dependency_path = attempt / 'getcpu-dependencies.json'
            dependency_path.write_text(json.dumps(dict(Images=str(dependencies), Files=files), indent=2) + '\n')
            pin(dependency_path); freeze(dependencies)
            scenario_dependencies['getcpu'] = dependency_path
            receipt['getcpu_native'] = dict(path=str(args.getcpu_receipt.resolve()), sha256=sha(native_path),
                guest=str(image), guest_sha256=sha(image), dependencies=str(dependency_path), expected_stdout=expected)
            scenarios.append(('getcpu', image))
        if args.segment_read_receipt:
            native_path = capture(args.segment_read_receipt, harness / 'segment-read-native-receipt.json')
            native = json.loads(native_path.read_text())
            expected = b'guest-segment-read: selectors, registers, word stores, cross-page: PASS\n'
            if (native.get('kind') != 'ordinary-guest-segment-read' or not native.get('passed') or
                    not native.get('completed') or native.get('expected_stdout_hex') != expected.hex()):
                raise RuntimeError('Passing Linux user-segment fixture required')
            linux = [row for row in native['commands'] if row['label'] == 'linux']
            if (len(linux) != 1 or linux[0].get('exit_code') != 0 or linux[0]['cleanup']['signals'] or
                    not linux[0]['cleanup']['group_gone']):
                raise RuntimeError('Segment-read Linux command/cleanup differs')
            for name, digest in linux[0]['logs'].items():
                capture(name, harness / 'segment-read-native' / Path(name).name, digest)
            if Path(linux[0]['stdout']).read_bytes() != expected or Path(linux[0]['stderr']).read_bytes():
                raise RuntimeError('Segment-read Linux output differs')
            source = ROOT / 'tests/GuestSegmentRead/fixture.S'
            capture(source, harness / 'segment-read-native/fixture.S', native['captured_inputs'][str(source)]['sha256'])
            image = capture(native['elf']['path'], attempt / 'guest-segment-read.elf', native['elf']['sha256'])
            _, out, err = run('segment-read-elf-identity', [readelf, '-h', '-lW', '-dW', image])
            if err or b'INTERP' in out or b'(NEEDED)' in out or b'EXEC (Executable file)' not in out:
                raise RuntimeError('Expected static Linux user-segment ELF')
            receipt['segment_read_native'] = dict(path=str(args.segment_read_receipt.resolve()), sha256=sha(native_path),
                guest=str(image), guest_sha256=sha(image), expected_stdout_hex=expected.hex())
            scenarios.append(('segment-read', image))
        receipt['requested_matrix']['scenarios'] = [name for name, _ in scenarios]
        receipt['ready_for_clean_delivery'] = True
        if not args.prepare_only:
            sys.path.insert(0, str(ROOT / 'scripts'))
            from campaign_delivery import load_clean_delivery
            api_path = pin(ROOT / 'tests/MachineApi/run.py')
            api = module(api_path, 'bitset_api_closure')
            for name in ('scripts/campaign_delivery.py', 'scripts/core_inputs.py', 'tests/WorkerInstances/run.py'):
                pin(ROOT / name)
            pointer = capture(args.clean_delivery, attempt / 'delivery-pointer.json')
            producer, delivery = load_clean_delivery(pointer)
            pin(producer)
            receipt['delivery'] = dict(path=str(producer), sha256=sha(producer),
                selected_count=delivery['selected_count'], assembly=delivery['assembly'])
            dotnet = pin(shutil.which('dotnet'))
            _, sdk, _ = run('sdk-version', [dotnet, '--version'])
            sdk = sdk.decode().strip()
            receipt['sdk_version'] = sdk
            # Isolate evaluation settings as well as bin/obj. Every build root
            # is below this test's attempt directory, including the worker.
            prepared = []
            for form in forms:
                selected = Path(delivery['raw_snapshot' if form == 'raw' else 'stable_output'])
                default = Path(delivery['stable_output'])
                inventory = delivery['raw_files' if form == 'raw' else 'final_files']
                closure = api.source_closure(HERE / 'Consumer.csproj')
                closure = {p: d for p, d in closure.items() if not p.is_relative_to(default)}
                product_closure = api.helper.source_closure(selected / 'TranslatedBlink.csproj')
                for source, digest in product_closure.items():
                    if not source.is_relative_to(selected) and delivery['authored_sources'].get(str(source.relative_to(ROOT))) != digest:
                        raise RuntimeError('Delivered authored source differs: ' + str(source))
                closure.update(product_closure)
                private = attempt / form / 'private'
                for source, digest in closure.items():
                    capture(source, private / source.relative_to(ROOT), digest)
                for name, digest in inventory.items():
                    capture(selected / name, private / selected.relative_to(ROOT) / name, digest)
                for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config'):
                    if (REPO / name).is_file():
                        capture(REPO / name, private / name)
                (private / 'global.json').write_text(json.dumps(dict(sdk=dict(version=sdk, rollForward='disable', allowPrerelease=False))) + '\n')
                pin(private / 'global.json')
                project = private / 'tests/GuestFutexBitset/Consumer.csproj'
                copied_product = private / selected.relative_to(ROOT) / 'TranslatedBlink.csproj'
                prepared.append((form, private, project, copied_product))
            # Capture every form before the first compile: later parent changes
            # cannot mix different wrapper source revisions between forms.
            for form, private, project, copied_product in prepared:
                common = ['-c', 'Release', '--disable-build-servers', '-m:2', '-p:UseSharedCompilation=false',
                    '-p:BuildInParallel=false', '-p:IlcSingleThreaded=true', '-p:BlinkProject=' + str(copied_product)]
                run(form + '-build', [dotnet, 'build', project, *common], timeout=1200, cwd=private)
                jit = attempt / form / 'jit'
                shutil.copytree(project.parent / 'bin/Release/net10.0', jit)
                hosts = [('jit', deployment(jit, 'jit', form))]
                if args.aot:
                    aot = attempt / form / 'nativeaot'
                    run(form + '-publish-nativeaot', [dotnet, 'publish', project, *common,
                        '-r', 'linux-x64', '-p:PublishAot=true', '-o', aot], timeout=2400, cwd=private)
                    hosts.append(('nativeaot', deployment(aot, 'nativeaot', form)))
                for runtime, command in hosts:
                    for mode in modes if not args.compile_only else []:
                        for index in range(args.runs):
                            for scenario, image in scenarios:
                                label = '-'.join((form, runtime, mode, scenario, str(index)))
                                report_path = attempt / (label + '.json')
                                row, out, err = run(label, [*command, mode, image, sha(image), report_path, scenario,
                                    *([scenario_dependencies[scenario]] if scenario in scenario_dependencies else [])],
                                    timeout=90, require_success=False)
                                report = json.loads(pin(report_path).read_text()) if report_path.is_file() else None
                                passed = bool(row['normal'] and not err and out == f'guest-consumer {mode} {scenario} PASS\n'.encode()
                                    and report and report.get('Passed') is True and report['ImageSha256'] == sha(image)
                                    and report['Mode'] == mode and report['HostRuntime'] == runtime and report['Scenario'] == scenario)
                                receipt['executions'].append(dict(form=form, mode=mode, host_runtime=runtime, scenario=scenario, run=index,
                                    passed=passed, report=str(report_path), sha256=sha(report_path) if report else None))
                                receipt['managed_execution'] = True
                                save()
            # Fresh qualification also requires the producer's original current
            # authored identities to remain valid at the end of the private run.
            try:
                final_producer, _ = load_clean_delivery(pointer)
                receipt['delivery_current_at_end'] = final_producer == producer
            except Exception as error:
                receipt['delivery_current_at_end'] = False
                receipt['delivery_current_at_end_error'] = repr(error)
            receipt['qualification_scope'] = 'Captured strict producer and frozen private source/deployment closures; parent owns final fresh integration'
        check()
        expected = len(forms) * len(modes) * len(runtimes) * args.runs * len(scenarios)
        receipt['passed'] = (args.prepare_only or args.compile_only or
            len(receipt['executions']) == expected and all(row['passed'] for row in receipt['executions']))
        receipt['complete_matrix'] = bool(receipt['managed_execution'] and set(forms) == set(FORMS) and
            set(modes) == set(MODES) and args.aot and len(receipt['executions']) == expected)
        receipt['scenario_results'] = {scenario: dict(
            runs=sum(row['scenario'] == scenario for row in receipt['executions']),
            passed=bool(receipt['managed_execution']) and
                all(row['passed'] for row in receipt['executions'] if row['scenario'] == scenario)) for scenario, _ in scenarios}
        receipt['fresh_qualification'] = receipt['passed'] and receipt['complete_matrix'] and receipt.get('delivery_current_at_end', False)
        receipt['final_identities_stable'] = True
    except BaseException as error:
        receipt['error'] = repr(error)
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
