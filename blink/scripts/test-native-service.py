#!/usr/bin/env python3
"""Native Linux and native Blink are independent guest oracles only."""
import argparse
import collections
import hashlib
import json
import os
from pathlib import Path
import re
import selectors
import shutil
import signal
import socket
import subprocess
import time

CAMPAIGN = Path(__file__).resolve().parents[1]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def expected(status, body):
    return (f"HTTP/1.1 {status}\r\nContent-Type: text/plain\r\n"
            f"Content-Length: {len(body)}\r\nConnection: close\r\n\r\n").encode() + body


def run(args):
    mode = 'native-blink' if args.blink else 'native-linux'
    output = CAMPAIGN / 'artifacts/guest' / mode
    output.mkdir(parents=True, exist_ok=True)
    guest = CAMPAIGN / 'build/guest/service'
    fixture = CAMPAIGN / 'tests/ServiceFixture/instance.txt'
    command = [str(guest), '0', str(fixture)]
    trace = output / 'syscalls.log'
    trace.unlink(missing_ok=True)
    if args.blink:
        command = [str(Path(args.blink).resolve()), '-jms', '-L', str(trace)] + command
    else:
        if not shutil.which('strace'):
            raise SystemExit('strace is required for a native syscall inventory')
        command = ['strace', '-f', '-qq', '-s', '128', '-o', str(trace)] + command
    receipt = {'mode': mode, 'command': command, 'guest_sha256': digest(guest),
               'fixture_sha256': digest(fixture), 'cases': [], 'passed': False}
    if args.blink:
        receipt['blink_sha256'] = digest(Path(args.blink).resolve())
    started = time.monotonic()
    deadline = started + args.timeout

    def remaining():
        value = deadline - time.monotonic()
        if value <= 0:
            raise TimeoutError('service test wall-clock deadline exceeded')
        return min(value, 10)

    stderr = (output / 'stderr.txt').open('wb')
    process = subprocess.Popen(command, stdout=subprocess.PIPE, stderr=stderr,
                               cwd=fixture.parent, start_new_session=True,
                               env={'PATH': os.environ.get('PATH', '/usr/bin:/bin'), 'LANG': 'C'})
    stdout = b''
    try:
        with selectors.DefaultSelector() as select:
            select.register(process.stdout, selectors.EVENT_READ)
            while b'\n' not in stdout:
                if not select.select(remaining()):
                    raise TimeoutError('no readiness output before timeout')
                chunk = os.read(process.stdout.fileno(), 4096)
                if not chunk:
                    raise RuntimeError('service exited before readiness')
                stdout += chunk
                if len(stdout) > 4096:
                    raise RuntimeError('readiness output limit exceeded')
        match = re.fullmatch(rb'READY ([0-9]+)\n', stdout)
        if not match:
            raise AssertionError(f'unexpected readiness: {stdout!r}')
        port = int(match[1])
        receipt['readiness_seconds'] = time.monotonic() - started
        cases = [
            ('health', b'GET /health HTTP/1.1\r\nHost: fixture\r\n\r\n', '200 OK', b'ok\n', False),
            ('file', b'GET /file HTTP/1.1\r\nHost: fixture\r\n\r\n', '200 OK', fixture.read_bytes(), False),
            ('fragmented', b'GET /file HTTP/1.1\r\nHost: fixture\r\n\r\n', '200 OK', fixture.read_bytes(), True),
            ('large', b'GET /large HTTP/1.1\r\nHost: fixture\r\n\r\n', '200 OK', bytes(97 + i % 26 for i in range(131072)), False),
            ('missing', b'GET /missing HTTP/1.1\r\nHost: fixture\r\n\r\n', '404 Not Found', b'not found\n', False),
            ('stop', b'POST /stop HTTP/1.1\r\nHost: fixture\r\nContent-Length: 0\r\n\r\n', '200 OK', b'stopped\n', False),
        ]
        for name, request, status, body, fragmented in cases:
            case = {'name': name, 'passed': False}
            receipt['cases'].append(case)
            (output / (name + '.request')).write_bytes(request)
            with socket.create_connection(('127.0.0.1', port), timeout=remaining()) as client:
                if fragmented:
                    for offset in range(0, len(request), 3):
                        client.settimeout(remaining())
                        client.sendall(request[offset:offset + 3])
                        time.sleep(.002)
                else:
                    client.sendall(request)
                response = bytearray()
                while True:
                    client.settimeout(remaining())
                    chunk = client.recv(4096)
                    if not chunk:
                        break
                    response += chunk
                    if len(response) > 262144:
                        raise RuntimeError('response limit exceeded')
            (output / (name + '.response')).write_bytes(response)
            wanted = expected(status, body)
            if response != wanted:
                raise AssertionError(f'{name}: exact wire response mismatch ({len(response)} bytes, wanted {len(wanted)})')
            case.update(passed=True, request_sha256=hashlib.sha256(request).hexdigest(),
                        response_sha256=hashlib.sha256(response).hexdigest(), response_bytes=len(response))
        tail, _ = process.communicate(timeout=remaining())
        stdout += tail
        receipt['exit_code'] = process.returncode
        if process.returncode != 0 or stdout != f'READY {port}\nSTOPPED\n'.encode():
            raise AssertionError(f'unexpected exit/output: {process.returncode}, {stdout!r}')
        receipt['passed'] = True
    except Exception as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        if process.poll() is None:
            os.killpg(process.pid, signal.SIGKILL)
            tail, _ = process.communicate(timeout=5)
            stdout += tail
        stderr.close()
        (output / 'stdout.txt').write_bytes(stdout)
        receipt['duration_seconds'] = time.monotonic() - started
        if trace.exists():
            lines = trace.read_text(errors='replace').splitlines()
            if args.blink:
                names = [m[1] for line in lines if (m := re.search(r'\(sys\) ([a-z][a-z0-9_]*)\(', line))]
                receipt['syscall_inventory_kind'] = 'complete observed native Blink guest syscall log'
            else:
                names = [m[1] for line in lines if (m := re.match(r'(?:[0-9]+\s+)?([a-z][a-z0-9_]*)\(', line))]
                receipt['syscall_inventory_kind'] = 'complete observed native Linux trace; execve is test launch'
            receipt['observed_syscalls'] = sorted(set(names))
            receipt['syscall_counts'] = dict(sorted(collections.Counter(names).items()))
        (output / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(receipt, indent=2))
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--blink', help='native pinned Blink executable; omitted runs native Linux under strace')
    parser.add_argument('--timeout', type=float, default=60, help='whole-test deadline in seconds')
    raise SystemExit(run(parser.parse_args()))
