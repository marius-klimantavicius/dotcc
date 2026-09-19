using System.Diagnostics;

namespace Managed.Emulation.Host;

public readonly record struct HostSleepResult(int Error, HostTimestamp Remaining);

/// <summary>One bounded worker wait, using a monotonic host clock. Interrupt and
/// disposal wake the wait without retaining pointers into translated storage.</summary>
public sealed class HostSleep : IDisposable
{
    private readonly object sync = new();
    private bool disposed, interrupted, waiting;
    public bool IsWaiting { get { lock (sync) return waiting; } }
    public void Interrupt() { lock (sync) { interrupted = true; Monitor.PulseAll(sync); } }
    public void Dispose() { lock (sync) { disposed = true; Monitor.PulseAll(sync); } }
    public HostSleepResult Sleep(long seconds, long nanoseconds) => Sleep(seconds, nanoseconds, default);
    public HostSleepResult Sleep(long seconds, long nanoseconds, CancellationToken cancellation)
    {
        if (seconds < 0 || nanoseconds < 0 || nanoseconds >= 1_000_000_000)
            return new(22, default);
        Int128 requested = (Int128)seconds * 1_000_000_000 + nanoseconds;
        // Register/dispose outside sync: disposal can wait for an in-flight
        // callback, which itself needs sync. Token cancellation is persistent.
        using var registration = cancellation.UnsafeRegister(static value =>
        {
            var owner = (HostSleep)value!;
            lock (owner.sync) Monitor.PulseAll(owner.sync);
        }, this);
        lock (sync)
        {
            if (disposed) return new(9, default);
            if (waiting) return new(16, default);
            waiting = true;
            long started = Stopwatch.GetTimestamp();
            try
            {
                while (true)
                {
                    Int128 elapsed = (Int128)(Stopwatch.GetTimestamp() - started) * 1_000_000_000 / Stopwatch.Frequency;
                    Int128 left = Int128.Max(0, requested - elapsed);
                    if (disposed || interrupted || cancellation.IsCancellationRequested)
                    {
                        interrupted = false;
                        return new(4, new((long)(left / 1_000_000_000), (long)(left % 1_000_000_000)));
                    }
                    if (left == 0) return new(0, default);
                    int milliseconds = (int)Int128.Min(1000, (left + 999_999) / 1_000_000);
                    Monitor.Wait(sync, milliseconds);
                }
            }
            finally { waiting = false; }
        }
    }
}
