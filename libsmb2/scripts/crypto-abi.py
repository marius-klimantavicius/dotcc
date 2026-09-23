#!/usr/bin/env python3
"""Compare pinned native and generated portable-crypto/ABI transcripts."""
import argparse
import json
import platform
import re
import time
from pathlib import Path
from common import ROOT, REPO, SOURCE_SPEC, fetch, run, sha

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--label', default='final', help='Receipt/build variant name (for example raw or final)')
parser.add_argument('--native-only', action='store_true', help='Validate only the native oracle; makes no managed claim')
parser.add_argument('--aot', action='store_true', help='Also publish and execute the generated consumer with NativeAOT')
parser.add_argument('--generated-project', type=Path, default=ROOT / 'generated/TranslatedLibsmb2/TranslatedLibsmb2.csproj',
                    help='Generated project under test (default: final postprocessed output)')
args = parser.parse_args()
if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]*', args.label):
    parser.error('--label must be a single alphanumeric variant name')
if args.native_only and args.aot:
    parser.error('--native-only and --aot cannot be combined')
if platform.system() != 'Linux' or platform.machine() not in ('x86_64', 'AMD64'):
    parser.error('The pinned ABI baseline targets Linux x64')

logs = ROOT / 'artifacts/crypto-abi' / args.label
build = ROOT / 'build/crypto-abi' / args.label
logs.mkdir(parents=True, exist_ok=True)
build.mkdir(parents=True, exist_ok=True)
probe = ROOT / 'tests/CryptoAbi'
receipt = dict(passed=False, label=args.label, scope='native-only' if args.native_only else ('native+jit+aot' if args.aot else 'native+jit'),
               started_unix=time.time(), source=SOURCE_SPEC, harness_sha256=sha(__file__), stages=[], checks=0)


def transcript(text):
    result = {}
    for line in text.splitlines():
        if not line:
            continue
        name, value = line.split('=', 1)
        if name in result:
            raise RuntimeError('Duplicate transcript key: ' + name)
        result[name] = value
    return result


def compare(text, expected, label):
    actual = transcript(text)
    differences = {name: dict(expected=expected.get(name), actual=actual.get(name))
                   for name in sorted(expected.keys() | actual.keys()) if expected.get(name) != actual.get(name)}
    if differences:
        (logs / (label + '-differences.json')).write_text(json.dumps(differences, indent=2) + '\n')
        raise RuntimeError(label + ' disagrees with the pinned baseline; see differences receipt')
    receipt['stages'].append(label)


try:
    source = fetch()
    expected = transcript((probe / 'expected.txt').read_text())
    receipt['checks'] = len(expected)
    receipt['test_inputs'] = {str(path.relative_to(ROOT)): sha(path) for path in sorted(probe.iterdir()) if path.is_file()}
    receipt['config'] = {str(path.relative_to(ROOT)): sha(path) for path in sorted((ROOT / 'config').rglob('*')) if path.is_file()}
    run(['cc', '--version'], logs / 'cc-version.log', receipt)
    units = ['sha1.c', 'sha224-256.c', 'sha384-512.c', 'usha.c', 'hmac.c', 'md4c.c', 'md5.c',
             'hmac-md5.c', 'aes.c', 'aes_reference.c', 'aes128ccm.c', 'smb2-signing.c', 'errors.c']
    receipt['native_units'] = {unit: sha(source / 'lib' / unit) for unit in units}
    # Native controls retain upstream's integer descriptor ABI. The default
    # translation profile registers an authored C# t_socket instead.
    defines = json.loads((ROOT / 'config/legacy-defines.json').read_text())
    command = ['cc', '-std=c17', '-O2', '-ffunction-sections', '-fdata-sections', '-D_DEFAULT_SOURCE', '-D_GNU_SOURCE',
               *['-D' + value for value in defines]]
    for include in (ROOT / 'config/managed', source / 'include', source / 'include/smb2', source / 'lib'):
        command += ['-I', include]
    native = build / 'native'
    command += [probe / 'native.c', *[source / 'lib' / unit for unit in units], '-Wl,--gc-sections', '-o', native]
    run(command, logs / 'native-build.log', receipt)
    native_text = run([native], logs / 'native.txt', receipt)
    compare(native_text, expected, 'native')
    receipt['native_binary_sha256'] = sha(native)
    if not args.native_only:
        generated = args.generated_project.resolve()
        if not generated.is_file():
            raise RuntimeError('Generated project is absent; run libsmb2/scripts/translate.sh first')
        receipt['generated_project'] = str(generated)
        receipt['generated_sources'] = {str(path.relative_to(generated.parent)): sha(path)
                                        for path in sorted(generated.parent.rglob('*.cs'))
                                        if 'obj' not in path.relative_to(generated.parent).parts and 'bin' not in path.relative_to(generated.parent).parts}
        run(['dotnet', '--info'], logs / 'dotnet-info.log', receipt)
        project_option = '-p:Libsmb2GeneratedProject=' + str(generated)
        project = probe / 'CryptoAbi.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo', project_option], logs / 'jit-build.log', receipt)
        assembly = probe / 'bin/Release/net10.0/CryptoAbi.dll'
        receipt['jit_binary_sha256'] = sha(assembly)
        receipt['generated_binary_sha256'] = sha(assembly.parent / 'TranslatedLibsmb2.dll')
        compare(run(['dotnet', assembly], logs / 'jit.txt', receipt), expected, 'jit')
        if args.aot:
            publish = build / 'aot'
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true',
                 '-p:PublishAot=true', project_option, '-o', publish, '--nologo'], logs / 'aot-build.log', receipt, timeout=1200)
            receipt['aot_binary_sha256'] = sha(publish / 'CryptoAbi')
            compare(run([publish / 'CryptoAbi'], logs / 'aot.txt', receipt), expected, 'aot')
    receipt['passed'] = True
    print(f"{receipt['scope']}: {len(expected)} checks matched; {logs / 'result.json'}")
except BaseException as error:
    receipt['failure'] = str(error)
    raise
finally:
    (logs / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
