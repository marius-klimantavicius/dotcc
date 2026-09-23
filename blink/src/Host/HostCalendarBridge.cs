using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe long blink_host_time(long* result)
    {
        try
        {
            if (environment == null) return EnvironmentError(19);
            var value = environment.GetTime(HostClock.Realtime);
            if (!value.Succeeded) return EnvironmentError((int)value.Error);
            if (result != null) *result = value.Value.Seconds;
            return value.Value.Seconds;
        }
        catch (Exception) { return EnvironmentError(5); }
    }
    public static unsafe Libc.tm* blink_host_gmtime_r(long* seconds, Libc.tm* result)
    {
        try
        {
            if (environment == null) { EnvironmentError(19); return null; }
            if (seconds == null || result == null) { EnvironmentError(14); return null; }
            Libc.tm value = default;
            // The generic UTC converter is a pure calendar operation. It never
            // reads the OS timezone; the complete record is copied on success.
            if (Libc.gmtime_r(seconds, &value) == null) { EnvironmentError(75); return null; }
            *result = value;
            return result;
        }
        catch (OutOfMemoryException) { EnvironmentError(12); return null; }
        catch (Exception) { EnvironmentError(5); return null; }
    }
    // This profile's local zone is explicitly UTC, independent of OS or TZ.
    public static unsafe Libc.tm* blink_host_localtime_r(long* seconds, Libc.tm* result)
        => blink_host_gmtime_r(seconds, result);
}
