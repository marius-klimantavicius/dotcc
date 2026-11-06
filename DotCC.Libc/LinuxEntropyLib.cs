#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Preserve Linux flags, partial reads, interruption and raw error results.
    // A BCL entropy generator cannot model GRND_NONBLOCK/GRND_RANDOM semantics.
    public static ulong __dotcc_linux_getrandom(void* buffer, ulong count, uint flags)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) return unchecked((ulong)-(long)ENOSYS);
        try
        {
            long result = HostLinuxGetrandom(buffer, count, flags);
            return result < 0 ? unchecked((ulong)-(long)Marshal.GetLastPInvokeError()) : (ulong)result;
        }
        catch (EntryPointNotFoundException) { return unchecked((ulong)-(long)ENOSYS); }
        catch (DllNotFoundException) { return unchecked((ulong)-(long)ENOSYS); }
    }

    [DllImport("libc", EntryPoint = "getrandom", SetLastError = true)]
    private static extern long HostLinuxGetrandom(void* buffer, ulong count, uint flags);
}
