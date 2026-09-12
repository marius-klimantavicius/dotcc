#!/usr/bin/env python3
"""Bounded controls for the test proxy; this does not qualify QUIC recovery."""
import json
from pathlib import Path
import socket
import subprocess
import sys
import tempfile
import threading
import time
import unittest

from proxy import Proxy, Rules


class ProxyControls(unittest.TestCase):
    def test_rule_validation_and_reproducible_jitter(self):
        for value in ({'unknown': 1}, {'drop_first': -1}, {'delay_ms': True}, {'jitter_ms': 10001}):
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

    def exchange(self, family, fault=False, bounded=False, mtu=False):
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
                        expected = [payload for ordinal, payload in enumerate(payloads, 1)
                                    if len(payload) <= (116 if ordinal <= 20 else 144)]
                        self.assertEqual(client_counts['mtu_drops'], len(payloads) - len(expected))
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


if __name__ == '__main__':
    unittest.main(verbosity=2)
