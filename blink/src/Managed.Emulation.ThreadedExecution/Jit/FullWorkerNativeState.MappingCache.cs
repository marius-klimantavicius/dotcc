using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Exact controller-local phase. Excluded cache storage is only a dormant
// hint array; false return means it is NOT usable by this invocation.
internal enum FullMappingCachePhase { Ordinary, OwnReservation, OwnTicket }

internal sealed unsafe partial class FullWorkerNativeState
{
    internal const long MappingCacheCharge = 3 * sizeof(ulong);
    private ulong cachedMappingEpoch, cachedRuntimeEpoch, cachedTopologyEpoch;
    partial void MappingCacheRetagged(FullCodeAuthority authority);
    partial void MappingCachePreserving(FullCodeAuthority authority);

    private void DropMappingCache(FullCodeAuthority authority, bool topology = false, bool race = false)
    {
        NativeMemory.Clear(Tlb, FullNativeMemory.TlbCount * FullMemoryTlbEntry.ByteSize);
        cachedMappingEpoch = cachedRuntimeEpoch = cachedTopologyEpoch = 0;
        authority.RecordMappingCacheDrop(topology, race);
    }

    // Call under Gate, outside this worker's native invocation, BEFORE a fill
    // and again AFTER it. The first call establishes provenance for every hint
    // that a pending fill may publish. A fill/Acquire that changes mapping roles
    // must never let a final stamp adopt older entries from a different mapping.
    // First-Code deltas may filter exact entries at unchanged topology.
    // No backing pointer is followed; no rights, identity, context, spans or
    // descriptor/ticket/proof epochs are synthesized or changed here.
    internal bool PrepareMappingCache(FullCodeAuthority authority, ulong observedRuntime)
        => PrepareMappingCache(authority, observedRuntime, out _);

    internal bool PrepareMappingCache(FullCodeAuthority authority, ulong observedRuntime, out bool discarded)
        => PrepareMappingCache(authority, observedRuntime, FullMappingCachePhase.Ordinary, -1, out discarded);

    internal bool PrepareMappingCache(FullCodeAuthority authority, ulong observedRuntime,
        FullMappingCachePhase phase, int worker, out bool discarded, ulong? context = null)
    {
        discarded = false;
        if (!Monitor.IsEntered(authority.Gate) || storage == null)
            throw new InvalidOperationException("Mapping cache requires a live worker and authority gate.");
        ulong topology = authority.MappingTopologyEpoch, mapping = authority.MappingAuthorityEpoch,
            runtime = authority.MappingCacheRuntimeEpoch;
        if (runtime == 0 || runtime == ulong.MaxValue || runtime != observedRuntime ||
            mapping == 0 || mapping == ulong.MaxValue || topology == 0 || topology == ulong.MaxValue)
        { discarded = true; DropMappingCache(authority); return false; }

        if (phase != FullMappingCachePhase.Ordinary)
        {
            // Do not publish/retag a single entry or either worker stamp under
            // exclusion, including the pre-promotion retry. Initial provenance
            // must already exist at this SAME mapping version. No fresh fill
            // can be adopted here and no descriptor/ticket is modified.
            if (cachedMappingEpoch != mapping || cachedTopologyEpoch != topology || cachedRuntimeEpoch == 0 ||
                cachedRuntimeEpoch == ulong.MaxValue ||
                !authority.CanPreserveOwnedMappingCache(phase, worker))
            { discarded = true; DropMappingCache(authority); return false; }
            MappingCachePreserving(authority); // erased contract race seam
            if (authority.MappingCacheRuntimeEpoch != runtime || authority.MappingAuthorityEpoch != mapping ||
                authority.MappingTopologyEpoch != topology ||
                !authority.CanPreserveOwnedMappingCache(phase, worker))
            { discarded = true; DropMappingCache(authority, race: true); }
            return false; // retained bytes are NOT ordinary admission
        }
        if (worker != -1 || !authority.MappingCacheAdmissionOpen)
        { discarded = true; DropMappingCache(authority); return false; }

        bool roleFilter = false;
        long kept = 0, provenance = 0, physical = 0, writes = 0;
        if (cachedTopologyEpoch != topology)
        { discarded = true; DropMappingCache(authority, topology: cachedTopologyEpoch != 0); }
        else if (cachedMappingEpoch != mapping)
        {
            // With identical nonzero topology, ONLY first-Code registrations
            // can account for this monotone mapping delta. An authenticated
            // controller context is required; legacy/contextless callers clear.
            if (context.HasValue && cachedMappingEpoch != 0 && cachedMappingEpoch < mapping &&
                cachedRuntimeEpoch != 0 && cachedRuntimeEpoch != ulong.MaxValue && cachedRuntimeEpoch <= runtime)
            {
                roleFilter = true;
                for (int i = 0; i < FullNativeMemory.TlbCount; ++i)
                {
                    ref var entry = ref Tlb[i];
                    if (entry.Host == null && entry.AuthorityEpoch == 0) { entry = default; continue; }
                    if (entry.AuthorityEpoch != cachedRuntimeEpoch || entry.Context != context.Value ||
                        entry.Host == null || entry.Identity != (ulong)entry.Host || (entry.Identity & 4095) != 0 ||
                        (entry.Page & 4095) != 0 || !FullNativeMemory.Canonical(entry.Page) ||
                        (entry.Permissions != FullMemoryAccess.Read &&
                            entry.Permissions != (FullMemoryAccess.Read | FullMemoryAccess.Write)) ||
                        entry.Flags != FullMemoryFlags.None || entry.ValidStart != 0 || entry.ValidEnd != 4096)
                    { ++provenance; entry = default; discarded = true; continue; }
                    if (!authority.CanPreserveCodeRoleMapping((nint)entry.Host,
                        (entry.Permissions & FullMemoryAccess.Write) != 0, out bool codeWrite))
                    {
                        if (codeWrite) ++writes; else ++physical;
                        entry = default; discarded = true; continue;
                    }
                    // No permission/context/span/identity change, and Host is
                    // NEVER followed here. Enrollment/lifetime are still owed.
                    entry.AuthorityEpoch = runtime; ++kept;
                }
            }
            else { discarded = true; DropMappingCache(authority); }
        }
        else if (cachedRuntimeEpoch == 0 || cachedRuntimeEpoch == ulong.MaxValue)
        { discarded = true; DropMappingCache(authority); }
        else if (cachedRuntimeEpoch != runtime)
        {
            for (int i = 0; i < FullNativeMemory.TlbCount; ++i)
                if (Tlb[i].AuthorityEpoch == cachedRuntimeEpoch)
                    Tlb[i].AuthorityEpoch = runtime;
        }

        MappingCacheRetagged(authority); // erasable contract-assembly race seam
        // Host observers and semantic admission deliberately do NOT take Gate.
        // A race discards even freshly filled hints. Later races are covered by
        // actual native enrollment and unchanged per-site epoch polls through
        // RET; this method never resamples or updates frame.ObservedGlobalEpoch.
        ulong finalRuntime = authority.MappingCacheRuntimeEpoch, finalMapping = authority.MappingAuthorityEpoch,
            finalTopology = authority.MappingTopologyEpoch;
        if (finalRuntime != runtime || finalMapping != mapping || finalTopology != topology ||
            !authority.MappingCacheAdmissionOpen)
        { discarded = true; DropMappingCache(authority, race: true); return false; }
        if (roleFilter) authority.RecordCodeRoleCacheFilter(kept, provenance, physical, writes);
        cachedTopologyEpoch = topology;
        cachedMappingEpoch = mapping; cachedRuntimeEpoch = runtime;
        return true;
    }
}
