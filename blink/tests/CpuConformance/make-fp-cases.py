#!/usr/bin/env python3
"""Serialize reviewed scalar FP inputs only; no instruction output oracle."""
import struct
from pathlib import Path

rows = []
def bits(value, single):
    return int.from_bytes(struct.pack('<f' if single else '<d', value), 'little')
def add(name, code, x, y=0, single=False, mxcsr=0x1f80, fault=0, memory=False):
    # Explicit expected halt describes the architectural trap class, not ALU output.
    halt = -9 if fault == 8 else -4 if fault == 11 else 0
    data_offset, pages = (4094 if single else 4092, 1) if memory else (64, 2)
    rows.append(' {"%s", {%s},%d,1,0,%d,%d,0xa5a5a5a512345678ULL,0x11223344,0x55667788,0x8d7,0x8d5,%d,0,0x%x,0,1,%d,1,%d,0x%xULL,0x%xULL},\n' %
                (name, ','.join(hex(b) for b in code), len(code), data_offset, pages,
                 fault, mxcsr, halt, 4 if single else 8, x, y))
def cvt(name, value, single=False, wide=True, truncate=False, mxcsr=0x1f80, fault=0, memory=False):
    code = [0xf3 if single else 0xf2] + ([0x48] if wide else []) + [0x0f, 0x2c if truncate else 0x2d, 0x03 if memory else 0xc0]
    add(name, code, value, single=single, mxcsr=mxcsr, fault=fault, memory=memory)
def cmp(name, x, y, single=False, unordered=True, mxcsr=0x1f80, fault=0, memory=False):
    add(name, ([] if single else [0x66])+[0x0f,0x2e if unordered else 0x2f,0x03 if memory else 0xc1], x,y,single,mxcsr,fault,memory)

for single in [False,True]:
    label = 'ss' if single else 'sd'
    for wide in [False,True]:
        width = '64' if wide else '32'
        for truncate in [False,True]:
            op = 'cvtt' if truncate else 'cvt'
            for rc in range(4):
                for v in [2.5,3.5,-2.5,-3.5]:
                    cvt(f'{op}{label}-{width}-rc{rc}-{v}', bits(v,single),single,wide,truncate,0x1f80|(rc<<13))
            values = [0.0,-0.0,1.0,-1.0]
            for j,v in enumerate(values):cvt(f'{op}{label}-{width}-exact{j}',bits(v,single),single,wide,truncate)
            if single:
                boundaries = [0x5effffff,0x5f000000,0xdf000000,0xdf000001] if wide else [0x4effffff,0x4f000000,0xcf000000,0xcf000001]
                specials = [0x7f800000,0xff800000,0x7fc00123,0xffc00123,0x7f800123,0xff800123]
            else:
                boundaries = [0x43dfffffffffffff,0x43e0000000000000,0xc3e0000000000000,0xc3e0000000000001] if wide else [bits(2147483647.0,False),bits(2147483648.0,False),bits(-2147483648.0,False),bits(-2147483649.0,False)]
                specials = [0x7ff0000000000000,0xfff0000000000000,0x7ff8000000000123,0xfff8000000000123,0x7ff0000000000123,0xfff0000000000123]
            for j,v in enumerate(boundaries+specials):cvt(f'{op}{label}-{width}-limit-special{j}',v,single,wide,truncate)
            cvt(f'{op}{label}-{width}-unmasked-invalid',specials[2],single,wide,truncate,0x1f00,8)
            cvt(f'{op}{label}-{width}-unmasked-precision',bits(2.5,single),single,wide,truncate,0x0f80,8)
    for rc in range(4):
        half=bits(0.5,single)
        nearby=[half] if single else [half-1,half,half+1]
        for j,v in enumerate(nearby):
            for negative in [False,True]:
                cvt(f'cvt{label}-half-rc{rc}-{j}-{int(negative)}',v|((1<<(31 if single else 63))if negative else 0),single,mxcsr=0x1f80|(rc<<13))
        if not single:
            for exponent in [-11,-12]:
                for negative in [False,True]:
                    cvt(f'cvtsd-shift-edge-rc{rc}-{exponent}-{int(negative)}',bits((-1 if negative else 1)*2.0**exponent,False),mxcsr=0x1f80|(rc<<13))
    for j,mx in enumerate([0,0x1f81,0x1fa0,0x1fbf]):
        cvt(f'cvt{label}-exact-sticky{j}',bits(2.0,single),single,mxcsr=mx)
    for j,mx in enumerate([0,1,4,8,0x10,2]):
        cvt(f'cvt{label}-precision-priority{j}',bits(2.5,single),single,mxcsr=mx,fault=8)
    cvt(f'cvt{label}-invalid-masked-precision-unmasked',specials[2],single,mxcsr=0x0f80)
    if not single:
        for rc in range(4):
            for j,v in enumerate([2147483647.5,-2147483648.5]):
                cvt(f'cvtsd-rounded32-boundary-rc{rc}-{j}',bits(v,False),False,False,mxcsr=0x1f80|(rc<<13))
    # Scalar conversions do not signal denormal; DM is intentionally cleared.
    for negative in [False,True]:
        for daz in [False,True]:
            for ftz in [False,True]:
                v=1|((1<<(31 if single else 63)) if negative else 0)
                mx=0x5e80|(0x40 if daz else 0)|(0x8000 if ftz else 0)
                cvt(f'cvt{label}-tiny-{int(negative)}-{int(daz)}-{int(ftz)}',v,single,mxcsr=mx)
    cvt(f'cvt{label}-memory-fault',specials[2],single,mxcsr=0,fault=11,memory=True)
    one=bits(1.0,single);qn=0x7fc00123 if single else 0x7ff8000000000123;sn=0x7f800123 if single else 0x7ff0000000000123
    for un in [False,True]:
        label2=('ucomi' if un else 'comi')+label[-1]
        for j,(x,y) in enumerate([(one,one),(bits(-0.0,single),0),(bits(-1.0,single),one),(qn,one),(sn,one),(qn,1),(sn,1)]):
            cmp(f'{label2}-masked{j}',x,y,single,un)
        for j,(x,y) in enumerate([(bits(-2.0,single),bits(-1.0,single)),(bits(float('-inf'),single),bits(float('inf'),single)),(1|(1<<(31 if single else 63)),bits(-0.0,single))]):
            cmp(f'{label2}-finite-order{j}',x,y,single,un)
        cmp(f'{label2}-negative-denormal-daz',1|(1<<(31 if single else 63)),0,single,un,0x1fc0)
        for order in [False,True]:
            for daz in [False,True]:
                for dm in [False,True]:
                    mx=0x1e80|(0x40 if daz else 0)|(0x100 if dm else 0)
                    cmp(f'{label2}-qnan-denormal-{int(order)}-{int(daz)}-{int(dm)}',1 if order else qn,qn if order else 1,single,un,mx)
        cmp(f'{label2}-qnan-denormal-mask-priority',qn,1,single,un,0x1e80,0)
        cmp(f'{label2}-snan-denormal-mask-priority',sn,1,single,un,0x1e80,0)
        cmp(f'{label2}-unmasked-snan',sn,one,single,un,0x1f00,8)
        cmp(f'{label2}-unmasked-qnan',qn,one,single,un,0x1f00,0 if un else 8)
        cmp(f'{label2}-unmasked-denormal',1,one,single,un,0x1e80,8)
        cmp(f'{label2}-daz',1,one,single,un,0x1ec0)
        cmp(f'{label2}-sticky',one,one,single,un,0x003f)
        cmp(f'{label2}-memory-fault',sn,one,single,un,0,11,True)

path=Path(__file__).with_name('fp-cases.h')
path.write_text('/* Generated by make-fp-cases.py: explicit inputs, no expected results. */\n'+''.join(rows))
print(f'{len(rows)} scalar FP inputs written to {path}')
