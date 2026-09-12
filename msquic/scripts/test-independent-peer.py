#!/usr/bin/env python3
"""Cross-connect native MsQuic and pinned aioquic as separate test processes."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
logs = ROOT / 'artifacts/independent-peer'
logs.mkdir(parents=True, exist_ok=True)
build = ROOT / 'build/independent-peer'
build.mkdir(parents=True, exist_ok=True)
pin = json.loads((ROOT / 'config/independent-inputs.json').read_text())
source_pin = json.loads((ROOT / 'config/source.json').read_text())
source = ROOT / 'ref' / source_pin['directory']
library = ROOT / 'build/native-oracle/bin/Release'
native = build / 'native-peer'
python = ROOT / 'build/independent-peer-venv/bin/python'
peer = ROOT / 'tests/IndependentPeer/peer.py'
receipt = dict(passed=False, product_dependency=False, msquic_revision=source_pin['commit'],
    independent_revision=pin['revision'], commands=[], cases=[], negative_cases=[],
    source_hashes={str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest()
        for p in (peer, ROOT / 'tests/NativePeer/peer.c', Path(__file__))})
(logs / 'interop-results.json').write_text(json.dumps(receipt, indent=2) + '\n')


def run(command, name):
    receipt['commands'].append(dict(name=name, arguments=command))
    result = subprocess.run(command, capture_output=True, text=True, timeout=60)
    (logs / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(f'{name}: exit {result.returncode}')


def wait_ready(process, path):
    end = time.monotonic() + 15
    while time.monotonic() < end:
        if process.poll() is not None:
            raise RuntimeError('Server exited before readiness')
        if path.exists() and path.read_text().strip():
            return int(path.read_text().strip())
        time.sleep(0.01)
    raise TimeoutError('Server readiness timeout')


def exchange(algorithm, cipher, family, aio_role, negative=None):
    name = '-'.join([algorithm, cipher, family, 'aio-' + aio_role] + ([negative] if negative else []))
    ready = build / (name + '.ready')
    ready.unlink(missing_ok=True)
    aio_receipt = logs / (name + '-aio.json')
    aio_receipt.unlink(missing_ok=True)
    certificate, key = build / (algorithm + '.pem'), build / (algorithm + '.key')
    trust = build / ('unrelated.pem' if negative == 'untrusted' else algorithm + '.pem')
    hostname = 'wrong.example.invalid' if negative == 'wrong-name' else 'localhost'
    environment = dict(os.environ, OPENSSL_CONF=str(build / 'p256.cnf'), SSL_CERT_FILE=str(trust))
    environment.pop('SSLKEYLOGFILE', None)

    def command(aio, role, port):
        if aio:
            return [str(python), str(peer), role, '--certificate', str(certificate), '--key', str(key),
                '--trust', str(trust), '--cipher', cipher, '--family', family, '--port', str(port),
                '--server-name', hostname, '--ready', str(ready), '--receipt', str(aio_receipt)]
        return [str(native), str(certificate), str(key), cipher, family, str(trust), hostname, role, str(port), str(ready)]

    server_is_aio = aio_role == 'server'
    processes = []
    try:
        with (logs / (name + '-server.log')).open('w') as server_log, (logs / (name + '-client.log')).open('w') as client_log:
            server_command = command(server_is_aio, 'server', 0)
            receipt['commands'].append(dict(name=name + '-server', arguments=server_command,
                environment={key: environment[key] for key in ('OPENSSL_CONF', 'SSL_CERT_FILE')}))
            server = subprocess.Popen(server_command, stdout=server_log, stderr=subprocess.STDOUT, env=environment)
            processes.append(server)
            port = wait_ready(server, ready)
            client_command = command(not server_is_aio, 'client', port)
            receipt['commands'].append(dict(name=name + '-client', arguments=client_command,
                environment={key: environment[key] for key in ('OPENSSL_CONF', 'SSL_CERT_FILE')}))
            client = subprocess.Popen(client_command, stdout=client_log, stderr=subprocess.STDOUT, env=environment)
            processes.append(client)
            client_exit = client.wait(timeout=30)
            server_exit = server.wait(timeout=30)
        aio_result = json.loads(aio_receipt.read_text())
        native_log = logs / (name + ('-client.log' if server_is_aio else '-server.log'))
        native_result = json.loads(next(line for line in native_log.read_text().splitlines() if line.startswith('{')))
        case = dict(name=name, certificate=algorithm, cipher=cipher, family=family, aio_role=aio_role,
                    client_exit=client_exit, server_exit=server_exit, aioquic=aio_result, msquic=native_result)
        if negative:
            valid = client_exit != 0 and not aio_result['passed'] and not native_result['passed']
            valid &= aio_result['received_bytes'] == 0 and aio_result['sent_bytes'] == 0
            valid &= native_result['client_bytes'] == 0 and native_result['server_bytes'] == 0
            # QUIC CRYPTO_ERROR + bad_certificate (42), or native MsQuic's
            # unknown_ca (48). An unrelated setup/timeout failure cannot pass.
            expected_alert = 48 if server_is_aio and negative == 'untrusted' else 42
            valid &= aio_result.get('close_code') == 0x100 + expected_alert
            case['tls_alert'] = expected_alert
            case['rejected_before_application_data'] = valid
            receipt['negative_cases'].append(case)
        else:
            valid = client_exit == 0 and server_exit == 0 and aio_result['passed'] and native_result['passed']
            valid &= aio_result['cipher'] == native_result['cipher'] == (0x1301 if cipher == '128' else 0x1302)
            valid &= aio_result['group'] == native_result['group'] == 23
            valid &= aio_result['quic_version'] == native_result['quic_version'] == 1
            valid &= aio_result['received_bytes'] == aio_result['sent_bytes'] == 65537
            valid &= native_result['client_bytes' if server_is_aio else 'server_bytes'] == 65537
            case['passed'] = valid
            receipt['cases'].append(case)
        if not valid:
            raise RuntimeError('Interop validation failed: ' + name)
        print(name + ': PASS', flush=True)
    finally:
        for process in processes:
            if process.poll() is None:
                process.kill()
            process.wait()


try:
    if hasattr(os, 'sched_getaffinity'):
        os.sched_setaffinity(0, sorted(os.sched_getaffinity(0))[:4])
    if not json.loads((logs / 'setup-results.json').read_text())['passed']:
        raise RuntimeError('Run independent-peer.py first')
    receipt['setup_receipt_sha256'] = hashlib.sha256((logs / 'setup-results.json').read_bytes()).hexdigest()
    receipt['native_inputs'] = json.loads((ROOT / 'config/native-inputs.json').read_text())
    receipt['native_library_sha256'] = hashlib.sha256((library / 'libmsquic.so').read_bytes()).hexdigest()
    run(['gcc', '-std=c17', '-D_GNU_SOURCE', '-DCX_PLATFORM_LINUX', '-I', str(source / 'src/inc'),
         str(ROOT / 'tests/NativePeer/peer.c'), '-L', str(library), '-Wl,-rpath,' + str(library),
         '-lmsquic', '-o', str(native)], 'native-build')
    (build / 'p256.cnf').write_text('openssl_conf = init\n[init]\nssl_conf = config\n[config]\nsystem_default = profile\n[profile]\nGroups = P-256\n')
    receipt['openssl_configuration'] = (build / 'p256.cnf').read_text()
    for algorithm in ('ecdsa', 'rsa', 'unrelated'):
        options = ['-newkey', 'rsa:2048'] if algorithm == 'rsa' else ['-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:prime256v1']
        run(['openssl', 'req', '-x509', *options, '-nodes', '-keyout', str(build / (algorithm + '.key')),
             '-out', str(build / (algorithm + '.pem')), '-days', '2', '-subj', '/CN=localhost',
             '-addext', 'subjectAltName=DNS:localhost,IP:127.0.0.1,IP:::1'], 'certificate-' + algorithm)
        (build / (algorithm + '.key')).chmod(0o600)
    receipt['certificate_sha256'] = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in build.glob('*.pem')}
    for algorithm in ('ecdsa', 'rsa'):
        for cipher in ('128', '256'):
            for family in ('ipv4', 'ipv6'):
                for role in ('client', 'server'):
                    exchange(algorithm, cipher, family, role)
    for role in ('client', 'server'):
        for negative in ('untrusted', 'wrong-name'):
            exchange('ecdsa', '128', 'ipv4', role, negative)
    receipt['passed'] = len(receipt['cases']) == 16 and len(receipt['negative_cases']) == 4
finally:
    (logs / 'interop-results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'cases': len(receipt['cases']), 'negative_cases': len(receipt['negative_cases'])}))
