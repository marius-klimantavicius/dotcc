using System;

namespace Managed.Emulation.Execution;

// R165 PARTIAL integer-native replacement, common to the eight-op family.
// No floating-point/SIMD/MMX instruction, stack adjustment, CALL or host image.
// Uncovered cells rejoin the retained UNQUALIFIED host path; no new admission.
internal static partial class FullExtended80TranscendentalEmitter
{
    private ref partial struct Writer
    {
        private void IntegerNative(FullExtended80Transcendental op,int exit)
        {
            int uncovered=L(),ready=L();
            C(9,0); // selected PE/C1; no pre/range event in these finite domains
            if(op==FullExtended80Transcendental.Fyl2x)
            {
                // x=2^k, canonical normal positive; y=zero or canonical normal.
                // Nonzero k*y is admitted only for -16000<=ilogb(y)<=16000.
                // The conservative exponent bound excludes ALL OE/UE/wrap cells.
                Load(0,160,32);Alu(7,0,1,false);J(uncovered,5);
                Load(0,152,32);Test(0,false);J(uncovered,5);
                Load(0,128);Imm(1,0x8000000000000000);RR(0x39,0,1);J(uncovered,5);
                Load(0,164,32);Test(0,false);int zeroY=L();J(zeroY,4);
                Alu(7,0,1,false);J(uncovered,5);
                Load(0,144,32);Test(0,false);int zeroLog=L();J(zeroLog,4);
                Load(0,148,32);Alu(7,0,-16000,false);J(uncovered,12);Alu(7,0,16000,false);J(uncovered,15);
                IntegerMultiplyLog();J(ready);
                Bind(zeroY);Bind(zeroLog);
                Load(0,144,32);Shift(7,0,31,false);Alu(4,0,0x8000,false);
                Load(1,40,16);Alu(4,1,0x8000,false);RR(0x31,0,1,false);Store(0,56,16);J(ready);
            }
            else if(op==FullExtended80Transcendental.Fyl2xp1)
            {
                Load(0,160,32);Test(0,false);J(uncovered,5);
                Load(0,164,32);Alu(7,0,1,false);J(uncovered,7); // only zero or normal y
                Load(0,24,16);Load(1,40,16);RR(0x31,0,1,false);Alu(4,0,0x8000,false);Store(0,56,16);J(ready);
            }
            else if(op==FullExtended80Transcendental.Fpatan)
            {
                Load(0,164,32);Test(0,false);J(uncovered,5); // y=+/-0
                Load(0,160,32);Alu(7,0,1,false);J(uncovered,7); // x=+0 or positive normal
                Load(0,152,32);Test(0,false);J(uncovered,5);
                Load(0,40,16);Store(0,56,16);J(ready);
            }
            else
            {
                Load(0,160,32);Test(0,false);int zero=L();J(zero,4);
                if(op==FullExtended80Transcendental.F2xm1)
                {
                    Load(0,160,32);Alu(7,0,1,false);J(uncovered,5);
                    Load(0,144,32);Test(0,false);J(uncovered,5);
                    Load(0,128);Imm(1,0x8000000000000000);RR(0x39,0,1);J(uncovered,5);
                    Store(0,48);Load(0,152,32);Test(0,false);int minus=L();J(minus,5);
                    Put(56,0x3fff);J(ready);Bind(minus);Put(56,0xbffe);J(ready);
                }
                else J(uncovered);
                Bind(zero);
                if(op==FullExtended80Transcendental.Fcos)
                {Imm(0,0x8000000000000000);Store(0,48);Put(56,0x3fff);}
                else {Load(0,16);Store(0,48);Load(0,24,16);Store(0,56,16);}
                if(Pair(op)){Imm(0,0x8000000000000000);Store(0,64);Put(72,0x3fff);}
            }
            Bind(ready);IntegerFinish(op,exit);Bind(uncovered);
        }

        // Exact unsigned significand * |k|, followed by a SINGLE PC64 rounding.
        // Input k is nonzero signed int, |k|<=16383, y normalized and bounded.
        // RDX:RAX product; R8 saves original low word; ECX shift in [1,14].
        private void IntegerMultiplyLog()
        {
            Load(0,144,32);Mov(8,0,false);Shift(7,8,31,false);
            RR(0x31,0,8,false);RR(0x29,0,8,false);Mov(8,0,false);
            Load(0,144,32);Shift(7,0,31,false);Alu(4,0,0x8000,false);
            Load(1,40,16);Alu(4,1,0x8000,false);RR(0x31,0,1,false);Store(0,224,32);
            Load(0,40,16);Alu(4,0,0x7fff,false);Store(0,228,32);
            Load(0,136);B(0x49);B(0xf7);B(0xe0); // mul r8 -> RDX:RAX
            Mov(8,0);Test(2);int noShift=L(),rounded=L();J(noShift,4);
            Bsr(1,2);Alu(0,1,1,false); // bitlength(high) = discarded count
            Load(10,228,32);RR(0x01,10,1,false);Store(10,228,32);
            DoubleShift(true,0,2); // quotient=(high:low)>>CL
            // remainder=originalLow & ((1<<shift)-1). Keep it exact; no jam loss.
            C(2,1);Shift(4,2);Alu(5,2,1);RR(0x21,8,2);
            Test(8);J(rounded,4);C(9,0x20);
            Load(2,8,16);Shift(5,2,10,false);Alu(4,2,3,false);
            Alu(7,2,0,false);int nearest=L(),increment=L(),truncate=L();J(nearest,4);
            Alu(7,2,3,false);J(truncate,4);
            // directed increment iff RC==up and sign+, or RC==down and sign-.
            Load(10,224,32);Shift(5,10,15,false);RR(0x01,2,10,false);Alu(7,2,2,false);J(increment,4);J(truncate);
            Bind(nearest);
            C(2,1);Alu(5,1,1,false);Shift(4,2); // exact halfway unit
            RR(0x39,8,2);J(increment,7);J(truncate,2);
            Mov(2,0);Alu(4,2,1);J(truncate,4);
            Bind(increment);Alu(0,0,1);C(9,0x220);J(truncate,5);
            // Carry out of all-one retained significand renormalizes once.
            Imm(0,0x8000000000000000);Load(2,228,32);Alu(0,2,1,false);Store(2,228,32);
            Bind(truncate);Bind(rounded);Bind(noShift);
            Store(0,48);Load(0,228,32);Load(1,224,32);RR(0x09,0,1,false);Store(0,56,16);
        }

        private void IntegerFinish(FullExtended80Transcendental op,int exit)
        {
            Store(9,80,16);Store(9,82,16);
            Mov(0,9,false);Alu(4,0,0x200,false);Store(0,232,16);
            C(0,Trig(op)?0x600:0x200);Store(0,234,16);
            C(8,(int)op+1|0x10100|(Pair(op)?0x200:0));
            Mov(0,9,false);Alu(4,0,32,false);int complete=L();J(complete,4);
            Load(0,8,16);Alu(4,0,32,false);J(complete,5);Alu(1,8,0x1000,false);
            Bind(complete);Store(8,84,32);
            // No pointer/raw host image was ever acquired; remove all private
            // classification/arithmetic scratch before either RET/tail exit.
            C(0,0);for(int p=96;p<232;p+=8)Store(0,p);for(int p=236;p<256;p+=4)Store(0,p,32);
            J(exit);
        }
    }
}
