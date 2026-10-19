namespace Managed.CPython;

// Pure lifetime bookkeeping. This class never evaluates Python or simulates an
// interpreter. It is separately testable while the real export binding is absent.
internal sealed class RuntimeOwnership
{
    private static RuntimeOwnership? processOwner;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private int executionThread;
    private int values;
    private volatile bool finalized;
    private volatile bool stopping;
    private volatile bool faulted;

    internal static RuntimeOwnership Claim()
    {
        var owner = new RuntimeOwnership();
        if (Interlocked.CompareExchange(ref processOwner, owner, null) != null)
            throw new InvalidOperationException("Only one CPython runtime may be initialized per process.");
        return owner;
    }

    internal void RequireOwner(bool allowStopping = false)
    {
        RequireCallerThread();
        RequireLive(allowStopping);
    }

    internal void RequireCallerThread()
    {
        if (thread != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("CPython owning operations must run on the initializing thread.");
    }

    internal void BindExecutionThread()
    {
        if (Interlocked.CompareExchange(ref executionThread, Environment.CurrentManagedThreadId, 0) != 0)
            throw new InvalidOperationException("CPython execution thread is already bound.");
    }

    internal void RequireExecutionThread(bool allowStopping = false)
    {
        if (executionThread != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Translated CPython work must run on its private execution thread.");
        RequireLive(allowStopping);
    }

    private void RequireLive(bool allowStopping)
    {
        ObjectDisposedException.ThrowIf(finalized, typeof(PythonRuntime));
        if (faulted) throw new InvalidOperationException("CPython runtime state is uncertain after a boundary failure; this process cannot initialize another runtime.");
        if (stopping && !allowStopping) throw new InvalidOperationException("CPython is stopping.");
    }

    internal void AddValue() { RequireExecutionThread(); values = checked(values + 1); }
    internal void MarkFaulted() => faulted = true;
    internal void ReleaseValue() { RequireExecutionThread(); if (values <= 0) throw new InvalidOperationException("Unbalanced Python reference."); values--; }
    internal void BeginStop()
    {
        RequireExecutionThread();
        if (values != 0) throw new InvalidOperationException($"Dispose all {values} owning Python values before finalization.");
        stopping = true;
    }

    // Called after Py_FinalizeEx returns, including its -1 stream-flush result.
    // A bridge exception before that point leaves the process reserved: uncertain
    // native state must never be reused as an allegedly fresh interpreter.
    internal void MarkFinalized()
    {
        RequireExecutionThread(allowStopping: true);
        ReleaseReservation();
    }

    // Before interpreter entry, or after the adapter proves a clean failed
    // initialization, no thread state needs to be restored to release the claim.
    internal void ReleaseCleanInitializationFailure()
    {
        RequireCallerThread();
        RequireLive(allowStopping: true);
        ReleaseReservation();
    }

    private void ReleaseReservation()
    {
        finalized = true;
        if (Interlocked.CompareExchange(ref processOwner, null, this) != this)
            throw new InvalidOperationException("Lost process runtime ownership.");
    }
}
