namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    // Managed owner state only. Not an execution permit or a code stamp.
    // Default Advance/TryAdvance invalidate this version. Only audited
    // reader/copy rendezvous and R200 temporary exclusion/publication use
    // AdvanceRuntime explicitly.
    // Two managed versions plus ten mutation/reset diagnostic words. No native ABI.
    internal const long MappingCacheCharge = 2 * sizeof(ulong) + 10 * sizeof(long);
    private ulong mappingAuthorityEpoch = 1;
    internal ulong MappingAuthorityEpoch => Interlocked.CompareExchange(ref mappingAuthorityEpoch, 0, 0);
    private ulong mappingTopologyEpoch = 1;
    internal ulong MappingTopologyEpoch => Interlocked.CompareExchange(ref mappingTopologyEpoch, 0, 0);
    internal ulong MappingCacheRuntimeEpoch => Interlocked.CompareExchange(ref *epoch, 0, 0);
    private long mappingTopologyAdvances, codeRoleRegistrations, codeRoleCacheFilters, codeRoleCacheKept,
        codeRoleCacheProvenanceDrops, codeRoleCachePhysicalDrops, codeRoleCacheWriteDrops,
        mappingCacheTopologyDrops, mappingCacheFallbackDrops, mappingCacheRaceDrops;
    internal long MappingTopologyAdvances => Volatile.Read(ref mappingTopologyAdvances);
    internal long CodeRoleRegistrations => Volatile.Read(ref codeRoleRegistrations);
    internal long CodeRoleCacheFilters => Volatile.Read(ref codeRoleCacheFilters);
    internal long CodeRoleCacheKept => Volatile.Read(ref codeRoleCacheKept);
    internal long CodeRoleCacheProvenanceDrops => Volatile.Read(ref codeRoleCacheProvenanceDrops);
    internal long CodeRoleCachePhysicalDrops => Volatile.Read(ref codeRoleCachePhysicalDrops);
    internal long CodeRoleCacheWriteDrops => Volatile.Read(ref codeRoleCacheWriteDrops);
    internal long MappingCacheTopologyDrops => Volatile.Read(ref mappingCacheTopologyDrops);
    internal long MappingCacheFallbackDrops => Volatile.Read(ref mappingCacheFallbackDrops);
    internal long MappingCacheRaceDrops => Volatile.Read(ref mappingCacheRaceDrops);

    // Charge EVERY generic mapping advance, including mapping-only completion.
    // The sole bypass is the private first-Code transition below. SC reads and
    // monotone, nonwrapping versions cover retirement/host observers off Gate.
    private bool TryAdvanceMappingTopology()
    {
        while (true)
        {
            ulong previous = MappingTopologyEpoch;
            if (previous == ulong.MaxValue)
            { Volatile.Write(ref epochExhausted, 1); return false; }
            if (Interlocked.CompareExchange(ref mappingTopologyEpoch, previous + 1, previous) == previous)
            {
                Interlocked.Increment(ref mappingTopologyAdvances);
                // Max is never a usable witness, including the first time it
                // is reached. Generic mapping/runtime revocation still runs.
                if (previous == ulong.MaxValue - 1)
                { Volatile.Write(ref epochExhausted, 1); return false; }
                return true;
            }
        }
    }
    private bool TryAdvanceMapping() => TryAdvanceMappingTopology() & TryAdvanceMappingVersion();

    private bool TryAdvanceMappingVersion()
    {
        while (true)
        {
            ulong previous = MappingAuthorityEpoch;
            if (previous == ulong.MaxValue)
            { Volatile.Write(ref epochExhausted, 1); return false; }
            if (Interlocked.CompareExchange(ref mappingAuthorityEpoch, previous + 1, previous) == previous)
                return true;
        }
    }
    // ONLY PrepareCodeSnapshot calls this, under Gate with mappingWriters
    // published. It still revokes runtime/data and drains all native frames
    // BEFORE setting Code. It changes no backing, PTE, lifetime or ownership.
    private void AdvanceFirstCodeRole()
    {
        if (!TryAdvanceMappingVersion() | !TryAdvanceRuntime())
        {
            Volatile.Write(ref epochExhausted, 1);
            throw new InvalidOperationException("Full-JIT authority generation exhausted.");
        }
        Interlocked.Increment(ref codeRoleRegistrations);
    }

    // Read-only physical registry lookup: never registers, walks PTEs/IO,
    // dereferences Host, grants rights, or reopens admission. Even read hints
    // with any borrower are conservatively rejected on this preservation path.
    internal bool CanPreserveCodeRoleMapping(nint page, bool write, out bool codeWrite)
    {
        if (!Monitor.IsEntered(Gate))
            throw new InvalidOperationException("Code-role preservation requires authority gate.");
        codeWrite = false;
        if (!MappingCacheAdmissionOpen || page == 0 || ((nuint)page & 4095) != 0) return false;
        int at = Find(page);
        if (at < 0 || cells[at].Address != page || cells[at].Writers >= 0) return false;
        codeWrite = write && cells[at].Code;
        return !codeWrite;
    }
    internal void RecordCodeRoleCacheFilter(long kept, long provenance, long physical, long writes)
    {
        Interlocked.Increment(ref codeRoleCacheFilters);
        Interlocked.Add(ref codeRoleCacheKept, kept);
        Interlocked.Add(ref codeRoleCacheProvenanceDrops, provenance);
        Interlocked.Add(ref codeRoleCachePhysicalDrops, physical);
        Interlocked.Add(ref codeRoleCacheWriteDrops, writes);
    }
    internal void RecordMappingCacheDrop(bool topology = false, bool race = false)
    {
        if (race) Interlocked.Increment(ref mappingCacheRaceDrops);
        else if (topology) Interlocked.Increment(ref mappingCacheTopologyDrops);
        else Interlocked.Increment(ref mappingCacheFallbackDrops);
    }

    // Evaluate both: runtime polling must still revoke on mapping exhaustion.
    private bool TryAdvance() => TryAdvanceMapping() & TryAdvanceRuntime();
    private void AdvanceRuntime()
    {
        if (!TryAdvanceRuntime())
        {
            Volatile.Write(ref epochExhausted, 1);
            throw new InvalidOperationException("Full-JIT authority generation exhausted.");
        }
    }
    // Host-code arena publication only: RequestPublication has paused store
    // epochs; REP kernel publication holds Gate across drain/seal, excluding
    // new issued-kernel enrollment. Keep each caller's native drain/lifetime
    // protocol. No guest PTE, backing, physical role or permission changes.
    internal void AdvanceCodePublicationRuntime()
    {
        if (!Monitor.IsEntered(Gate))
            throw new InvalidOperationException("Code publication requires authority gate.");
        AdvanceRuntime();
    }

    // Retention only, NEVER native/atomic/data admission. Callers must mask
    // MemoryTlb for the entire excluded invocation. Phase and worker come from
    // this controller's live local ownership, not from a copied frame/request.
    // Pending may still be draining old finite readers; they cannot mint a
    // mapping change without advancing MappingAuthorityEpoch. No retag occurs.
    internal bool CanPreserveOwnedMappingCache(FullMappingCachePhase phase, int worker)
    {
        if (!Monitor.IsEntered(Gate))
            throw new InvalidOperationException("Mapping preservation requires authority gate.");
        if ((uint)worker >= (uint)readers.Length || saturated ||
            Volatile.Read(ref epochExhausted) != 0 || PendingAtomicWorker != worker ||
            Volatile.Read(ref hostWriters) != 0 || Volatile.Read(ref copyWriters) != 0 ||
            Volatile.Read(ref nativeFrames) != 0 || readers[worker].CpuDepth != 0 ||
            readers[worker].ShortDepth != 0 || Volatile.Read(ref readers[worker].Admitted) != 0) return false;
        return phase switch
        {
            FullMappingCachePhase.OwnReservation => nativeTicketWorker == -1 && mappingWriters == 0,
            FullMappingCachePhase.OwnTicket => nativeTicketWorker == worker && mappingWriters == 1 &&
                NativeReservationDrained(worker),
            _ => false
        };
    }
    internal bool MappingCacheAdmissionOpen => CanReuseRetainedEligibility && SemanticReadersDrained();
}
