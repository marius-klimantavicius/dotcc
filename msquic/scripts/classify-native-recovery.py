#!/usr/bin/env python3
"""Retain repeated native recovery successes and failures without relabeling them.

This is an observation classifier, not a transport qualification pass. Inputs
must bind the same current peer baseline and contain five independent rebinding
observations per role/profile, one decreasing-ceiling observation, and successful
expired-mapping, probe-loss, and increasing-ceiling controls. Native successes
are retained; every managed failure needs a matching native failure witness.
"""
import argparse
import importlib.util
import hashlib
import itertools
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('recovery', ROOT / 'scripts/test-recovery.py')
recovery = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recovery)
check, sha = recovery.check, recovery.sha
NEGATIVE = ('rebinding', 'payload-ceiling-down')
POSITIVE = ('rebinding-expired-mapping', 'mtu-probe-loss', 'payload-ceiling-up')
REBINDING_REPETITIONS = 5


def canonical_hash(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(',', ':')).encode()).hexdigest()


def validate_receipt(receipt, baseline, baseline_sha):
    check(receipt['peer_receipt']['sha256'] == baseline_sha, 'Mixed or stale peer baselines')
    check(receipt['binary_hashes'] == baseline['binary_hashes'], 'Peer executable bindings differ')
    check(receipt['certificate_sha256'] == baseline['certificate_sha256']['ecdsa.pem'], 'Certificate differs')
    recovery.bind_files(receipt['source_hashes'], 'Recovery driver/proxy')


def execution_identity(case):
    commands = [command for command in case['commands'] if '--stats' in command]
    check(len(commands) == 1, 'Missing or duplicate proxy execution command')
    command = commands[0]
    stats = Path(command[command.index('--stats') + 1]).resolve()
    check(stats.is_file() and json.loads(stats.read_text()) == case['proxy'], 'Actual proxy receipt differs from case evidence')
    # Both the output identity and the complete kernel-run observation must be
    # unique: copying a receipt or its directory cannot manufacture a repeat.
    return str(stats), canonical_hash(dict(proxy=case['proxy'], server_pid=case['server_pid']))


def configuration(case):
    # Only OS-assigned destination ports differ between sequential pairs.
    return {k: v for k, v in case['proxy']['configuration'].items() if k != 'server_port'}


def ownership(peer):
    for key in ('host_resources', 'host_allocations', 'host_receive_leases',
                'host_send_errors', 'host_receive_errors', 'host_truncations'):
        check(peer[key] == 0, 'Managed ownership or I/O failure: ' + key)


def validate_harness_deadline(case):
    check(case['managed_role'] == 'both', 'Native deadline lacks an explicit diagnostic marker; investigate')
    check(not case['both_stream_fin_acknowledged_before_close'] and case['elapsed_seconds'] >= 15,
          'Missing actual incomplete FIN deadline interval')
    stats, _ = execution_identity(case)
    evidence = {}
    for role in ('client', 'server'):
        check(not case[role]['send_fin_acknowledged'], 'Deadline case unexpectedly acknowledged its send FIN')
        path = Path(stats).parent / (role + '.log')
        lines = path.read_text().splitlines()
        results = [json.loads(line) for line in lines if line.startswith('{')]
        check(results == [case[role]], 'Deadline endpoint log differs from its strict receipt')
        marker = 'client payload FIN timed out'
        check(lines.count(marker) == (1 if role == 'client' else 0), 'Missing or unexpected explicit client FIN deadline marker')
        required = ('Connection/FIN/shutdown lifecycle incomplete', 'Bidirectional payload/send completion totals mismatch')
        check(all(lines.count(line) == 1 for line in required), 'Incomplete lifecycle evidence missing')
        allowed_prefixes = ('host_diagnostics ', 'connection_statistics ', 'callback_sequence=', '{')
        check(all(line in required or line == marker or line.startswith(allowed_prefixes) for line in lines),
              'Unrecognized deadline error/exception; cannot classify')
        callbacks = [line for line in lines if line.startswith('callback_sequence=')]
        check(len(callbacks) == 1 and 'connection:QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE' in callbacks[0],
              'Deadline cleanup callback evidence missing')
        check('SHUTDOWN_INITIATED_BY_TRANSPORT' not in callbacks[0], 'Deadline case contains a transport shutdown')
        check(('SHUTDOWN_INITIATED_BY_PEER' in callbacks[0]) == (role == 'server'),
              'Expected client local cleanup and server peer-initiated shutdown')
        evidence[str(path)] = sha(path)
    return evidence


def validate_case(case, *, allow_mixed=False):
    scenario, role = case['scenario'], case['managed_role']
    result = dict(kind='strict_success' if case['passed'] else 'rebinding_new_path_unvalidated', evidence_files={})
    check(role in (('native', 'both', 'client', 'server') if allow_mixed else ('native', 'both')),
          'Classifier requires native/native or managed/managed pairs')
    check(case['proxy_stopped_before_server_cleanup'], 'Missing proxy/endpoint cleanup barrier')
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
        check(case['both_stream_fin_acknowledged_before_close'], 'Missing pre-close stream FIN acknowledgment barrier')
        for endpoint in ('client', 'server'):
            peer = case[endpoint]
            recovery.validate_peer(peer, endpoint, role in ('both', endpoint), case['runtime'], case['cipher'], case['family'])
            check(peer['send_fin_acknowledged'], 'Endpoint did not acknowledge its send FIN')
            check(peer['connected'] == peer['finished'] == peer['closed'] == 1, 'Endpoint did not complete')
            check(peer['transport_status'] == peer['transport_error'] == peer['peer_error'] == 0, 'Unexpected terminal error')
    else:
        check(not case['passed'], 'Decreasing-ceiling observation unexpectedly passed; investigate instead of classifying')
        terminal = [(case[e]['transport_status'], case[e]['transport_error'], case[e]['peer_error']) for e in ('client', 'server')]
        if terminal == [(62, 1, 0), (62, 1, 0)]:
            result['kind'] = 'transport_idle_62_1'
        elif terminal == [(0, 0, 0), (0, 0, 0)]:
            result['kind'] = 'managed_harness_fin_deadline'
            result['evidence_files'] = validate_harness_deadline(case)
        else:
            check(False, 'Expected actual ConnectionIdle 62/1 or explicitly evidenced harness FIN deadline')
        for endpoint in ('client', 'server'):
            peer = case[endpoint]
            check(not peer['passed'] and case[endpoint + '_exit'] == 1, 'Expected bounded incomplete endpoint, not a crash')
            check(peer['connected'] == peer['closed'] == 1 and peer['finished'] == 0, 'Unexpected terminal lifecycle')
            check(peer['family'] == case['family'] and peer['quic_version'] == 1 and peer['group'] == 23
                  and peer['cipher'] == (0x1301 if case['cipher'] == '128' else 0x1302), 'Negotiation mismatch')
            check(0 <= peer[endpoint + '_bytes'] < 65537 and peer['statistics_status'] == 0,
                  'Expected sampled incomplete payload accounting')
            if role in ('both', endpoint):
                ownership(peer)
    recovery.validate_faults(case['proxy'], 'mtu-probe-loss' if scenario == 'payload-ceiling-down' else scenario,
                            recovery.completed_server_pid(case) if scenario != 'payload-ceiling-down' else None)
    if scenario.startswith('rebinding'):
        server = case['server']
        if role not in ('both', 'server'):
            check(server['private_paths_snapshot'], 'Missing actual native core-header path snapshot')
        paths = [p for p in server['paths'] if p['in_use']]
        active = [p for p in paths if p['active']]
        check(len(active) == 1, 'Ambiguous active path')
        check(active[0]['remote_port'] == server['remote_port'] == case['proxy']['backend_ports'][-1],
              'Active path is not the rebound source port')
        if scenario == 'rebinding':
            check(case['post_exchange_settle_ms'] == 2000, 'Different path validation observation interval')
            check(not case['proxy']['configuration']['retire_old_backend_on_rebind'], 'Dual-live mapping was not tested')
            if case['passed']:
                check(not case.get('error') and active[0]['peer_validated'] and case['new_server_path_validated'],
                      'Strict success lacks actual new-path validation')
                # connection.c:6379 increments PATH_FAILURE for any timed-out
                # unvalidated path before removing it. An old path can expire
                # while the new active path is validated and remains usable.
                # This aggregate counter is not the active path's outcome.
                check(server['paths_validated'] >= 1 and isinstance(server['path_failures'], int)
                      and server['path_failures'] >= 0,
                      'Unexpected successful path counters')
            else:
                check(case['error'] == 'Server did not validate the new source port',
                      'Original strict failure was lost or changed')
                old = [p for p in paths if not p['active']]
                check(not case.get('new_server_path_validated') and not active[0]['peer_validated']
                      and len(old) == 1 and old[0]['peer_validated']
                      and old[0]['remote_port'] == case['proxy']['backend_ports'][0], 'Different native path outcome')
                check(all(p['send_challenge'] == p['send_response'] == 0 for p in paths)
                      and server['paths_validated'] == 1 and server['path_failures'] == 0, 'Different path counters or pending probes')
        else:
            check(case['passed'] and active[0]['peer_validated'] and case['new_server_path_validated'],
                  'Expired-mapping positive control did not validate the new path')
    if scenario in POSITIVE:
        check(case['passed'], 'Required positive control failed: ' + scenario)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('receipts', type=Path, nargs='+')
    parser.add_argument('--peer-receipt', type=Path, default=ROOT / 'artifacts/managed-peer/results.json')
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/recovery-native-observations/results.json')
    args = parser.parse_args()
    output = dict(strict_passed=False, observation_validated=False, entire_p7_qualified=False,
                  complete_evidence_validated=False, inputs={}, evidence_files={}, comparisons=[], unmatched_comparisons=[], source_sha256=sha(Path(__file__)),
                  limitations=['Successful payload delivery does not establish new-path validation.',
                               'Matching idle failures do not establish recovery from a decreasing payload ceiling.',
                               'Partial byte counts, packet counts, MTU discovery progress and timings may differ.'])
    try:
        baseline = json.loads(args.peer_receipt.read_text())
        baseline_sha = sha(args.peer_receipt)
        output['peer_receipt'] = dict(path=str(args.peer_receipt.resolve()), sha256=baseline_sha)
        cases = {}
        seen_paths, seen_hashes, seen_executions, seen_observations = set(), set(), set(), set()
        for path in args.receipts:
            path = path.resolve()
            digest = sha(path)
            check(str(path) not in seen_paths and digest not in seen_hashes, 'Duplicate receipt input: ' + str(path))
            seen_paths.add(str(path)); seen_hashes.add(digest)
            receipt = json.loads(path.read_text())
            output['inputs'][str(path)] = digest
            validate_receipt(receipt, baseline, baseline_sha)
            for index, case in enumerate(receipt['cases']):
                if case['scenario'] not in NEGATIVE + POSITIVE or case['managed_role'] not in ('native', 'both'):
                    continue
                execution, observation = execution_identity(case)
                check(execution not in seen_executions and observation not in seen_observations,
                      'Duplicate execution presented as an independent observation: ' + case['name'])
                seen_executions.add(execution); seen_observations.add(observation)
                key = tuple(case[k] for k in ('variant', 'runtime', 'family', 'cipher', 'scenario', 'managed_role'))
                outcome = validate_case(case)
                output['evidence_files'].update(outcome['evidence_files'])
                entry = dict(case=case, receipt=str(path), receipt_sha256=digest, case_index=index,
                             case_name=case['name'], strict_passed=case['passed'], proxy_receipt=execution,
                             observation_sha256=observation, outcome_kind=outcome['kind'], evidence_files=outcome['evidence_files'])
                cases.setdefault(key, []).append(entry)
        check(bool(cases), 'No observations')
        recovery.validate_baseline(baseline, sorted({key[0] for key in cases}))
        profiles = sorted({key[:4] for key in cases})
        required_profiles = set(itertools.product(('raw', 'optimized'), ('jit', 'aot'), ('ipv4', 'ipv6'), ('128', '256')))
        check(set(profiles) == required_profiles, 'Full sixteen-profile observation campaign required')
        output['rebinding_repetitions_per_role'] = REBINDING_REPETITIONS
        output['sampling_limits'] = 'Fixed five repetitions; observed frequencies are descriptive only. No equal-frequency or common-cause claim.'
        for profile in profiles:
            for scenario in NEGATIVE + POSITIVE:
                keys = [(*profile, scenario, role) for role in ('native', 'both')]
                check(all(key in cases for key in keys), 'Missing matched scenario/control: ' + str((*profile, scenario)))
                expected_count = REBINDING_REPETITIONS if scenario == 'rebinding' else 1
                native, managed = (cases[key] for key in keys)
                check(len(native) == len(managed) == expected_count, 'Missing or extra fixed repetitions: ' + str((*profile, scenario)))
                reference = configuration(native[0]['case'])
                check(all(configuration(item['case']) == reference for item in native + managed),
                      'Fault configuration differs between implementations or repetitions')
                native_failures = [item for item in native if not item['strict_passed']]
                managed_failures = [item for item in managed if not item['strict_passed']]
                native_kinds = {item['outcome_kind'] for item in native_failures}
                missing_kinds = sorted({item['outcome_kind'] for item in managed_failures} - native_kinds)
                matched = not missing_kinds
                def references(items):
                    return [{key: value for key, value in item.items() if key != 'case'} for item in items]
                output['comparisons'].append(dict(variant=profile[0], runtime=profile[1], family=profile[2], cipher=profile[3],
                    scenario=scenario, strict_passed=all(item['strict_passed'] for item in native + managed),
                    native_successes=len(native)-len(native_failures), native_failures=len(native_failures),
                    managed_successes=len(managed)-len(managed_failures), managed_failures=len(managed_failures),
                    native_outcome_counts={kind: sum(item['outcome_kind'] == kind for item in native) for kind in sorted({item['outcome_kind'] for item in native})},
                    managed_outcome_counts={kind: sum(item['outcome_kind'] == kind for item in managed) for kind in sorted({item['outcome_kind'] for item in managed})},
                    managed_failure_has_native_witness=matched, configuration_sha256=canonical_hash(reference),
                    native_observations=references(native), managed_observations=references(managed),
                    outcome='positive control passed' if scenario in POSITIVE else
                        ('all observed rebinding successes and failures retained; no frequency equivalence asserted' if scenario == 'rebinding' else
                         'client-direction ceiling decreases; incomplete transfer; actual terminal kinds retained')))
                if missing_kinds:
                    output['unmatched_comparisons'].append(dict(profile=list(profile), scenario=scenario, unmatched_managed_outcomes=missing_kinds))
        check(len(output['comparisons']) == 80, 'Incomplete logical scenario comparisons')
        check(sha(args.peer_receipt) == baseline_sha, 'Peer receipt changed during classification')
        for path, expected in output['inputs'].items():
            check(sha(Path(path)) == expected, 'Recovery receipt changed during classification')
        for path, expected in output['evidence_files'].items():
            check(sha(Path(path)) == expected, 'Terminal diagnostic evidence changed during classification')
        output['complete_evidence_validated'] = True
        output['observation_validated'] = not output['unmatched_comparisons']
        if not output['observation_validated']:
            output['unmatched_reason'] = 'Managed terminal kind lacks a same-profile/configuration native witness; fixed sampling is complete, no extra repetitions'
    except Exception as error:
        output['error'] = str(error)
        raise
    finally:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(output, indent=2) + '\n')
    print(json.dumps({key: output[key] for key in ('strict_passed', 'observation_validated', 'entire_p7_qualified')}))
    return 0 if output['observation_validated'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
