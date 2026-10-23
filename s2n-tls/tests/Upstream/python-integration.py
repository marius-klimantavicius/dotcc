"""Execute original required Python families with exact CLI command adapters.

No best-effort provider filtering, source edits, assertion replacement or
capability skipping. Missing peers/dependencies are reported as failures.
"""
import argparse
from contextlib import nullcontext
from datetime import datetime, timezone
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import uuid

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'tests/Managed'))
from execution_identity import hashes


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--executables', type=Path, required=True, help='Built original CLI endpoint manifest from build.py --endpoint')
    parser.add_argument('--peer-commands', type=Path, help='Exact command prefixes and staged peer assets, supplied by peer preparation')
    parser.add_argument('--form', choices=('raw', 'processed'), required=True)
    parser.add_argument('--mode', choices=('jit', 'aot'), required=True)
    parser.add_argument('--python', default=sys.executable, help='Isolated interpreter containing the declared upstream pytest dependencies')
    parser.add_argument('--case', action='append', help='Exact frozen Python source path; default all18 families')
    parser.add_argument('--node', action='append', help='Exact original pytest nodeid (file.py::test[parameters]); diagnostic replay only, never whole-family qualification')
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--collect-only', action='store_true', help='Parameter inventory only, always leaves runtime qualification unrun')
    parser.add_argument('--no-reruns', action='store_true', help='Disable the retry plugin; execute every selected original assertion once')
    parser.add_argument('--hrr-no-retry', action='store_true', help='Run the separate nine authored same-curve no-HRR controls')
    parser.add_argument('--available-port-probe', action='store_true', help='Probe original port candidates; consume only real EADDRINUSE conflicts')
    parser.add_argument('--hrr-proof', type=Path, help='Matching separate nine-control no-HRR proof for exact original inapplicable combinations')
    parser.add_argument('--pipe-read-limit', type=int, default=32768, help='Diagnostic fragmentation bound, at most the original 32768-byte os.read size')
    parser.add_argument('--workers', type=int, default=1, help='Original pytest-xdist workers; preserves upstream worker port partitioning')
    parser.add_argument('--port-low', type=int, help='Explicit original AvailablePorts lower constructor argument')
    parser.add_argument('--port-high', type=int, help='Explicit original AvailablePorts upper constructor argument (exclusive)')
    parser.add_argument('--provider-version', default='dotcc-managed', help='Truthful integration capability identity; managed default is not an OpenSSL vendor claim')
    parser.add_argument('--endpoint-kind', choices=('translated', 'native'), default='translated', help='Explicit scope of the same-pin main endpoint manifest')
    args = parser.parse_args()
    if not 1 <= args.pipe_read_limit <= 32768:
        parser.error('--pipe-read-limit must be within 1..32768')
    args.executables = args.executables.resolve()
    if args.peer_commands:
        args.peer_commands = args.peer_commands.resolve()
    if args.hrr_no_retry:
        if args.node or args.case:
            parser.error('--hrr-no-retry is a separate authored selection, without --node/--case')
        args.case = ['tests/integrationv2/test_hello_retry_requests.py']
    if args.port_low is None and args.port_high is None and args.endpoint_kind == 'translated':
        args.port_low, args.port_high = 18001, 30000
    if args.workers < 1: parser.error('--workers must be positive')
    if (args.port_low is None) != (args.port_high is None): parser.error('--port-low and --port-high must be supplied together')
    if args.port_low is not None and not 1 <= args.port_low < args.port_high <= 65536:
        parser.error('Original AvailablePorts bounds must be valid TCP ports')
    args.output = args.output.resolve()
    # Preserve a virtualenv executable's symlink path: resolving it to the host
    # interpreter would discard the pinned pytest environment.
    args.python = os.path.abspath(args.python)
    run_id = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ') + '-' + uuid.uuid4().hex[:8]
    work = ROOT / 'build/managed-python-upstream' / run_id
    work.mkdir(parents=True)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    report = {'evidence_kind': 'original-python-integration', 'endpoint_kind': args.endpoint_kind,
              'status': 'failed', 'form': args.form, 'mode': args.mode, 'cases': [], 'services': []}
    report['retry_policy'] = {'disabled': args.no_reruns, 'method': 'pytest -p no:rerunfailures' if args.no_reruns else 'original upstream markers',
                             'nonpassing_attempts_are_failures': True}
    if args.node:
        report.update(evidence_kind='focused-original-python-integration', family_qualification_status='unrun',
                      selected_nodeids=args.node)
    if args.hrr_no_retry:
        report.update(evidence_kind='authored-hrr-no-retry-controls', original_family_qualification='unrun',
                      scope='Nine authored no-HRR controls; none is counted as an original HRR assertion')

    def checkpoint():
        destination = args.output.with_name(args.output.stem + '-progress.json')
        temporary = destination.with_suffix('.tmp')
        temporary.write_text(json.dumps(dict(report, status='running'), indent=2) + '\n')
        temporary.replace(destination)
    config = json.loads((ROOT / 'config/upstream-tests.json').read_text())
    cases = [r for r in config['cases'] if r['status'].startswith('required') and r['path'].endswith('.py')]
    if args.case:
        if set(args.case) - {c['path'] for c in cases}: parser.error('--case must name exact required Python files')
        cases = [r for r in cases if r['path'] in args.case]
    if args.node:
        selected_files = {node.split('::', 1)[0] for node in args.node}
        if any('::' not in node for node in args.node) or selected_files - {Path(r['path']).name for r in cases}:
            parser.error('--node must identify an exact original test node in a selected required Python file')
        if len(set(args.node)) != len(args.node): parser.error('--node must not contain duplicates')
        cases = [r for r in cases if Path(r['path']).name in selected_files]
    for case in cases:
        report['cases'].append({'suite': 'upstream', 'case': case['path'], 'form': args.form, 'mode': args.mode,
                                'status': 'unrun', 'test_functions': case['test_functions'], 'skipped': 0})
    try:
        adapter_files = [ROOT / 'tests/Upstream' / name for name in
                         ('pytest_adapter.py', 'readiness.py', 'services.py', 'measured_psk.py', 'port_probe.py',
                          'hrr_applicability.py', 'python_receipt.py', 'python-adaptations.json')]
        adapter_files.append(ROOT / 'tests/Managed/execution_identity.py')
        original_adapters = hashes(adapter_files)
        frozen_adapters = work / 'adapters'; frozen_adapters.mkdir()
        for original in adapter_files: shutil.copyfile(original, frozen_adapters / original.name)
        copied_adapters = {str(original): hashlib.sha256((frozen_adapters / original.name).read_bytes()).hexdigest()
                           for original in adapter_files}
        if copied_adapters != original_adapters or hashes(adapter_files) != original_adapters:
            raise RuntimeError('Authored Python adapters changed during private snapshot creation')
        report['adapter_source_sha256'] = original_adapters
        report['frozen_adapter_sha256'] = hashes(frozen_adapters.iterdir())
        report['runner_sha256'] = hashes([Path(__file__)])
        shutil.copyfile(Path(__file__), work / 'python-integration.py')
        report['frozen_runner_sha256'] = hashes([work / 'python-integration.py'])
        specification = importlib.util.spec_from_file_location('dotcc_frozen_peer_services', frozen_adapters / 'services.py')
        service_module = importlib.util.module_from_spec(specification)
        specification.loader.exec_module(service_module)
        PeerServices, run_and_drain = service_module.PeerServices, service_module.run_and_drain
        stage = work / 'source'
        report['original_source_root'] = str(stage)
        shutil.copytree(ROOT / 'ref/s2n-tls/tests', stage / 'tests')
        # Every original Python source/helper is identified before execution.
        report['python_source_sha256'] = {p.relative_to(stage).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
                                          for p in stage.rglob('*.py')}
        if args.hrr_no_retry:
            authored = ROOT / 'tests/Upstream/test_hrr_no_retry.py'
            authored_control_target = stage / 'tests/integrationv2/dotcc_controls/test_hrr_no_retry.py'
            authored_control_target.parent.mkdir()
            shutil.copyfile(authored, authored_control_target)
            report['authored_control_sha256'] = hashes([authored, authored_control_target])
        endpoints = json.loads(args.executables.read_text()) if args.executables.is_file() else {'executables': []}
        report['endpoint_manifest_sha256'] = hashes([args.executables]) if args.executables.is_file() else {}
        endpoint_closure = {}
        commands = {}
        for row in endpoints['executables']:
            if row['form'] != args.form or row['mode'] != args.mode or row['status'] != 'built': continue
            name = Path(row['case']).stem
            if name not in ('s2nc', 's2nd'): continue
            for path, expected in row['execution_sha256'].items():
                if hashlib.sha256(Path(path).read_bytes()).hexdigest() != expected:
                    raise RuntimeError('Translated CLI closure identity changed: ' + path)
            endpoint_closure.update(row['execution_sha256'])
            commands[name] = row['command']
        for name in ('s2nc', 's2nd'):
            if name not in commands and not args.collect_only: raise RuntimeError('Required real ' + args.endpoint_kind + ' CLI is missing: ' + name)
        if args.peer_commands:
            peers = json.loads(args.peer_commands.read_text())
            report['peer_manifest_sha256'] = hashes([args.peer_commands])
            report['peer_environment'] = peers.get('environment', {})
            if set(commands) & peers.get('commands', {}).keys(): raise RuntimeError('Peer mapping cannot replace translated product endpoints')
            commands.update(peers.get('commands', {}))
            for asset in peers.get('assets', []):
                target = (stage / asset['destination']).resolve()
                if not target.is_relative_to(stage): raise RuntimeError('Peer asset destination escapes private stage')
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(asset['source'], target)
        else:
            peers = {}
        measured = None
        hrr = None
        if peers.get('measured_psk_peer'):
            if len(cases) != 1 or Path(cases[0]['path']).name != 'test_external_psk.py':
                raise RuntimeError('Measured PSK peer selection requires an external-PSK-only family run')
            specification = importlib.util.spec_from_file_location('dotcc_frozen_measured_psk', frozen_adapters / 'measured_psk.py')
            measured_module = importlib.util.module_from_spec(specification)
            specification.loader.exec_module(measured_module)
            measured = measured_module.validate(peers)
            report['measured_psk_peer'] = measured
        if args.hrr_proof:
            if args.hrr_no_retry or args.node or len(cases) != 1 or Path(cases[0]['path']).name != 'test_hello_retry_requests.py':
                raise RuntimeError('--hrr-proof requires an unchanged complete original HRR-family selection')
            sys.path.insert(0, str(frozen_adapters))
            from hrr_applicability import validate_proof
            hrr = validate_proof(args.hrr_proof, args.endpoint_kind, args.form, args.mode)
            report['hrr_applicability'] = hrr
        mapping = work / 'commands.json'; mapping.write_text(json.dumps(
            {'commands': commands, 'measured_psk_peer': measured, 'hrr_applicability': hrr}, indent=2) + '\n')
        env = dict(os.environ, PYTHONDONTWRITEBYTECODE='1', DOTCC_S2N_PYTHON_COMMANDS=str(mapping))
        env.update(peers.get('environment', {}))
        env['DOTCC_S2N_PIPE_READ_LIMIT'] = str(args.pipe_read_limit)
        if args.available_port_probe:
            env['DOTCC_S2N_AVAILABLE_PORT_PROBE'] = '1'
        if args.port_low is not None:
            env.update(DOTCC_S2N_PORT_LOW=str(args.port_low), DOTCC_S2N_PORT_HIGH=str(args.port_high))
        native_paths = []
        for prefix in peers.get('commands', {}).values():
            binary = shutil.which(prefix[0], path=env.get('PATH'))
            if binary is None: raise RuntimeError('Native peer executable missing: ' + prefix[0])
            native_paths.append(Path(binary).resolve())
            native_paths.extend(Path(p).resolve() for p in prefix[1:] if Path(p).is_file())
        for specification in peers.get('services', []):
            prefix = specification['command']
            binary = shutil.which(prefix[0], path=env.get('PATH'))
            if binary is None: raise RuntimeError('Declared service executable missing: ' + prefix[0])
            native_paths.append(Path(binary).resolve())
            native_paths.extend(Path(p).resolve() for p in prefix[1:] if Path(p).is_file())
        report['native_peer_sha256'] = hashes(native_paths)
        peer_configuration = [Path(value) for value in peers.get('environment', {}).values() if Path(value).is_file()]
        for value in peers.get('execution_inputs', []):
            selected = Path(value)
            if not selected.is_file(): raise RuntimeError('Declared peer execution input missing: ' + str(selected))
            peer_configuration.append(selected)
        if measured:
            peer_configuration.extend(Path(path) for path in measured['inputs_sha256'])
        if hrr:
            peer_configuration.extend(Path(path) for path in hrr['inputs_sha256'])
        report['peer_configuration_sha256'] = hashes(peer_configuration)
        fixture_paths = [p for p in stage.rglob('*') if p.is_file() and p.suffix not in ('.py', '.c', '.h')]
        report['fixture_sha256'] = hashes(fixture_paths)
        report['endpoint_closure_sha256'] = endpoint_closure
        # providers.py performs peer version discovery at import time. Point
        # those probes at the same declared CLI used for exchanges, before
        # importing the original provider module. Class-specific prefixes are
        # still resolved by the plugin when it starts an endpoint.
        peer_bin = work / 'peer-bin'; peer_bin.mkdir()
        for name, prefix in peers.get('commands', {}).items():
            if ':' in name or name in ('s2nc', 's2nd', 's2nc_head', 's2nd_head') or Path(name).name != name: continue
            executable = shutil.which(prefix[0], path=env.get('PATH')) if not Path(prefix[0]).is_absolute() else prefix[0]
            if executable is None: raise RuntimeError('Declared peer command is unavailable: ' + prefix[0])
            wrapper = peer_bin / name
            exact = [str(Path(executable).resolve()), *prefix[1:]]
            wrapper.write_text('#!' + str(Path(args.python).resolve()) + '\nimport os,sys\nos.execv(' + repr(exact[0]) + ', ' + repr(exact) + ' + sys.argv[1:])\n')
            wrapper.chmod(0o755)
        env['PATH'] = str(peer_bin) + os.pathsep + env.get('PATH', '')
        env['PYTHONPATH'] = str(frozen_adapters) + os.pathsep + env.get('PYTHONPATH', '')
        required_services = [s for s in peers.get('services', []) if s['name'] != 'apache-renegotiation' or
                             any(r['case'] == 'tests/integrationv2/test_renegotiate_apache.py' for r in report['cases'])]
        with nullcontext():
            for index, row in enumerate(report['cases']):
                row['status'] = 'failed'
                result_path = args.output.parent / (args.output.stem + f'-{index:02d}-parameters.json')
                log = args.output.parent / (args.output.stem + f'-{index:02d}.log')
                selectors = [str(stage / row['case'])]
                if args.hrr_no_retry:
                    selectors = [str(authored_control_target)]
                    row.update(case='tests/Upstream/test_hrr_no_retry.py',
                               test_functions=['test_default_curve_does_not_retry'])
                if args.node:
                    selectors = [str(stage / row['case']) + '::' + node.split('::', 1)[1]
                                 for node in args.node if node.split('::', 1)[0] == Path(row['case']).name]
                    row['selected_nodeids'] = [node for node in args.node if node.split('::', 1)[0] == Path(row['case']).name]
                argv = [args.python, '-m', 'pytest', '-p', 'pytest_adapter', '-p', 'no:cacheprovider',
                        '--provider-version', args.provider_version, '-ra', '--tb=long', *selectors]
                if args.collect_only: argv += ['--collect-only']
                elif args.workers > 1: argv += ['-n', str(args.workers)]
                if args.no_reruns: argv += ['-p', 'no:rerunfailures']
                try:
                    if hashes(endpoint_closure) != endpoint_closure: raise RuntimeError('Actual main endpoint closure changed before execution')
                    if hashes(report['frozen_adapter_sha256']) != report['frozen_adapter_sha256']:
                        raise RuntimeError('Private frozen Python adapter snapshot changed before execution')
                    if hashes(native_paths) != report['native_peer_sha256']: raise RuntimeError('Native peer identity changed before execution')
                    if hashes(peer_configuration) != report['peer_configuration_sha256']: raise RuntimeError('Native peer configuration changed before execution')
                    if hashes(fixture_paths) != report['fixture_sha256']: raise RuntimeError('Original fixture identity changed before execution')
                    row['execution_identity'] = {'endpoint_manifest_sha256': report['endpoint_manifest_sha256'],
                        'native_peer_sha256': report['native_peer_sha256'], 'fixtures': 'report.fixture_sha256'}
                    family_services = [s for s in required_services if s['name'] != 'apache-renegotiation' or
                                       row['case'] == 'tests/integrationv2/test_renegotiate_apache.py']
                    services = (nullcontext() if args.collect_only else
                        PeerServices(family_services, work / 'services' / str(index), env, report['services']))
                    with services, log.open('w') as stream:
                        process = run_and_drain(argv, cwd=stage / 'tests/integrationv2',
                            env=dict(env, DOTCC_S2N_PYTHON_REPORT=str(result_path)), stream=stream, timeout=14400)
                    row.update(command=argv, log=str(log), process=process, exit_code=process['exit_code'])
                    if hashes(endpoint_closure) != endpoint_closure: raise RuntimeError('Actual main endpoint closure changed during execution')
                    if hashes(native_paths) != report['native_peer_sha256']: raise RuntimeError('Native peer identity changed during execution')
                    if hashes(peer_configuration) != report['peer_configuration_sha256']: raise RuntimeError('Native peer configuration changed during execution')
                    if hashes(fixture_paths) != report['fixture_sha256']: raise RuntimeError('Original fixture identity changed during execution')
                    if not process['drained'] or process['remaining_endpoints']:
                        raise RuntimeError('Original pytest left endpoint processes; see process drain receipt')
                    if not result_path.is_file(): raise RuntimeError('Original pytest collection/execution failed; inspect dependency/peer transcript')
                    result = json.loads(result_path.read_text())
                    if result['collection_errors'] or not result['collected']:
                        raise RuntimeError('Original required parameter collection failed or produced no applicable cases')
                    if any(d['predicate'] is None or d['product_capability_vetoes'] for d in result['deselected']):
                        raise RuntimeError('Original collection contains unexplained or product-capability exclusions')
                    row.update(parameters=str(result_path), parameter_cases=len(result['collected']),
                               passed=result['passed'], skipped=result['skipped'], deselected=len(result['deselected']))
                    row['inapplicable'] = len(result.get('original_inapplicable', []))
                    if args.hrr_no_retry:
                        observations = result.get('hrr_no_retry_controls', [])
                        if len(result['collected']) != 9 or len(observations) != 9:
                            raise RuntimeError('Separate no-HRR controls must execute all nine exact combinations')
                    row['collection_status'] = 'passed'
                    if args.collect_only:
                        row.update(status='unrun', reason='Explicit collection-only inventory; assertions were not executed.')
                    elif process['exit_code'] == 0 and result['status'] == 'passed': row['status'] = 'passed'
                    else: raise RuntimeError('Required original parameter/assertion execution failed or skipped')
                except Exception as error:
                    row['error'] = str(error)
                checkpoint()
        observed = {p.relative_to(stage).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest() for p in stage.rglob('*.py')
                    if not (args.hrr_no_retry and p == authored_control_target)}
        if observed != report['python_source_sha256']: raise RuntimeError('Original Python test/helper source changed during execution')
        if args.hrr_no_retry and hashes(report['authored_control_sha256']) != report['authored_control_sha256']:
            raise RuntimeError('Authored no-HRR controls changed during execution')
        if report['cases'] and all(r['status'] == 'passed' for r in report['cases']): report['status'] = 'passed'
    except KeyboardInterrupt:
        report['error'] = 'Interrupted; endpoint process groups were drained by the runner finally path.'
        report['interrupted'] = True
    except Exception as error:
        report['error'] = str(error)
        for row in report['cases']:
            if row['status'] == 'unrun' and not args.collect_only: row.update(status='failed', error=str(error))
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print('REPORT ' + str(args.output))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
