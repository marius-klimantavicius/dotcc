using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static unsafe partial class FullNativeMemory
{
    // A descriptor is a mapping request, never an operation/helper dispatch.
    // Resolve both guest accesses in architectural order and publish all four
    // independent pointers together. Direct TLB collisions cannot lose one.
    internal static bool IsStackRequest(FullBlockFrame* f) => f->StackMemory != null && f->StackMemory->Version == FullStackMemoryDescriptor.CurrentVersion && f->StackMemory->Count != 0;

    private static void ValidateStackRequest(FullBlockFrame* f)
    {
        var d = f->StackMemory;
        if (d == null || d->Version != FullStackMemoryDescriptor.CurrentVersion || d->Count is < 1 or > 2 ||
            d->State != FullStackMemoryState.Request || d->Site != f->ResumeIndex ||
            f->MemoryResume != 1 || f->MemorySite != f->ResumeIndex ||
            d->Request0.Access != FullMemoryAccess.Read || d->Request0.Width is not (16 or 64) ||
            d->Count == 2 && (d->Request1.Access != FullMemoryAccess.Write || d->Request1.Width != d->Request0.Width))
            throw new InvalidOperationException("Invalid native stack memory request.");
    }

    // Classify the COMPLETE authenticated instruction/descriptor, never its
    // summary (which may still name a pair's read operand). Only LEAVE is a
    // pure read in this grammar; PUSH/POP memory and CALL all write.
    internal static bool StackTicketWritesContent(FullBlockFrame* f, in JitInstruction i, Blink.Machine* m)
    {
        ValidateStackRequest(f);
        if (f->Machine != m || m->ip != i.Ip || !FullBlockEmitter.ValidateStackMemory(i))
            throw new InvalidOperationException("Stack ticket differs from its immutable plan/state.");
        var d = f->StackMemory;
        uint count = i.Operation == JitOperation.Leave ? 1u : 2u;
        if (d->Count != count || d->Request0.Width != (uint)i.Width)
            throw new InvalidOperationException("Stack ticket differs from its immutable operand grammar.");
        ulong* registers = (ulong*)((byte*)m + 24);
        ulong source = i.Operation == JitOperation.Push ? CanonicalX87MemoryAddress(in i, m) :
            registers[i.Operation == JitOperation.Leave ? 5 : 4];
        if (d->Request0.Address != source)
            throw new InvalidOperationException("Stack ticket source differs from its immutable plan/state.");
        if (count == 2)
        {
            ulong destination;
            if (i.Operation == JitOperation.Push) destination = unchecked(registers[4] - (uint)i.Width / 8);
            else
            {
                // POP EA uses post-increment RSP, BEFORE EA32 truncation and
                // FS/GS addition. Validate addresses only; native owns data/RSP.
                var a = i.Address;
                long delta = (a.Base == 4 ? i.Width / 8 : 0) +
                    (a.Index == 4 ? (i.Width / 8 << a.Scale) : 0);
                var post = i with { Address = a with { Displacement = unchecked(a.Displacement + delta) } };
                destination = CanonicalX87MemoryAddress(in post, m);
            }
            if (d->Request1.Address != destination)
                throw new InvalidOperationException("Stack ticket destination differs from its immutable plan/state.");
        }
        return count == 2;
    }

    private static FullMemoryAssistResult TryFillStack(Blink p, Blink.Machine* m, FullBlockFrame* f, FullCodeAuthority authority)
    {
        ValidateStackRequest(f);
        ++f->MemoryAssists;
        var value = *f->StackMemory;
        ulong epoch = authority.Epoch;
        bool barrier = false;
        for (int n = 0; n < value.Count; ++n)
        {
            ref var request = ref (n == 0 ? ref value.Request0 : ref value.Request1);
            var result = ResolveStackRequest(p, m, f, authority, ref request, epoch, true, ref barrier);
            if (result != FullMemoryAssistResult.Resolved) return result;
        }
        if (authority.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        if (barrier) return FullMemoryAssistResult.WriteBarrier;
        // Warm later invocations use the ordinary checked lookup. Even when
        // these direct-map slots collide, the complete Ready descriptor below
        // retains both current accesses independently and makes forward progress.
        PublishStackTlb(f, in value.Request0, epoch);
        if (value.Count == 2) PublishStackTlb(f, in value.Request1, epoch);
        value.Epoch = epoch; value.Ticket = 0; value.State = FullStackMemoryState.Ready;
        *f->StackMemory = value;
        f->MemoryEpoch = epoch;
        return FullMemoryAssistResult.Resolved;
    }

    private static void PublishStackTlb(FullBlockFrame* f, in FullStackMemoryRequest r, ulong epoch)
    {
        if (f->MemoryTlb == null || !ValidTlbMask(f->MemoryTlbMask))
            throw new InvalidOperationException("Native stack mapping lacks its owned worker TLB.");
        ulong page = r.Address & ~4095UL;
        byte* host = r.Host0 - (int)(r.Address & 4095);
        var entry = new FullMemoryTlbEntry { Page = page, Host = host, AuthorityEpoch = epoch,
            Context = f->Context, Permissions = FullMemoryAccess.Read | r.Access,
            Identity = (ulong)host, ValidStart = 0, ValidEnd = 4096 };
        f->MemoryTlb[TlbIndex(page, f->MemoryTlbMask)] = entry;
        if (r.Host1 != null)
        {
            entry.Page = page + 4096; entry.Host = r.Host1; entry.Identity = (ulong)r.Host1;
            f->MemoryTlb[TlbIndex(entry.Page, f->MemoryTlbMask)] = entry;
        }
    }

    // The controller calls this before promotion and AGAIN at the final
    // authenticated header epoch. Only the final inspected value is published.
    // The sole write occupies at most two physical pages; source pointers stay
    // protected by the same passive lifetime/native enrollment through RET.
    internal static FullMemoryAssistResult InspectStackTicket(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, out nint page0, out nint page1)
    {
        ValidateStackRequest(f);
        page0 = page1 = 0;
        var value = *f->StackMemory;
        bool ignored = false;
        for (int n = 0; n < value.Count; ++n)
        {
            ref var request = ref (n == 0 ? ref value.Request0 : ref value.Request1);
            var result = ResolveStackRequest(p, m, f, authority, ref request, authority.Epoch, false, ref ignored);
            if (result != FullMemoryAssistResult.Resolved) return result;
        }
        var destination = value.Count == 2 ? value.Request1 : value.Request0;
        page0 = (nint)(destination.Host0 - (int)(destination.Address & 4095));
        page1 = (nint)destination.Host1;
        // Request remains non-executable until final explicit publication.
        *f->StackMemory = value;
        return FullMemoryAssistResult.Resolved;
    }

    private static FullMemoryAssistResult ResolveStackRequest(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, ref FullStackMemoryRequest request, ulong epoch, bool checkPermission, ref bool barrier)
    {
        uint bytes = request.Width / 8;
        f->MemoryAddress = request.Address; f->MemoryAccess = request.Access; f->MemoryWidth = request.Width;
        if (request.Address > ulong.MaxValue - (bytes - 1) ||
            !Canonical(request.Address) || !Canonical(request.Address + bytes - 1))
            return Fault(f, request.Address, 1);
        uint firstLength = Math.Min(bytes, 4096u - (uint)(request.Address & 4095));
        var result = ResolveStackPage(p, m, f, authority, request.Address, epoch, checkPermission, ref barrier, out byte* a);
        if (result != FullMemoryAssistResult.Resolved) return result;
        byte* b = null;
        if (firstLength != bytes)
        {
            result = ResolveStackPage(p, m, f, authority, (request.Address & ~4095UL) + 4096,
                epoch, checkPermission, ref barrier, out b);
            if (result != FullMemoryAssistResult.Resolved) return result;
        }
        request.Host0 = a + (int)(request.Address & 4095);
        request.Host1 = b; request.Length0 = firstLength;
        request.Reserved36 = 0; request.Reserved40 = 0;
        return FullMemoryAssistResult.Resolved;
    }

    private static FullMemoryAssistResult ResolveStackPage(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, ulong address, ulong epoch, bool checkPermission, ref bool barrier, out byte* host)
    {
        host = null;
        if (!JitPageIdentity.TryRead(p, m, address, out ulong pte) || (pte & (ulong)Blink.PAGE_V) == 0)
            return Fault(f, address, 1);
        if ((pte & (ulong)Blink.PAGE_U) == 0 ||
            (f->MemoryAccess & FullMemoryAccess.Write) != 0 && (pte & (ulong)Blink.PAGE_RW) == 0)
            return Fault(f, address, 2);
        // Preserve the failing request's original EA/width/access. The existing
        // materializer probes both endpoints, including a missing second page.
        if ((pte & (ulong)Blink.PAGE_RSRV) != 0) return FullMemoryAssistResult.DemandPage;
        host = (pte & (ulong)Blink.PAGE_HOST) != 0 ? p.FindHostPage(pte) : p.GetPageAddress(m->blink_host_system, pte, false);
        if (host == null || p.BlinkHostMemoryContains(host, 4096) == 0)
            return FullMemoryAssistResult.UnsupportedBacking;
        if ((pte & (ulong)Blink.PAGE_HOST) != 0 && p.BlinkHostMemoryFilePageAvailable(host) == 0)
            return Fault(f, address, unchecked((uint)-2));
        if (checkPermission)
        {
            bool allowed = authority.TryGetNativeDataPermission((nint)host,
                (f->MemoryAccess & FullMemoryAccess.Write) != 0, out ulong checkedEpoch);
            if (checkedEpoch != epoch) return FullMemoryAssistResult.Retry;
            bool shared = (pte & (ulong)Blink.PAGE_HOST) != 0 && p.BlinkHostMemoryFilePageShared(host) != 0;
            // Resolve both operands before publishing a Ready descriptor. Only
            // this operand's pure Read may retain owned shared backing; every
            // shared Write and every unproved shared Read keeps its old ticket.
            bool retainedSharedRead = shared && f->MemoryAccess == FullMemoryAccess.Read &&
                authority.SharedReadBackingOwned(p.BlinkHostMemoryFilePageLease(host), (nint)host);
            barrier |= !allowed || (shared && !retainedSharedRead);
        }
        return FullMemoryAssistResult.Resolved;
    }

    internal static void PublishStackTicket(FullBlockFrame* f, ulong epoch, ulong ticket)
    {
        ValidateStackRequest(f);
        if (ticket == 0 || epoch != f->ObservedGlobalEpoch)
            throw new InvalidOperationException("Invalid native stack ticket publication.");
        f->StackMemory->Epoch = epoch; f->StackMemory->Ticket = ticket;
        f->StackMemory->State = FullStackMemoryState.TicketReady;
        f->MemoryEpoch = epoch; f->MemoryFlags = FullMemoryFlags.BarrierTicket;
    }
}
