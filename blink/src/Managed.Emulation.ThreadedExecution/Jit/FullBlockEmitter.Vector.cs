using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Translation-time typed IR and lowering, independently authored. Only the
// AMD64 baseline SSE2 ISA is emitted. Host XMM0/1 and volatile integer scratch
// are used; guest XMM0..15 always live in Machine.xmm. No host MMX state.
internal static partial class FullBlockEmitter
{
    internal enum FullVectorOperation : ushort
    {
        Move = 1, MoveLow64, ScalarToVector, VectorToScalar,
        And, AndNot, Or, Xor,
        AddByte, AddWord, AddDword, AddQword, SubByte, SubWord, SubDword, SubQword,
        AddSignedByte, AddSignedWord, AddUnsignedByte, AddUnsignedWord,
        SubSignedByte, SubSignedWord, SubUnsignedByte, SubUnsignedWord,
        EqualByte, EqualWord, EqualDword, GreaterByte, GreaterWord, GreaterDword,
        MinUnsignedByte, MaxUnsignedByte, MinSignedWord, MaxSignedWord,
        PackSignedWord, PackSignedDword, PackUnsignedWord,
        UnpackLowByte, UnpackLowWord, UnpackLowDword, UnpackLowQword,
        UnpackHighByte, UnpackHighWord, UnpackHighDword, UnpackHighQword,
        MultiplyLowWord, MultiplyHighSignedWord, MultiplyHighUnsignedWord,
        MultiplyUnsignedDword, MultiplyAddWord, SumAbsoluteByte,
        AverageByte, AverageWord,
        ShiftRightWord, ShiftRightDword, ShiftRightQword,
        ShiftArithmeticWord, ShiftArithmeticDword,
        ShiftLeftWord, ShiftLeftDword, ShiftLeftQword,
        ShiftRightBytes, ShiftLeftBytes,
        ShuffleDword, ShuffleLowWord, ShuffleHighWord, ShuffleSingle, ShuffleDouble,
        MaskByte, MaskSingle, MaskDouble, InsertWord, ExtractWord
    }

    // Options[15:0] = enum, bit16 = selected-C aligned memory requirement.
    // Width is always 128 (XMM class), even for GPR transfers. SourceWidth is
    // the memory/scalar width: 16/32/64/128. Extra is imm8 for shuffles/lanes;
    // immediate shifts use Source=imm8. All selection is compile-time only.
    internal const uint VectorAlignedMemory = 1u << 16;
    internal static bool CanInlineVector(JitInstruction i) => i.Operation == JitOperation.VectorInteger;
    internal static FullVectorOperation VectorOperation(JitInstruction i) => (FullVectorOperation)(i.Options & 0xffff);

    private static bool VectorRegister(JitOperand p) => !p.IsImmediate && !p.IsMemory &&
        p.Register is >= 0 and <= 15 && p.ByteOffset == 0 && p.Immediate == 0;
    private static bool VectorMemory(JitOperand p) => p.IsMemory && !p.IsImmediate &&
        p.ByteOffset == 0 && p.Register == 0 && p.Immediate == 0;
    private static bool VectorImmediate(JitOperand p) => p.IsImmediate && !p.IsMemory &&
        p.Register == 0 && p.ByteOffset == 0 && p.Immediate <= 255;
    private static bool VectorShift(FullVectorOperation op) => (uint)op >= (uint)FullVectorOperation.ShiftRightWord && (uint)op <= (uint)FullVectorOperation.ShiftLeftQword;
    private static bool VectorShuffle(FullVectorOperation op) => (uint)op >= (uint)FullVectorOperation.ShuffleDword && (uint)op <= (uint)FullVectorOperation.ShuffleDouble;
    private static bool VectorMask(FullVectorOperation op) => (uint)op >= (uint)FullVectorOperation.MaskByte && (uint)op <= (uint)FullVectorOperation.MaskDouble;

    // False delegates to the scalar validator; invalid vector IR throws. The
    // core calls this BEFORE its scalar width gate, including width128.
    internal static bool ValidateVector(JitInstruction i)
    {
        if (!CanInlineVector(i)) return false;
        var op = VectorOperation(i);
        if (i.Width != 128 || i.Length is < 1 or > 15 || i.Target != 0 || i.Condition != 0 ||
            (i.Options & ~(0xffffu | VectorAlignedMemory)) != 0 ||
            (uint)op < (uint)FullVectorOperation.Move || (uint)op > (uint)FullVectorOperation.ExtractWord)
            throw new ArgumentException("Native vector operation/metadata");
        bool memory = i.Destination.IsMemory || i.Source.IsMemory;
        if (i.Destination.IsMemory && i.Source.IsMemory ||
            !(VectorRegister(i.Destination) || VectorMemory(i.Destination)) ||
            !(VectorRegister(i.Source) || VectorMemory(i.Source) || VectorShift(op) && VectorImmediate(i.Source) ||
                (op is FullVectorOperation.ShiftLeftBytes or FullVectorOperation.ShiftRightBytes) && VectorImmediate(i.Source)))
            throw new ArgumentException("Native vector operand");
        bool extra = VectorShuffle(op) || op is FullVectorOperation.InsertWord or FullVectorOperation.ExtractWord;
        if (extra ? !VectorImmediate(i.Extra) : i.Extra != default)
            throw new ArgumentException("Native vector immediate");
        int width = op is FullVectorOperation.ScalarToVector or FullVectorOperation.VectorToScalar ? i.SourceWidth :
            op == FullVectorOperation.MoveLow64 ? 64 :
            op is FullVectorOperation.InsertWord or FullVectorOperation.ExtractWord ? 16 : 128;
        if (i.SourceWidth != width || width is not (16 or 32 or 64 or 128) ||
            (op is FullVectorOperation.ScalarToVector or FullVectorOperation.VectorToScalar) && width is not (32 or 64))
            throw new ArgumentException("Native vector source width");
        if (i.Destination.IsMemory && op is not (FullVectorOperation.Move or FullVectorOperation.MoveLow64 or FullVectorOperation.VectorToScalar) ||
            op == FullVectorOperation.VectorToScalar && i.Source.IsMemory ||
            VectorMask(op) && memory || op == FullVectorOperation.ExtractWord && memory ||
            (op is FullVectorOperation.ShiftLeftBytes or FullVectorOperation.ShiftRightBytes) && !VectorImmediate(i.Source))
            throw new ArgumentException("Native vector memory/direction");
        if ((i.Options & VectorAlignedMemory) != 0 &&
            (!memory || width != 128 || op is FullVectorOperation.MoveLow64 or
                FullVectorOperation.ScalarToVector or FullVectorOperation.VectorToScalar ||
                VectorShuffle(op) || VectorMask(op)))
            throw new ArgumentException("Native vector alignment metadata");
        if (memory)
        {
            var a = i.Address;
            if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                a.RipRelative && (a.Base != -1 || a.Index != -1))
                throw new ArgumentException("Native vector address");
        }
        else if (i.Address != default) throw new ArgumentException("Native vector unused address");
        if (!VectorLayout.Supported) throw new PlatformNotSupportedException("Translated vector layout differs");
        return true;
    }

    private static class VectorLayout
    {
        internal static readonly bool Supported = HasExpectedVectorLayout();
    }
    internal static unsafe bool HasExpectedVectorLayout() =>
        sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
        sizeof(TranslationBlockHeader) == 160 && sizeof(Blink.Machine) >= 840 &&
        Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.xmm)).ToInt32() == 160 &&
        typeof(Blink.Machine).GetField(nameof(Blink.Machine.xmm))?.GetCustomAttribute<FixedBufferAttribute>() is
            { Length: 256, ElementType: var elementType } && elementType == typeof(byte) &&
        Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.xedd)).ToInt32() == 416 &&
        Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.segvcode)).ToInt32() == 839 &&
        Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.Temporary0)).ToInt32() == 184 &&
        Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.Temporary1)).ToInt32() == 192;

    private ref partial struct Writer
    {
        private bool TryVectorOperation(in JitInstruction i)
        {
            if (!CanInlineVector(i)) return false;
            var op = VectorOperation(i);
            bool memory = i.Destination.IsMemory || i.Source.IsMemory;
            if (memory)
            {
                if ((i.Options & VectorAlignedMemory) != 0) VectorCheckAlignment(i);
                NativeMemoryPrepare(i, i.Destination.IsMemory ? FullMemoryAccess.Write : FullMemoryAccess.Read, i.SourceWidth);
            }
            // Every mapping check precedes any operand write, including small
            // scalar MOVD loads and aliasing destination/source registers.
            if (op == FullVectorOperation.ScalarToVector || op == FullVectorOperation.InsertWord)
            {
                if (i.Source.IsMemory) VectorMemoryLoad(i.SourceWidth);
                else
                {
                    IntegerLoad(i.Source, i.SourceWidth, 0);
                    Bytes(0x66); if (i.SourceWidth == 64) Bytes(0x48);
                    Bytes(0x0f, 0x6e, 0xc8); // xmm1 <- eax/rax, zero upper bits
                }
                if (op == FullVectorOperation.InsertWord)
                {
                    VectorGuestLoad(0, i.Destination.Register);
                    Bytes(0x66, 0x0f, 0x7e, 0xc8); // eax <- low32 xmm1
                    Bytes(0x66, 0x0f, 0xc4, 0xc0, (byte)(i.Extra.Immediate & 7));
                }
                else Bytes(0x66, 0x0f, 0x6f, 0xc1); // xmm0 <- xmm1
            }
            else if (op == FullVectorOperation.VectorToScalar)
            {
                VectorGuestLoad(0, i.Source.Register);
                if (i.Destination.IsMemory) VectorMemoryStore(i.SourceWidth);
                else
                {
                    Bytes(0x66, i.SourceWidth == 64 ? (byte)0x49 : (byte)0x41, 0x0f, 0x7e, 0xc0); // r8 <- xmm0
                    Store(i.Destination, i.SourceWidth);
                }
                if (memory) NativeMemoryCount();
                return true;
            }
            else if (VectorMask(op) || op == FullVectorOperation.ExtractWord)
            {
                VectorGuestLoad(0, i.Source.Register);
                switch (op)
                {
                    case FullVectorOperation.MaskByte: Bytes(0x66, 0x44, 0x0f, 0xd7, 0xc0); break;
                    case FullVectorOperation.MaskSingle: Bytes(0x44, 0x0f, 0x50, 0xc0); break;
                    case FullVectorOperation.MaskDouble: Bytes(0x66, 0x44, 0x0f, 0x50, 0xc0); break;
                    default: Bytes(0x66, 0x44, 0x0f, 0xc5, 0xc0, (byte)(i.Extra.Immediate & 7)); break;
                }
                // Selected C PEXTRW uses Put16, preserving upper GPR bits.
                Store(i.Destination, op == FullVectorOperation.ExtractWord ? 16 : 32);
                return true;
            }
            else if (op is FullVectorOperation.Move or FullVectorOperation.MoveLow64)
            {
                if (i.Source.IsMemory)
                { VectorMemoryLoad(i.SourceWidth); Bytes(0x66, 0x0f, 0x6f, 0xc1); }
                else VectorGuestLoad(0, i.Source.Register);
                if (op == FullVectorOperation.MoveLow64) Bytes(0xf3, 0x0f, 0x7e, 0xc0); // zero upper64, even register form
                if (i.Destination.IsMemory)
                { VectorMemoryStore(i.SourceWidth); NativeMemoryCount(); return true; }
            }
            else
            {
                VectorGuestLoad(0, i.Destination.Register);
                if (i.Source.IsImmediate) VectorImmediateShift(op, (byte)i.Source.Immediate);
                else
                {
                    if (i.Source.IsMemory) VectorMemoryLoad(128);
                    else VectorGuestLoad(1, i.Source.Register);
                    if (VectorShuffle(op))
                    {
                        byte prefix = op == FullVectorOperation.ShuffleLowWord ? (byte)0xf2 :
                            op == FullVectorOperation.ShuffleHighWord ? (byte)0xf3 :
                            op == FullVectorOperation.ShuffleSingle ? (byte)0 : (byte)0x66;
                        if (prefix != 0) Bytes(prefix);
                        Bytes(0x0f, op is FullVectorOperation.ShuffleSingle or FullVectorOperation.ShuffleDouble ? (byte)0xc6 : (byte)0x70,
                            0xc1, (byte)i.Extra.Immediate);
                    }
                    else if (VectorShift(op) && !memory && i.Source.Register == i.Destination.Register)
                    {
                        // Selected portable C runs two 64-bit kernels. With
                        // x==y the second kernel rereads the changed low64 as
                        // its count. Preserve that observable source behavior.
                        Bytes(0x66, 0x0f, 0x6f, 0xd0); // xmm2 = original
                        Bytes(0x66, 0x0f, 0x73, 0xda, 8); // psrldq xmm2,8
                        byte opcode = VectorBinaryOpcode(op);
                        Bytes(0x66, 0x0f, opcode, 0xc1); // low half with original count
                        Bytes(0xf3, 0x0f, 0x7e, 0xc8); // xmm1 = changed low64
                        Bytes(0x66, 0x0f, opcode, 0xd1); // original high with reread count
                        Bytes(0x66, 0x0f, 0x6c, 0xc2); // join low halves
                    }
                    else { Bytes(0x66, 0x0f, VectorBinaryOpcode(op), 0xc1); }
                }
            }
            VectorGuestStore(0, i.Destination.Register);
            if (memory) NativeMemoryCount();
            return true;
        }

        private static byte VectorBinaryOpcode(FullVectorOperation op) => op switch
        {
            FullVectorOperation.And => 0xdb, FullVectorOperation.AndNot => 0xdf,
            FullVectorOperation.Or => 0xeb, FullVectorOperation.Xor => 0xef,
            FullVectorOperation.AddByte => 0xfc, FullVectorOperation.AddWord => 0xfd,
            FullVectorOperation.AddDword => 0xfe, FullVectorOperation.AddQword => 0xd4,
            FullVectorOperation.SubByte => 0xf8, FullVectorOperation.SubWord => 0xf9,
            FullVectorOperation.SubDword => 0xfa, FullVectorOperation.SubQword => 0xfb,
            FullVectorOperation.AddSignedByte => 0xec, FullVectorOperation.AddSignedWord => 0xed,
            FullVectorOperation.AddUnsignedByte => 0xdc, FullVectorOperation.AddUnsignedWord => 0xdd,
            FullVectorOperation.SubSignedByte => 0xe8, FullVectorOperation.SubSignedWord => 0xe9,
            FullVectorOperation.SubUnsignedByte => 0xd8, FullVectorOperation.SubUnsignedWord => 0xd9,
            FullVectorOperation.EqualByte => 0x74, FullVectorOperation.EqualWord => 0x75, FullVectorOperation.EqualDword => 0x76,
            FullVectorOperation.GreaterByte => 0x64, FullVectorOperation.GreaterWord => 0x65, FullVectorOperation.GreaterDword => 0x66,
            FullVectorOperation.MinUnsignedByte => 0xda, FullVectorOperation.MaxUnsignedByte => 0xde,
            FullVectorOperation.MinSignedWord => 0xea, FullVectorOperation.MaxSignedWord => 0xee,
            FullVectorOperation.PackSignedWord => 0x63, FullVectorOperation.PackSignedDword => 0x6b, FullVectorOperation.PackUnsignedWord => 0x67,
            FullVectorOperation.UnpackLowByte => 0x60, FullVectorOperation.UnpackLowWord => 0x61,
            FullVectorOperation.UnpackLowDword => 0x62, FullVectorOperation.UnpackLowQword => 0x6c,
            FullVectorOperation.UnpackHighByte => 0x68, FullVectorOperation.UnpackHighWord => 0x69,
            FullVectorOperation.UnpackHighDword => 0x6a, FullVectorOperation.UnpackHighQword => 0x6d,
            FullVectorOperation.MultiplyLowWord => 0xd5, FullVectorOperation.MultiplyHighSignedWord => 0xe5,
            FullVectorOperation.MultiplyHighUnsignedWord => 0xe4, FullVectorOperation.MultiplyUnsignedDword => 0xf4,
            FullVectorOperation.MultiplyAddWord => 0xf5, FullVectorOperation.SumAbsoluteByte => 0xf6,
            FullVectorOperation.AverageByte => 0xe0, FullVectorOperation.AverageWord => 0xe3,
            FullVectorOperation.ShiftRightWord => 0xd1, FullVectorOperation.ShiftRightDword => 0xd2, FullVectorOperation.ShiftRightQword => 0xd3,
            FullVectorOperation.ShiftArithmeticWord => 0xe1, FullVectorOperation.ShiftArithmeticDword => 0xe2,
            FullVectorOperation.ShiftLeftWord => 0xf1, FullVectorOperation.ShiftLeftDword => 0xf2, FullVectorOperation.ShiftLeftQword => 0xf3,
            _ => throw new ArgumentException("Native vector binary operation")
        };

        private void VectorImmediateShift(FullVectorOperation op, byte count)
        {
            byte code = op is FullVectorOperation.ShiftRightWord or FullVectorOperation.ShiftArithmeticWord or FullVectorOperation.ShiftLeftWord ? (byte)0x71 :
                op is FullVectorOperation.ShiftRightDword or FullVectorOperation.ShiftArithmeticDword or FullVectorOperation.ShiftLeftDword ? (byte)0x72 : (byte)0x73;
            int group = op is FullVectorOperation.ShiftArithmeticWord or FullVectorOperation.ShiftArithmeticDword ? 4 :
                op == FullVectorOperation.ShiftRightBytes ? 3 : op == FullVectorOperation.ShiftLeftBytes ? 7 :
                op is FullVectorOperation.ShiftLeftWord or FullVectorOperation.ShiftLeftDword or FullVectorOperation.ShiftLeftQword ? 6 : 2;
            Bytes(0x66, 0x0f, code, (byte)(0xc0 | group << 3), count);
        }

        private void VectorTransfer(byte prefix, byte opcode, int xmm, int hostBase, int offset)
        {
            Bytes(prefix, (byte)(0x40 | (hostBase >> 3)), 0x0f, opcode);
            Memory(xmm, hostBase & 7, offset);
        }
        private void VectorGuestLoad(int xmm, int guest) => VectorTransfer(0xf3, 0x6f, xmm, 11, 160 + guest * 16);
        private void VectorGuestStore(int xmm, int guest) => VectorTransfer(0xf3, 0x7f, xmm, 11, 160 + guest * 16);

        private void VectorCheckAlignment(JitInstruction i)
        {
            NativeMemoryAddress(i); // full EA32/FS/GS ordering from R61
            Bytes(0xa8, 15); int aligned = NearCondition(0x84);
            Bytes(0x49, 0x89, 0x82); U32(176); // EA, no machine fault-state mutation in leaf
            Bytes(0x41, 0xc7, 0x82); U32(168); U32(14);
            Bytes(0x41, 0x0f, 0xbe, 0x83); U32(839); // selected ThrowSegmentationFault preserves segvcode
            Bytes(0x41, 0x89, 0x82); U32(172);
            JumpExit(FullBlockExit.GuestFault);
            Patch(aligned, position);
        }

        // Prepare has authenticated BOTH mappings. A single page uses actual
        // MOVDQU/MOVQ/MOVD; crossing gathers into invocation scratch184..199.
        // No scalar NativeMemoryLoad/Store is called with width128.
        private void VectorMemoryLoad(int width)
        {
            Bytes(0x49, 0x83, 0xba); U32(232); Bytes(0);
            int crossing = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(224);
            if (width == 128) Bytes(0xf3, 0x0f, 0x6f, 0x0a);
            else if (width == 64) Bytes(0xf3, 0x0f, 0x7e, 0x0a);
            else if (width == 32) Bytes(0x66, 0x0f, 0x6e, 0x0a);
            else { Bytes(0x0f, 0xb7, 0x02, 0x66, 0x0f, 0x6e, 0xc8); }
            int done = NativeNearJump();
            Patch(crossing, position);
            Bytes(0x49, 0xc7, 0x82); U32(184); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(192); U32(0);
            for (int b = 0; b < width / 8; ++b)
            {
                NativeBytePointer(b);
                Bytes(0x8a, 0x02, 0x41, 0x88, 0x82); U32((uint)(184 + b));
            }
            VectorTransfer(0xf3, 0x6f, 1, 10, 184);
            Patch(done, position);
        }

        private void VectorMemoryStore(int width)
        {
            Bytes(0x49, 0x83, 0xba); U32(232); Bytes(0);
            int crossing = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(224);
            if (width == 128) Bytes(0xf3, 0x0f, 0x7f, 0x02);
            else if (width == 64) Bytes(0x66, 0x0f, 0xd6, 0x02);
            else if (width == 32) Bytes(0x66, 0x0f, 0x7e, 0x02);
            else { Bytes(0x66, 0x0f, 0x7e, 0xc0, 0x66, 0x89, 0x02); }
            int done = NativeNearJump();
            Patch(crossing, position);
            VectorTransfer(0xf3, 0x7f, 0, 10, 184);
            for (int b = 0; b < width / 8; ++b)
            {
                NativeBytePointer(b);
                Bytes(0x41, 0x8a, 0x82); U32((uint)(184 + b)); Bytes(0x88, 0x02);
            }
            Patch(done, position);
        }
    }
}
