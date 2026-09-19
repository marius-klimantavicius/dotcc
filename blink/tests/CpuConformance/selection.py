"""Reviewed normal-only selection; original stable IDs and bytes are retained."""
import hashlib
import json

# The normal-return witness reserves R12..R15/RSP. New instruction rows require
# reviewing that register/stack contract, not merely increasing a count.
REVIEWED_INPUTS = {
    'corpus.h': '8d6df7093c444d07d8d59bd200d2405216fa20b1de25b47936e6bb0ffa83d969',
    'fp-cases.h': '0ecfbb3df62bb8b9838315eafa473455019e60e384023b118b5fe96587c79053',
}

def select_normal(corpus, source):
    for name, digest in REVIEWED_INPUTS.items():
        if hashlib.sha256((source / name).read_bytes()).hexdigest() != digest:
            raise RuntimeError('CPU input needs renewed trampoline review: ' + name)
    cases = corpus['cases']
    if [row['index'] for row in cases] != list(range(495)):
        raise RuntimeError('CPU stable index inventory differs')
    if len({row['name'] for row in cases}) != len(cases):
        raise RuntimeError('CPU names are not unique')
    selected = [row for row in cases if row['fault'] == 0]
    excluded = [dict(index=row['index'], name=row['name'], expected_signal=row['fault'],
                     reason='custom-fault-case-excluded-by-current-user-scope')
                for row in cases if row['fault'] != 0]
    if len(selected) != 449 or len(excluded) != 46:
        raise RuntimeError('Reviewed normal selection count differs')
    evidence = dict(schema=1, policy='normal-only', reviewed_inputs=REVIEWED_INPUTS,
                    corpus_sha256=hashlib.sha256(json.dumps(corpus, sort_keys=True, separators=(',', ':')).encode()).hexdigest(),
                    total=495, selected=[dict(index=row['index'], name=row['name']) for row in selected],
                    excluded=excluded, hardware_completion='ordinary LEA/RET; no signal or trap sentinel')
    return selected, evidence
