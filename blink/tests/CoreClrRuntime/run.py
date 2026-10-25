#!/usr/bin/env python3
"""Public BlinkMachine CoreCLR execution, with optional bounded startup traces.

Only builds attempt-private copies. Never invokes translation or edits shared
generated output. Stale diagnosis is explicitly distinct from fresh validation.
"""
import argparse
import base64
import json
import os
from pathlib import Path
import shutil
import sys
from evidence import Evidence, HERE, BLINK, REPO, DOTNET, EXPECTED, manifest, module, sha

sys.path.insert(0, str(BLINK / 'scripts'))
from campaign_delivery import load_clean_delivery
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import locked_command

CORECLR_SCENARIOS = ['host', 'smoke', 'workload', 'service']
MODES = ['InProcess', 'SeparateProcess']
# The retained fixture writes 26 bytes; its C literal's trailing newline is not
# included in that write. Match the actual oracle, preserving the frozen ELF.
MEMFD_OUTPUT = b'guest memfd aliases passed'


def memfd_closure(ev, handoff_path):
    """Import the exact passing memory handoff without altering either image."""
    handoff_path = ev.pin(handoff_path)
    handoff = json.loads(handoff_path.read_text())
    if handoff.get('baseline') != ev.value['baseline'] or not handoff.get('readyForCoordinatorFreshIntegration'):
        raise RuntimeError('Completed memory handoff required')
    focused_path = ev.pin(handoff_path.parent / handoff['focusedReceipt'])
    focused = json.loads(focused_path.read_text())
    if not focused.get('passed') or not focused.get('completed'):
        raise RuntimeError('Passing finite native memfd oracle required')
    fixture = BLINK / 'tests/GuestSharedMapping/fixture.c'
    source_sha = handoff['sources']['tests/GuestSharedMapping/fixture.c']
    if focused['inputs'].get(str(fixture)) != source_sha:
        raise RuntimeError('Memory handoff/native source identity differs')
    ev.capture(fixture, ev.directory / 'memfd/fixture.c', source_sha)
    ev.pin(focused_path.parent / 'fixture.c', source_sha)
    guest = (focused_path.parent / 'guest-memfd').resolve()
    digest = focused['inputs'][str(guest)]
    build = [row for row in focused['commands'] if row['name'] == 'guest-build']
    oracle = [row for row in focused['commands'] if row['name'] == 'guest-linux']
    if (len(build) != 1 or len(oracle) != 1 or build[0].get('exit') != 0 or oracle[0].get('exit') != 0 or
        oracle[0]['command'] != [str(guest)] or build[0]['command'][-2:] != ['-o', str(guest)]):
        raise RuntimeError('Exact native memfd build/run evidence required')
    log = ev.pin(focused_path.parent / 'guest-linux.log')
    if log.read_bytes() != MEMFD_OUTPUT: raise RuntimeError('Native memfd stdout differs')
    retained = ev.directory / 'memfd/images/guest-memfd'
    ev.capture(guest, retained, digest)
    # The normal finite ELF is statically linked x86-64. It executes only via
    # BlinkMachine below; this harness never runs its code on the host.
    header = retained.read_bytes()[:64]
    if (len(header) != 64 or header[:7] != b'\x7fELF\x02\x01\x01' or
        int.from_bytes(header[16:18], 'little') != 2 or int.from_bytes(header[18:20], 'little') != 62):
        raise RuntimeError('Expected ordinary x86-64 Linux executable ELF')
    closure = dict(Images=str(retained.parent), Environment={}, Files=[dict(
        GuestPath='/bin/guest-memfd', File=retained.name, Sha256=digest, Executable=True)])
    path = ev.directory / 'memfd/closure.json'
    path.write_text(json.dumps(closure, indent=2) + '\n'); ev.pin(path)
    ev.tree(retained.parent)
    ev.value['memfd'] = dict(handoff=str(handoff_path), handoff_sha256=sha(handoff_path),
        native_receipt=str(focused_path), native_receipt_sha256=sha(focused_path),
        native_log=str(log), native_log_sha256=sha(log), source_sha256=source_sha,
        guest=str(retained), guest_sha256=digest, closure=str(path), closure_sha256=sha(path),
        scope='Public translated alias/close/unmap/truncate/private semantics; separate from CoreCLR qualification')
    return path


def deployment(ev, app, host_runtime):
    """Require and freeze the worker actually selected by ordinary deployment."""
    app = Path(app)
    native = host_runtime == 'nativeaot'
    executable = app / ('Probe' if native else 'Probe.dll')
    worker = app / 'blink-worker' / ('Managed.Emulation.Worker' if native else 'Managed.Emulation.Worker.dll')
    required = [executable, worker]
    if native:
        # Keep the matching NativeAOT deployment unambiguous, without a leftover
        # portable worker from an earlier build.
        if (worker.parent / 'Managed.Emulation.Worker.dll').exists():
            raise RuntimeError('NativeAOT worker deployment contains a portable worker')
    else:
        required += [app / 'Probe.runtimeconfig.json', worker.with_suffix('.runtimeconfig.json')]
    if any(not path.is_file() for path in required): raise RuntimeError('Incomplete ordinary ' + host_runtime + ' app/worker deployment')
    if native and any(not os.access(path, os.X_OK) or path.read_bytes()[:4] != b'\x7fELF' for path in required):
        raise RuntimeError('NativeAOT app and worker must be executable ELF files')
    files = ev.tree(app)
    discovered_worker = worker
    if not native and (worker.parent / 'Managed.Emulation.Worker').is_file():
        # Ordinary JIT builds may deploy an apphost, which discovery prefers.
        discovered_worker = worker.parent / 'Managed.Emulation.Worker'
    ev.value.setdefault('execution_closures', {})[host_runtime] = dict(path=str(app), files=files,
        app=str(executable), app_sha256=sha(executable), worker=str(discovered_worker),
        worker_sha256=sha(discovered_worker), worker_managed_assembly=None if native else str(worker))


def public_passed(report, mode, scenario, host_runtime, diagnostic_no_wx, memory_mib, virtual_memory_mib):
    if not report: return False
    result = report.get('Result') or {}
    worker = report.get('Worker')
    expected = MEMFD_OUTPUT if scenario == 'memfd' else EXPECTED.get(scenario)
    stdout = base64.b64decode(result.get('StandardOutput', ''), validate=True)
    stderr = base64.b64decode(result.get('StandardError', ''), validate=True)
    return (report.get('Passed') is True and report.get('Mode') == mode and report.get('Scenario') == scenario and
        report.get('HostRuntime') == host_runtime and report.get('HostDynamicCodeCompiled') == (host_runtime == 'jit') and
        report.get('DiagnosticNoWx') == diagnostic_no_wx and report.get('MemoryMiB') == memory_mib and
        report.get('VirtualMemoryMiB') == virtual_memory_mib and
        report.get('RunDisposed') is True and report.get('MachineDisposed') is True and report.get('WorkerGone') is True and
        (isinstance(worker, int) and worker > 0 if mode == 'SeparateProcess' else worker is None) and
        not report.get('Error') and not report.get('CleanupError') and result.get('Reason') == 0 and
        result.get('ExitCode') == 0 and result.get('Instructions', 0) > 0 and result.get('ResourcesReleased') is True and
        result.get('CaptureTruncated') is False and stderr == b'' and report.get('Stderr') == '' and
        report.get('Stdout') == stdout.decode() and
        (stdout == expected if expected is not None else b'10.0.12' in stdout) and
        (scenario != 'workload' or report.get('Written') == 'guest-output\nappend\n') and
        (scenario != 'service' or report.get('Responses') == [
            dict(Path='/health', Status=200, Body='CORECLR SERVICE OK\n'),
            dict(Path='/echo', Status=200, Body='CORECLR:payload\n'),
            dict(Path='/stop', Status=200, Body='STOP OK\n')]))


def stale_delivery(ev, pointer):
    """Check the actual old producer without comparing it to new authored edits.

    This deliberately cannot produce fresh qualification. Host/bridge source
    bytes are recovered from that producer's immutable staged profile.
    """
    producer = ev.pin(json.loads(Path(pointer).read_text())['receipt'])
    receipt = json.loads(producer.read_text())
    if receipt.get('schema_version') != 1 or receipt.get('status') != 'passed' or receipt.get('hash_policy') != 'strict' or receipt.get('object_relink'):
        raise RuntimeError('Stale diagnosis still requires a real completed strict producer')
    for label in ('delivery-link', 'raw-build', 'postprocess', 'processed-build', 'final-raw-build', 'final-processed-build'):
        rows = [r for r in receipt['commands'] if r['label'] == label]
        if len(rows) != 1 or rows[0].get('status') != 'passed' or rows[0].get('exit_code') != 0:
            raise RuntimeError('Incomplete stale producer command: ' + label)
    derivation = json.loads(ev.pin(receipt['threaded_derivation']).read_text())
    profile = Path(derivation['profile'])
    inputs_path = ev.pin(profile / 'inputs.json'); inputs = json.loads(inputs_path.read_text())
    assembly_path = ev.pin(receipt['assembly']); assembly = json.loads(assembly_path.read_text())
    core = module(ev.pin(BLINK / 'scripts/core_inputs.py'), 'coreclr_old_profile')
    entries = core.profile_sources(profile, BLINK, inputs)
    expected = [entry['path'] for entry in entries]
    if (not assembly.get('linked') or assembly.get('failures') or assembly.get('reused_objects') != 0 or
        assembly['selected'] != expected or set(assembly['objects']) != set(expected) or
        assembly['identity']['profile_inputs_sha256'] != sha(inputs_path) or assembly['identity']['compiler_sha256'] != inputs['compiler']):
        raise RuntimeError('Incomplete stale selected-source producer')
    for entry in entries:
        row = assembly['objects'][entry['path']]
        ev.pin(BLINK / entry['staged_path'], entry['sha256'])
        if row['source_sha256'] != entry['sha256']: raise RuntimeError('Stale source/object mismatch')
        ev.pin(row['object_path'], row['object_sha256']); ev.pin(row['receipt'])
    for name, digest in inputs['staged_headers'].items(): ev.pin(profile / name, digest)
    outputs = {}
    for form in ('raw', 'processed'):
        directory = Path(receipt['outputs'][form]['project']).parent
        actual = {p.relative_to(directory).as_posix(): sha(p) for p in directory.rglob('*')
            if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
        if actual != receipt['outputs'][form]['hashes']: raise RuntimeError('Stale delivered source changed: ' + form)
        outputs[form] = str(directory)
    return producer, dict(stable_output=outputs['processed'], raw_snapshot=outputs['raw'],
        profile=str(profile), assembly=dict(path=str(assembly_path), sha256=sha(assembly_path)))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--clean-delivery', type=Path, required=True)
    parser.add_argument('--native-receipt', type=Path, required=True)
    parser.add_argument('--diagnostic-stale', action='store_true', help='Label existing delivery as baseline diagnosis, never fresh qualification')
    parser.add_argument('--scenario', action='append', choices=[*CORECLR_SCENARIOS, 'memfd'])
    parser.add_argument('--mode', action='append', choices=['InProcess', 'SeparateProcess'])
    parser.add_argument('--trace', action='append', choices=['host', 'smoke', 'workload', 'service'], help='Additional lower-level startup diagnosis; does not replace public runs')
    parser.add_argument('--instruction-limit', type=int, default=100_000_000)
    parser.add_argument('--seconds', type=int, default=60)
    parser.add_argument('--memory-mib', type=int, default=256)
    parser.add_argument('--virtual-memory-mib', type=int, default=0,
        help='Explicit virtual reservation limit through 4096 MiB; zero retains MemoryLimit')
    parser.add_argument('--diagnostic-no-wx', action='store_true', help='Explicit experiment only; never qualifies default runtime W^X support')
    parser.add_argument('--form', choices=['raw', 'processed'], default='processed')
    parser.add_argument('--aot', action='store_true', help='Also publish/run matching NativeAOT HOST Probe and ordinary worker; guest remains normal CoreCLR')
    parser.add_argument('--memory-handoff', type=Path, help='Memory handoff receipt supplying the exact native memfd ELF for public execution')
    parser.add_argument('--compile-only', action='store_true', help='Build/publish private consumers only; never runs guests or qualifies runtime support')
    args = parser.parse_args()
    if args.memory_mib < 1 or args.memory_mib > 256 or args.instruction_limit < 1 or args.seconds < 1:
        parser.error('Positive bounds and memory at most 256 MiB required')
    if args.virtual_memory_mib and not args.memory_mib <= args.virtual_memory_mib <= 4096:
        parser.error('Explicit virtual budget must be between backing budget and 4096 MiB')
    if 'memfd' in (args.scenario or []) and not args.memory_handoff:
        parser.error('--scenario memfd requires --memory-handoff')
    scenarios = list(dict.fromkeys(args.scenario or [*CORECLR_SCENARIOS, *(['memfd'] if args.memory_handoff else [])]))
    modes = list(dict.fromkeys(args.mode or MODES))
    host_runtimes = ['jit', 'nativeaot'] if args.aot else ['jit']
    ev = Evidence('guest'); receipt = ev.value
    ev.pin(REPO / 'Scripts/production_support.py')
    receipt.update(stale_diagnostic=args.diagnostic_stale, diagnostic_no_wx=args.diagnostic_no_wx,
        fresh_qualification=False, executions=[], traces=[], bounds=dict(memory_mib=args.memory_mib,
        virtual_memory_mib=args.virtual_memory_mib,
        instruction_limit=args.instruction_limit, seconds=args.seconds), compile_only=args.compile_only,
        requested_matrix=dict(form=args.form, host_runtimes=host_runtimes, modes=modes, scenarios=scenarios),
        complete_public_matrix=False, complete_host_runtime_matrix=False, memfd_public_passed=False)
    if args.compile_only: receipt['scope'] = 'Compile-only private consumer/ordinary worker build; no guest execution or runtime qualification'
    try:
        ev.authored(freeze=True)
        native_path = ev.pin(args.native_receipt); native = json.loads(native_path.read_text())
        if not native.get('passed') or not native.get('completed') or len(native['native_cases']) != 3:
            raise RuntimeError('Passing retained native IL/workload/Kestrel oracle required')
        closure_path = ev.pin(native['closure']); closure = json.loads(closure_path.read_text())
        images = Path(closure['Images'])
        if manifest(images) != native['image_files']: raise RuntimeError('Native/guest image changed')
        for file in closure['Files']: ev.pin(images / file['File'], file['Sha256'])
        receipt['native'] = dict(path=str(native_path), sha256=sha(native_path), closure=str(closure_path), sha256_closure=sha(closure_path))
        memfd = memfd_closure(ev, args.memory_handoff) if args.memory_handoff else None
        # The parent may publish a new pointer while this old diagnosis runs.
        pointer = ev.directory / 'delivery-pointer.json'
        ev.capture(args.clean_delivery, pointer)
        producer, delivery = stale_delivery(ev, pointer) if args.diagnostic_stale else load_clean_delivery(pointer)
        ev.pin(producer)
        receipt['delivery'] = dict(path=str(producer), sha256=sha(producer), assembly=delivery['assembly'],
            form=args.form, source_label='stale baseline diagnostic' if args.diagnostic_stale else 'fresh delivery candidate')
        # Reuse the reviewed ElfRuntime/MachineApi closure walkers, preserving
        # relative source/project paths inside one attempt-owned tree.
        api = module(ev.pin(BLINK / 'tests/MachineApi/run.py'), 'coreclr_api_closure')
        workers = api.helper
        ev.pin(BLINK / 'tests/WorkerInstances/run.py'); ev.pin(BLINK / 'scripts/campaign_delivery.py')
        selected = Path(delivery['raw_snapshot' if args.form == 'raw' else 'stable_output'])
        default = Path(delivery['stable_output'])
        source_closure = api.source_closure(HERE / 'Probe/Probe.csproj')
        source_closure.update(api.source_closure(HERE / 'Trace/Trace.csproj'))
        source_closure = {p: d for p, d in source_closure.items() if not p.is_relative_to(default)}
        source_closure.update(workers.source_closure(selected / 'TranslatedBlink.csproj'))
        substitutions = {}
        if args.diagnostic_stale:
            profile = Path(delivery['profile'])
            inputs = json.loads((profile / 'inputs.json').read_text())
            source_closure = {p: d for p, d in source_closure.items() if not p.is_relative_to(BLINK / 'src/Managed.Emulation.Host')}
            for name, digest in inputs['staged_headers'].items():
                if name.startswith('host-project/'):
                    original = BLINK / 'src/Managed.Emulation.Host' / name.removeprefix('host-project/')
                    source_closure[original] = digest; substitutions[original] = profile / name
            bindings = json.loads((profile / 'host-bindings.json').read_text())
            for name in bindings['managedSources']:
                original = BLINK / name; frozen = profile / 'managed' / original.name
                digest = inputs['staged_headers']['managed/' + original.name]
                source_closure[original] = digest; substitutions[original] = frozen
            receipt['stale_host_sources'] = {str(p): str(s) for p, s in substitutions.items()}
        private = ev.directory / 'private'
        for source, digest in source_closure.items():
            target = private / source.relative_to(BLINK)
            ev.capture(substitutions.get(source, source), target, digest)
        receipt['source_closure'] = {str(source): dict(sha256=digest,
            retained=str(private / source.relative_to(BLINK))) for source, digest in source_closure.items()}
        for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config'):
            source = REPO / name
            if source.is_file(): shutil.copyfile(ev.pin(source), private / name)
        global_path = private / 'tests/CoreClrRuntime/global.json'
        shutil.copyfile(HERE / 'global.json', global_path); ev.pin(global_path)
        copied_project = private / selected.relative_to(BLINK) / 'TranslatedBlink.csproj'
        common = ['-c', 'Release', '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false', '-p:BlinkProject=' + str(copied_project)]
        dotnet = ev.pin(DOTNET)
        public = private / 'tests/CoreClrRuntime/Probe/Probe.csproj'
        ev.run('public-build', locked_command(list(map(str, [dotnet, 'build', public, *common])),
            env=ev.environment), timeout=1200, cwd=global_path.parent)
        jit = ev.directory / 'jit'
        shutil.copytree(public.parent / 'bin/Release/net10.0', jit)
        deployment(ev, jit, 'jit')
        # Retain the original receipt alias for readers of the JIT-only harness.
        receipt['execution_closure'] = receipt['execution_closures']['jit']['files']
        hosts = [('jit', [dotnet, jit / 'Probe.dll'])]
        if args.aot:
            aot = ev.directory / 'nativeaot'
            ev.run('public-publish-nativeaot', locked_command(list(map(str,
                [dotnet, 'publish', public, *common, '-r', 'linux-x64', '-p:PublishAot=true', '-o', aot])),
                env=ev.environment), timeout=2400, cwd=global_path.parent)
            deployment(ev, aot, 'nativeaot')
            hosts.append(('nativeaot', [aot / 'Probe']))
        policy = 'diagnostic-no-wx' if args.diagnostic_no_wx else 'wx-default'
        for host_runtime, command in hosts:
            for mode in modes if not args.compile_only else []:
                for scenario in scenarios:
                    label = '-'.join((args.form, host_runtime, mode, scenario))
                    report_path = ev.directory / (label + '.json')
                    stdout, stderr = ev.run(label, [*command, mode, memfd if scenario == 'memfd' else closure_path,
                        scenario, args.instruction_limit, args.seconds, args.memory_mib, policy, report_path,
                        args.virtual_memory_mib],
                        timeout=args.seconds + 30, require_success=False)
                    report = json.loads(ev.pin(report_path).read_text()) if report_path.exists() else None
                    row = receipt['commands'][-1]
                    normal = row['exit_code'] == 0 and not row['cleanup']['signals'] and row['cleanup']['group_gone']
                    passed = bool(normal and stdout == f'coreclr {mode} {scenario} PASS\n'.encode() and not stderr and
                        public_passed(report, mode, scenario, host_runtime, args.diagnostic_no_wx,
                            args.memory_mib, args.virtual_memory_mib))
                    receipt['executions'].append(dict(form=args.form, host_runtime=host_runtime, mode=mode,
                        scenario=scenario, passed=passed, report=str(report_path), sha256=sha(report_path) if report else None,
                        instructions=report['Result']['Instructions'] if report and report.get('Result') else None))
                    ev.save()
        if args.trace:
            project = private / 'tests/CoreClrRuntime/Trace/Trace.csproj'
            ev.run('trace-build', locked_command(list(map(str, [dotnet, 'build', project, *common])),
                env=ev.environment), timeout=1200, cwd=global_path.parent)
            trace_app = ev.directory / 'trace-jit'
            shutil.copytree(project.parent / 'bin/Release/net10.0', trace_app)
            receipt['trace_closure'] = ev.tree(trace_app)
            for scenario in dict.fromkeys(args.trace) if not args.compile_only else []:
                label = 'trace-' + scenario; report_path = ev.directory / (label + '.json')
                ev.run(label, [dotnet, trace_app / 'Trace.dll', closure_path, scenario, args.instruction_limit,
                    args.seconds, args.memory_mib, policy, report_path, args.virtual_memory_mib],
                    timeout=args.seconds + 30, require_success=False)
                if report_path.exists(): ev.pin(report_path)
                receipt['traces'].append(dict(form=args.form, host_runtime='jit', scenario=scenario, report=str(report_path),
                    sha256=sha(report_path) if report_path.exists() else None))
        ev.check()
        receipt['passed'] = args.compile_only or (bool(receipt['executions']) and all(row['passed'] for row in receipt['executions']))
        receipt['complete_public_matrix'] = (not args.compile_only and set(CORECLR_SCENARIOS) <= set(scenarios) and set(modes) == set(MODES) and
            len(receipt['executions']) == len(host_runtimes) * len(modes) * len(scenarios))
        receipt['complete_host_runtime_matrix'] = receipt['complete_public_matrix'] and args.aot
        memfd_rows = [row for row in receipt['executions'] if row['scenario'] == 'memfd']
        receipt['memfd_public_passed'] = bool(memfd_rows) and all(row['passed'] for row in memfd_rows)
        receipt['fresh_qualification'] = (receipt['passed'] and receipt['complete_host_runtime_matrix'] and
            not args.diagnostic_stale and not args.diagnostic_no_wx)
        receipt['qualification_scope'] = dict(form=args.form, host_runtimes=host_runtimes,
            all_forms=False, guest_runtime='ordinary CoreCLR 10.0.12 IL/JIT',
            supplementary_memfd='memfd' in scenarios)
    except BaseException as error: receipt['error'] = repr(error)
    return ev.finish()


if __name__ == '__main__': raise SystemExit(main())
