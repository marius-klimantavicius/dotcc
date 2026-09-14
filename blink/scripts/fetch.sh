#!/usr/bin/env bash
set -euo pipefail
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
python3 - "$campaign" "${1:-}" <<'PY'
import hashlib, json, pathlib, subprocess, sys, tarfile
root = pathlib.Path(sys.argv[1])
if sys.argv[2] not in ('', '--offline'):
    raise SystemExit('usage: fetch.sh [--offline]')
manifest = json.loads((root / 'config/source-manifest.json').read_text())
upstream = manifest['upstream']
ref = root / 'ref'
ref.mkdir(exist_ok=True)
archive = ref / upstream['archive']
if not archive.exists():
    if sys.argv[2] == '--offline':
        raise SystemExit(f'missing offline input: {archive}')
    temporary = archive.with_suffix('.download')
    subprocess.run(['curl', '--fail', '--location', '--retry', '3', upstream['url'], '-o', str(temporary)], check=True)
    if hashlib.sha256(temporary.read_bytes()).hexdigest() != upstream['sha256']:
        raise SystemExit(f'archive checksum mismatch: {temporary}')
    temporary.replace(archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != upstream['sha256']:
    raise SystemExit(f'archive checksum mismatch: {archive}')
source = ref / upstream['directory']
with tarfile.open(archive) as tar:
    if not source.exists():
        tar.extractall(ref, filter='data')
    expected = set()
    for entry in tar.getmembers():
        path = ref / entry.name
        expected.add(path)
        if entry.isfile():
            if not path.is_file() or path.is_symlink() or path.read_bytes() != tar.extractfile(entry).read():
                raise SystemExit(f'immutable source differs: {path}')
        elif entry.issym():
            if not path.is_symlink() or str(path.readlink()) != entry.linkname:
                raise SystemExit(f'immutable symlink differs: {path}')
        elif entry.isdir() and not path.is_dir():
            raise SystemExit(f'immutable directory missing: {path}')
    extras = set(source.rglob('*')) - expected
    if extras:
        raise SystemExit(f'unexpected files in immutable source: {sorted(map(str, extras))[:5]}')
for record in manifest['licenses'] + manifest['bootstrapTools'] + manifest['selectedAssemblyTests'] + [manifest['assemblyInclude']]:
    if hashlib.sha256((source / record['path']).read_bytes()).hexdigest() != record['sha256']:
        raise SystemExit(f'manifest checksum mismatch: {record["path"]}')
print(source)
PY
