using System;
using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// R182 source refinement, UNBUILT/UNRUN; qualification awaits the parent gate.
// Four exponent-independent 256-bit slots in Work96..255, no host FP interval.
// Source arithmetic/error/status contracts and certificate API: R182 ready/proof.
internal static partial class FullExtended80TranscendentalSoftwareEmitter
{
    internal const int CodeCapacity = 262144;
    internal static bool TryEmit(FullExtended80Transcendental op, Span<byte> destination,
        out int length, bool? windowsAbi = null)
        => Emit(op, destination, out length, windowsAbi ?? OperatingSystem.IsWindows(), false);
    internal static bool TryEmitContinuation(FullExtended80Transcendental op,
        Span<byte> destination, out int length) => Emit(op, destination, out length, false, true);
    private static bool Emit(FullExtended80Transcendental op, Span<byte> destination,
        out int length, bool windows, bool continuation)
    {
        length = 0;
        if ((uint)op > 7) return false;
        Span<int> labels = stackalloc int[4096], heads = stackalloc int[4096];
        labels.Fill(-1); heads.Fill(-1);
        var w = new Writer(destination[..Math.Min(destination.Length, CodeCapacity)], labels, heads);
        w.Family(op, windows, continuation);
        if (!w.Complete) return false;
        length = w.Length; return true;
    }
    private ref partial struct Writer
    {
        private const int A=96, BB=136, CC=176, DD=216;
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


    }
}
