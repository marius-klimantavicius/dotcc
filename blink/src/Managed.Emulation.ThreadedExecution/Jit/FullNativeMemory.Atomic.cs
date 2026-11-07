using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static unsafe partial class FullNativeMemory
{
    static partial void AtomicTicketMapped(FullBlockFrame* frame); // erased production-only test seam

    internal const FullMemoryAccess AtomicReadWrite = FullMemoryAccess.Read | FullMemoryAccess.Write | FullMemoryAccess.Atomic;
    internal static bool IsAtomicRequest(FullBlockFrame* f) => f != null && f->MemoryAccess == AtomicReadWrite;

    // Mapping/authentication only. Neither this path nor its controller adapter
    // reads the operand, changes flags/registers, or implements an RMW.
    private static void ValidateAtomicRequest(FullBlockFrame* f)
    {
        if (!IsAtomicRequest(f) || f->MemoryResume != 1 || f->MemorySite != f->ResumeIndex ||
            f->MemoryWidth is not (8 or 16 or 32 or 64 or 128) ||
            f->StackMemory != null && (f->StackMemory->Version != FullStackMemoryDescriptor.CurrentVersion || f->StackMemory->Count != 0))
            throw new InvalidOperationException("Invalid dense atomic mapping request.");
    }

    internal static ulong AtomicEffectiveAddress(in JitInstruction i, Blink.Machine* m)
    {
        var a = i.Address;
        ulong* registers = (ulong*)((byte*)m + 24);
        ulong value = a.RipRelative ? unchecked(i.Ip + (uint)i.Length) : a.Base < 0 ? 0 : registers[a.Base];
        if (a.Index >= 0) value = unchecked(value + (registers[a.Index] << a.Scale));
        value = unchecked(value + (ulong)a.Displacement);
        var operation = FullBlockEmitter.AtomicOperation(i);
        if (operation is FullAtomicOperation.BitSet or FullAtomicOperation.BitReset or FullAtomicOperation.BitComplement && !i.Source.IsImmediate)
        {
            ulong raw = registers[i.Source.Register];
            long bit = i.Width == 16 ? (short)raw : i.Width == 32 ? (int)raw : unchecked((long)raw);
            value = unchecked(value + (ulong)((bit & -(long)i.Width) >> 3));
        }
        // R118/R119: truncate the complete offset (including signed bitdisp),
        // then add the full FS/GS base. Never mask the segmented result.
        if (a.AddressWidth == 32) value = (uint)value;
        if (a.Segment == 4) value = unchecked(value + m->fs.@base);
        if (a.Segment == 5) value = unchecked(value + m->gs.@base);
        return value;
    }

    internal static bool AuthenticateAtomicRequest(FullBlockFrame* f, in JitInstruction i, Blink.Machine* m)
    {
        if (m == null || f == null || f->Machine != m || !FullBlockEmitter.CanInlineAtomic(i)) return false;
        try { FullBlockEmitter.ValidateAtomic(i); ValidateAtomicRequest(f); }
        catch (ArgumentException) { return false; }
        catch (InvalidOperationException) { return false; }
        return m->ip == i.Ip && f->MemoryWidth == (uint)i.Width && f->MemoryAddress == AtomicEffectiveAddress(in i, m) &&
            f->FsBase == m->fs.@base && f->GsBase == m->gs.@base;
    }

    private static FullMemoryAssistResult TryFillAtomic(Blink p, Blink.Machine* m, FullBlockFrame* f, FullCodeAuthority authority)
    {
        ++f->MemoryAssists;
        var result = InspectAtomicTicket(p, m, f, authority, out _, out _, out _, out _, out _);
        // No atomic TLB publication, even for known nonCode private pages.
        if (result == FullMemoryAssistResult.Resolved)
        { f->MemoryFlags = FullMemoryFlags.WriteBarrier; return FullMemoryAssistResult.WriteBarrier; }
        return result;
    }

    // Typed caller only: generic TryFillAtomic remains a ticket-only mapping.
    // Pair8 has width64 too, so width alone must never grant this path.
    internal static FullMemoryAssistResult TryFillScalarAtomic(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, in JitInstruction instruction)
    {
        if (!Monitor.IsEntered(authority.Gate)) throw new InvalidOperationException("Atomic fill needs authority gate.");
        if (!AuthenticateAtomicRequest(f, in instruction, m))
            throw new InvalidOperationException("Scalar atomic mapping lacks exact typed plan authentication.");
        if (!FullBlockEmitter.ScalarAtomicFast(instruction) ||
            (f->MemoryAddress & (f->MemoryWidth / 8 - 1)) != 0)
            return TryFillAtomic(p, m, f, authority);
        if (f->MemoryTlb == null || !ValidTlbMask(f->MemoryTlbMask))
            throw new InvalidOperationException("Scalar atomic mapping lacks worker TLB.");
        ++f->MemoryAssists;
        authority.ResetDataPermissionDenial();
        // Reuse full-span RW/EOF/host prechecks and preserve the c129 test seam
        // and its final epoch check. No operand is loaded during mapping.
        var result = InspectAtomicTicket(p, m, f, authority, out _, out _, out _, out nint page, out _);
        if (result != FullMemoryAssistResult.Resolved) return result;
        ulong epoch = authority.Epoch;
        // Unknown physical identities keep the existing ticket route. Do not
        // create new write authority merely because the guest PTE has PAGE_RW.
        if (!authority.HasScalarAtomicMappingAuthority(page, epoch))
        { f->MemoryFlags = FullMemoryFlags.WriteBarrier; return FullMemoryAssistResult.WriteBarrier; }
        result = ResolvePage(p, m, f, authority, f->MemoryAddress & ~4095UL, epoch, out var entry);
        if (result == FullMemoryAssistResult.UnsupportedBacking)
        { f->MemoryFlags = FullMemoryFlags.WriteBarrier; return FullMemoryAssistResult.WriteBarrier; }
        if (result != FullMemoryAssistResult.Resolved) return result;
        if (authority.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        if (entry.Permissions != (FullMemoryAccess.Read | FullMemoryAccess.Write) ||
            entry.Flags != FullMemoryFlags.None || entry.Host == null || entry.Identity != (ulong)entry.Host ||
            entry.ValidStart != 0 || entry.ValidEnd != 4096)
            throw new InvalidOperationException("Scalar atomic mapper lost ordinary native write authority.");
        f->MemoryTlb[TlbIndex(entry.Page, f->MemoryTlbMask)] = entry;
        f->MemoryFlags = FullMemoryFlags.None; f->MemoryTicket = 0; f->MemoryEpoch = epoch;
        return FullMemoryAssistResult.Resolved;
    }

    // Called under Gate AFTER actual native enrollment. This is admission
    // authentication; Flags.None on its own is never a capability.
    internal static bool AuthenticateScalarAtomicMapping(FullBlockFrame* f, in JitInstruction i,
        Blink.Machine* m, FullCodeAuthority authority, bool enrolled)
    {
        if (!Monitor.IsEntered(authority.Gate) || !enrolled ||
            !AuthenticateAtomicRequest(f, in i, m) || !FullBlockEmitter.ScalarAtomicFast(i) ||
            f->MemoryFlags != FullMemoryFlags.None || f->MemoryTicket != 0 ||
            f->MemoryTlb == null || !ValidTlbMask(f->MemoryTlbMask) ||
            f->MemoryEpoch != f->ObservedGlobalEpoch || authority.Epoch != f->ObservedGlobalEpoch ||
            (f->MemoryAddress & (f->MemoryWidth / 8 - 1)) != 0) return false;
        ref var e = ref f->MemoryTlb[TlbIndex(f->MemoryAddress, f->MemoryTlbMask)];
        return e.Page == (f->MemoryAddress & ~4095UL) && e.Host != null && ((ulong)e.Host & 4095) == 0 &&
            e.Identity == (ulong)e.Host && e.AuthorityEpoch == f->ObservedGlobalEpoch && e.Context == f->Context &&
            e.Flags == FullMemoryFlags.None && e.Permissions == (FullMemoryAccess.Read | FullMemoryAccess.Write) &&
            e.ValidStart == 0 && e.ValidEnd == 4096 &&
            authority.HasScalarAtomicWriteAuthority((nint)e.Host, f->ObservedGlobalEpoch);
    }

    internal static FullMemoryAssistResult InspectAtomicTicket(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, out byte* first, out byte* second, out uint firstLength, out nint page0, out nint page1)
    {
        if (!Monitor.IsEntered(authority.Gate)) throw new InvalidOperationException("Atomic inspection needs authority gate.");
        ValidateAtomicRequest(f);
        first = second = null; firstLength = 0; page0 = page1 = 0;
        uint bytes = f->MemoryWidth / 8;
        ulong address = f->MemoryAddress, epoch = authority.Epoch;
        if (address > ulong.MaxValue - (bytes - 1) || !Canonical(address) || !Canonical(unchecked(address + bytes - 1)))
        {
            // Production emission catches this before NeedMemory and chooses
            // #SS from the decoded address. A direct assist has no segment IR.
            f->FaultTrap = 13; f->FaultCode = 128; f->FaultAddress = 0;
            return FullMemoryAssistResult.GuestFault;
        }
        // Width128 is the authenticated CMPXCHG16B pair grammar. Canonical
        // endpoints above precede alignment; no PTE/host query precedes it.
        // Direct assist/inspection callers get the same precise #GP as native.
        if (f->MemoryWidth == 128 && (address & 15) != 0)
        {
            f->FaultTrap = 13; f->FaultCode = 128; f->FaultAddress = 0;
            return FullMemoryAssistResult.GuestFault;
        }
        firstLength = Math.Min(bytes, 4096u - (uint)(address & 4095));
        var result = ResolveAtomicPage(p, m, f, authority, address, out byte* a);
        if (result != FullMemoryAssistResult.Resolved) return result;
        byte* b = null;
        if (firstLength != bytes)
        {
            result = ResolveAtomicPage(p, m, f, authority, (address & ~4095UL) + 4096, out b);
            if (result != FullMemoryAssistResult.Resolved) return result;
        }
        AtomicTicketMapped(f); // read-only full-span proof, before final epoch authentication
        if (authority.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        // Publish outputs together only after BOTH RW/EOF checks. A failed CAS
        // still needs writable mappings; comparison is done later in native.
        page0 = (nint)a; page1 = (nint)b;
        first = a + (int)(address & 4095); second = b;
        return FullMemoryAssistResult.Resolved;
    }

    private static FullMemoryAssistResult ResolveAtomicPage(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, ulong address, out byte* host)
    {
        host = null;
        if (!JitPageIdentity.TryRead(p, m, address, out ulong pte) || (pte & (ulong)Blink.PAGE_V) == 0)
            return Fault(f, address, 1);
        if ((pte & (ulong)(Blink.PAGE_U | Blink.PAGE_RW)) != (ulong)(Blink.PAGE_U | Blink.PAGE_RW))
            return Fault(f, address, 2);
        if ((pte & (ulong)Blink.PAGE_RSRV) != 0) return FullMemoryAssistResult.DemandPage;
        byte* candidate = (pte & (ulong)Blink.PAGE_HOST) != 0 ? p.FindHostPage(pte) : p.GetPageAddress(m->blink_host_system, pte, false);
        if (candidate == null || p.BlinkHostMemoryContains(candidate, 4096) == 0) return FullMemoryAssistResult.UnsupportedBacking;
        if ((pte & (ulong)Blink.PAGE_HOST) != 0)
        {
            if (p.BlinkHostMemoryFilePageAvailable(candidate) == 0) return Fault(f, address, unchecked((uint)-2));
            if (p.BlinkHostMemoryFilePageShared(candidate) != 0 &&
                !authority.AtomicBackingOwned(p.BlinkHostMemoryFilePageLease(candidate)))
                return FullMemoryAssistResult.UnsupportedBacking;
        }
        host = candidate; return FullMemoryAssistResult.Resolved;
    }
}

internal sealed unsafe partial class FullCodeAuthority
{
    partial void AtomicHostReaderPending();
    internal bool ScalarAtomicAdmissionClosing(ulong checkedEpoch) =>
        Epoch != checkedEpoch || Volatile.Read(ref hostWriters) != 0 ||
        Volatile.Read(ref mappingWriters) != 0 || Volatile.Read(ref copyWriters) != 0 ||
        PendingAtomicWorker != -1 || !SemanticReadersDrained();

    // Pure reauthentication after enrollment; never register an unknown page
    // here (registration drains nativeFrames and would wait for this frame).
    internal bool HasScalarAtomicWriteAuthority(nint page, ulong checkedEpoch) =>
        Volatile.Read(ref nativeFrames) > 0 && !ScalarAtomicAdmissionClosing(checkedEpoch) &&
        HasScalarAtomicMappingAuthority(page, checkedEpoch);

    internal bool HasScalarAtomicMappingAuthority(nint page, ulong checkedEpoch)
    {
        if (!Monitor.IsEntered(Gate) || page == 0 || ((nuint)page & 4095) != 0 ||
            saturated || Volatile.Read(ref epochExhausted) != 0 || Epoch != checkedEpoch ||
            Volatile.Read(ref hostWriters) != 0 ||
            Volatile.Read(ref mappingWriters) != 0 || Volatile.Read(ref copyWriters) != 0 ||
            PendingAtomicWorker != -1 || !SemanticReadersDrained()) return false;
        int at = Find(page);
        return at >= 0 && cells[at].Address == page && !cells[at].Code && cells[at].Writers < 0;
    }

    internal bool AtomicBackingOwned(ulong lease) => lease != 0 && io.MemoryFileMutationOwned(lease);
}
