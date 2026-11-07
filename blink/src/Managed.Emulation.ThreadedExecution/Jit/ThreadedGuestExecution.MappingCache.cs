namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    private bool PrepareNativeMappingCache(Worker worker, FullBlockFrame* frame,
        bool reservation, bool ticket, out bool discarded)
    {
        if (ticket && !reservation)
            throw new InvalidOperationException("Mapping cache ticket lacks local reservation ownership.");
        var phase = ticket ? FullMappingCachePhase.OwnTicket : reservation ?
            FullMappingCachePhase.OwnReservation : FullMappingCachePhase.Ordinary;
        bool ready = worker.FullNativeState!.PrepareMappingCache(fullJit!.Authority,
            frame->ObservedGlobalEpoch, phase, phase == FullMappingCachePhase.Ordinary ? -1 :
                worker.FullCache.Worker, out discarded, frame->Context);
        // Excluded inspections use independently checked descriptors/pointers.
        // Even poisoned current-epoch ordinary hints are unreachable. Keep the
        // issued barrier ticket intact; it is independent of this dormant array.
        if (phase != FullMappingCachePhase.Ordinary) frame->MemoryTlb = null;
        return ready;
    }
}
