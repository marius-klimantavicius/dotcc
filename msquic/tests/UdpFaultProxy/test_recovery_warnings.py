#!/usr/bin/env python3
"""Warning policy controls without building or running the transport peers."""
from contextlib import redirect_stderr
import copy
import importlib.util
import io
from pathlib import Path
from types import SimpleNamespace
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location(
    'recovery', Path(__file__).resolve().parents[2] / 'scripts/test-recovery.py')
recovery = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recovery)


def observation(scenario='rebinding', role='both'):
    """Small synthetic snapshots with the previously observed terminal states."""
    case = dict(name='fixture-' + scenario, scenario=scenario, managed_role=role,
                variant='raw', runtime='jit', cipher='128', family='ipv4', passed=False,
                proxy_stopped_before_server_cleanup=True, proxy_exit=0, server_pid=123,
                both_stream_fin_acknowledged_before_close=True, post_exchange_settle_ms=2000)
    for endpoint in ('client', 'server'):
        case[endpoint + '_exit'] = 0
        case[endpoint] = dict(
            passed=True, family='ipv4', cipher=0x1301, group=23, quic_version=1,
            client_bytes=65537, server_bytes=65537, certificate_validation=endpoint == 'client',
            listener_preflight=True, sent_bytes=65537, send_completions=1,
            connected=1, finished=1, closed=1, transport_status=0, transport_error=0,
            peer_error=0, aot=False, statistics_status=0, core_sent_stream_bytes=65537,
            core_received_stream_bytes=65537, host_resources=0, host_allocations=0,
            host_receive_leases=0, host_send_errors=0, host_receive_errors=0,
            host_truncations=0, send_fin_acknowledged=True)
    case['server'].update(remote_port=20002, private_paths_snapshot=True,
                          paths_validated=1, path_failures=0, paths=[
        dict(in_use=1, active=1, peer_validated=0, remote_port=20002, send_challenge=0, send_response=0),
        dict(in_use=1, active=0, peer_validated=1, remote_port=20001, send_challenge=0, send_response=0)])
    case['proxy'] = dict(
        outcome='stopped', source_sha256=recovery.sha(recovery.PROXY),
        remaining_queue_packets=0, remaining_queue_bytes=0,
        peak_queue_packets=10, peak_queue_bytes=10000, io_errors=[], io_errors_omitted=0,
        configuration=dict(max_queue_packets=1024, max_queue_bytes=8388608,
                           retire_old_backend_on_rebind=False),
        backend_ports=[20001, 20002], rebindings=[dict(old_port=20001, new_port=20002)],
        directions={direction: dict(forwarded_packets=40, received_packets=50,
                                    queue_drops=0, truncated_drops=0, foreign_drops=0,
                                    send_errors=0, receive_errors=0, mtu_drops=0)
                    for direction in recovery.DIRECTIONS})
    case['error'] = 'Server did not validate the new source port'
    if scenario == 'payload-ceiling-down':
        case['error'] = 'Peer or proxy exit failed: ' + case['name']
        case['both_stream_fin_acknowledged_before_close'] = False
        case['proxy'].update(backend_ports=[20001], rebindings=[])
        case['proxy']['configuration']['client_to_server'] = dict(
            mtu_bytes=1472, mtu_bytes_after=1300, mtu_change_after=20)
        case['proxy']['directions']['client_to_server']['mtu_drops'] = 10
        for endpoint in ('client', 'server'):
            case[endpoint + '_exit'] = 1
            case[endpoint].update(passed=False, finished=0, send_fin_acknowledged=False,
                                  transport_status=62, transport_error=1,
                                  client_bytes=0, server_bytes=0)
    return case


class RecoveryWarningTests(unittest.TestCase):
    def run_observation(self, case, strict=False):
        receipt = dict(cases=[], warnings=[])
        def exchange(args, output, *selection):
            output['cases'].append(case)
            if not case['passed']:
                raise RuntimeError(case['error'])
        with patch.object(recovery, 'exchange', side_effect=exchange), redirect_stderr(io.StringIO()) as log:
            recovery.run_case(SimpleNamespace(strict_recovery=strict), receipt,
                              'raw', 'jit', case['managed_role'], 'ipv4', '128', case['scenario'])
        return receipt, log.getvalue()

    def test_known_outcomes_warn_for_all_peer_roles(self):
        for scenario in ('rebinding', 'payload-ceiling-down'):
            for role in ('both', 'native', 'client', 'server'):
                with self.subTest(scenario=scenario, role=role):
                    receipt, log = self.run_observation(observation(scenario, role))
                    self.assertIn('WARNING: [test]', log)
                    self.assertIn('timing dependent', log)
                    self.assertIn('also observable in upstream MsQuic', log)
                    self.assertEqual(len(receipt['warnings']), 1)
                    self.assertFalse(receipt['cases'][0]['passed'])
                    self.assertIn('error', receipt['cases'][0])

    def test_strict_mode_retains_failure(self):
        with self.assertRaisesRegex(RuntimeError, 'Server did not validate'):
            self.run_observation(observation(), strict=True)

    def test_unrelated_failures_remain_fatal(self):
        mutations = (
            lambda c: c.update(scenario='rebinding-expired-mapping'),
            lambda c: c.update(error='Process crashed'),
            lambda c: c.update(server_exit=-11),
            lambda c: c.update(both_stream_fin_acknowledged_before_close=False),
            lambda c: c['server'].update(host_allocations=1),
            lambda c: c['server'].update(remote_port=9999),
            lambda c: c['server']['paths'][1].update(peer_validated=0),
            lambda c: c['server']['paths'][0].update(send_challenge=1),
            lambda c: c['proxy'].update(remaining_queue_packets=1),
            lambda c: c['proxy'].update(io_errors_omitted=1),
            lambda c: c.pop('proxy_stopped_before_server_cleanup'),
        )
        for index, mutate in enumerate(mutations):
            with self.subTest(index=index):
                case = observation()
                mutate(case)
                with self.assertRaises(RuntimeError):
                    self.run_observation(case)

    def test_decreasing_mtu_requires_fault_and_bounded_terminal_evidence(self):
        mutations = (
            lambda c: c['proxy']['directions']['client_to_server'].update(mtu_drops=0),
            lambda c: c['client'].update(transport_status=110),
            lambda c: c['client'].update(host_receive_leases=1),
            lambda c: c.update(client_exit=-11),
            lambda c: c['client'].update(client_bytes=65537),
            lambda c: [c[e].update(transport_status=0, transport_error=0) for e in ('client', 'server')],
        )
        for index, mutate in enumerate(mutations):
            with self.subTest(index=index):
                case = observation('payload-ceiling-down')
                mutate(case)
                with self.assertRaises(RuntimeError):
                    self.run_observation(case)

    def test_warning_allows_next_case_and_success_is_not_warned(self):
        failed = observation()
        passed = copy.deepcopy(failed)
        passed.update(passed=True, scenario='baseline')
        passed.pop('error')
        cases = iter((failed, passed))
        receipt = dict(cases=[], warnings=[])
        def exchange(args, output, *selection):
            case = next(cases)
            output['cases'].append(case)
            if not case['passed']:
                raise RuntimeError(case['error'])
        with patch.object(recovery, 'exchange', side_effect=exchange), redirect_stderr(io.StringIO()):
            for scenario in ('rebinding', 'baseline'):
                recovery.run_case(SimpleNamespace(strict_recovery=False), receipt,
                                  'raw', 'jit', 'both', 'ipv4', '128', scenario)
        self.assertEqual(len(receipt['cases']), 2)
        self.assertEqual(len(receipt['warnings']), 1)
        self.assertTrue(receipt['cases'][1]['passed'])
        self.assertNotIn('warning', receipt['cases'][1])


if __name__ == '__main__':
    unittest.main()
