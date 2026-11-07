namespace Managed.Emulation.Execution;

// Fill-side observations, not recorded native branch IDs or CPU attribution.
// The second-page reasons imply that the first page passed at observation time.
internal enum FullMemoryMissReason : byte
{
    MalformedRequest,
    NonCanonicalAddress,
    AddressOverflow,
    MissingOrMalformedTlb,
    EmptyPageTag,
    ConflictPageTag,
    CachedEntryEpoch,
    FrameRuntimeEpochChanged,
    ContextMismatch,
    ReadToWritePermission,
    AccessPermission,
    EntryBounds,
    WriteBarrier,
    NullHost,
    ObservedReady,
    CrossingSecondPageEmptyPageTag,
    CrossingSecondPageConflictPageTag,
    CrossingSecondPageCachedEntryEpoch,
    CrossingSecondPageContextMismatch,
    CrossingSecondPageReadToWritePermission,
    CrossingSecondPageAccessPermission,
    CrossingSecondPageEntryBounds,
    CrossingSecondPageWriteBarrier,
    CrossingSecondPageNullHost,
    CrossingObservedReady,
    BarrierTicketEpochMismatch,
    BarrierTicketObservedReady,
    UnsupportedMultiAccess
}

internal static unsafe class FullMemoryMissDiagnostics
{
    internal const int ReasonCount = (int)FullMemoryMissReason.UnsupportedMultiAccess + 1;

    // Caller owns Authority.Gate, has authenticated the same token/site/frame,
    // and has established that MemoryTlb and any StackMemory descriptor still
    // belong to this live worker. Pointer shape alone cannot prove ownership.
    // Pass the retained EXIT frame and the current RUNTIME authority epoch,
    // before TryFill publishes entries; a new refill frame loses the old epoch.
    // No Header, Machine, epoch-pointer, or Host pointee is dereferenced here.
    // Only the owned descriptor Count and shape-checked worker TLB are read.
    //
    // Gate and worker ownership stabilize the inspected TLB, not the interval
    // since the leaf exited. Writers, revalidation and fills can change evidence.
    // ObservedReady therefore means no cause is visible now, not a native hit.
    // This does not admit a mapping, validate a lifetime, or replace TryFill.
    internal static FullMemoryMissReason Observe(in FullBlockFrame frame, ulong currentAuthorityEpoch)
    {
        // ABI5 stack requests can cover independently checked source/destination
        // accesses, even when Count=1 for LEAVE. Scalar fields cannot attribute
        // their full request. Do not inspect state, request hosts, or either TLB
        // slot: all nonzero counts remain explicitly outside these counters.
        if (frame.StackMemory != null && frame.StackMemory->Count != 0)
            return FullMemoryMissReason.UnsupportedMultiAccess;

        if (frame.MemoryResume != 1 || frame.MemorySite != frame.ResumeIndex ||
            !FullNativeMemory.ValidMemoryWidth(frame.MemoryWidth) ||
            ((uint)frame.MemoryAccess is < 1 or > 3 && frame.MemoryAccess != FullNativeMemory.AtomicReadWrite) ||
            frame.MemoryAccess == FullNativeMemory.AtomicReadWrite && frame.MemoryWidth is not (8 or 16 or 32 or 64 or 128))
            return FullMemoryMissReason.MalformedRequest;

        // NativeMemoryPrepare's exact-ticket path precedes ordinary address
        // checks and does not inspect the TLB. Its EA/site/ticket authentication
        // remains the caller's job; only its captured epoch checks are observed.
        if (frame.MemoryFlags == FullMemoryFlags.BarrierTicket)
        {
            if (frame.MemoryEpoch != frame.ObservedGlobalEpoch)
                return FullMemoryMissReason.BarrierTicketEpochMismatch;
            if (frame.ObservedGlobalEpoch != currentAuthorityEpoch)
                return FullMemoryMissReason.FrameRuntimeEpochChanged;
            return FullMemoryMissReason.BarrierTicketObservedReady;
        }

        // Valid atomics deliberately bypass every TLB slot. This existing
        // bucket records the required exclusive assist, not a cache miss.
        if (frame.MemoryAccess == FullNativeMemory.AtomicReadWrite) return FullMemoryMissReason.WriteBarrier;
        ulong address = frame.MemoryAddress;
        uint bytes = frame.MemoryWidth / 8;
        // Match NativeMemoryPrepare: starting canonicality, then unsigned
        // addition overflow, then ending canonicality, before any TLB read.
        if (!FullNativeMemory.Canonical(address))
            return FullMemoryMissReason.NonCanonicalAddress;
        if (address > ulong.MaxValue - (bytes - 1))
            return FullMemoryMissReason.AddressOverflow;
        if (!FullNativeMemory.Canonical(unchecked(address + bytes - 1)))
            return FullMemoryMissReason.NonCanonicalAddress;

        // Native accepts a configurable mask up to MaximumTlbCount - 1, but
        // today's production worker owns only TlbCount entries. The frame has
        // no capacity field: reject even otherwise-valid larger masks BEFORE
        // indexing. Smaller valid masks also stay inside that owned allocation.
        if (!FullNativeMemory.ValidTlbMask(frame.MemoryTlbMask) ||
            frame.MemoryTlbMask >= FullNativeMemory.TlbCount || frame.MemoryTlb == null)
            return FullMemoryMissReason.MissingOrMalformedTlb;

        ulong page = address & ~4095UL;
        var first = ObservePage(in frame, page, currentAuthorityEpoch, secondPage: false);
        if (first != FullMemoryMissReason.ObservedReady) return first;
        if ((address & 4095) <= 4096u - bytes) return FullMemoryMissReason.ObservedReady;
        var second = ObservePage(in frame, unchecked(page + 4096), currentAuthorityEpoch, secondPage: true);
        return second == FullMemoryMissReason.ObservedReady ? FullMemoryMissReason.CrossingObservedReady : second;
    }

    private static FullMemoryMissReason ObservePage(in FullBlockFrame frame, ulong page,
        ulong currentAuthorityEpoch, bool secondPage)
    {
        ref readonly var entry = ref frame.MemoryTlb[FullNativeMemory.TlbIndex(page, frame.MemoryTlbMask)];
        // Match NativeTlbLookup's first failing predicate. In particular, a
        // tag or cached-epoch mismatch precedes the runtime-epoch comparison;
        // bounds precede a write barrier, which precedes the null-host check.
        if (entry.Page != page)
        {
            // There is no valid bit. Only an entirely zero-valued entry is
            // labelled empty; a populated page-zero mapping is a conflict.
            bool empty = entry.Page == 0 && entry.Host == null && entry.AuthorityEpoch == 0 &&
                entry.Context == 0 && entry.Permissions == 0 && entry.Flags == 0 && entry.Identity == 0 &&
                entry.ValidStart == 0 && entry.ValidEnd == 0;
            return empty
                ? secondPage ? FullMemoryMissReason.CrossingSecondPageEmptyPageTag : FullMemoryMissReason.EmptyPageTag
                : secondPage ? FullMemoryMissReason.CrossingSecondPageConflictPageTag : FullMemoryMissReason.ConflictPageTag;
        }
        if (entry.AuthorityEpoch != frame.ObservedGlobalEpoch)
            return secondPage ? FullMemoryMissReason.CrossingSecondPageCachedEntryEpoch : FullMemoryMissReason.CachedEntryEpoch;
        if (frame.ObservedGlobalEpoch != currentAuthorityEpoch)
            return FullMemoryMissReason.FrameRuntimeEpochChanged;
        if (entry.Context != frame.Context)
            return secondPage ? FullMemoryMissReason.CrossingSecondPageContextMismatch : FullMemoryMissReason.ContextMismatch;
        if ((entry.Permissions & frame.MemoryAccess) != frame.MemoryAccess)
        {
            bool upgrade = (frame.MemoryAccess & FullMemoryAccess.Write) != 0 &&
                (entry.Permissions & FullMemoryAccess.Read) != 0 && (entry.Permissions & FullMemoryAccess.Write) == 0;
            return upgrade
                ? secondPage ? FullMemoryMissReason.CrossingSecondPageReadToWritePermission : FullMemoryMissReason.ReadToWritePermission
                : secondPage ? FullMemoryMissReason.CrossingSecondPageAccessPermission : FullMemoryMissReason.AccessPermission;
        }
        if (entry.ValidStart != 0 || entry.ValidEnd != 4096)
            return secondPage ? FullMemoryMissReason.CrossingSecondPageEntryBounds : FullMemoryMissReason.EntryBounds;
        if ((frame.MemoryAccess & FullMemoryAccess.Write) != 0 && (entry.Flags & FullMemoryFlags.WriteBarrier) != 0)
            return secondPage ? FullMemoryMissReason.CrossingSecondPageWriteBarrier : FullMemoryMissReason.WriteBarrier;
        if (entry.Host == null)
            return secondPage ? FullMemoryMissReason.CrossingSecondPageNullHost : FullMemoryMissReason.NullHost;
        return FullMemoryMissReason.ObservedReady;
    }
}
