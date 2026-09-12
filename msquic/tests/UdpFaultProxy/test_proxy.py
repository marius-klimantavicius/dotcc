#!/usr/bin/env python3
"""Bounded controls for the test proxy; this does not qualify QUIC recovery."""
import copy
import errno
import importlib.util
import json
import select
from pathlib import Path
import socket
import subprocess
import sys
import tempfile
import threading
import time
import unittest

from proxy import Proxy, Rules


_recovery_spec = importlib.util.spec_from_file_location(
    'proxy_recovery_validation', Path(__file__).resolve().parents[2] / 'scripts/test-recovery.py')
recovery = importlib.util.module_from_spec(_recovery_spec)
_recovery_spec.loader.exec_module(recovery)


class ProxyControls(unittest.TestCase):
    def test_rule_validation_and_reproducible_jitter(self):
        for value in ({'unknown': 1}, {'drop_first': -1}, {'delay_ms': True}, {'jitter_ms': 10001},
                      {'mtu_change_after': 2, 'mtu_change_after_drops': 1}):
            with self.assertRaises(ValueError):
                Rules.parse(value)
        first, second = Proxy({'server_port': 9, 'seed': 76}), Proxy({'server_port': 9, 'seed': 76})
        try:
            expected = [first.jitter('client_to_server', i, 17) for i in range(100)]
            for i in range(100):
                second.jitter('server_to_client', i, 17)
            self.assertEqual(expected, [second.jitter('client_to_server', i, 17) for i in range(100)])
            self.assertGreater(len(set(expected)), 10)
        finally:
            first.close()
            second.close()

    def exchange(self, family, fault=False, bounded=False, mtu=False, mtu_after_drop=False):
        host = '127.0.0.1' if family == socket.AF_INET else '::1'
        server = socket.socket(family, socket.SOCK_DGRAM)
        server.bind((host, 0))
        server.settimeout(0.05)
        seen, ports, errors = [], set(), []
        stop = threading.Event()

        def echo():
            while not stop.is_set():
                try:
                    data, sender = server.recvfrom(65535)
                    seen.append(data)
                    ports.add(sender[1])
                    server.sendto(data, sender)
                except socket.timeout:
                    continue
                except OSError as error:
                    if not stop.is_set():
                        errors.append(str(error))
                    return

        thread = threading.Thread(target=echo)
        thread.start()
        process = None
        client = socket.socket(family, socket.SOCK_DGRAM)
        client.bind((host, 0))
        client.settimeout(0.05)
        payloads = [b''] + [i.to_bytes(4, 'big') + bytes((i * 31 + j * 17) & 255 for j in range(100 + i)) for i in range(1, 81)]
        configuration = dict(family='ipv4' if family == socket.AF_INET else 'ipv6',
                             server_port=server.getsockname()[1], seed=7381)
        if fault:
            configuration.update(rebind_after_client_packets=12,
                                 client_to_server=dict(drop_first=1, drop_every=7, reorder_every=5,
                                                       duplicate_every=9, delay_ms=2, jitter_ms=3),
                                 server_to_client=dict(drop_every=11, reorder_every=6,
                                                       duplicate_every=13, delay_ms=1, jitter_ms=2))
        if bounded:
            configuration.update(max_queue_packets=1, max_queue_bytes=1,
                                 client_to_server=dict(delay_ms=200))
        if mtu:
            configuration['client_to_server'] = dict(mtu_bytes=116, mtu_change_after=20, mtu_bytes_after=144)
            if mtu_after_drop:
                configuration['client_to_server'].pop('mtu_change_after')
                configuration['client_to_server']['mtu_change_after_drops'] = 1
        try:
            with tempfile.TemporaryDirectory(prefix='dotcc-udp-proxy-') as temporary:
                directory = Path(temporary)
                config, ready, stats = (directory / name for name in ('config.json', 'ready.json', 'stats.json'))
                config.write_text(json.dumps(configuration))
                with (directory / 'proxy.log').open('w+') as log:
                    process = subprocess.Popen([sys.executable, str(Path(__file__).with_name('proxy.py')),
                                                '--config', str(config), '--ready', str(ready), '--stats', str(stats)],
                                               stdout=log, stderr=subprocess.STDOUT)
                    deadline = time.monotonic() + 5
                    while not ready.exists() and time.monotonic() < deadline and process.poll() is None:
                        time.sleep(0.01)
                    self.assertTrue(ready.exists(), 'proxy readiness')
                    endpoint = (host, json.loads(ready.read_text())['port'])
                    received = []
                    # Small bounded bursts avoid relying on kernel receive-buffer capacity.
                    for payload in payloads:
                        client.sendto(payload, endpoint)
                        time.sleep(0.002)
                    deadline = time.monotonic() + 0.8
                    while time.monotonic() < deadline:
                        try:
                            received.append(client.recvfrom(65535)[0])
                        except socket.timeout:
                            continue
                    process.terminate()
                    self.assertEqual(process.wait(timeout=5), 0)
                    report = json.loads(stats.read_text())
                    self.assertEqual(report['outcome'], 'stopped')
                    self.assertEqual(report['remaining_queue_packets'], 0)
                    self.assertEqual(report['remaining_queue_bytes'], 0)
                    self.assertLessEqual(report['peak_queue_packets'], configuration.get('max_queue_packets', 1024))
                    self.assertLessEqual(report['peak_queue_bytes'], configuration.get('max_queue_bytes', 8 * 1024 * 1024))
                    self.assertFalse(errors)
                    allowed = set(payloads)
                    self.assertTrue(all(value in allowed for value in received + seen), 'unchanged opaque payloads')
                    client_counts = report['directions']['client_to_server']
                    self.assertEqual(client_counts['received_packets'], len(payloads))
                    if mtu:
                        expected, drops, before_change = [], 0, 0
                        for ordinal, payload in enumerate(payloads, 1):
                            changed = drops >= 1 if mtu_after_drop else ordinal > 20
                            before_change += not changed
                            if len(payload) <= (144 if changed else 116):
                                expected.append(payload)
                            else:
                                drops += 1
                        self.assertEqual(client_counts['mtu_drops'], len(payloads) - len(expected))
                        self.assertEqual(client_counts['mtu_packets_before_change'], before_change)
                        self.assertEqual(client_counts['mtu_packets_after_change'], len(payloads) - before_change)
                        self.assertEqual(received, expected)
                        self.assertEqual(seen, expected)
                    elif bounded:
                        self.assertGreaterEqual(client_counts['queue_drops'], 80)
                        self.assertEqual(received, [b''])
                    elif fault:
                        self.assertEqual(client_counts['rule_drops'], 1 + len(payloads) // 7)
                        self.assertGreater(client_counts['forwarded_out_of_order'], 0)
                        self.assertGreater(client_counts['duplicates_forwarded'], 0)
                        self.assertGreater(len(received), 0)
                        self.assertEqual(len(report['rebindings']), 1)
                        self.assertEqual(len(ports), 2)
                        self.assertEqual(set(report['backend_ports']), ports)
                    else:
                        self.assertEqual(received, payloads)
                        self.assertEqual(seen, payloads)
                        self.assertEqual(client_counts['rule_drops'], 0)
                        self.assertEqual(client_counts['queue_drops'], 0)
                        self.assertEqual(client_counts['forwarded_packets'], len(payloads))
                        self.assertEqual(report['rebindings'], [])
        finally:
            if process is not None and process.poll() is None:
                process.kill()
                process.wait()
            client.close()
            stop.set()
            thread.join(timeout=2)
            server.close()
            self.assertFalse(thread.is_alive(), 'echo worker drained')

    def refused_backend(self, family, *, observe_process, exit_before_send):
        # The pipe acknowledgments establish socket closure independently of
        # process exit. No sleep guesses whether a UDP endpoint still exists.
        child_code = """
import json, socket, sys
family = socket.AF_INET if sys.argv[1] == 'ipv4' else socket.AF_INET6
host = '127.0.0.1' if family == socket.AF_INET else '::1'
value = socket.socket(family, socket.SOCK_DGRAM)
value.bind((host, 0))
print(json.dumps({'port': value.getsockname()[1]}), flush=True)
if sys.stdin.readline().strip() != 'close':
    raise RuntimeError('expected close command')
value.close()
print('closed', flush=True)
if sys.stdin.readline().strip() != 'exit':
    raise RuntimeError('expected exit command')
"""
        child = subprocess.Popen([sys.executable, '-u', '-c', child_code,
                                  'ipv4' if family == socket.AF_INET else 'ipv6'],
                                 stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                 stderr=subprocess.PIPE, text=True)
        proxy = None

        def read_line():
            self.assertTrue(select.select([child.stdout], [], [], 5)[0],
                            'bounded subprocess pipe acknowledgment')
            line = child.stdout.readline()
            self.assertTrue(line, 'subprocess closed its protocol pipe unexpectedly')
            return line.strip()

        def command(value):
            child.stdin.write(value + '\n')
            child.stdin.flush()

        try:
            port = json.loads(read_line())['port']
            proxy = Proxy(dict(family='ipv4' if family == socket.AF_INET else 'ipv6',
                               server_port=port), server_pid=child.pid if observe_process else None)
            with tempfile.TemporaryDirectory(prefix='dotcc-proxy-icmp-') as temporary:
                proxy.start(Path(temporary) / 'ready.json')
                backend_port = proxy.back.getsockname()[1]
                command('close')
                self.assertEqual(read_line(), 'closed')
                self.assertIsNone(child.poll(), 'socket closure must leave the child process alive')
                if exit_before_send:
                    command('exit')
                    self.assertEqual(child.wait(timeout=5), 0)
                before = time.monotonic_ns()
                payload = b'actual-kernel-port-unreachable-control'
                self.assertEqual(proxy.back.send(payload), len(payload))
                self.assertTrue(select.select([proxy.back], [], [], 5)[0],
                                'actual connected UDP refusal must become readable')
                # Exercise the proxy receive path, not a fabricated OSError.
                proxy.receive(proxy.back, 'server_to_client')
                after = time.monotonic_ns()
                if not exit_before_send:
                    self.assertIsNone(child.poll(), 'live-process refusal control exited unexpectedly')
                counts = proxy.stats['directions']['server_to_client']
                self.assertEqual(counts['receive_errors'], 1, 'retain the raw I/O error count')
                self.assertEqual(counts['send_errors'], 0)
                self.assertEqual(proxy.stats['io_errors_omitted'], 0)
                self.assertEqual(len(proxy.stats['io_errors']), 1)
                event = proxy.stats['io_errors'][0]
                self.assertEqual(event['errno'], errno.ECONNREFUSED)
                self.assertEqual(event['operation'], 'receive')
                self.assertEqual(event['direction'], 'server_to_client')
                self.assertEqual(event['local_port'], backend_port)
                self.assertGreaterEqual(event['monotonic_ns'], before)
                self.assertLessEqual(event['monotonic_ns'], after)
                if observe_process:
                    self.assertEqual(proxy.stats['server_process'],
                                     {'pid': child.pid, 'observation': 'pidfd'})
                    self.assertIs(event['server_process_exited'], exit_before_send)
                else:
                    self.assertIsNone(proxy.stats['server_process'])
                    self.assertIsNone(event['server_process_exited'],
                                      'missing lifetime evidence must remain unknown')
                original = copy.deepcopy(proxy.stats)
                with self.assertRaises(RuntimeError):
                    recovery.validate_io_errors(proxy.stats)
                if observe_process and exit_before_send:
                    classified = recovery.validate_io_errors(proxy.stats, child.pid)
                    self.assertEqual(classified['count'], 1)
                    self.assertEqual(classified['server_pid'], child.pid)
                    self.assertEqual(classified['classification'],
                                     'ECONNREFUSED observed after successful server process exit')
                    with self.assertRaises(RuntimeError):
                        recovery.validate_io_errors(proxy.stats, child.pid + 1)
                    for mutation in ('other-errno', 'omitted', 'raw-count', 'live', 'unknown', 'wrong-port'):
                        altered = copy.deepcopy(proxy.stats)
                        if mutation == 'other-errno':
                            altered['io_errors'][0]['errno'] = errno.EIO
                        elif mutation == 'omitted':
                            altered['io_errors_omitted'] = 1
                        elif mutation == 'raw-count':
                            altered['directions']['server_to_client']['receive_errors'] += 1
                        elif mutation == 'live':
                            altered['io_errors'][0]['server_process_exited'] = False
                        elif mutation == 'unknown':
                            altered['io_errors'][0]['server_process_exited'] = None
                        else:
                            altered['io_errors'][0]['local_port'] = 0
                        with self.subTest(evidence_mutation=mutation), self.assertRaises(RuntimeError):
                            recovery.validate_io_errors(altered, child.pid)
                else:
                    with self.assertRaises(RuntimeError):
                        recovery.validate_io_errors(proxy.stats, child.pid)
                self.assertEqual(proxy.stats, original,
                                 'classification must retain every raw error and event')
        finally:
            if proxy is not None:
                proxy.close()
            if child.poll() is None:
                child.kill()
            child.wait(timeout=5)
            for pipe in (child.stdin, child.stdout, child.stderr):
                pipe.close()

    def test_refusal_while_server_process_alive(self):
        for family in (socket.AF_INET, socket.AF_INET6):
            with self.subTest(family=family):
                self.refused_backend(family, observe_process=True, exit_before_send=False)

    def test_refusal_after_observed_server_exit(self):
        for family in (socket.AF_INET, socket.AF_INET6):
            with self.subTest(family=family):
                self.refused_backend(family, observe_process=True, exit_before_send=True)

    def test_refusal_without_process_observation(self):
        for family in (socket.AF_INET, socket.AF_INET6):
            with self.subTest(family=family):
                self.refused_backend(family, observe_process=False, exit_before_send=False)

    def test_completed_server_requires_successful_terminal_evidence(self):
        # Receipt-validator controls only; these synthetic rows do not stand in
        # for the actual kernel/process error controls above.
        case = dict(server_exit=0, server_pid=12345, managed_role='both',
                    runtime='jit', cipher='128', family='ipv4', server=dict(
                        passed=True, family='ipv4', cipher=0x1301, group=23,
                        quic_version=1, server_bytes=65537, certificate_validation=False,
                        listener_preflight=True, sent_bytes=65537, send_completions=1,
                        connected=1, finished=1, closed=1, transport_status=0,
                        transport_error=0, peer_error=0, aot=False, statistics_status=0,
                        core_sent_stream_bytes=65537, core_received_stream_bytes=65537,
                        host_resources=0, host_allocations=0, host_receive_leases=0,
                        host_send_errors=0, host_receive_errors=0, host_truncations=0))
        self.assertEqual(recovery.completed_server_pid(case), case['server_pid'])
        for field, value in [('passed', False), ('server_bytes', 65536),
                             ('connected', 0), ('finished', 0), ('closed', 0),
                             ('transport_status', 62), ('transport_error', 1),
                             ('peer_error', 1), ('host_resources', 1),
                             ('host_receive_leases', 1), ('host_send_errors', 1)]:
            altered = copy.deepcopy(case)
            altered['server'][field] = value
            with self.subTest(terminal_field=field), self.assertRaises(RuntimeError):
                recovery.completed_server_pid(altered)
        for field, value in [('server_exit', 1), ('server_pid', 0), ('server_pid', None)]:
            altered = copy.deepcopy(case)
            altered[field] = value
            with self.subTest(case_field=field, value=value), self.assertRaises(RuntimeError):
                recovery.completed_server_pid(altered)

    def test_ipv4_passthrough(self):
        self.exchange(socket.AF_INET)

    def test_ipv6_passthrough(self):
        self.exchange(socket.AF_INET6)

    def test_ipv4_faults_and_rebinding(self):
        self.exchange(socket.AF_INET, fault=True)

    def test_ipv6_faults_and_rebinding(self):
        self.exchange(socket.AF_INET6, fault=True)

    def test_queue_bounds(self):
        self.exchange(socket.AF_INET, bounded=True)

    def test_changing_payload_ceiling(self):
        self.exchange(socket.AF_INET, mtu=True)

    def test_changing_payload_ceiling_after_drop(self):
        self.exchange(socket.AF_INET, mtu=True, mtu_after_drop=True)

    def expired_mapping(self, family):
        host = '127.0.0.1' if family == socket.AF_INET else '::1'
        with socket.socket(family, socket.SOCK_DGRAM) as server, socket.socket(family, socket.SOCK_DGRAM) as client:
            server.bind((host, 0)); server.settimeout(1)
            client.bind((host, 0)); client.settimeout(1)
            proxy = Proxy(dict(family='ipv4' if family == socket.AF_INET else 'ipv6',
                server_port=server.getsockname()[1], rebind_after_client_packets=1,
                retire_old_backend_on_rebind=True))
            try:
                with tempfile.TemporaryDirectory(prefix='dotcc-expired-mapping-') as temporary:
                    proxy.start(Path(temporary) / 'ready.json')
                    target = (host, proxy.stats['listen_port'])
                    client.sendto(b'first', target)
                    proxy.receive(proxy.front, 'client_to_server'); proxy.forward_due()
                    data, old_address = server.recvfrom(32)
                    self.assertEqual(data, b'first')
                    old_backend = proxy.back
                    client.sendto(b'second', target)
                    proxy.receive(proxy.front, 'client_to_server'); proxy.forward_due()
                    data, new_address = server.recvfrom(32)
                    self.assertEqual(data, b'second')
                    self.assertNotEqual(old_address[1], new_address[1])
                    server.sendto(b'old-mapping', old_address)
                    server.sendto(b'new-mapping', new_address)
                    proxy.receive(old_backend, 'server_to_client')
                    proxy.receive(proxy.back, 'server_to_client'); proxy.forward_due()
                    self.assertEqual(client.recv(32), b'new-mapping')
                    row = proxy.stats['directions']['server_to_client']
                    self.assertEqual(row['retired_mapping_drops'], 1)
                    self.assertEqual(row['forwarded_packets'], 1)
                    self.assertEqual(len(proxy.sockets), 3)
                    self.assertEqual(len(proxy.retired_backends), 1)
            finally:
                proxy.close()
            self.assertEqual(proxy.stats['remaining_queue_packets'], 0)

    def test_ipv4_expired_mapping(self):
        self.expired_mapping(socket.AF_INET)

    def test_ipv6_expired_mapping(self):
        self.expired_mapping(socket.AF_INET6)


if __name__ == '__main__':
    unittest.main(verbosity=2)
