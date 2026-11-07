namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    // ENV services access only retained instance globals and private sample
    // data, never guest memory. They participate in pending-ticket exclusion,
    // but must not advance the epoch/drain their own prevalidated continuation
    // as a general guest-memory semantic reader does. RDRAND additionally owns
    // the exact generated RNG mutex; all service ownership lasts through RET.
    internal bool TryEnterProcessorEnvironmentRead(int worker, out SemanticReadScope scope)
    {
        scope = default;
        CheckSemanticWorker(worker);
        ref var state = ref readers[worker];
        if (state.CpuDepth != 0 || state.ShortDepth != 0 || Volatile.Read(ref state.Admitted) != 0)
            throw new InvalidOperationException("ENV service requires a fresh reader scope.");
        if (PendingAtomicWorker != -1) return false;
        Interlocked.Exchange(ref state.Admitted, 1);
        bool admitted = false;
        try
        {
            // SC publication plus pending recheck: a reservation either sees
            // this reader or this reader backs out before accessing globals.
            if (PendingAtomicWorker != -1) return false;
            state.CpuDepth = 1;
            scope = new(this, worker);
            admitted = true;
            return true;
        }
        finally { if (!admitted) Interlocked.Exchange(ref state.Admitted, 0); }
    }

    // Only the authenticated ENV Ready commit can use this handshake. It is
    // not ordinary TB/atomic admission: one private clock/RNG computation runs
    // with NativeLimit=1 and no guest-memory operand. Semantic readers cannot
    // race guest bytes with it. Ordinary native CAS still rejects ALL semantic
    // readers, including this lease. Real store/native enrollment is retained
    // until acknowledgement, so writers still wait for this finite commit.
    internal bool TryEnterProcessorEnvironmentNative(int worker, ulong validatedEpoch)
    {
        if (!Monitor.IsEntered(Gate))
            throw new InvalidOperationException("ENV native admission requires authority gate.");
        CheckSemanticWorker(worker);
        if (readers[worker].CpuDepth != 1 || readers[worker].ShortDepth != 0 ||
            Volatile.Read(ref readers[worker].Admitted) != 1)
            throw new InvalidOperationException("ENV native admission lacks its private service lease.");
        Interlocked.Increment(ref nativeFrames);
        if (Volatile.Read(ref hostWriters) == 0 && Volatile.Read(ref mappingWriters) == 0 &&
            Volatile.Read(ref copyWriters) == 0 && PendingAtomicWorker == -1 &&
            Volatile.Read(ref epochExhausted) == 0 && Epoch == validatedEpoch) return true;
        Interlocked.Decrement(ref nativeFrames);
        Interlocked.Increment(ref enrollmentMisses);
        return false;
    }
}
