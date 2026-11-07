namespace Managed.Emulation.Execution;

internal enum FullEligibilityQuery { Entry, PageIdentity, Contains, Available, Shared, Lease, Owned }

internal sealed unsafe partial class FullCodeAuthority
{
    // An instruction already executing recursively may be needed to release
    // a reader, mapping scope or its own reservation. Never defer that progress.
    // This is only a retry hint for PREVIOUSLY trusted source, not permission
    // to reuse it, acquire a pointer or enter native code.
    internal bool ShouldDeferTrustedSource(int worker)
    {
        if (!Monitor.IsEntered(Gate))
            throw new InvalidOperationException("Retained source retry requires authority gate.");
        if ((uint)worker >= (uint)readers.Length || saturated ||
            Volatile.Read(ref epochExhausted) != 0 ||
            CpuSemanticDepth(worker) != 0 || ShortSemanticDepth(worker) != 0 ||
            Volatile.Read(ref ownedMappings[worker]) != 0 ||
            PendingAtomicWorker == worker || nativeTicketWorker == worker)
            return false;
        return Volatile.Read(ref mappingWriters) != 0 || Volatile.Read(ref hostWriters) != 0 ||
            Volatile.Read(ref copyWriters) != 0 || PendingAtomicWorker != -1 || nativeTicketWorker != -1;
    }

    // Test-only implementation; every production call is erased by Roslyn.
    partial void ObserveEligibilityQuery(FullEligibilityQuery query);
    // Gate-owned scalar admission only. This does not validate a mapping or
    // mint eligibility: the caller must authenticate the exact flat-long key,
    // Live/code stamps and trust already granted after CanRunMultiple checked
    // the complete backing/writer protocol. Mapping/code/host/EOF changes stale
    // that stamp; runtime-only data pulses need not invalidate unchanged code.
    // Native enrollment still repeats its writer/epoch handshake, and emitted
    // per-site guards still authenticate each body. Foreign mutable shared
    // backing never acquired this trust and must keep taking the checked path.
    internal bool CanReuseRetainedEligibility
    {
        get
        {
            if (!Monitor.IsEntered(Gate))
                throw new InvalidOperationException("Retained eligibility requires authority gate.");
            return !saturated && Volatile.Read(ref epochExhausted) == 0 &&
                Volatile.Read(ref mappingWriters) == 0 && Volatile.Read(ref hostWriters) == 0 &&
                Volatile.Read(ref copyWriters) == 0 && PendingAtomicWorker == -1 && nativeTicketWorker == -1;
        }
    }
}
