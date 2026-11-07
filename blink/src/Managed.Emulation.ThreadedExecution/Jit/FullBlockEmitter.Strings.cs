namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineString(JitInstruction i) => FullStringLowering.IsString(i);

    internal static bool ValidateString(JitInstruction i)
    {
        if (!CanInlineString(i)) return false;
        var a = i.Address;
        if (i.Length is < 1 or > 15 || i.Width is not (8 or 16 or 32 or 64) ||
            i.SourceWidth != i.Width || i.Destination != default || i.Source != default ||
            i.Extra != default || i.Options != 0 || i.Target != 0 || i.Condition != 0 ||
            a.Base != (FullStringLowering.HasSource(i) ? 6 : 7) || a.Index != -1 ||
            a.Scale != 0 || a.Displacement != 0 || a.RipRelative ||
            a.AddressWidth is not (32 or 64) || a.Segment is not (-1 or 2 or 4 or 5) ||
            !FullStringLowering.HasSource(i) && a.Segment != -1)
            throw new ArgumentException("Invalid canonical native string IR");
        return true;
    }

    private ref partial struct Writer
    {
        // Ordinary core scaffold owns ONE issue and ONE final commit. Every
        // possible mapping/fault exit precedes data/flags/index effects.
        private bool TryStringOperation(in JitInstruction i)
        {
            if (!CanInlineString(i)) return false;
            if (FullStringLowering.IsPair(i))
            {
                NativeStringPairPrepare(i);
                // These are layout-only primitives from the original ABI5
                // partial. No stack operation/classifier is invoked. Parent
                // explicitly extends the ordered mapper's string grammar.
                NativeStackMemoryAdopt(32); NativeMemoryLoad(i.Width);
                if (i.Operation == JitOperation.StringMove)
                {
                    IntegerMov(8, 0); // snapshot entire element before overlap
                    NativeStackMemoryAdopt(80); NativeMemoryStore(i.Width);
                }
                else
                {
                    IntegerFrameStore(200, 0);
                    NativeStackMemoryAdopt(80); NativeMemoryLoad(i.Width);
                    IntegerMov(1, 0); IntegerFrameLoad(200, 0);
                    NativeStringCompare(i.Width); // source minus destination
                }
            }
            else
            {
                bool source = FullStringLowering.HasSource(i);
                var operand = i with { Address = source ? i.Address :
                    new(7, -1, 0, 0, false, i.Address.AddressWidth, -1) };
                NativeMemoryAddress(operand); NativeStringCanonical(i.Width, operand.Address.Segment);
                NativeMemoryPrepare(operand, i.Operation == JitOperation.StringStore ?
                    FullMemoryAccess.Write : FullMemoryAccess.Read, i.Width);
                if (i.Operation == JitOperation.StringStore)
                { IntegerLoad(new(0, 0), i.Width, 8); NativeMemoryStore(i.Width); }
                else
                {
                    NativeMemoryLoad(i.Width);
                    if (i.Operation == JitOperation.StringLoad)
                    {
                        // Architectural EAX writes clear high32; native8's
                        // byte-copy LODS32 is an explicitly retained gap.
                        IntegerMov(8, 0); Store(new(0, 0), i.Width);
                    }
                    else
                    {
                        IntegerMov(1, 0); IntegerLoad(new(0, 0), i.Width, 0);
                        NativeStringCompare(i.Width); // accumulator minus memory
                    }
                }
            }
            if (FullStringLowering.HasSource(i)) NativeStringStep(6, i);
            if (FullStringLowering.HasDestination(i)) NativeStringStep(7, i);
            NativeMemoryCount();
            return true;
        }

        private void NativeStringCompare(int width)
        {
            IntegerMov(9, 0); IntegerRR(0x31, 9, 1);
            IntegerRR(width == 8 ? (byte)0x28 : (byte)0x29, 0, 1, width);
            IntegerMov(8, 0);
            IntegerArithmeticFlags(false, false);
        }

        private void NativeStringStep(int guestRegister, JitInstruction i)
        {
            IntegerLoad(new(guestRegister, 0), i.Address.AddressWidth, 8);
            Bytes(0x41, 0xf7, 0x43, 12); U32(1024); // read guest DF; never set host DF
            int backward = NearCondition(0x85);
            IntegerConstant(0, 8, (uint)(i.Width / 8), i.Address.AddressWidth);
            int done = NativeNearJump();
            Patch(backward, position);
            IntegerConstant(5, 8, (uint)(i.Width / 8), i.Address.AddressWidth);
            Patch(done, position);
            Store(new(guestRegister, 0), i.Address.AddressWidth);
        }

        // RAX is the computed linear start (AS32 already truncated before
        // FS/GS addition). Check start, carry and endpoint before any mapping
        // or guest effect. SS applies only to the source; ES remains #GP13.
        // Keep RAX intact so callers cannot accidentally probe the endpoint.
        private void NativeStringCanonical(int width, int segment = -1)
        {
            IntegerMov(1, 0); IntegerShift(7, 1, 64, 47);
            IntegerConstant(0, 1, 1, 64); IntegerConstant(7, 1, 1, 64);
            int first = NearCondition(0x87);
            IntegerMov(2, 0); IntegerConstant(0, 2, (uint)(width / 8 - 1), 64);
            int wrap = NearCondition(0x82);
            IntegerMov(1, 2); IntegerShift(7, 1, 64, 47);
            IntegerConstant(0, 1, 1, 64); IntegerConstant(7, 1, 1, 64);
            int valid = NearCondition(0x86);
            Patch(first, position); Patch(wrap, position);
            Bytes(0x41, 0xc7, 0x82); U32(168); U32(segment == 2 ? 12u : 13u);
            Bytes(0x41, 0xc7, 0x82); U32(172); U32(128);
            Bytes(0x49, 0xc7, 0x82); U32(176); U32(0);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
            JumpExit(FullBlockExit.GuestFault);
            Patch(valid, position);
        }

        private void NativeStringPairPrepare(JitInstruction i)
        {
            var secondAccess = FullStringLowering.PairSecondAccess(i);
            NativeMemoryAddress(i); IntegerFrameStore(184, 0);
            NativeMemoryAddress(i with { Address = new(7, -1, 0, 0, false, i.Address.AddressWidth, -1) });
            IntegerFrameStore(192, 0);
            NativeStackMemoryDescriptor();
            NativeNormalizeDenseDescriptor(9);
            NativeStackMemoryCompare(0, FullStackMemoryDescriptor.CurrentVersion);
            IntegerFrameLoad(184, 0); IntegerFrameStore(216, 0);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x82); U32(248);
            Bytes(0x41, 0xc7, 0x82); U32(252); U32((uint)FullMemoryAccess.Read);
            Bytes(0x41, 0xc7, 0x82); U32(256); U32((uint)i.Width);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            // Descriptor is the ticket authority; reset stale single-access state.
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(344); U32(0);
            Bytes(0x41, 0x83, 0x79, 4, 0);
            int empty = NearCondition(0x84);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.Ready);
            int ready = NearCondition(0x84);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            int ticket = NearCondition(0x84);
            Patch(empty, position);
            NativeStackMemoryDword(4, 2);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x41, 8);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request);
            NativeStackMemoryZeroPointer(24);
            NativeStackMemoryStage(32, 184, FullMemoryAccess.Read, i.Width);
            NativeStackMemoryStage(80, 192, secondAccess, i.Width);
            // Source endpoint validation and all source page permissions precede
            // destination validation. Complete shape is staged before any miss.
            IntegerFrameLoad(184, 0); NativeStringCanonical(i.Width, i.Address.Segment);
            NativeStackMemoryProbe(32, FullMemoryAccess.Read, i.Width);
            IntegerFrameLoad(192, 0); NativeStringCanonical(i.Width);
            NativeStackMemoryProbe(80, secondAccess, i.Width);
            Bytes(0x49, 0x8b, 0x82); U32(136); Bytes(0x49, 0x89, 0x41, 16);
            int checkedMappings = NativeNearJump();
            Patch(ready, position); Patch(ticket, position);
            NativeStackMemoryCompare(4, 2);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x3b, 0x41, 8);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            NativeStackMemoryValidate(32, 184, FullMemoryAccess.Read, i.Width);
            NativeStackMemoryValidate(80, 192, secondAccess, i.Width);
            Bytes(0x49, 0x8b, 0x41, 16, 0x49, 0x3b, 0x82); U32(136);
            int observed = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(128); IntegerRR(0x85, 2, 2);
            ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x48, 0x3b, 0x02);
            int current = NearCondition(0x84);
            Patch(observed, position);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request);
            NativeStackMemoryZeroPointer(24);
            JumpExit(FullBlockExit.NeedMemory);
            Patch(current, position);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            int ordinary = NearCondition(0x85);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierConsumed);
            int authorized = NativeNearJump();
            Patch(ordinary, position);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x85, FullBlockExit.Invalidated);
            Patch(authorized, position); Patch(checkedMappings, position);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Consumed);
            NativeStackMemoryZeroPointer(24);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
        }
    }
}
