"""Opt-in transport fixture repair around the unchanged finite port iterator."""
import errno
import hashlib
import inspect
import json
from pathlib import Path
import socket
import textwrap


def probe(port):
    observations, sockets = [], []
    try:
        for family, host in ((socket.AF_INET, '127.0.0.1'), (socket.AF_INET6, '::1')):
            selected = socket.socket(family, socket.SOCK_STREAM)
            sockets.append(selected)
            if family == socket.AF_INET6:
                selected.setsockopt(socket.IPPROTO_IPV6, socket.IPV6_V6ONLY, 1)
            try:
                selected.bind((host, port))
            except OSError as error:
                observations.append({'host': host, 'errno': error.errno, 'result': 'occupied' if error.errno == errno.EADDRINUSE else 'error'})
                if error.errno != errno.EADDRINUSE:
                    raise
                return False, observations
            observations.append({'host': host, 'errno': 0, 'result': 'bound'})
        return True, observations
    finally:
        for selected in sockets:
            selected.close()


def install(iterator_type, report):
    original = iterator_type.__next__
    source = textwrap.dedent(inspect.getsource(original))
    policy = json.loads(Path(__file__).with_name('python-adaptations.json').read_text())['available_port_probe']
    if hashlib.sha256(source.encode()).hexdigest() != policy['original_next_sha256']:
        raise RuntimeError('Original AvailablePorts.__next__ changed; refusing the guarded transport adapter')
    report['available_port_probe'] = dict(policy, original_method=original.__qualname__,
        original_source_sha256=hashlib.sha256(source.encode()).hexdigest(),
        adapter_sha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(), candidates=[])

    def next_available(iterator):
        while True:
            candidate = original(iterator)
            free, observations = probe(candidate)
            report['available_port_probe']['candidates'].append({
                'candidate': candidate, 'probes': observations, 'returned': free})
            if free:
                return candidate

    iterator_type.__next__ = next_available
