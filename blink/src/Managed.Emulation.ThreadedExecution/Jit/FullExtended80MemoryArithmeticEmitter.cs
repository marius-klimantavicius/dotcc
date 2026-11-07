using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

internal enum FullExtended80MemoryArithmeticOperation : byte
{ Add, Multiply, Subtract, SubtractReverse, Divide, DivideReverse }
internal enum FullExtended80MemoryArithmeticFormat : byte
{ Binary32, Binary64, Int32, Int16 }

// R155 SOURCE_READY_UNEXECUTED. Own-project integer numeric foundation fork;
// immutable arithmetic method bodies pinned in D/numeric-body-pins.json.
// Work.Left=raw80 ST0; Work.Right low2/4/8 bytes=RAW memory operand.
// Work v1/384 and output protocol unchanged. Scratch96..255 only.
internal static class FullExtended80MemoryArithmeticEmitter
{
    internal const int CodeCapacity = 32768;
    internal const byte FirstKernelKey = 32, KernelCount = 24;
    internal static bool TryEmit(FullExtended80MemoryArithmeticOperation operation,
        FullExtended80MemoryArithmeticFormat format, Span<byte> destination,
        out int length, bool? windowsAbi = null) =>
        Emit(operation, format, destination, out length, windowsAbi ?? OperatingSystem.IsWindows(), false);

    internal static bool TryEmitContinuation(FullExtended80MemoryArithmeticOperation operation,
        FullExtended80MemoryArithmeticFormat format, Span<byte> destination, out int length) =>
        Emit(operation, format, destination, out length, false, true);

    // Parent catalog seam: no catalog mutation or runtime opcode field.
    internal static bool TryEmitKernel(byte key, Span<byte> destination, out int length)
    {
        int index = key - FirstKernelKey;
        if ((uint)index >= KernelCount) { length = 0; return false; }
        return TryEmitContinuation((FullExtended80MemoryArithmeticOperation)(index % 6),
            (FullExtended80MemoryArithmeticFormat)(index / 6), destination, out length);
    }

    private static bool Emit(FullExtended80MemoryArithmeticOperation operation,
        FullExtended80MemoryArithmeticFormat format, Span<byte> destination,
        out int length, bool windows, bool continuation)
    {
        length = 0;
        if ((uint)operation > 5 || (uint)format > 3) return false;
        var op = operation switch
        {
            FullExtended80MemoryArithmeticOperation.Add => FullExtended80Operation.Add,
            FullExtended80MemoryArithmeticOperation.Multiply => FullExtended80Operation.Multiply,
            FullExtended80MemoryArithmeticOperation.Subtract or FullExtended80MemoryArithmeticOperation.SubtractReverse => FullExtended80Operation.Subtract,
            _ => FullExtended80Operation.Divide
        };
        bool reverse = operation is FullExtended80MemoryArithmeticOperation.SubtractReverse or FullExtended80MemoryArithmeticOperation.DivideReverse;
        Span<int> labels = stackalloc int[512], heads = stackalloc int[512];
        labels.Fill(-1); heads.Fill(-1);
        var w = new Writer(destination, labels, heads);
        w.Body(op, format, reverse, windows, continuation);
        if (!w.Complete) return false;
        length = w.Length; return true;
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

        internal void Body(FullExtended80Operation op, FullExtended80MemoryArithmeticFormat format, bool reverse, bool windows, bool continuation)
        {
            int badVersion = L(), badSize = L(), done = L();
            int invalid = L(), nan = L(), infinity = L(), zero = L(), calculate = L(), rounded = L();
            // ENDBR64 is a no-op on baseline non-CET hardware; no feature needed.
            B(0xf3); B(0x0f); B(0x1e); B(0xfa);
            if (!continuation) Mov(11, windows ? 1 : 7);
            // Only output bytes [12,16),[48,88) initialized; inputs/continuation
            // and caller-reserved bytes are preserved even on rejected input.
            C(0, 0); Store(0, 12, 32);
            for (int p = 48; p < 88; p += 8) Store(0, p);
            Load(0, 0, 32); Alu(7, 0, 1, false); J(badVersion, 5);
            Load(0, 4, 32); Alu(7, 0, 384, false); J(badSize, 5);
            Load(0, 8, 16); Shift(5, 0, 8, false); Alu(4, 0, 3, false);
            // Software precision: PC00=>24, PC10=>53, PC01/11=>64.
            // Decode at use; Work.Control retains the exact raw guest CW.
            C(2, 40); int pcSet = L(); Test(0, false); J(pcSet, 4);
            C(2, 11); Alu(7, 0, 2, false); J(pcSet, 4); C(2, 0);
            Bind(pcSet); Store(2, 104, 32); Put(108, 0);
            Decode(16, 128, 144, 152, 160, 168);
            DecodeMemory(format);
            if (reverse)
            {
                Swap(128, 136, 64); Swap(144, 148, 32); Swap(152, 156, 32);
                Swap(160, 164, 32); Swap(168, 172, 32);
            }
            Load(0, 160, 32); Alu(7, 0, 5, false); J(invalid, 4);
            Load(0, 164, 32); Alu(7, 0, 5, false); J(invalid, 4);
            Load(0, 160, 32); Alu(7, 0, 3, false); J(nan, 4);
            Alu(7, 0, 4, false); J(nan, 4);
            Load(0, 164, 32); Alu(7, 0, 3, false); J(nan, 4);
            Alu(7, 0, 4, false); J(nan, 4);
            // Intel093 4.9.2: zero-divide precedes DE, even when the nonzero
            // numerator is denormal. Infinity / zero has no ZE (8.5.3).
            if (op == FullExtended80Operation.Divide) DivideZeroDivisor(invalid, done);
            // Class6 is a canonical or pseudo denormal, both arithmetic DE.
            // Map it to finite class1 after recording DE; NaNs took precedence.
            int afterDeA = L(), afterDeB = L();
            Load(0, 160, 32); Alu(7, 0, 6, false); J(afterDeA, 5);
            Put(108, 2); Put(160, 1);
            Bind(afterDeA); Load(0, 164, 32); Alu(7, 0, 6, false); J(afterDeB, 5);
            Put(108, 2); Put(164, 1);
            Bind(afterDeB);
            if (op == FullExtended80Operation.Subtract) Bits(156, 6, 1);
            Load(0, 160, 32); Alu(7, 0, 2, false); J(infinity, 4);
            Load(0, 164, 32); Alu(7, 0, 2, false); J(infinity, 4);
            Load(0, 160, 32); Test(0, false); J(zero, 4);
            Load(0, 164, 32); Test(0, false); J(zero, 4);
            J(calculate);

            Bind(invalid);
            Imm(9, 0xc000000000000000); C(2, 0xffff); C(8, 1);
            Special(9, 2, 8, 1 | 32 | 64); J(done);

            Bind(nan);
            NaN(done);

            Bind(infinity);
            if (op == FullExtended80Operation.Divide)
            {
                Load(0, 160, 32); Alu(7, 0, 2, false); int numeratorInf = L(); J(numeratorInf, 4);
                // A finite numerator (including signed zero) / infinity.
                Load(2, 152, 32); Load(0, 156, 32); RR(0x31, 2, 0, false);
                ZeroResult(done);
                Bind(numeratorInf); Load(0, 164, 32); Alu(7, 0, 2, false); J(invalid, 4);
                Load(2, 152, 32); Load(0, 156, 32); RR(0x31, 2, 0, false);
            }
            else if (op == FullExtended80Operation.Multiply)
            {
                Load(0, 160, 32); Test(0, false); J(invalid, 4);
                Load(0, 164, 32); Test(0, false); J(invalid, 4);
                Load(2, 152, 32); Load(0, 156, 32); RR(0x31, 2, 0, false);
            }
            else
            {
                int onlyB = L(), infSign = L();
                Load(0, 160, 32); Alu(7, 0, 2, false); J(onlyB, 5);
                Load(2, 152, 32); Load(0, 164, 32); Alu(7, 0, 2, false); J(infSign, 5);
                Load(0, 156, 32); RR(0x39, 2, 0, false); J(invalid, 5); J(infSign);
                Bind(onlyB); Load(2, 156, 32); Bind(infSign);
            }
            Shift(4, 2, 15, false); Alu(1, 2, 0x7fff, false);
            Imm(9, 0x8000000000000000); Load(8, 108, 32);
            Special(9, 2, 8, 1 | 16 | 128); J(done);

            Bind(zero);
            if (op is FullExtended80Operation.Multiply or FullExtended80Operation.Divide)
            {
                Load(2, 152, 32); Load(0, 156, 32); RR(0x31, 2, 0, false);
                ZeroResult(done);
            }
            else
            {
                int bothZero = L(), useB = L();
                Load(0, 160, 32); Load(2, 164, 32); RR(0x09, 0, 2, false);
                Test(0, false); J(bothZero, 4);
                Load(0, 160, 32); Test(0, false); J(useB, 4);
                CopyExact(128, 144, 152); J(rounded);
                Bind(useB); CopyExact(136, 148, 156); J(rounded);
                Bind(bothZero); Load(2, 152, 32); Load(0, 156, 32);
                RR(0x39, 2, 0, false); int sameZeroSign = L(); J(sameZeroSign, 4);
                CancellationSign(); Bind(sameZeroSign); ZeroResult(done);
            }

            Bind(calculate);
            if (op == FullExtended80Operation.Divide) Divide();
            else if (op == FullExtended80Operation.Multiply) Multiply(); else AddSubtract(done);
            Bind(rounded); RoundAndPack(done);

            Bind(badVersion); Put(12, 1); J(done);
            Bind(badSize); Put(12, 2); J(done);
            Bind(done); Exit(continuation);
        }

        // Direct raw memory lift into decoded operand B. No input write or
        // preliminary conversion/quieting/rounding; original IEEE denorm class6
        // survives until Body has resolved all higher-priority classes and ZE.
        private void DecodeMemory(FullExtended80MemoryArithmeticFormat format)
        {
            if (format is FullExtended80MemoryArithmeticFormat.Int16 or FullExtended80MemoryArithmeticFormat.Int32)
            { DecodeInteger(format == FullExtended80MemoryArithmeticFormat.Int16 ? 16 : 32); return; }
            int fraction = format == FullExtended80MemoryArithmeticFormat.Binary32 ? 23 : 52;
            int bias = fraction == 23 ? 127 : 1023, emax = fraction == 23 ? 255 : 2047;
            int eZero = L(), special = L(), zero = L(), finish = L(), notInf = L();
            Load(9, 32, fraction == 23 ? 32 : 64);
            Mov(2, 9); Shift(5, 2, fraction == 23 ? 31 : 63); Store(2, 156, 32);
            Mov(0, 9); Shift(5, 0, fraction); Alu(4, 0, emax, false);
            Store(0, 148, 32);
            Imm(0, (1UL << fraction) - 1); RR(0x21, 9, 0);
            Load(0, 148, 32); Test(0, false); J(eZero, 4);
            Alu(7, 0, emax, false); J(special, 4);
            Alu(5, 0, bias, false); Store(0, 148, 32);
            Imm(0, 1UL << fraction); RR(0x09, 9, 0); Shift(4, 9, 63 - fraction);
            Store(9, 136); Put(164, 1); J(finish);
            Bind(eZero); Test(9); J(zero, 4);
            Bsr(1, 9); C(0, 1 - bias - fraction); RR(0x01, 0, 1, false); Store(0, 148, 32);
            C(2, 63); RR(0x29, 2, 1, false); Mov(1, 2, false); Shift(4, 9);
            Store(9, 136); Put(164, 6); J(finish);
            Bind(zero); Store(9, 136); Put(148, -16382); Put(164, 0); J(finish);
            Bind(special);
            Shift(4, 9, 63 - fraction); Imm(0, 0x8000000000000000); RR(0x09, 9, 0); Store(9, 136);
            Load(2, 156, 32); Shift(4, 2, 15, false); Alu(1, 2, 0x7fff, false); Store(2, 172, 32);
            RR(0x39, 9, 0); J(notInf, 5); Put(164, 2); J(finish);
            Bind(notInf); Mov(0, 9); Shift(5, 0, 62); Alu(4, 0, 1, false);
            C(2, 4); RR(0x29, 2, 0, false); Store(2, 164, 32);
            Bind(finish);
        }

        private void DecodeInteger(int width)
        {
            int zero = L(), positive = L(), finish = L();
            Load(9, 32, width);
            // Sign extension and absolute value are exact for both signed minima.
            Shift(4, 9, 64 - width); Shift(7, 9, 64 - width);
            Mov(2, 9); Shift(5, 2, 63); Store(2, 156, 32);
            Test(9); J(zero, 4); J(positive, 9);
            Rex(true, 0, 9); B(0xf7); B(0xd8 | (9 & 7));
            Bind(positive); Bsr(1, 9); Store(1, 148, 32);
            C(2, 63); RR(0x29, 2, 1, false); Mov(1, 2, false); Shift(4, 9);
            Store(9, 136); Put(164, 1); J(finish);
            Bind(zero); Store(9, 136); Put(148, -16382); Put(164, 0);
            Bind(finish);
        }

        // Classes: zero0 finite1 infinity2 qNaN3 sNaN4 unsupported5 denormal6.
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
        private void CancellationSign()
        {
            Load(2, 8, 16); Shift(5, 2, 10, false); Alu(4, 2, 3, false);
            Alu(7, 2, 1, false); C(2, 0); Set(2, 4);
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

        // Unsigned 128-bit shift-right-with-jam. R9:R10 => R9:R10,
        // ECX=count >=0. Clobbers RAX/RDX/R8/RCX, never shifts by masked 64.
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

        private void Multiply()
        {
            Load(0, 128); Load(8, 136);
            Rex(true, 0, 8); B(0xf7); B(0xe0 | (8 & 7)); // MUL R8 => RDX:RAX
            Mov(9, 2); Mov(10, 0);
            Load(0, 144, 32); Load(2, 148, 32); RR(0x01, 0, 2, false); Alu(0, 0, 1, false);
            Store(0, 96, 32);
            Test(9); int normalized = L(); J(normalized, 8);
            C(1, 1); DoubleShift(false, 9, 10); Shift(4, 10, 1); Bits(96, 5, 1);
            Bind(normalized); Store(9, 112); Store(10, 120);
            Load(0, 152, 32); Load(2, 156, 32); RR(0x31, 0, 2, false); Store(0, 100, 32);
        }

        // Runs after unsupported/NaN priority, before recording denormal DE.
        // Normal is a fully masked outcome; parent still owns unmasked IE/ZE
        // suppression, SW/ES/B, stack/memory completion and deferred #MF.
        private void DivideZeroDivisor(int invalid, int done)
        {
            int nonzeroDivisor = L(), numeratorInf = L(), result = L(), classified = L();
            Load(0, 164, 32); Test(0, false); J(nonzeroDivisor, 5);
            Load(0, 160, 32); Test(0, false); J(invalid, 4); // 0 / 0
            Alu(7, 0, 2, false); J(numeratorInf, 4);
            C(8, 4); J(result); // finite nonzero / zero: ZE, no DE
            Bind(numeratorInf); C(8, 0); // infinity / zero: exact infinity
            Bind(result);
            Load(2, 152, 32); Load(0, 156, 32); RR(0x31, 2, 0, false);
            Shift(4, 2, 15, false); Alu(1, 2, 0x7fff, false); Imm(9, 0x8000000000000000);
            Store(9, 48); Store(2, 56, 16); Store(8, 80, 16);
            C(0, 1 | 128); Test(8, false); int noZe = L(); J(noZe, 4);
            Alu(1, 0, 32, false); J(classified);
            Bind(noZe); Alu(1, 0, 16, false); Bind(classified); Store(0, 84, 32); J(done);
            Bind(nonzeroDivisor);
        }

        // A,B are normalized unsigned64 significands. E=ea-eb if A>=B,
        // otherwise ea-eb-1. Generate leading1 then EXACTLY127 fraction bits.
        // At loop entry 0<=remainder<B; doubling needs a carry plus64 bits.
        // A carry implies subtraction, otherwise compare the low limb with B.
        // Subtraction modulo2^64 gives the true new remainder (always <B).
        // R9:R10=floor(normalized(A/B)*2^127), with bit0 jammed if remainder
        // is nonzero. This is truncation+sticky, never preliminary rounding;
        // Round/RightJam retain every PC24/53/64 guard and sticky decision,
        // including independent masked underflow rounding from this original.
        // No DIV/IDIV, host FP, runtime helper, or exponent-distance loop.
        private void Divide()
        {
            int below = L(), setup = L(), loop = L(), subtract = L(), nextBit = L(), exact = L();
            Load(0, 144, 32); Load(2, 148, 32); RR(0x29, 0, 2, false); Store(0, 96, 32);
            Load(0, 152, 32); Load(2, 156, 32); RR(0x31, 0, 2, false); Store(0, 100, 32);
            Load(0, 128); Load(8, 136); RR(0x39, 0, 8); J(below, 2);
            RR(0x29, 0, 8); J(setup);
            Bind(below); Shift(4, 0, 1); RR(0x29, 0, 8); Bits(96, 5, 1);
            Bind(setup); C(9, 0); C(10, 1); C(1, 127);
            Bind(loop);
            RR(0x01, 10, 10); RR(0x11, 9, 9); // ADD low; ADC high
            Shift(4, 0, 1); J(subtract, 2); // doubled remainder's 65th bit
            RR(0x39, 0, 8); J(nextBit, 2);
            Bind(subtract); RR(0x29, 0, 8); Alu(1, 10, 1);
            Bind(nextBit); Alu(5, 1, 1, false); J(loop, 5);
            Test(0); J(exact, 4); Alu(1, 10, 1);
            Bind(exact); Store(9, 112); Store(10, 120);
        }

        private void AddSubtract(int done)
        {
            int swap = L(), sorted = L(), subtract = L(), normalize = L(), haveHigh = L(), result = L();
            Load(0, 144, 32); Load(2, 148, 32); RR(0x39, 0, 2, false); J(swap, 12); J(sorted, 15);
            Load(9, 128); Load(10, 136); RR(0x39, 9, 10); J(sorted, 3);
            Bind(swap);
            Swap(128, 136, 64); Swap(144, 148, 32); Swap(152, 156, 32);
            Bind(sorted);
            Load(0, 144, 32); Store(0, 96, 32); Load(2, 148, 32); RR(0x29, 0, 2, false); Mov(1, 0, false);
            Load(0, 152, 32); Store(0, 100, 32);
            Load(9, 136); C(10, 0); RightJam();
            Load(0, 152, 32); Load(2, 156, 32); RR(0x39, 0, 2, false); J(subtract, 5);
            Load(0, 128); RR(0x01, 9, 0); J(result, 3); // JAE: no high carry.
            // 129th carry is still live across JAE/MOV. RCR feeds it into hi.
            Shift(3, 9, 1); Shift(3, 10, 1); C(0, 0); Set(0, 2); RR(0x09, 10, 0);
            Bits(96, 0, 1); J(result);
            Bind(subtract);
            C(0, 0); RR(0x29, 0, 10); Mov(10, 0); Load(0, 128); RR(0x19, 0, 9); Mov(9, 0);
            Mov(0, 9); RR(0x09, 0, 10); Test(0); J(normalize, 5);
            CancellationSign(); ZeroResult(done);
            Bind(normalize); Test(9); J(haveHigh, 5);
            Mov(9, 10); C(10, 0); Bits(96, 5, 64);
            Bind(haveHigh);
            Bsr(1, 9); C(2, 63); RR(0x29, 2, 1, false); Mov(1, 2, false);
            Load(0, 96, 32); RR(0x29, 0, 2, false); Store(0, 96, 32);
            DoubleShift(false, 9, 10); Shift(4, 10);
            Bind(result); Store(9, 112); Store(10, 120);
        }
        private void Swap(int a, int b, int width)
        { Load(0, a, width); Load(2, b, width); Store(2, a, width); Store(0, b, width); }

        // Input R9:R10, exponent [192]. Output rounded full-width significand
        // R9 and exponent [192], status[196]. Uses dynamic PC without opcode
        // interpretation. R10's high bit is guard; all remaining bits are tail.
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
            Alu(0, 0, 16383, false); Load(2, 100, 32); Shift(4, 2, 15, false);
            RR(0x09, 0, 2, false); Load(9, sig); Store(9, output); Store(0, output + 8, 16);
        }
    }
}
