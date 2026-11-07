using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Passive physical backing and anonymous-page retirement; independent of TB
// storage and executable code. No native epoch or mapping lock spans a helper.
internal sealed unsafe class FullMemoryLifetime : IDisposable
{
    private readonly object gate = new();
    private readonly nint[] retiredPages;
    private int readers, retiredCount, peakRetired;
    private long semanticBorrows, walkBorrows, deferredRecycles;
    private bool disposed;
    internal long Charge => ChargeFor((ulong)retiredPages.LongLength * 4096);
    internal static long ChargeFor(ulong limit) => 256 + checked((long)(limit / 4096)) * sizeof(nint);
    internal FullMemoryLifetime(ulong memoryLimitBytes)
        => retiredPages = new nint[checked((int)(memoryLimitBytes / 4096))];
    internal void BeginRead(Blink program, bool semantic)
    {
        // This is passive reclamation protection, not a writer-excluding lease.
        // Writers detach and queue without waiting for a semantic helper.
        if (program.BlinkHostMemoryInstructionReadBegin() != 0)
            throw new InvalidOperationException("Full-JIT data read protection failed.");
        try
        {
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                readers = checked(readers + 1);
                if (semantic) ++semanticBorrows; else ++walkBorrows;
            }
        }
        catch { program.BlinkHostMemoryInstructionReadEnd(); throw; }
    }

    internal void EndRead(Blink program, FullTranslationJit owner)
    {
        try
        {
            lock (gate)
            {
                if (readers <= 0) throw new InvalidOperationException("Full-JIT data read balance.");
                if (--readers == 0)
                    while (retiredCount != 0)
                    {
                        nint item = retiredPages[--retiredCount]; retiredPages[retiredCount] = 0;
                        // The authored raw primitive has no System dereference;
                        // its allocator belongs to this bound program instance.
                        owner.RecycleDeferredAnonymousPage(program, (byte*)(item & ~(nint)3), (item & 1) != 0, (item & 2) != 0);
                    }
            }
        }
        finally
        {
            // No semantic gate -> physical-owner/IO lock edge.
            if (program.BlinkHostMemoryInstructionReadEnd() != 0)
                throw new InvalidOperationException("Full-JIT data read protection did not release.");
        }
    }

    internal void RecycleAnonymousPage(Blink program, Blink.System* system, byte* page, bool clear, bool precise)
    {
        lock (gate)
        {
            if (page == null || ((nuint)page & 4095) != 0)
                throw new InvalidOperationException("Full-JIT anonymous retirement address invariant.");
            if (readers == 0) program.BlinkGuestRecycleAnonymousPage(system, page, clear ? 1 : 0);
            else
            {
                if (retiredCount == retiredPages.Length)
                    throw new InvalidOperationException("Full-JIT anonymous retirement resource invariant.");
                retiredPages[retiredCount++] = (nint)page | (clear ? (nint)1 : 0) | (precise ? (nint)2 : 0);
                peakRetired = Math.Max(peakRetired, retiredCount); ++deferredRecycles;
            }
        }
    }


    internal GuestJitStatistics Snapshot(GuestJitStatistics value)
    {
        lock (gate) return value with { SemanticDataBorrows = semanticBorrows,
            PageTableReadBorrows = walkBorrows, DeferredAnonymousRecycles = deferredRecycles,
            PeakDeferredAnonymousPages = peakRetired, ActiveDataBorrows = readers,
            DeferredAnonymousPages = retiredCount };
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (readers != 0 || retiredCount != 0)
                throw new InvalidOperationException("Disposing active Full-JIT physical borrows.");
            disposed = true;
        }
    }
}
