#!/usr/bin/env python3
"""Record matched native recovery limitations without changing strict failures.

This is an observation classifier, not a transport qualification pass. Inputs
must bind the same current peer baseline and contain both negative scenarios
plus successful expired-mapping, probe-loss, and increasing-ceiling controls.
"""
import argparse
import importlib.util
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('recovery', ROOT / 'scripts/test-recovery.py')
recovery = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recovery)
check, sha = recovery.check, recovery.sha
NEGATIVE = ('rebinding', 'payload-ceiling-down')
POSITIVE = ('rebinding-expired-mapping', 'mtu-probe-loss', 'payload-ceiling-up')


def configuration(case):
    # Only OS-assigned destination ports differ between sequential pairs.
    return {k: v for k, v in case['proxy']['configuration'].items() if k != 'server_port'}


def ownership(peer):
    for key in ('host_resources', 'host_allocations', 'host_receive_leases',
                'host_send_errors', 'host_receive_errors', 'host_truncations'):
        check(peer[key] == 0, 'Managed ownership or I/O failure: ' + key)


def validate_case(case):
    scenario, role = case['scenario'], case['managed_role']
    check(role in ('native', 'both'), 'Classifier requires native/native or managed/managed pairs')
    # The peer sends its response only after receiving the complete request.
    # A client-direction black hole can therefore prevent the server from ever
    # reaching its configured change ordinal. Require the actual triggering
    # direction, and do not claim both ceilings changed in this negative case.
    if scenario == 'payload-ceiling-down':
        config = case['proxy']['configuration']['client_to_server']
        counts = case['proxy']['directions']['client_to_server']
        check(config['mtu_bytes'] == 1472 and config['mtu_bytes_after'] == 1300
              and config['mtu_change_after'] == 20 and counts['received_packets'] > 20
              and counts['mtu_drops'] > 0, 'Client-direction decreasing ceiling was not exercised')
    check(case['proxy_exit'] == 0, 'Proxy exit failed')
    if scenario in POSITIVE or scenario == 'rebinding':
        check(case['client_exit'] == case['server_exit'] == 0, 'Successful payload pair exited unsuccessfully')
        for endpoint in ('client', 'server'):
            peer = case[endpoint]
            recovery.validate_peer(peer, endpoint, role == 'both', case['runtime'], case['cipher'], case['family'])
            check(peer['connected'] == peer['finished'] == peer['closed'] == 1, 'Endpoint did not complete')
            check(peer['transport_status'] == peer['transport_error'] == peer['peer_error'] == 0, 'Unexpected terminal error')
    else:
        check(not case['passed'], 'Decreasing-ceiling observation unexpectedly passed; investigate instead of classifying')
        for endpoint in ('client', 'server'):
            peer = case[endpoint]
            check(not peer['passed'] and case[endpoint + '_exit'] != 0, 'Expected incomplete endpoint')
            check(peer['connected'] == peer['closed'] == 1 and peer['finished'] == 0, 'Unexpected terminal lifecycle')
            check(peer['transport_status'] == 62 and peer['transport_error'] == 1 and peer['peer_error'] == 0,
                  'Expected actual ConnectionIdle status 62 and idle transport error 1')
            check(peer['family'] == case['family'] and peer['quic_version'] == 1 and peer['group'] == 23
                  and peer['cipher'] == (0x1301 if case['cipher'] == '128' else 0x1302), 'Negotiation mismatch')
            check(0 <= peer[endpoint + '_bytes'] < 65537 and peer['statistics_status'] == 0,
                  'Expected sampled incomplete payload accounting')
            if role == 'both':
                ownership(peer)
    recovery.validate_faults(case['proxy'], 'mtu-probe-loss' if scenario == 'payload-ceiling-down' else scenario,
                            recovery.completed_server_pid(case) if scenario != 'payload-ceiling-down' else None)
    if scenario.startswith('rebinding'):
        server = case['server']
        if role == 'native':
            check(server['private_paths_snapshot'], 'Missing actual native core-header path snapshot')
        paths = [p for p in server['paths'] if p['in_use']]
        active = [p for p in paths if p['active']]
        check(len(active) == 1, 'Ambiguous active path')
        check(active[0]['remote_port'] == server['remote_port'] == case['proxy']['backend_ports'][-1],
              'Active path is not the rebound source port')
        if scenario == 'rebinding':
            check(not case['passed'] and case['error'] == 'Server did not validate the new source port',
                  'Original strict failure was lost or changed')
            check(case['post_exchange_settle_ms'] == 2000, 'Different path validation observation interval')
            old = [p for p in paths if not p['active']]
            check(not active[0]['peer_validated'] and len(old) == 1 and old[0]['peer_validated']
                  and old[0]['remote_port'] == case['proxy']['backend_ports'][0], 'Different native path outcome')
            check(all(p['send_challenge'] == p['send_response'] == 0 for p in paths)
                  and server['paths_validated'] == 1 and server['path_failures'] == 0, 'Different path counters or pending probes')
            check(not case['proxy']['configuration']['retire_old_backend_on_rebind'], 'Dual-live mapping was not tested')
        else:
            check(case['passed'] and active[0]['peer_validated'] and case['new_server_path_validated'],
                  'Expired-mapping positive control did not validate the new path')
    if scenario in POSITIVE:
        check(case['passed'], 'Required positive control failed: ' + scenario)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('receipts', type=Path, nargs='+')
    parser.add_argument('--peer-receipt', type=Path, default=ROOT / 'artifacts/managed-peer/results.json')
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/recovery-native-observations/results.json')
    args = parser.parse_args()
    output = dict(strict_passed=False, observation_validated=False, entire_p7_qualified=False,
                  inputs={}, comparisons=[], source_sha256=sha(Path(__file__)),
                  limitations=['Successful payload delivery does not establish new-path validation.',
                               'Matching idle failures do not establish recovery from a decreasing payload ceiling.',
                               'Partial byte counts, packet counts, MTU discovery progress and timings may differ.'])
    try:
        baseline = json.loads(args.peer_receipt.read_text())
        baseline_sha = sha(args.peer_receipt)
        output['peer_receipt'] = dict(path=str(args.peer_receipt.resolve()), sha256=baseline_sha)
        cases = {}
        for path in args.receipts:
            receipt = json.loads(path.read_text())
            output['inputs'][str(path.resolve())] = sha(path)
            check(receipt['peer_receipt']['sha256'] == baseline_sha, 'Mixed or stale peer baselines: ' + str(path))
            check(receipt['binary_hashes'] == baseline['binary_hashes'], 'Peer executable bindings differ')
            check(receipt['certificate_sha256'] == baseline['certificate_sha256']['ecdsa.pem'], 'Certificate differs')
            recovery.bind_files(receipt['source_hashes'], 'Recovery driver/proxy')
            for case in receipt['cases']:
                if case['scenario'] not in NEGATIVE + POSITIVE or case['managed_role'] not in ('native', 'both'):
                    continue
                key = tuple(case[k] for k in ('variant', 'runtime', 'family', 'cipher', 'scenario', 'managed_role'))
                check(key not in cases, 'Ambiguous duplicate observation: ' + str(key))
                validate_case(case)
                cases[key] = case
        check(bool(cases), 'No observations')
        recovery.validate_baseline(baseline, sorted({key[0] for key in cases}))
        profiles = sorted({key[:4] for key in cases})
        for profile in profiles:
            for scenario in NEGATIVE + POSITIVE:
                keys = [(*profile, scenario, role) for role in ('native', 'both')]
                check(all(key in cases for key in keys), 'Missing matched scenario/control: ' + str((*profile, scenario)))
                native, managed = (cases[key] for key in keys)
                check(configuration(native) == configuration(managed), 'Fault configuration differs between implementations')
                output['comparisons'].append(dict(variant=profile[0], runtime=profile[1], family=profile[2], cipher=profile[3],
                    scenario=scenario, strict_passed=scenario in POSITIVE,
                    native_case=native['name'], managed_case=managed['name'],
                    outcome='positive control passed' if scenario in POSITIVE else
                        ('payload and FIN complete; rebound active path remains unvalidated' if scenario == 'rebinding' else
                         'client-direction ceiling decreases; incomplete transfer; both endpoints close with ConnectionIdle 62 / transport error 1')))
        check(sha(args.peer_receipt) == baseline_sha, 'Peer receipt changed during classification')
        for path, expected in output['inputs'].items():
            check(sha(Path(path)) == expected, 'Recovery receipt changed during classification')
        output['observation_validated'] = True
    except Exception as error:
        output['error'] = str(error)
        raise
    finally:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(output, indent=2) + '\n')
    print(json.dumps({key: output[key] for key in ('strict_passed', 'observation_validated', 'entire_p7_qualified')}))


if __name__ == '__main__':
    main()
