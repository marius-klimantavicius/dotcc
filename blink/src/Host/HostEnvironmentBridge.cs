using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

/// <summary>Authored POSIX host callbacks compiled beside unchanged generated
/// sources. The owner binds one explicit environment on its dedicated C worker
/// thread. C storage contains no reference to this managed object.</summary>
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    [ThreadStatic] private static HostEnvironment? environment;
    public static void BindHostEnvironment(HostEnvironment value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (environment != null) throw new InvalidOperationException("Host environment already bound on this worker.");
        environment = value;
    }
    public static void UnbindHostEnvironment() => environment = null;
    private static int EnvironmentError(int value) { Libc.errno = value; return -1; }

    public static unsafe int blink_host_clock_gettime(int clock, Libc.timespec* result)
    {
        try
        {
            if (environment == null) return EnvironmentError(19); // ENODEV
            if (result == null) return EnvironmentError(14); // EFAULT
            var value = environment.GetTime((HostClock)clock);
            if (!value.Succeeded) return EnvironmentError((int)value.Error);
            result->tv_sec = value.Value.Seconds;
            result->tv_nsec = value.Value.Nanoseconds;
            return 0;
        }
        catch (Exception) { return EnvironmentError(5); }
    }
    public static unsafe long blink_host_getrandom(void* destination, ulong length, uint flags)
    {
        try
        {
            if (environment == null) return EnvironmentError(19);
            if (destination == null && length != 0) return EnvironmentError(14);
            int count = (int)global::System.Math.Min(length, (ulong)HostEnvironment.MaximumEntropyChunk);
            var value = environment.GetRandom(new Span<byte>(destination, count), flags);
            return value.Succeeded ? value.Value : EnvironmentError((int)value.Error);
        }
        catch (Exception) { return EnvironmentError(5); }
    }
    public static unsafe int blink_host_getentropy(void* destination, ulong length)
    {
        if (length > HostEnvironment.MaximumEntropyChunk) return EnvironmentError(5);
        long result = blink_host_getrandom(destination, length, 0);
        return result < 0 ? -1 : 0;
    }
}
