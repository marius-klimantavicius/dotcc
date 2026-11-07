using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    // One worker has one host execution thread. Only that thread changes its
    // logical depths. Interlocked publication is the cross-thread handshake.
    // WriteState bit0 records an unrepresented saturated physical borrow;
    // bits1..30 count nested bounded write copies. No dynamically sized state.
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    private struct SemanticReaderState
    {
        internal int CpuDepth, ShortDepth, Admitted, WriteState;
    }
    private readonly SemanticReaderState[] readers;
    // Earliest enclosing actual-copy depth that touched code or an unknown
    // physical identity. This is writer freshness metadata, not participation.
    private readonly int[] codeCopyDepth;
    private long knownCodeWriteScopes, unknownCodeWriteScopes;
    internal long KnownCodeWriteScopes => Volatile.Read(ref knownCodeWriteScopes);
    internal long UnknownCodeWriteScopes => Volatile.Read(ref unknownCodeWriteScopes);
    private int atomicPendingWorker = -1, hostShortReaders, copyWriters;
    internal static long SemanticChargeFor(int workers) => checked(192L + 20L * workers);
    internal int PendingAtomicWorker => Interlocked.CompareExchange(ref atomicPendingWorker, -1, -1);
    internal int HostShortReaders => Volatile.Read(ref hostShortReaders);
    internal int CopyWriters => Volatile.Read(ref copyWriters);
    internal int CpuSemanticDepth(int worker) => readers[worker].CpuDepth;
    internal int ShortSemanticDepth(int worker) => readers[worker].ShortDepth;
    internal bool SemanticAdmitted(int worker) => Volatile.Read(ref readers[worker].Admitted) != 0;
    internal bool SemanticQuarantined => Volatile.Read(ref epochExhausted) != 0;
    partial void SemanticReaderAdmissionStep(int worker, bool published);
    partial void SemanticReaderPending(int worker);
    partial void HostReaderAdmissionStep(bool published);
    partial void NativeReservationPublished(int worker);

    // Release paths must not throw, allocate, or acquire Gate. A corrupt balance
    // closes all subsequent native admission instead of hiding an unsafe count.
    internal void QuarantineSemanticAdmission()
    {
        Volatile.Write(ref epochExhausted, 1);
        TryAdvance();
    }
    private static void IncrementBounded(ref int value)
    {
        while (true)
        {
            int previous = Volatile.Read(ref value);
            if (previous == int.MaxValue) throw new InvalidOperationException("Full-JIT semantic count exhausted.");
            if (Interlocked.CompareExchange(ref value, previous + 1, previous) == previous) return;
        }
    }
    private bool DecrementBounded(ref int value)
    {
        while (true)
        {
            int previous = Volatile.Read(ref value);
            if (previous <= 0) { QuarantineSemanticAdmission(); return false; }
            if (Interlocked.CompareExchange(ref value, previous - 1, previous) == previous) return true;
        }
    }
    private void CheckSemanticWorker(int worker)
    {
        if ((uint)worker >= (uint)readers.Length) throw new InvalidOperationException("Full-JIT semantic worker invalid.");
    }

    // SC-strength publication/double-check: a writer either sees the published
    // count, or the reader sees pending and rolls back BEFORE touching bytes.
    // Nested same-thread helpers never wait behind a writer draining their own
    // outer envelope. Finite copies already under Gate may drain nativeFrames
    // (ExitNative is Gate-free), then finish entirely before releasing Gate.
    private bool TryPublishSemanticReader(int worker, bool mappingOwned)
    {
        if (mappingOwned && Volatile.Read(ref mappingWriters) <= 0)
            throw new InvalidOperationException("Full-JIT mapping-owned copy lacks mutation admission.");
        // A mapping owner has already closed native enrollment and drained it
        // before changing mappings. Its finite copies must finish even when a
        // reservation arrived earlier and is waiting for that mapping count.
        bool finiteGateRead = Monitor.IsEntered(Gate) || mappingOwned;
        if (!finiteGateRead && PendingAtomicWorker != -1) { SemanticReaderPending(worker); return false; }
        SemanticReaderAdmissionStep(worker, published: false);
        Interlocked.Exchange(ref readers[worker].Admitted, 1);
        bool admitted = false;
        try
        {
            SemanticReaderAdmissionStep(worker, published: true);
            admitted = finiteGateRead || PendingAtomicWorker == -1;
            if (admitted)
            {
                // R183: semantic loads/copies and software LockBus RMWs must
                // not overlap a native scalar CAS. Publication closes ordinary
                // enrollment via SemanticReadersDrained; epoch then drains any
                // frame that won the race. No Gate is acquired while waiting.
                try { AdvanceRuntime(); WaitNativeFrames(); }
                catch { admitted = false; throw; }
            }
            return admitted;
        }
        finally { if (!admitted) Interlocked.Exchange(ref readers[worker].Admitted, 0); }
    }
    private bool TryEnterSemantic(int worker, bool cpu, bool mappingOwned = false)
    {
        CheckSemanticWorker(worker);
        ref var state = ref readers[worker];
        int depth = cpu ? state.CpuDepth : state.ShortDepth;
        if (depth == int.MaxValue) throw new InvalidOperationException("Full-JIT semantic nesting exhausted.");
        if (state.CpuDepth == 0 && state.ShortDepth == 0)
        {
            if (!TryPublishSemanticReader(worker, mappingOwned)) return false;
        }
        else if (Volatile.Read(ref state.Admitted) != 1)
            throw new InvalidOperationException("Full-JIT semantic nesting lacks admission.");
        if (cpu) state.CpuDepth = depth + 1;
        else state.ShortDepth = depth + 1;
        return true;
    }
    private void EnterSemantic(int worker, bool cpu, bool mappingOwned = false)
    {
        var spin = new SpinWait();
        while (!TryEnterSemantic(worker, cpu, mappingOwned))
        {
            if (Monitor.IsEntered(Gate)) throw new InvalidOperationException("Semantic admission cannot wait under authority gate.");
            spin.SpinOnce();
        }
    }
    private void ExitSemantic(int worker, bool cpu)
    {
        if ((uint)worker >= (uint)readers.Length) { QuarantineSemanticAdmission(); return; }
        ref var state = ref readers[worker];
        int depth = cpu ? state.CpuDepth : state.ShortDepth;
        if (depth <= 0) { QuarantineSemanticAdmission(); return; }
        if (cpu) state.CpuDepth = depth - 1;
        else state.ShortDepth = depth - 1;
        if (state.CpuDepth == 0 && state.ShortDepth == 0 && Interlocked.Exchange(ref state.Admitted, 0) != 1)
            QuarantineSemanticAdmission();
    }
    internal SemanticReadScope EnterCpuSemanticRead(int worker)
    {
        EnterSemantic(worker, cpu: true);
        return new(this, worker);
    }
    internal bool TryEnterCpuSemanticRead(int worker, out SemanticReadScope scope)
    {
        scope = default;
        if (!TryEnterSemantic(worker, cpu: true)) return false;
        scope = new(this, worker); return true;
    }
    internal void BeginGuestRead(int worker, bool mappingOwned = false) => EnterSemantic(worker, cpu: false, mappingOwned);
    internal void EndGuestRead(int worker) => ExitSemantic(worker, cpu: false);
    internal struct SemanticReadScope : IDisposable
    {
        private FullCodeAuthority? owner;
        private readonly int worker;
        internal SemanticReadScope(FullCodeAuthority owner, int worker) { this.owner = owner; this.worker = worker; }
        public void Dispose()
        {
            var admitted = owner; owner = null;
            admitted?.ExitSemantic(worker, cpu: true);
        }
    }

    // Scope surrounds ONLY the actual synchronous copy, at most 4096 bytes.
    // Counted participation prevents ticket promotion; the separate writer
    // count closes ordinary native enrollment. Caller revokes touched physical
    // code aliases under Gate before allowing the first store, without adding
    // a helper/syscall-duration physical Borrow node.
    internal void BeginGuestWrite(int worker, bool mappingOwned = false)
    {
        EnterSemantic(worker, cpu: false, mappingOwned);
        bool incremented = false;
        try
        {
            ref var state = ref readers[worker];
            if ((state.WriteState >> 1) == int.MaxValue >> 1)
                throw new InvalidOperationException("Full-JIT copy write nesting exhausted.");
            IncrementBounded(ref copyWriters); incremented = true;
            // Publish at EVERY actual-copy begin, including known non-code
            // destinations: their first code registration can race this copy.
            AdvanceContentSequence();
            AdvanceRuntime(); WaitNativeFrames();
            state.WriteState += 2;
        }
        catch
        {
            if (incremented)
            {
                // No store escaped, but balance the attempted write cycle.
                CompleteContentMutation(); DecrementBounded(ref copyWriters);
            }
            ExitSemantic(worker, cpu: false); throw;
        }
    }
    internal void EndGuestWrite(int worker)
    {
        if ((uint)worker >= (uint)readers.Length || readers[worker].WriteState < 2)
        { QuarantineSemanticAdmission(); return; }
        int depth = readers[worker].WriteState >> 1;
        if (codeCopyDepth[worker] == depth)
        {
            codeCopyDepth[worker] = 0;
            if (!TryAdvanceCode()) Volatile.Write(ref epochExhausted, 1);
        }
        readers[worker].WriteState -= 2;
        // Completion version precedes admission reopening, as for host writers.
        if (!TryAdvanceRuntime()) Volatile.Write(ref epochExhausted, 1);
        CompleteContentMutation();
        DecrementBounded(ref copyWriters);
        ExitSemantic(worker, cpu: false);
    }

    // Caller has already closed/drained native admission in BeginGuestWrite;
    // physical alias revocation follows this call, BEFORE the first store.
    // No new borrow or reader envelope is created. Unknown identities include
    // host-owned buffers and deliberately remain conservative, visibly counted.
    internal void NoteGuestWritePages(int worker, nint first, nint second = 0)
    {
        CheckSemanticWorker(worker);
        if (!Monitor.IsEntered(Gate) || readers[worker].WriteState < 2 ||
            first == 0 || ((nuint)first & 4095) != 0 || ((nuint)second & 4095) != 0)
            throw new InvalidOperationException("Full-JIT copy code freshness identity invalid.");
        bool unknown = saturated, code = false;
        ClassifyWritePage(first, ref unknown, ref code);
        if (second != 0 && second != first) ClassifyWritePage(second, ref unknown, ref code);
        if (!unknown && !code) return;
        if (unknown)
        {
            // Unknown physical writes retain global fallback for both code
            // freshness and managed data provenance. Never classify them as
            // first-Code-only, even when a role registration overlaps the copy.
            Interlocked.Increment(ref unknownCodeWriteScopes); Advance();
        }
        else Interlocked.Increment(ref knownCodeWriteScopes);
        int depth = readers[worker].WriteState >> 1;
        if (codeCopyDepth[worker] == 0) codeCopyDepth[worker] = depth;
        AdvanceCode();
    }
    private void ClassifyWritePage(nint page, ref bool unknown, ref bool code)
    {
        int at = Find(page);
        if (at < 0 || cells[at].Address != page) unknown = true;
        else code |= cells[at].Code;
    }

    internal bool TryBeginHostBackingRead(bool mappingOwned = false)
    {
        if (mappingOwned && Volatile.Read(ref mappingWriters) <= 0)
            throw new InvalidOperationException("Full-JIT mapping-owned backing copy lacks mutation admission.");
        bool finiteGateRead = Monitor.IsEntered(Gate) || mappingOwned;
        if (!finiteGateRead && PendingAtomicWorker != -1) { AtomicHostReaderPending(); return false; }
        HostReaderAdmissionStep(published: false);
        IncrementBounded(ref hostShortReaders);
        bool admitted = false;
        try
        {
            HostReaderAdmissionStep(published: true);
            admitted = finiteGateRead || PendingAtomicWorker == -1;
            if (admitted)
            {
                try { AdvanceRuntime(); WaitNativeFrames(); }
                catch { admitted = false; throw; }
            }
            return admitted;
        }
        finally { if (!admitted) DecrementBounded(ref hostShortReaders); }
    }
    internal void EndHostBackingRead() => DecrementBounded(ref hostShortReaders);

    internal SemanticSuspension PauseCpuSemanticRead(int worker)
    {
        CheckSemanticWorker(worker);
        ref var state = ref readers[worker];
        if (state.ShortDepth != 0 || state.WriteState >= 2)
            throw new InvalidOperationException("A bounded guest copy cannot invoke guest execution.");
        int saved = state.CpuDepth;
        if (saved != 0)
        {
            state.CpuDepth = 0;
            if (Interlocked.Exchange(ref state.Admitted, 0) != 1)
            { QuarantineSemanticAdmission(); throw new InvalidOperationException("Full-JIT semantic suspension lacks admission."); }
        }
        return new(this, worker, saved);
    }
    internal struct SemanticSuspension : IDisposable
    {
        private FullCodeAuthority? owner;
        private readonly int worker, depth;
        internal SemanticSuspension(FullCodeAuthority owner, int worker, int depth)
        { this.owner = owner; this.worker = worker; this.depth = depth; }
        public void Dispose()
        {
            var paused = owner; owner = null;
            if (paused == null) return;
            ref var state = ref paused.readers[worker];
            if (state.CpuDepth != 0 || state.ShortDepth != 0 || state.Admitted != 0 || state.WriteState >= 2)
            { paused.QuarantineSemanticAdmission(); throw new InvalidOperationException("Full-JIT recursive semantic scope leaked."); }
            if (depth == 0) return;
            // Mandatory even during exceptional unwind: paused C must reacquire
            // before it can resume, then its outer finally releases normally.
            paused.EnterSemantic(worker, cpu: true);
            state.CpuDepth = depth;
        }
    }

    internal bool SemanticReadersDrained()
    {
        if (Interlocked.CompareExchange(ref hostShortReaders, 0, 0) != 0) return false;
        for (int worker = 0; worker < readers.Length; ++worker)
            if (Interlocked.CompareExchange(ref readers[worker].Admitted, 0, 0) != 0) return false;
        return true;
    }
    internal bool NativeReservationDrained(int worker) => PendingAtomicWorker == worker && SemanticReadersDrained();
    internal bool TryReserveNativeTicket(int worker)
    {
        if (!Monitor.IsEntered(Gate)) throw new InvalidOperationException("Native reservation requires authority gate.");
        CheckSemanticWorker(worker);
        if (readers[worker].CpuDepth != 0 || readers[worker].ShortDepth != 0 || readers[worker].Admitted != 0)
            throw new InvalidOperationException("Native reservation cannot carry a live own-worker semantic frame.");
        if (nativeTicketWorker >= 0 || mappingWriters != 0 || Volatile.Read(ref hostWriters) != 0 ||
            Volatile.Read(ref copyWriters) != 0 || Volatile.Read(ref epochExhausted) != 0 ||
            Interlocked.CompareExchange(ref atomicPendingWorker, worker, -1) != -1) return false;
        // Reservation publishes exclusion only. Keep the SC pending handshake
        // and native drain; real role/mapping/writer transitions still Advance.
        try { AdvanceRuntime(); WaitNativeFrames(); NativeReservationPublished(worker); return true; }
        catch { AbortNativeReservation(worker); throw; }
    }
    internal void AbortNativeReservation(int worker)
    {
        // Gate-free and idempotent; never release another worker's reservation.
        Interlocked.CompareExchange(ref atomicPendingWorker, -1, worker);
    }
    internal void DrainNativeForFiniteGateRead()
    {
        if (!Monitor.IsEntered(Gate)) throw new InvalidOperationException("Finite gate read needs authority gate.");
        if (PendingAtomicWorker != -1) WaitNativeFrames();
    }
    private void MarkUntrackedBorrow(int worker) => Interlocked.Or(ref readers[worker].WriteState, 1);
    private bool HasForeignUntrackedBorrow(int worker)
    {
        for (int other = 0; other < readers.Length; ++other)
            if (other != worker && (Volatile.Read(ref readers[other].WriteState) & 1) != 0) return true;
        return false;
    }
}
