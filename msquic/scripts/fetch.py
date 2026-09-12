#!/usr/bin/env python3
"""Fetch/verify the latest-main snapshot resolved for this scope probe."""
import hashlib
import json
from pathlib import Path
import tarfile
import urllib.request

root = Path(__file__).resolve().parents[1]
spec = json.loads((root / 'config/source.json').read_text())
ref = root / 'ref'
ref.mkdir(parents=True, exist_ok=True)
archive = ref / spec['archive']
if not archive.exists():
    temporary = archive.with_suffix('.download')
    with urllib.request.urlopen(spec['url']) as response, temporary.open('wb') as output:
        while data := response.read(1024 * 1024):
            output.write(data)
    if hashlib.sha256(temporary.read_bytes()).hexdigest() != spec['sha256']:
        raise SystemExit('Downloaded archive checksum mismatch')
    temporary.rename(archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != spec['sha256']:
    raise SystemExit('Archive checksum mismatch')
with tarfile.open(archive) as contents:
    for member in contents:
        relative = Path(member.name).relative_to(spec['directory'])
        if '..' in relative.parts or relative.is_absolute():
            raise SystemExit(f'Unsafe path: {member.name}')
        target = ref / spec['directory'] / relative
        if member.isdir():
            target.mkdir(parents=True, exist_ok=True)
        elif member.isfile():
            data = contents.extractfile(member).read()
            if target.is_symlink() or (target.exists() and target.read_bytes() != data):
                raise SystemExit(f'Modified reference file: {target}')
            if not target.exists():
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(data)
                target.chmod(member.mode)
        else:
            raise SystemExit(f'Unsupported archive entry: {member.name}')
(ref / 'snapshot.json').write_text(json.dumps(spec, indent=2) + '\n')
print(f'Verified {spec["commit"]} and all archived source files')
print(ref / spec['directory'])
