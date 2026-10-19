#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Linux LP64 utimbuf is two signed 64-bit seconds. Uses the real
    /// atomic OS update (NULL means now), following the owner's working directory.</summary>
    public static int utime(byte* path, void* times)
    {
        if (path == null) { errno = EFAULT; return -1; }
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return -1; }
        fixed (byte* resolved = Encoding.UTF8.GetBytes(ResolvePath(path) + "\0"))
        {
            int result = PosixUtime(resolved, times);
            if (result != 0) errno = Marshal.GetLastPInvokeError();
            return result;
        }
    }

    [DllImport("libc", EntryPoint = "utime", SetLastError = true)]
    private static extern int PosixUtime(byte* path, void* times);
}
