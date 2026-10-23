"""Probe exact native record-rejection errors using cached native endpoints."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import selectors
import socket
import subprocess
import time

ROOT = Path(__file__).resolve().parents[2]


def probe(output, suite, cipher):
    a, b = socket.socketpair(); c, d = socket.socketpair()
    processes, streams = [], []
    row = {'iana_id': suite, 'cipher': cipher, 'version': 'S2N_SSLv3', 'status': 'failed', 'flipped': False, 'commands': [], 'logs': []}
    try:
        for role, descriptor in (('client', a), ('server', d)):
            log = output / (suite + '-' + role + '.log'); stream = log.open('w'); streams.append(stream)
            argv = [str(ROOT / 'build/native/s2n-endpoint'), role, str(descriptor.fileno()), 'S2N_SSLv3', suite, cipher,
                    str(ROOT / 'build/native/credentials'), str(ROOT / 'ref/s2n-tls'), 'validate']
            row['commands'].append(argv); row['logs'].append(str(log))
            processes.append(subprocess.Popen(argv, pass_fds=(descriptor.fileno(),), stdout=stream, stderr=subprocess.STDOUT))
        a.close(); d.close()
        with selectors.DefaultSelector() as selector:
            selector.register(b, selectors.EVENT_READ, (c, True)); selector.register(c, selectors.EVENT_READ, (b, False))
            buffers = {b: bytearray(), c: bytearray()}; deadline = time.monotonic() + 10
            while selector.get_map() and time.monotonic() < deadline:
                for key, events in selector.select(.1):
                    source = key.fileobj; target, client = key.data
                    try: data = source.recv(65536)
                    except ConnectionResetError: data = b''
                    if not data:
                        selector.unregister(source)
                        try: target.shutdown(socket.SHUT_WR)
                        except OSError: pass
                        continue
                    buffers[source] += data
                    while len(buffers[source]) >= 5:
                        length = int.from_bytes(buffers[source][3:5], 'big') + 5
                        if len(buffers[source]) < length: break
                        record = buffers[source][:length]; del buffers[source][:length]
                        if client and record[0] == 23 and not row['flipped']:
                            cbc = int(suite, 16) == 0x000a
                            if cbc and length != 29:
                                raise RuntimeError('Expected a 24-byte first SSLv3 3DES record')
                            offset = length - 1 - (8 if cbc else 0)
                            row['record_header_hex'] = record[:5].hex()
                            row['record_ciphertext_length'] = length - 5
                            row['record_before_hex'] = record.hex()
                            row['mutation_offset_from_record_start'] = offset
                            row['mutation_mask'] = 0x80
                            record[offset] ^= 0x80
                            row['record_after_hex'] = record.hex()
                            row['flipped'] = True
                        try: target.sendall(record)
                        except (BrokenPipeError, ConnectionResetError):
                            selector.unregister(source)
                            break
        for process in processes: process.wait(timeout=3)
        for stream in streams: stream.flush()
        row['exit_codes'] = [p.returncode for p in processes]
        errors = re.findall(r'\b(S2N_ERR_\w+)\b', Path(row['logs'][1]).read_text())
        row['native_error'] = errors[0] if errors else None
        expected = 'S2N_ERR_BAD_MESSAGE' if int(suite, 16) == 0x0004 else 'S2N_ERR_INTEGER_OVERFLOW'
        if row['flipped'] and all(code != 0 for code in row['exit_codes']) and row['native_error'] == expected:
            row['status'] = 'passed'
    except Exception as error:
        row['error'] = str(error)
    finally:
        for process in processes:
            if process.poll() is None: process.kill(); process.wait()
        for stream in streams: stream.close()
        for descriptor in (a, b, c, d): descriptor.close()
    return row


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args(); output = args.output.resolve(); output.mkdir(parents=True, exist_ok=False)
    binary = ROOT / 'build/native/s2n-endpoint'
    report = {'evidence_kind': 'cached-native-record-corruption-control', 'status': 'failed',
              'native_binary': str(binary), 'native_sha256': hashlib.sha256(binary.read_bytes()).hexdigest(),
              'scope': 'Native negative control only; the translated lifecycle cases compare these exact errors.',
              'rows': [probe(output, suite, cipher) for suite, cipher in [('0x0004', 'RC4-MD5'), ('0x000A', 'DES-CBC3-SHA')]]}
    if all(r['status'] == 'passed' for r in report['rows']): report['status'] = 'passed'
    (output / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
    print('REPORT ' + str(output / 'report.json'))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
