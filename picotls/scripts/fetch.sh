#!/usr/bin/env bash
source "$(dirname -- "$0")/common.sh"
if [[ "${1:-}" == "--no-fetch" ]]; then
    if (( $# != 1 )); then echo "Usage: $0 [--no-fetch]" >&2; exit 1; fi
    exec "$PYTHON_CMD" - "$PICOTLS_ROOT" <<'PY'
import json
from pathlib import Path
import sys

campaign = Path(sys.argv[1])
inputs = json.loads((campaign / 'config/inputs.json').read_text())
source = campaign / 'ref' / inputs['picotls']['directory']
picotest = source / 'deps/picotest'
if not source.is_dir() or not picotest.is_dir():
    raise SystemExit(f'Missing pinned local source directories: {source} and {picotest}')
print(source)
PY
fi
if (( $# )); then echo "Usage: $0 [--no-fetch]" >&2; exit 1; fi
exec "$PYTHON_CMD" - "$PICOTLS_ROOT" <<'PY'
import hashlib
import json
from pathlib import Path
import sys
import tarfile
import urllib.request

campaign = Path(sys.argv[1])
ref = campaign / 'ref'
ref.mkdir(parents=True, exist_ok=True)
inputs = json.loads((campaign / 'config/inputs.json').read_text())

def obtain(item, destination):
    archive = ref / (item['directory'] + '.tar.gz')
    if not archive.exists():
        temporary = archive.with_suffix('.download')
        with urllib.request.urlopen(item['url']) as response, temporary.open('wb') as output:
            while data := response.read(1024 * 1024):
                output.write(data)
        if hashlib.sha256(temporary.read_bytes()).hexdigest() != item['sha256']:
            raise SystemExit(f'Checksum mismatch: {temporary}; download retained for inspection')
        temporary.rename(archive)
    if hashlib.sha256(archive.read_bytes()).hexdigest() != item['sha256']:
        raise SystemExit(f'Checksum mismatch: {archive}; refusing to overwrite it')
    with tarfile.open(archive, 'r:gz') as contents:
        for member in contents.getmembers():
            relative = Path(member.name).relative_to(item['directory'])
            if '..' in relative.parts or relative.is_absolute():
                raise SystemExit(f'Unsafe archive path: {member.name}')
            target = destination / relative
            if member.isdir():
                target.mkdir(parents=True, exist_ok=True)
            elif member.isfile():
                original = contents.extractfile(member).read()
                if target.is_symlink():
                    raise SystemExit(f'Symlink in reference input: {target}')
                if target.exists():
                    if target.read_bytes() != original:
                        raise SystemExit(f'Modified reference input: {target}; refusing to overwrite it')
                else:
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.write_bytes(original)
                    target.chmod(member.mode)
            else:
                raise SystemExit(f'Unsupported archive entry: {member.name}')
    print(f'Verified {archive.name}: {item["sha256"]}', file=sys.stderr)

source = ref / inputs['picotls']['directory']
obtain(inputs['picotls'], source)
obtain(inputs['picotest'], source / 'deps/picotest')
print(source)
PY
