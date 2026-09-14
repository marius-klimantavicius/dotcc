using global::System;
using Managed.Emulation.Host;
namespace Managed.Emulation;
public static partial class Blink
{
    [ThreadStatic] private static HostSleep? sleepOwner;
    public static void BindHostSleep(HostSleep value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (sleepOwner != null) throw new InvalidOperationException("Sleep owner already bound.");
        sleepOwner = value;
    }
    public static void UnbindHostSleep() => sleepOwner = null;
    private static int SleepError(int error) { Libc.errno = error; return -1; }
    public static unsafe int blink_host_nanosleep(Libc.timespec* request, Libc.timespec* remaining)
    {
        if (sleepOwner == null) return SleepError(19);
        if (request == null) return SleepError(14);
        var result = sleepOwner.Sleep(request->tv_sec, request->tv_nsec);
        if (result.Error == 4 && remaining != null)
        {
            remaining->tv_sec = result.Remaining.Seconds;
            remaining->tv_nsec = result.Remaining.Nanoseconds;
        }
        return result.Error == 0 ? 0 : SleepError(result.Error);
    }
}
