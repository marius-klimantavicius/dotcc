#!/usr/bin/env python3
"""Bounded public CoreCLR normal-ReadyToRun service qualification, private builds only."""
import argparse
import base64
import json
import math
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time

sys.dont_write_bytecode = True
from support import HERE, BLINK, REPO, PINS, load_helpers, sha
from profile import read_profile

HOSTS = ['jit', 'nativeaot']
MODES = ['InProcess', 'SeparateProcess']
RESPONSES = [dict(Path='/health', Status=200, Body='CORECLR SERVICE OK\n'),
    dict(Path='/echo', Status=200, Body='CORECLR:payload\n'), dict(Path='/stop', Status=200, Body='STOP OK\n')]


def finite(value):
    return type(value) in (int, float) and math.isfinite(value) and value >= 0


def public_passed(report, mode, host, args, events):
    """Independently check the public report and its flushed host event stream."""
    if not report: return False
    result = report.get('Result') or {}
    worker = report.get('Worker')
    try:
        out = base64.b64decode(result.get('StandardOutput', ''), validate=True)
        err = base64.b64decode(result.get('StandardError', ''), validate=True)
    except (ValueError, TypeError): return False
    names = [r.get('Event') for r in events]
    required = ['image_imported', 'start_requested', 'start_returned', 'ready',
        'http_requested', 'http_response', 'http_requested', 'http_response', 'http_requested', 'http_response',
        'wait_returned', 'disposed']
    milestones = [n for n in names if n != 'guest_exit']
    # Completion can race the final HTTP response. Retain its actual observer
    # timestamp without requiring an invented order relative to /stop's body.
    exits = [r for r in events if r.get('Event') == 'guest_exit']
    responses = [r for r in events if r.get('Event') == 'http_response']
    timings = report.get('HttpTimings', [])
    return (report.get('Passed') is True and report.get('Mode') == mode and report.get('Scenario') == 'service' and
        report.get('HostRuntime') == host and report.get('HostDynamicCodeCompiled') == (host == 'jit') and
        report.get('DiagnosticNoWx') is False and report.get('MemoryMiB') == 256 and report.get('VirtualMemoryMiB') == 4096 and
        report.get('InstructionLimit') == args.instruction_limit and report.get('ExecutionSeconds') == args.seconds and
        report.get('NormalWaitCompleted') is True and report.get('CleanupStopRequested') is False and
        report.get('CleanupKillRequested') is False and report.get('RunDisposed') is True and
        report.get('MachineDisposed') is True and report.get('WorkerGone') is True and
        (type(worker) is int and worker > 0 if mode == 'SeparateProcess' else worker is None) and
        not report.get('Error') and not report.get('ObserverError') and report.get('CleanupErrors') == [] and
        report.get('EventErrors') == [] and result.get('Reason') == 0 and result.get('ExitCode') == 0 and
        type(result.get('Instructions')) is int and 0 < result['Instructions'] <= args.instruction_limit and
        result.get('ResourcesReleased') is True and result.get('CaptureTruncated') is False and
        out == b'SERVICE READY\nSERVICE STOPPED\n' and report.get('Stdout') == out.decode() and
        err == b'' and report.get('Stderr') == '' and report.get('Responses') == RESPONSES and
        all(finite(report.get(k)) for k in ('ImportSeconds', 'StartSeconds', 'ReadyWaitSeconds',
            'ReadySinceStartSeconds', 'WaitSeconds', 'GuestExitSinceStartSeconds', 'TotalSeconds')) and
        report['ReadySinceStartSeconds'] <= args.seconds and report['GuestExitSinceStartSeconds'] <= args.seconds and
        len(timings) == 3 and [r.get('Path') for r in timings] == [r['Path'] for r in RESPONSES] and
        all(finite(r.get(k)) for r in timings for k in ('Seconds', 'HeadersSeconds', 'SinceStartSeconds')) and
        all(r['HeadersSeconds'] <= r['Seconds'] <= r['SinceStartSeconds'] <= args.seconds for r in timings) and
        milestones == required and len(exits) == 1 and exits[0].get('Reason') == 'Exited' and
        exits[0].get('ExitCode') == 0 and exits[0].get('Instructions') == result['Instructions'] and
        [{k: r.get(k) for k in ('Path', 'Status', 'Body')} for r in responses] == RESPONSES and
        [r.get('Sequence') for r in events] == list(range(1, len(events) + 1)) and
        all(finite(r.get('ElapsedSeconds')) for r in events) and
        all(a['ElapsedSeconds'] <= b['ElapsedSeconds'] for a, b in zip(events, events[1:])))


def delivery(ev, pointer, campaign, evidence):
    """Pin LIVE pointer, producer, 110 fresh objects, and both delivered bodies."""
    pointer = ev.pin(pointer)
    producer, selected = campaign.load_clean_delivery(pointer)
    producer = ev.pin(producer)
    original = json.loads(producer.read_text())
    assembly_path = ev.pin(selected['assembly']['path'], selected['assembly']['sha256'])
    assembly = json.loads(assembly_path.read_text())
    if (selected['selected_count'] != 110 or len(assembly['objects']) != 110 or assembly['reused_objects'] != 0 or
            original.get('fresh_objects_requested') is not True or assembly.get('reuse_policy') != 'forced-fresh'):
        raise RuntimeError('Actual 110 fresh / zero reused threaded objects required')
    profile = Path(selected['profile'])
    inputs = json.loads(ev.pin(profile / 'inputs.json', selected['profile_inputs_sha256']).read_text())
    ev.pin(original['threaded_derivation'])
    for name, digest in inputs['staged_headers'].items(): ev.pin(profile / name, digest)
    for row in assembly['objects'].values():
        if row.get('exit_code') != 0 or row.get('classification') not in (
                'emitted object', 'emitted object; no override definitions active'):
            raise RuntimeError('Fresh object command did not emit its body')
        ev.pin(row['object_path'], row['object_sha256'])
        ev.pin(row['receipt']); ev.pin(row['log'])
    for name, digest in selected['authored_sources'].items(): ev.pin(BLINK / name, digest)
    for form, folder, key in [('raw', selected['raw_snapshot'], 'raw_files'),
            ('processed', selected['stable_output'], 'final_files')]:
        actual = {p.relative_to(folder).as_posix(): sha(p) for p in Path(folder).rglob('*') if p.is_file()
            and not {'bin', 'obj'}.intersection(p.relative_to(folder).parts)}
        if actual != selected[key]: raise RuntimeError('Actual ' + form + ' delivery body changed')
        for name, digest in actual.items(): ev.pin(Path(folder) / name, digest)
    for folder in selected['tool_directories'].values(): ev.tree(folder)
    for row in original['commands']:
        for name in ('stdout', 'stderr'):
            if row.get(name): ev.pin(row[name])
    return producer, selected, dict(pointer=str(pointer), pointer_sha256=sha(pointer),
        producer=str(producer), producer_sha256=sha(producer), assembly=selected['assembly'],
        selected_count=110, reused_objects=0, raw_files=selected['raw_files'], processed_files=selected['final_files'])


def freeze_sources(ev, selected, api):
    source = api.source_closure(HERE / 'Probe/Probe.csproj')
    default = Path(selected['stable_output'])
    source = {p: d for p, d in source.items() if not p.is_relative_to(default)}
    generated = Path(selected['raw_snapshot'] if ev.value['form'] == 'raw' else selected['stable_output'])
    source.update(api.helper.source_closure(generated / 'TranslatedBlink.csproj'))
    private = ev.directory / 'private'
    for path, digest in source.items():
        ev.pin(path, digest)
        ev.capture(path, private / path.relative_to(BLINK), digest)
    # Inventory source membership too: an added file must not silently enter
    # the later host/worker NativeAOT compilation.
    for folder in sorted({p.parent for p in source if p.suffix == '.csproj'}):
        ev.value.setdefault('source_membership', {})[str(folder)] = api.helper.manifest(folder)
    for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config', 'global.json'):
        path = REPO / name
        ev.value.setdefault('optional_config', {})[str(path)] = sha(path) if path.is_file() else None
        if path.is_file(): ev.capture(ev.pin(path), private / name)
    for path in HERE.rglob('*'):
        if path.is_file() and not {'bin', 'obj', '__pycache__'}.intersection(path.relative_to(HERE).parts):
            ev.capture(ev.pin(path), ev.directory / 'harness-inputs' / path.relative_to(HERE))
    ev.capture(ev.pin(HERE / 'global.json'), private / 'tests/GuestReadyToRunPublic/global.json')
    ev.value['source_closure'] = {str(p): dict(sha256=d, retained=str(private / p.relative_to(BLINK))) for p, d in source.items()}
    return private, private / generated.relative_to(BLINK) / 'TranslatedBlink.csproj'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--clean-delivery', type=Path, default=BLINK / 'artifacts/campaign/current-threaded.json')
    parser.add_argument('--profile-receipt', type=Path, required=True)
    parser.add_argument('--closure', type=Path, help='Explicit closure must match the normal profile receipt hash')
    parser.add_argument('--form', choices=['raw', 'processed'], default='processed')
    parser.add_argument('--host-runtime', action='append', choices=HOSTS)
    parser.add_argument('--mode', action='append', choices=MODES)
    parser.add_argument('--seconds', type=int, default=3600)
    parser.add_argument('--instruction-limit', type=int, default=3_000_000_000)
    parser.add_argument('--memory-mib', type=int, choices=[256], default=256)
    parser.add_argument('--virtual-memory-mib', type=int, choices=[4096], default=4096)
    parser.add_argument('--compile-only', action='store_true', help='Build selected private host/worker deployments; never run a guest')
    args = parser.parse_args()
    if not 1 <= args.seconds <= 3600 or not 1 <= args.instruction_limit <= 3_000_000_000:
        parser.error('Finite bounds required: seconds 1..3600, instructions 1..3000000000')
    hosts, modes = list(dict.fromkeys(args.host_runtime or HOSTS)), list(dict.fromkeys(args.mode or MODES))
    evidence, api, core, inspector = load_helpers()
    import campaign_delivery as campaign
    from production_support import locked_command

    class PublicEvidence(evidence.Evidence):
        def check(self):
            super().check()
            for folder, expected in self.value.get('source_membership', {}).items():
                if api.helper.manifest(Path(folder)) != expected: raise RuntimeError('Source membership changed: ' + folder)
            for name, expected in self.value.get('optional_config', {}).items():
                if (sha(name) if Path(name).is_file() else None) != expected: raise RuntimeError('Build config changed: ' + name)

        def run(self, label, command, timeout=60, cwd=None, env=None, require_success=True):
            # Keep Evidence's serial receipt/check protocol, while also cleaning
            # a process if launch/receipt persistence or a later report fails.
            self.check()
            row = dict(label=label, command=list(map(str, command)), cwd=str(cwd or self.directory),
                timeout_seconds=timeout, environment_overrides=env or {}, exit_code=None)
            self.value['commands'].append(row)
            out, err = self.directory / (label + '.stdout'), self.directory / (label + '.stderr')
            row.update(stdout=str(out), stderr=str(err))
            process = None; began = time.monotonic()
            self.save()
            try:
                with out.open('wb') as stdout, err.open('wb') as stderr:
                    process = subprocess.Popen(row['command'], cwd=row['cwd'], env=dict(self.environment, **(env or {})),
                        stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr, start_new_session=True)
                    row['pid'] = process.pid; self.save()
                    row['exit_code'] = process.wait(timeout=timeout)
            except BaseException as error:
                row['error'] = repr(error)
                raise
            finally:
                if process is not None: row['cleanup'] = evidence.cleanup(process)
                row['seconds'] = time.monotonic() - began
                row['logs'] = {str(p): sha(p) for p in (out, err) if p.is_file()}
                for path, digest in row['logs'].items(): self.pin(path, digest)
                self.save()
            self.check()
            okay = row['exit_code'] == 0 and not row['cleanup']['signals'] and row['cleanup']['group_gone']
            if require_success and not okay: raise RuntimeError(label + ' failed; see ' + str(err))
            print(label + ': ' + ('passed' if okay else 'failed'), flush=True)
            return out.read_bytes(), err.read_bytes()

    ev = PublicEvidence('readytorun-public-' + args.form)
    receipt = ev.value
    receipt.update(kind='coreclr-readytorun-public', form=args.form, compile_only=args.compile_only,
        scope='Public normal-R2R CoreCLR service; guest native code and residual JIT are interpreted; NativeAOT labels identify HOST only',
        prior_filter_stop='Prior coordinator automated content-filter stop retained; no safeguards disabled or bypassed',
        guest_runtime='CoreCLR 10.0.12 normal ReadyToRun with residual JIT; default tiering and W^X',
        bounds=dict(seconds=args.seconds, instruction_limit=args.instruction_limit, memory_mib=256, virtual_memory_mib=4096),
        requested_matrix=dict(form=args.form, host_runtimes=hosts, modes=modes, scenarios=['service']),
        executions=[], fresh_qualification=False, complete_form_matrix=False, complete_eight_cell_matrix=False)
    try:
        ev.environment.update(DOTNET_PROCESSOR_COUNT='2', DOTNET_ROOT=str(evidence.DOTNET.parent))
        receipt['host_environment'] = {k: ev.environment[k] for k in ('DOTNET_PROCESSOR_COUNT', 'DOTNET_ROOT',
            'LC_ALL', 'MSBUILDDISABLENODEREUSE', 'DOTNET_CLI_USE_MSBUILD_SERVER', 'DOTNET_CLI_TELEMETRY_OPTOUT')}
        ev.pin(sys.executable)
        for name, digest in PINS.items(): ev.pin(REPO / name, digest)
        closure = read_profile(ev, args.profile_receipt, args.closure, evidence, inspector)
        producer, selected, start_delivery = delivery(ev, args.clean_delivery, campaign, evidence)
        receipt['delivery_start'] = start_delivery
        private, blink_project = freeze_sources(ev, selected, api)
        # Freeze loaded local Python helper dependencies before the first build.
        for imported in list(sys.modules.values()):
            name = getattr(imported, '__file__', None)
            if name and Path(name).suffix == '.py' and Path(name).resolve().is_relative_to(REPO): ev.pin(name)
        dotnet = ev.pin(evidence.DOTNET)
        for folder in (dotnet.parent / 'sdk/10.0.112', dotnet.parent / 'host/fxr/10.0.12',
                dotnet.parent / 'shared/Microsoft.NETCore.App/10.0.12'):
            ev.tree(folder)
        for folder in sorted((dotnet.parent / 'packs').glob('*/10.0.12')):
            ev.tree(folder)
        if 'nativeaot' in hosts:
            # Match Microsoft.NETCore.Native.Unix.targets' normal Linux
            # selection: clang first, then its GCC alternative. Requiring an
            # unversioned clang would reject the existing valid GCC toolchain.
            driver = shutil.which('clang') or shutil.which('gcc')
            if not driver: raise RuntimeError('Missing NativeAOT host compiler driver: clang or gcc')
            receipt['native_host_driver'] = dict(path=str(ev.pin(driver)),
                sha256=sha(driver), selection='SDK default: clang, with gcc alternative')
            for name in ('ld', 'objcopy'):
                path = shutil.which(name)
                if not path: raise RuntimeError('Missing NativeAOT host build tool: ' + name)
                ev.pin(path)
            packages = Path(ev.environment.get('NUGET_PACKAGES', str(Path.home() / '.nuget/packages')))
            for name in ('microsoft.dotnet.ilcompiler', 'runtime.linux-x64.microsoft.dotnet.ilcompiler',
                    'microsoft.netcore.app.runtime.nativeaot.linux-x64'):
                folder = packages / name / '10.0.12'
                if folder.is_dir(): ev.tree(folder)
        project = private / 'tests/GuestReadyToRunPublic/Probe/Probe.csproj'
        cwd = project.parent.parent
        common = ['-c', 'Release', '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false',
            '-p:BlinkProject=' + str(blink_project)]
        info, _ = ev.run('host-dotnet-info', [dotnet, '--info'], cwd=cwd)
        version, _ = ev.run('host-sdk-version', [dotnet, '--version'], cwd=cwd)
        if version.strip() != b'10.0.112' or b'10.0.12' not in info or b'95017c711e' not in info:
            raise RuntimeError('Host SDK/runtime identity differs')
        revision, _ = ev.run('revision-start', ['git', 'rev-parse', 'HEAD'], cwd=REPO)
        receipt['revision'] = revision.decode().strip()
        if receipt['revision'] != evidence.BASELINE: raise RuntimeError('HEAD differs from preserved baseline')
        commands = []
        for host in hosts:
            app = ev.directory / host
            if host == 'jit':
                ev.run('public-build-jit', locked_command([dotnet, 'build', project, *common], env=ev.environment),
                    timeout=1200, cwd=cwd)
                shutil.copytree(project.parent / 'bin/Release/net10.0', app)
                command = [dotnet, app / 'Probe.dll']
            else:
                ev.run('public-publish-nativeaot', locked_command([dotnet, 'publish', project, *common,
                    '-r', 'linux-x64', '-p:PublishAot=true', '-o', app], env=ev.environment), timeout=2400, cwd=cwd)
                command = [app / 'Probe']
            core.deployment(ev, app, host)
            if host == 'nativeaot' and any(app.rglob('*.dll')):
                raise RuntimeError('NativeAOT host/worker deployment contains managed DLLs')
            commands.append((host, command))
        # Both selected host/worker deployments are frozen before the first cell.
        for host, command in commands:
            for mode in modes if not args.compile_only else []:
                label = '-'.join((args.form, host, mode, 'service'))
                report_path, event_path = ev.directory / (label + '.json'), ev.directory / (label + '.events.jsonl')
                stdout, stderr = ev.run(label, [*command, mode, closure, args.instruction_limit, args.seconds,
                    256, 4096, report_path, event_path], timeout=args.seconds + 60, require_success=False)
                # Command ownership has already been drained, even if either
                # report is absent or malformed. Keep failures in this attempt.
                report = json.loads(ev.pin(report_path).read_text()) if report_path.is_file() else None
                events = [json.loads(line) for line in ev.pin(event_path).read_text().splitlines()] if event_path.is_file() else []
                row = receipt['commands'][-1]
                normal = row['exit_code'] == 0 and not row['cleanup']['signals'] and row['cleanup']['group_gone']
                passed = bool(normal and not stderr and stdout == f'readytorun {mode} service PASS\n'.encode() and
                    public_passed(report, mode, host, args, events) and report['EventsPath'] == str(event_path))
                receipt['executions'].append(dict(form=args.form, host_runtime=host, mode=mode, scenario='service',
                    passed=passed, report=str(report_path), report_sha256=sha(report_path) if report else None,
                    events=str(event_path), events_sha256=sha(event_path) if event_path.is_file() else None,
                    instructions=(report.get('Result') or {}).get('Instructions') if report else None,
                    ready_seconds=report.get('ReadySinceStartSeconds') if report else None,
                    http_timings=report.get('HttpTimings') if report else None,
                    guest_exit_seconds=report.get('GuestExitSinceStartSeconds') if report else None,
                    outer_seconds=row['seconds']))
                ev.save()
                if not passed: raise RuntimeError('Public service cell failed; retained ' + label)
        _, _, end_delivery = delivery(ev, args.clean_delivery, campaign, evidence)
        receipt['delivery_end'] = end_delivery
        if end_delivery != start_delivery: raise RuntimeError('LIVE clean delivery changed during attempt')
        revision_end, _ = ev.run('revision-end', ['git', 'rev-parse', 'HEAD'], cwd=REPO)
        if revision_end != revision: raise RuntimeError('HEAD changed during attempt')
        ev.check()
        receipt['passed'] = args.compile_only or bool(receipt['executions']) and all(r['passed'] for r in receipt['executions'])
        receipt['complete_form_matrix'] = (not args.compile_only and receipt['passed'] and set(hosts) == set(HOSTS) and
            set(modes) == set(MODES) and len(receipt['executions']) == 4)
        # One-form attempts can be aggregated by the parent only after matching
        # their input identities; this runner never claims eight cells itself.
        receipt['fresh_qualification'] = receipt['passed'] and receipt['complete_form_matrix']
        receipt['qualification_scope'] = dict(form=args.form, all_forms=False,
            selected_cells_passed=not args.compile_only and receipt['passed'], normal_profile_only=True,
            nativeaot_is_host_only=True, runtime_method_selection_proven=False)
    except BaseException as error:
        receipt['error'] = repr(error)
        receipt['passed'] = False
    return ev.finish()


if __name__ == '__main__': raise SystemExit(main())
