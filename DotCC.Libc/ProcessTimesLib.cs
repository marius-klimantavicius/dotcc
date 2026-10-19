#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Actual process and waited-child CPU counters, plus elapsed
    /// real-time ticks. The caller's four consecutive LP64 clock_t fields
    /// match Linux's struct tms, so no second managed layout is involved.
    /// Tick units come from sysconf(_SC_CLK_TCK), never CLOCKS_PER_SEC.</summary>
    public static long times(void* buffer)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8)
        {
            errno = ENOTSUP;
            return -1;
        }
        long result = PosixProcessTimes(buffer);
        if (result == -1) errno = Marshal.GetLastPInvokeError();
        return result;
    }

    private static long ProcessClockTicksPerSecond()
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8)
        {
            errno = ENOTSUP;
            return -1;
        }
        long result = PosixProcessClockSysconf(2); // Linux _SC_CLK_TCK
        if (result == -1) errno = Marshal.GetLastPInvokeError();
        return result;
    }

    [DllImport("libc", EntryPoint = "times", SetLastError = true)]
    private static extern long PosixProcessTimes(void* buffer);

    [DllImport("libc", EntryPoint = "sysconf", SetLastError = true)]
    private static extern long PosixProcessClockSysconf(int name);
}
