using global::System;
using global::System.Threading;
using Managed.Emulation.Host;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    [ThreadStatic] private static HostSleep? sleepOwner;
    [ThreadStatic] private static CancellationToken sleepCancellation;
    [ThreadStatic] private static HostSignalWake? sleepSignalWake;
    public static void BindHostSleep(HostSleep value) => BindHostSleep(value, default);
    public static void BindHostSleep(HostSleep value, CancellationToken cancellation) => BindHostSleep(value, cancellation, null);
    public static void BindHostSleep(HostSleep value, CancellationToken cancellation, HostSignalWake? signalWake)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (sleepOwner != null) throw new InvalidOperationException("Sleep owner already bound.");
        sleepOwner = value;
        sleepCancellation = cancellation;
        sleepSignalWake = signalWake;
    }
    public static void UnbindHostSleep() { sleepOwner = null; sleepCancellation = default; sleepSignalWake = null; }
    private static int SleepError(int error) { Libc.errno = error; return -1; }
    public static unsafe int blink_host_nanosleep(Libc.timespec* request, Libc.timespec* remaining)
    {
        if (sleepOwner == null) return SleepError(19);
        if (request == null) return SleepError(14);
        using var wake = sleepSignalWake?.Begin(sleepCancellation);
        var result = sleepOwner.Sleep(request->tv_sec, request->tv_nsec, wake?.Token ?? sleepCancellation);
        if (result.Error == 4 && remaining != null)
        {
            remaining->tv_sec = result.Remaining.Seconds;
            remaining->tv_nsec = result.Remaining.Nanoseconds;
        }
        return result.Error == 0 ? 0 : SleepError(result.Error);
    }
}
