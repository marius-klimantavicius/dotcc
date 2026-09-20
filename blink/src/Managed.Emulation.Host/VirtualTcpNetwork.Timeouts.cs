using System.Diagnostics;

namespace Managed.Emulation.Host;

/// <summary>Canonical Linux old-timeval option value. This private profile
/// rounds positive values up to milliseconds and reports that effective value.
/// Zero disables the deadline; the largest supported value is int.MaxValue ms.</summary>
public readonly record struct SocketTimeout(long Seconds, long Microseconds);

public sealed partial class VirtualTcpNetwork
{
    public HostResult<int> SetTimeout(int handle, bool receive, SocketTimeout value)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor);
            if (value.Seconds < 0 || value.Microseconds is < 0 or >= 1000000)
                return Fail<int>(GuestError.Invalid);
            if (value.Seconds > int.MaxValue / 1000) return Fail<int>(GuestError.Unsupported);
            long milliseconds = value.Seconds * 1000 + (value.Microseconds + 999) / 1000;
            if (milliseconds > int.MaxValue) return Fail<int>(GuestError.Unsupported);
            if (receive) entry.ReceiveTimeoutMilliseconds = (int)milliseconds;
            else entry.SendTimeoutMilliseconds = (int)milliseconds;
            return HostResult<int>.Success(0);
        }
    }

    public HostResult<SocketTimeout> GetTimeout(int handle, bool receive)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<SocketTimeout>(GuestError.BadDescriptor);
            int milliseconds = receive ? entry.ReceiveTimeoutMilliseconds : entry.SendTimeoutMilliseconds;
            return HostResult<SocketTimeout>.Success(new(milliseconds / 1000, (milliseconds % 1000) * 1000));
        }
    }

    // The socket itself is nonblocking. Only a real WouldBlock reaches here;
    // no pending BCL send is abandoned, and no fabricated byte count is used.
    // Caller/owner cancellation takes precedence over no-progress expiry.
    private static async Task<bool> PauseSocketWait(int timeoutMilliseconds, long start, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        double delay = 5;
        if (timeoutMilliseconds != 0)
        {
            double remaining = timeoutMilliseconds - Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            if (remaining <= 0)
            {
                token.ThrowIfCancellationRequested();
                return false;
            }
            delay = Math.Min(delay, Math.Ceiling(remaining));
        }
        await Task.Delay((int)delay, token).ConfigureAwait(false);
        return true;
    }
}
