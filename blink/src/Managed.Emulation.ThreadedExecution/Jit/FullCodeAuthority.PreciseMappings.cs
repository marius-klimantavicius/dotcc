namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    // Source-only R203. This sequence proves mapping snapshot coherence; it is
    // neither a header stamp nor permission to enroll. Tickets deliberately do
    // not participate: their source/content proof remains R201's protocol.
    private readonly byte[] ownedMappings; // Gate: 0 none, 1 precise, 2 global
    private ulong mappingSequence = 1;
    private int mappingMutations, genericMappingMutations;
    private long preciseMappingScopes, globalMappingScopes, mappingEscalations, physicalRetirementScopes;
    // Fixed fields/array header plus conservative array alignment and the
    // controller's per-worker FullMappingScopeDepth field/padding.
    internal static long PreciseMappingChargeFor(int workers) => 128L + 16L * workers;
    internal ulong MappingSequence => Interlocked.CompareExchange(ref mappingSequence, 0, 0);
    internal int MappingMutations => Volatile.Read(ref mappingMutations);
    internal long PreciseMappingScopes => Volatile.Read(ref preciseMappingScopes);
    internal long GlobalMappingScopes => Volatile.Read(ref globalMappingScopes);
    internal long MappingEscalations => Volatile.Read(ref mappingEscalations);
    internal long PhysicalRetirementScopes => Volatile.Read(ref physicalRetirementScopes);

    private bool TryAdvanceMappingSequence()
    {
        while (true)
        {
            ulong previous = MappingSequence;
            if (previous == ulong.MaxValue)
            { Volatile.Write(ref epochExhausted, 1); return false; }
            if (Interlocked.CompareExchange(ref mappingSequence, previous + 1, previous) == previous) return true;
        }
    }
    private void BeginMappingChange(bool global)
    {
        // Quarantine denies native admission; it must not itself deny the
        // existing global cleanup mapping path (FreeMachine/FreeHostPages).
        // Actual version exhaustion still follows the original fail-closed
        // advancement contract. BeginOrExtend forces a quarantined scope global.
        Interlocked.Increment(ref mappingWriters);
        Interlocked.Increment(ref mappingMutations);
        try
        {
            if (!TryAdvanceMappingSequence()) throw new InvalidOperationException("Full-JIT mapping sequence exhausted.");
            if (global) { Interlocked.Increment(ref globalMappingScopes); AdvanceCode(); }
            else Interlocked.Increment(ref preciseMappingScopes);
            Advance();
        }
        catch
        {
            TryAdvanceMappingSequence();
            Interlocked.Decrement(ref mappingMutations);
            Interlocked.Decrement(ref mappingWriters);
            throw;
        }
    }
    private void EndMappingChange(bool global)
    {
        // Valid release is nonthrowing even when any version exhausts. Publish
        // completion before opening admission, including failed/no-effect maps.
        TryAdvanceMappingSequence();
        if ((global && !TryAdvanceCode()) | !TryAdvance()) Volatile.Write(ref epochExhausted, 1);
        Interlocked.Decrement(ref mappingMutations);
        Interlocked.Decrement(ref mappingWriters);
    }
    internal bool OwnsPreciseMapping(int worker) =>
        (uint)worker < (uint)ownedMappings.Length && Volatile.Read(ref ownedMappings[worker]) == 1;

    // Only the controller authenticates the live worker/context/scope. Range
    // normalization rejects logical/full/overflow values and covers whole PTEs.
    internal static bool TryPreciseMappingRange(long address, long size, out ulong start, out ulong length)
    {
        const ulong limit = 0x800000000000UL;
        start = length = 0;
        if (address < 0 || size <= 0 || (ulong)size >= limit || (ulong)address >= limit ||
            (ulong)size > limit - (ulong)address) return false;
        start = (ulong)address & ~4095UL;
        ulong end = ((ulong)address + (ulong)size + 4095) & ~4095UL;
        length = end - start;
        return length != limit;
    }
    internal void BeginOrExtendOwnedMapping(int worker, bool precise)
    {
        if (!Monitor.IsEntered(Gate) || (uint)worker >= (uint)ownedMappings.Length)
            throw new InvalidOperationException("Full-JIT mapping scope identity.");
        precise &= !saturated && Volatile.Read(ref epochExhausted) == 0;
        if (ownedMappings[worker] == 0)
        {
            BeginMappingChange(global: !precise);
            Volatile.Write(ref ownedMappings[worker], precise ? (byte)1 : (byte)2);
        }
        else if (!precise && ownedMappings[worker] == 1)
        {
            // Escalation is permanent through this worker's outermost finally.
            Volatile.Write(ref ownedMappings[worker], (byte)2);
            Interlocked.Increment(ref mappingEscalations); AdvanceCode();
        }
    }
    internal void EndOwnedMappingMutation(int worker)
    {
        lock (Gate)
        {
            if ((uint)worker >= (uint)ownedMappings.Length || ownedMappings[worker] == 0)
                throw new InvalidOperationException("Full-JIT mapping scope balance.");
            bool global = ownedMappings[worker] == 2;
            Volatile.Write(ref ownedMappings[worker], (byte)0);
            EndMappingChange(global);
        }
    }
    // No Gate acquisition under producer allocator/lifetime locks. Exact
    // physical revocation skips mutable drafts; the SC sequence and pending
    // exclusion make every overlapping compilation/revalidation untrusted.
    internal bool BeginPhysicalRetirement(bool precise)
    {
        precise &= !Volatile.Read(ref saturated) && Volatile.Read(ref epochExhausted) == 0;
        // Reclamation is a finally path. Exhaustion quarantines admission but
        // cannot abandon a detached page or throw across a reader release.
        Interlocked.Increment(ref mappingWriters);
        Interlocked.Increment(ref mappingMutations);
        if (!TryAdvanceMappingSequence() | (!precise && !TryAdvanceCode()) | !TryAdvance())
            Volatile.Write(ref epochExhausted, 1);
        if (precise) Interlocked.Increment(ref preciseMappingScopes);
        else Interlocked.Increment(ref globalMappingScopes);
        Interlocked.Increment(ref physicalRetirementScopes);
        return precise;
    }
    internal void EndPhysicalRetirement(bool precise) => EndMappingChange(global: !precise);
}
