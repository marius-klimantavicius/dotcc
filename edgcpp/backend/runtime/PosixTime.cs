#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    public int Gettimeofday(PosixTimeval* value, void* timezone)
    {
        if (value != null)
        {
            long ticks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
            value->tv_sec = Math.DivRem(ticks, TimeSpan.TicksPerSecond, out long remainder);
            value->tv_usec = remainder / 10;
        }
        if (timezone != null)
        {
            // The legacy timezone argument is not used by SQLite; POSIX leaves
            // it obsolete. Expose the host's standard offset, with no DST rule.
            ((int*)timezone)[0] = checked(-(int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
            ((int*)timezone)[1] = 0;
        }
        return 0;
    }

    public int Utimes(sbyte* path, PosixTimeval* times)
    {
        try
        {
            if (times != null && ((ulong)times[0].tv_usec >= 1000000 || (ulong)times[1].tv_usec >= 1000000))
            { SetError(22); return -1; }
            string name = Resolve(path);
            if (OperatingSystem.IsWindows())
            {
                long access, write;
                if (times == null) access = write = DateTime.UtcNow.ToFileTimeUtc();
                else
                {
                    // FILETIME counts 100 ns intervals since 1601, independently
                    // of host timezone. Validate both before changing either.
                    access = checked(checked(times[0].tv_sec + 11644473600L) * 10000000 + times[0].tv_usec * 10);
                    write = checked(checked(times[1].tv_sec + 11644473600L) * 10000000 + times[1].tv_usec * 10);
                    if (access < 0 || write < 0) { SetError(22); return -1; }
                }
                SafeFileHandle handle;
                fixed (char* p = name)
                    handle = OpenTimestampFile(p, 0x100, 7, null, 3, 0x02000000, 0); // WRITE_ATTRIBUTES, share R/W/delete, backup semantics
                using (handle)
                {
                    if (handle.IsInvalid || SetFileTime(handle, null, &access, &write) == 0)
                        return TimestampError();
                }
                return 0;
            }
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS()) { SetError(95); return -1; }
            // A single path syscall updates both timestamps. In particular it
            // does not open/close a descriptor, which could release process-wide
            // POSIX record locks held through a different descriptor.
            byte[] bytes = Encoding.UTF8.GetBytes(name + "\0");
            fixed (byte* p = bytes)
            {
                if (OperatingSystem.IsMacOS() && times != null)
                {
                    // Darwin suseconds_t is 32-bit, followed by tail padding.
                    DarwinTimeval* native = stackalloc DarwinTimeval[2];
                    native[0] = new() { Seconds = times[0].tv_sec, Microseconds = (int)times[0].tv_usec };
                    native[1] = new() { Seconds = times[1].tv_sec, Microseconds = (int)times[1].tv_usec };
                    if (UtimesUnix(p, native) != 0) return TimestampError();
                }
                else if (UtimesUnix(p, times) != 0) return TimestampError();
            }
            return 0;
        }
        catch (Exception error) { return Fail(error); }
    }

    private int TimestampError()
    { SetError(PosixPlatform.NativeErrorNumber(Marshal.GetLastPInvokeError())); return -1; }

    [StructLayout(LayoutKind.Sequential)]
    private struct DarwinTimeval { internal long Seconds; internal int Microseconds; }

    // Linux LP64 timeval matches the portable two-int64 record; macOS converts
    // to its native time_t/suseconds_t record above. No stat layout is reused.
    [LibraryImport("libc", EntryPoint = "utimes", SetLastError = true)]
    private static partial int UtimesUnix(byte* path, void* times);
    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true)]
    private static partial SafeFileHandle OpenTimestampFile(char* path, uint access, uint sharing,
        void* securityAttributes, uint creation, uint attributes, nint template);
    [LibraryImport("kernel32.dll", EntryPoint = "SetFileTime", SetLastError = true)]
    private static partial int SetFileTime(SafeFileHandle handle, long* creation, long* access, long* write);

    public uint Sleep(uint seconds)
    {
        // Thread.Sleep takes a signed millisecond count, whereas sleep accepts
        // the full unsigned seconds range. Chunk without overflow or busy waits.
        ulong milliseconds = (ulong)seconds * 1000;
        while (milliseconds != 0)
        {
            int chunk = (int)Math.Min(milliseconds, int.MaxValue);
            Thread.Sleep(chunk);
            milliseconds -= (uint)chunk;
        }
        return 0;
    }

    public int Usleep(uint microseconds)
    {
        // The BCL wait provides at least the requested delay; rounding up avoids
        // returning early for sub-millisecond requests and works under NativeAOT.
        if (microseconds != 0) Thread.Sleep(checked((int)(((ulong)microseconds + 999) / 1000)));
        return 0;
    }

    public long Sysconf(int name)
    {
        if (name == 30) return Environment.SystemPageSize; // portable _SC_PAGESIZE
        // Online CPUs available to this managed process, including the BCL's
        // affinity/container limits; also used by libc++ hardware_concurrency.
        if (name == 84) return Environment.ProcessorCount;
        if (name == 85) return PhysicalPages(); // portable _SC_PHYS_PAGES
        SetError(22);
        return -1;
    }

    public int Getpid() => Environment.ProcessId;
}
