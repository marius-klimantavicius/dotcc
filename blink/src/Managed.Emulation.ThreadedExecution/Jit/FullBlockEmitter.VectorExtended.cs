namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    // Distinct low16 namespace under VectorInteger; dispatch BEFORE R66.
    // All operations use Width128/SourceWidth128, XMM <- XMM/m128. The final
    // two use Extra=imm8. Memory always requires architectural alignment.
    internal enum FullVectorExtendedOperation : ushort
    {
        ShuffleByte = 0x2001, HorizontalAddWord, HorizontalAddDword, HorizontalAddSignedWord,
        MultiplyAddUnsignedSignedByte, HorizontalSubWord, HorizontalSubDword, HorizontalSubSignedWord,
        SignByte, SignWord, SignDword, MultiplyRoundedSignedWord,
        AbsoluteByte, AbsoluteWord, AbsoluteDword, MultiplyLowDword, AlignRightBytes, CarrylessMultiplyQword
    }

    internal static bool CanInlineVectorExtended(JitInstruction i) =>
        i.Operation == JitOperation.VectorInteger && (i.Options & 0xf000) == 0x2000;
    internal static FullVectorExtendedOperation VectorExtendedOperation(JitInstruction i) =>
        (FullVectorExtendedOperation)(i.Options & 0xffff);

    internal static bool ValidateVectorExtended(JitInstruction i)
    {
        if (!CanInlineVectorExtended(i)) return false;
        var op = VectorExtendedOperation(i);
        if (i.Width != 128 || i.SourceWidth != 128 || i.Length is < 1 or > 15 ||
            i.Target != 0 || i.Condition != 0 || (i.Options & ~(0xffffu | VectorAlignedMemory)) != 0 ||
            (uint)op < (uint)FullVectorExtendedOperation.ShuffleByte || (uint)op > (uint)FullVectorExtendedOperation.CarrylessMultiplyQword)
            throw new ArgumentException("Native vector extended operation/metadata");
        if (!VectorRegister(i.Destination) || !(VectorRegister(i.Source) || VectorMemory(i.Source)))
            throw new ArgumentException("Native vector extended operand");
        bool immediate = op is FullVectorExtendedOperation.AlignRightBytes or FullVectorExtendedOperation.CarrylessMultiplyQword;
        if (immediate ? !VectorImmediate(i.Extra) : i.Extra != default)
            throw new ArgumentException("Native vector extended immediate");
        if (((i.Options & VectorAlignedMemory) != 0) != i.Source.IsMemory)
            throw new ArgumentException("Native vector extended alignment metadata");
        if (i.Source.IsMemory)
        {
            var a = i.Address;
            if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                a.RipRelative && (a.Base != -1 || a.Index != -1))
                throw new ArgumentException("Native vector extended address");
        }
        else if (i.Address != default) throw new ArgumentException("Native vector extended unused address");
        if (!VectorLayout.Supported) throw new PlatformNotSupportedException("Translated vector layout differs");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryVectorExtendedOperation(in JitInstruction i)
        {
            if (!CanInlineVectorExtended(i)) return false;
            var op = VectorExtendedOperation(i);
            if (i.Source.IsMemory)
            {
                ScalarMiscCheckCanonical(i,128);
                // Common R79 #GP(0) convention; both mapping checks precede
                // any input data read or guest destination effect. A valid
                // aligned m128 never straddles a 4K guest page. No speculative
                // zero-result or unused-lane shortcut can skip the m128 read.
                VectorMoveCheckAlignment(i);
                NativeMemoryPrepare(i, FullMemoryAccess.Read, 128);
                VectorMemoryLoad(128);
            }
            else VectorGuestLoad(1, i.Source.Register);
            VectorGuestLoad(0, i.Destination.Register);
            Bytes(0x66, 0x0f, 0xef, 0xe4); // pxor xmm4,xmm4: output accumulator
            // XMM0/1 are immutable input snapshots, XMM2 extraction, XMM3
            // insertion, XMM4 output. XMM0..4 and RAX/RCX/RDX/R8/R9 are volatile
            // on both supported ABIs. No stack, calls, host optional ISA or
            // guest flag/MXCSR writes. Runtime arithmetic is generated below.
            switch (op)
            {
                case FullVectorExtendedOperation.ShuffleByte: VectorExtendedShuffle(); break;
                case FullVectorExtendedOperation.HorizontalAddWord:
                case FullVectorExtendedOperation.HorizontalAddDword:
                case FullVectorExtendedOperation.HorizontalAddSignedWord:
                case FullVectorExtendedOperation.HorizontalSubWord:
                case FullVectorExtendedOperation.HorizontalSubDword:
                case FullVectorExtendedOperation.HorizontalSubSignedWord:
                    VectorExtendedHorizontal(op); break;
                case FullVectorExtendedOperation.MultiplyAddUnsignedSignedByte:
                    VectorExtendedMultiplyAdd(); break;
                case FullVectorExtendedOperation.SignByte:
                case FullVectorExtendedOperation.SignWord:
                case FullVectorExtendedOperation.SignDword:
                case FullVectorExtendedOperation.AbsoluteByte:
                case FullVectorExtendedOperation.AbsoluteWord:
                case FullVectorExtendedOperation.AbsoluteDword:
                    VectorExtendedSignOrAbsolute(op); break;
                case FullVectorExtendedOperation.MultiplyRoundedSignedWord:
                case FullVectorExtendedOperation.MultiplyLowDword:
                    VectorExtendedMultiply(op); break;
                case FullVectorExtendedOperation.AlignRightBytes:
                    VectorExtendedAlign((byte)i.Extra.Immediate); break;
                case FullVectorExtendedOperation.CarrylessMultiplyQword:
                    VectorExtendedCarryless((byte)i.Extra.Immediate); break;
                default: throw new ArgumentException("Native vector extended emission operation");
            }
            VectorGuestStore(4, i.Destination.Register);
            if (i.Source.IsMemory) NativeMemoryCount();
            return true;
        }

        // Extract from a snapshot without consuming it. MOVD/Q zeros all
        // unwanted upper bits; explicit signed extraction is only for math.
        private void VectorExtendedExtract(int xmm, int offset, int width, int register, bool signed = false)
        {
            Bytes(0x66, 0x0f, 0x6f, (byte)(0xd0 | xmm));
            if (offset != 0) Bytes(0x66, 0x0f, 0x73, 0xda, (byte)offset);
            Bytes(0x66);
            int rex = 0x40 | (width == 64 ? 8 : 0) | (register >> 3);
            if (rex != 0x40) Bytes((byte)rex);
            Bytes(0x0f, 0x7e, (byte)(0xd0 | (register & 7)));
            if (signed && width < 32)
            { IntegerShift(4, register, 32, 32 - width); IntegerShift(7, register, 32, 32 - width); }
            else if (width < 32) IntegerTruncate(register, width);
        }

        // Insert EAX/RAX at a byte offset in the zeroed XMM4 accumulator.
        // Every lane owns disjoint bits; the final MOVDQU is the sole guest write.
        private void VectorExtendedAppend(int offset, int width)
        {
            if (width < 32) IntegerTruncate(0, width);
            Bytes(0x66); if (width == 64) Bytes(0x48);
            Bytes(0x0f, 0x6e, 0xd8); // xmm3 <- eax/rax, upper bits zero
            if (offset != 0) Bytes(0x66, 0x0f, 0x73, 0xfb, (byte)offset);
            Bytes(0x66, 0x0f, 0xeb, 0xe3); // por xmm4,xmm3
        }

        private void VectorExtendedShuffle()
        {
            // Bounded 16-byte table in invocation scratch, never guest state.
            // Memory gathering has already finished using these same slots.
            VectorTransfer(0xf3, 0x7f, 0, 10, 184);
            for (int b = 0; b < 16; ++b)
            {
                VectorExtendedExtract(1, b, 8, 0);
                IntegerMov(1, 0, 32); IntegerConstant(4, 0, 15);
                Bytes(0x41, 0x0f, 0xb6, 0x84, 0x02); U32(184); // eax=[r10+rax+184], index0..15
                IntegerShift(5, 1, 32, 7); IntegerConstant(5, 1, 1); // bit7 ? 0 : -1
                IntegerRR(0x21, 0, 1, 32);
                VectorExtendedAppend(b, 8);
            }
        }

        private void VectorExtendedHorizontal(FullVectorExtendedOperation op)
        {
            bool dword = op is FullVectorExtendedOperation.HorizontalAddDword or FullVectorExtendedOperation.HorizontalSubDword;
            bool saturated = op is FullVectorExtendedOperation.HorizontalAddSignedWord or FullVectorExtendedOperation.HorizontalSubSignedWord;
            bool subtract = op is FullVectorExtendedOperation.HorizontalSubWord or FullVectorExtendedOperation.HorizontalSubDword or FullVectorExtendedOperation.HorizontalSubSignedWord;
            int width = dword ? 32 : 16, step = width / 8, half = 64 / width;
            for (int lane = 0; lane < 128 / width; ++lane)
            {
                int source = lane / half, pair = lane % half * step * 2;
                VectorExtendedExtract(source, pair, width, 0, saturated);
                VectorExtendedExtract(source, pair + step, width, 1, saturated);
                IntegerRR(subtract ? (byte)0x29 : (byte)0x01, 0, 1, 32);
                if (saturated) VectorExtendedSaturateWord();
                VectorExtendedAppend(lane * step, width);
            }
        }

        private void VectorExtendedSaturateWord()
        {
            // Signed clamp in 32-bit intermediates; CMOV is baseline x64.
            IntegerImmediate(2, 32767, 32); IntegerRR(0x39, 0, 2, 32);
            Bytes(0x0f, 0x4f, 0xc2); // cmovg eax,edx
            IntegerImmediate(2, 0xffff8000, 32); IntegerRR(0x39, 0, 2, 32);
            Bytes(0x0f, 0x4c, 0xc2); // cmovl eax,edx
        }

        private void VectorExtendedMultiplyAdd()
        {
            for (int lane = 0; lane < 8; ++lane)
            {
                VectorExtendedExtract(0, lane * 2, 8, 0);
                VectorExtendedExtract(1, lane * 2, 8, 1, true);
                Bytes(0x0f, 0xaf, 0xc1); IntegerMov(8, 0, 32); // first product
                VectorExtendedExtract(0, lane * 2 + 1, 8, 0);
                VectorExtendedExtract(1, lane * 2 + 1, 8, 1, true);
                Bytes(0x0f, 0xaf, 0xc1); IntegerRR(0x01, 0, 8, 32);
                VectorExtendedSaturateWord(); VectorExtendedAppend(lane * 2, 16);
            }
        }

        private void VectorExtendedSignOrAbsolute(FullVectorExtendedOperation op)
        {
            bool absolute = op is FullVectorExtendedOperation.AbsoluteByte or FullVectorExtendedOperation.AbsoluteWord or FullVectorExtendedOperation.AbsoluteDword;
            int width = op is FullVectorExtendedOperation.SignByte or FullVectorExtendedOperation.AbsoluteByte ? 8 :
                op is FullVectorExtendedOperation.SignWord or FullVectorExtendedOperation.AbsoluteWord ? 16 : 32;
            for (int b = 0; b < 16; b += width / 8)
            {
                VectorExtendedExtract(absolute ? 1 : 0, b, width, 0, absolute);
                if (absolute) IntegerMov(1, 0, 32);
                else VectorExtendedExtract(1, b, width, 1, true);
                IntegerMov(2, 1, 32); IntegerShift(7, 2, 32, 31);
                IntegerRR(0x31, 0, 2, 32); IntegerRR(0x29, 0, 2, 32);
                if (!absolute)
                {
                    IntegerRR(0x85, 1, 1, 32);
                    Bytes(0x0f, 0x95, 0xc1, 0x0f, 0xb6, 0xc9); // ecx=(signed-control != 0)
                    IntegerUnary(3, 1, 32); IntegerRR(0x21, 0, 1, 32);
                }
                // INT_MIN absolute/negation wraps at lane width, never clamps.
                VectorExtendedAppend(b, width);
            }
        }

        private void VectorExtendedMultiply(FullVectorExtendedOperation op)
        {
            bool rounded = op == FullVectorExtendedOperation.MultiplyRoundedSignedWord;
            int width = rounded ? 16 : 32;
            for (int b = 0; b < 16; b += width / 8)
            {
                VectorExtendedExtract(0, b, width, 0, rounded);
                VectorExtendedExtract(1, b, width, 1, rounded);
                Bytes(0x0f, 0xaf, 0xc1);
                if (rounded)
                {
                    IntegerConstant(0, 0, 0x4000); IntegerShift(7, 0, 32, 15);
                    // -32768*-32768 rounds to 0x8000; PMULHRSW does NOT saturate.
                }
                VectorExtendedAppend(b, width);
            }
        }

        private void VectorExtendedAlign(byte count)
        {
            // Even a constant zero result has already read/prechecked m128.
            for (int b = 0; b < 16; ++b)
            {
                int at = count + b;
                if (at >= 32) continue;
                VectorExtendedExtract(at < 16 ? 1 : 0, at & 15, 8, 0);
                VectorExtendedAppend(b, 8);
            }
        }

        private void VectorExtendedCarryless(byte control)
        {
            VectorExtendedExtract(0, (control & 1) * 8, 64, 0);
            VectorExtendedExtract(1, (control >> 4 & 1) * 8, 64, 1);
            IntegerRR(0x31, 2, 2, 32); IntegerRR(0x31, 8, 8, 32); IntegerRR(0x31, 9, 9, 32);
            // Generated, fixed-bound binary polynomial multiplication. Each
            // selected bit XORs a 128-bit shifted multiplicand; there is no
            // integer carry and no runtime helper or optional PCLMUL opcode.
            for (int bit = 0; bit < 64; ++bit)
            {
                Bytes(0xf6, 0xc1, 1); int clear = NearCondition(0x84); // test cl,1
                IntegerRR(0x31, 8, 0); IntegerRR(0x31, 9, 2);
                Patch(clear, position);
                if (bit == 63) continue;
                Bytes(0x48, 0x0f, 0xa4, 0xc2, 1); // shld rdx,rax,1
                IntegerShift(4, 0, 64, 1); IntegerShift(5, 1, 64, 1);
            }
            IntegerMov(0, 8); VectorExtendedAppend(0, 64);
            IntegerMov(0, 9); VectorExtendedAppend(8, 64);
        }
    }
}
