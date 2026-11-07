namespace Managed.Emulation.Execution;

// Scalar aligned private RW mappings use baseline LOCK CMPXCHG. Every other
// form retains the mapping-only exclusive ticket and its existing GPR semantics.
// Admission must also exclude semantic/backing readers (R183 authority seam).
internal static partial class FullBlockEmitter
{
    internal static bool CanInlineAtomic(JitInstruction i) => i.Operation == JitOperation.AtomicMemory;
    internal static FullAtomicOperation AtomicOperation(JitInstruction i) =>
        (FullAtomicOperation)(i.Options & FullAtomicLowering.OperationMask);

    private static bool AtomicBit(FullAtomicOperation op) => op is
        FullAtomicOperation.BitSet or FullAtomicOperation.BitReset or FullAtomicOperation.BitComplement;
    private static bool AtomicUnary(FullAtomicOperation op) => op is
        FullAtomicOperation.Inc or FullAtomicOperation.Dec or FullAtomicOperation.Neg or FullAtomicOperation.Not;

    // Core calls this before scalar validation (pair CAS can have width128).
    internal static bool ValidateAtomic(JitInstruction i)
    {
        if (!CanInlineAtomic(i)) return false;
        var op = AtomicOperation(i);
        if ((uint)op < (uint)FullAtomicOperation.Exchange || (uint)op > (uint)FullAtomicOperation.BitComplement ||
            (i.Options & ~(FullAtomicLowering.OperationMask | FullAtomicLowering.Locked)) != 0 ||
            i.Length is < 1 or > 15 || i.Target != 0 || i.Condition != 0 || i.Extra != default ||
            i.SourceWidth != i.Width || i.Destination != new JitOperand(0, 0, IsMemory: true))
            throw new ArgumentException("Native atomic operation/metadata");
        var a = i.Address;
        if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
            a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
            a.RipRelative && (a.Base != -1 || a.Index != -1))
            throw new ArgumentException("Native atomic address");
        if (op == FullAtomicOperation.CompareExchangePair)
        {
            if (i.Width is not (64 or 128) || i.Source != default)
                throw new ArgumentException("Native atomic pair CAS");
            return true;
        }
        if (i.Width is not (8 or 16 or 32 or 64) || AtomicBit(op) && i.Width == 8)
            throw new ArgumentException("Native atomic width");
        bool exchange = op is FullAtomicOperation.Exchange or FullAtomicOperation.ExchangeAdd or FullAtomicOperation.CompareExchange;
        if (!exchange && (i.Options & FullAtomicLowering.Locked) == 0)
            throw new ArgumentException("Native atomic RMW requires LOCK");
        if (AtomicUnary(op))
        {
            if (i.Source != default) throw new ArgumentException("Native atomic unused source");
        }
        else if (!IntegerRegister(i.Source, i.Width, !exchange) ||
            i.Source.IsImmediate && (i.Source.Register != 0 || i.Source.ByteOffset != 0 || AtomicBit(op) && i.Source.Immediate > 255) ||
            !i.Source.IsImmediate && i.Source.Immediate != 0)
            throw new ArgumentException("Native atomic source");
        return true;
    }

    private ref partial struct Writer
    {
        // NativeMemoryAddress calls `if (AtomicBitAddress(i)) return;` after
        // Address(i,false). Apply bitdisp and EA32 truncation to the offset
        // BEFORE FS/GS; the selected C's final segmented-address mask is a bug.
        private bool AtomicBitAddress(JitInstruction i)
        {
            if (!CanInlineAtomic(i) || !AtomicBit(AtomicOperation(i))) return false;
            if (!i.Source.IsImmediate)
            {
                IntegerLoad(i.Source, i.Width, 1); IntegerSignExtend(1, i.Width);
                IntegerConstant(4, 1, unchecked((uint)-i.Width), 64);
                IntegerShift(7, 1, 64, 3); IntegerRR(0x01, 0, 1);
            }
            if (i.Address.AddressWidth == 32) IntegerMov(0, 0, 32);
            if (i.Address.Segment is 4 or 5)
            { Bytes(0x49, 0x03, 0x82); U32(i.Address.Segment == 4 ? 328u : 336u); }
            return true;
        }

        private bool TryAtomicOperation(in JitInstruction i)
        {
            if (!CanInlineAtomic(i)) return false;
            ScalarMiscCheckCanonical(i, i.Width);
            if (i.Width == 128 && AtomicOperation(i) == FullAtomicOperation.CompareExchangePair)
                AtomicPairAlignment(i);
            NativeMemoryPrepare(i, FullNativeMemory.AtomicReadWrite, i.Width, atomicFast: ScalarAtomicFast(i));
            Bytes(0x41, 0x83, 0xba); U32(324); Bytes((byte)FullMemoryFlags.BarrierConsumed);
            int slow = NearCondition(0x84);
            if (ScalarAtomicFast(i))
            {
                AtomicFast(i);
                int done = NativeNearJump();
                Patch(slow, position);
                AtomicTicketOperation(i);
                Patch(done, position);
            }
            else
            {
                // Pair forms can only arrive with the consumed exclusive ticket.
                int rejected = NativeNearJump();
                Patch(slow, position);
                AtomicTicketOperation(i);
                int done = NativeNearJump();
                Patch(rejected, position);
                Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
                JumpExit(FullBlockExit.NeedMemory);
                Patch(done, position);
            }
            NativeMemoryCount();
            return true;
        }

        private void AtomicTicketOperation(JitInstruction i)
        {
            var op = AtomicOperation(i);
            if (op == FullAtomicOperation.CompareExchangePair) AtomicPair(i);
            else
            {
                NativeMemoryLoad(i.Width);
                if (op == FullAtomicOperation.Exchange)
                {
                    IntegerFrameStore(184, 0);
                    IntegerLoad(i.Source, i.Width, 8); NativeMemoryStore(i.Width);
                    IntegerFrameLoad(184, 8); Store(i.Source, i.Width);
                }
                else if (op == FullAtomicOperation.CompareExchange) AtomicCompareExchange(i);
                else if (AtomicBit(op)) AtomicBitOperation(i);
                else AtomicArithmetic(i);
            }
        }

        private void AtomicArithmetic(JitInstruction i, bool storeMemory = true, bool publish = true)
        {
            var op = AtomicOperation(i); int w = i.Width;
            if (op == FullAtomicOperation.Not)
            {
                IntegerUnary(2, 0, w); IntegerMov(8, 0); IntegerTruncate(8, w);
                if (storeMemory) NativeMemoryStore(w); return;
            }
            if (op == FullAtomicOperation.ExchangeAdd) IntegerFrameStore(184, 0);
            if (op is FullAtomicOperation.Inc or FullAtomicOperation.Dec) IntegerImmediate(1, 1, 32);
            else if (op == FullAtomicOperation.Neg) IntegerImmediate(1, 0, 32);
            else IntegerLoad(i.Source, w, 1);
            IntegerMov(9, 0); IntegerRR(0x31, 9, 1);
            if (op is FullAtomicOperation.Adc or FullAtomicOperation.Sbb) Bytes(0x41, 0x0f, 0xba, 0x63, 12, 0);
            byte opcode = op switch
            {
                FullAtomicOperation.Add or FullAtomicOperation.Inc or FullAtomicOperation.ExchangeAdd => 0x01,
                FullAtomicOperation.Or => 0x09, FullAtomicOperation.Adc => 0x11, FullAtomicOperation.Sbb => 0x19,
                FullAtomicOperation.And => 0x21, FullAtomicOperation.Sub or FullAtomicOperation.Dec => 0x29,
                FullAtomicOperation.Xor => 0x31, FullAtomicOperation.Neg => 0,
                _ => throw new ArgumentException("Native atomic arithmetic")
            };
            if (op == FullAtomicOperation.Neg) IntegerUnary(3, 0, w);
            else IntegerRR(w == 8 ? (byte)(opcode - 1) : opcode, 0, 1, w);
            IntegerMov(8, 0);
            if (publish) IntegerArithmeticFlags(op is FullAtomicOperation.Inc or FullAtomicOperation.Dec,
                op is FullAtomicOperation.And or FullAtomicOperation.Or or FullAtomicOperation.Xor);
            IntegerTruncate(8, w);
            if (storeMemory) NativeMemoryStore(w);
            if (publish && op == FullAtomicOperation.ExchangeAdd)
            { IntegerFrameLoad(184, 8); Store(i.Source, w); }
        }

        private void AtomicCompareExchange(JitInstruction i, bool storeMemory = true)
        {
            int w = i.Width;
            IntegerFrameStore(184, 0); // observed memory, survives flag capture
            IntegerLoad(i.Source, w, 1); IntegerFrameStore(192, 1);
            IntegerMov(1, 0); IntegerLoad(new(0, 0), w, 0);
            IntegerMov(9, 0); IntegerRR(0x31, 9, 1);
            IntegerRR(w == 8 ? (byte)0x28 : (byte)0x29, 0, 1, w);
            IntegerMov(8, 0); IntegerArithmeticFlags(false, false);
            Bytes(0x41, 0xf6, 0x43, 12, 0x40); int equal = NearCondition(0x85);
            IntegerFrameLoad(184, 8); Store(new(0, 0), w);
            int done = NativeNearJump();
            Patch(equal, position);
            if (storeMemory) { IntegerFrameLoad(192, 8); NativeMemoryStore(w); }
            // Success leaves the accumulator intact, including upper32.
            // Failure writes width32 through Store and clears its upper32.
            Patch(done, position);
        }

        private void AtomicBitOperation(JitInstruction i, bool storeMemory = true, bool publish = true)
        {
            IntegerLoad(i.Source, i.Width, 1);
            IntegerConstant(4, 1, (uint)(i.Width - 1));
            IntegerPrefix(i.Width, 0, 1);
            byte opcode = AtomicOperation(i) switch
            {
                FullAtomicOperation.BitSet => 0xab,
                FullAtomicOperation.BitReset => 0xb3,
                _ => 0xbb
            };
            Bytes(0x0f, opcode, 0xc8); // register bit RMW, memory was checked
            IntegerMov(8, 0);
            if (publish)
            {
                Bytes(0x0f, 0x92, 0xc0, 0x0f, 0xb6, 0xc0);
                IntegerMergeFlags(1); // preserve all undefined stored flags
            }
            IntegerTruncate(8, i.Width);
            if (storeMemory) NativeMemoryStore(i.Width);
        }

        // Canonical EA/#SS or #GP is checked first. CMPXCHG16B then requires
        // 16-byte alignment even when comparison would fail. This #GP precedes
        // Prepare, TLB/MMU, ticket consumption and every architectural effect.
        // CMPXCHG8B deliberately has no corresponding alignment restriction.
        private void AtomicPairAlignment(JitInstruction i)
        {
            NativeMemoryAddress(i);
            Bytes(0x48, 0xa9); U32(15); // TEST final linear RAX,15
            int aligned = NearCondition(0x84);
            Bytes(0x41, 0xc7, 0x82); U32(168); U32(13);
            Bytes(0x41, 0xc7, 0x82); U32(172); U32(128); // Linux SI_KERNEL; architectural #GP(0)
            Bytes(0x49, 0xc7, 0x82); U32(176); U32(0);
            JumpExit(FullBlockExit.GuestFault);
            Patch(aligned, position);
        }

        // Portable architectural pair CAS: compare both halves before stores.
        // Host CX16 is optional; baseline integer instructions plus proved
        // exclusion implement both widths. The selected unaligned16B success
        // behavior is an explicit oracle defect, never native fault authority.
        private void AtomicPair(JitInstruction i)
        {
            int half = i.Width / 2;
            AtomicLoadPart(0, half); IntegerFrameStore(184, 0);
            AtomicLoadPart(half / 8, half); IntegerFrameStore(192, 0);
            IntegerLoad(new(0, 0), half, 0); IntegerFrameLoad(184, 1);
            IntegerRR(0x39, 0, 1, half); int lowFailed = NearCondition(0x85);
            IntegerLoad(new(2, 0), half, 0); IntegerFrameLoad(192, 1);
            IntegerRR(0x39, 0, 1, half); int highFailed = NearCondition(0x85);
            IntegerImmediate(0, 64, 32); IntegerMergeFlags(64);
            IntegerLoad(new(3, 0), half, 8); AtomicStorePart(0, half);
            IntegerLoad(new(1, 0), half, 8); AtomicStorePart(half / 8, half);
            int done = NativeNearJump();
            Patch(lowFailed, position); Patch(highFailed, position);
            IntegerImmediate(0, 0, 32); IntegerMergeFlags(64);
            IntegerFrameLoad(184, 8); Store(new(0, 0), half);
            IntegerFrameLoad(192, 8); Store(new(2, 0), half);
            Patch(done, position);
        }

        private void AtomicLoadPart(int index, int width)
        {
            IntegerImmediate(9, 0, 32);
            for (int b = 0; b < width / 8; ++b)
            {
                NativeBytePointer(index + b);
                Bytes(0x0f, 0xb6, 0x02);
                if (b != 0) IntegerShift(4, 0, 64, b * 8);
                IntegerRR(0x09, 9, 0);
            }
            IntegerMov(0, 9);
        }

        private void AtomicStorePart(int index, int width)
        {
            for (int b = 0; b < width / 8; ++b)
            {
                NativeBytePointer(index + b); IntegerMov(0, 8);
                if (b != 0) IntegerShift(5, 0, 64, b * 8);
                Bytes(0x88, 0x02);
            }
        }
    }
}
