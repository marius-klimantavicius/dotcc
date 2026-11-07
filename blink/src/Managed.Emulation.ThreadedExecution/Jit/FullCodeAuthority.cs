using System.Runtime.InteropServices;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal enum FullDataPermissionDenial
{
    None, Saturated, EpochExhausted, HostWriter, MappingWriter, CopyWriter,
    RegistryExhausted, CodePage, PhysicalWriter
}

// Physical, not virtual, authority. A data pointer is borrowed until the owning
// helper/syscall/nonopcode scope ends. Admission owns Gate; native execution
// releases it and polls the epoch. A writer advances it and drains native
// enrollment BEFORE the pointer escapes. No gate spans native code or a helper.
// A fixed node pool records overlapping workers and aliases without allocation.
// Saturation fails closed for multiple-instruction execution for this owner;
// retained bodies and checked single-step execution still work within quota.
internal sealed unsafe partial class FullCodeAuthority : IDisposable
{
    private struct Cell { public nint Address; public int Writers; public bool Code; }
    private struct Borrow { public int Cell, Worker, NextCell, PreviousCell, NextWorker, FreeNext; }
    private readonly InstanceIo io;
    private readonly Cell[] cells;
    private readonly Borrow[] borrows;
    private readonly int[] workerHeads;
    private readonly int mask;
    private int freeHead;
    private int nativeTicketWorker = -1;
    private bool saturated;
    private int epochExhausted, hostWriters, mappingWriters, nativeFrames, activeBorrows, peakBorrows, physicalPages;
    private long hostMutations, enrollmentMisses, epochAdvances, codeEpochAdvances;
    internal long EpochAdvances => Volatile.Read(ref epochAdvances);
    internal long CodeEpochAdvances => Volatile.Read(ref codeEpochAdvances);
    internal int ActiveBorrows => activeBorrows;
    internal int PeakBorrows => peakBorrows;
    internal int PhysicalPages => physicalPages;
    internal bool Saturated => saturated;
    internal long HostMutations => Volatile.Read(ref hostMutations);
    internal long EnrollmentMisses => Volatile.Read(ref enrollmentMisses);
    private ulong* epoch;
    internal readonly object Gate = new();
    internal long Charge => 1024 + MappingCacheCharge + ContentSequenceCharge + PreciseMappingChargeFor(workerHeads.Length) + cells.LongLength * sizeof(Cell) + borrows.LongLength * sizeof(Borrow) +
        workerHeads.LongLength * sizeof(int) + Environment.SystemPageSize + SemanticChargeFor(workerHeads.Length);
    internal ulong* EpochPointer => epoch;
    internal ulong Epoch => Volatile.Read(ref *epoch);
    // Runtime/data admission and translation freshness are distinct. Both
    // words live in the SAME precharged native page and outlive every frame.
    internal ulong* CodeEpochPointer => epoch + 1;
    internal ulong CodeEpoch => Volatile.Read(ref epoch[1]);
    // Gate-owned diagnostic for the immediately preceding permission request.
    internal FullDataPermissionDenial LastDataPermissionDenial { get; private set; }
    internal void ResetDataPermissionDenial() => LastDataPermissionDenial = FullDataPermissionDenial.None;
    internal int MappingWriters => Volatile.Read(ref mappingWriters);
    internal int HostWriters => Volatile.Read(ref hostWriters);
    internal FullCodeAuthority(InstanceIo io, ulong memoryLimitBytes, int workers, long cacheBudget)
    {
        this.io = io;
        int capacity = CellCapacity(memoryLimitBytes, cacheBudget);
        cells = new Cell[capacity]; mask = capacity - 1;
        int nodes = NodeCapacity(cacheBudget);
        borrows = new Borrow[nodes]; workerHeads = new int[workers]; workerHeads.AsSpan().Fill(-1);
        readers = new SemanticReaderState[workers];
        codeCopyDepth = new int[workers];
        ownedMappings = new byte[workers];
        for (int i = 0; i < cells.Length; ++i) cells[i].Writers = -1;
        for (int i = 0; i < nodes; ++i) borrows[i].FreeNext = i + 1 < nodes ? i + 1 : -1;
        epoch = (ulong*)NativeMemory.AllocZeroed((nuint)Environment.SystemPageSize);
        if (epoch == null) throw new OutOfMemoryException();
        epoch[0] = epoch[1] = 1;
    }
    private static int CellCapacity(ulong limit, long budget)
    {
        int capacity = 16;
        ulong desired = Math.Min(limit / 4096 * 4, (ulong)Math.Max(16, budget / 64));
        while ((ulong)capacity < desired) capacity *= 2;
        return capacity;
    }
    private static int NodeCapacity(long budget) => checked((int)Math.Clamp(budget / 1024, 16, int.MaxValue));
    internal static long ChargeFor(ulong limit, int workers, long budget) => 1024 + MappingCacheCharge + ContentSequenceCharge + PreciseMappingChargeFor(workers) +
        (long)CellCapacity(limit, budget) * sizeof(Cell) + (long)NodeCapacity(budget) * sizeof(Borrow) +
        (long)workers * sizeof(int) + Environment.SystemPageSize + SemanticChargeFor(workers);
    private int Find(nint page)
    {
        ulong h = (ulong)page >> 12; h ^= h >> 27; h *= 0x9e3779b97f4a7c15UL;
        int at = (int)(h & (uint)mask);
        for (int i = 0; i < cells.Length; ++i, at = (at + 1) & mask)
            if (cells[at].Address == page || cells[at].Address == 0) return at;
        return -1;
    }
    // Must precede the FIRST byte snapshot/validation of a code page. Closing
    // admission and draining invalidates every previously cached non-code write.
    internal bool PrepareCodeSnapshot(nint page)
    {
        if (!Monitor.IsEntered(Gate) || page == 0 || ((nuint)page & 4095) != 0)
            throw new InvalidOperationException("Full-JIT code snapshot authority invalid.");
        if (saturated) return false;
        int at = Find(page);
        if (at < 0) { Saturate(); WaitNativeFrames(); return false; }
        ref var cell = ref cells[at];
        if (!cell.Code)
        {
            Interlocked.Increment(ref mappingWriters);
            try
            {
                AdvanceFirstCodeRole(); WaitNativeFrames();
                if (cell.Address == 0) ++physicalPages;
                cell.Address = page; cell.Code = true;
            }
            finally { Interlocked.Decrement(ref mappingWriters); }
        }
        return cell.Writers < 0;
    }
    // Gate plus passive physical lease; native execution must recheck epoch.
    // Unknown physical pages are registered before becoming cacheable data.
    internal bool TryGetNativeDataPermission(nint page, bool write, out ulong dataEpoch)
    {
        dataEpoch = Epoch;
        if (!Monitor.IsEntered(Gate) || page == 0 || ((nuint)page & 4095) != 0)
            throw new InvalidOperationException("Full-JIT native data identity invalid.");
        LastDataPermissionDenial = saturated ? FullDataPermissionDenial.Saturated :
            Volatile.Read(ref epochExhausted) != 0 ? FullDataPermissionDenial.EpochExhausted :
            Volatile.Read(ref hostWriters) != 0 ? FullDataPermissionDenial.HostWriter :
            Volatile.Read(ref mappingWriters) != 0 ? FullDataPermissionDenial.MappingWriter :
            Volatile.Read(ref copyWriters) != 0 ? FullDataPermissionDenial.CopyWriter : FullDataPermissionDenial.None;
        if (LastDataPermissionDenial != FullDataPermissionDenial.None) return false;
        int at = Find(page);
        if (at < 0) { LastDataPermissionDenial = FullDataPermissionDenial.RegistryExhausted; Saturate(); WaitNativeFrames(); return false; }
        ref var cell = ref cells[at];
        if (cell.Address == 0)
        {
            if (write)
            {
                // Unknown writes obtain their first physical identity only at
                // a runtime barrier, before effects, never from a guessed bit.
                Interlocked.Increment(ref mappingWriters);
                try { Advance(); WaitNativeFrames(); ++physicalPages; cell.Address = page; }
                finally { Interlocked.Decrement(ref mappingWriters); }
                dataEpoch = Epoch;
            }
            else { ++physicalPages; cell.Address = page; }
        }
        if (write && (cell.Code || cell.Writers >= 0))
        {
            LastDataPermissionDenial = cell.Code ? FullDataPermissionDenial.CodePage : FullDataPermissionDenial.PhysicalWriter;
            return false;
        }
        return true;
    }

    // Under Gate. Return true exactly when code authority has been revoked.
    internal bool Expose(int worker, nint page)
    {
        if (!Monitor.IsEntered(Gate) || (uint)worker >= (uint)workerHeads.Length || page == 0 || ((nuint)page & 4095) != 0)
            throw new InvalidOperationException("Full-JIT physical borrow identity invalid.");
        if (saturated) { MarkUntrackedBorrow(worker); AdvanceCode(); Advance(); WaitNativeFrames(); return true; }
        int at = Find(page);
        if (at < 0) { MarkUntrackedBorrow(worker); bool changed = Saturate(); WaitNativeFrames(); return changed; }
        ref var cell = ref cells[at];
        if (cell.Address == 0) ++physicalPages;
        cell.Address = page;
        for (int n = cell.Writers; n >= 0; n = borrows[n].NextCell)
            if (borrows[n].Worker == worker) return false;
        if (freeHead < 0) { MarkUntrackedBorrow(worker); bool changed = Saturate(); WaitNativeFrames(); return changed; }
        int index = freeHead; freeHead = borrows[index].FreeNext;
        borrows[index] = new() { Cell = at, Worker = worker, PreviousCell = -1,
            NextCell = cell.Writers, NextWorker = workerHeads[worker], FreeNext = -1 };
        if (cell.Writers >= 0) borrows[cell.Writers].PreviousCell = index;
        cell.Writers = index; workerHeads[worker] = index;
        ++activeBorrows; peakBorrows = Math.Max(peakBorrows, activeBorrows);
        // Every new writable pointer, including non-code backing, must drain
        // an already enrolled checked ticket before escaping. The borrow list
        // excludes future foreign tickets; the return value is code revocation
        // only. Saturated ticket promotion also checks semantic participation.
        if (cell.Code) AdvanceCode();
        Advance(); WaitNativeFrames();
        return cell.Code;
    }
    private bool Saturate()
    {
        saturated = true; AdvanceCode(); Advance(); return true;
    }
    internal void EndBorrow(int worker)
    {
        lock (Gate)
        {
            if ((uint)worker >= (uint)workerHeads.Length) throw new InvalidOperationException("Full-JIT borrow worker invalid.");
            bool codeChanged = (readers[worker].WriteState & 1) != 0;
            Interlocked.And(ref readers[worker].WriteState, ~1);
            int n = workerHeads[worker]; workerHeads[worker] = -1;
            // Writer-list completion changes future Write/ticket eligibility,
            // even on non-code pages and without a runtime pulse.
            if (n >= 0 || codeChanged) TryAdvanceMapping();
            while (n >= 0)
            {
                var borrow = borrows[n]; --activeBorrows;
                codeChanged |= cells[borrow.Cell].Code;
                if (borrow.PreviousCell >= 0) borrows[borrow.PreviousCell].NextCell = borrow.NextCell;
                else cells[borrow.Cell].Writers = borrow.NextCell;
                if (borrow.NextCell >= 0) borrows[borrow.NextCell].PreviousCell = borrow.PreviousCell;
                borrows[n] = new() { FreeNext = freeHead }; freeHead = n; n = borrow.NextWorker;
            }
            // Includes a page first registered as code AFTER pointer escape.
            // Old bytes validated while the borrow was pending cannot become
            // trusted at completion merely because its last writer left.
            if (codeChanged && !TryAdvanceCode()) Volatile.Write(ref epochExhausted, 1);
        }
    }
    private bool TryAdvanceRuntime()
    {
        while (true)
        {
            ulong previous = Volatile.Read(ref *epoch);
            if (previous == ulong.MaxValue) return false;
            if (Interlocked.CompareExchange(ref *epoch, previous + 1, previous) == previous)
            { Interlocked.Increment(ref epochAdvances); return true; }
        }
    }
    internal void Advance()
    {
        if (!TryAdvance())
        {
            Volatile.Write(ref epochExhausted, 1);
            throw new InvalidOperationException("Full-JIT authority generation exhausted.");
        }
    }
    private bool TryAdvanceCode()
    {
        while (true)
        {
            ulong previous = Volatile.Read(ref epoch[1]);
            if (previous == ulong.MaxValue) return false;
            if (Interlocked.CompareExchange(ref epoch[1], previous + 1, previous) == previous)
            { Interlocked.Increment(ref codeEpochAdvances); return true; }
        }
    }
    internal void AdvanceCode()
    {
        if (!TryAdvanceCode())
        {
            Volatile.Write(ref epochExhausted, 1);
            throw new InvalidOperationException("Full-JIT code authority generation exhausted.");
        }
    }
    // Caller owns Gate plus a passive physical lease. A code page can be first
    // registered while an older borrower is blocked: the existing physical
    // writer list then prevents granting authority until that scope releases.
    internal bool CanRunMultiple(Blink program, Blink.Machine* machine, ulong pc, int length)
    {
        ObserveEligibilityQuery(FullEligibilityQuery.Entry);
        if (saturated || Volatile.Read(ref epochExhausted) != 0 || mappingWriters != 0 || Volatile.Read(ref hostWriters) != 0 ||
            Volatile.Read(ref copyWriters) != 0 || length > 4096 - (int)(pc & 4095)) return false;
        ObserveEligibilityQuery(FullEligibilityQuery.PageIdentity);
        if (!JitPageIdentity.TryRead(program, machine, pc, out ulong entry)) return false;
        ulong required = (ulong)(Blink.PAGE_V | Blink.PAGE_U | Blink.PAGE_HOST);
        if ((entry & required) != required || (entry & (ulong)(Blink.PAGE_XD | Blink.PAGE_RSRV)) != 0) return false;
        byte* page = program.FindHostPage(entry);
        if (page == null) return false;
        ObserveEligibilityQuery(FullEligibilityQuery.Contains);
        if (program.BlinkHostMemoryContains(page, 4096) == 0) return false;
        ObserveEligibilityQuery(FullEligibilityQuery.Available);
        if (program.BlinkHostMemoryFilePageAvailable(page) == 0) return false;
        ObserveEligibilityQuery(FullEligibilityQuery.Shared);
        if (program.BlinkHostMemoryFilePageShared(page) != 0)
        {
            ObserveEligibilityQuery(FullEligibilityQuery.Lease);
            ulong lease = program.BlinkHostMemoryFilePageLease(page);
            ObserveEligibilityQuery(FullEligibilityQuery.Owned);
            if (!io.MemoryFileMutationOwned(lease)) return false;
        }
        return PrepareCodeSnapshot((nint)page);
    }
    // Generic callers retain global code invalidation. Owned finite mapping
    // scopes use the separate begin/end proof in PreciseMappings instead.
    internal void BeginMappingMutation()
    {
        lock (Gate) { BeginMappingChange(global: true); ++genericMappingMutations; }
    }
    internal void EndMappingMutation()
    {
        lock (Gate)
        {
            if (genericMappingMutations <= 0) throw new InvalidOperationException("Full-JIT mapping mutation balance.");
            --genericMappingMutations; EndMappingChange(global: true);
        }
    }
    internal void BeginHostWriter()
    {
        Interlocked.Increment(ref hostWriters); Interlocked.Increment(ref hostMutations);
        try { AdvanceContentSequence(); AdvanceCode(); Advance(); }
        catch { CompleteContentMutation(); Interlocked.Decrement(ref hostWriters); throw; }
    }
    internal void WaitNativeFrames()
    {
        var spin = new SpinWait();
        while (Volatile.Read(ref nativeFrames) != 0) spin.SpinOnce();
    }
    internal void EndHostWriter()
    {
        // A validation/compilation may START after begin and overlap writes.
        // Publish a completion version BEFORE dropping admission. Without this
        // edge it could enroll as stable immediately after the writer leaves.
        // End is a nonthrowing observer finally; exhaustion disables multiple
        // admission and later Begin rejects before permitting another mutation.
        CompleteContentMutation();
        if (!TryAdvanceCode() | !TryAdvance()) Volatile.Write(ref epochExhausted, 1);
        Interlocked.Decrement(ref hostWriters);
    }
    internal bool TryEnterNative(ulong validatedEpoch)
    {
        Interlocked.Increment(ref nativeFrames);
        if (Volatile.Read(ref hostWriters) == 0 && Volatile.Read(ref mappingWriters) == 0 &&
            Volatile.Read(ref copyWriters) == 0 && PendingAtomicWorker == -1 &&
            SemanticReadersDrained() && Volatile.Read(ref epochExhausted) == 0 && Epoch == validatedEpoch) return true;
        Interlocked.Decrement(ref nativeFrames); Interlocked.Increment(ref enrollmentMisses); return false;
    }
    internal void ExitNative() => Interlocked.Decrement(ref nativeFrames);
    // Phase two: the reservation already closed native/reader admission. Never
    // wait for semantic readers here: they may need Gate to finish their helper.
    // Caller reauthenticates mappings and the exact decoded plan under Gate.
    internal bool TryPromoteNativeTicket(int worker, nint firstPage, nint secondPage = 0, bool writesContent = true)
    {
        if (!Monitor.IsEntered(Gate)) throw new InvalidOperationException("Native ticket requires authority gate.");
        if (PendingAtomicWorker != worker || nativeTicketWorker >= 0 || !NativeReservationDrained(worker) ||
            mappingWriters != 0 || Volatile.Read(ref hostWriters) != 0 || Volatile.Read(ref copyWriters) != 0 ||
            Volatile.Read(ref epochExhausted) != 0) return false;
        if (HasForeignUntrackedBorrow(worker)) return false;
        if (HasForeignBorrow(firstPage, worker) || HasForeignBorrow(secondPage, worker)) return false;
        Interlocked.Increment(ref mappingWriters);
        try
        {
            // Production callers derive this bit from their authenticated live
            // request. Pure read tickets retain the original exclusion protocol.
            if (writesContent) AdvanceContentSequence();
            AdvanceRuntime(); WaitNativeFrames();
            nativeTicketWritesContent = writesContent; nativeTicketWorker = worker; return true;
        }
        catch
        {
            if (writesContent) CompleteContentMutation();
            Interlocked.Decrement(ref mappingWriters); AbortNativeReservation(worker); throw;
        }
    }
    private bool HasForeignBorrow(nint page, int worker)
    {
        if (page == 0) return false;
        int at = Find(page);
        if (at < 0 || cells[at].Address != page) return false;
        for (int n = cells[at].Writers; n >= 0; n = borrows[n].NextCell)
            if (borrows[n].Worker != worker) return true;
        return false;
    }
    internal bool TryEnterNativeTicket(int worker, ulong validatedEpoch)
    {
        if (!Monitor.IsEntered(Gate) || nativeTicketWorker != worker || PendingAtomicWorker != worker)
            throw new InvalidOperationException("Native ticket admission identity.");
        Interlocked.Increment(ref nativeFrames);
        // Physical reclamation may close admission without taking Gate. Its
        // foreign count is a retry, and must be checked AFTER enrollment too.
        if (Volatile.Read(ref mappingWriters) == 1 && Volatile.Read(ref hostWriters) == 0 &&
            Volatile.Read(ref copyWriters) == 0 && NativeReservationDrained(worker) &&
            Volatile.Read(ref epochExhausted) == 0 && Epoch == validatedEpoch) return true;
        Interlocked.Decrement(ref nativeFrames); Interlocked.Increment(ref enrollmentMisses); return false;
    }
    internal void EndNativeTicket(int worker)
    {
        lock (Gate)
        {
            // Foreign mapping scopes may have begun while this native frame
            // drained. Relinquish only the ticket's own count, never theirs.
            if (nativeTicketWorker != worker || mappingWriters <= 0)
                throw new InvalidOperationException("Native ticket release identity.");
            // This count is temporary exclusion, not a PTE/backing/role change.
            // Foreign mapping scopes advance their own mapping version.
            try
            {
                // Callers revoked source aliases after RET, before completion.
                if (nativeTicketWritesContent) CompleteContentMutation();
                if (!TryAdvanceRuntime()) Volatile.Write(ref epochExhausted, 1);
            }
            finally
            {
                nativeTicketWritesContent = false;
                nativeTicketWorker = -1; Interlocked.Decrement(ref mappingWriters);
                AbortNativeReservation(worker);
            }
        }
    }
    public void Dispose()
    {
        lock (Gate)
        {
            if (mappingWriters != 0 || hostWriters != 0 || nativeFrames != 0 || copyWriters != 0 ||
                PendingAtomicWorker != -1 || !SemanticReadersDrained())
                throw new InvalidOperationException("Full-JIT mutation/native admission still active.");
            foreach (int head in workerHeads)
                if (head >= 0) throw new InvalidOperationException("Full-JIT physical borrow still active.");
            Volatile.Write(ref epochExhausted, 1); TryAdvanceMapping();
            NativeMemory.Free(epoch); epoch = null;
        }
    }
}
