#!/usr/bin/env python3
"""Build the pinned, separate native reference. Never used by the product."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]


def run(command, name):
    logs = ROOT / 'artifacts/native-oracle'
    logs.mkdir(parents=True, exist_ok=True)
    (logs / f'{name}.command.json').write_text(json.dumps(command, indent=2) + '\n')
    with (logs / f'{name}.log').open('w') as output:
        result = subprocess.run(command, stdout=output, stderr=subprocess.STDOUT)
    if result.returncode:
        raise SystemExit(f'{name} failed ({result.returncode}); see {logs / (name + ".log")}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--jobs', type=int, default=4)
    args = parser.parse_args()
    if args.jobs < 1:
        parser.error('--jobs must be positive')
    # Upstream's OpenSSL custom command uses ProcessorCount independently of
    # cmake --parallel. Bound its processor discovery on the Linux first target.
    if hasattr(os, 'sched_getaffinity'):
        os.sched_setaffinity(0, sorted(os.sched_getaffinity(0))[:args.jobs])
    source = json.loads((ROOT / 'config/source.json').read_text())
    native = json.loads((ROOT / 'config/native-inputs.json').read_text())
    run(['python3', str(ROOT / 'scripts/fetch.py')], 'verify-msquic')
    dependency = native['quictls']
    archive = ROOT / 'ref' / dependency['archive']
    if not archive.exists():
        pending = archive.with_suffix('.download')
        urllib.request.urlretrieve(dependency['url'], pending)
        pending.replace(archive)
    if hashlib.sha256(archive.read_bytes()).hexdigest() != dependency['sha256']:
        raise SystemExit('quictls archive checksum mismatch')
    # CMake/Perl may create source-side outputs. Isolate those from immutable ref/.
    work = ROOT / 'build/native-oracle-source'
    if not work.exists():
        shutil.copytree(ROOT / 'ref' / source['directory'], work)
    tls = work / 'submodules/quictls'
    if not (tls / 'Configure').exists():
        extraction = ROOT / 'build/native-dependencies'
        extraction.mkdir(parents=True, exist_ok=True)
        with tarfile.open(archive) as package:
            top = Path(package.getmembers()[0].name).parts[0]
            package.extractall(extraction, filter='data')
        shutil.copytree(extraction / top, tls, dirs_exist_ok=True)
    build = ROOT / 'build/native-oracle'
    run(['cmake', '-S', str(work), '-B', str(build), '-G', 'Ninja',
         '-DCMAKE_BUILD_TYPE=Release', '-DQUIC_TLS_LIB=quictls',
         '-DCMAKE_C_FLAGS=-DIS_OPENSSL_3=1 -DVER_GIT_HASH=' + source['commit'],
         '-DQUIC_BUILD_TOOLS=ON', '-DQUIC_BUILD_TEST=OFF',
         '-DQUIC_BUILD_PERF=OFF', '-DQUIC_ENABLE_LOGGING=OFF',
         '-DQUIC_EMBED_GIT_HASH=OFF', '-DQUIC_SOURCE_LINK=OFF'], 'configure')
    run(['cmake', '--build', str(build), '--target', 'quicsample',
         '--parallel', str(args.jobs)], 'build')
    receipt = {'msquic': source, 'native_inputs': native,
               'target': 'quicsample', 'result': 'built',
               'product_dependency': False,
               'runtime_gate': 'not run; compilation is not interop evidence'}
    (ROOT / 'artifacts/native-oracle/build-results.json').write_text(
        json.dumps(receipt, indent=2) + '\n')
    print(build / 'bin/Release/quicsample')


if __name__ == '__main__':
    main()
