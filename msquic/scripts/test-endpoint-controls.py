#!/usr/bin/env python3
"""Qualify live malformed-input and pre-validation amplification controls serially."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import struct
import subprocess
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
import sys
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import require_hash, observed_digest
from provenance import picotls_provenance
REPO = ROOT.parent
PICO = REPO / 'picotls'
PROJECT = ROOT / 'tests/EndpointControls/EndpointControls.csproj'


def sha(path):
    return observed_digest(path)


def require(value, message):
    if not value:
        raise RuntimeError(message)


def snapshot(paths):
    return {str(p.relative_to(REPO)): sha(p) for p in sorted(set(paths))}


def generated(directory, recorded):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    require(names and len(names) == len(set(names)) and
            all(Path(n).name == n and n.endswith('.cs') for n in names), 'Invalid generated manifest')
    actual = {str(p.relative_to(directory)) for p in directory.rglob('*.cs')
              if not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
    require(actual == set(names), 'Unmanifested generated source')
    require_hash({n: sha(directory / n) for n in names} ==
            {n: h for n, h in recorded.items() if n.endswith('.cs')}, 'Changed generated sources')
    for name, value in recorded.items():
        require(Path(name).name == name, 'Unsafe provenance path: ' + name)
        require_hash(sha(directory / name) == value, 'Changed generated input: ' + name)
    return [directory / name for name in recorded]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
    parser.add_argument('--modes', nargs='+', choices=['malformed', 'amplification'], default=['malformed', 'amplification'])
    parser.add_argument('--jit-only', action='store_true')
    parser.add_argument('--skip-native', action='store_true', help='Diagnostic amplification subset without native comparison')
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/endpoint-controls')
    args = parser.parse_args()
    for values in (args.variants, args.modes):
        if len(values) != len(set(values)):
            parser.error('Selections must be unique')
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    logs = Path(tempfile.mkdtemp(prefix='run-', dir=output))
    build = ROOT / 'build/endpoint-controls' / logs.name
    receipt = dict(passed=False, targeted_passed=False, entire_p7_qualified=False,
                   scope='Actual live UDP malformed/tag input and pre-validation amplification; not full endpoint fuzzing',
                   commands=[], cases=[], logs=str(logs.relative_to(REPO)))
    result_path = output / 'results.json'

    def save():
        result_path.write_text(json.dumps(receipt, indent=2) + '\n')

    def run(command, name, environment, timeout=900):
        command = list(map(str, command))
        record = dict(name=name, arguments=command, environment={key: environment[key] for key in
            ('UseLocalLalrCc', 'DOTCC_REQUIRED_SOURCE_REVISION', 'DOTCC_AMPLIFICATION_NATIVE_PEER',
             'DOTCC_AMPLIFICATION_NATIVE_OPENSSL_CONF') if key in environment})
        receipt['commands'].append(record)
        save()
        print('RUN ' + name, flush=True)
        path = logs / (name + '.log')
        with path.open('w') as stream:
            completed = subprocess.run(command, stdout=stream, stderr=subprocess.STDOUT,
                                       env=environment, timeout=timeout)
        record['exit_code'] = completed.returncode
        record['log_sha256'] = sha(path)
        save()
        require(completed.returncode == 0, name + ' failed; see ' + str(path))
        return path.read_text()

    try:
        closure_path = ROOT / 'config/product-closure.json'
        pico_path = PICO / 'artifacts/campaign/current-default.json'
        pin_path = ROOT / 'config/source.json'
        closure, pin = [json.loads(p.read_text()) for p in (closure_path, pin_path)]
        pico = picotls_provenance(PICO)
        require(closure['revision'] == pin['commit'], 'Closure revision mismatch')
        receipt['product_closure_sha256'] = sha(closure_path)
        receipt['picotls_provenance_sha256'] = sha(pico_path)
        receipt['source_revision'] = pin['commit']
        project = ET.parse(PROJECT)
        require(project.findtext('.//AllowUnsafeBlocks') == 'false', 'Endpoint consumer must use public safe API')
        refs = project.findall('.//ProjectReference')
        require(len(refs) == 1 and (PROJECT.parent / refs[0].attrib['Include']).resolve() ==
                ROOT / 'src/ManagedApi/ManagedApi.csproj', 'Expected ordinary owning API reference')
        require(not project.findall('.//Compile') and not project.findall('.//TrimmerRootAssembly') and
                not project.findall('.//TrimmerRootDescriptor'), 'No source-linked product or extra roots')
        paths = [Path(__file__).resolve(), closure_path, pico_path, pin_path]
        for folder in (PROJECT.parent, ROOT / 'src/ManagedApi', ROOT / 'src/BclHost', PICO / 'src/BclProvider'):
            paths += [p for p in folder.rglob('*') if p.is_file() and
                      not {'bin', 'obj'}.intersection(p.relative_to(folder).parts) and
                      p.suffix in ('.cs', '.csproj', '.props', '.targets')]
            for ancestor in (folder, *folder.parents):
                for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props',
                             'global.json', 'NuGet.Config', 'nuget.config'):
                    path = ancestor / name
                    if path.is_file(): paths.append(path)
                if ancestor == REPO: break
        for relative, digest in pico['tool_sha256'].items():
            path = REPO / relative
            require_hash(sha(path) == digest, 'Changed qualified compiler/postprocessor')
            paths.append(path)
        generated_inputs = {}
        for variant in args.variants:
            ms = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
            tls = PICO / 'generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')
            selected = generated(ms, closure['generated'][variant]) + generated(tls, pico[variant])
            paths += selected
            generated_inputs[variant] = (ms, tls)
        native = 'amplification' in args.modes and not args.skip_native
        environment = dict(os.environ, UseLocalLalrCc='false', DOTCC_REQUIRED_SOURCE_REVISION=pin['commit'])
        for name in ('SSLKEYLOGFILE', 'DOTCC_PEER_PROXY_DRAIN', 'DOTCC_PEER_SETTLE_MS',
                     'DOTCC_AMPLIFICATION_NATIVE_PEER', 'DOTCC_AMPLIFICATION_NATIVE_OPENSSL_CONF'):
            environment.pop(name, None)
        if native:
            baseline_path = ROOT / 'artifacts/managed-peer/results.json'
            baseline = json.loads(baseline_path.read_text())
            require(baseline['passed'], 'Native comparison needs a passing peer baseline')
            require_hash(baseline['closure_sha256'] == sha(closure_path), 'Peer baseline closure changed')
            for relative, digest in baseline['source_hashes'].items():
                path = REPO / relative
                require_hash(sha(path) == digest, 'Native/host baseline source changed: ' + relative)
                paths.append(path)
            native_peer = ROOT / 'build/managed-peer/native-peer'
            library = ROOT / 'build/native-oracle/bin/Release/libmsquic.so'
            for path in (native_peer, library):
                require_hash(baseline['binary_hashes'][str(path.relative_to(REPO))] == sha(path), 'Changed qualified native oracle')
                paths.append(path)
            openssl_config = ROOT / 'build/managed-peer/p256.cnf'
            require(openssl_config.read_text() == baseline['openssl_configuration'], 'Native group configuration changed')
            paths += [baseline_path, ROOT / 'tests/NativePeer/peer.c', openssl_config]
            environment['DOTCC_AMPLIFICATION_NATIVE_PEER'] = str(native_peer)
            environment['DOTCC_AMPLIFICATION_NATIVE_OPENSSL_CONF'] = str(openssl_config)
            receipt['native_peer_receipt_sha256'] = sha(baseline_path)
        frozen = snapshot(paths)
        receipt['input_sha256'] = frozen
        receipt['native_comparison_selected'] = native
        stable_outputs = {}
        for variant, (ms, tls) in generated_inputs.items():
            properties = ['-p:MsQuicProject=' + str(ms / 'TranslatedMsQuic.csproj'),
                          '-p:PicotlsProject=' + str(tls / 'TranslatedPicotls.csproj'),
                          '-p:UseArtifactsOutput=true', '-p:ArtifactsPath=' + str(build / variant / 'artifacts')]
            jit = build / variant / 'jit'
            run(['dotnet', 'build', PROJECT, '-c', 'Release', '--nologo', '-o', jit, *properties],
                variant + '-build', environment)
            commands = [('jit', ['dotnet', str(jit / 'EndpointControls.dll')], jit)]
            if not args.jit_only:
                aot = build / variant / 'aot'
                run(['dotnet', 'publish', PROJECT, '-c', 'Release', '--nologo', '-r', 'linux-x64',
                     '--self-contained', 'true', '-p:PublishAot=true', '-p:PublishTrimmed=true',
                     '-p:ILLinkTreatWarningsAsErrors=true', '-p:IlcTreatWarningsAsErrors=true',
                     '-o', aot, *properties], variant + '-publish', environment)
                header = (aot / 'EndpointControls').read_bytes()[:64]
                require(header[:6] == b'\x7fELF\x02\x01' and struct.unpack_from('<H', header, 18)[0] == 62,
                        'NativeAOT image is not Linux x64 ELF')
                commands.append(('aot', [str(aot / 'EndpointControls')], aot))
            for runtime, command, directory in commands:
                binaries = snapshot(p for p in directory.rglob('*') if p.is_file())
                for mode in args.modes:
                    name = '-'.join((variant, runtime, mode))
                    text = run([*command, '--' + mode], name, environment, timeout=240)
                    lines = text.splitlines()
                    metadata = [json.loads(line.removeprefix('EVIDENCE endpoint metadata '))
                                for line in lines if line.startswith('EVIDENCE endpoint metadata ')]
                    require(len(metadata) == 1 and metadata[0] == dict(aot=runtime == 'aot',
                            source_revision=pin['commit'], provider='picotls'), 'Wrong runtime/provider/source identity')
                    require(lines.count('PASS endpoint-controls mode=' + mode) == 1, 'Missing completed endpoint group')
                    roles = ('server', 'client') if mode == 'malformed' else (('managed', 'native') if native else ('managed',))
                    label = 'receiver' if mode == 'malformed' else 'server'
                    expected = {f'PASS endpoint {mode} cipher=0x{cipher:x} family={family} {label}={role}'
                                for cipher in (0x1301, 0x1302) for family in ('InterNetwork', 'InterNetworkV6') for role in roles}
                    actual = [line for line in lines if line.startswith('PASS endpoint ' + mode + ' ')]
                    require(set(actual) == expected and len(actual) == len(expected), 'Missing/duplicate endpoint profiles')
                    stable = [line for line in lines if not line.startswith('EVIDENCE endpoint ')]
                    if mode in stable_outputs: require(stable == stable_outputs[mode], 'Runtime profile transcripts differ')
                    else: stable_outputs[mode] = stable
                    receipt['cases'].append(dict(name=name, passed=True, variant=variant, runtime=runtime,
                        mode=mode, profiles=actual, evidence=[line for line in lines if line.startswith('EVIDENCE endpoint ')],
                        stdout=text, binary_sha256=binaries))
                    save()
                require_hash(snapshot(p for p in directory.rglob('*') if p.is_file()) == binaries, 'Executed binaries changed')
        require_hash(snapshot(paths) == frozen, 'Endpoint execution inputs changed during campaign')
        for variant, (ms, tls) in generated_inputs.items():
            generated(ms, closure['generated'][variant]); generated(tls, pico[variant])
        receipt['targeted_passed'] = True
        receipt['passed'] = set(args.variants) == {'raw', 'optimized'} and not args.jit_only and \
            set(args.modes) == {'malformed', 'amplification'} and native
    except BaseException as error:
        receipt['error'] = str(error)
        raise
    finally:
        save()
    print(json.dumps({k: receipt[k] for k in ('passed', 'targeted_passed', 'entire_p7_qualified')}), flush=True)


if __name__ == '__main__':
    main()
