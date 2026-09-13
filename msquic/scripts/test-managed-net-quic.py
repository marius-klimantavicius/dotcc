#!/usr/bin/env python3
"""Build and exercise the direct Managed.Net.Quic facade, serially in JIT/NativeAOT."""
import argparse
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--jit-only', action='store_true')
args = parser.parse_args()
logs = ROOT / 'artifacts/managed-net-quic'
logs.mkdir(parents=True, exist_ok=True)
project = ROOT / 'tests/ManagedNetQuic/ManagedNetQuic.csproj'


def run(command, name, env=None):
    print('RUN ' + name, flush=True)
    result = subprocess.run([str(part) for part in command], text=True,
                            capture_output=True, timeout=600, env=env)
    (logs / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(f'{name} failed; see {logs / (name + ".log")}')
    return result.stdout


run(['dotnet', 'build', project, '-c', 'Release', '--nologo'], 'build')
commands = [('jit', ['dotnet', project.parent / 'bin/Release/net10.0/ManagedNetQuic.dll'])]
if not args.jit_only:
    output = ROOT / 'build/managed-net-quic/aot'
    run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
         '-p:PublishAot=true', '-o', output, '--nologo'], 'aot-build')
    commands.append(('aot', [output / 'ManagedNetQuic']))
expected = None
for runtime, command in commands:
    for ipv6 in (False, True):
        for no_cache in (False, True):
            name = f'{runtime}-ipv{6 if ipv6 else 4}-' + ('uncached' if no_cache else 'cached')
            options = (['--ipv6'] if ipv6 else []) + (['--no-cache'] if no_cache else [])
            # Only test-generated identities/traffic appear in this file. Do not retain secrets.
            keylog = logs / (name + '.keys')
            keylog.unlink(missing_ok=True)
            env = dict(os.environ, SSLKEYLOGFILE=str(keylog))
            try:
                result = run([*command, *options], name, env)
                cases = [line for line in result.splitlines() if line.startswith('PASS ')]
                if len(cases) != 23 or (expected is not None and cases != expected):
                    raise RuntimeError('Missing or inconsistent facade test cases: ' + name)
                expected = cases
                secrets = {}
                for line in keylog.read_text().splitlines():
                    label, random, secret = line.split()
                    if len(random) != 64 or len(secret) not in (64, 96):
                        raise RuntimeError('Invalid TLS secret length: ' + name)
                    bytes.fromhex(random)
                    if not any(bytes.fromhex(secret)):
                        raise RuntimeError('Empty TLS secret: ' + name)
                    key = (label, random)
                    if key in secrets and secrets[key] != secret:
                        raise RuntimeError('Client/server TLS secrets disagree: ' + name)
                    secrets[key] = secret
                if {label for label, _ in secrets} != {
                    'CLIENT_HANDSHAKE_TRAFFIC_SECRET', 'SERVER_HANDSHAKE_TRAFFIC_SECRET',
                    'CLIENT_TRAFFIC_SECRET_0', 'SERVER_TRAFFIC_SECRET_0'
                }:
                    raise RuntimeError('Missing TLS secret labels: ' + name)
            finally:
                keylog.unlink(missing_ok=True)
print(f'PASS Managed.Net.Quic: 23 cases x {len(commands)} runtimes x IPv4/IPv6 x cached/uncached; TLS secrets checked')
