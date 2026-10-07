#!/usr/bin/env python3
"""Acquire and verify the complete pinned DuckDB source archive (without Git)."""
import argparse
import hashlib
import json
from pathlib import Path
import tarfile
import urllib.request

PIN = Path(__file__).parent / 'config/source.json'


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def acquire(output):
    output = Path(output).resolve()
    output.mkdir(parents=True, exist_ok=True)
    pin = json.loads(PIN.read_text())
    archive = output / pin['archive']
    if not archive.exists():
        temporary = archive.with_suffix(archive.suffix + '.download')
        with urllib.request.urlopen(pin['url']) as src, temporary.open('wb') as dst:
            while block := src.read(1024 * 1024):
                dst.write(block)
        temporary.replace(archive)
    if sha(archive) != pin['sha256']:
        raise ValueError('Pinned DuckDB archive hash mismatch: ' + str(archive))
    source = output / 'source'
    with tarfile.open(archive) as contents:
        entries = contents.getmembers()
        prefix = entries[0].name.split('/')[0]
        if not source.exists():
            contents.extractall(output, filter='data')
            (output / prefix).rename(source)
        verified, symlinks = [], []
        for entry in entries:
            relative = Path(entry.name).relative_to(prefix)
            path = source / relative
            if entry.issym():
                if not path.is_symlink() or str(path.readlink()) != entry.linkname:
                    raise ValueError('Modified pinned symlink: ' + str(relative))
                symlinks.append(dict(path=str(relative), target=entry.linkname))
                continue
            if entry.islnk():
                raise ValueError('Unexpected hard link in pinned source archive: ' + entry.name)
            if not entry.isfile():
                continue
            expected = hashlib.sha256(contents.extractfile(entry).read()).hexdigest()
            if path.is_symlink() or sha(path) != expected:
                raise ValueError('Modified pinned source: ' + str(relative))
            verified.append(dict(path=str(relative), sha256=expected))
        known = {item['path'] for item in verified + symlinks}
        actual = {str(path.relative_to(source)) for path in source.rglob('*')
                  if path.is_symlink() or not path.is_dir()}
        if actual != known:
            raise ValueError('Pinned source inventory mismatch: extras=' + str(sorted(actual - known)) +
                             ', missing=' + str(sorted(known - actual)))
    receipt = dict(pin=pin, archive=str(archive), source=str(source),
                   verifiedFiles=len(verified), files=verified, symlinks=symlinks)
    (output / 'source-manifest.json').write_text(json.dumps(receipt, indent=2) + '\n')
    return source


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    print(acquire(parser.parse_args().output_dir))
