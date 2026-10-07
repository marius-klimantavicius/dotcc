#!/usr/bin/env python3
"""Differential strict inet_pton and owner/thread errno checks, JIT and NativeAOT."""
import argparse
import ipaddress
import json
from pathlib import Path
import platform
import random
import shutil
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output-dir', type=Path, required=True)
parser.add_argument('--nativeaot', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
v4 = ['', '0.0.0.0', '255.255.255.255', '127.0.0.1', '192.0.2.1',
      '127.1', '1', '0x7f.0.0.1', '0177.0.0.1', '1.2.3.04', '01.2.3.4',
      '256.0.0.1', '1.2.3', '1.2.3.4.5', '.1.2.3', '1..2.3', '1.2.3.',
      '+1.2.3.4', '-1.2.3.4', ' 1.2.3.4', '1.2.3.4 ', '1.2.3.4/24',
      'example.com', '１.2.3.4', '12345678901234567890123456789012345678901234567890']
v6 = ['', '::', '::1', '1::', '2001:db8::1', '1:2:3:4:5:6:7:8',
      'ABCD:EF01:2345:6789:abcd:ef01:2345:6789', '::ffff:192.0.2.1',
      '::192.0.2.1', '1:2:3:4:5:6:192.0.2.1', '1:2:3:4:5::192.0.2.1',
      'ffff:ffff:ffff:ffff:ffff:ffff:255.255.255.255', ':', ':::', '1:::2',
      ':1', '1:', '1:2:3:4:5:6:7', '1:2:3:4:5:6:7:8:9', '1::2::3',
      '1:2:3:4:5:6:7:8::', '::1:2:3:4:5:6:7:8', '12345::', 'gggg::',
      '[::1]', 'fe80::1%eth0', 'fe80::1%2', '::ffff:192.168.001.1',
      '::ffff:256.1.1.1', '::ffff:127.1', '::ffff:0x7f.0.0.1',
      '1:2:3:4:5:6::192.0.2.1', '::1.2.3.4:5', ' ::1', '::1 ', '::1/128']
vectors = [(2, value) for value in v4] + [(10, value) for value in v6]
rng = random.Random(930)
for _ in range(160):
    ipv4 = ipaddress.IPv4Address(rng.getrandbits(32))
    ipv6 = ipaddress.IPv6Address(rng.getrandbits(128))
    vectors += [(2, str(ipv4)), (10, str(ipv6)), (10, ipv6.exploded),
                (10, '::ffff:' + str(ipv4)), (10, str(ipv6) + ':'),
                (2, '0' + str(ipv4)), (10, '[' + str(ipv6) + ']')]
vectors += [(2, '::1'), (10, '127.0.0.1'), (0, '127.0.0.1'), (-1, '::1'), (999, '')]
inputs = ''.join(f'{family}\t{value}\n' for family, value in vectors).encode()
(out / 'vectors.txt').write_bytes(inputs)
for name in ('LibcContext.cs', 'LibcInet.cs'):
    shutil.copy2(root / 'backend/runtime' / name, out / name)
shutil.copy2(root / 'backend/tests/libc_inet.cs', out / 'Program.cs')
shutil.copy2(root / 'backend/tests/libc_inet_native.c', out / 'native.c')
project = out / 'InetProbe.csproj'
project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors><Nullable>enable</Nullable>
</PropertyGroup></Project>''')
results = {'vectors': len(vectors), 'runs': [], 'passed': False}
def save():
    (out / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
def run(label, command, input=None):
    command = list(map(str, command))
    result = subprocess.run(command, input=input, capture_output=True)
    (out / (label + '.stdout')).write_bytes(result.stdout)
    (out / (label + '.stderr')).write_bytes(result.stderr)
    results['runs'].append({'label': label, 'command': command, 'exit': result.returncode})
    save()
    if result.returncode:
        raise RuntimeError(result.stdout.decode() + result.stderr.decode())
    return result.stdout
run('native-build', ['cc', '-std=c11', '-Wall', '-Wextra', '-Werror', out / 'native.c', '-o', out / 'native'])
expected = run('native', [out / 'native'], inputs)
run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo'])
actual = run('jit', ['dotnet', out / 'bin/Release/net10.0/InetProbe.dll'], inputs)
assert actual == expected, 'JIT differs from native inet_pton; inspect saved stdout'
results['jitMatchesNative'] = True
if args.nativeaot:
    system = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}[platform.system()]
    arch = 'arm64' if platform.machine().lower() in ('arm64', 'aarch64') else 'x64'
    run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', system + '-' + arch,
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot'])
    actual = run('aot', [out / 'aot' / ('InetProbe.exe' if system == 'win' else 'InetProbe')], inputs)
    assert actual == expected, 'NativeAOT differs from native inet_pton'
    results['aotMatchesNative'] = True
results['passed'] = True
save()
print(f'inet_pton: {len(vectors)} vectors match native; owner/thread errno checks passed')
