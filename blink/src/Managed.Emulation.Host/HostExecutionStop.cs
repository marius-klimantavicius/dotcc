using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Managed.Emulation.Host;

public enum HostExecutionStopReason { None = 0, Requested = 1, Deadline = 2, Budget = 3 }

/// <summary>One execution's cooperative stop state. Callbacks never access guest
/// pointers. Dispose only after the owning execution has returned, and never
/// from a callback registered on Token.</summary>
public sealed class HostExecutionStop : IDisposable
{
    private readonly object sync = new();
    private readonly CancellationTokenSource stopped = new();
    private readonly Timer timer;
    private readonly CancellationTokenRegistration external;
    private readonly TimeSpan? timeout;
    private readonly long started = Stopwatch.GetTimestamp();
    private int reason, notifying;
    private bool disposed;
    private AggregateException? notificationFailure;
    private List<NativePoll>? nativePolls;

    public HostExecutionStop(TimeSpan? timeout = null, CancellationToken cancellation = default)
    {
        if (timeout is { } value && value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));
        this.timeout = timeout;
        timer = new Timer(_ => RefreshDeadline(true), null, Timeout.Infinite, Timeout.Infinite);
        external = cancellation.UnsafeRegister(static value => ((HostExecutionStop)value!).RequestStop(), this);
        RefreshDeadline(true);
    }

    public CancellationToken Token => stopped.Token;
    public HostExecutionStopReason Reason
    {
        get { RefreshDeadline(); return (HostExecutionStopReason)Volatile.Read(ref reason); }
    }
    /// <summary>A throwing user token callback is retained as an owner failure;
    /// it must not escape a timer callback and terminate the host process.</summary>
    public AggregateException? NotificationFailure => Volatile.Read(ref notificationFailure);
    public bool RequestStop() => Latch(HostExecutionStopReason.Requested);
    public bool RequestBudgetStop() => Latch(HostExecutionStopReason.Budget);

    /// <summary>Optional native instruction-boundary stop cell. Its caller must
    /// finish every native invocation before disposing the cell or this owner.
    /// The cell is latched before Reason, without cancellation callback delay.</summary>
    public unsafe NativePoll AcquireNativePoll()
    {
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            var poll = new NativePoll(this, reason != 0);
            try { (nativePolls ??= new()).Add(poll); }
            catch { poll.Free(); throw; }
            return poll;
        }
    }

    public unsafe sealed class NativePoll : IDisposable
    {
        private readonly HostExecutionStop owner;
        private int* pointer;
        internal NativePoll(HostExecutionStop owner, bool stopped)
        {
            this.owner = owner;
            pointer = (int*)NativeMemory.AllocZeroed((nuint)sizeof(int));
            if (pointer == null) throw new OutOfMemoryException();
            *pointer = stopped ? 1 : 0;
        }
        public int* Pointer => pointer != null ? pointer : throw new ObjectDisposedException(nameof(NativePoll));
        internal void Latch() => Volatile.Write(ref *pointer, 1);
        internal void Free() { NativeMemory.Free(pointer); pointer = null; }
        public void Dispose()
        {
            lock (owner.sync)
            {
                if (pointer == null) return;
                owner.nativePolls!.Remove(this);
                Free();
            }
        }
    }

    private void RefreshDeadline(bool schedule = false)
    {
        // The timeout is immutable. Workers without their own deadline still
        // observe cancellation through the latched reason, without taking the
        // deadline gate at every instruction boundary.
        if (timeout == null) return;
        lock (sync)
        {
            if (disposed || reason != 0) return;
            TimeSpan remaining = timeout.Value - Stopwatch.GetElapsedTime(started);
            if (remaining > TimeSpan.Zero)
            {
                // Long deadlines use bounded timer slices. Recheck the actual
                // monotonic deadline rather than assuming a timer fires exactly.
                long milliseconds = remaining.Ticks / TimeSpan.TicksPerMillisecond;
                if (remaining.Ticks % TimeSpan.TicksPerMillisecond != 0) ++milliseconds;
                if (schedule) timer.Change((int)Math.Min(int.MaxValue, milliseconds), Timeout.Infinite);
                return;
            }
        }
        Latch(HostExecutionStopReason.Deadline);
    }

    private bool Latch(HostExecutionStopReason value)
    {
        lock (sync)
        {
            if (disposed || reason != 0) return false;
            if (nativePolls != null)
                foreach (NativePoll poll in nativePolls) poll.Latch();
            Volatile.Write(ref reason, (int)value);
            ++notifying;
        }
        try { stopped.Cancel(); }
        catch (AggregateException error) { Volatile.Write(ref notificationFailure, error); }
        finally
        {
            lock (sync) { --notifying; Monitor.PulseAll(sync); }
        }
        return true;
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (disposed) return;
            disposed = true;
        }
        external.Dispose();
        timer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        lock (sync) { while (notifying != 0) Monitor.Wait(sync); }
        stopped.Dispose();
    }
}
