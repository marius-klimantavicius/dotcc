#!/usr/bin/env python3
"""Extract the decoder's Mode macro dependency without Machine/POSIX headers."""
import hashlib, json, pathlib
root = pathlib.Path(__file__).resolve().parents[1]
upstream = root / 'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
inventory = json.loads((root / 'config/source-inventory.json').read_text())
hashes = {row['path']: row['sha256'] for row in inventory['files']}
for name in ['blink/x86.c', 'blink/modrm.h']:
    actual = hashlib.sha256((upstream / name).read_bytes()).hexdigest()
    if actual != hashes[name]:
        raise SystemExit('upstream checksum mismatch: ' + name)
source = (upstream / 'blink/x86.c').read_text()
needle = '#include "blink/modrm.h"'
assert source.count(needle) == 1
# The only modrm.h dependency used in x86.c is Mode. Preserve its exact
# DISABLE_METAL expansion; other processor modes remain outside this probe.
assert '#define Mode(x) XED_MODE_LONG' in (upstream / 'blink/modrm.h').read_text()
staged = root / 'generated/decoder-profile'
staged.mkdir(parents=True, exist_ok=True)
(staged / 'config.h').write_bytes((root / 'config/decoder-config.h').read_bytes())
(staged / 'x86.c').write_text(source.replace(needle, '#define Mode(x) XED_MODE_LONG'))
(staged / 'adaptation.json').write_text(json.dumps({
    'upstream_sha256': hashes['blink/x86.c'],
    'staged_sha256': hashlib.sha256((staged / 'x86.c').read_bytes()).hexdigest(),
    'adaptation': 'Replace modrm.h include with exact upstream DISABLE_METAL Mode expansion; no instruction algorithm edits.',
    'scope': 'decoder-only Linux x86-64 probe; not the full interpreter host port'
}, indent=2) + '\n')
