#!/usr/bin/env python3
"""Exercise the translated core through typed callbacks and separate peer processes.

Prerequisites are current P2/P3/P4/P5 receipts. Native libmsquic is linked only
into the independent oracle executable; ManagedPeer references the BCL host.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import resource
import subprocess
import tempfile
import time
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
parser = argparse.ArgumentParser()
parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
parser.add_argument('--jit-only', action='store_true')
parser.add_argument('--roles', nargs='+', choices=['both', 'client', 'server'], default=['both', 'client', 'server'])
parser.add_argument('--tls-receipt', type=Path, default=ROOT / 'artifacts/tls-adapter/results.json')
args = parser.parse_args()
BUILD = ROOT / 'build/managed-peer'
LOGS = ROOT / 'artifacts/managed-peer'
BUILD.mkdir(parents=True, exist_ok=True)
LOGS.mkdir(parents=True, exist_ok=True)
resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
receipt = dict(passed=False, phase='P6 pending', commands=[], prerequisites={}, cases=[])


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def generated_hashes(variant):
    return {p.name: sha(p) for p in sorted((ROOT / 'generated' / variant / 'TranslatedMsQuic').glob('*.cs'))}


def picotls_directory(variant):
    return REPO / 'picotls/generated' / ('TranslatedPicotlsRaw' if variant == 'raw' else 'TranslatedPicotls')


def picotls_hashes(variant):
    return {p.name: sha(p) for p in sorted(picotls_directory(variant).glob('*.cs'))}


def validate_generated_record(name, hashes, directory, expected_sources):
    if not isinstance(hashes, dict) or {key: value for key, value in hashes.items() if key.endswith('.cs')} != expected_sources:
        raise RuntimeError(name + ' does not bind the current generated sources')
    # Product receipts also bind the project and source manifest. Every recorded
    # file must match, even though runtime receipts can record only C# inputs.
    for relative, digest in hashes.items():
        source = directory / relative
        if not source.is_file() or sha(source) != digest:
            raise RuntimeError(name + ' references changed generated input ' + relative)


def require_receipt(name, path):
    evidence = json.loads(path.read_text())
    if not evidence.get('passed'):
        raise RuntimeError(name + ' is not validated')
    closure = evidence.get('closure_sha256', evidence.get('product_closure_sha256'))
    if closure is not None and closure != sha(ROOT / 'config/product-closure.json'):
        raise RuntimeError(name + ' has a stale closure receipt')
    for variant in args.variants:
        match = next((item for item in evidence.get('variants', []) if item['name'] == variant), None)
        if match is None or not match.get('passed'):
            raise RuntimeError(name + ' lacks ' + variant + ' qualification')
        hashes = match.get('generated_hashes', match.get('generated_sha256'))
        validate_generated_record(name + ' ' + variant, hashes,
            ROOT / 'generated' / variant / 'TranslatedMsQuic', generated_hashes(variant))
        if name == 'tls-adapter':
            validate_generated_record(name + ' picotls ' + variant, match.get('picotls_generated_sha256'),
                picotls_directory(variant), picotls_hashes(variant))
        runtimes = match.get('runtimes')
        if not args.jit_only and runtimes is not None and not {'jit', 'aot'}.issubset(runtimes):
            raise RuntimeError(name + ' lacks NativeAOT qualification')
    for relative, digest in evidence.get('source_hashes', evidence.get('input_sha256', {})).items():
        path_to_source = ROOT / relative
        if not path_to_source.exists() or sha(path_to_source) != digest:
            raise RuntimeError(name + ' references changed input ' + relative)
    receipt['prerequisites'][name] = dict(path=str(path), sha256=sha(path))


def run(command, name, environment=None, timeout=600):
    command = [str(item) for item in command]
    receipt['commands'].append(dict(name=name, arguments=command))
    result = subprocess.run(command, text=True, capture_output=True, env=environment, timeout=timeout)
    (LOGS / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(name + ' failed; see ' + str(LOGS / (name + '.log')))
    return result.stdout


def wait_ready(process, path):
    deadline = time.monotonic() + 20
    while time.monotonic() < deadline:
        if process.poll() is not None:
            raise RuntimeError('Server exited before readiness')
        if path.exists() and path.read_text().strip():
            port = int(path.read_text().strip())
            if not 0 < port < 65536:
                raise RuntimeError('Invalid listener port')
            return port
        time.sleep(0.01)
    raise TimeoutError('Server readiness timed out')


def parse_result(path):
    matches = [json.loads(line) for line in path.read_text().splitlines() if line.startswith('{"passed":')]
    if len(matches) != 1:
        raise RuntimeError('Missing or ambiguous peer result: ' + str(path))
    return matches[0]


def exchange(managed, variant, runtime, algorithm, cipher, family, managed_role):
    name = '-'.join([variant, runtime, algorithm, cipher, family, managed_role])
    ready = BUILD / (name + '.ready')
    ready.unlink(missing_ok=True)
    certificate = BUILD / (algorithm + '.pem')
    key = BUILD / (algorithm + '.key')
    environment = dict(os.environ, OPENSSL_CONF=str(BUILD / 'p256.cnf'), SSL_CERT_FILE=str(certificate))
    environment.pop('SSLKEYLOGFILE', None)
    processes = []
    case = dict(name=name, variant=variant, runtime=runtime, certificate=algorithm,
                cipher=cipher, family=family, managed_role=managed_role, passed=False)
    receipt['cases'].append(case)

    def command(role, port):
        is_managed = managed_role == 'both' or managed_role == role
        executable = managed if is_managed else [str(BUILD / 'native-peer')]
        return [*map(str, executable), str(certificate), str(key), cipher, family,
                str(certificate), 'localhost', role, str(port), str(ready)]

    try:
        with tempfile.TemporaryDirectory(prefix='dotcc-managed-peer-') as isolated_tmp:
            environment['TMPDIR'] = isolated_tmp
            server_path, client_path = LOGS / (name + '-server.log'), LOGS / (name + '-client.log')
            with server_path.open('w') as server_log, client_path.open('w') as client_log:
                server_command = command('server', 0)
                receipt['commands'].append(dict(name=name + '-server', arguments=server_command))
                server = subprocess.Popen(server_command, stdout=server_log, stderr=subprocess.STDOUT, env=environment)
                processes.append(server)
                port = wait_ready(server, ready)
                client_command = command('client', port)
                receipt['commands'].append(dict(name=name + '-client', arguments=client_command))
                client = subprocess.Popen(client_command, stdout=client_log, stderr=subprocess.STDOUT, env=environment)
                processes.append(client)
                case['client_exit'] = client.wait(timeout=45)
                case['server_exit'] = server.wait(timeout=45)
            case['client'] = parse_result(client_path)
            case['server'] = parse_result(server_path)
            valid = case['client_exit'] == case['server_exit'] == 0
            for role in ('client', 'server'):
                result = case[role]
                valid &= result['passed'] and result['family'] == family
                valid &= result['cipher'] == (0x1301 if cipher == '128' else 0x1302)
                valid &= result['group'] == 23 and result['quic_version'] == 1
                valid &= result[role + '_bytes'] == 65537
                valid &= result['certificate_validation'] == (role == 'client')
                if managed_role in (role, 'both'):
                    valid &= result['listener_preflight']
                    valid &= result['sent_bytes'] == 65537 and result['send_completions'] == 1
                    valid &= result['connected'] == result['finished'] == result['closed'] == 1
                    valid &= result['transport_status'] == result['transport_error'] == result['peer_error'] == 0
                    valid &= result['aot'] == (runtime == 'aot')
            case['passed'] = bool(valid)
            if not valid:
                raise RuntimeError('Peer interop validation failed: ' + name)
            print(name + ': PASS', flush=True)
    finally:
        for process in processes:
            if process.poll() is None:
                process.kill()
            process.wait()


try:
    if hasattr(os, 'sched_getaffinity'):
        os.sched_setaffinity(0, sorted(os.sched_getaffinity(0))[:4])
    for name in ('product-build', 'platform-host', 'packet-crypto', 'datapath-host'):
        require_receipt(name, ROOT / 'artifacts' / name / 'results.json')
    require_receipt('tls-adapter', args.tls_receipt)
    receipt['closure_sha256'] = sha(ROOT / 'config/product-closure.json')
    sources = sorted((ROOT / 'src/BclHost').glob('*.cs')) + sorted((ROOT / 'tests/ManagedPeer').glob('*.*'))
    sources += sorted((REPO / 'picotls/src/BclProvider').glob('*.cs'))
    for variant in args.variants:
        sources += sorted(picotls_directory(variant).glob('*.cs'))
        sources += [picotls_directory(variant) / 'TranslatedPicotls.csproj']
    sources += [ROOT / 'src/BclHost/BclHost.csproj', Path(__file__), ROOT / 'tests/NativePeer/peer.c']
    receipt['source_hashes'] = {str(p.relative_to(REPO)): sha(p) for p in sources}
    receipt['generated_hashes'] = {variant: generated_hashes(variant) for variant in args.variants}
    receipt['picotls_generated_hashes'] = {variant: picotls_hashes(variant) for variant in args.variants}
    pin = json.loads((ROOT / 'config/source.json').read_text())
    receipt['revision'] = pin['commit']
    library = ROOT / 'build/native-oracle/bin/Release'
    receipt['native_library_sha256'] = sha(library / 'libmsquic.so')
    receipt['native_library_product_dependency'] = False
    run(['gcc', '-std=c17', '-D_GNU_SOURCE', '-DCX_PLATFORM_LINUX', '-I', ROOT / 'ref' / pin['directory'] / 'src/inc',
         ROOT / 'tests/NativePeer/peer.c', '-L', library, '-Wl,-rpath,' + str(library), '-lmsquic', '-o', BUILD / 'native-peer'], 'native-build')
    receipt['native_peer_sha256'] = sha(BUILD / 'native-peer')
    configuration = 'openssl_conf = init\n[init]\nssl_conf = config\n[config]\nsystem_default = profile\n[profile]\nGroups = P-256\n'
    (BUILD / 'p256.cnf').write_text(configuration)
    receipt['openssl_configuration'] = configuration
    for algorithm in ('ecdsa', 'rsa'):
        options = ['-newkey', 'ec', '-pkeyopt', 'ec_paramgen_curve:prime256v1'] if algorithm == 'ecdsa' else ['-newkey', 'rsa:2048']
        run(['openssl', 'req', '-x509', *options, '-nodes', '-keyout', BUILD / (algorithm + '.key'), '-out', BUILD / (algorithm + '.pem'),
             '-days', '2', '-subj', '/CN=localhost', '-addext', 'subjectAltName=DNS:localhost,IP:127.0.0.1,IP:::1'], 'certificate-' + algorithm)
        (BUILD / (algorithm + '.key')).chmod(0o600)
    receipt['certificate_sha256'] = {p.name: sha(p) for p in BUILD.glob('*.pem')}
    for variant in args.variants:
        project = BUILD / variant
        project.mkdir(exist_ok=True)
        project_file = project / 'ManagedPeer.csproj'
        project_file.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>'
            '<AssemblyName>ManagedPeer</AssemblyName><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible>'
            '</PropertyGroup><ItemGroup><ProjectReference Include="' + escape(str(ROOT / 'src/BclHost/BclHost.csproj')) + '"/>'
            '<Compile Include="' + escape(str(ROOT / 'tests/ManagedPeer/Program.cs')) + '"/>'
            '<TrimmerRootAssembly Include="TranslatedMsQuic"/><TrimmerRootAssembly Include="TranslatedPicotls"/></ItemGroup></Project>\n')
        library_project = ROOT / 'generated' / variant / 'TranslatedMsQuic/TranslatedMsQuic.csproj'
        property_args = ['-p:MsQuicProject=' + str(library_project),
                         '-p:PicotlsProject=' + str(picotls_directory(variant) / 'TranslatedPicotls.csproj')]
        with tempfile.TemporaryDirectory(prefix='dotcc-managed-peer-build-') as isolated_tmp:
            build_environment = dict(os.environ, TMPDIR=isolated_tmp)
            run(['dotnet', 'build', project_file, '-c', 'Release', *property_args, '--nologo'], variant + '-build', build_environment)
            runtimes = [('jit', ['dotnet', project / 'bin/Release/net10.0/ManagedPeer.dll'])]
            if not args.jit_only:
                run(['dotnet', 'publish', project_file, '-c', 'Release', *property_args, '-r', 'linux-x64', '-p:PublishAot=true', '-o', project / 'aot', '--nologo'], variant + '-aot-build', build_environment)
                runtimes.append(('aot', [project / 'aot/ManagedPeer']))
        for runtime, command in runtimes:
            # Same translated input, two independent host processes, full pattern
            # and FIN. These controls run before the external native oracle pairs.
            if 'both' in args.roles:
                for cipher in ('128', '256'):
                    for family in ('ipv4', 'ipv6'):
                        exchange(command, variant, runtime, 'ecdsa', cipher, family, 'both')
            for algorithm in ('ecdsa', 'rsa'):
                for cipher in ('128', '256'):
                    for family in ('ipv4', 'ipv6'):
                        for role in (role for role in ('client', 'server') if role in args.roles):
                            exchange(command, variant, runtime, algorithm, cipher, family, role)
    if receipt['source_hashes'] != {str(p.relative_to(REPO)): sha(p) for p in sources}:
        raise RuntimeError('Host, provider or harness source changed during the matrix')
    if receipt['generated_hashes'] != {variant: generated_hashes(variant) for variant in args.variants}:
        raise RuntimeError('Generated source changed during the matrix')
    if receipt['picotls_generated_hashes'] != {variant: picotls_hashes(variant) for variant in args.variants}:
        raise RuntimeError('Generated picotls source changed during the matrix')
    for item in receipt['prerequisites'].values():
        if sha(Path(item['path'])) != item['sha256']:
            raise RuntimeError('A phase prerequisite changed during the matrix')
    binary_files = [BUILD / 'native-peer', library / 'libmsquic.so']
    for variant in args.variants:
        for directory in [BUILD / variant / 'bin/Release/net10.0'] + ([] if args.jit_only else [BUILD / variant / 'aot']):
            binary_files.extend(p for p in directory.rglob('*') if p.is_file())
    receipt['binary_hashes'] = {str(p.relative_to(REPO)): sha(p) for p in sorted(set(binary_files))}
    receipt['selected_roles'] = args.roles
    receipt.update(passed=all(case['passed'] for case in receipt['cases']),
        phase='P6 basic bidirectional stream matrix validated; authentication-negative and broader transport gates remain pending')
finally:
    (LOGS / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'cases': len(receipt['cases']), 'phase': receipt['phase']}))
