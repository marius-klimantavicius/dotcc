"""Managed s2n qualification. Missing real products/cases are failures, never skips.

This specialist is called by the supervisor-owned qualification.py integration
point. It does not build DotCC tools or touch campaign/native/provider outputs.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import socket
import shutil
import subprocess
import sys
import time
import uuid

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.delivery import ignore_build_caches, source_files
from campaigns.translation import relocate_project
sys.path.insert(0, str(ROOT / 'tests/Managed'))
from execution_identity import authored_sources, capture, hashes, verify
LIFECYCLE = ('ownership-freeze', 'pending-dispose', 'cancel-drain', 'io-exception',
             'distinct-concurrency', 'same-connection-serialization', 'invalid-credentials',
             'short-io-rc4', 'short-io-3des', 'short-io-tls13', 'gc-callbacks', 'async-validation', 'async-validation-dispose',
             'validation-exception', 'key-log-exception', 'hostname-rejection', 'untrusted-rejection',
             'explicit-verification-bypass', 'bad-mac-rc4', 'bad-padding-3des', 'bad-tag-tls13', 'truncated-eof',
             'connection-reuse', 'session-resumption', 'serialization-roundtrip', 'serialization-rc4-rejection', 'tls13-key-update', 'alpn',
             'allocation-failures', 'init-failure-quarantine')
SUITES = ('consumer', 'compatibility', 'interop', 'lifecycle', 'upstream', 'delivery', 'rooted-aot')


def load(path):
    return json.loads(Path(path).read_text())


def required_pairs():
    policy = load(ROOT / 'config/compatibility.json')
    pairs = [(s, v) for s in policy['suites'] for v in s['versions']]
    if len(pairs) != policy['suite_version_pairs'] or len(policy['suites']) != policy['suite_count']:
        raise RuntimeError('Frozen suite inventory is inconsistent')
    return pairs


class Qualification:
    def __init__(self, args):
        self.args = args
        run_id = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ') + '-' + uuid.uuid4().hex[:8]
        self.output = (args.output or ROOT / 'artifacts/managed' / run_id).resolve()
        self.output.mkdir(parents=True, exist_ok=False)
        self.work = ROOT / 'build/managed' / run_id
        self.work.mkdir(parents=True)
        self.report = {'evidence_kind': 'translated-product-qualification', 'status': 'failed',
                       'rid': args.rid, 'commands': [], 'rows': [], 'errors': [], 'product_snapshots': [], 'application_builds': [],
                       'qualification_script_sha256': hashlib.sha256(Path(__file__).read_bytes()).hexdigest()}
        self.fixtures = args.fixtures.resolve()
        self.source = args.source.resolve()
        self._sequence = 0
        self._applications = {}

    def snapshot_product(self, canonical, form):
        if not canonical.is_file(): raise RuntimeError('Actual translated product missing: ' + str(canonical))
        original = {p.relative_to(canonical.parent).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
                    for p in source_files(canonical.parent)}
        destination = self.work / 'products' / form / canonical.parent.name
        shutil.copytree(canonical.parent, destination, ignore=ignore_build_caches)
        copied = {p.relative_to(destination).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
                  for p in source_files(destination)}
        current = {p.relative_to(canonical.parent).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
                   for p in source_files(canonical.parent)}
        if copied != original or current != original:
            raise RuntimeError('Canonical promotion overlapped product snapshot creation: ' + str(canonical))
        project = destination / canonical.name
        relocate_project(project, canonical.parent)
        receipt = {'form': form, 'canonical_project': str(canonical), 'private_project': str(project),
                   'canonical_sha256': original, 'relocated_project_sha256': hashlib.sha256(project.read_bytes()).hexdigest()}
        self.report['product_snapshots'].append(receipt)
        (self.output / (form + '-product-snapshot.json')).write_text(json.dumps(receipt, indent=2) + '\n')
        return project

    def checkpoint(self):
        progress = dict(self.report, status='running', counts={s: sum(r['status'] == s for r in self.report['rows'])
                        for s in ('passed', 'failed', 'unrun', 'excluded', 'support')})
        temporary = self.output / 'progress.json.tmp'
        temporary.write_text(json.dumps(progress, indent=2) + '\n')
        temporary.replace(self.output / 'progress.json')

    def command(self, command, *, cwd=ROOT, timeout=300):
        self._sequence += 1
        log = self.output / f'{self._sequence:05d}.log'
        command = [str(p) for p in command]
        start = time.time()
        with log.open('w') as stream:
            try:
                result = subprocess.run(command, cwd=cwd, stdout=stream, stderr=subprocess.STDOUT,
                                        timeout=timeout, env=dict(os.environ))
                code = result.returncode
            except subprocess.TimeoutExpired:
                code = 'timeout'
        self.report['commands'].append({'command': command, 'cwd': str(cwd), 'exit_code': code,
                                       'started': start, 'finished': time.time(), 'log': str(log)})
        if code != 0:
            raise RuntimeError(f'Command failed ({code}): {log}')
        return log

    def build(self, project, product, form, mode):
        authored = authored_sources(ROOT, project.stem == 'ManagedTests')
        prefix = self.work / form / mode / project.stem
        artifacts = prefix / 'artifacts'
        argv = ['dotnet', 'publish' if mode == 'aot' else 'build', str(project), '-c', 'Release',
                '--artifacts-path', str(artifacts), '-p:S2nTlsProject=' + str(product), '-m:1', '-p:UseSharedCompilation=false']
        if self.args.restore == 'none':
            argv += ['--no-restore']
        elif self.args.restore == 'locked':
            argv += ['-p:RestoreLockedMode=true']
        if mode == 'aot':
            argv += ['-r', self.args.rid, '--self-contained', 'true', '-p:PublishAot=true',
                     '-o', str(prefix / 'publish')]
        self.command(argv, timeout=1200)
        if authored_sources(ROOT, project.stem == 'ManagedTests') != authored:
            raise RuntimeError('Authored source changed while the actual application was building')
        name = project.stem
        if mode == 'aot':
            executable = prefix / 'publish' / name
            command = [str(executable)]
        else:
            executable = artifacts / 'bin' / name / 'release' / (name + '.dll')
            command = ['dotnet', str(executable)]
        if not executable.is_file():
            raise RuntimeError('Build did not produce the expected ordinary application: ' + str(executable))
        identity_log = self.command([*command, '--identity'], timeout=120)
        runtime = json.loads(identity_log.read_text())
        snapshot = next(r for r in self.report['product_snapshots'] if r['form'] == form)
        record = capture(command, artifacts, runtime, authored, snapshot)
        record.update(form=form, mode=mode, project=str(project))
        receipt = self.output / (form + '-' + mode + '-' + project.stem + '-execution.json')
        receipt.write_text(json.dumps(record, indent=2) + '\n')
        self.report['application_builds'].append({'receipt': str(receipt), 'sha256': hashlib.sha256(receipt.read_bytes()).hexdigest(), 'form': form, 'mode': mode, 'project': str(project)})
        self._applications[tuple(command)] = (record, str(receipt))
        return command

    def exchange(self, suite, version, topology, command, form, mode, validation='validate'):
        row = {'suite': 'interop' if 'openssl' in topology else 'compatibility', 'form': form, 'mode': mode,
               'cipher': suite['name'], 'iana_id': suite['iana_id'], 'version': version,
               'topology': topology, 'validation': validation, 'status': 'failed', 'commands': [], 'logs': []}
        self.report['rows'].append(row)
        left, right = socket.socketpair()
        procs = []
        try:
            application, receipt = self._applications[tuple(command)]
            verify(application)
            row['application_execution_receipt'] = receipt
            fixture_paths = [p for p in self.fixtures.rglob('*') if p.is_file()] + [self.source / 'tests/pems/dhparams_2048.pem']
            row['fixture_sha256'] = hashes(fixture_paths)
            native_paths = []
            client, server = topology.split('-')
            for engine, role, descriptor in ((server, 'server', right), (client, 'client', left)):
                if engine == 'managed':
                    argv = [*command, '--endpoint']
                else:
                    name = 'openssl-sslv3-endpoint' if version == 'S2N_SSLv3' else 'openssl-endpoint'
                    endpoint = ROOT / 'build/native' / name
                    if not endpoint.is_file():
                        raise RuntimeError('Proven independent peer missing: ' + str(endpoint))
                    native_paths.append(endpoint)
                    row.setdefault('native_endpoint_sha256', {}).update(hashes([endpoint]))
                    argv = [str(endpoint)]
                argv += [role, str(descriptor.fileno()), version, suite['iana_id'], suite['name'],
                         str(self.fixtures), str(self.source), validation]
                self._sequence += 1
                log = self.output / f'{self._sequence:05d}-{role}.log'
                stream = log.open('w')
                proc = subprocess.Popen(argv, pass_fds=(descriptor.fileno(),), stdout=stream, stderr=subprocess.STDOUT)
                procs.append((proc, stream))
                row['commands'].append(argv); row['logs'].append(str(log))
            left.close(); right.close()
            deadline = time.monotonic() + 45
            for proc, stream in procs:
                try:
                    proc.wait(timeout=max(0.01, deadline - time.monotonic()))
                except subprocess.TimeoutExpired:
                    proc.kill(); proc.wait()
                stream.close()
            row['exit_codes'] = [p.returncode for p, _ in procs]
            if hashes(fixture_paths) != row['fixture_sha256']:
                raise RuntimeError('TLS fixture identities changed during the exchange')
            if native_paths and hashes(native_paths) != row['native_endpoint_sha256']:
                raise RuntimeError('Independent native endpoint identity changed during execution')
            row['observations'] = []
            for log in row['logs']:
                row['observations'] += [json.loads(line) for line in Path(log).read_text().splitlines() if line.startswith('{')]
            if validation == 'untrusted':
                if all(code != 0 for code in row['exit_codes']) and 'S2N_ERR_CERT_UNTRUSTED' in Path(row['logs'][1]).read_text():
                    row['status'] = 'passed'; row['expected_rejection'] = 'S2N_ERR_CERT_UNTRUSTED'
                return
            observations = row['observations']
            if row['exit_codes'] != [0, 0] or len(observations) != 2:
                raise RuntimeError('Endpoint failed or omitted negotiation/payload/shutdown observations')
            for observation, engine, role in zip(observations, (server, client), ('server', 'client')):
                expected_engine = 'translated-s2n' if engine == 'managed' else 'openssl-peer'
                if (observation['engine'] != expected_engine or observation['role'] != role or observation['version'] != version
                    or observation['iana_id'].lower() != suite['iana_id'].lower() or observation['sent'] != 1027
                    or observation['received'] != 1027 or observation['shutdown'] is not True):
                    raise RuntimeError('Exact forced negotiation, role, binary length or close_notify assertion differs')
            row['status'] = 'passed'
        except Exception as error:
            row['error'] = str(error)
        finally:
            left.close(); right.close()
            for proc, stream in procs:
                if proc.poll() is None:
                    proc.kill(); proc.wait()
                stream.close()
            self.checkpoint()

    def run_application(self, suite, command, arguments, form, mode, name):
        row = {'suite': suite, 'case': name, 'form': form, 'mode': mode, 'status': 'failed'}
        self.report['rows'].append(row)
        try:
            application, receipt = self._applications[tuple(command)]
            verify(application)
            row['application_execution_receipt'] = receipt
            fixture_paths = [p for p in self.fixtures.rglob('*') if p.is_file()] + [self.source / 'tests/pems/dhparams_2048.pem']
            row['fixture_sha256'] = hashes(fixture_paths)
            log = self.command([*command, *arguments], timeout=120)
            if hashes(fixture_paths) != row['fixture_sha256']:
                raise RuntimeError('TLS fixture identities changed during application execution')
            transcript = log.read_text()
            if suite == 'lifecycle' and ('"case":"' + name + '","status":"passed"') not in transcript:
                raise RuntimeError('Missing exact lifecycle completion observation')
            if suite == 'consumer' and 'PASS: SSLv3/RC4, SSLv3/3DES, TLS1.3' not in transcript:
                raise RuntimeError('Missing complete ordinary consumer assertion transcript')
            row.update(status='passed', log=str(log))
        except Exception as error:
            row['error'] = str(error)
        self.checkpoint()

    def unrun(self, suites, form, mode, reason):
        for suite in suites:
            if suite == 'rooted-aot' and mode != 'aot': continue
            if any(r.get('suite') == suite and r.get('form') == form and r.get('mode') == mode for r in self.report['rows']): continue
            if suite == 'upstream':
                config = load(ROOT / 'config/upstream-tests.json')
                for case in config['cases']:
                    self.report['rows'].append({'suite': suite, 'case': case['path'], 'form': form, 'mode': mode,
                        'status': case['status'] if case['status'] in ('excluded', 'support') else 'unrun',
                        'reason': case['reason'] if case['status'] in ('excluded', 'support') else reason,
                        'assertion_sites': case['assertion_sites'], 'skipped': 0})
                continue
            names = LIFECYCLE if suite == 'lifecycle' else ('required workload',)
            if suite in ('compatibility', 'interop'):
                for cipher, version in required_pairs():
                    roles = ('client', 'server') if suite == 'interop' else ('both',)
                    for role in roles:
                        self.report['rows'].append({'suite': suite, 'form': form, 'mode': mode,
                            'cipher': cipher['name'], 'iana_id': cipher['iana_id'], 'version': version,
                            'role': role, 'status': 'unrun', 'reason': reason})
            else:
                for name in names:
                    self.report['rows'].append({'suite': suite, 'case': name, 'form': form, 'mode': mode,
                                               'status': 'unrun', 'reason': reason})

    def run(self):
        selected = self.args.suite or ['consumer']
        suites = SUITES if 'all' in selected else tuple(dict.fromkeys(selected))
        forms = ('raw', 'processed') if self.args.form == 'all' else (self.args.form,)
        modes = ('jit', 'aot') if self.args.mode == 'all' else (self.args.mode,)
        # Freeze every selected canonical tree before the first long-running
        # workload, so later promotion cannot change a later form/mode build.
        products = {}
        for form in forms:
            canonical = self.args.product or ROOT / 'generated' / ('TranslatedS2nTls.Raw' if form == 'raw' else 'TranslatedS2nTls') / 'TranslatedS2nTls.csproj'
            try: products[form] = self.snapshot_product(canonical.resolve(), form)
            except Exception as error: products[form] = error
        for form in forms:
            for mode in modes:
                if suites == ('rooted-aot',) and mode != 'aot': continue
                try:
                    product = products[form]
                    if isinstance(product, Exception): raise product
                    if not product.is_file():
                        raise RuntimeError('Actual translated product missing: ' + str(product))
                    if self.args.rid != 'linux-x64':
                        raise RuntimeError('Only Linux x64 is selected; execution cannot be inferred on another target')
                    for path in (self.fixtures / 'rsa.pem', self.fixtures / 'rsa.key', self.fixtures / 'ecdsa.pem',
                                 self.fixtures / 'ecdsa.key', self.source / 'tests/pems/dhparams_2048.pem'):
                        if not path.is_file():
                            raise RuntimeError('Declared fixture prerequisite missing: ' + str(path))
                    if any(s in suites for s in ('consumer', 'compatibility', 'interop')):
                        app = self.build(ROOT / 'samples/ManagedConsumer/ManagedConsumer.csproj', product, form, mode)
                        if 'consumer' in suites:
                            self.run_application('consumer', app, ['--fixtures', self.fixtures, '--source', self.source], form, mode, 'legacy-modern-loopback')
                        for suite in ('compatibility', 'interop'):
                            if suite not in suites: continue
                            topologies = ('managed-managed',) if suite == 'compatibility' else ('managed-openssl', 'openssl-managed')
                            for cipher, version in required_pairs():
                                for topology in topologies:
                                    self.exchange(cipher, version, topology, app, form, mode)
                            if suite == 'compatibility':
                                for identifier, version in (('0x0004', 'S2N_SSLv3'), ('0x1301', 'S2N_TLS13')):
                                    cipher = next(s for s, v in required_pairs() if s['iana_id'].lower() == identifier and v == version)
                                    for validation in ('untrusted', 'bypass-untrusted'):
                                        self.exchange(cipher, version, 'managed-managed', app, form, mode, validation)
                    if 'lifecycle' in suites:
                        app = self.build(ROOT / 'tests/Managed/ManagedTests.csproj', product, form, mode)
                        for name in LIFECYCLE:
                            self.run_application('lifecycle', app, ['--case', name, '--fixtures', self.fixtures, '--source', self.source], form, mode, name)
                    if 'upstream' in suites:
                        path = self.output / (form + '-' + mode + '-upstream.json')
                        argv = [sys.executable, ROOT / 'tests/Upstream/run.py', '--form', form, '--mode', mode,
                                '--executables', self.args.upstream_executables, '--output', path]
                        if self.args.native_report: argv += ['--native-report', self.args.native_report]
                        if self.args.native_equivalence: argv += ['--native-equivalence', self.args.native_equivalence]
                        if self.args.c_report: argv += ['--c-report', self.args.c_report]
                        if self.args.campaign_run: argv += ['--campaign-run', self.args.campaign_run]
                        argv += ['--python-executables', self.args.python_executables, '--python', self.args.python, '--python-workers', self.args.python_workers]
                        if self.args.peer_commands: argv += ['--peer-commands', self.args.peer_commands]
                        for python_report in self.args.python_report or []: argv += ['--python-report', python_report]
                        for native_python_report in self.args.native_python_report or []: argv += ['--native-python-report', native_python_report]
                        try: self.command(argv, timeout=262800)
                        finally:
                            if path.is_file(): self.report['rows'] += load(path)['cases']
                    for delivery in ('delivery', 'rooted-aot'):
                        if delivery not in suites: continue
                        if delivery == 'rooted-aot' and mode != 'aot': continue
                        path = self.output / (form + '-' + mode + '-' + delivery + '.json')
                        argv = [sys.executable, ROOT / 'tests/Delivery/run.py', '--product', product, '--form', form, '--mode', mode,
                                '--fixtures', self.fixtures, '--source', self.source, '--output', path]
                        if delivery == 'rooted-aot': argv += ['--rooted']
                        try: self.command(argv, timeout=1800)
                        finally:
                            if path.is_file(): self.report['rows'] += load(path)['rows']
                except Exception as error:
                    self.report['errors'].append({'form': form, 'mode': mode, 'error': str(error)})
                    self.unrun(suites, form, mode, str(error))
        counts = {status: sum(r['status'] == status for r in self.report['rows']) for status in ('passed', 'failed', 'unrun', 'excluded', 'support')}
        self.report['counts'] = counts
        if not self.report['errors'] and not counts['failed'] and not counts['unrun'] and counts['passed']:
            self.report['status'] = 'passed'
        path = self.output / 'report.json'
        path.write_text(json.dumps(self.report, indent=2) + '\n')
        print('REPORT ' + str(path))
        print(json.dumps({'status': self.report['status'], 'counts': counts, 'errors': self.report['errors']}))
        return 0 if self.report['status'] == 'passed' else 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--suite', choices=(*SUITES, 'all'), action='append', help='Repeat to share private builds across suites; default consumer')
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='all')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='all')
    parser.add_argument('--rid', default='linux-x64')
    parser.add_argument('--product', type=Path)
    parser.add_argument('--output', type=Path)
    parser.add_argument('--fixtures', type=Path, default=ROOT / 'build/native/credentials')
    parser.add_argument('--source', type=Path, default=ROOT / 'ref/s2n-tls')
    parser.add_argument('--restore', choices=('normal', 'locked', 'none'), default='normal')
    parser.add_argument('--upstream-executables', type=Path, default=ROOT / 'build/managed-upstream/executables.json')
    parser.add_argument('--native-report', type=Path)
    parser.add_argument('--native-equivalence', type=Path)
    parser.add_argument('--c-report', type=Path)
    parser.add_argument('--campaign-run', type=Path)
    parser.add_argument('--python-report', type=Path, action='append')
    parser.add_argument('--python-executables', type=Path, default=ROOT / 'build/managed-upstream/python-endpoints.json')
    parser.add_argument('--peer-commands', type=Path)
    parser.add_argument('--python', default=sys.executable)
    parser.add_argument('--python-workers', type=int, default=1)
    parser.add_argument('--native-python-report', type=Path, action='append')
    args = parser.parse_args()
    if args.product and args.form == 'all': parser.error('--product requires one --form')
    return Qualification(args).run()


if __name__ == '__main__':
    raise SystemExit(main())
