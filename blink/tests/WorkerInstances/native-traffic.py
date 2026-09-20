#!/usr/bin/env python3
"""Bounded normal HTTP traffic on the unchanged native static-musl guest."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import runpy
import signal
import socket
import subprocess
import sys
import tempfile
import time

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
ENV = {'LANG': 'C', 'DOTNET_GCHeapHardLimit': '1000000', 'DOTNET_GCRegionRange': '2000000', 'DOTNET_GCRegionSize': '100000'}
FRAGMENTS = [1, 17, 59, 571, 1595, 3572, 3573]
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--native-gc-receipt', type=Path, required=True)
    args = parser.parse_args()
    base = ROOT / 'artifacts/worker-native-traffic'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    receipt = dict(passed=False, inputs={}, environment=ENV, cases=[], limits=dict(overall_seconds=60, ready_seconds=20, request_seconds=10, response_bytes=8192))
    process = None
    print(attempt, flush=True)
    def pin(path, expected=None):
        path = Path(path).resolve(); digest = sha(path)
        if expected is not None and digest != expected: raise RuntimeError('Identity differs: ' + str(path))
        receipt['inputs'][str(path)] = digest
        return path
    # Shared reviewed group cleanup, without running its managed matrix.
    helper = HERE / 'run.py'
    cleanup = runpy.run_path(str(helper), run_name='worker_cleanup')['cleanup']
    try:
        pin(__file__); pin(helper); pin(sys.executable)
        source = pin(args.native_gc_receipt); native = json.loads(source.read_text())
        if not native.get('passed') or not native.get('final_identities_stable') or native['environment'] != ENV:
            raise RuntimeError('Passing native GC-profile witness required')
        for path, digest in native['inputs'].items(): pin(path, digest)
        binary = pin(native['binary']['path'], native['binary']['sha256'])
        receipt['producer'] = dict(path=str(source), sha256=sha(source)); receipt['binary'] = native['binary']
        strace = pin(native['tools']['strace']['path'], native['tools']['strace']['sha256'])
        padded = b'GET /health HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\nX-Padding: ' + b'a' * 3500 + b'\r\n\r\n'
        missing = b'GET /missing HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n'
        if len(padded) != 3573 or len(missing) != 61: raise RuntimeError('Reviewed request shape changed')
        for name in ('health.response', 'stop.request', 'stop.response'):
            pin(source.parent / name, native['artifacts'][name])
        expected404 = b'HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 10\r\nConnection: close\r\n\r\nnot found\n'
        cases = [('large', padded, [len(padded)], (source.parent / 'health.response').read_bytes()),
                 ('fragmented', padded, FRAGMENTS, (source.parent / 'health.response').read_bytes()),
                 ('missing', missing, [len(missing)], expected404),
                 ('stop', (source.parent / 'stop.request').read_bytes(), None, (source.parent / 'stop.response').read_bytes())]
        receipt['command'] = [str(strace), '-f', '-qq', '-s', '128', '-o', str(attempt / 'native.strace'), '--', str(binary), '0']
        signal.alarm(60)
        with (attempt / 'native.stdout').open('wb') as out, (attempt / 'native.stderr').open('wb') as err:
            process = subprocess.Popen(receipt['command'], cwd=attempt, env=ENV, stdin=subprocess.DEVNULL,
                stdout=out, stderr=err, start_new_session=True)
            ready_until = time.monotonic() + 20
            while True:
                captured = (attempt / 'native.stdout').read_bytes()
                if len(captured) > 128: raise RuntimeError('Unexpected native readiness output')
                if b'\n' in captured: break
                if process.poll() is not None or time.monotonic() >= ready_until: raise RuntimeError('Native readiness failed')
                time.sleep(.01)
            match = re.fullmatch(rb'READY ([0-9]+)\n', captured)
            if not match: raise RuntimeError('Native readiness differs')
            for name, request, ends, expected in cases:
                ends = ends or [len(request)]
                (attempt / (name + '.request')).write_bytes(request)
                row = dict(name=name, write_end_offsets=ends, passed=False, request_sha256=sha(attempt / (name + '.request')))
                receipt['cases'].append(row); response = bytearray(); start = time.monotonic()
                try:
                    with socket.create_connection(('127.0.0.1', int(match[1])), timeout=10) as client:
                        position = 0
                        for end in ends:
                            client.settimeout(max(.001, 10 - (time.monotonic() - start)))
                            client.sendall(request[position:end]); position = end
                        while True:
                            remaining = 10 - (time.monotonic() - start)
                            if remaining <= 0: raise TimeoutError('HTTP deadline')
                            client.settimeout(remaining); chunk = client.recv(4096)
                            if not chunk: break
                            response.extend(chunk)
                            if len(response) > 8192: raise RuntimeError('Response bound')
                finally:
                    (attempt / (name + '.response')).write_bytes(response)
                    row.update(response_sha256=sha(attempt / (name + '.response')), seconds=time.monotonic() - start)
                if response != expected: raise RuntimeError('Native response differs: ' + name)
                row['passed'] = True
            receipt['exit_code'] = process.wait(timeout=10)
        if receipt['exit_code'] != 0 or (attempt / 'native.stderr').read_bytes() or (attempt / 'native.stdout').read_bytes() != captured + b'STOPPED\n':
            raise RuntimeError('Native shutdown differs')
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest: raise RuntimeError('Input changed: ' + path)
        receipt.update(passed=True, final_identities_stable=True)
    except BaseException as error: receipt['failure'] = repr(error)
    finally:
        signal.alarm(0)
        if process is not None:
            receipt['cleanup'] = cleanup(process)
            if receipt['cleanup']['signals'] or not receipt['cleanup']['group_gone']: receipt['passed'] = False
        receipt['artifacts'] = {p.name: sha(p) for p in attempt.iterdir() if p.is_file()}
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    def interrupted(number, frame): raise InterruptedError(str(number))
    signal.signal(signal.SIGTERM, interrupted); signal.signal(signal.SIGALRM, interrupted)
    sys.exit(main())
