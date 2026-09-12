#!/usr/bin/env python3
"""Validate pinned native MsQuic peers; never load native code into the product."""
import hashlib
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
pin = json.loads((ROOT / 'config/source.json').read_text())
source = ROOT / 'ref' / pin['directory']
logs = ROOT / 'artifacts/native-oracle'
logs.mkdir(parents=True, exist_ok=True)
build = ROOT / 'build/native-oracle'
program = build / 'native-peer'
library = build / 'bin/Release'
commands = []


def run(command, name, env=None, expect_failure=False):
    commands.append({'name': name, 'arguments': command,
                     'environment': {key: env[key] for key in ('OPENSSL_CONF', 'SSL_CERT_FILE') if env and key in env}})
    result = subprocess.run(command, capture_output=True, text=True, timeout=90, env=env)
    (logs / (name + '.log')).write_text(result.stdout + result.stderr)
    if bool(result.returncode) != expect_failure:
        raise RuntimeError(f'{name} failed ({result.returncode}); see {logs / (name + ".log")}')
    return result.stdout


if hasattr(os, 'sched_getaffinity'):
    os.sched_setaffinity(0, sorted(os.sched_getaffinity(0))[:4])
run(['gcc', '-std=c17', '-D_GNU_SOURCE', '-DCX_PLATFORM_LINUX', '-I', str(source / 'src/inc'),
     str(ROOT / 'tests/NativePeer/peer.c'), '-L', str(library), '-Wl,-rpath,' + str(library),
     '-lmsquic', '-o', str(program)], 'peer-build')
credentials = ROOT / 'build/native-oracle-certificates'
credentials.mkdir(parents=True, exist_ok=True)
configuration = credentials / 'p256.cnf'
configuration.write_text('''openssl_conf = initialization
[initialization]
ssl_conf = ssl_configuration
[ssl_configuration]
system_default = tls_profile
[tls_profile]
Groups = P-256
''')
environment = dict(os.environ, OPENSSL_CONF=str(configuration))
environment.pop('SSLKEYLOGFILE', None)
results = []
negative_results = []
try:
    for algorithm in ('ecdsa', 'rsa'):
        certificate = credentials / (algorithm + '.pem')
        key = credentials / (algorithm + '.key')
        key_options = ['-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:prime256v1'] if algorithm == 'ecdsa' else ['-newkey', 'rsa:2048']
        run(['openssl', 'req', '-x509', *key_options, '-nodes', '-keyout', str(key), '-out', str(certificate),
             '-days', '2', '-subj', '/CN=localhost', '-addext',
             'subjectAltName=DNS:localhost,IP:127.0.0.1,IP:::1'], 'certificate-' + algorithm)
        key.chmod(0o600)
        environment['SSL_CERT_FILE'] = str(certificate)
        for cipher in ('128', '256'):
            for family in ('ipv4', 'ipv6'):
                name = f'peer-{algorithm}-{cipher}-{family}'
                output = run([str(program), str(certificate), str(key), cipher, family], name, environment)
                receipt = json.loads(output)
                receipt['certificate'] = algorithm
                results.append(receipt)
    for name, trust, hostname in [('untrusted', credentials / 'ecdsa.pem', 'localhost'),
                                  ('wrong-name', credentials / 'rsa.pem', 'wrong.example.invalid')]:
        environment['SSL_CERT_FILE'] = str(trust)
        output = run([str(program), str(credentials / 'rsa.pem'), str(credentials / 'rsa.key'),
                      '128', 'ipv4', str(trust), hostname], 'peer-' + name, environment, expect_failure=True)
        rejected = json.loads(output)
        if rejected['passed'] or rejected['client_bytes'] or rejected['server_bytes']:
            raise RuntimeError('Authentication failure released application data: ' + name)
        negative_results.append({'case': name, 'rejected_before_application_data': True})
finally:
    receipt = {'msquic_revision': pin['commit'], 'commands': commands,
               'source_sha256': hashlib.sha256((ROOT / 'tests/NativePeer/peer.c').read_bytes()).hexdigest(),
               'openssl_configuration': configuration.read_text(), 'results': results, 'negative_results': negative_results,
               'passed': len(results) == 8 and len(negative_results) == 2 and all(result['passed'] for result in results),
               'product_dependency': False}
    (logs / 'peer-results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'cases': len(results), 'results': results}, indent=2))
