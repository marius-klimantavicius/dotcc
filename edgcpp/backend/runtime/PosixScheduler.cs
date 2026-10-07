#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    public int SchedulerGetCpu()
    {
        // Thread.GetCurrentProcessorId is a cached scheduling hint and cannot
        // implement the instantaneous sched_getcpu contract faithfully.
        if (OperatingSystem.IsLinux())
        {
            int result = LinuxCurrentProcessor();
            if (result < 0) SetError(Marshal.GetLastPInvokeError());
            return result;
        }
        if (OperatingSystem.IsWindows())
        {
            WindowsCurrentProcessor(out ProcessorNumber processor);
            // Windows processor numbers are relative to groups of at most64.
            // Include the group to avoid aliasing processors on large hosts.
            return processor.Group * 64 + processor.Number;
        }
        // There is no supported macOS equivalent. Callers such as DuckDB can
        // take their existing failure fallback instead of receiving a fake ID.
        SetError(38);
        return -1;
    }

    public int SchedulerYield()
    {
        // Yield's false means no other thread was selected, not an error.
        Thread.Yield();
        return 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessorNumber
    {
        internal ushort Group;
        internal byte Number, Reserved;
    }

    [LibraryImport("libc", EntryPoint = "sched_getcpu", SetLastError = true)]
    private static partial int LinuxCurrentProcessor();
    [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentProcessorNumberEx")]
    private static partial void WindowsCurrentProcessor(out ProcessorNumber processor);
}
