#nullable enable

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Resolution of the actual BCL source used by clock_gettime.
    /// NULL still validates the clock ID. No OS clock with a different source
    /// or resolution is substituted for the managed reading.</summary>
    public static int clock_getres(int clockId, timespec* resolution)
    {
        if (clockId != CLOCK_REALTIME && clockId != CLOCK_MONOTONIC) { errno = EINVAL; return -1; }
        if (resolution != null)
        {
            long nanos = clockId == CLOCK_REALTIME ? 100
                : (long)(((global::System.Int128)1_000_000_000 + global::System.Diagnostics.Stopwatch.Frequency - 1)
                    / global::System.Diagnostics.Stopwatch.Frequency);
            resolution->tv_sec = nanos / 1_000_000_000;
            resolution->tv_nsec = nanos % 1_000_000_000;
        }
        return 0;
    }

    public const int CLOCK_REALTIME = 0;
    public const int CLOCK_MONOTONIC = 1;

    /// <summary>POSIX wall and monotonic clocks. Clock readings have no mutable
    /// program state; errors use the calling owner's thread-local errno.</summary>
    public static int clock_gettime(int clockId, timespec* value)
    {
        if (clockId != CLOCK_REALTIME && clockId != CLOCK_MONOTONIC) { errno = EINVAL; return -1; }
        if (value == null) { errno = EFAULT; return -1; }
        if (clockId == CLOCK_REALTIME)
        {
            var now = global::System.DateTimeOffset.UtcNow;
            value->tv_sec = now.ToUnixTimeSeconds();
            value->tv_nsec = now.UtcTicks % global::System.TimeSpan.TicksPerSecond * 100;
        }
        else
        {
            long ticks = global::System.Diagnostics.Stopwatch.GetTimestamp();
            long frequency = global::System.Diagnostics.Stopwatch.Frequency;
            value->tv_sec = ticks / frequency;
            value->tv_nsec = (long)((global::System.Int128)(ticks % frequency) * 1_000_000_000 / frequency);
        }
        return 0;
    }

    /// <summary>Sleep for at least the requested interval, measured by the
    /// monotonic clock. Managed Thread.Interrupt maps to EINTR and reports the
    /// unslept interval; this does not install or emulate POSIX signal delivery.</summary>
    public static int nanosleep(timespec* request, timespec* remaining)
    {
        if (request == null) { errno = EFAULT; return -1; }
        long seconds = request->tv_sec, nanoseconds = request->tv_nsec;
        if (seconds < 0 || nanoseconds < 0 || nanoseconds >= 1_000_000_000) { errno = EINVAL; return -1; }
        // Int128 retains the full valid time_t range without overflowing when
        // converting seconds to nanoseconds or rounding the sleep chunk up.
        global::System.Int128 duration = (global::System.Int128)seconds * 1_000_000_000 + nanoseconds;
        long started = global::System.Diagnostics.Stopwatch.GetTimestamp();
        try
        {
            while (true)
            {
                var left = duration - PosixSleepElapsedNanoseconds(started);
                if (left <= 0) return 0;
                var milliseconds = (left + 999_999) / 1_000_000;
                global::System.Threading.Thread.Sleep(milliseconds > int.MaxValue ? int.MaxValue : (int)milliseconds);
            }
        }
        catch (global::System.Threading.ThreadInterruptedException)
        {
            if (remaining != null)
            {
                var left = duration - PosixSleepElapsedNanoseconds(started);
                if (left < 0) left = 0;
                remaining->tv_sec = (long)(left / 1_000_000_000);
                remaining->tv_nsec = (long)(left % 1_000_000_000);
            }
            errno = EINTR;
            return -1;
        }
    }

    private static global::System.Int128 PosixSleepElapsedNanoseconds(long started)
        => (global::System.Int128)(global::System.Diagnostics.Stopwatch.GetTimestamp() - started)
            * 1_000_000_000 / global::System.Diagnostics.Stopwatch.Frequency;
}
