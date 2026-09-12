#!/usr/bin/env python3
"""Measure matched public API consumers only after verified payload and clean close."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import resource
import shutil
import statistics
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
TEST = ROOT / 'tests/Benchmarks'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def check(condition, reason):
    if not condition:
        raise RuntimeError(reason)


def validate_generated(directory, recorded):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    check(names and len(names) == len(set(names))
          and all(Path(name).name == name and name.endswith('.cs') for name in names),
          'Invalid benchmark generated manifest')
    actual = {str(path.relative_to(directory)) for path in directory.rglob('*.cs')
              if not {'bin', 'obj'}.intersection(path.relative_to(directory).parts)}
    expected = {name for name in recorded if name.endswith('.cs')}
    check(actual == set(names) == expected, 'Benchmark generated source inventory differs from frozen closure')
    for name, digest in recorded.items():
        check(Path(name).name == name and sha(directory / name) == digest,
              'Benchmark generated input differs from frozen closure: ' + name)


def records(path):
    return [json.loads(line) for line in path.read_text().splitlines() if line.startswith('{')]


def one(rows, key, value):
    selected = [row for row in rows if row.get(key) == value]
    check(len(selected) == 1, 'Missing or duplicate endpoint record: ' + str(value))
    return selected[0]


def validate(rows, role, managed, runtime, args, cipher, revision, require_metadata):
    final = [row for row in rows if 'passed' in row]
    check(len(final) == 1 and final[0]['passed'] and final[0]['clean_close'], 'Endpoint did not verify clean completion')
    configuration = one(rows, 'metric', 'configuration')
    expected = dict(role=role, bytes=args.bytes, warmups=args.warmups, iterations=args.iterations,
        chunk_bytes=args.chunk_bytes, pipeline=1, transport_workers=1, stream_window=1048576,
        connection_window=8388608, send_buffering=False, pacing=True, ecn=False,
        encryption_offload=False, alpn='dotcc-bench-v1', cipher=0x1301 if cipher == '128' else 0x1302)
    check(all(configuration.get(k) == v for k, v in expected.items()), 'Endpoint benchmark configuration differs')
    identity = one(rows, 'metric', 'identity')
    check(identity['implementation'] == ('managed-owning' if managed else 'native'), 'Wrong endpoint implementation')
    if managed:
        check(identity['aot'] == (runtime == 'aot') and identity['tls_provider'] == 'Picotls', 'Wrong managed runtime/provider')
    else:
        check(identity['tls_provider_id'] == 1, 'Native reference is not the pinned OpenSSL provider')
    if require_metadata:
        check(identity['source_revision'] == revision, 'Endpoint metadata differs from source pin')
    handshake = one(rows, 'metric', 'handshake')
    check(handshake['scope'] == ('client_connect_api' if role == 'client' else 'server_accept_wait'), 'Handshake metric scope differs')
    check(handshake['wall_us'] > 0 and handshake['quic_version'] == 1 and handshake['group'] == 23 and handshake['cipher'] == expected['cipher'], 'Negotiated benchmark profile differs')
    transfers = [row for row in rows if row.get('metric') == 'transfer']
    check(len(transfers) == args.warmups + args.iterations, 'Missing transfer observations')
    for index, row in enumerate(transfers):
        check(row['index'] == index and row['warmup'] == (index < args.warmups), 'Transfer ordering/warmup mismatch')
        check(row['payload_sent'] == row['payload_received'] == args.bytes and row['fin_received'], 'Unverified benchmark payload/FIN')
        check(row['wall_us'] > 0 and row['cpu_us'] >= 0, 'Invalid timing observation')
        check((row['managed_allocated_bytes'] is not None) == managed, 'Wrong allocation counter domain')
    memory = [row for row in rows if row.get('metric') == 'memory']
    for phase in ('idle', 'drained'):
        check(one(memory, 'phase', phase)['rss_bytes'] > 0, 'Missing process memory observation')
    for phase in ('load_begin', 'load_end'):
        selected = [row for row in memory if row['phase'] == phase]
        check([row['index'] for row in selected] == list(range(len(transfers))), 'Missing load memory observations')
    samples = transfers[args.warmups:]
    rates = [(row['payload_sent'] + row['payload_received']) * 8 / row['wall_us'] for row in samples]
    return dict(identity=identity, configuration=configuration, handshake=handshake, transfers=transfers, memory=memory,
        summary=dict(measured_transfers=len(samples), aggregate_bidirectional_mbps_median=statistics.median(rates),
            transfer_wall_us_median=statistics.median(row['wall_us'] for row in samples),
            cpu_us_median=statistics.median(row['cpu_us'] for row in samples),
            managed_allocated_bytes_median=statistics.median(row['managed_allocated_bytes'] for row in samples) if managed else None,
            idle_rss_bytes=one(memory, 'phase', 'idle')['rss_bytes'],
            peak_rss_bytes=max(row['peak_rss_bytes'] for row in memory),
            drained_rss_bytes=one(memory, 'phase', 'drained')['rss_bytes']))


def report(receipt, path):
    lines = ['# Public API transport measurements', '',
        'Only verified transfers from pairs with clean shutdown are summarized. Warmups are excluded. '
        'Rates are aggregate bidirectional application payload, in decimal Mbit/s. '
        'These compare whole implementations and public API ownership costs, not compiler overhead alone.', '',
        '| Pair | Endpoint | Client connect ms | Median Mbit/s | Median transfer ms | Median CPU ms | Median managed allocated MiB | Idle / peak / drained RSS MiB |',
        '| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |']
    for case in receipt['cases']:
        if not case.get('passed'):
            continue
        for role in ('client', 'server'):
            row = case[role]['summary']
            allocated = row['managed_allocated_bytes_median']
            memory = ' / '.join(f'{row[k] / 1048576:.1f}' for k in ('idle_rss_bytes', 'peak_rss_bytes', 'drained_rss_bytes'))
            connect = f"{case[role]['handshake']['wall_us'] / 1000:.2f}" if role == 'client' else 'n/a'
            lines.append(f"| {case['name']} | {role} | {connect} | {row['aggregate_bidirectional_mbps_median']:.2f} | {row['transfer_wall_us_median'] / 1000:.2f} | {row['cpu_us_median'] / 1000:.2f} | " +
                ('n/a' if allocated is None else f'{allocated / 1048576:.2f}') + f' | {memory} |')
    native = {(case['family'], case['cipher']): case for case in receipt['cases'] if case.get('passed') and case['pair'] == 'native'}
    comparisons = [case for case in receipt['cases'] if case.get('passed') and case['pair'] == 'both' and (case['family'], case['cipher']) in native]
    if comparisons:
        lines += ['', 'Matched pair comparisons use the client endpoint interval in each pair. '
            'The ratio is managed-pair payload rate divided by native-pair payload rate; '
            'sequential scheduling and a small sample count limit interpretation.', '',
            '| Managed pair | Managed/native payload rate |', '| --- | ---: |']
        for case in comparisons:
            baseline = native[(case['family'], case['cipher'])]
            ratio = case['client']['summary']['aggregate_bidirectional_mbps_median'] / baseline['client']['summary']['aggregate_bidirectional_mbps_median']
            lines.append(f"| {case['name']} | {ratio:.3f}× |")
    lines += ['', 'The JSON receipt retains individual samples, client connection-open/start timing, '
        'server accept wait, process affinity, source/binary hashes, and every command. '
        'Server accept wait includes process launch orchestration and is not handshake latency.', '',
        'Native uses OpenSSL QUIC TLS and borrowed send buffers; managed uses translated picotls, BCL crypto, '
        'copied send/receive buffers, and ordinary GC. Native UDP batching/offload capabilities may differ. '
        'RSS includes runtime and prepared payloads, and peak RSS is a process lifetime high-water mark. '
        'Drained RSS does not prove memory reclamation; no endpoint forces GC. '
        'A small sequential local run does not establish statistical significance or platform portability.', '',
        f"Full requested matrix passed: {receipt['passed']}. Targeted run passed: {receipt.get('targeted_passed', False)}."]
    path.write_text('\n'.join(lines) + '\n')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
    parser.add_argument('--jit-only', action='store_true')
    parser.add_argument('--families', nargs='+', choices=['ipv4', 'ipv6'], default=['ipv4', 'ipv6'])
    parser.add_argument('--ciphers', nargs='+', choices=['128', '256'], default=['128', '256'])
    parser.add_argument('--pairs', nargs='+', choices=['native', 'both', 'client', 'server'], default=['native', 'both'])
    parser.add_argument('--bytes', type=int, default=16777216)
    parser.add_argument('--warmups', type=int, default=1)
    parser.add_argument('--iterations', type=int, default=3)
    parser.add_argument('--chunk-bytes', type=int, default=1048576)
    parser.add_argument('--timeout', type=int, default=900)
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/benchmarks')
    args = parser.parse_args()
    check(platform.system() == 'Linux' and platform.machine() in ('x86_64', 'amd64'), 'Benchmark currently requires Linux x64')
    check(shutil.which('taskset') is not None, 'taskset is required for recorded matched affinity')
    check(16777216 <= args.bytes <= 268435456 and 1 <= args.warmups <= 4 and 1 <= args.iterations <= 16 and 4096 <= args.chunk_bytes <= 4194304 and 30 <= args.timeout <= 900, 'Invalid bounded benchmark dimensions')
    args.output.mkdir(parents=True, exist_ok=True)
    build = ROOT / 'build/benchmarks'; build.mkdir(parents=True, exist_ok=True)
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    available = sorted(os.sched_getaffinity(0))
    cpus = dict(client=available[0], server=available[min(1, len(available) - 1)])
    complete = (not args.jit_only and set(args.variants) == {'raw', 'optimized'} and
                set(args.families) == {'ipv4', 'ipv6'} and set(args.ciphers) == {'128', '256'} and {'native', 'both'}.issubset(args.pairs))
    receipt = dict(passed=False, targeted_passed=False, cases=[], commands=[], cpu_affinity=cpus,
        environment=dict(platform=platform.platform(), processor=platform.processor(), allowed_cpus=available),
        exact_source_metadata_required=complete, performance_threshold=None)
    inputs = [Path(__file__).resolve(), ROOT / 'config/source.json', ROOT / 'config/api-profile.json', ROOT / 'config/product-closure.json',
              REPO / 'picotls/artifacts/translation/success.json']
    inputs += [REPO / name for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'global.json') if (REPO / name).is_file()]
    for directory in [TEST, ROOT / 'src/ManagedApi', ROOT / 'src/BclHost', REPO / 'picotls/src/BclProvider']:
        inputs += sorted(p for p in directory.rglob('*') if p.is_file() and p.suffix in ('.cs', '.c', '.csproj') and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts))
    for variant in args.variants:
        for directory in [ROOT / 'generated' / variant / 'TranslatedMsQuic', REPO / 'picotls/generated' / ('TranslatedPicotlsRaw' if variant == 'raw' else 'TranslatedPicotls')]:
            inputs += sorted(directory.glob('*.cs')) + sorted(directory.glob('*.csproj')) + [directory / 'Dotcc.SourceFiles.txt']
    certificate = ROOT / 'build/managed-peer/ecdsa.pem'
    key = ROOT / 'build/managed-peer/ecdsa.key'
    openssl = ROOT / 'build/managed-peer/p256.cnf'
    native_library = ROOT / 'build/native-oracle/bin/Release/libmsquic.so'
    inputs += [certificate, key, openssl, native_library]
    receipt['input_sha256'] = {str(p.relative_to(REPO)): sha(p) for p in inputs}
    pin = json.loads((ROOT / 'config/source.json').read_text())

    def run(command, name, timeout=900):
        command = list(map(str, command)); receipt['commands'].append(dict(name=name, arguments=command))
        result = subprocess.run(command, text=True, capture_output=True, timeout=timeout)
        (args.output / (name + '.log')).write_text(result.stdout + result.stderr)
        check(result.returncode == 0, name + ' failed; inspect its log')
        return result.stdout

    def exchange(variant, runtime, pair, family, cipher, executable):
        name = '-'.join([variant, runtime, pair, family, cipher])
        folder = args.output / name; folder.mkdir(parents=True, exist_ok=True)
        ready = folder / 'server.ready'; ready.unlink(missing_ok=True)
        case = dict(name=name, variant=variant, runtime=runtime, pair=pair, family=family, cipher=cipher, passed=False)
        receipt['cases'].append(case)
        environment = dict(os.environ, OPENSSL_CONF=str(openssl), SSL_CERT_FILE=str(certificate))
        if complete: environment['DOTCC_REQUIRED_SOURCE_REVISION'] = pin['commit']
        else: environment.pop('DOTCC_REQUIRED_SOURCE_REVISION', None)
        host = '127.0.0.1' if family == 'ipv4' else '::1'
        processes = []
        def start(role, port, log):
            managed = pair in ('both', role)
            command = executable if managed else [build / 'native-endpoint']
            command = ['taskset', '-c', str(cpus[role]), *command, role, certificate, key if role == 'server' else 'localhost',
                host, str(port), ready, str(args.bytes), str(args.warmups), str(args.iterations), str(args.chunk_bytes), '1', cipher]
            command = list(map(str, command)); receipt['commands'].append(dict(name=name + '-' + role, arguments=command))
            process = subprocess.Popen(command, stdout=log, stderr=subprocess.STDOUT, env=environment)
            processes.append(process)
            return process
        begin = time.monotonic()
        try:
            with (folder / 'server.log').open('w') as server_log, (folder / 'client.log').open('w') as client_log:
                server = start('server', 0, server_log)
                while not ready.exists() or not ready.read_text().strip():
                    check(server.poll() is None and time.monotonic() - begin < 30, 'Server failed before readiness')
                    time.sleep(0.01)
                client = start('client', int(ready.read_text()), client_log)
                case['client_exit'] = client.wait(timeout=max(1, args.timeout - (time.monotonic() - begin)))
                case['server_exit'] = server.wait(timeout=max(1, args.timeout - (time.monotonic() - begin)))
            check(case['client_exit'] == case['server_exit'] == 0, 'Benchmark endpoint failed')
            for role in ('client', 'server'):
                case[role] = validate(records(folder / (role + '.log')), role, pair in ('both', role), runtime,
                    args, cipher, pin['commit'], complete)
            case['passed'] = True
            print(name + ': verified payload, FIN, profile and close PASS', flush=True)
        except BaseException as error:
            case['error'] = str(error)
            raise
        finally:
            for process in reversed(processes):
                if process.poll() is None: process.kill()
                process.wait()
            case['elapsed_seconds'] = time.monotonic() - begin

    try:
        closure = json.loads((ROOT / 'config/product-closure.json').read_text())
        pico_provenance = json.loads((REPO / 'picotls/artifacts/translation/success.json').read_text())
        receipt['compiler_provenance'] = dict(commit=closure['compiler_commit'], dotcc_lib_sha256=closure['compiler_hashes']['DotCC.Lib.dll'])
        for variant in args.variants:
            generated = ROOT / 'generated' / variant / 'TranslatedMsQuic'
            validate_generated(generated, closure['generated'][variant])
            pico = REPO / 'picotls/generated' / ('TranslatedPicotlsRaw' if variant == 'raw' else 'TranslatedPicotls')
            validate_generated(pico, pico_provenance[variant])
        receipt['environment']['dotnet_info'] = run(['dotnet', '--info'], 'dotnet-info')
        receipt['environment']['gcc_version'] = run(['gcc', '--version'], 'gcc-version').splitlines()[0]
        source = ROOT / 'ref' / pin['directory']
        dependency = build / 'native.d'
        run(['gcc', '-std=c17', '-D_GNU_SOURCE', '-DCX_PLATFORM_LINUX', '-I', source / 'src/inc',
             '-O2', '-Wall', '-Wextra', '-MD', '-MF', dependency, TEST / 'native-endpoint.c', '-pthread',
             '-L', native_library.parent, '-Wl,-rpath,' + str(native_library.parent), '-lmsquic', '-o', build / 'native-endpoint'], 'native-build')
        dependencies = [Path(name).resolve() for name in dependency.read_text().replace('\\\n', ' ').split(':', 1)[1].split()]
        receipt['native_dependency_sha256'] = {str(p): sha(p) for p in dependencies if p.is_file()}
        receipt['binary_sha256'] = {str((build / 'native-endpoint').relative_to(REPO)): sha(build / 'native-endpoint')}
        if 'native' in args.pairs:
            for family in args.families:
                for cipher in args.ciphers: exchange('native', 'native', 'native', family, cipher, [])
        for variant in args.variants:
            generated = ROOT / 'generated' / variant / 'TranslatedMsQuic/TranslatedMsQuic.csproj'
            pico = REPO / 'picotls/generated' / ('TranslatedPicotlsRaw' if variant == 'raw' else 'TranslatedPicotls') / 'TranslatedPicotls.csproj'
            project = TEST / 'ManagedEndpoint/ManagedEndpoint.csproj'
            props = ['-p:MsQuicProject=' + str(generated), '-p:PicotlsProject=' + str(pico)]
            output = build / variant / 'jit'
            run(['dotnet', 'build', project, '-c', 'Release', '--nologo', '-o', output, *props], variant + '-jit-build')
            commands = [('jit', ['dotnet', output / 'PublicQuicBenchmark.dll'], output)]
            if not args.jit_only:
                output = build / variant / 'aot'
                run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', output, '--nologo', *props], variant + '-aot-build')
                commands.append(('aot', [output / 'PublicQuicBenchmark'], output))
            for runtime, executable, output in commands:
                receipt['binary_sha256'].update({str(p.relative_to(REPO)): sha(p) for p in sorted(output.rglob('*')) if p.is_file()})
                for pair in args.pairs:
                    if pair == 'native': continue
                    for family in args.families:
                        for cipher in args.ciphers: exchange(variant, runtime, pair, family, cipher, executable)
        check(receipt['input_sha256'] == {str(p.relative_to(REPO)): sha(p) for p in inputs}, 'Benchmark inputs changed during measurement')
        for variant in args.variants:
            validate_generated(ROOT / 'generated' / variant / 'TranslatedMsQuic', closure['generated'][variant])
            validate_generated(REPO / 'picotls/generated' / ('TranslatedPicotlsRaw' if variant == 'raw' else 'TranslatedPicotls'),
                               pico_provenance[variant])
        check(all(sha(Path(p)) == h for p, h in receipt['native_dependency_sha256'].items()), 'Native benchmark dependency changed')
        check(all(sha(REPO / p) == h for p, h in receipt['binary_sha256'].items()), 'Executed benchmark binary changed')
        receipt['targeted_passed'] = True
        receipt['passed'] = complete
    except BaseException as error:
        receipt['error'] = str(error)
        raise
    finally:
        (args.output / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
        report(receipt, args.output / 'report.md')


if __name__ == '__main__':
    main()
