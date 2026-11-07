using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static unsafe partial class FullNativeMemory
{
    // A permission proof authorizes no data access, writable pointer or ticket.
    // Suppressed numeric stores require their operand span. FX state transfers
    // require all 512 bytes even though their defined data span is 416 bytes.
    internal static bool IsCanonicalX87MemoryProbe(FullBlockFrame* frame) =>
        frame != null && frame->StackMemory != null &&
        frame->StackMemory->Version == FullCanonicalX87MemoryLowering.MemoryProbeTag;

    private static bool CanonicalX87ProbeShape(FullBlockFrame* frame, FullStackMemoryState state)
    {
        if (!IsCanonicalX87MemoryProbe(frame)) return false;
        var d = frame->StackMemory;
        ref var r = ref d->Request0;
        bool access = r.Width == 4096 ? r.Access is FullMemoryAccess.Read or FullMemoryAccess.Write :
            r.Access == FullMemoryAccess.Write && r.Width is 16 or 32 or 64 or 80;
        return access && d->Count == 1 && d->State == state && d->Site == frame->ResumeIndex &&
            frame->MemorySite == frame->ResumeIndex && frame->MemoryResume == 1 &&
            frame->MemoryAccess == r.Access && frame->MemoryWidth == r.Width &&
            frame->MemoryAddress == r.Address && frame->MemoryFlags == FullMemoryFlags.None &&
            frame->MemoryTicket == 0 && d->Ticket == 0 &&
            r.Host0 == null && r.Host1 == null && r.Length0 == 0 &&
            r.Reserved36 == 0 && r.Reserved40 == 0 &&
            d->Request1.Address == 0 && d->Request1.Host0 == null && d->Request1.Host1 == null &&
            d->Request1.Access == 0 && d->Request1.Width == 0 && d->Request1.Length0 == 0 &&
            d->Request1.Reserved36 == 0 && d->Request1.Reserved40 == 0;
    }

    internal static ulong CanonicalX87MemoryAddress(in JitInstruction instruction, Blink.Machine* machine)
    {
        var a = instruction.Address;
        ulong ea = unchecked((ulong)a.Displacement +
            (a.RipRelative ? instruction.Ip + (uint)instruction.Length : 0));
        ulong* registers = (ulong*)((byte*)machine + 24);
        if (a.Base >= 0) ea = unchecked(ea + registers[a.Base]);
        if (a.Index >= 0) ea = unchecked(ea + (registers[a.Index] << a.Scale));
        if (a.AddressWidth == 32) ea = (uint)ea;
        if (a.Segment == 4) ea = unchecked(ea + machine->fs.@base);
        if (a.Segment == 5) ea = unchecked(ea + machine->gs.@base);
        return ea;
    }

    internal static bool IsCanonicalX87FxData(in JitInstruction instruction, out FullMemoryAccess access)
    {
        access = 0;
        if (!FullBlockEmitter.CanInlineCanonicalX87Control(instruction)) return false;
        try { FullBlockEmitter.ValidateCanonicalX87Control(instruction); }
        catch (ArgumentException) { return false; }
        var op = (FullCanonicalX87ControlOperation)(instruction.Options & 0xffff);
        if (instruction.SourceWidth != 3328 || op is not
            (FullCanonicalX87ControlOperation.Fxsave or FullCanonicalX87ControlOperation.Fxrstor)) return false;
        access = op == FullCanonicalX87ControlOperation.Fxsave ? FullMemoryAccess.Write : FullMemoryAccess.Read;
        return true;
    }

    // Header/lease/context authentication remains the controller's obligation.
    internal static bool AuthenticateCanonicalX87MemoryProbe(FullBlockFrame* frame,
        in JitInstruction instruction, Blink.Machine* machine)
    {
        if (frame == null || machine == null || frame->Machine != machine ||
            !CanonicalX87ProbeShape(frame, FullStackMemoryState.Request) || machine->ip != instruction.Ip)
            return false;
        bool store;
        try
        {
            if (IsCanonicalX87FxData(in instruction, out var access))
            {
                if (frame->MemoryWidth != 4096 || frame->MemoryAccess != access) return false;
                return frame->MemoryAddress == CanonicalX87MemoryAddress(in instruction, machine);
            }
            if (FullBlockEmitter.CanInlineCanonicalX87Memory(instruction))
            {
                FullBlockEmitter.ValidateCanonicalX87Memory(instruction);
                store = !FullCanonicalX87MemoryLowering.IsLoad(
                    (FullCanonicalX87MemoryOperation)(instruction.Options & 0xffff));
            }
            else if (FullBlockEmitter.CanInlineCanonicalX87MemoryArithmetic(instruction))
            {
                FullBlockEmitter.ValidateCanonicalX87MemoryArithmetic(instruction);
                store = instruction.Options == (uint)FullCanonicalX87MemoryArithmeticOperation.Fbstp;
            }
            else return false;
        }
        catch (ArgumentException) { return false; }
        return store && frame->MemoryWidth == (uint)instruction.SourceWidth &&
            frame->MemoryAccess == FullMemoryAccess.Write &&
            frame->MemoryAddress == CanonicalX87MemoryAddress(in instruction, machine);
    }

    internal static FullMemoryAssistResult TryFillCanonicalX87MemoryProbe(Blink program,
        Blink.Machine* machine, FullBlockFrame* frame, FullCodeAuthority authority)
    {
        if (!Monitor.IsEntered(authority.Gate))
            throw new InvalidOperationException("x87 permission proof needs authority gate.");
        if (machine == null || frame == null || frame->Machine != machine ||
            !CanonicalX87ProbeShape(frame, FullStackMemoryState.Request))
            throw new InvalidOperationException("Invalid x87 permission proof.");
        ++frame->MemoryAssists;
        ulong epoch = authority.Epoch;
        // Resolve on a LOCAL value: backing pointers never escape into the
        // proof, TLB, physical writer registry or exclusive ticket.
        var request = frame->StackMemory->Request0;
        bool ignored = false;
        var result = ResolveStackRequest(program, machine, frame, authority,
            ref request, epoch, false, ref ignored);
        if (result != FullMemoryAssistResult.Resolved) return result;
        if (authority.Epoch != epoch || frame->ObservedGlobalEpoch != epoch)
            return FullMemoryAssistResult.Retry;
        frame->StackMemory->Epoch = epoch;
        frame->StackMemory->State = FullStackMemoryState.Ready;
        frame->MemoryEpoch = epoch;
        return FullMemoryAssistResult.Resolved;
    }
}
