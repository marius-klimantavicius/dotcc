#!/usr/bin/env python3
"""Qualify unchanged Kestrel ELF with fixed configuration and diagnostic IPC disabled."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import runpy
import shutil
import signal
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[3]
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--guest-receipt', required=True, type=Path)
    args = parser.parse_args()
    base = ROOT / 'blink/artifacts/kestrel-native-profile'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    print(attempt, flush=True)
    receipt = dict(passed=False, completed=False, inputs={}, scope=__doc__)
    def pin(path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError('Input identity differs: ' + str(path))
        receipt['inputs'][str(path)] = digest
        return path
    try:
        pin(__file__)
        helper = pin(Path(__file__).with_name('build-native.py'))
        shutil.copy2(helper, attempt / 'build-native.py')
        shutil.copy2(__file__, attempt / 'native-profile.py')
        pin(sys.executable)
        parent_path = pin(args.guest_receipt)
        parent = json.loads(parent_path.read_text())
        if not parent.get('passed') or not parent.get('static_elf_verified') or not parent['native_cleanup']['normal']:
            raise RuntimeError('Passing static Kestrel native receipt required')
        binary = pin(parent['binary']['path'], parent['binary']['sha256'])
        receipt['producer'] = dict(path=str(parent_path), sha256=sha(parent_path))
        receipt['binary'] = dict(parent['binary'])
        receipt['tools'] = {'strace': dict(parent['tools']['strace'])}
        pin(receipt['tools']['strace']['path'], receipt['tools']['strace']['sha256'])
        api = runpy.run_path(str(helper), run_name='kestrel_native_helper')
        environment = dict(api['GUEST_ENVIRONMENT'],
            DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE='false', DOTNET_EnableDiagnostics='0')
        receipt['configuration'] = dict(environment=environment,
            rationale='Fixed image/configuration needs no hot reload; this fixture does not expose debugger or diagnostic IPC. Kestrel transport is unchanged.',
            documentation=[
                'https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/?view=aspnetcore-10.0',
                'https://learn.microsoft.com/en-us/dotnet/core/runtime-config/debugging-profiling'])
        api['native_oracle'](attempt, binary, receipt, environment)
        # Preserve every real Date byte, comparing all other semantic fields.
        prior = {row['name']: row for row in parent['native_cases']}
        for row in receipt['native_cases']:
            expected = prior[row['name']]
            for field in ('request_sha256', 'request_bytes', 'write_end_offsets'):
                if row[field] != expected[field]:
                    raise RuntimeError('Native request schedule changed')
            comparable = lambda value: {k: v for k, v in value.items() if k != 'date'}
            if comparable(row['semantic']) != comparable(expected['semantic']):
                raise RuntimeError('Native HTTP semantics changed')
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest:
                raise RuntimeError('Input changed during native profile: ' + path)
        receipt.update(passed=True, final_identities_stable=True)
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
        raise
    finally:
        receipt['completed'] = True
        receipt['artifacts'] = {str(p.relative_to(attempt)): sha(p) for p in attempt.iterdir()
                                if p.is_file() and p.name != 'receipt.json'}
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
        print(json.dumps(dict(passed=receipt['passed'], receipt=str(attempt / 'receipt.json'))), flush=True)


if __name__ == '__main__':
    def interrupted(number, frame):
        raise InterruptedError(f'received signal {number}')
    signal.signal(signal.SIGTERM, interrupted)
    signal.signal(signal.SIGALRM, interrupted)
    main()
