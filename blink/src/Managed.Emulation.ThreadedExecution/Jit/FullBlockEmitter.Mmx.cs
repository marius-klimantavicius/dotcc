using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    // Distinct low16 namespace. Core MUST dispatch before broad R66 Vector.
    // Width64 denotes the MMX IR class, not the physical XMM storage width.
    // SourceWidth is the exact memory/scalar width; EMMS has SourceWidth0.
    // MaskMove=0x3007 is RESERVED/MISSING until sparse-page MMU support;
    // classifier, validator and writer all reject it.
    internal enum FullMmxOperation : ushort
    {
        Move = 0x3001, ScalarToMmx, MmxToScalar, MmxToXmm, XmmToMmx, NonTemporalStore, MaskMove, Emms,
        And, AndNot, Or, Xor,
        AddByte, AddWord, AddDword, AddQword, SubByte, SubWord, SubDword, SubQword,
        AddSignedByte, AddSignedWord, AddUnsignedByte, AddUnsignedWord,
        SubSignedByte, SubSignedWord, SubUnsignedByte, SubUnsignedWord,
        EqualByte, EqualWord, EqualDword, GreaterByte, GreaterWord, GreaterDword,
        MinUnsignedByte, MaxUnsignedByte, MinSignedWord, MaxSignedWord,
        PackSignedWord, PackSignedDword, PackUnsignedWord,
        UnpackLowByte, UnpackLowWord, UnpackLowDword, UnpackHighByte, UnpackHighWord, UnpackHighDword,
        MultiplyLowWord, MultiplyHighSignedWord, MultiplyHighUnsignedWord,
        MultiplyUnsignedDword, MultiplyAddWord, SumAbsoluteByte, AverageByte, AverageWord,
        ShiftRightWord, ShiftRightDword, ShiftRightQword, ShiftArithmeticWord, ShiftArithmeticDword,
        ShiftLeftWord, ShiftLeftDword, ShiftLeftQword, ShuffleWord, MaskByte, InsertWord, ExtractWord,
        ShuffleByte, HorizontalAddWord, HorizontalAddDword, HorizontalAddSignedWord,
        MultiplyAddUnsignedSignedByte, HorizontalSubWord, HorizontalSubDword, HorizontalSubSignedWord,
        SignByte, SignWord, SignDword, MultiplyRoundedSignedWord, AbsoluteByte, AbsoluteWord, AbsoluteDword, AlignRightBytes
    }

    internal static bool CanInlineMmx(JitInstruction i) =>
        i.Operation == JitOperation.VectorInteger && (i.Options & 0xf000) == 0x3000;
    internal static FullMmxOperation MmxOperation(JitInstruction i) => (FullMmxOperation)(i.Options & 0xffff);
    private static bool MmxRegister(JitOperand p) => VectorRegister(p) && p.Register <= 7;
    private static bool MmxShift(FullMmxOperation op) =>
        op is >= FullMmxOperation.ShiftRightWord and <= FullMmxOperation.ShiftLeftQword;

    internal static bool ValidateMmx(JitInstruction i)
    {
        if (!CanInlineMmx(i)) return false;
        var op = MmxOperation(i);
        if (i.Width != 64 || i.Length is < 1 or > 15 || i.Target != 0 || i.Condition != 0 ||
            (i.Options & ~0xffffu) != 0 || op is < FullMmxOperation.Move or > FullMmxOperation.AlignRightBytes)
            throw new ArgumentException("Native MMX operation/metadata");
        if (op == FullMmxOperation.MaskMove)
            throw new ArgumentException("Native MMX masked store selected-page request MISSING");
        if (op == FullMmxOperation.Emms)
        {
            if (i.SourceWidth != 0 || i.Destination != default || i.Source != default || i.Extra != default || i.Address != default)
                throw new ArgumentException("Native EMMS operands");
        }
        else
        {
            bool memory = i.Destination.IsMemory || i.Source.IsMemory;
            bool destination = MmxRegister(i.Destination), source = MmxRegister(i.Source) || VectorMemory(i.Source);
            switch (op)
            {
                case FullMmxOperation.Move:
                    destination |= VectorMemory(i.Destination); break;
                case FullMmxOperation.ScalarToMmx:
                case FullMmxOperation.InsertWord:
                    source = VectorRegister(i.Source) || VectorMemory(i.Source); break;
                case FullMmxOperation.MmxToScalar:
                    destination = VectorRegister(i.Destination) || VectorMemory(i.Destination);
                    source = MmxRegister(i.Source); break;
                case FullMmxOperation.MmxToXmm:
                    destination = VectorRegister(i.Destination); source = MmxRegister(i.Source); break;
                case FullMmxOperation.XmmToMmx:
                    source = VectorRegister(i.Source); break;
                case FullMmxOperation.MaskByte:
                case FullMmxOperation.ExtractWord:
                    destination = VectorRegister(i.Destination); source = MmxRegister(i.Source); break;
                case FullMmxOperation.NonTemporalStore:
                    destination = VectorMemory(i.Destination); source = MmxRegister(i.Source); break;
                default:
                    if (MmxShift(op)) source |= VectorImmediate(i.Source);
                    break;
            }
            if (!destination || !source || i.Destination.IsMemory && i.Source.IsMemory)
                throw new ArgumentException("Native MMX operand/direction");
            bool immediate = op is FullMmxOperation.ShuffleWord or FullMmxOperation.InsertWord or
                FullMmxOperation.ExtractWord or FullMmxOperation.AlignRightBytes;
            if (immediate ? !VectorImmediate(i.Extra) : i.Extra != default)
                throw new ArgumentException("Native MMX extra operand");
            int expected = op is FullMmxOperation.InsertWord or FullMmxOperation.ExtractWord ? 16 :
                op is FullMmxOperation.ScalarToMmx or FullMmxOperation.MmxToScalar ? i.SourceWidth : 64;
            if (i.SourceWidth != expected || expected is not (16 or 32 or 64) ||
                op is FullMmxOperation.ScalarToMmx or FullMmxOperation.MmxToScalar && expected is not (32 or 64))
                throw new ArgumentException("Native MMX exact width");
            if (memory)
            {
                var a = i.Address;
                if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                    a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                    a.RipRelative && (a.Base != -1 || a.Index != -1))
                    throw new ArgumentException("Native MMX address");
            }
            else if (i.Address != default) throw new ArgumentException("Native MMX unused address");
        }
        if (!MmxLayout.Supported) throw new PlatformNotSupportedException("Translated MMX/fpu/frame layout differs");
        return true;
    }

    private static class MmxLayout
    {
        internal static readonly int TagOffset = Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.fpu)).ToInt32() +
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.tw)).ToInt32();
        internal static readonly bool Supported = HasExpectedMmxLayout();
    }

    internal static unsafe bool HasExpectedMmxLayout()
    {
        Blink.Machine m = default;
        // Verify actual translated nested field by reference, as well as the
        // reflection offset used for EMMS emission. No assumed fpu/tw offset.
        nint actual = Unsafe.ByteOffset(ref Unsafe.As<Blink.Machine, byte>(ref m),
            ref Unsafe.As<int, byte>(ref m.fpu.tw));
        nint actualSw = Unsafe.ByteOffset(ref Unsafe.As<Blink.Machine, byte>(ref m),
            ref Unsafe.As<uint, byte>(ref m.fpu.sw));
        nint actualCw = Unsafe.ByteOffset(ref Unsafe.As<Blink.Machine, byte>(ref m),
            ref Unsafe.As<uint, byte>(ref m.fpu.cw));
        return HasExpectedVectorLayout() && actual == MmxLayout.TagOffset &&
            actual >= 456 && actual + sizeof(int) <= 688 && sizeof(Blink.MachineFpu) <= 104 &&
            actualSw == 648 && actualCw == 672;
    }

    private ref partial struct Writer
    {
        private bool TryMmxOperation(in JitInstruction i)
        {
            if (!CanInlineMmx(i)) return false;
            var op = MmxOperation(i);
            if (op == FullMmxOperation.MaskMove)
                throw new ArgumentException("Native MMX masked store selected-page request MISSING");
            // Every admitted MMX operation is waiting, including EMMS and
            // MMX/XMM moves: pending unmasked x87 precedes ALL effects/MMU.
            // Parent helper is GPR-only; actual SW/CW offsets are guarded above.
            FloatMmxWaitingException(i);
            if (op == FullMmxOperation.Emms)
            {
                MmxEmms();
                return true;
            }
            bool memory = i.Destination.IsMemory || i.Source.IsMemory;
            if (memory)
            {
                ScalarMiscCheckCanonical(i, i.SourceWidth);
                NativeMemoryPrepare(i, i.Destination.IsMemory ? FullMemoryAccess.Write : FullMemoryAccess.Read, i.SourceWidth);
            }
            // Architectural MMX m64 has no extra alignment8 requirement.
            // Both mappings/permissions are checked before reads or effects.
            if (op is FullMmxOperation.ScalarToMmx or FullMmxOperation.InsertWord)
            {
                if (i.Source.IsMemory) VectorMemoryLoad(i.SourceWidth);
                else
                {
                    IntegerLoad(i.Source, i.SourceWidth, 0);
                    Bytes(0x66); if (i.SourceWidth == 64) Bytes(0x48);
                    Bytes(0x0f, 0x6e, 0xc8);
                }
                if (op == FullMmxOperation.InsertWord)
                {
                    MmxLoad(0, i.Destination.Register);
                    Bytes(0x66, 0x0f, 0x7e, 0xc8, 0x66, 0x0f, 0xc4, 0xc0, (byte)(i.Extra.Immediate & 3));
                }
                else Bytes(0x66, 0x0f, 0x6f, 0xc1);
            }
            else if (op == FullMmxOperation.MmxToScalar)
            {
                MmxLoad(0, i.Source.Register);
                if (i.Destination.IsMemory) VectorMemoryStore(i.SourceWidth);
                else
                {
                    Bytes(0x66, i.SourceWidth == 64 ? (byte)0x49 : (byte)0x41, 0x0f, 0x7e, 0xc0);
                    Store(i.Destination, i.SourceWidth);
                }
                MmxTransition();
                if (memory) NativeMemoryCount();
                return true;
            }
            else if (op is FullMmxOperation.MaskByte or FullMmxOperation.ExtractWord)
            {
                MmxLoad(0, i.Source.Register);
                if (op == FullMmxOperation.MaskByte) Bytes(0x66, 0x44, 0x0f, 0xd7, 0xc0);
                else Bytes(0x66, 0x44, 0x0f, 0xc5, 0xc0, (byte)(i.Extra.Immediate & 3));
                Store(i.Destination, 32); // architectural PEXTRW zeros upper GPR
                MmxTransition();
                return true;
            }
            else if (op == FullMmxOperation.MmxToXmm)
            { MmxLoad(0, i.Source.Register); VectorGuestStore(0, i.Destination.Register); MmxTransition(); return true; }
            else if (op == FullMmxOperation.XmmToMmx)
                VectorTransfer(0xf3, 0x7e, 0, 11, 160 + i.Source.Register * 16);
            else if (op is FullMmxOperation.Move or FullMmxOperation.NonTemporalStore)
            {
                if (i.Source.IsMemory) { VectorMemoryLoad(64); Bytes(0x66, 0x0f, 0x6f, 0xc1); }
                else MmxLoad(0, i.Source.Register);
                if (i.Destination.IsMemory)
                {
                    // A checked ordinary write supplies at least NT ordering.
                    VectorMemoryStore(64); MmxTransition(); NativeMemoryCount(); return true;
                }
            }
            else
            {
                MmxLoad(0, i.Destination.Register);
                if (i.Source.IsImmediate) MmxImmediateShift(op, (byte)i.Source.Immediate);
                else
                {
                    if (i.Source.IsMemory) VectorMemoryLoad(64); else MmxLoad(1, i.Source.Register);
                    if (op >= FullMmxOperation.ShuffleByte) MmxExtended(op, (byte)i.Extra.Immediate);
                    else if (op == FullMmxOperation.ShuffleWord)
                        Bytes(0xf2, 0x0f, 0x70, 0xc1, (byte)i.Extra.Immediate);
                    else if (op is FullMmxOperation.PackSignedWord or FullMmxOperation.PackSignedDword or FullMmxOperation.PackUnsignedWord)
                    {
                        // Join BOTH four-word/two-dword inputs before packing.
                        // A plain128-bit pack + truncation would lose source.
                        Bytes(0x66, 0x0f, 0x6c, 0xc1, 0x66, 0x0f, MmxBinaryOpcode(op), 0xc0);
                    }
                    else if (op is FullMmxOperation.UnpackHighByte or FullMmxOperation.UnpackHighWord or FullMmxOperation.UnpackHighDword)
                    {
                        // MMX high half begins at byte4, not byte8 as in XMM.
                        Bytes(0x66, 0x0f, 0x73, 0xd8, 4, 0x66, 0x0f, 0x73, 0xd9, 4);
                        byte low = op == FullMmxOperation.UnpackHighByte ? (byte)0x60 :
                            op == FullMmxOperation.UnpackHighWord ? (byte)0x61 : (byte)0x62;
                        Bytes(0x66, 0x0f, low, 0xc1);
                    }
                    else Bytes(0x66, 0x0f, MmxBinaryOpcode(op), 0xc1);
                }
            }
            MmxStore(0, i.Destination.Register);
            MmxTransition();
            if (memory) NativeMemoryCount();
            return true;
        }

        private static byte MmxBinaryOpcode(FullMmxOperation op) => op switch
        {
            FullMmxOperation.And => 0xdb, FullMmxOperation.AndNot => 0xdf, FullMmxOperation.Or => 0xeb, FullMmxOperation.Xor => 0xef,
            FullMmxOperation.AddByte => 0xfc, FullMmxOperation.AddWord => 0xfd, FullMmxOperation.AddDword => 0xfe, FullMmxOperation.AddQword => 0xd4,
            FullMmxOperation.SubByte => 0xf8, FullMmxOperation.SubWord => 0xf9, FullMmxOperation.SubDword => 0xfa, FullMmxOperation.SubQword => 0xfb,
            FullMmxOperation.AddSignedByte => 0xec, FullMmxOperation.AddSignedWord => 0xed, FullMmxOperation.AddUnsignedByte => 0xdc, FullMmxOperation.AddUnsignedWord => 0xdd,
            FullMmxOperation.SubSignedByte => 0xe8, FullMmxOperation.SubSignedWord => 0xe9, FullMmxOperation.SubUnsignedByte => 0xd8, FullMmxOperation.SubUnsignedWord => 0xd9,
            FullMmxOperation.EqualByte => 0x74, FullMmxOperation.EqualWord => 0x75, FullMmxOperation.EqualDword => 0x76,
            FullMmxOperation.GreaterByte => 0x64, FullMmxOperation.GreaterWord => 0x65, FullMmxOperation.GreaterDword => 0x66,
            FullMmxOperation.MinUnsignedByte => 0xda, FullMmxOperation.MaxUnsignedByte => 0xde, FullMmxOperation.MinSignedWord => 0xea, FullMmxOperation.MaxSignedWord => 0xee,
            FullMmxOperation.PackSignedWord => 0x63, FullMmxOperation.PackSignedDword => 0x6b, FullMmxOperation.PackUnsignedWord => 0x67,
            FullMmxOperation.UnpackLowByte => 0x60, FullMmxOperation.UnpackLowWord => 0x61, FullMmxOperation.UnpackLowDword => 0x62,
            FullMmxOperation.MultiplyLowWord => 0xd5, FullMmxOperation.MultiplyHighSignedWord => 0xe5, FullMmxOperation.MultiplyHighUnsignedWord => 0xe4,
            FullMmxOperation.MultiplyUnsignedDword => 0xf4, FullMmxOperation.MultiplyAddWord => 0xf5, FullMmxOperation.SumAbsoluteByte => 0xf6,
            FullMmxOperation.AverageByte => 0xe0, FullMmxOperation.AverageWord => 0xe3,
            FullMmxOperation.ShiftRightWord => 0xd1, FullMmxOperation.ShiftRightDword => 0xd2, FullMmxOperation.ShiftRightQword => 0xd3,
            FullMmxOperation.ShiftArithmeticWord => 0xe1, FullMmxOperation.ShiftArithmeticDword => 0xe2,
            FullMmxOperation.ShiftLeftWord => 0xf1, FullMmxOperation.ShiftLeftDword => 0xf2, FullMmxOperation.ShiftLeftQword => 0xf3,
            _ => throw new ArgumentException("Native MMX binary operation")
        };

        private void MmxImmediateShift(FullMmxOperation op, byte count)
        {
            byte code = op is FullMmxOperation.ShiftRightWord or FullMmxOperation.ShiftArithmeticWord or FullMmxOperation.ShiftLeftWord ? (byte)0x71 :
                op is FullMmxOperation.ShiftRightDword or FullMmxOperation.ShiftArithmeticDword or FullMmxOperation.ShiftLeftDword ? (byte)0x72 : (byte)0x73;
            int group = op is FullMmxOperation.ShiftArithmeticWord or FullMmxOperation.ShiftArithmeticDword ? 4 :
                op is FullMmxOperation.ShiftLeftWord or FullMmxOperation.ShiftLeftDword or FullMmxOperation.ShiftLeftQword ? 6 : 2;
            Bytes(0x66, 0x0f, code, (byte)(0xc0 | group << 3), count);
        }

        // XMM0/1 immutable snapshots; XMM2 extraction, XMM3 insertion,
        // XMM4 accumulator. Only volatile GPRs, SSE2 and integer arithmetic.
        private void MmxExtract(int xmm, int offset, int width, int register, bool signed = false)
        {
            Bytes(0x66, 0x0f, 0x6f, (byte)(0xd0 | xmm));
            if (offset != 0) Bytes(0x66, 0x0f, 0x73, 0xda, (byte)offset);
            Bytes(0x66); int rex = 0x40 | (width == 64 ? 8 : 0) | (register >> 3);
            if (rex != 0x40) Bytes((byte)rex);
            Bytes(0x0f, 0x7e, (byte)(0xd0 | (register & 7)));
            if (signed && width < 32)
            { IntegerShift(4, register, 32, 32 - width); IntegerShift(7, register, 32, 32 - width); }
            else if (width < 32) IntegerTruncate(register, width);
        }
        private void MmxAppend(int offset, int width)
        {
            if (width < 32) IntegerTruncate(0, width);
            Bytes(0x66); if (width == 64) Bytes(0x48);
            Bytes(0x0f, 0x6e, 0xd8);
            if (offset != 0) Bytes(0x66, 0x0f, 0x73, 0xfb, (byte)offset);
            Bytes(0x66, 0x0f, 0xeb, 0xe3);
        }
        private void MmxSaturateWord()
        {
            IntegerImmediate(2, 32767, 32); IntegerRR(0x39, 0, 2, 32); Bytes(0x0f, 0x4f, 0xc2);
            IntegerImmediate(2, 0xffff8000, 32); IntegerRR(0x39, 0, 2, 32); Bytes(0x0f, 0x4c, 0xc2);
        }
        private void MmxExtended(FullMmxOperation op, byte control)
        {
            Bytes(0x66, 0x0f, 0xef, 0xe4);
            if (op == FullMmxOperation.ShuffleByte)
            {
                // This table is invocation scratch184..191; checked crossing
                // gathers have already finished before scratch is reused.
                VectorTransfer(0x66, 0xd6, 0, 10, 184);
                for (int b = 0; b < 8; ++b)
                {
                    MmxExtract(1, b, 8, 0); IntegerMov(1, 0, 32); IntegerConstant(4, 0, 7);
                    Bytes(0x41, 0x0f, 0xb6, 0x84, 0x02); U32(184);
                    IntegerShift(5, 1, 32, 7); IntegerConstant(5, 1, 1); IntegerRR(0x21, 0, 1, 32);
                    MmxAppend(b, 8);
                }
            }
            else if (op == FullMmxOperation.AlignRightBytes)
            {
                for (int b = 0; b < 8; ++b)
                {
                    int at = control + b;
                    if (at >= 16) continue;
                    MmxExtract(at < 8 ? 1 : 0, at & 7, 8, 0); MmxAppend(b, 8);
                }
            }
            else if (op is FullMmxOperation.HorizontalAddWord or FullMmxOperation.HorizontalAddDword or FullMmxOperation.HorizontalAddSignedWord or
                FullMmxOperation.HorizontalSubWord or FullMmxOperation.HorizontalSubDword or FullMmxOperation.HorizontalSubSignedWord)
            {
                bool dword = op is FullMmxOperation.HorizontalAddDword or FullMmxOperation.HorizontalSubDword;
                bool sat = op is FullMmxOperation.HorizontalAddSignedWord or FullMmxOperation.HorizontalSubSignedWord;
                bool sub = op is FullMmxOperation.HorizontalSubWord or FullMmxOperation.HorizontalSubDword or FullMmxOperation.HorizontalSubSignedWord;
                int width = dword ? 32 : 16, step = width / 8, half = 32 / width;
                for (int lane = 0; lane < 64 / width; ++lane)
                {
                    int input = lane / half, pair = lane % half * step * 2;
                    MmxExtract(input, pair, width, 0, sat); MmxExtract(input, pair + step, width, 1, sat);
                    IntegerRR(sub ? (byte)0x29 : (byte)0x01, 0, 1, 32);
                    if (sat) MmxSaturateWord();
                    MmxAppend(lane * step, width);
                }
            }
            else if (op == FullMmxOperation.MultiplyAddUnsignedSignedByte)
            {
                for (int lane = 0; lane < 4; ++lane)
                {
                    MmxExtract(0, lane * 2, 8, 0); MmxExtract(1, lane * 2, 8, 1, true);
                    Bytes(0x0f, 0xaf, 0xc1); IntegerMov(8, 0, 32);
                    MmxExtract(0, lane * 2 + 1, 8, 0); MmxExtract(1, lane * 2 + 1, 8, 1, true);
                    Bytes(0x0f, 0xaf, 0xc1); IntegerRR(0x01, 0, 8, 32); MmxSaturateWord(); MmxAppend(lane * 2, 16);
                }
            }
            else if (op == FullMmxOperation.MultiplyRoundedSignedWord)
            {
                for (int b = 0; b < 8; b += 2)
                {
                    MmxExtract(0, b, 16, 0, true); MmxExtract(1, b, 16, 1, true); Bytes(0x0f, 0xaf, 0xc1);
                    IntegerConstant(0, 0, 0x4000); IntegerShift(7, 0, 32, 15); MmxAppend(b, 16);
                }
            }
            else
            {
                bool absolute = op is FullMmxOperation.AbsoluteByte or FullMmxOperation.AbsoluteWord or FullMmxOperation.AbsoluteDword;
                int width = op is FullMmxOperation.SignByte or FullMmxOperation.AbsoluteByte ? 8 :
                    op is FullMmxOperation.SignWord or FullMmxOperation.AbsoluteWord ? 16 : 32;
                for (int b = 0; b < 8; b += width / 8)
                {
                    MmxExtract(absolute ? 1 : 0, b, width, 0, absolute);
                    if (absolute) IntegerMov(1, 0, 32); else MmxExtract(1, b, width, 1, true);
                    IntegerMov(2, 1, 32); IntegerShift(7, 2, 32, 31);
                    IntegerRR(0x31, 0, 2, 32); IntegerRR(0x29, 0, 2, 32);
                    if (!absolute)
                    {
                        IntegerRR(0x85, 1, 1, 32); Bytes(0x0f, 0x95, 0xc1, 0x0f, 0xb6, 0xc9);
                        IntegerUnary(3, 1, 32); IntegerRR(0x21, 0, 1, 32);
                    }
                    MmxAppend(b, width);
                }
            }
            Bytes(0x66, 0x0f, 0x6f, 0xc4);
        }
    }
}
