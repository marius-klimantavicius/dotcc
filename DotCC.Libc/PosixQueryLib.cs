#nullable enable
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public static void bzero(void* destination, ulong length) => memset(destination, 0, length);
    public static int sched_yield() { Thread.Yield(); return 0; }

    public static long sysconf(int name) => name switch
    {
        30 => Environment.SystemPageSize,
        83 or 84 => Environment.ProcessorCount,
        _ => UnsupportedSysconf()
    };
    private static long UnsupportedSysconf() { errno = EINVAL; return -1; }

    /// <summary>POSIX/XSI strerror_r: copy to caller storage, reporting insufficient capacity.</summary>
    public static int strerror_r(int error, byte* buffer, ulong length)
    {
        if (buffer == null) return EINVAL;
        byte* message = strerror(error);
        ulong count = (ulong)strlen(message);
        if (length <= count)
        {
            if (length != 0) { memcpy(buffer, message, length - 1); buffer[length - 1] = 0; }
            return ERANGE;
        }
        memcpy(buffer, message, count + 1);
        return 0;
    }

    /// <summary>Fill the dotcc Linux-layout utsname from managed operating-system identity APIs.</summary>
    public static int uname(void* output)
    {
        if (output == null) { errno = EFAULT; return -1; }
        string system = OperatingSystem.IsLinux() ? "Linux" : OperatingSystem.IsMacOS() ? "Darwin" :
            OperatingSystem.IsFreeBSD() ? "FreeBSD" : "Windows";
        string machine = global::System.Runtime.InteropServices.RuntimeInformation.OSArchitecture switch
        {
            global::System.Runtime.InteropServices.Architecture.X64 => "x86_64", global::System.Runtime.InteropServices.Architecture.X86 => "i686",
            global::System.Runtime.InteropServices.Architecture.Arm64 => "aarch64", global::System.Runtime.InteropServices.Architecture.Arm => "arm",
            var value => value.ToString()
        };
        string[] fields = [system, Environment.MachineName, Environment.OSVersion.Version.ToString(),
            global::System.Runtime.InteropServices.RuntimeInformation.OSDescription, machine, ""];
        byte* target = (byte*)output;
        for (int i = 0; i < fields.Length; i++)
        {
            var buffer = new Span<byte>(target + 65 * i, 65);
            buffer.Clear();
            // Encoder.Convert never cuts a UTF-8 sequence in the middle.
            Encoding.UTF8.GetEncoder().Convert(fields[i].AsSpan(), buffer[..64], true, out _, out _, out _);
        }
        return 0;
    }
}
