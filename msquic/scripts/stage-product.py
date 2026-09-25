#!/usr/bin/env python3
"""Stage unchanged selected upstream inputs plus explicitly recorded host overlays."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys
from source_inputs import portable_fragments, source_units

ROOT = Path(__file__).resolve().parents[1]
pin = json.loads((ROOT / 'config/source.json').read_text())
if '--no-fetch' in sys.argv[1:]:
    if sys.argv[1:] != ['--no-fetch']:
        raise SystemExit('Usage: stage-product.py [--no-fetch]')
else:
    if sys.argv[1:]:
        raise SystemExit('Usage: stage-product.py [--no-fetch]')
    subprocess.run([sys.executable, str(ROOT / 'scripts/fetch.py')], check=True)
host = ROOT / 'config/managed-host'
overlay = json.loads((host / 'overlay.json').read_text())
reference = ROOT / 'ref' / pin['directory']
if not reference.is_dir():
    raise SystemExit('Missing pinned source directory: ' + str(reference))
units = source_units(reference)
# These are unchanged upstream implementations, not maintained source patches.
# Refresh them before copying the host inputs for this selected version.
for filename, contents in portable_fragments(reference, overlay).items():
    target = ROOT / 'src/Host' / filename
    if not target.is_file() or target.read_text() != contents:
        target.write_text(contents)
stage = ROOT / 'build/product-source'
stage.mkdir(parents=True, exist_ok=True)
replacements = {item['source']: item for item in overlay['overlays']}
for relative in replacements:
    if not (reference / relative).is_file():
        raise RuntimeError('Missing upstream header for host overlay: ' + relative)
manifest = dict(revision=pin['commit'], source_archive_sha256=pin['sha256'],
    data_model='LP64, little-endian, Linux x64; host ABI validation required',
    product_closure_frozen=False, files=[], units=[], excluded_units=['src/platform/pcp.c'],
    defines=['CX_PLATFORM_LINUX=1', '__linux__=1', '_GNU_SOURCE=1', 'NDEBUG=1',
             'QUIC_BUILD_STATIC=1', 'QUIC_EVENTS_STUB=1', 'QUIC_LOGS_STUB=1',
             'VER_GIT_HASH=' + pin['commit']],
    include_dirs=['src/inc', 'src/core', 'src/platform', 'host', 'system'])


def digest(contents):
    return hashlib.sha256(contents).hexdigest()


def write(relative, contents, origin, role):
    target = stage / relative
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(contents)
    manifest['files'].append(dict(path=relative, sha256=digest(contents), origin=origin, role=role))


for directory in ('src/inc', 'src/core', 'src/platform'):
    for source in sorted((reference / directory).rglob('*')):
        if not source.is_file():
            continue
        relative = source.relative_to(reference).as_posix()
        if relative in replacements:
            replacement = replacements[relative]
            path = host / replacement['overlay']
            write(relative, path.read_bytes(), str(path.relative_to(ROOT)), 'host overlay')
        else:
            write(relative, source.read_bytes(), relative, 'unchanged upstream')
for prefix, directory in [('system', host / 'system'), ('host', ROOT / 'src/Host')]:
    for source in sorted(directory.rglob('*')):
        if source.is_file():
            write((Path(prefix) / source.relative_to(directory)).as_posix(), source.read_bytes(),
                  source.relative_to(ROOT).as_posix(), 'authored host contract')
for relative in units:
    if relative in manifest['excluded_units']:
        continue
    manifest['units'].append(relative)
manifest['units'].extend((Path('host') / p.relative_to(ROOT / 'src/Host')).as_posix()
                         for p in sorted((ROOT / 'src/Host').glob('*.c')))
# Remove obsolete files only from this generated staging directory. Never edit ref.
selected = {entry['path'] for entry in manifest['files']}
for path in stage.rglob('*'):
    if path.is_file() and path.relative_to(stage).as_posix() not in selected:
        path.unlink()
(stage / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
print(json.dumps({'stage': str(stage), 'units': len(manifest['units']),
                  'files': len(manifest['files']), 'overlays': len(replacements)}))
