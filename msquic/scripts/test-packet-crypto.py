#!/usr/bin/env python3
"""Native controls and raw/optimized JIT/AOT tests of the actual packet callbacks."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[1]
import sys
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import policy, observed_digest
LOG = ROOT / 'artifacts/packet-crypto'
BUILD = ROOT / 'build/packet-crypto'
LOG.mkdir(parents=True, exist_ok=True)
BUILD.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, transport_validated=False, tls_adapter_validated=False,
               commands=[], variants=[])


def sha(path):
    return observed_digest(path)


def run(command, name):
    args = [str(value) for value in command]
    receipt['commands'].append(dict(name=name, arguments=args))
    result = subprocess.run(args, capture_output=True, text=True, timeout=600)
    (LOG / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError(f'{name}: exit {result.returncode}; see {LOG / (name + ".log")}')
    return result.stdout


try:
    closure = ROOT / 'config/product-closure.json'
    receipt['product_closure_sha256'] = sha(closure)
    product = json.loads((ROOT / 'artifacts/product-build/results.json').read_text())
    if not product['passed']:
        raise RuntimeError('Product library build gate is not passing')
    run([sys.executable, ROOT / 'scripts/generate-crypto-test-table.py'], 'test-table')
    sources = [ROOT / 'src/BclHost' / name for name in ['Crypto.cs', 'MsQuicHost.Resources.cs', 'Status.cs']]
    sources += [ROOT / 'tests/PacketCrypto' / name for name in ['Program.cs', 'TestHost.cs']]
    sources += [ROOT / 'build/packet-crypto-common/Unexercised.cs']
    inputs = sources + [ROOT / 'tests/PacketCrypto/vectors.json', ROOT / 'tests/PacketCrypto/native.c']
    receipt['input_sha256'] = {str(path.relative_to(ROOT)): sha(path) for path in inputs}
    vectors = json.loads((ROOT / 'tests/PacketCrypto/vectors.json').read_text())
    reference = ROOT / ('ref/msquic-' + vectors['source_commit'])
    if sha(reference / vectors['source']) != vectors['source_sha256']:
        policy().issue('Native vector source differs from the pinned input')
    vector = vectors['initial_v1']
    native_header = BUILD / 'native-vectors.h'
    native_header.write_text('\n'.join('#define ' + name + ' "' + vector[field] + '"'
        for name, field in [('INITIAL_HEADER', 'unprotected_header'), ('INITIAL_PAYLOAD', 'crypto_payload'),
                            ('INITIAL_PACKET', 'protected_packet')]) + '\n')
    native_build = ROOT / 'build/native-oracle'
    platform = native_build / 'obj/Release/libmsquic_platform.a'
    crypto = native_build / '_deps/opensslquic-build/quictls/lib'
    native = BUILD / 'native'
    receipt['native_libraries_sha256'] = {str(path.relative_to(ROOT)): sha(path)
        for path in [platform, crypto / 'libssl.a', crypto / 'libcrypto.a']}
    run(['gcc', '-std=gnu17', '-fms-extensions', '-O2', '-DCX_PLATFORM_LINUX', '-D_GNU_SOURCE',
         '-DNDEBUG', '-DQUIC_EVENTS_STUB', '-DQUIC_LOGS_STUB', '-I' + str(reference / 'src/inc'),
         '-include', native_header, ROOT / 'tests/PacketCrypto/native.c', platform,
         '-L' + str(crypto), '-lssl', '-lcrypto', '-ldl', '-lpthread', '-o', native], 'native-build')
    native_output = run([native], 'native')
    if native_output.strip() != 'native packet-crypto: Initial, AES128/256 HP, and four KBKDF controls passed':
        raise RuntimeError('Unexpected native control output')
    receipt['native'] = dict(passed=True, output=native_output.strip(), executable_sha256=sha(native))
    for variant in ['raw', 'optimized']:
        directory = BUILD / variant
        directory.mkdir(exist_ok=True)
        generated = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
        generated_sources = (generated / 'Dotcc.SourceFiles.txt').read_text().splitlines()
        hashes = {name: sha(generated / name) for name in generated_sources}
        source_items = '\n'.join('    <Compile Include="' + escape(str(path)) + '" />' for path in sources)
        project = directory / 'PacketCrypto.csproj'
        project.write_text('''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings>
    <IsAotCompatible>true</IsAotCompatible></PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="''' + escape(str(generated / 'TranslatedMsQuic.csproj')) + '''" />
''' + source_items + '''
    <None Include="''' + escape(str(ROOT / 'tests/PacketCrypto/vectors.json')) + '''" Link="vectors.json" CopyToOutputDirectory="Always" />
  </ItemGroup>
</Project>
''')
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:BuildProjectReferences=false'], variant + '-build')
        jit = run(['dotnet', directory / 'bin/Release/net10.0/PacketCrypto.dll'], variant + '-jit')
        if jit.strip() != 'packet-crypto: 167 checks passed; resources drained':
            raise RuntimeError('Unexpected JIT receipt')
        run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
             '-p:BuildProjectReferences=false', '-o', directory / 'aot', '--nologo'], variant + '-aot-build')
        aot = run([directory / 'aot/PacketCrypto'], variant + '-aot')
        if aot != jit:
            raise RuntimeError('JIT/AOT output mismatch')
        receipt['variants'].append(dict(name=variant, passed=True, output=jit.strip(),
            generated_sha256=hashes, aot_sha256=sha(directory / 'aot/PacketCrypto')))
        if any(sha(generated / name) != value for name, value in hashes.items()):
            policy().issue('Generated source changed during packet tests')
        print(variant + ': 167 packet checks PASS under JIT and NativeAOT', flush=True)
    if sha(closure) != receipt['product_closure_sha256']:
        policy().issue('Frozen product closure changed during packet tests')
    if any(sha(ROOT / path) != value for path, value in receipt['input_sha256'].items()):
        policy().issue('Packet test source changed during execution')
    receipt['passed'] = True
finally:
    (LOG / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'variants': len(receipt['variants'])}))
