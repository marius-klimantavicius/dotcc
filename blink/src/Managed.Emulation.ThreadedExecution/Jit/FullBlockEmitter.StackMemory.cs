using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

internal enum FullStackMemoryState : uint { Empty = 0, Request = 1, Ready = 2, TicketReady = 3, Consumed = 4 }

// One fixed worker-owned allocation. Neither request owns a mapping: passive
// backing lifetime and native admission/exclusion must cover the entire leaf.
// Host0 is the exact first byte, Host1 the next page's first byte or null.
[StructLayout(LayoutKind.Explicit, Size = FullStackMemoryRequest.ByteSize)]
internal unsafe struct FullStackMemoryRequest
{
    internal const int ByteSize = 48;
    [FieldOffset(0)] public ulong Address;
    [FieldOffset(8)] public byte* Host0;
    [FieldOffset(16)] public byte* Host1;
    [FieldOffset(24)] public FullMemoryAccess Access;
    [FieldOffset(28)] public uint Width;
    [FieldOffset(32)] public uint Length0;
    [FieldOffset(36)] public uint Reserved36;
    [FieldOffset(40)] public ulong Reserved40;
}

// Source/read is always Request0; Request1 is the only write (when Count=2).
// Runtime resolves BOTH requests in that order before publishing Ready or
// TicketReady, including four independent pointers on direct-TLB collisions.
// TicketReady additionally requires exclusive admission and revocation of ALL
// physical destination aliases, final reinspection, and exclusion through RET.
// The ordinary single-request prepare clears Count before staging its request.
[StructLayout(LayoutKind.Explicit, Size = FullStackMemoryDescriptor.ByteSize)]
internal unsafe struct FullStackMemoryDescriptor
{
    internal const int ByteSize = 128;
    internal const uint CurrentVersion = 1;
    [FieldOffset(0)] public uint Version;
    [FieldOffset(4)] public uint Count;
    [FieldOffset(8)] public uint Site;
    [FieldOffset(12)] public FullStackMemoryState State;
    [FieldOffset(16)] public ulong Epoch;
    [FieldOffset(24)] public ulong Ticket;
    [FieldOffset(32)] public FullStackMemoryRequest Request0;
    [FieldOffset(80)] public FullStackMemoryRequest Request1;
}

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineStackMemory(JitInstruction i) => i.Operation == JitOperation.Leave ||
        i.Operation is (JitOperation.Push or JitOperation.IndirectCall) && i.Source.IsMemory ||
        i.Operation == JitOperation.Pop && i.Destination.IsMemory;

    internal static bool ValidateStackMemory(JitInstruction i)
    {
        if (!CanInlineStackMemory(i)) return false;
        var memory = new JitOperand(0, 0, IsMemory: true);
        var a = i.Address;
        if (i.Length is < 1 or > 15 || i.Width is not (16 or 64) || i.SourceWidth != i.Width ||
            i.Target != 0 || i.Condition != 0 || i.Options != 0 || i.Extra != default ||
            a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
            a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
            a.RipRelative && (a.Base != -1 || a.Index != -1))
            throw new ArgumentException("Native stack memory metadata/address");
        bool valid = i.Operation switch
        {
            JitOperation.Leave => i.Source == default && i.Destination == default && a == new JitAddress(5, -1, 0, 0),
            JitOperation.Push => i.Source == memory && i.Destination == default,
            JitOperation.Pop => i.Source == default && i.Destination == memory,
            JitOperation.IndirectCall => i.Width == 64 && i.Source == memory && i.Destination == default,
            _ => false
        };
        if (!valid) throw new ArgumentException("Native stack memory operand shape");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryStackMemoryOperation(in JitInstruction i)
        {
            if (!CanInlineStackMemory(i) || i.Operation == JitOperation.IndirectCall) return false;
            NativeStackMemoryPrepare(i);
            NativeStackMemoryAdopt(32); NativeMemoryLoad(i.Width); IntegerMov(8, 0);
            if (i.Operation != JitOperation.Leave)
            { NativeStackMemoryAdopt(80); NativeMemoryStore(i.Width); }
            IntegerFrameLoad(200, 0); Bytes(0x49, 0x89, 0x43, 56); // final guest RSP
            if (i.Operation == JitOperation.Leave) Store(new(5, 0), i.Width);
            NativeMemoryCount(); return true;
        }

        // Called by the existing complete control-site emitter. Never let the
        // core append Commit(next) after this dynamic target commit/dispatch.
        private void NativeStackMemoryControl(JitInstruction i) => NativeNearMemoryCall(i);

        private void NativeStackMemoryDescriptor()
        {
            IntegerFrameLoad(384, 9); IntegerRR(0x85, 9, 9);
            ExitCondition(0x84, FullBlockExit.Invalidated);
        }

        private void NativeStackMemoryDword(int offset, uint value)
        { Bytes(0x41, 0xc7, 0x41, (byte)offset); U32(value); }

        private void NativeStackMemoryZeroPointer(int offset)
        { Bytes(0x49, 0xc7, 0x41, (byte)offset); U32(0); }

        private void NativeStackMemoryCompare(int offset, uint value)
        {
            Bytes(0x41, 0x81, 0x79, (byte)offset); U32(value);
            ExitCondition(0x85, FullBlockExit.Invalidated);
        }

        private void NativeStackMemoryExplicitAddress(JitInstruction i, bool afterPop)
        {
            Address(i, truncate: false);
            // POP evaluates the explicit destination with post-increment RSP.
            // Apply every RSP contribution BEFORE EA32 truncation and FS/GS.
            if (afterPop && i.Address.Base == 4) IntegerConstant(0, 0, (uint)(i.Width / 8), 64);
            if (afterPop && i.Address.Index == 4) IntegerConstant(0, 0, (uint)(i.Width / 8 << i.Address.Scale), 64);
            if (i.Address.AddressWidth == 32) IntegerMov(0, 0, 32);
            if (i.Address.Segment is 4 or 5)
            { Bytes(0x49, 0x03, 0x82); U32(i.Address.Segment == 4 ? 328u : 336u); }
        }

        private void NativeStackMemoryPrepare(JitInstruction i)
        {
            ValidateStackMemory(i);
            int count = i.Operation == JitOperation.Leave ? 1 : 2;
            int bytes = i.Width / 8;
            if (i.Operation is JitOperation.Push or JitOperation.IndirectCall)
            {
                NativeStackMemoryExplicitAddress(i, false); IntegerFrameStore(184, 0);
                Load(new(4, 0), 64, 0); IntegerConstant(5, 0, (uint)bytes, 64);
                IntegerFrameStore(192, 0); IntegerFrameStore(200, 0);
            }
            else
            {
                Load(new(i.Operation == JitOperation.Leave ? 5 : 4, 0), 64, 0); IntegerFrameStore(184, 0);
                IntegerConstant(0, 0, (uint)bytes, 64); IntegerFrameStore(200, 0);
                if (count == 2)
                { NativeStackMemoryExplicitAddress(i, true); IntegerFrameStore(192, 0); }
            }
            NativeStackMemoryDescriptor();
            // Reject incompatible single-request ownership before union normalization.
            Bytes(0x49, 0x83, 0xba); U32(344); Bytes(0);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            NativeNormalizeDenseDescriptor(9);
            NativeStackMemoryCompare(0, FullStackMemoryDescriptor.CurrentVersion);
            // Dense publication carries its identity in descriptor.Ticket;
            // frame.MemoryTicket is reserved for an ordinary single request.
            // Authenticate incoming ownership BEFORE any summary/marker reset.
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            int noIncomingTicket = NearCondition(0x85);
            Bytes(0x41, 0x81, 0xba); U32(324); U32((uint)FullMemoryFlags.BarrierTicket);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x49, 0x8b, 0x41, 16, 0x49, 0x3b, 0x82); U32(240);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            int incomingChecked = NativeNearJump();
            Patch(noIncomingTicket, position);
            Bytes(0x41, 0xf7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierTicket);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x85, FullBlockExit.Invalidated);
            Patch(incomingChecked, position);
            // A complete request exists before any possible NeedMemory exit.
            IntegerFrameLoad(184, 0); IntegerFrameStore(216, 0);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x82); U32(248);
            Bytes(0x41, 0xc7, 0x82); U32(252); U32((uint)FullMemoryAccess.Read);
            Bytes(0x41, 0xc7, 0x82); U32(256); U32((uint)i.Width);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            Bytes(0x41, 0x83, 0x79, 4, 0);
            int empty = NearCondition(0x84); // ordinary prepare may clear Count alone
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.Ready);
            int ready = NearCondition(0x84);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            int ticket = NearCondition(0x84);
            Patch(empty, position);
            // A malformed/canceled descriptor cannot discard an owned ticket.
            Bytes(0x41, 0xf7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierTicket);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            NativeStackMemoryDword(4, (uint)count);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x41, 8);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request);
            NativeStackMemoryZeroPointer(24);
            NativeStackMemoryStage(32, 184, FullMemoryAccess.Read, i.Width);
            if (count == 2) NativeStackMemoryStage(80, 192, FullMemoryAccess.Write, i.Width);
            NativeStackMemoryProbe(32, FullMemoryAccess.Read, i.Width);
            if (count == 2) NativeStackMemoryProbe(80, FullMemoryAccess.Write, i.Width);
            // All checked pointers are independent of later direct-map slots.
            Bytes(0x49, 0x8b, 0x82); U32(136); Bytes(0x49, 0x89, 0x41, 16);
            int checkedMappings = NativeNearJump();
            Patch(ready, position); Patch(ticket, position);
            NativeStackMemoryCompare(4, (uint)count);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x3b, 0x41, 8);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            NativeStackMemoryValidate(32, 184, FullMemoryAccess.Read, i.Width);
            if (count == 2) NativeStackMemoryValidate(80, 192, FullMemoryAccess.Write, i.Width);
            Bytes(0x49, 0x8b, 0x41, 16, 0x49, 0x3b, 0x82); U32(136);
            int observed = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(128); IntegerRR(0x85, 2, 2);
            ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x48, 0x3b, 0x02);
            int current = NearCondition(0x84);
            Patch(observed, position);
            // Leave stale owned tickets intact for controller cleanup after RET.
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            ExitCondition(0x84, FullBlockExit.Invalidated);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request);
            NativeStackMemoryZeroPointer(24);
            JumpExit(FullBlockExit.NeedMemory); // stale pointers never reach a data load
            Patch(current, position);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            int ordinary = NearCondition(0x85);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x84, FullBlockExit.Invalidated);
            // Only after BOTH requests, pointers and epoch have been checked.
            Bytes(0x41, 0xc7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierConsumed);
            int authorized = NativeNearJump();
            Patch(ordinary, position);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            Patch(authorized, position); Patch(checkedMappings, position);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Consumed);
            NativeStackMemoryZeroPointer(24);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
        }

        private void NativeStackMemoryStage(int request, int temporary, FullMemoryAccess access, int width)
        {
            IntegerFrameLoad(temporary, 0); Bytes(0x49, 0x89, 0x41, (byte)request);
            NativeStackMemoryDword(request + 24, (uint)access);
            NativeStackMemoryDword(request + 28, (uint)width);
            NativeStackMemoryZeroPointer(request + 8); NativeStackMemoryZeroPointer(request + 16);
            NativeStackMemoryDword(request + 32, (uint)(width / 8));
        }

        private void NativeStackMemoryProbe(int request, FullMemoryAccess access, int width)
        {
            Bytes(0x49, 0x8b, 0x41, (byte)request); IntegerFrameStore(216, 0);
            Bytes(0x41, 0xc7, 0x82); U32(252); U32((uint)access);
            // Check canonicality of BOTH endpoints and wrapping before lookup.
            IntegerMov(1, 0); IntegerShift(4, 1, 64, 16); IntegerShift(7, 1, 64, 16);
            IntegerRR(0x39, 0, 1); ExitCondition(0x85, FullBlockExit.NeedMemory);
            IntegerConstant(0, 0, (uint)(width / 8 - 1), 64); ExitCondition(0x82, FullBlockExit.NeedMemory);
            IntegerMov(1, 0); IntegerShift(4, 1, 64, 16); IntegerShift(7, 1, 64, 16);
            IntegerRR(0x39, 0, 1); ExitCondition(0x85, FullBlockExit.NeedMemory);
            IntegerFrameLoad(216, 0); NativeTlbLookup(access);
            Bytes(0x49, 0x89, 0x51, (byte)(request + 8));
            IntegerFrameLoad(216, 0); IntegerMov(1, 0, 32); IntegerConstant(4, 1, 4095);
            IntegerConstant(7, 1, (uint)(4096 - width / 8));
            int single = NearCondition(0x86);
            IntegerImmediate(2, 4096, 32); IntegerRR(0x29, 2, 1, 32);
            Bytes(0x41, 0x89, 0x51, (byte)(request + 32));
            IntegerConstant(4, 0, 0xfffff000, 64); IntegerConstant(0, 0, 4096, 64);
            NativeTlbLookup(access); Bytes(0x49, 0x89, 0x51, (byte)(request + 16));
            Patch(single, position);
        }

        private void NativeStackMemoryValidate(int request, int temporary, FullMemoryAccess access, int width)
        {
            IntegerFrameLoad(temporary, 0); Bytes(0x49, 0x3b, 0x41, (byte)request);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            IntegerMov(1, 0); IntegerShift(4, 1, 64, 16); IntegerShift(7, 1, 64, 16);
            IntegerRR(0x39, 0, 1); ExitCondition(0x85, FullBlockExit.Invalidated);
            IntegerConstant(0, 0, (uint)(width / 8 - 1), 64); ExitCondition(0x82, FullBlockExit.Invalidated);
            IntegerMov(1, 0); IntegerShift(4, 1, 64, 16); IntegerShift(7, 1, 64, 16);
            IntegerRR(0x39, 0, 1); ExitCondition(0x85, FullBlockExit.Invalidated);
            IntegerFrameLoad(temporary, 0);
            NativeStackMemoryCompare(request + 24, (uint)access); NativeStackMemoryCompare(request + 28, (uint)width);
            Bytes(0x49, 0x83, 0x79, (byte)(request + 8), 0); ExitCondition(0x84, FullBlockExit.Invalidated);
            IntegerMov(1, 0, 32); IntegerConstant(4, 1, 4095);
            IntegerImmediate(2, 4096, 32); IntegerRR(0x29, 2, 1, 32);
            IntegerConstant(7, 2, (uint)(width / 8));
            int crossing = NearCondition(0x82);
            IntegerImmediate(2, (uint)(width / 8), 32);
            Bytes(0x49, 0x83, 0x79, (byte)(request + 16), 0); ExitCondition(0x85, FullBlockExit.Invalidated);
            int length = NativeNearJump();
            Patch(crossing, position);
            Bytes(0x49, 0x83, 0x79, (byte)(request + 16), 0); ExitCondition(0x84, FullBlockExit.Invalidated);
            Patch(length, position);
            Bytes(0x41, 0x3b, 0x51, (byte)(request + 32)); ExitCondition(0x85, FullBlockExit.Invalidated);
        }

        // Preserve R8 (the source value) while selecting destination pointers.
        private void NativeStackMemoryAdopt(int request)
        {
            NativeStackMemoryDescriptor();
            Bytes(0x49, 0x8b, 0x51, (byte)(request + 8)); IntegerFrameStore(224, 2);
            Bytes(0x49, 0x8b, 0x51, (byte)(request + 16)); IntegerFrameStore(232, 2);
            Bytes(0x41, 0x8b, 0x51, (byte)(request + 32));
            Bytes(0x41, 0x89, 0x92); U32(260);
        }
    }
}
