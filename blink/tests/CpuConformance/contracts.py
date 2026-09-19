"""Narrow invariants for normal nondeterministic and normalized-pointer rows."""
RDTSC = 'rdtsc-defined-state'
REP_OFFSETS = {
    'rep-movsb-forward-offsets': (16, 48),
    'rep-movsb-backward-offsets': (0xffffffffffffffff, 31),
}


def compared_fields(case, fields):
    # Preserve raw timestamp halves in every stdout capture. Their values have
    # no cross-process equality contract; all other existing fields still do.
    return [name for name in fields if case['name'] != RDTSC or name not in {'ax', 'dx'}]


def invariants(case, state, corpus):
    differences = []
    if case['name'] == RDTSC:
        for name in ['ax', 'dx']:
            if int(state[name], 16) >> 32:
                differences.append('rdtsc.' + name + '.zeroextension')
        expected = {'cx': case['cx'], 'xmm': case['xmmInput'],
                    'mxcsr': case['mxcsr'], 'signal': 0, 'ip': len(case['code']) // 2,
                    'memory': corpus['dataInput'][:case['dataPages'] * corpus['pageSize'] * 2]}
        for name, value in expected.items():
            if state[name] != value:
                differences.append('rdtsc.unchanged.' + name)
        if (int(state['flags'], 16) ^ int(case['flags'], 16)) & int(case['flagMask'], 16):
            differences.append('rdtsc.unchanged.definedFlags')
        # BX preservation is checked against each process's supplied data
        # pointer by hardware.c/interpreter.c, without comparing host addresses.
    if case['name'] in REP_OFFSETS:
        source, destination = REP_OFFSETS[case['name']]
        for name, expected in [('ax', source), ('dx', destination), ('cx', 0)]:
            if int(state[name], 16) != expected:
                differences.append('rep.normalized.' + name)
        if int(state['flags'], 16) & 0x400:
            differences.append('rep.directionFlagNotClear')
    return differences
