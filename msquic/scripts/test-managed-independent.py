#!/usr/bin/env python3
"""Qualify translated MsQuic against the pinned, separate aioquic test peer."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import resource
import subprocess
import tempfile
import time
import zipfile

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
BUILD = ROOT / 'build/managed-peer'
PEER = ROOT / 'tests/IndependentPeer/peer.py'
PYTHON = ROOT / 'build/independent-peer-venv/bin/python'
HELPERS = Path(__file__).with_name('test-recovery.py')
spec = importlib.util.spec_from_file_location('recovery_controls', HELPERS)
controls = importlib.util.module_from_spec(spec)
spec.loader.exec_module(controls)
sha, check = controls.sha, controls.check


def verify_oracle():
    pin = json.loads((ROOT / 'config/independent-inputs.json').read_text())
    setup = json.loads((ROOT / 'artifacts/independent-peer/setup-results.json').read_text())
    check(setup.get('passed') and setup['inputs'] == pin, 'Independent peer setup is stale')
    sites = list((ROOT / 'build/independent-peer-venv/lib').glob('python*/site-packages'))
    check(len(sites) == 1, 'Ambiguous independent interpreter installation')
    installed = {}
    for wheel in pin['wheels']:
        archive = ROOT / 'ref/independent-peer-wheels' / wheel['filename']
        check(sha(archive) == wheel['sha256'], 'Changed independent wheel: ' + wheel['filename'])
        with zipfile.ZipFile(archive) as contents:
            for name in contents.namelist():
                # These wheels install their executable code at site-packages
                # root. Match every Python module/native extension to its wheel.
                if not name.endswith(('.py', '.so')):
                    continue
                expected = hashlib.sha256(contents.read(name)).hexdigest()
                path = sites[0] / name
                check(path.is_file() and sha(path) == expected, 'Changed independent code: ' + name)
                installed[str(path.relative_to(REPO))] = expected
    check(bool(installed), 'No independent executable code was verified')
    return dict(revision=pin['revision'], product_dependency=False, installed_hashes=installed,
                python_sha256=sha(PYTHON), source_sha256=sha(PEER))


def exchange(args, receipt, variant, runtime, algorithm, cipher, family, managed_role, negative=None):
    name = '-'.join((variant, runtime, algorithm, cipher, family, managed_role, negative or 'positive'))
    directory = args.output / name
    directory.mkdir(parents=True, exist_ok=True)
    ready, aio_receipt = directory / 'server.ready', directory / 'aioquic.json'
    for path in (ready, aio_receipt):
        path.unlink(missing_ok=True)
    certificate, key = BUILD / (algorithm + '.pem'), BUILD / (algorithm + '.key')
    trust = args.output / 'unrelated.pem' if negative == 'untrusted' else certificate
    hostname = 'wrong.example.invalid' if negative == 'wrong-name' else 'localhost'
    managed = (['dotnet', str(BUILD / variant / 'bin/Release/net10.0/ManagedPeer.dll')]
               if runtime == 'jit' else [str(BUILD / variant / 'aot/ManagedPeer')])
    case = dict(name=name, variant=variant, runtime=runtime, certificate=algorithm, cipher=cipher,
                family=family, managed_role=managed_role, negative=negative, passed=False, commands=[])
    receipt['cases'].append(case)
    processes = []

    def command(role, port):
        if role == managed_role:
            return [*managed, str(certificate), str(key), cipher, family, str(trust), hostname, role, str(port), str(ready)]
        return [str(PYTHON), str(PEER), role, '--certificate', str(certificate), '--key', str(key),
                '--trust', str(trust), '--cipher', cipher, '--family', family, '--port', str(port),
                '--server-name', hostname, '--alpn', 'different-protocol' if negative == 'alpn' else 'dotcc-probe',
                '--ready', str(ready), '--receipt', str(aio_receipt)]

    def start(role, port, log, environment):
        arguments = command(role, port)
        case['commands'].append(arguments)
        process = subprocess.Popen(arguments, stdout=log, stderr=subprocess.STDOUT, env=environment)
        processes.append(process)
        return process

    started = time.monotonic()
    try:
        with tempfile.TemporaryDirectory(prefix='dotcc-managed-independent-') as isolated_tmp:
            environment = dict(os.environ, TMPDIR=isolated_tmp, OPENSSL_CONF=str(BUILD / 'p256.cnf'), SSL_CERT_FILE=str(trust))
            environment.pop('SSLKEYLOGFILE', None)
            environment.pop('PYTHONPATH', None)
            with (directory / 'server.log').open('w') as server_log, (directory / 'client.log').open('w') as client_log:
                server = start('server', 0, server_log, environment)
                port = controls.wait_ready(server, ready)
                client = start('client', port, client_log, environment)
                case['client_exit'] = client.wait(timeout=45)
                case['server_exit'] = server.wait(timeout=45)
            independent = json.loads(aio_receipt.read_text())
            managed_result = controls.read_peer(directory / (managed_role + '.log'))
            case.update(aioquic=independent, managed=managed_result)
            if negative:
                check(case['client_exit'] != 0 and case['server_exit'] != 0, 'Authentication failure unexpectedly succeeded')
                check(not independent['passed'] and not managed_result['passed'], 'Negative peer reported success')
                check(independent['received_bytes'] == independent['sent_bytes'] == 0
                      and managed_result['client_bytes'] == managed_result['server_bytes'] == managed_result['sent_bytes'] == 0,
                      'Application data escaped authentication failure')
                check(managed_result['host_resources'] == managed_result['host_allocations']
                      == managed_result['host_receive_leases'] == 0 and managed_result['connected'] == 0,
                      'Authentication rejection leaked host ownership or established a connection')
                # ALPN can reject before the listener admits an application
                # connection; in that case there is no app handle to shut down.
                # The reused BCL verifier reports unknown_ca for an untrusted
                # chain. aioquic reports bad_certificate for its trust failure.
                # Pinned aioquic tls.py selects AlertHandshakeFailure(40) for
                # disjoint ALPN; the MsQuic server sends no_application_protocol.
                alert = ((40 if managed_role == 'client' else 120) if negative == 'alpn'
                         else 48 if negative == 'untrusted' and managed_role == 'client' else 42)
                check(independent.get('close_code') == 0x100 + alert, 'Failure was not the expected TLS alert')
                case.update(tls_alert=alert, rejected_before_application_data=True)
            else:
                check(case['client_exit'] == case['server_exit'] == 0, 'Interop peer exited unsuccessfully')
                controls.validate_peer(managed_result, managed_role, True, runtime, cipher, family)
                check(independent['passed'] and independent['family'] == family
                      and independent['cipher'] == managed_result['cipher'] and independent['group'] == 23
                      and independent['quic_version'] == 1 and independent['alpn'] == 'dotcc-probe'
                      and independent['handshake'] and independent['verified_fin']
                      and not independent['early_data'] and not independent['resumed']
                      and independent['received_bytes'] == independent['sent_bytes'] == 65537,
                      'Independent peer profile or payload mismatch')
            case['passed'] = True
            print(name + ': PASS', flush=True)
    except BaseException as error:
        case['error'] = str(error)
        raise
    finally:
        for process in processes:
            if process.poll() is None:
                process.kill()
            process.wait()
        case['elapsed_seconds'] = time.monotonic() - started


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
    parser.add_argument('--jit-only', action='store_true')
    parser.add_argument('--roles', nargs='+', choices=['client', 'server'], default=['client', 'server'])
    parser.add_argument('--families', nargs='+', choices=['ipv4', 'ipv6'], default=['ipv4', 'ipv6'])
    parser.add_argument('--ciphers', nargs='+', choices=['128', '256'], default=['128', '256'])
    parser.add_argument('--certificates', nargs='+', choices=['ecdsa', 'rsa'], default=['ecdsa', 'rsa'])
    parser.add_argument('--skip-negatives', action='store_true')
    parser.add_argument('--peer-receipt', type=Path, default=ROOT / 'artifacts/managed-peer/results.json')
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/managed-independent')
    args = parser.parse_args()
    args.output = args.output.resolve()
    args.output.mkdir(parents=True, exist_ok=True)
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    if hasattr(os, 'sched_getaffinity'):
        os.sched_setaffinity(0, sorted(os.sched_getaffinity(0))[:4])
    receipt = dict(passed=False, targeted_passed=False, phase='P7 independent basic stream and authentication subset',
                   entire_p7_qualified=False, cases=[], source_hashes={str(path.relative_to(REPO)): sha(path)
                       for path in (Path(__file__), HELPERS, PEER, ROOT / 'config/independent-inputs.json')})
    try:
        baseline = json.loads(args.peer_receipt.read_text())
        controls.validate_baseline(baseline, args.variants)
        receipt['peer_receipt'] = dict(path=str(args.peer_receipt), sha256=sha(args.peer_receipt))
        receipt['oracle'] = verify_oracle()
        receipt['binary_hashes'] = baseline['binary_hashes']
        for algorithm in args.certificates:
            check(sha(BUILD / (algorithm + '.pem')) == baseline['certificate_sha256'][algorithm + '.pem'],
                  'Baseline certificate changed')
        if not args.skip_negatives:
            command = ['openssl', 'req', '-x509', '-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:prime256v1', '-nodes',
                       '-keyout', str(args.output / 'unrelated.key'), '-out', str(args.output / 'unrelated.pem'),
                       '-days', '2', '-subj', '/CN=Unrelated Test Root']
            result = subprocess.run(command, capture_output=True, text=True, timeout=30)
            (args.output / 'unrelated-certificate.log').write_text(result.stdout + result.stderr)
            check(result.returncode == 0, 'Unrelated root generation failed')
            (args.output / 'unrelated.key').chmod(0o600)
            receipt['negative_root_sha256'] = sha(args.output / 'unrelated.pem')
        for variant in args.variants:
            for runtime in (['jit'] if args.jit_only else ['jit', 'aot']):
                for role in args.roles:
                    for algorithm in args.certificates:
                        for cipher in args.ciphers:
                            for family in args.families:
                                check(any(case.get('passed') and case['variant'] == variant and case['runtime'] == runtime
                                          and case['managed_role'] == role and case['certificate'] == algorithm
                                          and case['cipher'] == cipher and case['family'] == family
                                          for case in baseline['cases']), 'Missing selected native baseline case')
                                exchange(args, receipt, variant, runtime, algorithm, cipher, family, role)
                    if not args.skip_negatives:
                        for negative in ('untrusted', 'wrong-name', 'alpn'):
                            exchange(args, receipt, variant, runtime, args.certificates[0], args.ciphers[0], args.families[0], role, negative)
        controls.validate_baseline(baseline, args.variants)
        controls.bind_files(receipt['source_hashes'], 'Interop source')
        controls.bind_files(receipt['oracle']['installed_hashes'], 'Independent executable')
        check(sha(args.peer_receipt) == receipt['peer_receipt']['sha256'], 'Baseline receipt changed')
        full = (set(args.variants) == {'raw', 'optimized'} and not args.jit_only and not args.skip_negatives
                and set(args.roles) == {'client', 'server'} and set(args.families) == {'ipv4', 'ipv6'}
                and set(args.ciphers) == {'128', '256'} and set(args.certificates) == {'ecdsa', 'rsa'})
        receipt.update(passed=full, targeted_passed=not full, cases_passed=len(receipt['cases']))
    except BaseException as error:
        receipt['error'] = str(error)
        raise
    finally:
        (args.output / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps({key: receipt[key] for key in ('passed', 'targeted_passed', 'cases_passed', 'entire_p7_qualified')}))


if __name__ == '__main__':
    main()
