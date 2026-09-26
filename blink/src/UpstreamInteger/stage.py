#!/usr/bin/env python3
"""Stage reviewed INC/NEG auxiliary-carry and CMPXCHG8B width corrections."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['src/UpstreamInteger/stage.py']

import argparse
import difflib
import hashlib
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REVISION = _CAMPAIGN_SOURCE["revision"]
UPSTREAM = ROOT / 'ref' / ('blink-' + REVISION) / 'blink'
sha = lambda value: hashlib.sha256(value).hexdigest()
SOURCE_PINS = _CAMPAIGN_INPUTS['SOURCE_PINS']
BLOCK_PINS = _CAMPAIGN_INPUTS['BLOCK_PINS']
GUARD = '#ifndef DISABLE_JIT\n#error Reviewed integer staging requires DISABLE_JIT\n#endif\n'

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', type=Path, required=True)
parser.add_argument('--receipt', type=Path, required=True)
args = parser.parse_args()
for target in (args.output.resolve(), args.receipt.resolve()):
    if target.is_relative_to((ROOT/'ref').resolve()) or target.is_relative_to(HERE):
        raise SystemExit('Staging output/receipt must not overwrite the immutable reference or authored sources')

script_hash = sha(Path(__file__).read_bytes())
reviewed_patch = (HERE/'integer.patch').read_bytes()
receipt = {'kind': 'reviewed-upstream-integer-correction', 'upstream': REVISION,
           'stage_sha256': script_hash,
           'scope': 'INC/NEG8/16/32/64 AF and CMPXCHG8B nonmatch zeroextension; DISABLE_JIT required',
           'sources': {}}
staged_files, patch = {}, ''
for name, expected in SOURCE_PINS.items():
    original = (UPSTREAM/name).read_bytes()
    if sha(original) != expected:
        raise SystemExit('Immutable upstream source mismatch: ' + name)
    source = original.decode()
    blocks = []
    for marker, block_hash in BLOCK_PINS[name].items():
        if source.count(marker) != 1:
            raise SystemExit('Upstream function boundary differs: ' + marker)
        start = source.index(marker)
        end = source.index('\n}', start) + 3
        before = source[start:end]
        if sha(before.encode()) != block_hash:
            raise SystemExit('Upstream function hash differs: ' + marker)
        if marker.startswith('i64 Neg'):
            old, new = '  af = cf = !!x;', '  af = !!(x & 15);\n  cf = !!x;'
        elif name == 'alu.c':
            old, new = 'af = (z & 15) < (y & 15);', 'af = (z & 15) < (x & 15);'
        else:
            old = '    Write32(m->ax, a);\n    Write32(m->dx, d);'
            new = '    Write64(m->ax, a);\n    Write64(m->dx, d);'
        if before.count(old) != 1:
            raise SystemExit('Reviewed expression differs: ' + marker)
        after = before.replace(old, new)
        source = source[:start] + after + source[end:]
        blocks.append({'function': marker, 'original_sha256': block_hash,
                       'replacement_sha256': sha(after.encode())})
    # Each marker lies after the ordinary includes that establish config.h.
    first = next(iter(BLOCK_PINS[name]))
    source = source.replace(first, GUARD + first, 1)
    staged_files[name] = source.encode()
    patch += ''.join(difflib.unified_diff(original.decode().splitlines(True), source.splitlines(True),
                                        fromfile='a/blink/'+name, tofile='b/blink/'+name))
    receipt['sources'][name] = {'source_sha256': expected, 'staged_sha256': sha(source.encode()),
                                'blocks': blocks}
if patch.encode() != reviewed_patch:
    raise SystemExit('Generated integer diff differs from the checked-in review patch')
if sha(Path(__file__).read_bytes()) != script_hash or (HERE/'integer.patch').read_bytes() != reviewed_patch:
    raise SystemExit('Staging implementation changed during derivation')
args.output.mkdir(parents=True, exist_ok=True)
for name, value in staged_files.items():
    (args.output/name).write_bytes(value)
(args.output/'integer.patch').write_bytes(reviewed_patch)
receipt['patch_sha256'] = sha(reviewed_patch)
args.receipt.parent.mkdir(parents=True, exist_ok=True)
pending = args.receipt.with_suffix(args.receipt.suffix + '.tmp')
pending.write_text(json.dumps(receipt, indent=2) + '\n')
pending.replace(args.receipt)
