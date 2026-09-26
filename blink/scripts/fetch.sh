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
# Existing source trees may evolve. Text adapters check their own replacement
# inputs; compilation and tests determine compatibility of all other inputs.
for record in manifest['licenses'] + manifest['bootstrapTools'] + manifest['selectedAssemblyTests'] + [manifest['assemblyInclude']]:
    if not (source / record['path']).is_file():
        raise SystemExit(f'missing selected input: {record["path"]}')
print(source)
PY
