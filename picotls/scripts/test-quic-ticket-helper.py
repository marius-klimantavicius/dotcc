#!/usr/bin/env python3
"""Native OpenSSL-backed control of the authored post-handshake QUIC ticket helper."""
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
BUILD, LOG = ROOT / 'build/quic-ticket-helper', ROOT / 'artifacts/quic-ticket-helper'
BUILD.mkdir(parents=True, exist_ok=True)
LOG.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, commands=[], ticket_protection='test-only in-process opaque ID store; provider cryptography tested separately')


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def run(command, name):
    command = [str(value) for value in command]
    receipt['commands'].append(dict(name=name, arguments=command))
    print('RUN ' + name, flush=True)
    result = subprocess.run(command, text=True, capture_output=True, timeout=180)
    (LOG / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(name + ' failed; see ' + str(LOG / (name + '.log')))
    return result.stdout


try:
    inputs = json.loads((ROOT / 'config/inputs.json').read_text())
    source = ROOT / 'ref' / inputs['picotls']['directory']
    files = [ROOT / 'src/Host/picotls-quic.c', ROOT / 'tests/QuicTicketHelper/native.c',
             source / 'lib/picotls.c', source / 'include/picotls.h', source / 'include/picotls/openssl.h',
             ROOT / 'build/oracle/libpicotls-core.a', ROOT / 'build/oracle/libpicotls-openssl.a']
    receipt['input_sha256'] = {str(path.relative_to(ROOT)): sha(path) for path in files}
    run(['gcc', '-std=gnu17', '-O2', '-D_GNU_SOURCE', '-DPTLS_HAVE_LOG=0', '-DPICOTLS_USE_DTRACE=0',
         '-I' + str(source), '-I' + str(source / 'include'), '-c', ROOT / 'src/Host/picotls-quic.c',
         '-o', BUILD / 'helper.o'], 'helper-build')
    run(['gcc', '-std=gnu17', '-O2', '-D_GNU_SOURCE', '-I' + str(source / 'include'),
         ROOT / 'tests/QuicTicketHelper/native.c', BUILD / 'helper.o',
         ROOT / 'build/oracle/libpicotls-openssl.a', ROOT / 'build/oracle/libpicotls-core.a',
         '-lssl', '-lcrypto', '-lpthread', '-ldl', '-o', BUILD / 'native'], 'control-build')
    for kind in ['ec', 'rsa']:
        key_args = ['-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:P-256'] if kind == 'ec' else ['-newkey', 'rsa:2048']
        run(['openssl', 'req', '-x509', *key_args, '-nodes', '-days', '2', '-subj', '/CN=localhost',
             '-addext', 'subjectAltName=DNS:localhost', '-addext', 'extendedKeyUsage=serverAuth',
             '-keyout', BUILD / (kind + '-key.pem'), '-out', BUILD / (kind + '-cert.pem')], kind + '-certificate')
        (BUILD / (kind + '-key.pem')).chmod(0o600)
    output = run([BUILD / 'native', BUILD / 'ec-cert.pem', BUILD / 'ec-key.pem', BUILD / 'rsa-cert.pem', BUILD / 'rsa-key.pem'], 'native')
    expected = 'PASS native QUIC ticket helper: 4 cipher/certificate cases, delayed fresh tickets, distinct PSKs, fresh-DHE resumption, rejection and rollback'
    if output.strip() != expected:
        raise RuntimeError('Unexpected native control receipt')
    if receipt['input_sha256'] != {str(path.relative_to(ROOT)): sha(path) for path in files}:
        raise RuntimeError('Helper/reference/native inputs changed during validation')
    receipt.update(passed=True, cases=4, output=output.strip(), executable_sha256=sha(BUILD / 'native'))
finally:
    (LOG / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps(dict(passed=receipt['passed'])))
