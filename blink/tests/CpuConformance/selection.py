"""Reviewed normal-only selection; original stable IDs and bytes are retained."""
import hashlib
import json

# The normal-return witness reserves R12..R15/RSP. Appended rows also use
# caller-saved RDI; FXSAVE/FXRSTOR restores the saved state and REP STOSQ runs
# with DF=0. New rows require this register/stack review, not only a new count.
REVIEWED_INPUTS = {
    'corpus.h': 'ff76a1e70d76331c482f7765b073e02b95af7277ae4d4f25cf3d04f6abde336c',
    'fp-cases.h': '0ecfbb3df62bb8b9838315eafa473455019e60e384023b118b5fe96587c79053',
}
# Exact first495 exported descriptors from the qualified normal-only baseline
# artifacts/cpu-conformance/attempt-7j03qmvz/corpus.stdout, canonical JSON.
ORIGINAL_CASES_SHA256 = 'bcdd790f0b537c50715135afdd8401850cdff1ffa58cd4f04e46484fc208767a'
# First512 descriptors are pinned from the preserved failing native attempt;
# later source repairs must not change those architectural observations/inputs.
FIRST512_SHA256 = 'b1476860cd0c7037f08b54f74cafe0b7d1a718b8393f2c7ab6187b2339d1dcd3'
APPENDED_NAMES = [
    'inc-qword-preserve-carry', 'inc-dword-preserve-clear-carry',
    'dec-byte-preserve-clear-carry', 'dec-word-preserve-carry',
    'shl-byte-count-zero', 'shl-byte-count-one', 'shr-word-count-one',
    'sar-dword-count31', 'shl-word-masked-count-zero', 'shl-dword-masked-count-one',
    'div-byte-valid', 'div-dword-valid', 'idiv-word-valid', 'idiv-dword-valid',
    'cmpxchg8b-match', 'cmpxchg8b-nonmatch', 'fxsave-fxrstor-xmm-mxcsr',
    'inc-byte-wrap-preserve-carry', 'inc-word-overflow-preserve-clear-carry',
]

def select_normal(corpus, source):
    for name, digest in REVIEWED_INPUTS.items():
        if hashlib.sha256((source / name).read_bytes()).hexdigest() != digest:
            raise RuntimeError('CPU input needs renewed trampoline review: ' + name)
    cases = corpus['cases']
    if [row['index'] for row in cases] != list(range(514)):
        raise RuntimeError('CPU stable index inventory differs')
    original_digest = hashlib.sha256(json.dumps(cases[:495], sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if original_digest != ORIGINAL_CASES_SHA256:
        raise RuntimeError('Original495 descriptors changed')
    first512_digest = hashlib.sha256(json.dumps(cases[:512], sort_keys=True, separators=(',', ':')).encode()).hexdigest()
    if first512_digest != FIRST512_SHA256:
        raise RuntimeError('First512 descriptors changed after the preserved failure')
    if [row['name'] for row in cases[495:]] != APPENDED_NAMES or any(row['fault'] for row in cases[495:]):
        raise RuntimeError('Appended normal case inventory differs')
    if len({row['name'] for row in cases}) != len(cases):
        raise RuntimeError('CPU names are not unique')
    selected = [row for row in cases if row['fault'] == 0]
    excluded = [dict(index=row['index'], name=row['name'], expected_signal=row['fault'],
                     reason='custom-fault-case-excluded-by-current-user-scope')
                for row in cases if row['fault'] != 0]
    if len(selected) != 468 or len(excluded) != 46:
        raise RuntimeError('Reviewed normal selection count differs')
    evidence = dict(schema=2, policy='normal-only', reviewed_inputs=REVIEWED_INPUTS,
                    original495_sha256=original_digest,
                    first512_sha256=first512_digest,
                    appended=[dict(index=row['index'], name=row['name']) for row in cases[495:]],
                    corpus_sha256=hashlib.sha256(json.dumps(corpus, sort_keys=True, separators=(',', ':')).encode()).hexdigest(),
                    total=514, selected=[dict(index=row['index'], name=row['name']) for row in selected],
                    excluded=excluded, hardware_completion='ordinary LEA/RET; no signal or trap sentinel')
    return selected, evidence
