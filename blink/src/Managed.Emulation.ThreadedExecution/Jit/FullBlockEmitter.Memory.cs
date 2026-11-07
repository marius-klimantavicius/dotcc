namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool IsNativeMemoryControl(JitInstruction i) => i.Operation is
        JitOperation.Call or JitOperation.Ret or JitOperation.ReturnAdjust or
        JitOperation.IndirectJump or JitOperation.IndirectCall;

    private static bool NativeMemoryAlu(JitInstruction i) => (i.Destination.IsMemory || i.Source.IsMemory) &&
        i.Operation is (JitOperation.Add or JitOperation.Adc or JitOperation.Sub or JitOperation.Sbb or
        JitOperation.And or JitOperation.Or or JitOperation.Xor or JitOperation.Cmp or JitOperation.Test or
        JitOperation.Inc or JitOperation.Dec or JitOperation.Neg or JitOperation.Not or
        JitOperation.ConditionalMove or JitOperation.SetCondition);

    internal static bool CanInlineMemory(JitInstruction i) => IsNativeMemoryControl(i) || NativeMemoryAlu(i) ||
        i.Operation is JitOperation.LoadMov or JitOperation.LoadMovzx or JitOperation.StoreMov or
            JitOperation.Push or JitOperation.Pop ||
        (i.Operation is JitOperation.Mov or JitOperation.Movzx or JitOperation.Movsx) &&
            (i.Destination.IsMemory || i.Source.IsMemory);

    // True means fully validated by this partial, false delegates to scalar IR.
    internal static bool ValidateMemory(JitInstruction i)
    {
        if (ValidateStackMemory(i)) return true;
        if (!CanInlineMemory(i)) return false;
        if (i.Width is not (8 or 16 or 32 or 64)) throw new ArgumentException("Native memory width");
        var a = i.Address;
        bool usesAddress = i.Destination.IsMemory || i.Source.IsMemory ||
            i.Operation is JitOperation.LoadMov or JitOperation.LoadMovzx or JitOperation.StoreMov;
        if (usesAddress && (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
            a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
            a.RipRelative && (a.Base != -1 || a.Index != -1))) throw new ArgumentException("Native memory address");
        if (i.Options != 0 || i.Extra.IsMemory) throw new ArgumentException("Native memory options");
        if (i.Destination.IsMemory && i.Source.IsMemory) throw new ArgumentException("Two explicit memory operands");
        if (IsNativeMemoryControl(i) && i.Width != 64) throw new ArgumentException("Native near control width");
        if ((i.Operation is JitOperation.Push or JitOperation.Pop) && i.Width is not (16 or 64))
            throw new ArgumentException("Native stack width");
        if (i.Operation is JitOperation.Movzx or JitOperation.Movsx or JitOperation.LoadMovzx)
        {
            if (!i.Source.IsMemory && i.Operation != JitOperation.LoadMovzx || i.Width is not (16 or 32 or 64) ||
                i.SourceWidth is not (8 or 16 or 32) || i.SourceWidth >= i.Width ||
                i.SourceWidth == 32 && (i.Operation != JitOperation.Movsx || i.Width != 64))
                throw new ArgumentException("Native memory extension");
        }
        if (NativeMemoryAlu(i))
        {
            if (i.Options != 0 || i.Extra.IsMemory ||
                (i.Operation is JitOperation.SetCondition or JitOperation.ConditionalMove) && (i.Condition is < 0 or > 15))
                throw new ArgumentException("Native memory ALU options");
            if (i.Operation == JitOperation.SetCondition && (i.Width != 8 || !i.Destination.IsMemory) ||
                i.Operation == JitOperation.ConditionalMove && (i.Width == 8 || !i.Source.IsMemory))
                throw new ArgumentException("Native memory conditional operand");
            if ((i.Operation is JitOperation.Inc or JitOperation.Dec or JitOperation.Neg or JitOperation.Not) && !i.Destination.IsMemory)
                throw new ArgumentException("Native memory unary operand");
            if (!i.Destination.IsMemory) ValidateOperand(i.Destination, i.Width, true);
            if (!i.Source.IsMemory && i.Operation is not (JitOperation.Inc or JitOperation.Dec or JitOperation.Neg or JitOperation.Not or JitOperation.SetCondition))
                ValidateOperand(i.Source, i.Width, false);
            return true;
        }
        if ((i.Operation is JitOperation.LoadMov or JitOperation.StoreMov) && i.SourceWidth != 0 && i.SourceWidth != i.Width)
            throw new ArgumentException("Native MOV memory source width");
        if ((i.Operation is JitOperation.IndirectJump or JitOperation.IndirectCall) && i.Source.IsImmediate)
            throw new ArgumentException("Native indirect control source");
        if (i.Operation == JitOperation.ReturnAdjust && (!i.Source.IsImmediate || i.Source.Immediate > ushort.MaxValue))
            throw new ArgumentException("Native RET adjustment");
        bool sourceRegister = i.Operation is JitOperation.StoreMov or JitOperation.Push or JitOperation.IndirectCall ||
            i.Operation == JitOperation.IndirectJump && !i.Source.IsMemory || i.Destination.IsMemory;
        if (sourceRegister && !i.Source.IsMemory) ValidateOperand(i.Source, i.Width, false);
        if (i.Operation is JitOperation.LoadMov or JitOperation.LoadMovzx or JitOperation.Pop || i.Source.IsMemory && !IsNativeMemoryControl(i))
            ValidateOperand(i.Destination, i.Width, true);
        if (i.Operation == JitOperation.Pop && i.Destination.IsMemory ||
            i.Operation == JitOperation.IndirectCall && i.Source.IsMemory ||
            i.Operation == JitOperation.Push && i.Source.IsMemory)
            throw new ArgumentException("Native two-access stack form is not implemented");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryMemoryOperation(in JitInstruction i) => NativeMemoryOperation(i);

        private bool NativeMemoryOperation(JitInstruction i)
        {
            if (!CanInlineMemory(i) || IsNativeMemoryControl(i)) return false;
            if (NativeMemoryAlu(i)) { NativeMemoryAluOperation(i); return true; }
            int width = i.Width;
            if (i.Operation == JitOperation.Push)
            {
                // Capture source BEFORE RSP subtraction (PUSH RSP).
                NativeMemoryPrepare(i with { Address = new(4, -1, 0, -width / 8) }, FullMemoryAccess.Write, width);
                Load(i.Source, width, 0); Bytes(0x49, 0x89, 0xc0);
                NativeMemoryStore(width);
                Bytes(0x49, 0x83, 0x6b, 56, (byte)(width / 8)); // guest RSP, never host RSP
            }
            else if (i.Operation == JitOperation.Pop)
            {
                NativeMemoryPrepare(i with { Address = new(4, -1, 0, 0) }, FullMemoryAccess.Read, width);
                NativeMemoryLoad(width); Bytes(0x49, 0x89, 0xc0);
                Bytes(0x49, 0x83, 0x43, 56, (byte)(width / 8));
                Store(i.Destination, width); // POP RSP writes after increment
            }
            else if (i.Operation == JitOperation.StoreMov || i.Destination.IsMemory)
            {
                NativeMemoryPrepare(i, FullMemoryAccess.Write, width);
                Load(i.Source, width, 0); Bytes(0x49, 0x89, 0xc0);
                NativeMemoryStore(width);
            }
            else
            {
                int sourceWidth = i.SourceWidth == 0 ? width : i.SourceWidth;
                NativeMemoryPrepare(i, FullMemoryAccess.Read, sourceWidth);
                NativeMemoryLoad(sourceWidth);
                if (i.Operation == JitOperation.Movsx)
                {
                    if (sourceWidth == 32) Bytes(0x48, 0x63, 0xc0);
                    else Bytes(0x48, 0x0f, sourceWidth == 8 ? (byte)0xbe : (byte)0xbf, 0xc0);
                }
                if (width == 32) Bytes(0x89, 0xc0);
                Bytes(0x49, 0x89, 0xc0); Store(i.Destination, width);
            }
            NativeMemoryCount();
            return true;
        }

        // Reuses baseline flag construction from the integer partial. Memory
        // permission and cross-page checks finish BEFORE all register/flag work.
        private void NativeMemoryAluOperation(JitInstruction i)
        {
            int w = i.Width;
            JitOperation op = i.Operation;
            bool compare = op is JitOperation.Cmp or JitOperation.Test;
            bool unary = op is JitOperation.Inc or JitOperation.Dec or JitOperation.Neg or JitOperation.Not;
            FullMemoryAccess access = i.Destination.IsMemory && !compare ?
                op == JitOperation.SetCondition ? FullMemoryAccess.Write : FullMemoryAccess.Read | FullMemoryAccess.Write : FullMemoryAccess.Read;
            if (op == JitOperation.ConditionalMove)
            {
                // Selected Blink semantics read memory ONLY on a true condition.
                // The false path still WriteRegister(old), clearing high32.
                Condition(i.Condition); IntegerRR(0x85, 0, 0, 32);
                int selected = NearCondition(0x85);
                IntegerLoad(i.Destination, w, 8); Store(i.Destination, w);
                int done = NativeNearJump();
                Patch(selected, position);
                NativeMemoryPrepare(i, FullMemoryAccess.Read, w); NativeMemoryLoad(w);
                IntegerMov(8, 0); Store(i.Destination, w); NativeMemoryCount();
                Patch(done, position); return;
            }
            NativeMemoryPrepare(i, access, w);
            if (op == JitOperation.SetCondition)
            {
                Condition(i.Condition); IntegerMov(8, 0); NativeMemoryStore(8); NativeMemoryCount(); return;
            }
            NativeMemoryLoad(w);
            if (i.Source.IsMemory)
            { IntegerMov(1, 0); IntegerLoad(i.Destination, w, 0); }
            else if (unary) IntegerImmediate(1, op == JitOperation.Neg || op == JitOperation.Not ? 0UL : 1UL, 32);
            else IntegerLoad(i.Source, w, 1);
            if (op == JitOperation.Not)
            { IntegerUnary(2, 0, w); IntegerMov(8, 0); IntegerTruncate(8, w); NativeMemoryStore(w); NativeMemoryCount(); return; }
            IntegerMov(9, 0); IntegerRR(0x31, 9, 1);
            if (op is JitOperation.Adc or JitOperation.Sbb) Bytes(0x41, 0x0f, 0xba, 0x63, 12, 0);
            int opcode = op switch
            {
                JitOperation.Add or JitOperation.Inc => 0x01, JitOperation.Adc => 0x11, JitOperation.Sbb => 0x19,
                JitOperation.Sub or JitOperation.Cmp or JitOperation.Dec => 0x29,
                JitOperation.And or JitOperation.Test => 0x21, JitOperation.Or => 0x09, JitOperation.Xor => 0x31,
                JitOperation.Neg => 0, _ => throw new ArgumentException("Native memory ALU operation")
            };
            if (op == JitOperation.Neg) IntegerUnary(3, 0, w);
            else IntegerRR((byte)(w == 8 ? opcode - 1 : opcode), 0, 1, w);
            IntegerMov(8, 0);
            IntegerArithmeticFlags(op is JitOperation.Inc or JitOperation.Dec,
                op is JitOperation.And or JitOperation.Or or JitOperation.Xor or JitOperation.Test);
            IntegerTruncate(8, w);
            if (!compare)
            {
                if (i.Destination.IsMemory) NativeMemoryStore(w);
                else Store(i.Destination, w);
            }
            NativeMemoryCount();
        }

        // Complete control-site path: core must not subsequently Commit(next).
        private void NativeMemoryControl(JitInstruction i) => NativeNearControl(i);

        private void NativeMemoryCount()
        { Bytes(0x49, 0xff, 0x82); U32(264); }

        // EA arithmetic is generated. Address-size truncation occurs BEFORE
        // FS/GS addition. Other long-mode segment bases are architecturally zero.
        private void NativeMemoryAddress(JitInstruction i)
        {
            if (TryScalarMiscAddress(i)) return;
            Address(i, truncate: false);
            if (AtomicBitAddress(i) || MemoryIntegerBitAddress(i)) return;
            if (i.Address.AddressWidth == 32) Bytes(0x89, 0xc0);
            if (i.Address.Segment is 4 or 5)
            { Bytes(0x49, 0x03, 0x82); U32(i.Address.Segment == 4 ? 328u : 336u); }
        }

        // Clobbers all volatile scratch registers, no architectural effects.
        private void NativeMemoryPrepare(JitInstruction i, FullMemoryAccess access, int bitWidth, bool atomicFast = false)
        {
            if (!FullNativeMemory.ValidMemoryWidth(unchecked((uint)bitWidth)))
                throw new ArgumentOutOfRangeException(nameof(bitWidth));
            // An ordinary request cannot inherit a previous two-access shape.
            Bytes(0x49, 0x8b, 0x8a); U32(384); // rcx = optional worker descriptor
            Bytes(0x48, 0x85, 0xc9);
            int noStack = NearCondition(0x84);
            NativeNormalizeDenseDescriptor(1);
            Bytes(0xc7, 0x41, 4); U32(0);
            Patch(noStack, position);
            NativeMemoryAddress(i);
            // An exclusive runtime ticket is usable only for its exact request.
            Bytes(0x41, 0x83, 0xba); U32(324); Bytes((byte)FullMemoryFlags.BarrierTicket);
            int ordinary = NearCondition(0x85);
            Bytes(0x49, 0x3b, 0x82); U32(216); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x81, 0xba); U32(252); U32((uint)access); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x81, 0xba); U32(256); U32((uint)bitWidth); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x8b, 0x8a); U32(144);
            Bytes(0x41, 0x3b, 0x8a); U32(248); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x49, 0x8b, 0x8a); U32(344); Bytes(0x48, 0x85, 0xc9); ExitCondition(0x84, FullBlockExit.Invalidated);
            NativeMemoryEpochCheck();
            Bytes(0x41, 0xc7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierConsumed);
            Bytes(0x49, 0xc7, 0x82); U32(344); U32(0);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
            int readyTicket = NativeNearJump();
            Patch(ordinary, position);
            Bytes(0x49, 0x89, 0x82); U32(216); // staged EA, not guest faultaddr
            Bytes(0x41, 0xc7, 0x82); U32(252); U32((uint)access);
            Bytes(0x41, 0xc7, 0x82); U32(256); U32((uint)bitWidth);
            Bytes(0x41, 0x8b, 0x8a); U32(144);
            Bytes(0x41, 0x89, 0x8a); U32(248);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(232); U32(0); // no second page yet
            Bytes(0x41, 0xc7, 0x82); U32(260); U32((uint)(bitWidth / 8));
            if ((access & FullMemoryAccess.Atomic) != 0)
            {
                if (!atomicFast) JumpExit(FullBlockExit.NeedMemory);
                else
                {
                    // Only typed scalar forms reach this option. Natural EA
                    // alignment also proves a complete operand in one page.
                    Bytes(0x48, 0xa9); U32((uint)(bitWidth / 8 - 1));
                    ExitCondition(0x85, FullBlockExit.NeedMemory);
                    NativeTlbLookup(FullMemoryAccess.Read | FullMemoryAccess.Write, atomicFast: true);
                    Bytes(0x49, 0x89, 0x92); U32(224);
                    Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
                }
                Patch(readyTicket, position);
                return;
            }
            // Bounds and canonicality: do not dereference a host pointer until
            // both page permissions/lifetimes are established.
            Bytes(0x48, 0x89, 0xc1, 0x48, 0xc1, 0xe1); Bytes( 16, 0x48, 0xc1, 0xf9, 16, 0x48, 0x39, 0xc1);
            ExitCondition(0x85, FullBlockExit.NeedMemory);
            if (bitWidth > 8)
            {
                if (bitWidth / 8 <= 128) Bytes(0x48, 0x83, 0xc0, (byte)(bitWidth / 8 - 1));
                else IntegerConstant(0, 0, (uint)(bitWidth / 8 - 1), 64);
                ExitCondition(0x82, FullBlockExit.NeedMemory); // address overflow
                Bytes(0x48, 0x89, 0xc1, 0x48, 0xc1, 0xe1); Bytes( 16, 0x48, 0xc1, 0xf9, 16, 0x48, 0x39, 0xc1);
                ExitCondition(0x85, FullBlockExit.NeedMemory);
                Bytes(0x49, 0x8b, 0x82); U32(216);
            }
            NativeTlbLookup(access); // RAX exact EA -> RDX exact checked host
            Bytes(0x49, 0x89, 0x92); U32(224);
            Bytes(0x49, 0x8b, 0x82); U32(216);
            Bytes(0x89, 0xc1, 0x81, 0xe1); U32(4095);
            Bytes(0x81, 0xf9); U32((uint)(4096 - bitWidth / 8));
            int single = NearCondition(0x86);
            Bytes(0xba); U32(4096); Bytes(0x29, 0xca);
            Bytes(0x41, 0x89, 0x92); U32(260);
            Bytes(0x48, 0x25); U32(0xfffff000); Bytes(0x48, 0x05); U32(4096);
            NativeTlbLookup(access);
            Bytes(0x49, 0x89, 0x92); U32(232);
            Patch(single, position);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
            Patch(readyTicket, position);
        }

        private void NativeMemoryEpochCheck()
        {
            Bytes(0x49, 0x8b, 0x8a); U32(240);
            Bytes(0x49, 0x3b, 0x8a); U32(136); ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x49, 0x8b, 0x92); U32(128);
            Bytes(0x48, 0x85, 0xd2); ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x48, 0x3b, 0x0a); ExitCondition(0x85, FullBlockExit.NeedMemory);
        }

        // RAX=EA; return RDX=host+offset. Pointer array is worker-owned, never
        // from a retained arena. All page/header pointers require live epochs.
        private void NativeTlbLookupInline(FullMemoryAccess access, bool atomicFast = false)
        {
            Bytes(0x49, 0x89, 0xc0); // r8=EA
            Bytes(0x41, 0x8b, 0x8a); U32(288);
            Bytes(0x83, 0xf9, 1); ExitCondition(0x82, FullBlockExit.NeedMemory);
            Bytes(0x81, 0xf9); U32(FullNativeMemory.MaximumTlbCount - 1); ExitCondition(0x87, FullBlockExit.NeedMemory);
            Bytes(0x8d, 0x51, 1, 0x85, 0xd1); ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x49, 0x8b, 0x92); U32(280);
            Bytes(0x48, 0x85, 0xd2); ExitCondition(0x84, FullBlockExit.NeedMemory);
            Bytes(0x48, 0x89, 0xc1, 0x48, 0xc1, 0xe9, 12); // rcx=page
            // TlbIndex: page ^ ((page >> 5) & ~1UL). Bit0 stays unmodified.
            // R8 retains EA; RDX retains table; R9 may retain a two-access value.
            // Only the low32 index bits are needed before the <=4095 mask.
            Bytes(0x48, 0xc1, 0xe8, 17, 0x83, 0xe0, 0xfe, 0x31, 0xc1);
            Bytes(0x41, 0x23, 0x8a); U32(288); // and ecx,mask
            Bytes(0x48, 0xc1, 0xe1, 6, 0x48, 0x01, 0xca);
            Bytes(0x4c, 0x89, 0xc0); // restore RAX=EA before exact page-tag check
            Bytes(0x48, 0x25); U32(0xfffff000);
            Bytes(0x48, 0x39, 0x02); ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x49, 0x8b, 0x8a); U32(136);
            Bytes(0x48, 0x3b, 0x4a, 16); ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x49, 0x89, 0x8a); U32(240);
            Bytes(0x49, 0x8b, 0x82); U32(128);
            Bytes(0x48, 0x3b, 0x08); ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x49, 0x8b, 0x4a, 120, 0x48, 0x3b, 0x4a, 24); ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x8b, 0x4a, 32, 0x83, 0xe1, (byte)access, 0x83, 0xf9, (byte)access);
            ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x83, 0x7a, 48, 0); ExitCondition(0x85, FullBlockExit.NeedMemory);
            Bytes(0x81, 0x7a, 52); U32(4096); ExitCondition(0x85, FullBlockExit.NeedMemory);
            if ((access & FullMemoryAccess.Write) != 0)
            { Bytes(0xf7, 0x42, 36); U32((uint)FullMemoryFlags.WriteBarrier); ExitCondition(0x85, FullBlockExit.NeedMemory); }
            if (atomicFast)
            {
                Bytes(0x83, 0x7a, 32, 3); ExitCondition(0x85, FullBlockExit.NeedMemory);
                Bytes(0x83, 0x7a, 36, 0); ExitCondition(0x85, FullBlockExit.NeedMemory);
                Bytes(0x48, 0x8b, 0x42, 8, 0x48, 0x3b, 0x42, 40);
                ExitCondition(0x85, FullBlockExit.NeedMemory);
                Bytes(0x48, 0xa9); U32(4095); ExitCondition(0x85, FullBlockExit.NeedMemory);
            }
            Bytes(0x48, 0x8b, 0x52, 8, 0x48, 0x85, 0xd2); ExitCondition(0x84, FullBlockExit.NeedMemory);
            Bytes(0x4c, 0x89, 0xc0, 0x25); U32(4095); Bytes(0x48, 0x01, 0xc2);
        }

        // No host memory fault recovery. Cross-page paths use prechecked bytes
        // from both mappings; all validation precedes the first data effect.
        private void NativeMemoryLoad(int bitWidth)
        {
            if (bitWidth is not (8 or 16 or 32 or 64))
                throw new ArgumentOutOfRangeException(nameof(bitWidth));
            Bytes(0x49, 0x83, 0xba); U32(232); Bytes(0);
            int crossing = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(224);
            if (bitWidth is 8 or 16) Bytes(0x0f, bitWidth == 8 ? (byte)0xb6 : (byte)0xb7, 0x02);
            else { if (bitWidth == 64) Bytes(0x48); Bytes(0x8b, 0x02); }
            int done = NativeNearJump();
            Patch(crossing, position);
            Bytes(0x45, 0x31, 0xc9); // r9 = result
            for (int b = 0; b < bitWidth / 8; ++b)
            {
                NativeBytePointer(b);
                Bytes(0x0f, 0xb6, 0x02);
                if (b != 0) Bytes(0x48, 0xc1, 0xe0, (byte)(8 * b));
                Bytes(0x49, 0x09, 0xc1);
            }
            Bytes(0x4c, 0x89, 0xc8);
            Patch(done, position);
        }

        private void NativeMemoryStore(int bitWidth)
        {
            if (bitWidth is not (8 or 16 or 32 or 64))
                throw new ArgumentOutOfRangeException(nameof(bitWidth));
            Bytes(0x49, 0x83, 0xba); U32(232); Bytes(0);
            int crossing = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(224);
            if (bitWidth == 16) Bytes(0x66);
            Bytes(bitWidth == 64 ? (byte)0x4c : (byte)0x44, bitWidth == 8 ? (byte)0x88 : (byte)0x89, 0x02);
            int done = NativeNearJump();
            Patch(crossing, position);
            for (int b = 0; b < bitWidth / 8; ++b)
            {
                NativeBytePointer(b);
                Bytes(0x4c, 0x89, 0xc0);
                if (b != 0) Bytes(0x48, 0xc1, 0xe8, (byte)(8 * b));
                Bytes(0x88, 0x02);
            }
            Patch(done, position);
        }

        private void NativeBytePointer(int index)
        {
            Bytes(0xb9); U32((uint)index);
            Bytes(0x41, 0x3b, 0x8a); U32(260);
            int second = NearCondition(0x83);
            Bytes(0x49, 0x8b, 0x92); U32(224); Bytes(0x48, 0x01, 0xca);
            int done = NativeNearJump();
            Patch(second, position);
            Bytes(0x41, 0x2b, 0x8a); U32(260);
            Bytes(0x49, 0x8b, 0x92); U32(232); Bytes(0x48, 0x01, 0xca);
            Patch(done, position);
        }

        private int NativeNearJump() { Bytes(0xe9); int at = position; U32(0); return at; }

        private void NativeDynamicCommitAndDispatch()
        {
            Bytes(0x49, 0x89, 0x03); // IP is final BEFORE any possible cap exit
            Bytes(0x41, 0xc6, 0x43, 8, 0);
            EmitCompletedIncrement();
            Bytes(0x49, 0xff, 0x42, 48);
            GuardCap();
            Bytes(0x49, 0x8b, 0x03);
            NativeJumpCache();
        }

        private void NativeJumpCache()
        {
            Bytes(0x41, 0x81, 0xba); U32(320); U32(255);
            ExitCondition(0x85, FullBlockExit.Continue);
            // Stable generation BEFORE reading any stale retained Header ptr.
            Bytes(0x49, 0x8b, 0x92); U32(296); Bytes(0x48, 0x85, 0xd2); ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x12, 0x49, 0x3b, 0x92); U32(304); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x8a); U32(312); Bytes(0x48, 0x85, 0xc9); ExitCondition(0x84, FullBlockExit.Continue);
            // Same byte-PC/fold8 hash as managed publication. R8 keeps the full target;
            // RDX generation, RCX cache base and R10/R11 survive the extra three XOR bytes.
            Bytes(0x49, 0x89, 0xc0, 0x48, 0xc1, 0xe8, 8, 0x4c, 0x31, 0xc0);
            Bytes(0x41, 0x23, 0x82); U32(320); Bytes(0x48, 0xc1, 0xe0, 6, 0x48, 0x01, 0xc1);
            Bytes(0x48, 0x39, 0x51, 16); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x4c, 0x39, 0x01); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x42, 120, 0x48, 0x3b, 0x41, 8); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x42, 104, 0x48, 0x3b, 0x41, 24); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x51, 32, 0x48, 0x8b, 0x49, 40, 0x48, 0x85, 0xc9); ExitCondition(0x84, FullBlockExit.Continue);
            // Header now protected by the native store epoch and generation.
            Bytes(0x83, 0x79, 40, 1); ExitCondition(0x85, FullBlockExit.Continue);
            // Only validated complete-writer-protocol bodies may be chained.
            // A cap1 body from mutable external backing must stay cap1 even
            // when a trusted source encounters its nonowning jump hint later.
            Bytes(0x48, 0x83, 0xb9); U32(136); Bytes(1); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x39, 0x51, 8); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x4c, 0x39, 0x41, 16); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x39, 0x41, 0); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x42, 120, 0x48, 0x3b, 0x41, 32); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x41, 112, 0x48, 0x85, 0xc0); ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x48, 0x3b, 0x41, 120); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x82); U32(376); Bytes(0x48, 0x3b, 0x81); U32(128); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x92); U32(368); Bytes(0x48, 0x85, 0xd2); ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x48, 0x3b, 0x02); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x82); U32(136);
            Bytes(0x49, 0x8b, 0x92); U32(128); Bytes(0x48, 0x85, 0xd2); ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x48, 0x3b, 0x02); ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x41, 24, 0x48, 0x85, 0xc0); ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x49, 0x89, 0x4a, 96, 0x48, 0x8b); Bytes( 0x51, 8, 0x49, 0x89, 0x52, 112);
            Bytes(0x41, 0xc7, 0x82); U32(144); U32(0);
            Bytes(0x49, 0xff, 0x82); U32(160);
            Bytes(0x4c, 0x89, windowsAbi ? (byte)0xd1 : (byte)0xd7, 0xff, 0xe0);
        }
    }
}
