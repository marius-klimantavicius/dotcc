#nullable enable
using System;

namespace Edg.Runtime;

/// <summary>C clock() in the portable ABI's million ticks per second. CPU time
/// belongs to the host process, including all owners and managed/runtime work.</summary>
public static class LibcCpuClock
{
    // Sum before conversion: separately rounding user/kernel time would discard
    // a microsecond. Int128 avoids TimeSpan.TotalTime's signed tick overflow.
    public static long FromTicks(long user,long kernel)
    {
        if(user<0 || kernel<0)return -1;
        return (long)(((Int128)user+kernel)/10);
    }
    public static long Clock()
    {
        if(OperatingSystem.IsBrowser() || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS())return -1;
        try {
            Environment.ProcessCpuUsage usage=Environment.CpuUsage;
            return FromTicks(usage.UserTime.Ticks,usage.PrivilegedTime.Ticks);
        }
        catch(PlatformNotSupportedException){return -1;}
    }
}
