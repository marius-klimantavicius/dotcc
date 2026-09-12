#!/usr/bin/env python3
"""Serial source-linked owning facade and actual transport controls."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
PICO = ROOT.parent / 'picotls'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
parser.add_argument('--jit-only', action='store_true')
MODES = {
    'datagram-late-ack': ('actual late DATAGRAM acknowledgment after loss', 'PASS facade DATAGRAM late ACK'),
    'versions': ('scoped version policy ownership and validation', 'PASS facade scoped version policies:'),
    'handshake-snapshots': ('negotiated metadata after native TLS retirement', 'PASS facade handshake snapshots:'),
    'network': ('endpoint/interface/binding/scheduling/statistics', 'PASS facade network parameters:'),
    'flags': ('selected flags and stream credit', 'PASS facade flag controls:'),
    'ticket-rotation': ('imported ticket-key rotation', 'PASS facade ticket-key rotation'),
    'certificates': ('certificate policy completion and lifetime', 'PASS facade certificate policies:'),
    'callbacks': ('callback replacement, INLINE and DoS notifications', 'PASS facade callback controls'),
    'resumption': ('server ticket policy', 'PASS managed API configuration control:'),
    'transport': ('owning stream and lifetime transport', 'PASS facade stream/lifetime'),
    'packets': ('key updates and DATAGRAM transport', 'PASS facade packet controls'),
    'handshake-faults': ('Retry and stateless reset', 'PASS facade handshake fault controls'),
}
group = parser.add_mutually_exclusive_group()
group.add_argument('--all', action='store_const', const='all', dest='mode', help='Run every facade group after one build/publish per variant')
for name, (description, _) in MODES.items():
    group.add_argument('--' + name, action='store_const', const=name, dest='mode', help='Run ' + description + ' controls')
args = parser.parse_args()
LOG = ROOT / 'artifacts' / ('managed-api' + ('-' + args.mode if args.mode else ''))
LOG.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, transport_validated=False, selected_mode=args.mode or 'configuration', variants=[], commands=[])


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def generated(directory):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    if not names or len(names) != len(set(names)) or any(Path(n).name != n or not n.endswith('.cs') for n in names):
        raise RuntimeError('Invalid generated source manifest')
    return {name: sha(directory / name) for name in names}


def stable_transcript(value):
    if isinstance(value, dict):
        return {name: stable_transcript(output) for name, output in value.items()}
    # This exact evidence record contains randomized encrypted UDP bytes and
    # scheduling-dependent loss counts. Keep it unchanged in logs/receipts; the
    # test asserts its invariants before emitting a separate stable PASS row.
    return '\n'.join(line for line in value.splitlines()
                     if not line.startswith('EVIDENCE datagram-late-ack '))


def run(command, name, environment=None):
    command = list(map(str, command))
    receipt['commands'].append(dict(name=name, arguments=command))
    print('RUN ' + name, flush=True)
    result = subprocess.run(command, capture_output=True, text=True, timeout=900, env=environment)
    (LOG / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(name + ' failed; see ' + str(LOG / (name + '.log')))
    return result.stdout


try:
    closure = ROOT / 'config/product-closure.json'
    provenance = PICO / 'artifacts/translation/success.json'
    frozen, pico_frozen = json.loads(closure.read_text()), json.loads(provenance.read_text())
    sources = [Path(__file__).resolve(), closure, provenance, ROOT / 'tests/TlsAdapter/Credentials.cs']
    for directory in [ROOT / 'src/ManagedApi', ROOT / 'src/BclHost', ROOT / 'tests/ManagedApi', PICO / 'src/BclProvider']:
        sources += sorted(directory.glob('*.cs')) + sorted(directory.glob('*.csproj'))
    receipt['input_sha256'] = {os.path.relpath(p, ROOT): sha(p) for p in sources}
    require_metadata = set(args.variants) == {'raw', 'optimized'} and not args.jit_only
    runtime_environment = dict(os.environ)
    if require_metadata:
        runtime_environment['DOTCC_REQUIRED_SOURCE_REVISION'] = json.loads((ROOT / 'config/source.json').read_text())['commit']
    else:
        runtime_environment.pop('DOTCC_REQUIRED_SOURCE_REVISION', None)
    receipt['exact_source_metadata_required'] = require_metadata
    for variant in args.variants:
        msquic = ROOT / 'generated' / variant / 'TranslatedMsQuic'
        picotls = PICO / 'generated' / ('TranslatedPicotlsRaw' if variant == 'raw' else 'TranslatedPicotls')
        hashes, pico_hashes = generated(msquic), generated(picotls)
        if any(frozen['generated'][variant].get(n) != h for n, h in hashes.items()):
            raise RuntimeError('MsQuic translation differs from the frozen product closure')
        if any(pico_frozen[variant].get(n) != h for n, h in pico_hashes.items()):
            raise RuntimeError('picotls translation differs from its successful receipt')
        project = ROOT / 'tests/ManagedApi/ManagedApi.csproj'
        properties = ['-p:MsQuicProject=' + str(msquic / 'TranslatedMsQuic.csproj'),
                      '-p:PicotlsProject=' + str(picotls / 'TranslatedPicotls.csproj')]
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo', *properties], variant + '-build')
        commands = [('jit', ['dotnet', project.parent / 'bin/Release/net10.0/MsQuic.ManagedApi.dll'])]
        if not args.jit_only:
            destination = ROOT / 'build/managed-api' / variant / 'aot'
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                 '-o', destination, '--nologo', *properties], variant + '-aot-build')
            commands.append(('aot', [destination / 'MsQuic.ManagedApi']))
        entry = dict(name=variant, passed=False, generated_sha256=hashes, picotls_generated_sha256=pico_hashes)
        modes = [None, *MODES] if args.mode == 'all' else [args.mode]
        for runtime, command in commands:
            outputs = {}
            for mode in modes:
                invocation = [*command, '--' + mode] if mode else command
                name = variant + '-' + ((mode or 'configuration') + '-' if args.mode == 'all' else '') + runtime
                output = run(invocation, name, runtime_environment)
                expected = MODES[mode][1] if mode else 'PASS managed API configuration control:'
                if not any(line.startswith(expected) for line in output.splitlines()):
                    raise RuntimeError('Missing facade consumer success receipt: ' + str(mode))
                outputs[mode or 'configuration'] = output.strip()
            entry[runtime] = outputs if args.mode == 'all' else next(iter(outputs.values()))
        if not args.jit_only and stable_transcript(entry['jit']) != stable_transcript(entry['aot']):
            raise RuntimeError('JIT/NativeAOT control results differ')
        if hashes != generated(msquic) or pico_hashes != generated(picotls):
            raise RuntimeError('Translation changed during control')
        entry['passed'] = True
        receipt['variants'].append(entry)
    if receipt['input_sha256'] != {os.path.relpath(p, ROOT): sha(p) for p in sources}:
        raise RuntimeError('Authored inputs changed during control')
    receipt['targeted_passed'] = True
    receipt['transport_validated'] = args.mode is not None
    receipt['passed'] = set(args.variants) == {'raw', 'optimized'} and not args.jit_only
finally:
    (LOG / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps(dict(passed=receipt['passed'], variants=len(receipt['variants']))))
