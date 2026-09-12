#!/usr/bin/env python3
"""Test-only loopback UDP fault proxy. QUIC payloads remain opaque.

Rules depend on the direction and received packet ordinal, not cross-direction
event ordering. A fixed seed reproduces jitter for the same packet sequence;
live retransmission counts and timing remain properties of the tested peers.
"""
import argparse
from dataclasses import dataclass
import hashlib
import heapq
import ipaddress
import json
from pathlib import Path
import selectors
import signal
import socket
import time


DIRECTIONS = ('client_to_server', 'server_to_client')


@dataclass(frozen=True)
class Rules:
    drop_first: int = 0
    drop_every: int = 0
    reorder_every: int = 0
    duplicate_every: int = 0
    delay_ms: int = 0
    jitter_ms: int = 0
    reorder_hold_ms: int = 25
    duplicate_delay_ms: int = 1
    mtu_bytes: int = 0
    mtu_change_after: int = 0
    mtu_bytes_after: int = 0

    @classmethod
    def parse(cls, value):
        if not isinstance(value, dict) or set(value) - set(cls.__dataclass_fields__):
            raise ValueError('Unknown or invalid packet fault rule')
        result = cls(**value)
        for name, number in vars(result).items():
            if type(number) is not int or number < 0 or number > 1_000_000:
                raise ValueError('Invalid nonnegative bounded rule: ' + name)
            if name.endswith('_ms') and number > 10_000:
                raise ValueError('Fault delay exceeds ten seconds')
            if name in ('mtu_bytes', 'mtu_bytes_after') and number > 65535:
                raise ValueError('Invalid UDP payload ceiling')
        return result


def atomic_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(value, indent=2, sort_keys=True) + '\n')
    temporary.replace(path)


class Proxy:
    def __init__(self, config):
        allowed = {'family', 'listen_host', 'listen_port', 'server_host', 'server_port', 'seed',
                   'max_queue_packets', 'max_queue_bytes', 'rebind_after_client_packets', *DIRECTIONS}
        if not isinstance(config, dict) or set(config) - allowed:
            raise ValueError('Unknown proxy configuration field')
        family = config.get('family', 'ipv4')
        if family not in ('ipv4', 'ipv6'):
            raise ValueError('Select ipv4 or ipv6')
        self.family = socket.AF_INET if family == 'ipv4' else socket.AF_INET6
        local = '127.0.0.1' if family == 'ipv4' else '::1'
        self.listen_host = config.get('listen_host', local)
        self.server_host = config.get('server_host', local)
        for address in (self.listen_host, self.server_host):
            parsed = ipaddress.ip_address(address)
            if not parsed.is_loopback or parsed.version != (4 if family == 'ipv4' else 6):
                raise ValueError('This test proxy requires same-family loopback addresses')
        self.listen_port = config.get('listen_port', 0)
        self.server_port = config['server_port']
        self.seed = config.get('seed', 1)
        self.packet_limit = config.get('max_queue_packets', 1024)
        self.byte_limit = config.get('max_queue_bytes', 8 * 1024 * 1024)
        self.rebind_after = config.get('rebind_after_client_packets', 0)
        bounds = [(self.listen_port, 0, 65535), (self.server_port, 1, 65535),
                  (self.seed, 0, (1 << 64) - 1), (self.packet_limit, 1, 65536),
                  (self.byte_limit, 1, 64 * 1024 * 1024), (self.rebind_after, 0, 1_000_000)]
        if any(type(value) is not int or not low <= value <= high for value, low, high in bounds):
            raise ValueError('Invalid proxy port, seed, queue bound, or rebind ordinal')
        self.rules = {direction: Rules.parse(config.get(direction, {})) for direction in DIRECTIONS}
        self.config = dict(family=family, listen_host=self.listen_host, listen_port=self.listen_port,
                           server_host=self.server_host, server_port=self.server_port, seed=self.seed,
                           max_queue_packets=self.packet_limit, max_queue_bytes=self.byte_limit,
                           rebind_after_client_packets=self.rebind_after,
                           **{direction: vars(rule) for direction, rule in self.rules.items()})
        self.selector = selectors.DefaultSelector()
        self.front = None
        self.back = None
        self.sockets = []
        self.client = None
        self.pending = []
        self.queue_bytes = 0
        self.serial = 0
        self.running = True
        self.started = time.monotonic()
        self.rebound = False
        self.highest_forwarded = dict.fromkeys(DIRECTIONS, 0)
        counters = ('received_packets', 'received_bytes', 'forwarded_packets', 'forwarded_bytes',
                    'rule_drops', 'mtu_drops', 'queue_drops', 'shutdown_drops', 'truncated_drops', 'foreign_drops',
                    'send_errors', 'receive_errors', 'delayed_scheduled', 'reordered_scheduled', 'forwarded_out_of_order',
                    'duplicates_scheduled', 'duplicates_forwarded')
        self.stats = dict(format='dotcc-udp-fault-proxy-v1', outcome='starting', configuration=self.config,
                          source_sha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
                          peak_queue_packets=0, peak_queue_bytes=0, backend_ports=[], rebindings=[],
                          directions={name: dict.fromkeys(counters, 0) for name in DIRECTIONS})

    def create_socket(self):
        value = socket.socket(self.family, socket.SOCK_DGRAM)
        self.sockets.append(value)
        if self.family == socket.AF_INET6:
            value.setsockopt(socket.IPPROTO_IPV6, socket.IPV6_V6ONLY, 1)
        value.setblocking(False)
        return value

    def create_backend(self):
        value = self.create_socket()
        value.bind((self.listen_host, 0))
        value.connect((self.server_host, self.server_port))
        self.selector.register(value, selectors.EVENT_READ, 'server_to_client')
        self.stats['backend_ports'].append(value.getsockname()[1])
        return value

    def start(self, ready):
        self.front = self.create_socket()
        self.front.bind((self.listen_host, self.listen_port))
        self.selector.register(self.front, selectors.EVENT_READ, 'client_to_server')
        self.back = self.create_backend()
        self.stats['listen_port'] = self.front.getsockname()[1]
        self.stats['outcome'] = 'running'
        atomic_json(ready, dict(port=self.stats['listen_port'], backend_port=self.back.getsockname()[1],
                               family=self.config['family'], seed=self.seed))

    def jitter(self, direction, ordinal, maximum):
        if maximum == 0:
            return 0
        key = f'{self.seed}:{direction}:{ordinal}'.encode('ascii')
        return int.from_bytes(hashlib.blake2s(key, digest_size=8).digest(), 'big') % (maximum + 1)

    def schedule(self, direction, ordinal, payload, delay_ms, duplicate=False):
        count = self.stats['directions'][direction]
        if len(self.pending) >= self.packet_limit or self.queue_bytes + len(payload) > self.byte_limit:
            count['queue_drops'] += 1
            return False
        self.serial += 1
        heapq.heappush(self.pending, (time.monotonic() + delay_ms / 1000, self.serial,
                                     direction, ordinal, payload, duplicate))
        self.queue_bytes += len(payload)
        self.stats['peak_queue_packets'] = max(self.stats['peak_queue_packets'], len(self.pending))
        self.stats['peak_queue_bytes'] = max(self.stats['peak_queue_bytes'], self.queue_bytes)
        return True

    def receive(self, value, direction):
        count = self.stats['directions'][direction]
        for _ in range(64):
            try:
                payload, _, flags, sender = value.recvmsg(65535)
            except BlockingIOError:
                return
            except ConnectionRefusedError:
                count['receive_errors'] += 1
                return
            count['received_packets'] += 1
            count['received_bytes'] += len(payload)
            ordinal = count['received_packets']
            if flags & socket.MSG_TRUNC:
                count['truncated_drops'] += 1
                continue
            if direction == 'client_to_server':
                if self.client is None:
                    self.client = sender
                elif sender != self.client:
                    count['foreign_drops'] += 1
                    continue
            elif self.client is None:
                count['foreign_drops'] += 1
                continue
            rule = self.rules[direction]
            ceiling = rule.mtu_bytes_after if rule.mtu_change_after and ordinal > rule.mtu_change_after else rule.mtu_bytes
            if ceiling and len(payload) > ceiling:
                count['mtu_drops'] += 1
                continue
            if ordinal <= rule.drop_first or (rule.drop_every and ordinal % rule.drop_every == 0):
                count['rule_drops'] += 1
                continue
            delay = rule.delay_ms + self.jitter(direction, ordinal, rule.jitter_ms)
            reorder = rule.reorder_every and ordinal % rule.reorder_every == 0
            if reorder:
                delay += rule.reorder_hold_ms
            if self.schedule(direction, ordinal, payload, delay):
                if delay:
                    count['delayed_scheduled'] += 1
                if reorder:
                    count['reordered_scheduled'] += 1
            if rule.duplicate_every and ordinal % rule.duplicate_every == 0:
                if self.schedule(direction, ordinal, payload, delay + rule.duplicate_delay_ms, True):
                    count['duplicates_scheduled'] += 1

    def forward_due(self):
        now = time.monotonic()
        while self.pending and self.pending[0][0] <= now:
            _, _, direction, ordinal, payload, duplicate = heapq.heappop(self.pending)
            self.queue_bytes -= len(payload)
            count = self.stats['directions'][direction]
            if direction == 'client_to_server' and self.rebind_after and not self.rebound and count['forwarded_packets'] >= self.rebind_after:
                previous = self.back.getsockname()[1]
                self.back = self.create_backend()
                self.rebound = True
                self.stats['rebindings'].append(dict(after_forwarded_packets=count['forwarded_packets'],
                                                    old_port=previous, new_port=self.back.getsockname()[1]))
                # Keep the previous socket receiving until shutdown. This avoids
                # introducing an undocumented packet blackhole during validation.
            try:
                sent = self.back.send(payload) if direction == 'client_to_server' else self.front.sendto(payload, self.client)
                if sent != len(payload):
                    raise OSError('Partial UDP send')
            except OSError:
                count['send_errors'] += 1
                continue
            count['forwarded_packets'] += 1
            count['forwarded_bytes'] += len(payload)
            if duplicate:
                count['duplicates_forwarded'] += 1
            else:
                if ordinal < self.highest_forwarded[direction]:
                    count['forwarded_out_of_order'] += 1
                self.highest_forwarded[direction] = max(self.highest_forwarded[direction], ordinal)

    def run(self):
        while self.running:
            self.forward_due()
            timeout = min(0.05, max(0, self.pending[0][0] - time.monotonic())) if self.pending else 0.05
            for key, _ in self.selector.select(timeout):
                self.receive(key.fileobj, key.data)
        self.stats['outcome'] = 'stopped'

    def close(self):
        for _, _, direction, _, _, _ in self.pending:
            self.stats['directions'][direction]['shutdown_drops'] += 1
        self.pending.clear()
        self.queue_bytes = 0
        self.selector.close()
        for value in self.sockets:
            value.close()
        self.stats['elapsed_seconds'] = time.monotonic() - self.started
        self.stats['remaining_queue_packets'] = 0
        self.stats['remaining_queue_bytes'] = 0


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--config', required=True, type=Path)
    parser.add_argument('--ready', required=True, type=Path)
    parser.add_argument('--stats', required=True, type=Path)
    args = parser.parse_args()
    args.ready.unlink(missing_ok=True)
    args.stats.unlink(missing_ok=True)
    raw = args.config.read_bytes()
    proxy = Proxy(json.loads(raw))
    proxy.stats['configuration_sha256'] = hashlib.sha256(raw).hexdigest()

    def stop(_signum, _frame):
        proxy.running = False

    signal.signal(signal.SIGTERM, stop)
    signal.signal(signal.SIGINT, stop)
    try:
        proxy.start(args.ready)
        proxy.run()
        return 0
    except Exception as error:
        proxy.stats['outcome'] = 'failed'
        proxy.stats['error'] = str(error)
        raise
    finally:
        proxy.close()
        atomic_json(args.stats, proxy.stats)


if __name__ == '__main__':
    raise SystemExit(main())
