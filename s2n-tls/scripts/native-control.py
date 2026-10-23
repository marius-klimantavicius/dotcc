#!/usr/bin/env python3
"""Build isolated native-only controls and assert forced binary TLS exchanges."""
import argparse
import datetime
import hashlib
import json
import os
import re
from pathlib import Path
import shutil
import socket
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.compat import references
from campaigns.identity import exact_replace


def digest_tree(path):
    h = hashlib.sha256()
    for file in sorted(path.rglob('*')):
        if file.is_file():
            h.update(file.relative_to(path).as_posix().encode())
            h.update(file.read_bytes())
    return h.hexdigest()


class Run:
    def __init__(self, output, reuse_credentials=False):
        self.output = output
        output.mkdir(parents=True, exist_ok=True)
        self.commands = []
        self.reuse_credentials = reuse_credentials

    def command(self, argv, cwd=ROOT, timeout=600, env=None, expected_exit=0):
        log = self.output / ('%03d-%s.log' % (len(self.commands), Path(str(argv[0])).name))
        entry = {'argv': list(map(str, argv)), 'cwd': str(cwd), 'log': str(log)}
        self.commands.append(entry)
        print('RUN ' + ' '.join(entry['argv']), flush=True)
        with log.open('w') as stream:
            result = subprocess.run(entry['argv'], cwd=cwd, env=env, stdout=stream, stderr=subprocess.STDOUT, timeout=timeout)
        entry['exit_code'] = result.returncode
        if expected_exit is not None and result.returncode != expected_exit:
            raise RuntimeError('Command failed; see ' + str(log) + '\n' + log.read_text()[-4000:])
        return log

    def openssl(self, name, source):
        stage = ROOT / 'build/native' / name
        signature = digest_tree(source)
        stamp = stage / '.dotcc-source.json'
        old = json.loads(stamp.read_text()) if stamp.exists() else {}
        if old.get('digest') != signature:
            if stage.exists():
                shutil.rmtree(stage)
            shutil.copytree(source, stage)
            legacy_peer = name == 'openssl-peer-sslv3'
            options = ['no-shared', 'enable-ssl3'] if legacy_peer else [
                'no-shared', 'no-tests', 'enable-ssl3', 'enable-ssl3-method', 'enable-weak-ssl-ciphers']
            self.command(['perl', './Configure', 'linux-x86_64', *options,
                          '--prefix=' + str(stage / 'install'), '--openssldir=' + str(stage / 'install/ssl')], stage)
            if not legacy_peer:
                self.command(['make', '-j2', 'build_generated'], stage)
            stamp.write_text(json.dumps({'digest': signature}) + '\n')
        print('HEADERS_READY ' + str(stage / 'include'), flush=True)
        self.command(['make', '-j2', *(['build_libs', 'build_apps'] if name == 'openssl-peer-sslv3' else [])], stage)
        self.command([str(stage / 'apps/openssl'), 'version', '-a'])
        return stage

    def prepare_s2n(self, source):
        stage = ROOT / 'build/native/s2n-source'
        adaptations = json.loads((ROOT / 'config/adaptations.json').read_text())
        signature = hashlib.sha256((digest_tree(source) + json.dumps(adaptations, sort_keys=True)).encode()).hexdigest()
        stamp = stage / '.dotcc-source.json'
        old = json.loads(stamp.read_text()) if stamp.exists() else {}
        if old.get('digest') != signature:
            if stage.exists():
                shutil.rmtree(stage)
            shutil.copytree(source, stage)
            for adaptation in adaptations['replacements']:
                path = stage / adaptation['path']
                path.write_text(exact_replace(path.read_text(), adaptation['old'], adaptation['new'], label=adaptation['reason']))
            stamp.write_text(json.dumps({'digest': signature, 'adaptations': adaptations}) + '\n')
        self.adaptations = adaptations
        return stage

    def cmake(self, source, native, backend):
        self.command(['cmake', '-S', str(source), '-B', str(native),
                      '-DCMAKE_BUILD_TYPE=Debug', '-DBUILD_SHARED_LIBS=OFF', '-DBUILD_TESTING=ON',
                      '-DCMAKE_EXPORT_COMPILE_COMMANDS=ON', '-DS2N_USE_CRYPTO_SHARED_LIBS=OFF',
                      '-DOPENSSL_ROOT_DIR=' + str(backend), '-DCMAKE_PREFIX_PATH=' + str(backend),
                      '-Dcrypto_INCLUDE_DIR=' + str(backend / 'include'),
                      '-Dcrypto_STATIC_LIBRARY=' + str(backend / 'libcrypto.a'),
                      '-DS2N_ENFORCE_PROPER_LIBCRYPTO_FEATURE_PROBE=ON'])
        self.command(['cmake', '--build', str(native), '-j2', '--target', 's2n'])

    def probe_dh(self, native, source, backend, suffix, expected_exit):
        executable = ROOT / 'build/native' / ('dh-leading-zero-' + suffix)
        self.command(['cc', '-std=gnu99', '-g', '-O1', '-I' + str(source),
                      '-I' + str(source / 'api'), '-I' + str(backend / 'include'),
                      str(ROOT / 'tests/NativeOracle/dh_leading_zero.c'), str(native / 'lib/libs2n.a'),
                      str(backend / 'libcrypto.a'), '-lpthread', '-ldl', '-lrt', '-Wl,--wrap=DH_generate_key', '-o', str(executable)])
        log = self.command([str(executable), str(source / 'tests/pems/dhparams_2048.pem')], expected_exit=expected_exit)
        observation = json.loads(next(line for line in log.read_text().splitlines() if line.startswith('{')))
        return {'kind': suffix, 'observation': observation, 'exit_code': self.commands[-1]['exit_code'], 'log': str(log)}

    def build(self, sources):
        backend = self.openssl('openssl-legacy', sources['openssl-legacy'])
        peer = self.openssl('openssl-peer', sources['openssl-peer'])
        legacy_peer = self.openssl('openssl-peer-sslv3', sources['openssl-peer-sslv3'])
        source = self.prepare_s2n(sources['s2n-tls'])
        pristine = ROOT / 'build/native/s2n'
        native = ROOT / 'build/native/s2n-adapted'
        self.cmake(sources['s2n-tls'], pristine, backend)
        self.dh_regression = [self.probe_dh(pristine, sources['s2n-tls'], backend, 'pristine', None)]
        self.cmake(source, native, backend)
        self.dh_regression.append(self.probe_dh(native, source, backend, 'adapted', 0))
        expected = json.loads((ROOT / 'config/source-manifest.json').read_text())['units']
        compiled = []
        commands = json.loads((native / 'compile_commands.json').read_text())
        for command in commands:
            if 'CMakeFiles/s2n.dir/' in command['command']:
                compiled.append(Path(command['file']).relative_to(source).as_posix())
        if sorted(compiled) != sorted(expected):
            raise RuntimeError('Native CMake source closure differs from frozen manifest')
        self.native_configuration = {'source': str(source), 'build': str(native), 'compiled_c_units': compiled,
                                     'feature_flags': (native / 'CMakeFiles/s2n.dir/flags.make').read_text()}
        self.command(['cc', '-std=gnu99', '-g', '-O1', '-I' + str(source),
                      '-I' + str(source / 'api'), '-I' + str(backend / 'include'),
                      '-I' + str(ROOT / 'src/Boundary'), str(ROOT / 'src/Boundary/dotcc_s2n_compat.c'),
                      str(ROOT / 'tests/NativeOracle/s2n_endpoint.c'), str(native / 'lib/libs2n.a'),
                      str(backend / 'libcrypto.a'), '-lpthread', '-ldl', '-lrt',
                      '-o', str(ROOT / 'build/native/s2n-endpoint')])
        self.command(['cc', '-std=gnu99', '-g', '-O1', '-I' + str(peer / 'include'),
                      str(ROOT / 'tests/NativeOracle/openssl_endpoint.c'), str(peer / 'libssl.a'),
                      str(peer / 'libcrypto.a'), '-lpthread', '-ldl',
                      '-o', str(ROOT / 'build/native/openssl-endpoint')])
        self.command(['cc', '-std=gnu99', '-g', '-O1', '-I' + str(legacy_peer / 'include'),
                      str(ROOT / 'tests/NativeOracle/openssl_endpoint.c'), str(legacy_peer / 'libssl.a'),
                      str(legacy_peer / 'libcrypto.a'), '-lpthread', '-ldl',
                      '-o', str(ROOT / 'build/native/openssl-sslv3-endpoint')])
        self.credentials(peer)
        self.command(['cc', '--version'])
        for executable in ('s2n-endpoint', 'openssl-endpoint', 'openssl-sslv3-endpoint'):
            log = self.command(['ldd', str(ROOT / 'build/native' / executable)])
            if re.search(r'lib(?:crypto|ssl|s2n)\.', log.read_text()):
                raise RuntimeError('Unexpected dynamic crypto/TLS dependency in isolated native control')
        return native

    def credentials(self, peer):
        fixtures = ROOT / 'build/native/credentials'
        fixtures.mkdir(parents=True, exist_ok=True)
        binary = str(peer / 'apps/openssl')
        if self.reuse_credentials:
            paths = [fixtures / (kind + suffix) for kind in ('rsa', 'ecdsa') for suffix in ('.pem', '.key')]
            before = {str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in paths}
            for kind in ('rsa', 'ecdsa'):
                certificate, key = fixtures / (kind + '.pem'), fixtures / (kind + '.key')
                self.command([binary, 'verify', '-CAfile', certificate, certificate])
                self.command([binary, 'x509', '-in', certificate, '-checkend', '3600', '-noout'])
                cert_public = self.command([binary, 'x509', '-in', certificate, '-pubkey', '-noout']).read_bytes()
                key_public = self.command([binary, 'pkey', '-in', key, '-pubout']).read_bytes()
                if cert_public != key_public:
                    raise RuntimeError('Existing native fixture certificate/key mismatch: ' + kind)
            if before != {str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in paths}:
                raise RuntimeError('Native fixture validation modified credential bytes')
            self.credential_policy = {'mode': 'reuse-validated', 'sha256': before,
                                      'checks': 'Pinned native signature/trust, at least3600seconds validity, exact SPKI/key agreement'}
            return
        config = fixtures / 'openssl.cnf'
        template = '[req]\ndistinguished_name=dn\nx509_extensions=ext\nprompt=no\n[dn]\nCN=localhost-{kind}\n[ext]\nsubjectAltName=DNS:localhost\nbasicConstraints=critical,CA:TRUE\nkeyUsage=digitalSignature,keyEncipherment,keyCertSign\nextendedKeyUsage=serverAuth,clientAuth\n'
        for kind, key_args in [('rsa', ['-newkey', 'rsa:2048']), ('ecdsa', ['-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:P-256'])]:
            config.write_text(template.format(kind=kind))
            self.command([binary, 'req', '-x509', '-nodes', *key_args, '-sha256', '-days', '2',
                          '-config', str(config), '-keyout', str(fixtures / (kind + '.key')),
                          '-out', str(fixtures / (kind + '.pem'))])
        self.credential_policy = {'mode': 'generated', 'validity_days': 2}

    def upstream(self):
        config = json.loads((ROOT / 'config/upstream-tests.json').read_text())
        targets = config['native_control_tests']
        native = ROOT / 'build/native/s2n-adapted'
        self.command(['cmake', '--build', str(native), '-j2', '--target', *targets])
        env = dict(os.environ, S2N_DONT_MLOCK='1')
        rows = []
        for target in targets:
            log = self.command([str(native / 'bin' / target)], ROOT / 'build/native/s2n-source/tests/unit', env=env)
            text = log.read_text()
            match = re.search(r'PASSED\s+(\d+) tests', text)
            row = {'name': target, 'status': 'passed' if match and int(match[1]) > 0 and 'SKIPPED' not in text else 'failed',
                   'assertions': int(match[1]) if match else 0, 'log': str(log), 'skipped': 0}
            rows.append(row)
        return rows

    def exchange(self, suite, version, topology, validation='validate'):
        client_kind, server_kind = topology.split('-')
        left, right = socket.socketpair()
        procs = []
        logs = []
        row = {'suite': suite['name'], 'iana_id': suite['iana_id'], 'version': version,
               'topology': topology, 'validation': validation, 'status': 'failed', 'commands': []}
        try:
            for kind, role, sock in [(server_kind, 'server', right), (client_kind, 'client', left)]:
                endpoint = 'openssl-sslv3' if kind == 'openssl' and version == 'S2N_SSLv3' else kind
                executable = ROOT / 'build/native' / (endpoint + '-endpoint')
                argv = [str(executable), role, str(sock.fileno()), version, suite['iana_id'], suite['name'],
                        str(ROOT / 'build/native/credentials'), str(ROOT / 'ref/s2n-tls'), validation]
                row['commands'].append(argv)
                log = self.output / ('exchange-%s-%s-%s-%s-%s.log' % (version, suite['iana_id'], topology, validation, role))
                stream = log.open('w')
                proc = subprocess.Popen(argv, pass_fds=(sock.fileno(),), stdout=stream, stderr=subprocess.STDOUT)
                procs.append((proc, stream))
                logs.append(str(log))
            left.close()
            right.close()
            for proc, stream in procs:
                try:
                    proc.wait(timeout=15)
                except subprocess.TimeoutExpired:
                    proc.kill()
                    proc.wait()
                stream.close()
            row['exit_codes'] = [p.returncode for p, _ in procs]
            row['logs'] = logs
            row['observations'] = []
            for log in logs:
                lines = Path(log).read_text().splitlines()
                observations = [json.loads(line) for line in lines if line.startswith('{')]
                row['observations'].extend(observations)
            if validation == 'untrusted':
                if all(code != 0 for code in row['exit_codes']) and 'S2N_ERR_CERT_UNTRUSTED' in Path(logs[1]).read_text():
                    row['status'] = 'passed'
                    row['expected_rejection'] = 'S2N_ERR_CERT_UNTRUSTED'
            if all(code == 0 for code in row['exit_codes']) and len(row['observations']) == 2:
                if all(o['version'] == version and o['iana_id'].lower() == suite['iana_id'].lower()
                       and o['sent'] == 1027 and o['received'] == 1027 and o['shutdown'] for o in row['observations']):
                    row['status'] = 'passed'
        finally:
            left.close()
            right.close()
            for proc, stream in procs:
                if proc.poll() is None:
                    proc.kill()
                    proc.wait()
                stream.close()
        print(json.dumps({k: v for k, v in row.items() if k not in ('logs', 'observations', 'commands')}), flush=True)
        return row


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--fetch', choices=('never', 'missing'), default='never')
    parser.add_argument('--build-only', action='store_true')
    parser.add_argument('--no-build', action='store_true')
    parser.add_argument('--no-upstream', action='store_true', help='Explicit diagnostic omission; report upstream as unrun.')
    parser.add_argument('--reuse-credentials', action='store_true', help='Validate existing test credentials with pinned OpenSSL without changing their bytes; missing/expired/mismatched fixtures fail.')
    parser.add_argument('--selection', choices=('required', 'all'), default='all')
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    output = args.output or ROOT / 'artifacts/native' / datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ')
    run = Run(output, args.reuse_credentials)
    report = {'scope': 'Native controls only; no translated product acceptance is claimed.', 'commands': run.commands, 'exchanges': [], 'upstream': []}
    try:
        os.environ['DOTCC_CAMPAIGN_FETCH'] = args.fetch
        sources = references(ROOT, args.fetch == 'never')
        report['sources'] = json.loads((ROOT / 'config/inputs.json').read_text())
        run.command([sys.executable, str(ROOT / 'scripts/source-inventory.py')])
        if not args.no_build:
            run.build(sources)
            report['adaptations'] = run.adaptations
            report['dh_leading_zero_regression'] = run.dh_regression
            report['native_configuration'] = run.native_configuration
            report['credential_policy'] = run.credential_policy
        else:
            report['omissions'] = ['Build/provenance and deterministic DH control omitted (--no-build); diagnostic execution only.']
        identity_paths = [ROOT / 'build/native' / name for name in ('s2n-endpoint', 'openssl-endpoint', 'openssl-sslv3-endpoint')]
        identity_paths += list((ROOT / 'tests/NativeOracle').glob('*.[ch]'))
        identity_paths += [ROOT / 'src/Boundary/dotcc_s2n_compat.c', ROOT / 'src/Boundary/dotcc_s2n_compat.h']
        identity_paths += [ROOT / 'scripts/native-control.py', ROOT / 'scripts/source-inventory.py']
        identity_paths += list((ROOT / 'config').glob('*.json'))
        report['observed_sha256'] = {str(p): hashlib.file_digest(p.open('rb'), 'sha256').hexdigest() for p in identity_paths}
        report['policy'] = 'Same authored src/Boundary/dotcc_s2n_compat.c as product; no copied test_all policy or native endpoint connection-field writes.'
        if not args.build_only:
            if not args.no_upstream:
                report['upstream'] = run.upstream()
            config = json.loads((ROOT / 'config/compatibility.json').read_text())
            for suite in config['suites']:
                for version in suite['versions']:
                    if args.selection == 'required' and not (version == 'S2N_SSLv3' or version == 'S2N_TLS13'):
                        continue
                    for topology in ('s2n-s2n', 's2n-openssl', 'openssl-s2n'):
                        report['exchanges'].append(run.exchange(suite, version, topology))
            for suite in (config['suites'][0], next(s for s in config['suites'] if s['minimum_version'] == 'S2N_TLS13')):
                version = suite['versions'][0]
                report['exchanges'].append(run.exchange(suite, version, 's2n-s2n', 'untrusted'))
                report['exchanges'].append(run.exchange(suite, version, 's2n-s2n', 'bypass-untrusted'))
            failed = [x for x in report['exchanges'] if x['status'] != 'passed']
            if failed:
                raise RuntimeError('%d forced exchanges failed' % len(failed))
            if any(row['status'] != 'passed' for row in report['upstream']):
                raise RuntimeError('Missing positive native upstream assertion count')
            if args.no_upstream:
                report['omissions'] = ['Native upstream controls explicitly unrun (--no-upstream); native full gate remains open.']
        report['status'] = 'passed'
    except Exception as error:
        report['status'] = 'failed'
        report['error'] = str(error)
        print(str(error), file=sys.stderr)
    finally:
        report['counts'] = {'passed': sum(x['status'] == 'passed' for x in report['exchanges']),
                            'failed': sum(x['status'] != 'passed' for x in report['exchanges']), 'skipped': 0}
        (output / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
        print('REPORT ' + str(output / 'report.json'), flush=True)
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
