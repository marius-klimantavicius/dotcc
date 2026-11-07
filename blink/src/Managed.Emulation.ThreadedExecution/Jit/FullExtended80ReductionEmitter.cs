using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R154 integer-native scale/remainder foundation. Bounded writer and exact
// decode/NaN/round-pack mechanics derived from the existing R126 foundation;
// no QEMU code or binary64/host FP computation. New semantics AUTHORED_UNRUN.
internal static class FullExtended80ReductionEmitter
{
    internal const int CodeCapacity = 32768;
    internal static bool TryEmit(FullExtended80Reduction op,Span<byte> destination,out int length,bool? windowsAbi = null)
        => Emit(op,destination,out length,windowsAbi ?? OperatingSystem.IsWindows(),false);
    internal static bool TryEmitContinuation(FullExtended80Reduction op,Span<byte> destination,out int length)
        => Emit(op,destination,out length,false,true);
    private static bool Emit(FullExtended80Reduction op,Span<byte> destination,out int length,bool windows,bool continuation)
    {
        length = 0; if (op is not (FullExtended80Reduction.Scale or FullExtended80Reduction.Remainder or FullExtended80Reduction.RemainderNearest)) return false;
        Span<int> labels = stackalloc int[512],heads = stackalloc int[512]; labels.Fill(-1); heads.Fill(-1);
        var w = new Writer(destination[..Math.Min(destination.Length,CodeCapacity)],labels,heads);
        w.Reduction(op,windows,continuation); if (!w.Complete) return false; length = w.Length; return true;
    }
    private ref struct Writer
    {
        private Span<byte> code;
        private Span<int> labels, heads;
        private int next, n;
        private bool failed;
        internal int Length => n;
        internal bool Complete
        {
            get
            {
                if (failed) return false;
                for (int i = 0; i < next; ++i) if (heads[i] >= 0) return false;
                return true;
            }
        }
        internal Writer(Span<byte> bytes, Span<int> positions, Span<int> fixups)
        { code = bytes; labels = positions; heads = fixups; next = n = 0; failed = false; }
        private void B(int b)
        { if (n < code.Length) code[n++] = (byte)b; else failed = true; }
        private void D(int value)
        {
            if (code.Length - n < 4) { failed = true; return; }
            BinaryPrimitives.WriteInt32LittleEndian(code[n..], value); n += 4;
        }
        private void Q(ulong value) { D(unchecked((int)value)); D(unchecked((int)(value >> 32))); }
        private int L()
        { if (next >= labels.Length) { failed = true; return 0; } return next++; }
        private void Bind(int label)
        {
            if (failed) return;
            if (labels[label] >= 0) { failed = true; return; }
            labels[label] = n;
            int p = heads[label]; heads[label] = -1;
            while (p >= 0)
            {
                int previous = BinaryPrimitives.ReadInt32LittleEndian(code[p..]);
                BinaryPrimitives.WriteInt32LittleEndian(code[p..], n - p - 4); p = previous;
            }
        }
        // cc uses the low nibble of 0F 8x; -1 is unconditional E9.
        private void J(int label, int cc = -1)
        {
            if (failed) return;
            if (cc < 0) B(0xe9); else { B(0x0f); B(0x80 | cc); }
            int p = n; D(labels[label] >= 0 ? labels[label] - p - 4 : heads[label]);
            if (!failed && labels[label] < 0) heads[label] = p;
        }
        private void Rex(bool wide, int reg = 0, int rm = 0)
        { int r = 0x40 | (wide ? 8 : 0) | (reg >= 8 ? 4 : 0) | (rm >= 8 ? 1 : 0); if (r != 0x40) B(r); }
        private void RR(int opcode, int dst, int src, bool wide = true)
        { Rex(wide, src, dst); B(opcode); B(0xc0 | ((src & 7) << 3) | (dst & 7)); }
        private void Mov(int dst, int src, bool wide = true) => RR(0x89, dst, src, wide);
        private void Imm(int reg, ulong value, bool wide = true)
        { Rex(wide, 0, reg); B(0xb8 | (reg & 7)); if (wide) Q(value); else D(unchecked((int)value)); }
        private void C(int reg, int value) => Imm(reg, unchecked((uint)value), false);
        private void Mem(int reg, int offset)
        { B(0x80 | ((reg & 7) << 3) | 3); D(offset); } // [r11+disp32]
        private void Load(int reg, int offset, int width = 64)
        {
            Rex(width == 64, reg, 11);
            if (width == 16) { B(0x0f); B(0xb7); } else B(0x8b);
            Mem(reg, offset);
        }
        private void Store(int reg, int offset, int width = 64)
        { if (width == 16) B(0x66); Rex(width == 64, reg, 11); B(0x89); Mem(reg, offset); }
        // Put uses C(0,...): it clobbers RAX; MOV stores preserve flags.
        private void Put(int offset, int value)
        { C(0, value); Store(0, offset, 32); }
        private void Alu(int group, int reg, int value, bool wide = true)
        { Rex(wide, 0, reg); B(0x81); B(0xc0 | (group << 3) | (reg & 7)); D(value); }
        private void Bits(int offset, int group, int value)
        { Rex(false, 0, 11); B(0x81); Mem(group, offset); D(value); }
        private void Test(int reg, bool wide = true) => RR(0x85, reg, reg, wide);
        private void Shift(int group, int reg, int count = -1, bool wide = true)
        { Rex(wide, 0, reg); B(count < 0 ? 0xd3 : 0xc1); B(0xc0 | (group << 3) | (reg & 7)); if (count >= 0) B(count); }
        private void DoubleShift(bool right, int dst, int src)
        { Rex(true, src, dst); B(0x0f); B(right ? 0xad : 0xa5); B(0xc0 | ((src & 7) << 3) | (dst & 7)); }
        private void Bsr(int dst, int src)
        { Rex(true, dst, src); B(0x0f); B(0xbd); B(0xc0 | ((dst & 7) << 3) | (src & 7)); }
        private void Set(int reg, int cc)
        { Rex(false, 0, reg); B(0x0f); B(0x90 | cc); B(0xc0 | (reg & 7)); }
        private void Exit(bool continuation)
        {
            Load(0, 12, 32);
            if (continuation) { B(0x41); B(0xff); B(0xa3); D(88); } else B(0xc3);
        }

        internal void Reduction(FullExtended80Reduction op,bool windows,bool continuation)
        {
            int version=L(),size=L(),invalid=L(),nan=L(),finish=L(),exit=L();
            B(0xf3);B(0x0f);B(0x1e);B(0xfa); if(!continuation)Mov(11,windows?1:7);
            C(0,0);Store(0,12,32);for(int p=48;p<88;p+=8)Store(0,p);
            Load(0,0,32);Alu(7,0,1,false);J(version,5);Load(0,4,32);Alu(7,0,384,false);J(size,5);
            Put(104,0);Put(108,0);Put(240,0x02000000);Put(244,op==FullExtended80Reduction.Scale?1:op==FullExtended80Reduction.Remainder?2:4);
            Decode(16,128,144,152,160,168);Decode(32,136,148,156,164,172);
            Load(0,160,32);Alu(7,0,5,false);J(invalid,4);Load(0,164,32);Alu(7,0,5,false);J(invalid,4);
            Load(0,160,32);Alu(7,0,3,false);J(nan,4);Alu(7,0,4,false);J(nan,4);
            Load(0,164,32);Alu(7,0,3,false);J(nan,4);Alu(7,0,4,false);J(nan,4);
            if(op==FullExtended80Reduction.Scale)
            {
                // Zero * 2^(+infinity), infinity * 2^(-infinity) are IE,
                // before a possible denormal in the other operand.
                int ordinary=L(),negative=L();Load(0,164,32);Alu(7,0,2,false);J(ordinary,5);
                Load(0,156,32);Test(0,false);J(negative,5);
                Load(0,160,32);Test(0,false);J(invalid,4);J(ordinary);
                Bind(negative);Load(0,160,32);Alu(7,0,2,false);J(invalid,4);Bind(ordinary);
            }
            else
            {
                Load(0,160,32);Alu(7,0,2,false);J(invalid,4);
                Load(0,164,32);Test(0,false);J(invalid,4);
            }
            int aNormal=L(),bNormal=L();
            Load(0,160,32);Alu(7,0,6,false);J(aNormal,5);Put(108,2);Put(160,1);Bind(aNormal);
            Load(0,164,32);Alu(7,0,6,false);J(bNormal,5);Put(108,2);Put(164,1);Bind(bNormal);
            if(op==FullExtended80Reduction.Scale)Scale(finish);
            else Remainder(op==FullExtended80Reduction.RemainderNearest,finish);
            J(finish);
            Bind(invalid);Imm(9,0xc000000000000000);C(2,0xffff);C(8,1);Special(9,2,8,1|32|64);J(finish);
            Bind(nan);NaN(finish);
            Bind(finish);
            // A pre-operation unmasked exception suppresses ordinary quotient
            // conditions, independently of candidate/normal arithmetic flags.
            Load(0,80,16);Load(1,8,16);Alu(6,1,-1,false);RR(0x21,0,1,false);Alu(4,0,3,false);
            int conditions=L();Test(0,false);J(conditions,4);Put(240,0x02000000);Bind(conditions);J(exit);
            Bind(version);Put(12,1);J(exit);Bind(size);Put(12,2);Bind(exit);Exit(continuation);
        }
        private void Scale(int done)
        {
            int zero=L(),infinity=L(),finite=L(),factorZero=L(),factorBig=L(),factorReady=L(),factorPositive=L();
            Load(0,160,32);Alu(7,0,2,false);J(infinity,4);
            Load(0,164,32);Alu(7,0,2,false);J(finite,5);
            Load(0,156,32);Test(0,false);J(zero,5);J(infinity);
            Bind(finite);Load(0,160,32);Test(0,false);J(zero,4);
            // Exact truncation of ST1; magnitudes >=2^16 all imply massive
            // overflow/underflow even after one +/-24576 range adjustment.
            Load(0,164,32);Test(0,false);J(factorZero,4);
            Load(0,148,32);Alu(7,0,0,false);J(factorZero,12);Alu(7,0,16,false);J(factorBig,13);
            C(1,63);RR(0x29,1,0,false);Load(8,136);Shift(5,8);J(factorReady);
            Bind(factorBig);C(8,65536);J(factorReady);Bind(factorZero);C(8,0);
            Bind(factorReady);Load(0,156,32);Test(0,false);J(factorPositive,4);
            Rex(false,0,8);B(0xf7);B(0xd8); // neg r8d
            Bind(factorPositive);CopyExact(128,144,152);Load(0,96,32);RR(0x01,0,8,false);Store(0,96,32);
            RoundAndPack(done);J(done);
            Bind(zero);Load(2,152,32);ZeroResult(done);
            Bind(infinity);Imm(9,0x8000000000000000);Load(2,152,32);Shift(4,2,15,false);Alu(1,2,0x7fff,false);
            Load(8,108,32);Special(9,2,8,1|16|128);J(done);
        }
        private void Remainder(bool nearest,int done)
        {
            int zero=L(),copy=L(),negativeDifference=L(),divide=L(),full=L(),partial=L(),normalize=L(),noIncrement=L();
            Load(0,160,32);Test(0,false);J(zero,4);
            Load(0,164,32);Alu(7,0,2,false);J(copy,4);
            Load(0,144,32);Load(1,148,32);RR(0x29,0,1,false);Store(0,204,32);
            Test(0,false);J(negativeDifference,8);
            Alu(7,0,64,false);J(full,12);
            // Explicit AMD partial policy, within Intel's allowed N32..63.
            Alu(4,0,31,false);Alu(0,0,32,false);Store(0,200,32);Put(240,0x47000400);
            // Put uses RAX. Reload N after writing condition bits: the exact
            // partial exponent is EA-N, never EA-(the CC literal in EAX).
            Load(0,200,32);Load(1,144,32);RR(0x29,1,0,false);Store(1,96,32);J(divide);
            Bind(full);Store(0,200,32);Load(1,148,32);Store(1,96,32);Put(240,0x47000000);
            Bind(divide);
            // A << N fits128 and its high64 < normalized divisor, so DIV
            // cannot #DE and its quotient fits64 for every N0..63.
            Load(0,128);Load(1,200,32);C(2,0);Test(1,false);int shiftDone=L();J(shiftDone,4);
            Mov(2,0);Shift(4,0);C(8,64);RR(0x29,8,1,false);Mov(1,8,false);Shift(5,2);
            Bind(shiftDone);Load(8,136);Rex(true,0,8);B(0xf7);B(0xf0); // div r8
            Store(0,208);Store(2,216);Load(0,152,32);Store(0,100,32);
            Load(0,204,32);Alu(7,0,64,false);J(partial,13);
            if(nearest)
            {
                // Compare r to divisor-r, avoiding an overflowing 2*r.
                RR(0x29,8,2);RR(0x39,2,8);J(noIncrement,2);int increment=L();J(increment,7);
                Load(0,208);Alu(4,0,1);Test(0);J(noIncrement,4);
                Bind(increment);Store(8,216);Load(0,208);Alu(0,0,1);Store(0,208);Bits(100,6,1);
                Bind(noIncrement);
            }
            QuotientConditions();J(normalize);
            Bind(partial);J(normalize);
            Bind(negativeDifference);
            if(nearest)
            {
                Alu(7,0,-1,false);J(copy,5);Load(8,128);Load(9,136);RR(0x39,8,9);J(copy,6);
                // With exponent difference -1, q rounds to1 iff sigA>sigB.
                // (2*B-A) modulo64 is the exact positive magnitude (<B).
                RR(0x01,9,9);RR(0x29,9,8);Store(9,216);Load(0,144,32);Store(0,96,32);
                Load(0,152,32);Alu(6,0,1,false);Store(0,100,32);Put(208,1);Put(212,0);
                QuotientConditions();J(normalize);
            }
            else J(copy);
            Bind(copy);Put(240,0x47000000);CopyExact(128,144,152);RoundAndPack(done);J(done);
            Bind(normalize);Load(9,216);Test(9);J(zero,4);
            Bsr(0,9);C(1,63);RR(0x29,1,0,false);Shift(4,9);Store(9,112);Put(120,0);Put(124,0);
            Load(0,96,32);RR(0x29,0,1,false);Store(0,96,32);RoundAndPack(done);J(done);
            Bind(zero);Load(0,160,32);Test(0,false);int haveConditions=L();J(haveConditions,5);
            Put(240,0x47000000);Bind(haveConditions);Load(2,152,32);ZeroResult(done);
        }
        private void QuotientConditions()
        {
            Load(0,208);Load(1,152,32);Load(2,156,32);RR(0x31,1,2,false);Test(1,false);
            int positive=L();J(positive,4);Rex(true,0,0);B(0xf7);B(0xd8);Bind(positive);
            Alu(4,0,7);Mov(8,0,false);Alu(4,8,4,false);Shift(4,8,6,false);
            Mov(1,0,false);Alu(4,1,2,false);Shift(4,1,13,false);RR(0x09,8,1,false);
            Alu(4,0,1,false);Shift(4,0,9,false);RR(0x09,8,0,false);Alu(1,8,0x47000000,false);Store(8,240,32);
        }

        private void Decode(int input, int sig, int exp, int sign, int cls, int rawSe)
        {
            int eZero = L(), special = L(), unsupported = L(), finish = L(), denormal = L();
            Load(9, input); Store(9, sig); Load(0, input + 8, 16); Store(0, rawSe, 32);
            Mov(2, 0, false); Shift(5, 2, 15, false); Store(2, sign, 32);
            Alu(4, 0, 0x7fff, false); Store(0, exp, 32);
            Test(0, false); J(eZero, 4); Alu(7, 0, 0x7fff, false); J(special, 4);
            Test(9); J(unsupported, 9); // JNS means explicit integer bit is zero.
            Alu(5, 0, 16383, false); Store(0, exp, 32); Put(cls, 1); J(finish);
            Bind(eZero); Test(9); J(denormal, 5); Put(cls, 0); Put(exp, -16382); J(finish);
            Bind(denormal);
            Bsr(1, 9); C(2, 63); RR(0x29, 2, 1, false); Mov(1, 2, false);
            Shift(4, 9); Store(9, sig); C(0, -16382); RR(0x29, 0, 2, false);
            Store(0, exp, 32); Put(cls, 6); J(finish);
            Bind(special); Test(9); J(unsupported, 9);
            Imm(0, 0x8000000000000000); RR(0x39, 9, 0); int notInf = L(); J(notInf, 5);
            Put(cls, 2); J(finish);
            Bind(notInf); Mov(0, 9); Shift(5, 0, 62); Alu(4, 0, 1, false);
            C(2, 4); RR(0x29, 2, 0, false); Store(2, cls, 32); J(finish);
            Bind(unsupported); Put(cls, 5); Bind(finish);
        }

        private void NaN(int done)
        {
            int chooseA = L(), chooseB = L(), selected = L(), equalClass = L();
            C(8, 0); Load(0, 160, 32); Alu(7, 0, 4, false); int aQuiet = L(); J(aQuiet, 5);
            C(8, 1); Bind(aQuiet); Load(0, 164, 32); Alu(7, 0, 4, false);
            int bQuiet = L(); J(bQuiet, 5); C(8, 1); Bind(bQuiet);
            Load(0, 160, 32); Alu(7, 0, 3, false); J(chooseA, 4);
            Load(0, 164, 32); Alu(7, 0, 3, false); J(chooseB, 4);
            // Both signaling => larger payload; sole signaling => itself.
            Load(0, 160, 32); Alu(7, 0, 4, false); J(chooseB, 5);
            Load(0, 164, 32); Alu(7, 0, 4, false); J(chooseA, 5); J(equalClass);
            Bind(chooseA);
            // If both quiet, compare rather than blindly prefer A.
            Load(0, 160, 32); Alu(7, 0, 3, false); int pickA = L(); J(pickA, 5);
            Load(0, 164, 32); Alu(7, 0, 3, false); J(equalClass, 4);
            Bind(pickA); Load(9, 128); Load(2, 168, 32); J(selected);
            Bind(equalClass);
            Load(9, 128); Load(10, 136); RR(0x39, 9, 10); J(pickA, 7); J(chooseB, 2);
            // Intel093 Table4-8 selects the larger same-class significand.
            // Equal significands deterministically select LEFT, including its
            // sign (not architecturally prescribed), before sNaN quieting.
            // Admission5 is retained only as a legacy enum value; never emit it.
            J(pickA);
            Bind(chooseB); Load(9, 136); Load(2, 172, 32);
            Bind(selected); Imm(0, 0x4000000000000000); RR(0x09, 9, 0);
            // PreOperationException applies only to sNaN; qNaN raises no IE.
            Store(9, 48); Store(2, 56, 16); Store(8, 80, 16);
            C(0, 1 | 64); Test(8, false); int q = L(); J(q, 4); Alu(1, 0, 32, false);
            Bind(q); Store(0, 84, 32); J(done);
        }

        private void Special(int sig, int se, int status, int classification)
        {
            Store(sig, 48); Store(se, 56, 16); Store(status, 80, 16);
            C(0, classification); Load(1, 108, 32); Test(1, false); int noDe = L(); J(noDe, 4);
            Alu(1, 0, 32, false); Bind(noDe); Store(0, 84, 32);
        }
        private void ZeroResult(int done)
        {
            Shift(4, 2, 15, false); C(9, 0); Load(8, 108, 32);
            Special(9, 2, 8, 1 | 16); J(done);
        }
        private void CopyExact(int sig, int exponent, int sign)
        {
            Load(9, sig); Store(9, 112); Put(120, 0); Put(124, 0);
            Load(0, exponent, 32); Store(0, 96, 32); Load(0, sign, 32); Store(0, 100, 32);
        }

        private void RightJam()
        {
            int finish = L(), wide = L(), huge = L(), exactly64 = L(), noTail = L();
            Test(1, false); J(finish, 4); Alu(7, 1, 64, false); J(wide, 3);
            // Lost low bits are extracted by left-shifting the old low limb.
            Mov(0, 10); C(2, 64); RR(0x29, 2, 1, false); Store(1, 208, 32);
            Mov(1, 2, false); Shift(4, 0); Load(1, 208, 32);
            DoubleShift(true, 10, 9); Shift(5, 9);
            Test(0); J(finish, 4); Alu(1, 10, 1); J(finish);
            Bind(wide); Alu(7, 1, 128, false); J(huge, 3);
            Alu(5, 1, 64, false); Test(1, false); J(exactly64, 4);
            Mov(0, 10); Mov(10, 9); Mov(8, 9); Shift(5, 10);
            C(2, 64); RR(0x29, 2, 1, false); Mov(1, 2, false); Shift(4, 8);
            RR(0x09, 0, 8); C(9, 0); Test(0); J(noTail, 4); Alu(1, 10, 1);
            Bind(noTail); J(finish);
            Bind(exactly64); Mov(0, 10); Mov(10, 9); C(9, 0);
            Test(0); J(finish, 4); Alu(1, 10, 1); J(finish);
            Bind(huge); RR(0x09, 10, 9); C(9, 0); Test(10); C(10, 0); Set(10, 5);
            Bind(finish);
        }

        private void Round()
        {
            int exact = L(), increment = L(), truncate = L(), nearest = L(), down = L(), shifted = L();
            Load(0, 108, 32); Store(0, 196, 32);
            Load(1, 104, 32); RightJam();
            Test(10); J(exact, 4); Bits(196, 1, 32);
            Load(0, 8, 16); Shift(5, 0, 10, false); Alu(4, 0, 3, false);
            Test(0, false); J(nearest, 4); Alu(7, 0, 3, false); J(truncate, 4);
            Alu(7, 0, 1, false); J(down, 4);
            Load(0, 100, 32); Test(0, false); J(increment, 4); J(truncate);
            Bind(down); Load(0, 100, 32); Test(0, false); J(increment, 5); J(truncate);
            Bind(nearest); Imm(0, 0x8000000000000000); RR(0x39, 10, 0);
            J(increment, 7); J(truncate, 2);
            Mov(0, 9); Alu(4, 0, 1); Test(0); J(truncate, 4);
            Bind(increment);
            Bits(196, 1, 512); Alu(0, 9, 1);
            Load(1, 104, 32); Shift(4, 9); Test(9); J(shifted, 5);
            Imm(9, 0x8000000000000000); Bits(192, 0, 1); J(shifted);
            Bind(exact); Bind(truncate); Load(1, 104, 32); Shift(4, 9);
            Bind(shifted);
        }

        private void RoundAndPack(int done)
        {
            int tiny = L(), overflow = L(), rangeNormal = L(), packNormal = L();
            Load(9, 112); Load(10, 120); Load(0, 96, 32); Store(0, 192, 32); Round();
            Store(9, 176); Load(0, 192, 32); Store(0, 184, 32);
            Load(2, 196, 32); Store(2, 188, 32);
            Put(84, 1); Load(0, 108, 32); Test(0, false); int noDe = L(); J(noDe, 4);
            Bits(84, 1, 32); Bind(noDe);
            Load(0, 184, 32); Alu(7, 0, 16383, false); J(overflow, 15);
            Alu(7, 0, -16382, false); J(tiny, 12); J(rangeNormal);

            Bind(tiny);
            Bits(84, 1, 2 | 4);
            Load(2, 188, 32); Alu(1, 2, 16, false); Store(2, 82, 16);
            Alu(0, 0, 24576, false); Pack(176, 64); J(rangeNormal);

            Bind(overflow);
            Bits(84, 1, 2 | 8);
            Load(2, 188, 32); Alu(1, 2, 8, false); Store(2, 82, 16);
            Alu(5, 0, 24576, false); Pack(176, 64);
            // Saturation is a range rounding, inexact even when PC was exact.
            int toInf = L(), toMax = L(), saturation = L();
            Load(0, 8, 16); Shift(5, 0, 10, false); Alu(4, 0, 3, false);
            Test(0, false); J(toInf, 4); Alu(7, 0, 3, false); J(toMax, 4);
            Load(2, 100, 32); Alu(7, 0, 1, false); int positive = L(); J(positive, 5);
            Test(2, false); J(toInf, 5); J(toMax);
            Bind(positive); Test(2, false); J(toInf, 4); J(toMax);
            Bind(toInf); Imm(9, 0x8000000000000000); C(0, 0x7fff); C(8, 8 | 32 | 512);
            J(saturation);
            Bind(toMax); Imm(9, ulong.MaxValue); Load(1, 104, 32); Shift(4, 9);
            C(0, 0x7ffe); C(8, 8 | 32);
            Bind(saturation); Load(2, 100, 32); Shift(4, 2, 15, false); RR(0x09, 0, 2, false);
            Store(9, 48); Store(0, 56, 16); Load(2, 108, 32); RR(0x09, 8, 2, false);
            Store(8, 80, 16); J(done);

            Bind(rangeNormal);
            // Compute masked result independently from the original exact
            // intermediate, avoiding double rounding through the wrapped result.
            Load(9, 112); Load(10, 120); Load(0, 96, 32); Store(0, 192, 32);
            Alu(7, 0, -16382, false); J(packNormal, 13);
            C(1, -16382); RR(0x29, 1, 0, false); RightJam(); Put(192, -16382);
            Bind(packNormal); Round(); Store(9, 48);
            Load(0, 192, 32); Alu(0, 0, 16383, false);
            // At minimum exponent a missing integer bit denotes subnormal/zero.
            int normalized = L(); Test(9); J(normalized, 8); C(0, 0);
            Bind(normalized); Load(2, 100, 32); Shift(4, 2, 15, false); RR(0x09, 0, 2, false);
            Store(0, 56, 16);
            Load(8, 196, 32); Mov(0, 8, false); Alu(4, 0, 32, false); Test(0, false);
            int inexact = L(), status = L(); J(inexact, 5);
            Bits(84, 1, 16); J(status);
            Bind(inexact); Load(0, 84, 32); Alu(4, 0, 4, false); Test(0, false); J(status, 4);
            Alu(1, 8, 16, false);
            Bind(status); Store(8, 80, 16); J(done);
        }
        // EAX unbiased exponent, [sig] full significand, sign[100]. Exponent
        // wrapping for these operations always stays within the binary80 range.
        private void Pack(int sig, int output)
        {
            // Intel Vol1 8.5.4/5 explicitly specifies FSCALE's massive range
            // response if one bias adjustment still over/underflows: +/-inf/0.
            int infinity=L(),zero=L(),sign=L();
            Alu(7,0,16383,false);J(infinity,15);Alu(7,0,-16382,false);J(zero,12);
            Alu(0,0,16383,false);Load(9,sig);J(sign);
            Bind(infinity);C(0,0x7fff);Imm(9,0x8000000000000000);J(sign);
            Bind(zero);C(0,0);C(9,0);Bind(sign);
            Load(2,100,32);Shift(4,2,15,false);RR(0x09,0,2,false);
            Store(9,output);Store(0,output+8,16);
        }
    }
}
