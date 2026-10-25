#!/usr/bin/env python3
"""Consume a fresh strict producer in private raw/processed JIT/AOT matrices.

--prepare-only checks the valid ordinary Linux guest without any product build.
Never translates, builds a shared project, or qualifies a stale producer.
"""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import signal
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]


def module(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    selection = parser.add_mutually_exclusive_group(required=True)
    selection.add_argument('--prepare-only', action='store_true')
    selection.add_argument('--clean-delivery', type=Path)
    parser.add_argument('--form', action='append', choices=('raw', 'processed'))
    parser.add_argument('--mode', action='append', choices=('InProcess', 'SeparateProcess'))
    parser.add_argument('--aot', action='store_true')
    parser.add_argument('--runs', type=int, default=1, choices=range(1, 4))
    args = parser.parse_args()
    evidence_path = ROOT / 'tests/CoreClrRuntime/evidence.py'
    evidence = module(evidence_path, 'shared_identity_evidence')
    evidence.ARTIFACTS = HERE / '.artifacts'
    ev = evidence.Evidence('managed')
    sha = evidence.sha
    receipt, attempt = ev.value, ev.directory
    receipt.update(kind='ordinary-guest-shared-identity',
        scope='Ordinary alias/private/anonymous/bitset/offset futex and concurrent dup2 shared/private mmap; private public BlinkMachine consumers',
        native_blink_oracle_resolved=False, managed_execution=False, fresh_qualification=False, executions=[],
        prior_coordinator_termination='Automated content filter citing possible cyber risk retained; no safeguards changed')
    ev.environment.update(DOTNET_PROCESSOR_COUNT='2')
    forms = list(dict.fromkeys(args.form or ('raw', 'processed')))
    modes = list(dict.fromkeys(args.mode or ('InProcess', 'SeparateProcess')))
    try:
        ev.pin(evidence_path)
        for name in ('run-managed.py', 'Consumer.cs', 'Consumer.csproj', 'fixture.c', 'entry.S'):
            ev.capture(HERE / name, attempt / 'harness' / name)
        linker = ROOT / 'tests/GuestThreads/fixture.ld'
        ev.capture(linker, attempt / 'fixture.ld')
        guest = attempt / 'guest-shared-identity.elf'
        ev.run('guest-c', ['cc', '-c', '-O2', '-std=c17', '-Wall', '-Wextra', '-Werror', '-ffreestanding',
            '-fno-stack-protector', '-fno-pie', attempt / 'harness/fixture.c', '-o', attempt / 'fixture.o'])
        ev.run('guest-entry', ['cc', '-c', '-nostdlib', attempt / 'harness/entry.S', '-o', attempt / 'entry.o'])
        ev.run('guest-link', ['ld', '-static', '-z', 'noexecstack', '-z', 'max-page-size=4096', '-T', attempt / 'fixture.ld',
            attempt / 'fixture.o', attempt / 'entry.o', '-o', guest])
        ev.pin(guest)
        identity, error = ev.run('guest-identity', ['readelf', '-h', '-lW', '-dW', guest])
        if error or b'INTERP' in identity or b'(NEEDED)' in identity or b'EXEC (Executable file)' not in identity:
            raise RuntimeError('Expected ordinary static ELF')
        expected = b'guest-shared-identity: alias/private/anon/bitset/offset/dup2 shared+private: PASS\n'
        for index in range(args.runs):
            output, error = ev.run('linux-' + str(index), [guest], timeout=20)
            if error or output != expected: raise RuntimeError('Linux guest output differs')
        receipt['guest'] = dict(path=str(guest), sha256=sha(guest), expected_stdout_hex=expected.hex())
        receipt['linux_passed'] = True
        if not args.prepare_only:
            sys.path.insert(0, str(ROOT / 'scripts'))
            from campaign_delivery import load_clean_delivery
            ev.pin(ROOT / 'scripts/campaign_delivery.py')
            api_path = ROOT / 'tests/MachineApi/run.py'
            api = module(api_path, 'shared_identity_source_closure')
            ev.pin(api_path); ev.pin(ROOT / 'tests/WorkerInstances/run.py')
            ev.capture(args.clean_delivery, attempt / 'delivery-pointer.json')
            producer, delivery = load_clean_delivery(attempt / 'delivery-pointer.json')
            ev.pin(producer)
            receipt['producer'] = dict(path=str(producer), sha256=sha(producer))
            dotnet = ev.pin(shutil.which('dotnet'))
            sdk, _ = ev.run('sdk', [dotnet, '--version'])
            prepared = []
            for form in forms:
                selected = Path(delivery['raw_snapshot' if form == 'raw' else 'stable_output'])
                default = Path(delivery['stable_output'])
                closure = {p: d for p, d in api.source_closure(HERE / 'Consumer.csproj').items()
                    if not p.is_relative_to(default)}
                product = api.helper.source_closure(selected / 'TranslatedBlink.csproj')
                for source, digest in product.items():
                    if not source.is_relative_to(selected) and delivery['authored_sources'].get(str(source.relative_to(ROOT))) != digest:
                        raise RuntimeError('Unpinned product source: ' + str(source))
                closure.update(product)
                private = attempt / form / 'private'
                for source, digest in closure.items(): ev.capture(source, private / source.relative_to(ROOT), digest)
                for name, digest in delivery['raw_files' if form == 'raw' else 'final_files'].items():
                    ev.capture(selected / name, private / selected.relative_to(ROOT) / name, digest)
                for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config'):
                    if (ROOT.parent / name).is_file(): ev.capture(ROOT.parent / name, private / name)
                (private / 'global.json').write_text(json.dumps(dict(sdk=dict(version=sdk.decode().strip(), rollForward='disable', allowPrerelease=False))) + '\n')
                ev.pin(private / 'global.json')
                prepared.append((form, private, private / 'tests/GuestSharedIdentity/Consumer.csproj',
                    private / selected.relative_to(ROOT) / 'TranslatedBlink.csproj'))
            for form, private, project, product in prepared:
                common = ['-c', 'Release', '--disable-build-servers', '-m:2', '-p:UseSharedCompilation=false',
                    '-p:BuildInParallel=false', '-p:IlcSingleThreaded=true', '-p:BlinkProject=' + str(product)]
                ev.run(form + '-build', [dotnet, 'build', project, *common], timeout=1200, cwd=private)
                jit = attempt / form / 'jit'
                shutil.copytree(project.parent / 'bin/Release/net10.0', jit)
                hosts = [('jit', jit)]
                if args.aot:
                    aot = attempt / form / 'nativeaot'
                    ev.run(form + '-aot', [dotnet, 'publish', project, *common, '-r', 'linux-x64',
                        '-p:PublishAot=true', '-o', aot], timeout=2400, cwd=private)
                    hosts.append(('nativeaot', aot))
                for runtime, deployed in hosts:
                    native = runtime == 'nativeaot'
                    app = deployed / ('GuestSharedIdentityConsumer' if native else 'GuestSharedIdentityConsumer.dll')
                    worker = deployed / 'blink-worker' / ('Managed.Emulation.Worker' if native else 'Managed.Emulation.Worker.dll')
                    ev.pin(app); ev.pin(worker)
                    if native and any(p.read_bytes()[:4] != b'\x7fELF' for p in (app, worker)):
                        raise RuntimeError('NativeAOT host/worker ELF missing')
                    if native and (worker.parent / 'Managed.Emulation.Worker.dll').exists():
                        raise RuntimeError('Ambiguous NativeAOT worker closure')
                    ev.tree(deployed)
                    command = [app] if native else [dotnet, app]
                    for mode in modes:
                        for index in range(args.runs):
                            label = '-'.join((form, runtime, mode, str(index)))
                            report = attempt / (label + '.json')
                            output, error = ev.run(label, [*command, mode, guest, sha(guest), report], timeout=80, require_success=False)
                            value = json.loads(ev.pin(report).read_text()) if report.is_file() else None
                            row = receipt['commands'][-1]
                            passed = bool(value and value.get('Passed') and value.get('Mode') == mode and
                                value.get('HostRuntime') == runtime and value.get('ImageSha256') == sha(guest) and
                                row['exit_code'] == 0 and not row['cleanup']['signals'] and row['cleanup']['group_gone'] and
                                not error and output == f'shared-identity {mode} PASS\n'.encode())
                            receipt['executions'].append(dict(form=form, host_runtime=runtime, mode=mode, run=index,
                                passed=passed, report=str(report), sha256=sha(report) if value else None))
                            receipt['managed_execution'] = True
                            ev.save()
            latest, _ = load_clean_delivery(attempt / 'delivery-pointer.json')
            receipt['delivery_current_at_end'] = latest == producer
        ev.check()
        count = len(forms) * len(modes) * (2 if args.aot else 1) * args.runs
        receipt['passed'] = args.prepare_only or (len(receipt['executions']) == count and
            all(row['passed'] for row in receipt['executions']) and receipt.get('delivery_current_at_end') is True)
        receipt['complete_matrix'] = bool(receipt['managed_execution'] and len(forms) == 2 and len(modes) == 2 and
            args.aot and len(receipt['executions']) == count)
        receipt['fresh_qualification'] = receipt['passed'] and receipt['complete_matrix']
    except BaseException as error:
        receipt['error'] = repr(error)
    return ev.finish()


def interrupted(number, frame):
    raise InterruptedError('Received signal ' + str(number))


if __name__ == '__main__':
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
