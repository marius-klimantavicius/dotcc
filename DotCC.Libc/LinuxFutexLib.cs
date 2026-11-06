#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // A specific host service for Linux LP64 FUTEX_WAIT/FUTEX_WAKE, not a
    // syscall dispatcher. The caller owns stable unmanaged word/timeout storage.
    // Linux x64 timespec is the source's two signed 64-bit fields. Other hosts
    // and operations are explicitly unsupported. libc errno stays unchanged.
    public static ulong __dotcc_linux_futex_3arg(void* address, uint operation, uint value)
        => LinuxFutex(address, operation, value, null, 1);

    public static ulong __dotcc_linux_futex_4arg(void* address, uint operation, uint value, void* timeout)
        => LinuxFutex(address, operation, value, timeout, 0);

    private static ulong LinuxFutex(void* address, uint operation, uint value, void* timeout, uint command)
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64
            || (operation & ~0x180u) != command) return unchecked((ulong)-(long)ENOSYS);
        try
        {
            long result = HostLinuxFutex(202, address, operation, value, timeout, null, 0);
            return result < 0 ? unchecked((ulong)-(long)Marshal.GetLastPInvokeError()) : (ulong)result;
        }
        catch (EntryPointNotFoundException) { return unchecked((ulong)-(long)ENOSYS); }
        catch (DllNotFoundException) { return unchecked((ulong)-(long)ENOSYS); }
    }

    [DllImport("libc", EntryPoint = "syscall", SetLastError = true)]
    private static extern long HostLinuxFutex(long number, void* address, uint operation, uint value, void* timeout, void* secondAddress, uint thirdValue);
}
