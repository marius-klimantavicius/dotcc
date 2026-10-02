#!/usr/bin/env python3
"""Acquire the pinned libc++ source and exact vendor configuration; never build tests."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import tarfile
import urllib.request

HERE = Path(__file__).resolve().parent


def sha256(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def acquire(output, archive=None):
    pin = json.loads((HERE / 'source.json').read_text())
    output = output.resolve()
    # Fresh directories preserve both success and failure artifacts. Nothing is
    # silently replaced when a source/configuration changes between invocations.
    output.mkdir(parents=True, exist_ok=False)
    (output / 'source.json').write_bytes((HERE / 'source.json').read_bytes())
    retained_archive = output / pin['archive']
    if archive:
        shutil.copy2(archive, retained_archive)
    else:
        with urllib.request.urlopen(pin['archiveUrl']) as source, retained_archive.open('xb') as target:
            shutil.copyfileobj(source, target)
    actual = sha256(retained_archive)
    if actual != pin['sha256']:
        raise RuntimeError(f'Pinned archive SHA-256 mismatch: {actual}; input retained at {retained_archive}')
    source_root = output / pin['archiveRoot']
    with tarfile.open(retained_archive, 'r:xz') as source:
        members = source.getmembers()
        for member in members:
            if member.name.split('/')[0] != pin['archiveRoot']:
                raise RuntimeError(f'Unexpected archive root: {member.name}')
        source.extractall(output, members=members, filter='data')
    if (source_root / 'LICENSE.TXT').read_bytes() != (HERE / 'LICENSE.TXT').read_bytes():
        raise RuntimeError('Pinned license text differs from source archive')
    configuration = output / 'config'
    shutil.copytree(HERE / 'config', configuration)
    shutil.copy2(source_root / 'vendor/llvm/default_assertion_handler.in',
                 configuration / '__assertion_handler')
    inputs = {}
    for parent in (source_root, configuration):
        for path in sorted(parent.rglob('*')):
            if path.is_file():
                inputs[str(path.relative_to(output))] = sha256(path)
    manifest = {
        'schema': 1, 'source': pin, 'fileHashes': inputs,
        'includeOrder': ['config', pin['archiveRoot'] + '/include', 'portable-libc-headers'],
        'patches': [], 'executedUpstreamTests': False,
        'providerStatus': 'source/configuration acquired; generated library acceptance is separate',
    }
    (output / 'acquisition.json').write_text(json.dumps(manifest, indent=2) + '\n')
    return output / 'acquisition.json'


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--archive', type=Path, help='reuse a retained archive, still verified against the pinned hash')
    args = parser.parse_args()
    print(acquire(args.output_dir, args.archive))
