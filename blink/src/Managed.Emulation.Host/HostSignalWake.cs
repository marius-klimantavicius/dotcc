namespace Managed.Emulation.Host;

/// <summary>One worker's transient signal notification. It owns no guest pointers
/// or descriptors. A request remains pending until the guest signal checkpoint;
/// active operations retain their assigned interrupt even after that checkpoint.</summary>
public sealed class HostSignalWake : IDisposable
{
    private readonly object sync = new();
    private readonly HashSet<Lease> active = new();
    private long requested, acknowledged;
    private bool queued, disposed;
    private AggregateException? notificationFailure;
    public AggregateException? NotificationFailure { get { lock (sync) return notificationFailure; } }

    /// <summary>Only marks state and queues work; token callbacks never execute
    /// inline on the caller, which may hold translated upstream locks.</summary>
    public void Request()
    {
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            requested = checked(requested + 1);
            foreach (Lease lease in active) Volatile.Write(ref lease.Interrupted, 1);
            Schedule();
        }
    }

    /// <summary>Call on the owning worker immediately before upstream consumes
    /// queued signals. Finishing an I/O operation alone is not acknowledgement.</summary>
    public void Checkpoint()
    {
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            acknowledged = requested;
        }
    }

    public Lease Begin(CancellationToken permanent = default)
    {
        var lease = new Lease(this, permanent);
        try
        {
            lock (sync)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                active.Add(lease);
                if (requested != acknowledged) Volatile.Write(ref lease.Interrupted, 1);
                Schedule();
            }
            return lease;
        }
        catch { lease.Dispose(); throw; }
    }

    // Called under sync. At most one dispatcher is queued/running per worker.
    private void Schedule()
    {
        if (queued || !active.Any(lease => lease.Interrupted != 0 && !lease.CancelStarted)) return;
        queued = true;
        try
        {
            if (!ThreadPool.QueueUserWorkItem(static state => ((HostSignalWake)state!).Dispatch(), this))
                throw new InvalidOperationException("Cannot queue guest signal wake.");
        }
        catch
        {
            queued = false;
            Monitor.PulseAll(sync);
            throw;
        }
    }

    private void Dispatch()
    {
        while (true)
        {
            Lease? lease;
            lock (sync)
            {
                lease = active.FirstOrDefault(item => item.Interrupted != 0 && !item.CancelStarted);
                if (lease == null) { queued = false; Monitor.PulseAll(sync); return; }
                lease.CancelStarted = true;
            }
            // Never hold our lock while invoking BCL operation callbacks.
            try { lease.Source.Cancel(); }
            catch (Exception error)
            {
                lock (sync)
                    notificationFailure = notificationFailure == null ? new AggregateException(error)
                        : new AggregateException(notificationFailure, error);
            }
            finally
            {
                bool release;
                lock (sync)
                {
                    lease.CancelFinished = true;
                    release = ClaimSourceDisposal(lease);
                }
                if (release) lease.Source.Dispose();
            }
        }
    }

    private void Release(Lease lease)
    {
        bool release;
        lock (sync)
        {
            if (lease.Released) return;
            lease.Released = true;
            active.Remove(lease);
            release = ClaimSourceDisposal(lease);
        }
        if (release) lease.Source.Dispose();
    }

    // Called under sync. Release can race the dispatcher's finally block; only
    // one of them may dispose the source after cancellation callbacks finish.
    private static bool ClaimSourceDisposal(Lease lease)
    {
        if (!lease.Released || lease.SourceDisposalClaimed ||
            (lease.CancelStarted && !lease.CancelFinished)) return false;
        lease.SourceDisposalClaimed = true;
        return true;
    }

    /// <summary>Owner must first quiesce its operations. Drain queued callbacks
    /// before disposing the worker, without waiting while holding caller locks.</summary>
    public void Dispose()
    {
        lock (sync)
        {
            if (disposed) return;
            if (active.Count != 0) throw new InvalidOperationException("Signal wake still has active operations.");
            disposed = true;
            while (queued) Monitor.Wait(sync);
        }
    }

    public sealed class Lease : IDisposable
    {
        private readonly HostSignalWake owner;
        internal readonly CancellationTokenSource Source;
        internal int Interrupted;
        internal bool CancelStarted, CancelFinished, Released, SourceDisposalClaimed;
        internal Lease(HostSignalWake owner, CancellationToken permanent)
        {
            this.owner = owner;
            Source = CancellationTokenSource.CreateLinkedTokenSource(permanent);
        }
        public CancellationToken Token => Source.Token;
        public bool WasInterrupted => Volatile.Read(ref Interrupted) != 0;
        public void Dispose() => owner.Release(this);
    }
}
