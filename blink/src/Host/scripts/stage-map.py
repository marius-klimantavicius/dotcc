#!/usr/bin/env python3
"""Apply the reviewed two-function host capability boundary to pinned map.c."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['src/Host/scripts/stage-map.py']

import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
UPSTREAM = ROOT / ("ref/" + _CAMPAIGN_SOURCE["directory"] + '/blink/map.c')
PIN = _CAMPAIGN_INPUTS['PIN']
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', required=True, type=Path)
parser.add_argument('--receipt', required=True, type=Path)
args = parser.parse_args()
original = UPSTREAM.read_bytes()
if hashlib.sha256(original).hexdigest() != PIN:
    raise SystemExit('immutable upstream map.c checksum mismatch')
source = original.decode()
changes = []
for signature, next_signature, body in [
    ('static long GetSystemPageSize(void)', 'static void *PortableMmap',
     'return BlinkHostMemoryPageSize();'),
    ('static int GetBitsInAddressSpace(void)', 'static u64 GetVirtualAddressSpace',
     'return BlinkHostMemoryAddressBits();')]:
    start = source.index(signature)
    end = source.index(next_signature, start)
    before = source[start:end]
    after = signature + ' {\n  ' + body + '\n}\n\n'
    source = source[:start] + after + source[end:]
    changes.append(dict(function=signature, beforeSha256=hashlib.sha256(before.encode()).hexdigest(),
                        replacement=after))
include = '#include "blink/map.h"\n'
if source.count(include) != 1:
    raise SystemExit('map include boundary changed')
source = source.replace(include, include + '#include "HostMemory.h"\n')
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(source)
args.receipt.parent.mkdir(parents=True, exist_ok=True)
args.receipt.write_text(json.dumps(dict(kind='explicit-nonlinear-host-capabilities-only',
    originalSha256=PIN, stagedSha256=hashlib.sha256(source.encode()).hexdigest(),
    changes=changes, addedInclude='HostMemory.h',
    requiredCapabilities=['NOLINEAR', 'HAVE_MAP_ANONYMOUS', 'campaign sys/mman.h']), indent=2)+'\n')
