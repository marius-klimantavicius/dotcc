#!/usr/bin/env python3
"""Replace only unsafe host diagnostic probing in pinned upstream debug.c."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['src/Host/scripts/stage-debug.py']

import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
UPSTREAM = ROOT / ("ref/" + _CAMPAIGN_SOURCE["directory"] + '/blink/debug.c')
PIN = _CAMPAIGN_INPUTS['PIN']
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output', required=True, type=Path)
parser.add_argument('--receipt', required=True, type=Path)
args = parser.parse_args()
original = UPSTREAM.read_bytes()
if hashlib.sha256(original).hexdigest() != PIN:
    raise SystemExit('immutable upstream debug.c checksum mismatch')
source = original.decode()
start = source.index('i64 ReadWordSafely(int mode, u8 *p) {')
end = source.index('// TODO(jart): This function should be immune', start)
before = source[start:end]
after = '''i64 ReadWordSafely(int mode, u8 *p) {
  /* Host diagnostics can inspect only the current worker's owned mappings.
   * Guest translation and the existing ReadWord implementation are unchanged. */
  if (mode < XED_MODE_REAL || mode > XED_MODE_LONG) return FAKE_WORD;
  if (!BlinkHostMemoryContains(p, (size_t)2 << mode)) {
    return FAKE_WORD >> ((8 - (2 << mode)) * 8);
  }
  return ReadWord(mode, p);
}

'''
source = source[:start] + after + source[end:]
include = '#include <errno.h>\n'
if source.count(include) != 1:
    raise SystemExit('debug include boundary changed')
source = source.replace(include, '#include "HostMemory.h"\n' + include)
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(source)
args.receipt.parent.mkdir(parents=True, exist_ok=True)
args.receipt.write_text(json.dumps(dict(kind='owned-host-diagnostic-read-only',
    originalSha256=PIN, stagedSha256=hashlib.sha256(source.encode()).hexdigest(),
    function='ReadWordSafely', beforeSha256=hashlib.sha256(before.encode()).hexdigest(),
    replacement=after, addedInclude='HostMemory.h'), indent=2)+'\n')
