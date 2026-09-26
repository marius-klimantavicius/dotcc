#!/usr/bin/env python3
"""Qualify the real public ProjectReference consumer; serialize all builds/runs."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import signal
import struct
import subprocess
import tempfile
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
import sys
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import policy, observed_digest
from provenance import picotls_provenance
REPO, PICO = ROOT.parent, ROOT.parent / 'picotls'
PROJECT = ROOT / 'samples/ManagedConsumer/ManagedConsumer.csproj'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
parser.add_argument('--jit-only', action='store_true')
args = parser.parse_args()
if len(args.variants) != len(set(args.variants)):
    parser.error('Variants must be unique')
FULL = set(args.variants) == {'raw', 'optimized'} and not args.jit_only
LOGROOT = ROOT / 'artifacts/public-consumer'
LOGROOT.mkdir(parents=True, exist_ok=True)
LOG = Path(tempfile.mkdtemp(prefix='run-', dir=LOGROOT))
BUILD = ROOT / 'build/public-consumer' / LOG.name
BUILD.mkdir(parents=True)
receipt = dict(passed=False, targeted_passed=False, exact_source_metadata_required=FULL,
               scope='Public ProjectReference stream/FIN/resumption and authentication consumer',
               uncovered=['Full selected API coverage belongs to the P8 matrix'],
               commands=[], variants=[], cases=[], logs=str(LOG))


def sha(path):
    return observed_digest(path)


def snapshot(paths):
    return {os.path.relpath(p, ROOT): sha(p) for p in sorted(set(paths))}


def generated(directory, recorded):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    if not names or len(names) != len(set(names)) or any(Path(n).name != n or not n.endswith('.cs') for n in names):
        raise RuntimeError('Invalid source manifest: ' + str(directory))
    # The SDK compiles nested sources by default, so a top-level glob misses
    # stale inputs that are absent from the compiler's source manifest.
    actual = {str(p.relative_to(directory)) for p in directory.rglob('*.cs')
              if not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
    if set(names) != actual:
        raise RuntimeError('Unmanifested or missing generated sources')
    current = {n: sha(directory / n) for n in names}
    if current != {n: h for n, h in recorded.items() if n.endswith('.cs')}:
        policy().issue('Generated inputs differ from translation receipt')
    for name, digest in recorded.items():
        if Path(name).name != name:
            raise RuntimeError('Unsafe provenance path: ' + name)
        if sha(directory / name) != digest:
            policy().issue('Changed generated project/manifest/source: ' + name)
    return {p.name: sha(p) for p in sorted(directory.iterdir()) if p.is_file() and p.suffix in ('.cs', '.csproj', '.txt')}


def run(command, name, environment=None, timeout=900, expected=0):
    command = list(map(str, command))
    receipt['commands'].append(dict(name=name, arguments=command))
    print('RUN ' + name, flush=True)
    with (LOG / (name + '.log')).open('w') as output:
        result = subprocess.run(command, stdout=output, stderr=subprocess.STDOUT, env=environment, timeout=timeout)
    if result.returncode != expected:
        raise RuntimeError(name + ' exit ' + str(result.returncode) + '; see ' + str(LOG))


def records(path):
    return [json.loads(line) for line in path.read_text().splitlines() if line.startswith('{')]


def one(items, key):
    matches = [item for item in items if key in item]
    if len(matches) != 1:
        raise RuntimeError('Missing/ambiguous ' + key + ' receipt')
    return matches[0]


def metadata(items, runtime, expected_alpn='dotcc-public-sample'):
    value = one(items, 'metadata')
    if value['aot'] != (runtime == 'aot') or value['library_version'] != '2.7.0' or value['tls_provider'] != 'picotls' or value['effective_versions'] != [1]:
        raise RuntimeError('Incorrect executable/runtime/profile identity')
    if FULL and value['source_revision'] != pin['commit']:
        raise RuntimeError('Actual source metadata does not match pin')
    if value.get('alpn') != expected_alpn:
        raise RuntimeError('Sample did not configure the intended application protocol')
    return value


def binary_hashes(directory):
    files = [p for p in directory.rglob('*') if p.is_file()]
    return {str(p.relative_to(directory)): sha(p) for p in sorted(files)}


def check_native(path):
    header = path.read_bytes()[:64]
    if header[:6] != b'\x7fELF\x02\x01' or struct.unpack_from('<H', header, 18)[0] != 62:
        raise RuntimeError('Published consumer is not a Linux x64 ELF executable')


def wait_ready(server, ready):
    deadline = time.monotonic() + 25
    while time.monotonic() < deadline:
        if server.poll() is not None:
            raise RuntimeError('Server exited before listener readiness')
        if ready.exists() and ready.read_text().strip():
            port = int(ready.read_text().strip())
            if not 0 < port < 65536:
                raise RuntimeError('Invalid listening port')
            return port
        time.sleep(0.01)
    raise TimeoutError('Server readiness timeout')


def pair(command, variant, runtime, family, certificates, untrusted, environment, negative=None):
    name = '-'.join([variant, runtime, family, negative or 'roundtrip-resumption'])
    case = dict(name=name, passed=False, variant=variant, runtime=runtime, family=family,
                negative=negative, processes=[])
    receipt['cases'].append(case)
    ip = '127.0.0.1' if family == 'ipv4' else '::1'
    ready = BUILD / (name + '.ready')
    server_path, client_path = LOG / (name + '-server.log'), LOG / (name + '-client.log')
    processes = []
    with tempfile.TemporaryDirectory(prefix='public-quic-') as temporary:
        env = dict(environment, TMPDIR=temporary)
        try:
            with server_path.open('w') as server_log, client_path.open('w') as client_log:
                rounds = '1' if negative else '2'
                server_command = [*command, 'server', str(certificates / 'server.pem'), str(certificates / 'server-key.pem'), ip, '0', str(ready), rounds]
                receipt['commands'].append(dict(name=name + '-server', arguments=server_command))
                server = subprocess.Popen(server_command, stdout=server_log, stderr=subprocess.STDOUT, env=env)
                processes.append(server)
                port = wait_ready(server, ready)
                trust = (untrusted if negative == 'wrong-trust' else certificates) / 'root.pem'
                hostname = 'wrong.example.invalid' if negative == 'wrong-name' else 'localhost'
                client_command = [*command, 'client', str(trust), hostname, ip, str(port), rounds]
                if negative == 'wrong-alpn':
                    client_command.append('--alpn=dotcc-public-mismatch')
                receipt['commands'].append(dict(name=name + '-client', arguments=client_command))
                client = subprocess.Popen(client_command, stdout=client_log, stderr=subprocess.STDOUT, env=env)
                processes.append(client)
                case['client_exit'] = client.wait(timeout=80)
                if negative and server.poll() is None:
                    # The rejected connection does not fulfill the listener's
                    # accept. Cancel its public wait and require graceful drain.
                    server.send_signal(signal.SIGINT)
                case['server_exit'] = server.wait(timeout=25)
            client_records, server_records = records(client_path), records(server_path)
            case['client_metadata'] = metadata(client_records, runtime,
                'dotcc-public-mismatch' if negative == 'wrong-alpn' else 'dotcc-public-sample')
            case['server_metadata'] = metadata(server_records, runtime)
            case['client'], case['server'] = one(client_records, 'passed'), one(server_records, 'passed')
            if negative:
                failed = case['client']
                alert = failed.get('tls_alert')
                certificate_status = failed.get('status') in (200000513, 200000514, 200000515)
                actual_alert = isinstance(alert, int) and (40 <= alert <= 49 or alert == 116)
                # Pinned core connection.c maps CRYPTO_NO_APPLICATION_PROTOCOL
                # (0x100 + TLS alert120) to the Linux ALPN status92.
                rejection = (failed.get('status') == 92 and failed.get('transport_error') == 0x178 and alert == 120) if negative == 'wrong-alpn' else (certificate_status or actual_alert)
                if case['client_exit'] != 1 or failed.get('role') != 'client' or failed.get('error_kind') != 'transport' or not rejection:
                    raise RuntimeError('Negative case lacks its actual role-specific TLS/ALPN rejection')
                if failed.get('application_bytes_sent') != 0 or failed.get('application_bytes_received') != 0:
                    raise RuntimeError('Negotiation/authentication rejection admitted application data')
                if case['server_exit'] != 1 or case['server'].get('error_kind') not in ('canceled', 'transport'):
                    raise RuntimeError('Rejected server did not drain after cancel/error')
                if case['server'].get('role') != 'server' or case['server'].get('application_bytes_sent') != 0 or case['server'].get('application_bytes_received') != 0:
                    raise RuntimeError('Rejected server admitted application data or reported the wrong role')
                if negative == 'wrong-alpn' and case['server'].get('error_kind') != 'canceled':
                    raise RuntimeError('ALPN mismatch unexpectedly escaped the listener lookup into an accepted connection')
                if any('round' in r for r in client_records + server_records):
                    raise RuntimeError('Negative case completed an application exchange')
            else:
                if case['client_exit'] != 0 or case['server_exit'] != 0:
                    raise RuntimeError('Public consumer failed')
                for role, items in [('client', client_records), ('server', server_records)]:
                    final = case[role]
                    if not final['passed'] or final['connections'] != 2 or final['role'] != role:
                        raise RuntimeError('Incomplete public consumer success record')
                    rounds = [r for r in items if 'round' in r]
                    if len(rounds) != 2:
                        raise RuntimeError('Missing full or resumed connection')
                    for index, observed in enumerate(rounds):
                        if observed != dict(role=role, round=index, bytes_sent=65537, bytes_received=65537, fin_received=True, resumed=index != 0):
                            raise RuntimeError('Payload/FIN/resumption mismatch')
                    case[role + '_rounds'] = rounds
            if not case['client']['clean_close'] or not case['server']['clean_close']:
                raise RuntimeError('Consumer owners did not complete disposal')
            case['passed'] = True
            print(name + ': PASS', flush=True)
        finally:
            for process in processes:
                if process.poll() is None:
                    process.kill()  # cleanup only; never counts as successful drain
                process.wait()
                case['processes'].append(dict(pid=process.pid, exit=process.returncode))


try:
    closure_path, pico_path = ROOT / 'config/product-closure.json', PICO / 'artifacts/campaign/current-default.json'
    closure, pico = json.loads(closure_path.read_text()), picotls_provenance(PICO)
    pin = json.loads((ROOT / 'config/source.json').read_text())
    profile = json.loads((ROOT / 'config/api-profile.json').read_text())
    if closure['revision'] != pin['commit'] or profile['revision'] != pin['commit'] or closure['api_profile_sha256'] != sha(ROOT / 'config/api-profile.json'):
        policy().issue('Source/profile/product closure mismatch')
    receipt.update(product_closure_sha256=sha(closure_path), picotls_translation_sha256=sha(pico_path),
                   profile_sha256=sha(ROOT / 'config/api-profile.json'), source_revision=pin['commit'])
    # This must remain a genuine consuming assembly, not another source-linked
    # facade harness or a friend assembly with a convenient name.
    project_tree = ET.parse(PROJECT)
    refs = project_tree.findall('.//ProjectReference')
    if len(refs) != 1 or (PROJECT.parent / refs[0].attrib['Include']).resolve() != ROOT / 'src/ManagedApi/ManagedApi.csproj':
        raise RuntimeError('Sample must reference the actual ManagedApi project')
    if project_tree.findtext('.//AssemblyName') != 'PublicQuicSample' or project_tree.findtext('.//AllowUnsafeBlocks') != 'false':
        raise RuntimeError('Public consumer identity/unsafe contract changed')
    if project_tree.findall('.//Compile') or project_tree.findall('.//TrimmerRootAssembly') or project_tree.findall('.//TrimmerRootDescriptor'):
        raise RuntimeError('Consumer must not source-link or add trimming roots')
    paths = [Path(__file__).resolve(), closure_path, pico_path, ROOT / 'config/source.json', ROOT / 'config/api-profile.json']
    stage = ROOT / 'build/product-source'
    if sha(stage / 'manifest.json') != closure['stage_manifest_sha256']:
        policy().issue('MsQuic staged source manifest differs from the closure')
    paths.append(stage / 'manifest.json')
    for item in closure['source_files']:
        path = stage / item['path']
        if sha(path) != item['sha256']:
            policy().issue('Changed staged MsQuic source: ' + item['path'])
        paths.append(path)
    for directory in [PROJECT.parent, ROOT / 'src/ManagedApi', ROOT / 'src/BclHost', PICO / 'src/BclProvider']:
        paths += [p for p in directory.rglob('*') if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts) and p.suffix in ('.cs', '.csproj', '.props', '.targets', '.xml')]
    # Bind ancestor MSBuild policy and SDK/package selection for every project.
    for base in [PROJECT.parent, ROOT / 'src/ManagedApi', ROOT / 'src/BclHost', PICO / 'src/BclProvider', ROOT / 'generated', PICO / 'generated']:
        for directory in [base, *base.parents]:
            for name in ['Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'global.json', 'NuGet.Config', 'nuget.config']:
                path = directory / name
                if path.is_file(): paths.append(path)
            if directory == REPO: break
    for name, digest in {**pico['core_source_sha256'], **pico['upstream_header_sha256']}.items():
        path = PICO / 'ref' / pico['inputs']['picotls']['directory'] / name
        if sha(path) != digest: policy().issue('Changed pinned picotls source: ' + name)
        paths.append(path)
    for item in pico['host_sources']:
        path = PICO / item['path']
        if sha(path) != item['sha256']: policy().issue('Changed picotls host source')
        paths.append(path)
    for variant in args.variants:
        msquic = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
        picotls = PICO / 'generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')
        ms_hashes = generated(msquic, closure['generated'][variant])
        pico_hashes = generated(picotls, pico[variant])
        paths += [msquic / name for name in ms_hashes] + [picotls / name for name in pico_hashes]
    frozen = snapshot(paths)
    receipt['input_sha256'] = frozen
    environment = dict(os.environ)
    environment.pop('SSLKEYLOGFILE', None)
    if FULL: environment['DOTCC_REQUIRED_SOURCE_REVISION'] = pin['commit']
    else: environment.pop('DOTCC_REQUIRED_SOURCE_REVISION', None)
    for variant in args.variants:
        msquic = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
        picotls = PICO / 'generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')
        properties = ['-p:MsQuicProject=' + str(msquic / 'TranslatedMsQuic.csproj'),
                      '-p:PicotlsProject=' + str(picotls / 'TranslatedPicotls.csproj'),
                      '-p:UseArtifactsOutput=true', '-p:ArtifactsPath=' + str(BUILD / variant / 'artifacts')]
        entry = dict(name=variant, passed=False, generated_sha256=generated(msquic, closure['generated'][variant]),
                     picotls_generated_sha256=generated(picotls, pico[variant]), runtimes={})
        receipt['variants'].append(entry)
        jit = BUILD / variant / 'jit'
        run(['dotnet', 'build', PROJECT, '-c', 'Release', '--nologo', '-o', jit, *properties], variant + '-build', environment)
        commands = [('jit', ['dotnet', str(jit / 'PublicQuicSample.dll')], jit)]
        if not args.jit_only:
            aot = BUILD / variant / 'aot'
            run(['dotnet', 'publish', PROJECT, '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true',
                 '-p:PublishAot=true', '-p:PublishTrimmed=true', '-p:ILLinkTreatWarningsAsErrors=true',
                 '-p:IlcTreatWarningsAsErrors=true', '-o', aot, '--nologo', *properties], variant + '-publish', environment)
            check_native(aot / 'PublicQuicSample')
            commands.append(('aot', [str(aot / 'PublicQuicSample')], aot))
        for runtime, command, output in commands:
            binaries = binary_hashes(output)
            certificates, untrusted = BUILD / (variant + '-' + runtime + '-certificates'), BUILD / (variant + '-' + runtime + '-untrusted')
            run([*command, 'certificates', certificates], variant + '-' + runtime + '-certificates', environment, timeout=30)
            run([*command, 'certificates', untrusted], variant + '-' + runtime + '-untrusted', environment, timeout=30)
            for family in ['ipv4', 'ipv6']:
                pair(command, variant, runtime, family, certificates, untrusted, environment)
                for negative in ['wrong-trust', 'wrong-name', 'wrong-alpn']:
                    pair(command, variant, runtime, family, certificates, untrusted, environment, negative)
            if binary_hashes(output) != binaries: policy().issue('Executed binaries changed during matrix')
            entry['runtimes'][runtime] = dict(passed=True, binary_sha256=binaries, output=str(output))
        if snapshot(paths) != frozen: raise RuntimeError('Product/sample/dependency inputs changed during matrix')
        entry['passed'] = True
    # Rediscover sources after execution: hashing the original path list alone
    # cannot detect a new nested file introduced during the matrix.
    for variant in args.variants:
        generated(ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic'), closure['generated'][variant])
        generated(PICO / 'generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls'), pico[variant])
    receipt['targeted_passed'] = True
    receipt['passed'] = FULL
except BaseException as error:
    receipt['error'] = str(error)
    raise
finally:
    encoded = json.dumps(receipt, indent=2) + '\n'
    (LOG / 'results.json').write_text(encoded)
    (LOGROOT / 'results.json').write_text(encoded)
print(json.dumps(dict(passed=receipt['passed'], targeted_passed=receipt['targeted_passed'], cases=len(receipt['cases']))))
