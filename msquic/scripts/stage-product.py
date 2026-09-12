#!/usr/bin/env python3
"""Stage unchanged selected upstream inputs plus explicitly recorded host overlays."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
subprocess.run([sys.executable, str(ROOT / 'scripts/fetch.py')], check=True)
pin = json.loads((ROOT / 'config/source.json').read_text())
inventory = json.loads((ROOT / 'config/source-inventory.json').read_text())
host = ROOT / 'config/managed-host'
overlay = json.loads((host / 'overlay.json').read_text())
reference = ROOT / 'ref' / pin['directory']
stage = ROOT / 'build/product-source'
stage.mkdir(parents=True, exist_ok=True)
replacements = {item['source']: item for item in overlay['overlays']}
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
        relative = str(source.relative_to(reference))
        if relative in replacements:
            replacement = replacements[relative]
            if digest(source.read_bytes()) != replacement['original_sha256']:
                raise RuntimeError('Overlay source pin mismatch: ' + relative)
            path = host / replacement['overlay']
            write(relative, path.read_bytes(), str(path.relative_to(ROOT)), 'host overlay')
        else:
            write(relative, source.read_bytes(), relative, 'unchanged upstream')
for prefix, directory in [('system', host / 'system'), ('host', ROOT / 'src/Host')]:
    for source in sorted(directory.rglob('*')):
        if source.is_file():
            write(str(Path(prefix) / source.relative_to(directory)), source.read_bytes(),
                  str(source.relative_to(ROOT)), 'authored host contract')
for unit in inventory['units']:
    relative = unit['path']
    if relative in manifest['excluded_units']:
        continue
    if digest((reference / relative).read_bytes()) != unit['sha256']:
        raise RuntimeError('Source inventory pin mismatch: ' + relative)
    manifest['units'].append(relative)
manifest['units'].extend(str(Path('host') / p.relative_to(ROOT / 'src/Host'))
                         for p in sorted((ROOT / 'src/Host').glob('*.c')))
for key, filename in [('portable_fragment', 'portable.c'), ('route_fragment', 'route.c')]:
    fragment = overlay[key]
    text = (reference / fragment['source']).read_text()
    extracted = text[text.index(fragment['start']):text.index(fragment['end'])]
    if digest(extracted.encode()) != fragment['sha256'] or not (ROOT / 'src/Host' / filename).read_text().endswith(extracted):
        raise RuntimeError('Portable upstream fragment changed: ' + filename)
# Remove obsolete files only from this generated staging directory. Never edit ref.
selected = {entry['path'] for entry in manifest['files']}
for path in stage.rglob('*'):
    if path.is_file() and str(path.relative_to(stage)) not in selected:
        path.unlink()
(stage / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
print(json.dumps({'stage': str(stage), 'units': len(manifest['units']),
                  'files': len(manifest['files']), 'overlays': len(replacements)}))
