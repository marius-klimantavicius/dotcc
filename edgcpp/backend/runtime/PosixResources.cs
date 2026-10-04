#nullable enable
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

[StructLayout(LayoutKind.Sequential)]
public struct PosixRlimit { public ulong rlim_cur, rlim_max; }

public sealed unsafe partial class PosixContext
{
    public int Getrlimit(int resource, PosixRlimit* result)
    {
        if (result == null) { SetError(14); return -1; }
        if (resource != 0) { SetError(22); return -1; }
        if (OperatingSystem.IsWindows()) { *result = new PosixRlimit { rlim_cur = ulong.MaxValue, rlim_max = ulong.MaxValue }; return 0; }
        PosixRlimit native;
        if (GetrlimitUnix(0, &native) != 0) return Fail(new Win32Exception(Marshal.GetLastPInvokeError()));
        // Darwin uses signed-long maximum for RLIM_INFINITY.
        if (OperatingSystem.IsMacOS())
        {
            if (native.rlim_cur == long.MaxValue) native.rlim_cur = ulong.MaxValue;
            if (native.rlim_max == long.MaxValue) native.rlim_max = ulong.MaxValue;
        }
        *result = native; return 0;
    }
    public int Setrlimit(int resource, PosixRlimit* value)
    {
        if (value == null) { SetError(14); return -1; }
        if (resource != 0 || value->rlim_cur > value->rlim_max) { SetError(22); return -1; }
        if (OperatingSystem.IsWindows())
        {
            // There is no POSIX SIGXCPU soft-limit delivery on Windows. Never
            // claim a finite budget was enforced when it was not.
            if (value->rlim_cur == ulong.MaxValue && value->rlim_max == ulong.MaxValue) return 0;
            SetError(95); return -1;
        }
        PosixRlimit native = *value;
        if (OperatingSystem.IsMacOS())
        {
            if ((native.rlim_cur != ulong.MaxValue && native.rlim_cur > long.MaxValue) ||
                (native.rlim_max != ulong.MaxValue && native.rlim_max > long.MaxValue)) { SetError(22); return -1; }
            if (native.rlim_cur == ulong.MaxValue) native.rlim_cur = long.MaxValue;
            if (native.rlim_max == ulong.MaxValue) native.rlim_max = long.MaxValue;
        }
        return SetrlimitUnix(0, &native) == 0 ? 0 : Fail(new Win32Exception(Marshal.GetLastPInvokeError()));
    }
    [LibraryImport("libc", EntryPoint = "getrlimit", SetLastError = true)] private static partial int GetrlimitUnix(int resource, PosixRlimit* result);
    [LibraryImport("libc", EntryPoint = "setrlimit", SetLastError = true)] private static partial int SetrlimitUnix(int resource, PosixRlimit* value);
}
