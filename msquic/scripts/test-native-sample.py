#!/usr/bin/env python3
"""Authenticated native sample smoke; payload-content/profile gates remain open."""
import json
import os
from pathlib import Path
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
logs = ROOT / 'artifacts/native-oracle'
logs.mkdir(parents=True, exist_ok=True)
credentials = ROOT / 'build/native-oracle-certificates'
credentials.mkdir(parents=True, exist_ok=True)
certificate = credentials / 'localhost.pem'
key = credentials / 'localhost.key'
if not certificate.exists():
    subprocess.run(['openssl', 'req', '-x509', '-newkey', 'ec',
                    '-pkeyopt', 'ec_paramgen_curve:prime256v1', '-nodes',
                    '-keyout', str(key), '-out', str(certificate), '-days', '2',
                    '-subj', '/CN=localhost', '-addext',
                    'subjectAltName=DNS:localhost,IP:127.0.0.1,IP:::1'],
                   check=True, capture_output=True)
    key.chmod(0o600)
program = ROOT / 'build/native-oracle/bin/Release/quicsample'
environment = dict(os.environ, SSL_CERT_FILE=str(certificate))
environment.pop('SSLKEYLOGFILE', None)
server_command = [str(program), '-server', f'-cert_file:{certificate}', f'-key_file:{key}']
client_command = [str(program), '-client', '-target:localhost']
(logs / 'sample.command.json').write_text(json.dumps({
    'server': server_command, 'client': client_command,
    'environment': {'SSL_CERT_FILE': str(certificate)},
    'certificate_validation': True}, indent=2) + '\n')
with (logs / 'sample-server.log').open('w') as server_log:
    server = subprocess.Popen(server_command, stdin=subprocess.PIPE,
                              stdout=server_log, stderr=subprocess.STDOUT, env=environment)
    try:
        time.sleep(0.5)
        client = subprocess.run(client_command, capture_output=True, text=True,
                                timeout=20, env=environment)
        (logs / 'sample-client.log').write_text(client.stdout + client.stderr)
        if server.poll() is None:
            server.communicate(b'\n', timeout=10)
    finally:
        if server.poll() is None:
            server.kill()
            server.wait()
client_text = (logs / 'sample-client.log').read_text()
server_text = (logs / 'sample-server.log').read_text()
passed = (client.returncode == 0 and server.returncode == 0
          and all('Connected' in output and 'Data received' in output
                  and 'Data sent' in output and 'All done' in output
                  for output in (client_text, server_text)))
receipt = {'passed': passed, 'certificate_validation': True,
           'alpn': 'sample (upstream sample constant)',
           'native_msquic_only': True, 'port': 4567,
           'limitations': ['Upstream sample sends 100-byte buffers and reports receive events; payload contents and received lengths are not checked.',
                           'Group/cipher negotiation not constrained or queried by this sample.',
                           'This is not independent-stack interop or translated product evidence.']}
(logs / 'sample-results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps(receipt, indent=2))
raise SystemExit(0 if passed else 1)
