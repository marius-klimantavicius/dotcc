#!/usr/bin/env python3
"""Actual CxPlatTls services and typed packet-key callbacks, serial JIT/NativeAOT.

Uses existing frozen translations; never fetches, translates, or changes the core.
An initial --variants optimized --jit-only probe does not satisfy the full gate.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[1]
import sys
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import policy, observed_digest
from provenance import picotls_provenance
PICOTLS = ROOT.parent / 'picotls'
sys.path.insert(0, str(ROOT / 'tests/PlatformHost'))
from bootstrap import write_test_host

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
parser.add_argument('--jit-only', action='store_true')
args = parser.parse_args()
BUILD, LOG = ROOT / 'build/tls-adapter', ROOT / 'artifacts/tls-adapter'
BUILD.mkdir(parents=True, exist_ok=True)
LOG.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, transport_validated=False, commands=[], variants=[])


def sha(path):
    return observed_digest(path)


def generated_hashes(directory):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    if not names or len(names) != len(set(names)) or any(Path(name).name != name or not name.endswith('.cs') for name in names):
        raise RuntimeError('Unsafe or empty generated source manifest')
    return {name: sha(directory / name) for name in names}


def run(command, name):
    command = [str(value) for value in command]
    receipt['commands'].append(dict(name=name, arguments=command))
    print('RUN ' + name, flush=True)
    result = subprocess.run(command, text=True, capture_output=True, timeout=900)
    (LOG / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(name + ' failed; see ' + str(LOG / (name + '.log')))
    return result.stdout


try:
    closure = ROOT / 'config/product-closure.json'
    frozen = json.loads(closure.read_text())
    receipt['product_closure_sha256'] = sha(closure)
    provenance = PICOTLS / 'artifacts/campaign/current-default.json'
    pico_frozen = picotls_provenance(PICOTLS)
    receipt['picotls_translation_sha256'] = sha(provenance)
    source_names = ['MsQuicHost.Resources.cs', 'MsQuicHost.Platform.cs', 'MsQuicHost.Queue.cs', 'Status.cs', 'Crypto.cs']
    sources = [ROOT / 'src/BclHost' / name for name in source_names]
    sources += sorted((ROOT / 'src/BclHost').glob('Tls*.cs'))
    sources += sorted((ROOT / 'tests/TlsAdapter').glob('*.cs'))
    sources += [ROOT / 'tests/QuicTlsFeasibility/RawPeer.cs']
    inputs = sources + sorted((PICOTLS / 'src/BclProvider').glob('*.cs'))
    inputs += [Path(__file__).resolve(), ROOT / 'tests/PlatformHost/bootstrap.py', PICOTLS / 'src/BclProvider/BclProvider.csproj']
    receipt['input_sha256'] = {os.path.relpath(path, ROOT): sha(path) for path in inputs}
    for variant in args.variants:
        directory = BUILD / variant
        directory.mkdir(exist_ok=True)
        generated = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
        picotls = PICOTLS / 'generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')
        hashes, pico_hashes = generated_hashes(generated), generated_hashes(picotls)
        if any(frozen['generated'][variant].get(name) != digest for name, digest in hashes.items()):
            policy().issue('MsQuic generated library does not match frozen product closure')
        if any(pico_frozen[variant].get(name) != digest for name, digest in pico_hashes.items()):
            policy().issue('picotls generated library does not match successful translation')
        write_test_host(generated, directory,
                        'RegisterPlatform(ref table); RegisterCrypto(ref table); RegisterTls(ref table);', 'CreateTlsTable')
        project = directory / 'TlsAdapter.csproj'
        project.write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings>
    <IsAotCompatible>true</IsAotCompatible><NoWarn>CS0162</NoWarn></PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="''' + escape(str(generated / 'TranslatedMsQuic.csproj')) + '''" />
    <ProjectReference Include="''' + escape(str(PICOTLS / 'src/BclProvider/BclProvider.csproj')) + '''" />
''' + ''.join('    <Compile Include="' + escape(str(path)) + '" />\n' for path in sources) + '''
    <TrimmerRootAssembly Include="TranslatedMsQuic" />
  </ItemGroup>
</Project>
''')
        properties = ['-p:PicotlsProject=' + str(picotls / 'TranslatedPicotls.csproj')]
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo', *properties], variant + '-build')
        entry = dict(name=variant, passed=False, generated_sha256=hashes, picotls_generated_sha256=pico_hashes)
        commands = [('jit', ['dotnet', directory / 'bin/Release/net10.0/TlsAdapter.dll'])]
        if not args.jit_only:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                 '-o', directory / 'aot', '--nologo', *properties], variant + '-aot-build')
            commands.append(('aot', [directory / 'aot/TlsAdapter']))
        for runtime, command in commands:
            result_path = LOG / (variant + '-' + runtime + '.json')
            output = run([*command, result_path], variant + '-' + runtime)
            result = json.loads(result_path.read_text())
            if not result['Passed'] or not output.startswith('PASS TLS adapter:'):
                raise RuntimeError('Incorrect TLS adapter success receipt')
            entry[runtime] = result
        if not args.jit_only and entry['jit'] != entry['aot']:
            raise RuntimeError('JIT/NativeAOT cases differ')
        if hashes != generated_hashes(generated) or pico_hashes != generated_hashes(picotls):
            policy().issue('Generated source changed during TLS adapter validation')
        entry['passed'] = True
        receipt['variants'].append(entry)
    if receipt['input_sha256'] != {os.path.relpath(path, ROOT): sha(path) for path in inputs}:
        policy().issue('Authored TLS adapter/provider sources changed during validation')
    if receipt['product_closure_sha256'] != sha(closure) or receipt['picotls_translation_sha256'] != sha(provenance):
        policy().issue('Translation closure changed during TLS adapter validation')
    if set(args.variants) == {'raw', 'optimized'} and not args.jit_only:
        if receipt['variants'][0]['jit'] != receipt['variants'][1]['jit']:
            raise RuntimeError('Raw/optimized cases differ')
        receipt['passed'] = True
    receipt['targeted_passed'] = True
finally:
    (LOG / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps(dict(passed=receipt['passed'], variants=len(receipt['variants']))))
