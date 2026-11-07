using Managed.Emulation.Host;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution : IHostGuestMemoryCopies, IHostMemoryFileRead
{
    private long guestCopyBytes;
    private int guestCopyQuarantined;
    internal ulong GuestCopyBytes => unchecked((ulong)Volatile.Read(ref guestCopyBytes));
    bool IHostGuestMemoryCopies.TryReserveGuestCopyBytes(ulong count)
    {
        if (count > long.MaxValue || Volatile.Read(ref guestCopyQuarantined) != 0) return false;
        while (true)
        {
            long previous = Volatile.Read(ref guestCopyBytes);
            if (previous < 0 || (ulong)previous > memoryLimitBytes || count > memoryLimitBytes - (ulong)previous ||
                count > (ulong)(long.MaxValue - previous)) return false;
            if (Interlocked.CompareExchange(ref guestCopyBytes, previous + (long)count, previous) == previous) return true;
        }
    }
    void IHostGuestMemoryCopies.ReleaseGuestCopyBytes(ulong count)
    {
        // Tagged owned snapshots may be reclaimed on a different worker, after
        // its creating syscall or recursive signal frame has already unwound.
        while (true)
        {
            long previous = Volatile.Read(ref guestCopyBytes);
            if (previous < 0 || count > (ulong)previous)
            {
                Volatile.Write(ref guestCopyQuarantined, 1);
                fullJit?.Authority.QuarantineSemanticAdmission(); return;
            }
            if (Interlocked.CompareExchange(ref guestCopyBytes, previous - (long)count, previous) == previous) return;
        }
    }
    void IHostGuestMemoryCopies.BeginGuestRead()
    {
        if (fullJit == null) return;
        var worker = RequireCurrent();
        fullJit.Authority.BeginGuestRead(worker.FullCache.Worker, worker.FullMappingBorrow);
        try { FullAtomicCopyReadStarted((nint)worker.Machine); }
        catch { fullJit.Authority.EndGuestRead(worker.FullCache.Worker); throw; }
    }
    void IHostGuestMemoryCopies.EndGuestRead()
    {
        if (fullJit == null) return;
        var worker = current.Value;
        if (worker == null) { fullJit.Authority.QuarantineSemanticAdmission(); return; }
        fullJit.Authority.EndGuestRead(worker.FullCache.Worker);
        FullAtomicCopyReadEnded((nint)worker.Machine);
    }
    void IHostGuestMemoryCopies.BeginGuestWrite(nint address, int count)
    {
        if (fullJit == null) return;
        if ((uint)count > 4096 || (count != 0 && (address == 0 || (nuint)address > nuint.MaxValue - (uint)(count - 1))))
            throw new InvalidOperationException("Full-JIT guest write copy exceeds its bounded physical range.");
        var currentWorker = RequireCurrent();
        int worker = currentWorker.FullCache.Worker;
        FullAtomicCopyWriteStarting((nint)currentWorker.Machine, address, count);
        fullJit.Authority.BeginGuestWrite(worker, currentWorker.FullMappingBorrow);
        try
        {
            // This may also be an owned host buffer. Revoking a non-guest
            // identity is harmless; guessing guest virtual addresses is not.
            // Bounded copies span at most two physical pages. No persistent
            // helper/syscall borrow is created for either page.
            if (count != 0)
            {
                nuint first = (nuint)address & ~(nuint)4095;
                nuint last = ((nuint)address + (uint)(count - 1)) & ~(nuint)4095;
                // Exact physical aliases, including unknown-provenance bodies,
                // are revoked before the first copy byte. Unrelated code does
                // not need a global code-epoch pulse merely because this
                // finite destination is not in the data-pointer registry.
                fullJit.RevokeBoundedCopy(worker, (nint)first, last == first ? 0 : (nint)last);
            }
        }
        catch { fullJit.Authority.EndGuestWrite(worker); throw; }
    }
    void IHostGuestMemoryCopies.EndGuestWrite()
    {
        if (fullJit == null) return;
        var worker = current.Value;
        if (worker == null) { fullJit.Authority.QuarantineSemanticAdmission(); return; }
        fullJit.Authority.EndGuestWrite(worker.FullCache.Worker);
    }
    bool IHostMemoryFileRead.TryBeginMemoryFileRead() => fullJit == null ||
        fullJit.Authority.TryBeginHostBackingRead(current.Value?.FullMappingBorrow == true);
    void IHostMemoryFileRead.EndMemoryFileRead() => fullJit?.Authority.EndHostBackingRead();
}
