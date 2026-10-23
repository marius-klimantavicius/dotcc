"""Prove original port order minus actual occupied IPv4/IPv6 kernel candidates."""
import argparse
import errno
import json
import os
from pathlib import Path
import socket
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'ref/s2n-tls/tests/integrationv2'))
from common import AvailablePorts
import port_probe


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    for name in ('PYTEST_XDIST_WORKER', 'PYTEST_XDIST_WORKER_COUNT'):
        os.environ.pop(name, None)
    low = 65000
    while low + 5 <= 65536 and not all(port_probe.probe(port)[0] for port in range(low, low + 5)):
        low += 5
    if low + 5 > 65536:
        raise RuntimeError('No unused bounded five-port contract fixture')
    held = []
    try:
        for family, host, port in ((socket.AF_INET, '127.0.0.1', low + 1),
                                   (socket.AF_INET6, '::1', low + 3)):
            selected = socket.socket(family, socket.SOCK_STREAM)
            held.append(selected)
            if family == socket.AF_INET6:
                selected.setsockopt(socket.IPPROTO_IPV6, socket.IPV6_V6ONLY, 1)
            selected.bind((host, port))
            selected.listen(1)
        original = list(AvailablePorts(low=low, high=low + 5))
        report = {}
        port_probe.install(AvailablePorts, report)
        returned = list(AvailablePorts(low=low, high=low + 5))
        trace = report['available_port_probe']['candidates']
        assert original == list(range(low, low + 5))
        assert returned == [low, low + 2, low + 4]
        assert [row['candidate'] for row in trace] == original
        assert [row['candidate'] for row in trace if not row['returned']] == [low + 1, low + 3]
        assert all(row['probes'][-1]['errno'] == errno.EADDRINUSE for row in trace if not row['returned'])
        report.update(status='passed', original_sequence=original, returned_sequence=returned,
                      held_real_ipv4_listener=low + 1, held_real_ipv6_listener=low + 3)
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(report, indent=2) + '\n')
        print('REPORT ' + str(args.output))
    finally:
        for selected in held:
            selected.close()


if __name__ == '__main__':
    main()
