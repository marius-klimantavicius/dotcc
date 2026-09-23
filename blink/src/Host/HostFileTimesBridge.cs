using global::System;
using Managed.Emulation.Host;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static unsafe bool ReadFileTimes(Libc.timespec* times,
        out VirtualFileTimeUpdate access, out VirtualFileTimeUpdate modify)
    {
        if (times == null) { access = modify = new(FileTimeSelection.Now); return true; }
        access = FileTimeRequest(times[0]);
        modify = FileTimeRequest(times[1]);
        return access.IsValid && modify.IsValid;
    }
    private static VirtualFileTimeUpdate FileTimeRequest(Libc.timespec value)
    {
        if (value.tv_nsec == 1073741823) return new(FileTimeSelection.Now);
        if (value.tv_nsec == 1073741822) return new(FileTimeSelection.Omit);
        if (value.tv_nsec < 0 || value.tv_nsec >= 1_000_000_000)
            return new((FileTimeSelection)(-1));
        return new(FileTimeSelection.Explicit, new(value.tv_sec, (int)value.tv_nsec));
    }
    public static unsafe int blink_host_futimens(int fd, Libc.timespec* times)
    {
        try {
            if (io == null) return IoError(19);
            if (!ReadFileTimes(times, out var access, out var modify)) return IoError(22);
            return (int)IoResult(io.SetFileTimes(fd, access, modify));
        } catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_utimensat(int directoryFd, byte* path, Libc.timespec* times, int flags)
    {
        try {
            if (io == null) return IoError(19);
            if (!ReadFileTimes(times, out var access, out var modify)) return IoError(22);
            if (access.Selection == FileTimeSelection.Omit && modify.Selection == FileTimeSelection.Omit)
                return (int)IoResult(io.SetFileTimesAt(directoryFd, "", access, modify, flags));
            if (path == null) return IoError(14); // Linux null-path fd extension is not selected.
            if ((flags & ~256) != 0) return IoError(95);
            int length = 0;
            while (length < PathLimit && path[length] != 0) ++length;
            if (length == PathLimit) return IoError(36);
            string name;
            try { name = PathEncoding.GetString(new ReadOnlySpan<byte>(path, length)); }
            catch (global::System.Text.DecoderFallbackException) { return IoError(22); }
            return (int)IoResult(io.SetFileTimesAt(directoryFd, name, access, modify, flags));
        } catch (Exception error) { return IoException(error); }
    }
}
