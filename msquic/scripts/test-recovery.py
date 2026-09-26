#!/usr/bin/env python3
"""Run existing qualified transport peers through the test-only UDP fault proxy.

This qualifies packet delivery under the selected faults, not the other P7
feature gates. Build and qualify current peers with test-managed-peer.py first.
Known upstream recovery limitations warn and continue; --strict-recovery retains
the original fail-fast qualification behavior.
"""
import argparse
import errno
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import resource
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
BUILD = ROOT / 'build/managed-peer'
PROXY = ROOT / 'tests/UdpFaultProxy/proxy.py'
DIRECTIONS = ('client_to_server', 'server_to_client')
SCENARIOS = {
    'baseline': {},
    'handshake-loss': {'client_to_server': {'drop_first': 1}},
    'loss': {direction: {'drop_every': 11} for direction in DIRECTIONS},
    'reordering': {direction: {'reorder_every': 5, 'reorder_hold_ms': 25} for direction in DIRECTIONS},
    'duplication': {direction: {'duplicate_every': 7} for direction in DIRECTIONS},
    'delay': {direction: {'delay_ms': 5, 'jitter_ms': 3} for direction in DIRECTIONS},
    'combined': {direction: {'drop_every': 13, 'reorder_every': 7,
                            'duplicate_every': 11, 'delay_ms': 2, 'jitter_ms': 3}
                 for direction in DIRECTIONS},
    'rebinding': {'rebind_after_client_packets': 12},
    'rebinding-expired-mapping': {'rebind_after_client_packets': 12, 'retire_old_backend_on_rebind': True},
    'mtu-probe-loss': {direction: {'mtu_bytes': 1300} for direction in DIRECTIONS},
    'payload-ceiling-up': {direction: {'mtu_bytes': 1300, 'mtu_change_after_drops': 1,
                                     'mtu_bytes_after': 1472} for direction in DIRECTIONS},
    'payload-ceiling-down': {direction: {'mtu_bytes': 1472, 'mtu_change_after': 20,
                                       'mtu_bytes_after': 1300} for direction in DIRECTIONS},
}
CLASSIFIER = ROOT / 'scripts/classify-native-recovery.py'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def check(condition, message):
    if not condition:
        raise RuntimeError(message)


def bind_files(record, name):
    check(isinstance(record, dict) and bool(record), name + ' has no file hashes')
    for relative, expected in record.items():
        path = REPO / relative
        check(path.is_file() and sha(path) == expected, name + ' changed: ' + relative)


def validate_baseline(baseline, variants):
    check(baseline.get('passed'), 'Current baseline peer matrix is not validated')
    bind_files(baseline.get('source_hashes'), 'Baseline source')
    bind_files(baseline.get('binary_hashes'), 'Baseline executable')
    check(baseline['closure_sha256'] == sha(ROOT / 'config/product-closure.json'), 'Baseline closure changed')
    for variant in variants:
        directories = (
            ('generated_hashes', ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')),
            ('picotls_generated_hashes', REPO / 'picotls/generated' /
                ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')),
        )
        for field, directory in directories:
            current = {path.name: sha(path) for path in sorted(directory.glob('*.cs'))}
            check(bool(current) and baseline[field][variant] == current, 'Baseline generated sources changed: ' + field + ' ' + variant)


def read_peer(path):
    rows = [json.loads(line) for line in path.read_text().splitlines() if line.startswith('{"passed":')]
    check(len(rows) == 1, 'Missing or ambiguous peer result: ' + str(path))
    return rows[0]


def wait_ready(process, path, as_json=False):
    deadline = time.monotonic() + 20
    while time.monotonic() < deadline:
        check(process.poll() is None, 'Process exited before readiness: ' + str(path))
        if path.exists() and path.read_text().strip():
            value = json.loads(path.read_text())['port'] if as_json else int(path.read_text())
            check(type(value) is int and 0 < value < 65536, 'Invalid ready port')
            return value
        time.sleep(0.01)
    raise TimeoutError('Readiness timed out: ' + str(path))


def validate_peer(result, role, managed, runtime, cipher, family):
    check(result['passed'] and result['family'] == family, role + ' failed')
    check(result['cipher'] == (0x1301 if cipher == '128' else 0x1302)
          and result['group'] == 23 and result['quic_version'] == 1, role + ' negotiation mismatch')
    check(result[role + '_bytes'] == 65537, role + ' payload mismatch')
    check(result['certificate_validation'] == (role == 'client'), role + ' authentication mismatch')
    if managed:
        check(result['listener_preflight'] and result['sent_bytes'] == 65537
              and result['send_completions'] == 1, role + ' send lifetime mismatch')
        check(result['connected'] == result['finished'] == result['closed'] == 1,
              role + ' lifecycle mismatch')
        check(result['transport_status'] == result['transport_error'] == result['peer_error'] == 0,
              role + ' transport error')
        check(result['aot'] == (runtime == 'aot'), role + ' runtime mismatch')
        # Native Stats.Send/Recv.TotalStreamBytes count encoded/received STREAM
        # frame bytes, including retransmissions. Exact unique bytes are checked
        # by the application pattern and FIN assertions above.
        check(result['statistics_status'] == 0 and result['core_sent_stream_bytes'] >= 65537
              and result['core_received_stream_bytes'] >= 65537, role + ' core stream accounting mismatch')
        check(result['host_resources'] == result['host_allocations'] == result['host_receive_leases'] == 0,
              role + ' host ownership did not drain')
        check(result['host_send_errors'] == result['host_receive_errors'] == result['host_truncations'] == 0,
              role + ' host I/O failed')


def completed_server_pid(case):
    """Only a successfully validated endpoint can support teardown classification."""
    check(case['server_exit'] == 0, 'Server did not exit successfully')
    server = case['server']
    validate_peer(server, 'server', case['managed_role'] in ('server', 'both'),
                  case['runtime'], case['cipher'], case['family'])
    check(server['connected'] == server['finished'] == server['closed'] == 1
          and server['transport_status'] == server['transport_error'] == server['peer_error'] == 0,
          'Server did not complete payload/FIN and close successfully')
    check(type(case['server_pid']) is int and case['server_pid'] > 0, 'Missing actual server process identity')
    return case['server_pid']


def validate_io_errors(stats, completed_pid=None):
    """Retain raw errors; only classify refusals observed after successful server exit.

    This establishes when the error was observed, not when an ICMP was generated.
    Live-process errors, unknown lifetime, other errors and missing events fail.
    """
    events = stats['io_errors']
    check(stats['io_errors_omitted'] == 0 and len(events) <= 64, 'Incomplete proxy I/O evidence')
    counts = {(direction, operation): 0 for direction in DIRECTIONS for operation in ('send', 'receive')}
    for event in events:
        pair = (event['direction'], event['operation'])
        check(pair in counts, 'Invalid proxy I/O event')
        counts[pair] += 1
        check(completed_pid is not None
              and stats['server_process'] == dict(pid=completed_pid, observation='pidfd')
              and event['server_process_exited'] is True
              and event['errno'] == errno.ECONNREFUSED
              and pair in (('client_to_server', 'send'), ('server_to_client', 'receive'))
              and event['local_port'] in stats['backend_ports']
              and type(event['monotonic_ns']) is int and event['monotonic_ns'] > 0,
              'Unexpected proxy I/O error: ' + json.dumps(event, sort_keys=True))
    for (direction, operation), count in counts.items():
        check(stats['directions'][direction][operation + '_errors'] == count,
              'Proxy raw I/O counts disagree with event evidence')
    return dict(classification='ECONNREFUSED observed after successful server process exit',
                count=len(events), server_pid=completed_pid)


def validate_faults(stats, scenario, completed_pid=None):
    check(stats['outcome'] == 'stopped', 'Proxy failed')
    check(stats['source_sha256'] == sha(PROXY), 'Proxy executable source changed')
    check(stats['remaining_queue_packets'] == stats['remaining_queue_bytes'] == 0, 'Proxy queue leaked')
    check(stats['peak_queue_packets'] <= stats['configuration']['max_queue_packets']
          and stats['peak_queue_bytes'] <= stats['configuration']['max_queue_bytes'], 'Queue bounds exceeded')
    counts = stats['directions']
    for direction in DIRECTIONS:
        row = counts[direction]
        check(row['forwarded_packets'] > 0, 'No traffic: ' + direction)
        for field in ('queue_drops', 'truncated_drops', 'foreign_drops'):
            check(row[field] == 0, 'Unexpected proxy ' + direction + ' ' + field)
    io_observation = validate_io_errors(stats, completed_pid)
    total = lambda field: sum(counts[direction][field] for direction in DIRECTIONS)
    required = {
        'handshake-loss': ('rule_drops',), 'loss': ('rule_drops',),
        'reordering': ('forwarded_out_of_order',), 'duplication': ('duplicates_forwarded',),
        'delay': ('delayed_scheduled',),
        'combined': ('rule_drops', 'forwarded_out_of_order', 'duplicates_forwarded', 'delayed_scheduled'),
        'mtu-probe-loss': ('mtu_drops',), 'payload-ceiling-up': ('mtu_drops',), 'payload-ceiling-down': ('mtu_drops',),
    }
    for counter in required.get(scenario, ()):
        check(total(counter) > 0, 'Configured fault did not occur: ' + scenario + ' ' + counter)
    if scenario in ('payload-ceiling-up', 'payload-ceiling-down'):
        for direction in DIRECTIONS:
            check(counts[direction]['mtu_packets_before_change'] > 0
                  and counts[direction]['mtu_packets_after_change'] > 0,
                  'Traffic ended before the payload ceiling changed')
            if scenario == 'payload-ceiling-up':
                check(counts[direction]['mtu_drops'] >= stats['configuration'][direction]['mtu_change_after_drops'] > 0,
                      'Increasing ceiling did not follow an actual oversized-packet drop')
    if scenario == 'handshake-loss':
        check(counts['client_to_server']['rule_drops'] == 1, 'Initial loss count mismatch')
    if scenario.startswith('rebinding'):
        check(len(stats['rebindings']) == 1 and len(set(stats['backend_ports'])) == 2,
              'Source port did not change exactly once')
        if scenario == 'rebinding-expired-mapping':
            check(stats['configuration']['retire_old_backend_on_rebind'], 'Old mapping expiry not configured')
            check(counts['server_to_client']['retired_mapping_drops'] > 0, 'No replies exercised the expired old mapping')
    else:
        check(not stats['rebindings'], 'Unexpected source port change')
    if scenario == 'baseline':
        check(total('rule_drops') == total('mtu_drops') == total('duplicates_forwarded') == 0,
              'Baseline introduced a fault')
    return io_observation


def exchange(args, receipt, variant, runtime, role, family, cipher, scenario):
    name = '-'.join((variant, runtime, role, family, cipher, scenario))
    directory = args.output / name
    directory.mkdir(parents=True, exist_ok=True)
    ready, proxy_ready, stats_path = (directory / filename for filename in ('server.ready', 'proxy.ready.json', 'proxy.stats.json'))
    barrier = directory / 'proxy-drain'
    barrier_ready, barrier_release = Path(str(barrier) + '.ready'), Path(str(barrier) + '.release')
    client_transfer, server_transfer, transfer_release = (Path(str(barrier) + suffix) for suffix in
        ('.client.transfer-ready', '.server.transfer-ready', '.transfer-release'))
    for path in (ready, proxy_ready, stats_path, barrier_ready, barrier_release,
                 client_transfer, server_transfer, transfer_release):
        path.unlink(missing_ok=True)
    certificate, key = BUILD / 'ecdsa.pem', BUILD / 'ecdsa.key'
    managed = (['dotnet', str(BUILD / variant / 'bin/Release/net10.0/ManagedPeer.dll')]
               if runtime == 'jit' else [str(BUILD / variant / 'aot/ManagedPeer')])
    processes = []
    proxy = None
    case = dict(name=name, variant=variant, runtime=runtime, managed_role=role,
                family=family, cipher=cipher, scenario=scenario, passed=False,
                post_exchange_settle_ms=2000 if scenario.startswith('rebinding') else 0)
    receipt['cases'].append(case)
    started = time.monotonic()

    def peer_command(peer_role, port):
        executable = managed if role in (peer_role, 'both') else [str(BUILD / 'native-peer')]
        return [*executable, str(certificate), str(key), cipher, family, str(certificate),
                'localhost', peer_role, str(port), str(ready)]

    def start(command, log, environment):
        case.setdefault('commands', []).append(command)
        process = subprocess.Popen(command, stdout=log, stderr=subprocess.STDOUT, env=environment)
        processes.append(process)
        return process

    try:
        with tempfile.TemporaryDirectory(prefix='dotcc-recovery-') as isolated_tmp:
            environment = dict(os.environ, TMPDIR=isolated_tmp, OPENSSL_CONF=str(BUILD / 'p256.cnf'),
                               SSL_CERT_FILE=str(certificate), DOTCC_PEER_SETTLE_MS=str(case['post_exchange_settle_ms']),
                               DOTCC_PEER_SHARE_UDP_BINDING='0', DOTCC_PEER_PROXY_DRAIN=str(barrier))
            environment.pop('SSLKEYLOGFILE', None)
            with (directory / 'server.log').open('w') as server_log, \
                    (directory / 'client.log').open('w') as client_log, \
                    (directory / 'proxy.log').open('w') as proxy_log:
                server = start(peer_command('server', 0), server_log, environment)
                case['server_pid'] = server.pid
                server_port = wait_ready(server, ready)
                config = dict(family=family, server_port=server_port, seed=args.seed,
                              max_queue_packets=1024, max_queue_bytes=8 * 1024 * 1024,
                              **SCENARIOS[scenario])
                config_path = directory / 'proxy.config.json'
                config_path.write_text(json.dumps(config, indent=2) + '\n')
                proxy = start([sys.executable, str(PROXY), '--config', str(config_path),
                               '--ready', str(proxy_ready), '--stats', str(stats_path),
                               '--server-pid', str(server.pid)], proxy_log, environment)
                proxy_port = wait_ready(proxy, proxy_ready, as_json=True)
                client = start(peer_command('client', proxy_port), client_log, environment)
                deadline = time.monotonic() + 60
                case['both_stream_fin_acknowledged_before_close'] = False
                while time.monotonic() < deadline:
                    if (client_transfer.exists() and client_transfer.read_text().strip() == str(client.pid)
                            and server_transfer.exists() and server_transfer.read_text().strip() == str(server.pid)):
                        release_tmp = transfer_release.with_suffix('.tmp')
                        release_tmp.write_text('streams-acknowledged\n')
                        release_tmp.replace(transfer_release)
                        case['both_stream_fin_acknowledged_before_close'] = True
                        break
                    if (client.poll() is not None or server.poll() is not None
                            or (barrier_ready.exists() and barrier_ready.read_text().strip() == str(server.pid))):
                        # Preserve genuine incomplete/idle outcomes for strict
                        # negative observations; never release a partial stream.
                        break
                    time.sleep(0.01)
                else:
                    raise TimeoutError('Recovery stream FIN acknowledgment barrier timed out')
                case['client_exit'] = client.wait(timeout=60)
                deadline = time.monotonic() + 20
                while not (barrier_ready.exists() and barrier_ready.read_text().strip() == str(server.pid)):
                    check(server.poll() is None, 'Server exited before proxy drain barrier')
                    check(time.monotonic() < deadline, 'Server did not reach proxy drain barrier')
                    time.sleep(0.01)
                # The server has observed connection shutdown and retains its
                # listener binding until the proxy has flushed/stopped. Delayed
                # duplicates must not be sent to a deliberately closed socket.
                time.sleep(0.1)
                proxy.terminate()
                case['proxy_exit'] = proxy.wait(timeout=5)
                check(case['proxy_exit'] == 0, 'Proxy failed before server cleanup release')
                release_tmp = barrier_release.with_suffix('.tmp')
                release_tmp.write_text('proxy-stopped\n')
                release_tmp.replace(barrier_release)
                case['server_exit'] = server.wait(timeout=60)
                case['proxy_stopped_before_server_cleanup'] = True
            for peer_role in ('client', 'server'):
                result = read_peer(directory / (peer_role + '.log'))
                case[peer_role] = result
            stats = json.loads(stats_path.read_text())
            case['proxy'] = stats
            check(case['client_exit'] == case['server_exit'] == case['proxy_exit'] == 0,
                  'Peer or proxy exit failed: ' + name)
            check(case['both_stream_fin_acknowledged_before_close'], 'Endpoints closed before both stream FIN acknowledgments')
            for peer_role in ('client', 'server'):
                validate_peer(case[peer_role], peer_role, role in (peer_role, 'both'), runtime, cipher, family)
                check(case[peer_role]['send_fin_acknowledged'], peer_role + ' lacks actual send FIN acknowledgment')
                if role in (peer_role, 'both'):
                    check(case[peer_role].get('share_udp_binding_requested') is False,
                          'Recovery peer differs from the ordinary baseline binding profile')
                    if peer_role == 'client':
                        check(case[peer_role].get('share_udp_binding') is False,
                              'Recovery client unexpectedly enabled shared binding')
            if role in ('client', 'both'):
                check(case['client']['settle_ms'] == case['post_exchange_settle_ms'], 'Client did not apply the requested validation interval')
            case['proxy_io_observation'] = validate_faults(stats, scenario, completed_server_pid(case))
            if scenario.startswith('rebinding'):
                server_result = case['server']
                active = [path for path in server_result.get('paths', []) if path['in_use'] and path['active']]
                check(len(active) == 1, 'Missing or ambiguous actual server active-path snapshot')
                if role not in ('server', 'both'):
                    check(server_result.get('private_paths_snapshot'), 'Native server lacks exact core-header path diagnostics')
                check(server_result['remote_port'] == active[0]['remote_port'] == stats['backend_ports'][-1]
                      and active[0]['peer_validated'], 'Server did not validate the new source port')
                if role in ('server', 'both'):
                    check(server_result['active_path_validated'], 'Managed active-path summaries disagree')
                case['new_server_path_validated'] = True
            case['passed'] = True
            print(name + ': PASS', flush=True)
    except BaseException as error:
        case['error'] = str(error)
        raise
    finally:
        for process in reversed(processes):
            if process.poll() is None:
                if process is proxy:
                    process.terminate()
                    try:
                        process.wait(timeout=5)
                    except subprocess.TimeoutExpired:
                        process.kill()
                else:
                    process.kill()
            process.wait()
        case['elapsed_seconds'] = time.monotonic() - started
        if stats_path.exists() and 'proxy' not in case:
            case['proxy'] = json.loads(stats_path.read_text())


def known_upstream_outcome(case):
    """Recognize bounded, evidenced outcomes, never arbitrary scenario failures."""
    if case['scenario'] == 'rebinding':
        if case.get('error') != 'Server did not validate the new source port':
            return None
    elif case['scenario'] == 'payload-ceiling-down':
        if case.get('error') != 'Peer or proxy exit failed: ' + case['name']:
            return None
    else:
        return None
    spec = importlib.util.spec_from_file_location('native_recovery_observations', CLASSIFIER)
    classifier = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(classifier)
    try:
        return classifier.validate_case(case, allow_mixed=True)
    except (RuntimeError, KeyError, TypeError, ValueError, OSError):
        # Missing evidence, different terminal states, resource leaks, proxy
        # failures and other regressions retain the original fatal failure.
        return None


def run_case(args, receipt, variant, runtime, role, family, cipher, scenario):
    first = len(receipt['cases'])
    try:
        exchange(args, receipt, variant, runtime, role, family, cipher, scenario)
    except RuntimeError:
        if args.strict_recovery or len(receipt['cases']) != first + 1:
            raise
        case = receipt['cases'][-1]
        observation = known_upstream_outcome(case)
        if observation is None:
            raise
        message = (f"[test] {case['name']}: {case['error']}. "
                   "These tests are timing dependent and these outcomes are also observable in upstream MsQuic. "
                   f"Known outcome: {observation['kind']}; continuing verification. "
                   "This does not count as successful recovery.")
        case['known_upstream_observation'] = observation
        case['warning'] = message
        receipt.setdefault('warnings', []).append(message)
        print('WARNING: ' + message, file=sys.stderr, flush=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
    runtime_selection = parser.add_mutually_exclusive_group()
    runtime_selection.add_argument('--jit-only', action='store_true')
    runtime_selection.add_argument('--runtimes', nargs='+', choices=['jit', 'aot'],
                                   help='Select runtimes, including isolated strict NativeAOT observations')
    parser.add_argument('--roles', nargs='+', choices=['both', 'client', 'server', 'native'], default=['both', 'client', 'server'],
                        help='native selects an additional test-only native-to-native control')
    parser.add_argument('--families', nargs='+', choices=['ipv4', 'ipv6'], default=['ipv4', 'ipv6'])
    parser.add_argument('--ciphers', nargs='+', choices=['128', '256'], default=['128', '256'])
    parser.add_argument('--scenarios', nargs='+', choices=SCENARIOS, default=list(SCENARIOS))
    parser.add_argument('--seed', type=int, default=7381)
    parser.add_argument('--strict-recovery', action='store_true',
                        help='Fail instead of warning for recognized upstream recovery limitations')
    parser.add_argument('--peer-receipt', type=Path, default=ROOT / 'artifacts/managed-peer/results.json')
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/recovery')
    args = parser.parse_args()
    args.output = args.output.resolve()
    args.output.mkdir(parents=True, exist_ok=True)
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    if hasattr(os, 'sched_getaffinity'):
        os.sched_setaffinity(0, sorted(os.sched_getaffinity(0))[:4])
    receipt = dict(passed=False, targeted_passed=False, verification_passed=False,
                   phase='P7 packet recovery subset', strict_recovery=args.strict_recovery,
                   entire_p7_qualified=False, cases=[], warnings=[], source_hashes={
                       str(path.relative_to(REPO)): sha(path) for path in (Path(__file__), PROXY, CLASSIFIER)})
    runtimes = args.runtimes or (['jit'] if args.jit_only else ['jit', 'aot'])
    if len(runtimes) != len(set(runtimes)):
        parser.error('Runtimes must be unique')
    try:
        baseline = json.loads(args.peer_receipt.read_text())
        validate_baseline(baseline, args.variants)
        receipt['peer_receipt'] = dict(path=str(args.peer_receipt), sha256=sha(args.peer_receipt))
        receipt['binary_hashes'] = baseline['binary_hashes']
        receipt['certificate_sha256'] = sha(BUILD / 'ecdsa.pem')
        check(baseline['certificate_sha256']['ecdsa.pem'] == receipt['certificate_sha256'], 'Baseline certificate changed')
        for variant in args.variants:
            for runtime in runtimes:
                for role in args.roles:
                    for family in args.families:
                        for cipher in args.ciphers:
                            check(role == 'native' or any(case.get('passed') and case['variant'] == variant and case['runtime'] == runtime
                                      and case['managed_role'] == role and case['family'] == family
                                      and case['cipher'] == cipher and case['certificate'] == 'ecdsa'
                                      for case in baseline['cases']),
                                  'Baseline lacks selected variant/runtime/role/family/cipher')
                            for scenario in args.scenarios:
                                run_case(args, receipt, variant, runtime, role, family, cipher, scenario)
        validate_baseline(baseline, args.variants)
        bind_files(receipt['source_hashes'], 'Recovery source')
        check(sha(args.peer_receipt) == receipt['peer_receipt']['sha256'], 'Baseline receipt changed during recovery')
        full = (set(args.variants) == {'raw', 'optimized'} and set(runtimes) == {'jit', 'aot'}
                and set(args.roles) == {'both', 'client', 'server'} and set(args.families) == {'ipv4', 'ipv6'}
                and set(args.ciphers) == {'128', '256'} and set(args.scenarios) == set(SCENARIOS))
        warned = sum('warning' in case for case in receipt['cases'])
        receipt.update(passed=full and not warned, targeted_passed=not full and not warned,
                       verification_passed=True, cases_passed=sum(case['passed'] for case in receipt['cases']),
                       cases_warned=warned)
    except BaseException as error:
        receipt['error'] = str(error)
        raise
    finally:
        (args.output / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps({key: receipt[key] for key in ('passed', 'targeted_passed', 'verification_passed',
                                                  'cases_passed', 'cases_warned', 'entire_p7_qualified')}))


if __name__ == '__main__':
    main()
