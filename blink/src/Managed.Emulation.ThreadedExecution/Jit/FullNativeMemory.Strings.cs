using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static unsafe partial class FullNativeMemory
{
    // Explicit string grammar; stack validation is unchanged. The descriptor
    // describes only mapping requests. All address/data/index/flag computation
    // remains generated native code, and source resolution precedes destination.
    // sourceSegment comes from the authenticated ordinary Plan or issued REP IR,
    // never from mutable descriptor fields. No descriptor/ABI field is added.
    private static void ValidateStringPairRequest(FullBlockFrame* f, bool compare, int sourceSegment = -1)
    {
        var d = f->StackMemory;
        if (sourceSegment is not (-1 or 2 or 4 or 5) || d == null || d->Version != FullStackMemoryDescriptor.CurrentVersion || d->Count != 2 ||
            d->State != FullStackMemoryState.Request || d->Site != f->ResumeIndex ||
            f->MemoryResume != 1 || f->MemorySite != f->ResumeIndex ||
            d->Request0.Access != FullMemoryAccess.Read || d->Request0.Width is not (8 or 16 or 32 or 64) ||
            d->Request1.Access != (compare ? FullMemoryAccess.Read : FullMemoryAccess.Write) ||
            d->Request1.Width != d->Request0.Width)
            throw new InvalidOperationException("Invalid native string pair mapping request.");
    }

    internal static bool StringPairTicketWritesContent(FullBlockFrame* f, bool compare, int width, int sourceSegment)
    {
        ValidateStringPairRequest(f, compare, sourceSegment);
        var d = f->StackMemory;
        if (d->Request0.Width != (uint)width ||
            !((f->MemoryAddress == d->Request0.Address && f->MemoryAccess == d->Request0.Access) ||
              (f->MemoryAddress == d->Request1.Address && f->MemoryAccess == d->Request1.Access)) ||
            f->MemoryWidth != (uint)width)
            throw new InvalidOperationException("String ticket summary differs from its immutable pair grammar.");
        return !compare;
    }

    private static FullMemoryAssistResult ResolveStringRequest(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, ref FullStackMemoryRequest request, ulong epoch, bool permission, ref bool barrier, int segment = -1)
    {
        uint bytes = request.Width / 8;
        f->MemoryAddress = request.Address; f->MemoryAccess = request.Access; f->MemoryWidth = request.Width;
        if (request.Address > ulong.MaxValue - (bytes - 1) ||
            !Canonical(request.Address) || !Canonical(request.Address + bytes - 1))
        {
            f->FaultTrap = segment == 2 ? 12u : 13u; f->FaultCode = 128; f->FaultAddress = 0; f->MemoryResume = 0;
            return FullMemoryAssistResult.GuestFault;
        }
        return ResolveStackRequest(p, m, f, authority, ref request, epoch, permission, ref barrier);
    }

    internal static FullMemoryAssistResult TryFillStringPair(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, bool compare, int sourceSegment = -1)
    {
        ValidateStringPairRequest(f, compare, sourceSegment);
        ++f->MemoryAssists;
        var value = f->StackMemory[0];
        ulong epoch = authority.Epoch;
        bool barrier = false;
        var result = ResolveStringRequest(p, m, f, authority, ref value.Request0, epoch, true, ref barrier, sourceSegment);
        if (result != FullMemoryAssistResult.Resolved) return result;
        result = ResolveStringRequest(p, m, f, authority, ref value.Request1, epoch, true, ref barrier);
        if (result != FullMemoryAssistResult.Resolved) return result;
        if (authority.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        if (barrier) return FullMemoryAssistResult.WriteBarrier;
        PublishStackTlb(f, in value.Request0, epoch);
        PublishStackTlb(f, in value.Request1, epoch);
        value.Epoch = epoch; value.Ticket = 0; value.State = FullStackMemoryState.Ready;
        f->StackMemory[0] = value; f->MemoryEpoch = epoch;
        return FullMemoryAssistResult.Resolved;
    }

    internal static FullMemoryAssistResult InspectStringPairTicket(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, bool compare, out nint page0, out nint page1, int sourceSegment = -1)
    {
        ValidateStringPairRequest(f, compare, sourceSegment);
        page0 = page1 = 0;
        var value = f->StackMemory[0];
        bool ignored = false;
        var result = ResolveStringRequest(p, m, f, authority, ref value.Request0, authority.Epoch, false, ref ignored, sourceSegment);
        if (result != FullMemoryAssistResult.Resolved) return result;
        result = ResolveStringRequest(p, m, f, authority, ref value.Request1, authority.Epoch, false, ref ignored);
        if (result != FullMemoryAssistResult.Resolved) return result;
        // A CMPS ticket is read-only. These pages select conservative authority
        // revocation/exclusion; no generated store or RW permission is invented.
        page0 = (nint)(value.Request1.Host0 - (int)(value.Request1.Address & 4095));
        page1 = (nint)value.Request1.Host1;
        f->StackMemory[0] = value;
        return FullMemoryAssistResult.Resolved;
    }

    internal static void PublishStringPairTicket(FullBlockFrame* f, ulong epoch, ulong ticket, bool compare)
    {
        ValidateStringPairRequest(f, compare);
        if (ticket == 0 || epoch != f->ObservedGlobalEpoch)
            throw new InvalidOperationException("Invalid native string pair ticket publication.");
        f->StackMemory->Epoch = epoch; f->StackMemory->Ticket = ticket;
        f->StackMemory->State = FullStackMemoryState.TicketReady;
        f->MemoryEpoch = epoch; f->MemoryFlags = FullMemoryFlags.BarrierTicket;
    }
}
