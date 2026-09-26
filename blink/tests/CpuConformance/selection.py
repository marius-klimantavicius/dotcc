"""Reviewed normal-only selection; original stable IDs and bytes are retained."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['tests/CpuConformance/selection.py']

import hashlib
import json

# The normal-return witness reserves R12..R15/RSP. Appended rows also use
# caller-saved RDI; FXSAVE/FXRSTOR restores the saved state and REP STOSQ runs
# with DF=0. The exact balanced stack recipe saves/restores RSP through DI,
# then clears DI; no other row may change RSP and none changes R12..R15.
# REP MOVSB normalizes SI/DI into AX/DX. New rows require renewed review.
REVIEWED_INPUTS = _CAMPAIGN_INPUTS['REVIEWED_INPUTS']
# Exact first495 exported descriptors from the qualified normal-only baseline
# artifacts/cpu-conformance/attempt-7j03qmvz/corpus.stdout, canonical JSON.
ORIGINAL_CASES_SHA256 = _CAMPAIGN_INPUTS['ORIGINAL_CASES_SHA256']
# First512 descriptors are pinned from the preserved failing native attempt;
# later source repairs must not change those architectural observations/inputs.
FIRST512_SHA256 = _CAMPAIGN_INPUTS['FIRST512_SHA256']
FIRST514_SHA256 = _CAMPAIGN_INPUTS['FIRST514_SHA256']
FIRST546_SHA256 = _CAMPAIGN_INPUTS['FIRST546_SHA256']
FIRST550_SHA256 = _CAMPAIGN_INPUTS['FIRST550_SHA256']
APPENDED_NAMES = [
    'inc-qword-preserve-carry', 'inc-dword-preserve-clear-carry',
    'dec-byte-preserve-clear-carry', 'dec-word-preserve-carry',
    'shl-byte-count-zero', 'shl-byte-count-one', 'shr-word-count-one',
    'sar-dword-count31', 'shl-word-masked-count-zero', 'shl-dword-masked-count-one',
    'div-byte-valid', 'div-dword-valid', 'idiv-word-valid', 'idiv-dword-valid',
    'cmpxchg8b-match', 'cmpxchg8b-nonmatch', 'fxsave-fxrstor-xmm-mxcsr',
    'inc-byte-wrap-preserve-carry', 'inc-word-overflow-preserve-clear-carry',
    'adc-word-overflow',
    'sbb-qword-overflow',
    'and-dword-zeroextend',
    'xor-word-upper-preserve',
    'neg-byte-minimum',
    'rol-byte-one',
    'ror-qword-one',
    'shld-dword-one',
    'sse2-paddsb-saturating',
    'sse2-psubusw-saturating',
    'sse2-pcmpeqd',
    'sse2-punpcklbw',
    'sse2-pshufd-reverse',
    'sse2-psllw-seven',
    'sse2-movdqu-unaligned-roundtrip',
    'sse2-movdqa-aligned-roundtrip',
    'address-sib-scale-disp8',
    'address-negative-disp32',
    'address-rip-literal',
    'movzx-byte-to-dword',
    'movsx-word-to-qword',
    'store-qword-valid-page-cross',
    'rep-movsb-forward-offsets',
    'rep-movsb-backward-offsets',
    'cmovne-dword-taken',
    'cmovne-dword-not-taken',
    'cmovl-qword-taken',
    'cmovl-qword-not-taken',
    'cmovb-word-taken',
    'clflush-valid-mapped',
    'rdtsc-defined-state',
    'stack-balanced-data-push-pop',
    'neg-word-minimum',
    'neg-dword-minimum',
    'neg-qword-minimum',
    'neg-byte-low-nibble-nonzero',
    'cmpss-true-mask-upper-preserve',
    'cmpss-false-mask-upper-preserve',
    'cmpsd-true-mask-upper-preserve',
    'cmpsd-false-mask-upper-preserve',
    'cmpps-mixed-mask-lanes',
    'cmpps-false-mask-lanes',
    'cmppd-mixed-mask-lanes',
    'cmppd-false-mask-lanes',
    'hashtable-ordered-mask-truncate-2.16',
    'hashtable-ordered-mask-truncate-5.04',
]

def select_normal(corpus, source):
    for name, digest in REVIEWED_INPUTS.items():
        if hashlib.sha256((source / name).read_bytes()).hexdigest() != digest:
            raise RuntimeError('CPU input needs renewed trampoline review: ' + name)
    cases = corpus['cases']
    if [row['index'] for row in cases] != list(range(560)):
        raise RuntimeError('CPU stable index inventory differs')
    original_digest = hashlib.sha256(json.dumps(cases[:495], sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if original_digest != ORIGINAL_CASES_SHA256:
        raise RuntimeError('Original495 descriptors changed')
    first512_digest = hashlib.sha256(json.dumps(cases[:512], sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if first512_digest != FIRST512_SHA256:
        raise RuntimeError('First512 descriptors changed after the preserved failure')
    first514_digest = hashlib.sha256(json.dumps(cases[:514], sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if first514_digest != FIRST514_SHA256:
        raise RuntimeError('First514 qualified descriptors changed')
    first546_digest = hashlib.sha256(json.dumps(cases[:546], sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if first546_digest != FIRST546_SHA256:
        raise RuntimeError('First546 descriptors changed after preserved native failure')
    first550_digest = hashlib.sha256(json.dumps(cases[:550], sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if first550_digest != FIRST550_SHA256:
        raise RuntimeError('First550 qualified descriptors changed')
    if [row['name'] for row in cases[495:]] != APPENDED_NAMES or any(row['fault'] for row in cases[495:]):
        raise RuntimeError('Appended normal case inventory differs')
    if len({row['name'] for row in cases}) != len(cases):
        raise RuntimeError('CPU names are not unique')
    selected = [row for row in cases if row['fault'] == 0]
    excluded = [dict(index=row['index'], name=row['name'], expected_signal=row['fault'],
                     reason='custom-fault-case-excluded-by-current-user-scope')
                for row in cases if row['fault'] != 0]
    if len(selected) != 514 or len(excluded) != 46:
        raise RuntimeError('Reviewed normal selection count differs')
    evidence = dict(schema=2, policy='normal-only', reviewed_inputs=REVIEWED_INPUTS,
                    original495_sha256=original_digest,
                    first512_sha256=first512_digest,
                    first514_sha256=first514_digest,
                    first546_sha256=first546_digest,
                    first550_sha256=first550_digest,
                    appended=[dict(index=row['index'], name=row['name']) for row in cases[495:]],
                    corpus_sha256=hashlib.sha256(json.dumps(corpus, sort_keys=True, separators=(',', ':')).encode()).hexdigest(),
                    total=560, selected=[dict(index=row['index'], name=row['name']) for row in selected],
                    excluded=excluded, hardware_completion='ordinary LEA/RET; no signal or trap sentinel')
    return selected, evidence
