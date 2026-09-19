using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int blink_host_gettimeofday(timeval* result, void* timezone)
    {
        try
        {
            if (environment == null) return EnvironmentError(19);
            if (result == null) return EnvironmentError(14);
            if (timezone != null) return EnvironmentError(95);
            var value = environment.GetTime(HostClock.Realtime);
            if (!value.Succeeded) return EnvironmentError((int)value.Error);
            result->tv_sec = value.Value.Seconds;
            result->tv_usec = value.Value.Nanoseconds / 1000;
            return 0;
        }
        catch (Exception) { return EnvironmentError(5); }
    }
    public static unsafe int blink_host_clock_getres(int clock, Libc.timespec* result)
    {
        try
        {
            if (environment == null) return EnvironmentError(19);
            var value = environment.GetResolution((HostClock)clock);
            if (!value.Succeeded) return EnvironmentError((int)value.Error);
            if (result != null)
            {
                result->tv_sec = value.Value.Seconds;
                result->tv_nsec = value.Value.Nanoseconds;
            }
            return 0;
        }
        catch (Exception) { return EnvironmentError(5); }
    }
}
