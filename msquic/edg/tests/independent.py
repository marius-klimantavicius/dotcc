#!/usr/bin/env python3
"""Run EDG MsQuic/picotls against the repository's pinned aioquic oracle.

The peer is test-only and installed offline from checksum-verified wheels into
the output directory. Existing authored ManagedPeer and aioquic peer sources
are reused; generated product C# is never patched.
"""
from __future__ import annotations

import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time
import zipfile
from authored import adapt as adapt_authored

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    value = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(value)
    return value


PROVIDER = module('edg_msquic_provider', HERE.parent / 'provider.py')
CONTROLS = module('msquic_existing_peer_controls', ROOT / 'scripts/test-recovery.py')


def adapt_peer(source, emitted, picotls):
    source, changes = adapt_authored(PROVIDER, source, 'Program.cs', emitted, picotls)
    for callback, record in [('StreamCallback', 'QUIC_STREAM_EVENT'),
                             ('ConnectionCallback', 'QUIC_CONNECTION_EVENT'),
                             ('ListenerCallback', 'QUIC_LISTENER_EVENT')]:
        start = source.index('private static uint ' + callback + '(')
        end = source.index('\n    private static ', start + 1)
        body = source[start:end]
        for member, path in PROVIDER.field_paths(emitted, record).items():
            if member == path:
                continue
            body, count = re.subn(r'notification->' + re.escape(member) + r'\b', 'notification->' + path, body)
            if count:
                changes.append({'scope': callback, 'old': 'notification->' + member,
                                'new': 'notification->' + path, 'count': count})
        source = source[:start] + body + source[end:]
    replacements = {
        'AppName = name,': 'AppName = (sbyte*)name,',
        'AppName = appName,': 'AppName = (sbyte*)appName,',
        'address.Ip.sa_family, name, port)': 'address.Ip.sa_family, (sbyte*)name, port)',
        'settings.IsSet': 'settings.' + PROVIDER.field_paths(emitted, 'QUIC_SETTINGS')['IsSet'],
    }
    for old, new in replacements.items():
        count = source.count(old)
        if count:
            source = source.replace(old, new)
            changes.append({'old': old, 'new': new, 'count': count})
    return source, changes


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--prepared', required=True, type=Path, help='Successful provider.py output')
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--aot', action='store_true')
    parser.add_argument('--runtime', default='linux-x64')
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {'status': 'running', 'commands': [], 'cases': [], 'inputs': [],
              'scope': 'EDG MsQuic + EDG picotls/BCL; independent aioquic; both roles; IPv4/IPv6; AES128/AES256; ECDSA',
              'limits': ['Pinned oracle wheels require CPython 3.12 Linux x64.',
                         'This focused matrix does not rerun the full dotcc authentication/fault campaign.']}

    def save():
        (output / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def record(path):
        report['inputs'].append({'path': str(path.resolve()), 'sha256': PROVIDER.digest(path)})

    def run(label, command, timeout=900):
        command = list(map(str, command))
        entry = {'label': label, 'command': command}
        report['commands'].append(entry)
        save()
        with (output / (label + '.stdout')).open('wb') as stdout, (output / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, stdout=stdout, stderr=stderr, timeout=timeout,
                                    env=dict(os.environ, DOTNET_PROCESSOR_COUNT='4'))
        entry['exitCode'] = result.returncode
        save()
        print(label + ': exit ' + str(result.returncode), flush=True)
        if result.returncode:
            raise RuntimeError(label + ' failed; retained logs contain details')

    try:
        manifest_path = args.prepared.resolve() / 'provider-manifest.json'
        manifest = json.loads(manifest_path.read_text())
        for path in [Path(__file__).resolve(), HERE / 'authored.py', HERE.parent / 'provider.py', ROOT / 'scripts/test-recovery.py',
                     ROOT / 'tests/IndependentPeer/peer.py', ROOT / 'config/independent-inputs.json', manifest_path]:
            record(path)
        for data in (manifest, manifest['picotls']):
            record(Path(data['generatedProject']))
            for project_path in data['projects'].values():
                record(Path(project_path))
            for category in ('generatedSources', 'authoredSources'):
                for item in data[category]:
                    path = Path(item['path'])
                    if PROVIDER.digest(path) != item['sha256']:
                        raise RuntimeError('Provider input changed: ' + str(path))
                    record(path)
                    if item.get('adaptedPath'):
                        path = Path(item['adaptedPath'])
                        if PROVIDER.digest(path) != item['adaptedSha256']:
                            raise RuntimeError('Adapted provider input changed: ' + str(path))
                        record(path)
        pin = json.loads((ROOT / 'config/independent-inputs.json').read_text())
        wheels = [ROOT / 'ref/independent-peer-wheels' / item['filename'] for item in pin['wheels']]
        for path, item in zip(wheels, pin['wheels']):
            if PROVIDER.digest(path) != item['sha256']:
                raise RuntimeError('Pinned independent wheel checksum mismatch: ' + str(path))
            record(path)
        environment = output / 'oracle-venv'
        run('oracle-venv', [sys.executable, '-m', 'venv', '--without-pip', environment])
        python = environment / 'bin/python'
        run('oracle-install', [sys.executable, '-m', 'pip', '--python', python, 'install', '--no-index', '--no-deps', *wheels])
        sites = list((environment / 'lib').glob('python*/site-packages'))
        if len(sites) != 1:
            raise RuntimeError('Ambiguous independent interpreter site-packages')
        for wheel in wheels:
            with zipfile.ZipFile(wheel) as contents:
                for name in contents.namelist():
                    if not name.endswith(('.py', '.so')):
                        continue
                    path = sites[0] / name
                    if PROVIDER.digest(path) != hashlib.sha256(contents.read(name)).hexdigest():
                        raise RuntimeError('Installed oracle differs from pinned wheel: ' + name)
                    record(path)
        certificate, key = output / 'ecdsa.pem', output / 'ecdsa.key'
        run('certificate', ['openssl', 'req', '-x509', '-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:prime256v1',
            '-keyout', key, '-out', certificate, '-nodes', '-days', '2', '-subj', '/CN=localhost',
            '-addext', 'subjectAltName=DNS:localhost,IP:127.0.0.1,IP:::1'])
        configuration = output / 'p256.cnf'
        configuration.write_text('openssl_conf = default_conf\n[default_conf]\nssl_conf = ssl_sect\n[ssl_sect]\nsystem_default = system_default_sect\n[system_default_sect]\nGroups = P-256\n')
        record(configuration)
        record(certificate)
        source = ROOT / 'tests/ManagedPeer/Program.cs'
        record(source)
        core = Path(manifest['generatedProject']).parent
        pico = Path(manifest['picotls']['generatedProject']).parent
        emitted = '\n'.join(p.read_text() for p in PROVIDER.generated_sources(core) if p.name.startswith('Translated'))
        pico_text = '\n'.join(p.read_text() for p in PROVIDER.generated_sources(pico) if p.name.startswith('Translated'))
        text, changes = adapt_peer(source.read_text(), emitted, pico_text)
        peer_dir = output / 'ManagedPeer'
        peer_dir.mkdir()
        adapted = peer_dir / 'Program.cs'
        adapted.write_text(text)
        record(adapted)
        report['peerAdaptation'] = {'path': str(source), 'adaptedPath': str(adapted), 'replacements': changes}
        project = peer_dir / 'ManagedPeer.csproj'
        PROVIDER.project(project, 'ManagedPeer', [adapted], [Path(manifest['projects']['BclHost'])], executable=True)
        record(project)
        run('peer-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        commands = [('jit', ['dotnet', str(peer_dir / 'bin/Release/net10.0/ManagedPeer.dll')])]
        if args.aot:
            run('peer-aot-build', ['dotnet', 'publish', project, '-c', 'Release', '-r', args.runtime,
                '-p:PublishAot=true', '-p:TreatWarningsAsErrors=true', '-p:IlcTreatWarningsAsErrors=true',
                '-o', output / 'aot'], timeout=1800)
            commands.append(('aot', [str(output / 'aot/ManagedPeer')]))
        for runtime, managed in commands:
            for family in ('ipv4', 'ipv6'):
                for cipher in ('128', '256'):
                    for managed_role in ('client', 'server'):
                        name = '-'.join((runtime, family, cipher, managed_role))
                        directory = output / name
                        directory.mkdir()
                        ready, oracle_receipt = directory / 'ready', directory / 'aioquic.json'
                        case = {'name': name, 'passed': False, 'commands': []}
                        report['cases'].append(case)
                        env = dict(os.environ, DOTNET_PROCESSOR_COUNT='4', OPENSSL_CONF=str(configuration),
                                   SSL_CERT_FILE=str(certificate), DOTCC_PEER_SHARE_UDP_BINDING='0')
                        env.pop('PYTHONPATH', None)
                        env.pop('SSLKEYLOGFILE', None)
                        processes = []

                        def start(role, port, log):
                            if role == managed_role:
                                command = [*managed, str(certificate), str(key), cipher, family,
                                           str(certificate), 'localhost', role, str(port), str(ready)]
                            else:
                                command = list(map(str, [python, ROOT / 'tests/IndependentPeer/peer.py', role,
                                    '--certificate', certificate, '--key', key, '--trust', certificate,
                                    '--cipher', cipher, '--family', family, '--port', str(port),
                                    '--server-name', 'localhost', '--alpn', 'dotcc-probe',
                                    '--ready', ready, '--receipt', oracle_receipt]))
                            case['commands'].append(command)
                            save()
                            process = subprocess.Popen(command, stdout=log, stderr=subprocess.STDOUT, env=env)
                            processes.append(process)
                            return process

                        try:
                            with (directory / 'server.log').open('w') as server_log, (directory / 'client.log').open('w') as client_log:
                                server = start('server', 0, server_log)
                                port = CONTROLS.wait_ready(server, ready)
                                client = start('client', port, client_log)
                                case['clientExit'] = client.wait(timeout=60)
                                case['serverExit'] = server.wait(timeout=60)
                            if case['clientExit'] or case['serverExit']:
                                raise RuntimeError('Independent exchange failed: ' + name)
                            actual = CONTROLS.read_peer(directory / (managed_role + '.log'))
                            oracle = json.loads(oracle_receipt.read_text())
                            CONTROLS.validate_peer(actual, managed_role, True, runtime, cipher, family)
                            if not (oracle['passed'] and oracle['family'] == family and
                                oracle['cipher'] == actual['cipher'] and oracle['group'] == 23 and
                                oracle['quic_version'] == 1 and oracle['alpn'] == 'dotcc-probe' and
                                oracle['handshake'] and oracle['verified_fin'] and
                                not oracle['early_data'] and not oracle['resumed'] and
                                oracle['received_bytes'] == oracle['sent_bytes'] == 65537):
                                raise RuntimeError('Independent payload/profile mismatch: ' + name)
                            case.update(passed=True, managed=actual, oracle=oracle)
                            print(name + ': PASS', flush=True)
                        finally:
                            for process in processes:
                                if process.poll() is None:
                                    process.kill()
                                process.wait()
                            save()
        for item in report['inputs']:
            if PROVIDER.digest(Path(item['path'])) != item['sha256']:
                raise RuntimeError('Input changed during independent tests: ' + item['path'])
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
