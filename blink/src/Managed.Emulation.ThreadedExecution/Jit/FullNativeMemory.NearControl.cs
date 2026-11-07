using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Mapping-only near CALL pair adapter. Native owns the target read/check and
// the sole return-address store. Preserve the dense descriptor/ticket protocol.
internal static unsafe partial class FullNativeMemory
{
    internal static bool AuthenticateNearCall(FullBlockFrame* f, in JitInstruction i, Blink.Machine* m)
    {
        if (f == null || m == null || f->Machine != m || !IsStackRequest(f) ||
            i.Operation != JitOperation.IndirectCall || !i.Source.IsMemory ||
            !FullBlockEmitter.CanInlineNearControl(i)) return false;
        var d = f->StackMemory;
        ulong source = CanonicalX87MemoryAddress(in i, m); // ordinary EA arithmetic ONLY
        ulong destination = unchecked(((ulong*)((byte*)m + 56))[0] - 8);
        return m->ip == i.Ip && d->Count == 2 && d->Site == f->ResumeIndex &&
            f->MemorySite == f->ResumeIndex && f->MemoryResume == 1 && f->MemoryWidth == 64 &&
            d->Request0.Address == source && d->Request0.Access == FullMemoryAccess.Read && d->Request0.Width == 64 &&
            d->Request1.Address == destination && d->Request1.Access == FullMemoryAccess.Write && d->Request1.Width == 64 &&
            f->FsBase == m->fs.@base && f->GsBase == m->gs.@base &&
            ((f->MemoryAddress == source && f->MemoryAccess == FullMemoryAccess.Read) ||
             (f->MemoryAddress == destination && f->MemoryAccess == FullMemoryAccess.Write)) &&
            d->State is >= FullStackMemoryState.Request and <= FullStackMemoryState.TicketReady;
    }

    private static FullMemoryAssistResult ResolveNearCallRequest(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority a, ref FullStackMemoryRequest r, ulong epoch, bool permission, bool stack, ref bool barrier)
    {
        f->MemoryAddress = r.Address; f->MemoryAccess = r.Access; f->MemoryWidth = 64;
        if (r.Address > ulong.MaxValue - 7 || !Canonical(r.Address) || !Canonical(unchecked(r.Address + 7)))
        {
            f->FaultTrap = stack ? 12u : 13u; f->FaultCode = 128; f->FaultAddress = 0; f->MemoryResume = 0;
            return FullMemoryAssistResult.GuestFault;
        }
        return ResolveStackRequest(p, m, f, a, ref r, epoch, permission, ref barrier);
    }

    internal static FullMemoryAssistResult TryFillNearCall(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority a, in JitInstruction i)
    {
        if (!Monitor.IsEntered(a.Gate)) throw new InvalidOperationException("Near CALL mapping needs authority gate");
        ValidateStackRequest(f);
        if (!AuthenticateNearCall(f, in i, m)) throw new InvalidOperationException("Near CALL mapping differs from exact plan/state");
        ++f->MemoryAssists; var value = *f->StackMemory; ulong epoch = a.Epoch; bool barrier = false;
        // Source PTE/backing faults precede stack PTE/backing faults, which
        // precede target-canonical #GP. No scalar value read by this adapter.
        var result = ResolveNearCallRequest(p, m, f, a, ref value.Request0, epoch, true,
            FullScalarMiscLowering.UsesStackSegment(i.Address), ref barrier);
        if (result != FullMemoryAssistResult.Resolved) return result;
        result = ResolveNearCallRequest(p, m, f, a, ref value.Request1, epoch, true, true, ref barrier);
        if (result != FullMemoryAssistResult.Resolved) return result;
        if (a.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        if (barrier) return FullMemoryAssistResult.WriteBarrier;
        PublishStackTlb(f, in value.Request0, epoch); PublishStackTlb(f, in value.Request1, epoch);
        value.Epoch = epoch; value.Ticket = 0; value.State = FullStackMemoryState.Ready;
        *f->StackMemory = value; f->MemoryEpoch = epoch;
        return FullMemoryAssistResult.Resolved;
    }

    internal static FullMemoryAssistResult InspectNearCallTicket(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority a, in JitInstruction i, out nint page0, out nint page1)
    {
        ValidateStackRequest(f); page0 = page1 = 0;
        if (!AuthenticateNearCall(f, in i, m)) throw new InvalidOperationException("Near CALL ticket differs from exact plan/state");
        var value = *f->StackMemory; ulong epoch = a.Epoch; bool ignored = false;
        var result = ResolveNearCallRequest(p, m, f, a, ref value.Request0, epoch, false,
            FullScalarMiscLowering.UsesStackSegment(i.Address), ref ignored);
        if (result != FullMemoryAssistResult.Resolved) return result;
        result = ResolveNearCallRequest(p, m, f, a, ref value.Request1, epoch, false, true, ref ignored);
        if (result != FullMemoryAssistResult.Resolved) return result;
        if (a.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        page0 = (nint)(value.Request1.Host0 - (int)(value.Request1.Address & 4095)); page1 = (nint)value.Request1.Host1;
        *f->StackMemory = value;
        return FullMemoryAssistResult.Resolved;
    }
}
