"""Decode measured CPUID rows; report evidence without equating a bit with proof."""
FEATURES = [
 ('fpu','cpuid-features','dx',0),('tsc','cpuid-features','dx',4),('pae','cpuid-features','dx',6),
 ('cmpxchg8b','cpuid-features','dx',8),('cmov','cpuid-features','dx',15),('clflush','cpuid-features','dx',19),
 ('mmx','cpuid-features','dx',23),('fxsr','cpuid-features','dx',24),('sse','cpuid-features','dx',25),('sse2','cpuid-features','dx',26),
 ('sse3','cpuid-features','cx',0),('pclmulqdq','cpuid-features','cx',1),('ssse3','cpuid-features','cx',9),
 ('cmpxchg16b','cpuid-features','cx',13),('sse4.1','cpuid-features','cx',19),('sse4.2','cpuid-features','cx',20),
 ('popcnt','cpuid-features','cx',23),('aes','cpuid-features','cx',25),('xsave','cpuid-features','cx',26),
 ('avx','cpuid-features','cx',28),('rdrand','cpuid-features','cx',30),('hypervisor','cpuid-features','cx',31),
 ('fsgsbase','cpuid-structured','bx',0),('avx2','cpuid-structured','bx',5),('bmi2','cpuid-structured','bx',8),
 ('erms','cpuid-structured','bx',9),('rdseed','cpuid-structured','bx',18),('adx','cpuid-structured','bx',19),('rdpid','cpuid-structured','cx',22),
 ('lahf-sahf','cpuid-extended-features','cx',0),('blink-jit','cpuid-extended-features','cx',31),
 ('extended-fpu','cpuid-extended-features','dx',0),('extended-cmpxchg8b','cpuid-extended-features','dx',8),
 ('syscall','cpuid-extended-features','dx',11),('extended-cmov','cpuid-extended-features','dx',15),
 ('nx','cpuid-extended-features','dx',20),('extended-mmx','cpuid-extended-features','dx',23),
 ('extended-fxsr','cpuid-extended-features','dx',24),('rdtscp','cpuid-extended-features','dx',27),
 ('long-mode','cpuid-extended-features','dx',29),('invariant-tsc','cpuid-invariant-tsc','dx',8),
]
EXCLUDED={'fpu','extended-fpu','mmx','extended-mmx','blink-jit','bmi2','adx','sse4.1','sse4.2','aes','avx','avx2','xsave'}
POLICY_EXCLUDED={'sse3','ssse3','pclmulqdq','popcnt','cmpxchg16b','fsgsbase','erms','rdrand','rdseed','rdpid','lahf-sahf','rdtscp','invariant-tsc'}
def inventory(rows, hardware):
    for case in ['cpuid-thermal-power','cpuid-structured-unknown','cpuid-unknown','cpuid-extended-unknown']:
        if any(int(rows[case][reg],16) for reg in ['ax','bx','cx','dx']):
            raise RuntimeError('zero-output policy violated: '+case)
    out=[]
    for name,case,register,bit in FEATURES:
        advertised=bool(int(rows[case][register],16)&(1<<bit))
        if name in (EXCLUDED | POLICY_EXCLUDED) and advertised:raise RuntimeError('excluded feature advertised: '+name)
        evidence=('representative cases only; scalar correction qualification is receipt-specific' if name in {'sse','sse2'} else
                  'one CMOV case; not full condition/width coverage' if name in {'cmov','extended-cmov'} else
                  'selected long-mode mapping corpus only' if name in {'pae','long-mode'} else
                  'emulation identity, not instruction correctness' if name=='hypervisor' else
                  'no instruction-family qualification in this corpus')
        out.append(dict(feature=name,case=case,register=register,bit=bit,advertised=advertised,
                        hardwareAdvertised=bool(int(hardware[case][register],16)&(1<<bit)),evidence=evidence,
                        candidateForAdvertisementReduction=advertised and name in POLICY_EXCLUDED))
    return out
